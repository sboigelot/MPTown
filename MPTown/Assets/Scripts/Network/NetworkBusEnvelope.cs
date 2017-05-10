using System;
using Assets.Scripts.Helpers;

namespace Assets.Scripts.Network
{
    [Serializable]
    public class NetworkBusEnvelope
    {
        public byte[] Payload;
        public string PayloadType;
        public Guid GroupId;
        public int Index;
        public int GroupTotal;
        
        public NetworkBusEnvelope(object payload)
        {
            Payload = BinarySerializationHelper.Serialize(payload);
            PayloadType = payload.GetType().Name;
        }

        public NetworkBusEnvelope(Guid groupId, int index, int groupTotal, byte[] payload)
        {
            GroupId = groupId;
            Index = index;
            GroupTotal = groupTotal;
            Payload = payload;
            PayloadType = "Sub";
        }

        public T Open<T>()
        {
            return BinarySerializationHelper.Deserialize<T>(Payload);
        }
    }
}