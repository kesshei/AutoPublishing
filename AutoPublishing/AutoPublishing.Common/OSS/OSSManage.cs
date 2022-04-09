using AutoPublishing.Common.OOS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPublishing.Common
{
    public static class OSSManage
    {
        private static AliyunOSSHelper aliyunOSSHelper;
        public static AliyunOSSHelper OSS
        {
            get
            {
                if (aliyunOSSHelper == null)
                {
                    var config = new OSSConfig();
                    ConfigManage.BindInstance("AliOSSConfig", config);
                    aliyunOSSHelper = new AliyunOSSHelper(config);
                }
                return aliyunOSSHelper;
            }
        }
        public static List<string> GetBucketList()
        {
            return OSS.GetBucketList();
        }
        public static bool PutObject(string key, Stream content)
        {
            return OSS.PutObject(key, content);
        }
        public static bool PutObject(string key, string filePath)
        {
            return OSS.PutObject(key, filePath);
        }
        public static Stream GetObject(string key)
        {
            return OSS.GetObject(key);
        }
        public static string GetObjectData(string key)
        {
            return new StreamReader(OSS.GetObject(key), Encoding.UTF8).ReadToEnd();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path">fun/</param>
        /// <returns></returns>
        public static List<(string path, DateTime time)> GetMetaObjectList(string path)
        {
            return OSS.GetMetaObjectList(path);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path">fun/</param>
        /// <returns></returns>
        public static List<string> GetObjectList(string path)
        {
            return OSS.GetObjectList(path);
        }
        public static List<string> GetObjectList()
        {
            return OSS.GetObjectList();
        }
        public static List<string> GetOSSFloderList()
        {
            var list = OSS.GetObjectList().SelectMany(t => GetAllDirectoryName(t)).Select(t => t.Replace("\\", "/")).Where(t => !string.IsNullOrEmpty(t)).Distinct().ToList();
            list.Sort();
            return list;
        }
        private static List<string> GetAllDirectoryName(string path)
        {
            var list = new List<string>();
            while (!string.IsNullOrEmpty(path))
            {
                path = Path.GetDirectoryName(path);
                list.Add(path);
            }
            return list;
        }
        public static List<string> GetObjectDownUrlList()
        {
            return OSS.GetObjectDownUrlList();
        }
        public static string GetDownUrl(string key, string CDNDomin = null)
        {
            return OSS.GetDownUrl(key, CDNDomin);
        }
        public static (bool IsSuccess, string Url) UploadFile(string OSSPath, string OSSFileName, string FileName, string CDNDomin = null)
        {
            var key = $"{OSSPath}/{OSSFileName}";
            string url = null;
            var IsTrue = PutObject(key, FileName);
            if (IsTrue)
            {
                url = GetDownUrl(key, CDNDomin);
            }
            return (IsTrue, url);
        }
        public static (bool IsSuccess, string Url) UploadString(string OSSPath, string OSSFileName, string data)
        {
            var key = $"{OSSPath}/{OSSFileName}";
            string url = null;
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream, Encoding.UTF8);
            writer.Write(data);
            writer.Flush();
            stream.Seek(0, SeekOrigin.Begin);
            var IsTrue = PutObject(key, stream);
            if (IsTrue)
            {
                url = GetDownUrl(key);
            }
            return (IsTrue, url);
        }
        public static string GetPathKey(string OSSPath, string OSSFileName)
        {
            return $"{OSSPath}/{OSSFileName}";
        }
        /// <summary>
        /// 删除对象
        /// </summary>
        /// <param name="key"></param>
        public static void DeleteObject(string key)
        {
            OSS.DeleteObject(key);
        }
    }
}
