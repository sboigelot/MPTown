using UnityEngine;

namespace PyralisStudio.TheCorp.Engine.Camera
{
    public class SimpleRtsCam : MonoBehaviour
    {
        public float ScrollSpeed = 15;

        public bool ScrollOnEdge = false;

        public float ScrollEdge = 0.1f;

        public float PanSpeed = 10;

        public Vector2 zoomRange = new Vector2(-10, 100);

        public float CurrentZoom = 0;

        public float ZoomZpeed = 1;

        public float ZoomRotation = 1;

        public Vector2 zoomAngleRange = new Vector2(20, 70);

        public float rotateSpeed = 10;

        private Vector3 initialPosition;

        private Vector3 initialRotation;

        public Bounds boundingBox = new Bounds(new Vector3(0f, 0f, 0f), new Vector3(500f, 500f, 500f));

        public void Start()
        {
            initialPosition = transform.position;
            initialRotation = transform.eulerAngles;
        }

        public void Update()
        {
            // panning     
            if (Input.GetMouseButton(1))
            {
                transform.Translate(
                    Vector3.right*Time.deltaTime*PanSpeed*(Input.mousePosition.x - Screen.width*0.5f) / (Screen.width*0.5f),
                    Space.World);
                transform.Translate(
                    Vector3.forward*Time.deltaTime*PanSpeed*(Input.mousePosition.y - Screen.height*0.5f) / (Screen.height*0.5f),
                    Space.World);
            }
            else
            {
                if (Input.GetKey("d") || ScrollOnEdge && Input.mousePosition.x >= Screen.width*(1 - ScrollEdge))
                {
                    transform.Translate(Vector3.right*Time.deltaTime*PanSpeed, Space.Self);
                }
                else if (Input.GetKey("q") || ScrollOnEdge && Input.mousePosition.x <= Screen.width*ScrollEdge)
                {
                    transform.Translate(Vector3.right*Time.deltaTime * -PanSpeed, Space.Self);
                }

                if (Input.GetKey("z") || ScrollOnEdge && Input.mousePosition.y >= Screen.height*(1 - ScrollEdge))
                {
                    transform.Translate(Vector3.forward*Time.deltaTime*PanSpeed, Space.Self);
                }
                else if (Input.GetKey("s") || ScrollOnEdge && Input.mousePosition.y <= Screen.height*ScrollEdge)
                {
                    transform.Translate(Vector3.forward*Time.deltaTime * -PanSpeed, Space.Self);
                }

                if (Input.GetKey("a"))
                {
                    transform.Rotate(Vector3.up*Time.deltaTime * -rotateSpeed, Space.World);
                }
                else if (Input.GetKey("e"))
                {
                    transform.Rotate(Vector3.up*Time.deltaTime * rotateSpeed, Space.World);
                }
            }

            // zoom in/out
            CurrentZoom -= Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * 1000 * ZoomZpeed;

            CurrentZoom = Mathf.Clamp(CurrentZoom, zoomRange.x, zoomRange.y);

            var newPos = new Vector3(
                transform.position.x,
                transform.position.y - (transform.position.y - (initialPosition.y + CurrentZoom)) * 0.1f,
                transform.position.z);

            transform.position = newPos;

            float x = transform.eulerAngles.x -
                      (transform.eulerAngles.x - (initialRotation.x + CurrentZoom*ZoomRotation)) * 0.1f;
            x = Mathf.Clamp(x, zoomAngleRange.x, zoomAngleRange.y);

            transform.eulerAngles = new Vector3(x, transform.eulerAngles.y, transform.eulerAngles.z);

            transform.position = boundingBox.ClosestPoint(transform.position);
        }
    }
}