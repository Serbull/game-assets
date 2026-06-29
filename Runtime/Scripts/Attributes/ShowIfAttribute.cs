using UnityEngine;

namespace Serbull.GameAssets
{
    public class ShowIfAttribute : PropertyAttribute
    {
        public string ConditionName { get; }
        public bool Inverted { get; }

        public ShowIfAttribute(string conditionName, bool inverted = false)
        {
            ConditionName = conditionName;
            Inverted = inverted;
        }
    }
}
