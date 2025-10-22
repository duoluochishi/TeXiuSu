using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TeXiuSi.Behaviors
{
    public class DecimalInputBehavior : Behavior<TextBox>
    {
        // 附加属性：用于设置允许的小数位数，此处默认为 3
        public static readonly DependencyProperty DecimalPlacesProperty =
            DependencyProperty.Register(
                nameof(DecimalPlaces), typeof(int), typeof(DecimalInputBehavior),
                new PropertyMetadata(3));

        public int DecimalPlaces
        {
            get => (int)GetValue(DecimalPlacesProperty);
            set => SetValue(DecimalPlacesProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PreviewTextInput += OnPreviewTextInput;
            AssociatedObject.TextChanged += OnTextChanged;
            // 允许输入负数，可以根据需要添加 PreviewKeyDown 检查 '-' 键
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.PreviewTextInput -= OnPreviewTextInput;
            AssociatedObject.TextChanged -= OnTextChanged;
        }

        private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // 允许数字、小数点和控制字符（如退格、Enter）
            if (!char.IsDigit(e.Text, 0) && e.Text != "." && e.Text != "-")
            {
                e.Handled = true; // 阻止非法字符输入
            }

            // 阻止输入多个小数点
            if (e.Text == "." && AssociatedObject.Text.Contains("."))
            {
                e.Handled = true;
            }
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = AssociatedObject;
            string currentText = textBox.Text;

            // 构建正则表达式来检查小数点后的位数
            // 例如，如果 DecimalPlaces 为 3，则 pattern 是 ^-?\d*(\.\d{0,3})?$
            string pattern = $@"^-?\d*(\.\d{{0,{DecimalPlaces}}})?$";

            if (!Regex.IsMatch(currentText, pattern))
            {
                // 如果当前文本不符合规则，则恢复到上一次合法的值
                // 找到符合规则的最长前缀
                Match match = Regex.Match(currentText, pattern);

                if (match.Success && match.Value.Length < currentText.Length)
                {
                    // 仅在发现非法输入时修正
                    string correctedText = currentText.Substring(0, currentText.Length - 1);

                    // 阻止 TextChanged 再次触发
                    textBox.TextChanged -= OnTextChanged;
                    textBox.Text = correctedText;
                    textBox.TextChanged += OnTextChanged;

                    // 保持光标在正确位置（如果可能，但此处简化为末尾）
                    textBox.CaretIndex = correctedText.Length;
                }
            }
        }
    }
}
