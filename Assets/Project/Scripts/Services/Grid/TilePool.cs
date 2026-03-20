using Project.Scripts.Configs;
using Project.Scripts.Tiles;
using UnityEngine;
using UnityEngine.Pool;

namespace Project.Scripts.Services.Grid
{
    public class TilePool
    {
        private readonly float _tileVisualSize;
        private readonly ObjectPool<Tile> _pool;


        public TilePool(Tile prefab, Transform parent, AnimationConfig animConfig, float cellSize, float tileScale)
        {
            _tileVisualSize = cellSize * tileScale;
            _pool = new ObjectPool<Tile>(
                createFunc: () =>
                {
                    var t = Object.Instantiate(prefab, parent);
                    t.Animator.Init(animConfig);
                    return t;
                },
                actionOnGet: t =>
                {
                    t.transform.localScale = Vector3.one * _tileVisualSize;
                    t.Animator.SetTargetScale(_tileVisualSize);
                    t.gameObject.SetActive(true);
                },
                actionOnRelease: t =>
                {
                    DG.Tweening.DOTween.Kill(t.transform);
                    t.gameObject.SetActive(false);
                },
                actionOnDestroy: t => Object.Destroy(t.gameObject),
                // TODO: disable collectionCheck after confirming that the hole bug is fixed
                collectionCheck: true,
                defaultCapacity: 36,
                maxSize: 100
            );
        }

        public Tile Get() => _pool.Get();

        public void Release(Tile tile) => _pool.Release(tile);
    }
}