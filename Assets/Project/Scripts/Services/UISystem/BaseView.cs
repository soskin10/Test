using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace Project.Scripts.Services.UISystem
{
    public abstract class BaseView<TViewModel> : MonoBehaviour, IView where TViewModel : BaseViewModel
    {
        public TViewModel ViewModel { get; private set; }
        public bool IsVisible { get; private set; }
        
        
        protected CompositeDisposable Disposables { get; } = new();
        
        
        private void OnDestroy()
        {
            Disposables.Dispose();
        }
        
        
        public async UniTask InitializeAsync(TViewModel viewModel)
        {
            ViewModel = viewModel;
            await ViewModel.InitializeAsync();
            await OnBindViewModel();
        }
        
        public async UniTask ShowAsync()
        {
            if (IsVisible)
                return;
            
            gameObject.SetActive(true);
            IsVisible = true;
            
            await OnShow();
        }
        
        public async UniTask HideAsync()
        {
            if (false == IsVisible)
                return;
            
            await OnHide();
            
            IsVisible = false;
            gameObject.SetActive(false);
        }
        
        public void Close()
        {
            Disposables.Dispose();
            
            if (null != ViewModel)
                ViewModel.Cleanup();
            
            OnClose();
            Destroy(gameObject);
        }
        
        
        protected virtual UniTask OnBindViewModel()
        {
            return UniTask.CompletedTask;
        }
        
        protected virtual UniTask OnShow()
        {
            return UniTask.CompletedTask;
        }
        
        protected virtual UniTask OnHide()
        {
            return UniTask.CompletedTask;
        }
        
        protected virtual void OnClose()
        {
        }
    }
}