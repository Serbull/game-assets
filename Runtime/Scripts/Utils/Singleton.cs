using UnityEngine;

namespace Serbull.GameAssets
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindInstance();
                }

                return _instance;
            }
        }

        private static T FindInstance()
        {
            var instance = FindAnyObjectByType<T>(FindObjectsInactive.Include);

            if (instance == null)
            {
                Debug.LogWarning($"No have Instances type of {typeof(T)}");
            }

            return instance;
        }
    }
}
