using Assets.Scripts.Utility;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI {
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class VariableText : MonoBehaviour {
        [TextArea, SerializeField] private string _textBefore = "";
        [SerializeField] private FloatVariable _floatValue = null;
        [TextArea, SerializeField] private string _textAfter = "";

        private TextMeshProUGUI _textField;

        private void Awake()
        {
            _textField = GetComponent<TextMeshProUGUI>();
        }

        private void Update()
        {
            if (_floatValue == null) return;
            _textField.text = _textBefore + _floatValue.Value + _textAfter;
        }
    }
}