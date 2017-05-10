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
        private void OnStringReceived(string msg)
        {
            contents.Add(msg);
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
            RegisterMessageHandler<string>((payload) => OnStringReceived((string)payload));
        }
    }
}
