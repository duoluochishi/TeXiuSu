using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace TeXiuSi.Convert
{
    public class BooleanToColorConverter : IValueConverter
    {
        // 当bool为true时，返回的颜色/画刷
        public Brush TrueBrush { get; set; } = Brushes.Green; // 默认绿色

        // 当bool为false时，返回的颜色/画刷
        public Brush FalseBrush { get; set; } = Brushes.Red;  // 默认红色

        // 将 bool 转换为 Brush
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isTrue)
            {
                // 如果目标类型是 Brush 或其基类
                if (targetType.IsAssignableFrom(typeof(Brush)))
                {
                    return isTrue ? TrueBrush : FalseBrush;
                }
            }
            // 否则返回未修改的值或一个默认值
            return value;
        }

        // 不实现从 Brush 转换回 bool 的逻辑
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
