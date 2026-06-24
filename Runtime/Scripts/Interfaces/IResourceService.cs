
namespace Serbull.GameAssets
{
    public interface IResourceService
    {
        public void AddResource(string resource, int count);

        public void SpendResource(string resource, int count);
    }
}
