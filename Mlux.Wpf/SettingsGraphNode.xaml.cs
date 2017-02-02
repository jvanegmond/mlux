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
using Mlux.Wpf.Bindings;

namespace Mlux.Wpf
{
    /// <summary>
    /// Interaction logic for SetttingsGraphNode.xaml
    /// </summary>
    public partial class SettingsGraphNode : UserControl
    {
        private NodeType _draggingType = NodeType.TimeOnly;
        private TimeNodeView _node;

        public event EventHandler StartDrag;
        public event EventHandler MouseOver;

        public SettingsGraphNode()
        {
            InitializeComponent();
            Cursor = Cursors.SizeWE;

            Loaded += (sender, args) => Redraw();
        }

        public TimeNodeView Node
        {
            get { return _node; }
            set
            {
                _node = value;
                Redraw();
            }
        }

        private void Redraw()
        {
            if (Parent == null) return;

            var width = ((Panel)Parent).ActualWidth;
            var height = ((Panel)Parent).ActualHeight;

            Height = height;

            var percentageWidth = _node.TimeOfDay.TotalSeconds / TimeSpan.FromDays(1).TotalSeconds;
            var x =  percentageWidth * width;

            Margin = new Thickness(x, 0, 0, 0);
        }

        private void TemperatureNode_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _draggingType = NodeType.Temperature;
        }

        private void BrightnessNode_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _draggingType = NodeType.Brightness;
        }

        private void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var width = ((Panel)Parent).ActualWidth;
            var height = ((Panel)Parent).ActualHeight;

            // Start drag

            var percentageWidth = Node.TimeOfDay.TotalSeconds / TimeSpan.FromDays(1).TotalSeconds;
            var x = percentageWidth * width;

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
                Width = 20
            });

            // Draw draggable brightness node
            var brightnessY = (1 - SettingsGraph.GetNodeValuePercentage(SettingsGraph.GetCurrentValue(Node, NodeType.Brightness), NodeType.Brightness)) * height;
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
            var temperatureY = (1 - SettingsGraph.GetNodeValuePercentage(SettingsGraph.GetCurrentValue(Node, NodeType.Temperature), NodeType.Temperature)) * height;
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

            StartDrag?.Invoke(this, EventArgs.Empty);
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
            Mouse.OverrideCursor = Cursors.SizeWE;
        }

        private void DraggableMouseEnter(object sender, MouseEventArgs e)
        {
            // Set the top bar values to the currently mouse-over node
            MouseOver?.Invoke(this, EventArgs.Empty);
        }

    }
}
