using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace TSocketTest
{
    class Program
    {
        static void Main(string[] args)
        {
            //UDP
            var udpClient = new MyUDPClient();
            udpClient.Create(new IPEndPoint(IPAddress.Any, 40000));
            udpClient.Send(new byte[1024], new IPEndPoint(IPAddress.Parse("127.0.0.1"), 50000));


            //TCP-Server
            var tcpServer = new MyTCPServer();
            tcpServer.StartListen(new IPEndPoint(IPAddress.Any, 40001));

            Thread.Sleep(1000);

            //TCP-Client--带重连
            var tcpClient = new MyTCPClient();
            tcpClient.Connect(new IPEndPoint(IPAddress.Any, 0), new IPEndPoint(IPAddress.Parse("127.0.0.1"), 40001));
            tcpClient.Send(new byte[1024]);



            Console.WriteLine("------------输入任意行按Enter，断开所有");
            Console.ReadLine();

            //销毁
            udpClient.Destroy();
            tcpClient.Disconnect();
            tcpServer.StopListen();

            Console.WriteLine("------------已断开所有");
            Console.ReadLine();
        }
    }
}
