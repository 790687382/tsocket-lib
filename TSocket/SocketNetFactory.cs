/*
 * 备注:
 * 在网络通信中,对socket的操作都大同小异，但是协议千变万化，若耦合在通信里面，扩展性太差。
 * 这是一个超级轻量级的通信库，旨在方便创建网络通信和实现自定义协议处理~~~~
 * 
 * 使用这个库只需要：
 * 1.定义自己的解析结果包，即TPackage，建议您使用一个基类，不同的协议包继承自这个基类；
 * 2.定义自己的协议编解码对象：即TBuilder（继承AbsProtocolBuilder<TPackage>）,粘包处理在里面实现即可；
 * 3.通信对象创建好以后，实现几个事件函数；
 * 4.调用相关函数开始您的网络；
 * 
 * 问：TCP-Client通信如何断线重连？
 * 答：连接状态的断线事件通知里面重新调用Create()函数即可，具体参考demo
 * 
 * 
 */
namespace TSocket
{
    /// <summary>
    /// 通信接口创建
    /// </summary>
    public class SocketNetFactory
    {
        /// <summary>
        /// 创建TCP客户端
        /// </summary>
        /// <typeparam name="TBuilder">协议解包自定义实现</typeparam>
        /// <typeparam name="TPackage">自定义用户层数据包</typeparam>
        /// <returns></returns>
        public static ISocketNetClient<TPackage> CreateTCPClient<TBuilder, TPackage>() where TBuilder : AbsProtocolBuilder<TPackage>, new()
        {
            return CreateSocketNetClient<TBuilder, TPackage>(EnumNetworkType.TCP,
                                                            NetworkGlobal.TCP_RECEIVE_BUFFER_SIZE,
                                                            NetworkGlobal.TCP_SOCKET_RECEIVE_SIZE,
                                                            NetworkGlobal.TCP_SOCKET_SEND_SIZE);
        }

        /// <summary>
        /// 创建UDP客户端
        /// </summary>
        /// <typeparam name="TBuilder">协议解包自定义实现</typeparam>
        /// <typeparam name="TPackage">自定义用户层数据包</typeparam>
        /// <returns></returns>
        public static ISocketNetClient<TPackage> CreateUDPClient<TBuilder, TPackage>() where TBuilder : AbsProtocolBuilder<TPackage>, new()
        {
            return CreateSocketNetClient<TBuilder, TPackage>(EnumNetworkType.UDP,
                                                            NetworkGlobal.UDP_RECEIVE_BUFFER_SIZE,
                                                            NetworkGlobal.UDP_SOCKET_RECEIVE_SIZE,
                                                            NetworkGlobal.UDP_SOCKET_SEND_SIZE);
        }

        /// <summary>
        /// 创建TCP/UDP客户端，自定义收发缓冲区大小
        /// </summary>
        /// <typeparam name="TBuilder">协议解包自定义实现</typeparam>
        /// <typeparam name="TPackage">自定义用户层数据包</typeparam>
        /// <param name="type">TCP或者UDP</param>
        /// <param name="dataRecvSize">数据接收缓冲区大小</param>
        /// <param name="socketRecvSize">Socket接收缓冲区大小</param>
        /// <param name="socketSendSize">Socket发送缓冲区大小</param>
        /// <returns></returns>
        public static ISocketNetClient<TPackage> CreateSocketNetClient<TBuilder, TPackage>(EnumNetworkType type,
            int dataRecvSize,
            int socketRecvSize, 
            int socketSendSize) where TBuilder : AbsProtocolBuilder<TPackage>, new()
        {
            return new SocketNetClient<TBuilder, TPackage>(type, dataRecvSize, socketRecvSize, socketSendSize);
        }

        /// <summary>
        /// 创建TCP服务端
        /// </summary>
        /// <typeparam name="TBuilder">协议解包自定义实现</typeparam>
        /// <typeparam name="TPackage">自定义用户层数据包</typeparam>
        /// <returns></returns>
        public static ISocketNetServer<TPackage> CreateTCPServer<TBuilder, TPackage>() where TBuilder : AbsProtocolBuilder<TPackage>, new()
        {
            return new SocketNetServer<TBuilder, TPackage>();
        }
    }
}
