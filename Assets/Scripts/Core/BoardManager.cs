using System;
using System.Collections.Generic;
using UnityEngine;
using BlockBlast.Data;
using BlockBlast.Effects;

namespace BlockBlast.Core
{
    /// <summary>
    /// Quản lý bảng chơi 8x8
    /// </summary>
    public class BoardManager : MonoBehaviour
    {
        public event Action<int, int> OnCellFilled;
        public event Action<List<int>> OnRowsCleared;
        public event Action<List<int>> OnColumnsCleared;

        private GameConfig config;
        private Cell[,] cells;
        private int width;
        private int height;
        private List<Vector2Int> currentPreviewCells = new List<Vector2Int>();

        [SerializeField] private GameObject cellPrefab;
        [SerializeField] private Transform boardContainer;
        [SerializeField] private GameObject destroyEffectPrefab; // Optional: Cell destroy effect

        public void Initialize(GameConfig gameConfig)
        {
            config = gameConfig;
            width = config.boardWidth;
            height = config.boardHeight;
            cells = new Cell[width, height];

            CreateBoard();
        }

        private void CreateBoard()
        {
            // Tạo visual cho board
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

            // Center board
            Vector3 offset = new Vector3(
                -width * config.cellSize / 2f + config.cellSize / 2f,
                -height * config.cellSize / 2f + config.cellSize / 2f,
                0
            );
            boardContainer.localPosition = offset;
        }

        public bool CanPlaceBlock(BlockShape shape, Vector2Int position)
        {
            foreach (Vector2Int cell in shape.cells)
            {
                int x = position.x + cell.x;
                int y = position.y + cell.y;

                // Kiểm tra ngoài board
                if (x < 0 || x >= width || y < 0 || y >= height)
                    return false;

                // Kiểm tra cell đã được lấp đầy
                if (cells[x, y].IsFilled)
                    return false;
            }

            return true;
        }

        public void PlaceBlock(BlockShape shape, Vector2Int position, Sprite stoneSprite)
        {
            foreach (Vector2Int cell in shape.cells)
            {
                int x = position.x + cell.x;
                int y = position.y + cell.y;

                cells[x, y].SetFilled(true, stoneSprite, config.cellBackgroundSprite);
                OnCellFilled?.Invoke(x, y);
            }
        }

        public (List<int> rows, List<int> columns) CheckAndClearLines()
        {
            List<int> rowsToRemove = new List<int>();
            List<int> columnsToRemove = new List<int>();

            // Kiểm tra các hàng ngang
            for (int y = 0; y < height; y++)
            {
                bool isRowFull = true;
                for (int x = 0; x < width; x++)
                {
                    if (!cells[x, y].IsFilled)
                    {
                        isRowFull = false;
                        break;
                    }
                }

                if (isRowFull)
                    rowsToRemove.Add(y);
            }

            // Kiểm tra các cột dọc
            for (int x = 0; x < width; x++)
            {
                bool isColumnFull = true;
                for (int y = 0; y < height; y++)
                {
                    if (!cells[x, y].IsFilled)
                    {
                        isColumnFull = false;
                        break;
                    }
                }

                if (isColumnFull)
                    columnsToRemove.Add(x);
            }

            // Xóa các ô
            foreach (int row in rowsToRemove)
            {
                for (int x = 0; x < width; x++)
                {
                    SpawnDestroyEffect(x, row);
                    cells[x, row].SetFilled(false, null, config.cellBackgroundSprite);
                }
            }

            foreach (int column in columnsToRemove)
            {
                for (int y = 0; y < height; y++)
                {
                    // Tránh spawn effect 2 lần cho cell nằm ở giao điểm
                    bool alreadyDestroyed = false;
                    foreach (int row in rowsToRemove)
                    {
                        if (y == row)
                        {
                            alreadyDestroyed = true;
                            break;
                        }
                    }
                    
                    if (!alreadyDestroyed)
                    {
                        SpawnDestroyEffect(column, y);
                    }
                    
                    cells[column, y].SetFilled(false, null, config.cellBackgroundSprite);
                }
            }

            if (rowsToRemove.Count > 0)
                OnRowsCleared?.Invoke(rowsToRemove);

            if (columnsToRemove.Count > 0)
                OnColumnsCleared?.Invoke(columnsToRemove);

            return (rowsToRemove, columnsToRemove);
        }

        private (List<int> rows, List<int> columns) CheckLinesAfterPlacement(BlockShape shape, Vector2Int position)
        {
            List<int> rowsToRemove = new List<int>();
            List<int> columnsToRemove = new List<int>();

            // Tạo bản sao tạm thời của board state
            bool[,] tempBoard = new bool[width, height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    tempBoard[x, y] = cells[x, y].IsFilled;
                }
            }

            // Đặt block vào bản sao
            foreach (Vector2Int cell in shape.cells)
            {
                int x = position.x + cell.x;
                int y = position.y + cell.y;
                if (x >= 0 && x < width && y >= 0 && y < height)
                {
                    tempBoard[x, y] = true;
                }
            }

            // Kiểm tra các hàng ngang
            for (int y = 0; y < height; y++)
            {
                bool isRowFull = true;
                for (int x = 0; x < width; x++)
                {
                    if (!tempBoard[x, y])
                    {
                        isRowFull = false;
                        break;
                    }
                }
                if (isRowFull)
                    rowsToRemove.Add(y);
            }

            // Kiểm tra các cột dọc
            for (int x = 0; x < width; x++)
            {
                bool isColumnFull = true;
                for (int y = 0; y < height; y++)
                {
                    if (!tempBoard[x, y])
                    {
                        isColumnFull = false;
                        break;
                    }
                }
                if (isColumnFull)
                    columnsToRemove.Add(x);
            }

            return (rowsToRemove, columnsToRemove);
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
        private void SpawnDestroyEffect(int x, int y)
        {
            if (destroyEffectPrefab != null)
            {
                Vector3 worldPos = GetWorldPosition(new Vector2Int(x, y));
                GameObject effectObj = Instantiate(destroyEffectPrefab, worldPos, Quaternion.identity);
                
                CellDestroyEffect effect = effectObj.GetComponent<CellDestroyEffect>();
                if (effect != null)
                {
                    Sprite stoneSprite = cells[x, y].GetCurrentStoneSprite();
                    effect.PlayEffect(worldPos, stoneSprite);
                }
            }
        }

        public void LoadBoardState(int[,] state)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool isFilled = state[x, y] == 1;
                    // Khi load, sử dụng sprite mặc định nếu có fill
                    Sprite sprite = isFilled && config.blockStoneSprites.Length > 0 
                        ? config.blockStoneSprites[0] 
                        : null;
                    cells[x, y].SetFilled(isFilled, sprite, config.cellBackgroundSprite);
                }
            }
        }

        public void HighlightPreview(BlockShape shape, Vector2Int position, bool canPlace, Sprite blockSprite)
        {
            // Clear previous preview
            ClearPreview();

            if (!canPlace)
                return;

            // Show preview với sprite của block
            foreach (Vector2Int cell in shape.cells)
            {
                int x = position.x + cell.x;
                int y = position.y + cell.y;

                // Chỉ preview nếu nằm trong board
                if (x >= 0 && x < width && y >= 0 && y < height)
                {
                    cells[x, y].ShowPreview(blockSprite, canPlace);
                    currentPreviewCells.Add(new Vector2Int(x, y));
                }
            }

            // Kiểm tra xem có dòng/cột nào sẽ bị xóa không
            var (rows, columns) = CheckLinesAfterPlacement(shape, position);

            Debug.Log($"[Preview] Rows to clear: {rows.Count}, Columns to clear: {columns.Count}");
            if (rows.Count > 0)
                Debug.Log($"[Preview] Rows: {string.Join(", ", rows)}");
            if (columns.Count > 0)
                Debug.Log($"[Preview] Columns: {string.Join(", ", columns)}");

            // Bật glow cho các cell trong dòng/cột sẽ bị xóa
            foreach (int row in rows)
            {
                for (int x = 0; x < width; x++)
                {
                    if (cells[x, row].IsFilled)
                    {
                        Debug.Log($"[Glow] Setting glow ON for cell ({x}, {row})");
                        cells[x, row].SetGlow(true);
                        cells[x, row].SetPreviewSpriteForFilledCell(blockSprite);
                    }
                }
            }

            foreach (int column in columns)
            {
                for (int y = 0; y < height; y++)
                {
                    if (cells[column, y].IsFilled)
                    {
                        Debug.Log($"[Glow] Setting glow ON for cell ({column}, {y})");
                        cells[column, y].SetGlow(true);
                        cells[column, y].SetPreviewSpriteForFilledCell(blockSprite);
                    }
                }
            }
        }

        public void ClearPreview()
        {
            // Clear preview sprites
            foreach (Vector2Int pos in currentPreviewCells)
            {
                if (pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height)
                {
                    cells[pos.x, pos.y].ClearPreview();
                }
            }
            currentPreviewCells.Clear();

            // Clear glow và restore sprite trên tất cả cells
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    cells[x, y].ClearPreview(); // Gọi ClearPreview cho tất cả cells
                }
            }
        }
    }
}
