using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AutoPublishing.Model
{
    /// <summary>
    /// 远程服务类型
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum NetType
    {
        /// <summary>
        /// web服务
        /// </summary>
        Web,
        /// <summary>
        /// oss对象存储服务
        /// </summary>
        OSS
    }
}
