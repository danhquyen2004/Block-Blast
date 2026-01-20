using System;
using System.Collections.Generic;
using UnityEngine;
using BlockBlast.Data;

namespace BlockBlast.Core
{
    /// <summary>
    /// Quản lý việc sinh và quản lý các block
    /// </summary>
    public class BlockSpawner : MonoBehaviour
    {
        public event Action OnAllBlocksPlaced;

        [SerializeField] private GameObject blockPrefab;
        [SerializeField] private Transform[] spawnPositions; // 3 vị trí spawn
        
        private GameConfig config;
        private List<Block> currentBlocks = new List<Block>();

        public void Initialize(GameConfig gameConfig)
        {
            config = gameConfig;
        }

        public void SpawnBlocks()
        {
            // Xóa các block cũ nếu có
            ClearCurrentBlocks();

            // Tạo 3 block mới
            for (int i = 0; i < config.blockSpawnCount; i++)
            {
                BlockShape shape = BlockShapeData.GetRandomShape();
                Block block = CreateBlock(shape, spawnPositions[i].position);
                currentBlocks.Add(block);
            }
        }

        private Block CreateBlock(BlockShape shape, Vector3 position)
        {
            GameObject blockObj = Instantiate(blockPrefab);
            blockObj.transform.position = position;

            Block block = blockObj.GetComponent<Block>();
            Sprite stoneSprite = config.blockStoneSprites[UnityEngine.Random.Range(0, config.blockStoneSprites.Length)];
            block.Initialize(shape, stoneSprite, config.cellSize);
            block.OnBlockPlaced += OnBlockPlaced;

            return block;
        }

        private void OnBlockPlaced(Block block)
        {
            currentBlocks.Remove(block);

            // Kiểm tra xem đã đặt hết 3 block chưa
            bool allPlaced = true;
            foreach (Block b in currentBlocks)
            {
                if (b != null && b.gameObject.activeInHierarchy)
                {
                    allPlaced = false;
                    break;
                }
            }

            if (allPlaced)
            {
                OnAllBlocksPlaced?.Invoke();
            }
        }

        private void ClearCurrentBlocks()
        {
            foreach (Block block in currentBlocks)
            {
                if (block != null)
                {
                    block.OnBlockPlaced -= OnBlockPlaced;
                    Destroy(block.gameObject);
                }
            }
            currentBlocks.Clear();
        }

        public List<Block> GetCurrentBlocks()
        {
            return currentBlocks;
        }

        public List<BlockShape> GetCurrentBlockShapes()
        {
            List<BlockShape> shapes = new List<BlockShape>();
            foreach (Block block in currentBlocks)
            {
                if (block != null && block.gameObject.activeInHierarchy)
                {
                    shapes.Add(block.Shape);
                }
            }
            return shapes;
        }

        private void OnDestroy()
        {
            ClearCurrentBlocks();
        }
    }
}
