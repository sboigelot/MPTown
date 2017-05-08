using Assets.Scripts.Network;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Scripts.Controllers
{
    [RequireComponent(typeof(NetworkBus))]
    class PlayerChatcontrollerUsingNetworkBus : NetworkBusUser
    {
        NetworkBus networkBus;
        
        private void OnStringReceived(NetworkBusEnvelope envelope)
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
            if (!NetworkBus.isLocalPlayer)
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
            base.Send(text);
        }

        protected override void RegisterMessageHandlers()
        {
            RegisterMessageHandler<string>((envelope) => OnStringReceived(envelope));
        }
    }
}
