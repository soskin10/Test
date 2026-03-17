using System;
using Cysharp.Threading.Tasks;
using R3;

namespace Project.Scripts.Services.UISystem
{
    public abstract class BaseViewModel : IDisposable
    {
        public bool IsInitialized { get; private set; }
        
        
        protected CompositeDisposable Disposables { get; } = new();
        
        
        public async UniTask InitializeAsync()
        {
            if (IsInitialized)
                return;
            
            await OnInitializeAsync();
            IsInitialized = true;
        }
        
        public void Cleanup()
        {
            if (false == IsInitialized)
                return;
            
            OnCleanup();
            Disposables.Dispose();
            IsInitialized = false;
        }
        
        public void Dispose()
        {
            Cleanup();
        }
        
        
        protected virtual UniTask OnInitializeAsync()
        {
            return UniTask.CompletedTask;
        }
        
        protected virtual void OnCleanup()
        {
        }
    }
}