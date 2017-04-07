using Assets.Scripts.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Network
{
    [Serializable]
    public class NetworkBusEnvelope
    {
        public string PayloadType;
        public byte[] Payload;

        public T Open<T>()
        {
            return BinarySerializationHelper.Deserialize<T>(Payload);
        }

        public NetworkBusEnvelope(object payload)
        {
            Payload = BinarySerializationHelper.Serialize(payload);
            PayloadType = payload.GetType().Name;
        }
    }
}
