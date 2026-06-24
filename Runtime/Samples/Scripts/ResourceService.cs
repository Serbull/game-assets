using UnityEngine;

namespace Serbull.GameAssets.Samples
{
    public class ResourceService : IResourceService
    {
        public void AddResource(string resource, int count)
        {
            Debug.Log($"Add resource '{resource}' x{count}");
        }

        public void SpendResource(string resource, int count)
        {
            Debug.Log($"Spend resource '{resource}' x{count}");
        }
    }
}
