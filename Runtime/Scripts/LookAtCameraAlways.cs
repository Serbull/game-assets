using UnityEngine;

namespace Serbull.GameAssets
{
    public class LookAtCameraAlways : MonoBehaviour
    {
        private Transform _camera;

        private void LateUpdate()
        {
            if (_camera == null)
            {
                _camera = Camera.main.transform;
                return;
            }

            transform.rotation = _camera.rotation;
        }
    }
}
