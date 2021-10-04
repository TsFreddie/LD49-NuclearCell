using System;
using UnityEngine;
#if UNITY_EDITOR 
using UnityEditor;
#endif

namespace NuclearCell
{
    [Serializable]
    public struct SocketConfig
    {
        public Transform SocketTransform;
        public int Type;
        public SocketOrientation Orientation;
    }

    /// <summary>
    /// Basic mount, no requirement
    /// </summary>
    public class Socket : MonoBehaviour
    {
        [Header("Gameplay")]
        public SocketConfig[] Configs;

        public bool Occupied;
        public bool GoingOut;

        public float SuccessTolerance = 0.1f;

        public int Slot { get; set; } = -1;
        public float Wait = 0;
        public Vector3 TargetPos { get; set; }

        public void Update()
        {
            if (Wait > 0)
            {
                Wait -= Time.deltaTime;
                return;
            }
            transform.position = Vector3.Lerp(transform.position, TargetPos, 2.0f * Time.deltaTime);
            if (GoingOut && (transform.position - TargetPos).magnitude < 0.1f)
            {
                Destroy(gameObject);
            }
        }

        public virtual int TryMount(Brick brick, Transform plugTransform)
        {
            if (Occupied) return -1;

            var nearest = -1;
            var distance = float.PositiveInfinity;
            for (var i = 0; i < Configs.Length; i++)
            {
                var deltaPos = (Vector2)Configs[i].SocketTransform.position - (Vector2)plugTransform.position;
                var deltaLength = deltaPos.magnitude;
                if (distance > deltaLength)
                {
                    distance = deltaLength;
                    nearest = i;
                }
            }

            if (distance <= SuccessTolerance)
            {
                var rating = Mathf.Min(((int)((1.0f - (distance / SuccessTolerance)) * 10f) * 10) + 60, 100);
                brick.SetConnectedTransform(Configs[nearest].SocketTransform);
                Occupied = true;
                return rating;
            }
            else
            {
                return -1;
            }
        }

        public virtual void Reset() { }

        public void Release()
        {
            SocketManager.Singleton.Release(Slot);
            Slot = -1;
            GoingOut = true;
            TargetPos = new Vector3(TargetPos.x, TargetPos.y, TargetPos.z + 0.5f);
        }

#if UNITY_EDITOR 
        protected void OnDrawGizmos()
        {
            foreach (var socket in Configs)
            {
                // Draw a yellow sphere at the transform's position
                var plugOnScreen = Camera.current.WorldToScreenPoint(socket.SocketTransform.position);
                plugOnScreen.y += 12;
                Handles.Label(Camera.current.ScreenToWorldPoint(plugOnScreen), "SocketType " + socket.Type.ToString() + " (" + socket.Orientation.ToString() + ")", "sv_label_3");
            }
        }
#endif
    }
}