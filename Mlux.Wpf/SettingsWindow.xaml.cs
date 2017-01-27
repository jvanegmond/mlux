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
using Mlux.Wpf.Bindings;

namespace Mlux.Wpf
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private readonly TimeProfileView _timeProfile;

        public SettingsWindow(TimeProfileView timeProfile)
        {
            _timeProfile = timeProfile;
            InitializeComponent();

            SettingsGraph.Profile = _timeProfile;
        }
    }
}
