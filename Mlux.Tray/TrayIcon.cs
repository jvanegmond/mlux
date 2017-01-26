using System;
using System.Drawing;
using System.Windows.Forms;
using NLog;

namespace Mlux.Tray
{
    public delegate void TrayIconClickedDelegate(TrayIcon icon, EventArgs e);

    public class TrayIcon : IDisposable
    {
        public event TrayIconClickedDelegate ExitClick;
        public event TrayIconClickedDelegate MainClick;

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private NotifyIcon _trayIcon;

        public TrayIcon()
        {
            Log.Debug("Loading tray icon");
            
            var trayMenu = new ContextMenu();
            trayMenu.MenuItems.Add("Exit", TrayIconOnExit);
            
            _trayIcon = new NotifyIcon
            {
                Text = "Mlux Demo",
                Icon = new Icon(SystemIcons.Application, 40, 40)
            };
            _trayIcon.Click += TrayIconOnClick;
            
            _trayIcon.ContextMenu = trayMenu;
            _trayIcon.Visible = true;
        }

        private void TrayIconOnExit(object sender, EventArgs e)
        {
            ExitClick?.Invoke(this, new EventArgs());
        }

        private void TrayIconOnClick(object sender, EventArgs e)
        {
            Log.Info("Tray icon click");

            MainClick?.Invoke(this, new EventArgs());
        }

        public void Dispose()
        {
            _trayIcon?.Dispose();
            _trayIcon = null;
        }
    }
}