using AutoPublishing.Common;
using AutoPublishing.Model;
using AutoPublishing.ServerBLL;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AutoPublishing.OSSServer
{
    public class WorkServer : BackgroundService
    {
        public string RootPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Temp");
        public void CheckRootPath()
        {
            if (Directory.Exists(RootPath))
            {
                Directory.Delete(RootPath, true);
            }
            Directory.CreateDirectory(RootPath);
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            CheckRootPath();
            _ = Task.Run(() =>
              {
                  Heartbeat(stoppingToken);
              });
            _ = Task.Run(() =>
              {
                  ProcessTaskInfo(stoppingToken);
              });

            await Task.Run(async () =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, stoppingToken);
                }
            }, stoppingToken);
        }
        /// <summary>
        /// 处理任务,每5秒获取一次任务信息
        /// </summary>
        /// <param name="stoppingToken"></param>
        private void ProcessTaskInfo(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var list = OSSManage.GetMetaObjectList($"{ConfigManage.GetSetting("ossUrl")}/");
                var needProcess = new List<string>();
                if (list.Any() == true)
                {
                    foreach (var item in list.Where(t => t.path.Contains("zip")))
                    {
                        var jsonFile = $"{Path.GetFileNameWithoutExtension(item.path)}.json";
                        if (!list.Any(t => t.path == jsonFile))
                        {
                            if (!needProcess.Contains(item.path))
                            {
                                needProcess.Add(item.path);
                            }
                        }
                        else
                        {
                            //删除过期任务数据,超过三个小时，都算过期
                            if ((DateTime.Now - item.time).TotalHours >= 3)
                            {
                                Console.WriteLine($"过期时间:{(DateTime.Now - item.time).TotalHours},具体时间:{item.time}");
                                var zipFile = item.path;
                                DeleteFile(new List<string>() { zipFile, jsonFile });
                                Log.Info($"删除无效任务:{zipFile},{jsonFile}");
                            }
                        }
                    }
                }
                if (needProcess.Any())
                {
                    ProcessTask(needProcess);
                }
                SpinWait.SpinUntil(() => false, 5 * 1000);
            }
        }
        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="files"></param>
        private void DeleteFile(List<string> files)
        {
            foreach (var item in files)
            {
                OSSManage.DeleteObject(OSSManage.GetPathKey(ConfigManage.GetSetting("ossUrl"), item));
            }
        }
        /// <summary>
        /// 处理任务
        /// </summary>
        /// <param name="Tasks"></param>
        private void ProcessTask(List<string> Tasks)
        {
            CheckRootPath();
            foreach (var item in Tasks)
            {
                var fileName = Path.GetFileNameWithoutExtension(item);
                //保存文件
                var RFilePath = Path.Combine(RootPath, item);
                using var stream = OSSManage.GetObject(OSSManage.GetPathKey(ConfigManage.GetSetting("ossUrl"), item));
                byte[] buf = new byte[1024];
                var fs = File.Open(RFilePath, FileMode.OpenOrCreate);
                var len = 0;
                // 通过输入流将文件的内容读取到文件或者内存中。
                while ((len = stream.Read(buf, 0, 1024)) != 0)
                {
                    fs.Write(buf, 0, len);
                }
                fs.Close();

                var task = ServerProcess.ProcessTaskInfo(RootPath, RFilePath);
                var data = JsonConvert.SerializeObject(task);
                OSSManage.UploadString(ConfigManage.GetSetting("ossUrl"), $"{fileName}.json", data);
            }
        }
        /// <summary>
        /// 心跳
        /// 默认每5秒一次
        /// </summary>
        private static void Heartbeat(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var result = OSSManage.UploadString(ConfigManage.GetSetting("ossUrl"), "heartbeat.txt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                Console.Title = $"OSS Server :{DateTime.Now} 心跳 :{result.IsSuccess}";
                SpinWait.SpinUntil(() => false, 5 * 1000);
            }
        }
    }
}
