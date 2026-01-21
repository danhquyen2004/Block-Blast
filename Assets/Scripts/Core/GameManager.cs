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
            scoreManager.OnComboChanged += uiManager.UpdateCombo;
            scoreManager.OnComboChanged += OnComboChanged;
            dragHandler.OnBlockPlacedSuccessfully += OnBlockPlaced;

            // UI Callbacks
            uiManager.SetRestartButtonListener(RestartGame);
            uiManager.SetNewGameButtonListener(StartNewGame);
            uiManager.SetLoadGameButtonListener(LoadGame);

            // Bắt đầu game mới hoặc load
            if (saveManager.HasSaveFile())
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
            saveManager.LoadGameState(boardManager, scoreManager);
            blockSpawner.SpawnBlocks();
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
            boardManager.PlaceBlock(block.Shape, position, block.StoneSprite);
            
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
            
            // Kiểm tra game over sau khi đặt block
            CheckGameOverDelayedAsync(0.3f).Forget();
        }

        private void OnLinesCleared(System.Collections.Generic.List<int> lines)
        {
            // Có thể thêm hiệu ứng vỡ ở đây
            Debug.Log($"Cleared {lines.Count} lines");
        }
        
        private void OnComboChanged(int comboCount)
        {
            // Hiển thị combo effect tại vị trí đặt block
            if (comboCount > 0 && comboEffectHandler != null)
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
        }

        private async UniTaskVoid AutoSaveDelayedAsync(float delay)
        {
            await UniTask.Delay(System.TimeSpan.FromSeconds(delay), cancellationToken: this.GetCancellationTokenOnDestroy());
            await saveManager.AutoSaveAsync(boardManager, scoreManager, blockSpawner);
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
                scoreManager.OnComboChanged -= uiManager.UpdateCombo;
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
