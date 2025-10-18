using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TeXiuSi.Helper
{

    /// <summary>
    /// 为接收到数据的事件定义委托。
    /// </summary>
    /// <param name="message">接收到的十六进制字符串数据。</param>
    public delegate void DataReceivedHandler(string message);
    public class CanBusHelper
    {
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
            return SerialPort.GetPortNames()
                .OrderBy(p => int.Parse(Regex.Match(p, @"\d+").Value))
                .ToList();
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
            int bytesToRead = _serialPort.BytesToRead;
            if (bytesToRead <= 0) return;

            // 确保读取的字节数不超过缓冲区大小
            int bytesToProcess = Math.Min(bytesToRead, _receiveBuffer.Length);
            _serialPort.Read(_receiveBuffer, 0, bytesToProcess);

            // 使用一个临时数组，只转换实际接收到的字节
            byte[] receivedBytes = new byte[bytesToProcess];
            Array.Copy(_receiveBuffer, receivedBytes, bytesToProcess);

            // 转换为十六进制字符串，用空格分隔
            string hexString = BitConverter.ToString(receivedBytes).Replace("-", " ");

            // 触发事件，通知订阅者有新数据到达
            OnDataReceived?.Invoke(hexString);
        }
        #endregion

        #region 发送方法

        /// <summary>
        /// 使能指定CAN ID的电机。
        /// </summary>
        /// <param name="canId">电机的CAN ID。</param>
        public void EnableMotor(int canId)
        {
            byte[] motorEnableMsg = { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFC };
            _canProcessor.SendToCanData(_serialPort, motorEnableMsg, canId.ToString("X"), 1, 1, true, 8);
        }

        /// <summary>
        /// 失能指定CAN ID的电机。
        /// </summary>
        /// <param name="canId">电机的CAN ID。</param>
        public void DisableMotor(int canId)
        {
            byte[] motorDisableMsg = { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFD };
            _canProcessor.SendToCanData(_serialPort, motorDisableMsg, canId.ToString("X"), 1, 1, true, 8);
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
        #endregion
    }
}
