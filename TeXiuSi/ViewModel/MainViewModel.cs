using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TeXiuSi.Helper;
using TeXiuSi.Model;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace TeXiuSi.ViewModel
{
    public partial class MainViewModel : ObservableObject
    {

        #region 

        //失能 急停 视角还原 设置（直接）disabled scram
        public IRelayCommand DisabledCommand { get; }

        public IRelayCommand ScramCommand { get; }

        #endregion


        #region motion control


        public IRelayCommand ZMinusCommand1 { get; }
        public IRelayCommand YMinusCommand1 { get; }
        public IRelayCommand XPlusCommand1 { get; }
        public IRelayCommand ZPlusCommand1 { get; }
        public IRelayCommand YPlusCommand1 { get; }
        public IRelayCommand XMinusCommand1 { get; }

        // 第二个GroupBox的命令
        public IRelayCommand ZMinusCommand2 { get; }
        public IRelayCommand YMinusCommand2 { get; }
        public IRelayCommand XPlusCommand2 { get; }
        public IRelayCommand ZPlusCommand2 { get; }
        public IRelayCommand YPlusCommand2 { get; }
        public IRelayCommand XMinusCommand2 { get; }

        // 底部按钮命令
        public IRelayCommand ResetToZeroCommand { get; }
        public IRelayCommand LoadCommand { get; }
        public IRelayCommand SendCommand { get; }



        #region 点位运动
        //private string _joint1Angle;
        //public string Joint1Angle
        //{
        //    get { return _joint1Angle; }
        //    set { _joint1Angle = value; OnPropertyChanged(); }
        //}
        //private string _joint2Angle;
        //public string Joint2Angle
        //{
        //    get { return _joint2Angle; }
        //    set { _joint2Angle = value; OnPropertyChanged(); }
        //}
        //private string _joint4Angle;
        //public string Joint4Angle
        //{
        //    get { return _joint4Angle; }
        //    set { _joint4Angle = value; OnPropertyChanged(); }
        //}
        //private string _joint5Angle;
        //public string Joint5Angle
        //{
        //    get { return _joint5Angle; }
        //    set { _joint5Angle = value; OnPropertyChanged(); }
        //}
        //private string _joint6Angle;
        //public string Joint6Angle
        //{
        //    get { return _joint6Angle; }
        //    set { _joint6Angle = value; OnPropertyChanged(); }
        //}
        //private string _joint3Angle;
        //public string Joint3Angle
        //{
        //    get { return _joint3Angle; }
        //    set { _joint3Angle = value; OnPropertyChanged(); }
        //}

        private double _xvalue = 1; // 初始值
        public double XValue
        {
            get => _xvalue;
            set => SetProperty(ref _xvalue, value);
        }

        private double _yvalue = 1; // 初始值
        public double YValue
        {
            get => _yvalue;
            set => SetProperty(ref _yvalue, value);
        }

        private double _zvalue = 1; // 初始值
        public double ZValue
        {
            get => _zvalue;
            set => SetProperty(ref _zvalue, value);
        }

        private double _rollvalue = 1; // 初始值
        public double Rollvalue
        {
            get => _rollvalue;
            set => SetProperty(ref _rollvalue, value);
        }

        private double _pitchvalue = 1; // 初始值
        public double Pitchvalue
        {
            get => _pitchvalue;
            set => SetProperty(ref _pitchvalue, value);
        }

        private double _yawvalue = 1; // 初始值
        public double YawValue
        {
            get => _yawvalue;
            set => SetProperty(ref _yawvalue, value);
        }

       

        // 设置步长变量
        private int _stepXValue = 1;
        public int StepXValue
        {
            get => _stepXValue;
            set => SetProperty(ref _stepXValue, value);
        }


        private double _stepYvalue = 1; // 初始值
        public double StepYValue
        {
            get => _stepYvalue;
            set => SetProperty(ref _stepYvalue, value);
        }

        private double _stepZvalue = 1; // 初始值
        public double StepZValue
        {
            get => _stepZvalue;
            set => SetProperty(ref _stepZvalue, value);
        }

        private double _stepRollvalue = 1; // 初始值
        public double StepRollvalue
        {
            get => _stepRollvalue;
            set => SetProperty(ref _stepRollvalue, value);
        }

        private double _stepPitchvalue = 1; // 初始值
        public double StepPitchvalue
        {
            get => _stepPitchvalue;
            set => SetProperty(ref _stepPitchvalue, value);
        }

        private double _stepYawvalue = 1; // 初始值
        public double StepYawValue
        {
            get => _stepYawvalue;
            set => SetProperty(ref _stepYawvalue, value);
        }
        #endregion



        #endregion

        #region ArcMotionVisible
        // 圆弧运动相关的命令
        public IRelayCommand LoadInstructionPointCommand { get; }
        public IRelayCommand SendInstructionPointCommand { get; }
        public IRelayCommand DrawArcCommand { get; }

        // 坐标点属性（用于绑定doubleUpDown的值）
        [ObservableProperty]
        private double StartPointX;

        [ObservableProperty]
        private double StartPointY;

        [ObservableProperty]
        private double StartPointZ;

        [ObservableProperty]
        private double StartPointRx;

        [ObservableProperty]
        private double StartPointRy;

        [ObservableProperty]
        private double StartPointRz;

        [ObservableProperty]
        private double MidPointX;

        [ObservableProperty]
        private double MidPointY;

        [ObservableProperty]
        private double MidPointZ;

        [ObservableProperty]
        private double MidPointRx;

        [ObservableProperty]
        private double MidPointRy;

        [ObservableProperty]
        private double MidPointRz;

        [ObservableProperty]
        private double EndPointX;

        [ObservableProperty]
        private double EndPointY;

        [ObservableProperty]
        private double EndPointZ;

        [ObservableProperty]
        private double EndPointRx;

        [ObservableProperty]
        private double EndPointRy;

        [ObservableProperty]
        private double EndPointRz;

        [ObservableProperty]
        private int SelectedInstructionPointIndex;
        #endregion


        #region JointMotionVisible
        // 关节角度属性
        [ObservableProperty]
        public string Joint1Angle = "0.000";

        [ObservableProperty]
        public string Joint2Angle = "0.000";

        [ObservableProperty]
        public string Joint3Angle = "0.000";

        [ObservableProperty]
        public string Joint4Angle = "0.000";

        [ObservableProperty]
        public string Joint5Angle = "0.000";

        [ObservableProperty]
        public string Joint6Angle = "0.000";

        // 增减步长
        public const double StepIncrement = 1.0;

        // 关节增减命令
        public IRelayCommand Joint1DecreaseCommand { get; }
        public IRelayCommand Joint1IncreaseCommand { get; }
        public IRelayCommand Joint2DecreaseCommand { get; }
        public IRelayCommand Joint2IncreaseCommand { get; }
        public IRelayCommand Joint3DecreaseCommand { get; }
        public IRelayCommand Joint3IncreaseCommand { get; }
        public IRelayCommand Joint4DecreaseCommand { get; }
        public IRelayCommand Joint4IncreaseCommand { get; }
        public IRelayCommand Joint5DecreaseCommand { get; }
        public IRelayCommand Joint5IncreaseCommand { get; }
        public IRelayCommand Joint6DecreaseCommand { get; }
        public IRelayCommand Joint6IncreaseCommand { get; }

        // 底部按钮命令
        public IRelayCommand JointResetToZeroCommand { get; }
        public IRelayCommand JointEditCommand { get; }
        #endregion

        #region StraightLineMotionVisible
        // 直线运动坐标属性
        [ObservableProperty]
        public string StraightLineX = "0.000";

        [ObservableProperty]
        public string StraightLineY = "0.000";

        [ObservableProperty]
        public string StraightLineZ = "0.000";

        [ObservableProperty]
        public string StraightLineRx = "0.000";

        [ObservableProperty]
        public string StraightLineRy = "0.000";

        [ObservableProperty]
        public string StraightLineRz = "0.000";

        // 坐标增减步长（mm）
        private const double PositionStepIncrement = 1.0;

        // 角度增减步长（度）
        private const double RotationStepIncrement = 1.0;

        // 坐标增减命令
        public IRelayCommand XDecreaseCommand { get; }
        public IRelayCommand XIncreaseCommand { get; }
        public IRelayCommand YDecreaseCommand { get; }
        public IRelayCommand YIncreaseCommand { get; }
        public IRelayCommand ZDecreaseCommand { get; }
        public IRelayCommand ZIncreaseCommand { get; }
        public IRelayCommand RxDecreaseCommand { get; }
        public IRelayCommand RxIncreaseCommand { get; }
        public IRelayCommand RyDecreaseCommand { get; }
        public IRelayCommand RyIncreaseCommand { get; }
        public IRelayCommand RzDecreaseCommand { get; }
        public IRelayCommand RzIncreaseCommand { get; }

        // 底部按钮命令
        public IRelayCommand StraightLineResetToZeroCommand { get; }
        public IRelayCommand StraightLineEditCommand { get; }
        #endregion


        #region 联动控制


        public IRelayCommand LinkageControlCommand { get; }


        private EnumBindingItem<LinkageSettings> _selectedLinkageSettingsMode;
        public EnumBindingItem<LinkageSettings> SelectedLinkageSettingsMode
        {
            get { return _selectedLinkageSettingsMode; }
            set
            {
                _selectedLinkageSettingsMode = value;
                OnPropertyChanged();
                if (_selectedLinkageSettingsMode != null)
                {
                    Console.WriteLine($"选择了: {_selectedLinkageSettingsMode.DisplayName}，枚举值为: {_selectedLinkageSettingsMode.Value}");
                }
            }
        }

        private EnumBindingItem<FeedbackCommand> _selectedFeedbackCommandMode;
        public EnumBindingItem<FeedbackCommand> SelectedFeedbackCommandMode
        {
            get { return _selectedFeedbackCommandMode; }
            set
            {
                _selectedFeedbackCommandMode = value;
                OnPropertyChanged();
                if (_selectedFeedbackCommandMode != null)
                {
                    Console.WriteLine($"选择了: {_selectedFeedbackCommandMode.DisplayName}，枚举值为: {_selectedFeedbackCommandMode.Value}");
                }
            }
        }


        #endregion


        #region 夹爪控制


        public IRelayCommand JawControlCommand { get; }



        private EnumBindingItem<EnergyState> _selectedJawStatus;
        public EnumBindingItem<EnergyState> SelectedJawStatus
        {
            get { return _selectedJawStatus; }
            set
            {
                _selectedJawStatus = value;
                OnPropertyChanged();
                if (_selectedJawStatus != null)
                {
                    Console.WriteLine($"选择了: {_selectedJawStatus.DisplayName}，枚举值为: {_selectedJawStatus.Value}");
                }
            }
        }


        private ObservableCollection<EnumBindingItem<EnergyState>> _jawStatus;
        public ObservableCollection<EnumBindingItem<EnergyState>> JawStatus
        {
            get { return _jawStatus; }
            set { _jawStatus = value; OnPropertyChanged(); }
        }

        private EnumBindingItem<YesOrNo> _selectedJClearAbout;
        public EnumBindingItem<YesOrNo> SelectedClearAbout
        {
            get { return _selectedJClearAbout; }
            set
            {
                _selectedJClearAbout = value;
                OnPropertyChanged();
                if (_selectedJClearAbout != null)
                {
                    Console.WriteLine($"选择了: {_selectedJClearAbout.DisplayName}，枚举值为: {_selectedJClearAbout.Value}");
                }
            }
        }


        private ObservableCollection<EnumBindingItem<YesOrNo>> _beClearAboutMistakes;
        public ObservableCollection<EnumBindingItem<YesOrNo>> BeClearAboutMistakes
        {
            get { return _beClearAboutMistakes; }
            set { _beClearAboutMistakes = value; OnPropertyChanged(); }
        }

        private EnumBindingItem<YesOrNo> _selectedJSetZero;
        public EnumBindingItem<YesOrNo> SelectedSetZero
        {
            get { return _selectedJSetZero; }
            set
            {
                _selectedJSetZero = value;
                OnPropertyChanged();
                if (_selectedJSetZero != null)
                {
                    Console.WriteLine($"选择了: {_selectedJSetZero.DisplayName}，枚举值为: {_selectedJSetZero.Value}");
                }
            }
        }


        private ObservableCollection<EnumBindingItem<YesOrNo>> _setZero;
        public ObservableCollection<EnumBindingItem<YesOrNo>> SetZero
        {
            get { return _setZero; }
            set { _setZero = value; OnPropertyChanged(); }
        }



        #endregion

        // 通用的枚举绑定项类（如果尚未定义）
        public class EnumBindingItem<T> where T : Enum
        {
            public string DisplayName { get; set; }
            public T Value { get; set; }
        }

        // 用于存储所有操作模式选项
        public ObservableCollection<EnumBindingItem<OperationType>> OperationModes { get; set; }

        public ObservableCollection<EnumBindingItem<SportType>> SportTypes { get; set; }

        private EnumBindingItem<OperationType> _selectedOperationMode;

        // 绑定到ComboBox的SelectedItem属性
        public EnumBindingItem<OperationType> SelectedOperationMode
        {
            get { return _selectedOperationMode; }
            set
            {
                _selectedOperationMode = value;
                OnPropertyChanged();
                // 当选项改变时，你可以在这里执行操作
                if (_selectedOperationMode != null)
                {
                    // 你可以获取到选择的中文名和枚举值
                    Console.WriteLine($"选择了: {_selectedOperationMode.DisplayName}，枚举值为: {_selectedOperationMode.Value}");
                }
            }
        }
        private EnumBindingItem<SportType> _selectedSportType;

        // 绑定到ComboBox的SelectedItem属性
        public EnumBindingItem<SportType> SelectedSportType
        {
            get { return _selectedSportType; }
            set
            {
                _selectedSportType = value;
                OnPropertyChanged();
                // 当选项改变时，你可以在这里执行操作
                if (_selectedSportType != null)
                {
                    // 你可以获取到选择的中文名和枚举值
                    Console.WriteLine($"选择了: {_selectedSportType.DisplayName}，枚举值为: {_selectedSportType.Value}");

                    IsPointMotionVisible = _selectedSportType.Value == SportType.PointMotion;
                    IsArcMotionVisible = _selectedSportType.Value == SportType.ArcMotion;
                    IsJointMotionVisible = _selectedSportType.Value == SportType.JointMotion;
                    IsStraightLineMotionVisible = _selectedSportType.Value == SportType.StraightLineMotion;
                }
            }
        }

        private bool _isPointMotionVisible;
        public bool IsPointMotionVisible
        {
            get { return _isPointMotionVisible; }
            set { _isPointMotionVisible = value; OnPropertyChanged(); }
        }

        private bool _isArcMotionVisible;
        public bool IsArcMotionVisible
        {
            get { return _isArcMotionVisible; }
            set { _isArcMotionVisible = value; OnPropertyChanged(); }
        }

        private bool _isJointMotionVisible;
        public bool IsJointMotionVisible
        {
            get { return _isJointMotionVisible; }
            set { _isJointMotionVisible = value; OnPropertyChanged(); }
        }

        private bool _isStraightLineMotionVisible;
        public bool IsStraightLineMotionVisible
        {
            get { return _isStraightLineMotionVisible; }
            set { _isStraightLineMotionVisible = value; OnPropertyChanged(); }
        }

        // 在ViewModel中添加以下属性和初始化代码
        private ObservableCollection<EnumBindingItem<LinkageSettings>> _linkageSettingsModes;
        public ObservableCollection<EnumBindingItem<LinkageSettings>> LinkageSettingsModes
        {
            get { return _linkageSettingsModes; }
            set { _linkageSettingsModes = value; OnPropertyChanged(); }
        }


        private ObservableCollection<EnumBindingItem<FeedbackCommand>> _feedbackCommandModes;
        public ObservableCollection<EnumBindingItem<FeedbackCommand>> FeedbackCommandModes
        {
            get { return _feedbackCommandModes; }
            set { _feedbackCommandModes = value; OnPropertyChanged(); }
        }


        private ObservableCollection<EnumBindingItem<ControlInstruction>> _controlInstructionModes;
        public ObservableCollection<EnumBindingItem<ControlInstruction>> ControlInstructionModes
        {
            get { return _controlInstructionModes; }
            set { _controlInstructionModes = value; OnPropertyChanged(); }
        }

        private EnumBindingItem<ControlInstruction> _selectedControlInstructionMode;
        public EnumBindingItem<ControlInstruction> SelectedControlInstructionMode
        {
            get { return _selectedControlInstructionMode; }
            set
            {
                _selectedControlInstructionMode = value;
                OnPropertyChanged();
                if (_selectedControlInstructionMode != null)
                {
                    Console.WriteLine($"选择了: {_selectedControlInstructionMode.DisplayName}，枚举值为: {_selectedControlInstructionMode.Value}");
                }
            }
        }

        private ObservableCollection<EnumBindingItem<AddressOffset>> _addressOffsetModes;
        public ObservableCollection<EnumBindingItem<AddressOffset>> AddressOffsetModes
        {
            get { return _addressOffsetModes; }
            set { _addressOffsetModes = value; OnPropertyChanged(); }
        }

        private EnumBindingItem<AddressOffset> _selectedAddressOffsetMode;
        public EnumBindingItem<AddressOffset> SelectedAddressOffsetMode
        {
            get { return _selectedAddressOffsetMode; }
            set
            {
                _selectedAddressOffsetMode = value;
                OnPropertyChanged();
                if (_selectedAddressOffsetMode != null)
                {
                    Console.WriteLine($"选择了: {_selectedAddressOffsetMode.DisplayName}，枚举值为: {_selectedAddressOffsetMode.Value}");
                }
            }
        }


        private double _sharedJawValue;

        /// <summary>
        /// 夹爪间距
        /// </summary>
        public double SharedJawValue
        {
            get { return _sharedJawValue; }
            set
            {
                // 检查值是否真的改变了，避免不必要的更新
                if (_sharedJawValue != value)
                {
                    _sharedJawValue = value;
                    // 通知UI，这个属性的值已经变了！
                    OnPropertyChanged();
                }
            }
        }
        private double _sharedJawTorqueValue;

        /// <summary>
        /// 夹爪扭矩
        /// </summary>
        public double SharedJawTorqueValue
        {
            get { return _sharedJawTorqueValue; }
            set
            {
                // 检查值是否真的改变了，避免不必要的更新
                if (_sharedJawTorqueValue != value)
                {
                    _sharedJawTorqueValue = value;
                    // 通知UI，这个属性的值已经变了！
                    OnPropertyChanged();
                }
            }
        }
        // 我们可以把 Min/Max 值也放在这里，让UI更灵活
        public double MinValue { get; set; } = 0;
        public double MaxValue { get; set; } = 50;
        public MainViewModel()
        {

            //dmArmHelper dmArmHelper = new dmArmHelper();

            //dmArmHelper.testArm();

            SharedJawValue = 0;

            //Sports = new ObservableCollection<Sports>();
            OperationModes = new ObservableCollection<EnumBindingItem<OperationType>>();

            SportTypes = new ObservableCollection<EnumBindingItem<SportType>>();


            // 遍历OperationType枚举的所有值
            foreach (SportType opType in Enum.GetValues(typeof(SportType)))
            {
                // 创建新的绑定项，并添加到集合中
                SportTypes.Add(new EnumBindingItem<SportType>
                {
                    DisplayName = opType.GetDescription(), // 使用我们创建的扩展方法获取中文描述
                    Value = opType
                });
            }

            if (SportTypes.Count > 0)
            {

                SelectedSportType = SportTypes[0];
            }


            // 遍历OperationType枚举的所有值
            foreach (OperationType opType in Enum.GetValues(typeof(OperationType)))
            {
                // 创建新的绑定项，并添加到集合中
                OperationModes.Add(new EnumBindingItem<OperationType>
                {
                    DisplayName = opType.GetDescription(), // 使用我们创建的扩展方法获取中文描述
                    Value = opType
                });
            }

            if (OperationModes.Count > 0)
            {

                SelectedOperationMode = OperationModes[0];
            }

            // 初始化联动设置指令
            LinkageSettingsModes = new ObservableCollection<EnumBindingItem<LinkageSettings>>();
            foreach (LinkageSettings mode in Enum.GetValues(typeof(LinkageSettings)))
            {
                LinkageSettingsModes.Add(new EnumBindingItem<LinkageSettings>
                {
                    DisplayName = mode.GetDescription(),
                    Value = mode
                });
            }
            if (LinkageSettingsModes.Count > 0)
                SelectedLinkageSettingsMode = LinkageSettingsModes[0];

            // 初始化反馈指令偏移值
            FeedbackCommandModes = new ObservableCollection<EnumBindingItem<FeedbackCommand>>();
            foreach (FeedbackCommand mode in Enum.GetValues(typeof(FeedbackCommand)))
            {
                FeedbackCommandModes.Add(new EnumBindingItem<FeedbackCommand>
                {
                    DisplayName = mode.GetDescription(),
                    Value = mode
                });
            }
            if (FeedbackCommandModes.Count > 0)
                SelectedFeedbackCommandMode = FeedbackCommandModes[0];

            // 初始化控制执行偏移值
            ControlInstructionModes = new ObservableCollection<EnumBindingItem<ControlInstruction>>();
            foreach (ControlInstruction mode in Enum.GetValues(typeof(ControlInstruction)))
            {
                ControlInstructionModes.Add(new EnumBindingItem<ControlInstruction>
                {
                    DisplayName = mode.GetDescription(),
                    Value = mode
                });
            }
            if (ControlInstructionModes.Count > 0)
                SelectedControlInstructionMode = ControlInstructionModes[0];

            // 初始化地址偏移值
            AddressOffsetModes = new ObservableCollection<EnumBindingItem<AddressOffset>>();
            foreach (AddressOffset mode in Enum.GetValues(typeof(AddressOffset)))
            {
                AddressOffsetModes.Add(new EnumBindingItem<AddressOffset>
                {
                    DisplayName = mode.GetDescription(),
                    Value = mode
                });
            }
            if (AddressOffsetModes.Count > 0)
                SelectedAddressOffsetMode = AddressOffsetModes[0];


            // 初始化夹爪设置指令
            JawStatus = new ObservableCollection<EnumBindingItem<EnergyState>>();
            foreach (EnergyState mode in Enum.GetValues(typeof(EnergyState)))
            {
                JawStatus.Add(new EnumBindingItem<EnergyState>
                {
                    DisplayName = mode.GetDescription(),
                    Value = mode
                });
            }
            if (JawStatus.Count > 0)
                SelectedJawStatus = JawStatus[0];



            // 清除错误
            BeClearAboutMistakes = new ObservableCollection<EnumBindingItem<YesOrNo>>();
            foreach (YesOrNo mode in Enum.GetValues(typeof(YesOrNo)))
            {
                BeClearAboutMistakes.Add(new EnumBindingItem<YesOrNo>
                {
                    DisplayName = mode.GetDescription(),
                    Value = mode
                });
            }
            if (JawStatus.Count > 0)
                SelectedClearAbout = BeClearAboutMistakes[0];


            // 初始化夹爪设置指令零点
            SetZero = new ObservableCollection<EnumBindingItem<YesOrNo>>();
            foreach (YesOrNo mode in Enum.GetValues(typeof(YesOrNo)))
            {
                SetZero.Add(new EnumBindingItem<YesOrNo>
                {
                    DisplayName = mode.GetDescription(),
                    Value = mode
                });
            }
            if (SetZero.Count > 0)
                SelectedSetZero = SetZero[0];


            #region MainControl
            DisabledCommand = new RelayCommand(OnDisabled);

            ScramCommand = new RelayCommand(OnScram);
            #endregion

            // 实例化 RelayCommand，传入要执行的方法
            #region MainPanel1
            // 初始化所有命令
            XMinusCommand1 = new RelayCommand(OnXMinus1);
            YMinusCommand1 = new RelayCommand(OnYMinus1);
            ZMinusCommand1 = new RelayCommand(OnZMinus1);
            XPlusCommand1 = new RelayCommand(OnXPlus1);
            YPlusCommand1 = new RelayCommand(OnYPlus1);
            ZPlusCommand1 = new RelayCommand(OnZPlus1);

            XMinusCommand2 = new RelayCommand(OnXMinus2);
            YMinusCommand2 = new RelayCommand(OnYMinus2);
            ZMinusCommand2 = new RelayCommand(OnZMinus2);
            XPlusCommand2 = new RelayCommand(OnXPlus2);
            YPlusCommand2 = new RelayCommand(OnYPlus2);
            ZPlusCommand2 = new RelayCommand(OnZPlus2);

            ResetToZeroCommand = new RelayCommand(OnResetToZero);
            LoadCommand = new RelayCommand(OnLoad);
            SendCommand = new RelayCommand(OnSend);
            #endregion

            #region ArcMotionVisible
            // 初始化圆弧运动命令
            LoadInstructionPointCommand = new RelayCommand(OnLoadInstructionPoint);
            SendInstructionPointCommand = new RelayCommand(OnSendInstructionPoint);
            DrawArcCommand = new RelayCommand(OnDrawArc);

            // 初始化默认值
            InitializeDefaultValues();
            #endregion


            #region JointMotionVisible
            // 初始化关节增减命令
            Joint1DecreaseCommand = new RelayCommand(() => AdjustJointAngle(1, -StepIncrement));
            Joint1IncreaseCommand = new RelayCommand(() => AdjustJointAngle(1, StepIncrement));
            Joint2DecreaseCommand = new RelayCommand(() => AdjustJointAngle(2, -StepIncrement));
            Joint2IncreaseCommand = new RelayCommand(() => AdjustJointAngle(2, StepIncrement));
            Joint3DecreaseCommand = new RelayCommand(() => AdjustJointAngle(3, -StepIncrement));
            Joint3IncreaseCommand = new RelayCommand(() => AdjustJointAngle(3, StepIncrement));
            Joint4DecreaseCommand = new RelayCommand(() => AdjustJointAngle(4, -StepIncrement));
            Joint4IncreaseCommand = new RelayCommand(() => AdjustJointAngle(4, StepIncrement));
            Joint5DecreaseCommand = new RelayCommand(() => AdjustJointAngle(5, -StepIncrement));
            Joint5IncreaseCommand = new RelayCommand(() => AdjustJointAngle(5, StepIncrement));
            Joint6DecreaseCommand = new RelayCommand(() => AdjustJointAngle(6, -StepIncrement));
            Joint6IncreaseCommand = new RelayCommand(() => AdjustJointAngle(6, StepIncrement));

            // 初始化底部按钮命令
            JointResetToZeroCommand = new RelayCommand(OnJointResetToZero);
            JointEditCommand = new RelayCommand(OnJointEdit);
            #endregion


            #region StraightLineMotionVisible
            // 初始化坐标增减命令
            XDecreaseCommand = new RelayCommand(() => AdjustCoordinate("X", -PositionStepIncrement));
            XIncreaseCommand = new RelayCommand(() => AdjustCoordinate("X", PositionStepIncrement));
            YDecreaseCommand = new RelayCommand(() => AdjustCoordinate("Y", -PositionStepIncrement));
            YIncreaseCommand = new RelayCommand(() => AdjustCoordinate("Y", PositionStepIncrement));
            ZDecreaseCommand = new RelayCommand(() => AdjustCoordinate("Z", -PositionStepIncrement));
            ZIncreaseCommand = new RelayCommand(() => AdjustCoordinate("Z", PositionStepIncrement));

            RxDecreaseCommand = new RelayCommand(() => AdjustCoordinate("Rx", -RotationStepIncrement));
            RxIncreaseCommand = new RelayCommand(() => AdjustCoordinate("Rx", RotationStepIncrement));
            RyDecreaseCommand = new RelayCommand(() => AdjustCoordinate("Ry", -RotationStepIncrement));
            RyIncreaseCommand = new RelayCommand(() => AdjustCoordinate("Ry", RotationStepIncrement));
            RzDecreaseCommand = new RelayCommand(() => AdjustCoordinate("Rz", -RotationStepIncrement));
            RzIncreaseCommand = new RelayCommand(() => AdjustCoordinate("Rz", RotationStepIncrement));

            // 初始化底部按钮命令
            StraightLineResetToZeroCommand = new RelayCommand(OnStraightLineResetToZero);
            StraightLineEditCommand = new RelayCommand(OnStraightLineEdit);
            #endregion


            #region 联动控制
            LinkageControlCommand = new RelayCommand(OnLinkageControl);
            #endregion


            #region 夹爪控制
            JawControlCommand = new RelayCommand(OnJawageControl);

            #endregion
        }
        private void InitializeDefaultValues()
        {
            // 设置默认坐标值
            StartPointX = StartPointY = StartPointZ = 0;
            StartPointRx = StartPointRy = StartPointRz = 0;

            MidPointX = MidPointY = MidPointZ = 0;
            MidPointRx = MidPointRy = MidPointRz = 0;

            EndPointX = EndPointY = EndPointZ = 0;
            EndPointRx = EndPointRy = EndPointRz = 0;

            SelectedInstructionPointIndex = 1;
        }

        #region MainControl
        private void OnDisabled()
        {
            try
            {

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"直线运动回零失败: {ex.Message}");
            }
        }
        private void OnScram()
        {
            try
            {

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"直线运动回零失败: {ex.Message}");
            }
        }

        #endregion

        #region 
        // 第一个GroupBox的命令方法
        private void OnZMinus1()
        {
            // Z轴负方向逻辑
            ZValue -= StepZValue <= 0 ? 0 : StepZValue;
        }

        private void OnYMinus1()
        {
            // Y轴负方向逻辑
            YValue -= StepYValue<=0?0: StepYValue;
        }

        private void OnXPlus1()
        {
            // X轴正方向逻辑
            XValue += StepXValue <= 0 ? 0 : StepXValue;
        }

        private void OnZPlus1()
        {
            // Z轴正方向逻辑
            ZValue += StepZValue <= 0 ? 0 : StepZValue;
        }

        private void OnYPlus1()
        {
            // Y轴正方向逻辑
            YValue += StepYValue <= 0 ? 0 : StepYValue;
        }

        private void OnXMinus1()
        {
            // X轴负方向逻辑
            XValue -= StepXValue <= 0 ? 0 : StepXValue;
        }

        // 第二个GroupBox的命令方法
        private void OnZMinus2()
        {
            // Z轴负方向逻辑
            YawValue -= StepYawValue <= 0 ? 0 : StepYawValue;
        }

        private void OnYMinus2()
        {
            // Y轴负方向逻辑
            Pitchvalue -= StepPitchvalue <= 0 ? 0 : StepPitchvalue;
        }

        private void OnXPlus2()
        {
            // X轴正方向逻辑
            Rollvalue += StepRollvalue <= 0 ? 0 : StepRollvalue;
        }

        private void OnZPlus2()
        {
            // Z轴正方向逻辑
            YawValue += StepYawValue <= 0 ? 0 : StepYawValue;
        }

        private void OnYPlus2()
        {
            // Y轴正方向逻辑
            Pitchvalue += StepPitchvalue <= 0 ? 0 : StepPitchvalue;
        }

        private void OnXMinus2()
        {
            // X轴负方向逻辑
            Rollvalue -= StepRollvalue <= 0 ? 0 : StepRollvalue;
        }

        // 底部按钮命令方法
        private void OnResetToZero()
        {
            // 回零逻辑
            Rollvalue=0; YawValue=0; Pitchvalue=0;  
            XValue=0;YawValue=0;ZValue=0;

        }

        private void OnLoad()
        {
            // 装载逻辑
        }

        private void OnSend()
        {
            // 发送逻辑
        }
        #endregion


        #region
        // 装载指令点命令方法
        private void OnLoadInstructionPoint()
        {
            // 实现装载指令点逻辑
            // 例如：从文件或数据库加载坐标数据
            try
            {
                // 根据选择的指令点序号加载对应的坐标数据
                LoadCoordinateData(SelectedInstructionPointIndex);
            }
            catch (Exception ex)
            {
                // 处理异常
                System.Diagnostics.Debug.WriteLine($"装载指令点失败: {ex.Message}");
            }
        }

        // 发送指令点命令方法
        private void OnSendInstructionPoint()
        {
            // 实现发送指令点逻辑
            // 例如：将当前坐标数据发送到设备
            try
            {
                var instructionData = new ArcInstructionData
                {
                    StartPoint = new Point6D(StartPointX, StartPointY, StartPointZ, StartPointRx, StartPointRy, StartPointRz),
                    MidPoint = new Point6D(MidPointX, MidPointY, MidPointZ, MidPointRx, MidPointRy, MidPointRz),
                    EndPoint = new Point6D(EndPointX, EndPointY, EndPointZ, EndPointRx, EndPointRy, EndPointRz),
                    InstructionPointIndex = SelectedInstructionPointIndex
                };

                // 发送数据到设备
                SendDataToDevice(instructionData);
            }
            catch (Exception ex)
            {
                // 处理异常
                System.Diagnostics.Debug.WriteLine($"发送指令点失败: {ex.Message}");
            }
        }

        // 画弧命令方法
        private void OnDrawArc()
        {
            // 实现画弧逻辑
            try
            {
                // 验证数据
                if (!ValidateArcData())
                {
                    // 显示错误信息
                    return;
                }

                // 执行画弧操作
                ExecuteArcMotion();
            }
            catch (Exception ex)
            {
                // 处理异常
                System.Diagnostics.Debug.WriteLine($"画弧操作失败: {ex.Message}");
            }
        }

        private bool ValidateArcData()
        {
            // 验证坐标数据是否有效
            // 这里可以添加具体的验证逻辑
            return StartPointX >= 0 && StartPointX <= 50 &&
                   StartPointY >= 0 && StartPointY <= 50 &&
                   StartPointZ >= 0 && StartPointZ <= 50 &&
                   // 其他坐标的验证...
                   true;
        }

        private void LoadCoordinateData(int pointIndex)
        {
            // 实现从数据源加载坐标数据的逻辑
            // 这里需要根据您的具体需求实现
        }

        private void SendDataToDevice(ArcInstructionData data)
        {
            // 实现发送数据到设备的逻辑
            // 这里需要根据您的具体设备通信协议实现
        }

        private void ExecuteArcMotion()
        {
            // 实现执行圆弧运动的逻辑
            // 这里需要根据您的具体运动控制需求实现
        }



        #endregion


        #region JointMotionVisible
        // 调整关节角度
        private void AdjustJointAngle(int jointNumber, double increment)
        {
            try
            {
                string currentValue;

                switch (jointNumber)
                {
                    case 1:
                        currentValue = Joint1Angle;
                        break;
                    case 2:
                        currentValue = Joint2Angle;
                        break;
                    case 3:
                        currentValue = Joint3Angle;
                        break;
                    case 4:
                        currentValue = Joint4Angle;
                        break;
                    case 5:
                        currentValue = Joint5Angle;
                        break;
                    case 6:
                        currentValue = Joint6Angle;
                        break;
                    default:
                        currentValue = "0.000";
                        break;
                }

                // 变量 currentValue 现在包含了所需的值

                if (double.TryParse(currentValue, out double value))
                {
                    double newValue = value + increment;
                    string formattedValue = newValue.ToString("F3", CultureInfo.InvariantCulture);

                    // 更新对应的关节角度
                    switch (jointNumber)
                    {
                        case 1:
                            Joint1Angle = formattedValue;
                            break;
                        case 2:
                            Joint2Angle = formattedValue;
                            break;
                        case 3:
                            Joint3Angle = formattedValue;
                            break;
                        case 4:
                            Joint4Angle = formattedValue;
                            break;
                        case 5:
                            Joint5Angle = formattedValue;
                            break;
                        case 6:
                            Joint6Angle = formattedValue;
                            break;
                    }

                    // 触发关节角度变化事件
                    OnJointAngleChanged(jointNumber, newValue);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"调整关节{jointNumber}角度失败: {ex.Message}");
            }
        }

        // 关节回零命令方法
        private void OnJointResetToZero()
        {
            try
            {
                // 将所有关节角度重置为零
                Joint1Angle = "0.000";
                Joint2Angle = "0.000";
                Joint3Angle = "0.000";
                Joint4Angle = "0.000";
                Joint5Angle = "0.000";
                Joint6Angle = "0.000";

                // 执行回零操作
                ExecuteJointResetToZero();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"关节回零失败: {ex.Message}");
            }
        }

        // 关节编辑命令方法
        private void OnJointEdit()
        {
            try
            {
                // 验证关节角度数据
                if (!ValidateJointAngles())
                {
                    // 显示错误信息
                    return;
                }

                // 执行编辑操作
                ExecuteJointEdit();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"关节编辑失败: {ex.Message}");
            }
        }

        // 验证关节角度数据
        private bool ValidateJointAngles()
        {
            try
            {
                // 检查所有关节角度是否为有效的数字
                return double.TryParse(Joint1Angle, out _) &&
                       double.TryParse(Joint2Angle, out _) &&
                       double.TryParse(Joint3Angle, out _) &&
                       double.TryParse(Joint4Angle, out _) &&
                       double.TryParse(Joint5Angle, out _) &&
                       double.TryParse(Joint6Angle, out _);
            }
            catch
            {
                return false;
            }
        }

        // 获取所有关节角度
        public double[] GetAllJointAngles()
        {
            try
            {
                return new double[]
                {
                double.Parse(Joint1Angle),
                double.Parse(Joint2Angle),
                double.Parse(Joint3Angle),
                double.Parse(Joint4Angle),
                double.Parse(Joint5Angle),
                double.Parse(Joint6Angle)
                };
            }
            catch
            {
                return new double[] { 0, 0, 0, 0, 0, 0 };
            }
        }

        // 设置所有关节角度
        public void SetAllJointAngles(double[] angles)
        {
            if (angles.Length == 6)
            {
                Joint1Angle = angles[0].ToString("F3");
                Joint2Angle = angles[1].ToString("F3");
                Joint3Angle = angles[2].ToString("F3");
                Joint4Angle = angles[3].ToString("F3");
                Joint5Angle = angles[4].ToString("F3");
                Joint6Angle = angles[5].ToString("F3");
            }
        }

        // 以下方法需要根据您的具体业务逻辑实现

        private void OnJointAngleChanged(int jointNumber, double newValue)
        {
            // 当关节角度变化时执行的操作
            // 例如：更新机器人状态、发送数据到设备等
            System.Diagnostics.Debug.WriteLine($"关节{jointNumber}角度变为: {newValue}");
        }

        private void ExecuteJointResetToZero()
        {
            // 执行关节回零的具体逻辑
            // 例如：发送回零指令到机器人控制器
        }

        private void ExecuteJointEdit()
        {
            // 执行关节编辑的具体逻辑
            // 例如：打开编辑对话框、保存配置等
        }
        #endregion

        #region StraightLineMotionVisible

        // 调整坐标值
        private void AdjustCoordinate(string coordinateType, double increment)
        {
            try
            {
                string currentValue;

                switch (coordinateType)
                {
                    case "X":
                        currentValue = StraightLineX;
                        break;
                    case "Y":
                        currentValue = StraightLineY;
                        break;
                    case "Z":
                        currentValue = StraightLineZ;
                        break;
                    case "Rx":
                        currentValue = StraightLineRx;
                        break;
                    case "Ry":
                        currentValue = StraightLineRy;
                        break;
                    case "Rz":
                        currentValue = StraightLineRz;
                        break;
                    default:
                        currentValue = "0.000";
                        break;
                }
                // 变量 currentValue 现在已赋值

                if (double.TryParse(currentValue, out double value))
                {
                    double newValue = value + increment;
                    string formattedValue = newValue.ToString("F3", CultureInfo.InvariantCulture);

                    // 更新对应的坐标值
                    switch (coordinateType)
                    {
                        case "X":
                            StraightLineX = formattedValue;
                            break;
                        case "Y":
                            StraightLineY = formattedValue;
                            break;
                        case "Z":
                            StraightLineZ = formattedValue;
                            break;
                        case "Rx":
                            StraightLineRx = formattedValue;
                            break;
                        case "Ry":
                            StraightLineRy = formattedValue;
                            break;
                        case "Rz":
                            StraightLineRz = formattedValue;
                            break;
                    }

                    // 触发坐标变化事件
                    OnCoordinateChanged(coordinateType, newValue);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"调整{coordinateType}坐标失败: {ex.Message}");
            }
        }

        // 直线运动回零命令方法
        private void OnStraightLineResetToZero()
        {
            try
            {
                // 将所有坐标重置为零
                StraightLineX = "0.000";
                StraightLineY = "0.000";
                StraightLineZ = "0.000";
                StraightLineRx = "0.000";
                StraightLineRy = "0.000";
                StraightLineRz = "0.000";

                // 执行回零操作
                ExecuteStraightLineResetToZero();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"直线运动回零失败: {ex.Message}");
            }
        }

        // 直线运动编辑命令方法
        private void OnStraightLineEdit()
        {
            try
            {
                // 验证坐标数据
                if (!ValidateStraightLineCoordinates())
                {
                    // 显示错误信息
                    return;
                }

                // 执行编辑操作
                ExecuteStraightLineEdit();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"直线运动编辑失败: {ex.Message}");
            }
        }

        // 验证直线运动坐标数据
        private bool ValidateStraightLineCoordinates()
        {
            try
            {
                // 检查所有坐标是否为有效的数字
                return double.TryParse(StraightLineX, out _) &&
                       double.TryParse(StraightLineY, out _) &&
                       double.TryParse(StraightLineZ, out _) &&
                       double.TryParse(StraightLineRx, out _) &&
                       double.TryParse(StraightLineRy, out _) &&
                       double.TryParse(StraightLineRz, out _);
            }
            catch
            {
                return false;
            }
        }

        // 获取所有直线运动坐标
        public StraightLineCoordinates GetAllStraightLineCoordinates()
        {
            try
            {
                return new StraightLineCoordinates
                {
                    X = double.Parse(StraightLineX),
                    Y = double.Parse(StraightLineY),
                    Z = double.Parse(StraightLineZ),
                    Rx = double.Parse(StraightLineRx),
                    Ry = double.Parse(StraightLineRy),
                    Rz = double.Parse(StraightLineRz)
                };
            }
            catch
            {
                return new StraightLineCoordinates();
            }
        }

        // 设置所有直线运动坐标
        public void SetAllStraightLineCoordinates(StraightLineCoordinates coordinates)
        {
            StraightLineX = coordinates.X.ToString("F3");
            StraightLineY = coordinates.Y.ToString("F3");
            StraightLineZ = coordinates.Z.ToString("F3");
            StraightLineRx = coordinates.Rx.ToString("F3");
            StraightLineRy = coordinates.Ry.ToString("F3");
            StraightLineRz = coordinates.Rz.ToString("F3");
        }

        // 以下方法需要根据您的具体业务逻辑实现

        private void OnCoordinateChanged(string coordinateType, double newValue)
        {
            // 当坐标变化时执行的操作
            // 例如：更新机器人状态、发送数据到设备等
            System.Diagnostics.Debug.WriteLine($"{coordinateType}坐标变为: {newValue}");

            // 可以在这里添加直线运动轨迹计算
            CalculateStraightLineTrajectory();
        }

        private void CalculateStraightLineTrajectory()
        {
            // 计算直线运动轨迹
            // 这里可以实现直线插补算法
            try
            {
                var coordinates = GetAllStraightLineCoordinates();
                // 执行轨迹计算逻辑
                // ...
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"轨迹计算失败: {ex.Message}");
            }
        }

        private void ExecuteStraightLineResetToZero()
        {
            // 执行直线运动回零的具体逻辑
            // 例如：发送回零指令到机器人控制器
        }

        private void ExecuteStraightLineEdit()
        {
            // 执行直线运动编辑的具体逻辑
            // 例如：打开编辑对话框、保存配置、发送运动指令等

            // 获取当前坐标
            var coordinates = GetAllStraightLineCoordinates();

            // 发送直线运动指令
            SendStraightLineMotionCommand(coordinates);
        }

        private void SendStraightLineMotionCommand(StraightLineCoordinates coordinates)
        {
            // 发送直线运动指令到设备
            // 这里需要根据您的具体设备通信协议实现
        }

        #region 联动控制
        private void OnLinkageControl()
        {
            try
            {

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"联动控制失败: {ex.Message}");
            }
        }
        #endregion

        #region 夹爪控制
        private void OnJawageControl()
        {
            try
            {

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"联动控制失败: {ex.Message}");
            }
        }
        #endregion
    }

    // 直线运动坐标数据结构
    public class StraightLineCoordinates
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double Rx { get; set; }
        public double Ry { get; set; }
        public double Rz { get; set; }

        public StraightLineCoordinates()
        {
            X = Y = Z = Rx = Ry = Rz = 0.0;
        }

        public StraightLineCoordinates(double x, double y, double z, double rx, double ry, double rz)
        {
            X = x;
            Y = y;
            Z = z;
            Rx = rx;
            Ry = ry;
            Rz = rz;
        }

        public override string ToString()
        {
            return $"X:{X:F3}, Y:{Y:F3}, Z:{Z:F3}, Rx:{Rx:F3}, Ry:{Ry:F3}, Rz:{Rz:F3}";
        }
        #endregion


    }
    // 辅助类 - 6维坐标点
    public class Point6D
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double Rx { get; set; }
        public double Ry { get; set; }
        public double Rz { get; set; }

        public Point6D(double x, double y, double z, double rx, double ry, double rz)
        {
            X = x;
            Y = y;
            Z = z;
            Rx = rx;
            Ry = ry;
            Rz = rz;
        }
    }

    // 圆弧指令数据结构
    public class ArcInstructionData
    {
        public Point6D StartPoint { get; set; }
        public Point6D MidPoint { get; set; }
        public Point6D EndPoint { get; set; }
        public int InstructionPointIndex { get; set; }
    }
}
