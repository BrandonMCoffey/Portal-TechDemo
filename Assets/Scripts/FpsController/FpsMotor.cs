using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace Assets.Scripts.FpsController {
    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
    public class FpsMotor : MonoBehaviour {
        private FpsInput _input;

        public float WalkSpeed = 5f;
        public float GravityMultiplier = 1f;
        public float MaxSlopeAngle = 55f;
        public float MaxStepHeight = 0.2f;

        public bool JumpingEnabled = true;
        public float JumpHeight = 3f;
        public bool JumpSecondaryEnabled;

        public bool SprintingEnabled = true;
        public float SprintSpeed = 8f;
        public bool SprintStaminaEnabled;
        public float SprintStaminaDuration = 80f;
        public float SprintStaminaRestoreRate = 5f;

        public bool CrouchingEnabled;
        public float CrouchSpeed = 3f;

        private float _currentSpeed;
        private bool _isCrouched;
        private bool _isSprinting;

        private void Awake()
        {
            _input = GetComponent<FpsInput>();
        }

        private void OnEnable()
        {
            _input.SprintInput += ActivateSprint;
            _input.CrouchInput += ActivateCrouch;
            _input.JumpInput += OnJump;
        }

        private void OnDisable()
        {
            _input.SprintInput -= ActivateSprint;
            _input.CrouchInput -= ActivateCrouch;
            _input.JumpInput -= OnJump;
        }

        private void Update()
        {
            _currentSpeed = _isSprinting ? SprintSpeed : (_isCrouched ? CrouchSpeed : WalkSpeed);
        }

        public void ActivateCrouch(bool activate)
        {
            _isCrouched = activate;
        }

        public void ActivateSprint(bool activate)
        {
            _isSprinting = activate;
        }

        public void OnJump()
        {
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(FpsMotor)), InitializeOnLoad]
    public class FpsMotorEditor : Editor {
        private FpsMotor _setup;

        private void OnEnable()
        {
            _setup = (FpsMotor) target;
        }

        public override void OnInspectorGUI()
        {
            GUILayout.Label("Movement Setup", new GUIStyle(GUI.skin.label) {alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 15});
            EditorGUILayout.Space();

            _setup.WalkSpeed = EditorGUILayout.Slider(new GUIContent("Walk Speed"), _setup.WalkSpeed, 0f, 20f);
            _setup.GravityMultiplier = EditorGUILayout.Slider(new GUIContent("Gravity Multiplier"), _setup.GravityMultiplier, 0f, 10f);
            _setup.MaxSlopeAngle = EditorGUILayout.Slider(new GUIContent("Max Slope Angle"), _setup.MaxSlopeAngle, 0f, 90f);
            _setup.MaxStepHeight = EditorGUILayout.Slider(new GUIContent("Max Step Height"), _setup.MaxStepHeight, 0f, 2f);

            EditorGUILayout.Space();
            _setup.JumpingEnabled = EditorGUILayout.ToggleLeft(new GUIContent("Enable Jumping"), _setup.JumpingEnabled);
            {
                GUI.enabled = _setup.JumpingEnabled;
                EditorGUI.indentLevel++;
                _setup.JumpHeight = EditorGUILayout.Slider(new GUIContent("Jump Height"), _setup.JumpHeight, 0.1f, 16f);
                _setup.JumpSecondaryEnabled = EditorGUILayout.ToggleLeft(new GUIContent("Add a Double Jump"), _setup.JumpSecondaryEnabled);
                EditorGUI.indentLevel--;
                GUI.enabled = true;
            }

            EditorGUILayout.Space();
            _setup.SprintingEnabled = EditorGUILayout.ToggleLeft(new GUIContent("Enable Sprinting"), _setup.SprintingEnabled);
            {
                GUI.enabled = _setup.SprintingEnabled;
                EditorGUI.indentLevel++;
                _setup.SprintSpeed = EditorGUILayout.Slider(new GUIContent("Sprint Speed"), _setup.SprintSpeed, 0.1f, 24f);
                _setup.SprintStaminaEnabled = EditorGUILayout.ToggleLeft(new GUIContent("Enable Sprint Stamina"), _setup.SprintStaminaEnabled);
                {
                    GUI.enabled = _setup.SprintStaminaEnabled;
                    EditorGUI.indentLevel++;
                    _setup.SprintStaminaDuration = EditorGUILayout.Slider(new GUIContent("Stamina Duration"), _setup.SprintStaminaDuration, 0f, 100f);
                    _setup.SprintStaminaRestoreRate = EditorGUILayout.Slider(new GUIContent("Stamina Restore Rate"), _setup.SprintStaminaRestoreRate, 0f, 10f);
                    EditorGUI.indentLevel--;
                }
                EditorGUI.indentLevel--;
                GUI.enabled = true;
            }

            EditorGUILayout.Space();
            _setup.CrouchingEnabled = EditorGUILayout.ToggleLeft(new GUIContent("Enable Crouching"), _setup.CrouchingEnabled);
            {
                GUI.enabled = _setup.CrouchingEnabled;
                EditorGUI.indentLevel++;
                _setup.CrouchSpeed = EditorGUILayout.Slider(new GUIContent("Crouch Walk Speed"), _setup.CrouchSpeed, 0f, 12f);
                EditorGUI.indentLevel--;
                GUI.enabled = true;
            }
        }
    }
#endif
}