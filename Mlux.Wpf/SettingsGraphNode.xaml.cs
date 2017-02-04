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
        private NodeType _draggingType = NodeType.None;
        private Point _mouseOffset;

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

            Divider.MouseLeftButtonDown += (sender, args) => StartDrag(NodeType.TimeOnly, args);
            BrightnessNode.MouseLeftButtonDown += (sender, args) => StartDrag(NodeType.Brightness, args);
            TemperatureNode.MouseLeftButtonDown += (sender, args) => StartDrag(NodeType.Temperature, args);
        }

        private void StartDrag(NodeType type, MouseButtonEventArgs args)
        {
            var uiElement = GetUiElementForType(type);

            _mouseOffset = args.GetPosition(uiElement);

            DraggingType = type;
        }

        private IInputElement GetUiElementForType(NodeType type)
        {
            switch (type)
            {
                case NodeType.TimeOnly:
                    return Divider;
                case NodeType.Brightness:
                    return BrightnessNode;
                case NodeType.Temperature:
                    return TemperatureNode;
            }
            return null;
        }

        private void SetPositions()
        {
            if (Parent == null) return;

            var width = ((Panel)Parent).ActualWidth;
            var height = ((Panel)Parent).ActualHeight;

            Height = height;

            // Set margin to left to set it correctly on time
            var percentageWidth = Node.TimeOfDay.TotalSeconds / TimeSpan.FromDays(1).TotalSeconds;
            var x = (percentageWidth * width) + (ActualWidth / 2);

            Margin = new Thickness(x, 0, 0, 0);

            // Set margin on brightness
            var startOffset = BrightnessNode.ActualHeight;
            height -= startOffset * 2;

            var percentage = ((double)Node.Brightness - TimeProfile.MinBrightness) / TimeProfile.MaxBrightness;
            BrightnessNode.Margin = new Thickness(0, startOffset + ((1d - percentage) * height), 0, 0);

            // Set margin on temperature
            percentage = ((double)Node.Temperature - TimeProfile.MinTemperature) / TimeProfile.MaxTemperature;
            TemperatureNode.Margin = new Thickness(0, startOffset + ((1d - percentage) * height), 0, 0);
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

        public void Drag(MouseEventArgs e)
        {
            var pos = e.GetPosition((Panel)Parent);

            var width = ((Panel)Parent).ActualWidth;
            var height = ((Panel)Parent).ActualHeight;

            var percentageX = pos.X / width;
            var percentageY = 1d - (pos.Y / height);

            // Time
            Node.TimeOfDay = TimeSpan.FromSeconds(TimeSpan.FromDays(1).TotalSeconds * percentageX);

            // Brightness
            if (DraggingType == NodeType.Brightness)
            {
                Node.Brightness = (int)GetValue(percentageY, TimeProfile.MinBrightness, TimeProfile.MaxBrightness);
            }

            // Temperature
            if (DraggingType == NodeType.Temperature)
            {
                //Node.Temperature = (int)Clamp(percentageY * 100d, TimeProfile.MinBrightness, TimeProfile.MaxBrightness);
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
