using UnityEngine;
using UnityEditor;

namespace NuclearCell
{
    public class Phone : MonoBehaviour
    {
        public Transform PortTransform;

        [Header("Gameplay")]
        public int Type;

        protected void OnDrawGizmos()
        {
            // Draw a yellow sphere at the transform's position
            var plugOnScreen = Camera.current.WorldToScreenPoint(PortTransform.position);
            plugOnScreen.y += 21;
            Handles.Label(Camera.current.ScreenToWorldPoint(plugOnScreen), "PortType " + Type.ToString(), "sv_label_2");
        }
    }
}