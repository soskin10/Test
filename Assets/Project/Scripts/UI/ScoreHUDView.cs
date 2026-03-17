using Project.Scripts.Services;
using R3;
using TMPro;
using UnityEngine;

namespace Project.Scripts.UI
{
    public class ScoreHUDView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _scoreText;


        private CompositeDisposable _disposables = new();


        public void Bind(IScoreService scoreService)
        {
            scoreService.Score
                .Subscribe(score => _scoreText.text = $"Score: {score}")
                .AddTo(_disposables);
        }


        private void OnDestroy()
        {
            _disposables.Dispose();
        }
    }
}
