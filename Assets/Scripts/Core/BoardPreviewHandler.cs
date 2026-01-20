using System.Collections.Generic;
using UnityEngine;
using BlockBlast.Data;

namespace BlockBlast.Core
{
    /// <summary>
    /// Xử lý preview khi drag block
    /// </summary>
    public class BoardPreviewHandler
    {
        private readonly Cell[,] cells;
        private readonly int width;
        private readonly int height;
        private readonly BoardLineChecker lineChecker;

        private List<Vector2Int> currentPreviewCells = new List<Vector2Int>();

        public BoardPreviewHandler(Cell[,] cells, int width, int height, BoardLineChecker lineChecker)
        {
            this.cells = cells;
            this.width = width;
            this.height = height;
            this.lineChecker = lineChecker;
        }

        /// <summary>
        /// Hiển thị preview khi drag block
        /// </summary>
        public void ShowPreview(BlockShape shape, Vector2Int position, bool canPlace, Sprite blockSprite)
        {
            ClearPreview(null);

            if (!canPlace)
                return;

            ShowBlockPreview(shape, position, blockSprite);
            HighlightLinesToClear(shape, position, blockSprite);
        }

        /// <summary>
        /// Xóa preview (bỏ qua các cells đang pending clear)
        /// </summary>
        public void ClearPreview(HashSet<Vector2Int> cellsPendingClear)
        {
            // Clear preview cells
            foreach (Vector2Int pos in currentPreviewCells)
            {
                if (IsValidCell(pos.x, pos.y) && !IsCellPending(pos, cellsPendingClear))
                {
                    cells[pos.x, pos.y].ClearPreview();
                }
            }
            currentPreviewCells.Clear();

            // Clear glow trên tất cả cells
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (!IsCellPending(new Vector2Int(x, y), cellsPendingClear))
                    {
                        cells[x, y].ClearPreview();
                    }
                }
            }
        }

        #region Private Methods
        private void ShowBlockPreview(BlockShape shape, Vector2Int position, Sprite blockSprite)
        {
            foreach (Vector2Int cell in shape.cells)
            {
                int x = position.x + cell.x;
                int y = position.y + cell.y;

                if (IsValidCell(x, y))
                {
                    cells[x, y].ShowPreview(blockSprite, true);
                    currentPreviewCells.Add(new Vector2Int(x, y));
                }
            }
        }

        private void HighlightLinesToClear(BlockShape shape, Vector2Int position, Sprite blockSprite)
        {
            var (rows, columns) = lineChecker.SimulatePlacement(shape, position);

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
                        cells[column, y].SetGlow(true);
                        cells[column, y].SetPreviewSpriteForFilledCell(blockSprite);
                    }
                }
            }
        }

        private bool IsValidCell(int x, int y)
        {
            return x >= 0 && x < width && y >= 0 && y < height;
        }

        private bool IsCellPending(Vector2Int pos, HashSet<Vector2Int> cellsPending)
        {
            return cellsPending != null && cellsPending.Contains(pos);
        }
        #endregion
    }
}
