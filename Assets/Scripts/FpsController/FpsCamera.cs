using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace Assets.Scripts.FpsController {
    public class FpsCamera : MonoBehaviour {
        public Camera Camera;
        public Transform CameraParent;
        public float CameraSmoothing = 10f;
        public float MouseSensitivity = 10f;
        public float CameraLookUpDownRange = 90f;
        public bool CameraShakeEnabled;
        public bool HeadBobEnabled;
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(FpsCamera)), InitializeOnLoad]
    public class FpsCameraEditor : Editor {
        private FpsCamera _setup;

        private void OnEnable()
        {
            _setup = (FpsCamera) target;
        }

        public override void OnInspectorGUI()
        {
            GUILayout.Label("Camera Setup", new GUIStyle(GUI.skin.label) {alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 15});
            EditorGUILayout.Space();

            _setup.Camera = (Camera) EditorGUILayout.ObjectField(new GUIContent("Player Camera"), _setup.Camera, typeof(Camera), true);
            _setup.CameraParent = (Transform) EditorGUILayout.ObjectField(new GUIContent("Player Camera"), _setup.CameraParent, typeof(Transform), true);
            _setup.CameraLookUpDownRange = EditorGUILayout.Slider(new GUIContent("Look Up Down Range"), _setup.CameraLookUpDownRange, 0f, 90f);
            _setup.MouseSensitivity = EditorGUILayout.Slider(new GUIContent("Mouse Sensitivity"), _setup.MouseSensitivity, 0f, 20f);
            _setup.CameraSmoothing = EditorGUILayout.Slider(new GUIContent("Camera Smoothing"), _setup.CameraSmoothing, 0f, 20f);
            _setup.CameraShakeEnabled = EditorGUILayout.ToggleLeft(new GUIContent("Enable Camera Shake"), _setup.CameraShakeEnabled);
            _setup.HeadBobEnabled = EditorGUILayout.ToggleLeft(new GUIContent("Enable Head Bob"), _setup.HeadBobEnabled);
        }
    }
#endif
}