using UnityEngine;
using BlockBlast.Data;
using BlockBlast.UI;
using BlockBlast.Effects;
using Cysharp.Threading.Tasks;
using System.Threading;
using DG.Tweening;

namespace BlockBlast.Core
{
    /// <summary>
    /// Game Manager chính - điều khiển toàn bộ flow của game
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameConfig config;
        [SerializeField] private BoardManager boardManager;
        [SerializeField] private BlockSpawner blockSpawner;
        [SerializeField] private ScoreManager scoreManager;
        [SerializeField] private SaveManager saveManager;
        [SerializeField] private UIManager uiManager;
        [SerializeField] private BlockDragHandler dragHandler;
        [SerializeField] private ComboEffectHandler comboEffectHandler;

        private bool isGameOver = false;
        private Vector3 lastBlockPlacementPosition; // Vị trí đặt block cuối cùng

        private void Awake()
        {
            // Setup 60 FPS xuyên suốt game
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0; // Tắt VSync để targetFrameRate có hiệu lực
            
            Debug.Log("Game Settings: Target FPS set to 60");
        }

        private void Start()
        {
            InitializeGame();
        }

        private void InitializeGame()
        {
            // Initialize các manager
            boardManager.Initialize(config);
            blockSpawner.Initialize(config);
            scoreManager.Initialize(config);
            uiManager.Initialize();

            // Subscribe events
            boardManager.OnRowsCleared += OnLinesCleared;
            boardManager.OnColumnsCleared += OnLinesCleared;
            blockSpawner.OnAllBlocksPlaced += OnAllBlocksPlaced;
            scoreManager.OnScoreChanged += uiManager.UpdateScore;
            scoreManager.OnBestScoreChanged += uiManager.UpdateBestScore;
            // Không dùng UI combo panel nữa, dùng ComboEffectHandler
            // scoreManager.OnComboChanged += uiManager.UpdateCombo;
            scoreManager.OnComboChanged += OnComboChanged;
            dragHandler.OnBlockPlacedSuccessfully += OnBlockPlaced;

            // UI Callbacks
            uiManager.SetRestartButtonListener(RestartGame);
            uiManager.SetNewGameButtonListener(StartNewGame);
            uiManager.SetLoadGameButtonListener(LoadGame);

            // Kiểm tra save file và set button interactable
            bool hasSaveFile = saveManager.HasSaveFile();
            uiManager.SetContinueButtonInteractable(hasSaveFile);
            uiManager.SetLoadGameButtonInteractable(hasSaveFile);

            // Bắt đầu game mới hoặc load
            if (hasSaveFile)
            {
                uiManager.ShowMenu();
            }
            else
            {
                StartNewGame();
            }
        }

        private void StartNewGame()
        {
            uiManager.HideMenu();
            boardManager.ClearBoard();
            scoreManager.ResetScore();
            blockSpawner.SpawnBlocks();
            isGameOver = false;
        }

        private void LoadGame()
        {
            uiManager.HideMenu();
            saveManager.LoadGameState(boardManager, scoreManager, blockSpawner);
            isGameOver = false;
        }

        private void RestartGame()
        {
            uiManager.HideGameOver();
            StartNewGame();
        }

        public void OnBlockPlaced(Block block, Vector2Int position)
        {
            // Lưu vị trí đặt block cho combo effect
            lastBlockPlacementPosition = boardManager.GetWorldPosition(position);
            
            // Đặt block lên board
            boardManager.PlaceBlock(block.Shape, position, block.StoneSprite, block.SpriteIndex);
            
            // Cộng điểm cho việc đặt block
            scoreManager.AddScoreForPlacedBlock(block.Shape.CellCount);

            // Kiểm tra và xóa các hàng/cột đầy
            var (rows, columns) = boardManager.CheckAndClearLines();
            int totalLines = rows.Count + columns.Count;
            
            // Cộng điểm cho việc xóa hàng/cột
            scoreManager.AddScoreForClearedLines(totalLines);

            // Lưu game tự động
            AutoSaveDelayedAsync(1f).Forget();

            // Đánh dấu block đã đặt thành công
            block.PlaceSuccess();
            
            // Kiểm tra game over SAU KHI clear effect hoàn tất
            // Nếu có clear lines thì đợi effect duration + thêm buffer time
            float delayTime = totalLines > 0 ? boardManager.GetClearEffectDuration() + 0.1f : 0.3f;
            CheckGameOverDelayedAsync(delayTime).Forget();
        }

        private void OnLinesCleared(System.Collections.Generic.List<int> lines)
        {
            // Có thể thêm hiệu ứng vỡ ở đây
            Debug.Log($"Cleared {lines.Count} lines");
        }
        
        private void OnComboChanged(int comboCount)
        {
            // Hiển thị combo effect tại vị trí đặt block (chỉ từ x2 trở lên)
            if (comboCount >= 2 && comboEffectHandler != null)
            {
                comboEffectHandler.ShowComboAt(lastBlockPlacementPosition, comboCount);
            }
        }

        private void OnAllBlocksPlaced()
        {
            // Sinh 3 block mới
            blockSpawner.SpawnBlocks();

            // Kiểm tra game over
            CheckGameOverDelayedAsync(0.5f).Forget();
        }

        private async UniTaskVoid CheckGameOverDelayedAsync(float delay)
        {
            await UniTask.Delay(System.TimeSpan.FromSeconds(delay), cancellationToken: this.GetCancellationTokenOnDestroy());

            var currentBlocks = blockSpawner.GetCurrentBlockShapes();
            bool canPlace = boardManager.CanPlaceAnyBlock(currentBlocks);

            if (!canPlace)
            {
                GameOver();
            }
        }

        private void GameOver()
        {
            isGameOver = true;
            uiManager.ShowGameOver(scoreManager.CurrentScore);
            saveManager.DeleteSaveFileAsync().Forget();
            
            // Disable continue button vì đã xóa save file
            uiManager.SetContinueButtonInteractable(false);
            uiManager.SetLoadGameButtonInteractable(false);
        }

        private async UniTaskVoid AutoSaveDelayedAsync(float delay)
        {
            await UniTask.Delay(System.TimeSpan.FromSeconds(delay), cancellationToken: this.GetCancellationTokenOnDestroy());
            await saveManager.AutoSaveAsync(boardManager, scoreManager, blockSpawner);
            
            // Enable continue button sau khi save thành công
            uiManager.SetContinueButtonInteractable(true);
            uiManager.SetLoadGameButtonInteractable(true);
        }

        private void OnDestroy()
        {
            // Unsubscribe events
            if (boardManager != null)
            {
                boardManager.OnRowsCleared -= OnLinesCleared;
                boardManager.OnColumnsCleared -= OnLinesCleared;
            }

            if (blockSpawner != null)
            {
                blockSpawner.OnAllBlocksPlaced -= OnAllBlocksPlaced;
            }

            if (scoreManager != null)
            {
                scoreManager.OnScoreChanged -= uiManager.UpdateScore;
                scoreManager.OnBestScoreChanged -= uiManager.UpdateBestScore;
                // scoreManager.OnComboChanged -= uiManager.UpdateCombo;
                scoreManager.OnComboChanged -= OnComboChanged;
            }

            if (dragHandler != null)
            {
                dragHandler.OnBlockPlacedSuccessfully -= OnBlockPlaced;
            }
        }

        private void OnApplicationQuit()
        {
            // Lưu game khi thoát
            if (!isGameOver)
            {
                saveManager.AutoSaveAsync(boardManager, scoreManager, blockSpawner).Forget();
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            // Lưu game khi pause (trên mobile)
            if (pauseStatus && !isGameOver)
            {
                saveManager.AutoSaveAsync(boardManager, scoreManager, blockSpawner).Forget();
            }
        }
    }
}
