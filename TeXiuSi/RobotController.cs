using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TeXiuSi.Helper;
using TeXiuSi.ViewModel;

namespace TeXiuSi
{
    public class RobotController
    {
        private MainViewModel viewModel;
        // 模拟您的依赖项 (IK 求解器、关节、日志)
        private dynamic robotDynamicsHelper; // 假设这是一个包含 IK/FK 方法的类
        private dynamic Log;
        private Timer timerPoint; // 模拟计时器，用于 Stop()

        // 简化模拟关节角度
        private double[] joints = new double[6];

        public event EventHandler<double[]> ArcMotionChangeEvent;

        public EventHandler<double[]> handler;
        // 构造函数，用于初始化
        public RobotController(MainViewModel vm)
        {
            this.viewModel = vm;
            // 模拟初始化
            this.robotDynamicsHelper = new RobotDynamicsHelper();
            robotDynamicsHelper.ConfigureRobot();
            this.timerPoint = new Timer();

            handler = ArcMotionChangeEvent;
        }

        // 运动开始前的初始化函数：计算圆弧参数
        public void StartArcMotion(Vector3 P_start, Vector3 P_aux, Vector3 P_end, double targetRoll, double targetPitch, double targetYaw)
        {
            // 1. 设置 ViewModel 的起点、辅助点和终点
            //viewModel.startPosition = P_start;
            //viewModel.auxPosition = P_aux;
            //viewModel.targetPosition = P_end;

            // 姿态的起点通常通过前运动学 (FK) 从当前关节角度计算，这里简化为零或假定已设置
            // viewModel.startRoll = ...
            // viewModel.targetRoll = targetRoll; // 假设 target 姿态已传入

            // 2. 计算圆弧几何参数
            try
            {
                double startAngle, endAngle;
                viewModel.ArcGeometry = ArcMotionHelper.CalculateArcParameters(P_start, P_aux, P_end, out startAngle, out endAngle);
                viewModel.StartAngleRad = startAngle;
                viewModel.EndAngleRad = endAngle;

                // 3. 启动运动状态
                viewModel.isMoving = true;
                viewModel.currentStep = 0;
                // timerPoint.Start(); // 模拟启动计时器
                //Log.Information("圆弧运动参数计算成功，启动运动。");

            }
            catch (Exception ex)
            {
                Log.Error("圆弧参数计算失败: " + ex.Message);
                viewModel.isMoving = false;
            }
        }


        /// <summary>
        /// 【核心】计时器 Tick 函数，实现圆弧插补
        /// </summary>
        public void timerArcMotion_Tick(object sender, EventArgs e)
        {
            try
            {
                if (!viewModel.isMoving)
                {
                    timerPoint.Stop();
                    return;
                }

                double t = (double)viewModel.currentStep / viewModel.totalMovementSteps; // 归一化时间 t (从 0.0 到 1.0)

                // --- 1. 笛卡尔空间位置圆弧插值 (X, Y, Z) ---
                Vector3 nextPoint = ArcMotionHelper.CalculateArcPoint(
                    t,
                    viewModel.ArcGeometry,
                    viewModel.StartAngleRad,
                    viewModel.EndAngleRad
                );

                double nextX = nextPoint.X;
                double nextY = nextPoint.Y;
                double nextZ = nextPoint.Z;

                // --- 2. 姿态线性插值 (Rx, Ry, Rz) ---
                // 姿态依然保持线性插值，以简化代码
                double nextRoll = viewModel.StartPointRz + t * (viewModel.EndPointRx - viewModel.StartPointRz);
                double nextPitch = viewModel.StartPointRy + t * (viewModel.EndPointRy - viewModel.StartPointRy);
                double nextYaw = viewModel.StartPointRz + t * (viewModel.EndPointRz - viewModel.StartPointRz);

                // --- 3. 调用 IK 求解器计算下一帧的关节角度 ---
                double[] angles = robotDynamicsHelper.UserComputeInverseKinematicsMethod(
                    nextX, nextY, nextZ,
                    nextRoll, nextPitch, nextYaw
                );

                // 检查并更新关节角度 (与您原有的逻辑相同)
                if (angles == null || angles.Length < 6)
                {
                    Log.Information("IK 计算失败，终止运动");
                    viewModel.isMoving = false;
                    return;
                }
                handler?.Invoke(this, angles);
                // ... ForwardKinematics(angles); // 模拟调用前运动学更新模型

                // 更新关节角度到内部数组和 ViewModel
                for (int i = 0; i < 6; i++)
                {
                    joints[i] = angles[i];
                }
                viewModel.Joint1Angle = angles[0].ToString("F3");
                // ... (更新其他关节到 ViewModel) ...


                // --- 4. 步进和终止条件 ---
                viewModel.currentStep++;

                if (viewModel.currentStep >= viewModel.totalMovementSteps)
                {
                    viewModel.isMoving = false;
                    timerPoint.Stop();
                    Log.Information("圆弧运动完成。");
                }

            }
            catch (Exception ex)
            {
                Log.Error("轨迹计算失败:" + ex.Message);
                viewModel.isMoving = false;
                timerPoint.Stop();
            }
        }
    }
}
