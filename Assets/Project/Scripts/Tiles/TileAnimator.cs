using Cysharp.Threading.Tasks;
using DG.Tweening;
using Project.Scripts.Configs;
using UnityEngine;

namespace Project.Scripts.Tiles
{
    public class TileAnimator : MonoBehaviour
    {
        private AnimationConfig _config;
        
        
        public void Init(AnimationConfig config) => _config = config;

        public UniTask AnimateSwapTo(Vector3 target) =>
            transform.DOMove(target, _config.SwapDuration).ToUniTask();

        public UniTask AnimateFallTo(Vector3 target) =>
            transform.DOMove(target, _config.FallDuration)
                .SetEase(_config.FallEase).ToUniTask();

        public UniTask AnimateDestroy() =>
            transform.DOScale(Vector3.zero, _config.DestroyDuration)
                .SetEase(_config.DestroyEase).ToUniTask();

        public UniTask AnimateSpawn()
        {
            transform.localScale = Vector3.zero;
            return transform.DOScale(Vector3.one, _config.SpawnDuration).ToUniTask();
        }
    }
}