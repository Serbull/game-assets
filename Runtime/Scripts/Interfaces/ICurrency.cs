
namespace Serbull.GameAssets
{
    public interface ICurrency
    {
        long Amount { get; }
        void Spend(long amount);
        void Add(long amount);
    }
}
