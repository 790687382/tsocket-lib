using System;
using System.Net;

namespace TSocket
{
    /// <summary>
    /// 异常参数，通常用作日志记录、问题排查即可
    /// </summary>
    public class ExceptionHappenedArgs
    {        
        /// <summary>
        /// 发生异常的地址,可以为客户端、服务端、UDP
        /// </summary>
        public IPEndPoint ExceptionEP { get; private set; }
        public string Description { get; private set; }
        public Exception Ex { get; private set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="ep">发生异常的地址</param>
        /// <param name="description">简要描述</param>
        /// <param name="exception">ex</param>
        public ExceptionHappenedArgs(IPEndPoint ep,string description,Exception exception)
        {
            ExceptionEP = ep;
            Description = description;
            Ex = exception;
        }
    }
}
