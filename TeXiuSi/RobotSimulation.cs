using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TeXiuSi
{
    public class RobotSimulation
    {
        public RobotSimulation() { 
        
        }


        public List<TrajectoryPoint> StartArcPlanningAndInterpolation(
        (double X, double Y, double Z) pStart, (double X, double Y, double Z) pMid, (double X, double Y, double Z) pEnd,
        (float R, float P, float Y) rpyStart, (float R, float P, float Y) rpyEnd,
        float totalTime, float timeStep)
        {
            // --- 1. 计算圆弧几何参数 (步骤二) ---
            var arcParams = ArcGeometryCalculator.CalculateArcCenter(pStart, pMid, pEnd);

            if (arcParams.IsCollinear)
            {
                Console.WriteLine("轨迹：三点共线，应执行直线运动。");
                return null;
            }
            if (!arcParams.IsCollinear)
            {
                Console.WriteLine("--- 圆弧几何计算结果 ---");
                Console.WriteLine($"圆心 C: X={arcParams.Center.X:F3}, Y={arcParams.Center.Y:F3}, Z={arcParams.Center.Z:F3}");
                Console.WriteLine($"半径 R: {arcParams.Radius:F3}");
                Console.WriteLine($"法向量 N: X={arcParams.Normal.X:F3}, Y={arcParams.Normal.Y:F3}, Z={arcParams.Normal.Z:F3}");

                // 接下来就可以进行路径点插值了
                // NextStep: GenerateInterpolatedPoints(startPoint, endPoint, arcParams);
            }
            Console.WriteLine("--- 圆弧几何参数计算成功 ---");

            // 将 Math.NET Spatial 的 Point3D 转换为 System.Numerics.Vector3
            var Pstart = new Vector3((float)pStart.X, (float)pStart.Y, (float)pStart.Z);
            var Pend = new Vector3((float)pEnd.X, (float)pEnd.Y, (float)pEnd.Z);

            // --- 2. 路径点插值 (步骤四) ---
            var trajectory = ArcInterpolator.GenerateInterpolatedPoints(
                arcParams, Pstart, Pend, rpyStart, rpyEnd, totalTime, timeStep
            );

            Console.WriteLine($"--- 轨迹点生成成功 ({trajectory.Count} 个点) ---");
            return trajectory;
            // --- 3. 模拟器输出/IK 求解 (下一步) ---
            foreach (var point in trajectory)
            {
                // 在模拟器中，您需要将这些笛卡尔点 (位置和四元数) 传递给模拟器的 IK 求解模块
                // 模拟器IK求解 (下一步需要实现)
                // JointAngles joints = SimulatorIK.Solve(point); 

                // 打印输出，验证轨迹点
                Console.WriteLine($"时间:{point.Time:F2}s | 位置:({point.Position.X:F3}, {point.Position.Y:F3}, {point.Position.Z:F3}) | 姿态 Q:({point.Orientation.X:F3}, {point.Orientation.Y:F3}, {point.Orientation.Z:F3}, {point.Orientation.W:F3})");


            }
        }
    }
}
