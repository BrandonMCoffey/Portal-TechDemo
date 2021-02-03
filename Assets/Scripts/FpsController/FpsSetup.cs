using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace Assets.Scripts.FpsController {
    public class FpsSetup : MonoBehaviour {
        // Health Values
        public bool HealthEnabled = true;
        public int HealthHitPoints = 4;

        // HUD Values
        public bool PauseStopsTime = true;
        public GameObject PauseMenu;
        public bool LockCursorToGame = true;
        public Slider SprintStaminaMeter;
        public Slider HealthMeter;

        // Audio Values
        public float AudioVolume = 5f;
        public AudioClip AudioFootSteps;
        public AudioClip AudioPlayerJump;
        public AudioClip AudioPlayerHurt;
        public AudioClip AudioPlayerDeath;
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(FpsSetup)), InitializeOnLoad]
    public class FpsEditor : Editor {
        private FpsSetup _setup;

        private void OnEnable()
        {
            _setup = (FpsSetup) target;
        }

        public override void OnInspectorGUI()
        {
            GUILayout.Label("Health Setup", new GUIStyle(GUI.skin.label) {alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 15});
            EditorGUILayout.Space();

            _setup.HealthEnabled = EditorGUILayout.ToggleLeft(new GUIContent("Enable Health"), _setup.HealthEnabled);
            {
                GUI.enabled = _setup.HealthEnabled;
                EditorGUILayout.BeginHorizontal();
                EditorGUI.indentLevel++;
                EditorGUILayout.PrefixLabel(new GUIContent("Hit Points(Health)"));
                EditorGUI.indentLevel--;
                _setup.HealthHitPoints = EditorGUILayout.IntField(_setup.HealthHitPoints);
                EditorGUILayout.EndHorizontal();
                GUI.enabled = true;
            }

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("HUD Setup", new GUIStyle(GUI.skin.label) {alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 15});
            EditorGUILayout.Space();

            _setup.PauseStopsTime = EditorGUILayout.ToggleLeft(new GUIContent("Enable Time Stop on Pause"), _setup.PauseStopsTime);
            _setup.PauseMenu = (GameObject) EditorGUILayout.ObjectField(new GUIContent("Pause Menu Panel"), _setup.PauseMenu, typeof(GameObject), true);
            _setup.LockCursorToGame = EditorGUILayout.ToggleLeft(new GUIContent("Lock and Hide Cursor in Game"), _setup.LockCursorToGame);
            _setup.SprintStaminaMeter = (Slider) EditorGUILayout.ObjectField(new GUIContent("Sprint Stamina Meter"), _setup.SprintStaminaMeter, typeof(Slider), true);
            _setup.HealthMeter = (Slider) EditorGUILayout.ObjectField(new GUIContent("Health Meter"), _setup.HealthMeter, typeof(Slider), true);

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Audio Setup", new GUIStyle(GUI.skin.label) {alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 15});
            EditorGUILayout.Space();

            _setup.AudioVolume = EditorGUILayout.Slider(new GUIContent("AudioVolume Level"), _setup.AudioVolume, 0f, 10f);
            EditorGUILayout.Space();

            _setup.AudioFootSteps = (AudioClip) EditorGUILayout.ObjectField(new GUIContent("Footsteps"), _setup.AudioFootSteps, typeof(AudioClip), false);
            _setup.AudioPlayerJump = (AudioClip) EditorGUILayout.ObjectField(new GUIContent("Player Jump"), _setup.AudioPlayerJump, typeof(AudioClip), false);
            _setup.AudioPlayerHurt = (AudioClip) EditorGUILayout.ObjectField(new GUIContent("Player Hurt"), _setup.AudioPlayerHurt, typeof(AudioClip), false);
            _setup.AudioPlayerDeath = (AudioClip) EditorGUILayout.ObjectField(new GUIContent("Player Death"), _setup.AudioPlayerDeath, typeof(AudioClip), false);
        }
    }
#endif
}