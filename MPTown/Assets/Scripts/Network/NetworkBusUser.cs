using System;
using System.Collections.Generic;
using Assets.Scripts.Data.Messages;
using Assets.Scripts.Helpers;
using UnityEngine;

namespace Assets.Scripts.Network
{
    public abstract class NetworkBusUser : MonoBehaviour
    {
        private readonly Dictionary<string, Action<object>> messageHandlers
            = new Dictionary<string, Action<object>>();

        protected NetworkBus NetworkBus;
        
        protected bool IsSinglePlayer
        {
            get
            {
                return NetworkBus == null || NetworkBus.LocalIdentity == null;
            }
        }

        public void RegisterMessageHandler<T>(Action<object> handler)
        {
            messageHandlers[typeof(T).Name] = handler;
        }

        public virtual void Start()
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
            NetworkBus.NetworkBusUsers.Add(this);
        }

        protected abstract void RegisterMessageHandlers();

        public virtual void OnDestroy()
        {
            if (NetworkBus != null)
            {
                NetworkBus.NetworkBusUsers.Remove(this);
            }
        }

        protected void SendIfServer<T>(T data, bool sendToSelf = false)
        {
            if (NetworkBus.IsServer)
            {
                NetworkBus.LocalBus.SendMessage(data, sendToSelf);
            }
        }

        protected void SendIfClient<T>(T data, bool sendToSelf = false)
        {
            if (!NetworkBus.IsServer)
            {
                NetworkBus.LocalBus.SendMessage(data, sendToSelf);
            }
        }

        protected void Send<T>(T data, bool sendToSelf = false)
        {
            if (IsSinglePlayer)
            {
                OnMessageReceived(new NetworkBusEnvelope(data));
            }
            else
            {
                NetworkBus.LocalBus.SendMessage(data, sendToSelf);
            }
        }

        public void OnMessageReceived(NetworkBusEnvelope envelope)
        {
            if (messageHandlers.ContainsKey(envelope.PayloadType))
            {
                var payLoad = envelope.Open<object>();
                messageHandlers[envelope.PayloadType](payLoad);
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