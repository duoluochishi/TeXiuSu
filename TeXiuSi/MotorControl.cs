
#define IRB67001
using HelixToolkit.Wpf;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Xml.Linq;
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
        string baseUrdfPath = "";
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

        //private const string MODEL_PATH1 = "Txs-base.stl";
        //private const string MODEL_PATH2 = "Txs-s1.stl";
        //private const string MODEL_PATH3 = "Txs-s2.stl";
        //private const string MODEL_PATH4 = "Txs-s3.stl";
        //private const string MODEL_PATH5 = "Txs-s4.stl";
        //private const string MODEL_PATH6 = "Txs-s5.stl";
        //private const string MODEL_PATH7 = "Txs-s6.stl";
        //private const string MODEL_PATH8 = "Txs-s7.stl";
        //private const string MODEL_PATH9 = "Txs-s8.stl";
        //private const string MODEL_PATH1 = "Txs-base.stl";
        private const string MODEL_PATH1 = "Txs-s1.stl";
        private const string MODEL_PATH2 = "Txs-s2.stl";
        private const string MODEL_PATH3 = "Txs-s3.stl";
        private const string MODEL_PATH4 = "Txs-s4.stl";
        private const string MODEL_PATH5 = "Txs-s5.stl";
        private const string MODEL_PATH6 = "Txs-s6.stl";
        private const string MODEL_PATH7 = "Txs-s7.stl";
        private const string MODEL_PATH8 = "Txs-s8.stl";
        //private const string MODEL_PATH9 = "Txs-s8.stl";

        //private const string MODEL_PATH10 = "HG13133-HTX1001.stl";


#endif
        #endregion


        #region URDF_Param
        // 存储关节名称和对应的旋转对象，用于后续 UI 绑定
        // Key: 关节名称 (如 "shoulder_pan_joint"), Value: 旋转轴对象
        private Dictionary<string, AxisAngleRotation3D> jointControls = new Dictionary<string, AxisAngleRotation3D>();
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

            //EnableAll();
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
                    ControlCmd(0xFC);
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
                    //ControlCmd((ushort)(motor.CanId + (int)motor.GetMotorMode()), 0xFD);
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

        //public void SaveMotorParam(Motor motor)
        //{
        //    ushort id = motor.CanId;
        //    Control_Mode_Code mode = motor.GetMotorMode();
        //    ControlCmd((ushort)(id + (int)mode), 0xFD);
        //    Thread.Sleep(10);

        //    _readWriteSave = true;
        //    byte idLow = (byte)(id & 0xFF);
        //    byte idHigh = (byte)((id >> 8) & 0xFF);

        //    byte[] data = { idLow, idHigh, 0xAA, 0x01, 0x00, 0x00, 0x00, 0x00 };
        //    _usbHw.FdcanFrameSend(data, 0x7FF);
        //    Thread.Sleep(100);
        //}

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
        public byte[] ControlCmd(byte cmd)
        {
            byte[] data = { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, cmd };

            return data;
            //_usbHw.FdcanFrameSend(data, id);
        }
        /// <summary>
        /// 刷新获取参数
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cmd"></param>
        public byte[] ControlRefreshCmd(byte Id, byte cmd, byte jicunInfo)
        {
            byte CanIDL = 0;
            byte CanIDH = 0;
            SplitCanIdForLittleEndian(Id, out CanIDL, out CanIDH);
            //int info = 0x7FF;
            byte[] data = { CanIDL, CanIDH, cmd, jicunInfo, 0x00, 0x00, 0x00, 0x00 };
            Log.Information($"Send Refresh Data {data}");
            return data;
            //_usbHw.FdcanFrameSend(data, id);
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

        //public void SetZeroPosition(Motor motor)
        //{
        //    ControlCmd((ushort)(motor.CanId + (int)motor.GetMotorMode()), 0xFE);
        //}

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
            //modelsNames.Add(MODEL_PATH9);
            //modelsNames.Add(MODEL_PATH10);


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


            #region UrdfLoad

            //baseUrdfPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName + "\\3D_Models_Urdf\\DM9_URDF.urdf";
            //var goupInfo = LoadUrdf(baseUrdfPath);

            //var ins = 00;

            #endregion

        }
        #region UrdfLoad

        public Model3DGroup LoadUrdf(string urdfPath)
        {
            XDocument doc = XDocument.Load(urdfPath);
            // 1. 找到 Base Link (通常是没有 Parent 的 Link)
            // ... (XML解析逻辑，找到根节点名)

            // 2. 开始递归构建
            return BuildLink("base_link", doc);
        }

        private Model3DGroup BuildLink(string linkName, XDocument doc)
        {
            Model3DGroup currentGroup = new Model3DGroup();

            // A. 加载当前 Link 的 STL 模型
            // 解析 XML 找到 <link name="linkName"> 下的 <visual> <mesh filename="...">
            string stlPath = GetMeshPathFromXml(doc, linkName);
            if (!string.IsNullOrEmpty(stlPath))
            {
                // 使用 HelixToolkit 的 ModelImporter 加载 STL
                var importer = new ModelImporter();
                var model = importer.Load(stlPath);
                currentGroup.Children.Add(model);
            }

            // B. 寻找连接到这个 Link 的所有子关节 (Joints)
            // 查找所有 <joint> 标签中 parent link 是当前 linkName 的
            var childJoints = GetChildJointsFromXml(doc, linkName);

            foreach (var jointXml in childJoints)
            {
                // --- 关键步骤：处理关节变换 ---

                // 1. 读取 URDF 中的 Origin (XYZ 和 RPY)
                // URDF 的 RPY 通常是欧拉角，需要转换成 WPF 的 Matrix3D 或 Transform3D
                var offsetTransform = ParseOriginToTransform(jointXml);

                // 2. 创建用于动态旋转的 Transform (这就是你要控制运动的地方)
                // 读取 <axis xyz="0 0 1"/> 确定绕哪个轴转
                var rotationAxis = ParseAxis(jointXml);
                var dynamicRotation = new AxisAngleRotation3D(rotationAxis, 0); // 初始0度
                var rotateTransform = new RotateTransform3D(dynamicRotation);

                // 保存引用，以便界面滑块可以控制它
                string jointName = jointXml.Attribute("name").Value;
                jointControls[jointName] = dynamicRotation;

                // 3. 递归构建子 Link
                string childLinkName = jointXml.Element("child").Attribute("link").Value;
                Model3DGroup childGroup = BuildLink(childLinkName, doc);

                // 4. 组装层级 (最重要的部分！)
                // 创建一个包装组，把 偏移(Origin) 和 旋转(Motion) 应用上去
                var jointWrapper = new Model3DGroup();
                System.Windows.Media.Media3D.Transform3DGroup transformGroup = new System.Windows.Media.Media3D.Transform3DGroup();
                transformGroup.Children.Add(rotateTransform); // 先应用动态旋转
                transformGroup.Children.Add(offsetTransform); // 再应用安装偏移

                jointWrapper.Transform = transformGroup;
                jointWrapper.Children.Add(childGroup); // 将子 Link 放入包装组

                // 将包装好的子结构加入当前结构
                currentGroup.Children.Add(jointWrapper);
            }

            return currentGroup;
        }

        // 设置你的模型存放的根目录 (Assets 文件夹的绝对路径)
        // 比如：AppDomain.CurrentDomain.BaseDirectory + "Assets\\"

        private string GetMeshPathFromXml(XDocument doc, string linkName)
        {
            baseUrdfPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName + "\\3D_Models_Urdf";

            // 1. 找到对应的 Link 节点
            var linkNode = doc.Descendants("link")
                              .FirstOrDefault(x => (string)x.Attribute("name") == linkName);

            if (linkNode == null) return null;

            // 2. 找到 visual -> geometry -> mesh 节点
            var meshNode = linkNode.Element("visual")?
                                   .Element("geometry")?
                                   .Element("mesh");

            if (meshNode == null) return null;

            //// 3. 获取 filename 属性
            //string rawPath = (string)meshNode.Attribute("filename");
            //if (string.IsNullOrEmpty(rawPath)) return null;

            //// 4. 路径处理：将 package:// 替换为本地路径
            //// 假设 rawPath 是 "package://ur_description/meshes/ur5/visual/base.stl"
            //if (rawPath.StartsWith("package://"))
            //{
            //    // 去掉前缀，拼接本地根目录
            //    // 这里只是个示例，具体截取逻辑看你的文件夹结构
            //    string relativePath = rawPath.Substring("package://".Length);
            //    return Path.Combine(_modelBaseDirectory, relativePath);
            //}

            //// 如果是相对路径或绝对路径直接返回
            //return rawPath;

            //// 获取 filename，例如 "package://DM9_URDF_2/meshes/base_link.STL"
            string rawPath = (string)meshNode.Attribute("filename");

            if (string.IsNullOrEmpty(rawPath)) return null;

            // 处理 package:// 路径
            if (rawPath.StartsWith("package://"))
            {
                // 1. 去掉 "package://" 前缀
                // 结果变成: "DM9_URDF_2/meshes/base_link.STL"
                string pathWithoutPrefix = rawPath.Substring("package://".Length);

                // 2. 将正斜杠 / 替换为 Windows 的反斜杠 \
                pathWithoutPrefix = pathWithoutPrefix.Replace('/', '\\');

                // 3. 拼接成本地绝对路径
                // 结果变成: "C:\MyRobotProject\Assets\DM9_URDF_2\meshes\base_link.STL"
                return Path.Combine(baseUrdfPath, pathWithoutPrefix);
            }

            return rawPath;
        }
        private IEnumerable<XElement> GetChildJointsFromXml(XDocument doc, string currentLinkName)
        {
            // 查找所有 <joint> 节点
            // 且该节点的 <parent link="..."> 属性等于 currentLinkName
            return doc.Descendants("joint")
                      .Where(j => (string)j.Element("parent")?.Attribute("link") == currentLinkName);
        }
        private Vector3D ParseAxis(XElement jointXml)
        {
            var axisElem = jointXml.Element("axis");
            if (axisElem == null) return new Vector3D(0, 0, 1); // 默认 Z 轴

            string xyz = (string)axisElem.Attribute("xyz");
            var parts = ParseStringArray(xyz);

            // 返回旋转轴向量
            return new Vector3D(parts[0], parts[1], parts[2]);
        }

        private Transform3D ParseOriginToTransform(XElement jointXml)
        {
            var originElem = jointXml.Element("origin");

            // 默认值
            double x = 0, y = 0, z = 0;
            double roll = 0, pitch = 0, yaw = 0; // rpy in radians

            if (originElem != null)
            {
                // 1. 解析 XYZ (位移)
                var xyzAttr = (string)originElem.Attribute("xyz");
                if (!string.IsNullOrEmpty(xyzAttr))
                {
                    var parts = ParseStringArray(xyzAttr);
                    x = parts[0];
                    y = parts[1];
                    z = parts[2];
                }

                // 2. 解析 RPY (旋转 - 弧度)
                var rpyAttr = (string)originElem.Attribute("rpy");
                if (!string.IsNullOrEmpty(rpyAttr))
                {
                    var parts = ParseStringArray(rpyAttr);
                    roll = parts[0];
                    pitch = parts[1];
                    yaw = parts[2];
                }
            }

            // --- 构建变换矩阵 ---
            var group = new System.Windows.Media.Media3D.Transform3DGroup();

            // 1. 处理旋转 (URDF RPY -> WPF RotateTransform)
            // 这里的顺序很重要。URDF 标准 RPY 通常对应：先绕X转，再绕Y转，再绕Z转 (Fixed Frame)
            // 在 WPF TransformGroup 中，顺序是相反的（因为是矩阵乘法），或者我们直接计算矩阵

            // 将弧度转换为角度
            double rDeg = RadToDeg(roll);
            double pDeg = RadToDeg(pitch);
            double yDeg = RadToDeg(yaw);

            // 注意：这里的旋转顺序可能需要根据具体的 URDF 文件微调
            // 标准顺序通常是 Z * Y * X (Intrinsic) 或 X * Y * Z (Extrinsic)
            // 最稳妥的方法是分别添加旋转：
            group.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), rDeg))); // Roll
            group.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), pDeg))); // Pitch
            group.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), yDeg))); // Yaw

            // 2. 处理位移
            group.Children.Add(new TranslateTransform3D(x, y, z));

            return group;
        }
        // 辅助：解析 "0.1 -0.5 3.14" 这样的字符串为 double[]
        private double[] ParseStringArray(string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return new double[] { 0, 0, 0 };

            // Split 按照空格分割，移除空项
            var parts = str.Split(new char[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            double[] result = new double[parts.Length];
            for (int i = 0; i < parts.Length; i++)
            {
                // 关键：使用 InvariantCulture 确保 "." 总是被识别为小数点
                double.TryParse(parts[i], NumberStyles.Any, CultureInfo.InvariantCulture, out result[i]);
            }
            return result;
        }

        // 辅助：弧度转角度
        private double RadToDeg(double radians)
        {
            return radians * (180.0 / Math.PI);
        }
        #endregion

        /// <summary>
        /// 读取并组合机械臂模型
        /// </summary>
        /// <param name="modelsNames"></param>
        /// <returns></returns>
        public Model3DGroup Initialize_Environment(List<string> modelsNames, out List<Joint> jointsInfo)
        {
            var joints = new List<Joint>();
            try
            {
                //Helix Toolkit提供的类，用于加载各种格式的3D模型文件，这里用来加载.stl文件。
                ModelImporter import = new ModelImporter();
                //DeviceOperation.Instance.joints = new List<Joint>();


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
                    joints.Add(new Joint(link)
                    {

                        Type = 0,


                    });
                }

                RA.Children.Add(joints[0].model);
                RA.Children.Add(joints[1].model);
                RA.Children.Add(joints[2].model);
                RA.Children.Add(joints[3].model);
                RA.Children.Add(joints[4].model);
                RA.Children.Add(joints[5].model);
                //RA.Children.Add(DeviceOperation.Instance.joints[6].model);
                //RA.Children.Add(DeviceOperation.Instance.joints[7].model);
                //RA.Children.Add(DeviceOperation.Instance.joints[8].model);
                //RA.Children.Add(DeviceOperation.Instance.joints[9].model);
                //RA.Children.Add(DeviceOperation.Instance.joints[10].model);
#if IRB6700
                //RA.Children.Add(DeviceOperation.Instance.joints[11].model);
                //RA.Children.Add(DeviceOperation.Instance.joints[12].model);
                //RA.Children.Add(DeviceOperation.Instance.joints[13].model);
                //RA.Children.Add(joints[14].model);
                //RA.Children.Add(DeviceOperation.Instance.joints[15].model);
                //RA.Children.Add(DeviceOperation.Instance.joints[16].model);
                //RA.Children.Add(DeviceOperation.Instance.joints[17].model);
                //RA.Children.Add(joints[18].model);
                //RA.Children.Add(joints[19].model);
#endif

#if IRB6700
                Color cableColor = Colors.DarkSlateGray;
                changeModelColor(joints[6], cableColor);
                changeModelColor(joints[7], cableColor);
                changeModelColor(joints[8], cableColor);
                //changeModelColor(joints[9], cableColor);
                //changeModelColor(joints[10], cableColor);
                //changeModelColor(joints[11], cableColor);
                //changeModelColor(joints[12], cableColor);
                //changeModelColor(joints[13], cableColor);

                //changeModelColor(joints[14], Colors.Gray);

                //changeModelColor(joints[15], Colors.Red);
                //changeModelColor(joints[16], Colors.Red);
                //changeModelColor(joints[17], Colors.Red);

                //changeModelColor(joints[18], Colors.Gray);
                //changeModelColor(joints[19], Colors.Gray);
                //关节的运动范围
                joints[0].angleMin = -180;
                joints[0].angleMax = 180;
                //旋转轴的方向矢量。您这里是 $(1, 0, 1)$，即 $Z$ 轴。
                joints[0].rotAxisX = 0;
                joints[0].rotAxisY = 0;
                joints[0].rotAxisZ = 1;
                joints[0].rotPointX = 0;
                joints[0].rotPointY = 0;
                joints[0].rotPointZ = 0;
                //关节的运动范围
                joints[1].angleMin = -100;
                joints[1].angleMax = 60;
                //旋转轴的方向矢量。您这里是 $(0, 1, 0)$，即 $Y$ 轴。
                joints[1].rotAxisX = 0;
                joints[1].rotAxisY = 1;
                joints[1].rotAxisZ = 0;
                joints[1].rotPointX = 348;
                joints[1].rotPointY = -243;
                joints[1].rotPointZ = 775;
                //关节的运动范围
                joints[2].angleMin = -90;
                joints[2].angleMax = 90;
                //旋转轴的方向矢量。您这里是 $(0, 1, 0)$，即 $Y$ 轴。
                joints[2].rotAxisX = 0;
                joints[2].rotAxisY = 1;
                joints[2].rotAxisZ = 0;
                //旋转轴上的一点（定义旋转轴的位置）。
                joints[2].rotPointX = 347;
                joints[2].rotPointY = -376;
                joints[2].rotPointZ = 1923;

                //关节的运动范围
                joints[3].angleMin = -180;
                joints[3].angleMax = 180;
                //旋转轴的方向矢量。您这里是 $(1, 0, 0)$，即 $X$ 轴。
                joints[3].rotAxisX = 1;
                joints[3].rotAxisY = 0;
                joints[3].rotAxisZ = 0;
                //旋转轴上的一点（定义旋转轴的位置）。
                joints[3].rotPointX = 60;
                joints[3].rotPointY = 0;
                joints[3].rotPointZ = 2125;

                //关节的运动范围
                joints[4].angleMin = -115;
                joints[4].angleMax = 115;
                //旋转轴的方向矢量。您这里是 $(0, 1, 0)$，即 $Y$ 轴。
                joints[4].rotAxisX = 0;
                joints[4].rotAxisY = 1;
                joints[4].rotAxisZ = 0;
                //旋转轴上的一点（定义旋转轴的位置）。
                joints[4].rotPointX = 1815;
                joints[4].rotPointY = 0;
                joints[4].rotPointZ = 2125;

                //关节的运动范围
                joints[5].angleMin = -180;
                joints[5].angleMax = 180;
                //旋转轴的方向矢量。您这里是 $(1, 0, 0)$，即 $X$ 轴。
                joints[5].rotAxisX = 1;
                joints[5].rotAxisY = 0;
                joints[5].rotAxisZ = 0;
                //旋转轴上的一点（定义旋转轴的位置）。
                joints[5].rotPointX = 2008;
                joints[5].rotPointY = 0;
                joints[5].rotPointZ = 2125;

#else
                changeModelColor(joints[0], Colors.Red);
                changeModelColor(joints[6], Colors.Black);
                changeModelColor(joints[7], Colors.Black);

                RA.Children.Add(joints[6].model);
                RA.Children.Add(joints[7].model);
                //RA.Children.Add(joints[8].model);

                #region
                // 确保在加载模型并添加到 joints[0] 之后执行此诊断代码
                //var baseModel = joints[1].model;
                //GeometryModel3D geometryModel = null;

                //// 尝试获取 GeometryModel3D
                //if (baseModel is GeometryModel3D gModel)
                //{
                //    geometryModel = gModel;
                //}
                //else if (baseModel is Model3DGroup group && group.Children.Count > 0 && group.Children[0] is GeometryModel3D firstChild)
                //{
                //    // 如果 joints[0].model 是 Model3DGroup，通常第一个子元素是实际的几何模型
                //    geometryModel = firstChild;
                //}

                //if (geometryModel != null)
                //{
                //    // 关键步骤：获取模型的边界框。边界框是相对于模型自身的局部坐标系而言的。
                //    var bounds = geometryModel.Bounds;

                //    // 输出边界框的最小值和最大值
                //    // 这代表了模型在各个轴上的范围 (X, Y, Z)
                //    // 假设您的Z轴是垂直向上的：

                //    double minZ = bounds.Z;
                //    double maxZ = bounds.Z + bounds.SizeZ;

                //    // 输出到调试或日志中（使用 C# 的 Console.WriteLine 或 Log.Debug）
                //    System.Console.WriteLine("--- J0 Model Local Bounds ---");
                //    System.Console.WriteLine($"X 范围: {bounds.X} 到 {bounds.X + bounds.SizeX}");
                //    System.Console.WriteLine($"Y 范围: {bounds.Y} 到 {bounds.Y + bounds.SizeY}");
                //    System.Console.WriteLine($"Z 范围: {minZ} 到 {maxZ}");
                //    System.Console.WriteLine("-----------------------------");

                //    // 假设您希望模型的最低点（基座底部）位于 Z=0
                //    // 如果 Z 轴垂直向上，模型在局部坐标系中的最低点就是 minZ。
                //    // 要将模型抬升到 Z=0，需要的额外抬升量就是 -minZ。

                //    // 示例：如果 minZ = -100，则需要抬升 100。
                //    // 示例：如果 minZ = 50，则说明模型基座已经位于 Z=50，需要下移 -50。
                //    double Z_Local_Offset_To_Zero = -minZ;

                //    System.Console.WriteLine($"建议的 STL 自身 Z 轴补偿量: {Z_Local_Offset_To_Zero}");
                //}
                //else
                //{
                //    System.Console.WriteLine("无法获取 GeometryModel3D 的边界信息。");
                //}

                //if (joints.Count > 0)
                //{
                //    // *** 尝试修正 Z 轴补偿方向 ***
                //    // X, Y 轴：抵消 XML 中 1_Link Body 的初始偏移
                //    double compensationX_mm = 63.55;
                //    double compensationY_mm = 0;
                //    // Z 轴：(抵消 XML 偏移 27.15) + (抬升 STL 底部 120) = 92.85
                //    double compensationZ_mm = 120;

                //    // ⭐ X, Y 补偿 XML 偏移，使 J1 关节中心回到 (0, 0, Z)
                //    //double compensationX_mm = 8.3638;
                //    //double compensationY_mm = 12.614;
                //    //// Z 补偿 XML 偏移，使 J1 关节中心回到 Z=0
                //    //double compensationZ_mm = -27.15;

                //    var compensationTransform = new TranslateTransform3D(
                //        compensationX_mm,
                //        compensationY_mm,
                //        compensationZ_mm
                //    );

                //    //baseModel = joints[0].model;

                //    // 应用变换逻辑不变 (Transform3DGroup 或直接赋值)
                //    if (baseModel.Transform != null)
                //    {
                //        var group = new Transform3DGroup();
                //        group.Children.Add(baseModel.Transform);
                //        group.Children.Add(compensationTransform);
                //        baseModel.Transform = group;
                //    }
                //    else
                //    {
                //        baseModel.Transform = compensationTransform;
                //    }

                //    // 保持旋转中心设置在 (0, 0, 0)，以确保关节在世界原点旋转
                //    joints[0].rotPointX = 0;
                //    joints[0].rotPointY = 0;
                //    joints[0].rotPointZ = 0;
                //}

                //foreach (string modelName in modelsNames)
                //{
                //    // ... (模型加载和 joints.Add(new Joint(link) { ... }) 代码保持不变) ...

                //    // 假设此时 joints 列表中已经包含了所有模型
                //}

                //... (RA.Children.Add(joints[i].model) 代码保持不变) ...

                // ----------------------------------------------------
                // ⭐ 步骤 1: 应用 joints[0] (基座)的指定平移
                // ----------------------------------------------------
                //if (joints.Count > 0)
                //{
                //    // 用户指定的 joints[0] 补偿值 (mm)
                //    double compensationX_mm_J0 = 63.55;
                //    double compensationY_mm_J0 = 0.0;
                //    double compensationZ_mm_J0 = 120.0;

                //    var compensationTransform_J0 = new TranslateTransform3D(
                //        compensationX_mm_J0,
                //        compensationY_mm_J0,
                //        compensationZ_mm_J0
                //    );

                //    var baseModel = joints[0].model;

                //    // 应用变换逻辑
                //    if (baseModel.Transform != null)
                //    {
                //        var group = new Transform3DGroup();
                //        group.Children.Add(baseModel.Transform);
                //        group.Children.Add(compensationTransform_J0);
                //        baseModel.Transform = group;
                //    }
                //    else
                //    {
                //        baseModel.Transform = compensationTransform_J0;
                //    }

                //    // 保持旋转中心设置在 (0, 0, 0)
                //    joints[0].rotPointX = 0;
                //    joints[0].rotPointY = 0;
                //    joints[0].rotPointZ = 0;
                //}

                //// ----------------------------------------------------
                //// ⭐ 步骤 2: 对 joints[1] 及所有后续关节应用 Y 轴 +100 mm 的额外偏移
                //// ----------------------------------------------------
                //if (joints.Count > 1)
                //{
                //    // Y 轴额外偏移量
                //    double extraY_offset = -500.0; // mm

                //    // 创建额外的 Y 轴平移变换
                //    var extraYTransform = new TranslateTransform3D(63.5, extraY_offset, 118);

                //    // 从索引 1 开始遍历所有后续关节
                //    for (int i = 1; i < joints.Count; i++)
                //    {
                //        var currentModel = joints[i].model;

                //        // 应用变换逻辑（组合变换）
                //        if (currentModel.Transform != null)
                //        {
                //            var group = new Transform3DGroup();
                //            group.Children.Add(currentModel.Transform);
                //            group.Children.Add(extraYTransform);
                //            currentModel.Transform = group;
                //        }
                //        else
                //        {
                //            currentModel.Transform = extraYTransform;
                //        }

                //        // 【注意】如果这些关节的 rotPoint 坐标是相对于世界坐标系的，
                //        // 那么它们的 rotPointY 也需要相应增加 100 mm。
                //        // 示例（仅作演示，实际值请根据您之前的计算来确定）：
                //        // joints[i].rotPointY += extraY_offset;
                //    }
                //}

                #endregion



                joints[0].angleMin = -180;
                joints[0].angleMax = 180;
                joints[0].rotAxisX = 0;
                joints[0].rotAxisY = 0;
                joints[0].rotAxisZ = 1;
                joints[0].rotPointX = 0;
                joints[0].rotPointY = 0;
                joints[0].rotPointZ = 0;


                joints[1].angleMin = -100;
                joints[1].angleMax = 60;
                joints[1].rotAxisX = 0;
                joints[1].rotAxisY = 1;
                joints[1].rotAxisZ = 0;
                joints[1].rotPointX = 175;
                joints[1].rotPointY = -200;
                joints[1].rotPointZ = 500;

                joints[2].angleMin = -90;
                joints[2].angleMax = 90;
                joints[2].rotAxisX = 0;
                joints[2].rotAxisY = 1;
                joints[2].rotAxisZ = 0;
                joints[2].rotPointX = 190;
                joints[2].rotPointY = -700;
                joints[2].rotPointZ = 1595;

                joints[3].angleMin = -180;
                joints[3].angleMax = 180;
                joints[3].rotAxisX = 1;
                joints[3].rotAxisY = 0;
                joints[3].rotAxisZ = 0;
                joints[3].rotPointX = 400;
                joints[3].rotPointY = 0;
                joints[3].rotPointZ = 1765;

                joints[4].angleMin = -115;
                joints[4].angleMax = 115;
                joints[4].rotAxisX = 0;
                joints[4].rotAxisY = 1;
                joints[4].rotAxisZ = 0;
                joints[4].rotPointX = 1405;
                joints[4].rotPointY = 50;
                joints[4].rotPointZ = 1765;

                joints[5].angleMin = -180;
                joints[5].angleMax = 180;
                joints[5].rotAxisX = 1;
                joints[5].rotAxisY = 0;
                joints[5].rotAxisZ = 0;
                joints[5].rotPointX = 1405;
                joints[5].rotPointY = 0;
                joints[5].rotPointZ = 1765;

                //joints[7].angleMin = -180;
                //joints[7].angleMax = 180;
                //joints[7].rotAxisX = 1;
                //joints[7].rotAxisY = 0;
                //joints[7].rotAxisZ = 0;
                //joints[7].rotPointX = 1405;
                //joints[7].rotPointY = 0;
                //joints[7].rotPointZ = 1765;
#endif


            }
            catch (Exception e)
            {
                Log.Error("Exception Error:" + e.StackTrace);
            }

            jointsInfo = joints;
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
        /// 将完整的 CAN ID 拆分为低字节 (L) 和高字节 (H)，并确保为小端序。
        /// </summary>
        /// <param name="canId">完整的 CAN ID (uint 类型)</param>
        /// <param name="canIdL">输出：CAN ID 的低字节 (D[0])</param>
        /// <param name="canIdH">输出：CAN ID 的高字节 (D[1])</param>
        public void SplitCanIdForLittleEndian(uint canId, out byte canIdL, out byte canIdH)
        {
            // 1. 获取 CAN ID 的低 8 位 (Low Byte)
            // 使用 & 0xFF 掩码取出最低 8 位。
            // 这就是 CANID_L (D[0])
            canIdL = (byte)(canId & 0xFF);

            // 2. 获取 CAN ID 的高位 (High Byte)
            // 将 CAN ID 右移 8 位，丢弃低 8 位，剩下的部分就是高位。
            // 这就是 CANID_H (D[1])
            canIdH = (byte)(canId >> 8);
        }
        /// <summary>
        /// 写入参数
        /// </summary>
        /// <param name="CANID_L">CAN ID 低字节 (D[0])</param>
        /// <param name="CANID_H">CAN ID 高字节 (D[1])</param>
        /// <param name="register">要写入的寄存器地址 (RID, D[3])</param>
        /// <param name="value">要写入的浮点数数据 (D[4]到D[7])</param>
        /// <returns></returns>
        public CANCommand WriteParam(byte CANID_L, byte CANID_H, Register register, float value)
        {
            // 1. 将 float (4 字节) 转换为 byte[]
            // BitConverter 默认与系统架构的字节序一致（通常是小端序，符合您的要求）
            byte[] floatBytes = BitConverter.GetBytes(value);

            // 2. 构造完整的 8 字节数据数组
            // 结构：CANID_L, CANID_H, 0x55, RID, 数据[4], 数据[5], 数据[6], 数据[7]
            byte[] dataField = new byte[8];

            dataField[0] = CANID_L;         // D[0]
            dataField[1] = CANID_H;         // D[1]
            dataField[2] = 0x55;            // D[2] (固定值 0x55)
            dataField[3] = (byte)register;  // D[3] (寄存器 ID)

            // 将 floatBytes (4 字节) 拷贝到 dataField 的后 4 个字节 (D[4] 到 D[7])
            // 由于是小端序，floatBytes[0] 是最低有效字节 (LSB)，但直接放入 D[4] 即可
            Array.Copy(floatBytes, 0, dataField, 4, 4);

            // 3. 返回 CANCommand
            // 注意：System.Convert.ToUInt32("7FF") 会将 "7FF" 视为十进制数 779。
            // 如果您想将其作为 16 进制解析，需要指定基数 16。
            // 但在 CAN ID 处，7FF 通常是 16 进制，所以直接写 0x7FF 更安全。
            uint canId = 0x7FF;

            return new CANCommand(
                register.GetDescription(),
                // 假设您的 CANCommand 构造函数第二个参数是数据长度 (8)
                (byte)dataField.Length,
                canId,
                dataField
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
        /// <summary>
        /// 控制关节发送位置速度信息，位置为弧度
        /// </summary>
        /// <param name="controlFrame"></param>
        /// <param name="ID"></param>
        /// <param name="positionDes"></param>
        /// <param name="velocityDes"></param>
        /// <param name="currentLimit"></param>
        public CANCommand SendPositionAndSpeeed(ControlFrame controlFrame, byte ID, float positionDes, float velocityDes, float currentLimit)
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
                    cANCommand.ID = (uint)(ID + 0x100);
                    Buffer.BlockCopy(pBytes, 0, data, 0, 4); // 复制 P_des 到 data[0]..data[3]
                    Buffer.BlockCopy(vBytes, 0, data, 4, 4); // 复制 V_des 到 data[4]..data[7]

                    cANCommand.Data = data;
                    break;
                case ControlFrame.Spd:
                    //帧 ID 为设定的 CAN ID 值加上 0x200 的偏移 V_des：速度给定，浮点型，低位在前，高位在后此处发送命令的 CAN ID 是 0x200 + ID。
                    cANCommand.ID = (uint)(ID + 0x200);

                    Buffer.BlockCopy(vBytes, 0, data, 0, 4); // 复制 P_des 到 data[0]..data[3]
                    cANCommand.Data = data;

                    break;
                case ControlFrame.ForcePositionMixed:
                    //P_des：位置给定，单位为 rad，浮点类型，低位在前，高位在后；
                    //V_des：限速值，单位 rad/ s，放大 100 倍，类型为无符号 16 位，低位在前，高位在后， 范围为 0 - 10000，超过 10000 会限制在 10000，故对应的实际速度限定幅值为 0~100rad / s；
                    //I_des：扭矩电流限定标幺值，放大 10000 倍，类型为无符号 16 位，，低位在前，高位在 后，范围为 0 - 10000，超过 10000 会限制在 10000，对应的实际电流限定标幺幅值为 0 - 1.0
                    //电流标幺值：实际电流值除以最大相电流值。
                    cANCommand.ID = (uint)(ID + 0x300);
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
            return cANCommand;
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
