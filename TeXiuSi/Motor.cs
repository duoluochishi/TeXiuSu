using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeXiuSi.Protocol;
using ValueType = TeXiuSi.Protocol.ValueType;

namespace TeXiuSi
{
    public class Motor
    {
        // 限制参数数组
        public static readonly Limit_param[] LimitParams = new Limit_param[(int)DM_Motor_Type.Num_Of_Motor]
        {
            new Limit_param(12.566f, 50f, 5f),     // DM3507
            new Limit_param(12.5f, 30f, 10f),      // DM4310
            new Limit_param(12.5f, 50f, 10f),      // DM4310_48V
            new Limit_param(12.5f, 10f, 28f),      // DM4340
            new Limit_param(12.5f, 20f, 28f),      // DM4340_48V
            new Limit_param(12.5f, 45f, 12f),      // DM6006
            new Limit_param(12.566f, 20f, 120f),   // DM6248
            new Limit_param(12.5f, 45f, 20f),      // DM8006
            new Limit_param(12.5f, 45f, 54f),      // DM8009
            new Limit_param(12.5f, 25f, 200f),     // DM10010L
            new Limit_param(12.5f, 20f, 200f),     // DM10010
            new Limit_param(12.5f, 280f, 1f),      // DMH3510
            new Limit_param(12.5f, 45f, 10f),      // DMH6215
            new Limit_param(12.5f, 2000f, 2f),     // DMS3519
            new Limit_param(12.5f, 45f, 10f)       // DMG6220
        };

        private readonly Stopwatch _stopwatch;
        private double _deltaTime;
        private long _lastTicks;
        private readonly Dictionary<int, ValueType> _paramMap;

        public DM_Motor_Type MotorType { get; }
        public Control_Mode Mode { get; private set; }
        public ushort MasterId { get; }
        public ushort CanId { get; }
        public Limit_param LimitParam { get; set; }

        // 状态变量
        public float StateQ { get; private set; }
        public float StateDq { get; private set; }
        public float StateTau { get; private set; }


        public Motor(DM_Motor_Type motorType, Control_Mode ctrlMode, ushort canId, ushort masterId)
        {
            MotorType = motorType;
            Mode = ctrlMode;
            MasterId = masterId;
            CanId = canId;
            LimitParam = LimitParams[(int)motorType];
            _paramMap = new Dictionary<int, ValueType>();

            _stopwatch = new Stopwatch();
            _stopwatch.Start();
            _lastTicks = _stopwatch.ElapsedTicks;
        }
        public void UpdateTimeInterval()
        {
            long currentTicks = _stopwatch.ElapsedTicks;
            _deltaTime = (double)(currentTicks - _lastTicks) / Stopwatch.Frequency;
            _lastTicks = currentTicks;
        }

        public double GetTimeInterval()
        {
            return _deltaTime;
        }

        public void ReceiveData(float q, float dq, float tau)
        {
            StateQ = q;
            StateDq = dq;
            StateTau = tau;
        }

        public void SetParam(int key, float value)
        {
            _paramMap[key] = new Protocol.ValueType { IsFloat = true, FloatValue = value };
        }

        public void SetParam(int key, uint value)
        {
            _paramMap[key] = new ValueType { IsFloat = false, Uint32Value = value };
        }

        public float GetParamAsFloat(int key)
        {
            if (_paramMap.TryGetValue(key, out ValueType value) && value.IsFloat)
            {
                return value.FloatValue;
            }
            return 0f;
        }

        public uint GetParamAsUint32(int key)
        {
            if (_paramMap.TryGetValue(key, out ValueType value) && !value.IsFloat)
            {
                return value.Uint32Value;
            }
            return 0;
        }

        public bool HasParam(int key)
        {
            return _paramMap.ContainsKey(key);
        }

        public void SetMode(Control_Mode_Code modeCode)
        {

            switch (modeCode)
            {
                case Control_Mode_Code.MIT_MODE:
                    Mode = Control_Mode.MIT;
                    break;
                case Control_Mode_Code.POS_VEL_MODE:
                    Mode = Control_Mode.POS_VEL;
                    break;
                case Control_Mode_Code.VEL_MODE:
                    Mode = Control_Mode.VEL;
                    break;
                case Control_Mode_Code.POS_FORCE_MODE:
                    Mode = Control_Mode.POS_FORCE;
                    break;
                default:
                    // 保持原来的Mode不变
                    break;
            }
        }

        public Control_Mode_Code GetMotorMode()
        {
            switch (Mode)
            {
                case Control_Mode.MIT:
                    return Control_Mode_Code.MIT_MODE;
                case Control_Mode.POS_VEL:
                    return Control_Mode_Code.POS_VEL_MODE;
                case Control_Mode.VEL:
                    return Control_Mode_Code.VEL_MODE;
                case Control_Mode.POS_FORCE:
                    return Control_Mode_Code.POS_FORCE_MODE;
                default:
                    return Control_Mode_Code.MIT_MODE;
            }
        }
    }
}
