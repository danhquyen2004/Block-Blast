using System.Collections.Generic;
using UnityEngine;

namespace BlockBlast.Data
{
    /// <summary>
    /// Chứa tất cả các hình dạng block có thể xuất hiện
    /// </summary>
    public static class BlockShapeData
    {
        private static List<BlockShape> allShapes;

        public static List<BlockShape> AllShapes
        {
            get
            {
                if (allShapes == null)
                {
                    InitializeShapes();
                }
                return allShapes;
            }
        }

        private static void InitializeShapes()
        {
            allShapes = new List<BlockShape>();

            // 1x1 - Ô đơn
            allShapes.Add(new BlockShape(new int[,] { { 1 } }));

            // 2x1 - Thanh ngang 2
            allShapes.Add(new BlockShape(new int[,] { { 1, 1 } }));

            // 1x2 - Thanh dọc 2
            allShapes.Add(new BlockShape(new int[,] { { 1 }, { 1 } }));

            // 3x1 - Thanh ngang 3
            allShapes.Add(new BlockShape(new int[,] { { 1, 1, 1 } }));

            // 1x3 - Thanh dọc 3
            allShapes.Add(new BlockShape(new int[,] { { 1 }, { 1 }, { 1 } }));

            // 2x2 - Ô vuông
            allShapes.Add(new BlockShape(new int[,] { { 1, 1 }, { 1, 1 } }));

            // // 3x3 - Ô vuông lớn
            // allShapes.Add(new BlockShape(new int[,] { 
            //     { 1, 1, 1 }, 
            //     { 1, 1, 1 }, 
            //     { 1, 1, 1 } 
            // }));

            // L-shape
            allShapes.Add(new BlockShape(new int[,] { 
                { 1, 0 }, 
                { 1, 0 }, 
                { 1, 1 } 
            }));

            // L-shape flipped
            allShapes.Add(new BlockShape(new int[,] { 
                { 0, 1 }, 
                { 0, 1 }, 
                { 1, 1 } 
            }));

            // T-shape
            allShapes.Add(new BlockShape(new int[,] { 
                { 1, 1, 1 }, 
                { 0, 1, 0 } 
            }));

            // Z-shape
            allShapes.Add(new BlockShape(new int[,] { 
                { 1, 1, 0 }, 
                { 0, 1, 1 } 
            }));

            // S-shape
            allShapes.Add(new BlockShape(new int[,] { 
                { 0, 1, 1 }, 
                { 1, 1, 0 } 
            }));
        }

        public static BlockShape GetRandomShape()
        {
            int randomIndex = Random.Range(0, AllShapes.Count);
            return AllShapes[randomIndex];
        }

        public static int GetShapeIndex(BlockShape shape)
        {
            if (shape == null) return -1;
            
            for (int i = 0; i < AllShapes.Count; i++)
            {
                if (ShapesAreEqual(AllShapes[i], shape))
                {
                    return i;
                }
            }
            return -1;
        }

        public static BlockShape GetShapeByIndex(int index)
        {
            if (index < 0 || index >= AllShapes.Count)
            {
                return null;
            }
            return AllShapes[index];
        }

        private static bool ShapesAreEqual(BlockShape a, BlockShape b)
        {
            if (a.cells.Count != b.cells.Count) return false;
            
            for (int i = 0; i < a.cells.Count; i++)
            {
                if (a.cells[i] != b.cells[i]) return false;
            }
            return true;
        }
    }
}
