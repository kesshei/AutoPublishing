using AutoPublishing.Client.Publish;
using AutoPublishing.Common;
using AutoPublishing.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace AutoPublishing.Client
{
    /// <summary>
    /// oss服务发送
    /// </summary>  
    public class OSSServer : IPublishServer
    {
        public TaskInfo Publish(string zipFilePath)
        {
            var fileName = Path.GetFileName(zipFilePath);
            try
            {
                var result = OSSManage.UploadFile(ConfigManage.GetSetting("ossUrl"), fileName, zipFilePath);
                if (result.IsSuccess)
                {
                    var task = GetTaskInfo(fileName);
                    return task;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                //移除无效的文件
                DeleteFile(new List<string>() { fileName, $"{Path.GetFileNameWithoutExtension(fileName)}.json" });
            }
            return null;
        }
        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="files"></param>
        private static void DeleteFile(List<string> files)
        {
            foreach (var item in files)
            {
                OSSManage.DeleteObject(OSSManage.GetPathKey(ConfigManage.GetSetting("ossUrl"), item));
            }
        }
        /// <summary>
        /// 获取任务信息
        /// </summary>
        public static TaskInfo GetTaskInfo(string fileName)
        {
            var startTime = DateTime.Now;
            while (true)
            {
                var task = CheckTaskInfo(fileName);
                if (task?.IsComplete == true)
                {
                    return task;
                }
                var nowTime = DateTime.Now;
                if ((nowTime - startTime).TotalMinutes >= 5)//5分钟
                {
                    break;
                }
                SpinWait.SpinUntil(() => false, 1000);
            }
            return null;
        }
        /// <summary>
        /// 检查任务信息
        /// </summary>
        public static TaskInfo CheckTaskInfo(string fileName)
        {
            return GetOSSInfo<TaskInfo>($"{Path.GetFileNameWithoutExtension(fileName)}.json");
        }
        /// <summary>
        /// 获取线上信息
        /// </summary>
        public static T GetOSSInfo<T>(string fileName)
        {
            try
            {
                var taskInfoPath = OSSManage.GetPathKey(ConfigManage.GetSetting("ossUrl"), fileName);
                var data = OSSManage.GetObjectData(taskInfoPath);
                return JsonConvert.DeserializeObject<T>(data);
            }
            catch (Exception)
            {
            }
            return default;
        }
        public static void ClearOSS()
        {
            var list = OSSManage.GetMetaObjectList($"{ConfigManage.GetSetting("ossUrl")}/");
            DeleteFile(list.Where(t => t.path != "heartbeat.txt").Select(t => t.path).ToList());
        }
    }
}
