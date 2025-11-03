using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace TeXiuSi.ViewModel
{
    public partial class ParameterControlViewModel : ObservableObject
    {
        public ParameterControlViewModel()
        {

        }

        // Tab: 关节设置 (Joint Settings)
        [ObservableProperty]
        private int _selectedJointIndex;

        [ObservableProperty]
        private decimal _maxAngleValue;

        [ObservableProperty]
        private decimal _minAngleValue;

        [ObservableProperty]
        private decimal _maxVelocityValue;

        [ObservableProperty]
        private decimal _maxAccelerationValue;

        [RelayCommand]
        private void JointSettingEditor()
        {
            // Method implementation will be added later
        }

        // Tab: 末端设置 (End Effector Settings)
        [ObservableProperty]
        private int _selectedLoadIndex;

        [ObservableProperty]
        private decimal _maxLinearVelocityValue;

        [ObservableProperty]
        private decimal _maxAngularVelocityValue;

        [ObservableProperty]
        private decimal _maxLinearAccelerationValue;

        [ObservableProperty]
        private decimal _maxAngularAccelerationValue;

        [RelayCommand]
        private void TerminalSettingConfirm()
        {
            // Method implementation will be added later
        }

        [RelayCommand]
        private void TerminalSettingEditor()
        {
            // Method implementation will be added later
        }

        // Tab: 碰撞等级 (Collision Level)
        [ObservableProperty]
        private double _collisionLevel1;

        [ObservableProperty]
        private double _collisionLevel2;

        [ObservableProperty]
        private double _collisionLevel3;

        [ObservableProperty]
        private double _collisionLevel4;

        [ObservableProperty]
        private double _collisionLevel5;

        [ObservableProperty]
        private double _collisionLevel6;

        [RelayCommand]
        private void CollisionGradeEditor()
        {
            // Method implementation will be added later
        }

        // Tab: 关节信息 (Joint Information)
        [RelayCommand]
        private void ViewMotionInformation()
        {
            // Method implementation will be added later
        }

        [RelayCommand]
        private void ViewStatusInformation()
        {
            // Method implementation will be added later
        }
    }
}
