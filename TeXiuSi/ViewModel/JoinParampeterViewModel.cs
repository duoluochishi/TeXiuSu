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
    public class JoinParampeterViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<JointParameterNodel> _jointParameters;
        public ObservableCollection<JointParameterNodel> JointParameterName
        {
            get { return _jointParameters; }
            set { _jointParameters = value; OnPropertyChanged("JointParameterName"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }



        public JoinParampeterViewModel() {
            JointParameterName = new ObservableCollection<JointParameterNodel>();

            JointParameterNodel jointMotionNode1 = new JointParameterNodel();

            jointMotionNode1.Id = 1;
            // 状态和保护
            jointMotionNode1.CanineStateCode = "0";          // 状态码
            jointMotionNode1.ErrorStatus = "0";             // 错误状态
            jointMotionNode1.EnabledState = "0";            // 使能状态
            jointMotionNode1.CommunicationStatus = "0";     // 通信状态
            jointMotionNode1.CollisionProtection = "0";     // 碰撞保护状态
            jointMotionNode1.RotorBlockingProtection = "0"; // 转子堵转保护状态
            jointMotionNode1.AngleState = "0";              // 角度状态

            // 运动参数
            jointMotionNode1.PositionDegree = "0";          // 位置度数
            jointMotionNode1.MotorSpeed = "0";              // 电机速度
            jointMotionNode1.AngularAcceleration = "0";     // 角加速度

            // 电气和温度
            jointMotionNode1.PowerSupplyVoltage = "0";      // 供电电压
            jointMotionNode1.WiringCurrent = "0";           // 接线电流
            jointMotionNode1.MotorTemperature = "0";        // 电机温度
            jointMotionNode1.DriverTemperature = "0";       // 驱动器温度

            JointParameterName.Add(jointMotionNode1);

            JointParameterNodel jointMotionNode2 = new JointParameterNodel();
            jointMotionNode1.Id = 2;
            // 状态和保护
            jointMotionNode2.CanineStateCode = "0";
            jointMotionNode2.ErrorStatus = "0";
            jointMotionNode2.EnabledState = "0";
            jointMotionNode2.CommunicationStatus = "0";
            jointMotionNode2.CollisionProtection = "0";
            jointMotionNode2.RotorBlockingProtection = "0";
            jointMotionNode2.AngleState = "0";

            // 运动参数
            jointMotionNode2.PositionDegree = "0";
            jointMotionNode2.MotorSpeed = "0";
            jointMotionNode2.AngularAcceleration = "0";

            // 电气和温度
            jointMotionNode2.PowerSupplyVoltage = "0";
            jointMotionNode2.WiringCurrent = "0";
            jointMotionNode2.MotorTemperature = "0";
            jointMotionNode2.DriverTemperature = "0";

            JointParameterName.Add(jointMotionNode2);
            JointParameterNodel jointMotionNode3 = new JointParameterNodel();
            jointMotionNode1.Id = 3;
            // 状态和保护
            jointMotionNode3.CanineStateCode = "0";
            jointMotionNode3.ErrorStatus = "0";
            jointMotionNode3.EnabledState = "0";
            jointMotionNode3.CommunicationStatus = "0";
            jointMotionNode3.CollisionProtection = "0";
            jointMotionNode3.RotorBlockingProtection = "0";
            jointMotionNode3.AngleState = "0";

            // 运动参数
            jointMotionNode3.PositionDegree = "0";
            jointMotionNode3.MotorSpeed = "0";
            jointMotionNode3.AngularAcceleration = "0";

            // 电气和温度
            jointMotionNode3.PowerSupplyVoltage = "0";
            jointMotionNode3.WiringCurrent = "0";
            jointMotionNode3.MotorTemperature = "0";
            jointMotionNode3.DriverTemperature = "0";

            JointParameterName.Add(jointMotionNode3);
            JointParameterNodel jointMotionNode4 = new JointParameterNodel();
            jointMotionNode1.Id = 4;
            // 状态和保护
            jointMotionNode4.CanineStateCode = "0";
            jointMotionNode4.ErrorStatus = "0";
            jointMotionNode4.EnabledState = "0";
            jointMotionNode4.CommunicationStatus = "0";
            jointMotionNode4.CollisionProtection = "0";
            jointMotionNode4.RotorBlockingProtection = "0";
            jointMotionNode4.AngleState = "0";

            // 运动参数
            jointMotionNode4.PositionDegree = "0";
            jointMotionNode4.MotorSpeed = "0";
            jointMotionNode4.AngularAcceleration = "0";

            // 电气和温度
            jointMotionNode4.PowerSupplyVoltage = "0";
            jointMotionNode4.WiringCurrent = "0";
            jointMotionNode4.MotorTemperature = "0";
            jointMotionNode4.DriverTemperature = "0";
            JointParameterName.Add(jointMotionNode4);
            JointParameterNodel jointMotionNode5 = new JointParameterNodel();
            jointMotionNode1.Id = 5;
            // 状态和保护
            jointMotionNode5.CanineStateCode = "0";          // 狗的状态码
            jointMotionNode5.ErrorStatus = "0";             // 错误状态
            jointMotionNode5.EnabledState = "0";            // 使能状态
            jointMotionNode5.CommunicationStatus = "0";     // 通信状态
            jointMotionNode5.CollisionProtection = "0";     // 碰撞保护状态
            jointMotionNode5.RotorBlockingProtection = "0"; // 转子堵转保护状态
            jointMotionNode5.AngleState = "0";              // 角度状态

            // 运动参数
            jointMotionNode5.PositionDegree = "0";          // 位置度数
            jointMotionNode5.MotorSpeed = "0";              // 电机速度
            jointMotionNode5.AngularAcceleration = "0";     // 角加速度

            // 电气和温度
            jointMotionNode5.PowerSupplyVoltage = "0";      // 供电电压
            jointMotionNode5.WiringCurrent = "0";           // 接线电流
            jointMotionNode5.MotorTemperature = "0";        // 电机温度
            jointMotionNode5.DriverTemperature = "0";       // 驱动器温度
            JointParameterName.Add(jointMotionNode5);
            JointParameterNodel jointMotionNode6 = new JointParameterNodel();
            jointMotionNode1.Id = 6;
            // 状态和保护
            jointMotionNode6.CanineStateCode = "0";
            jointMotionNode6.ErrorStatus = "0";
            jointMotionNode6.EnabledState = "0";
            jointMotionNode6.CommunicationStatus = "0";
            jointMotionNode6.CollisionProtection = "0";
            jointMotionNode6.RotorBlockingProtection = "0";
            jointMotionNode6.AngleState = "0";

            // 运动参数
            jointMotionNode6.PositionDegree = "0";
            jointMotionNode6.MotorSpeed = "0";
            jointMotionNode6.AngularAcceleration = "0";

            // 电气和温度
            jointMotionNode6.PowerSupplyVoltage = "0";
            jointMotionNode6.WiringCurrent = "0";
            jointMotionNode6.MotorTemperature = "0";
            jointMotionNode6.DriverTemperature = "0";
            JointParameterName.Add(jointMotionNode6);
        }



    }
}
