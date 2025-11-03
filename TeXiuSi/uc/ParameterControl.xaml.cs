using System.Windows.Controls;
using TeXiuSi.ViewModel;

namespace TeXiuSi.uc
{
    /// <summary>
    /// Interaction logic for ParameterControl.xaml
    /// </summary>
    public partial class ParameterControl : UserControl
    {
        public ParameterControl()
        {
            InitializeComponent();
            DataContext = new ParameterControlViewModel();
        }
    }
}
