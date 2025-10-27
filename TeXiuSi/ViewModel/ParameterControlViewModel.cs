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


        #region Joint setting
        public IRelayCommand JointSettingEditorCommand { get; }
        #endregion

        #region Terminal setting
        public IRelayCommand TerminalSettingConfirmCommand { get; }

        public IRelayCommand TerminalSettingEditorCommand { get; }
        #endregion

        #region Collision grade

        #endregion

        public IRelayCommand CollisionGradeEditorCommand { get; }
        #region Joint information

        #endregion


        public ParameterControlViewModel()
        {
            #region Joint setting
            JointSettingEditorCommand = new RelayCommand(OnJointSettingEditor);
            #endregion

            #region Terminal setting
            TerminalSettingConfirmCommand = new RelayCommand(OnTerminalSettingConfirm);
            TerminalSettingEditorCommand = new RelayCommand(OnTerminalSettingEditor);

            #endregion

            #region Collision grade

            CollisionGradeEditorCommand = new RelayCommand(OnCollisionGradeEditor);
            #endregion

            #region Joint information

            #endregion
        }
        private void OnJointSettingEditor()
        {
            try
            {

            }
            catch (Exception ex)
            {
                
            }
        }

        private void OnTerminalSettingConfirm()
        {
            try
            {

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
        private void OnCollisionGradeEditor()
        {
            try
            {

            }
            catch (Exception ex)
            {
           
            }
        }

    }
}
