using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TSocketTest
{

    /// <summary>
    /// 协议包基类,此处是用户自定义的协议包
    /// </summary>
    abstract class ProtocolPackageBase
    {
        public abstract int PackType { get; }
    }


    class ProtocolPackageAAAAA : ProtocolPackageBase
    {
        public override int PackType 
        { 
            get 
            {
                return 1;
            }
        }

        public string Name { get; set; }
    }


    class ProtocolPackageBBBBB : ProtocolPackageBase
    {
        public override int PackType
        {
            get
            {
                return 2;
            }
        }

        public int Age { get; set; }
    }
}
