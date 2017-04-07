using System;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace Assets.Scripts.Controllers
{
    class NetworkBroadcastBus : NetworkBehaviour
    {
        private Queue<byte[]> Outbox = new Queue<byte[]>();

        private Queue<byte[]> Inbox = new Queue<byte[]>();

        public Action<byte[]> OnMessageReceived;
        
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

        public void SendMessage(byte[] message)
        {
            Outbox.Enqueue(message);
        }

        private void Dispatch(byte[] message)
        {
            if (!isLocalPlayer)
                return;

            if (isServer)
            {
                Broadcast(message);
            }
            else
            {
                CmdSendToServer(message);
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
            Inbox.Enqueue(data);
        }
    }
}
