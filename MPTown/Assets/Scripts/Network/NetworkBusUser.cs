using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Network
{
    public abstract class NetworkBusUser : MonoBehaviour
    {
        private readonly Dictionary<string, Action<NetworkBusEnvelope>> messageHandlers
            = new Dictionary<string, Action<NetworkBusEnvelope>>();

        protected NetworkBus NetworkBus;

        public void RegisterMessageHandler<T>(Action<NetworkBusEnvelope> handler)
        {
            messageHandlers[typeof(T).Name] = handler;
        }

        public void Start()
        {
            RegisterMessageHandlers();
            if (NetworkBus.LocalBus != null)
            {
                OnLocalBusFound(NetworkBus.LocalBus);
            }
            else
            {
                NetworkBus.OnLocalBusFound += OnLocalBusFound;
            }
        }

        private void OnLocalBusFound(NetworkBus bus)
        {
            NetworkBus = bus;
            NetworkBus.OnMessageReceived += OnMessageReceived;
        }

        protected abstract void RegisterMessageHandlers();

        public void OnDestroy()
        {
            if (NetworkBus != null)
            {
                NetworkBus.OnMessageReceived -= OnMessageReceived;
            }
        }

        protected void SendIfServer<T>(T data)
        {
            if (NetworkBus.IsServer)
            {
                NetworkBus.LocalBus.SendMessage(data);
            }
        }

        protected void SendIfClient<T>(T data)
        {
            if (!NetworkBus.IsServer)
            {
                NetworkBus.LocalBus.SendMessage(data);
            }
        }

        protected void Send<T>(T data)
        {
            NetworkBus.LocalBus.SendMessage(data);
        }

        private void OnMessageReceived(NetworkBusEnvelope envelope)
        {
            if (messageHandlers.ContainsKey(envelope.PayloadType))
            {
                messageHandlers[envelope.PayloadType](envelope);
            }
            else
            {
                HandleUnHandledMessage(envelope);
            }
        }

        protected virtual void HandleUnHandledMessage(NetworkBusEnvelope envelope)
        {
        }
    }
}