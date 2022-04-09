using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AutoPublishing.Model
{
    /// <summary>
    /// 任务体
    /// </summary>
    public class TaskInfo
    {
        public string ID { get; set; }
        /// <summary>
        /// 任务名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 预执行命令
        /// </summary>
        public string CMDName { get; set; }
        /// <summary>
        /// 网络服务
        /// </summary>
        public NetType NetType { get; set; }
        /// <summary>
        /// 客户端主目录地址
        /// </summary>
        public string LocalRootDir { get; set; }
        /// <summary>
        /// 服务端主目录地址
        /// </summary>
        public string ServerRootDir { get; set; }
        /// <summary>
        /// 服务信息
        /// </summary>
        public List<ServerInfo> ServerInfos { get; set; } = new List<ServerInfo>();
        /// <summary>
        /// 不包含的文件名
        /// </summary>
        public List<string> NotIncluded { get; set; } = new List<string>();
        public bool IsComplete { get; set; }
        /// <summary>
        /// 加载配置列表
        /// </summary>
        public static TaskInfo Load(string path)
        {
            if (File.Exists(path))
            {
                string data;
                using (FileStream fs = new(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using StreamReader sr = new(fs);
                    data = sr.ReadToEnd();
                }
                var config = JsonConvert.DeserializeObject<TaskInfo>(data);
                return config;
            }
            return null;
        }
        /// <summary>
        /// 保存任务列表
        /// </summary>
        public void Save(string path)
        {
            var data = JsonConvert.SerializeObject(this, new JsonSerializerSettings() { DateFormatString = "yyyy-MM-dd HH:mm:ss" });
            File.WriteAllText(path, data, Encoding.UTF8);
        }
    }
    /// <summary>
    /// 服务信息
    /// </summary>
    public class ServerInfo
    {
        /// <summary>
        /// 相对主目录的执行路径
        /// </summary>
        public string ExePath { get; set; }
        public bool IsClose { get; set; }
        public bool IsStart { get; set; }
    }
}
