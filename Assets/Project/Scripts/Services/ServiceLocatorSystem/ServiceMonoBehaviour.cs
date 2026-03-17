using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Project.Scripts.Services.ServiceLocatorSystem
{
    public abstract class ServiceMonoBehaviour : MonoBehaviour, IService
    {
        protected bool IsShuttingDown => _isShuttingDown;
        
        
        private bool _isRegistered;
        private bool _isShuttingDown;
        
        
        public virtual async UniTask OnShutdownAsync()
        {
            await UniTask.CompletedTask;
        }
        
        public abstract UniTask InitAsync();
        
        
        protected virtual int GetPriority() => 0;
        
        protected virtual void OnAwake()
        {
        }
        
        protected virtual void OnBeforeDestroy()
        {
        }
        
        protected virtual void OnApplicationQuitting()
        {
        }
        
        
        private void Awake()
        {
            RegisterSelf();
            OnAwake();
        }
        
        private void OnDestroy()
        {
            OnBeforeDestroy();
            UnregisterSelf();
        }
        
        private void OnApplicationQuit()
        {
            _isShuttingDown = true;
            OnApplicationQuitting();
        }
        
        private void RegisterSelf()
        {
            if (false == _isRegistered)
            {
                ServiceLocator.Register(this, GetPriority());
                _isRegistered = true;
            }
        }
        
        private void UnregisterSelf()
        {
            if (_isRegistered && false == ServiceLocator.IsShuttingDown)
            {
                ServiceLocator.Unregister(GetType());
                _isRegistered = false;
            }
        }
    }
    
    
    public abstract class ManualServiceMonoBehaviour : MonoBehaviour, IService
    {
        protected bool IsShuttingDown => _isShuttingDown;
        protected bool IsRegistered => _isRegistered;
        
        
        private bool _isRegistered;
        private bool _isShuttingDown;
        
        
        public virtual async UniTask OnShutdownAsync()
        {
            await UniTask.CompletedTask;
        }
        
        public abstract UniTask InitAsync();
        
        
        protected virtual int GetPriority() => 0;
        
        protected void Register()
        {
            if (false == _isRegistered)
            {
                ServiceLocator.Register(this, GetPriority());
                _isRegistered = true;
            }
        }
        
        protected void RegisterAs<TInterface>() where TInterface : class
        {
            if (false == _isRegistered)
            {
                if (this is TInterface interfaceInstance)
                {
                    ServiceLocator.Register<TInterface>(interfaceInstance, GetPriority());
                    _isRegistered = true;
                }
                else
                    Debug.LogError($"{GetType().Name} does not implement {typeof(TInterface).Name}");
            }
        }
        
        protected void Unregister()
        {
            if (_isRegistered && false == ServiceLocator.IsShuttingDown)
            {
                ServiceLocator.Unregister(GetType());
                _isRegistered = false;
            }
        }
        
        protected virtual void OnDestroy()
        {
            Unregister();
        }
        
        protected virtual void OnApplicationQuit()
        {
            _isShuttingDown = true;
        }
    }
}