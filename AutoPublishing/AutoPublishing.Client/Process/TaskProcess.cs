using AutoPublishing.Client.PublishServer;
using AutoPublishing.Common;
using AutoPublishing.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;

namespace AutoPublishing.Client
{
    /// <summary>
    /// 任务处理
    /// </summary>
    public class TaskProcess
    {
        private static List<TaskInfo> taskInfos = new List<TaskInfo>();
        /// <summary>
        /// 处理任务
        /// </summary>
        public static void Process(List<string> list)
        {
            var tasks = GetAllConfig().Where(t => list.Contains(t.Name)).ToList();
            if (tasks?.Any() == false)
            {
                return;
            }
            //预处理
            var names = tasks.Select(t => t.CMDName).Distinct().ToList();
            foreach (var item in names)
            {
                Console.WriteLine($"----- 预处理 {item}");
                PreProcess.Process(item);
                Console.WriteLine($"----- 预处理 {item} 处理完毕!");
            }
            foreach (var task in tasks)
            {
                task.ID = Guid.NewGuid().ToString("N");
                Console.WriteLine($"{task.Name} 开始同步服务");
                try
                {
                    var taskInfo = Publishing(task);
                    if (taskInfo?.IsComplete == true)
                    {
                        Console.WriteLine($"{task.Name} 同步成功!");
                    }
                    else
                    {
                        Console.WriteLine($"{task.Name} 同步失败!");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{task.Name} 同步失败:" + e.Message);
                }
            }
            Console.WriteLine("全部同步完毕!");
        }
        /// <summary>
        /// 发布
        /// </summary>
        /// <param name="taskInfo"></param>
        /// <returns></returns>
        static TaskInfo Publishing(TaskInfo taskInfo)
        {
            string tempPath = "";
            string R7zPath = "";
            try
            {
                var dic = FileCopyToTemp(taskInfo);
                dic.TryGetValue("tempPath", out tempPath);
                dic.TryGetValue("RPath", out R7zPath);

                return PublishFactory.GetPublishServer(taskInfo.NetType).Publish(R7zPath);
            }
            catch (Exception e)
            {
                Console.WriteLine($"服务发送异常:{e.Message}");
            }
            finally
            {
                if (Directory.Exists(tempPath))
                {
                    Directory.Delete(tempPath, true);
                }
                if (File.Exists(R7zPath))
                {
                    File.Delete(R7zPath);
                }
            }
            return null;
        }
        /// <summary>
        /// 文件拷贝到临时目录
        /// </summary>
        /// <param name="taskInfo"></param>
        /// <returns></returns>
        static Dictionary<string, string> FileCopyToTemp(TaskInfo taskInfo)
        {
            var dic = new Dictionary<string, string>();
            //压缩到临时文件
            var tempPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Temp", taskInfo.ID);
            if (!Directory.Exists(tempPath))
            {
                Directory.CreateDirectory(tempPath);
            }
            //复制任务
            var TaskInfoPath = Path.Combine(tempPath, "TaskInfo.json");
            taskInfo.Save(TaskInfoPath);
            //复制数据
            var dataPath = Path.Combine(tempPath, "Data");
            if (!Directory.Exists(dataPath))
            {
                Directory.CreateDirectory(dataPath);
            }
            var dir = new DirectoryInfo(taskInfo.LocalRootDir);
            foreach (var fileInfo in dir.GetFiles("*", SearchOption.AllDirectories))
            {
                //如果不包含的不替换
                if (taskInfo.NotIncluded.Contains(fileInfo.Name))
                {
                    continue;
                }
                var tempDir = fileInfo.FullName.Replace(taskInfo.LocalRootDir, dataPath);
                var targetDir = new FileInfo(tempDir).DirectoryName;
                if (!Directory.Exists(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                }
                File.Copy(fileInfo.FullName, tempDir, true);
            }
            //压缩为7z
            var fileName = $"{taskInfo.ID}.zip";
            var realPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Temp", fileName);
            ZipFileHelper.Instance.CompressDirectory(tempPath, realPath);
            dic.Add("tempPath", tempPath);
            dic.Add("RPath", realPath);
            return dic;
        }
        public static List<TaskInfo> GetAllConfig()
        {
            if (taskInfos?.Any() == false)
            {
                var root = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TaskInfo");
                var dir = new DirectoryInfo(root);
                var files = dir.GetFiles("*.json").Select(t => TaskInfo.Load(t.FullName));

                if (files?.Any() == false)
                {
                    Console.WriteLine($"从 {root} 没有加载到任务");
                }
                else
                {
                    taskInfos = files.ToList();
                }
            }
            return taskInfos;
        }
        /// <summary>
        /// 检查服务
        /// </summary>
        public static bool CheckServerState(List<string> names)
        {
            var tasks = GetAllConfig().Where(t => names.Contains(t.Name)).Select(t => t.NetType).ToList();
            //预处理
            var types = tasks?.Distinct();
            if (types?.Any() == true)
            {
                foreach (var item in types)
                {
                    if (!GetNetTypeServerState(item))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        /// <summary>
        /// 根据网络类型获取服务地址 
        /// </summary>
        public static bool GetNetTypeServerState(NetType netType)
        {
            /// <summary>
            /// 检查web服务状态
            /// </summary>
            /// <returns></returns>
            static bool CheckWEBServerState(string url)
            {
                using (var client = new HttpClient())
                {
                    HttpResponseMessage response = client.GetAsync(url).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        var result = response.Content.ReadAsStringAsync().Result;
                        return !string.IsNullOrEmpty(result);
                    }
                }
                return false;
            }
            /// <summary>
            /// 检查oss服务状态
            /// </summary>
            /// <returns></returns>
            static bool CheckOSSServerState(string url)
            {
                try
                {
                    var data = OSSManage.GetObjectData(url);
                    if (DateTime.TryParse(data, out var time))
                    {
                        if (Math.Abs((DateTime.Now - time).TotalSeconds) < 20)
                        {
                            return true;
                        }
                    }
                }
                catch (Exception)
                {
                }
                return false;
            }
            bool State = false;
            string url = null;
            switch (netType)
            {
                case NetType.Web:
                    url = $"{ConfigManage.GetSetting("serverUrl")}";
                    State = CheckWEBServerState(url);
                    break;
                case NetType.OSS:
                    url = OSSManage.GetPathKey(ConfigManage.GetSetting("ossUrl"), "heartbeat.txt");
                    State = CheckOSSServerState(url);
                    break;
            }
            return State;
        }
    }
}
