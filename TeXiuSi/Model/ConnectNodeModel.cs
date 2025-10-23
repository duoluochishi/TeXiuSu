using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeXiuSi.Model
{
    public class ConnectNodeModel : INotifyPropertyChanged
    {
        public ConnectNodeModel()
        {


        }

        public event PropertyChangedEventHandler PropertyChanged;



        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        // 备注：连接类型  0：CAN 1:IP
        private int _type;
        public int Type
        {
            get { return _type; }
            set { _type = value; OnPropertyChanged("Type"); }
        }


        // 备注：地址
        private string _address;
        public string Address
        {
            get { return _address; }
            set { _address = value; OnPropertyChanged("Address"); }
        }

    }
}
