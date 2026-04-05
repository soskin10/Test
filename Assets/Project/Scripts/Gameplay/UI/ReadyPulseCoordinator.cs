using System;
using DG.Tweening;
using Project.Scripts.Configs;
using Project.Scripts.Configs.Battle;
using R3;

namespace Project.Scripts.Gameplay.UI
{
    public class ReadyPulseCoordinator : IReadyPulseCoordinator, IDisposable
    {
        public Observable<float> Alpha => _subject;


        private readonly Subject<float> _subject = new();
        private float _alpha = 1f;
        private Tween _tween;


        public ReadyPulseCoordinator(BattleAnimationConfig config)
        {
            _tween = DOTween.To(
                    () => _alpha,
                    v => { _alpha = v; _subject.OnNext(v); },
                    config.ReadyPulseAlpha,
                    config.ReadyPulseDuration * 0.5f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(config.ReadyPulseEase);
        }

        public void Dispose()
        {
            _tween?.Kill();
            _tween = null;
            _subject.Dispose();
        }
    }
}