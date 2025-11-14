using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TeXiuSi.Helper;

namespace TeXiuSi
{
    // 定义一个更通用的轨迹点结构体
    public struct TrajectoryPoint
    {
        public Vector3 Position;
        public Quaternion Orientation;
        public float Time;
    }
    public class ArcInterpolator
    {
        /// <summary>
        /// 根据圆弧几何参数和姿态，生成一系列平滑的轨迹点。
        /// </summary>
        /// <param name="arcParams">圆弧几何参数 (Center, Radius, Normal)</param>
        /// <param name="pStart">起点</param>
        /// <param name="pEnd">终点</param>
        /// <param name="rpyStart">起始姿态 (度)</param>
        /// <param name="rpyEnd">结束姿态 (度)</param>
        /// <param name="totalTime">总运动时间 (秒)</param>
        /// <param name="timeStep">时间步长 (秒，决定点的密度)</param>
        /// <returns>笛卡尔空间轨迹点列表。</returns>
        public static List<TrajectoryPoint> GenerateInterpolatedPoints(
            ArcGeometryCalculator.ArcParameters arcParams,
            Vector3 pStart, Vector3 pEnd,
            (float R, float P, float Y) rpyStart,
            (float R, float P, float Y) rpyEnd,
            float totalTime, float timeStep = 0.01f)
        {
            var points = new List<TrajectoryPoint>();

            // 1. 初始化
            var C = new Vector3((float)arcParams.Center.X, (float)arcParams.Center.Y, (float)arcParams.Center.Z);
            var R = (float)arcParams.Radius;

            // 计算起始和结束的姿态四元数
            var qStart = KinematicsHelper.RPYToQuaternion(rpyStart.R, rpyStart.P, rpyStart.Y);
            var qEnd = KinematicsHelper.RPYToQuaternion(rpyEnd.R, rpyEnd.P, rpyEnd.Y);

            // 2. 建立圆弧局部坐标系
            // 径向向量 U：从圆心指向起点
            Vector3 U_s = Vector3.Normalize(pStart - C);

            // 法向量 N：圆弧所在平面法向量
            Vector3 N_unit = new Vector3((float)arcParams.Normal.X, (float)arcParams.Normal.Y, (float)arcParams.Normal.Z);

            // 切向向量 V：垂直于 U 和 N (U x N)，与圆弧相切的方向
            Vector3 V = Vector3.Cross(N_unit, U_s);

            // 3. 计算圆弧总角度
            Vector3 U_e = Vector3.Normalize(pEnd - C);
            // 使用 Vector3.Dot 和 Vector3.Cross 计算夹角，同时确定方向
            float dotProduct = Vector3.Dot(U_s, U_e);
            float crossMagnitude = Vector3.Dot(Vector3.Cross(U_s, U_e), N_unit);
            float totalAngle = (float)Math.Atan2(crossMagnitude, dotProduct);

            // 确保角度在 0 到 2PI 之间，或者直接使用 Atan2 结果（通常更方便）
            // totalAngle = totalAngle < 0 ? totalAngle + (float)(2 * Math.PI) : totalAngle;


            // 4. 循环插值
            for (float t = 0; t <= totalTime; t += timeStep)
            {
                float ratio = t / totalTime; // 插值比例 (0.0 到 1.0)

                // 确保最后一个点精确是终点
                if (t + timeStep > totalTime)
                {
                    ratio = 1.0f;
                    t = totalTime;
                }

                // A. 位置插值 (圆弧)
                float currentAngle = totalAngle * ratio;

                // 旋转 U_s 向量： P(t) = C + R * ( U_s * cos(angle) + V * sin(angle) )
                // 注意：这里需要一个旋转矩阵或四元数来旋转 U_s。
                // 简单起见，我们直接利用 U_s 和 V 向量在圆弧平面上进行插值

                // 旋转 U_s 向量：通过当前角度 currentAngle 旋转 U_s 得到新的径向向量 U_t
                // 创建一个绕 N_unit 旋转 currentAngle 的四元数
                Quaternion rotationQ = Quaternion.CreateFromAxisAngle(N_unit, currentAngle);

                // 旋转 U_s 向量，得到 U_t
                Vector3 U_t = Vector3.Transform(U_s, rotationQ);

                // 计算当前点位置：圆心 + 半径 * 新径向向量
                Vector3 currentPos = C + R * U_t;

                // B. 姿态插值 (Slerp)
                Quaternion currentQ = Quaternion.Slerp(qStart, qEnd, ratio);

                points.Add(new TrajectoryPoint
                {
                    Position = currentPos,
                    Orientation = currentQ,
                    Time = t
                });

                if (ratio == 1.0f) break; // 避免 timeStep 带来的浮点误差
            }

            return points;
        }
    }
}
