using UnityEngine;

namespace Serbull.GameAssets
{
    public interface IPetService
    {
        public class PetData
        {
            public string title;
            public string rarity;
            public Sprite icon;
        }

        ICurrency EggShopCurrency { get; }
        void AddEggWithPreview(string id);
        void AddPet(string id);
        PetData GetPetData(string petId);
    }
}
