using System.Net;
using System.Text;

namespace TSocket
{
    /// <summary>
    /// 通信协议构建基类，可由用户自定义实现自己的协议构建
    /// Socket只负责基础网络通信
    /// </summary>
    /// <typeparam name="TPackage">自定义协议数据包</typeparam>
    public abstract class AbsProtocolBuilder<TPackage>
    {
        protected Encoding m_encodingType = Encoding.UTF8;
        /// <summary>
        /// 编码方式,此参数没啥用
        /// </summary>
        public Encoding EncodingType
        {
            get { return m_encodingType; }
            set { m_encodingType = value; }
        }


        private AbsSocketNetBase<TPackage> m_socket = null;
        /// <summary>
        /// 通信对象
        /// </summary>
        internal AbsSocketNetBase<TPackage> Socket
        {
            get
            {
                return m_socket;
            }

            set
            {
                m_socket = value;
            }
        }

        /// <summary>
        /// 必须为无参构造函数
        /// </summary>
        public AbsProtocolBuilder()
        {
            
        }


        //public abstract AbsProtocolBuilder<TPackage> Clone();

        /// <summary>
        /// 发布收数事件
        /// </summary>
        /// <param name="args"></param>
        protected void PublishData(DataReceivedArgs<TPackage> args)
        {
            if (m_socket != null)
            {
                m_socket.PublishSocketDataReceived(args);
            }
        }

        /// <summary>
        /// 发布收数事件
        /// </summary>
        /// <param name="netType"></param>
        /// <param name="remoteEP"></param>
        /// <param name="localEP"></param>
        /// <param name="pack"></param>
        protected void PublishData(EnumNetworkType netType, IPEndPoint remoteEP, IPEndPoint localEP, TPackage pack)
        {
            PublishData(new DataReceivedArgs<TPackage>(netType, localEP, remoteEP, pack));
        }


        /// <summary>
        /// 数据编码
        /// </summary>
        /// <param name="src">需要编码的源数据</param>
        /// <param name="srcOffset">源数据偏移</param>
        /// <param name="srcLength">源数据长度</param>
        /// <param name="dest">目标数据，用于网络发送</param>
        /// <param name="destOffset">目标数据偏移</param>
        /// <param name="destLength">目标数据长度</param>
        /// <param name="userTag">自定义参数</param>
        /// <returns>是否编码成功</returns>
        public abstract bool EncodeParse(byte[] src, int srcOffset, int srcLength, ref byte[] dest, ref int destOffset, ref int destLength, object userTag = null);

        /// <summary>
        /// 自定义解码，协议解析完成后需调用PublishData()函数发布事件给应用层
        /// </summary>
        /// <param name="netType"></param>
        /// <param name="data">接受缓冲区</param>
        /// <param name="offset">目标数据偏移</param>
        /// <param name="length">目标数据长度</param>
        /// <param name="remoteEP">数据来源地址</param>
        /// <param name="localEP">本地绑定地址</param>
        public abstract void DecodeParse(EnumNetworkType netType,byte[] data, int offset, int length, IPEndPoint remoteEP, IPEndPoint localEP);
        /// <summary>
        /// 重置，比如连接断开后清空缓存等
        /// </summary>
        public abstract void Reset();
        /// <summary>
        /// 销毁
        /// </summary>
        public abstract void Dispose();
    }
}
