using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mlux.Wpf
{
    public class WindowManager
    {
        public void Start(bool startHidden)
        {
            var window = new MainWindow();
            window.ShowDialog();
        }
    }
}
