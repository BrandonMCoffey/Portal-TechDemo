using Audio;
using UnityEngine;
using Utility;

namespace Game
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private AudioClip _startingSong = null;

        private void Start()
        {
            if (_startingSong != null) {
                if (!AudioManager.Instance.IsPlaying()) {
                    AudioManager.Instance.PlaySong(_startingSong);
                }
            } else {
                Debug.Log("Warning: No Starting Song on Main Menu Controller");
            }
        }

        public void ResetData()
        {
            SavingSystem.ResetAll();
        }

        public void LoadScene(string sceneName)
        {
            SceneLoader.LoadScene(sceneName);
        }

        public void ExitApplication()
        {
            Application.Quit();
        }
    }
}