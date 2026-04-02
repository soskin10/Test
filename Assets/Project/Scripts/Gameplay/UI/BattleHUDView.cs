using Cysharp.Threading.Tasks;
using Project.Scripts.Services.UISystem;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;

namespace Project.Scripts.Gameplay.UI
{
    public class BattleHUDView : BaseView<BattleHUDViewModel>
    {
        [Header("Top")]
        [Tooltip("Container for TopBar and EnemyPanel — receives safe area top offset")]
        [SerializeField] private RectTransform _topContainer;

        [Tooltip("Text displaying the enemy opponent name in the top bar")]
        [SerializeField] private TMP_Text _enemyNameText;

        [Header("Avatars")]
        [Tooltip("AvatarSlotView for the enemy avatar — portrait, HP bar, energy bar, hit reaction")]
        [SerializeField] private AvatarSlotView _enemyAvatarSlot;

        [Tooltip("AvatarSlotView for the player avatar — portrait, HP bar, energy bar, hit reaction, activation input")]
        [SerializeField] private AvatarSlotView _playerAvatarSlot;

        [Tooltip("RectTransform of the player avatar panel — positioned at board top edge at runtime")]
        [SerializeField] private RectTransform _playerPanel;

        [Header("Enemy Heroes")]
        [Tooltip("Four HeroSlotView components for the enemy side, ordered left to right")]
        [SerializeField] private HeroSlotView[] _enemyHeroSlots;

        [Header("Player Heroes")]
        [Tooltip("Four HeroSlotView components for the player side, ordered left to right")]
        [SerializeField] private HeroSlotView[] _playerHeroSlots;

        [Header("Floating Damage Numbers")]
        [Tooltip("Prefab with FloatingDamageNumberComponent — contains TMP label")]
        [SerializeField] private FloatingDamageNumberComponent _floatingDamagePrefab;

        [Tooltip("Parent RectTransform for pooled floating damage objects; leave empty to use this transform")]
        [SerializeField] private RectTransform _floatingDamageContainer;

        [Header("Layout")]
        [Tooltip("Extra vertical offset in canvas units added above the board top edge (positive = higher)")]
        [SerializeField] private float _playerPanelPadding = 50f;


        private Canvas _canvas;
        private bool _isOverlay;
        private RectTransform _referenceRect;
        private ObjectPool<FloatingDamageNumberComponent> _floatingPool;

#if UNITY_EDITOR
        private Camera _cachedCamera;
#endif


        protected override UniTask OnBindViewModel()
        {
            if (_enemyNameText)
                _enemyNameText.text = ViewModel.EnemyName;

            _enemyAvatarSlot.Bind(ViewModel.EnemyAvatar);
            _playerAvatarSlot.Bind(ViewModel.PlayerAvatar);

            ViewModel.EnemyAvatar.Hit
                .Subscribe(damage => SpawnDamageNumber(damage, _enemyAvatarSlot.HitAnchor))
                .AddTo(Disposables);

            ViewModel.PlayerAvatar.Hit
                .Subscribe(damage => SpawnDamageNumber(damage, _playerAvatarSlot.HitAnchor))
                .AddTo(Disposables);

            BindHeroSlots(_enemyHeroSlots, ViewModel.EnemyHeroSlots);
            BindHeroSlots(_playerHeroSlots, ViewModel.PlayerHeroSlots);

            _canvas = GetComponentInParent<Canvas>();
            _isOverlay = _canvas && _canvas.renderMode == RenderMode.ScreenSpaceOverlay;
            _referenceRect = _playerPanel.parent as RectTransform;

            if (_floatingDamagePrefab)
            {
                var container = _floatingDamageContainer
                    ? _floatingDamageContainer
                    : (RectTransform)transform;

                _floatingPool = new ObjectPool<FloatingDamageNumberComponent>(
                    createFunc: () => Instantiate(_floatingDamagePrefab, container),
                    actionOnGet: c => c.gameObject.SetActive(true),
                    actionOnRelease: c => { c.Kill(); c.gameObject.SetActive(false); },
                    actionOnDestroy: c => { if (c) Destroy(c.gameObject); },
                    defaultCapacity: 4,
                    maxSize: 16);
            }

            ApplyTopSafeAreaOffset();

#if UNITY_EDITOR
            _cachedCamera = Camera.main;
            PositionPlayerPanel(ViewModel.BoardTopWorldY, _cachedCamera);
            ApplyBoardWidth(ViewModel.BoardHalfWidth, ViewModel.BoardCenterX, _cachedCamera);
#else
            PositionPlayerPanel(ViewModel.BoardTopWorldY, Camera.main);
            ApplyBoardWidth(ViewModel.BoardHalfWidth, ViewModel.BoardCenterX, Camera.main);
#endif

            return UniTask.CompletedTask;
        }

        protected override void OnClose()
        {
            _floatingPool?.Dispose();
            _floatingPool = null;
        }


#if UNITY_EDITOR
        private void Update()
        {
            if (false == Application.isPlaying || null == ViewModel || !_referenceRect)
                return;

            ApplyTopSafeAreaOffset();
            PositionPlayerPanel(ViewModel.BoardTopWorldY, _cachedCamera);
            ApplyBoardWidth(ViewModel.BoardHalfWidth, ViewModel.BoardCenterX, _cachedCamera);
        }
#endif

        private void ApplyTopSafeAreaOffset()
        {
            if (!_canvas)
                return;

            var topInsetPx = Screen.height - Screen.safeArea.yMax;
            var topInsetCanvas = topInsetPx / _canvas.scaleFactor;

            _topContainer.anchoredPosition = new Vector2(0f, -topInsetCanvas);
        }

        private void ApplyBoardWidth(float boardHalfWidth, float boardCenterX, Camera cam)
        {
            if (!cam || !_canvas || !_referenceRect)
                return;

            var leftWorld = new Vector3(boardCenterX - boardHalfWidth, 0f, 0f);
            var rightWorld = new Vector3(boardCenterX + boardHalfWidth, 0f, 0f);

            var camForCanvas = _isOverlay ? null : _canvas.worldCamera;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _referenceRect, cam.WorldToScreenPoint(leftWorld), camForCanvas, out var leftLocal);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _referenceRect, cam.WorldToScreenPoint(rightWorld), camForCanvas, out var rightLocal);

            var boardCanvasWidth = rightLocal.x - leftLocal.x;
            _topContainer.sizeDelta = new Vector2(boardCanvasWidth, _topContainer.sizeDelta.y);
        }

        private void PositionPlayerPanel(float boardTopWorldY, Camera cam)
        {
            if (!cam || !_canvas || !_referenceRect)
                return;

            var worldPos = new Vector3(cam.transform.position.x, boardTopWorldY, 0f);
            var screenPoint = (Vector2)cam.WorldToScreenPoint(worldPos);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _referenceRect,
                screenPoint,
                _isOverlay ? null : _canvas.worldCamera,
                out var localPoint);

            var referenceHeight = _referenceRect.rect.height;
            _playerPanel.anchoredPosition = new Vector2(0f, localPoint.y + referenceHeight * 0.5f + _playerPanelPadding);
        }

        private void SpawnDamageNumber(int damage, RectTransform anchor)
        {
            if (null == _floatingPool)
                return;

            var item = _floatingPool.Get();
            item.Play(damage, anchor, ViewModel.BattleAnimConfig, () => _floatingPool.Release(item));
        }

        private void BindHeroSlots(HeroSlotView[] views, HeroSlotViewModel[] viewModels)
        {
            if (null == views || null == viewModels)
                return;

            var count = Mathf.Min(views.Length, viewModels.Length);
            for (var i = 0; i < count; i++)
            {
                if (views[i])
                    views[i].Bind(viewModels[i]);
            }
        }
    }
}
