using AutoPublishing.Model;
using AutoPublishing.ServerBLL;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;

namespace AutoPublishing.Server.Controllers
{
    public class FileController : Controller
    {
        public string RootPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Temp");
        public FileController()
        {
            CheckRootPath();
        }
        public void CheckRootPath()
        {
            if (Directory.Exists(RootPath))
            {
                Directory.Delete(RootPath, true);
            }
            Directory.CreateDirectory(RootPath);
        }
        [HttpPost]
        public IActionResult Upload(List<IFormFile> files)
        {
            CheckRootPath();
            var data = new List<TaskInfo>();
            foreach (var formFile in files)
            {
                if (formFile.Length > 0)
                {
                    Console.WriteLine(Environment.NewLine);
                    try
                    {
                        var tempName = formFile.FileName;
                        Log.Info($"开始接收上传文件:{tempName}");
                        var RFilePath = Path.Combine(RootPath, tempName);

                        //保存文件
                        var stream = formFile.OpenReadStream();
                        // 把 Stream 转换成 byte[] 
                        byte[] bytes = new byte[stream.Length];
                        stream.Read(bytes, 0, bytes.Length);
                        // 设置当前流的位置为流的开始 
                        stream.Seek(0, SeekOrigin.Begin);
                        // 把 byte[] 写入文件 
                        FileStream fs = new(RFilePath, FileMode.Create);
                        BinaryWriter bw = new(fs);
                        bw.Write(bytes);
                        bw.Close();
                        fs.Close();

                        var task = ServerProcess.ProcessTaskInfo(RootPath, RFilePath);
                        data.Add(task);
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                    }
                }
            }
            return Ok(data);
        }
    }
}
