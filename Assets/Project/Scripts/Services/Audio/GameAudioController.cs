using System;
using Project.Scripts.Services.Audio.AudioSystem;
using Project.Scripts.Services.EventBusSystem;
using Project.Scripts.Services.EventBusSystem.Events;
using Scripts.Services.Audio.AudioSystem.Configs;
using UnityEngine;

namespace Project.Scripts.Services.Audio
{
    public class GameAudioController : IDisposable
    {
        private readonly AudioService _audioService;
        private readonly IDisposable _matchSub;
        private readonly IDisposable _bombSub;

        public GameAudioController(AudioService audioService, EventBus eventBus)
        {
            _audioService = audioService;
            _matchSub = eventBus.Subscribe<MatchPlayedEvent>(e =>
            {
                var pitch = Mathf.Clamp(1f + e.CascadeIndex * 0.1f, 1f, 2f);
                _audioService.Play(AudioTags.Group_Gameplay, AudioTags.Sound_Match, pitch: pitch);
            });
            
            _bombSub = eventBus.Subscribe<BombActivatedEvent>(_ =>
                _audioService.Play(AudioTags.Group_Gameplay, AudioTags.Sound_Bomb));
        }

        public void StartMusic() =>
            _audioService.PlayGroup(AudioTags.Group_MainMusic);

        public void Dispose()
        {
            _matchSub.Dispose();
            _bombSub.Dispose();
        }
    }
}