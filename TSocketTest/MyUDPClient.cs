using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using TSocket;

namespace TSocketTest
{
    class MyUDPClient
    {
        ISocketNetClient<ProtocolPackageBase> m_socket = null;
        public MyUDPClient()
        {
            //创建对象，添加事件
            m_socket = SocketNetFactory.CreateUDPClient<ProtocolBuilder, ProtocolPackageBase>();
            m_socket.OnSocketStatusChangedEvent += M_socket_OnSocketStatusChangedEvent;
            m_socket.OnDataReceivedEvent += M_socket_OnDataReceivedEvent;
            m_socket.OnExceptionHappenedEvent += M_socket_OnExceptionHappenedEvent;
        }


        #region 事件
        private void M_socket_OnSocketStatusChangedEvent(object arg1, SocketStatusChangedArgs<ProtocolPackageBase> arg2)
        {
            Console.WriteLine("UDP状态改变:" + arg2.Status);
        }

        private void M_socket_OnExceptionHappenedEvent(object arg1, ExceptionHappenedArgs arg2)
        {
            Console.WriteLine("UDP异常通知：" + arg2.Description);
        }

        private void M_socket_OnDataReceivedEvent(object arg1, DataReceivedArgs<ProtocolPackageBase> arg2)
        {
            Console.WriteLine("UDP收到数据");
            switch (arg2.Package.PackType)
            {
                case 1:
                    {
                        //应用层处理
                        var obj = (ProtocolPackageAAAAA)arg2.Package;
                    }
                    break;
                case 2:
                    {
                        var obj = (ProtocolPackageBBBBB)arg2.Package;
                    }
                    break;
                default:
                    break;
            }
        }

        #endregion


        public void Create(IPEndPoint localEP)
        {
            m_socket.Create(localEP, null);
        }

        public void Destroy()
        {
            m_socket.Destroy();
        }

        public void Send(byte[] data, IPEndPoint remoteEP)
        {
            m_socket.SendTo(data, remoteEP, true, null);
        }
    }
}
