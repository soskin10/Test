using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Project.Scripts.Services.ServiceLocatorSystem;
using UnityEngine;
using ZLinq;

namespace Project.Scripts.Services.UISystem
{
    public class UIService : ServiceMonoBehaviour
    {
        [SerializeField] private Canvas _backgroundCanvas;
        [SerializeField] private Canvas _mainCanvas;
        [SerializeField] private Canvas _popupCanvas;
        [SerializeField] private Canvas _systemCanvas;
        
        
        private readonly Dictionary<Type, GameObject> _registeredViews = new();
        private readonly Dictionary<Type, IView> _activeViews = new();
        private readonly Dictionary<Type, UILayer> _viewLayers = new();
        
        
        public override async UniTask InitAsync()
        {
            SetupCanvasLayers();
            await UniTask.CompletedTask;
        }
        
        public void RegisterView<TView>(GameObject prefab, UILayer layer) where TView : MonoBehaviour, IView
        {
            var type = typeof(TView);
            _registeredViews[type] = prefab;
            _viewLayers[type] = layer;
            Debug.Log($"View {type.Name} registered on layer {layer}");
        }
        
        public async UniTask<TView> Show<TView, TViewModel>() 
            where TView : BaseView<TViewModel> 
            where TViewModel : BaseViewModel, new()
        {
            var viewType = typeof(TView);
    
            if (_activeViews.TryGetValue(viewType, out var existingView))
            {
                var typedView = existingView as TView;
                if (!typedView)
                {
                    Debug.LogError($"Active view is not of type {viewType.Name}, removing corrupted entry");
                    _activeViews.Remove(viewType);
                }
                else
                {
                    await typedView.ShowAsync();
                    return typedView;
                }
            }
    
            if (false == _registeredViews.TryGetValue(viewType, out var prefab))
            {
                Debug.LogError($"View {viewType.Name} not registered!");
                return null;
            }
            
            var canvas = GetCanvasForLayer(_viewLayers[viewType]);
            var viewObject = Instantiate(prefab, canvas.transform);
            var view = viewObject.GetComponent<TView>();
            if (!view)
            {
                Debug.LogError($"Prefab doesn't have {viewType.Name} component!");
                Destroy(viewObject);
                return null;
            }
            
            var viewModel = new TViewModel();
            await view.InitializeAsync(viewModel);
            await view.ShowAsync();
            
            _activeViews[viewType] = view;
            
            Debug.Log($"View {viewType.Name} shown");
            return view;
        }
        
        public async UniTask Hide<TView>() where TView : MonoBehaviour, IView
        {
            var viewType = typeof(TView);
            
            if (false == _activeViews.TryGetValue(viewType, out var view))
            {
                Debug.LogWarning($"View {viewType.Name} is not active");
                return;
            }
            
            await view.HideAsync();
        }
        
        public void Close<TView>() where TView : MonoBehaviour, IView
        {
            var viewType = typeof(TView);
            
            if (false == _activeViews.TryGetValue(viewType, out var view))
            {
                Debug.LogWarning($"View {viewType.Name} is not active");
                return;
            }
            
            view.Close();
            _activeViews.Remove(viewType);
            
            Debug.Log($"View {viewType.Name} closed");
        }
        
        public TView GetCurrent<TView>() where TView : MonoBehaviour, IView
        {
            var viewType = typeof(TView);
            
            if (_activeViews.TryGetValue(viewType, out var view))
                return view as TView;
            
            return null;
        }
        
        public void CloseAll()
        {
            var types = _activeViews.Keys.AsValueEnumerable().ToList();
            
            for (var i = 0; i < types.Count; i++)
            {
                var view = _activeViews[types[i]];
                view.Close();
            }
            
            _activeViews.Clear();
            Debug.Log("All views closed");
        }
        
        
        protected override int GetPriority() => -100;
        
        
        private void SetupCanvasLayers()
        {
            SetupCanvas(_backgroundCanvas, UILayer.Background);
            SetupCanvas(_mainCanvas, UILayer.Main);
            SetupCanvas(_popupCanvas, UILayer.Popup);
            SetupCanvas(_systemCanvas, UILayer.System);
        }
        
        private void SetupCanvas(Canvas canvas, UILayer layer)
        {
            if (!canvas)
            {
                Debug.LogError($"Canvas for layer {layer} is not assigned!");
                return;
            }
            
            canvas.sortingOrder = (int)layer;
        }
        
        private Canvas GetCanvasForLayer(UILayer layer)
        {
            return layer switch
            {
                UILayer.Background => _backgroundCanvas,
                UILayer.Main => _mainCanvas,
                UILayer.Popup => _popupCanvas,
                UILayer.System => _systemCanvas,
                _ => _mainCanvas
            };
        }
    }
}