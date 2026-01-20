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
        private Sprite currentBlockSprite; // Lưu sprite của block vừa đặt để dùng cho hiệu ứng clear
        private HashSet<Vector2Int> cellsPendingClear = new HashSet<Vector2Int>(); // Các cell sẽ bị clear, giữ preview

        

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

        public void PrepareClearEffect(BlockShape shape, Vector2Int position, Sprite blockSprite)
        {
            // Lưu sprite cho hiệu ứng clear
            currentBlockSprite = blockSprite;
            
            // Tính toán các cells sẽ bị clear SAU KHI đặt block
            var (rows, columns) = CheckLinesAfterPlacement(shape, position);
            
            // Lưu các cells sẽ bị clear để ClearPreview không reset chúng
            cellsPendingClear.Clear();
            foreach (int row in rows)
            {
                for (int x = 0; x < width; x++)
                {
                    cellsPendingClear.Add(new Vector2Int(x, row));
                }
            }
            foreach (int column in columns)
            {
                for (int y = 0; y < height; y++)
                {
                    cellsPendingClear.Add(new Vector2Int(column, y));
                }
            }
        }

        public void PlaceBlock(BlockShape shape, Vector2Int position, Sprite stoneSprite)
        {
            // Cập nhật sprite hiện tại - đây là sprite thực tế của block vừa đặt
            currentBlockSprite = stoneSprite;
            
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

            // Nếu có dòng/cột để xóa, lưu lại các cell và bắt đầu hiệu ứng
            if (rowsToRemove.Count > 0 || columnsToRemove.Count > 0)
            {
                // Lưu các cell sẽ bị clear để ClearPreview không reset chúng
                cellsPendingClear.Clear();
                foreach (int row in rowsToRemove)
                {
                    for (int x = 0; x < width; x++)
                    {
                        cellsPendingClear.Add(new Vector2Int(x, row));
                    }
                }
                foreach (int column in columnsToRemove)
                {
                    for (int y = 0; y < height; y++)
                    {
                        cellsPendingClear.Add(new Vector2Int(column, y));
                    }
                }
                
                StartCoroutine(ClearLinesWithEffect(rowsToRemove, columnsToRemove));
            }

            return (rowsToRemove, columnsToRemove);
        }

        private System.Collections.IEnumerator ClearLinesWithEffect(List<int> rowsToRemove, List<int> columnsToRemove)
        {
            // Tạo danh sách các cell cần clear (tránh duplicate)
            HashSet<Vector2Int> cellsToClear = new HashSet<Vector2Int>();
            
            foreach (int row in rowsToRemove)
            {
                for (int x = 0; x < width; x++)
                {
                    cellsToClear.Add(new Vector2Int(x, row));
                }
            }
            
            foreach (int column in columnsToRemove)
            {
                for (int y = 0; y < height; y++)
                {
                    cellsToClear.Add(new Vector2Int(column, y));
                }
            }

            // Bước 1: Đảm bảo TẤT CẢ cells đều có sprite và glow đúng
            foreach (Vector2Int pos in cellsToClear)
            {
                Cell cell = cells[pos.x, pos.y];
                // Set sprite cho tất cả cells (kể cả cells đã có preview)
                if (currentBlockSprite != null)
                {
                    cell.SetClearEffectSprite(currentBlockSprite);
                }
                // Đảm bảo glow đang bật
                cell.SetGlow(true);
            }

            // Không cần delay, bắt đầu animation ngay

            // Bước 2: Scale animation (từ 1 về 0)
            float duration = 0.3f;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float scale = Mathf.Lerp(1f, 0f, t);
                
                foreach (Vector2Int pos in cellsToClear)
                {
                    cells[pos.x, pos.y].SetScale(scale);
                }
                
                yield return null;
            }

            // Bước 3: Xóa cells và reset scale
            foreach (Vector2Int pos in cellsToClear)
            {
                Cell cell = cells[pos.x, pos.y];
                cell.SetScale(1f); // Reset scale về 1
                cell.SetGlow(false);
                cell.SetFilled(false, null, config.cellBackgroundSprite);
                
                // Spawn destroy effect nếu có
                SpawnDestroyEffect(pos.x, pos.y);
            }

            // Invoke events
            if (rowsToRemove.Count > 0)
                OnRowsCleared?.Invoke(rowsToRemove);

            if (columnsToRemove.Count > 0)
                OnColumnsCleared?.Invoke(columnsToRemove);
            
            // Clear danh sách pending và reset sprite
            cellsPendingClear.Clear();
            currentBlockSprite = null; // Reset sprite đệ tránh ảnh hưởng block tiếp theo
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

            // Bật glow cho các cell trong dòng/cột sẽ bị xóa
            foreach (int row in rows)
            {
                for (int x = 0; x < width; x++)
                {
                    if (cells[x, row].IsFilled)
                    {
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
            Debug.Log("clear preview called");
            // Clear preview sprites (bỏ qua cells đang pending clear)
            foreach (Vector2Int pos in currentPreviewCells)
            {
                if (pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height)
                {
                    if (!cellsPendingClear.Contains(pos))
                    {
                        cells[pos.x, pos.y].ClearPreview();
                    }
                }
            }
            currentPreviewCells.Clear();

            // Clear glow và restore sprite trên tất cả cells (bỏ qua cells pending clear)
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    if (!cellsPendingClear.Contains(pos))
                    {
                        cells[x, y].ClearPreview(); // Gọi ClearPreview cho cells không pending
                    }
                }
            }
            
            // QUAN TRỌNG: Nếu không có cells pending clear, reset currentBlockSprite
            // Nếu có cells pending, để coroutine tự clear sau
            if (cellsPendingClear.Count == 0)
            {
                currentBlockSprite = null;
            }
        }
    }
}
