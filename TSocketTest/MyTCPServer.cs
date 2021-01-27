using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using TSocket;

namespace TSocketTest
{
    class MyTCPServer
    {
        ISocketNetServer<ProtocolPackageBase> m_socket = null;

        public MyTCPServer()
        {
            //创建对象，添加事件
            m_socket = SocketNetFactory.CreateTCPServer<ProtocolBuilder, ProtocolPackageBase>();
            m_socket.OnClientSocketStatusChangedEvent += M_socket_OnClientSocketStatusChangedEvent; 
            m_socket.OnDataReceivedEvent += M_socket_OnDataReceivedEvent;
            m_socket.OnExceptionHappenedEvent += M_socket_OnExceptionHappenedEvent;
            m_socket.OnListenStatusChangedEvent += M_socket_OnListenStatusChangedEvent;
        }

        #region 事件
        private void M_socket_OnListenStatusChangedEvent(object arg1, ListenStatusChangedArgs arg2)
        {
            Console.WriteLine("服务端监听状态改变通知:" + arg2.IsListen);
        }

        private void M_socket_OnExceptionHappenedEvent(object arg1, ExceptionHappenedArgs arg2)
        {
            Console.WriteLine("服务的异常通知：" + arg2.Description);
        }

        private void M_socket_OnDataReceivedEvent(object arg1, DataReceivedArgs<ProtocolPackageBase> arg2)
        {
            Console.WriteLine("服务端收到客户端消息通知");
        }

        private void M_socket_OnClientSocketStatusChangedEvent(object arg1, SocketStatusChangedArgs<ProtocolPackageBase> arg2)
        {
            Console.WriteLine(string.Format("服务的的客户端状态改变：{0}    {1}", arg2.Target.RemoteEndPoint, arg2.Status));
        }
        #endregion

        public void StartListen(IPEndPoint listenEP)
        {
            m_socket.Start(listenEP);
        }

        public void StopListen()
        {
            m_socket.Stop();
        }

        public void Send(byte[] data,IPEndPoint clientEP = null)
        {
            m_socket.Send(clientEP, data, false, true, null);
        }

    }
}
