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

        public InteractData(Transform targetObject, Vector3 targetOffset, Action callback, string text, float holdTime)
        {
            TargetObject = targetObject;
            TargetOffset = targetOffset;
            Callback = callback;
            Text = text;
            HoldTime = holdTime;
        }
    }

    public struct TriggerInteractData
    {
        public Transform TargetObject;
        public Vector3 TargetOffset;
        public string Text;
        public float HoldTime;

        public TriggerInteractData(Transform targetObject, Vector3 targetOffset, string text, float holdTime = 0.5f)
        {
            TargetObject = targetObject;
            TargetOffset = targetOffset;
            Text = text;
            HoldTime = holdTime;
        }
    }
}
