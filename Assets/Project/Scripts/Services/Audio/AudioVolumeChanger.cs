using Project.Scripts.Services.Audio.AudioSystem;
using Project.Scripts.Services.ServiceLocatorSystem;
using UnityEngine;

namespace Project.Scripts.Services.Audio
{
    public class AudioVolumeChanger : MonoBehaviour
    {
        AudioManager _audioManager;

        
        private void Awake()
        {
            _audioManager = ServiceLocator.Get<AudioManager>();
        }

        
        public void SetMusicVolume(float volume)
        {
            _audioManager.SetMusicVolume(volume);
        }

        public void SetSFXVolume(float volume)
        {
            _audioManager.SetSFXVolume(volume);
        }
    }
}