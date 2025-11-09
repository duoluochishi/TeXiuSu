using RoboDk;
using RoboDk.API;
using RobotDynamics.MathUtilities;
using RobotDynamics.Robots;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeXiuSi.Helper
{
    public class RobotDynamicsHelper
    {
        // 定义常量：弧度到度数的转换因子
        private const double RadToDeg = 180.0 / Math.PI;

        public Robot MainRobot = null;
        public Robot ConfigureRobot()
        {
            // 这是一个通用的 6DOF 机械臂配置示例，基于 DH 参数的理解。
            // 请务必替换为您的机械臂的准确 DH 参数。

            Robot Ro = new Robot()

                 // -----------------------------------------------------
                 // 关节 0：J0 (Base) - 绕 Z 轴旋转
                 // J0 的旋转点在 (0, 0, 0)。相对偏移为 (0, 0, 0)
                 // -----------------------------------------------------
                 .AddJoint('z', new Vector(0, 0, 0)) // P0 偏移

                 // -----------------------------------------------------
                 // 关节 1：J1 - 绕 Y 轴旋转
                 // P1 相对于 P0 的偏移: (348, -243, 775) - (0, 0, 0)
                 // -----------------------------------------------------
                 .AddJoint('y', new Vector(348, -243, 775)) // P1 偏移

                 // -----------------------------------------------------
                 // 关节 2：J2 - 绕 Y 轴旋转
                 // P2 相对于 P1 的偏移: (-1, -133, 1148)
                 // -----------------------------------------------------
                 .AddJoint('y', new Vector(-1, -133, 1148)) // P2 偏移

                 // -----------------------------------------------------
                 // 关节 3：J3 - 绕 X 轴旋转 (注意：您的低级设置是 X 轴)
                 // P3 相对于 P2 的偏移: (-287, 376, 202)
                 // -----------------------------------------------------
                 .AddJoint('x', new Vector(-287, 376, 202)) // P3 偏移

                 // -----------------------------------------------------
                 // 关节 4：J4 - 绕 Y 轴旋转
                 // P4 相对于 P3 的偏移: (1755, 0, 0)
                 // -----------------------------------------------------
                 .AddJoint('y', new Vector(1755, 0, 0)) // P4 偏移

                 // -----------------------------------------------------
                 // 关节 5：J5 (末端关节) - 绕 X 轴旋转
                 // P5 相对于 P4 的偏移: (193, 0, 0)
                 // -----------------------------------------------------
                 .AddJoint('x', new Vector(193, 0, 0)); // P5 偏移

            // -----------------------------------------------------
            // 末端执行器/工具中心点 (Tool Center Point - TCP)
            // 如果 J5 的旋转点到 TCP 还有一个固定连杆，需要添加 Link.Fixed
            // 假设工具从 P5 处沿 X 轴延伸 100mm
            // .Links.Add(Link.Fixed(new Vector(100, 0, 0))); // 仅作示例
            // -----------------------------------------------------
            MainRobot = Ro;
            return Ro;



        }
        // --- 姿态计算辅助函数 ---
        /// <summary>
        /// 根据欧拉角 (Roll, Pitch, Yaw) 计算 3x3 旋转矩阵
        /// 注意：此库使用的欧拉角顺序可能不同 (XYZ, ZYX 等)，这里提供 ZYX 示例
        /// ZYX 顺序：Yaw (绕 Z) -> Pitch (绕 Y) -> Roll (绕 X)
        /// </summary>
        /// <param name="roll">绕 X 轴旋转，弧度</param>
        /// <param name="pitch">绕 Y 轴旋转，弧度</param>
        /// <param name="yaw">绕 Z 轴旋转，弧度</param>
        /// <returns>3x3 旋转矩阵</returns>
        public static RotationMatrix GetRotationMatrixFromZYX(double roll, double pitch, double yaw)
        {
            double cosR = Math.Cos(roll), sinR = Math.Sin(roll);
            double cosP = Math.Cos(pitch), sinP = Math.Sin(pitch);
            double cosY = Math.Cos(yaw), sinY = Math.Sin(yaw);

            // R = Rz(Yaw) * Ry(Pitch) * Rx(Roll)
            // 计算旋转矩阵的 9 个元素
            double R11 = cosY * cosP;
            double R12 = cosY * sinP * sinR - sinY * cosR;
            double R13 = cosY * sinP * cosR + sinY * sinR;

            double R21 = sinY * cosP;
            double R22 = sinY * sinP * sinR + cosY * cosR;
            double R23 = sinY * sinP * cosR - cosY * sinR;

            double R31 = -sinP;
            double R32 = cosP * sinR;
            double R33 = cosP * cosR;

            // *** FIX: 使用 double[3, 3] 数组构建旋转矩阵，以匹配库的 RotationMatrix(double[,] matrix) 构造函数 ***
            double[,] rotationArray = new double[3, 3]
            {
            { R11, R12, R13 },
            { R21, R22, R23 },
            { R31, R32, R33 }
            };

            return new RotationMatrix(rotationArray);
        }

        public double[] UserComputeInverseKinematicsMethod(double targetPositionX, double targetPositionY, double targetPositionZ, double targetRollDegInfo, double targetPitchDegInfo, double targetYawDegInfo)
        {
            double[] douAngle = new double[6];

            // 1. 配置机械臂模型
            Robot robotArm = ConfigureRobot();

            Console.WriteLine($"已配置 {robotArm.Links.Count} 关节机械臂模型.");

            // 2. 定义目标位置 (I_r_IE) 和目标姿态 (C_IE_des)

            // --- 目标位置 (x, y, z) (单位: mm) ---
            // 假设您希望末端执行器位于 (X=600, Y=250, Z=400)
            Vector targetPosition = new Vector(targetPositionX, targetPositionY, targetPositionZ);

            // --- 目标姿态 (3x3 旋转矩阵) ---
            // 姿态定义：
            //double targetRollDeg = 0;   // 绕 X 轴滚转 (度数)
            //double targetPitchDeg = 45; // 绕 Y 轴俯仰 (度数)
            //double targetYawDeg = 0;    // 绕 Z 轴偏航 (度数)

            double targetRollDeg = targetRollDegInfo;   // 绕 X 轴滚转 (度数)
            double targetPitchDeg = targetPitchDegInfo; // 绕 Y 轴俯仰 (度数)
            double targetYawDeg = targetYawDegInfo;    // 绕 Z 轴偏航 (度数)

            // 转换为弧度
            double targetRollRad = targetRollDeg / RadToDeg;
            double targetPitchRad = targetPitchDeg / RadToDeg;
            double targetYawRad = targetYawDeg / RadToDeg;

            RotationMatrix targetRotation = GetRotationMatrixFromZYX(
                targetRollRad,
                targetPitchRad,
                targetYawRad
            );

            Console.WriteLine("\n--- 求解目标 ---");
            Console.WriteLine($"目标位置 I_r_IE: ({targetPosition.X:F1}, {targetPosition.Y:F1}, {targetPosition.Z:F1})");
            Console.WriteLine($"目标姿态 (Pitch={targetPitchDeg} 度, Roll={targetRollDeg} 度, Yaw={targetYawDeg} 度)");


            Log.Information("\n--- 求解目标 ---");
            Log.Information($"目标位置 I_r_IE: ({targetPosition.X:F1}, {targetPosition.Y:F1}, {targetPosition.Z:F1})");
            Log.Information($"目标姿态 (Pitch={targetPitchDeg} 度, Roll={targetRollDeg} 度, Yaw={targetYawDeg} 度)");
            // 3. 定义初始关节角 q_0（建议使用）
            // 提供初始值可以帮助迭代算法更快收敛，并可能找到更合理的解。
            // 假设初始所有关节角为 0 弧度。
            double[] q_0_start = new double[] { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 };

            // 4. 调用逆运动学求解器
            try
            {
                // ComputeInverseKinematics 返回一个 IterationResult 对象
                IterationResult result = robotArm.ComputeInverseKinematics(
                    I_r_IE: targetPosition,
                    C_IE_des: targetRotation,
                    q_0: q_0_start,
                    tol: 0.5f,        // 容忍度，调低会更精确，但可能需要更多迭代
                    max_it: 200       // 增加最大迭代次数
                                      // lambda 和 alpha 保持默认值通常效果不错
                );

                Console.WriteLine("\n--- 逆运动学求解结果 ---");
                Console.WriteLine($"迭代次数: {result.numberOfIterationsPerfomred}");


                if (result.DidConverge)
                {
                    Console.WriteLine("状态: 成功收敛到目标位姿。");
                    Log.Information("状态: 成功收敛到目标位姿。");
                }
                else
                {
                    Console.WriteLine("状态: 未完全收敛（可能达到最大迭代次数）。");
                    Log.Error("状态: 未完全收敛（可能达到最大迭代次数）。");
                }
                // 获取最佳关节角结果
                double[] jointAnglesRad = result.q;

                Console.WriteLine("\n计算得到的关节角 (单位: 度):");
                for (int i = 0; i < jointAnglesRad.Length; i++)
                {
                    // 将弧度转换为度数进行输出
                    double angleInDegrees = jointAnglesRad[i] * RadToDeg;
                    Console.WriteLine($"  关节 {i + 1}: {angleInDegrees:F2} 度");
                    Log.Information($"  关节 {i + 1}: {angleInDegrees:F2} 度");
                    douAngle[i] = angleInDegrees;
                }
                return douAngle;
            }
            catch (Exception ex)
            {

                // 捕获 IK 内部抛出的异常（例如：初始 q 长度错误）
                Console.WriteLine("\n--- 运行时错误 ---");
                Console.WriteLine($"逆运动学求解发生异常: {ex.Message}");
                return douAngle;
            }
        }
    }
}
