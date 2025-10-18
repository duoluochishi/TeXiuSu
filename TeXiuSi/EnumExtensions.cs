using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TeXiuSi
{
    public static class EnumExtensions
    {
        /// <summary>
        /// 获取枚举值的Description特性描述
        /// </summary>
        /// <param name="value">枚举值</param>
        /// <returns>Description特性的内容，如果不存在则返回枚举名称</returns>
        public static string GetDescription(this Enum value)
        {
            // 获取枚举的类型
            Type type = value.GetType();

            // 获取枚举值的名称
            string name = Enum.GetName(type, value);
            if (name == null)
            {
                return null;
            }

            // 获取字段信息
            FieldInfo field = type.GetField(name);
            if (field == null)
            {
                return name; // 如果没有字段，返回名称
            }

            // 获取Description特性
            DescriptionAttribute attr = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;

            // 返回Description属性，如果不存在，则返回枚举成员的名称
            return attr == null ? name : attr.Description;
        }
    }
}
