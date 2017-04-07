using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Scripts.Controllers
{
    [RequireComponent(typeof(NetworkBroadcastBus))]
    class PlayerChatcontrollerUsingNetworkBus : NetworkBehaviour
    {
        NetworkBroadcastBus networkBus;

        public void Start()
        {
            networkBus = GetComponent<NetworkBroadcastBus>();
            networkBus.OnMessageReceived += OnMessageReceived;
        }

        public void OnDestroy()
        {
            networkBus.OnMessageReceived -= OnMessageReceived;
        }

        private void OnMessageReceived(byte[] obj)
        {
            var msg = Encoding.UTF8.GetString(obj);
            contents.Add(msg);
        }

        public List<string> contents = new List<string>();
        public void OnGUI()
        {
            if (!isLocalPlayer)
                return;
            GUILayout.BeginVertical();
            if (GUILayout.Button("Send message"))
            {
                Send("Hello from me " + DateTime.Now.ToLongTimeString());
            }

            foreach (var text in contents)
            {
                GUILayout.Label(text);
            }

            GUILayout.EndVertical();
        }
        
        private void Send(string text)
        {
            var data = Encoding.UTF8.GetBytes(text);
            networkBus.SendMessage(data);
        }
    }
}
