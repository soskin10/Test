using Project.Scripts.Configs;
using Project.Scripts.Tiles;
using UnityEngine;
using UnityEngine.Pool;

namespace Project.Scripts.Services
{
    public class TilePool
    {
        private readonly ObjectPool<Tile> _pool;


        public TilePool(Tile prefab, Transform parent, AnimationConfig animConfig)
        {
            _pool = new ObjectPool<Tile>(
                createFunc: () =>
                {
                    var t = Object.Instantiate(prefab, parent);
                    t.Animator.Init(animConfig);
                    return t;
                },
                actionOnGet: t =>
                {
                    t.transform.localScale = Vector3.one;
                    t.gameObject.SetActive(true);
                },
                actionOnRelease: t => t.gameObject.SetActive(false),
                actionOnDestroy: t => Object.Destroy(t.gameObject),
                collectionCheck: false,
                defaultCapacity: 36,
                maxSize: 100
            );
        }

        public Tile Get() => _pool.Get();

        public void Release(Tile tile) => _pool.Release(tile);
    }
}