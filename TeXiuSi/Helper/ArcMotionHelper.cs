using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using static TeXiuSi.ViewModel.MainViewModel;

namespace TeXiuSi.Helper
{
    public static class ArcMotionHelper
    {

        // 辅助结构体，用于高精度计算
        public struct Vector3d
        {
            public double X, Y, Z;

            // 修正: 添加采用 3 个 double 参数的构造函数
            public Vector3d(double x, double y, double z) { X = x; Y = y; Z = z; }

            // 从 float Vector3 转换的构造函数
            public Vector3d(Vector3 v) { X = v.X; Y = v.Y; Z = v.Z; }

            // 运算符重载 (简化几何运算)
            public static Vector3d operator -(Vector3d v1, Vector3d v2)
            {
                return new Vector3d(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);
            }
            public static Vector3d operator *(double s, Vector3d v)
            {
                return new Vector3d(v.X * s, v.Y * s, v.Z * s);
            }
            public static Vector3d operator /(Vector3d v, double s)
            {
                return new Vector3d(v.X / s, v.Y / s, v.Z / s);
            }

            // 静态方法：叉乘
            public static Vector3d Cross(Vector3d v1, Vector3d v2)
            {
                return new Vector3d(
                    v1.Y * v2.Z - v1.Z * v2.Y,
                    v1.Z * v2.X - v1.X * v2.Z,
                    v1.X * v2.Y - v1.Y * v2.X
                );
            }

            // 静态方法：点积
            public static double Dot(Vector3d v1, Vector3d v2)
            {
                return v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z;
            }

            // 静态方法：长度平方
            public static double LengthSquared(Vector3d v)
            {
                return v.X * v.X + v.Y * v.Y + v.Z * v.Z;
            }

            // 转换为 System.Numerics.Vector3
            public Vector3 ToVector3() => new Vector3((float)X, (float)Y, (float)Z);
        }
        /// <summary>
        /// 【第 1 步】根据 P1, P2, P3 计算圆弧的几何参数（圆心、半径、法向量和基向量）。
        /// </summary>
        /// <summary>
        /// 根据空间中的三个点 P1 (起点), P2 (辅助点), P3 (终点) 
        /// 计算圆弧的几何参数（圆心、半径、法向量和基向量）及起止角度。
        /// </summary>
        // 定义一个浮点容差，用于判断数值是否接近零。

        private const double Tolerance = 1e-8; // 提高容差精度

        public static ArcParameters CalculateArcParameters(Vector3 P1, Vector3 P2, Vector3 P3, out double startAngle, out double endAngle)
        {
            // 1. 计算向量 V12, V23, N (使用 float/Vector3 进行规范化)
            Vector3 V12 = P2 - P1;
            Vector3 V23 = P3 - P2;

            if (Vector3.Cross(V12, V23).LengthSquared() < Tolerance)
            {
                throw new InvalidOperationException("三点共线，圆弧退化为直线。");
            }

            Vector3 N_unnormalized = Vector3.Cross(V12, V23);
            Vector3 N = Vector3.Normalize(N_unnormalized);

            // ==========================================================
            // 2. 核心修正：使用 double 进行圆心计算
            // ==========================================================
            Vector3d P1d = new Vector3d(P1);
            Vector3d P2d = new Vector3d(P2);
            Vector3d N_d = new Vector3d(N);

            Vector3d V12d = new Vector3d(V12); // P2d - P1d
            Vector3d V23d = new Vector3d(V23); // P3d - P2d

            double L1d = P2.LengthSquared() - P1.LengthSquared();
            double L2d = P3.LengthSquared() - P2.LengthSquared();

            Vector3d T1d = Vector3d.Cross(V23d, N_d);
            Vector3d T2d = Vector3d.Cross(V12d, N_d);

            double Denomd = 4.0 * Vector3d.Dot(V12d, T1d);

            if (Math.Abs(Denomd) < Tolerance)
            {
                throw new InvalidOperationException("圆心计算分母接近零。");
            }

            // 计算 Centerd
            Vector3d Centerd = new Vector3d(
                (L1d * T1d.X - L2d * T2d.X) / Denomd,
                (L1d * T1d.Y - L2d * T2d.Y) / Denomd,
                (L1d * T1d.Z - L2d * T2d.Z) / Denomd
            );
            Vector3 Center = Centerd.ToVector3(); // 转换回 float/Vector3

            // 3. 计算半径 R 和基向量 E1, E2
            float Radius = (Center - P1).Length();
            if (Radius < Tolerance)
            {
                throw new InvalidOperationException("计算半径过小。");
            }

            Vector3 R_start = P1 - Center;
            Vector3 E1 = Vector3.Normalize(R_start);
            Vector3 E2 = Vector3.Normalize(Vector3.Cross(N, E1));

            // 4. 角度计算：使用高精度的 R_end 向量和容差
            startAngle = 0.0;

            // 重新计算 R_end, R_aux 以避免 Center 转换的误差
            Vector3 R_end = P3 - Center;

            // U轴投影
            double end_u_component = Vector3.Dot(R_end, E1);
            // V轴投影
            double end_v_component = Vector3.Dot(R_end, E2);

            // **强制修正：解决 0.9113 的根源**
            if (Math.Abs(end_u_component) < Radius * Tolerance)
            {
                end_u_component = 0.0; // 强制设为 0
            }

            endAngle = Math.Atan2(end_v_component, end_u_component);

            // 5. 角度方向修正 (使用 P2 检查方向，逻辑与上一个版本相同)
            Vector3 R_aux = P2 - Center;
            double aux_u_component = Vector3.Dot(R_aux, E1);
            double aux_v_component = Vector3.Dot(R_aux, E2);

            if (Math.Abs(aux_u_component) < Radius * Tolerance) aux_u_component = 0.0;
            if (Math.Abs(aux_v_component) < Radius * Tolerance) aux_v_component = 0.0;

            double auxAngle = Math.Atan2(aux_v_component, aux_u_component);

            // 简化方向检查：如果 EndAngle 和 AuxAngle 跨度接近 180 度，则调整为长弧
            double angleDifference = endAngle - auxAngle;
            if (angleDifference > Math.PI) angleDifference -= 2 * Math.PI;
            if (angleDifference < -Math.PI) angleDifference += 2 * Math.PI;

            if (Math.Abs(angleDifference) > Math.PI / 2) // 如果 P2 在终点的另一侧
            {
                if (endAngle < 0) endAngle += 2 * Math.PI;
                else endAngle -= 2 * Math.PI;
            }

            return new ArcParameters
            {
                Center = Center,
                Radius = Radius,
                Normal = N,
                E1 = E1,
                E2 = E2
            };
        }
        /// <summary>
        /// 【第 2 步】根据归一化时间 t (0.0 到 1.0) 计算圆弧上的下一个点 P_next。
        /// </summary>
        public static Vector3 CalculateArcPoint(double t, ArcParameters arc, double startAngle, double endAngle)
        {
            // 计算当前的角度 (弧度)
            double currentAngle = startAngle + t * (endAngle - startAngle);

            // 计算圆弧上的点的相对位置 (R)
            // R = R * (cos(theta) * E1 + sin(theta) * E2)
            float R_x = (float)(arc.Radius * Math.Cos(currentAngle));
            float R_y = (float)(arc.Radius * Math.Sin(currentAngle));

            // 转换回世界坐标
            // P_next = Center + R_x * E1 + R_y * E2
            Vector3 P_next = arc.Center + R_x * arc.E1 + R_y * arc.E2;

            return P_next;
        }
    }
}
