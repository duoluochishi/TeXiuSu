using CommunityToolkit.Mvvm.Messaging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using TeXiuSi.Helper;
using TeXiuSi.Protocol;

namespace TeXiuSi
{
    using Microsoft.Win32;

    /// <summary>
    /// Inclusion of PEAK PCAN-Basic namespace
    /// </summary>
    using Peak.Can.Basic;
    using Peak.Can.Basic.BackwardCompatibility;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows;
    using System.Windows.Forms;
    using System.Windows.Markup;
    using System.Windows.Media.Media3D;
    using System.Windows.Threading;
    using TeXiuSi.Model;
    using TeXiuSi.PCAN;
    using TPCANBitrateFD = System.String;
    using TPCANHandle = System.UInt16;
    using TPCANParameter = Peak.Can.Basic.BackwardCompatibility.TPCANParameter;
    using TPCANTimestampFD = System.UInt64;


    public class DeviceOperation
    {
        #region Delegates
        /// <summary>
        /// Read-Delegate Handler
        /// </summary>
        private delegate void ReadDelegateHandler();

        private ReadDelegateHandler m_ReadDelegate;
        #endregion

        private static Lazy<DeviceOperation> m_Instance = new Lazy<DeviceOperation>(() => new DeviceOperation());

        /// <summary>
        /// Stores the status of received messages for its display
        /// </summary>
        private System.Collections.ArrayList m_LastMsgsList;
        public bool IsRunning { get; private set; }

        public readonly BroadcastBlock<MotorFeedbackFrame> bufferBlock = new BroadcastBlock<MotorFeedbackFrame>(data => data);
        /// <summary>
        /// 获取单例
        /// </summary>
        /// <returns></returns>
        public static DeviceOperation GetInstance()
        {
            return m_Instance.Value;
        }
        /// <summary>
        /// 单例实例
        /// </summary>
        public static DeviceOperation Instance => m_Instance.Value;

        /// <summary>
        /// 连接数据返回参数
        /// </summary>
        //public DataSourcePuller dataPuller => DataSourcePuller.Instance;

        /// <summary>
        /// Receive-Event
        /// </summary>
        private System.Threading.AutoResetEvent m_ReceiveEvent;

        public event EventHandler UpdateMessageType;



        public event EventHandler SurfaceViewUpdatePatient;

        //public SerialCommunication sc;

        public CanBusHelper _canBusHelper;

        public MotorControl _motorControl;

        public List<Joint> joints = null;

        public Model3D model3D;

        public bool isInJointsPage;


        #region  Thread for message reading (using events)
        public bool rdbEvent;

        private System.Threading.Thread m_ReadThread;
        #endregion


        #region 全局设置参数

        public float Speed;

        /// <summary>
        /// 使能/失能
        /// </summary>
        public bool bolEnable;
        #endregion


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
            bolEnable = false;
            rdbEvent = false;
            isInJointsPage = false;
            IsRunning = false;
            // Creates the event used for signalize incomming messages 
            //
            m_ReceiveEvent = new System.Threading.AutoResetEvent(false);

            //链接后调用
            //InitMessageRev();
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
            _motorControl = new MotorControl();

            model3D = _motorControl.Initialize_Environment(_motorControl.modelsNames, out joints);
        }


        #region eventResive

        public async Task InitMessageRev()
        {
            await CANReadThreadFunc();
            //System.Threading.ThreadStart threadDelegate = new System.Threading.ThreadStart(this.CANReadThreadFunc);
            //m_ReadThread = new System.Threading.Thread(threadDelegate);
            ////m_ReadThread.IsBackground = true;
            //m_ReadThread.Start();
        }
        /// <summary>
        /// Thread-Function used for reading PCAN-Basic messages
        /// </summary>
        private async Task CANReadThreadFunc()
        {


            UInt32 iBuffer;
            TPCANStatus stsResult;

            iBuffer = System.Convert.ToUInt32(m_ReceiveEvent.SafeWaitHandle.DangerousGetHandle().ToInt32());
            // Sets the handle of the Receive-Event.
            //
            stsResult = PCANBasic.SetValue(m_PcanHandle, TPCANParameter.PCAN_RECEIVE_EVENT, ref iBuffer, sizeof(UInt32));

            if (stsResult != TPCANStatus.PCAN_ERROR_OK)
            {
                Log.Error(_pANHelper.GetFormatedError(stsResult), "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            await ReadCanMessagesAsync();
            // While this mode is selected
            //while (rdbEvent)
            //{
            //    // Waiting for Receive-Event
            //    // 
            //    if (m_ReceiveEvent.WaitOne(50))
            //        // Process Receive-Event using .NET Invoke function
            //        // in order to interact with Winforms UI (calling the 
            //        // function ReadMessages)
            //        // 
            //        this.Invoke(m_ReadDelegate);
            //    // 1. 获取主 UI 线程的 Dispatcher
            //    //Dispatcher uiDispatcher = Application.Current.Dispatcher;

            //    // 2. 使用 Dispatcher.Invoke() 或 InvokeAsync() 执行委托
            //    // Invoke() 是同步阻塞调用（等待 UI 更新完成）
            //    // InvokeAsync() 是异步非阻塞调用（推荐）

            //    //uiDispatcher.Invoke(m_ReadDelegate);

            //}
        }
        #endregion

        /// <summary>
        /// 清空
        /// </summary>
        public void ClearInit()
        {
            m_PcanHandle = 0;
            m_Baudrate = TPCANBaudrate.PCAN_BAUD_1M;
            _stsResult = new TPCANStatus();
        }

        #region  CANControl
        public void Connect(UInt32 IOPort,
            UInt16 Interrupt, TPCANBaudrate m_Baudrate, TPCANType m_HwType)
        {

            // Connects a selected PCAN-Basic channel
            //
            //if (m_IsFD)
            //    stsResult = PCANBasic.InitializeFD(
            //        m_PcanHandle,
            //        txtBitrate.Text);
            //else
            _stsResult = PCANBasic.Initialize(
                m_PcanHandle,
                m_Baudrate,
                m_HwType,
                IOPort,
                 Interrupt);

            //if (_stsResult != TPCANStatus.PCAN_ERROR_OK)
            //    if (_stsResult != TPCANStatus.PCAN_ERROR_CAUTION)
            //        Log.Error(DeviceOperation.Instance._pANHelper.GetFormatedError(_stsResult));
            //    else
            //    {

            //        Log.Information("The bitrate being used is different than the given one");

            //        _stsResult = TPCANStatus.PCAN_ERROR_OK;
            //    }
            //else
            //    // Prepares the PCAN-Basic's PCAN-Trace file
            //    //
            //    ConfigureTraceFile();



            if (_stsResult == TPCANStatus.PCAN_ERROR_OK)
            {
                // 连接/初始化成功
                // TPCANHandle 是有效的，可以开始读写
                Console.WriteLine($"通道 {m_PcanHandle} 初始化成功。");
                Log.Information($"通道 {m_PcanHandle} 初始化成功。");
                ConfigureTraceFile();

                _stsResult = TPCANStatus.PCAN_ERROR_OK;
            }
            else if (_stsResult == TPCANStatus.PCAN_ERROR_CAUTION)
            {
                // 初始化成功，但波特率与预设值不同（适用于 Bitrate Adapting 模式）
                Console.WriteLine($"通道 {m_PcanHandle} 初始化成功，但有警告。");
                ConfigureTraceFile();
                _stsResult = TPCANStatus.PCAN_ERROR_OK;
            }
            else if (_stsResult == TPCANStatus.PCAN_ERROR_ILLHANDLE)
            {
                // 通道句柄无效，说明您选择的 PCAN_xx 句柄可能不正确
                Console.WriteLine($"错误：句柄无效。");
                Log.Error(DeviceOperation.Instance._pANHelper.GetFormatedError(_stsResult));
            }
            else if (_stsResult == TPCANStatus.PCAN_ERROR_ILLHW)
            {
                // 硬件不可用，可能是未插入或驱动问题
                Console.WriteLine($"错误：硬件不可用。");
                Log.Error(DeviceOperation.Instance._pANHelper.GetFormatedError(_stsResult));
            }
            else
            {
                // 其他错误
                Console.WriteLine($"通道初始化失败，错误代码：{_stsResult}");
                Log.Error(DeviceOperation.Instance._pANHelper.GetFormatedError(_stsResult));
            }

            // Sets the connection status of the main-form
            //
            //SetConnectionStatus(stsResult == TPCANStatus.PCAN_ERROR_OK);
            //链接后获取状态
            InitMessageRev();
        }
        private void Release()
        {
            // Releases a current connected PCAN-Basic channel
            //
            PCANBasic.Uninitialize(m_PcanHandle);
            if (m_ReadThread != null)
            {
                m_ReadThread.Abort();
                m_ReadThread.Join();
                m_ReadThread = null;
            }

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

        #region 整体控制


        /// <summary>
        /// 使能/失能所有关机
        /// </summary>
        public void ControlConnectOfJoints(ControlPowModel controlPowModel)
        {
            try
            {
                if (_stsResult != TPCANStatus.PCAN_ERROR_OK)
                {
                    Log.Error($"失能失败连接丢失");
                    return;
                }

                switch (controlPowModel)
                {
                    case ControlPowModel.PositionSpd:
                        bolEnable = true;
                        break;
                    case ControlPowModel.Spd:
                        bolEnable = false;
                        break;
                    default:
                        break;
                }

                foreach (JointNode opType in Enum.GetValues(typeof(JointNode)))
                {
                    var typeCommad = _motorControl.ControlCmd((byte)controlPowModel);

                    Write(((int)opType).ToString("X2"), typeCommad);
                }


            }
            catch (Exception ex)
            {
                Log.Error($"失能失败{ex.Message}");
            }
        }




        /// <summary>
        /// 清除错误/保存零点
        /// </summary>
        public void ClearTheErrorOfJoints(ControlPowModelOther controlPowModelOther)
        {
            try
            {
                if (_stsResult != TPCANStatus.PCAN_ERROR_OK)
                {
                    Log.Error($"失能失败连接丢失");
                    return;
                }


                foreach (JointNode opType in Enum.GetValues(typeof(JointNode)))
                {
                    var typeCommad = _motorControl.ControlCmd((byte)controlPowModelOther);

                    Write(((int)opType).ToString("X2"), typeCommad);
                }


            }
            catch (Exception ex)
            {
                Log.Error($"清除错误/保存零点失败{ex.Message}");
            }
        }

        /// <summary>
        /// 刷新数据
        /// </summary>
        public void RefreshInfoEditorOfJoints(ControlPowModelOther controlPowModelOther, Register register)
        {
            try
            {
                if (_stsResult != TPCANStatus.PCAN_ERROR_OK)
                {
                    Log.Error($"失能失败连接丢失");
                    return;
                }
                //这里先写死

                foreach (JointNode opType in Enum.GetValues(typeof(JointNode)))
                {
                    var typeCommad = _motorControl.ControlRefreshCmd((byte)opType, (byte)controlPowModelOther, (byte)register);

                    Write(((int)opType).ToString("X2"), typeCommad);
                }


            }
            catch (Exception ex)
            {
                Log.Error($"清除错误/保存零点失败{ex.Message}");
            }
        }

        /// <summary>
        ///  发送关节运动命令
        /// </summary>
        /// <param name="controlFrame"></param>
        /// <param name="ID"></param>
        /// <param name="positionDes">弧度</param>
        /// <param name="velocityDes"></param>
        /// <param name="currentLimit"></param>
        public void ControlJointsMove(ControlFrame controlFrame, double[] positionDess, float velocityDes, float currentLimit)
        {

            try
            {
                if (_stsResult != TPCANStatus.PCAN_ERROR_OK)
                {
                    Log.Error($"运动失败连接丢失");
                    return;
                }
                int i = 0;

                foreach (JointNode opType in Enum.GetValues(typeof(JointNode)))
                {
                    CANCommand cANCommand = _motorControl.SendPositionAndSpeeed(controlFrame, (byte)opType, (float)positionDess[i], Speed, 1);

                    Write(((int)opType).ToString("X2"), cANCommand.Data);
                    i++;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"清除错误/保存零点失败{ex.Message}");
            }
        }


        #endregion


        #region 单个关节控制
        public void ControlConnectOfJoints(ControlPowModel controlPowModel, JointNode opType)
        {
            try
            {
                if (_stsResult != TPCANStatus.PCAN_ERROR_OK)
                {
                    Log.Error($"失能失败连接丢失");
                    return;
                }

                switch (controlPowModel)
                {
                    case ControlPowModel.PositionSpd:
                        bolEnable = true;
                        break;
                    case ControlPowModel.Spd:
                        bolEnable = false;
                        break;
                    default:
                        break;
                }


                var typeCommad = _motorControl.ControlCmd((byte)controlPowModel);

                Write(((int)JointNode.Nodel1).ToString("X2"), typeCommad);

            }
            catch (Exception ex)
            {
                Log.Error($"失能失败{ex.Message}");
            }
        }
        /// <summary>
        /// 清除错误/保存零点
        /// </summary>
        public void ClearTheErrorOfJoints(ControlPowModelOther controlPowModelOther, JointNode opType)
        {
            try
            {
                if (_stsResult != TPCANStatus.PCAN_ERROR_OK)
                {
                    Log.Error($"失能失败连接丢失");
                    return;
                }


                var typeCommad = _motorControl.ControlCmd((byte)controlPowModelOther);

                Write(((int)JointNode.Nodel1).ToString("X2"), typeCommad);



            }
            catch (Exception ex)
            {
                Log.Error($"清除错误/保存零点失败{ex.Message}");
            }
        }
        /// <summary>
        /// 写入参数值
        /// </summary>
        /// <param name="jointNode"></param>
        /// <param name="register"></param>
        /// <param name="Value"></param>
        public void SetParam(JointNode jointNode, Register register, float Value)
        {
            try
            {
                byte CanIDL = 0;
                byte CanIDH = 0;

                if (_stsResult != TPCANStatus.PCAN_ERROR_OK)
                {
                    Log.Error($"失能失败连接丢失");
                    return;
                }
                _motorControl.SplitCanIdForLittleEndian((byte)jointNode, out CanIDL, out CanIDH);


                CANCommand cANCommand = _motorControl.WriteParam(CanIDL, CanIDH, register, Value);


                Write(((int)JointNode.Nodel1).ToString("X2"), cANCommand.Data);



            }
            catch (Exception ex)
            {
                Log.Error($"清除错误/保存零点失败{ex.Message}");
            }
        }
        #endregion

        /// <summary>
        /// 写入消息
        /// </summary>
        /// <param name="textID"></param>
        /// <param name="byteMessage"></param>
        public void Write(string textID, byte[] byteMessage)
        {

            TPCANStatus stsResult;

            // Send the message
            //
            stsResult = m_IsFD ? WriteFrameFD() : WriteFrame(textID, byteMessage);

            // The message was successfully sent
            //
            if (stsResult == TPCANStatus.PCAN_ERROR_OK)
                Log.Information("Message was successfully SENT");
            // An error occurred.  We show the error.
            //			
            else
                Log.Error(_pANHelper.GetFormatedError(stsResult));

        }
        private TPCANStatus WriteFrameFD()
        {
            TPCANMsgFD CANMsg;
            //TextBox txtbCurrentTextBox;
            //int iLength;

            // We create a TPCANMsgFD message structure 
            //
            CANMsg = new TPCANMsgFD();
            CANMsg.DATA = new byte[64];

            // We configurate the Message.  The ID,
            // Length of the Data, Message Type 
            // and the data
            //
            //CANMsg.ID = System.Convert.ToUInt32(txtID.Text, 16);
            //CANMsg.DLC = System.Convert.ToByte(8);
            //CANMsg.MSGTYPE = (chbExtended.Checked) ? TPCANMessageType.PCAN_MESSAGE_EXTENDED : TPCANMessageType.PCAN_MESSAGE_STANDARD;
            //CANMsg.DLC = System.Convert.ToByte(8);
            //CANMsg.MSGTYPE = (chbExtended.Checked) ? TPCANMessageType.PCAN_MESSAGE_EXTENDED : TPCANMessageType.PCAN_MESSAGE_STANDARD;
            //CANMsg.MSGTYPE |= (chbFD.Checked) ? TPCANMessageType.PCAN_MESSAGE_FD : TPCANMessageType.PCAN_MESSAGE_STANDARD;
            //CANMsg.MSGTYPE |= (chbBRS.Checked) ? TPCANMessageType.PCAN_MESSAGE_BRS : TPCANMessageType.PCAN_MESSAGE_STANDARD;

            //// If a remote frame will be sent, the data bytes are not important.
            ////
            //if (chbRemote.Checked)
            //    CANMsg.MSGTYPE |= TPCANMessageType.PCAN_MESSAGE_RTR;
            //else
            //{
            //    // We get so much data as the Len of the message
            //    //
            //    iLength = MotorControl.GetLengthFromDLC(CANMsg.DLC, (CANMsg.MSGTYPE & TPCANMessageType.PCAN_MESSAGE_FD) == 0);
            //    for (int i = 0; i < iLength; i++)
            //    {
            //        txtbCurrentTextBox = (TextBox)this.Controls.Find("txtData" + i.ToString(), true)[0];
            //        CANMsg.DATA[i] = System.Convert.ToByte(txtbCurrentTextBox.Text, 16);
            //    }
            //}

            // The message is sent to the configured hardware
            //
            return PCANBasic.WriteFD(m_PcanHandle, ref CANMsg);
        }
        private TPCANStatus WriteFrame(string txtID, byte[] bytesWrite)
        {
            if (bytesWrite == null || bytesWrite.Count() == 0)
            {
                Log.Error("dont have any message");
                return TPCANStatus.PCAN_ERROR_XMTFULL;
            }
            TPCANMsg CANMsg;
            //TextBox txtbCurrentTextBox;

            // We create a TPCANMsg message structure 
            //
            CANMsg = new TPCANMsg();
            CANMsg.DATA = new byte[8];

            // We configurate the Message.  The ID,
            // Length of the Data, Message Type
            // and the data
            //关节ID 或者下参数加偏移 ---待补充
            CANMsg.ID = System.Convert.ToUInt32(txtID, 16);
            CANMsg.LEN = System.Convert.ToByte(8);

            //扩展协议逻辑保留
            //CANMsg.MSGTYPE = (chbExtended.Checked) ? TPCANMessageType.PCAN_MESSAGE_EXTENDED : TPCANMessageType.PCAN_MESSAGE_STANDARD;
            CANMsg.MSGTYPE = TPCANMessageType.PCAN_MESSAGE_STANDARD;
            // If a remote frame will be sent, the data bytes are not important.
            //
            //if (chbRemote.Checked)
            //    CANMsg.MSGTYPE |= TPCANMessageType.PCAN_MESSAGE_RTR;
            //else
            //{
            // We get so much data as the Len of the message
            //
            for (int i = 0; i < MotorControl.GetLengthFromDLC(CANMsg.LEN, true); i++)
            {
                //txtbCurrentTextBox = bytesWrite[0];
                CANMsg.DATA[i] = bytesWrite[i];
            }
            //}

            // The message is sent to the configured hardware
            //
            return PCANBasic.Write(m_PcanHandle, ref CANMsg);
        }
        #endregion

        #region Message
        private CancellationTokenSource _cts; // 用于取消任务的Token Source
        //// 调用启动：
        //_ = ReadCanMessagesAsync();
        /// <summary>
        /// Thread-Function used for reading PCAN-Basic messages
        /// </summary>
        private async Task ReadCanMessagesAsync()
        {
            // --- 1. 初始化设置 (同步，只执行一次) ---
            UInt32 iBuffer;
            TPCANStatus stsResult;

            // 获取事件句柄 (Handles/Pointers 必须使用适当的方法获取)
            // 注意：这里使用 Int32 是因为 PCANBasic.SetValue 的签名要求
            iBuffer = System.Convert.ToUInt32(m_ReceiveEvent.SafeWaitHandle.DangerousGetHandle().ToInt32());

            // 设置 PCAN 接收事件句柄
            stsResult = PCANBasic.SetValue(m_PcanHandle, TPCANParameter.PCAN_RECEIVE_EVENT, ref iBuffer, sizeof(UInt32));

            if (stsResult != TPCANStatus.PCAN_ERROR_OK)
            {
                Log.Error(_pANHelper.GetFormatedError(stsResult), "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // --- 2. 启动后台循环任务 (异步) ---
            _cts = new CancellationTokenSource();
            CancellationToken token = _cts.Token;

            try
            {
                // Task.Run 将阻塞的 WaitOne 循环移动到线程池
                await Task.Run(() => ReadLoopTask(token), token);
            }
            catch (OperationCanceledException)
            {
                // 任务被取消时的正常退出
            }
            catch (Exception ex)
            {
                // 记录其他异常
                Log.Error($"读取任务发生错误: {ex.Message}", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void ReadLoopTask(CancellationToken token)
        {
            // 在这个方法中，我们假设 rdbEvent.Checked 是从 UI 线程安全获取的，
            // 或者在启动任务时，rdbEvent 状态是固定的。
            TPCANStatus stsResult;
            while (!token.IsCancellationRequested) // 使用 Cancellation Token 来控制循环退出
            {
                //判断是否在关节状态界面 不是不用发
                if (true)
                {

                }
                // 阻塞调用，等待 CAN 接收事件（最大等待 50ms）
                //bool eventTriggered = m_ReceiveEvent.WaitOne(50);

                //if (eventTriggered)
                //{
                // 接收事件触发，需要将消息处理切换回 UI 线程
                // TaskScheduler.FromCurrentSynchronizationContext() 必须在 UI 线程上获取

                // 为了在 Task.Run 内部安全地获取 UI 线程上下文并执行 Invoke，
                // 必须使用 Task.Factory.StartNew，并指定之前获取到的 UI Context。

                // 假设您在 StartReadingAsync 启动之前已经获取了 UI 线程的上下文
                // 建议：在类级别存储 TaskScheduler.FromCurrentSynchronizationContext()

                // 假设我们现在直接使用 WinForms 的 Invoke（这在 Task.Run 内部是可行的）

                // *** 替代 this.Invoke(m_ReadDelegate) 的方法：***
                //this.Invoke(m_ReadDelegate); // 使用 WinForms/WPF 的 Invoke/Dispatcher.Invoke 
                // 确保 m_ReadDelegate (即 ReadMessages) 在 UI 线程执行

                //ReadMessage();
                stsResult = m_IsFD ? ReadMessageFD() : ReadMessage();
                if (stsResult != TPCANStatus.PCAN_ERROR_OK)
                    // If an error occurred, an information message is included
                    //
                    //IncludeTextMessage(GetFormatedError(stsResult));
                    //bufferBlock.SendAsync(data);
                    Log.Information("receive data success");
                //}

                // 检查 rdbEvent.Checked 是否被取消，如果 UI 控件发生变化，需要在 UI 线程处理
                // 简单的 WinForms/WPF 应用通常依赖于外部调用 StopReading 来退出循环
            }
        }
        // 停止读取的方法
        public void StopReading()
        {
            if (_cts != null)
                _cts.Cancel();
        }

        /// <summary>
        /// Function for reading messages on FD devices
        /// </summary>
        /// <returns>A TPCANStatus error code</returns>
        private TPCANStatus ReadMessageFD()
        {
            TPCANMsgFD CANMsg;
            TPCANTimestampFD CANTimeStamp;
            TPCANStatus stsResult;

            // We execute the "Read" function of the PCANBasic                
            //
            stsResult = PCANBasic.ReadFD(m_PcanHandle, out CANMsg, out CANTimeStamp);
            if (stsResult != TPCANStatus.PCAN_ERROR_QRCVEMPTY)
                // We process the received message
                //
                return stsResult;
            //ProcessMessage(CANMsg, CANTimeStamp);

            return stsResult;
        }

        /// <summary>
        /// Function for reading CAN messages on normal CAN devices
        /// </summary>
        /// <returns>A TPCANStatus error code</returns>
        private TPCANStatus ReadMessage()
        {
            TPCANMsg CANMsg;
            TPCANTimestamp CANTimeStamp;
            TPCANStatus stsResult;

            // We execute the "Read" function of the PCANBasic                
            //
            stsResult = PCANBasic.Read(m_PcanHandle, out CANMsg, out CANTimeStamp);
            if (stsResult != TPCANStatus.PCAN_ERROR_QRCVEMPTY)
                // We process the received message
                //
                ProcessMessage(CANMsg, CANTimeStamp);

            return stsResult;
        }

        #region Time读取法
        /// <summary>
        /// Function for reading PCAN-Basic messages
        /// </summary>
        //private void ReadMessagesTime()
        //{
        //    TPCANStatus stsResult;

        //    // We read at least one time the queue looking for messages.
        //    // If a message is found, we look again trying to find more.
        //    // If the queue is empty or an error occurr, we get out from
        //    // the dowhile statement.
        //    //			
        //    do
        //    {
        //        stsResult = m_IsFD ? ReadMessageFD() : ReadMessage();
        //        if (stsResult == TPCANStatus.PCAN_ERROR_ILLOPERATION)
        //            break;
        //    } while (IsRunning && (!System.Convert.ToBoolean(stsResult & TPCANStatus.PCAN_ERROR_QRCVEMPTY)));
        //}
        /// <summary>
        /// Processes a received message, in order to show it in the Message-ListView
        /// </summary>
        /// <param name="theMsg">The received PCAN-Basic message</param>
        /// <returns>True if the message must be created, false if it must be modified</returns>
        //private void ProcessMessage(TPCANMsgFD theMsg, TPCANTimestampFD itsTimeStamp)
        //{
        //    // We search if a message (Same ID and Type) is 
        //    // already received or if this is a new message
        //    //
        //    lock (m_LastMsgsList.SyncRoot)
        //    {
        //        foreach (MessageStatus msg in m_LastMsgsList)
        //        {
        //            if ((msg.CANMsg.ID == theMsg.ID) && (msg.CANMsg.MSGTYPE == theMsg.MSGTYPE))
        //            {
        //                // Modify the message and exit
        //                //
        //                msg.Update(theMsg, itsTimeStamp);
        //                return;
        //            }
        //        }
        //        // Message not found. It will created
        //        //
        //        InsertMsgEntry(theMsg, itsTimeStamp);
        //    }
        //}
        ///// <summary>
        ///// Inserts a new entry for a new message in the Message-ListView
        ///// </summary>
        ///// <param name="newMsg">The messasge to be inserted</param>
        ///// <param name="timeStamp">The Timesamp of the new message</param>
        //private void InsertMsgEntry(TPCANMsgFD newMsg, TPCANTimestampFD timeStamp)
        //{
        //    MessageStatus msgStsCurrentMsg;
        //    ListViewItem lviCurrentItem;

        //    lock (m_LastMsgsList.SyncRoot)
        //    {
        //        // We add this status in the last message list
        //        //
        //        msgStsCurrentMsg = new MessageStatus(newMsg, timeStamp, lstMessages.Items.Count);
        //        msgStsCurrentMsg.ShowingPeriod = chbShowPeriod.Checked;
        //        m_LastMsgsList.Add(msgStsCurrentMsg);

        //        // Add the new ListView Item with the Type of the message
        //        //	
        //        lviCurrentItem = lstMessages.Items.Add(msgStsCurrentMsg.TypeString);
        //        // We set the ID of the message
        //        //
        //        lviCurrentItem.SubItems.Add(msgStsCurrentMsg.IdString);
        //        // We set the length of the Message
        //        //
        //        lviCurrentItem.SubItems.Add(MotorControl.GetLengthFromDLC(newMsg.DLC, (newMsg.MSGTYPE & TPCANMessageType.PCAN_MESSAGE_FD) == 0).ToString());
        //        // we set the message count message (this is the First, so count is 1)            
        //        //
        //        lviCurrentItem.SubItems.Add(msgStsCurrentMsg.Count.ToString());
        //        // Add time stamp information if needed
        //        //
        //        lviCurrentItem.SubItems.Add(msgStsCurrentMsg.TimeString);
        //        // We set the data of the message. 	
        //        //
        //        lviCurrentItem.SubItems.Add(msgStsCurrentMsg.DataString);
        //    }
        //}
        #endregion

        /// <summary>
        /// Processes a received message, in order to show it in the Message-ListView
        /// </summary>
        /// <param name="theMsg">The received PCAN-Basic message</param>
        /// <returns>True if the message must be created, false if it must be modified</returns>
        private void ProcessMessage(TPCANMsg theMsg, TPCANTimestamp itsTimeStamp)
        {
            try
            {
                TPCANMsgFD newMsg;
                TPCANTimestampFD newTimestamp;

                newMsg = new TPCANMsgFD();
                newMsg.DATA = new byte[64];
                newMsg.ID = theMsg.ID;
                newMsg.DLC = theMsg.LEN;
                for (int i = 0; i < ((theMsg.LEN > 8) ? 8 : theMsg.LEN); i++)
                    newMsg.DATA[i] = theMsg.DATA[i];
                newMsg.MSGTYPE = theMsg.MSGTYPE;

                newTimestamp = System.Convert.ToUInt64(itsTimeStamp.micros + 1000 * itsTimeStamp.millis + 0x100000000 * 1000 * itsTimeStamp.millis_overflow);
                Log.Information($"recevie Data {newMsg.DATA}");
                //InsertMsgEntry(theMsg, itsTimeStamp);


                MessageStatus msgStsCurrentMsg = new MessageStatus(newMsg, newTimestamp, 1);



                //转换信息-出来第一位有些不同

                MotorFeedbackFrame frame = MotorFeedbackFrame.Parse(newMsg.DATA);

                bufferBlock.SendAsync(frame);
            }
            catch (Exception ex)
            {

                Log.Error($"receive data error{ex.Message}");
            }

        }


        #endregion

        ~DeviceOperation()
        {
            Release();
            ClearInit();
        }
    }
}
