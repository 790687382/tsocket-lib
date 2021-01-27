using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using TSocket;

namespace TSocketTest
{
    class MyTCPClient
    {
        ISocketNetClient<ProtocolPackageBase> m_socket = null;
        public MyTCPClient()
        {
            //创建对象，添加事件
            m_socket = SocketNetFactory.CreateTCPClient<ProtocolBuilder, ProtocolPackageBase>();
            m_socket.OnSocketStatusChangedEvent += M_socket_OnSocketStatusChangedEvent;
            m_socket.OnDataReceivedEvent += M_socket_OnDataReceivedEvent;
            m_socket.OnExceptionHappenedEvent += M_socket_OnExceptionHappenedEvent;
        }


        #region 事件
        private void M_socket_OnSocketStatusChangedEvent(object arg1, SocketStatusChangedArgs<ProtocolPackageBase> arg2)
        {
            Console.WriteLine("TCP-Client连接状态改变:" + arg2.Status);


            //展示处理TCP的断线重连
            if (m_socket.NetProtocolType == EnumNetworkType.TCP)
            {
                if (arg2.Status == EnumNetworkStatus.Shutdown && !m_socket.Disposed)
                {
                    new Thread(new ThreadStart(() => 
                    {
                        try
                        {
                            //休眠5s，避免不断重连，
                            Thread.Sleep(5000);
                            m_socket.Create();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("TCP-Client重连失败");
                        }
                    })).Start();
                }
            }
        }

        private void M_socket_OnExceptionHappenedEvent(object arg1, ExceptionHappenedArgs arg2)
        {
            Console.WriteLine("TCP-Client异常通知:" + arg2.Description);
        }

        private void M_socket_OnDataReceivedEvent(object arg1, DataReceivedArgs<ProtocolPackageBase> arg2)
        {
            Console.WriteLine("TCP-Client收到数据");
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


        public void Connect(IPEndPoint localEP, IPEndPoint serverEP)
        {
            m_socket.Create(localEP, serverEP);
        }

        public void Disconnect()
        {
            m_socket.Destroy();
        }

        public void Send(byte[] data)
        {
            m_socket.Send(data, true, null);
        }
    }
}
