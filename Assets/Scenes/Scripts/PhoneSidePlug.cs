using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NuclearCell
{
    public class PhoneSidePlug : MonoBehaviour
    {
        public Transform PlugTransform;

        public float MoveSpeed = 1f;
        public float DropSpeed = 1f;

        public float SuccessTolerance = 0.06f;

        private Rigidbody _rigidbody;

        protected void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        protected void FixedUpdate()
        {
            var move = Input.GetAxis("Horizontal");
            _rigidbody.velocity = new Vector3(move * MoveSpeed, 0, DropSpeed);
        }

        protected void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.tag == "Phone")
            {
                var phone = collision.gameObject.GetComponent<Phone>();
                var deltaPos = Mathf.Abs(phone.PortTransform.position.x - PlugTransform.position.x);
                if (deltaPos <= SuccessTolerance)
                {
                    Debug.Log("Success");
                    var rating = Mathf.Min(((int)((1.0f - (deltaPos / SuccessTolerance)) * 10f) * 10) + 60, 100);
                    Debug.Log(rating);
                }
                else
                {
                    Debug.Log("Explode");
                }
            }
        }
    }
}
