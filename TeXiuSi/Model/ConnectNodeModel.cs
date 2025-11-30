using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeXiuSi.Model
{
    public class ConnectNodeModel :  INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public ConnectNodeModel()
        {


        }
       
        // 备注：连接类型  0：CAN 1:IP
        private int _type = 1; // 初始值
        public int Type
        {
            get { return _type; }
            set { _type = value; OnPropertyChanged("Type"); }
        }
        // 备注：名称
        private string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; OnPropertyChanged("Name"); }
        }

        // 备注：地址
        private string _address;
        public string Address
        {
            get { return _address; }
            set { _address = value; OnPropertyChanged("Address"); }
        }
        //public override string ToString()
        //{
        //    // 当 ComboBox 选中一个 ConnectNodeModel 对象时，它会调用这个方法来获取要显示在主区域的文本。
        //    return this.Name;
        //}
    }
}
