using System;
using Cysharp.Threading.Tasks;
using Project.Scripts.Constants;
using Project.Scripts.UI;
using R3;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project.Scripts.Bootstrap
{
    public class BootstrapController : MonoBehaviour
    {
        [Tooltip("Loading screen UI shown during bootstrap")]
        [SerializeField] private LoadingScreen _loadingScreen;

        [Tooltip("Delay in seconds before the progress bar appears")]
        [SerializeField] private float _initialDelaySeconds = 0.1f;

        [Tooltip("Delay in seconds after scene is fully loaded before activating it")]
        [SerializeField] private float _finalLoadingDelaySeconds = 0.3f;


        private readonly Subject<float> _progressSubject = new();


        private async void Start()
        {
            try
            {
                await StartAsync();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Critical error during bootstrap: {ex}");
            }
        }

        private void OnDestroy()
        {
            _progressSubject?.Dispose();
        }

        private async UniTask StartAsync()
        {
            _loadingScreen.Show();

            await UniTask.Delay((int)(_initialDelaySeconds * 1000));

            _loadingScreen.ShowProgressBar();
            _loadingScreen.SubscribeToProgress(_progressSubject);

            _progressSubject.OnNext(0f);

            var asyncOp = SceneManager.LoadSceneAsync(SceneNames.GamePlay, LoadSceneMode.Single);
            if (asyncOp == null)
            {
                Debug.LogError($"Failed to load scene: {SceneNames.GamePlay}. Make sure it is added to Build Settings.");
                return;
            }

            asyncOp.allowSceneActivation = false;

            while (asyncOp.progress < 0.9f)
            {
                _progressSubject.OnNext(asyncOp.progress / 0.9f);
                await UniTask.Yield();
            }

            _progressSubject.OnNext(1f);
            await UniTask.Delay((int)(_finalLoadingDelaySeconds * 1000));

            _loadingScreen.Hide();

            asyncOp.allowSceneActivation = true;
        }
    }
}