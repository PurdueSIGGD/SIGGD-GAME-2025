using System;
using System.Collections.Generic;
using UnityEngine;

namespace Extensions.EventBus
{
    public static class EventBus<T> where T : IEvent
    {
        private static readonly HashSet<IEventBinding<T>> bindings = new();
        
        static EventBus() => EventBusTracker.Register<T>();
        
        public static void Register(EventBinding<T> binding) => bindings.Add(binding);
        public static void Deregister(EventBinding<T> binding) => bindings.Remove(binding);

        public static void Raise(T @event)
        {
            foreach (var binding in bindings)
            {
                binding.OnEvent.Invoke(@event);
                binding.OnEventNoArgs.Invoke();
            }
        }
        
        public static void Clear() => bindings.Clear();
    }
    
    internal static class EventBusTracker
    {
        private static readonly HashSet<Type> allEventBusTypes = new();

        internal static void Register<TEvent>() where TEvent : IEvent
        {
            allEventBusTypes.Add(typeof(EventBus<TEvent>));
        }

        public static void ClearAll()
        {
            foreach (var busType in allEventBusTypes)
            {
                var clearMethod = busType.GetMethod(
                    "Clear", 
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static
                );
                clearMethod?.Invoke(null, null);
                Debug.Log($"Cleared EventBus<{busType.GetGenericArguments()[0].Name}>");
            }
        }
    }
}