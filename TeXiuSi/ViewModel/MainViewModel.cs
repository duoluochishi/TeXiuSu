using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeXiuSi.Helper;
using TeXiuSi.Model;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace TeXiuSi.ViewModel
{
    public class MainViewModel : ObservableObject
    {
        //[ObservableProperty]
        //public ObservableCollection<Sports> Sports { get; set; }


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

        private ObservableCollection<EnumBindingItem<FeedbackCommand>> _feedbackCommandModes;
        public ObservableCollection<EnumBindingItem<FeedbackCommand>> FeedbackCommandModes
        {
            get { return _feedbackCommandModes; }
            set { _feedbackCommandModes = value; OnPropertyChanged(); }
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

            dmArmHelper dmArmHelper = new dmArmHelper();

            dmArmHelper.testArm();

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

            if (OperationModes.Count > 0) {

                SelectedOperationMode=OperationModes[0];
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
        }
    }
}
