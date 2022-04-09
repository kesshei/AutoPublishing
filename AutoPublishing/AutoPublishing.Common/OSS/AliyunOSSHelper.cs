using Aliyun.OSS;
using AutoPublishing.Common.OOS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPublishing.Common
{
    public class AliyunOSSHelper
    {
        private OSSConfig oSSConfig;
        OssClient ossClient;
        public AliyunOSSHelper(OSSConfig oSSConfig)
        {
            this.oSSConfig = oSSConfig;
        }
        public AliyunOSSHelper(string endpoint, string accessKeyId, string accessKeySecret, string bucketName = null)
        {
            this.oSSConfig = new OSSConfig() { Endpoint = endpoint, AccessKeyId = accessKeyId, AccessKeySecret = accessKeySecret, BucketName = bucketName };
        }
        private OssClient GetOssClient()
        {
            if (ossClient == null)
            {
                ossClient = new OssClient(this.oSSConfig.Endpoint, this.oSSConfig.AccessKeyId, this.oSSConfig.AccessKeySecret);
            }
            return ossClient;
        }
        public void UseBucketName(string bucketName)
        {
            if (string.IsNullOrWhiteSpace(bucketName))
            {
                throw new Exception("bucketName is empty!");
            }
            if (!GetBucketList().Contains(bucketName))
            {
                throw new Exception("bucketName is not exsit!");
            }
            this.oSSConfig.BucketName = bucketName;
        }
        public List<string> GetBucketList()
        {
            var client = GetOssClient();
            return client.ListBuckets().Select(s => s.Name).ToList();
        }
        public bool CreateBucket(string bucketName)
        {
            bool IsTrue = false;
            if (!GetBucketList().Contains(bucketName))
            {
                var client = GetOssClient();
                var result = client.CreateBucket(bucketName);
                if (result.Name == bucketName)
                {
                    IsTrue = true;
                }
            }
            return IsTrue;
        }
        public void DeleteBucket(string bucketName)
        {
            var client = GetOssClient();
            client.DeleteBucket(bucketName);
        }
        public bool PutObject(string key, Stream content)
        {
            var client = GetOssClient();
            var result = client.PutObject(this.oSSConfig.BucketName, key, content);
            return result.HttpStatusCode == System.Net.HttpStatusCode.OK;
        }
        public bool PutObject(string key, string filePath)
        {
            var client = GetOssClient();
            var result = client.PutObject(this.oSSConfig.BucketName, key, filePath);
            return result.HttpStatusCode == System.Net.HttpStatusCode.OK;
        }
        public Stream GetObject(string key)
        {
            var client = GetOssClient();
            var result = client.GetObject(this.oSSConfig.BucketName, key);
            return result.ResponseStream;
        }
        public void DeleteObject(string key)
        {
            var client = GetOssClient();
            client.DeleteObject(this.oSSConfig.BucketName, key);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path">fun/</param>
        /// <returns></returns>
        public List<string> GetObjectList(string path)
        {
            var client = GetOssClient();
            ListObjectsRequest listObjectsRequest = new ListObjectsRequest(this.oSSConfig.BucketName);
            // "/" 为文件夹的分隔符
            listObjectsRequest.Delimiter = "/";
            // 列出fun目录下的所有文件和文件夹
            listObjectsRequest.Prefix = path;
            var listResult = client.ListObjects(listObjectsRequest);
            return listResult.ObjectSummaries.Select(s => s.Key.Replace(path, "")).ToList();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path">fun/</param>
        /// <returns></returns>
        public List<(string path, DateTime time)> GetMetaObjectList(string path)
        {
            var client = GetOssClient();
            ListObjectsRequest listObjectsRequest = new ListObjectsRequest(this.oSSConfig.BucketName);
            // "/" 为文件夹的分隔符
            listObjectsRequest.Delimiter = "/";
            // 列出fun目录下的所有文件和文件夹
            listObjectsRequest.Prefix = path;
            var listResult = client.ListObjects(listObjectsRequest);
            return listResult.ObjectSummaries.Select(s => (s.Key.Replace(path, ""), s.LastModified.ToLocalTime())).ToList();
        }
        public List<string> GetObjectList()
        {
            var client = GetOssClient();
            var listResult = client.ListObjects(this.oSSConfig.BucketName);
            return listResult.ObjectSummaries.Select(s => s.Key).ToList();
        }
        public List<string> GetObjectDownUrlList()
        {
            var client = GetOssClient();
            var listResult = client.ListObjects(this.oSSConfig.BucketName);
            return listResult.ObjectSummaries.Select(s => GetDownUrl(s.Key)).ToList();
        }
        public string GetDownUrl(string key, string CDNDomin = null)
        {
            if (!string.IsNullOrWhiteSpace(CDNDomin))
            {
                return $"{CDNDomin}/{key}";
            }
            else
            {
                return $"https://{this.oSSConfig.BucketName}.{this.oSSConfig.Endpoint}/{key}";
            }
        }
    }
}
