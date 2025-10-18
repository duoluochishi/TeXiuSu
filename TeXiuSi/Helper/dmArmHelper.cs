using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TeXiuSi.Helper
{
    public class dmArmHelper
    {


        //// 使用 DllImport 特性声明要调用的 C++ 函数
        //// DllName: DLL文件的名称
        //// EntryPoint: 函数在 C++ DLL中的名称（这里因为使用了 extern "C"，所以名称一致）
        //[DllImport("dm_arm.dll", CallingConvention = CallingConvention.Cdecl)]



        //public static extern string init_motor();

        // C++ DmMotor 对象的指针句柄
        private IntPtr _motorHandle;

        // 使用 DllImport 声明 C++ DLL 中的 C 接口函数
        [DllImport("Project1.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr create_dm_motor();

        [DllImport("Project1.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void destroy_dm_motor(IntPtr motor);

        [DllImport("Project1.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void dm_motor_init(IntPtr motor);

        [DllImport("Project1.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int Add(int a, int b);
        


        // 析构函数或 IDisposable：用于释放 C++ 对象
        public void Dispose()
        {
            if (_motorHandle != IntPtr.Zero)
            {
                destroy_dm_motor(_motorHandle);
                _motorHandle = IntPtr.Zero; // 防止重复释放
            }
        }

        // 封装 C++ 中的 init() 方法
        public void Init()
        {
            if (_motorHandle == IntPtr.Zero)
            {
                throw new ObjectDisposedException("DmMotor object has been destroyed.");
            }
            dm_motor_init(_motorHandle);
        }


        public void testArm()
        {
            //var a = Add(1,2);
            //Console.WriteLine($"Calling : Result = {a.ToString()}");

            _motorHandle = create_dm_motor();
            if (_motorHandle == IntPtr.Zero)
            {
                throw new InvalidOperationException("Failed to create DmMotor instance.");
            }


            Init();
            //var str = init_motor();

            //Console.WriteLine($"The result of  {str}");

        }
    }
}
