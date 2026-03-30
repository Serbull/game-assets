using UnityEngine;
using System;

namespace Serbull.GameAssets.Interact
{
    public struct InteractData
    {
        public Transform TargetObject;
        public Vector3 TargetOffset;
        public Action Callback;
        public string Text;
        public float HoldTime;

        public InteractData(Transform targetObject, Vector3 targetOffset, Action callback, string text, float holdTime = 0.5f)
        {
            TargetObject = targetObject;
            TargetOffset = targetOffset;
            Callback = callback;
            Text = text;
            HoldTime = holdTime;
        }
    }
}
