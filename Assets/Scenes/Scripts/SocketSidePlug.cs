using UnityEngine;

namespace NuclearCell
{
    public class SocketSidePlug : MonoBehaviour
    {
        public Transform PlugTransform;
        public Vector3 GrabAnchor;
        public bool Started = false;

        private Rigidbody _rigidbody;
        private Collider _collider;

        protected void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _collider = GetComponent<Collider>();
        }

        protected void FixedUpdate()
        {

        }

        protected void OnMouseDown()
        {
            var ogLayer = gameObject.layer;
            gameObject.layer = 31;
            LayerMask thisOnly = 1 << gameObject.layer;
            var ray = GameManager.Singleton.WallCam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit, 100, thisOnly))
            {

            }
            gameObject.layer = ogLayer;
        }

        protected void OnMouseUp()
        {
            Debug.Log("Bye");
        }
    }
}