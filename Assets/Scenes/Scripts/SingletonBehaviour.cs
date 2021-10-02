using UnityEngine;

namespace NuclearCell
{
    public abstract class SingletonBehaviour<T> : MonoBehaviour
        where T : SingletonBehaviour<T>
    {
        private static T s_singleton;

        public static T Singleton
        {
            get
            {
                if (s_singleton == null)
                {
                    var instances = FindObjectsOfType<T>();
                    var count = instances.Length;
                    if (count == 0)
                    {
                        var obj = new GameObject() { name = typeof(T).Name };

                        s_singleton = obj.AddComponent<T>();
                    }
                    else
                    {
                        for (var i = 1; i < instances.Length; i++)
                        {
                            Destroy(instances[i]);
                        }

                        s_singleton = instances[0];
                    }
                }

                return s_singleton;
            }
        }

        protected virtual void Awake()
        {
            if (s_singleton == null)
                s_singleton = this as T;
            else
                Destroy(this);
        }

        protected virtual void OnDestroy()
        {
            if (s_singleton == this)
                s_singleton = null;
        }
    }
}