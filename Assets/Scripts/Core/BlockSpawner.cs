using System;
using System.Collections.Generic;
using UnityEngine;
using BlockBlast.Data;
using BlockBlast.Utils;

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
                if (block != null)
                {
                    currentBlocks.Add(block);
                }
            }
        }

        private Block CreateBlock(BlockShape shape, Vector3 position)
        {
            if (blockPrefab == null)
            {
                Debug.LogError("[BlockSpawner] Block prefab chưa được assign trong Inspector!");
                return null;
            }
            
            Block blockPrefabComponent = blockPrefab.GetComponent<Block>();
            if (blockPrefabComponent == null)
            {
                Debug.LogError("[BlockSpawner] Block prefab không có Block component!");
                return null;
            }
            
            // Dùng singleton instance
            if (ObjectPoolingBlock.Instant == null)
            {
                Debug.LogError("[BlockSpawner] Chưa có ObjectPoolingBlock trong scene! Tạo GameObject và add component ObjectPoolingBlock.");
                return null;
            }
            
            Block block = ObjectPoolingBlock.Instant.GetObjectType(blockPrefabComponent);
            if (block == null)
            {
                Debug.LogError("[BlockSpawner] Không thể lấy block từ pool!");
                return null;
            }
            
            block.gameObject.SetActive(true);
            block.transform.position = position;

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
                    block.gameObject.SetActive(false);
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
