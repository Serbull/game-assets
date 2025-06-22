using UnityEngine;
using System.Linq;

namespace Serbull.GameAssets
{
    public static class MathfUtils
    {
        public static int GetRandomIndexByWeight(int[] weights)
        {
            return GetRandomIndexByWeight(weights.Select(i => (float)i).ToArray());
        }

        public static int GetRandomIndexByWeight(float[] weights)
        {
            var weightSum = weights.Sum();
            var random = Random.Range(0, weightSum);
            var currentSum = 0f;

            for (int i = 0; i < weights.Length; i++)
            {
                if (weights[i] <= 0)
                {
                    Debug.LogError($"Weight should be more zero (current: {weights[i]})");
                    return -1;
                }

                currentSum += weights[i];

                if (random <= currentSum)
                {
                    return i;
                }
            }

            Debug.LogError($"Not found id. Weight sum: {weightSum}. Random: {random}");
            return -1;
        }
    }
}
