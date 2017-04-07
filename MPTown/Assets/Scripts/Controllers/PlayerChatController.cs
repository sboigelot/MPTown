using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Scripts.Controllers
{
    class PlayerChatController : NetworkBehaviour
    {
        public List<string> contents = new List<string>();

        public void OnGUI()
        {
            if (!isLocalPlayer)
                return;

            GUILayout.BeginVertical();
            if (isServer)
            {
                if (GUILayout.Button("Send to clients"))
                {
                    Broadcast("Hello from server " + DateTime.Now.ToLongTimeString());
                }
            }
            else
            {
                if (GUILayout.Button("Send to Server"))
                {
                   CmdSendToServer("Hello from client " + DateTime.Now.ToLongTimeString());
                }
            }

            foreach (var text in contents)
            {
                GUILayout.Label(text);
            }

            GUILayout.EndVertical();
        }

        [Command]
        public void CmdSendToServer(string text)
        {
            Broadcast("Relay: " + text);
        }

        public void Broadcast(string text)
        {
            foreach(var client in NetworkServer.connections)
            {
                var player = client.playerControllers[0].gameObject;
                var chatController = player.GetComponent<PlayerChatController>();
                chatController.RpcSendToClients(text);
            }
        }

        [ClientRpc]
        public void RpcSendToClients(string text)
        {
            contents.Add(text);
        }
    }
}
