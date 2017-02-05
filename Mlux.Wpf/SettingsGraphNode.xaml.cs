using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using Mlux.Wpf.Bindings;

namespace Mlux.Wpf
{
    /// <summary>
    /// Interaction logic for SetttingsGraphNode.xaml
    /// </summary>
    public partial class SettingsGraphNode : UserControl
    {
        private const int AllowedOverlapPixels = 3;

        private NodeType _draggingType = NodeType.None;

        public NodeType DraggingType
        {
            get
            {
                return _draggingType;

            }
            set
            {
                _draggingType = value;
                if (_draggingType == NodeType.None)
                {
                    // Stop
                    Divider.Fill = new SolidColorBrush(Colors.Gray);
                }
                else
                {
                    // Start
                    Divider.Fill = new SolidColorBrush(Colors.Black);
                }
            }
        }

        public TimeNodeView Node { get; }

        public SettingsGraphNode(TimeNodeView node)
        {
            Node = node;

            InitializeComponent();
            Cursor = Cursors.SizeWE;

            Loaded += (sender, args) => SetPositions();
            Node.PropertyChanged += (sender, args) =>
            {
                if (DraggingType == NodeType.None)
                {
                    SetPositions();
                }
            };

            Divider.MouseLeftButtonDown += (sender, args) => DraggingType = NodeType.Time;
            BrightnessNode.MouseLeftButtonDown += (sender, args) => DraggingType = NodeType.Brightness;
            TemperatureNode.MouseLeftButtonDown += (sender, args) => DraggingType = NodeType.Temperature;
        }

        private void SetPositions()
        {
            if (Parent == null) return;

            var width = ((Panel)Parent).ActualWidth;
            var height = ((Panel)Parent).ActualHeight;

            Height = height;

            // Set margin to left to set it correctly on time
            var percentageWidth = Node.TimeOfDay.TotalSeconds / TimeSpan.FromDays(1).TotalSeconds;
            var timeMargin = (percentageWidth * width) - (ActualWidth / 2);

            Margin = new Thickness(timeMargin, 0, 0, 0);

            // As the margin is calculated from the top of the element. This prevents setting it to full height (at 0% of value) will push it out of the parent element's area.
            var nodeHeight = BrightnessNode.ActualHeight;
            height -= nodeHeight;

            // Calculate brightness and temperature vertical margins (from the top)
            var brightnessPercentage = ((double)Node.Brightness - TimeProfile.MinBrightness) / (TimeProfile.MaxBrightness - TimeProfile.MinBrightness);
            var brightnessMargin = (1d - brightnessPercentage) * height;

            var temperaturePercentage = ((double)Node.Temperature - TimeProfile.MinTemperature) / (TimeProfile.MaxTemperature - TimeProfile.MinTemperature);
            var temperatureMargin = (1d - temperaturePercentage) * height;

            // Check if the margins are possible colliding the two nodes together
            var nodeWidth = BrightnessNode.ActualWidth;
            var horizontalOffset = 0d;
            if (Math.Abs(brightnessMargin - temperatureMargin) < (nodeHeight - AllowedOverlapPixels))
            {
                horizontalOffset = nodeWidth + 1;
            }

            // Margins are calculated, set it
            BrightnessNode.Margin = new Thickness(-1 * horizontalOffset, brightnessMargin, 0, 0);
            TemperatureNode.Margin = new Thickness(horizontalOffset, temperatureMargin, 0, 0);
        }

        public void Drag(MouseEventArgs e)
        {
            // Do not process mouse move events when we are not dragging anything (our parent should catch this but still better check)
            if (DraggingType == NodeType.None) return;

            var pos = e.GetPosition((Panel)Parent);

            var width = ((Panel)Parent).ActualWidth;
            var height = ((Panel)Parent).ActualHeight;

            // Set TimeOfDay correctly
            if (DraggingType == NodeType.Time)
            {
                var percentageX = pos.X/width;
                Node.TimeOfDay = TimeSpan.FromSeconds(TimeSpan.FromDays(1).TotalSeconds*percentageX);
            }

            // Set values correctly (Brightness or Temperature)
            var nodeHeight = BrightnessNode.ActualHeight;
            height -= nodeHeight;

            var percentageY = 1d - ((pos.Y - (nodeHeight / 2)) / height);

            if (DraggingType == NodeType.Brightness)
            {
                Node.Brightness = (int)GetValue(percentageY, TimeProfile.MinBrightness, TimeProfile.MaxBrightness);
            }

            // Temperature
            if (DraggingType == NodeType.Temperature)
            {
                Node.Temperature = (int)GetValue(percentageY, TimeProfile.MinTemperature, TimeProfile.MaxTemperature);
            }

            SetPositions();
        }

        private static double GetValue(double percentage, double min, double max)
        {
            var difference = max - min;

            percentage *= difference;
            percentage += min;

            if (percentage < min) return min;
            if (percentage > max) return max;

            return percentage;
        }
    }
}
