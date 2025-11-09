using DM_USB2CAN;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TeXiuSi.Protocol;
using Xceed.Wpf.Toolkit;

namespace TeXiuSi.Helper
{

    /// <summary>
    /// 为接收到数据的事件定义委托。
    /// </summary>
    /// <param name="message">接收到的十六进制字符串数据。</param>
    public delegate void DataReceivedHandler(string message);
    public class CanBusHelper
    {
        private readonly Dictionary<ushort, Motor> _motors;
        private static CanBusHelper m_Instance = null;
        byte[] dataTemp = new byte[4 * 1024];//4KB
        private bool _readWriteSave;
        public static CanBusHelper Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = new CanBusHelper();
                }

                return m_Instance;
            }
        }

        #region 私有成员
        private readonly SerialPort _serialPort;
        private readonly CanProcess _canProcessor;
        private readonly byte[] _receiveBuffer = new byte[4096]; // 4KB 接收缓冲区
        #endregion

        #region 公共属性
        /// <summary>
        /// 获取一个值，该值指示串口是否已连接。
        /// </summary>
        public bool IsConnected => _serialPort.IsOpen;

        // 电机参数范围定义
        public float P_MIN { get; set; } = -12.5f;
        public float P_MAX { get; set; } = 12.5f;
        public float V_MIN { get; set; } = -45.0f;
        public float V_MAX { get; set; } = 45.0f;
        public float KP_MIN { get; set; } = 0.0f;
        public float KP_MAX { get; set; } = 500.0f;
        public float KD_MIN { get; set; } = 0.0f;
        public float KD_MAX { get; set; } = 5.0f;
        public float T_MIN { get; set; } = -18.0f;
        public float T_MAX { get; set; } = 18.0f;
        #endregion

        #region 事件
        /// <summary>
        /// 当从串口接收到新数据时触发此事件。
        /// </summary>
        public event DataReceivedHandler OnDataReceived;
        #endregion

        public CanBusHelper()
        {
            _serialPort = new SerialPort();
            _canProcessor = new CanProcess();
        }

        #region 连接与数据处理
        /// <summary>
        /// 获取系统上所有可用的COM端口列表，并按数字大小排序。
        /// </summary>
        public static List<string> GetAvailablePorts()
        {
            return SerialPort.GetPortNames().Distinct().ToArray()
                .OrderBy(p => int.Parse(Regex.Match(p, @"\d+").Value))
                .ToList();
        }
        public void AddMotor(Motor motor)
        {
            _motors[motor.CanId] = motor;
            _motors[motor.MasterId] = motor;
        }
        /// <summary>
        /// 连接到指定的串口。
        /// </summary>
        /// <param name="portName">端口名称 (例如 "COM3")。</param>
        /// <returns>成功返回 true，失败返回 false。</returns>
        public bool Connect(string portName)
        {
            if (IsConnected) Disconnect();

            try
            {
                _serialPort.PortName = portName;
                _serialPort.BaudRate = 921600;
                _serialPort.DataBits = 8;
                _serialPort.Parity = Parity.None;
                _serialPort.StopBits = StopBits.One;
                _serialPort.ReadTimeout = 500;
                _serialPort.WriteTimeout = 500;

                _serialPort.DataReceived += SerialPort_DataReceived;
                _serialPort.Open();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"连接失败: {ex.Message}");
                return false;
            }
            return true;
        }

        /// <summary>
        /// 断开当前活动的串口连接。
        /// </summary>
        public void Disconnect()
        {
            if (IsConnected)
            {
                try
                {
                    _serialPort.DataReceived -= SerialPort_DataReceived;
                    _serialPort.Close();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"断开连接时出错: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 内部方法，用于处理 SerialPort 的 DataReceived 事件。
        /// </summary>
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (!_serialPort.IsOpen)
            {
                Log.Error("串口断开！");
                return;

            }
            int bytesToRead = _serialPort.BytesToRead;
            if (bytesToRead <= 0) return;

            //// 确保读取的字节数不超过缓冲区大小
            //int bytesToProcess = Math.Min(bytesToRead, _receiveBuffer.Length);
            //_serialPort.Read(_receiveBuffer, 0, bytesToProcess);

            //// 使用一个临时数组，只转换实际接收到的字节
            //byte[] receivedBytes = new byte[bytesToProcess];
            //Array.Copy(_receiveBuffer, receivedBytes, bytesToProcess);

            //// 转换为十六进制字符串，用空格分隔
            //string hexString = BitConverter.ToString(receivedBytes).Replace("-", " ");

            //// 触发事件，通知订阅者有新数据到达


            int Length = _serialPort.BytesToRead * 2;//处理为HEX时每Byte被分为了两位char
            if (bytesToRead <= 0)
            {
                Log.Warning("获取数据长度为0");
                return;
            }

            _serialPort.Read(dataTemp, 0, _serialPort.BytesToRead);//dataTemp必须非局部变量（多线程访问）

            string hexString = BitConverter.ToString(dataTemp).Replace("-", "");

            // 限制长度
            if (hexString.Length > Length)
            {
                hexString = hexString.Substring(0, Length);
            }
            Log.Information($"获取数据为Hex{hexString}");
            //触发事件，通知订阅者有新数据到达
            OnDataReceived?.Invoke(hexString);
        }
        #endregion

        #region 发送方法

        /// <summary>
        /// 使能指定CAN ID的电机。
        /// </summary>
        /// <param name="canId">电机的CAN ID。</param>
        public void EnableMotor(Motor motor)
        {
            var uintInfo = (ushort)(motor.CanId + (int)motor.GetMotorMode());
            ControlCmd(uintInfo, 0xFC);
        }

        /// <summary>
        /// 失能指定CAN ID的电机。
        /// </summary>
        /// <param name="canId">电机的CAN ID。</param>
        public void DisableMotor(Motor motor)
        {
            //byte[] motorDisableMsg = { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFD };
            //_canProcessor.SendToCanData(_serialPort, motorDisableMsg, canId.ToString("X"), 1, 1, true, 8);
            var uintInfo = (ushort)(motor.CanId + (int)motor.GetMotorMode());
            ControlCmd(uintInfo, 0xFD);
        }

        /// <summary>
        /// 清除错误
        /// </summary>
        /// <param name="motor"></param>
        public void ClearError(Motor motor)
        {
            //byte[] motorDisableMsg = { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFD };
            //_canProcessor.SendToCanData(_serialPort, motorDisableMsg, canId.ToString("X"), 1, 1, true, 8);
            var uintInfo = (ushort)(motor.CanId + (int)motor.GetMotorMode());
            ControlCmd(uintInfo, 0xFB);
        }

        /// <summary>
        /// 使用MIT控制模式向电机发送控制指令。
        /// </summary>
        /// <param name="canId">电机的CAN ID。</param>
        /// <param name="position">目标位置 (rad)。</param>
        /// <param name="velocity">目标速度 (rad/s)。</param>
        /// <param name="kp">位置环增益。</param>
        /// <param name="kd">速度环增益。</param>
        /// <param name="torque">前馈力矩 (Nm)。</param>
        public void SendMitCommand(int canId, float position, float velocity, float kp, float kd, float torque)
        {
            if (!IsConnected) return;

            // 将浮点数值转换为无符号整数
            ushort pos_tmp = _canProcessor.float_to_uint(position, P_MIN, P_MAX, 16);
            ushort vel_tmp = _canProcessor.float_to_uint(velocity, V_MIN, V_MAX, 12);
            ushort kp_tmp = _canProcessor.float_to_uint(kp, KP_MIN, KP_MAX, 12);
            ushort kd_tmp = _canProcessor.float_to_uint(kd, KD_MIN, KD_MAX, 12);
            ushort tor_tmp = _canProcessor.float_to_uint(torque, T_MIN, T_MAX, 12);

            // 通过位运算将整数打包到8字节的数组中
            byte[] msg = new byte[8];
            msg[0] = (byte)(pos_tmp >> 8);
            msg[1] = (byte)(pos_tmp & 0xFF);
            msg[2] = (byte)(vel_tmp >> 4);
            msg[3] = (byte)(((vel_tmp & 0x0F) << 4) | (kp_tmp >> 8));
            msg[4] = (byte)(kp_tmp & 0xFF);
            msg[5] = (byte)(kd_tmp >> 4);
            msg[6] = (byte)(((kd_tmp & 0x0F) << 4) | (tor_tmp >> 8));
            msg[7] = (byte)(tor_tmp & 0xFF);

            _canProcessor.ControlSendtoCAN(_serialPort, msg, canId.ToString("X"), 0, 1, 1, true);
        }



        /// <summary>
        /// RID寄存器读-控制模式
        /// </summary>
        /// <param name="canId">电机的CAN ID。</param>
        public void RegisterRead(int canid)
        {
            byte[] ReadRID_msg = { 0x00, 0x00, 0x33, 0x00 };
            ReadRID_msg[0] = (byte)(canid & 0xff);//low Byte
            ReadRID_msg[1] = (byte)(canid >> 8);
            //RID枚举中存储了前32个寄存器对应地址
            ReadRID_msg[3] = (byte)RID.cmode;//控制模式
            _canProcessor.SendToCanData(_serialPort, ReadRID_msg, "0x7ff", 1, 1, true, 8);//0x7ff标准帧

        }
        // 封装成一个函数：读取指定的 RID 寄存器
        public void ReadDamiaoRID(int canid, RID registerID)
        {
            // 消息结构：[ID_Low][ID_High][Command: 0x33][Register_Address]
            byte[] ReadRID_msg = { 0x00, 0x00, 0x33, 0x00 };

            // 填充目标电机 ID
            ReadRID_msg[0] = (byte)(canid & 0xff);
            ReadRID_msg[1] = (byte)(canid >> 8);

            // 填充寄存器地址
            ReadRID_msg[3] = (byte)registerID;

            // 发送
            _canProcessor.SendToCanData(_serialPort, ReadRID_msg, "0x7ff", 1, 1, true, 8);
        }
        /// <summary>
        /// MIT电机调试
        /// </summary>
        /// <param name="canid"></param>
        /// <param name="registerID"></param>
        /// <param name="serialPort"></param>
        public void WriteMotorProtocol(int canid)
        {
            //电机调试
            byte[] MotorMIT_msg = new byte[8];
            //根据协议填充发送数据
            //can 给定
            float POS = 10, VEL = 10, KP = 100, KD = 1, TOR = 1;
            UInt16 pos_tmp, vel_tmp, kp_tmp, kd_tmp, tor_tmp;
            pos_tmp = _canProcessor.float_to_uint(POS, P_MIN, P_MAX, 16);//根据范围线性转换为uint
            vel_tmp = _canProcessor.float_to_uint(VEL, V_MIN, V_MAX, 12);
            kp_tmp = _canProcessor.float_to_uint(KP, KP_MIN, KP_MAX, 12);
            kd_tmp = _canProcessor.float_to_uint(KD, KD_MIN, KD_MAX, 12);
            tor_tmp = _canProcessor.float_to_uint(TOR, T_MIN, T_MAX, 12);
            MotorMIT_msg[0] = (byte)(pos_tmp >> 8);
            MotorMIT_msg[1] = (byte)(pos_tmp & 0xFF);
            MotorMIT_msg[2] = (byte)((vel_tmp >> 4) & 0xFF);
            MotorMIT_msg[3] = (byte)((byte)(((vel_tmp & 0xF) << 4) & 0xFF) | (byte)((kp_tmp >> 8) & 0xFF));
            MotorMIT_msg[4] = (byte)(kp_tmp & 0xFF);
            MotorMIT_msg[5] = (byte)((kd_tmp >> 4) & 0xFF);
            MotorMIT_msg[6] = (byte)(((byte)((kd_tmp & 0xF) << 4) & 0xFF) | (byte)((tor_tmp >> 8) & 0xFF));
            MotorMIT_msg[7] = (byte)(tor_tmp & 0xFF);
            _canProcessor.ControlSendtoCAN(_serialPort, MotorMIT_msg, String.Format("{0:X4}", canid), 0, 1, 1, true);//发送电机控制id需要改为canid
        }

        /// <summary>
        /// MIT待定功能
        /// </summary>
        /// <param name="motor"></param>
        /// <param name="kp"></param>
        /// <param name="kd"></param>
        /// <param name="q"></param>
        /// <param name="dq"></param>
        /// <param name="tau"></param>
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


            _canProcessor.SendToCanData(_serialPort, data, canId.ToString("X"), 1, 1, true, 8);
        }
        // 辅助方法
        private ushort FloatToUint(float x, float xmin, float xmax, int bits)
        {
            float span = xmax - xmin;
            float dataNorm = (x - xmin) / span;
            ushort dataUint = (ushort)(dataNorm * ((1 << bits) - 1));
            return dataUint;
        }
        /// <summary>
        /// 写入参数模式
        /// </summary>
        /// <param name="motor"></param>
        /// <param name="rid"></param>
        /// <param name="data"></param>
        public void WriteMotorParam(Motor motor, byte rid = 10)
        {
            byte[] writeData = { (byte)motor.Mode, 0x00, 0x00, 0x00 };
            ushort id = motor.CanId;
            byte idLow = (byte)(id & 0xFF);
            byte idHigh = (byte)((id >> 8) & 0xFF);

            byte[] sendData = new byte[8];
            sendData[0] = idLow;
            sendData[1] = idHigh;
            sendData[2] = 0x55;
            sendData[3] = rid;
            Array.Copy(writeData, 0, sendData, 4, Math.Min(writeData.Length, 4));
            _canProcessor.SendToCanData(_serialPort, sendData, "0x7ff", 1, 1, true, 8);

        }

        /// <summary>
        /// 保存位置零点
        /// </summary>
        /// <param name="motor"></param>
        public void SetZeroPosition(Motor motor)
        {
            ControlCmd((ushort)(motor.CanId + (int)motor.GetMotorMode()), 0xFE);
        }
        #endregion
        /// <summary>
        /// 联合
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cmd"></param>
        public void ControlCmd(ushort id, byte cmd)
        {
            byte[] data = { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, cmd };
            _canProcessor.SendToCanData(_serialPort, data, id.ToString("X"), 1, 1, true, 8);
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
        /// <summary>
        /// 读取参数
        /// </summary>
        /// <param name="motor"></param>
        /// <param name="rid"></param>
        /// <returns></returns>
        public float ReadMotorParam(Motor motor, byte rid)
        {
            _readWriteSave = true;
            ushort id = motor.CanId;
            byte idLow = (byte)(id & 0xFF);
            byte idHigh = (byte)((id >> 8) & 0xFF);

            byte[] data = { idLow, idHigh, 0x33, rid, 0x00, 0x00, 0x00, 0x00 };
            _canProcessor.SendToCanData(_serialPort, data, "0x7ff", 1, 1, true, 8);
            Thread.Sleep(2);
            return 0f;
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

            //_usbHw.FdcanFrameSend(sendData, 0x7FF);
            _canProcessor.SendToCanData(_serialPort, sendData, "0x7FF", 1, 1, true, 8);
        }
        /// <summary>
        /// 保存参数
        /// </summary>
        /// <param name="motor"></param>
        public void SaveMotorParam(Motor motor)
        {
            ushort id = motor.CanId;
            Control_Mode_Code mode = motor.GetMotorMode();
            ControlCmd((ushort)(id + (int)mode), 0xFD);
            Thread.Sleep(10);
            _readWriteSave = true;
            byte idLow = (byte)(id & 0xFF);
            byte idHigh = (byte)((id >> 8) & 0xFF);

            byte[] data = { idLow, idHigh, 0x55, 0x01, 0x00, 0x00, 0x00, 0x00 };
            _canProcessor.SendToCanData(_serialPort, data, "0x7FF", 1, 1, true, 8);
            Thread.Sleep(100);
        }

        /// <summary>
        /// 刷新状态
        /// </summary>
        /// <param name="motor"></param>
        public void RefreshMotorStatus(Motor motor)
        {
            byte idLow = (byte)(motor.CanId & 0xFF);
            byte idHigh = (byte)((motor.CanId >> 8) & 0xFF);

            byte[] data = { idLow, idHigh, 0xCC, 0x00, 0x00, 0x00, 0x00, 0x00 };
            //_usbHw.FdcanFrameSend(data, 0x7FF);
            _canProcessor.SendToCanData(_serialPort, data, "0x7FF", 1, 1, true, 8);
        }
        #region 整体控制方法

        #endregion

        private float Uint8ToFloat(byte[] data, int startIndex)
        {
            return BitConverter.ToSingle(data, startIndex);
        }
        private uint FloatToUint32(float data)
        {
            return BitConverter.ToUInt32(BitConverter.GetBytes(data), 0);
        }
        private float UintToFloat(ushort x, float xmin, float xmax, int bits)
        {
            float span = xmax - xmin;
            float dataNorm = (float)x / ((1 << bits) - 1);
            float data = dataNorm * span + xmin;
            return data;
        }

        /// <summary>
        /// 收到数据处理
        /// </summary>
        /// <param name="value"></param>
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
        private bool IsInRanges(byte rid)
        {
            // 这里需要根据实际协议确定哪些RID是整型范围
            // 暂时返回false，需要根据实际文档实现
            return false;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)] //强制单字节对齐 否则 Marshal.SizeOf计算结构体长度 会按照四字节对齐
    struct CAN_Function                //can发送功能相关结构体  15bytes
    {
        //public Byte      sendFlag;       //发送标志位
        public UInt32 sendTimes;        //发送次数
        public UInt32 sendInterval;     //发送间隔 单位100us
        public Byte canIdType;        //can id    ID 类型
        public UInt32 CANID;            //can ID
        public Byte canFrameType;     //can frame 帧 类型  
        public Byte canDataLen;       //数据长度
        public Byte idAcc;            //ID累加操作标志位
        public Byte dataAcc;          //DATA累加操作标志位
    };

}
