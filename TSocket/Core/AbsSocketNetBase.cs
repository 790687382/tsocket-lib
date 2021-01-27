using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace TSocket
{
    /// <summary>
    /// 套接字通信基类
    /// </summary>
    abstract class AbsSocketNetBase<TPackage> : ISocketNetClient<TPackage>
    {
        bool m_disposed = false;
        IPEndPoint m_localEndPoint;
        IPEndPoint m_remoteEndPoint;
        int m_socketStatus = (int)EnumNetworkStatus.Undefined;
        Socket m_socket;
        byte[] m_dataBuffer;
        EnumNetworkType m_netProtocolType = EnumNetworkType.Unknown;

        //接收异步操作对象：注意创建、启动接收、完成接收的设置区分
        SocketAsyncEventArgs m_recvEventArgs;
        int m_dataBufferSize = 64 * 1024;
        int m_socketRecvBufferSize = 64 * 1024;
        int m_socketSendBufferSize = 64 * 1024;

        public event Action<object, DataReceivedArgs<TPackage>> OnDataReceivedEvent;
        public event Action<object, ExceptionHappenedArgs> OnExceptionHappenedEvent;
        public event Action<object, SocketStatusChangedArgs<TPackage>> OnSocketStatusChangedEvent;

        #region 属性
        public EnumNetworkStatus SocketStatus
        {
            get { return (EnumNetworkStatus)m_socketStatus; }
        }

        public IPEndPoint LocalEndPoint
        {
            get { return m_localEndPoint; }
        }

        public IPEndPoint RemoteEndPoint
        {
            get { return m_remoteEndPoint; }
        }

        public EnumNetworkType NetProtocolType
        {
            get { return m_netProtocolType; }
        }

        public bool Disposed
        {
            get { return m_disposed; }
        }
        #endregion

        /// <summary>
        /// 实现发送自定义编码。发送函数的encode参数为true
        /// </summary>
        /// <param name="src">待编码的数据</param>
        /// <param name="srcOffset"></param>
        /// <param name="srcLength"></param>
        /// <param name="dest">编码结果，用于网络发送</param>
        /// <param name="destOffset"></param>
        /// <param name="destLength"></param>
        /// <param name="userTag">编码时可能用到的附加参数</param>
        /// <returns></returns>
        protected abstract bool DataEncode(byte[] src, int srcOffset, int srcLength, ref byte[] dest, ref int destOffset, ref int destLength, object userTag = null);
        /// <summary>
        /// 通知子类异常，通常仅仅用于日志记录
        /// </summary>
        /// <param name="description"></param>
        /// <param name="ex"></param>
        protected abstract void SocketExceptionHappened(string description, Exception ex);
        /// <summary>
        /// 通知子类收到数据
        /// </summary>
        /// <param name="type"></param>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="remoteEP"></param>
        protected abstract void SocketDataReceived(EnumNetworkType type, byte[] data, int offset, int length, IPEndPoint remoteEP);
        /// <summary>
        /// 通知子类状态
        /// </summary>
        /// <param name="status"></param>
        protected abstract void SocketStatusChanged(EnumNetworkStatus status);

        /// <summary>
        /// 获取编码对象，编码对象由子类创建
        /// </summary>
        /// <returns></returns>
        public abstract object GetPackageBuilder();

        #region 构造函数
        /// <summary>
        /// TCP创建 或 UDP创建
        /// </summary>
        /// <param name="type">TCP\UDP</param>
        /// <param name="dataBufferSize">异步套接字数据缓冲大小（用户数据区）</param>
        /// <param name="socketRecvBufferSize">socket接收缓冲大小</param>
        /// <param name="socketSendBufferSize">socket发送缓冲大小</param>
        public AbsSocketNetBase(EnumNetworkType type, int dataBufferSize, int socketRecvBufferSize, int socketSendBufferSize)
        {
            Init(type, dataBufferSize, socketRecvBufferSize, socketSendBufferSize);
        }

        /// <summary>
        /// 通常用于构建TCP服务端的客户端
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="type"></param>
        /// <param name="dataBufferSize"></param>
        /// <param name="socketRecvBufferSize"></param>
        /// <param name="socketSendBufferSize"></param>
        public AbsSocketNetBase(Socket socket, EnumNetworkType type, int dataBufferSize, int socketRecvBufferSize, int socketSendBufferSize)
            : this(type, dataBufferSize, socketRecvBufferSize, socketSendBufferSize)
        {
            m_localEndPoint = (IPEndPoint)socket.LocalEndPoint;
            m_remoteEndPoint = (IPEndPoint)socket.RemoteEndPoint;

            m_socket = socket;
            m_socket.ReceiveBufferSize = socketRecvBufferSize;
            m_socket.SendBufferSize = socketSendBufferSize;
            m_socket.NoDelay = true;
            Interlocked.Exchange(ref m_socketStatus, (int)EnumNetworkStatus.Established);
        }

        /// <summary>
        /// TCP/UDP主动创建
        /// </summary>
        /// <param name="localEP">默认本机地址，可为null，启动时再给定</param>
        /// <param name="remoteEP">默认远端地址，可为null，启动时再给定</param>
        /// <param name="type"></param>
        /// <param name="dataBufferSize"></param>
        /// <param name="socketRecvBufferSize"></param>
        /// <param name="socketSendBufferSize"></param>
        public AbsSocketNetBase(IPEndPoint localEP, IPEndPoint remoteEP, EnumNetworkType type, int dataBufferSize, int socketRecvBufferSize, int socketSendBufferSize)
            : this(type, dataBufferSize, socketRecvBufferSize, socketSendBufferSize)
        {
            m_localEndPoint = localEP;
            m_remoteEndPoint = remoteEP;
        }

        private void Init(EnumNetworkType type, int dataBufferSize, int socketRecvBufferSize, int socketSendBufferSize)
        {
            m_socketStatus = (int)EnumNetworkStatus.Undefined;
            m_netProtocolType = type;
            m_dataBufferSize = dataBufferSize;
            m_socketRecvBufferSize = socketRecvBufferSize;
            m_socketSendBufferSize = socketSendBufferSize;
            m_dataBuffer = new byte[m_dataBufferSize];

            m_recvEventArgs = new SocketAsyncEventArgs();
            //m_recvEventArgs.RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            //m_recvEventArgs.UserToken = this;
            if (m_netProtocolType == EnumNetworkType.UDP ||
                m_netProtocolType == EnumNetworkType.Multicast)
            {
                //TCP启动接收为ReceiveAsync
                //UDP启动接收为ReceiveFromAsync，此处必须对RemoteEndPoint赋值，这样接收时才能获取是谁发来的
                m_recvEventArgs.RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            }
            m_recvEventArgs.SetBuffer(m_dataBuffer, 0, m_dataBuffer.Length);//设置接收缓冲区
            m_recvEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnReceiveIOCompleted);
        }
        #endregion

        #region 创建网络
        public virtual void Create(IPEndPoint localEP, IPEndPoint remoteEP)
        {
            if (localEP == null)
            {
                throw new ArgumentNullException("本地网络地址不能为null");
            }
            m_localEndPoint = localEP;

            if (m_netProtocolType == EnumNetworkType.TCP)
            {
                //创建TCP时，服务端地址必须正确
                Create(remoteEP);
            }
            else if (m_netProtocolType == EnumNetworkType.UDP)
            {
                //创建UDP时，可指定本地地址，远端地址可有可无
                m_remoteEndPoint = remoteEP;
                Create();
            }
            else if (m_netProtocolType == EnumNetworkType.Multicast)
            {
                //创建组播时，本地IP通常为IPAddress.Any,不能为127.0.0.1，本地端口必须为组播端口
                //远端地址不能为空，IP为组播IP，端口为组播端口
                if (remoteEP == null)
                {
                    throw new Exception("组播组的地址不能为空");//如224.2.2.2:6666
                }
                if (localEP == null)
                {
                    throw new Exception("组播本地地址不能为空");//如127.0.0.1:6666，端口必须和m_remoteEndPoint一样
                }
                if (localEP.Port != remoteEP.Port)
                {
                    throw new Exception("组播绑定端口必须保持一致");//端口必须一样
                }
                m_localEndPoint = localEP;
                m_remoteEndPoint = remoteEP;
                Create();
            }
            else
            {
                throw new Exception(string.Format("暂不支持的通信协议类型:{0}", m_netProtocolType));
            }
        }

        public virtual void Create(IPEndPoint remoteEP)
        {
            if (remoteEP == null)
            {
                throw new ArgumentNullException("远端网络地址不能为null");
            }
            if (m_socketStatus == (int)EnumNetworkStatus.Established)
            {
                throw new InvalidOperationException("当前网络已建立，请先销毁");
            }
            m_remoteEndPoint = remoteEP;
            Create();
        }

        public virtual void Create()
        {
            if (m_socketStatus == (int)EnumNetworkStatus.Established ||
                m_socketStatus == (int)EnumNetworkStatus.Establishing)
            {
                throw new InvalidOperationException("当前网络已建立，请先销毁");
            }
            if (m_netProtocolType == EnumNetworkType.Unknown)
            {
                throw new Exception(string.Format("暂不支持的通信协议类型:{0}", m_netProtocolType));
            }
            Interlocked.Exchange(ref m_socketStatus, (int)EnumNetworkStatus.Establishing);
            SocketStatusChanged((EnumNetworkStatus)m_socketStatus);
            try
            {
                if (m_netProtocolType == EnumNetworkType.TCP)
                {
                    if (m_remoteEndPoint == null)
                    {
                        throw new Exception("TCP连接远端地址不能为空");
                    }
                    m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    m_socket.NoDelay = true;
                    //socket自带的心跳功能
                    m_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, NetworkGlobal.HEART_INTERVAL);
                    m_socket.NoDelay = true;
                }
                else
                {
                    m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    m_socket.IOControl(-1744830452, new byte[] { 0, 0, 0, 0 }, new byte[] { 0, 0, 0, 0 });
                    //组播
                    if (m_netProtocolType == EnumNetworkType.Multicast)
                    {
                        if (m_remoteEndPoint == null)
                        {
                            throw new Exception("组播组的地址不能为空");//如224.2.2.2:6666
                        }
                        if (m_localEndPoint == null)
                        {
                            throw new Exception("组播本地地址不能为空");//如127.0.0.1:6666，端口必须和m_remoteEndPoint一样
                        }
                        if (m_localEndPoint.Port != m_remoteEndPoint.Port)
                        {
                            throw new Exception("组播绑定端口必须保持一致");//端口必须一样
                        }
                        m_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                        m_socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(m_remoteEndPoint.Address));
                    }
                }
                m_socket.ReceiveBufferSize = m_socketRecvBufferSize;
                m_socket.SendBufferSize = m_socketSendBufferSize;
                //绑定
                if (m_localEndPoint == null)
                {
                    //如果仍然没指定本机地址，则分配随机端口，IP为0.0.0.0
                    m_socket.Bind(new IPEndPoint(IPAddress.Any, 0));
                }
                else
                {
                    //组播的本地IP不能为127.0.0.1，可以为IPAddress.Any、0.0.0.0、局域网IP
                    m_socket.Bind(m_localEndPoint);
                }
                m_localEndPoint = (IPEndPoint)m_socket.LocalEndPoint;
                //TCP连接超时
                if (m_netProtocolType == EnumNetworkType.TCP)
                {
                    //连接
                    IAsyncResult ar = m_socket.BeginConnect(m_remoteEndPoint, null, null);
                    WaitHandle waitHandle = ar.AsyncWaitHandle;
                    //处理链接超时
                    if (!waitHandle.WaitOne(TimeSpan.FromSeconds(NetworkGlobal.CONNECTION_TIMEOUT), false))
                    {
                        var errMsg = string.Format("TCP连接{0}超时{1}秒.", m_remoteEndPoint, NetworkGlobal.CONNECTION_TIMEOUT);
                        throw new TimeoutException(errMsg);
                    }
                    m_socket.EndConnect(ar);
                    waitHandle.Close();
                }
                if (Interlocked.CompareExchange(ref m_socketStatus, (int)EnumNetworkStatus.Established, (int)EnumNetworkStatus.Establishing) != (int)EnumNetworkStatus.Establishing)
                {
                    //如：在连接过程中，其它线程调用了断开、销毁等函数
                    throw new InvalidOperationException("连接过程中，通信状态异常");
                }
            }
            catch (Exception)
            {
                DestroySocket();
                throw;
            }
            SocketStatusChanged((EnumNetworkStatus)m_socketStatus);
            StartReceive();
        }

        /*
        #region TCP网络连接
        private void CreateTCP()
        {
            if (m_socketStatus == (int)EnumNetworkStatus.Established ||
                m_socketStatus == (int)EnumNetworkStatus.Establishing)
            {
                return;
            }
            else
            {
                if (m_remoteEndPoint == null)
                {
                    throw new ArgumentNullException("服务端地址不能为空");
                }
                try
                {
                    Interlocked.Exchange(ref m_socketStatus, (int)EnumNetworkStatus.Establishing);
                    SocketStatusChanged((EnumNetworkStatus)m_socketStatus);
                    //IPv4
                    m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    m_socket.ReceiveBufferSize = m_socketRecvBufferSize;
                    m_socket.SendBufferSize = m_socketSendBufferSize;
                    m_socket.NoDelay = true;
                    //socket自带的心跳功能
                    m_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, NetworkGlobal.HEART_INTERVAL);
                    //绑定
                    if (m_localEndPoint == null)
                    {
                        //如果仍然没指定本机地址，则分配随机端口，IP为0.0.0.0
                        m_socket.Bind(new IPEndPoint(IPAddress.Any, 0));
                    }
                    else
                    {
                        m_socket.Bind(m_localEndPoint);
                    }
                    m_localEndPoint = (IPEndPoint)m_socket.LocalEndPoint;
                    //连接
                    IAsyncResult ar = m_socket.BeginConnect(m_remoteEndPoint, null, null);
                    WaitHandle waitHandle = ar.AsyncWaitHandle;

                    //处理链接超时
                    if (!waitHandle.WaitOne(TimeSpan.FromSeconds(NetworkGlobal.CONNECTION_TIMEOUT), false))
                    {
                        var errMsg = string.Format("TCP连接{0}超时{1}秒.", m_remoteEndPoint, NetworkGlobal.CONNECTION_TIMEOUT);
                        throw new TimeoutException(errMsg);
                    }
                    m_socket.EndConnect(ar);
                    waitHandle.Close();
                }
                catch (Exception)
                {
                    DestroySocket();
                    throw;
                }
                if (Interlocked.CompareExchange(ref m_socketStatus, (int)EnumNetworkStatus.Established, (int)EnumNetworkStatus.Establishing) == (int)EnumNetworkStatus.Establishing)
                {
                    //连接成功通知
                    SocketStatusChanged((EnumNetworkStatus)m_socketStatus);
                    //处理连接数据
                    StartReceive();
                }
                else
                {
                    DestroySocket();
                    //如：在连接过程中调用了断开、销毁等函数
                    throw new InvalidOperationException("连接过程中，通信状态异常");
                }
            }
        }

        #endregion

        #region UDP网络构建
        /// <summary>
        /// 创建并启动网络收发
        /// </summary>
        private void CreateUDP()
        {
            if (m_socketStatus == (int)EnumNetworkStatus.Established ||
                m_socketStatus == (int)EnumNetworkStatus.Establishing)
            {
                return;
            }

            try
            {
                Interlocked.Exchange(ref m_socketStatus, (int)EnumNetworkStatus.Establishing);
                SocketStatusChanged((EnumNetworkStatus)m_socketStatus);
                //socket创建
                m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                m_socket.ReceiveBufferSize = m_socketRecvBufferSize;
                m_socket.SendBufferSize = m_socketSendBufferSize;
                m_socket.IOControl(-1744830452, new byte[] { 0, 0, 0, 0 }, new byte[] { 0, 0, 0, 0 });
                //绑定
                if (m_localEndPoint == null)
                {
                    m_socket.Bind(new IPEndPoint(IPAddress.Any, 0));
                }
                else
                {
                    m_socket.Bind(m_localEndPoint);
                }
                m_localEndPoint = (IPEndPoint)m_socket.LocalEndPoint;
                if (Interlocked.CompareExchange(ref m_socketStatus, (int)EnumNetworkStatus.Established, (int)EnumNetworkStatus.Establishing) == (int)EnumNetworkStatus.Establishing)
                {
                    SocketStatusChanged((EnumNetworkStatus)m_socketStatus);
                    StartReceive();
                }
                else
                {
                    DestroySocket();
                    throw new InvalidOperationException("UDP启动过程中，通信状态异常");
                }
            }
            catch (Exception)
            {
                DestroySocket();
                throw;
            }
        }
        #endregion
        */

        #endregion

        #region 销毁网络
        /// <summary>
        /// 销毁TCP\UDP
        /// </summary>
        public virtual void Destroy()
        {
            DestroySocket();
        }
        #endregion

        #region 数据接收
        /// <summary>
        /// 启动数据接收
        /// </summary>
        internal void StartReceive()
        {
            if (m_socketStatus != (int)EnumNetworkStatus.Established)
            {
                DestroySocket();
                return;
            }
            try
            {
                if (m_netProtocolType == EnumNetworkType.TCP)
                {
                    //TCP开始一次异步数据接收,注意完成事件类型
                    if (!m_socket.ReceiveAsync(m_recvEventArgs))
                    {
                        //同步返回
                        OnReceiveIOCompleted(null, m_recvEventArgs);
                    }
                }
                else if (m_netProtocolType == EnumNetworkType.UDP ||
                         m_netProtocolType == EnumNetworkType.Multicast)
                {
                    //注意：此处为ReceiveFromAsync，则完成事件必须为ReceiveFrom，且必须指定异步套接字的RemoteEndPoint字段
                    if (!m_socket.ReceiveFromAsync(m_recvEventArgs))
                    {
                        //同步返回
                        OnReceiveIOCompleted(null, m_recvEventArgs);
                    }
                }
                else
                {
                    throw new Exception(string.Format("暂不支持的协议类型:{0}", m_netProtocolType));
                }
            }
            catch (Exception ex)
            {
                SocketExceptionHappened("套接字投递一次异步接收失败,套接字即将销毁", ex);
                DestroySocket();
            }
        }

        /// <summary>
        /// 完成端口收到数据事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnReceiveIOCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.LastOperation == SocketAsyncOperation.Receive)//TCP
            {
                switch (m_recvEventArgs.SocketError)
                {
                    case SocketError.Success:
                        {
                            if (m_recvEventArgs.BytesTransferred > 0)
                            {
                                try
                                {
                                    //SocketDataReceived(m_netProtocolType, e.Buffer, e.Offset, e.BytesTransferred, (IPEndPoint)e.RemoteEndPoint);
                                    SocketDataReceived(m_netProtocolType, e.Buffer, e.Offset, e.BytesTransferred, m_remoteEndPoint);
                                }
                                catch (Exception ex)
                                {
                                    SocketExceptionHappened("OnReceiveIOCompleted收数处理失败", ex);
                                }
                                finally
                                {
                                    //投递下一次接收
                                    StartReceive();
                                }
                            }
                            else
                            {
                                //服务器连接断开
                                var exception = new Exception(string.Format("SocketError = {0}", m_recvEventArgs.SocketError));
                                SocketExceptionHappened("OnReceiveIOCompleted收数处理失败,将断开连接", exception);
                                DestroySocket();
                            }
                        }
                        break;
                    case SocketError.ConnectionReset:
                    case SocketError.ConnectionAborted:
                    case SocketError.ConnectionRefused:
                        {
                            //服务器连接断开
                            var exception = new Exception(string.Format("SocketError = {0}", m_recvEventArgs.SocketError));
                            SocketExceptionHappened("OnReceiveIOCompleted收数处理失败,将断开连接", exception);
                            DestroySocket();
                        }
                        break;
                    default:
                        {
                            var exception = new Exception(string.Format("SocketError = {0}", m_recvEventArgs.SocketError));
                            SocketExceptionHappened("OnReceiveIOCompleted收数处理失败,但未作处理", exception);
                            //投递下一次接收
                            StartReceive();
                        }
                        break;
                }
            }
            else if (e.LastOperation == SocketAsyncOperation.ReceiveFrom)//UDP
            {
                try
                {
                    if (e.BytesTransferred > 0 &&
                        e.SocketError == SocketError.Success)
                    {
                        SocketDataReceived(m_netProtocolType, e.Buffer, e.Offset, e.BytesTransferred, (IPEndPoint)e.RemoteEndPoint);
                    }
                }
                catch (Exception ex)
                {
                    SocketExceptionHappened("OnReceiveIOCompleted收数处理失败", ex);
                }
                finally
                {
                    //投递下一次接收
                    StartReceive();
                }
            }
            else
            {
                var ex = new Exception(string.Format("LastOperation = {0}", e.LastOperation));
                SocketExceptionHappened("OnReceiveIOCompleted收数处理:暂不支持的套接字操作", ex);
            }
        }
        #endregion

        #region 数据发送
        #region Send
        public virtual int Send(byte[] data, bool encode = true, object userTag = null)
        {
            return Send(data, 0, data.Length, encode, userTag);
        }

        public virtual int Send(byte[] data, int offset, int length, bool encode = true, object userTag = null)
        {
            if (m_socketStatus != (int)EnumNetworkStatus.Established)
            {
                throw new InvalidOperationException("Socket未就绪");
            }
            if (encode)
            {
                int tempLength = 0;
                int tempOffset = 0;
                byte[] dest = null;
                if (DataEncode(data, offset, length, ref dest, ref tempOffset, ref tempLength, userTag))
                {
                    return m_socket.Send(dest, tempOffset, tempLength, SocketFlags.None);
                }
                else
                {
                    throw new InvalidOperationException("编码失败");
                }
            }
            else
            {
                return m_socket.Send(data, offset, length, SocketFlags.None);
            }
        }
        #endregion

        #region BeginSend
        public virtual void BeginSend(byte[] data, bool encode = true, object userTag = null)
        {
            BeginSend(data, 0, data.Length, encode, userTag);
        }

        public virtual void BeginSend(byte[] data, int offset, int length, bool encode = true, object userTag = null)
        {
            if (m_socketStatus != (int)EnumNetworkStatus.Established)
            {
                throw new InvalidOperationException("Socket未就绪");
            }
            if (encode)
            {
                int tempLength = 0;
                int tempOffset = 0;
                byte[] dest = null;
                if (DataEncode(data, offset, length, ref dest, ref tempOffset, ref tempLength, userTag))
                {
                    m_socket.BeginSend(dest, tempOffset, tempLength, SocketFlags.None, null, null);
                }
                else
                {
                    throw new InvalidOperationException("编码失败");
                }
            }
            else
            {
                m_socket.BeginSend(data, offset, length, SocketFlags.None, null, null);
            }
        }
        #endregion

        #region SendTo
        public virtual int SendTo(byte[] buffer, bool encode = true, object userTag = null)
        {
            return SendTo(buffer, m_remoteEndPoint, encode, userTag);
        }

        public virtual int SendTo(byte[] buffer, int offset, int length, bool encode = true, object userTag = null)
        {
            return SendTo(buffer, offset, length, m_remoteEndPoint, encode, userTag);
        }

        public virtual int SendTo(byte[] buffer, IPEndPoint remoteEP, bool encode = true, object userTag = null)
        {
            return SendTo(buffer, 0, buffer.Length, remoteEP, encode, userTag);
        }
        public virtual int SendTo(byte[] buffer, int offset, int length, EndPoint remoteEP, bool encode = true, object userTag = null)
        {
            if (m_socketStatus != (int)EnumNetworkStatus.Established)
            {
                throw new InvalidOperationException("Socket未就绪");
            }
            if (encode)
            {
                int tempLength = 0;
                int tempOffset = 0;
                byte[] dest = null;
                if (DataEncode(buffer, offset, length, ref dest, ref tempOffset, ref tempLength, userTag))
                {
                    return m_socket.SendTo(dest, tempOffset, tempLength, SocketFlags.None, remoteEP);
                }
                else
                {
                    throw new InvalidOperationException("编码失败");
                }
            }
            else
            {
                return m_socket.SendTo(buffer, offset, length, SocketFlags.None, remoteEP);
            }
        }
        #endregion

        #region BeginSendTo
        public virtual void BeginSendTo(byte[] buffer, bool encode = true, object userTag = null)
        {
            BeginSendTo(buffer, m_remoteEndPoint, encode, userTag);
        }

        public virtual void BeginSendTo(byte[] buffer, int offset, int length, bool encode = true, object userTag = null)
        {
            BeginSendTo(buffer, offset, length, m_remoteEndPoint, encode, userTag);
        }

        public virtual void BeginSendTo(byte[] buffer, IPEndPoint remoteEP, bool encode = true, object userTag = null)
        {
            BeginSendTo(buffer, 0, buffer.Length, remoteEP, encode, userTag);
        }

        public virtual void BeginSendTo(byte[] buffer, int offset, int length, IPEndPoint remoteEP, bool encode = true, object userTag = null)
        {
            if (m_socketStatus != (int)EnumNetworkStatus.Established)
            {
                throw new InvalidOperationException("Socket未就绪");
            }
            if (encode)
            {
                int tempLength = 0;
                int tempOffset = 0;
                byte[] dest = null;
                if (DataEncode(buffer, offset, length, ref dest, ref tempOffset, ref tempLength, userTag))
                {
                    m_socket.BeginSendTo(dest, tempOffset, tempLength, SocketFlags.None, remoteEP, null, null);
                }
                else
                {
                    throw new InvalidOperationException("编码失败");
                }
            }
            else
            {
                m_socket.BeginSendTo(buffer, offset, length, SocketFlags.None, remoteEP, null, null);
            }
        }
        #endregion
        #endregion

        #region 事件回调
        protected void Callback_SocketDataReceived(object sender, DataReceivedArgs<TPackage> args)
        {
            if (OnDataReceivedEvent != null)
            {
                OnDataReceivedEvent(sender, args);
            }
        }

        protected void Callback_SocketExceptionHappened(object sender, ExceptionHappenedArgs args)
        {
            if (OnExceptionHappenedEvent != null)
            {
                OnExceptionHappenedEvent(sender, args);
            }
        }

        protected void Callback_SocketStatusChanged(object sender, SocketStatusChangedArgs<TPackage> args)
        {
            if (OnSocketStatusChangedEvent != null)
            {
                OnSocketStatusChangedEvent(sender, args);
            }
        }

        internal void PublishSocketDataReceived(DataReceivedArgs<TPackage> args)
        {
            Callback_SocketDataReceived(this, args);
        }

        #endregion

        /// <summary>
        /// 销毁套接字
        /// </summary>
        protected virtual void DestroySocket()
        {
            int oldState = Interlocked.Exchange(ref m_socketStatus, (int)EnumNetworkStatus.Shutdown);
            if (oldState == (int)EnumNetworkStatus.Established ||
                oldState == (int)EnumNetworkStatus.Establishing)
            {
                try
                {
                    if (m_socket != null)
                    {
                        m_socket.Close();
                        m_socket.Dispose();
                        m_socket = null;
                    }
                }
                catch (Exception ex)
                {
                    SocketExceptionHappened("销毁套接字异常,仍然执行", ex);
                }
                finally
                {
                    SocketStatusChanged((EnumNetworkStatus)m_socketStatus);
                }
            }
        }

        public virtual void Dispose()
        {
            m_disposed = true;
            DestroySocket();
        }
    }
}