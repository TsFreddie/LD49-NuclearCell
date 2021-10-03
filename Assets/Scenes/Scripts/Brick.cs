using UnityEngine;
#if UNITY_EDITOR 
using UnityEditor;
#endif

namespace NuclearCell
{
    public class Brick : MonoBehaviour
    {
        public Transform PlugTransform;
        public Vector3 GrabAnchor;
        public bool Started = false;
        public bool Picked = false;
        public bool Dropped = false;

        [Header("Physics")]
        public float TerminalVelocityX = 0.5f;
        public float TerminalVelocityY = 1.0f;

        [Header("Arguments")]
        public float RandomTorqueScale = 5.0f;

        [Header("Gameplay")]
        public int Type;
        public SocketOrientation Orientation;

        private Rigidbody _rigidbody;
        private Vector3 _targetPos;
        private float _baseZ;
        private float _targetZ;
        private float _targetScale;
        private bool _pickState;
        private Transform _connectedTransform = null;

        protected void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _targetPos = transform.position;
            _baseZ = transform.position.z;
            _targetScale = 1.0f;
            _pickState = false;
        }

        protected void Update()
        {
            if (Started && !Dropped && _connectedTransform == null)
            {
                if (Picked)
                {
                    var mousePos = GameManager.Singleton.WallCam.ScreenToWorldPoint(Input.mousePosition);
                    _targetPos = new Vector3(mousePos.x - GrabAnchor.x, mousePos.y - GrabAnchor.y, _baseZ + _targetZ);
                }
            }
            else
            {
                if (_connectedTransform)
                {
                    transform.position = Vector3.Lerp(transform.position, _connectedTransform.position - PlugTransform.localPosition, 15.0f * Time.deltaTime);
                    // TODO: support rotation
                    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.identity, 15.0f * Time.deltaTime);
                }
                else if (!Started)
                {
                    transform.position = Vector3.Lerp(transform.position, _targetPos, 15.0f * Time.deltaTime);
                }
            }
        }

        protected void FixedUpdate()
        {
            if (Started && !Dropped && _connectedTransform == null)
            {
                if (Picked)
                {
                    if (!_pickState)
                    {
                        Pick();
                    }
                    _rigidbody.MovePosition(_targetPos);
                    _rigidbody.rotation = Quaternion.Lerp(transform.rotation, Quaternion.identity, Time.deltaTime * 5);
                }
                else if (!Picked && _pickState)
                {
                    if (!Mount())
                    {
                        Drop();
                    }
                }
            }
            var scale = Mathf.Lerp(transform.localScale.x, _targetScale, Time.deltaTime * 15);
            transform.localScale = new Vector3(scale, scale, scale);

            // clamp velocity
            var vel = _rigidbody.velocity;
            if (vel.x > TerminalVelocityX)
                vel.x = TerminalVelocityX;
            else if (vel.x < -TerminalVelocityX)
                vel.x = -TerminalVelocityX;

            if (vel.y > TerminalVelocityY)
                vel.y = TerminalVelocityY;
            else if (vel.y < -TerminalVelocityY)
                vel.y = -TerminalVelocityY;
            _rigidbody.velocity = vel;
        }

        public void Pick()
        {
            _rigidbody.isKinematic = true;
            _targetScale = 1.0f;
            _targetZ = -1.5f;
            _pickState = true;
        }

        public void Drop(bool forceDrop = false)
        {
            transform.localScale = Vector3.one;
            _rigidbody.isKinematic = false;
            _rigidbody.velocity = Vector3.zero;
            _pickState = false;
            _targetScale = 1.0f;
            _targetZ = 0.0f;
            // add random torque for fun
            _rigidbody.AddTorque(new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * Random.Range(1.0f * RandomTorqueScale, 3.0f * RandomTorqueScale), ForceMode.Impulse);

            if (forceDrop)
            {
                Dropped = true;
            }
            else
            {
                _rigidbody.AddForce(new Vector3(0, 0, 15.0f), ForceMode.Impulse);
            }
        }

        public bool Mount()
        {
            foreach (var socket in GameManager.Singleton.SocketMounts)
            {
                if (socket.CanMount(Type))
                {
                    var mountScore = socket.TryMount(PlugTransform);
                    if (mountScore >= 0)
                    {
                        _connectedTransform = socket.SocketTransform;
                        _rigidbody.isKinematic = true;
                        return true;
                    }
                }
            }
            return false;
        }

        protected void OnMouseDown()
        {
            if (!Dropped)
            {
                var ray = GameManager.Singleton.WallCam.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out var hit, 100))
                {
                    Picked = true;
                    Started = true;
                    GrabAnchor = hit.point - _rigidbody.position;
                }
            }
        }

        protected void OnTriggerEnter(Collider other)
        {
            if (other.tag == "DropZone")
            {
                if (!Dropped)
                {
                    // TODO!: trigger gameover
                }
                Destroy(gameObject);
            }
        }

        protected void OnMouseUp()
        {
            Picked = false;
        }

#if UNITY_EDITOR 
        protected void OnDrawGizmos()
        {
            // Draw a yellow sphere at the transform's position
            var plugOnScreen = Camera.current.WorldToScreenPoint(PlugTransform.position);
            plugOnScreen.y -= 24;
            Handles.Label(Camera.current.ScreenToWorldPoint(plugOnScreen), "BrickType " + Type.ToString() + " (" + Orientation.ToString() + ")", "sv_label_7");
        }
#endif
    }
}