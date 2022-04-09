using AutoPublishing.Client.Publish;
using AutoPublishing.Common;
using AutoPublishing.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AutoPublishing.Client
{
    /// <summary>
    /// web服务发送
    /// </summary>
    public class WebServer : IPublishServer
    {
        public TaskInfo Publish(string zipFilePath)
        {
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 10, 0);
                var multipartFormDataContent = new MultipartFormDataContent()
                    {
                        {
                            new ByteArrayContent(File.ReadAllBytes(zipFilePath)),    // 文件流
                            "files",                                                 // 对应 服务器 WebAPI 的传入参数
                            Path.GetFileName(zipFilePath)                            // 上传的文件名称
                        }
                    };
                var url = $"{ConfigManage.GetSetting("serverUrl")}/File/Upload";
                HttpResponseMessage response = client.PostAsync(url, multipartFormDataContent).Result;
                if (response.IsSuccessStatusCode)
                {
                    var result = response.Content.ReadAsStringAsync().Result;
                    return JsonConvert.DeserializeObject<List<TaskInfo>>(result).FirstOrDefault();
                }
            }
            return null;
        }
    }
}
