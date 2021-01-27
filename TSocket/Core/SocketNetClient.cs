using System;
using System.Net;
using System.Net.Sockets;

namespace TSocket
{
    /// <summary>
    /// 通信客户端，可用于TCP\UDP
    /// </summary>
    /// <typeparam name="TBuilder"></typeparam>
    /// <typeparam name="TPackage"></typeparam>
    class SocketNetClient<TBuilder, TPackage> : AbsSocketNetBase<TPackage> where TBuilder : AbsProtocolBuilder<TPackage>, new()
    {
        //协议解析器，由用户自己构建
        TBuilder m_protocolBuilder = null;

        /// <summary>
        /// 用于TCP客户端、UDP客户端
        /// </summary>
        /// <param name="type">TCP或UDP</param>
        /// <param name="dataRecvSize"></param>
        /// <param name="socketRecvSize"></param>
        /// <param name="socketSendSize"></param>
        public SocketNetClient(EnumNetworkType type, int dataRecvSize, int socketRecvSize, int socketSendSize)
            : base(type, dataRecvSize, socketRecvSize, socketSendSize)
        {
            m_protocolBuilder = new TBuilder();
            m_protocolBuilder.Socket = this;
        }

        /// <summary>
        /// 用于TCP服务端的客户端
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="type"></param>
        /// <param name="dataRecvSize"></param>
        /// <param name="socketRecvSize"></param>
        /// <param name="socketSendSize"></param>
        public SocketNetClient(Socket socket, EnumNetworkType type, int dataRecvSize, int socketRecvSize, int socketSendSize)
            : base(socket, type, dataRecvSize, socketRecvSize, socketSendSize)
        {
            m_protocolBuilder = new TBuilder();
            m_protocolBuilder.Socket = this;
        }


        /// <summary>
        /// 获取协议解析对象
        /// </summary>
        /// <returns></returns>
        public override object GetPackageBuilder()
        {
            return m_protocolBuilder;
        }

        protected override bool DataEncode(byte[] src, int srcOffset, int srcLength, ref byte[] dest, ref int destOffset, ref int destLength, object userTag = null)
        {
            return m_protocolBuilder.EncodeParse(src, srcOffset, srcLength, ref dest, ref destOffset, ref destLength, userTag);
        }

        protected override void SocketDataReceived(EnumNetworkType type, byte[] data, int offset, int length, IPEndPoint remoteEP)
        {
            m_protocolBuilder.DecodeParse(type, data, offset, length, remoteEP, LocalEndPoint);
        }

        protected override void SocketExceptionHappened(string description, Exception ex)
        {
            Callback_SocketExceptionHappened(this, new ExceptionHappenedArgs(RemoteEndPoint, description, ex));
        }

        protected override void SocketStatusChanged(EnumNetworkStatus status)
        {
            Callback_SocketStatusChanged(this, new SocketStatusChangedArgs<TPackage>(status, this));
        }

        protected override void DestroySocket()
        {
            base.DestroySocket();
            m_protocolBuilder.Reset();
        }

        public override void Dispose()
        {
            base.Dispose();
            m_protocolBuilder.Dispose();
        }
    }
}
