using UnityEngine;

namespace NuclearCell
{
    public class SocketSidePlug : MonoBehaviour
    {
        public Transform PlugTransform;
        public Vector3 GrabAnchor;
        public bool Started = false;
        public bool Picked = false;

        private Rigidbody _rigidbody;
        private Collider _collider;
        private Vector3 _targetPos;

        protected void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _collider = GetComponent<Collider>();
            _targetPos = transform.position;
        }

        protected void Update()
        {
            if (Started)
            {
                if (Picked)
                {
                    var mousePos = GameManager.Singleton.WallCam.ScreenToWorldPoint(Input.mousePosition);
                    _targetPos = new Vector3(mousePos.x - GrabAnchor.x, mousePos.y - GrabAnchor.y, transform.position.z);
                }
            }
        }

        protected void FixedUpdate()
        {
            if (Started)
            {
                if (Picked)
                {
                    if (_rigidbody.useGravity)
                    {
                        _rigidbody.useGravity = false;
                    }
                    _rigidbody.MovePosition(_targetPos);
                }
                else if (!Picked && !_rigidbody.useGravity)
                {
                    _rigidbody.velocity = Vector3.zero;
                    _rigidbody.useGravity = true;
                }
            }

        }

        protected void OnMouseDown()
        {
            var ogLayer = gameObject.layer;
            gameObject.layer = 31;
            LayerMask thisOnly = 1 << gameObject.layer;
            var ray = GameManager.Singleton.WallCam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit, 100, thisOnly))
            {
                Picked = true;
                Started = true;
                GrabAnchor = hit.point - _rigidbody.position;
            }
            gameObject.layer = ogLayer;
        }

        protected void OnMouseUp()
        {
            Picked = false;
        }
    }
}