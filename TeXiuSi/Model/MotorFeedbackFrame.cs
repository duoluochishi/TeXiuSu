using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public byte MstId { get;  set; }
        public byte Id { get;  set; }
        public string Name { get; set; }
        public ErrorType Err { get;  set; }

        public string strStatus
        { 
            get { 
            
                return Err.GetDescription();
            } 
        
        }

        // 定点数原始值 (Signed Fixed-point Raw Values)
        public short RawPosition { get;  set; }
        public short RawVelocity { get;  set; }
        public short RawTorque { get;  set; } // 仅使用了4位，如果实际是12位需要修改解析逻辑

        // 实际物理量 (Float Values)
        public float Position { get;  set; }
        public float Velocity { get;  set; }
        public float Torque { get;  set; }


        // 温度 (通常为带符号的8位整数 sbyte)
        public sbyte MosTemperature { get;  set; }
        public sbyte RotorTemperature { get;  set; }


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
            // 假设 D[0] 到 D[2] 是按表格字段分割的

            // D[0]: MST_ID[7:4], ID[3:0]
            frame.MstId = (byte)((data[0] >> 4) & 0x0F);

            // ID (8位): ID[3:0] (D[0]低4位) | ID[7:4] (D[1]高4位)
            // 注意：ID 字段为 8 位，其低 4 位在 D[0]，高 4 位在 D[1]。
            frame.Id = (byte)(((data[1] & 0xF0) >> 4) | (data[0] & 0x0F));

            // ERR (8位): ERR[3:0] (D[1]低4位) | ERR[7:4] (D[2]高4位)
            // 注意：ERR 字段为 8 位，其低 4 位在 D[1]，高 4 位在 D[2]。
            frame.Err = (ErrorType)(((data[2] & 0xF0) >> 4) | (data[1] & 0x0F));


            // --- 4.2. 解析 POS (16位定点数) ---
            // 表格显示 POS[15:8] 在 D[2]，POS[7:0] 在 D[3]。
            // 这是一个 Field-Specific Big-Endian (大端序) 结构。
            // D[2] (高位字节) 拼接 D[3] (低位字节)
            ushort posCombined = (ushort)((data[2] << 8) | data[3]);

            // 提取 16 位有符号数
            frame.RawPosition = (short)posCombined;
            frame.Position = frame.RawPosition * POSITION_SCALE_FACTOR;


            // --- 4.3. 解析 VEL (12位定点数) ---
            // VEL[11:4] 在 D[4]，VEL[3:0] 在 D[5] 的高 4 位 [7:4]。
            // 12位原始值 = D[4] (高8位) + D[5][7:4] (低4位)
            ushort velCombined = (ushort)((data[4] << 4) | ((data[5] & 0xF0) >> 4));

            // 12位有符号扩展到 16 位 (SXT)
            // 12位符号位在 0x0800
            short rawVel12 = (short)(velCombined & 0x0FFF);
            if ((rawVel12 & 0x0800) != 0)
            {
                // 如果符号位是 1，用 1 填充高 4 位
                rawVel12 |= 
                unchecked((short)(rawVel12 | 0xF000));
            }
            frame.RawVelocity = rawVel12;
            frame.Velocity = frame.RawVelocity * VELOCITY_SCALE_FACTOR;


            // --- 4.4. 解析 T (扭矩) (4位/12位) ---
            // 表格中 T[11:8] 在 D[5] 的低 4 位 [3:0]。
            // T[7:0] 的位置不明确（D[6]被 T_MOS 占用）。
            // 假设协议中仅使用 D[5] 的 4 位来表示扭矩：
            byte rawTorque4 = (byte)(data[5] & 0x0F);

            // 将 4 位有符号数扩展到 16 位
            // 4位符号位在 0x0008
            short rawTorque = (short)(rawTorque4 & 0x0F);
            if ((rawTorque & 0x08) != 0)
            {
                // 如果符号位是 1，用 1 填充高位
                rawTorque |= unchecked((short)(rawVel12 | 0xF000));
            }

            frame.RawTorque = rawTorque;
            frame.Torque = frame.RawTorque * TORQUE_SCALE_FACTOR;


            // --- 4.5. 解析温度 (8位整数) ---
            // D[6] = T_MOS (8位), D[7] = T_Rotor (8位)
            frame.MosTemperature = (sbyte)data[6];
            frame.RotorTemperature = (sbyte)data[7];

            return frame;
        }
    }
}
