
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeXiuSi.Protocol
{
    // 电机类型枚举
    public enum DM_Motor_Type
    {
        DM3507 = 0,
        DM4310,
        DM4310_48V,
        DM4340,
        DM4340_48V,
        DM6006,
        DM6248,
        DM8006,
        DM8009,
        DM10010L,
        DM10010,
        DMH3510,
        DMH6215,
        DMS3519,
        DMG6220,
        Num_Of_Motor
    }

    // 控制模式枚举
    public enum Control_Mode_Code
    {
        MIT_MODE = 0x100,
        POS_VEL_MODE = 0x200,
        VEL_MODE = 0x300,
        POS_FORCE_MODE = 0x400
    }

    // 控制模式
    public enum Control_Mode
    {
        /// <summary>
        /// MIT
        /// </summary>
        MIT = 0,
        /// <summary>
        /// 位置速度
        /// </summary>
        POS_VEL = 1,
        /// <summary>
        /// 速度
        /// </summary>
        VEL = 2,
        /// <summary>
        /// 力位混控
        /// </summary>
        POS_FORCE = 3
    }
    // 限制参数结构
    public struct Limit_param
    {
        public float Q_MAX;      // 位置最大值
        public float DQ_MAX;     // 速度最大值
        public float TAU_MAX;    // 扭矩最大值

        public Limit_param(float q_max, float dq_max, float tau_max)
        {
            Q_MAX = q_max;
            DQ_MAX = dq_max;
            TAU_MAX = tau_max;
        }
    }

    // 电机数据结构
    public struct DmActData
    {
        public DM_Motor_Type MotorType;
        public Control_Mode Mode;
        public ushort CanId;
        public ushort MstId;
    }

    // CAN值类型
    public struct CanValueType
    {
        public uint Id;
        public byte[] Data;
        public uint Timestamp;
    }

    // USB硬件接口（需要根据实际硬件实现）
    public interface IUsbHardware : IDisposable
    {
        void FdcanFrameSend(byte[] data, uint id);
        void SetFrameCallback(Action<CanValueType> callback);
        void Start();
        void Stop();
    }

    // 参数值类型
    public struct ValueType
    {
        public bool IsFloat;
        public float FloatValue;
        public uint Uint32Value;
    }
    public class JointProtocol
    {

    }
}
