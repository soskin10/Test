using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ZLinq;

namespace Project.Scripts.Services.ServiceLocatorSystem
{
    public static class ServiceLocator
    {
        public static event Action<float, string> OnInitProgress;
        
        
        public static bool IsShuttingDown => _isShuttingDown;
        
        
        private static readonly Dictionary<Type, object> _services = new();
        private static readonly Dictionary<Type, int> _priorities = new();
        private static bool _isShuttingDown;
        
        
        public static void Register<T>(T service, int priority = 0) where T : class
        {
            if (_isShuttingDown) 
                return;
            
            var type = typeof(T);
            RegisterInternal(type, service, priority);
        }
        
        public static void RegisterAs<TInterface, TImplementation>(TImplementation service, int priority = 0) 
            where TInterface : class 
            where TImplementation : class, TInterface
        {
            if (_isShuttingDown) 
                return;
            
            var interfaceType = typeof(TInterface);
            RegisterInternal(interfaceType, service, priority);
        }
        
        public static void Unregister<T>() where T : class
        {
            Unregister(typeof(T));
        }
        
        public static void Unregister(Type type)
        {
            if (_services.Remove(type))
            {
                _priorities.Remove(type);
                Debug.Log($"Service {type.Name} unregistered");
            }
        }
        
        public static T Get<T>() where T : class
        {
            if (_isShuttingDown)
            {
                Debug.LogError($"Cannot get service {typeof(T).Name} during shutdown");
                return null;
            }
            
            var type = typeof(T);
            if (_services.TryGetValue(type, out var service))
            {
                if (service is MonoBehaviour mono && !mono)
                {
                    Debug.LogWarning($"Service {type.Name} (MonoBehaviour) was destroyed, removing from locator");
                    Unregister(type);
                    return null;
                }
                
                return service as T;
            }
            
            Debug.LogError($"Service {type.Name} not found!");
            return null;
        }
        
        public static bool TryGet<T>(out T service) where T : class
        {
            service = null;
            if (_isShuttingDown) 
                return false;
            
            var type = typeof(T);
            if (false == _services.TryGetValue(type, out var obj))
                return false;
            
            if (obj is MonoBehaviour mono && !mono)
            {
                Unregister(type);
                return false;
            }
            
            service = obj as T;
            return null != service;
        }
        
        public static bool Has<T>() where T : class
        {
            if (_isShuttingDown) 
                return false;
            
            var type = typeof(T);
            if (false == _services.TryGetValue(type, out var service))
                return false;
                
            if (service is MonoBehaviour mono && !mono)
            {
                Unregister(type);
                return false;
            }
            
            return true;
        }
        
        public static async UniTask InitAllAsync(int delayBetweenServicesMs = 0)
        {
            var orderedServices = _services
                .AsValueEnumerable()
                .OrderBy(kvp => _priorities.GetValueOrDefault(kvp.Key, 0))
                .ToList();
            
            var totalServices = orderedServices
                .AsValueEnumerable()
                .Count(kvp => kvp.Value is IService);
            
            var currentIndex = 0;
            for (var i = 0; i < orderedServices.Count; i++)
            {
                if (_isShuttingDown) 
                    return;
                
                var kvp = orderedServices[i];
                var type = kvp.Key;
                var service = kvp.Value;
                
                if (service is not IService initService)
                    continue;
                
                if (service is MonoBehaviour mono && !mono)
                {
                    Debug.LogWarning($"Service {type.Name} (MonoBehaviour) was destroyed before initialization");
                    Unregister(type);
                    continue;
                }
                
                try
                {
                    var progress = (float)currentIndex / totalServices;
                    OnInitProgress?.Invoke(progress, $"Initializing {type.Name}...");
                    
                    await initService.InitAsync();
                    Debug.Log($"Service {type.Name} initialized");
                    
                    currentIndex++;
                    
                    progress = (float)currentIndex / totalServices;
                    OnInitProgress?.Invoke(progress, $"{type.Name} ready");
                    
                    if (delayBetweenServicesMs > 0) 
                        await UniTask.Delay(delayBetweenServicesMs);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to initialize {type.Name}: {e.Message}\n{e.StackTrace}");
                    throw;
                }
            }
            
            OnInitProgress?.Invoke(1.0f, "All services initialized");
            Debug.Log("All services initialized");
        }
        
        public static void Shutdown()
        {
            ShutdownAsync().GetAwaiter().GetResult();
        }
        
        public static async UniTask ShutdownAsync()
        {
            if (_isShuttingDown) 
                return;
            
            _isShuttingDown = true;
            Debug.Log("ServiceLocator shutdown starting...");
            
            var orderedServices = _services
                .AsValueEnumerable()
                .OrderByDescending(kvp => _priorities.GetValueOrDefault(kvp.Key, 0))
                .ToList();

            for (var i = 0; i < orderedServices.Count; i++)
            {
                var kvp = orderedServices[i];
                var type = kvp.Key;
                var service = kvp.Value;

                try
                {
                    if (service is ServiceMonoBehaviour monoService && monoService)
                    {
                        Debug.Log($"Shutting down MonoBehaviour service: {type.Name}");
                        await monoService.OnShutdownAsync();
                    }
                    else if (service is IAsyncDisposable asyncDisposable)
                    {
                        Debug.Log($"Disposing async service: {type.Name}");
                        await asyncDisposable.DisposeAsync();
                    }
                    else if (service is IDisposable disposable)
                    {
                        Debug.Log($"Disposing service: {type.Name}");
                        disposable.Dispose();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error shutting down {type.Name}: {e.Message}\n{e.StackTrace}");
                }
            }

            _services.Clear();
            _priorities.Clear();
            Debug.Log("ServiceLocator shutdown complete");
        }
        
        
        private static void RegisterInternal(Type type, object service, int priority)
        {
            if (false == _services.TryAdd(type, service))
            {
                Debug.Log($"Service {type.Name} already registered, replacing...");
                _services[type] = service;
                _priorities[type] = priority;
            }
            else
            {
                _priorities.Add(type, priority);
                Debug.Log($"Service {type.Name} registered with priority {priority}");
            }
        }
        
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Clear()
        {
            _services.Clear();
            _priorities.Clear();
            OnInitProgress = null;
            _isShuttingDown = false;
        }
    }
}