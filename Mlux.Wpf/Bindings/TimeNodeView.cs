using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mlux.Lib.Time;

namespace Mlux.Wpf.Bindings
{
    public class TimeNodeView : NotifyPropertyChangedBase
    {
        private int _brightness;
        private int _temperature;
        private TimeSpan _timeOfDay;

        public int Brightness
        {
            get { return _brightness; }
            set
            {
                var changed = _brightness != value;
                _brightness = value;

                if (changed) OnPropertyChanged();
            }
        }

        public int Temperature
        {
            get { return _temperature; }
            set
            {
                var changed = _temperature != value;
                _temperature = value;

                if (changed) OnPropertyChanged();

            }
        }

        public TimeSpan TimeOfDay
        {
            get { return _timeOfDay; }
            set
            {
                var changed = _timeOfDay != value;
                _timeOfDay = value;

                if (changed) OnPropertyChanged();
            }
        }

        public TimeNodeView()
        {
            Brightness = 80;
            Temperature = 6500;
            TimeOfDay = TimeSpan.FromHours(8.5);
        }

        public TimeNodeView(TimeNode node)
        {
            CopyFrom(node);
        }

        public void CopyFrom(TimeNode node)
        {
            Brightness = (int)node.Properties.First(_ => _.Name == NodeProperty.Brightness).Value;
            Temperature = (int)node.Properties.First(_ => _.Name == NodeProperty.ColorTemperature).Value;
            TimeOfDay = node.TimeOfDay;
        }
    }
}