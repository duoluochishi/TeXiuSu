#define IRB6700


using HelixToolkit.Wpf;
using RobotDynamics.MathUtilities;
using RobotDynamics.Robots;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using TeXiuSi.Helper;
using TeXiuSi.Model;
using TeXiuSi.uc;
using TeXiuSi.ViewModel;

using Siemens.RosSharp.Urdf;
using static System.Net.Mime.MediaTypeNames;

namespace TeXiuSi
{


    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        #region param
        //provides functionality to 3d models
        //这是一个Model3DGroup对象，它像一个容器，把机械臂的所有独立部件（如底座、大臂、小臂等）组合在一起，方便统一管理。
        //Model3DGroup RA = new Model3DGroup(); //RoboticArm 3d group
        Model3D geom = null; //Debug sphere to check in which point the joint is rotatin



        bool switchingJoint = false;
        bool isAnimating = false;

        RobotDynamicsHelper robotDynamicsHelper = null;


        ModelVisual3D visual;
        double LearningRate = 0.01;
        double SamplingDistance = 0.15;
        double DistanceThreshold = 20;
        //provides render to model3d objects
        ModelVisual3D RoboticArm = new ModelVisual3D();
        //这是一个变换组，可以包含旋转、平移、缩放等多种变换
        Transform3DGroup F1;
        Transform3DGroup F2;
        Transform3DGroup F3;
        Transform3DGroup F4;
        Transform3DGroup F5;
        Transform3DGroup F6;
        //定义一个旋转变换。它需要三个核心参数：
        //旋转轴(AxisAngleRotation3D) : 一个三维向量，如(0, 0, 1) 表示绕Z轴旋转。
        //旋转角度(angles[i]) : 从外部传入的该关节需要旋转的角度。
        //旋转中心(Point3D) : 旋转所围绕的点。
        RotateTransform3D R;
        TranslateTransform3D T;
        RobotDynamics.MathUtilities.Vector reachingPoint;
        int movements = 10;
        System.Windows.Forms.Timer timerPoint;
        System.Windows.Forms.Timer _animationTimer;



        #endregion
        public MainViewModel viewModel;

        public MainWindow()
        {
            InitializeComponent();

            //ApplicationThemeManager.Apply(this);

            robotDynamicsHelper = new RobotDynamicsHelper();

            viewModel = new MainViewModel();


            this.DataContext = viewModel;


            viewModel.AngleChangeEvent += ViewModel_AngleChangeEvent;
            viewModel.TrajectoryReady += OnTrajectoryReady;

            #region Init

            Log.Information("主窗口已初始化。");

            RoboticArm.Content = DeviceOperation.Instance.model3D;

            /** Debug sphere to check in which point the joint is rotating**/
            var builder = new MeshBuilder(true, true);
            var position = new Point3D(0, 0, 0);
            builder.AddSphere(position, 50, 15, 15);
            geom = new GeometryModel3D(builder.ToMesh(), Materials.Brown);
            visual = new ModelVisual3D();
            visual.Content = geom;
             
            viewPort3d.RotateGesture = new MouseGesture(MouseAction.RightClick);
            viewPort3d.PanGesture = new MouseGesture(MouseAction.LeftClick);
            viewPort3d.Children.Add(visual);
            viewPort3d.Children.Add(RoboticArm);
            viewPort3d.Camera.LookDirection = new Vector3D(-74, -1206, -629);
            viewPort3d.Camera.UpDirection = new Vector3D(0.025, 0.4000, 0.917);
            viewPort3d.Camera.Position = new Point3D(-96.3, 1131, 735);

            double[] angles = { DeviceOperation.Instance.joints[0].angle, DeviceOperation.Instance.joints[1].angle, DeviceOperation.Instance.joints[2].angle, DeviceOperation.Instance.joints[3].angle, DeviceOperation.Instance.joints[4].angle, DeviceOperation.Instance.joints[5].angle };
            ForwardKinematics(angles);

            //changeSelectedJoint();

            #region Timer
            timerPoint = new System.Windows.Forms.Timer();
            timerPoint.Interval = 5;
            timerPoint.Tick += new System.EventHandler(timer1_Tick);
           
             _animationTimer = new System.Windows.Forms.Timer();
            _animationTimer.Tick += AnimationTimer_Tick; // 定时器触发的方法
            #endregion

            robotDynamicsHelper.ConfigureRobot();
            #endregion

    
            // 假设 get_current_pose() 能返回当前位姿的六个值
            double[] currentPose = GetCurrentEndEffectorPose(angles);

            double[] angles1 = robotDynamicsHelper.UserComputeInverseKinematicsMethod(
                   currentPose[0], currentPose[1], currentPose[2],
                   currentPose[3], currentPose[4], currentPose[5]
               );
        }

        public void Load3DModel()
        {

        }

        // 接收 ViewModel 发出的“轨迹已准备好”事件
        private void OnTrajectoryReady(object sender, EventArgs e)
        {
            // 设置定时器间隔为规划时使用的 timeStep (转换为 TimeSpan)
            _animationTimer.Interval = 50;

            // 开始动画模拟
            _animationTimer.Start();
        }

        // 定时器触发，每隔 timeStep 执行一次
        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            // 1. 从 ViewModel 的队列中获取下一个关节角度
            JointAnglesEventArgs nextPoint = viewModel.GetNextJointAngles();

            if (nextPoint != null)
            {
                // 2. 执行运动（调用模拟器 API）
                MoveAction(nextPoint.Angles);

                // 3. (可选) 更新 UI 状态
                // UpdateLog($"时间: {nextPoint.CurrentTime:F2}s, 运动中...");
            }
            else
            {
                // 4. 队列为空，运动完成，停止定时器
                _animationTimer.Stop();
                //UpdateLog("圆弧运动模拟完成！");
            }
        }
        /// <summary>
        /// 圆弧运动通知
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //private void ViewModel_ArcMotionChangeEvent(object sender, double[] e)
        //{
        //    MoveAction(e);
        //}
        // 初始化定时器
        //private new System.Windows.Forms.Timer() _animationTimer;
        private float _timeStep = 10; // 保持与规划时使用的 timeStep 一致
       
        // 订阅事件的响应方法
        private void OnNewJointAnglesReceived(object sender, JointAnglesEventArgs e)
        {
            // 确保 UI 更新操作在主 UI 线程上执行 (WPF 必需)
            // 尽管事件可能在 UI 线程上触发，但为了安全，使用 Dispatcher 是好习惯。
            Dispatcher.Invoke(() =>
            {
                // 1. 获取 IK 求解出的关节角度
                double[] jointAngles = e.Angles;

                // 2. 执行运动（调用模拟器 API）
                MoveAction(jointAngles);

                // 3. (可选) 更新 UI 状态或日志
                //UpdateLog($"时间: {e.CurrentTime:F2}s, 开始运动到关节角度...");

                // 4. 继续下一个时间步（如果不是使用定时器的话）
                // 如果您使用计时器来驱动，则不需要这一行
                // _viewModel.SimulateNextStep();
            });
            // *** 必须使用 Dispatcher.Invoke/BeginInvoke 切换回 UI 线程 ***
            // *** 明确转换为 Action 委托 ***
            //Dispatcher.BeginInvoke(new Action(() =>
            //{
            //    JointAnglesEventArgs nextPoint = viewModel.GetNextJointAngles();

            //    if (nextPoint != null)
            //    {
            //        MoveAction(nextPoint.Angles);
            //    }
            //    else
            //    {
            //        _animationTimer.Stop();
            //        UpdateLog("圆弧运动模拟完成！");
            //    }
            //})); // 注意 Action 的闭合括号
        }
        private void ViewModel_AngleChangeEvent(object sender, EventArgs e)
        {
            DeviceOperation.Instance.joints[0].angle = viewModel.doubleAngles[0];
            DeviceOperation.Instance.joints[1].angle = viewModel.doubleAngles[1];
            DeviceOperation.Instance.joints[2].angle = viewModel.doubleAngles[2];
            DeviceOperation.Instance.joints[3].angle = viewModel.doubleAngles[3];
            DeviceOperation.Instance.joints[4].angle = viewModel.doubleAngles[4];
            DeviceOperation.Instance.joints[5].angle = viewModel.doubleAngles[5];
            execute_fk();
        }

        private void jointSelector_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            changeSelectedJoint();
        }

        private void changeSelectedJoint()
        {
            if (DeviceOperation.Instance.joints == null)
                return;

            //int sel = ((int)jointSelector.Value) - 1;
            //switchingJoint = true;
            //unselectModel();
            //if (sel < 0)
            //{
            //    jointX.IsEnabled = false;
            //    jointY.IsEnabled = false;
            //    jointZ.IsEnabled = false;
            //    jointXAxis.IsEnabled = false;
            //    jointYAxis.IsEnabled = false;
            //    jointZAxis.IsEnabled = false;
            //}
            //else
            //{
            //    if (!jointX.IsEnabled)
            //    {
            //        jointX.IsEnabled = true;
            //        jointY.IsEnabled = true;
            //        jointZ.IsEnabled = true;
            //        jointXAxis.IsEnabled = true;
            //        jointYAxis.IsEnabled = true;
            //        jointZAxis.IsEnabled = true;
            //    }
            //    jointX.Value = DeviceOperation.Instance.joints[sel].rotPointX;
            //    jointY.Value = DeviceOperation.Instance.joints[sel].rotPointY;
            //    jointZ.Value = DeviceOperation.Instance.joints[sel].rotPointZ;
            //    jointXAxis.IsChecked = DeviceOperation.Instance.joints[sel].rotAxisX == 1 ? true : false;
            //    jointYAxis.IsChecked = DeviceOperation.Instance.joints[sel].rotAxisY == 1 ? true : false;
            //    jointZAxis.IsChecked = DeviceOperation.Instance.joints[sel].rotAxisZ == 1 ? true : false;
            //    selectModel(DeviceOperation.Instance.joints[sel].model);
            //    updateSpherePosition();
            //}
            switchingJoint = false;
        }
       

        private void ViewPort3D_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Perform the hit test on the mouse's position relative to the viewport.
            HitTestResult result = VisualTreeHelper.HitTest(viewPort3d, e.GetPosition(viewPort3d));
            RayMeshGeometry3DHitTestResult mesh_result = result as RayMeshGeometry3DHitTestResult;

            if (DeviceOperation.Instance._motorControl.oldSelectedModel != null)
                unselectModel();

            if (mesh_result != null)
            {
                selectModel(mesh_result.ModelHit);
            }
        }
        private void ViewPort3D_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point mousePos = e.GetPosition(viewPort3d);
            PointHitTestParameters hitParams = new PointHitTestParameters(mousePos);
            VisualTreeHelper.HitTest(viewPort3d, null, ResultCallback, hitParams);
        }
        public HitTestResultBehavior ResultCallback(HitTestResult result)
        {
            // Did we hit 3D?
            RayHitTestResult rayResult = result as RayHitTestResult;
            if (rayResult != null)
            {
                // Did we hit a MeshGeometry3D?
                RayMeshGeometry3DHitTestResult rayMeshResult = rayResult as RayMeshGeometry3DHitTestResult;
                geom.Transform = new TranslateTransform3D(new Vector3D(rayResult.PointHit.X, rayResult.PointHit.Y, rayResult.PointHit.Z));

                if (rayMeshResult != null)
                {
                    // Yes we did!
                }
            }

            return HitTestResultBehavior.Continue;
        }
        private void unselectModel()
        {
            DeviceOperation.Instance._motorControl.changeModelColor(DeviceOperation.Instance._motorControl.oldSelectedModel, DeviceOperation.Instance._motorControl.oldColor);
        }


        private void selectModel(Model3D pModel)
        {
            try
            {
                Model3DGroup models = ((Model3DGroup)pModel);
                DeviceOperation.Instance._motorControl.oldSelectedModel = models.Children[0] as GeometryModel3D;
            }
            catch (Exception)
            {
                DeviceOperation.Instance._motorControl.oldSelectedModel = (GeometryModel3D)pModel;
            }
            DeviceOperation.Instance._motorControl.oldColor = DeviceOperation.Instance._motorControl.changeModelColor(DeviceOperation.Instance._motorControl.oldSelectedModel, ColorHelper.HexToColor("#ff3333"));
        }



        /**
         * This methodes execute the FK (Forward Kinematics). It starts from the first joint, the base.
         * */
        private void execute_fk()
        {
            /** Debug sphere, it takes the x,y,z of the textBoxes and update its position
             * This is useful when using x,y,z in the "new Point3D(x,y,z)* when defining a new RotateTransform3D() to check where the joints is actually  rotating */
            double[] angles = { DeviceOperation.Instance.joints[0].angle, DeviceOperation.Instance.joints[1].angle, DeviceOperation.Instance.joints[2].angle, DeviceOperation.Instance.joints[3].angle, DeviceOperation.Instance.joints[4].angle, DeviceOperation.Instance.joints[5].angle };
            ForwardKinematics(angles);
            //updateSpherePosition();
        }

        /// <summary>
        /// 这个方法是机械臂能够活动的关键。它根据给定的每个关节的角度，计算出每个部件在3D空间中的最终位置和姿态。这就是所谓的正向运动学 (Forward Kinematics)
        /// </summary>
        /// <param name="angles"></param>
        /// <returns></returns>
        public Vector3D ForwardKinematics(double[] angles)
        {
            //The base only has rotation and is always at the origin, so the only transform in the transformGroup is the rotation R
            // --- 关节 1 (基座) 的变换 ---
            F1 = new Transform3DGroup();
            R = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(DeviceOperation.Instance.joints[0].rotAxisX, DeviceOperation.Instance.joints[0].rotAxisY, DeviceOperation.Instance.joints[0].rotAxisZ), angles[0]), new Point3D(DeviceOperation.Instance.joints[0].rotPointX, DeviceOperation.Instance.joints[0].rotPointY, DeviceOperation.Instance.joints[0].rotPointZ));
            F1.Children.Add(R);

            //This moves the first joint attached to the base, it may translate and rotate. Since the joint are already in the right position (the .stl model also store the joints position
            //in the virtual world when they were first created, so if you load all the .stl models of the joint they will be automatically positioned in the right locations)
            //so in all of these cases the first translation is always 0, I just left it for future purposes if something need to be moved
            //After that, the joint needs to rotate of a certain amount (given by the value in the slider), and the rotation must be executed on a specific point
            //After some testing it looks like the point 175, -200, 500 is the sweet spot to achieve the rotation intended for the joint
            //finally we also need to apply the transformation applied to the base 
            // --- 关节 2 的变换 ---
            F2 = new Transform3DGroup();
            T = new TranslateTransform3D(0, 0, 0);
            R = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(DeviceOperation.Instance.joints[1].rotAxisX, DeviceOperation.Instance.joints[1].rotAxisY, DeviceOperation.Instance.joints[1].rotAxisZ), angles[1]), new Point3D(DeviceOperation.Instance.joints[1].rotPointX, DeviceOperation.Instance.joints[1].rotPointY, DeviceOperation.Instance.joints[1].rotPointZ));
            F2.Children.Add(T);
            F2.Children.Add(R);
            F2.Children.Add(F1);

            //The second joint is attached to the first one. As before I found the sweet spot after testing, and looks like is rotating just fine. No pre-translation as before
            //and again the previous transformation needs to be applied
            // --- 关节 3 的变换 ---
            F3 = new Transform3DGroup();
            T = new TranslateTransform3D(0, 0, 0);
            R = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(DeviceOperation.Instance.joints[2].rotAxisX, DeviceOperation.Instance.joints[2].rotAxisY, DeviceOperation.Instance.joints[2].rotAxisZ), angles[2]), new Point3D(DeviceOperation.Instance.joints[2].rotPointX, DeviceOperation.Instance.joints[2].rotPointY, DeviceOperation.Instance.joints[2].rotPointZ));
            F3.Children.Add(T);
            F3.Children.Add(R);
            F3.Children.Add(F2);

            // --- 关节 4 的变换 ---
            F4 = new Transform3DGroup();
            T = new TranslateTransform3D(0, 0, 0); //1500, 650, 1650
            R = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(DeviceOperation.Instance.joints[3].rotAxisX, DeviceOperation.Instance.joints[3].rotAxisY, DeviceOperation.Instance.joints[3].rotAxisZ), angles[3]), new Point3D(DeviceOperation.Instance.joints[3].rotPointX, DeviceOperation.Instance.joints[3].rotPointY, DeviceOperation.Instance.joints[3].rotPointZ));
            F4.Children.Add(T);
            F4.Children.Add(R);
            F4.Children.Add(F3);

            // --- 关节 5 的变换 ---
            F5 = new Transform3DGroup();
            T = new TranslateTransform3D(0, 0, 0);
            R = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(DeviceOperation.Instance.joints[4].rotAxisX, DeviceOperation.Instance.joints[4].rotAxisY, DeviceOperation.Instance.joints[4].rotAxisZ), angles[4]),
                new Point3D(DeviceOperation.Instance.joints[4].rotPointX, DeviceOperation.Instance.joints[4].rotPointY, DeviceOperation.Instance.joints[4].rotPointZ));
            F5.Children.Add(T);
            F5.Children.Add(R);
            F5.Children.Add(F4);

            //NB: I was having a nightmare trying to understand why it was always rotating in a weird way... SO I realized that the order in which
            //you add the Children is actually VERY IMPORTANT in fact before I was applyting F and then T and R, but the previous transformation
            //Should always be applied as last (FORWARD Kinematics)
            // --- 关节 6 的变换 ---
            F6 = new Transform3DGroup();
            T = new TranslateTransform3D(0, 0, 0);
            R = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(DeviceOperation.Instance.joints[5].rotAxisX, DeviceOperation.Instance.joints[5].rotAxisY, DeviceOperation.Instance.joints[5].rotAxisZ), angles[5]), new Point3D(DeviceOperation.Instance.joints[5].rotPointX, DeviceOperation.Instance.joints[5].rotPointY, DeviceOperation.Instance.joints[5].rotPointZ));
            F6.Children.Add(T);
            F6.Children.Add(R);
            F6.Children.Add(F5);

            //变换的链式关系: 这是运动学的核心。例如，关节2的最终姿态不仅取决于自身的旋转，还取决于其父关节（关节1）的旋转。因此，F2的变换组中包含了F1。这样一层层传递下去，就构成了机械臂的完整运动链
            DeviceOperation.Instance.joints[0].model.Transform = F1; //First joint
            DeviceOperation.Instance.joints[1].model.Transform = F2; //Second joint (the "biceps")
            DeviceOperation.Instance.joints[2].model.Transform = F3; //third joint (the "knee" or "elbow")
            DeviceOperation.Instance.joints[3].model.Transform = F4; //the "forearm"
            DeviceOperation.Instance.joints[4].model.Transform = F5; //the tool plate
            DeviceOperation.Instance.joints[5].model.Transform = F6; //the tool

            //Tx.Content = DeviceOperation.Instance.joints[5].model.Bounds.Location.X;
            //Ty.Content = DeviceOperation.Instance.joints[5].model.Bounds.Location.Y;
            //Tz.Content = DeviceOperation.Instance.joints[5].model.Bounds.Location.Z;
            //Tx_Copy.Content = geom.Bounds.Location.X;
            //Ty_Copy.Content = geom.Bounds.Location.Y;
            //Tz_Copy.Content = geom.Bounds.Location.Z;


            DeviceOperation.Instance.joints[7].model.Transform = F1; //Cables

            //DeviceOperation.Instance.joints[8].model.Transform = F2; //Cables

            DeviceOperation.Instance.joints[6].model.Transform = F3; //The ABB writing
            //DeviceOperation.Instance.joints[9].model.Transform = F3; //Cables

            return new Vector3D(DeviceOperation.Instance.joints[5].model.Bounds.Location.X, DeviceOperation.Instance.joints[5].model.Bounds.Location.Y, DeviceOperation.Instance.joints[5].model.Bounds.Location.Z);
        }


        // 假设您将其重命名为 GetCurrentEndEffectorPose 并返回包含 6 个值的数组
        public double[] GetCurrentEndEffectorPose(double[] angles)
        {
            // ... (保留您所有的 F1 到 F6 的变换计算代码，这些代码是必需的) ...
            //The base only has rotation and is always at the origin, so the only transform in the transformGroup is the rotation R
            // --- 关节 1 (基座) 的变换 ---
            F1 = new Transform3DGroup();
            R = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(DeviceOperation.Instance.joints[0].rotAxisX, DeviceOperation.Instance.joints[0].rotAxisY, DeviceOperation.Instance.joints[0].rotAxisZ), angles[0]), new Point3D(DeviceOperation.Instance.joints[0].rotPointX, DeviceOperation.Instance.joints[0].rotPointY, DeviceOperation.Instance.joints[0].rotPointZ));
            F1.Children.Add(R);

            //This moves the first joint attached to the base, it may translate and rotate. Since the joint are already in the right position (the .stl model also store the joints position
            //in the virtual world when they were first created, so if you load all the .stl models of the joint they will be automatically positioned in the right locations)
            //so in all of these cases the first translation is always 0, I just left it for future purposes if something need to be moved
            //After that, the joint needs to rotate of a certain amount (given by the value in the slider), and the rotation must be executed on a specific point
            //After some testing it looks like the point 175, -200, 500 is the sweet spot to achieve the rotation intended for the joint
            //finally we also need to apply the transformation applied to the base 
            // --- 关节 2 的变换 ---
            F2 = new Transform3DGroup();
            T = new TranslateTransform3D(0, 0, 0);
            R = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(DeviceOperation.Instance.joints[1].rotAxisX, DeviceOperation.Instance.joints[1].rotAxisY, DeviceOperation.Instance.joints[1].rotAxisZ), angles[1]), new Point3D(DeviceOperation.Instance.joints[1].rotPointX, DeviceOperation.Instance.joints[1].rotPointY, DeviceOperation.Instance.joints[1].rotPointZ));
            F2.Children.Add(T);
            F2.Children.Add(R);
            F2.Children.Add(F1);

            //The second joint is attached to the first one. As before I found the sweet spot after testing, and looks like is rotating just fine. No pre-translation as before
            //and again the previous transformation needs to be applied
            // --- 关节 3 的变换 ---
            F3 = new Transform3DGroup();
            T = new TranslateTransform3D(0, 0, 0);
            R = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(DeviceOperation.Instance.joints[2].rotAxisX, DeviceOperation.Instance.joints[2].rotAxisY, DeviceOperation.Instance.joints[2].rotAxisZ), angles[2]), new Point3D(DeviceOperation.Instance.joints[2].rotPointX, DeviceOperation.Instance.joints[2].rotPointY, DeviceOperation.Instance.joints[2].rotPointZ));
            F3.Children.Add(T);
            F3.Children.Add(R);
            F3.Children.Add(F2);

            // --- 关节 4 的变换 ---
            F4 = new Transform3DGroup();
            T = new TranslateTransform3D(0, 0, 0); //1500, 650, 1650
            R = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(DeviceOperation.Instance.joints[3].rotAxisX, DeviceOperation.Instance.joints[3].rotAxisY, DeviceOperation.Instance.joints[3].rotAxisZ), angles[3]), new Point3D(DeviceOperation.Instance.joints[3].rotPointX, DeviceOperation.Instance.joints[3].rotPointY, DeviceOperation.Instance.joints[3].rotPointZ));
            F4.Children.Add(T);
            F4.Children.Add(R);
            F4.Children.Add(F3);

            // --- 关节 5 的变换 ---
            F5 = new Transform3DGroup();
            T = new TranslateTransform3D(0, 0, 0);
            R = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(DeviceOperation.Instance.joints[4].rotAxisX, DeviceOperation.Instance.joints[4].rotAxisY, DeviceOperation.Instance.joints[4].rotAxisZ), angles[4]),
                new Point3D(DeviceOperation.Instance.joints[4].rotPointX, DeviceOperation.Instance.joints[4].rotPointY, DeviceOperation.Instance.joints[4].rotPointZ));
            F5.Children.Add(T);
            F5.Children.Add(R);
            F5.Children.Add(F4);

            //NB: I was having a nightmare trying to understand why it was always rotating in a weird way... SO I realized that the order in which
            //you add the Children is actually VERY IMPORTANT in fact before I was applyting F and then T and R, but the previous transformation
            //Should always be applied as last (FORWARD Kinematics)
            // --- 关节 6 的变换 ---
            F6 = new Transform3DGroup();
            T = new TranslateTransform3D(0, 0, 0);
            R = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(DeviceOperation.Instance.joints[5].rotAxisX, DeviceOperation.Instance.joints[5].rotAxisY, DeviceOperation.Instance.joints[5].rotAxisZ), angles[5]), new Point3D(DeviceOperation.Instance.joints[5].rotPointX, DeviceOperation.Instance.joints[5].rotPointY, DeviceOperation.Instance.joints[5].rotPointZ));
            F6.Children.Add(T);
            F6.Children.Add(R);
            F6.Children.Add(F5);
            // --- 提取位置 (X, Y, Z) ---
            double X = DeviceOperation.Instance.joints[5].model.Bounds.Location.X;
            double Y = DeviceOperation.Instance.joints[5].model.Bounds.Location.Y;
            double Z = DeviceOperation.Instance.joints[5].model.Bounds.Location.Z;


            // --- 3. 提取姿态 (Rx, Ry, Rz) - 困难点，需要矩阵转换 ---

            // 3a. 从最终变换组获取 4x4 矩阵
            Matrix3D M = F6.Value;

            // 3b. 提取旋转子矩阵 (3x3)
            // M.M11 到 M.M33 构成了旋转矩阵 R

            // 注意：接下来的矩阵到欧拉角转换通常依赖于一个外部数学库，
            // 或者需要您手动编写数学公式。

            // **假设机械臂使用 Z-Y-X 欧拉角 (Yaw, Pitch, Roll) **

            double cy = Math.Sqrt(M.M11 * M.M11 + M.M21 * M.M21);
            double Rx, Ry, Rz;

            if (cy > 1e-6) // 非万向锁情况
            {
                // Ry (Pitch)
                Ry = Math.Atan2(-M.M31, cy);

                // Rz (Yaw)
                Rz = Math.Atan2(M.M21, M.M11);

                // Rx (Roll)
                Rx = Math.Atan2(M.M32, M.M33);
            }
            else // 万向锁 (Gimbal Lock) 情况
            {
                // Ry = -M.M31;
                // 简化：Rz 和 Rx 之一可自由选择
                Rz = Math.Atan2(-M.M12, M.M22); // 例如，将 Rx 设为 0
                Ry = Math.Atan2(-M.M31, cy);
                Rx = 0.0;

                // 通常需要更复杂的万向锁处理或使用四元数
            }

            // 返回完整的 6DOF 位姿
            return new double[] { X, Y, Z, Rx, Ry, Rz };
        }


        #region 老方法
        //public void timer1_Tick(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        double[] angles = { DeviceOperation.Instance.joints[0].angle, DeviceOperation.Instance.joints[1].angle, DeviceOperation.Instance.joints[2].angle, DeviceOperation.Instance.joints[3].angle, DeviceOperation.Instance.joints[4].angle, DeviceOperation.Instance.joints[5].angle };


        //        #region 之前的关节角度计算
        //        if (robotDynamicsHelper.MainRobot == null)
        //        {
        //            return;
        //        }
        //        Vector3D reachinCopyPoint = new Vector3D(viewModel.XValue, viewModel.YawValue, viewModel.ZValue);
        //        // 获取最佳关节角结果
        //        //double[] jointAnglesRad = result.q;
        //        angles = InverseKinematics(reachinCopyPoint, angles);
        //        //angles = robotDynamicsHelper.UserComputeInverseKinematicsMethod(reachingPoint.X, reachingPoint.Y, reachingPoint.Z, viewModel.Rollvalue, viewModel.Pitchvalue, viewModel.YawValue);
        //        if (angles == null && angles.Length == 0)
        //        {
        //            Log.Information("计算失败");
        //            return;
        //        }
        //        joint1.Value = DeviceOperation.Instance.joints[0].angle = angles[0];
        //        joint2.Value = DeviceOperation.Instance.joints[1].angle = angles[1];
        //        joint3.Value = DeviceOperation.Instance.joints[2].angle = angles[2];
        //        joint4.Value = DeviceOperation.Instance.joints[3].angle = angles[3];
        //        joint5.Value = DeviceOperation.Instance.joints[4].angle = angles[4];
        //        joint6.Value = DeviceOperation.Instance.joints[5].angle = angles[5];

        //        #endregion
        //        // 将计算出的角度更新回ViewModel，以便UI（如关节角度文本框）可以同步更新
        //        viewModel.Joint1Angle = (DeviceOperation.Instance.joints[0].angle = angles[0]).ToString("F3");
        //        viewModel.Joint2Angle = (DeviceOperation.Instance.joints[1].angle = angles[1]).ToString("F3");
        //        viewModel.Joint3Angle = (DeviceOperation.Instance.joints[2].angle = angles[2]).ToString("F3");
        //        viewModel.Joint4Angle = (DeviceOperation.Instance.joints[3].angle = angles[3]).ToString("F3");
        //        viewModel.Joint5Angle = (DeviceOperation.Instance.joints[4].angle = angles[4]).ToString("F3");
        //        viewModel.Joint6Angle = (DeviceOperation.Instance.joints[5].angle = angles[5]).ToString("F3");

        //        if ((--movements) <= 0)
        //        {
        //            //button.Content = "Go to position";
        //            isAnimating = false;
        //            timer1.Stop();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error("计算失败" + ex.Message);

        //        timer1.Stop();
        //    }

        //}
        #endregion

        public double[] InverseKinematics(Vector3D target, double[] angles)
        {
            if (DistanceFromTarget(target, angles) < DistanceThreshold)
            {
                movements = 0;
                return angles;
            }

            double[] oldAngles = { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 };
            angles.CopyTo(oldAngles, 0);
            for (int i = 0; i <= 5; i++)
            {
                // Gradient descent
                // Update : Solution -= LearningRate * Gradient
                double gradient = PartialGradient(target, angles, i);
                angles[i] -= LearningRate * gradient;

                // Clamp
                angles[i] = Clamp(angles[i], DeviceOperation.Instance.joints[i].angleMin, DeviceOperation.Instance.joints[i].angleMax);

                // Early termination
                if (DistanceFromTarget(target, angles) < DistanceThreshold || checkAngles(oldAngles, angles))
                {
                    movements = 0;
                    return angles;
                }
            }

            return angles;
        }

        #region 新位移策略 

        public void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                if (!viewModel.isMoving)
                {
                    timerPoint.Stop();
                    return;
                }

                double t = (double)viewModel.currentStep / viewModel.totalMovementSteps; // 归一化时间 t (从 0.0 到 1.0)

                // --- 1. 笛卡尔空间位置线性插值 (X, Y, Z) ---
                // P_next = P_start + t * (P_target - P_start)
                double nextX = viewModel.startPosition.X + t * (viewModel.targetPosition.X - viewModel.startPosition.X);
                double nextY = viewModel.startPosition.Y + t * (viewModel.targetPosition.Y - viewModel.startPosition.Y);
                double nextZ = viewModel.startPosition.Z + t * (viewModel.targetPosition.Z - viewModel.startPosition.Z);

                // --- 2. 姿态线性插值 (Rx, Ry, Rz) ---
                // 注意：欧拉角线性插值可能导致非均匀旋转或万向锁问题，
                // 但对于小角度运动，它比四元数 Slerp 实现更简单。
                double nextRoll = viewModel.startRoll + t * (viewModel.targetRoll - viewModel.startRoll);
                double nextPitch = viewModel.startPitch + t * (viewModel.targetPitch - viewModel.startPitch);
                double nextYaw = viewModel.startYaw + t * (viewModel.targetYaw - viewModel.startYaw);

                // --- 3. 调用 IK 求解器计算下一帧的关节角度 ---

                double[] currentAngles = { DeviceOperation.Instance.joints[0].angle, DeviceOperation.Instance.joints[1].angle, DeviceOperation.Instance.joints[2].angle, DeviceOperation.Instance.joints[3].angle, DeviceOperation.Instance.joints[4].angle, DeviceOperation.Instance.joints[5].angle };

                // 调用包含 Rx, Ry, Rz 的新方法
                double[] angles = robotDynamicsHelper.UserComputeInverseKinematicsMethod(
                    nextX, nextY, nextZ,
                    nextRoll, nextPitch, nextYaw
                );

                // 检查并更新关节角度 (与您原有的逻辑相同)
                if (angles == null || angles.Length == 0)
                {
                    Log.Information("IK 计算失败，终止运动");
                    viewModel.isMoving = false;
                    return;
                }
               
                MoveAction(angles);
                // --- 4. 步进和终止条件 ---
                viewModel.currentStep++;

                if (viewModel.currentStep >= viewModel.totalMovementSteps)
                {
                    viewModel.isMoving = false;
                    // 可选：确保最终位置被精确设置一次
                    // angles = robotDynamicsHelper.UserComputeInverseKinematicsMethod(targetX, targetY, targetZ, targetRoll, targetPitch, targetYaw);
                    // ... 再次更新关节角度 ...
                }

            }
            catch (Exception ex)
            {
                Log.Error("轨迹计算失败:" + ex.Message);
                viewModel.isMoving = false;
                timerPoint.Stop();
            }
        }

        public void MoveAction(double[] angles)
        {
            ForwardKinematics(angles);
            // ... 将 angles 赋值给 DeviceOperation.Instance.joints[i].angle 和 UI 控件 (与您原有代码相同) ...
            joint1.Value = DeviceOperation.Instance.joints[0].angle = angles[0];
            joint1.Value = DeviceOperation.Instance.joints[0].angle = angles[0];
            joint2.Value = DeviceOperation.Instance.joints[1].angle = angles[1];
            joint3.Value = DeviceOperation.Instance.joints[2].angle = angles[2];
            joint4.Value = DeviceOperation.Instance.joints[3].angle = angles[3];
            joint5.Value = DeviceOperation.Instance.joints[4].angle = angles[4];
            joint6.Value = DeviceOperation.Instance.joints[5].angle = angles[5];


            // 将计算出的角度更新回ViewModel，以便UI（如关节角度文本框）可以同步更新
            viewModel.Joint1Angle = (DeviceOperation.Instance.joints[0].angle = angles[0]).ToString("F3");
            viewModel.Joint2Angle = (DeviceOperation.Instance.joints[1].angle = angles[1]).ToString("F3");
            viewModel.Joint3Angle = (DeviceOperation.Instance.joints[2].angle = angles[2]).ToString("F3");
            viewModel.Joint4Angle = (DeviceOperation.Instance.joints[3].angle = angles[3]).ToString("F3");
            viewModel.Joint5Angle = (DeviceOperation.Instance.joints[4].angle = angles[4]).ToString("F3");
            viewModel.Joint6Angle = (DeviceOperation.Instance.joints[5].angle = angles[5]).ToString("F3");
        }

        /// <summary>
        /// 圆弧运动Time
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void timerArcMotion_Tick(object sender, EventArgs e)
        {
            try
            {
                //ForwardKinematics(angles);


            }
            catch (Exception ex)
            {
                Log.Error("轨迹计算失败:" + ex.Message);
                viewModel.isMoving = false;

            }
        }


        #endregion

        public bool checkAngles(double[] oldAngles, double[] angles)
        {
            for (int i = 0; i <= 5; i++)
            {
                if (oldAngles[i] != angles[i])
                    return false;
            }

            return true;
        }

        public static T Clamp<T>(T value, T min, T max)
    where T : System.IComparable<T>
        {
            T result = value;
            if (value.CompareTo(max) > 0)
                result = max;
            if (value.CompareTo(min) < 0)
                result = min;
            return result;
        }
        public double DistanceFromTarget(Vector3D target, double[] angles)
        {
            Vector3D point = ForwardKinematics(angles);
            return Math.Sqrt(Math.Pow((point.X - target.X), 2.0) + Math.Pow((point.Y - target.Y), 2.0) + Math.Pow((point.Z - target.Z), 2.0));
        }
        public double PartialGradient(Vector3D target, double[] angles, int i)
        {
            // Saves the angle,
            // it will be restored later
            double angle = angles[i];

            // Gradient : [F(x+SamplingDistance) - F(x)] / h
            double f_x = DistanceFromTarget(target, angles);

            angles[i] += SamplingDistance;
            double f_x_plus_d = DistanceFromTarget(target, angles);

            double gradient = (f_x_plus_d - f_x) / SamplingDistance;

            // Restores
            angles[i] = angle;

            return gradient;
        }


        #region joinsts info

        private void joint_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (isAnimating)
                return;

            DeviceOperation.Instance.joints[0].angle = joint1.Value;
            DeviceOperation.Instance.joints[1].angle = joint2.Value;
            DeviceOperation.Instance.joints[2].angle = joint3.Value;
            DeviceOperation.Instance.joints[3].angle = joint4.Value;
            DeviceOperation.Instance.joints[4].angle = joint5.Value;
            DeviceOperation.Instance.joints[5].angle = joint6.Value;
            execute_fk();
        }
        private void ReachingPoint_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                //reachingPoint = new Vector3D(Double.Parse(TbX.Text), Double.Parse(TbY.Text), Double.Parse(TbZ.Text));
                //geom.Transform = new TranslateTransform3D(reachingPoint);
            }
            catch (Exception)
            {

            }
        }
        private void CheckBox_StateChanged(object sender, RoutedEventArgs e)
        {
            if (switchingJoint)
                return;

            //int sel = ((int)jointSelector.Value) - 1;
            //DeviceOperation.Instance.joints[sel].rotAxisX = jointXAxis.IsChecked.Value ? 1 : 0;
            //DeviceOperation.Instance.joints[sel].rotAxisY = jointYAxis.IsChecked.Value ? 1 : 0;
            //DeviceOperation.Instance.joints[sel].rotAxisZ = jointZAxis.IsChecked.Value ? 1 : 0;
        }
        private void rotationPointChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (switchingJoint)
                return;

            //int sel = ((int)jointSelector.Value) - 1;
            //DeviceOperation.Instance.joints[sel].rotPointX = (int)jointX.Value;
            //DeviceOperation.Instance.joints[sel].rotPointY = (int)jointY.Value;
            //DeviceOperation.Instance.joints[sel].rotPointZ = (int)jointZ.Value;
            //updateSpherePosition();
        }

        public void StartInverseKinematics(object sender, RoutedEventArgs e)
        {
            if (timerPoint.Enabled)
            {
                //button.Content = "Go to position";
                isAnimating = false;
                timerPoint.Stop();
                movements = 0;
            }
            else
            {
                Vector3D reachingPointCopy = new Vector3D(reachingPoint.X, reachingPoint.Y, reachingPoint.Z);


                geom.Transform = new TranslateTransform3D(reachingPointCopy);
                movements = 5000;
                //button.Content = "STOP";
                isAnimating = true;
                //timer1.Start();
                StartMovement(reachingPoint.X, reachingPoint.Y, reachingPoint.Z, viewModel.Rollvalue, viewModel.Pitchvalue, viewModel.YawValue);
            }
        }

        public void StartMovement(double x, double y, double z, double roll, double pitch, double yaw)
        {
            // 1. 记录起点：从当前模型获取
            // 假设您能从当前关节角度通过正运动学 (FK) 获取当前 X,Y,Z,Rx,Ry,Rz
            // 如果不能直接获取 Rx, Ry, Rz，您需要先通过 FK 拿到 RotationMatrix 并转换为欧拉角。
            double[] currentAngles = { DeviceOperation.Instance.joints[0].angle, DeviceOperation.Instance.joints[1].angle, DeviceOperation.Instance.joints[2].angle, DeviceOperation.Instance.joints[3].angle, DeviceOperation.Instance.joints[4].angle, DeviceOperation.Instance.joints[5].angle };
            // 假设 get_current_pose() 能返回当前位姿的六个值
            double[] currentPose = GetCurrentEndEffectorPose(currentAngles);

            //发送位移参数给关节
            DeviceOperation.Instance.ControlJointsMove( viewModel.SelectedSportType.Value,currentPose,viewModel.TotalSpeed,1);

            viewModel.startPosition = new Vector3D(currentPose[0], currentPose[1], currentPose[2]);
            viewModel.startRoll = currentPose[3];
            viewModel.startPitch = currentPose[4];
            viewModel.startYaw = currentPose[5];

            // 2. 记录终点
            viewModel.targetPosition = new Vector3D(x, y, z);
            viewModel.targetRoll = roll;
            viewModel.targetPitch = pitch;
            viewModel.targetYaw = yaw;

            // 3. 重置步数并启动
            viewModel.currentStep = 0;
            viewModel.isMoving = true;
            // 确保 timer1 已经启动
            timerPoint.Start();
        }
        #endregion

        private void JawTorqueTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // 使用正则表达式来验证输入
            // 允许输入：数字、一个小数点、一个负号在开头
            //TextBox textBox = sender as TextBox;
            //string newText = textBox.Text.Insert(textBox.CaretIndex, e.Text);

            //// 正则表达式解释:
            //// ^                - 字符串开始
            //// -?               - 可选的负号
            //// \d*              - 任意数量的数字
            //// (\.\d{0,3})?     - 一个可选的分组，包含:
            ////   \.             -   一个小数点
            ////   \d{0,3}        -   0到3个数字 (因为你的格式化是 F3)
            //// $                - 字符串结束
            //Regex regex = new Regex(@"^-?\d*(\.\d{0,3})?$");

            //// 如果新文本不匹配正则表达式，则阻止输入
            //if (!regex.IsMatch(newText))
            //{
            //    e.Handled = true; // 设置为 true 表示事件已处理，输入不会发生
            //}

            var textBox = sender as TextBox;

            // 检查输入的字符
            // 如果是数字，则允许输入
            if (char.IsDigit(e.Text, 0))
            {
                e.Handled = false; // false = 不处理事件，即允许输入
                return;
            }

            // 如果是小数点
            if (e.Text == ".")
            {
                // 检查文本框中是否已经有小数点了
                if (!textBox.Text.Contains("."))
                {
                    e.Handled = false; // 如果没有，则允许输入
                    return;
                }
            }

            // 对于所有其他字符，都阻止输入
            e.Handled = true; // true = 事件已处理，即阻止输入
        }


        // 4. 附加逻辑：处理粘贴操作，防止粘贴无效内容
        private static void TextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            // 检查粘贴板中是否有文本
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));

                // 使用 double.TryParse 检查粘贴的文本是否为有效的浮点数
                // (我们只关心它是否由数字和小数点组成)
                if (!double.TryParse(text, out _))
                {
                    // 如果不是有效数字，则取消粘贴命令
                    e.CancelCommand();
                }
            }
            else
            {
                // 如果粘贴的不是文本，也取消
                e.CancelCommand();
            }
        }

        private void btnMin_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).WindowState = WindowState.Minimized;
        }

        private void btnMax_Click(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            window.WindowState = window.WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            //判断电机状态


            Window.GetWindow(this).Close();
        }
        private void NameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox == null) return;

            // 假设您要限制输入为最多三位小数
            string pattern = @"^-?\d*(\.\d{1,3})?$"; // 匹配整数或小数点后最多三位的数字

            if (!System.Text.RegularExpressions.Regex.IsMatch(textBox.Text, pattern))
            {
                // 如果文本不符合要求，则撤销上一次的更改
                // ⚠️ 注意：需要一种机制来保存上一次合法的值
                // 最简单但粗暴的方式是：
                // string currentText = textBox.Text;
                // string correctedText = Regex.Match(currentText, @"^-?\d*(\.\d{0,3})?").Value;
                // textBox.Text = correctedText;
                // textBox.CaretIndex = correctedText.Length; // 保持光标在末尾
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ConnectPage connectPage = new ConnectPage();
            connectPage.ShowDialog();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            TrajectoryLibraryDialog trajectoryLibraryDialog = new TrajectoryLibraryDialog();
            trajectoryLibraryDialog.ShowDialog();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            JointParameterWindow jointParameterWindow = new JointParameterWindow();
            jointParameterWindow.ShowDialog();
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            VersionUpgrade versionUpgrade = new VersionUpgrade();
            versionUpgrade.ShowDialog();
        }

        private void btnViewRefresh_Click(object sender, RoutedEventArgs e)
        {
            // 调用 ZoomExtents() 方法来重置视角，使其包含所有 3D 对象。
            // 这是实现“视角还原”的最佳实践。
            viewPort3d.ZoomExtents();

            // 如果想重置到默认相机位置，可以使用：
            // viewPort3d.ResetCamera();

            viewPort3d.Camera.LookDirection = new Vector3D(-74, -1206, -629);
            viewPort3d.Camera.UpDirection = new Vector3D(0.025, 0.4000, 0.917);
            viewPort3d.Camera.Position = new Point3D(-96.3, 1131, 735);
        }

        private void btnSet_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SimulateMotionButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //double x = double.Parse(viewModel.StraightLineX);
                //double y = double.Parse(viewModel.StraightLineY);
                //double z = double.Parse(viewModel.StraightLineZ);

                double x = viewModel.XValue;
                double y = viewModel.YValue;
                double z = viewModel.ZValue;
                reachingPoint = new RobotDynamics.MathUtilities.Vector(x, y, z);

                // 调用现有的启动方法，但传入null参数
                StartInverseKinematics(null, null);
            }
            catch (FormatException)
            {
                MessageBox.Show("请输入有效的X, Y, Z坐标值。", "输入错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"发生未知错误: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        // 这是一个概念性的方法，需要您进行复杂的数学推导
        public double[] InverseKinematics(double x, double y, double z, double alpha, double beta, double gamma)
        {
            // 1. 进行复杂的代数或几何运算
            // 2. 求解出六个关节的角度

            // ... 大量的 IK 解算代码 ...

            // 返回计算出的六个关节角度
            //return new double[] { theta1, theta2, theta3, theta4, theta5, theta6 };
            return null;
        }

        // 1. 限制输入字符（只允许数字、小数点和负号）
        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            string fullText = textBox.Text.Insert(textBox.CaretIndex, e.Text); // 预测输入后的完整文本

            // 使用正则表达式匹配：
            // ^-?：允许开头的负号
            // \d*：任意数量的数字
            // (\.\d{0,2})?：可选的小数点，后面最多跟两位数字
            // $：结束
            Regex regex = new Regex(@"^-?\d*(\.\d{0,2})?$");

            // 如果输入后的完整文本不匹配这个模式，就取消事件（即阻止输入）
            if (!regex.IsMatch(fullText))
            {
                e.Handled = true;
            }

            // 额外的处理：如果已经是负号了，不再允许输入负号
            if (e.Text == "-" && textBox.Text.Contains("-"))
            {
                e.Handled = true;
            }

            // 额外的处理：如果已经是小数点，不再允许输入小数点
            if (e.Text == "." && textBox.Text.Contains("."))
            {
                e.Handled = true;
            }
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            // 允许输入数字 (0-9)
            Regex regex = new Regex("[^0-9.-]+");

            // 如果输入的字符不是数字、小数点或负号，则阻止输入
            if (regex.IsMatch(e.Text))
            {
                e.Handled = true;
                return;
            }

            TextBox textBox = sender as TextBox;
            if (textBox == null) return;

            // 额外的逻辑：确保只有一个小数点或负号
            if (e.Text == "." && textBox.Text.Contains("."))
            {
                e.Handled = true;
            }
            if (e.Text == "-" && (textBox.Text.Contains("-") || textBox.CaretIndex != 0))
            {
                e.Handled = true;
            }
        }

        // 2. 限制用户粘贴的内容
        private void PastingNumberValidation(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                if (!IsTextAllowed(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private bool IsTextAllowed(string text)
        {
            // 允许数字、小数点和负号
            Regex regex = new Regex("^[0-9.-]+$");
            return regex.IsMatch(text);
        }

        // 3. 检查值是否超过 180 的限制
        private void LimitValue_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox == null) return;

            if (double.TryParse(textBox.Text, out double value))
            {
                // 限制不能大于 180
                if (value > 180.0)
                {
                    // 将值强制设置为 180.0
                    textBox.Text = "180.00";

                    // 提示用户 (可选)
                    // MessageBox.Show("输入值不能大于 180。", "验证错误");
                }

                // 额外的逻辑：如果您的机械臂角度通常在 -180 到 180 之间，您也可以添加下限
                // if (value < -180.0) 
                // { 
                //     textBox.Text = "-180.00"; 
                // }
            }
            else
            {
                // 如果解析失败，您可以选择清空或设置为默认值
                // textBox.Text = "0.00";
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            DeviceOperation.GetInstance().RefreshInfoEditorOfJoints(ControlPowModelOther.Refresh, Register.CTRL_MODE);
        }
    }
}
