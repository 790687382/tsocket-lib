using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using TSocket;

namespace TSocketTest
{
    /// <summary>
    /// 实现自定义的协议解析
    /// </summary>
    class ProtocolBuilder : AbsProtocolBuilder<ProtocolPackageBase>
    {

        public override bool EncodeParse(byte[] src, int srcOffset, int srcLength, ref byte[] dest, ref int destOffset, ref int destLength, object userTag = null)
        {
            //1.这里实现自定义的数据编码，如添加包头、包尾
            //....
            //....代码省略



            //2.不编码，直接返回也是可以的
            dest = src;
            destOffset = srcOffset;
            destLength = srcLength;
            return true;
        }


        public override void DecodeParse(EnumNetworkType netType, byte[] data, int offset, int length, IPEndPoint remoteEP, IPEndPoint localEP)
        {
            //1.这里实现自定义数据解码，如TCP的粘包处理
            //....
            //....代码省略
            //....
            //解析一包完成后记得发布事件,类似如下
            var packA = new ProtocolPackageAAAAA()
            {
                Name = "我是收到的AAAAA数据",
            };
            base.PublishData(netType, remoteEP, localEP, packA);

        }

        public override void Reset()
        {
            //网络断开后会调用此函数，用户自定义清理，如清理粘包处理的缓冲区
        }

        public override void Dispose()
        {
            //销毁网络会调用此函数
        }
    }
}
