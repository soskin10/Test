using System;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Scripts.UI
{
    public class LoadingScreen : MonoBehaviour
    {
        [Tooltip("Root panel GameObject shown during loading")]
        [SerializeField] private GameObject _loadingPanel;

        [Tooltip("Slider representing loading progress (0–1)")]
        [SerializeField] private Slider _progressBar;


        private IDisposable _subscription;


        public void SubscribeToProgress(Observable<float> progressObservable)
        {
            _subscription = progressObservable.Subscribe(progress =>
            {
                _progressBar.value = progress;
            });
        }

        public void Show()
        {
            _loadingPanel.SetActive(true);
            _progressBar.value = 0;
        }

        public void ShowProgressBar()
        {
            _progressBar.gameObject.SetActive(true);
        }

        public void Hide()
        {
            _loadingPanel.SetActive(false);
        }


        private void OnDestroy()
        {
            _subscription?.Dispose();
        }
    }
}