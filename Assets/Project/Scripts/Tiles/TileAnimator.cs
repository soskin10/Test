using Cysharp.Threading.Tasks;
using DG.Tweening;
using Project.Scripts.Configs;
using Project.Scripts.Configs.Board;
using UnityEngine;

namespace Project.Scripts.Tiles
{
    public class TileAnimator : MonoBehaviour
    {
        private BoardAnimationConfig _config;
        private Vector3 _targetScale = Vector3.one;


        public void Init(BoardAnimationConfig config)
        {
            _config = config;
        }

        public void SetTargetScale(float cellSize)
        {
            _targetScale = Vector3.one * cellSize;
        }

        public UniTask AnimateSwapTo(Vector3 target)
        {
            return transform.DOMove(target, _config.SwapDuration).ToUniTask();
        }

        public UniTask AnimateFallTo(Vector3 target)
        {
            return transform.DOMove(target, _config.FallDuration).SetEase(_config.FallEase).ToUniTask();
        }

        public UniTask AnimateDestroy()
        {
            return transform.DOScale(Vector3.zero, _config.DestroyDuration).SetEase(_config.DestroyEase).ToUniTask();
        }

        public UniTask AnimateSpawn()
        {
            transform.localScale = Vector3.zero;
            
            return transform.DOScale(_targetScale, _config.SpawnDuration).ToUniTask();
        }
    }
}