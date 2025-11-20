using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using TeXiuSi.Helper;

namespace TeXiuSi.Model
{
    public class Joint : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;


        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Model3D model = null;
        public double angle = 0;
        public double angleMin = -180;
        public double angleMax = 180;
        public int rotPointX = 0;
        public int rotPointY = 0;
        public int rotPointZ = 0;
        public int rotAxisX = 0;
        public int rotAxisY = 0;
        public int rotAxisZ = 0;

        // 备注：节点类型  0：4310 1:4340
        private int _id;
        public int ID
        {
            get { return _id; }
            set { _id = value; OnPropertyChanged("ID"); }
        }

        // 备注：节点类型  0：4310 1:4340
        private int _type;
        public int Type
        {
            get { return _type; }
            set { _type = value; OnPropertyChanged("Type"); }
        }

        private JointNode _jointInfo;
        public JointNode JointInfo
        {
            get { return _jointInfo; }
            set { _jointInfo = value; OnPropertyChanged("JointInfo"); }
        }
        // 备注：名称
        private string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; OnPropertyChanged("Name"); }
        }

        public Joint(Model3D pModel)
        {
            model = pModel;
        }
        public Joint()
        {
           
        }

    }
}
