using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace TeXiuSi.Convert
{
    public class PercentageConverter : IValueConverter
    {
        // 从 ViewModel 的 double 转换为显示的 string (例如: 100.0 -> "100 %")
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double d)
            {
                // 直接格式化为字符串，并添加百分号
                return $"{d:F0} %"; // F0 表示不带小数位的格式
            }
            return value; // 返回原始值以防出错
        }

        // 从显示的 string (例如: "105 %") 转换回 ViewModel 的 double (例如: 105.0)
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string s)
            {
                // 移除百分号和空格
                string numericString = s.Replace(" %", "").Trim();

                if (double.TryParse(numericString, out double result))
                {
                    return result;
                }
            }
            // 转换失败时，返回 Binding.DoNothing 或原值
            return Binding.DoNothing;
        }
    }
}
