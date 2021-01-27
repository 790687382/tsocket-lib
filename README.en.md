# TSocket

####Introduction

It is a custom protocol analysis socket communication library by C#, using SocketAsyncEventArgs.

It allows you to customize the data protocol processing (such as TCP packet sticking processing), through a few simple calls, register a few event functions, you can carry out network communication without caring about the details of communication.

Support TCP server, TCP client, UDP client and multicast



####Origin

Socket is often used to communicate with different devices, and the communication protocol of each device is different, which makes me feel very puzzled: is it impossible to write a common communication library for all protocol？

I need a simple communication: simple code, simple call, good performance, business independent, thus development TSocketLib.



####Instructions

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