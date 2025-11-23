using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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


        private ObservableCollection<Joint> _ZeroModels;
        public ObservableCollection<Joint> ZeroNodeModels
        {
            get { return _ZeroModels; }
            set { _ZeroModels = value; OnPropertyChanged(nameof(_ZeroModels)); }
        }

        private Joint _selectedZeroNode;
        public Joint SelectedZeroNode
        {
            get { return _selectedZeroNode; }
            set { _selectedZeroNode = value; OnPropertyChanged(nameof(_selectedZeroNode)); }
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


            ConnectNodeModels = new ObservableCollection<Joint>();


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
            if (ConnectNodeModels.Count > 0)
            {

                SelectedNode = ConnectNodeModels[0];
            }

            foreach (JointNode opType in Enum.GetValues(typeof(JointNode)))
            {
                // 创建新的绑定项，并添加到集合中
                ZeroNodeModels.Add(new Joint
                {
                    Name = opType.GetDescription(), // 使用我们创建的扩展方法获取中文描述
                    Type = 0,
                    JointInfo = opType
                });
            }
            if (ZeroNodeModels.Count > 0)
            {

                SelectedZeroNode = ZeroNodeModels[0];
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
                //如果是失能给提示
                if (true)
                {
                    // 1. 获取实际的枚举值
                    ControlPowModel selectedModel = this.SelectedControlPowModel.Value;
                    switch (selectedModel)
                    {
                        case ControlPowModel.PositionSpd:
                            break;
                        case ControlPowModel.Spd:
                            string message = "机械臂失能会直接落下，请确保机械臂已处于安全位置？";
                            string title = "操作确认";

                            // 2. 显示带有 Yes 和 No 按钮的提示框
                            MessageBoxResult result = MessageBox.Show(
                                message,
                                title,
                                MessageBoxButton.YesNo, // 指定显示 Yes 和 No 按钮
                                MessageBoxImage.Question // 指定显示问号图标
                            );

                            // 3. 根据用户的选择进行判断
                            if (result == MessageBoxResult.Yes)
                            {
                                // 用户点击了“是” (Yes)
                                Console.WriteLine("用户选择了保存并退出。");
                                // 执行保存和退出逻辑...
                            }
                            else if (result == MessageBoxResult.No)
                            {
                                return;
                            }
                            break;
                        default:
                            break;
                    }

                }

            }
            catch (Exception ex)
            {

            }
        }

        public void ConnectControl()
        {
            ControlPowModel selectedModel = this.SelectedControlPowModel.Value;
            if (SelectedNode.JointInfo == JointNode.NodelAll)
            {
                DeviceOperation.Instance.ControlConnectOfJoints(selectedModel);

            }
            else
            {
                DeviceOperation.Instance.ControlConnectOfJoints(selectedModel, SelectedNode.JointInfo);

            }
        }

        private void OnZeroEditor()
        {
            try
            {
                DeviceOperation.Instance.ClearTheErrorOfJoints(ControlPowModelOther.SaveZero);
            }
            catch (Exception ex)
            {
                Log.Error($"零点设置失败{ex.Message}");
            }
        }

        private void OnEliminateErrorsEditor()
        {
            try
            {
                DeviceOperation.Instance.ClearTheErrorOfJoints(ControlPowModelOther.ClearError);
            }
            catch (Exception ex)
            {
                Log.Error($"清除错误失败{ex.Message}");
            }
        }
    }
}
