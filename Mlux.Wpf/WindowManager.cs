using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Mlux.Wpf
{
    public class WindowManager
    {
        public void Start(bool startHidden)
        {
            if (Application.Current == null)
            {
                new Application();
            }

            var window = new MainWindow();
            window.ShowDialog();
        }
    }
}
