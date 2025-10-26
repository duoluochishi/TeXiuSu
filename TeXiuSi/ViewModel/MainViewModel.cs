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
        [ObservableProperty]
        private ObservableCollection<EnumBindingItem<OperationType>> _operationModes;

        [ObservableProperty]
        private ObservableCollection<EnumBindingItem<SportType>> _sportTypes;

        [ObservableProperty]
        private EnumBindingItem<OperationType> _selectedOperationMode;

        [ObservableProperty]
        private EnumBindingItem<SportType> _selectedSportType;

        [ObservableProperty]
        private bool _isPointMotionVisible;

        [ObservableProperty]
        private bool _isArcMotionVisible;

        [ObservableProperty]
        private bool _isJointMotionVisible;

        [ObservableProperty]
        private bool _isStraightLineMotionVisible;

        [ObservableProperty]
        private ObservableCollection<EnumBindingItem<LinkageSettings>> _linkageSettingsModes;

        [ObservableProperty]
        private EnumBindingItem<LinkageSettings> _selectedLinkageSettingsMode;

        [ObservableProperty]
        private ObservableCollection<EnumBindingItem<FeedbackCommand>> _feedbackCommandModes;

        [ObservableProperty]
        private EnumBindingItem<FeedbackCommand> _selectedFeedbackCommandMode;

        [ObservableProperty]
        private ObservableCollection<EnumBindingItem<ControlInstruction>> _controlInstructionModes;

        [ObservableProperty]
        private EnumBindingItem<ControlInstruction> _selectedControlInstructionMode;

        [ObservableProperty]
        private ObservableCollection<EnumBindingItem<AddressOffset>> _addressOffsetModes;

        [ObservableProperty]
        private EnumBindingItem<AddressOffset> _selectedAddressOffsetMode;

        [ObservableProperty]
        private double _sharedJawValue;

        [ObservableProperty]
        private double _sharedJawTorqueValue;

        public double MinValue { get; set; } = 0;
        public double MaxValue { get; set; } = 50;

        public MainViewModel()
        {
            dmArmHelper dmArmHelper = new dmArmHelper();
            dmArmHelper.testArm();

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

        partial void OnSelectedSportTypeChanged(EnumBindingItem<SportType> value)
        {
            if (value != null)
            {
                Console.WriteLine($"选择了: {value.DisplayName}，枚举值为: {value.Value}");
                IsPointMotionVisible = value.Value == SportType.PointMotion;
                IsArcMotionVisible = value.Value == SportType.ArcMotion;
                IsJointMotionVisible = value.Value == SportType.JointMotion;
                IsStraightLineMotionVisible = value.Value == SportType.StraightLineMotion;
            }
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
