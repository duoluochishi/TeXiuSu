using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeXiuSi.ViewModel
{
    public class JointMotionNode : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        // 备注：线速度
        private string _linearVelocity;
        public string LinearVelocity
        {
            get { return _linearVelocity; }
            set { _linearVelocity = value; OnPropertyChanged("LinearVelocity"); }
        }

        //---

        // 备注：角速度
        private string _angularVelocity;
        public string AngularVelocity
        {
            get { return _angularVelocity; }
            set { _angularVelocity = value; OnPropertyChanged("AngularVelocity"); }
        }

        //---

        // 备注：浅层加速度 (注：此为字面翻译，如果指的是“法向加速度”或“切向加速度”请自行修改备注)
        private string _shallowAcceleration;
        public string ShallowAcceleration
        {
            get { return _shallowAcceleration; }
            set { _shallowAcceleration = value; OnPropertyChanged("ShallowAcceleration"); }
        }

        //---

        // 备注：角加速度 (与上次列表中的重复，但仍按要求生成)
        private string _angularAcceleration;
        public string AngularAcceleration
        {
            get { return _angularAcceleration; }
            set { _angularAcceleration = value; OnPropertyChanged("AngularAcceleration"); }
        }
    }
}
