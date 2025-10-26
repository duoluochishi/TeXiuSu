using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using TeXiuSi.Helper;
using TeXiuSi.Model;

namespace TeXiuSi.ViewModel
{
    // 通用的枚举绑定项类（如果尚未定义）
    public class EnumBindingItem<T> where T : Enum
    {
        public string DisplayName { get; set; }
        public T Value { get; set; }
    }

    public partial class MainViewModel : ObservableObject
    {
        public ObservableCollection<EnumBindingItem<OperationType>> OperationModes { get; set; }
        public ObservableCollection<EnumBindingItem<SportType>> SportTypes { get; set; }

        private EnumBindingItem<OperationType> _selectedOperationMode;
        public EnumBindingItem<OperationType> SelectedOperationMode
        {
            get { return _selectedOperationMode; }
            set
            {
                _selectedOperationMode = value;
                OnPropertyChanged();
                if (_selectedOperationMode != null)
                {
                    Console.WriteLine($"选择了: {_selectedOperationMode.DisplayName}，枚举值为: {_selectedOperationMode.Value}");
                }
            }
        }

        private EnumBindingItem<SportType> _selectedSportType;
        public EnumBindingItem<SportType> SelectedSportType
        {
            get { return _selectedSportType; }
            set
            {
                _selectedSportType = value;
                OnPropertyChanged();
                if (_selectedSportType != null)
                {
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
        public double SharedJawValue
        {
            get { return _sharedJawValue; }
            set
            {
                if (_sharedJawValue != value)
                {
                    _sharedJawValue = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _sharedJawTorqueValue;
        public double SharedJawTorqueValue
        {
            get { return _sharedJawTorqueValue; }
            set
            {
                if (_sharedJawTorqueValue != value)
                {
                    _sharedJawTorqueValue = value;
                    OnPropertyChanged();
                }
            }
        }

        public double MinValue { get; set; } = 0;
        public double MaxValue { get; set; } = 50;

        public MainViewModel()
        {
            //dmArmHelper dmArmHelper = new dmArmHelper();
            //dmArmHelper.testArm();

            SharedJawValue = 0;

            OperationModes = new ObservableCollection<EnumBindingItem<OperationType>>();
            foreach (OperationType opType in Enum.GetValues(typeof(OperationType)))
            {
                OperationModes.Add(new EnumBindingItem<OperationType>
                {
                    DisplayName = opType.GetDescription(),
                    Value = opType
                });
            }
            if (OperationModes.Count > 0)
            {
                SelectedOperationMode = OperationModes[0];
            }

            SportTypes = new ObservableCollection<EnumBindingItem<SportType>>();
            foreach (SportType opType in Enum.GetValues(typeof(SportType)))
            {
                SportTypes.Add(new EnumBindingItem<SportType>
                {
                    DisplayName = opType.GetDescription(),
                    Value = opType
                });
            }
            if (SportTypes.Count > 0)
            {
                SelectedSportType = SportTypes[0];
            }

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

        #region Window Control Commands
        [RelayCommand]
        private void MinimizeWindow() { }

        [RelayCommand]
        private void MaximizeWindow() { }

        [RelayCommand]
        private void CloseWindow() { }
        #endregion

        #region Main Control Commands
        [RelayCommand]
        private void ConnectArm() { }

        [RelayCommand]
        private void DisableArm() { }

        [RelayCommand]
        private void EmergencyStop() { }
        #endregion

        #region Point Motion Commands
        [RelayCommand]
        private void PointMotion(string direction) { }

        [RelayCommand]
        private void ArmPose(string direction) { }

        [RelayCommand]
        private void Home() { }

        [RelayCommand]
        private void Load() { }

        [RelayCommand]
        private void Send() { }
        #endregion

        #region Arc Motion Commands
        [RelayCommand]
        private void LoadArcMotion() { }

        [RelayCommand]
        private void SendArcMotion() { }

        [RelayCommand]
        private void DrawArc() { }
        #endregion

        #region Joint Motion Commands
        [RelayCommand]
        private void AdjustJointDegree(string parameter) { }

        [RelayCommand]
        private void HomeJoint() { }

        [RelayCommand]
        private void EditJoint() { }
        #endregion

        #region Linear Motion Commands
        [RelayCommand]
        private void AdjustLinearMotion(string parameter) { }

        [RelayCommand]
        private void HomeLinear() { }

        [RelayCommand]
        private void EditLinear() { }
        #endregion

        #region Linkage Control Commands
        [RelayCommand]
        private void ConfirmLinkageSettings() { }
        #endregion

        #region Claw Control Commands
        [RelayCommand]
        private void DisableClaw() { }

        [RelayCommand]
        private void ClearClawError() { }

        [RelayCommand]
        private void SetClawZeroPoint() { }
        #endregion

        #region Other Commands
        [RelayCommand]
        private void OpenTrajectoryLibrary() { }

        [RelayCommand]
        private void ShowJointStatus() { }

        [RelayCommand]
        private void UpgradeVersion() { }

        [RelayCommand]
        private void ResetView() { }

        [RelayCommand]
        private void OpenSettings() { }
        #endregion

        #region Version Upgrade Commands
        [RelayCommand]
        private void ReturnToMain() { }

        [RelayCommand]
        private void LoadPackage() { }

        [RelayCommand]
        private void StartUpgrade() { }
        #endregion

        #region Motor Configuration Commands
        [RelayCommand]
        private void ConfirmMotorStatus() { }

        [RelayCommand]
        private void ConfirmZeroPoint() { }

        [RelayCommand]
        private void ConfirmInstallation() { }

        [RelayCommand]
        private void ClearMotorError() { }
        #endregion

        #region Parameter Control Commands
        [RelayCommand]
        private void EditJointLimitParameters() { }

        [RelayCommand]
        private void ConfirmLoadValue() { }

        [RelayCommand]
        private void EditCollisionProtectionLevel() { }

        [RelayCommand]
        private void ViewMotionInformation() { }

        [RelayCommand]
        private void ViewStatusInformation() { }
        #endregion

        #region Trajectory Library Commands
        [RelayCommand]
        private void ImportFile() { }

        [RelayCommand]
        private void NewTrajectory() { }

        [RelayCommand]
        private void DeleteTrajectory() { }

        [RelayCommand]
        private void SendTrajectory() { }

        [RelayCommand]
        private void ExportFile() { }
        #endregion
    }
}
