using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPublishing.Common.OOS
{
    public class OSSConfig
    {
        public string Endpoint  { get;set;}
        public string AccessKeyId { get; set; }
        public string AccessKeySecret { get; set; }
        public string BucketName { get; set; }  
    }
}
