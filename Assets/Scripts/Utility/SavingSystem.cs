using System;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

namespace Assets.Scripts.Utility {
    public static class SavingSystem {
        public enum ScoreStrings {
            Level01,
            Level02,
            Level03
        }

        public static int GetScore(ScoreStrings scoreString)
        {
            return PlayerPrefs.GetInt(GetScoreString(scoreString));
        }

        public static void SetScore(ScoreStrings scoreString, int score)
        {
            PlayerPrefs.SetInt(GetScoreString(scoreString), score);
        }

        public static float GetTime(ScoreStrings scoreString)
        {
            return PlayerPrefs.GetFloat(GetTimeString(scoreString));
        }

        public static string GetTimeFormatted(ScoreStrings scoreString)
        {
            return FormatTime(PlayerPrefs.GetFloat(GetTimeString(scoreString)));
        }

        public static void SetTime(ScoreStrings scoreString, float time)
        {
            PlayerPrefs.SetFloat(GetTimeString(scoreString), time);
        }

        public static void ResetAll()
        {
            SetScore(ScoreStrings.Level01, 0);
            SetScore(ScoreStrings.Level02, 0);
            SetScore(ScoreStrings.Level03, 0);
            SetTime(ScoreStrings.Level01, 0);
            SetTime(ScoreStrings.Level02, 0);
            SetTime(ScoreStrings.Level03, 0);
        }

        private static string GetScoreString(ScoreStrings scoreString)
        {
            return scoreString switch
            {
                ScoreStrings.Level01 => "Level01Score",
                ScoreStrings.Level02 => "Level02Score",
                ScoreStrings.Level03 => "Level03Score",
                _                    => "HighScore"
            };
        }

        private static string GetTimeString(ScoreStrings scoreString)
        {
            return scoreString switch
            {
                ScoreStrings.Level01 => "Level01Time",
                ScoreStrings.Level02 => "Level02Time",
                ScoreStrings.Level03 => "Level03Time",
                _                    => "BestTime"
            };
        }

        public static string FormatTime(float time)
        {
            float minutes = time / 60;
            float seconds = time % 60;
            //float millis = (time * 1000) % 1000;
            return Mathf.FloorToInt(minutes).ToString() + ":" + Mathf.FloorToInt(seconds).ToString("00"); // + ":" + Mathf.FloorToInt(millis).ToString("000");
        }
    }
}