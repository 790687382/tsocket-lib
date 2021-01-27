using System.Net;
using System.Net.Sockets;

namespace TSocket
{
    /// <summary>
    /// 接收数据参数
    /// </summary>
    public class DataReceivedArgs<TPackage>
    {
        /// <summary>
        /// 协议类型TCP\UDP
        /// </summary>
        public EnumNetworkType NetProtocolType { get; private set; }

        /// <summary>
        /// 本地地址
        /// </summary>
        public IPEndPoint LocalEP { get; private set; }

        /// <summary>
        /// 来源地址
        /// </summary>
        public IPEndPoint RemoteEP { get; private set; }

        /// <summary>
        /// 用户自定义实现包结构
        /// </summary>
        public TPackage Package { get; private set; }

        public DataReceivedArgs(EnumNetworkType type, IPEndPoint localEP, IPEndPoint remoteEP, TPackage package)
        {
            NetProtocolType = type;
            LocalEP = localEP;
            RemoteEP = remoteEP;
            Package = package;
        }
    }
}
