using Assets.Scripts.Audio;
using Assets.Scripts.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts {
    public class MainMenuController : MonoBehaviour {
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