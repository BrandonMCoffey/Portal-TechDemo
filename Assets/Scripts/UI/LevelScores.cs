using System;
using Assets.Scripts.Utility;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI {
    internal enum ScoreType {
        Score,
        Time
    }

    [RequireComponent(typeof(TextMeshProUGUI))]
    public class LevelScores : MonoBehaviour {
        [SerializeField] private string _textBeforeScore = "Score: ";
        [SerializeField] private SavingSystem.ScoreStrings _scoreString = SavingSystem.ScoreStrings.Level01;
        [SerializeField] private ScoreType _scoreType = ScoreType.Score;
        [SerializeField] private string _textAfterScore = "";

        private TextMeshProUGUI _text;

        private void Awake()
        {
            _text = GetComponent<TextMeshProUGUI>();
        }

        private void OnEnable()
        {
            _text.text = _scoreType switch
            {
                ScoreType.Score => _textBeforeScore + SavingSystem.GetScore(_scoreString) + _textAfterScore,
                ScoreType.Time  => _textBeforeScore + SavingSystem.GetTimeFormatted(_scoreString) + _textAfterScore,
                _               => _textBeforeScore + "0" + _textBeforeScore
            };
        }
    }
}