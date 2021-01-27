using System;
using System.Net;

namespace TSocket
{
    /// <summary>
    /// Socket Client通信接口
    /// </summary>
    /// <typeparam name="TPackage"></typeparam>
    public interface ISocketNetClient<TPackage> : IDisposable
    {
        /// <summary>
        /// 收到数据事件
        /// </summary>
        event Action<object, DataReceivedArgs<TPackage>> OnDataReceivedEvent;
        /// <summary>
        /// 通信异常事件上报
        /// </summary>
        event Action<object, ExceptionHappenedArgs> OnExceptionHappenedEvent;
        /// <summary>
        /// 网络状态事件
        /// </summary>
        event Action<object, SocketStatusChangedArgs<TPackage>> OnSocketStatusChangedEvent;


        /// <summary>
        /// 网络状态
        /// </summary>
        EnumNetworkStatus SocketStatus { get; }
        /// <summary>
        /// 本地绑定地址
        /// </summary>
        IPEndPoint LocalEndPoint { get; }
        /// <summary>
        /// 远端地址
        /// TCP通信时，为服务端地址
        /// UDP通信时，可为null,不为null则是默认的接收地址
        /// 组播通信时，为组播地址
        /// </summary>
        IPEndPoint RemoteEndPoint { get; }
        /// <summary>
        /// 网络类型
        /// </summary>
        EnumNetworkType NetProtocolType { get; }
        /// <summary>
        /// 是否完全销毁
        /// </summary>
        bool Disposed { get; }


        /// <summary>
        /// 创建网络，异常抛出，具体根基协议类型EnumNetworkType来调用和区分
        /// </summary>
        /// <param name="localEP">本地绑定地址</param>
        /// <param name="remoteEP">远端地址，参考RemoteEndPoint的注释</param>
        void Create(IPEndPoint localEP, IPEndPoint remoteEP);
        /// <summary>
        /// 本地地址默认自动分配
        /// </summary>
        /// <param name="remoteEP">远端地址，参考RemoteEndPoint的注释</param>
        void Create(IPEndPoint remoteEP);
        /// <summary>
        /// 使用已有的参数创建，通常断线重连时使用
        /// </summary>
        void Create();
        /// <summary>
        /// 断开网络
        /// </summary>
        void Destroy();


        /// <summary>
        /// 获取解包对象
        /// </summary>
        /// <returns></returns>
        object GetPackageBuilder();

        #region TCP发送
        int Send(byte[] data, bool encode = true, object userTag = null);
        /// <summary>
        /// TCP发送数据
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="offset">数据偏移</param>
        /// <param name="length">有效数据长度</param>
        /// <param name="encode">是否编码：不编码直接发送(也许外面已经协议编码完成)；编码则调用AbsProtocolBuilder的编码函数,用于添加协议头等</param>
        /// <param name="userTag">通常用于编码时比较特殊的自定义信息</param>
        /// <returns></returns>
        int Send(byte[] data, int offset, int length, bool encode = true, object userTag = null);

        void BeginSend(byte[] data, bool encode = true, object userTag = null);
        /// <summary>
        /// 参考 Send()
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="encode"></param>
        /// <param name="userTag"></param>
        void BeginSend(byte[] data, int offset, int length, bool encode = true, object userTag = null);
        #endregion


        #region UDP发送
        int SendTo(byte[] buffer, bool encode = true, object userTag = null);
        int SendTo(byte[] buffer, int offset, int length, bool encode = true, object userTag = null);
        int SendTo(byte[] buffer, IPEndPoint remoteEP, bool encode = true, object userTag = null);
        /// <summary>
        /// 参考 Send()
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="remoteEP">UDP接收地址</param>
        /// <param name="encode"></param>
        /// <param name="userTag"></param>
        /// <returns></returns>
        int SendTo(byte[] buffer, int offset, int length, EndPoint remoteEP, bool encode = true, object userTag = null);

        void BeginSendTo(byte[] buffer, bool encode = true, object userTag = null);
        void BeginSendTo(byte[] buffer, int offset, int length, bool encode = true, object userTag = null);
        void BeginSendTo(byte[] buffer, IPEndPoint remoteEP, bool encode = true, object userTag = null);
        /// <summary>
        /// 参考 Send()
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="remoteEP">UDP接收地址</param>
        /// <param name="encode"></param>
        /// <param name="userTag"></param>
        void BeginSendTo(byte[] buffer, int offset, int length, IPEndPoint remoteEP, bool encode = true, object userTag = null);
        #endregion
    }
}
