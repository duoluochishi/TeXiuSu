using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeXiuSi.Helper;
using TeXiuSi.Model;
using static TeXiuSi.ViewModel.MainViewModel;

namespace TeXiuSi.ViewModel
{
    public partial class MotorConfigurationViewModel : ObservableObject
    {

        private ObservableCollection<Joint> _connectNodeModels;
        public ObservableCollection<Joint> ConnectNodeModels
        {
            get { return _connectNodeModels; }
            set { _connectNodeModels = value; OnPropertyChanged(nameof(_connectNodeModels)); }
        }

        private Joint _selectedNode;
        public Joint SelectedNode
        {
            get { return _selectedNode; }
            set { _selectedNode = value; OnPropertyChanged(nameof(SelectedNode)); }
        }

        private ControlPowModel _selectedControlPowM;
        public ControlPowModel SelectedControlPowM
        {
            get { return _selectedControlPowM; }
            set { _selectedControlPowM = value; OnPropertyChanged(nameof(_selectedControlPowM)); }
        }


        public ObservableCollection<EnumBindingItem<ControlPowModel>> ControlPowModels { get; set; }


        private EnumBindingItem<ControlPowModel> _selectedControlPowModel;

        // 绑定到ComboBox的SelectedItem属性
        public EnumBindingItem<ControlPowModel> SelectedControlPowModel
        {
            get { return _selectedControlPowModel; }
            set
            {
                _selectedControlPowModel = value;
                OnPropertyChanged();

            }
        }

        public IRelayCommand MotorSettingEditorCommand { get; }

        public IRelayCommand ZeroEditorCommand { get; }


        public IRelayCommand EliminateErrorsEditorCommand { get; }



        public MotorConfigurationViewModel()
        {


            var SportTypes = new ObservableCollection<EnumBindingItem<JointNode>>();

           
            foreach (JointNode opType in Enum.GetValues(typeof(JointNode)))
            {
                // 创建新的绑定项，并添加到集合中
                ConnectNodeModels.Add(new Joint
                {
                    Name = opType.GetDescription(), // 使用我们创建的扩展方法获取中文描述
                    Type = 0,
                    JointInfo = opType
                });
            }


            ControlPowModels = new ObservableCollection<EnumBindingItem<ControlPowModel>>();

            foreach (ControlPowModel opType in Enum.GetValues(typeof(ControlPowModel)))
            {
                // 创建新的绑定项，并添加到集合中
                ControlPowModels.Add(new EnumBindingItem<ControlPowModel>
                {
                    DisplayName = opType.GetDescription(), // 使用我们创建的扩展方法获取中文描述
                    Value = opType
                });
            }

            if (ControlPowModels.Count > 0)
            {

                SelectedControlPowModel = ControlPowModels[0];
            }

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
