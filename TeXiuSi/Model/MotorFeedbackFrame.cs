using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeXiuSi.Helper;

namespace TeXiuSi.Model
{
    public class MotorFeedbackFrame
    {
        // ====================================================================
        // 1. 常量/比例因子 (请根据实际协议修改这些值)
        // ====================================================================

        /// <summary> 位置定点数转实际物理量的比例因子。请替换为您协议中的实际值。 </summary>
        private const float POSITION_SCALE_FACTOR = 0.001f;

        /// <summary> 速度定点数转实际物理量的比例因子。请替换为您协议中的实际值。 </summary>
        private const float VELOCITY_SCALE_FACTOR = 0.01f;

        /// <summary> 扭矩定点数转实际物理量的比例因子。请替换为您协议中的实际值。 </summary>
        private const float TORQUE_SCALE_FACTOR = 0.01f;

        // ====================================================================
        // 2. 存储字段
        // ====================================================================

        public byte MstId { get; set; }
        public byte Id { get; set; }
        public string Name { get; set; }
        public ErrorType Err { get; set; }

        public string strStatus
        {
            get
            {

                return Err.GetDescription();
            }

        }

        // 定点数原始值 (Signed Fixed-point Raw Values)
        //public short RawPosition { get;  set; }
        //public short RawVelocity { get;  set; }
        //public short RawTorque { get;  set; } // 仅使用了4位，如果实际是12位需要修改解析逻辑

        // 实际物理量 (Float Values)
        public float Position { get; set; }
        public float Velocity { get; set; }
        public float Torque { get; set; }


        // 温度 (通常为带符号的8位整数 sbyte)
        public sbyte MosTemperature { get; set; }
        public sbyte RotorTemperature { get; set; }

        //单个读取返回的
        public static byte byteStatueInfo { get; set; }


        private static List<float> _limit_param_4310 = new List<float>() {

            (float)12.5,
            30,10
        };

        private static List<float> _limit_param_4340 = new List<float>() {

            (float)12.5,
           8,28
        };
        // ====================================================================
        // 4. 解析方法
        // ====================================================================

        /// <summary>
        /// 解析反馈帧数据。
        /// 假设 data 数组是包含了 D[0] 到 D[7] 的 8 个字节（即 new byte[64] 中的前 8 个字节）。
        /// </summary>
        /// <param name="data">包含 D[0] 到 D[7] 的字节数组。</param>
        /// <returns>MotorFeedbackFrame 实例。</returns>
        public static MotorFeedbackFrame Parse(byte[] data)
        {
            if (data == null || data.Length < 8)
            {
                throw new ArgumentException("数据数组长度不足 8 字节 (D[0] 到 D[7])。");
            }

            var frame = new MotorFeedbackFrame();

            // --- 4.1. 解析 ID/ERR/MST_ID (位域解析) ---


            // D[0] 的低 4 位 [3:0] 作为 ID
            // 原始代码：frame.Id = (byte)(((data[1] & 0xF0) >> 4) | (data[0] & 0x0F));
            // 假设现在 ID 只有 4 位
            frame.Id = (byte)(data[0] & 0x0F);

            // D[0] 的高 4 位 [7:4] 作为 ERROR
            // 原始代码：frame.Err = (ErrorType)(((data[2] & 0xF0) >> 4) | (data[1] & 0x0F));
            // 假设现在 ERROR 只有 4 位
            frame.Err = (ErrorType)((data[0] >> 4) & 0x0F);


            JointNode id = (JointNode)frame.Id;

            #region 原始读取
            //// --- 4.2. 解析 POS (16位定点数) ---
            //// 表格显示 POS[15:8] 在 D[2]，POS[7:0] 在 D[3]。
            //// 这是一个 Field-Specific Big-Endian (大端序) 结构。
            //// D[2] (高位字节) 拼接 D[3] (低位字节)
            //ushort posCombined = (ushort)((data[2] << 8) | data[3]);

            //// 提取 16 位有符号数
            ////frame.RawPosition = (short)posCombined;
            ////frame.Position = frame.RawPosition * POSITION_SCALE_FACTOR;
            //frame.Position = (short)posCombined;

            //// --- 4.3. 解析 VEL (12位定点数) ---
            //// VEL[11:4] 在 D[4]，VEL[3:0] 在 D[5] 的高 4 位 [7:4]。
            //// 12位原始值 = D[4] (高8位) + D[5][7:4] (低4位)
            //ushort velCombined = (ushort)((data[4] << 4) | ((data[5] & 0xF0) >> 4));

            //// 12位有符号扩展到 16 位 (SXT)
            //// 12位符号位在 0x0800
            //short rawVel12 = (short)(velCombined & 0x0FFF);
            //if ((rawVel12 & 0x0800) != 0)
            //{
            //    // 如果符号位是 1，用 1 填充高 4 位
            //    rawVel12 |=
            //    unchecked((short)(rawVel12 | 0xF000));
            //}
            ////frame.RawVelocity = rawVel12;
            ////frame.Velocity = frame.RawVelocity * VELOCITY_SCALE_FACTOR;
            //frame.Velocity = rawVel12;

            //// --- 4.4. 解析 T (扭矩) (4位/12位) ---
            //// 表格中 T[11:8] 在 D[5] 的低 4 位 [3:0]。
            //// T[7:0] 的位置不明确（D[6]被 T_MOS 占用）。
            //// 假设协议中仅使用 D[5] 的 4 位来表示扭矩：
            //byte rawTorque4 = (byte)(data[5] & 0x0F);

            //// 将 4 位有符号数扩展到 16 位
            //// 4位符号位在 0x0008
            //short rawTorque = (short)(rawTorque4 & 0x0F);
            //if ((rawTorque & 0x08) != 0)
            //{
            //    // 如果符号位是 1，用 1 填充高位
            //    rawTorque |= unchecked((short)(rawVel12 | 0xF000));
            //}

            ////frame.RawTorque = rawTorque;
            ////frame.Torque = frame.RawTorque * TORQUE_SCALE_FACTOR;
            //frame.Torque = rawTorque;
            #endregion

            if (data[0] == (byte)id && data[2] == 0x33 && data[3] == 0x0a)
            {
                // 匹配成功：这是设置电机模式的命令

                // C# 数组索引从 0 开始，所以 id-1
                //if (id > 0 && id <= motorsMode.Length)
                //{
                //    motorsMode[(int)id - 1] = rxbuf[4];

                //}
                byteStatueInfo = data[4];
                //_limit_MontorsMode[id].Add(data[4]);
            }
            // ## 电机状态解析分支 (else)
            else
            {
                // 未匹配成功：这是解析电机状态数据的逻辑

                // C# 中的位操作 (`<<`, `|`, `&`) 与 C 语言中行为一致，
                // 但需要注意运算数的类型，这里都使用 uint 进行计算。

                if ((byte)id >= 1 && (byte)id <= 3) // 电机 ID 1 到 3 (使用 limit_param_4340 参数)
                {
                    // C 语言中 Motor[id - 1] 的索引对应 C# 数组的 (int)id - 1
                    int index = (int)id - 1;

                    // 1. 位置 (Position) - 16 位数据: rxbuf[1] (高 8 位) | rxbuf[2] (低 8 位)
                    uint posRaw = (uint)(data[1] << 8 | data[2]);
                    frame.Position = UintToFloat(posRaw, _limit_param_4340[0], 16);

                    // 2. 速度 (Velocity) - 12 位数据: rxbuf[3] (高 8 位) | (rxbuf[4] 的高 4 位)
                    uint velRaw = (uint)(data[3] << 4 | (data[4] >> 4));
                    frame.Velocity = UintToFloat(velRaw, _limit_param_4340[1], 12);

                    // 3. 扭矩 (Torque) - 12 位数据: (rxbuf[4] 的低 4 位) | rxbuf[5] (低 8 位)
                    uint torqueRaw = (uint)((data[4] & 0x0f) << 8 | data[5]);
                    frame.Torque = UintToFloat(torqueRaw, _limit_param_4340[2], 12);
                }
                else if ((byte)id > 3 && (byte)id < 8) // 电机 ID 4 到 7 (使用 limit_param_4310 参数)
                {
                    int index = (int)id - 1;

                    // 数据解析逻辑与上面相同，只是使用的极限参数不同
                    uint posRaw = (uint)(data[1] << 8 | data[2]);
                    frame.Position = UintToFloat(posRaw, _limit_param_4310[0], 16);

                    uint velRaw = (uint)(data[3] << 4 | (data[4] >> 4));
                    frame.Velocity = UintToFloat(velRaw, _limit_param_4310[1], 12);

                    uint torqueRaw = (uint)((data[4] & 0x0f) << 8 | data[5]);
                    frame.Torque = UintToFloat(torqueRaw, _limit_param_4310[2], 12);
                }
                else if ((byte)id == 8) // 电机 ID 8 (也使用 limit_param_4310 参数，但索引特殊)
                {
                    // 注意：原始 C 代码中，ID=8 时，索引是 id - 2 (即 6)
                    // Motor[id - 2] => Motor[6]
                    int index = (int)id - 2;

                    // 数据解析逻辑相同
                    uint posRaw = (uint)(data[1] << 8 | data[2]);
                    frame.Position = UintToFloat(posRaw, _limit_param_4310[0], 16);

                    uint velRaw = (uint)(data[3] << 4 | (data[4] >> 4));
                    frame.Velocity = UintToFloat(velRaw, _limit_param_4310[1], 12);

                    uint torqueRaw = (uint)((data[4] & 0x0f) << 8 | data[5]);
                    frame.Torque = UintToFloat(torqueRaw, _limit_param_4310[2], 12);
                }
            }



            // --- 4.5. 解析温度 (8位整数) ---
            // D[6] = T_MOS (8位), D[7] = T_Rotor (8位)
            frame.MosTemperature = (sbyte)data[6];
            frame.RotorTemperature = (sbyte)data[7];

            return frame;
        }
        /// <summary>
        /// 将一个无符号整数（uint32_t）转换为一个浮点数（float），
        /// 并将其映射到 [-range, range] 的范围内。
        /// </summary>
        /// <param name="x">要转换的无符号整数值。</param>
        /// <param name="range">目标范围的绝对值（例如，如果范围是 [-5.0, 5.0]，则传入 5.0）。</param>
        /// <param name="bits">用于表示整数 x 的位数（例如，8、10 或 12）。</param>
        /// <returns>映射到 [-range, range] 范围内的浮点数。</returns>
        public static float UintToFloat(UInt32 x, float range, byte bits)
        {
            // C 函数中的逻辑：
            // float span = range * 2;
            // float data_norm = (float)x / ((1U << bits) - 1U);
            // float result = data_norm * span - range;

            // 1. 计算范围的跨度 (span = 2 * range)
            float span = range * 2.0f;

            // 2. 计算最大值 (max_val = (1U << bits) - 1U)
            // 1U << bits 在 C# 中对应 1U << bits，结果是 uint 类型。
            // 我们必须确保 bits 不超过 32。
            if (bits >= 32)
            {
                // 针对 bits=32 的特殊情况，最大值是 2^32 - 1，即 uint.MaxValue
                // 在 C# 中，(1U << 32) 会导致溢出或得到 1U << 0 = 1U，所以需要特殊处理。
                // 但在正常的量化/归一化场景中，bits 通常 < 32。
                // 我们假设 bits <= 31 以遵循 (1U << bits) - 1U 的计算逻辑。
                // 如果 bits=32， max_val = uint.MaxValue
                // 但遵循原始 C 逻辑，我们计算 max_val。
            }

            // 最大的量化值。这代表了 range。
            // 如果 bits=10，则 max_val = 1023。
            uint maxVal = (1U << bits) - 1U;

            // 3. 计算归一化值 (data_norm = x / max_val)
            // 必须进行浮点数除法。
            float dataNorm = (float)x / (float)maxVal;

            // 4. 计算结果
            // 结果 = 归一化值 * 跨度 - range
            float result = dataNorm * span - range;

            return result;
        }
    }
}
