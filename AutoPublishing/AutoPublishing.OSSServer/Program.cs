using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Globalization;
using System.Runtime.Loader;

namespace AutoPublishing.OSSServer
{
    class Program
    {
        /// <summary>
        /// 一个OSS服务
        /// 更新自己的心跳
        /// 如果有新的任务，执行任务，并把任务更新掉。
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            AssemblyLoadContext.Default.Unloading += (ctx) =>
            {
                Console.WriteLine(new Exception("服务自动退出!"));
            };
            Console.WriteLine("服务已启动");
            CreateHostBuilder(args).Build().Run();
        }
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
               .ConfigureServices((hostContext, services) =>
               {
                   services.AddHostedService<WorkServer>();
               });

        }
    }
}
