using System;
using System.Collections.Generic;
using UnityEngine;
using BlockBlast.Data;
using BlockBlast.Effects;
using BlockBlast.Utils;

namespace BlockBlast.Core
{
    /// <summary>
    /// Quản lý bảng chơi 8x8
    /// Coordinate với các handler để xử lý logic
    /// </summary>
    public class BoardManager : MonoBehaviour
    {
        #region Events
        public event Action<int, int> OnCellFilled;
        public event Action<List<int>> OnRowsCleared;
        public event Action<List<int>> OnColumnsCleared;
        #endregion

        #region Serialized Fields
        [SerializeField] private GameObject cellPrefab;
        [SerializeField] private Transform boardContainer;
        [SerializeField] private GameObject destroyEffectPrefab;
        
        [Header("Clear Effect Settings")]
        [SerializeField] private float clearEffectDuration = 0.3f;
        #endregion

        #region Private Fields
        private GameConfig config;
        private Cell[,] cells;
        private int width;
        private int height;
        
        // Handlers
        private BoardLineChecker lineChecker;
        private BoardClearEffectHandler clearEffectHandler;
        private BoardPreviewHandler previewHandler;
        #endregion

        #region Initialization
        public void Initialize(GameConfig gameConfig)
        {
            config = gameConfig;
            width = config.boardWidth;
            height = config.boardHeight;
            cells = new Cell[width, height];

            CreateBoard();
            InitializeHandlers();
        }

        private void CreateBoard()
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    GameObject cellObj = Instantiate(cellPrefab, boardContainer);
                    cellObj.transform.localPosition = new Vector3(x * config.cellSize, y * config.cellSize, 0);
                    cellObj.name = $"Cell_{x}_{y}";

                    Cell cell = cellObj.GetComponent<Cell>();
                    cell.Initialize(x, y, false);
                    cell.SetFilled(false, null, config.cellBackgroundSprite);
                    cells[x, y] = cell;
                }
            }

            CenterBoard();
        }

        private void CenterBoard()
        {
            Vector3 offset = new Vector3(
                -width * config.cellSize / 2f + config.cellSize / 2f,
                -height * config.cellSize / 2f + config.cellSize / 2f,
                0
            );
            boardContainer.localPosition = offset;
        }

        private void InitializeHandlers()
        {
            lineChecker = new BoardLineChecker(cells, width, height);
            clearEffectHandler = new BoardClearEffectHandler(cells, clearEffectDuration, config.cellBackgroundSprite, SpawnDestroyEffect);
            previewHandler = new BoardPreviewHandler(cells, width, height, lineChecker);
        }
        #endregion

        #region Block Placement
        public bool CanPlaceBlock(BlockShape shape, Vector2Int position)
        {
            foreach (Vector2Int cell in shape.cells)
            {
                int x = position.x + cell.x;
                int y = position.y + cell.y;

                if (!IsValidCell(x, y) || cells[x, y].IsFilled)
                    return false;
            }
            return true;
        }

        public void PlaceBlock(BlockShape shape, Vector2Int position, Sprite stoneSprite)
        {
            clearEffectHandler.ClearEffectSprite = stoneSprite;
            
            foreach (Vector2Int cell in shape.cells)
            {
                int x = position.x + cell.x;
                int y = position.y + cell.y;

                cells[x, y].SetFilled(true, stoneSprite, config.cellBackgroundSprite);
                OnCellFilled?.Invoke(x, y);
            }
        }

        public bool CanPlaceAnyBlock(List<BlockShape> blocks)
        {
            foreach (BlockShape block in blocks)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        if (CanPlaceBlock(block, new Vector2Int(x, y)))
                            return true;
                    }
                }
            }
            return false;
        }
        #endregion

        #region Line Clearing
        public (List<int> rows, List<int> columns) CheckAndClearLines()
        {
            var (rows, columns) = lineChecker.FindCompletedLines();

            if (rows.Count > 0 || columns.Count > 0)
            {
                var cellsToClear = lineChecker.GetCellsToClear(rows, columns);
                StartCoroutine(clearEffectHandler.PlayClearEffect(
                    cellsToClear, 
                    rows, 
                    columns,
                    OnRowsCleared,
                    OnColumnsCleared
                ));
            }

            return (rows, columns);
        }
        #endregion

        #region Preview System
        /// <summary>
        /// Chuẩn bị hiệu ứng clear TRƯỚC khi ClearPreview được gọi
        /// </summary>
        public void PrepareClearEffect(BlockShape shape, Vector2Int position, Sprite blockSprite)
        {
            var (rows, columns) = lineChecker.SimulatePlacement(shape, position);
            var cellsToClear = lineChecker.GetCellsToClear(rows, columns);
            clearEffectHandler.PrepareClearEffect(cellsToClear, blockSprite);
        }

        public void HighlightPreview(BlockShape shape, Vector2Int position, bool canPlace, Sprite blockSprite)
        {
            previewHandler.ShowPreview(shape, position, canPlace, blockSprite);
        }

        public void ClearPreview()
        {
            previewHandler.ClearPreview(clearEffectHandler.CellsPendingClear);
            
            // Reset sprite nếu không có cells pending
            if (clearEffectHandler.CellsPendingClear.Count == 0)
            {
                clearEffectHandler.ClearEffectSprite = null;
            }
        }
        #endregion

        #region Utility Methods
        private bool IsValidCell(int x, int y)
        {
            return x >= 0 && x < width && y >= 0 && y < height;
        }

        public Vector3 GetWorldPosition(Vector2Int gridPosition)
        {
            return boardContainer.TransformPoint(new Vector3(
                gridPosition.x * config.cellSize,
                gridPosition.y * config.cellSize,
                0
            ));
        }

        public Vector2Int GetGridPosition(Vector3 worldPosition)
        {
            Vector3 localPos = boardContainer.InverseTransformPoint(worldPosition);
            int x = Mathf.RoundToInt(localPos.x / config.cellSize);
            int y = Mathf.RoundToInt(localPos.y / config.cellSize);
            return new Vector2Int(x, y);
        }

        private void SpawnDestroyEffect(int x, int y)
        {
            if (destroyEffectPrefab == null)
                return;

            CellDestroyEffect effectPrefab = destroyEffectPrefab.GetComponent<CellDestroyEffect>();
            if (effectPrefab == null)
            {
                Debug.LogError("Destroy effect prefab không có CellDestroyEffect component!");
                return;
            }

            // Dùng singleton instance
            if (ObjectPoolingCellDestroyEffect.Instant == null)
            {
                Debug.LogError("[BoardManager] Chưa có ObjectPoolingCellDestroyEffect trong scene! Tạo GameObject và add component.");
                return;
            }

            Vector3 worldPos = GetWorldPosition(new Vector2Int(x, y));
            CellDestroyEffect effect = ObjectPoolingCellDestroyEffect.Instant.GetObjectType(effectPrefab);
            
            if (effect != null)
            {
                effect.PlayEffect(worldPos, cells[x, y].GetCurrentStoneSprite());
            }
        }
        #endregion

        #region Board State Management
        public void ClearBoard()
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    cells[x, y].SetFilled(false, null, config.cellBackgroundSprite);
                }
            }
        }

        public int[,] GetBoardState()
        {
            int[,] state = new int[width, height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    state[x, y] = cells[x, y].IsFilled ? 1 : 0;
                }
            }
            return state;
        }

        public void LoadBoardState(int[,] state)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool isFilled = state[x, y] == 1;
                    Sprite sprite = isFilled && config.blockStoneSprites.Length > 0 
                        ? config.blockStoneSprites[0] 
                        : null;
                    cells[x, y].SetFilled(isFilled, sprite, config.cellBackgroundSprite);
                }
            }
        }
        #endregion
    }
}
