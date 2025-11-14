using MathNet.Spatial.Euclidean;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeXiuSi
{
    public class ArcGeometryCalculator
    {
        // 定义一个结构体来存储圆弧的几何信息
        public struct ArcParameters
        {
            public Point3D Center;      // 圆心坐标 (X, Y, Z)
            public double Radius;       // 半径 R
            public UnitVector3D Normal; // 所在平面法向量 N
            public bool IsCollinear;    // 三点是否共线
        }

        /// <summary>
        /// 根据三个三维点计算圆弧的几何参数。
        /// </summary>
        /// <param name="pStart">起点 (Xs, Ys, Zs)</param>
        /// <param name="pMid">中点 (Xm, Ym, Zm)</param>
        /// <param name="pEnd">终点 (Xe, Ye, Ze)</param>
        /// <returns>包含圆心、半径和法向量的 ArcParameters 结构体。</returns>
        public static ArcParameters CalculateArcCenter(
            (double X, double Y, double Z) pStart,
            (double X, double Y, double Z) pMid,
            (double X, double Y, double Z) pEnd)
        {
            // 1. 将自定义元组转换为 Math.NET Spatial 的 Point3D 对象
            var P1 = new Point3D(pStart.X, pStart.Y, pStart.Z);
            var P2 = new Point3D(pMid.X, pMid.Y, pMid.Z);
            var P3 = new Point3D(pEnd.X, pEnd.Y, pEnd.Z);

            // 检查三点是否共线或重合
            if (P1.Equals(P2) || P2.Equals(P3) || P1.Equals(P3))
            {
                Console.WriteLine("警告：点重合，无法形成圆弧。");
                return new ArcParameters { IsCollinear = true };
            }

            try
            {
                // 2. 使用 Math.NET Spatial 的 FromPoints 静态方法，直接通过三点创建 Circle3D 对象
                // 这个方法自动完成了复杂的几何计算（求中垂线交点）
                var circle = Circle3D.FromPoints(P1, P2, P3);

                // 3. 提取结果
                return new ArcParameters
                {
                    Center = circle.CenterPoint,
                    Radius = circle.Radius,
                    Normal = circle.Axis, // Axis 就是圆弧所在平面的法向量
                    IsCollinear = false
                };
            }
            catch (ArgumentException ex)
            {
                // 如果三点共线，FromPoints 方法会抛出异常
                Console.WriteLine($"几何计算失败：{ex.Message}");
                Console.WriteLine("可能是三点共线，应执行直线插补。");
                return new ArcParameters { IsCollinear = true };
            }
        }
    }
}
