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
        private TimeProfileView _profile;
        private Grid _draggableElement;

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
                var percentageHeight = 1d - GetBrightnessAsPercentageOfMax(node.Brightness);

                var x = VerticalAxisWidthMargin + (percentageWidth * (width - VerticalAxisWidthMargin));
                var y = percentageHeight * (height - HorizontalAxisHeightMargin);

                var temperatureColor = GetColorFromTemperature(node.Temperature);

                var draggableElement = new Grid
                {
                    Margin = new Thickness(x, y, 0, 0)
                };
                draggableElement.MouseLeftButtonDown += DraggableOnMouseLeftButtonDown;
                draggableElement.MouseEnter += DraggableMouseEnter;

                // Draw the element at the node position
                draggableElement.Children.Add(new Label()
                {
                    Background = new SolidColorBrush(Colors.Blue),
                    Content = $"{node.Brightness}%",
                });

                GraphCanvas.Children.Add(draggableElement);

                // Draw gradient stop
                BackgroundBrush.GradientStops.Add(new GradientStop()
                {
                    Color = temperatureColor,
                    Offset = percentageWidth,
                });
            }
        }

        private void DraggableMouseEnter(object sender, MouseEventArgs e)
        {
            var draggableElement = (Grid)sender;

            var nodeIndex = Array.IndexOf(GraphCanvas.Children.OfType<Grid>().ToArray(), draggableElement);
            var node = _profile.Nodes[nodeIndex];

            SelectedNode.DataContext = node;
        }

        private void DraggableOnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var draggableElement = (Grid)sender;
            _draggableElement = draggableElement;
        }

        public MluxSettingsGraph()
        {
            InitializeComponent();

            SizeChanged += MluxSettingsGraph_SizeChanged;
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            _draggableElement = null;

            base.OnMouseLeftButtonUp(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_draggableElement == null) return;

            var pos = e.GetPosition(GraphCanvas);

            var nodeIndex = Array.IndexOf(GraphCanvas.Children.OfType<Grid>().ToArray(), _draggableElement);
            var node = _profile.Nodes[nodeIndex];

            var minTime = TimeSpan.Zero;
            if (nodeIndex > 0)
            {
                var prevNode = _profile.Nodes[nodeIndex - 1];
                minTime = prevNode.TimeOfDay.Add(TimeSpan.FromMinutes(15));
            }

            var width = Math.Max(100, GraphCanvas.ActualWidth);
            var height = Math.Max(100, GraphCanvas.ActualHeight);

            var xRel = pos.X / width;
            var yRel = pos.Y / height;

            xRel = Math.Min(1d, xRel);

            var roundedDays = Math.Round(xRel * 24 * 4) / 24 / 4;

            var newTimeSpan = TimeSpan.FromDays(roundedDays);
            if (newTimeSpan < minTime)
            {
                newTimeSpan = minTime;
            }

            node.Brightness = (int)((1 - yRel) * 100); // TODO: Fetch max brightness
            node.TimeOfDay = newTimeSpan;

            var percentageWidth = node.TimeOfDay.TotalSeconds / TimeSpan.FromDays(1).TotalSeconds;
            var percentageHeight = 1d - GetBrightnessAsPercentageOfMax(node.Brightness);

            var x = VerticalAxisWidthMargin + (percentageWidth * (width - VerticalAxisWidthMargin));
            var y = percentageHeight * (height - HorizontalAxisHeightMargin);

            _draggableElement.Margin = new Thickness(x, y, 0, 0);

            base.OnMouseMove(e);
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
                var percentageHeight = 1d - GetBrightnessAsPercentageOfMax(n);

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
                Width = 1
            });

            // Make the gradient
            BackgroundBrush.GradientStops.Clear();
            Background.Margin = new Thickness(VerticalAxisWidthMargin, 0, 0, HorizontalAxisHeightMargin);
        }

        private double GetBrightnessAsPercentageOfMax(int brightness)
        {
            return (brightness - TimeProfile.MinBrightness) / (double)(TimeProfile.MaxBrightness - TimeProfile.MinBrightness);
        }

        private Color GetColorFromTemperature(int temperature)
        {
            var temp = ColorTemperature.GetColorProfile(temperature);
            return new Color()
            {
                A = byte.MaxValue,
                R = (byte)(byte.MaxValue * temp.Red),
                G = (byte)(byte.MaxValue * temp.Green),
                B = (byte)(byte.MaxValue * temp.Blue),
            };
        }
    }
}
