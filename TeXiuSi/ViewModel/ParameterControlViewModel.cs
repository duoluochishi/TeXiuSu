using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeXiuSi.ViewModel
{
    public partial class ParameterControlViewModel : ObservableObject
    {

        private bool _IsEditing = false;

        public bool IsEditing
        {
            get => _IsEditing;
            set => SetProperty(ref _IsEditing, value);

        }

        #region Joint setting

        private double _maximumAngle = 0.0; // 初始值
        public double MaximumAngle
        {
            get => _maximumAngle;
            set => SetProperty(ref _maximumAngle, value);
        }

        private double _minimumAngle = 0; // 初始值
        public double MinimumAngle
        {
            get => _minimumAngle;
            set => SetProperty(ref _minimumAngle, value);
        }
        private double _minimumSpeed = 0; // 初始值
        public double MinimumSpeed
        {
            get => _minimumSpeed;
            set => SetProperty(ref _minimumSpeed, value);
        }
        private double _minimumAcceleration = 0; // 初始值
        public double MinimumAcceleration
        {
            get => _minimumAcceleration;
            set => SetProperty(ref _minimumAcceleration, value);
        }


        private bool _jointIsEditing = true;

        public bool JointIsEditing
        {
            get => _jointIsEditing;
            set => SetProperty(ref _jointIsEditing, value);

        }

        public IRelayCommand JointSettingEditorCommand { get; }

        public IRelayCommand CancelCommand { get; }

        public IRelayCommand Initializationommand { get; }

        public IRelayCommand SaveCommand { get; }


        #endregion

        #region Terminal setting

        private bool _terminalIsEditing = true;

        public bool TerminalIsEditing
        {
            get => _terminalIsEditing;
            set => SetProperty(ref _terminalIsEditing, value);

        }

        // 新生成的属性 (1. 最大线速度)
        private double _maximumLinearVelocity = 0;
        public double MaximumLinearVelocity
        {
            get => _maximumLinearVelocity;
            set => SetProperty(ref _maximumLinearVelocity, value);
        }

        // 新生成的属性 (2. 最大角速度)
        private double _maximumAngularVelocity = 0;
        public double MaximumAngularVelocity
        {
            get => _maximumAngularVelocity;
            set => SetProperty(ref _maximumAngularVelocity, value);
        }

        // 新生成的属性 (3. 最大线加速度)
        private double _maximumLinearAcceleration = 0;
        public double MaximumLinearAcceleration
        {
            get => _maximumLinearAcceleration;
            set => SetProperty(ref _maximumLinearAcceleration, value);
        }

        // 新生成的属性 (4. 最大角加速度)
        private double _maximumAngularAcceleration = 0;
        public double MaximumAngularAcceleration
        {
            get => _maximumAngularAcceleration;
            set => SetProperty(ref _maximumAngularAcceleration, value);
        }
        public IRelayCommand TerminalSettingConfirmCommand { get; }

        public IRelayCommand TerminalSettingEditorCommand { get; }


        public IRelayCommand TerminalCancelCommand { get; }

        public IRelayCommand TerminalInitializationommand { get; }

        public IRelayCommand TerminalSaveCommand { get; }
        #endregion

        #region Collision grade


        private bool _collisionIsEditing = true;

        public bool CollisionIsEditing
        {
            get => _collisionIsEditing;
            set => SetProperty(ref _collisionIsEditing, value);

        }
        private double _impactLevel1 = 0; // 初始值
        public double ImpactLevel1
        {
            get => _impactLevel1;
            set => SetProperty(ref _impactLevel1, value);
        }
        private double _impactLevel2 = 0; // 初始值
        public double ImpactLevel2
        {
            get => _impactLevel2;
            set => SetProperty(ref _impactLevel2, value);
        }
        private double _impactLevel3 = 0; // 初始值
        public double ImpactLevel3
        {
            get => _impactLevel3;
            set => SetProperty(ref _impactLevel3, value);
        }
        private double _impactLevel4 = 0; // 初始值
        public double ImpactLevel4
        {
            get => _impactLevel4;
            set => SetProperty(ref _impactLevel4, value);
        }
        private double _impactLevel5 = 0; // 初始值
        public double ImpactLevel5
        {
            get => _impactLevel5;
            set => SetProperty(ref _impactLevel5, value);
        }
        private double _impactLevel6 = 0; // 初始值
        public double ImpactLevel6
        {
            get => _impactLevel6;
            set => SetProperty(ref _impactLevel6, value);
        }
        public IRelayCommand CollisionGradeEditorCommand { get; }

        public IRelayCommand CollisionCancelCommand { get; }


        public IRelayCommand CollisionSaveCommand { get; }
        #endregion


        #region Joint information




        #endregion


        public ParameterControlViewModel()
        {
            #region Joint setting
            JointSettingEditorCommand = new RelayCommand(OnJointSettingEditor);

            CancelCommand = new RelayCommand(CancleJoint);

            Initializationommand = new RelayCommand(InitializationJoint);

            SaveCommand = new RelayCommand(SaveJoint);


            #endregion

            #region Terminal setting
            TerminalSettingConfirmCommand = new RelayCommand(OnTerminalSettingConfirm);

            TerminalSettingEditorCommand = new RelayCommand(OnTerminalSettingEditor);


            TerminalCancelCommand = new RelayCommand(TerminalCancleJoint);

            TerminalInitializationommand = new RelayCommand(TerminalInitializationJoint);

            TerminalSaveCommand = new RelayCommand(TerminalSaveJoint);

            #endregion

            #region Collision grade

            CollisionGradeEditorCommand = new RelayCommand(OnCollisionGradeEditor);
            #endregion

            #region Joint information

            #endregion
        }
        #region Joint setting
        private void OnJointSettingEditor()
        {
            try
            {
                JointIsEditing = false;

                IsEditing = true;
            }
            catch (Exception ex)
            {

            }
        }

        private void CancleJoint()
        {
            try
            {
                //当前界面visual切换
                JointIsEditing = true;
                //tab封锁 


                IsEditing=false;
            }
            catch (Exception ex)
            {

            }
        }
        private void InitializationJoint()
        {
            try
            {
                //当前界面visual切换
                JointIsEditing = true;
                //tab封锁 

                IsEditing = false;

            }
            catch (Exception ex)
            {

            }
        }
        private void SaveJoint()
        {
            try
            {
                //当前界面visual切换
                JointIsEditing = true;
                //tab封锁 

                IsEditing = false;

            }
            catch (Exception ex)
            {

            }
        }
        #endregion


        #region Terminal setting

        private void OnTerminalSettingConfirm()
        {
            try
            {
                TerminalIsEditing = false;
                IsEditing = true;
            }
            catch (Exception ex)
            {

            }
        }
        private void OnTerminalSettingEditor()
        {
            try
            {

            }
            catch (Exception ex)
            {

            }
        }

        private void TerminalCancleJoint()
        {
            try
            {
                //当前界面visual切换
                TerminalIsEditing = true;
                //tab封锁 
                IsEditing = false;


            }
            catch (Exception ex)
            {

            }
        }
        private void TerminalInitializationJoint()
        {
            try
            {
                //当前界面visual切换
                TerminalIsEditing = true;
                //tab封锁 

                IsEditing = false;

            }
            catch (Exception ex)
            {

            }
        }
        private void TerminalSaveJoint()
        {
            try
            {
                //当前界面visual切换
                TerminalIsEditing = true;
                //tab封锁 
                IsEditing = false;


            }
            catch (Exception ex)
            {

            }
        }
        #endregion

        #region Joint information

        #endregion

        private void OnCollisionGradeEditor()
        {
            try
            {

            }
            catch (Exception ex)
            {

            }
        }

        //公共enable

    }
}
