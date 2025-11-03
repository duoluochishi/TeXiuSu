using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace TeXiuSi.Convert
{
    public class BooleanNegationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 接收布尔值，返回它的反值
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return value; // 非布尔值原样返回，但通常不会发生
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 反向转换通常不需要实现，除非进行双向绑定
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return value;
        }
    }
}
