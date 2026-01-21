using System;
using UnityEngine;

namespace BlockBlast.Data
{
    /// <summary>
    /// Dữ liệu game để lưu và load
    /// </summary>
    [System.Serializable]
    public class GameData
    {
        public int currentScore;
        public int bestScore;
        public int currentCombo;
        public int movesSinceLastScore;
        
        // Chuyển từ int[,] sang int[] vì JsonUtility không hỗ trợ mảng 2D
        public int[] boardState; // Flatten array: 0 = trống, 1 = có ô
        public int[] boardSpriteIndices; // Flatten array: Index của sprite cho mỗi ô
        
        public BlockShapeInfo[] currentBlocks; // 3 khối hiện tại

        [System.Serializable]
        public class BlockShapeInfo
        {
            public int shapeIndex;
            public int spriteIndex;
            public bool isPlaced;
        }

        public GameData()
        {
            currentScore = 0;
            bestScore = 0;
            currentCombo = 0;
            movesSinceLastScore = 0;
            boardState = new int[64]; // 8x8 = 64
            boardSpriteIndices = new int[64]; // 8x8 = 64
            currentBlocks = new BlockShapeInfo[3];
        }

        public string ToJson()
        {
            return JsonUtility.ToJson(this);
        }

        public static GameData FromJson(string json)
        {
            return JsonUtility.FromJson<GameData>(json);
        }
    }
}
