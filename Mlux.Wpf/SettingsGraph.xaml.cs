using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Mlux.Lib.Display;
using Mlux.Lib.Time;
using Mlux.Wpf.Bindings;

namespace Mlux.Wpf
{
    public  enum NodeType
    {
        TimeOnly,
        Brightness,
        Temperature
    }
    /// <summary>
    /// Interaction logic for SettingsGraph.xaml
    /// </summary>
    public partial class SettingsGraph : UserControl
    {

        private TimeProfileView _profile;
        private Grid _draggingElement;
        private NodeType _draggingType = NodeType.TimeOnly;

        public const int AxisWidthMargin = 50;
        public const int AxisHeightMargin = 30;

        public TimeProfileView Profile
        {
            get { return _profile; }
            set
            {
                _profile = value;
            }
        }

        public SettingsGraph()
        {
            InitializeComponent();

            SizeChanged += MluxSettingsGraph_SizeChanged;
            Loaded += (sender, args) => MluxSettingsGraph_SizeChanged(sender, null);
        }

        private void DrawNodes()
        {
            GraphCanvas.Children.Clear();
            GraphCanvas.Margin = new Thickness(AxisWidthMargin, 0, 0, AxisHeightMargin);

            // Draw the nodes
            foreach (var node in _profile.Nodes)
            {
                GraphCanvas.Children.Add(new SettingsGraphNode() { Node = node });
            }
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            _draggingElement = null;
            _draggingType = NodeType.TimeOnly;
            Mouse.OverrideCursor = null;

            base.OnMouseLeftButtonUp(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_draggingElement == null) return;

            var pos = e.GetPosition(GraphCanvas);

            var nodeIndex = Array.IndexOf(GraphCanvas.Children.OfType<Grid>().ToArray(), _draggingElement);
            var node = _profile.Nodes[nodeIndex];

            var minTime = TimeSpan.Zero;
            if (nodeIndex > 0)
            {
                var prevNode = _profile.Nodes[nodeIndex - 1];
                minTime = prevNode.TimeOfDay.Add(TimeSpan.FromMinutes(15));
            }

            var width = Math.Max(100, GraphCanvas.ActualWidth);
            var height = Math.Max(100, GraphCanvas.ActualHeight);

            // Get time stuff (horizontal)
            var xRel = (pos.X - AxisWidthMargin) / (width - AxisWidthMargin);
            xRel = Math.Min(1d, xRel);
            var roundedDays = Math.Round(xRel * 24 * 4) / 24 / 4;
            var newTimeSpan = TimeSpan.FromDays(roundedDays);
            if (newTimeSpan < minTime) // TODO: Same thing with max time
            {
                newTimeSpan = minTime;
            }
            node.TimeOfDay = newTimeSpan;

            var percentageWidth = node.TimeOfDay.TotalSeconds / TimeSpan.FromDays(1).TotalSeconds;
            var x = AxisWidthMargin + percentageWidth * (width - AxisWidthMargin);

            _draggingElement.Margin = new Thickness(x, 0, 0, 0);

            if (_draggingType != NodeType.TimeOnly)
            {
                // Get value of thing
                var yRel = 1d - (pos.Y / height);
                SetNodeValuePercentage(node, yRel, _draggingType);

                var value = GetCurrentValue(node, _draggingType);
                var percentageHeight = 1d - GetNodeValuePercentage(value, _draggingType);

                var y = percentageHeight * (height - AxisHeightMargin);

                var valueGrid = _draggingElement.Children.OfType<Grid>().ElementAt(((int)_draggingType - 1));    // based on index of grid in children
                valueGrid.Margin = new Thickness(0, y, 0, 0);
            }
            base.OnMouseMove(e);
        }

        private void SetNodeValuePercentage(TimeNodeView node, double percentage, NodeType type)
        {
            if (type == NodeType.Brightness)
            {
                // Round it to 5
                node.Brightness = RoundTo((int)(percentage * 100d), 5);
            }
            else
            {
                node.Temperature = RoundTo((int)((percentage * 3200d) + 3300d), 50);
            }
        }

        private int RoundTo(int value, int roundTo)
        {
            var remainder = value % roundTo;
            if (remainder <= (roundTo / 2))
            {
                return value - remainder; // round down
            }
            else
            {
                return value - remainder + roundTo; // round up
            }
        }

        private void MluxSettingsGraph_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawChrome();
            DrawNodes();
        }

        private void DrawChrome()
        {
            var width = Math.Max(100, ChromeCanvas.ActualWidth);
            var height = Math.Max(100, ChromeCanvas.ActualHeight);

            ChromeCanvas.Children.Clear();

            // Draw hour of day on horizontal axis/scale
            for (var n = 0; n <= 24; n++)
            {
                var percentageWidth = n / 24d;

                ChromeCanvas.Children.Add(new Label()
                {
                    Margin = new Thickness(AxisWidthMargin + (percentageWidth * (width - AxisWidthMargin)), height - AxisHeightMargin, 0, 0),
                    Height = AxisHeightMargin,
                    Content = $"{n}:00"
                });
            }
            ChromeCanvas.Children.Add(new Rectangle()
            {
                Fill = new SolidColorBrush(Colors.Black),
                Margin = new Thickness(AxisWidthMargin, height - AxisHeightMargin, 0, 0),
                Height = 1,
                Width = width - AxisWidthMargin
            });

            // Draw brightness on vertical axis/scale
            for (var n = 0; n <= 100; n += 10)
            {
                var percentageHeight = 1d - GetNodeValuePercentage(n, NodeType.Brightness);

                ChromeCanvas.Children.Add(new Label()
                {
                    Margin = new Thickness(0, percentageHeight * (height - AxisHeightMargin /* - label height ?? */), 0, 0),
                    Content = $"{n}"
                });
            }
            ChromeCanvas.Children.Add(new Rectangle()
            {
                Fill = new SolidColorBrush(Colors.Black),
                Margin = new Thickness(AxisWidthMargin, 0, 0, 0),
                Height = height - AxisHeightMargin,
                Width = 1
            });

            // Draw the current position
            var now = TimeProvider.Now;
            var w = now.TimeOfDay.TotalSeconds / TimeSpan.FromDays(1).TotalSeconds;
            double x = AxisWidthMargin + (w * (width - AxisWidthMargin));

            ChromeCanvas.Children.Add(new Rectangle()
            {
                Fill = new SolidColorBrush(Colors.DarkRed),
                Margin = new Thickness(x, 0, 0, 0),
                Height = height - AxisHeightMargin,
                Width = 2
            });
        }

        private Color GetBackgroundColor(int temperature, int brightness)
        {
            // get color gamma adjustment
            var gamma = ColorTemperature.GetColorProfile(temperature);

            // turn brightness 0-100 into a value between 0.6-1.0 (first between 0-0.4 then add 0.6)
            var brightnessAdjust = (brightness / 250d) + 0.6d;

            return new Color()
            {
                A = byte.MaxValue,
                R = (byte)(byte.MaxValue * brightnessAdjust * gamma.Red),
                G = (byte)(byte.MaxValue * brightnessAdjust * gamma.Green),
                B = (byte)(byte.MaxValue * brightnessAdjust * gamma.Blue),
            };
        }

        private void Delete_OnClick(object sender, RoutedEventArgs e)
        {

        }

        private void AddNew_OnClick(object sender, RoutedEventArgs e)
        {

        }

        public static double GetNodeValuePercentage(int value, NodeType type)
        {
            if (type == NodeType.Brightness)
            {
                return (value - TimeProfile.MinBrightness) / (double)(TimeProfile.MaxBrightness - TimeProfile.MinBrightness);
            }
            else
            {
                return ((double)value - (TimeProfile.MaxTemperature - TimeProfile.MinTemperature)) / TimeProfile.MinTemperature;
            }
        }

        public static int GetCurrentValue(TimeNodeView node, NodeType type)
        {
            if (type == NodeType.Brightness)
            {
                return node.Brightness;
            }
            else
            {
                return node.Temperature;
            }
        }
    }
}
