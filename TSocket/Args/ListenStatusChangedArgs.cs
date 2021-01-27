using System;
using System.Net;

namespace TSocket
{
    /// <summary>
    /// 监听状态改变事件参数
    /// </summary>
    public class ListenStatusChangedArgs
    {
        /// <summary>
        /// 本地监听地址
        /// </summary>
        public IPEndPoint LocalEP { get; private set; }

        /// <summary>
        /// 是否监听
        /// </summary>
        public bool IsListen { get; private set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="localEP">监听地址</param>
        /// <param name="listen">监听状态</param>
        public ListenStatusChangedArgs(IPEndPoint localEP, bool listen)
        {
            LocalEP = localEP;
            IsListen = listen;
        }
    }
}
