using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quobject.SocketIoClientDotNet.Client;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.ComponentModel;

namespace WpfApplication1
{
    public class Ketnoi : INotifyPropertyChanged
    {
        public string mes = string.Empty;
        public int _x;
        public int _y;
        public int _player;

        public event PropertyChangedEventHandler PropertyChanged;

        public Ketnoi() { }

        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public string Message
        {
            get
            {
                return mes;
            }
            set
            {
                if(value != this.mes)
                {
                    this.mes = value;
                    NotifyPropertyChanged("Message");
                }
            }
        }
    }
}
