using UnityEngine;

namespace Serbull.GameAssets.Samples
{
    public class ResourceGiver : IResourceGiver
    {
        public void AddResource(string resource, int count)
        {
            Debug.Log($"Add resource '{resource}' x{count}");
        }
    }
}
