using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPublishing.Common
{
    public class ZipFileHelper : ICompress
    {
        public static ZipFileHelper Instance = new ZipFileHelper();

        public CompressType CompressType { get { return CompressType.System; } }

        public bool CompressDirectory(string directoryName, string CompressFile, string password = "")
        {
            try
            {
                ZipFile.CreateFromDirectory(directoryName, CompressFile, CompressionLevel.Fastest, true);
                return true;
            }
            catch (Exception)
            {
            }
            return false;
        }

        public bool Decompression(string CompressFile, string targetAddress, string password = "")
        {
            try
            {
                ZipFile.ExtractToDirectory(CompressFile, targetAddress, true);
                return true;
            }
            catch (Exception)
            {
            }
            return false;
        }
    }
}
