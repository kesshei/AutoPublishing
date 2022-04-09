using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPublishing.Model
{
    /// <summary>
    /// 公共简单的日志方法
    /// </summary>
    public static class Log
    {
        public static void Info(string msg)
        {
            Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} - {msg}");
        }   
        public static void Error(Exception exception, string msg = "")
        {
            Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} - {exception.Message}");
        }
    }
}
