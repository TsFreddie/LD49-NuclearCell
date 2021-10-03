using UnityEngine;
using UnityEditor;

namespace NuclearCell
{
    /// <summary>
    /// Basic mount, no requirement
    /// </summary>
    public class Socket : MonoBehaviour
    {
        [Header("Data")]
        public Transform SocketTransform;

        [Header("Gameplay")]
        public int Type;
        public SocketOrientation Orientation;

        public float SuccessTolerance = 0.1f;

        public virtual bool CanMount(int type)
        {
            return Type == type;
        }

        public virtual int TryMount(Transform plugTransform)
        {
            var deltaPos = (Vector2)SocketTransform.position - (Vector2)plugTransform.position;
            var deltaLength = deltaPos.magnitude;
            if (deltaLength <= SuccessTolerance)
            {
                var rating = Mathf.Min(((int)((1.0f - (deltaLength / SuccessTolerance)) * 10f) * 10) + 60, 100);
                Debug.Log("Socket: " + rating);
                return rating;
            }
            else
            {
                return -1;
            }
        }

        public virtual void Reset() { }

        protected void OnDrawGizmos()
        {
            // Draw a yellow sphere at the transform's position
            var plugOnScreen = Camera.current.WorldToScreenPoint(SocketTransform.position);
            plugOnScreen.y += 21;
            Handles.Label(Camera.current.ScreenToWorldPoint(plugOnScreen), "SocketType " + Type.ToString() + " (" + Orientation.ToString() + ")", "sv_label_2");
        }
    }
}