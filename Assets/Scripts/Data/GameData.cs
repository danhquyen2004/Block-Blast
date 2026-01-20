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
        public int[,] boardState; // 0 = trống, 1 = có ô
        public BlockShapeInfo[] currentBlocks; // 3 khối hiện tại

        [System.Serializable]
        public class BlockShapeInfo
        {
            public int shapeIndex;
            public bool isPlaced;
        }

        public GameData()
        {
            currentScore = 0;
            bestScore = 0;
            currentCombo = 0;
            movesSinceLastScore = 0;
            boardState = new int[8, 8];
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
