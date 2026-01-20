using System;
using UnityEngine;
using BlockBlast.Data;

namespace BlockBlast.Core
{
    /// <summary>
    /// Quản lý điểm số và combo
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        public event Action<int> OnScoreChanged;
        public event Action<int> OnComboChanged;
        public event Action<int> OnBestScoreChanged;

        private GameConfig config;
        private int currentScore = 0;
        private int bestScore = 0;
        private int currentCombo = 0;
        private int movesSinceLastScore = 0;

        public int CurrentScore => currentScore;
        public int BestScore => bestScore;
        public int CurrentCombo => currentCombo;

        public void Initialize(GameConfig gameConfig)
        {
            config = gameConfig;
            LoadBestScore();
        }

        public void AddScoreForPlacedBlock(int cellCount)
        {
            int score = cellCount * config.baseScorePerCell;
            currentScore += score;
            OnScoreChanged?.Invoke(currentScore);
        }

        public void AddScoreForClearedLines(int lineCount)
        {
            if (lineCount > 0)
            {
                // Tính điểm với combo
                float multiplier = 1f + (currentCombo * config.comboMultiplier);
                int baseScore = lineCount * config.baseScorePerLine;
                int score = Mathf.RoundToInt(baseScore * multiplier);

                currentScore += score;
                OnScoreChanged?.Invoke(currentScore);

                // Cập nhật combo
                currentCombo++;
                movesSinceLastScore = 0;
                OnComboChanged?.Invoke(currentCombo);

                // Kiểm tra best score
                if (currentScore > bestScore)
                {
                    bestScore = currentScore;
                    SaveBestScore();
                    OnBestScoreChanged?.Invoke(bestScore);
                }
            }
            else
            {
                // Không ghi điểm, tăng số nước đi
                movesSinceLastScore++;

                // Reset combo nếu không ghi điểm trong 3 nước đi
                if (movesSinceLastScore >= config.moveCountForCombo)
                {
                    if (currentCombo > 0)
                    {
                        currentCombo = 0;
                        OnComboChanged?.Invoke(currentCombo);
                    }
                }
            }
        }

        public void ResetScore()
        {
            currentScore = 0;
            currentCombo = 0;
            movesSinceLastScore = 0;
            OnScoreChanged?.Invoke(currentScore);
            OnComboChanged?.Invoke(currentCombo);
        }

        private void LoadBestScore()
        {
            bestScore = PlayerPrefs.GetInt("BestScore", 0);
            OnBestScoreChanged?.Invoke(bestScore);
        }

        private void SaveBestScore()
        {
            PlayerPrefs.SetInt("BestScore", bestScore);
            PlayerPrefs.Save();
        }

        public void LoadGameData(GameData data)
        {
            currentScore = data.currentScore;
            currentCombo = data.currentCombo;
            movesSinceLastScore = data.movesSinceLastScore;
            bestScore = data.bestScore;

            OnScoreChanged?.Invoke(currentScore);
            OnComboChanged?.Invoke(currentCombo);
            OnBestScoreChanged?.Invoke(bestScore);
        }

        public void SaveToGameData(GameData data)
        {
            data.currentScore = currentScore;
            data.currentCombo = currentCombo;
            data.movesSinceLastScore = movesSinceLastScore;
            data.bestScore = bestScore;
        }
    }
}
