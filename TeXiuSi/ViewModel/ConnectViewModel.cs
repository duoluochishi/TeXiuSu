using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TeXiuSi.Model;

namespace TeXiuSi.ViewModel
{
    public class ConnectViewModel : INotifyPropertyChanged
    {

        private ObservableCollection<ConnectNodeModel> _connectNodes;
        public ObservableCollection<ConnectNodeModel> ConnectNodes
        {
            get { return _connectNodes; }
            set { _connectNodes = value; OnPropertyChanged("ConnectNodes"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler CanExecuteChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public RelayCommand ConnectCommand { get; set; }


        public ConnectViewModel()
        {



            // 在ViewModel的构造函数中，完成对命令的设置
            ConnectCommand = new RelayCommand(Connnectiong);
            //ConnectCommand.canExecuteFunc = new Func<object, bool>(this.CanDoSomething);//命令执行先进行判断,这里和MyCommand中的CanExecute都可以进行判断。
            //ConnectCommand.executeActions = new Action<object>(this.DoSomething);//执行命令,带一个参数
        }
        public void DoSomething(object param)
        {
            //执行方法，返回界面的命令带的参数
        }

        public void Connnectiong()
        {
            //执行方法，返回界面的命令带的参数
        }
        public bool CanDoSomething(object param)
        {
            //这里可以和界面进行数据交互，进行判断。判断能否做这个事情，大部分时候返回true就行了，返回false,方法就不执行了。
            return true;
        }
    }
}
