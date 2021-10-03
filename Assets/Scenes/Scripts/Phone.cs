using UnityEngine;
#if UNITY_EDITOR 
using UnityEditor;
#endif

namespace NuclearCell
{
    public class Phone : MonoBehaviour
    {
        public Transform PortTransform;

        [Header("Gameplay")]
        public int Type;
#if UNITY_EDITOR 
        protected void OnDrawGizmos()
        {
            // Draw a yellow sphere at the transform's position
            var plugOnScreen = Camera.current.WorldToScreenPoint(PortTransform.position);
            plugOnScreen.y += 8;
            Handles.Label(Camera.current.ScreenToWorldPoint(plugOnScreen), "PortType " + Type.ToString(), "sv_label_3");
        }
#endif
    }
}