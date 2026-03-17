using System;
using Cysharp.Threading.Tasks;
using Project.Scripts.Configs;
using Project.Scripts.Constants;
using Project.Scripts.Services.Audio.AudioSystem;
using Project.Scripts.Services.EventBusSystem;
using Project.Scripts.Services.ServiceLocatorSystem;
using Project.Scripts.UI;
using R3;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

namespace Project.Scripts.Bootstrap
{
    public class BootstrapController : MonoBehaviour
    {
        [SerializeField] private LoadingScreen _loadingScreen;
        [SerializeField] private float _initialDelaySeconds = 0.1f;
        [SerializeField] private float _delayBetweenServicesInitSeconds = 0.05f;
        [SerializeField] private float _finalLoadingDelaySeconds = 0.3f;

        [Header("Audio")]
        [SerializeField] private AudioMixer _audioMixer;
        [SerializeField] private AudioMixerGroup _musicMixerGroup;
        [SerializeField] private AudioMixerGroup _sfxMixerGroup;

        
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

        private async UniTask StartAsync()
        {
            _loadingScreen.Show();

            await UniTask.Delay((int)(_initialDelaySeconds * 1000));

            _loadingScreen.ShowProgressBar();
            _loadingScreen.SubscribeToProgress(_progressSubject);

            RegisterServices();

            ServiceLocator.OnInitProgress += OnServiceInitProgress;
            await ServiceLocator.InitAllAsync((int)(_delayBetweenServicesInitSeconds * 1000));
            ServiceLocator.OnInitProgress -= OnServiceInitProgress;
    
            await UniTask.Delay((int)(_finalLoadingDelaySeconds * 1000));

            _loadingScreen.Hide();

            SceneManager.LoadScene(SceneNames.GamePlay, LoadSceneMode.Single);
        }

        private void RegisterServices()
        {
            var eventBus = new EventBus();
            ServiceLocator.Register(eventBus, priority: 1);
            
            var mainConfig = Resources.Load<MainConfig>("Configs/MainConfig");
            ServiceLocator.Register(mainConfig, priority: 2);
            
            var audioManager = CreateMonoService<AudioManager>("AudioManager");
            audioManager.AudioMixer = _audioMixer;
            audioManager.MusicMixerGroup = _musicMixerGroup;
            audioManager.SFXMixerGroup = _sfxMixerGroup;

            var audioService = new AudioService(mainConfig.AudioMusicConfig, mainConfig.AudioSFXConfig, audioManager);
            ServiceLocator.Register(audioService, 3);
        }

        private void OnServiceInitProgress(float progress, string status)
        {
            _progressSubject.OnNext(progress);
        }
        
        private T CreateMonoService<T>(string serviceName) where T : ServiceMonoBehaviour
        {
            var serviceObject = new GameObject(serviceName);
            DontDestroyOnLoad(serviceObject);

            var service = serviceObject.AddComponent<T>();
            Debug.Log($"MonoBehaviour service {serviceName} created");
            return service;
        }

        private void OnDestroy()
        {
            _progressSubject?.Dispose();
        }
    }
}