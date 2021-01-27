# TSocket

#### 介绍
C# 编写的一款自定义协议解析的socket通信库，使用SocketAsyncEventArgs。
它允许您自定义数据协议处理(如TCP粘包处理)，通过简单的几个调用，注册几个事件函数就能进行网络通信，而不必关心通信的细节。
支持TCP-Server\TCP-Client\UDP-Client\Multicast.

#### 由来
经常使用socket与不同设备的通信，每个设备的通信协议都不一样，这让我觉得很困扰：不可能针对每个协议编写一套通信类，把业务耦合到一起。
我需要一个简单的通信：代码简洁、调用简单、性能良好、业务无关，由此开发。

#### 使用
1.首先你需要定义你的应用层协议数据包基类，子类代表了你的数据协议对象：见TSocketTest\ProtocolPackage.cs；
2.定义你的协议解析类：见TSocketTest\ProtocolBuilder.cs；
  这个类的作用是：
       在发送数据时，可以对其编码，如添加包头包尾；当然也可以不编码处理直接发送(send()函数的encode=false即可)；
       在收到数据时，可以对其解码，如TCP粘包处理，完成后通过事件发布回调你的ProtocolPackage数据包。
3.参考TSocketTest\MyUDPClient.cs、TSocketTest\MyTCPServer.cs、TSocketTest\MyTCPClient.cs 建立网络连接；
  建立网络连接非常简单，通常4步：
  1.调用工厂函数TSocket\SocketNetFactory.cs创建对应类型的通信接口，包括TCP-Client、TCP-Server、UDP-Client，泛型参数即是1.2中的协议包基类和协议解析对象；
  2.注册事件函数：主要包括网络状态事件通知、异常事件通知、收到数据事件通知；
  3.调用相关方法启动网络，如TCP服务启动监听、TCP客户端创建连接、UDP创建等。
4.接下来就非常简单了，调用接口的相关方法进行数据发送、处理网络事件即可。

####-------------------------------------------------------------------------------------------------

####Introduction

It is a custom protocol analysis socket communication library by C#, using SocketAsyncEventArgs.

It allows you to customize the data protocol processing (such as TCP packet sticking processing), through a few simple calls, register a few event functions, you can carry out network communication without caring about the details of communication.

Support TCP server, TCP client, UDP client and multicast



####Origin

Socket is often used to communicate with different devices, and the communication protocol of each device is different, which makes me feel very puzzled: is it impossible to write a common communication library for all protocol？

I need a simple communication: simple code, simple call, good performance, business independent, thus development TSocketLib.



####Use

1. First of all, you need to define your application layer protocol packet base class. The subclass represents your data protocol object: see tsockettest\ ProtocolPackage.cs ；

2. Define your protocol parsing/building class: see tsockettest\ ProtocolBuilder.cs ；

The function of this class is:

When sending data, you can encode it, such as adding the head or tail into the packet; of course, you can send it directly without encoding (send(....,encode = false ) function);

When data is received, it can be decoded, such as TCP packet sticking. After that, publish your protocolpackage packet through event callback.

3. Refer to tsockettest\ MyUDPClient.cs 、TSocketTest\ MyTCPServer.cs 、TSocketTest\ MyTCPClient.cs Establish network connection;

Establishing a network connection is very simple, usually in 4 steps:

1. Call the factory function tsocket\ SocketNetFactory.cs Create corresponding types of communication interfaces, including TCP client, TCP server and UDP client. The T params are "ProtocolPackage" and "ProtocolBuilder" ;

2. Registration event function: it mainly includes network status event notification, exception event notification and data event notification;

3. Call relevant methods to start the network, such as TCP service start monitoring, TCP client create connection, UDP create, etc.

4. Next, it is very simple to call the relevant methods of the interface to send data and handle network events.