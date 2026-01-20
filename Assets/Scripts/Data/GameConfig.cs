using UnityEngine;

namespace BlockBlast.Data
{
    /// <summary>
    /// Cấu hình chính của game
    /// </summary>
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Block Blast/Game Config")]
    public class GameConfig : ScriptableObject
    {
        [Header("Board Settings")]
        public int boardWidth = 8;
        public int boardHeight = 8;

        [Header("Block Settings")]
        public int blockSpawnCount = 3;
        public float cellSize = 1f;
        public float blockSpacing = 2f;

        [Header("Scoring")]
        public int baseScorePerCell = 1;
        public int baseScorePerLine = 8;
        public float comboMultiplier = 0.1f; // Công thức: 8 * (1 + combo * 0.1)

        [Header("Combo Settings")]
        public int moveCountForCombo = 3; // Số nước đi để duy trì combo

        [Header("Animation Settings")]
        public float blockPlacementDuration = 0.2f;
        public float blockReturnDuration = 0.3f;
        public float lineClearDelay = 0.3f;
        public float cellDestroyDuration = 0.2f;

        [Header("Visual Settings")]
        public Sprite emptyCellSprite;
        public Sprite cellBackgroundSprite;
        public Sprite boardBackgroundSprite;
        
        [Header("Block Sprites")]
        public Sprite[] blockStoneSprites; // blueStone, redStone, greenStone, etc.
    }
}
