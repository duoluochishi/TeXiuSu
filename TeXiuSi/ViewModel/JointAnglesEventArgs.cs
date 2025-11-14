using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeXiuSi.ViewModel
{
    public class JointAnglesEventArgs : EventArgs
    {
        // 假设您的 IK 求解结果是 double[]
        public double[] Angles { get; }

        // 传递当前时间点（可选，但有助于调试）
        public float CurrentTime { get; }

        public JointAnglesEventArgs(double[] angles, float time)
        {
            Angles = angles;
            CurrentTime = time;
        }
    }
}
