using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace TSocket
{
    /// <summary>
    /// TCP服务端实现
    /// </summary>
    /// <typeparam name="TBuilder">解协议实现</typeparam>
    /// <typeparam name="TPackage">用户自定义解析后的应用层数据包</typeparam>
    class SocketNetServer<TBuilder, TPackage> : ISocketNetServer<TPackage> where TBuilder : AbsProtocolBuilder<TPackage>, new()
    {
        Socket m_socket;
        IPEndPoint m_listenEP;
        int m_state = (int)EnumNetworkStatus.Undefined;
        //在线客户端
        ConcurrentDictionary<IPEndPoint, SocketNetClient<TBuilder, TPackage>> m_clients;
        public event Action<object, DataReceivedArgs<TPackage>> OnDataReceivedEvent;
        public event Action<object, ExceptionHappenedArgs> OnExceptionHappenedEvent;
        public event Action<object, SocketStatusChangedArgs<TPackage>> OnClientSocketStatusChangedEvent;
        public event Action<object, ListenStatusChangedArgs> OnListenStatusChangedEvent;

        #region 属性/普通函数
        public bool IsListen
        {
            get { return m_state == (int)EnumNetworkStatus.Established ? true : false; }
        }

        public IPEndPoint ListenEP
        {
            get { return m_listenEP; }
        }
        #endregion

        public SocketNetServer()
        {
            m_clients = new ConcurrentDictionary<IPEndPoint, SocketNetClient<TBuilder, TPackage>>();
        }

        #region 启动/停止监听
        /// <summary>
        /// 启动监听
        /// </summary>
        public void Start(IPEndPoint listenEP)
        {
            if (m_state == (int)EnumNetworkStatus.Established)
            {
                return;
            }
            if (listenEP == null)
            {
                throw new Exception("监听地址不能为null");
            }
            Interlocked.Exchange(ref m_state, (int)EnumNetworkStatus.Establishing);
            //创建
            m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //绑定
            m_socket.Bind(listenEP);
            m_listenEP = (IPEndPoint)m_socket.LocalEndPoint;
            //监听
            m_socket.Listen(200);
            if (Interlocked.CompareExchange(ref m_state, (int)EnumNetworkStatus.Established, (int)EnumNetworkStatus.Establishing) != (int)EnumNetworkStatus.Establishing)
            {
                Stop();
                throw new InvalidOperationException("启动过程中，状态异常");
            }
            //接收客户端连接
            StartAcceptConnection(null);
            if (OnListenStatusChangedEvent != null)
            {
                OnListenStatusChangedEvent(this, new ListenStatusChangedArgs(m_listenEP, true));
            }
        }

        /// <summary>
        /// 停止监听
        /// </summary>
        public void Stop()
        {
            if (Interlocked.CompareExchange(ref m_state, (int)EnumNetworkStatus.Shutdown, (int)EnumNetworkStatus.Established) == (int)EnumNetworkStatus.Established)
            {
                //断开所有客户端
                Disconnect();
                //断开socke监听服务
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
                    if (OnExceptionHappenedEvent != null)
                    {
                        OnExceptionHappenedEvent(this, new ExceptionHappenedArgs(m_listenEP, "停止监听服务异常", ex));
                    }
                }
                finally
                {
                    if (OnListenStatusChangedEvent != null)
                    {
                        OnListenStatusChangedEvent(this, new ListenStatusChangedArgs(m_listenEP, false));
                    }
                }
            }
        }
        #endregion

        #region 连接处理
        /// <summary>
        /// 启动接收连接
        /// </summary>
        /// <param name="asyn">首次启动为null</param>
        private void StartAcceptConnection(SocketAsyncEventArgs asyn)
        {
            if (m_state != (int)EnumNetworkStatus.Established)
            {
                return;
            }
            if (asyn == null)
            {
                //首次构建
                asyn = new SocketAsyncEventArgs();
                asyn.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
            }
            asyn.AcceptSocket = null;
            try
            {
                if (!m_socket.AcceptAsync(asyn))
                {
                    //同步完成
                    OnAcceptCompleted(null, asyn);
                }
            }
            catch (Exception ex)
            {
                if (OnExceptionHappenedEvent != null)
                {
                    OnExceptionHappenedEvent(this, new ExceptionHappenedArgs(m_listenEP, "套接字启动连接接收失败", ex));
                }
            }
        }

        /// <summary>
        /// 一个连接接收完成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAcceptCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (m_state != (int)EnumNetworkStatus.Established)
            {
                return;
            }
            if (e.SocketError == SocketError.Success)
            {
                //和客户端关联的socket
                Socket socket = e.AcceptSocket;
                try
                {
                    if (socket.Connected)
                    {
                        var client = new SocketNetClient<TBuilder, TPackage>(socket,
                            EnumNetworkType.TCP,
                            NetworkGlobal.TCP_RECEIVE_BUFFER_SIZE,
                            NetworkGlobal.TCP_SOCKET_RECEIVE_SIZE,
                            NetworkGlobal.TCP_SOCKET_SEND_SIZE);
                        client.OnSocketStatusChangedEvent += Client_OnClientSocketStatusChangedEvent;
                        client.OnDataReceivedEvent += Client_OnDataReceivedEvent;
                        client.OnExceptionHappenedEvent += Client_OnExceptionHappenedEvent;
                        //更新上线
                        m_clients.AddOrUpdate((IPEndPoint)socket.RemoteEndPoint, client, (v, k) => { return client; });
                        //启动收收数
                        client.StartReceive();
                        //上线通知
                        if (OnClientSocketStatusChangedEvent != null)
                        {
                            var args = new SocketStatusChangedArgs<TPackage>(EnumNetworkStatus.Established, client);
                            OnClientSocketStatusChangedEvent(this, args);
                        }
                    }
                }
                catch (SocketException ex)
                {
                    if (OnExceptionHappenedEvent != null)
                    {
                        var args = new ExceptionHappenedArgs((IPEndPoint)socket.RemoteEndPoint, "处理客户端连接上线失败", ex);
                        OnExceptionHappenedEvent(this, args);
                    }
                }
            }
            //投递下一个接受请求
            StartAcceptConnection(e);
        }

        #region 客户端事件
        private void Client_OnExceptionHappenedEvent(object arg1, ExceptionHappenedArgs arg2)
        {
            if (OnExceptionHappenedEvent != null)
            {
                OnExceptionHappenedEvent(this, arg2);
            }
        }

        private void Client_OnDataReceivedEvent(object arg1, DataReceivedArgs<TPackage> arg2)
        {
            if (OnDataReceivedEvent != null)
            {
                OnDataReceivedEvent(this, arg2);
            }
        }

        private void Client_OnClientSocketStatusChangedEvent(object arg1, SocketStatusChangedArgs<TPackage> arg2)
        {
            switch (arg2.Status)
            {
                case EnumNetworkStatus.Shutdown:
                    {
                        SocketNetClient<TBuilder, TPackage> client = null;
                        m_clients.TryRemove(arg2.Target.RemoteEndPoint, out client);
                    }
                    break;
                default:
                    break;
            }
            if (OnClientSocketStatusChangedEvent != null)
            {
                OnClientSocketStatusChangedEvent(this, arg2);
            }
        }
        #endregion
        #endregion

        #region 功能函数
        /// <summary>
        /// 获取在线的连接
        /// </summary>
        /// <returns></returns>
        public IList<NetworkConnection<TPackage>> GetConnection()
        {
            var conList = new List<NetworkConnection<TPackage>>();
            foreach (var item in m_clients)
            {
                conList.Add(new NetworkConnection<TPackage>(item.Key, item.Value));
            }
            return conList;
        }

        /// <summary>
        /// 断开所有连接
        /// </summary>
        public void Disconnect()
        {
            foreach (var item in m_clients)
            {
                item.Value.Dispose();
            }
            m_clients.Clear();
        }

        /// <summary>
        /// 断开指定连接
        /// </summary>
        /// <param name="endpoint"></param>
        public void Disconnect(IPEndPoint endpoint)
        {
            SocketNetClient<TBuilder, TPackage> client = null;
            if (m_clients.TryRemove(endpoint, out client))
            {
                client.Dispose();
            }
        }

        /// <summary>
        /// 获取指定连接，失败返回null
        /// </summary>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        public NetworkConnection<TPackage> GetConnection(IPEndPoint endpoint)
        {
            NetworkConnection<TPackage> target = null;
            SocketNetClient<TBuilder, TPackage> client = null;
            if (m_clients.TryGetValue(endpoint, out client))
            {
                target = new NetworkConnection<TPackage>(client.RemoteEndPoint, client);
            }
            return target;
        }
        #endregion

        #region 发送数据
        //编码发送
        public virtual void Send(IPEndPoint clientEP, byte[] data, bool isAsync = true, bool encode = true, object userTag = null)
        {
            Send(clientEP, data, 0, data.Length, isAsync, encode, userTag);
        }
        //编码发送
        public virtual void Send(IPEndPoint clientEP, byte[] data, int offset, int length, bool isAsync = true, bool encode = true, object userTag = null)
        {
            PrivateSend(clientEP, data, offset, length, isAsync, encode, userTag);
        }

        private void PrivateSend(IPEndPoint clientEP, byte[] data, int offset, int length, bool isAsync, bool encode, object userTag)
        {
            if (m_state != (int)EnumNetworkStatus.Established)
            {
                throw new InvalidProgramException(string.Format("服务状态{0},无法发送", (EnumNetworkStatus)m_state));
            }
            if (clientEP != null)
            {
                SocketNetClient<TBuilder, TPackage> client = null;
                if (m_clients.TryGetValue(clientEP, out client))
                {
                    if (isAsync)
                    {
                        client.BeginSend(data, offset, length, encode, userTag);
                    }
                    else
                    {
                        client.Send(data, offset, length, encode, userTag);
                    }
                }
                else
                {
                    throw new Exception(string.Format("客户端{0}不在线,无法发送", clientEP));
                }
            }
            else
            {
                //发送给所有连接
                foreach (var item in m_clients)
                {
                    try
                    {
                        if (isAsync)
                        {
                            item.Value.BeginSend(data, offset, length, encode, userTag);
                        }
                        else
                        {
                            item.Value.Send(data, offset, length, encode, userTag);
                        }
                    }
                    catch (Exception)
                    {
                        //注意异常没抛出
                    }
                }
            }
        }
        #endregion

        public void Dispose()
        {
            Disconnect();
            m_clients.Clear();
            Stop();
        }
    }
}
