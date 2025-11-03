using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace TeXiuSi.Convert
{
    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 期望 value 是一个 bool
            if (value is bool isVisible)
            {
                // 如果 isVisible 为 true，则返回 Collapsed (隐藏)
                // 如果 isVisible 为 false，则返回 Visible (显示)
                return isVisible ? Visibility.Collapsed : Visibility.Visible;
            }

            // 默认返回 Collapsed，以防意外
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // ConvertBack 通常不需要实现，除非您支持双向绑定
            throw new NotSupportedException();
        }
    }
}
