using System;
using System.Collections.Generic;
using UnityEngine;
using BlockBlast.Data;
using BlockBlast.Utils;
using BlockBlast.Effects;

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
        [SerializeField] private GameObject spawnEffectPrefab; // Prefab particle effect khi spawn
        
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

            int spriteIndex = UnityEngine.Random.Range(0, config.blockStoneSprites.Length);
            Sprite stoneSprite = config.blockStoneSprites[spriteIndex];
            block.Initialize(shape, stoneSprite, spriteIndex, config.cellSize);
            block.OnBlockPlaced += OnBlockPlaced;

            // Spawn effect
            SpawnBlockEffect(position);

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

        // Lưu thông tin các block hiện tại
        public GameData.BlockShapeInfo[] GetCurrentBlocksInfo()
        {
            GameData.BlockShapeInfo[] blocksInfo = new GameData.BlockShapeInfo[config.blockSpawnCount];
            for (int i = 0; i < config.blockSpawnCount; i++)
            {
                if (i < currentBlocks.Count && currentBlocks[i] != null && currentBlocks[i].gameObject.activeInHierarchy)
                {
                    Block block = currentBlocks[i];
                    blocksInfo[i] = new GameData.BlockShapeInfo
                    {
                        shapeIndex = BlockShapeData.GetShapeIndex(block.Shape),
                        spriteIndex = block.SpriteIndex,
                        isPlaced = false
                    };
                }
                else
                {
                    // Block đã được đặt hoặc không tồn tại
                    blocksInfo[i] = new GameData.BlockShapeInfo
                    {
                        shapeIndex = -1,
                        spriteIndex = 0,
                        isPlaced = true
                    };
                }
            }
            return blocksInfo;
        }

        // Load và spawn blocks từ saved data
        public void SpawnBlocksFromSave(GameData.BlockShapeInfo[] blocksInfo)
        {
            ClearCurrentBlocks();

            if (blocksInfo == null || blocksInfo.Length == 0)
            {
                Debug.Log("[BlockSpawner] No saved blocks, spawning new random blocks");
                SpawnBlocks();
                return;
            }

            bool hasAnyBlock = false;
            for (int i = 0; i < Mathf.Min(blocksInfo.Length, config.blockSpawnCount); i++)
            {
                GameData.BlockShapeInfo info = blocksInfo[i];
                
                if (info != null && !info.isPlaced && info.shapeIndex >= 0)
                {
                    BlockShape shape = BlockShapeData.GetShapeByIndex(info.shapeIndex);
                    if (shape != null)
                    {
                        Block block = CreateBlockWithSprite(shape, spawnPositions[i].position, info.spriteIndex);
                        if (block != null)
                        {
                            currentBlocks.Add(block);
                            hasAnyBlock = true;
                        }
                    }
                }
            }

            // Nếu không có block nào được load, spawn random mới
            if (!hasAnyBlock)
            {
                Debug.Log("[BlockSpawner] No valid saved blocks, spawning new random blocks");
                SpawnBlocks();
            }
        }

        // Tạo block với sprite index cụ thể
        private Block CreateBlockWithSprite(BlockShape shape, Vector3 position, int spriteIndex)
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
            
            if (ObjectPoolingBlock.Instant == null)
            {
                Debug.LogError("[BlockSpawner] Chưa có ObjectPoolingBlock trong scene!");
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

            // Đảm bảo sprite index hợp lệ
            spriteIndex = Mathf.Clamp(spriteIndex, 0, config.blockStoneSprites.Length - 1);
            Sprite stoneSprite = config.blockStoneSprites[spriteIndex];
            block.Initialize(shape, stoneSprite, spriteIndex, config.cellSize);
            block.OnBlockPlaced += OnBlockPlaced;

            // Spawn effect
            SpawnBlockEffect(position);

            return block;
        }

        private void SpawnBlockEffect(Vector3 position)
        {
            if (spawnEffectPrefab == null)
                return;

            BlockSpawnEffect effectPrefab = spawnEffectPrefab.GetComponent<BlockSpawnEffect>();
            if (effectPrefab == null)
            {
                Debug.LogError("[BlockSpawner] Spawn effect prefab không có BlockSpawnEffect component!");
                return;
            }

            if (ObjectPoolingBlockSpawnEffect.Instant == null)
            {
                Debug.LogError("[BlockSpawner] Chưa có ObjectPoolingBlockSpawnEffect trong scene!");
                return;
            }

            BlockSpawnEffect effect = ObjectPoolingBlockSpawnEffect.Instant.GetObjectType(effectPrefab);
            if (effect != null)
            {
                effect.PlayEffect(position);
            }
        }

        private void OnDestroy()
        {
            ClearCurrentBlocks();
        }
    }
}
