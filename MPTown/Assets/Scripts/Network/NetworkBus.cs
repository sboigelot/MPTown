using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using Assets.Scripts.Helpers;
using UnityEngine;
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

        private readonly Queue<OutgoingNetworkBusEnvelope> outbox = new Queue<OutgoingNetworkBusEnvelope>();

        private readonly Dictionary<Guid, List<NetworkBusEnvelope>> subBox = new Dictionary<Guid, List<NetworkBusEnvelope>>();
        
        public List<NetworkBusUser> NetworkBusUsers = new List<NetworkBusUser>();

        public static bool IsServer
        {
            get { return LocalIdentity != null && LocalIdentity.isServer; }
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

            if (outbox.Count > 0)
            {
                Dispatch(outbox.Dequeue());
            }

            if (NetworkBusUsers.Any())
            {
                while (inbox.Count != 0)
                {
                    var msg = inbox.Dequeue();
                    foreach (var networkBusUser in NetworkBusUsers)
                    {
                        networkBusUser.OnMessageReceived(msg);
                    }
                }
            }
        }

        public void OnGUI()
        {
            GUILayout.BeginVertical();
            GUILayout.Space(150);
            GUILayout.TextField(string.Format("inbox: {0}, outbox: {1}",inbox.Count,outbox.Count));

            foreach (var subBoxKey in subBox.Keys)
            {
                GUILayout.TextField(string.Format("sub: {0}, {1} / {2}", subBoxKey, subBox[subBoxKey].Count, subBox[subBoxKey][0].GroupTotal));
            }

            GUILayout.EndVertical();
        }

        public void SendMessage<T>(T message)
        {
            SendMessage(message, false);
        }

        public void SendMessage<T>(T message, bool sendToSelf)
        {
            outbox.Enqueue(
                new OutgoingNetworkBusEnvelope
                {
                    Envelope = new NetworkBusEnvelope(message),
                    SendToSelf = sendToSelf
                });
        }

        private void Dispatch(OutgoingNetworkBusEnvelope outgoingNetworkBusEnvelope)
        {
            var message = outgoingNetworkBusEnvelope.Envelope;

            if (!isLocalPlayer)
                return;

            var data = BinarySerializationHelper.Serialize(message);

            if (data.Length > MaxBufferSize)
            {
                var splitSize = MaxBufferSize / 2;
                var groupId = Guid.NewGuid();
                var groupSize = (int) Math.Ceiling((float) data.Length / splitSize);

                for (var i = 0; i < groupSize; i ++)
                {
                    var payload = data.Skip(i * splitSize).Take(splitSize).ToArray();
                    var subEnveloppe = new NetworkBusEnvelope(
                        groupId,
                        i,
                        groupSize,
                        payload);
                    
                    outbox.Enqueue(
                        new OutgoingNetworkBusEnvelope
                        {
                            Envelope = subEnveloppe,
                            SendToSelf = outgoingNetworkBusEnvelope.SendToSelf
                        });
                }

                return;
            }
            
            if (isServer)
            {
                Broadcast(data, outgoingNetworkBusEnvelope.SendToSelf);
            }
            else
            {
                CmdSendToServer(data);
            }
        }

        [Command]
        public void CmdSendToServer(byte[] data)
        {
            Broadcast(data, false);
        }

        public void Broadcast(byte[] data, bool sendToSelf)
        {
            foreach (var client in NetworkServer.connections)
            {
                var playerController = client.playerControllers[0];
                if (playerController.playerControllerId == LocalIdentity.playerControllerId)
                {
                    //TODO this doesn't seem to allow network transfert anymore
                    //if (!sendToSelf)
                    //{
                    //    continue;
                    //}

                    //TODO possible bypass to not sent to self through socket
                    //var message = BinarySerializationHelper.Deserialize<NetworkBusEnvelope>(data);
                    //inbox.Enqueue(message);
                    //continue;
                }

                var player = playerController.gameObject;
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