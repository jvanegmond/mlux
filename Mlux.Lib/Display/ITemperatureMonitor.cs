using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mlux.Lib.Display
{
    public interface ITemperatureMonitor : IMonitor
    {
        int GetTemperature();
        void SetTemperature(int temperature);
    }
}
