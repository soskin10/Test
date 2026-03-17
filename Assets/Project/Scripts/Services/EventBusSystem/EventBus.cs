using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Project.Scripts.Services.ServiceLocatorSystem;
using UnityEngine;

namespace Project.Scripts.Services.EventBusSystem
{
    public class EventBus : IService, IAsyncDisposable
    {
        private readonly Dictionary<Type, List<Delegate>> _subscribers = new();


        public UniTask InitAsync()
        {
            Debug.Log("EventBus initialized");
            return UniTask.CompletedTask;
        }

        public ValueTask DisposeAsync()
        {
            _subscribers.Clear();
            Debug.Log("EventBus disposed");
            return default;
        }

        public IDisposable Subscribe<T>(Action<T> handler) where T : struct
        {
            var eventType = typeof(T);
            if (false == _subscribers.ContainsKey(eventType))
                _subscribers[eventType] = new List<Delegate>();

            _subscribers[eventType].Add(handler);

            return new Subscription(() =>
            {
                if (_subscribers.TryGetValue(eventType, out var list))
                {
                    list.Remove(handler);
                    if (list.Count == 0)
                        _subscribers.Remove(eventType);
                }
            });
        }

        public void Publish<T>(T @event) where T : struct
        {
            var eventType = typeof(T);
            if (_subscribers.TryGetValue(eventType, out var subscriber))
            {
                for (var i = subscriber.Count - 1; i >= 0; i--)
                {
                    var handler = subscriber[i];
                    ((Action<T>)handler).Invoke(@event);
                }
            }
        }
        

        private class Subscription : IDisposable
        {
            public Subscription(Action unsubscribe) => _unsubscribe = unsubscribe;
            
            
            private Action _unsubscribe;


            public void Dispose()
            {
                _unsubscribe?.Invoke();
                _unsubscribe = null;
            }
        }
    }
}