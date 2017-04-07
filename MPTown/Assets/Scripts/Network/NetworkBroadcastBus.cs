using Assets.Scripts.Helpers;
using Assets.Scripts.Network;
using System;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace Assets.Scripts.Controllers
{
    public class NetworkBroadcastBus : NetworkBehaviour
    {
        public static NetworkBroadcastBus LocalBus;

        public static Action<NetworkBroadcastBus> OnLocalBusFound;

        public static NetworkIdentity LocalIdentity;
 
        public static bool IsServer
        {
            get { return LocalIdentity.isServer; }
        }

        public override void OnStartLocalPlayer()
        {
            LocalBus = this;
            LocalIdentity = this.gameObject.GetComponent<NetworkIdentity>();
            if(OnLocalBusFound!=null)
            {
                OnLocalBusFound(LocalBus);
            }
        }

        private Queue<NetworkBusEnvelope> Outbox = new Queue<NetworkBusEnvelope>();

        private Queue<NetworkBusEnvelope> Inbox = new Queue<NetworkBusEnvelope>();

        public Action<NetworkBusEnvelope> OnMessageReceived;
        
        public void Update()
        {
            if (!isLocalPlayer)
                return;

            while(Outbox.Count > 0)
            {
                Dispatch(Outbox.Dequeue());
            }

            if (OnMessageReceived != null)
            {
                while (Inbox.Count != 0)
                {
                    OnMessageReceived(Inbox.Dequeue());
                }
            }
        }

        public void SendMessage<T>(T message)
        {
            Outbox.Enqueue(new NetworkBusEnvelope(message));
        }

        private void Dispatch(NetworkBusEnvelope message)
        {
            if (!isLocalPlayer)
                return;

            if (isServer)
            {
                var data = BinarySerializationHelper.Serialize(message);
                Broadcast(data);
            }
            else
            {
                var data = BinarySerializationHelper.Serialize(message);
                CmdSendToServer(data);
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
                var chatController = player.GetComponent<NetworkBroadcastBus>();
                chatController.RpcSendToClients(data);
            }
        }

        [ClientRpc]
        public void RpcSendToClients(byte[] data)
        {
            var message = BinarySerializationHelper.Deserialize<NetworkBusEnvelope>(data);
            Inbox.Enqueue(message);
        }
    }
}
