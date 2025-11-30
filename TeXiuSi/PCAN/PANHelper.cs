using MathNet.Numerics.RootFinding;
using Peak.Can.Basic.BackwardCompatibility;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TeXiuSi.Helper;
using TeXiuSi.Model;


namespace TeXiuSi.PCAN
{
    using PCANBasic = Peak.Can.Basic.BackwardCompatibility.PCANBasic;
    using TPCANBitrateFD = System.String;
    using TPCANHandle = System.UInt16;
    using TPCANTimestampFD = System.UInt64;
    public class PANHelper
    {

        /// <summary>
        /// Handles of non plug and play PCAN-Hardware
        /// </summary>
        private TPCANHandle[] m_NonPnPHandles;

        public PANHelper()
        {

            // Creates an array with all possible non plug-and-play PCAN-Channels
            //
            m_NonPnPHandles = new TPCANHandle[]
            {
                PCANBasic.PCAN_ISABUS1,
                PCANBasic.PCAN_ISABUS2,
                PCANBasic.PCAN_ISABUS3,
                PCANBasic.PCAN_ISABUS4,
                PCANBasic.PCAN_ISABUS5,
                PCANBasic.PCAN_ISABUS6,
                PCANBasic.PCAN_ISABUS7,
                PCANBasic.PCAN_ISABUS8,
                PCANBasic.PCAN_DNGBUS1
            };


            _limit_MontorsMode = new Dictionary<JointNode, List<byte>>();

            foreach (JointNode opType in Enum.GetValues(typeof(JointNode)))
            {
                _limit_MontorsMode.Add(opType, new List<byte>());
            }
        }
        public List<ConnectNodeModel> SetCanList()
        {
            TPCANStatus stsResult;
            uint iChannelsCount;
            bool bIsFD;
            List<ConnectNodeModel> connectNodeModels = new List<ConnectNodeModel>();

            // Clears the Channel comboBox and fill it again with 
            // the PCAN-Basic handles for no-Plug&Play hardware and
            // the detected Plug&Play hardware
            //
            //cbbChannel.Items.Clear();
            try
            {
                // Includes all no-Plug&Play Handles
                for (int i = 0; i < m_NonPnPHandles.Length; i++)
                    connectNodeModels.Add(new ConnectNodeModel
                    {

                        Type = 0,
                        Name = FormatChannelName(m_NonPnPHandles[i])
                    });

                // Checks for available Plug&Play channels
                //
                stsResult = PCANBasic.GetValue(PCANBasic.PCAN_NONEBUS, TPCANParameter.PCAN_ATTACHED_CHANNELS_COUNT, out iChannelsCount, sizeof(uint));
                if (stsResult == TPCANStatus.PCAN_ERROR_OK)
                {
                    TPCANChannelInformation[] info = new TPCANChannelInformation[iChannelsCount];

                    stsResult = PCANBasic.GetValue(PCANBasic.PCAN_NONEBUS, TPCANParameter.PCAN_ATTACHED_CHANNELS, info);
                    if (stsResult == TPCANStatus.PCAN_ERROR_OK)
                        // Include only connectable channels
                        //
                        foreach (TPCANChannelInformation channel in info)
                            if ((channel.channel_condition & PCANBasic.PCAN_CHANNEL_AVAILABLE) == PCANBasic.PCAN_CHANNEL_AVAILABLE)
                            {
                                bIsFD = (channel.device_features & PCANBasic.FEATURE_FD_CAPABLE) == PCANBasic.FEATURE_FD_CAPABLE;
                                connectNodeModels.Add(new ConnectNodeModel
                                {

                                    Type = 0,
                                    Name = FormatChannelName(channel.channel_handle, bIsFD)

                                });
                            }
                }

                //cbbChannel.SelectedIndex = cbbChannel.Items.Count - 1;
                //btnInit.Enabled = cbbChannel.Items.Count > 0;

                if (stsResult != TPCANStatus.PCAN_ERROR_OK)
                    MessageBox.Show(GetFormatedError(stsResult));
                return connectNodeModels;
            }
            catch (DllNotFoundException)
            {
                MessageBox.Show("Unable to find the library: PCANBasic.dll !", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //Environment.Exit(-1);

                return connectNodeModels;
            }
        }
        /// <summary>
        /// Help Function used to get an error as text
        /// </summary>
        /// <param name="error">Error code to be translated</param>
        /// <returns>A text with the translated error</returns>
        public string GetFormatedError(TPCANStatus error)
        {
            StringBuilder strTemp;

            // Creates a buffer big enough for a error-text
            //
            strTemp = new StringBuilder(256);
            // Gets the text using the GetErrorText API function
            // If the function success, the translated error is returned. If it fails,
            // a text describing the current error is returned.
            //
            if (PCANBasic.GetErrorText(error, 0, strTemp) != TPCANStatus.PCAN_ERROR_OK)
                return string.Format("An error occurred. Error-code's text (0x{0:X}) couldn't be retrieved", error);
            else
                return strTemp.ToString();
        }
        /// <summary>
        /// Gets the formated text for a PCAN-Basic channel handle
        /// </summary>
        /// <param name="handle">PCAN-Basic Handle to format</param>
        /// <returns>The formatted text for a channel</returns>
        private string FormatChannelName(TPCANHandle handle)
        {
            return FormatChannelName(handle, false);
        }


        /// <summary>
        /// Gets the formated text for a PCAN-Basic channel handle
        /// </summary>
        /// <param name="handle">PCAN-Basic Handle to format</param>
        /// <param name="isFD">If the channel is FD capable</param>
        /// <returns>The formatted text for a channel</returns>
        private string FormatChannelName(TPCANHandle handle, bool isFD)
        {
            TPCANDevice devDevice;
            byte byChannel;

            // Gets the owner device and channel for a 
            // PCAN-Basic handle
            //
            if (handle < 0x100)
            {
                devDevice = (TPCANDevice)(handle >> 4);
                byChannel = (byte)(handle & 0xF);
            }
            else
            {
                devDevice = (TPCANDevice)(handle >> 8);
                byChannel = (byte)(handle & 0xFF);
            }

            // Constructs the PCAN-Basic Channel name and return it
            //
            if (isFD)
                return string.Format("{0}:FD {1} ({2:X2}h)", devDevice, byChannel, handle);
            else
                return string.Format("{0} {1} ({2:X2}h)", devDevice, byChannel, handle);
        }


        public UInt16 ConvertChage(string cbbChannel)
        {

            bool bNonPnP;
            string strTemp;

            // Get the handle fromt he text being shown
            //
            strTemp = cbbChannel;
            strTemp = strTemp.Substring(strTemp.IndexOf('(') + 1, 3);

            strTemp = strTemp.Replace('h', ' ').Trim(' ');

            // Determines if the handle belong to a No Plug&Play hardware 
            //
            var m_PcanHandle = System.Convert.ToUInt16(strTemp, 16);
            bNonPnP = m_PcanHandle <= PCANBasic.PCAN_DNGBUS1;
            Log.Information($"bNonPnp:{bNonPnP}");

            return m_PcanHandle;
        }

        #region 解析报文
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
        public static float UintToFloat1(uint x, float range, byte bits)
        {
            float span = range * 2.0f;
            uint maxVal = (1U << bits) - 1U;
            float dataNorm = (float)x / (float)maxVal;
            float result = dataNorm * span - range;
            return result;
        }

        private List<float> _limit_param_4310 = new List<float>() {

            (float)12.5,
            30,10
        };

        private List<float> _limit_param_4340 = new List<float>() {

            (float)12.5,
           8,28
        };

        private Dictionary<JointNode, List<byte>> _limit_MontorsMode;
        /// <summary>
        /// 处理接收到的字节数组 (rxbuf)，根据 ID 解析命令或电机状态。
        /// </summary>
        /// <param name="id">电机的 ID (1 到 8)。</param>
        /// <param name="rxbuf">接收到的字节数据 (uint8_t[] 对应 C# 中的 byte[])。</param>
        public void Process(JointNode id, byte[] rxbuf, ref MotorFeedbackFrame frame)
        {
            
            // 检查 rxbuf 长度是否足够，至少需要 6 个字节用于状态解析
            if (rxbuf == null || rxbuf.Length < 6)
            {
                frame = new MotorFeedbackFrame();
                // 实际应用中可能需要抛出异常或记录错误
                return;
            }

            // C# 中 byte 转换为 uint32_t/uint 的类型转换是隐式的或使用 (uint)
            // C# 中的数组索引与 C 相同 (从 0 开始)

            // ## 模式设置/命令分支 (if)
            if (rxbuf[0] == (byte)id && rxbuf[2] == 0x33 && rxbuf[3] == 0x0a)
            {
                // 匹配成功：这是设置电机模式的命令

                // C# 数组索引从 0 开始，所以 id-1
                //if (id > 0 && id <= motorsMode.Length)
                //{
                //    motorsMode[(int)id - 1] = rxbuf[4];
                
                //}
                _limit_MontorsMode[id].Add(rxbuf[4]);
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
                    uint posRaw = (uint)(rxbuf[1] << 8 | rxbuf[2]);
                    frame.Position = UintToFloat(posRaw, _limit_param_4340[0], 16);

                    // 2. 速度 (Velocity) - 12 位数据: rxbuf[3] (高 8 位) | (rxbuf[4] 的高 4 位)
                    uint velRaw = (uint)(rxbuf[3] << 4 | (rxbuf[4] >> 4));
                    frame.Velocity = UintToFloat(velRaw, _limit_param_4340[1], 12);

                    // 3. 扭矩 (Torque) - 12 位数据: (rxbuf[4] 的低 4 位) | rxbuf[5] (低 8 位)
                    uint torqueRaw = (uint)((rxbuf[4] & 0x0f) << 8 | rxbuf[5]);
                    frame.Torque = UintToFloat(torqueRaw, _limit_param_4340[2], 12);
                }
                else if ((byte)id > 3 && (byte)id < 8) // 电机 ID 4 到 7 (使用 limit_param_4310 参数)
                {
                    int index = (int)id - 1;

                    // 数据解析逻辑与上面相同，只是使用的极限参数不同
                    uint posRaw = (uint)(rxbuf[1] << 8 | rxbuf[2]);
                    frame.Position = UintToFloat(posRaw, _limit_param_4310[0], 16);

                    uint velRaw = (uint)(rxbuf[3] << 4 | (rxbuf[4] >> 4));
                    frame.Velocity = UintToFloat(velRaw, _limit_param_4310[1], 12);

                    uint torqueRaw = (uint)((rxbuf[4] & 0x0f) << 8 | rxbuf[5]);
                    frame.Torque = UintToFloat(torqueRaw, _limit_param_4310[2], 12);
                }
                else if ((byte)id == 8) // 电机 ID 8 (也使用 limit_param_4310 参数，但索引特殊)
                {
                    // 注意：原始 C 代码中，ID=8 时，索引是 id - 2 (即 6)
                    // Motor[id - 2] => Motor[6]
                    int index = (int)id - 2;

                    // 数据解析逻辑相同
                    uint posRaw = (uint)(rxbuf[1] << 8 | rxbuf[2]);
                    frame.Position = UintToFloat(posRaw, _limit_param_4310[0], 16);

                    uint velRaw = (uint)(rxbuf[3] << 4 | (rxbuf[4] >> 4));
                    frame.Velocity = UintToFloat(velRaw, _limit_param_4310[1], 12);

                    uint torqueRaw = (uint)((rxbuf[4] & 0x0f) << 8 | rxbuf[5]);
                    frame.Torque = UintToFloat(torqueRaw, _limit_param_4310[2], 12);
                }
            }
        }
        #endregion
    }
}
