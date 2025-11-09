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


            Task.Run(() =>
            {
                //_canBusHelper = new CanBusHelper();//初始化

                var listSerialprot = CanBusHelper.GetAvailablePorts();

                if (listSerialprot == null & listSerialprot.Count == 0)
                {

                }
                Log.Information($"CAN(COM) List {string.Join(".", listSerialprot)}");
                if (true)
                {

                }
            });
        }


        ~DeviceOperation()
        {
        }
    }
}
