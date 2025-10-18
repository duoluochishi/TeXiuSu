using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TeXiuSi.uc
{
    /// <summary>
    /// ParameterControl.xaml 的交互逻辑
    /// </summary>
    public partial class ParameterControl : UserControl
    {
        public ParameterControl()
        {
            InitializeComponent();
        }

        private void btnsportInfo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 实例化 Window
                JointMotionParameters motorConfiguration = new JointMotionParameters();

                // 使用 ShowDialog() 模态显示
                motorConfiguration.ShowDialog();
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private void btnConditionInfo_Click(object sender, RoutedEventArgs e)
        {
            JointParameterWindow jointParameterWindow = new JointParameterWindow();
            jointParameterWindow.ShowDialog();
        }
    }
}
