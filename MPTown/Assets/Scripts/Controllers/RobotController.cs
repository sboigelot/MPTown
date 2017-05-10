using Assets.Scripts.Data;
using Assets.Scripts.Network;
using UnityEngine;

namespace Assets.Scripts.Controllers
{
    [RequireComponent(typeof(ClientSideTraveller))]
    public class RobotController : NetworkBusUser
    {
        public RobotData Robot;

        public void Update()
        {
            
        }

        public void Spawn()
        {
            
        }

        protected override void RegisterMessageHandlers()
        {
        }
    }
}