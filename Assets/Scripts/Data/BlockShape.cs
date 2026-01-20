using System.Collections.Generic;
using UnityEngine;

namespace BlockBlast.Data
{
    /// <summary>
    /// Định nghĩa hình dạng của các khối block
    /// </summary>
    [System.Serializable]
    public class BlockShape
    {
        public int width;
        public int height;
        public List<Vector2Int> cells; // Vị trí các ô trong khối (relative positions)

        public BlockShape(int[,] pattern)
        {
            height = pattern.GetLength(0);
            width = pattern.GetLength(1);
            cells = new List<Vector2Int>();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (pattern[y, x] == 1)
                    {
                        cells.Add(new Vector2Int(x, y));
                    }
                }
            }
        }

        public int CellCount => cells.Count;
    }
}
