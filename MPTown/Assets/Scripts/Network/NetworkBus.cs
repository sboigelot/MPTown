using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.Scripts.Helpers;
using UnityEngine.Networking;

namespace Assets.Scripts.Network
{
    public class NetworkBus : NetworkBehaviour
    {
        public const int MaxBufferSize = 1024;
        public static NetworkBus LocalBus;

        public static Action<NetworkBus> OnLocalBusFound;

        public static NetworkIdentity LocalIdentity;

        private readonly Queue<NetworkBusEnvelope> inbox = new Queue<NetworkBusEnvelope>();

        private readonly Queue<NetworkBusEnvelope> outbox = new Queue<NetworkBusEnvelope>();

        private readonly Dictionary<Guid, List<NetworkBusEnvelope>> subBox = new Dictionary<Guid, List<NetworkBusEnvelope>>();

        public Action<NetworkBusEnvelope> OnMessageReceived;

        public static bool IsServer
        {
            get { return LocalIdentity.isServer; }
        }

        public override void OnStartLocalPlayer()
        {
            LocalBus = this;
            LocalIdentity = gameObject.GetComponent<NetworkIdentity>();
            if (OnLocalBusFound != null)
            {
                OnLocalBusFound(LocalBus);
            }
        }

        public void Update()
        {
            if (!isLocalPlayer)
                return;

            while (outbox.Count > 0)
            {
                Dispatch(outbox.Dequeue());
            }

            if (OnMessageReceived != null)
            {
                while (inbox.Count != 0)
                {
                    OnMessageReceived(inbox.Dequeue());
                }
            }
        }

        public void SendMessage<T>(T message)
        {
            outbox.Enqueue(new NetworkBusEnvelope(message));
        }

        private void Dispatch(NetworkBusEnvelope message)
        {
            if (!isLocalPlayer)
                return;

            var data = BinarySerializationHelper.Serialize(message);
            var packets = new List<byte[]> {data};

            if (data.Length > MaxBufferSize)
            {
                packets.Clear();
                var groupId = Guid.NewGuid();
                var groupSize = (int) Math.Ceiling((float) data.Length / MaxBufferSize);

                for (var i = 0; i < groupSize; i ++)
                {
                    var payload = data.Skip(i * MaxBufferSize).Take(MaxBufferSize).ToArray();
                    var subEnveloppe = new NetworkBusEnvelope(
                        groupId,
                        i,
                        groupSize,
                        payload);

                    var packet = BinarySerializationHelper.Serialize(subEnveloppe);
                    packets.Add(packet);
                }
            }

            foreach (var packet in packets)
            {
                if (isServer)
                {
                    Broadcast(packet);
                }
                else
                {
                    CmdSendToServer(packet);
                }
            }
        }

        [Command]
        public void CmdSendToServer(byte[] data)
        {
            Broadcast(data);
        }

        public void Broadcast(byte[] data)
        {
            foreach (var client in NetworkServer.connections)
            {
                var player = client.playerControllers[0].gameObject;
                var chatController = player.GetComponent<NetworkBus>();
                chatController.RpcSendToClients(data);
            }
        }

        [ClientRpc]
        public void RpcSendToClients(byte[] data)
        {
            var message = BinarySerializationHelper.Deserialize<NetworkBusEnvelope>(data);

            if (message.PayloadType == "Sub" && message.GroupId != Guid.Empty)
            {
                EnqueueSubMessage(message);
            }
            else
            {
                inbox.Enqueue(message);
            }
        }

        private void EnqueueSubMessage(NetworkBusEnvelope sub)
        {
            if (!subBox.ContainsKey(sub.GroupId))
            {
                subBox.Add(sub.GroupId, new List<NetworkBusEnvelope>());
            }

            var group = subBox[sub.GroupId];
            group.Add(sub);

            if (group.Count == sub.GroupTotal)
            {
                RebuildSubGroup(sub.GroupId);
            }
        }

        private void RebuildSubGroup(Guid subGroupId)
        {
            var group = subBox[subGroupId];
            subBox.Remove(subGroupId);

            var buffer = new List<byte>();
            foreach (var sub in group.OrderBy(sub=>sub.Index))
            {
                buffer.AddRange(sub.Payload);
            }

            var message = BinarySerializationHelper.Deserialize<NetworkBusEnvelope>(buffer.ToArray());
            inbox.Enqueue(message);
        }
    }
}