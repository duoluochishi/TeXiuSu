using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeXiuSi.Model;

namespace TeXiuSi.ViewModel
{
    public class JoinMotionParametersViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<JointMotionNodeModel> _jointMotions;
        public ObservableCollection<JointMotionNodeModel> JointMotionsName
        {
            get { return _jointMotions; }
            set { _jointMotions = value; OnPropertyChanged("JointMotionsName"); }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public JoinMotionParametersViewModel() {
            
            // 假设 JointMotionNodeModel 和 JointMotionsName 已在作用域内定义

            JointMotionsName = new ObservableCollection<JointMotionNodeModel>();

            JointMotionNodeModel jointMotionNode1 = new JointMotionNodeModel();
            jointMotionNode1.Id = 1;
            // 状态和保护
            jointMotionNode1.LinearVelocity = "0";      // 线速度
            jointMotionNode1.AngularVelocity = "0";     // 角速度
            jointMotionNode1.LinearAcceleration = "0";  // 线性加速度
            jointMotionNode1.AngularAcceleration = "0"; // 角加速度
            JointMotionsName.Add(jointMotionNode1);



            // 对象二 (Id = 2)
            JointMotionNodeModel jointMotionNode2 = new JointMotionNodeModel();
            jointMotionNode2.Id = 2;
            jointMotionNode2.LinearVelocity = "0";
            jointMotionNode2.AngularVelocity = "0";
            jointMotionNode2.LinearAcceleration = "0";
            jointMotionNode2.AngularAcceleration = "0";
            JointMotionsName.Add(jointMotionNode2);

            // 对象三 (Id = 3)
            JointMotionNodeModel jointMotionNode3 = new JointMotionNodeModel();
            jointMotionNode3.Id = 3;
            jointMotionNode3.LinearVelocity = "0";
            jointMotionNode3.AngularVelocity = "0";
            jointMotionNode3.LinearAcceleration = "0";
            jointMotionNode3.AngularAcceleration = "0";
            JointMotionsName.Add(jointMotionNode3);

            // 对象四 (Id = 4)
            JointMotionNodeModel jointMotionNode4 = new JointMotionNodeModel();
            jointMotionNode4.Id = 4;
            jointMotionNode4.LinearVelocity = "0";
            jointMotionNode4.AngularVelocity = "0";
            jointMotionNode4.LinearAcceleration = "0";
            jointMotionNode4.AngularAcceleration = "0";
            JointMotionsName.Add(jointMotionNode4);

            // 对象五 (Id = 5)
            JointMotionNodeModel jointMotionNode5 = new JointMotionNodeModel();
            jointMotionNode5.Id = 5;
            jointMotionNode5.LinearVelocity = "0";
            jointMotionNode5.AngularVelocity = "0";
            jointMotionNode5.LinearAcceleration = "0";
            jointMotionNode5.AngularAcceleration = "0";
            JointMotionsName.Add(jointMotionNode5);

            // 对象六 (Id = 6)
            JointMotionNodeModel jointMotionNode6 = new JointMotionNodeModel();
            jointMotionNode6.Id = 6;
            jointMotionNode6.LinearVelocity = "0";
            jointMotionNode6.AngularVelocity = "0";
            jointMotionNode6.LinearAcceleration = "0";
            jointMotionNode6.AngularAcceleration = "0";
            JointMotionsName.Add(jointMotionNode6);

        }
    }
}
