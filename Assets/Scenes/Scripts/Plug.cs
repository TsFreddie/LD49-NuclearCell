using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace NuclearCell
{
    public class Plug : MonoBehaviour
    {
        public Transform PlugTransform;

        public float MoveSpeed = 1f;
        public float DropSpeed = 1f;
        public float SpeedUpMoveSpeed = 2.5f;

        public float SuccessTolerance = 0.06f;

        [Header("Gameplay")]
        public int Type;

        private Rigidbody _rigidbody;
        private Transform _targetTransform = null;

        protected void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        protected void Update()
        {
            if (_targetTransform != null)
            {
                transform.position = Vector3.Lerp(transform.position, _targetTransform.position - PlugTransform.localPosition, 15.0f * Time.deltaTime);
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.identity, 15.0f * Time.deltaTime);
            }
        }

        protected void FixedUpdate()
        {
            if (_targetTransform == null)
            {
                var move = Input.GetAxis("Horizontal");
                var dropSpeed = Input.GetKey(KeyCode.Space) ? SpeedUpMoveSpeed : DropSpeed;
                _rigidbody.velocity = new Vector3(move * MoveSpeed, 0, dropSpeed);
            }
        }

        // TODO!: do success and gameover logic
        protected void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.tag == "Phone")
            {
                var phone = collision.gameObject.GetComponentReferenced<Phone>();
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
                    Debug.Log(rating);
                }
                else
                {
                    Debug.Log("Explode");
                }
            }
        }

        public void Connected(Transform target)
        {
            _rigidbody.isKinematic = true;
            _targetTransform = target;
        }

        protected void OnDrawGizmos()
        {
            // Draw a yellow sphere at the transform's position
            var plugOnScreen = Camera.current.WorldToScreenPoint(PlugTransform.position);
            plugOnScreen.y -= 21;
            Handles.Label(Camera.current.ScreenToWorldPoint(plugOnScreen), "PlugType " + Type.ToString(), "sv_label_2");
        }
    }
}
