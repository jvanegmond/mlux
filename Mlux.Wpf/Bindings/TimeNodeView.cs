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
        public TimeNode UnderlyingNode { get; set; }
        private int _brightness;
        private int _temperature;
        private TimeSpan _timeOfDay;

        public int Brightness
        {
            get { return _brightness; }
            set
            {
                value = Math.Max(value, TimeProfile.MinBrightness);
                value = Math.Min(value, TimeProfile.MaxBrightness);

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
                value = Math.Max(value, TimeProfile.MinTemperature);
                value = Math.Min(value, TimeProfile.MaxTemperature);

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
                if (value.TotalDays > 1)
                {
                    value = TimeSpan.FromDays(1);
                }
                if (value.TotalDays < 0)
                {
                    value = TimeSpan.Zero;
                }

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

        public TimeNodeView(TimeNode underlyingNode)
        {
            UnderlyingNode = underlyingNode;
            CopyFrom(underlyingNode);

            PropertyChanged += TimeNodeView_PropertyChanged;
        }

        private void TimeNodeView_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (UnderlyingNode == null) return;

            UnderlyingNode.Properties.First(_ => _.Name == NodeProperty.Brightness).Value = Brightness;
            UnderlyingNode.Properties.First(_ => _.Name == NodeProperty.ColorTemperature).Value = Temperature;
            UnderlyingNode.TimeOfDay = TimeOfDay;
        }

        public void CopyFrom(TimeNode node)
        {
            Brightness = (int)node.Properties.First(_ => _.Name == NodeProperty.Brightness).Value;
            Temperature = (int)node.Properties.First(_ => _.Name == NodeProperty.ColorTemperature).Value;
            TimeOfDay = node.TimeOfDay;
        }
    }
}