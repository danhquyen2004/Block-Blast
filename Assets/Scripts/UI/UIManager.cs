using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BlockBlast.UI
{
    /// <summary>
    /// Quản lý UI chính của game
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("Score UI")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI bestScoreText;
        
        [Header("Combo UI")]
        [SerializeField] private TextMeshProUGUI comboText;
        [SerializeField] private GameObject comboPanel;
        [SerializeField] private Animator comboAnimator;

        [Header("Game Over UI")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private TextMeshProUGUI finalScoreText;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button continueButton;

        [Header("Menu UI")]
        [SerializeField] private GameObject menuPanel;
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button loadGameButton;

        public void Initialize()
        {
            UpdateScore(0);
            UpdateBestScore(0);
            UpdateCombo(0);
            HideGameOver();
            HideMenu();
        }

        public void UpdateScore(int score)
        {
            if (scoreText != null)
                scoreText.text = $"Score: {score}";
        }

        public void UpdateBestScore(int bestScore)
        {
            if (bestScoreText != null)
                bestScoreText.text = $"Best: {bestScore}";
        }

        public void UpdateCombo(int combo)
        {
            if (combo > 0)
            {
                if (comboText != null)
                    comboText.text = $"Combo x{combo}";
                
                if (comboPanel != null)
                    comboPanel.SetActive(true);

                // Trigger animation
                if (comboAnimator != null)
                    comboAnimator.SetTrigger("Show");
            }
            else
            {
                if (comboPanel != null)
                    comboPanel.SetActive(false);
            }
        }

        public void ShowGameOver(int finalScore)
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
                if (finalScoreText != null)
                    finalScoreText.text = $"Final Score: {finalScore}";
            }
        }

        public void HideGameOver()
        {
            if (gameOverPanel != null)
                gameOverPanel.SetActive(false);
        }

        public void ShowMenu()
        {
            if (menuPanel != null)
                menuPanel.SetActive(true);
        }

        public void HideMenu()
        {
            if (menuPanel != null)
                menuPanel.SetActive(false);
        }

        public void SetRestartButtonListener(System.Action callback)
        {
            if (restartButton != null)
                restartButton.onClick.AddListener(() => callback());
        }

        public void SetContinueButtonListener(System.Action callback)
        {
            if (continueButton != null)
                continueButton.onClick.AddListener(() => callback());
        }

        public void SetNewGameButtonListener(System.Action callback)
        {
            if (newGameButton != null)
                newGameButton.onClick.AddListener(() => callback());
        }

        public void SetLoadGameButtonListener(System.Action callback)
        {
            if (loadGameButton != null)
                loadGameButton.onClick.AddListener(() => callback());
        }
    }
}
