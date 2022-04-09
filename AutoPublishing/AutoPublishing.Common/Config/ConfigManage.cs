using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPublishing.Common
{
    /// <summary>
    /// 配置管理
    /// </summary>
    public static class ConfigManage
    {
        /// <summary>
        /// 配置对象
        /// </summary>
        private static IConfigurationRoot _Configuration;
        /// <summary>
        /// 锁
        /// </summary>
        private static object LockObject = new object();
        /// <summary>
        /// 配置内容
        /// </summary>
        public static IConfigurationRoot Configuration
        {
            get
            {
                if (_Configuration == null)
                {
                    lock (LockObject)
                    {
                        if (_Configuration == null)
                        {
                            _Configuration = new ConfigurationBuilder().SetBasePath(Environment.CurrentDirectory).AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();
                        }
                    }
                }
                return _Configuration;
            }
            set
            {
                _Configuration = value;
            }
        }
        /// <summary>
        /// 重新生成build
        /// </summary>
        public static void Reload()
        {
            Configuration?.Reload();
        }

        /// <summary>
        /// 配置 IConfiguration
        /// </summary>
        public static void Configure(IConfigurationRoot configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// 获取setting
        /// </summary>
        public static string GetSetting(string key)
        {
            return Configuration?.GetValue(key, "");
        }
        /// <summary>
        /// 获取数据库连接字符串
        /// </summary>
        public static string GetConnectionString(string name)
        {
            return Configuration?.GetConnectionString(name);
        }
        /// <summary>
        /// 绑定到实体 
        /// </summary>
        public static void BindInstance(string key, object instance)
        {
            Configuration?.GetSection(key).Bind(instance);
        }
    }
}
