using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPublishing.Common
{
    /// <summary>
    /// 压缩
    /// </summary>
    public interface ICompress
    {
        CompressType CompressType { get; }
        bool CompressDirectory(string directoryName, string CompressFile, string password = "");
        bool Decompression(string CompressFile, string targetAddress, string password = "");
    }
    public enum CompressType
    {
        System,
        SharpZip,
        R7Z
    }
}
