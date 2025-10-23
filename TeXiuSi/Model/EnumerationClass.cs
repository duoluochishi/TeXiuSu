using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TeXiuSi.Model
{
    public class EnumerationClass
    {
        public EnumerationClass()
        {

        }



    }

    /// <summary>
    /// 运动方式
    /// </summary>
    public enum SportType
    {
        /// <summary>
        /// 点位运动
        /// </summary>
        [Description("点位运动")]
        PointMotion = 0,
        /// <summary>
        /// 圆弧运动
        /// </summary>
        [Description("圆弧运动")]
        ArcMotion = 1,
        /// <summary>
        /// 关节运动
        /// </summary>
        [Description("关节运动")]
        JointMotion = 2,
        /// <summary>
        /// 直线运动
        /// </summary>
        [Description("直线运动")]
        StraightLineMotion = 3,
    }
    /// <summary>
    /// 操作模式
    /// </summary>
    public enum OperationType
    {
        /// <summary>
        /// CAN模式
        /// </summary>
        [Description("CAN模式")]
        CANModel = 0,
        /// <summary>
        /// 待机模式
        /// </summary>
        [Description("待机模式")]
        StandbyModel = 1,
    }

    /// <summary>
    /// 联动设置指令
    /// </summary>

    public enum LinkageSettings
    {
        [Description("主臂")]
        MasterArm = 0x00,

        [Description("从臂")]
        SlaveArm = 0x01
    }

    /// <summary>
    /// 反馈指令偏移值
    /// </summary>
    public enum FeedbackCommand
    {
        [Description("0x00")]
        Maisheng = 0x00,

        [Description("0x01")]
        Option1 = 0x01,

        [Description("0x02")]
        Option2 = 0x02
    }

    /// <summary>
    /// 控制执行偏移值
    /// </summary>
    public enum ControlInstruction
    {
        [Description("0x00")]
        Maisheng = 0x00,

        [Description("0x01")]
        Option1 = 0x01,

        [Description("0x02")]
        Option2 = 0x02
    }

    /// <summary>
    /// 地址偏移值
    /// </summary>
    public enum AddressOffset
    {
        [Description("0x00")]
        Maisheng = 0x00,


        [Description("0x01")]
        Option1 = 0x01,

        [Description("0x02")]
        Option2 = 0x02
    }

    /// <summary>
    /// 寄存器ID枚举的占位符。
    /// </summary>
    public enum RID
    {
        cmode = 0x33 // 示例代码中的控制模式寄存器地址
    }
}
