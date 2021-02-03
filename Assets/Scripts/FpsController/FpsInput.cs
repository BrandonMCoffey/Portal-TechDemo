using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace Assets.Scripts.FpsController {
    public class FpsInput : MonoBehaviour {
        public event Action PauseInput = delegate { };
        public event Action<bool> SprintInput = delegate { };
        public event Action<bool> CrouchInput = delegate { };
        public event Action JumpInput = delegate { };

        public bool InvertMouseX;
        public bool InvertMouseY;
        public KeyCode PauseMenuKey = KeyCode.Escape;
        public KeyCode SprintKey = KeyCode.LeftShift;
        public bool SprintHold = true;
        public KeyCode CrouchKey = KeyCode.LeftControl;
        public bool CrouchHold = true;
        public KeyCode JumpKey = KeyCode.Space;
        public bool JumpHold;

        private bool _isSprinting;
        private bool _isCrouching;

        private void Update()
        {
            if (Input.GetKeyDown(PauseMenuKey)) {
                PauseInput?.Invoke();
                return;
            }

            if (SprintHold) {
                if (Input.GetKeyDown(SprintKey)) {
                    SprintInput?.Invoke(true);
                    _isSprinting = true;
                } else if (Input.GetKeyUp(SprintKey)) {
                    SprintInput?.Invoke(false);
                    _isSprinting = false;
                }
            } else {
                if (Input.GetKeyDown(SprintKey)) {
                    _isSprinting = !_isSprinting;
                    SprintInput?.Invoke(_isSprinting);
                }
            }

            if (CrouchHold) {
                if (Input.GetKeyDown(CrouchKey)) {
                    CrouchInput?.Invoke(true);
                    _isCrouching = true;
                } else if (Input.GetKeyUp(CrouchKey)) {
                    CrouchInput?.Invoke(false);
                    _isCrouching = false;
                }
            } else {
                if (Input.GetKeyDown(CrouchKey)) {
                    _isCrouching = !_isCrouching;
                    CrouchInput?.Invoke(_isCrouching);
                }
            }

            if (Input.GetKeyDown(JumpKey)) {
                JumpInput?.Invoke();
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(FpsInput)), InitializeOnLoad]
    public class FpsInputSetup : Editor {
        private FpsInput _setup;

        private void OnEnable()
        {
            _setup = (FpsInput) target;
        }

        public override void OnInspectorGUI()
        {
            GUILayout.Label("Input Setup", new GUIStyle(GUI.skin.label) {alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 15});
            EditorGUILayout.Space();

            _setup.PauseMenuKey = (KeyCode) EditorGUILayout.EnumPopup(new GUIContent("Pause Menu Key"), _setup.PauseMenuKey);

            EditorGUILayout.Space();
            GUILayout.Label("Camera Input", new GUIStyle(GUI.skin.label) {fontStyle = FontStyle.Bold, fontSize = 15});

            _setup.InvertMouseX = EditorGUILayout.ToggleLeft(new GUIContent("Invert Mouse X"), _setup.InvertMouseX);
            _setup.InvertMouseY = EditorGUILayout.ToggleLeft(new GUIContent("Invert Mouse Y"), _setup.InvertMouseY);

            EditorGUILayout.Space();
            GUILayout.Label("Motor Input", new GUIStyle(GUI.skin.label) {fontStyle = FontStyle.Bold, fontSize = 15});

            _setup.SprintKey = (KeyCode) EditorGUILayout.EnumPopup(new GUIContent("Sprint Key"), _setup.SprintKey);
            _setup.SprintHold = EditorGUILayout.ToggleLeft(new GUIContent("Hold Sprint Key"), _setup.SprintHold);
            _setup.CrouchKey = (KeyCode) EditorGUILayout.EnumPopup(new GUIContent("Crouch Key"), _setup.CrouchKey);
            _setup.CrouchHold = EditorGUILayout.ToggleLeft(new GUIContent("Hold Crouch Key"), _setup.CrouchHold);
            _setup.JumpKey = (KeyCode) EditorGUILayout.EnumPopup(new GUIContent("Jump Key"), _setup.JumpKey);
            _setup.JumpHold = EditorGUILayout.ToggleLeft(new GUIContent("Hold Jump Key"), _setup.JumpHold);
        }
    }
#endif
}