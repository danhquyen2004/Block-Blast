using System.Collections.Generic;
using UnityEngine;
using BlockBlast.Data;

namespace BlockBlast.Core
{
    /// <summary>
    /// Xử lý logic kiểm tra các hàng/cột đầy trên board
    /// </summary>
    public class BoardLineChecker
    {
        private readonly Cell[,] cells;
        private readonly int width;
        private readonly int height;

        public BoardLineChecker(Cell[,] cells, int width, int height)
        {
            this.cells = cells;
            this.width = width;
            this.height = height;
        }

        /// <summary>
        /// Tìm tất cả các hàng và cột đã đầy
        /// </summary>
        public (List<int> rows, List<int> columns) FindCompletedLines()
        {
            List<int> rows = new List<int>();
            List<int> columns = new List<int>();

            for (int y = 0; y < height; y++)
            {
                if (IsRowComplete(y))
                    rows.Add(y);
            }

            for (int x = 0; x < width; x++)
            {
                if (IsColumnComplete(x))
                    columns.Add(x);
            }

            return (rows, columns);
        }

        /// <summary>
        /// Giả lập đặt block và kiểm tra các hàng/cột sẽ bị xóa
        /// </summary>
        public (List<int> rows, List<int> columns) SimulatePlacement(BlockShape shape, Vector2Int position)
        {
            List<int> rows = new List<int>();
            List<int> columns = new List<int>();

            bool[,] tempBoard = CreateBoardSnapshot();
            ApplyBlockToSnapshot(tempBoard, shape, position);

            rows = FindFullRows(tempBoard);
            columns = FindFullColumns(tempBoard);

            return (rows, columns);
        }

        /// <summary>
        /// Lấy danh sách tất cả cells trong các hàng/cột đã cho
        /// </summary>
        public HashSet<Vector2Int> GetCellsToClear(List<int> rows, List<int> columns)
        {
            HashSet<Vector2Int> cellsSet = new HashSet<Vector2Int>();
            
            foreach (int row in rows)
            {
                for (int x = 0; x < width; x++)
                    cellsSet.Add(new Vector2Int(x, row));
            }
            
            foreach (int column in columns)
            {
                for (int y = 0; y < height; y++)
                    cellsSet.Add(new Vector2Int(column, y));
            }
            
            return cellsSet;
        }

        #region Private Methods
        private bool IsRowComplete(int y)
        {
            for (int x = 0; x < width; x++)
            {
                if (!cells[x, y].IsFilled)
                    return false;
            }
            return true;
        }

        private bool IsColumnComplete(int x)
        {
            for (int y = 0; y < height; y++)
            {
                if (!cells[x, y].IsFilled)
                    return false;
            }
            return true;
        }

        private bool[,] CreateBoardSnapshot()
        {
            bool[,] snapshot = new bool[width, height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    snapshot[x, y] = cells[x, y].IsFilled;
                }
            }
            return snapshot;
        }

        private void ApplyBlockToSnapshot(bool[,] snapshot, BlockShape shape, Vector2Int position)
        {
            foreach (Vector2Int cell in shape.cells)
            {
                int x = position.x + cell.x;
                int y = position.y + cell.y;
                if (IsValidCell(x, y))
                {
                    snapshot[x, y] = true;
                }
            }
        }

        private List<int> FindFullRows(bool[,] board)
        {
            List<int> rows = new List<int>();
            for (int y = 0; y < height; y++)
            {
                bool isFull = true;
                for (int x = 0; x < width; x++)
                {
                    if (!board[x, y])
                    {
                        isFull = false;
                        break;
                    }
                }
                if (isFull) rows.Add(y);
            }
            return rows;
        }

        private List<int> FindFullColumns(bool[,] board)
        {
            List<int> columns = new List<int>();
            for (int x = 0; x < width; x++)
            {
                bool isFull = true;
                for (int y = 0; y < height; y++)
                {
                    if (!board[x, y])
                    {
                        isFull = false;
                        break;
                    }
                }
                if (isFull) columns.Add(x);
            }
            return columns;
        }

        private bool IsValidCell(int x, int y)
        {
            return x >= 0 && x < width && y >= 0 && y < height;
        }
        #endregion
    }
}
