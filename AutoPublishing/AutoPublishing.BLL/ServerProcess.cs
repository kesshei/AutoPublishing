using AutoPublishing.Common;
using AutoPublishing.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AutoPublishing.ServerBLL
{
    /// <summary>
    /// 服务处理
    /// 通过对压缩包进行处理
    /// </summary>
    public static class ServerProcess
    {
        /// <summary>
        /// 处理任务信息
        /// </summary>
        /// <param name="RootPath">本地根路径</param>
        /// <param name="zipFilePath">压缩文件实际地址</param>
        /// <returns></returns>
        public static TaskInfo ProcessTaskInfo(string RootPath, string zipFilePath)
        {
            var DirPath = Path.Combine(RootPath, $"{Path.GetFileNameWithoutExtension(zipFilePath)}");
            var result = ZipFileHelper.Instance.Decompression(zipFilePath, RootPath);
            if (result)
            {
                Log.Info($"解压成功:{DirPath}");
                var taskInfo = TaskInfo.Load(Path.Combine(DirPath, "TaskInfo.json"));
                if (taskInfo != null)
                {
                    Log.Info($"任务 {taskInfo.Name}:{taskInfo.ID} 开始处理");
                    taskInfo.IsComplete = Publishing(taskInfo, DirPath);
                    Log.Info($"任务 {taskInfo.Name}:{taskInfo.ID} 处理状态:{ taskInfo.IsComplete}");
                    try
                    {
                        if (Directory.Exists(DirPath))
                        {
                            Directory.Delete(DirPath, true);
                        }
                        if (File.Exists(zipFilePath))
                        {
                            File.Delete(zipFilePath);
                        }
                    }
                    catch (Exception)
                    {
                    }
                    return taskInfo;
                }
                else
                {
                    Log.Info($"没法加载任务信息:{DirPath}");
                }
            }
            return new TaskInfo();
        }
        private static bool Publishing(TaskInfo taskInfo, string realPath)
        {
            if (taskInfo == null)
            {
                return false;
            }
            if (!Directory.Exists(taskInfo.ServerRootDir))
            {
                return false;
            }
            var ListState = new List<bool>();

            //先关闭服务
            foreach (var item in taskInfo.ServerInfos)
            {
                item.IsClose = CloseProcess(taskInfo, item);
                ListState.Add(item.IsClose);
            }
            //然后，覆盖文件
            var result = CoverFile(taskInfo, realPath);

            //然后启动服务
            foreach (var item in taskInfo.ServerInfos)
            {
                item.IsStart = StartProcess(taskInfo, item);
                ListState.Add(item.IsStart);
            }
            return !ListState.Any(t => t == false);
        }
        public static bool CloseProcess(TaskInfo taskInfo, ServerInfo serverInfo)
        {

            var p = GetProcessByAddress(taskInfo, serverInfo);
            if (p != null)
            {
                try
                {
                    p?.Kill();
                }
                catch { }
            }
            else
            {
                return true;
            }
            Thread.Sleep(1000);
            return CloseProcess(taskInfo, serverInfo);
        }
        /// <summary>
        /// 根据路径获取进程
        /// </summary>
        public static Process GetProcessByAddress(TaskInfo taskInfo, ServerInfo serverInfo)
        {
            var exeFullPath = Path.Combine(taskInfo.ServerRootDir, serverInfo.ExePath);
            var name = Path.GetFileNameWithoutExtension(exeFullPath);
            var p = Process.GetProcessesByName(name).Where(t => t.MainModule.FileName == exeFullPath);
            if (p?.Any() == true)
            {
                return p.First();
            }
            return null;
        }
        /// <summary>
        /// 覆盖文件
        /// </summary>
        public static bool CoverFile(TaskInfo taskInfo, string realPath)
        {
            var sourceDire = Path.Combine(realPath, "Data");
            if (Directory.Exists(sourceDire) && Directory.Exists(taskInfo.ServerRootDir))
            {
                DirectoryInfo sourceDireInfo = new(sourceDire);
                FileInfo[] fileInfos = sourceDireInfo.GetFiles("*", SearchOption.AllDirectories);
                foreach (FileInfo fInfo in fileInfos)
                {
                    //如果不包含的不替换
                    if (taskInfo.NotIncluded.Contains(fInfo.Name))
                    {
                        continue;
                    }
                    string sourceFile = fInfo.FullName;
                    string destFile = sourceFile.Replace(sourceDire, taskInfo.ServerRootDir);

                    var targetDir = new FileInfo(destFile).DirectoryName;
                    if (!Directory.Exists(targetDir))
                    {
                        Directory.CreateDirectory(targetDir);
                    }
                    System.IO.File.Copy(sourceFile, destFile, true);
                }
                return true;
            }
            return false;
        }
        /// <summary>
        /// 启动进程
        /// </summary>
        public static bool StartProcess(TaskInfo taskInfo, ServerInfo serverInfo)
        {
            try
            {
                var exeFullPath = Path.Combine(taskInfo.ServerRootDir, serverInfo.ExePath);
                ProcessStartInfo start = new ProcessStartInfo();
                start.WorkingDirectory = new FileInfo(exeFullPath).DirectoryName;
                start.WindowStyle = ProcessWindowStyle.Normal;
                start.UseShellExecute = true;
                start.FileName = exeFullPath;
                Process.Start(start);
                Thread.Sleep(1000);
                return GetProcessByAddress(taskInfo, serverInfo) != null;
            }
            catch (Exception)
            {
            }
            return false;
        }
    }
}
