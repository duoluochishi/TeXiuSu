using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TeXiuSi.Helper
{
    public class KinematicsHelper
    {
        // PI / 180 = 0.0174532925
        private const float DEG_TO_RAD = (float)Math.PI / 180f;


        // 1. RPY (度) 到 Quaternion (四元数) 的转换
        // 我们假设机械臂的 RPY 旋转顺序是 ZYX (Yaw-Pitch-Roll)，这是机器人学中常见的顺序。
        public static Quaternion RPYToQuaternion(float rollDeg, float pitchDeg, float yawDeg)
        {
            // 角度转弧度
            float roll = rollDeg * DEG_TO_RAD;
            float pitch = pitchDeg * DEG_TO_RAD;
            float yaw = yawDeg * DEG_TO_RAD;

            // 旋转角的半角
            float cr = (float)Math.Cos(roll * 0.5f);
            float sr = (float)Math.Sin(roll * 0.5f);
            float cp = (float)Math.Cos(pitch * 0.5f);
            float sp = (float)Math.Sin(pitch * 0.5f);
            float cy = (float)Math.Cos(yaw * 0.5f);
            float sy = (float)Math.Sin(yaw * 0.5f);

            // ZYX 顺序的四元数计算公式 (W, X, Y, Z)
            float w = cy * cp * cr + sy * sp * sr;
            float x = cy * cp * sr - sy * sp * cr;
            float y = sy * cp * sr + cy * sp * cr;
            float z = sy * cp * cr - cy * sp * sr;

            return new Quaternion(x, y, z, w);
        }

        // 2. Quaternion 到 RPY (度) 的转换 (方便查看)
        public static (float Roll, float Pitch, float Yaw) QuaternionToRPY(Quaternion q)
        {
            // 简化版，用于显示，实际可能需要更鲁棒的实现来避免万向锁问题
            // ... 此处省略复杂转换，假设模拟器仅需 Quaternion 即可
            return (0f, 0f, 0f);
        }
        // 辅助函数：将四元数转换为 RPY 角度 (度)
        // 假设您的 IK 函数需要的是度数
        public static (double Roll, double Pitch, double Yaw) QuaternionToRPY_Degrees(Quaternion q)
        {
            // 使用标准的 ZYX 欧拉角约定（这是机器人学中常用的）
            // 此处是标准转换公式，可能会有万向锁问题（Gimbal Lock），但适用于大多数非奇点情况。

            // 弧度
            double rollRad = Math.Atan2(2.0 * (q.W * q.X + q.Y * q.Z), 1.0 - 2.0 * (q.X * q.X + q.Y * q.Y));
            double pitchRad = Math.Asin(2.0 * (q.W * q.Y - q.Z * q.X));
            double yawRad = Math.Atan2(2.0 * (q.W * q.Z + q.X * q.Y), 1.0 - 2.0 * (q.Y * q.Y + q.Z * q.Z));

            // 转换为度数
            double RAD_TO_DEG = 180.0 / Math.PI;

            double rollDeg = rollRad * RAD_TO_DEG;
            double pitchDeg = pitchRad * RAD_TO_DEG;
            double yawDeg = yawRad * RAD_TO_DEG;

            return (rollDeg, pitchDeg, yawDeg);
        }
    }
}
