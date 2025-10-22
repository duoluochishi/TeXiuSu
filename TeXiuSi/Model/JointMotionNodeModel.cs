using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace TeXiuSi.Model
{
    public class JointMotionNodeModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;



        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // 备注：状态码
        private int _id;
        public int Id
        {
            get { return _id; }
            set { _id = value; OnPropertyChanged("Id"); }
        }


        // 备注：线速度
        private string _linearVelocity;
        public string LinearVelocity
        {
            get { return _linearVelocity; }
            set { _linearVelocity = value; OnPropertyChanged("LinearVelocity"); }
        }

        // 备注：角速度
        private string _angularVelocity;
        public string AngularVelocity
        {
            get { return _angularVelocity; }
            set { _angularVelocity = value; OnPropertyChanged("AngularVelocity"); }
        }
        // 备注：线性加速度
        private string _linearAcceleration;
        public string LinearAcceleration
        {
            get { return _linearAcceleration; }
            set { _linearAcceleration = value; OnPropertyChanged("LinearAcceleration"); }
        }
        // 备注：角加速度
        private string _angularAcceleration;
        public string AngularAcceleration
        {
            get { return _angularAcceleration; }
            set { _angularAcceleration = value; OnPropertyChanged("AngularAcceleration"); }
        }

    }
}
