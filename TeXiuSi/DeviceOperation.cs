using CommunityToolkit.Mvvm.Messaging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeXiuSi.Helper;
using TeXiuSi.Protocol;

namespace TeXiuSi
{

    /// <summary>
    /// Inclusion of PEAK PCAN-Basic namespace
    /// </summary>
    using Peak.Can.Basic;
    using System.Threading;
    using TeXiuSi.PCAN;
    using TPCANBitrateFD = System.String;
    using TPCANHandle = System.UInt16;
    using TPCANTimestampFD = System.UInt64;


    public class DeviceOperation
    {

        private static Lazy<DeviceOperation> m_Instance = new Lazy<DeviceOperation>(() => new DeviceOperation());

        /// <summary>
        /// 获取单例
        /// </summary>
        /// <returns></returns>
        public static DeviceOperation GetInstance()
        {
            return m_Instance.Value;
        }
        /// <summary>
        /// 单例
        /// </summary>
        public static DeviceOperation Instance => DeviceOperation.GetInstance();

        /// <summary>
        /// 连接数据返回参数
        /// </summary>
        //public DataSourcePuller dataPuller => DataSourcePuller.Instance;



        public event EventHandler UpdateMessageType;



        public event EventHandler SurfaceViewUpdatePatient;

        //public SerialCommunication sc;

        public CanBusHelper _canBusHelper;


        private void ReceiveMessage<TMessage>(Action<TMessage> callback) where TMessage : class
        {
            WeakReferenceMessenger.Default.Register<TMessage>(this,
                ((recipient, message) => { callback.Invoke(message); }));
        }
        //WeakReferenceMessenger.Default.Send(new TherapyMessage(studyInfo.Id));

        #region  PCAN
        /// <summary>
        /// Saves the desired connection mode
        /// </summary>
        private bool m_IsFD;
        /// <summary>
        /// Saves the handle of a PCAN hardware
        /// </summary>
        public TPCANHandle m_PcanHandle;
        /// <summary>
        /// Saves the baudrate register for a conenction
        /// </summary>
        public TPCANBaudrate m_Baudrate;
        /// <summary>
        /// Saves the type of a non-plug-and-play hardware
        /// </summary>
        public TPCANType m_HwType;


        // Connnectiong
        public TPCANStatus _stsResult;

        public PANHelper _pANHelper;

        #endregion

        private DeviceOperation()
        {
            InitData();
        }

        private void InitData()
        {
            //CanId：电机的接收地址，用于控制器向电机发送控制命令

            //MstId：电机的发送地址，用于电机向控制器发送状态反馈
            //关节1: CanId = 0x001, MstId = 0x101
            //关节2: CanId = 0x002, MstId = 0x102
            //关节3: CanId = 0x003, MstId = 0x103
            //关节4: CanId = 0x004, MstId = 0x104
            //关节5: CanId = 0x005, MstId = 0x105
            //关节6: CanId = 0x006, MstId = 0x106
            //初始化Moto
            // 六轴机械臂的电机配置
            List<DmActData> armMotors = new List<DmActData>
            {
                // 关节1 - 基座
                // CanId=0x001: 控制器发送命令到这个地址
                // MstId=0x101: 电机从这个地址发送反馈数据
                new DmActData
                {
                    MotorType = DM_Motor_Type.DM4310,
                    Mode = Control_Mode.MIT,
                    CanId = 0x001,      // 控制命令地址
                    MstId = 0x101       // 状态反馈地址
                },
                // 关节2 - 肩部
                new DmActData
                {
                    MotorType = DM_Motor_Type.DM4310,
                    Mode = Control_Mode.MIT,
                    CanId = 0x002,      // 控制命令地址  
                    MstId = 0x102       // 状态反馈地址
                },
                // 关节3 - 肘部
                new DmActData
                {
                    MotorType = DM_Motor_Type.DM4310,
                    Mode = Control_Mode.MIT,
                    CanId = 0x003,      // 控制命令地址
                    MstId = 0x103       // 状态反馈地址
                },
                // 关节4 - 腕部1
                new DmActData
                {
                    MotorType = DM_Motor_Type.DM3507,
                    Mode = Control_Mode.MIT,
                    CanId = 0x004,      // 控制命令地址
                    MstId = 0x104       // 状态反馈地址
                },
                // 关节5 - 腕部2  
                new DmActData
                {
                    MotorType = DM_Motor_Type.DM3507,
                    Mode = Control_Mode.MIT,
                    CanId = 0x005,      // 控制命令地址
                    MstId = 0x105       // 状态反馈地址
                },
                // 关节6 - 末端
                new DmActData
                {
                    MotorType = DM_Motor_Type.DM3507,
                    Mode = Control_Mode.MIT,
                    CanId = 0x006,      // 控制命令地址
                    MstId = 0x106       // 状态反馈地址
                }
            };


            //Task.Run(() =>
            //{
            //    //_canBusHelper = new CanBusHelper();//初始化

            //    var listSerialprot = CanBusHelper.GetAvailablePorts();

            //    if (listSerialprot == null & listSerialprot.Count == 0)
            //    {

            //    }
            //    Log.Information($"CAN(COM) List {string.Join(".", listSerialprot)}");
            //    if (true)
            //    {

            //    }
            //});


            _pANHelper = new PANHelper();

        }

        /// <summary>
        /// 清空
        /// </summary>
        public void ClearInit()
        {
            m_PcanHandle = 0;
            m_Baudrate = 0;
            m_Baudrate = 0;
            _stsResult=new TPCANStatus();
        }

        #region  CANControl
        public void Connect(UInt32 IOPort,
            UInt16 Interrupt)
        {

            // Connects a selected PCAN-Basic channel
            //
            //if (m_IsFD)
            //    stsResult = PCANBasic.InitializeFD(
            //        m_PcanHandle,
            //        txtBitrate.Text);
            //else
            _stsResult = PCANBasic.Initialize(
                DeviceOperation.Instance.m_PcanHandle,
                DeviceOperation.Instance.m_Baudrate,
                DeviceOperation.Instance.m_HwType,
                IOPort,
                 Interrupt);

            if (_stsResult != TPCANStatus.PCAN_ERROR_OK)
                if (_stsResult != TPCANStatus.PCAN_ERROR_CAUTION)
                    Log.Error(DeviceOperation.Instance._pANHelper.GetFormatedError(_stsResult));
                else
                {

                    Log.Information("The bitrate being used is different than the given one");

                    _stsResult = TPCANStatus.PCAN_ERROR_OK;
                }
            else
                // Prepares the PCAN-Basic's PCAN-Trace file
                //
                ConfigureTraceFile();

            // Sets the connection status of the main-form
            //
            //SetConnectionStatus(stsResult == TPCANStatus.PCAN_ERROR_OK);

        }
        private void Release()
        {
            // Releases a current connected PCAN-Basic channel
            //
            PCANBasic.Uninitialize(m_PcanHandle);
            //tmrRead.Enabled = false;
            //if (m_ReadThread != null)
            //{
            //    m_ReadThread.Abort();
            //    m_ReadThread.Join();
            //    m_ReadThread = null;
            //}

            // Sets the connection status of the main-form
            //
            //SetConnectionStatus(false);
        }
        /// <summary>
        /// Configures the PCAN-Trace file for a PCAN-Basic Channel
        /// </summary>
        private void ConfigureTraceFile()
        {
            UInt32 iBuffer;
            TPCANStatus stsResult;

            // Configure the maximum size of a trace file to 5 megabytes
            //
            iBuffer = 5;
            stsResult = PCANBasic.SetValue(m_PcanHandle, TPCANParameter.PCAN_TRACE_SIZE, ref iBuffer, sizeof(UInt32));
            if (stsResult != TPCANStatus.PCAN_ERROR_OK)

                Log.Error(_pANHelper.GetFormatedError(stsResult));

            // Configure the way how trace files are created: 
            // * Standard name is used
            // * Existing file is ovewritten, 
            // * Only one file is created.
            // * Recording stopts when the file size reaches 5 megabytes.
            //
            iBuffer = PCANBasic.TRACE_FILE_SINGLE | PCANBasic.TRACE_FILE_OVERWRITE;
            stsResult = PCANBasic.SetValue(m_PcanHandle, TPCANParameter.PCAN_TRACE_CONFIGURE, ref iBuffer, sizeof(UInt32));
            if (stsResult != TPCANStatus.PCAN_ERROR_OK)
                Log.Error(_pANHelper.GetFormatedError(stsResult));
        }

        #endregion


        ~DeviceOperation()
        {
            Release();
            ClearInit();
        }
    }
}
