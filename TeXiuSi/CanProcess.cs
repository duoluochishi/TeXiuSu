using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeXiuSi
{
    public class CanProcess
    {
        // 通用CAN数据发送
        public void SendToCanData(SerialPort com, byte[] newMsg, string frameId_t, ushort sendtime, uint sendInterval, bool mode, byte len)
        {
            if (com == null || !com.IsOpen) return;
            // 在实际实现中，您需要根据设备的协议将 frameId_t, newMsg 等参数
            // 格式化为最终的串口数据包。
            // 例如: byte[] packet = CreateSerialPacket(frameId_t, newMsg, len);
            // com.Write(packet, 0, packet.Length);
            Debug.WriteLine($"信息: 发送通用CAN数据到ID {frameId_t}。");
        }

        // 控制指令发送
        public void ControlSendtoCAN(SerialPort com, byte[] newMsg, string frameId_t, int ctrMode, ushort sendtime, uint sendInterval, bool mode)
        {
            if (com == null || !com.IsOpen) return;
            // 同上，格式化并发送数据包。
            Debug.WriteLine($"信息: 发送控制CAN数据到ID {frameId_t}，模式为 {ctrMode}。");
        }

        // 将浮点数按比例转换为指定位数的无符号整数
        public ushort float_to_uint(float value, float min, float max, int bits)
        {
            // 将输入值限制在[min, max]范围内
            value = Math.Max(min, Math.Min(max, value));
            double span = max - min;
            double scaled = (value - min) / span;
            // 计算转换后的整数值
            return (ushort)(scaled * ((1 << bits) - 1));
        }
    }
}
