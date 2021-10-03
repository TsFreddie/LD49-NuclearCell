using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NuclearCell
{
    public static class GetComponentExtension
    {
        public static T GetComponentReferenced<T>(this GameObject obj) where T : Component
        {
            if (obj.GetComponent<T>())
            {
                return obj.GetComponent<T>();
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

        public static T GetComponentReferenced<T>(this Component obj) where T : Component
        {
            if (obj.GetComponent<T>())
            {
                return obj.GetComponent<T>();
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