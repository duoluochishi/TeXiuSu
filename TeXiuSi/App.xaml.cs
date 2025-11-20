using Microsoft.Extensions.Configuration;
using Serilog; 
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace TeXiuSi
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // 1. 构建配置对象，从 appsettings.json 中读取配置
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) // 设置基础路径为程序运行目录
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // 2. 使用配置初始化 Serilog Logger
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration) // **关键步骤：从配置中读取 Serilog 部分**
                .CreateLogger();

            Log.Information("应用程序已使用 appsettings.json 配置启动。");

            // ... 后续代码保持不变
            Current.Exit += OnApplicationExit;


            // 确保 DeviceOperation 在 MainWindow 实例化之前被实例化和初始化
            DeviceOperation.Instance.ClearInit();

            base.OnStartup(e);
        }

        private void OnApplicationExit(object sender, ExitEventArgs e)
        {
            // 在程序退出时，确保所有缓冲的日志都被写入
            Log.CloseAndFlush();
        }



    }
}
