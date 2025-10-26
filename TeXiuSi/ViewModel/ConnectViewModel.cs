using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using TeXiuSi.Model;

namespace TeXiuSi.ViewModel
{
    public partial class ConnectViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<ConnectNodeModel> _connectNodes;

        public ConnectViewModel()
        {
            _connectNodes = new ObservableCollection<ConnectNodeModel>();
        }

        [RelayCommand]
        private void Connect()
        {
            // Connnectiong
        }
    }
}
