using System;

namespace Serbull.GameAssets
{
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class ButtonAttribute : Attribute
    {
        public string Label { get; }
        public int Space { get; set; } = 0;

        public ButtonAttribute(string label = null)
        {
            Label = label;
        }

        public ButtonAttribute(string label, int space)
        {
            Label = label;
            Space = space;
        }
    }
}
