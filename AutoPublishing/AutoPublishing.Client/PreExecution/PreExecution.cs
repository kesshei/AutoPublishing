using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AutoPublishing.Client
{
    /// <summary>
    /// 预处理
    /// </summary>
    public static class PreProcess
    {
        /// <summary>
        /// 预处理
        /// </summary>
        /// <param name="name"></param>
        public static void Process(string name = "")
        {
            var PreExecutions = PreExecution.LoadConfig();
            var ProjectTypeProcesss = ProjectTypeProcess.LoadConfig();
            if (!string.IsNullOrEmpty(name))
            {
                PreExecutions = PreExecutions.Where(t => t.CMDName == name).ToList();
            }
            foreach (var item in PreExecutions)
            {
                foreach (var process in ProjectTypeProcesss.Where(t => t.ProjectType == item.ProjectType).ToList())
                {
                    process.Run(item);
                }
            }
        }
    }

    /// <summary>
    /// 预执行命令
    /// git 或 dotnet
    /// </summary>
    public class PreExecution
    {
        /// <summary>
        /// 命令名称
        /// </summary>
        public string CMDName { get; set; }
        /// <summary>
        /// 项目类型
        /// </summary>
        public ProjectType ProjectType { get; set; }
        /// <summary>
        /// 项目主目录
        /// </summary>
        public string ProjectDirectory { get; set; }
        private static object LockObject = new object();
        /// <summary>
        /// 加载配置列表
        /// </summary>
        public static List<PreExecution> LoadConfig()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", $"{nameof(PreExecution)}s.json");
            if (File.Exists(path))
            {
                string data;
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        data = sr.ReadToEnd();
                    }
                }
                var config = JsonConvert.DeserializeObject<List<PreExecution>>(data);
                return config;
            }
            else
            {
                var emptyConfig = new List<PreExecution>() { new PreExecution() };
                SaveConfig(emptyConfig);
                return emptyConfig;
            }
        }
        /// <summary>
        /// 保存任务列表
        /// </summary>
        public static void SaveConfig(List<PreExecution> tableTos)
        {
            lock (LockObject)
            {
                var data = JsonConvert.SerializeObject(tableTos, new JsonSerializerSettings() { DateFormatString = "yyyy-MM-dd HH:mm:ss" });
                var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config");
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", $"{nameof(PreExecution)}s.json");
                File.WriteAllText(path, data, Encoding.UTF8);
            }
        }
    }
    public class ProjectTypeProcess
    {
        /// <summary>
        /// 项目类型
        /// </summary>
        public ProjectType ProjectType { get; set; }
        /// <summary>
        /// 命令列表
        /// </summary>
        public List<string> CMDs { get; } = new List<string>();
        public void Run(PreExecution preExecution)
        {
            foreach (var cmd in CMDs)
            {
                ProcessRun(preExecution.ProjectDirectory, cmd);
            }
        }
        private void ProcessRun(string WorkingDirectory, string CMD)
        {
            var process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.CreateNoWindow = true;         // 不创建新窗口    
            process.StartInfo.UseShellExecute = false;       //不启用shell启动进程  
            process.StartInfo.RedirectStandardOutput = true; // 重定向标准输出    
            process.StartInfo.RedirectStandardError = false; //重定向标准错误
            process.StartInfo.WorkingDirectory = WorkingDirectory;  //定义执行的路径 
            process.StartInfo.Arguments = $"/c {CMD}";

            try
            {
                process.Start();
                while (!process.StandardOutput.EndOfStream)
                {
                    Console.WriteLine(process.StandardOutput.ReadLine());
                }
                process.WaitForExit();
                process.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        private static object LockObject = new object();
        /// <summary>
        /// 加载配置列表
        /// </summary>
        public static List<ProjectTypeProcess> LoadConfig()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", $"{nameof(ProjectTypeProcess)}s.json");
            if (File.Exists(path))
            {
                string data;
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        data = sr.ReadToEnd();
                    }
                }
                var config = JsonConvert.DeserializeObject<List<ProjectTypeProcess>>(data);
                return config;
            }
            else
            {
                var emptyConfig = new List<ProjectTypeProcess>() { new ProjectTypeProcess() };
                SaveConfig(emptyConfig);
                return emptyConfig;
            }
        }
        /// <summary>
        /// 保存任务列表
        /// </summary>
        public static void SaveConfig(List<ProjectTypeProcess> tableTos)
        {
            lock (LockObject)
            {
                var data = JsonConvert.SerializeObject(tableTos, new JsonSerializerSettings() { DateFormatString = "yyyy-MM-dd HH:mm:ss" });
                var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config");
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", $"{nameof(ProjectTypeProcess)}s.json");
                File.WriteAllText(path, data, Encoding.UTF8);
            }
        }
    }
    /// <summary>
    /// 项目类型
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ProjectType
    {
        /// <summary>
        /// dotNet
        /// </summary>
        DotNet
    }
}
