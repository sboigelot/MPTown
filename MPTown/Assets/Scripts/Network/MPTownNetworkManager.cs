using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Scripts.Network
{
    public class MPTownNetworkManager : NetworkManager
    {
        public override void OnServerConnect(NetworkConnection conn)
        {
            Debug.Log("OnPlayerConnected");
        }

        public void OnClickHost()
        {
            this.StartHost(this.connectionConfig, this.maxConnections);
        }
    }
}
