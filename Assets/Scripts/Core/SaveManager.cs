using System.IO;
using UnityEngine;
using BlockBlast.Data;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace BlockBlast.Core
{
    /// <summary>
    /// Quản lý việc lưu và load game
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        private const string SAVE_FILE_NAME = "gamedata.json";
        private string savePath;

        private void Awake()
        {
            savePath = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
        }

        public async UniTask SaveGameAsync(GameData data, CancellationToken ct = default)
        {
            try
            {
                string json = data.ToJson();
                // Sử dụng async File I/O
                await File.WriteAllTextAsync(savePath, json, ct);
                Debug.Log($"Game saved to: {savePath}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to save game: {e.Message}");
            }
        }

        // Giữ lại phương thức sync cho backward compatibility
        public void SaveGame(GameData data)
        {
            SaveGameAsync(data).Forget();
        }

        public async UniTask<GameData> LoadGameAsync(CancellationToken ct = default)
        {
            if (!HasSaveFile())
            {
                Debug.Log("No save file found");
                return null;
            }

            try
            {
                // Sử dụng async File I/O
                string json = await File.ReadAllTextAsync(savePath, ct);
                GameData data = GameData.FromJson(json);
                Debug.Log($"Game loaded from: {savePath}");
                return data;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load game: {e.Message}");
                return null;
            }
        }

        // Giữ lại sync version
        public GameData LoadGame()
        {
            if (!HasSaveFile())
            {
                Debug.Log("No save file found");
                return null;
            }

            try
            {
                string json = File.ReadAllText(savePath);
                GameData data = GameData.FromJson(json);
                Debug.Log($"Game loaded from: {savePath}");
                return data;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load game: {e.Message}");
                return null;
            }
        }

        public bool HasSaveFile()
        {
            return File.Exists(savePath);
        }

        public async UniTask DeleteSaveFileAsync(CancellationToken ct = default)
        {
            if (HasSaveFile())
            {
                await UniTask.RunOnThreadPool(() => File.Delete(savePath), cancellationToken: ct);
                Debug.Log("Save file deleted");
            }
        }

        public void DeleteSaveFile()
        {
            DeleteSaveFileAsync().Forget();
        }

        public async UniTask AutoSaveAsync(BoardManager boardManager, ScoreManager scoreManager, BlockSpawner blockSpawner, CancellationToken ct = default)
        {
            GameData data = new GameData();
            
            // Lưu board state
            data.boardState = boardManager.GetBoardState();
            data.boardSpriteIndices = boardManager.GetBoardSpriteIndices();
            
            // Debug: Đếm số ô đã fill
            int filledCells = 0;
            for (int i = 0; i < data.boardState.Length; i++)
            {
                if (data.boardState[i] == 1) filledCells++;
            }
            Debug.Log($"[SaveManager] Saving board with {filledCells} filled cells");
            
            // Lưu score data
            scoreManager.SaveToGameData(data);
            
            // Lưu block data
            data.currentBlocks = blockSpawner.GetCurrentBlocksInfo();
            Debug.Log($"[SaveManager] Saving {data.currentBlocks.Length} blocks info");

            await SaveGameAsync(data, ct);
        }

        public async UniTask LoadGameStateAsync(BoardManager boardManager, ScoreManager scoreManager, CancellationToken ct = default)
        {
            GameData data = await LoadGameAsync(ct);
            if (data != null)
            {
                // Load board
                boardManager.LoadBoardState(data.boardState, data.boardSpriteIndices);
                
                // Load score
                scoreManager.LoadGameData(data);
            }
        }

        // Giữ sync version
        public void LoadGameState(BoardManager boardManager, ScoreManager scoreManager, BlockSpawner blockSpawner)
        {
            GameData data = LoadGame();
            if (data != null)
            {
                // Debug: Đếm số ô đã fill
                int filledCells = 0;
                if (data.boardState != null)
                {
                    for (int i = 0; i < data.boardState.Length; i++)
                    {
                        if (data.boardState[i] == 1) filledCells++;
                    }
                }
                Debug.Log($"[SaveManager] Loading board with {filledCells} filled cells");
                
                // Load board
                boardManager.LoadBoardState(data.boardState, data.boardSpriteIndices);
                
                // Load score
                scoreManager.LoadGameData(data);
                
                // Load blocks
                if (data.currentBlocks != null && data.currentBlocks.Length > 0)
                {
                    Debug.Log($"[SaveManager] Loading {data.currentBlocks.Length} saved blocks");
                    blockSpawner.SpawnBlocksFromSave(data.currentBlocks);
                }
                else
                {
                    Debug.Log("[SaveManager] No saved blocks, spawning new ones");
                    blockSpawner.SpawnBlocks();
                }
            }
            else
            {
                Debug.LogWarning("[SaveManager] No save data found to load");
            }
        }
    }
}
