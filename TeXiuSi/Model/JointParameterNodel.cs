using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeXiuSi.Model
{
    public class JointParameterNodel:INotifyPropertyChanged
    {
        public JointParameterNodel() { 

        
        }

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

        // 备注：状态码
        private string _canineStateCode;
        public string CanineStateCode
        {
            get { return _canineStateCode; }
            set { _canineStateCode = value; OnPropertyChanged("CanineStateCode"); }
        }

        //---
        // 备注：电机速度
        private string _motorSpeed;
        public string MotorSpeed
        {
            get { return _motorSpeed; }
            set { _motorSpeed = value; OnPropertyChanged("MotorSpeed"); }
        }

        //---
        // 备注：位置度数
        private string _positionDegree;
        public string PositionDegree
        {
            get { return _positionDegree; }
            set { _positionDegree = value; OnPropertyChanged("PositionDegree"); }
        }

        //---

        // 备注：电源电压
        private string _powerSupplyVoltage;
        public string PowerSupplyVoltage
        {
            get { return _powerSupplyVoltage; }
            set { _powerSupplyVoltage = value; OnPropertyChanged("PowerSupplyVoltage"); }
        }

        //---

        // 备注：接线电流
        private string _wiringCurrent;
        public string WiringCurrent
        {
            get { return _wiringCurrent; }
            set { _wiringCurrent = value; OnPropertyChanged("WiringCurrent"); }
        }

        //---

        // 备注：角加速度
        private string _angularAcceleration;
        public string AngularAcceleration
        {
            get { return _angularAcceleration; }
            set { _angularAcceleration = value; OnPropertyChanged("AngularAcceleration"); }
        }

        //---
        // 备注：电机温度
        private string _motorTemperature;
        public string MotorTemperature
        {
            get { return _motorTemperature; }
            set { _motorTemperature = value; OnPropertyChanged("MotorTemperature"); }
        }

        //---
        // 备注：驱动器温度
        private string _driverTemperature;
        public string DriverTemperature
        {
            get { return _driverTemperature; }
            set { _driverTemperature = value; OnPropertyChanged("DriverTemperature"); }
        }

        //---

        // 备注：碰撞保护状态
        private string _collisionProtection;
        public string CollisionProtection
        {
            get { return _collisionProtection; }
            set { _collisionProtection = value; OnPropertyChanged("CollisionProtection"); }
        }

        //---

        // 备注：转子堵转保护状态
        private string _rotorBlockingProtection;
        public string RotorBlockingProtection
        {
            get { return _rotorBlockingProtection; }
            set { _rotorBlockingProtection = value; OnPropertyChanged("RotorBlockingProtection"); }
        }

        //---

        // 备注：错误状态
        private string _errorStatus;
        public string ErrorStatus
        {
            get { return _errorStatus; }
            set { _errorStatus = value; OnPropertyChanged("ErrorStatus"); }
        }

        //---
        // 备注：使能状态
        private string _enabledState;
        public string EnabledState
        {
            get { return _enabledState; }
            set { _enabledState = value; OnPropertyChanged("EnabledState"); }
        }

        //---
        // 备注：通信状态
        private string _communicationStatus;
        public string CommunicationStatus
        {
            get { return _communicationStatus; }
            set { _communicationStatus = value; OnPropertyChanged("CommunicationStatus"); }
        }

        //---
        // 备注：角度状态
        private string _angleState;
        public string AngleState
        {
            get { return _angleState; }
            set { _angleState = value; OnPropertyChanged("AngleState"); }
        }
    }
}
