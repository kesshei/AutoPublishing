using AutoPublishing.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace AutoPublishing.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            //OSSServer.ClearOSS();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Reset();
            stopwatch.Start();

            var list = new List<string>();
            var serverUrl = ConfigManage.GetSetting("serverUrl");
            Console.WriteLine($"加載服务端地址:{serverUrl}");
            Console.WriteLine($"加載OSS端地址:{OSSManage.GetDownUrl(ConfigManage.GetSetting("ossUrl"))}");
            Console.WriteLine("开始发布程序!");
            if (args?.Any() == true)
            {
                list.AddRange(args);
            }
            else
            {
                ConfigManage.BindInstance("servers", list);
            }
            Console.WriteLine($"处理配置列表：{string.Join(",", list)}");
            if (TaskProcess.CheckServerState(list))
            {
                TaskProcess.Process(list);
                stopwatch.Stop();
                Console.WriteLine($"整体耗费时间: {stopwatch.Elapsed.TotalSeconds} 秒!");
            }
            else
            {
                Console.WriteLine("服务地址没有连接成功,请注意查看服务状态!");
            }
            Console.ReadLine();
        }
    }
}
