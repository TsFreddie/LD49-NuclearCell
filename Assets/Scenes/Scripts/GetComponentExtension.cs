using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NuclearCell
{
    public static class GetComponentExtension
    {
        public static T GetComponentSmart<T>(this GameObject obj) where T : Component
        {
            var comp = obj.GetComponent<T>();
            if (!comp) comp = obj.GetComponentInParent<T>();
            if (comp)
            {
                return comp;
            }
            else
            {
                var referenceFinder = obj.GetComponent<ReferenceFinder>();
                if (referenceFinder != null && referenceFinder.Script is T)
                {
                    return referenceFinder.Script as T;
                }
            }

            return null;
        }

        public static T GetComponentSmart<T>(this Component obj) where T : Component
        {
            var comp = obj.GetComponent<T>();
            if (!comp) comp = obj.GetComponentInParent<T>();
            if (comp)
            {
                return comp;
            }
            else
            {
                var referenceFinder = obj.GetComponent<ReferenceFinder>();
                if (referenceFinder != null && referenceFinder.Script is T)
                {
                    return referenceFinder.Script as T;
                }
            }
            return null;
        }
    }
}