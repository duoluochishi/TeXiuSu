using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Xaml.Behaviors.Layout;
using Serilog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeXiuSi.Helper;

namespace TeXiuSi
{
    public class SerialCommunication : ViewModelBase
    {
        private static SerialCommunication m_Instance = null;
        private string m_PortName = "COM1";//端口名
        private int m_BautRate = 115200;//波特率
        private int m_DataLength = 8;//数据位
        private StopBits m_StopBit = StopBits.One;//停止位
        private Parity m_Parity = Parity.None;//奇偶校验位
        private SerialPort m_SerialPort = null;

        private CanBusHelper canBusHelper;
        /// <summary>
        /// 是否初始化
        /// </summary>
        private bool isInitialized = false;

        public static SerialCommunication Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = new SerialCommunication();
                }

                return m_Instance;
            }
        }
        public SerialCommunication()
        {
            canBusHelper = new CanBusHelper();

            InitSerialPort();


        }
        /// <summary>
        /// 初始化端口部分
        /// </summary>
        /// <returns></returns>
        private void InitSerialPort()
        {
            if (isInitialized)
            {
                return;
            }

            OpenPort();
            isInitialized = InitDevice();
            if (!isInitialized)
            {
                Log.Error(nameof(SerialCommunication), "MsgType.Error", new Exception("Serial port init fail."));
            }
        }

        private void OpenPort()
        {
            if (m_SerialPort != null && m_SerialPort.IsOpen == true)
            {
                return;
            }

            //Configuration config = ConfigHelper.LoadConfiguration(System.IO.Path.Combine(AppContext.BaseDirectory, "Config", "serial.config"));
            //if (config != null)
            //{
            //    string portName = config.AppSettings.Settings["PortName"].Value;
            //    string bautRate = config.AppSettings.Settings["BautRate"].Value;
            //    if (!string.IsNullOrEmpty(portName))
            //    {
            //        m_PortName = portName;
            //    }

            //    if (!int.TryParse(bautRate, out m_BautRate))
            //    {
            //        Log.Error(this.GetType().ToString(), "MsgType.Error", "tryParse BautRate failed: " + bautRate);
            //        return;
            //    }
            //}
            Log.Information(string.Format("portName={0}, bautRate={1}, parity={2}, dataLength={3}, stopBit={4}", m_PortName, m_BautRate, m_Parity, m_DataLength, m_StopBit));

            try
            {
                m_SerialPort = new SerialPort(m_PortName, m_BautRate, m_Parity, m_DataLength, m_StopBit);
                m_SerialPort.Open();
            }
            catch (Exception ex)
            {
                Log.Error(this.GetType().ToString(), "MsgType.Error", "Serial port open fail." + ex.Message);
                //throw ex;
            }
        }

        private bool InitDevice()
        {
            Log.Information("Start to check device.");
            var res = true;
            //var req = canBusHelper.SerialRequestPacket();
            ////req = new byte[] {0xEE,0x01,0x0A,0x01,0x01,0x00,0x00,0xFF,0x23,0x60};
            //var resp = SendReceive(req);
            //if (resp == null || resp.Length != 14 || resp[10] != 0x00)
            //{


            //    Log.Information(nameof(SerialCommunication), "MsgType.Error", $"Joint Init device fail.");
            //    res = false;
            //}
            //else
            //{

            //}

            return res;
        }
    }
}
