namespace Serbull.GameAssets
{
    using System;
    using System.Collections.Generic;

    public static class EventBus
    {
        private static readonly Dictionary<Type, List<Delegate>> _subscribers = new();

        public static void Subscribe<T>(Action<T> handler)
        {
            var type = typeof(T);
            if (!_subscribers.TryGetValue(type, out var list))
            {
                list = new List<Delegate>();
                _subscribers[type] = list;
            }

            if (!list.Contains(handler))
                list.Add(handler);
        }

        public static void Unsubscribe<T>(Action<T> handler)
        {
            var type = typeof(T);
            if (_subscribers.TryGetValue(type, out var list))
            {
                list.Remove(handler);
            }
        }

        public static void Publish<T>(T evt)
        {
            var type = typeof(T);

            if (!_subscribers.TryGetValue(type, out var list))
                return;

            var copy = list.ToArray();
            for (int i = 0; i < copy.Length; i++)
            {
                ((Action<T>)copy[i])?.Invoke(evt);
            }
        }
    }
}
