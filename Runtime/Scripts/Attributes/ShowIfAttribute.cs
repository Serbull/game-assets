using UnityEngine;

namespace Serbull.GameAssets
{
    public class ShowIfAttribute : PropertyAttribute
    {
        public string ConditionName { get; }

        public ShowIfAttribute(string conditionName)
        {
            ConditionName = conditionName;
        }
    }
}
