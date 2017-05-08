using Assets.Scripts.Network;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Scripts.Controllers
{
    [RequireComponent(typeof(NetworkBus))]
    class PlayerChatcontrollerUsingNetworkBus : NetworkBehaviour
    {
        NetworkBus networkBus;

        public void Start()
        {
            networkBus = GetComponent<NetworkBus>();
            networkBus.OnMessageReceived += OnMessageReceived;
        }

        public void OnDestroy()
        {
            networkBus.OnMessageReceived -= OnMessageReceived;
        }

        private void OnMessageReceived(NetworkBusEnvelope envelope)
        {
            if (envelope.PayloadType == "String")
            {
                var msg = envelope.Open<string>();
                contents.Add(msg);
            }
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
            networkBus.SendMessage<string>(text);
        }
    }
}
