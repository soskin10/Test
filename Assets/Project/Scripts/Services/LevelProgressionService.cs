using Project.Scripts.Configs;
using Project.Scripts.Constants;
using Project.Scripts.Services.UISystem;
using UnityEngine.SceneManagement;

namespace Project.Scripts.Services
{
    public class LevelProgressionService : ILevelProgressionService
    {
        public static int CurrentLevelId { get; private set; } = 1;


        private readonly LevelDatabase _levelDatabase;
        private readonly UIService _uiService;


        public LevelProgressionService(LevelDatabase levelDatabase, UIService uiService)
        {
            _levelDatabase = levelDatabase;
            _uiService = uiService;
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
            _uiService.CloseAll();
            SceneManager.LoadScene(SceneNames.GamePlay);
        }
    }
}