using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR 
using UnityEditor;
#endif

namespace NuclearCell
{
    public class Plug : MonoBehaviour
    {
        public Transform PlugTransform;

        public float MoveSpeed = 1f;
        public float DropSpeed = 1f;
        public float SpeedUpMoveSpeed = 2.5f;

        public float SuccessTolerance = 0.06f;

        public bool PreStart = false;
        public Transform StartingTransform;
        public bool Started = false;

        [Header("Gameplay")]
        public int Type;

        private Rigidbody _rigidbody;
        private Transform _targetTransform = null;

        private bool _pressingDrop = false;

        protected void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        protected void Update()
        {
            if (PreStart)
            {
                if (StartingTransform != null)
                {
                    if ((transform.position - StartingTransform.position).sqrMagnitude < 0.001f)
                    {
                        PreStart = false;
                        Started = true;
                        transform.position = StartingTransform.position;
                        transform.rotation = StartingTransform.rotation;
                    }
                    else
                    {
                        transform.position = Vector3.Lerp(transform.position, StartingTransform.position, 15.0f * Time.deltaTime);
                        transform.rotation = Quaternion.Lerp(transform.rotation, StartingTransform.rotation, 15.0f * Time.deltaTime);
                    }
                }
                else
                {
                    PreStart = false;
                    Started = true;
                }
            }

            if (!Started)
            {
                _pressingDrop = false;
                return;
            }

            if (Input.GetKeyDown(KeyCode.Space))
                _pressingDrop = true;

            if (Input.GetKeyUp(KeyCode.Space))
                _pressingDrop = false;

            if (_targetTransform != null)
            {
                transform.position = Vector3.Lerp(transform.position, _targetTransform.position - PlugTransform.localPosition, 15.0f * Time.deltaTime);
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.identity, 15.0f * Time.deltaTime);
            }

            if (transform.position.z > 7.6 ||
                transform.position.z < -8 ||
                transform.position.x < -5 ||
                transform.position.x > 5 ||
                transform.position.y > 2 ||
                transform.position.y < -2
            )
            {
                GameManager.Singleton.StartPlugSelection();
                Destroy(gameObject);
            }
        }

        protected void FixedUpdate()
        {
            if (!Started) return;
            if (_targetTransform == null)
            {
                var move = Input.GetAxis("Horizontal");
                var dropSpeed = _pressingDrop ? SpeedUpMoveSpeed : DropSpeed;
                _rigidbody.velocity = new Vector3(move * MoveSpeed, 0, dropSpeed);
            }
        }

        // TODO!: do success and gameover logic
        protected void OnCollisionEnter(Collision collision)
        {
            var phone = collision.gameObject.GetComponentSmart<Phone>();
            if (phone == null) return;
            if (phone.Type != Type)
            {
                Debug.Log("Explode");
                return;
            }

            var deltaPos = Mathf.Abs(phone.PortTransform.position.x - PlugTransform.position.x);
            if (deltaPos <= SuccessTolerance)
            {
                var rating = Mathf.Min(((int)((1.0f - (deltaPos / SuccessTolerance)) * 10f) * 10) + 60, 100);
                Connected(phone.PortTransform);
                // SUCCESS!
                GameManager.Singleton.PlugSuccess(this, phone.Session, rating);
            }
            else
            {
                Debug.Log("Explode");
            }
        }

        public void Connected(Transform target)
        {
            _rigidbody.isKinematic = true;
            _targetTransform = target;
        }

#if UNITY_EDITOR 
        protected void OnDrawGizmos()
        {
            // Draw a yellow sphere at the transform's position
            var plugOnScreen = Camera.current.WorldToScreenPoint(PlugTransform.position);
            plugOnScreen.y -= 24;
            Handles.Label(Camera.current.ScreenToWorldPoint(plugOnScreen), "PlugType " + Type.ToString(), "sv_label_7");
        }
#endif
    }
}
