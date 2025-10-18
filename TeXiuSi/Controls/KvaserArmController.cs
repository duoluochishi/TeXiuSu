using Kvaser.CanLib;
using System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeXiuSi.Controls
{
    public class KvaserArmController
    {
        Canlib.canStatus R;
        int V;

        public KvaserArmController() {

            Canlib.canInitializeLibrary();
        }


      

        //private Kvaser.CanLib.canHandle m_channelHandle;
        //private int m_channelNumber = 0;
        //private const int BITRATE = Canlib.canBITRATE_500K; // 注意：波特率常量也变了！

        //public bool OpenChannel()
        //{
        //    try
        //    {
               
        //        // 新的初始化方法
        //        CanLib.Initialize(); // 替换掉了 Canlib.canInitializeLibrary();

        //        // 打开通道（方法名和参数基本一致）
        //        m_channelHandle = CanLib.OpenChannel(m_channelNumber, CanLib.canOPEN_ACCEPT_VIRTUAL);

        //        if (m_channelHandle.IsInvalid) // 新的判断句柄是否有效的方式
        //        {
        //            Console.WriteLine("打开通道失败: 句柄无效");
        //            return false;
        //        }

        //        // 设置总线参数
        //        CanLib.canStatus status = CanLib.SetBusParams(m_channelHandle, BITRATE, 0, 0, 0, 0, 0);
        //        if (status != CanLib.canStatus.canOK)
        //        {
        //            Console.WriteLine($"设置总线参数失败: {status}");
        //            return false;
        //        }

        //        // 启动总线
        //        status = CanLib.BusOn(m_channelHandle);
        //        if (status != CanLib.canStatus.canOK)
        //        {
        //            Console.WriteLine($"总线启动失败: {status}");
        //            return false;
        //        }

        //        Console.WriteLine($"通道 {m_channelNumber} 已成功打开并启动。");
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"初始化异常: {ex.Message}");
        //        return false;
        //    }
        //}

        //public void SendCommand(byte[] data)
        //{
        //    int id = 0x201;
        //    // 发送报文
        //    Kvaser.CanLib.canStatus status = CanLib.Write(m_channelHandle, id, data, data.Length, CanLib.canMSG_STD);
        //    if (status != CanLib.canStatus.canOK)
        //    {
        //        Console.WriteLine($"发送失败: {status}");
        //    }
        //}

        //public void CloseChannel()
        //{
        //    if (!m_channelHandle.IsInvalid)
        //    {
        //        CanLib.BusOff(m_channelHandle);
        //        CanLib.Close(m_channelHandle);
        //        Console.WriteLine("通道已关闭。");
        //    }
        //}
    }
}
