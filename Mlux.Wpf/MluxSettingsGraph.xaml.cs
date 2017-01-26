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

        public int VerticalAxisWidthMargin { get; } = 50;
        public int HorizontalAxisHeightMargin { get; } = 30;

        public MluxSettingsGraph()
        {
            InitializeComponent();
            _timer = new Timer(Timercallback, null, TimeSpan.Zero, Timeout.InfiniteTimeSpan);
            SizeChanged += MluxSettingsGraph_SizeChanged;
        }

        private void Timercallback(object state)
        {
            try
            {
                if (Visibility == Visibility.Visible)
                {
                    Dispatcher.Invoke(UpdateProfile);
                    _updateProfileCompletEvent.WaitOne();
                }
            }
            catch (Exception err)
            {
                
            }
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

            // Design time the profile is not yet set
            if (_profile == null) return;

            // Draw the current position
            var now = TimeProvider.Now;
            var w = now.TimeOfDay.TotalSeconds / TimeSpan.FromDays(1).TotalSeconds;
            var h = 1d - GetBrightnessAsPercentageOfMax(Convert.ToInt32(_profile.GetCurrentValue(_profile.Previous(now), _profile.Next(now), TimeUtil.GetRelativeTime(now), NodeProperty.Brightness)));
            double x = VerticalAxisWidthMargin + (w * (width - VerticalAxisWidthMargin));
            double y = h * (height - HorizontalAxisHeightMargin);

            GraphCanvas.Children.Add(new Ellipse()
            {
                Fill = new SolidColorBrush(Colors.Red),
                Margin = new Thickness(x - 5, y - 5, 0, 0),
                Height = 10,
                Width = 10
            });

            // Make the gradient
            BackgroundBrush.GradientStops.Clear();
            Background.Margin = new Thickness(VerticalAxisWidthMargin, 0, 0, HorizontalAxisHeightMargin);

            // Draw the nodes
            foreach (var node in _profile.Nodes)
            {
                var percentageWidth = node.TimeOfDay.TotalSeconds / TimeSpan.FromDays(1).TotalSeconds;

                var brightness = (int)node.Properties.First(_ => _.Name == NodeProperty.Brightness).Value;
                var temperature = (int)node.Properties.First(_ => _.Name == NodeProperty.ColorTemperature).Value;

                var percentageHeight = 1d - GetBrightnessAsPercentageOfMax(brightness);

                x = VerticalAxisWidthMargin + (percentageWidth * (width - VerticalAxisWidthMargin));
                y = percentageHeight * (height - HorizontalAxisHeightMargin);

                var temperatureColor = GetColorFromTemperature(temperature);

                // Draw the element at the node position
                GraphCanvas.Children.Add(new Label()
                {
                    Background = new SolidColorBrush(temperatureColor),
                    Margin = new Thickness(x, y, 0, 0),
                    Content = $"{brightness}%"
                });

                // Draw gradient stop
                BackgroundBrush.GradientStops.Add(new GradientStop()
                {
                    Color = temperatureColor,
                    Offset = percentageWidth
                });

                // Draw horizontal and vertical lines to indicate on axis/scales where they are exactly (helpers)
                GraphCanvas.Children.Add(new Rectangle()
                {
                    Fill = new SolidColorBrush(Colors.LightGray),
                    Margin = new Thickness(x, 0, 0, 0),
                    Width = 1,
                    Height = height - HorizontalAxisHeightMargin
                });

                GraphCanvas.Children.Add(new Rectangle()
                {
                    Fill = new SolidColorBrush(Colors.LightGray),
                    Margin = new Thickness(VerticalAxisWidthMargin, y, 0, 0),
                    Width = width - VerticalAxisWidthMargin,
                    Height = 1
                });
            }

            _updateProfileCompletEvent.Set();
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
