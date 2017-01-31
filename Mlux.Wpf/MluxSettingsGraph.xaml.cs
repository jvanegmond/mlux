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
    /// <summary>
    /// Interaction logic for MluxSettingsGraph.xaml
    /// </summary>
    public partial class MluxSettingsGraph : UserControl
    {
        private enum NodeType
        {
            TimeOnly,
            Brightness,
            Temperature
        }

        private TimeProfileView _profile;
        private Grid _draggingElement;
        private NodeType _draggingType = NodeType.TimeOnly;

        public const int VerticalAxisWidthMargin = 50;
        public const int HorizontalAxisHeightMargin = 30;

        public TimeProfileView Profile
        {
            get { return _profile; }
            set
            {
                _profile = value;
            }
        }

        private void DrawNodes()
        {
            var width = Math.Max(100, GraphCanvas.ActualWidth);
            var height = Math.Max(100, GraphCanvas.ActualHeight);

            // Draw the nodes
            foreach (var node in _profile.Nodes)
            {
                var percentageWidth = node.TimeOfDay.TotalSeconds / TimeSpan.FromDays(1).TotalSeconds;
                var x = VerticalAxisWidthMargin + (percentageWidth * (width - VerticalAxisWidthMargin));

                var draggableElement = new Grid
                {
                    Margin = new Thickness(x, 0, 0, 0),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    Cursor = Cursors.SizeWE,
                };
                draggableElement.MouseLeftButtonDown += DraggableOnMouseLeftButtonDown;
                draggableElement.MouseEnter += DraggableMouseEnter;

                // Draw vertical line
                draggableElement.Children.Add(new Rectangle()
                {
                    Fill = new SolidColorBrush(Colors.Blue),
                    Height = height,
                    Width = 2
                });

                // Draw draggable brightness node
                var brightnessY = (1 - GetNodeValuePercentage(GetCurrentValue(node, NodeType.Brightness), NodeType.Brightness)) * height;
                var brightnessGrid = new Grid()
                {
                    Children =
                    {
                        new Ellipse()
                        {
                            Fill = new SolidColorBrush(Colors.Gray),
                            Height = 15,
                            Width = 15,
                        }
                    },
                    Margin = new Thickness(0, brightnessY, 0, 0),
                    VerticalAlignment = VerticalAlignment.Top
                };
                brightnessGrid.MouseLeftButtonDown += BrightnessGrid_MouseLeftButtonDown;
                draggableElement.Children.Add(brightnessGrid);

                // Draw draggable temperature node
                var temperatureY = (1 - GetNodeValuePercentage(GetCurrentValue(node, NodeType.Temperature), NodeType.Temperature)) * height;
                var temperatureGrid = new Grid()
                {
                    Children =
                    {
                        new Ellipse()
                        {
                            Fill = new SolidColorBrush(Colors.Red),
                            Height = 15,
                            Width = 15,
                        }
                    },
                    Margin = new Thickness(0, temperatureY, 0, 0),
                    VerticalAlignment = VerticalAlignment.Top
                };
                temperatureGrid.MouseLeftButtonDown += TemperatureGrid_MouseLeftButtonDown;
                draggableElement.Children.Add(temperatureGrid);


                GraphCanvas.Children.Add(draggableElement);
            }
        }

        private void TemperatureGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _draggingType = NodeType.Temperature;
        }

        private void BrightnessGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _draggingType = NodeType.Brightness;
        }

        private void DraggableOnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Start dragging element
            var draggableElement = (Grid)sender;
            _draggingElement = draggableElement;
            Mouse.OverrideCursor = Cursors.SizeWE;
        }

        private void DraggableMouseEnter(object sender, MouseEventArgs e)
        {
            // Set the top bar values to the currently mouse-over node
            var draggableElement = (Grid)sender;

            var nodeIndex = Array.IndexOf(GraphCanvas.Children.OfType<Grid>().ToArray(), draggableElement);
            var node = _profile.Nodes[nodeIndex];

            SelectedNode.DataContext = node;
        }

        public MluxSettingsGraph()
        {
            InitializeComponent();

            SizeChanged += MluxSettingsGraph_SizeChanged;
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
            var xRel = (pos.X - VerticalAxisWidthMargin) / (width - VerticalAxisWidthMargin);
            xRel = Math.Min(1d, xRel);
            var roundedDays = Math.Round(xRel * 24 * 4) / 24 / 4;
            var newTimeSpan = TimeSpan.FromDays(roundedDays);
            if (newTimeSpan < minTime) // TODO: Same thing with max time
            {
                newTimeSpan = minTime;
            }
            node.TimeOfDay = newTimeSpan;

            var percentageWidth = node.TimeOfDay.TotalSeconds / TimeSpan.FromDays(1).TotalSeconds;
            var x = VerticalAxisWidthMargin + percentageWidth * (width - VerticalAxisWidthMargin);

            _draggingElement.Margin = new Thickness(x, 0, 0, 0);

            if (_draggingType != NodeType.TimeOnly)
            {
                // Get value of thing
                var yRel = 1d - (pos.Y / height);
                SetNodeValuePercentage(node, yRel, _draggingType);

                var value = GetCurrentValue(node, _draggingType);
                var percentageHeight = 1d - GetNodeValuePercentage(value, _draggingType);

                var y = percentageHeight * (height - HorizontalAxisHeightMargin);

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

        private int GetCurrentValue(TimeNodeView node, NodeType type)
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

        private void MluxSettingsGraph_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Clear();
            DrawChrome();
            DrawNodes();
        }

        private void Clear()
        {
            GraphCanvas.Children.Clear();
        }

        private void DrawChrome()
        {
            var width = Math.Max(100, GraphCanvas.ActualWidth);
            var height = Math.Max(100, GraphCanvas.ActualHeight);

            // Draw hour of day on horizontal axis/scale
            for (var n = 0; n < 23; n++)
            {
                var percentageWidth = n / 24d;

                GraphCanvas.Children.Add(new Label()
                {
                    Margin = new Thickness(VerticalAxisWidthMargin + (percentageWidth * width), height - HorizontalAxisHeightMargin, 0, 0),
                    Height = HorizontalAxisHeightMargin,
                    Content = $"{n}:00"
                });
            }
            GraphCanvas.Children.Add(new Rectangle()
            {
                Fill = new SolidColorBrush(Colors.Black),
                Margin = new Thickness(VerticalAxisWidthMargin, height - HorizontalAxisHeightMargin, 0, 0),
                Height = 1,
                Width = width - VerticalAxisWidthMargin
            });

            // Draw brightness on vertical axis/scale
            for (var n = 0; n <= 100; n += 10)
            {
                var percentageHeight = 1d - GetNodeValuePercentage(n, NodeType.Brightness);

                GraphCanvas.Children.Add(new Label()
                {
                    Margin = new Thickness(0, percentageHeight * (height - HorizontalAxisHeightMargin /* - label height ?? */), 0, 0),
                    Content = $"{n}"
                });
            }
            GraphCanvas.Children.Add(new Rectangle()
            {
                Fill = new SolidColorBrush(Colors.Black),
                Margin = new Thickness(VerticalAxisWidthMargin, 0, 0, 0),
                Height = height - HorizontalAxisHeightMargin,
                Width = 1
            });

            // Draw the current position
            var now = TimeProvider.Now;
            var w = now.TimeOfDay.TotalSeconds / TimeSpan.FromDays(1).TotalSeconds;
            double x = VerticalAxisWidthMargin + (w * (width - VerticalAxisWidthMargin));

            GraphCanvas.Children.Add(new Rectangle()
            {
                Fill = new SolidColorBrush(Colors.DarkRed),
                Margin = new Thickness(x, 0, 0, 0),
                Height = height,
                Width = 2
            });

            // Make the gradient
            BackgroundBrush.GradientStops.Clear();
            Background.Margin = new Thickness(VerticalAxisWidthMargin, 0, 0, HorizontalAxisHeightMargin);
        }

        private double GetNodeValuePercentage(int value, NodeType type)
        {
            if (type == NodeType.Brightness)
            {
                return (value - TimeProfile.MinBrightness) / (double)(TimeProfile.MaxBrightness - TimeProfile.MinBrightness);
            }
            else
            {
                return (value - 3300d) / 3200d;
            }
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
    }
}
