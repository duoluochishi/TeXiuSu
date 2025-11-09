using System;
using System.Collections.Generic;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
