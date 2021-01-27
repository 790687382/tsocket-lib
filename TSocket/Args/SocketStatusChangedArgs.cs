namespace TSocket
{
    /// <summary>
    /// Socket状态改变事件参数
    /// </summary>
    public class SocketStatusChangedArgs<TPackage>
    {
        /// <summary>
        /// Socket状态
        /// </summary>
        public EnumNetworkStatus Status { get; private set; }
        /// <summary>
        /// 发生状态的通信对象
        /// </summary>
        public ISocketNetClient<TPackage> Target { get; private set; }

        public SocketStatusChangedArgs(EnumNetworkStatus status, ISocketNetClient<TPackage> target)
        {
            Status = status;
            Target = target;
        }
    }
}
