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
using Mlux.Wpf.Icons;

namespace Mlux.Wpf
{
    public enum NodeType
    {
        None,
        Time,
        Brightness,
        Temperature
    }
    /// <summary>
    /// Interaction logic for SettingsGraph.xaml
    /// </summary>
    public partial class SettingsGraph : UserControl
    {

        private TimeProfileView _profile;

        public const int AxisWidth = 50;
        public const int AxisHeight = 30;

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

            SizeChanged += (sender, args) =>
            {
                DrawNodes();
                DrawChrome();
            };

            Loaded += (sender, args) =>
            {
                DrawNodes();
            };

            MouseLeave += (sender, args) => StopDrag();
            MouseLeftButtonUp += (sender, args) => StopDrag();
        }

        private void DrawNodes()
        {
            GraphCanvas.Children.Clear();

            var margin = ChromeCanvas.Margin;
            GraphCanvas.Margin = new Thickness(margin.Left + AxisWidth + 1, margin.Top, margin.Right, margin.Bottom + AxisHeight);

            // Draw the first and last gradientstop (offset 0 and 1) with interpolated values based on midnight
            var lastNode = _profile.Last();
            var firstNode = _profile.First();

            if (BackgroundBrush.GradientStops.Count == 0)
            {

                BackgroundBrush.GradientStops.Add(new GradientStop(Colors.DeepPink, 0));
                BackgroundBrush.GradientStops.Add(new GradientStop(Colors.DeepPink, 1));

                lastNode.PropertyChanged += (sender, args) => UpdateFirstLastGradientStops(lastNode, firstNode);
                firstNode.PropertyChanged += (sender, args) => UpdateFirstLastGradientStops(lastNode, firstNode);
            }

            UpdateFirstLastGradientStops(lastNode, firstNode);

            // Draw the nodes
            foreach (var node in _profile.Nodes)
            {
                var uiNode = new SettingsGraphNode(node);
                GraphCanvas.Children.Add(uiNode);
                uiNode.MouseEnter += (sender, args) => SelectedNode.DataContext = ((SettingsGraphNode)sender).Node;

                var gradientStop = new GradientStop();

                SetGradientStop(node, gradientStop);
                node.PropertyChanged += (sender, args) => SetGradientStop(node, gradientStop);

                BackgroundBrush.GradientStops.Add(gradientStop);
            }
        }

        private void UpdateFirstLastGradientStops(TimeNodeView lastNode, TimeNodeView firstNode)
        {
            var brightness = (int)LinearNodeInterpolation.Interpolate(TimeSpan.Zero, lastNode.UnderlyingNode, firstNode.UnderlyingNode, NodeProperty.Brightness);
            var temperature = (int)LinearNodeInterpolation.Interpolate(TimeSpan.Zero, lastNode.UnderlyingNode, firstNode.UnderlyingNode, NodeProperty.ColorTemperature);

            SetGradientStop(TimeSpan.Zero, brightness, temperature, BackgroundBrush.GradientStops[0]);
            SetGradientStop(TimeSpan.FromDays(1), brightness, temperature, BackgroundBrush.GradientStops[1]);
        }

        private void SetGradientStop(TimeNodeView node, GradientStop gradientStop)
        {
            SetGradientStop(node.TimeOfDay, node.Brightness, node.Temperature, gradientStop);
        }

        private void SetGradientStop(TimeSpan timeOfDay, int brightness, int temperature, GradientStop gradientStop)
        {
            var percentageWidth = timeOfDay.TotalSeconds / TimeSpan.FromDays(1).TotalSeconds;

            gradientStop.Offset = percentageWidth;

            var color = GetBackgroundColor(temperature, brightness);

            gradientStop.Color = color;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            var draggingNodes = GraphCanvas.Children.OfType<SettingsGraphNode>();
            foreach (var draggingNode in draggingNodes)
            {
                if (draggingNode.DraggingType == NodeType.None) continue;

                draggingNode.Drag(e);
            }

            base.OnMouseMove(e);
        }

        private void StopDrag()
        {
            var draggingNodes = GraphCanvas.Children.OfType<SettingsGraphNode>();
            foreach (var draggingNode in draggingNodes)
            {
                draggingNode.DraggingType = NodeType.None;
            }

            Mouse.OverrideCursor = null;
        }

        private void DrawChrome()
        {
            var width = Math.Max(100, ChromeCanvas.ActualWidth);
            var height = Math.Max(100, ChromeCanvas.ActualHeight);

            ChromeCanvas.Children.Clear();

            // Draw hour of day on horizontal scale
            var scaleWidth = width - AxisWidth;
            for (var n = 0; n <= 24; n++)
            {
                var percentage = n / 24d;

                // Draw little nudges below the horizontal bar
                ChromeCanvas.Children.Add(new Rectangle()
                {
                    Margin = new Thickness(AxisWidth + (percentage * scaleWidth), height - AxisHeight, 0, 0),
                    Width = 1,
                    Height = 2,
                    Fill = new SolidColorBrush(Colors.Black)
                });

                if (n % 6 != 0) continue;

                // Draw the label
                var labelWidth = 30;
                ChromeCanvas.Children.Add(new TextBlock()
                {
                    Margin = new Thickness(AxisWidth + (percentage * scaleWidth) - (labelWidth / 2d), height - AxisHeight, 0, 0),
                    Padding = new Thickness(),
                    Height = AxisHeight,
                    Text = $"{n}:00"
                });
            }

            // Draw sunrise/sundown on scale
            var sunriseTime = _profile.UnderlyingProfile.GetSunrise();
            var sundownTime = _profile.UnderlyingProfile.GetSundown();

            var sunrisePercentage = sunriseTime.TotalSeconds / TimeSpan.FromDays(1).TotalSeconds;
            var sundownPercentage = sundownTime.TotalSeconds / TimeSpan.FromDays(1).TotalSeconds;

            const int iconSize = 30;
            ChromeCanvas.Children.Add(new IconSunrise()
            {
                Color = Colors.Orange,
                Width = iconSize,
                Height = iconSize,
                Margin = new Thickness(AxisWidth + (sunrisePercentage * scaleWidth) - (iconSize / 2d), height - AxisHeight, 0, 0),
            });
            ChromeCanvas.Children.Add(new IconSundown()
            {
                Color = Colors.OrangeRed,
                Width = iconSize,
                Height = iconSize,
                Margin = new Thickness(AxisWidth + (sundownPercentage * scaleWidth) - (iconSize / 2d), height - AxisHeight, 0, 0),
            });

            // Draw horizontal bar
            ChromeCanvas.Children.Add(new Rectangle()
            {
                Fill = new SolidColorBrush(Colors.Black),
                Margin = new Thickness(AxisWidth, height - AxisHeight, 0, 0),
                Height = 1,
                Width = width - AxisWidth
            });

            // Draw brightness on vertical axis/scale
            var scaleHeight = height - AxisHeight;
            for (var n = 0; n <= 100; n += 10)
            {
                var percentage = 1d - (n / 100d);

                // Draw little nudges next to the horizontal bar
                ChromeCanvas.Children.Add(new Rectangle()
                {
                    Margin = new Thickness(AxisWidth - 2, (percentage * scaleHeight), 0, 0),
                    Width = 2,
                    Height = 1,
                    Fill = new SolidColorBrush(Colors.Black)
                });

                if (n % 20 != 0 || n == 0) continue;

                // Draw the label
                var labelHeight = 25;
                ChromeCanvas.Children.Add(new TextBlock()
                {
                    Margin = new Thickness(10, (percentage * scaleHeight), 0, 0),
                    Text = $"{n}%",
                    Height = labelHeight,
                });
            }

            // Draw vertical bar
            ChromeCanvas.Children.Add(new Rectangle()
            {
                Fill = new SolidColorBrush(Colors.Black),
                Margin = new Thickness(AxisWidth, 0, 0, 0),
                Height = height - AxisHeight + 1,
                Width = 1
            });

            // Draw the current position
            var now = TimeProvider.Now;
            var w = now.TimeOfDay.TotalSeconds / TimeSpan.FromDays(1).TotalSeconds;
            var x = AxisWidth + (w * (width - AxisWidth));

            ChromeCanvas.Children.Add(new Rectangle()
            {
                Fill = new SolidColorBrush(Colors.DarkRed),
                Margin = new Thickness(x, 0, 0, 0),
                Height = height - AxisHeight,
                Width = 1
            });
        }

        private Color GetBackgroundColor(int temperature, int brightness)
        {
            // get color gamma adjustment
            var gamma = ColorTemperature.GetColorProfile(temperature);

            return new Color()
            {
                A = byte.MaxValue,
                R = (byte)(byte.MaxValue * gamma.Red),
                G = (byte)(byte.MaxValue * gamma.Green),
                B = (byte)(byte.MaxValue * gamma.Blue),
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
