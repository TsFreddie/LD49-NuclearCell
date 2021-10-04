using UnityEngine;

namespace NuclearCell
{
    /// <summary>
    /// Shield of a mount, click to open
    /// </summary>
    public class ShieldedMount : Socket
    {
        public GameObject Shield;
        public Transform OpenPosition;
        public Transform HalfPosition;
        public Transform ClosePosition;

        public bool Opened;

        public float ShutoffTime = 2.5f;
        public float WarningTime = 1f;

        private float _openTime = 0.0f;

        protected void Start()
        {
            Reset();
        }

        protected void Update()
        {
            var targetTransform = Opened ? OpenPosition : ClosePosition;
            var targetPosition = targetTransform.position;
            var targetRotation = targetTransform.rotation;

            if (_openTime > 0)
            {
                _openTime -= Time.deltaTime;
                if (_openTime <= WarningTime)
                {
                    var t = 1.0f - (_openTime / WarningTime);
                    targetPosition = Vector3.Lerp(OpenPosition.position, HalfPosition.position, t);
                    targetRotation = Quaternion.Lerp(OpenPosition.rotation, HalfPosition.rotation, t);
                }
            }
            else
            {
                Opened = false;
            }

            Shield.transform.position = Vector3.Lerp(Shield.transform.position, targetPosition, Time.deltaTime * 15.0f);
            Shield.transform.rotation = Quaternion.Lerp(Shield.transform.rotation, targetRotation, Time.deltaTime * 15.0f);
        }

        public override int TryMount(Brick brick, Transform plugTransform)
        {
            if (!Opened) return -1;
            return base.TryMount(brick, plugTransform);
        }

        public override void Reset()
        {
            Opened = false;
        }

        protected void OnMouseDown()
        {
            Opened = true;
            _openTime = ShutoffTime;
        }
    }
}