using Project.Scripts.Configs;
using Project.Scripts.Configs.Levels;
using Project.Scripts.Constants;
using UnityEngine.SceneManagement;

namespace Project.Scripts.Services
{
    public class LevelProgressionService : ILevelProgressionService
    {
        public static int CurrentLevelId { get; private set; } = 1;


        private readonly LevelDatabase _levelDatabase;


        public LevelProgressionService(LevelDatabase levelDatabase)
        {
            _levelDatabase = levelDatabase;
        }


        public void Advance()
        {
            CurrentLevelId = _levelDatabase.GetNextId(CurrentLevelId);
            LoadScene();
        }

        public void Retry()
        {
            LoadScene();
        }


        private void LoadScene()
        {
            SceneManager.LoadScene(SceneNames.GamePlay);
        }
    }
}