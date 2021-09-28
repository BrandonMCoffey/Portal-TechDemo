using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Utility;
using Utility.References;

namespace UI
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class VariableText : MonoBehaviour
    {
        [TextArea, SerializeField] private string _textBefore = "";
        [SerializeField] private List<FloatReference> _floatValues = null;
        [SerializeField] private bool _convertToTime = false;
        [TextArea, SerializeField] private string _textAfter = "";

        private TextMeshProUGUI _textField;

        private void Awake()
        {
            _textField = GetComponent<TextMeshProUGUI>();
        }

        private void Update()
        {
            if (_floatValues == null) return;
            float total = _floatValues.Sum(floatValue => floatValue.Value);
            string value = total.ToString("0");
            if (_convertToTime) value = SavingSystem.FormatTime(total);
            _textField.text = _textBefore + value + _textAfter;
        }
    }
}