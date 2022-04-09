using AutoPublishing.Model;

namespace AutoPublishing.Client.Publish
{
    /// <summary>
    /// 发布服务接口
    /// </summary>
    public interface IPublishServer
    {
        /// <summary>
        /// 发布接口
        /// </summary>
        /// <param name="zipFilePath">压缩文件地址</param>
        TaskInfo Publish(string zipFilePath);
    }
}
