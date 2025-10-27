using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeXiuSi.ViewModel
{
    public partial class MotorConfigurationViewModel : ObservableObject
    {

        public IRelayCommand MotorSettingEditorCommand { get; }

        public IRelayCommand ZeroEditorCommand { get; }


        public IRelayCommand EliminateErrorsEditorCommand { get; }

        

        public MotorConfigurationViewModel() {

            MotorSettingEditorCommand = new RelayCommand(OnMotorSettingEditor);

            ZeroEditorCommand = new RelayCommand(OnZeroEditor);

            EliminateErrorsEditorCommand = new RelayCommand(OnEliminateErrorsEditor);
        }

        private void OnMotorSettingEditor()
        {
            try
            {

            }
            catch (Exception ex)
            {

            }
        }

        private void OnZeroEditor()
        {
            try
            {

            }
            catch (Exception ex)
            {

            }
        }

        private void OnEliminateErrorsEditor()
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
