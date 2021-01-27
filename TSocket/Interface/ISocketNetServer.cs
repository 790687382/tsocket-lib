using System;
using System.Collections.Generic;
using System.Net;

namespace TSocket
{
    /// <summary>
    /// TCP服务端接口
    /// </summary>
    /// <typeparam name="TPackage"></typeparam>
    public interface ISocketNetServer<TPackage> : IDisposable
    {
        /// <summary>
        /// 收到数据事件(完成协议解包)
        /// </summary>
        event Action<object, DataReceivedArgs<TPackage>> OnDataReceivedEvent;
        /// <summary>
        /// 内部异常事件通知
        /// </summary>
        event Action<object, ExceptionHappenedArgs> OnExceptionHappenedEvent;
        /// <summary>
        /// 客户端状态通知，通常只需要关注建立成功、关闭成功
        /// </summary>
        event Action<object, SocketStatusChangedArgs<TPackage>> OnClientSocketStatusChangedEvent;
        /// <summary>
        /// 服务端Socket的状态
        /// </summary>
        event Action<object, ListenStatusChangedArgs> OnListenStatusChangedEvent;


        /// <summary>
        /// 服务端Socket的状态
        /// </summary>
        bool IsListen { get; }
        /// <summary>
        /// 服务端Socket的本地监听地址
        /// </summary>
        IPEndPoint ListenEP { get; }


        /// <summary>
        /// 启动监听
        /// </summary>
        /// <param name="listenEP"></param>
        void Start(IPEndPoint listenEP);
        /// <summary>
        /// 停止监听，将销毁本地Socket和所有连接
        /// </summary>
        void Stop();


        /// <summary>
        /// 获取当前在线连接
        /// </summary>
        /// <returns></returns>
        IList<NetworkConnection<TPackage>> GetConnection();
        /// <summary>
        /// 获取指定地址的连接，失败返回null
        /// </summary>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        NetworkConnection<TPackage> GetConnection(IPEndPoint endpoint);
        /// <summary>
        /// 断开所有连接
        /// </summary>
        void Disconnect();
        /// <summary>
        /// 断开指定连接
        /// </summary>
        /// <param name="endpoint"></param>
        void Disconnect(IPEndPoint endpoint);

        /// <summary>
        /// 数据发送
        /// </summary>
        /// <param name="clientEP">客户端地址，如果为null则发送给所有连接</param>
        /// <param name="data">要发送的数据</param>
        /// <param name="isAsync">是否异步发送</param>
        /// <param name="encode">是否编码</param>
        /// <param name="userTag">传递给编码的自定义参数</param>
        void Send(IPEndPoint clientEP, byte[] data, bool isAsync = true, bool encode = true, object userTag = null);
        /// <summary>
        /// 数据发送
        /// </summary>
        /// <param name="clientEP">客户端地址，如果为null则发送给所有连接</param>
        /// <param name="data">要发送的数据</param>
        /// <param name="offset">数据偏移</param>
        /// <param name="length">数据有效长度</param>
        /// <param name="isAsync">是否异步发送</param>
        /// <param name="encode">是否编码</param>
        /// <param name="userTag">传递给编码的自定义参数</param>
        void Send(IPEndPoint clientEP, byte[] data, int offset, int length, bool isAsync = true, bool encode = true, object userTag = null);
    }
}
