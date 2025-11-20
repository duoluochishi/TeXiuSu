
#define IRB6700
using HelixToolkit.Wpf;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using TeXiuSi.Helper;
using TeXiuSi.Model;
using TeXiuSi.Protocol;

namespace TeXiuSi
{



    public class MotorControl : IDisposable
    {
        private readonly Dictionary<ushort, Motor> _motors;
        private readonly IUsbHardware _usbHw;
        private readonly List<DmActData> _dataPtr;
        private bool _readWriteSave;
        private bool _disposed = false;
        private List<CANCommand> commandList = new List<CANCommand>();



        #region
        public List<string> modelsNames;
        public Color oldColor = Colors.White;
        public GeometryModel3D oldSelectedModel = null;

        string basePath = "";
        //provides functionality to 3d models
        //这是一个Model3DGroup对象，它像一个容器，把机械臂的所有独立部件（如底座、大臂、小臂等）组合在一起，方便统一管理。
        Model3DGroup RA = new Model3DGroup(); //RoboticArm 3d group
#if IRB6700
        //directroy of all stl files
        private const string MODEL_PATH1 = "IRB6700-MH3_245-300_IRC5_rev02_LINK01_CAD.stl";
        private const string MODEL_PATH2 = "IRB6700-MH3_245-300_IRC5_rev00_LINK02_CAD.stl";
        private const string MODEL_PATH3 = "IRB6700-MH3_245-300_IRC5_rev02_LINK03_CAD.stl";
        private const string MODEL_PATH4 = "IRB6700-MH3_245-300_IRC5_rev01_LINK04_CAD.stl";
        private const string MODEL_PATH5 = "IRB6700-MH3_245-300_IRC5_rev01_LINK05_CAD.stl";
        private const string MODEL_PATH6 = "IRB6700-MH3_245-300_IRC5_rev01_LINK06_CAD.stl";
        private const string MODEL_PATH7 = "IRB6700-MH3_245-300_IRC5_rev02_LINK01_CABLE.stl";
        private const string MODEL_PATH8 = "IRB6700-MH3_245-300_IRC5_rev02_LINK01m_CABLE.stl";
        private const string MODEL_PATH9 = "IRB6700-MH3_245-300_IRC5_rev00_LINK02_CABLE.stl";
        private const string MODEL_PATH10 = "IRB6700-MH3_245-300_IRC5_rev00_LINK02m_CABLE.stl";
        private const string MODEL_PATH11 = "IRB6700-MH3_245-300_IRC5_rev00_LINK03a_CABLE.stl";
        private const string MODEL_PATH12 = "IRB6700-MH3_245-300_IRC5_rev00_LINK03b_CABLE.stl";
        private const string MODEL_PATH13 = "IRB6700-MH3_245-300_IRC5_rev02_LINK03m_CABLE.stl";
        private const string MODEL_PATH14 = "IRB6700-MH3_245-300_IRC5_rev01_LINK04_CABLE.stl";
        private const string MODEL_PATH15 = "IRB6700-MH3_245-300_IRC5_rev00_ROD_CAD.stl";
        private const string MODEL_PATH16 = "IRB6700-MH3_245-300_IRC5_rev00_LOGO1_CAD.stl";
        private const string MODEL_PATH17 = "IRB6700-MH3_245-300_IRC5_rev00_LOGO2_CAD.stl";
        private const string MODEL_PATH18 = "IRB6700-MH3_245-300_IRC5_rev00_LOGO3_CAD.stl";
        private const string MODEL_PATH19 = "IRB6700-MH3_245-300_IRC5_rev01_BASE_CAD.stl";
        private const string MODEL_PATH20 = "IRB6700-MH3_245-300_IRC5_rev00_CYLINDER_CAD.stl";
#else

        private const string MODEL_PATH1 = "IRB4600_20kg-250_LINK1_CAD_rev04.stl";
        private const string MODEL_PATH2 = "IRB4600_20kg-250_LINK2_CAD_rev04.stl";
        private const string MODEL_PATH3 = "IRB4600_20kg-250_LINK3_CAD_rev005.stl";
        private const string MODEL_PATH4 = "IRB4600_20kg-250_LINK4_CAD_rev04.stl";
        private const string MODEL_PATH5 = "IRB4600_20kg-250_LINK5_CAD_rev04.stl";
        private const string MODEL_PATH6 = "IRB4600_20kg-250_LINK6_CAD_rev04.stl";
        private const string MODEL_PATH7 = "IRB4600_20kg-250_LINK3_CAD_rev04.stl";
        private const string MODEL_PATH8 = "IRB4600_20kg-250_CABLES_LINK1_rev03.stl";
        private const string MODEL_PATH9 = "IRB4600_20kg-250_CABLES_LINK2_rev03.stl";
        private const string MODEL_PATH10 = "IRB4600_20kg-250_CABLES_LINK3_rev03.stl";
        private const string MODEL_PATH11 = "IRB4600_20kg-250_BASE_CAD_rev04.stl";
#endif
        #endregion

        public MotorControl(uint nomBaud, uint datBaud, string sn, List<DmActData> dataPtr)
        {
            _motors = new Dictionary<ushort, Motor>();
            _dataPtr = dataPtr;

            // 初始化所有电机
            foreach (var data in _dataPtr)
            {
                var motor = new Motor(data.MotorType, data.Mode, data.CanId, data.MstId);
                AddMotor(motor);
            }

            // 这里需要根据实际硬件创建USB实例
            // _usbHw = new ConcreteUsbHardware(nomBaud, datBaud, sn);
            _usbHw = new DummyUsbHardware(); // 临时使用虚拟实现

            Thread.Sleep(500);

            _usbHw.SetFrameCallback(CanFrameCallback);
            Thread.Sleep(200);

            EnableAll();
            Console.WriteLine("**********Motor_Control initialization successful**********\n");
        }

        public MotorControl()
        {
            InitModel();

        }


        ~MotorControl()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Console.WriteLine("Enter ~Motor_Control");
                    DisableAll();
                    _usbHw?.Dispose();
                }
                _disposed = true;
            }
        }

        public void AddMotor(Motor motor)
        {
            _motors[motor.CanId] = motor;
            _motors[motor.MasterId] = motor;
        }

        public void EnableAll()
        {
            // 切换控制模式到MIT
            foreach (var motor in _motors.Values)
            {
                if (!_motors.ContainsKey(motor.CanId)) continue;

                SwitchControlMode(motor, Control_Mode.MIT);
                Thread.Sleep(2);
            }

            // 读取参数
            foreach (var motor in _motors.Values)
            {
                if (!_motors.ContainsKey(motor.CanId)) continue;

                for (int j = 0; j < 5; j++)
                {
                    ReadMotorParam(motor, 10);
                    Thread.Sleep(2);
                }
            }

            // 输出模式信息
            foreach (var motor in _motors.Values)
            {
                if (!_motors.ContainsKey(motor.CanId)) continue;

                uint param = motor.GetParamAsUint32(10);
                Console.Error.WriteLine($"id: {motor.CanId} mode is: {param}");
            }

            // 使能所有电机
            foreach (var motor in _motors.Values)
            {
                if (!_motors.ContainsKey(motor.CanId)) continue;

                for (int j = 0; j < 5; j++)
                {
                    ControlCmd((ushort)(motor.CanId + (int)motor.GetMotorMode()), 0xFC);
                    Thread.Sleep(2);
                }
            }
        }

        public void DisableAll()
        {
            foreach (var motor in _motors.Values)
            {
                if (!_motors.ContainsKey(motor.CanId)) continue;

                for (int j = 0; j < 5; j++)
                {
                    ControlCmd((ushort)(motor.CanId + (int)motor.GetMotorMode()), 0xFD);
                    Thread.Sleep(2);
                }
            }
        }

        public float ReadMotorParam(Motor motor, byte rid)
        {
            _readWriteSave = true;
            ushort id = motor.CanId;
            byte idLow = (byte)(id & 0xFF);
            byte idHigh = (byte)((id >> 8) & 0xFF);

            byte[] data = { idLow, idHigh, 0x33, rid, 0x00, 0x00, 0x00, 0x00 };
            _usbHw.FdcanFrameSend(data, 0x7FF);
            Thread.Sleep(2);
            return 0f;
        }

        public void SaveMotorParam(Motor motor)
        {
            ushort id = motor.CanId;
            Control_Mode_Code mode = motor.GetMotorMode();
            ControlCmd((ushort)(id + (int)mode), 0xFD);
            Thread.Sleep(10);

            _readWriteSave = true;
            byte idLow = (byte)(id & 0xFF);
            byte idHigh = (byte)((id >> 8) & 0xFF);

            byte[] data = { idLow, idHigh, 0xAA, 0x01, 0x00, 0x00, 0x00, 0x00 };
            _usbHw.FdcanFrameSend(data, 0x7FF);
            Thread.Sleep(100);
        }

        public void RefreshMotorStatus(Motor motor)
        {
            byte idLow = (byte)(motor.CanId & 0xFF);
            byte idHigh = (byte)((motor.CanId >> 8) & 0xFF);

            byte[] data = { idLow, idHigh, 0xCC, 0x00, 0x00, 0x00, 0x00, 0x00 };
            _usbHw.FdcanFrameSend(data, 0x7FF);
        }

        /// <summary>
        /// 失能
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cmd"></param>
        public void ControlCmd(ushort id, byte cmd)
        {
            byte[] data = { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, cmd };
            _usbHw.FdcanFrameSend(data, id);
        }

        /// <summary>
        /// 写入参数模式
        /// </summary>
        /// <param name="motor"></param>
        /// <param name="rid"></param>
        /// <param name="data"></param>
        public void WriteMotorParam(Motor motor, byte rid, byte[] data)
        {
            _readWriteSave = true;
            ushort id = motor.CanId;
            byte idLow = (byte)(id & 0xFF);
            byte idHigh = (byte)((id >> 8) & 0xFF);

            byte[] sendData = new byte[8];
            sendData[0] = idLow;
            sendData[1] = idHigh;
            sendData[2] = 0x55;
            sendData[3] = rid;
            Array.Copy(data, 0, sendData, 4, Math.Min(data.Length, 4));

            _usbHw.FdcanFrameSend(sendData, 0x7FF);
        }

        public void SetZeroPosition(Motor motor)
        {
            ControlCmd((ushort)(motor.CanId + (int)motor.GetMotorMode()), 0xFE);
        }

        public void ControlMit(Motor motor, float kp, float kd, float q, float dq, float tau)
        {
            ushort id = motor.CanId;
            if (!_motors.ContainsKey(id))
            {
                Console.Error.WriteLine($"[Error] In control_mit,no motor with id {motor.CanId} is registered.");
                Environment.Exit(-1);
            }

            var m = _motors[id];
            ushort kpUint = FloatToUint(kp, 0, 500, 12);
            ushort kdUint = FloatToUint(kd, 0, 5, 12);
            Limit_param limitParamCmd = m.LimitParam;
            ushort qUint = FloatToUint(q, -limitParamCmd.Q_MAX, limitParamCmd.Q_MAX, 16);
            ushort dqUint = FloatToUint(dq, -limitParamCmd.DQ_MAX, limitParamCmd.DQ_MAX, 12);
            ushort tauUint = FloatToUint(tau, -limitParamCmd.TAU_MAX, limitParamCmd.TAU_MAX, 12);

            ushort canId = (ushort)(id + (int)Control_Mode_Code.MIT_MODE);
            byte[] data = new byte[8];
            data[0] = (byte)((qUint >> 8) & 0xFF);
            data[1] = (byte)(qUint & 0xFF);
            data[2] = (byte)(dqUint >> 4);
            data[3] = (byte)(((dqUint & 0xF) << 4) | ((kpUint >> 8) & 0xF));
            data[4] = (byte)(kpUint & 0xFF);
            data[5] = (byte)(kdUint >> 4);
            data[6] = (byte)(((kdUint & 0xF) << 4) | ((tauUint >> 8) & 0xF));
            data[7] = (byte)(tauUint & 0xFF);

            _usbHw.FdcanFrameSend(data, canId);
        }

        public void ControlPosVel(Motor motor, float pos, float vel)
        {
            ushort id = motor.CanId;
            if (!_motors.ContainsKey(id))
            {
                Console.Error.WriteLine($"[Error] In control_pos_vel,no motor with id {motor.CanId} is registered.");
                Environment.Exit(-1);
            }

            ushort canId = (ushort)(id + (int)Control_Mode_Code.POS_VEL_MODE);
            byte[] posBytes = BitConverter.GetBytes(pos);
            byte[] velBytes = BitConverter.GetBytes(vel);

            byte[] data = new byte[8];
            Array.Copy(posBytes, 0, data, 0, 4);
            Array.Copy(velBytes, 0, data, 4, 4);

            _usbHw.FdcanFrameSend(data, canId);
        }

        public void ControlVel(Motor motor, float vel)
        {
            ushort id = motor.CanId;
            if (!_motors.ContainsKey(id))
            {
                Console.Error.WriteLine($"[Error] In control_vel,no motor with id {motor.CanId} is registered.");
                Environment.Exit(-1);
            }

            ushort canId = (ushort)(id + (int)Control_Mode_Code.VEL_MODE);
            byte[] velBytes = BitConverter.GetBytes(vel);

            byte[] data = new byte[8];
            Array.Copy(velBytes, 0, data, 0, 4);

            _usbHw.FdcanFrameSend(data, canId);
        }

        private void ReceiveParam(byte[] data)
        {
            ushort canId = (ushort)((data[1] << 8) | data[0]);
            byte rid = data[3];

            if (!_motors.ContainsKey(canId))
            {
                Console.Error.WriteLine($"[Error] In receive_param,no motor with id {canId} is registered.");
                return;
            }

            if (IsInRanges(rid))
            {
                uint dataUint32 = (uint)((data[7] << 24) | (data[6] << 16) | (data[5] << 8) | data[4]);
                _motors[canId].SetParam(rid, dataUint32);

                if (rid == 10)
                {
                    // 1. 确定要设置的模式
                    Control_Mode_Code targetMode;

                    switch (dataUint32)
                    {
                        case 1:
                            targetMode = Control_Mode_Code.MIT_MODE;
                            break;
                        case 2:
                            targetMode = Control_Mode_Code.POS_VEL_MODE;
                            break;
                        case 3:
                            targetMode = Control_Mode_Code.VEL_MODE;
                            break;
                        case 4:
                            targetMode = Control_Mode_Code.POS_FORCE_MODE;
                            break;
                        default:
                            // 对应原代码中的 _ => _motors[canId].GetMotorMode()
                            targetMode = _motors[canId].GetMotorMode();
                            break;
                    }

                    // 2. 调用 SetMode 方法
                    _motors[canId].SetMode(targetMode);
                }
            }
            else
            {
                float dataFloat = Uint8ToFloat(data, 4);
                _motors[canId].SetParam(rid, dataFloat);
            }
        }

        /// <summary>
        /// 切换电机控制模式：control mode 控制模式 like:damiao::MIT_MODE, damiao::POS_VEL_MODE, damiao::VEL_MODE, damiao::POS_FORCE_MODE
        /// </summary>
        /// <param name="motor"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public bool SwitchControlMode(Motor motor, Control_Mode mode)
        {
            byte[] writeData = { (byte)mode, 0x00, 0x00, 0x00 };
            byte rid = 10;
            WriteMotorParam(motor, rid, writeData);

            if (!_motors.ContainsKey(motor.CanId))
            {
                Console.Error.WriteLine($"[Error] In switchControlMode,no motor with id {motor.CanId} is registered.");
                Environment.Exit(-1);
                return false;
            }

            return true;
        }

        public bool ChangeMotorParam(Motor motor, byte rid, float data)
        {
            byte[] dataBytes;
            if (IsInRanges(rid))
            {
                uint dataUint32 = FloatToUint32(data);
                dataBytes = BitConverter.GetBytes(dataUint32);
            }
            else
            {
                dataBytes = BitConverter.GetBytes(data);
            }

            WriteMotorParam(motor, rid, dataBytes);

            if (!_motors.ContainsKey(motor.CanId))
            {
                Console.Error.WriteLine($"[Error] In change_motor_param,no motor with id {motor.CanId} is registered.");
                Environment.Exit(-1);
                return false;
            }

            return true;
        }

        public void ChangeMotorLimit(Motor motor, float pMax, float qMax, float tMax)
        {
            motor.LimitParam = new Limit_param(pMax, qMax, tMax);
        }

        private void CanFrameCallback(CanValueType value)
        {
            uint canId = value.Id;

            if (_readWriteSave && _motors.ContainsKey((ushort)canId))
            {
                if (value.Data[2] == 0x33 || value.Data[2] == 0x55 || value.Data[2] == 0xAA)
                {
                    if (value.Data[2] == 0x33 || value.Data[2] == 0x55)
                    {
                        ReceiveParam(value.Data);
                        _readWriteSave = false;
                    }
                    _readWriteSave = false;
                }
            }
            else
            {
                ushort qUint = (ushort)((value.Data[1] << 8) | value.Data[2]);
                ushort dqUint = (ushort)((value.Data[3] << 4) | (value.Data[4] >> 4));
                ushort tauUint = (ushort)(((value.Data[4] & 0xF) << 8) | value.Data[5]);

                if (!_motors.ContainsKey((ushort)canId)) return;

                var motor = _motors[(ushort)canId];
                Limit_param limitParamReceive = motor.LimitParam;

                float receiveQ = UintToFloat(qUint, -limitParamReceive.Q_MAX, limitParamReceive.Q_MAX, 16);
                float receiveDq = UintToFloat(dqUint, -limitParamReceive.DQ_MAX, limitParamReceive.DQ_MAX, 12);
                float receiveTau = UintToFloat(tauUint, -limitParamReceive.TAU_MAX, limitParamReceive.TAU_MAX, 12);

                motor.ReceiveData(receiveQ, receiveDq, receiveTau);
                motor.UpdateTimeInterval();

                double interval = motor.GetTimeInterval();
                // Console.Error.WriteLine($"motor id is: {canId}: {interval}");
            }
        }

        // 辅助方法
        private ushort FloatToUint(float x, float xmin, float xmax, int bits)
        {
            float span = xmax - xmin;
            float dataNorm = (x - xmin) / span;
            ushort dataUint = (ushort)(dataNorm * ((1 << bits) - 1));
            return dataUint;
        }

        private float UintToFloat(ushort x, float xmin, float xmax, int bits)
        {
            float span = xmax - xmin;
            float dataNorm = (float)x / ((1 << bits) - 1);
            float data = dataNorm * span + xmin;
            return data;
        }

        private float Uint8ToFloat(byte[] data, int startIndex)
        {
            return BitConverter.ToSingle(data, startIndex);
        }

        private uint FloatToUint32(float data)
        {
            return BitConverter.ToUInt32(BitConverter.GetBytes(data), 0);
        }

        private bool IsInRanges(byte rid)
        {
            // 这里需要根据实际协议确定哪些RID是整型范围
            // 暂时返回false，需要根据实际文档实现
            return false;
        }

        #region InitMode

        public void InitModel()
        {

            basePath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName + "\\3D_Models\\";
            modelsNames = new List<string>();
            modelsNames.Add(MODEL_PATH1);
            modelsNames.Add(MODEL_PATH2);
            modelsNames.Add(MODEL_PATH3);
            modelsNames.Add(MODEL_PATH4);
            modelsNames.Add(MODEL_PATH5);
            modelsNames.Add(MODEL_PATH6);
            modelsNames.Add(MODEL_PATH7);
            modelsNames.Add(MODEL_PATH8);
            modelsNames.Add(MODEL_PATH9);
            modelsNames.Add(MODEL_PATH10);
            modelsNames.Add(MODEL_PATH11);//Until here for the 4600
#if IRB6700
            modelsNames.Add(MODEL_PATH12);
            modelsNames.Add(MODEL_PATH13);
            modelsNames.Add(MODEL_PATH14);
            modelsNames.Add(MODEL_PATH15);
            modelsNames.Add(MODEL_PATH16);
            modelsNames.Add(MODEL_PATH17);
            modelsNames.Add(MODEL_PATH18);
            modelsNames.Add(MODEL_PATH19);
            modelsNames.Add(MODEL_PATH20);
#endif


        }
        /// <summary>
        /// 读取并组合机械臂模型
        /// </summary>
        /// <param name="modelsNames"></param>
        /// <returns></returns>
        public Model3DGroup Initialize_Environment(List<string> modelsNames)
        {
            try
            {
                //Helix Toolkit提供的类，用于加载各种格式的3D模型文件，这里用来加载.stl文件。
                ModelImporter import = new ModelImporter();
                DeviceOperation.Instance.joints = new List<Joint>();

                foreach (string modelName in modelsNames)
                {
                    var materialGroup = new MaterialGroup();
                    Color mainColor = Colors.White;
                    EmissiveMaterial emissMat = new EmissiveMaterial(new SolidColorBrush(mainColor));
                    DiffuseMaterial diffMat = new DiffuseMaterial(new SolidColorBrush(mainColor));
                    SpecularMaterial specMat = new SpecularMaterial(new SolidColorBrush(mainColor), 200);
                    materialGroup.Children.Add(emissMat);
                    materialGroup.Children.Add(diffMat);
                    materialGroup.Children.Add(specMat);

                    var link = import.Load(basePath + modelName);
                    GeometryModel3D model = link.Children[0] as GeometryModel3D;
                    model.Material = materialGroup;
                    model.BackMaterial = materialGroup;
                    //这是一个自定义的辅助类，用于封装每个关节（即机械臂的每个可动部件）。它不仅包含Model3D（3D模型），还存储了该关节的旋转角度、旋转轴、旋转中心点等重要信息
                    DeviceOperation.Instance.joints.Add(new Joint(link)
                    {

                        Type = 0,


                    });
                }

                RA.Children.Add(DeviceOperation.Instance.joints[0].model);
                RA.Children.Add(DeviceOperation.Instance.joints[1].model);
                RA.Children.Add(DeviceOperation.Instance.joints[2].model);
                RA.Children.Add(DeviceOperation.Instance.joints[3].model);
                RA.Children.Add(DeviceOperation.Instance.joints[4].model);
                RA.Children.Add(DeviceOperation.Instance.joints[5].model);
                //RA.Children.Add(DeviceOperation.Instance.joints[6].model);
                //RA.Children.Add(DeviceOperation.Instance.joints[7].model);
                //RA.Children.Add(DeviceOperation.Instance.joints[8].model);
                //RA.Children.Add(DeviceOperation.Instance.joints[9].model);
                //RA.Children.Add(DeviceOperation.Instance.joints[10].model);
#if IRB6700
                //RA.Children.Add(DeviceOperation.Instance.joints[11].model);
                //RA.Children.Add(DeviceOperation.Instance.joints[12].model);
                //RA.Children.Add(DeviceOperation.Instance.joints[13].model);
                RA.Children.Add(DeviceOperation.Instance.joints[14].model);
                //RA.Children.Add(DeviceOperation.Instance.joints[15].model);
                //RA.Children.Add(DeviceOperation.Instance.joints[16].model);
                //RA.Children.Add(DeviceOperation.Instance.joints[17].model);
                RA.Children.Add(DeviceOperation.Instance.joints[18].model);
                RA.Children.Add(DeviceOperation.Instance.joints[19].model);
#endif

#if IRB6700
                Color cableColor = Colors.DarkSlateGray;
                changeModelColor(DeviceOperation.Instance.joints[6], cableColor);
                changeModelColor(DeviceOperation.Instance.joints[7], cableColor);
                changeModelColor(DeviceOperation.Instance.joints[8], cableColor);
                changeModelColor(DeviceOperation.Instance.joints[9], cableColor);
                changeModelColor(DeviceOperation.Instance.joints[10], cableColor);
                changeModelColor(DeviceOperation.Instance.joints[11], cableColor);
                changeModelColor(DeviceOperation.Instance.joints[12], cableColor);
                changeModelColor(DeviceOperation.Instance.joints[13], cableColor);

                changeModelColor(DeviceOperation.Instance.joints[14], Colors.Gray);

                changeModelColor(DeviceOperation.Instance.joints[15], Colors.Red);
                changeModelColor(DeviceOperation.Instance.joints[16], Colors.Red);
                changeModelColor(DeviceOperation.Instance.joints[17], Colors.Red);

                changeModelColor(DeviceOperation.Instance.joints[18], Colors.Gray);
                changeModelColor(DeviceOperation.Instance.joints[19], Colors.Gray);
                //关节的运动范围
                DeviceOperation.Instance.joints[0].angleMin = -180;
                DeviceOperation.Instance.joints[0].angleMax = 180;
                //旋转轴的方向矢量。您这里是 $(1, 0, 1)$，即 $Z$ 轴。
                DeviceOperation.Instance.joints[0].rotAxisX = 0;
                DeviceOperation.Instance.joints[0].rotAxisY = 0;
                DeviceOperation.Instance.joints[0].rotAxisZ = 1;
                DeviceOperation.Instance.joints[0].rotPointX = 0;
                DeviceOperation.Instance.joints[0].rotPointY = 0;
                DeviceOperation.Instance.joints[0].rotPointZ = 0;
                //关节的运动范围
                DeviceOperation.Instance.joints[1].angleMin = -100;
                DeviceOperation.Instance.joints[1].angleMax = 60;
                //旋转轴的方向矢量。您这里是 $(0, 1, 0)$，即 $Y$ 轴。
                DeviceOperation.Instance.joints[1].rotAxisX = 0;
                DeviceOperation.Instance.joints[1].rotAxisY = 1;
                DeviceOperation.Instance.joints[1].rotAxisZ = 0;
                DeviceOperation.Instance.joints[1].rotPointX = 348;
                DeviceOperation.Instance.joints[1].rotPointY = -243;
                DeviceOperation.Instance.joints[1].rotPointZ = 775;
                //关节的运动范围
                DeviceOperation.Instance.joints[2].angleMin = -90;
                DeviceOperation.Instance.joints[2].angleMax = 90;
                //旋转轴的方向矢量。您这里是 $(0, 1, 0)$，即 $Y$ 轴。
                DeviceOperation.Instance.joints[2].rotAxisX = 0;
                DeviceOperation.Instance.joints[2].rotAxisY = 1;
                DeviceOperation.Instance.joints[2].rotAxisZ = 0;
                //旋转轴上的一点（定义旋转轴的位置）。
                DeviceOperation.Instance.joints[2].rotPointX = 347;
                DeviceOperation.Instance.joints[2].rotPointY = -376;
                DeviceOperation.Instance.joints[2].rotPointZ = 1923;

                //关节的运动范围
                DeviceOperation.Instance.joints[3].angleMin = -180;
                DeviceOperation.Instance.joints[3].angleMax = 180;
                //旋转轴的方向矢量。您这里是 $(1, 0, 0)$，即 $X$ 轴。
                DeviceOperation.Instance.joints[3].rotAxisX = 1;
                DeviceOperation.Instance.joints[3].rotAxisY = 0;
                DeviceOperation.Instance.joints[3].rotAxisZ = 0;
                //旋转轴上的一点（定义旋转轴的位置）。
                DeviceOperation.Instance.joints[3].rotPointX = 60;
                DeviceOperation.Instance.joints[3].rotPointY = 0;
                DeviceOperation.Instance.joints[3].rotPointZ = 2125;

                //关节的运动范围
                DeviceOperation.Instance.joints[4].angleMin = -115;
                DeviceOperation.Instance.joints[4].angleMax = 115;
                //旋转轴的方向矢量。您这里是 $(0, 1, 0)$，即 $Y$ 轴。
                DeviceOperation.Instance.joints[4].rotAxisX = 0;
                DeviceOperation.Instance.joints[4].rotAxisY = 1;
                DeviceOperation.Instance.joints[4].rotAxisZ = 0;
                //旋转轴上的一点（定义旋转轴的位置）。
                DeviceOperation.Instance.joints[4].rotPointX = 1815;
                DeviceOperation.Instance.joints[4].rotPointY = 0;
                DeviceOperation.Instance.joints[4].rotPointZ = 2125;

                //关节的运动范围
                DeviceOperation.Instance.joints[5].angleMin = -180;
                DeviceOperation.Instance.joints[5].angleMax = 180;
                //旋转轴的方向矢量。您这里是 $(1, 0, 0)$，即 $X$ 轴。
                DeviceOperation.Instance.joints[5].rotAxisX = 1;
                DeviceOperation.Instance.joints[5].rotAxisY = 0;
                DeviceOperation.Instance.joints[5].rotAxisZ = 0;
                //旋转轴上的一点（定义旋转轴的位置）。
                DeviceOperation.Instance.joints[5].rotPointX = 2008;
                DeviceOperation.Instance.joints[5].rotPointY = 0;
                DeviceOperation.Instance.joints[5].rotPointZ = 2125;

#else
                changeModelColor(DeviceOperation.Instance.joints[6], Colors.Red);
                changeModelColor(DeviceOperation.Instance.joints[7], Colors.Black);
                changeModelColor(DeviceOperation.Instance.joints[8], Colors.Black);
                changeModelColor(DeviceOperation.Instance.joints[9], Colors.Black);
                changeModelColor(DeviceOperation.Instance.joints[10], Colors.Gray);

                RA.Children.Add(DeviceOperation.Instance.joints[0].model);
                RA.Children.Add(DeviceOperation.Instance.joints[1].model);
                RA.Children.Add(DeviceOperation.Instance.joints[2].model);
                RA.Children.Add(DeviceOperation.Instance.joints[3].model);
                RA.Children.Add(DeviceOperation.Instance.joints[4].model);
                RA.Children.Add(DeviceOperation.Instance.joints[5].model);
                RA.Children.Add(DeviceOperation.Instance.joints[6].model);
                RA.Children.Add(DeviceOperation.Instance.joints[7].model);
                RA.Children.Add(DeviceOperation.Instance.joints[8].model);
                RA.Children.Add(DeviceOperation.Instance.joints[9].model);
                RA.Children.Add(DeviceOperation.Instance.joints[10].model);
                
                DeviceOperation.Instance.joints[0].angleMin = -180;
                DeviceOperation.Instance.joints[0].angleMax = 180;
                DeviceOperation.Instance.joints[0].rotAxisX = 0;
                DeviceOperation.Instance.joints[0].rotAxisY = 0;
                DeviceOperation.Instance.joints[0].rotAxisZ = 1;
                DeviceOperation.Instance.joints[0].rotPointX = 0;
                DeviceOperation.Instance.joints[0].rotPointY = 0;
                DeviceOperation.Instance.joints[0].rotPointZ = 0;

                DeviceOperation.Instance.joints[1].angleMin = -100;
                DeviceOperation.Instance.joints[1].angleMax = 60;
                DeviceOperation.Instance.joints[1].rotAxisX = 0;
                DeviceOperation.Instance.joints[1].rotAxisY = 1;
                DeviceOperation.Instance.joints[1].rotAxisZ = 0;
                DeviceOperation.Instance.joints[1].rotPointX = 175; 
                DeviceOperation.Instance.joints[1].rotPointY = -200;
                DeviceOperation.Instance.joints[1].rotPointZ = 500;

                DeviceOperation.Instance.joints[2].angleMin = -90;
                DeviceOperation.Instance.joints[2].angleMax = 90;
                DeviceOperation.Instance.joints[2].rotAxisX = 0;
                DeviceOperation.Instance.joints[2].rotAxisY = 1;
                DeviceOperation.Instance.joints[2].rotAxisZ = 0;
                DeviceOperation.Instance.joints[2].rotPointX = 190;
                DeviceOperation.Instance.joints[2].rotPointY = -700;
                DeviceOperation.Instance.joints[2].rotPointZ = 1595;

                DeviceOperation.Instance.joints[3].angleMin = -180;
                DeviceOperation.Instance.joints[3].angleMax = 180;
                DeviceOperation.Instance.joints[3].rotAxisX = 1;
                DeviceOperation.Instance.joints[3].rotAxisY = 0;
                DeviceOperation.Instance.joints[3].rotAxisZ = 0;
                DeviceOperation.Instance.joints[3].rotPointX = 400;
                DeviceOperation.Instance.joints[3].rotPointY = 0;
                DeviceOperation.Instance.joints[3].rotPointZ = 1765;

                DeviceOperation.Instance.joints[4].angleMin = -115;
                DeviceOperation.Instance.joints[4].angleMax = 115;
                DeviceOperation.Instance.joints[4].rotAxisX = 0;
                DeviceOperation.Instance.joints[4].rotAxisY = 1;
                DeviceOperation.Instance.joints[4].rotAxisZ = 0;
                DeviceOperation.Instance.joints[4].rotPointX = 1405;
                DeviceOperation.Instance.joints[4].rotPointY = 50;
                DeviceOperation.Instance.joints[4].rotPointZ = 1765;

                DeviceOperation.Instance.joints[5].angleMin = -180;
                DeviceOperation.Instance.joints[5].angleMax = 180;
                DeviceOperation.Instance.joints[5].rotAxisX = 1;
                DeviceOperation.Instance.joints[5].rotAxisY = 0;
                DeviceOperation.Instance.joints[5].rotAxisZ = 0;
                DeviceOperation.Instance.joints[5].rotPointX = 1405;
                DeviceOperation.Instance.joints[5].rotPointY = 0;
                DeviceOperation.Instance.joints[5].rotPointZ = 1765;
#endif

            }
            catch (Exception e)
            {
                Log.Error("Exception Error:" + e.StackTrace);
            }
            return RA;
        }

        public Color changeModelColor(Joint pJoint, Color newColor)
        {
            Model3DGroup models = ((Model3DGroup)pJoint.model);
            return changeModelColor(models.Children[0] as GeometryModel3D, newColor);
        }
        public Color changeModelColor(GeometryModel3D pModel, Color newColor)
        {
            if (pModel == null)
                return oldColor;

            Color previousColor = Colors.Black;

            MaterialGroup mg = (MaterialGroup)pModel.Material;
            if (mg.Children.Count > 0)
            {
                try
                {
                    previousColor = ((EmissiveMaterial)mg.Children[0]).Color;
                    ((EmissiveMaterial)mg.Children[0]).Color = newColor;
                    ((DiffuseMaterial)mg.Children[1]).Color = newColor;
                }
                catch (Exception exc)
                {
                    previousColor = oldColor;
                }
            }

            return previousColor;

        }
        #endregion


        #region  Help functions
        /// <summary>
        /// Convert a CAN DLC value into the actual data length of the CAN/CAN-FD frame.
        /// </summary>
        /// <param name="dlc">A value between 0 and 15 (CAN and FD DLC range)</param>
        /// <param name="isSTD">A value indicating if the msg is a standard CAN (FD Flag not checked)</param>
        /// <returns>The length represented by the DLC</returns>
        public static int GetLengthFromDLC(int dlc, bool isSTD)
        {
            if (dlc <= 8)
                return dlc;

            if (isSTD)
                return 8;

            switch (dlc)
            {
                case 9: return 12;
                case 10: return 16;
                case 11: return 20;
                case 12: return 24;
                case 13: return 32;
                case 14: return 48;
                case 15: return 64;
                default: return dlc;
            }
        }

        #endregion

        #region protocol
        private void InitializeCommands()
        {
            commandList = new List<CANCommand>();
            // 初始化常用命令
            commandList.Add(new CANCommand("使能", 0, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFC }));
            commandList.Add(new CANCommand("失能", 0, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFD }));
            commandList.Add(new CANCommand("保存零点", 0, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFE }));
            commandList.Add(new CANCommand("清除错误", 0, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFB }));
            // 参数写入
            commandList.Add(new CANCommand("MIT", 1, System.Convert.ToUInt32("7FF"), new byte[] { 0xFF, 0xFF, 0x55, (byte)Register.CTRL_MODE, 0x01, 0x00, 0x00, 0x00 }));
            commandList.Add(new CANCommand("位置速度", 1, System.Convert.ToUInt32("7FF"), new byte[] { 0xFF, 0xFF, 0x55, (byte)Register.CTRL_MODE, 0x02, 0x00, 0x00, 0x00 }));
            commandList.Add(new CANCommand("速度", 1, System.Convert.ToUInt32("7FF"), new byte[] { 0xFF, 0xFF, 0x55, (byte)Register.CTRL_MODE, 0x03, 0x00, 0x00, 0x00 }));
            commandList.Add(new CANCommand("力位混控", 1, System.Convert.ToUInt32("7FF"), new byte[] { 0xFF, 0xFF, 0x55, (byte)Register.CTRL_MODE, 0x04, 0x00, 0x00, 0x00 }));
        }

        public static string GetEnumDescription(Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = (DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
            return attribute?.Description ?? value.ToString();
        }
        /// <summary>
        /// 写入参数
        /// </summary>
        /// <param name="CANID_L"></param>
        /// <param name="CANID_H"></param>
        /// <param name="register"></param>
        /// <returns></returns>
        public CANCommand WriteParam(byte CANID_L, byte CANID_H, Register register)
        {

            return new CANCommand(register.GetDescription(), 1, System.Convert.ToUInt32("7FF"), new byte[] { CANID_L, CANID_H, 0x55, (byte)register, 0x02, 0x00, 0x00, 0x00 }
                  );

        }
        /// <summary> 
        /// 读取参数
        /// </summary>
        /// <param name="CANID_L"></param>
        /// <param name="CANID_H"></param>
        /// <param name="register"></param>
        /// <returns></returns>
        public CANCommand ReadParam(byte CANID_L, byte CANID_H, Register register)
        {

            return new CANCommand(register.GetDescription(), 1, System.Convert.ToUInt32("7FF"), new byte[] { CANID_L, CANID_H, 0x33, (byte)register, 0x02, 0x00, 0x00, 0x00 }
                  );

        }



        /// <summary>
        /// 将一个 16 位无符号整数 (ushort) 拆分成一个包含两个字节的数组，
        /// 并且按照 "低位在前" (Little-Endian) 的顺序排列。
        /// </summary>
        /// <param name="value">要拆分的 16 位数值 (例如 0x0001)。</param>
        /// <returns>包含两个字节的数组，顺序为 [低位, 高位]。</returns>
        public static byte[] SplitToLittleEndian(ushort value)
        {
            // 1. 使用 BitConverter 将 ushort 转换为字节数组
            //    注意：BitConverter.GetBytes() 默认会根据当前系统架构来决定字节序。
            byte[] bytes = BitConverter.GetBytes(value);

            // 2. 检查系统架构的字节序：
            //    如果当前系统是大端序 (Big-Endian)，则需要反转数组，确保低位在前。
            //    但在大多数现代 PC 系统（Intel/AMD）上，BitConverter.IsLittleEndian 为 true，
            //    因此 bytes 数组默认已经是 [低位, 高位] 的顺序。
            if (BitConverter.IsLittleEndian)
            {
                // 对于小端序系统 (Low Byte, High Byte)，这是我们想要的 [低位, 高位]。
                // 例如：0x0001 -> { 0x01, 0x00 }
                // 例如：0x1234 -> { 0x34, 0x12 }
                return bytes;
            }
            else
            {
                // 对于大端序系统 (High Byte, Low Byte)，我们需要反转以得到 [低位, 高位]。
                // 例如：0x0001 -> { 0x00, 0x01 } (系统默认) -> { 0x01, 0x00 } (反转后)
                Array.Reverse(bytes);
                return bytes;
            }
        }
        public void SendPositionAndSpeeed(ControlFrame controlFrame, UInt32 ID, float positionDes, float velocityDes,float currentLimit)
        {
            // 1. 将 float 类型的位置值转换为 4 字节数组
            byte[] pBytes = BitConverter.GetBytes(positionDes);

            // 2. 将 float 类型的速度值转换为 4 字节数组
            byte[] vBytes = BitConverter.GetBytes(velocityDes);

            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(pBytes);
                Array.Reverse(vBytes);
            }
            byte[] data = new byte[8];
           
            CANCommand cANCommand = new CANCommand(ControlFrame.PositionSpd.GetDescription(), 0, data, ControlFrame.PositionSpd.GetDescription(), controlFrame);

            //根据模式不同0-8位的不同
            switch (controlFrame)
            {
                case ControlFrame.PositionSpd:
                    //帧 ID 为设定的 CAN ID 值加上 0x100 的偏移 P_des：位置给定，浮点型，低位在前，高位在后 V_des：速度给定，浮点型，低位在前，高位在后
                    //此处发送命令的 CAN ID 是 0x100 + ID。速度给定是梯形加速度运行下最高速度的，即为匀速段的速度值。
                    cANCommand.ID = ID + 0x100;
                    Buffer.BlockCopy(pBytes, 0, data, 0, 4); // 复制 P_des 到 data[0]..data[3]
                    Buffer.BlockCopy(vBytes, 0, data, 4, 4); // 复制 V_des 到 data[4]..data[7]

                    cANCommand.Data = data;
                    break;
                case ControlFrame.Spd:
                    //帧 ID 为设定的 CAN ID 值加上 0x200 的偏移 V_des：速度给定，浮点型，低位在前，高位在后此处发送命令的 CAN ID 是 0x200 + ID。
                    cANCommand.ID = ID + 0x200;
                    
                    Buffer.BlockCopy(vBytes, 0, data, 0, 4); // 复制 P_des 到 data[0]..data[3]
                    cANCommand.Data = data;

                    break;
                case ControlFrame.ForcePositionMixed:
                    //P_des：位置给定，单位为 rad，浮点类型，低位在前，高位在后；
                    //V_des：限速值，单位 rad/ s，放大 100 倍，类型为无符号 16 位，低位在前，高位在后， 范围为 0 - 10000，超过 10000 会限制在 10000，故对应的实际速度限定幅值为 0~100rad / s；
                    //I_des：扭矩电流限定标幺值，放大 10000 倍，类型为无符号 16 位，，低位在前，高位在 后，范围为 0 - 10000，超过 10000 会限制在 10000，对应的实际电流限定标幺幅值为 0 - 1.0
                    //电流标幺值：实际电流值除以最大相电流值。
                    cANCommand.ID = ID + 0x300;
                    // 2. --- V_des (限速) 处理 ---
                    // a. 放大 100 倍并四舍五入
                    //    vValue 的范围是 0~10000
                    ushort vValue = (ushort)Math.Round(velocityDes * 100.0f);
                    // b. ushort -> 2 bytes (Little-Endian)
                    byte[] vBytesMixe = BitConverter.GetBytes(vValue);

                    // 3. --- I_des (限流) 处理 ---
                    // a. 放大 10000 倍并四舍五入
                    //    iValue 的范围是 0~10000
                    ushort iValue = (ushort)Math.Round(currentLimit * 10000.0f);
                    // b. ushort -> 2 bytes (Little-Endian)
                    byte[] iBytes = BitConverter.GetBytes(iValue);
                    // --- 4. 统一处理字节序 (确保全部是 Little-Endian) ---
                    // 如果系统是 Big-Endian，需要反转所有字节数组
                    if (!BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(pBytes);
                        Array.Reverse(vBytes);
                        Array.Reverse(iBytes);
                    }
                    // --- 5. 组合 8 字节报文 ---
                    byte[] dataForce = new byte[8];

                    // D[0] - D[3]: P_des (4 bytes)
                    Buffer.BlockCopy(pBytes, 0, data, 0, 4);

                    // D[4] - D[5]: V_des (2 bytes)
                    Buffer.BlockCopy(vBytes, 0, data, 4, 2);

                    // D[6] - D[7]: I_des (2 bytes)
                    Buffer.BlockCopy(iBytes, 0, data, 6, 2);

                    cANCommand.Data = dataForce;
                    break;
                case ControlFrame.Mit:
                    break;
                default:
                    break;
            }

        }

        #endregion

    }
    // 虚拟USB硬件实现（用于测试）
    public class DummyUsbHardware : IUsbHardware
    {
        private Action<CanValueType> _frameCallback;

        public void FdcanFrameSend(byte[] data, uint id)
        {
            Console.WriteLine($"CAN Send - ID: 0x{id:X}, Data: {BitConverter.ToString(data)}");
        }

        public void SetFrameCallback(Action<CanValueType> callback)
        {
            _frameCallback = callback;
        }

        public void Start()
        {
            // 模拟接收数据
            Task.Run(() =>
            {
                while (true)
                {
                    Thread.Sleep(100);
                    // 可以在这里模拟接收数据用于测试
                }
            });
        }

        public void Stop()
        {
        }

        public void Dispose()
        {
        }
    }
}
