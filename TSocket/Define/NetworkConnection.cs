using System.Net;

namespace TSocket
{
    /// <summary>
    /// 连接信息
    /// </summary>
    public class NetworkConnection<TPackage>
    {
        /// <summary>
        /// 客户端地址
        /// </summary>
        public IPEndPoint RemoteEP { get; private set; }
        /// <summary>
        /// 客户端通信接口
        /// </summary>
        public ISocketNetClient<TPackage> Target { get; private set; }

        public NetworkConnection(IPEndPoint ep, ISocketNetClient<TPackage> target)
        {
            RemoteEP = ep;
            Target = target;
        }
    }
}
