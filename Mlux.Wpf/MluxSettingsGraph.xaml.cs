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
using Mlux.Lib.Time;

namespace Mlux.Wpf
{
    /// <summary>
    /// Interaction logic for MluxSettingsGraph.xaml
    /// </summary>
    public partial class MluxSettingsGraph : UserControl
    {
        private TimeProfile _profile;
        private Timer _timer;
        private ManualResetEvent _updateProfileCompletEvent = new ManualResetEvent(false);

        public int VerticalAxisWidth { get; } = 50;
        public int HorizontalAxisHeight { get; } = 30;

        public MluxSettingsGraph()
        {
            InitializeComponent();
            _timer = new Timer(Timercallback, null, TimeSpan.Zero, Timeout.InfiniteTimeSpan);
            SizeChanged += MluxSettingsGraph_SizeChanged;
        }

        private void Timercallback(object state)
        {
            Dispatcher.Invoke(UpdateProfile);
            _updateProfileCompletEvent.WaitOne();
            _timer.Change(TimeSpan.FromSeconds(1), Timeout.InfiniteTimeSpan);
        }

        private void MluxSettingsGraph_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _timer.Change(TimeSpan.Zero, Timeout.InfiniteTimeSpan);
        }

        public TimeProfile Profile
        {
            get { return _profile; }
            set
            {
                _profile = value;
                UpdateProfile();
            }
        }

        private void UpdateProfile()
        {
            GraphCanvas.Children.Clear();
            var width = Math.Max(100, GraphCanvas.ActualWidth);
            var height = Math.Max(100, GraphCanvas.ActualHeight);

            // Draw hour of day on horizontal axis
            for (var n = 0; n < 23; n++)
            {
                var percentageWidth = n / 24d;

                GraphCanvas.Children.Add(new Label()
                {
                    Margin = new Thickness(VerticalAxisWidth + (percentageWidth * width), height - HorizontalAxisHeight, 0, 0),
                    Height = HorizontalAxisHeight,
                    Content = $"{n}:00"
                });
            }
            GraphCanvas.Children.Add(new Rectangle()
            {
                Fill = new SolidColorBrush(Colors.Black),
                Margin = new Thickness(VerticalAxisWidth, height - HorizontalAxisHeight, 0, 0),
                Height = 1,
                Width = width - VerticalAxisWidth
            });

            // Draw color temperature on vertical axis
            for (var n = 3200; n <= 6500; n += 100)
            {
                var percentageHeight = 1d - GetTemperatureAsPercentageOfMax(n);

                GraphCanvas.Children.Add(new Label()
                {
                    Margin = new Thickness(0, percentageHeight * (height - HorizontalAxisHeight /* - label height ?? */), 0, 0),
                    Content = $"{n}"
                });
            }
            GraphCanvas.Children.Add(new Rectangle()
            {
                Fill = new SolidColorBrush(Colors.Black),
                Margin = new Thickness(VerticalAxisWidth, 0, 0, 0),
                Height = height - HorizontalAxisHeight,
                Width = 1
            });

            // Design time the profile is not yet set
            if (_profile == null) return;

            // Draw the current position
            var time = DateTime.Now;
            var w = time.TimeOfDay.TotalSeconds / TimeSpan.FromDays(1).TotalSeconds;
            var h = 1d - GetTemperatureAsPercentageOfMax(Convert.ToInt32(_profile.GetCurrentValue(time, NodeProperty.ColorTemperature)));
            double x = VerticalAxisWidth + (w * (width - VerticalAxisWidth));
            double y = h * (height - HorizontalAxisHeight);

            GraphCanvas.Children.Add(new Ellipse()
            {
                Fill = new SolidColorBrush(Colors.Red),
                Margin = new Thickness(x - 5, y - 5, 0, 0),
                Height = 10,
                Width = 10
            });

            // Draw the nodes
            foreach (var node in _profile.Nodes)
            {
                var percentageWidth = node.TimeOfDay.TotalSeconds / TimeSpan.FromDays(1).TotalSeconds;

                var brightness = (int)node.Properties.First(_ => _.Name == NodeProperty.Brightness).Value;
                var temperature = (int)node.Properties.First(_ => _.Name == NodeProperty.ColorTemperature).Value;

                var percentageHeight = 1d - GetTemperatureAsPercentageOfMax(temperature);

                x = VerticalAxisWidth + (percentageWidth * (width - VerticalAxisWidth));
                y = percentageHeight * (height - HorizontalAxisHeight);

                // Draw the element at the node position
                GraphCanvas.Children.Add(new Label()
                {
                    Background = new SolidColorBrush(GetColorFromTemperature(temperature)),
                    Margin = new Thickness(x, y, 0, 0),
                    Content = $"{brightness}%"
                });

                GraphCanvas.Children.Add(new Rectangle()
                {
                    Fill = new SolidColorBrush(Colors.LightGray),
                    Margin = new Thickness(x, 0, 0, 0),
                    Width = 1,
                    Height = height - HorizontalAxisHeight
                });

                GraphCanvas.Children.Add(new Rectangle()
                {
                    Fill = new SolidColorBrush(Colors.LightGray),
                    Margin = new Thickness(VerticalAxisWidth, y, 0, 0),
                    Width = width - VerticalAxisWidth,
                    Height = 1
                });
            }

            _updateProfileCompletEvent.Set();
        }

        private Color GetColorFromTemperature(int temperature)
        {
            var percentage = GetTemperatureAsPercentageOfMax(temperature);
            return new Color()
            {
                A = byte.MaxValue,
                R = (byte)(percentage * byte.MaxValue),
                G = 0,
                B = (byte)((1d - percentage) * byte.MaxValue),
            };
        }

        private double GetTemperatureAsPercentageOfMax(int temperature)
        {
            return (temperature - TimeProfile.MinTemperature) / (double)(TimeProfile.MaxTemperature - TimeProfile.MinTemperature);
        }
    }
}
