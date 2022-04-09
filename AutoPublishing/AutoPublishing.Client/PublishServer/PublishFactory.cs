
using AutoPublishing.Client.Publish;
using AutoPublishing.Model;

namespace AutoPublishing.Client.PublishServer
{
    /// <summary>
    /// 发布工厂
    /// </summary>
    public static class PublishFactory
    {
        /// <summary>
        /// 获取发布服务
        /// </summary>
        /// <param name="NetType"></param>
        /// <returns></returns>
        public static IPublishServer GetPublishServer(NetType NetType)
        {
            IPublishServer publishServer = null;
            switch (NetType)
            {
                case NetType.Web:
                    publishServer = new WebServer();
                    break;
                case NetType.OSS:
                    publishServer = new OSSServer();
                    break;
            }
            return publishServer;
        }
    }
}
