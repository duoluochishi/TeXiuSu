using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MathNet.Numerics.RootFinding;
using Peak.Can.Basic.BackwardCompatibility;
using Serilog;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using TeXiuSi.Model;
using TeXiuSi.PCAN;
using static TeXiuSi.ViewModel.MainViewModel;

namespace TeXiuSi.ViewModel
{
    public partial class ConnectViewModel : ObservableObject
    {

        //private PANHelper _pANHelper;


        private ObservableCollection<ConnectNodeModel> _observableCollectionSimple;
        public ObservableCollection<ConnectNodeModel> observableCollectionSimple
        {
            get { return _observableCollectionSimple; }
            set { _observableCollectionSimple = value; OnPropertyChanged(nameof(observableCollectionSimple)); }
        }
        // 2. 用于绑定 ComboBox 选中项的属性
        private ConnectNodeModel _selectedNode;
        public ConnectNodeModel SelectedNode
        {
            get { return _selectedNode; }
            set { _selectedNode = value; OnPropertyChanged(nameof(SelectedNode)); }
        }


        public ObservableCollection<EnumBindingItem<TPCANBaudrate>> TPCANBaudrates { get; set; }

        private EnumBindingItem<TPCANBaudrate> _selectedTPCANBaudrate;

        // 绑定到ComboBox的SelectedItem属性
        public EnumBindingItem<TPCANBaudrate> SelectedTPCANBaudrate
        {
            get { return _selectedTPCANBaudrate; }
            set
            {
                _selectedTPCANBaudrate = value;
                OnPropertyChanged();
                // 当选项改变时，你可以在这里执行操作
                //if (_selectedSportType != null)
                //{
                //    // 你可以获取到选择的中文名和枚举值
                //    Console.WriteLine($"选择了: {_selectedSportType.DisplayName}，枚举值为: {_selectedSportType.Value}");
                //}
            }
        }

        public ObservableCollection<EnumBindingItem<TPCANType>> TPCANTypes { get; set; }

        private EnumBindingItem<TPCANType> _selectedTPCANType;

        // 绑定到ComboBox的SelectedItem属性
        public EnumBindingItem<TPCANType> SelectedTPCANType
        {
            get { return _selectedTPCANType; }
            set
            {
                _selectedTPCANType = value;
                OnPropertyChanged();
                // 当选项改变时，你可以在这里执行操作
                //if (_selectedSportType != null)
                //{
                //    // 你可以获取到选择的中文名和枚举值
                //    Console.WriteLine($"选择了: {_selectedSportType.DisplayName}，枚举值为: {_selectedSportType.Value}");
                //}
            }
        }
        private ObservableCollection<IOModel> _observableCollectionIOModel;
        public ObservableCollection<IOModel> observableCollectionIOModel
        {
            get { return _observableCollectionIOModel; }
            set { _observableCollectionIOModel = value; OnPropertyChanged(nameof(_observableCollectionIOModel)); }
        }
        // 2. 用于绑定 ComboBox 选中项的属性
        private IOModel _selectedIOModel;
        public IOModel SelectedIOModel
        {
            get { return _selectedIOModel; }
            set { _selectedIOModel = value; OnPropertyChanged(nameof(_selectedIOModel)); }
        }

        public ObservableCollection<InterruptModel> _observableCollectionInterruptModel;
        public ObservableCollection<InterruptModel> observableCollectionInterruptModel
        {
            get { return _observableCollectionInterruptModel; }
            set { _observableCollectionInterruptModel = value; OnPropertyChanged(nameof(_observableCollectionInterruptModel)); }
        }
        // 2. 用于绑定 ComboBox 选中项的属性
        public InterruptModel _selectedInterruptModel;
        public InterruptModel SelectedInterruptModel
        {
            get { return _selectedInterruptModel; }
            set { _selectedInterruptModel = value; OnPropertyChanged(nameof(_selectedInterruptModel)); }
        }
        public IRelayCommand ConnectCommand { get; }

        public ConnectViewModel()
        {
            //初始化
            _observableCollectionSimple = new ObservableCollection<ConnectNodeModel>();
            DeviceOperation.Instance._pANHelper = new PANHelper();
            _observableCollectionSimple =
            new ObservableCollection<ConnectNodeModel>(
                DeviceOperation.Instance._pANHelper.SetCanList().OrderBy(node => node.Name) // 假设         ConnectNodeModel 有一个 Name 属性
            );

            observableCollectionIOModel = new ObservableCollection<IOModel> {

                 new IOModel() { Type = 0, Name = "0100", Value = 0x0100 },
                 new IOModel() { Type = 0, Name = "0120", Value = 0x0120 },
                 new IOModel() { Type = 0, Name = "0140", Value = 0x0140 },
                 new IOModel() { Type = 0, Name = "0200", Value = 0x0200 },
                 new IOModel() { Type = 0, Name = "0220", Value = 0x0220 },
                 new IOModel() { Type = 0, Name = "0240", Value = 0x0240 },
                 new IOModel() { Type = 0, Name = "0260", Value = 0x0260 },
                 new IOModel() { Type = 0, Name = "0278", Value = 0x0278 },
                 new IOModel() { Type = 0, Name = "0280", Value = 0x0280 },
                 new IOModel() { Type = 0, Name = "02A0", Value = 0x02A0 },
                 new IOModel() { Type = 0, Name = "02C0", Value = 0x02C0 },
                 new IOModel() { Type = 0, Name = "02E0", Value = 0x02E0 },
                 new IOModel() { Type = 0, Name = "02E8", Value = 0x02E8 },
                 new IOModel() { Type = 0, Name = "02F8", Value = 0x02F8 },
                 new IOModel() { Type = 0, Name = "0300", Value = 0x0300 },
                 new IOModel() { Type = 0, Name = "0320", Value = 0x0320 },
                 new IOModel() { Type = 0, Name = "0340", Value = 0x0340 },
                 new IOModel() { Type = 0, Name = "0360", Value = 0x0360 },
                 new IOModel() { Type = 0, Name = "0378", Value = 0x0378 },
                 new IOModel() { Type = 0, Name = "0380", Value = 0x0380 },
                 new IOModel() { Type = 0, Name = "03BC", Value = 0x03BC },
                 new IOModel() { Type = 0, Name = "03E0", Value = 0x03E0 },
                 new IOModel() { Type = 0, Name = "03E8", Value = 0x03E8 },
                 new IOModel() { Type = 0, Name = "03F8", Value = 0x03F8 }

            };

            // 1. 定义新的十进制数值，已转换为 UInt16 类型
            ushort value3 = System.Convert.ToUInt16(3);
            ushort value4 = System.Convert.ToUInt16(4);
            ushort value5 = System.Convert.ToUInt16(5);
            ushort value7 = System.Convert.ToUInt16(7);
            ushort value9 = System.Convert.ToUInt16(9);
            ushort value10 = System.Convert.ToUInt16(10);
            ushort value11 = System.Convert.ToUInt16(11);
            ushort value12 = System.Convert.ToUInt16(12);
            ushort value15 = System.Convert.ToUInt16(15);
            observableCollectionInterruptModel = new ObservableCollection<InterruptModel> {

                 new InterruptModel() { Type = 0, Name = "0100", Value = value3 },
                 new InterruptModel() { Type = 0, Name = "0120", Value = value4 },
                 new InterruptModel() { Type = 0, Name = "0140", Value = value5 },
                 new InterruptModel() { Type = 0, Name = "0200", Value = value7 },
                 new InterruptModel() { Type = 0, Name = "0220", Value = value9 },
                 new InterruptModel() { Type = 0, Name = "0240", Value = value10 },
                 new InterruptModel() { Type = 0, Name = "0260", Value = value11 },
                 new InterruptModel() { Type = 0, Name = "0278", Value = value12 },
                 new InterruptModel() { Type = 0, Name = "0280", Value = value15 },
            };
            SelectedTPCANBaudrate = TPCANBaudrates[0];
            SelectedTPCANType = TPCANTypes[0];
            SelectedIOModel = observableCollectionIOModel[0];
            SelectedInterruptModel = observableCollectionInterruptModel[0];
            ConnectCommand = new RelayCommand(Connect);
        }

        [RelayCommand]
        private void Connect()
        {
            DeviceOperation.Instance.Connect(SelectedIOModel.Value, SelectedInterruptModel.Value);
        }
    }
}
