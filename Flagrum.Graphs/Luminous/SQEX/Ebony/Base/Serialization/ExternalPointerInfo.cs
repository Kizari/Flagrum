using System;
using System.Collections.Generic;

namespace Ebony.Base.Serialization
{
    public class ExternalPointerInfo
    {
        private ushort Protocol { get; }
        private IList<object> Params { get; } = new List<object>();
        private IList<string> Keys { get; } = new List<string>();
        private object PointerAddress { get; }

        public ExternalPointerInfo(ushort protocol, object pointerAddress)
        {
            this.Protocol = protocol;
            this.PointerAddress = pointerAddress;
        }

        public void AddKey(string key)
        {
            this.Keys.Add(key);
        }

        public IList<object> GetParamsBuffer()
        {
            return this.Params;
        }
    }
}
