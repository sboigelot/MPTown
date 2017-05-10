using UnityEngine;

namespace Assets.Scripts.Controllers
{
    [RequireComponent(typeof(Transform))]
    public class ClientSideTraveller : MonoBehaviour
    {
        public bool Moving = false;

        public Vector3 Destination;

        public float Speed;

        public float DestinationTolerence;

        public bool RotateTowardDestination;
        
        public void FixedUpdate()
        {
            if(!Moving)
                return;

            var journey = Destination - transform.position;

            if (journey.magnitude >= DestinationTolerence)
            {
                var move = journey.normalized * Speed * Time.fixedDeltaTime;

                transform.position = new Vector3(
                    transform.position.x + move.x,
                    transform.position.y + move.y,
                    transform.position.z + move.z);

                if (RotateTowardDestination)
                {
                    //TODO improve, this is for 2D
                    transform.rotation = Destination.x > transform.position.x
                        ? new Quaternion(0f, 0f, 0f, 0f)
                        : new Quaternion(0f, 180f, 0f, 0f);
                }
            }
        }
    }
}