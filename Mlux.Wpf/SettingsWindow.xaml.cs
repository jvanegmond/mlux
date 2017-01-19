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
using System.Windows.Shapes;
using Mlux.Lib.Time;

namespace Mlux.Wpf
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private TimeProfile _profile;

        public SettingsWindow()
        {
            InitializeComponent();
        }

        public TimeProfile Profile
        {
            get { return _profile; }
            set
            {
                _profile = value;
                SettingsGraph.Profile = _profile;
            }
        }
    }
}
