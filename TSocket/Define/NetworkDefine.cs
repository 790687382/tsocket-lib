
namespace TSocket
{
    /// <summary>
    /// 定义的一些常用配置
    /// </summary>
    public class NetworkGlobal
    {
        public const int TCP_SOCKET_RECEIVE_SIZE = 64 * 1024;
        public const int TCP_SOCKET_SEND_SIZE = 64 * 1024;
        public const int TCP_RECEIVE_BUFFER_SIZE = 64 * 1024;

        public const int UDP_SOCKET_RECEIVE_SIZE = 1024  * 128;
        public const int UDP_SOCKET_SEND_SIZE = 1024  * 128;
        public const int UDP_RECEIVE_BUFFER_SIZE = 64 * 1024;

        //Socket自带的心跳上报间隔（ms）
        public const int HEART_INTERVAL = 10000;
        //TCP连接超时时间（秒）
        public const int CONNECTION_TIMEOUT = 5;
    }

    /// <summary>
    /// socket的网络状态
    /// </summary>
    public enum EnumNetworkStatus
    {
        /// <summary>
        /// 初始状态
        /// </summary>
        Undefined = 0,
        /// <summary>
        /// socket构建中状态
        /// </summary>
        Establishing,
        /// <summary>
        /// socket正常状态
        /// </summary>
        Established,
        /// <summary>
        /// socket关闭状态
        /// </summary>
        Shutdown,
    }

    /// <summary>
    /// 网络协议类型
    /// </summary>
    public enum EnumNetworkType
    {
        Unknown = 0,
        TCP,
        UDP,
        /// <summary>
        /// 组播，用于组播收发
        /// </summary>
        Multicast,
    }
}
