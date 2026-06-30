namespace Serbull.GameAssets
{
    public interface IPetService
    {
        ICurrency EggShopCurrency { get; }
        
        void AddEggWithPreview(string id);
        void AddPet(string id);
    }
}
