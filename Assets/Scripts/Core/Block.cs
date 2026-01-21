using System;
using System.Collections.Generic;
using UnityEngine;
using BlockBlast.Data;
using DG.Tweening;

namespace BlockBlast.Core
{
    /// <summary>
    /// Class đại diện cho một khối block có thể drag và drop
    /// </summary>
    public class Block : MonoBehaviour
    {
        public event Action<Block> OnBlockPlaced;
        
        public BlockShape Shape { get; private set; }
        public Sprite StoneSprite { get; private set; }
        public int SpriteIndex { get; private set; }

        [SerializeField] private GameObject cellPrefab;
        private List<GameObject> cellVisuals = new List<GameObject>();
        private Vector3 originalPosition;
        private bool isDragging = false;
        private bool isPlaced = false;
        private Camera mainCamera;
        private float cellSize;
        private Vector3 pivotOffset; // Offset from bottom-left to center

        public Vector3 SizeBlockInitial = new Vector3(0.7f, 0.7f, 0.7f);

        public void Initialize(BlockShape shape, Sprite stoneSprite, int spriteIndex, float size)
        {
            Shape = shape;
            StoneSprite = stoneSprite;
            SpriteIndex = spriteIndex;
            cellSize = size;
            mainCamera = Camera.main;
            originalPosition = transform.position;
            
            // Reset state
            isDragging = false;
            isPlaced = false;

            // Clear old visuals nếu có
            ClearVisuals();

            CreateVisuals();
            
            // Spawn animation
            PlaySpawnAnimation();
        }

        private void ClearVisuals()
        {
            foreach (GameObject cellObj in cellVisuals)
            {
                if (cellObj != null)
                    Destroy(cellObj);
            }
            cellVisuals.Clear();
        }

        private void PlaySpawnAnimation()
        {
            transform.localScale = Vector3.zero;
            transform.DOScale(SizeBlockInitial, 0.3f)
                .SetEase(Ease.OutBack);
        }

        private void CreateVisuals()
        {
            // Tạo visual cho từng cell trong block
            foreach (Vector2Int cellPos in Shape.cells)
            {
                GameObject cellObj = Instantiate(cellPrefab, transform);
                cellObj.transform.localPosition = new Vector3(cellPos.x * cellSize, cellPos.y * cellSize, 0);
                
                SpriteRenderer sr = cellObj.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.sprite = StoneSprite;
                    sr.sortingOrder = 2; // Block spawn với order cao hơn board
                }

                cellVisuals.Add(cellObj);
            }

            // Center the block pivot
            Vector3 center = Vector3.zero;
            foreach (Vector2Int cellPos in Shape.cells)
            {
                center += new Vector3(cellPos.x * cellSize, cellPos.y * cellSize, 0);
            }
            center /= Shape.cells.Count;

            // Store pivot offset for grid position calculation
            pivotOffset = center;

            foreach (GameObject cellObj in cellVisuals)
            {
                cellObj.transform.localPosition -= center;
            }

            // Tự động resize BoxCollider2D để fit với tất cả cells
            UpdateCollider();
        }

        private void UpdateCollider()
        {
            BoxCollider2D collider = GetComponent<BoxCollider2D>();
            if (collider == null)
            {
                collider = gameObject.AddComponent<BoxCollider2D>();
            }

            // Tính bounds của tất cả cells (sau khi đã center)
            Vector2 min = Vector2.one * float.MaxValue;
            Vector2 max = Vector2.one * float.MinValue;

            foreach (GameObject cellObj in cellVisuals)
            {
                Vector3 localPos = cellObj.transform.localPosition;
                
                // Mỗi cell có size = cellSize
                min.x = Mathf.Min(min.x, localPos.x - cellSize / 2f);
                min.y = Mathf.Min(min.y, localPos.y - cellSize / 2f);
                max.x = Mathf.Max(max.x, localPos.x + cellSize / 2f);
                max.y = Mathf.Max(max.y, localPos.y + cellSize / 2f);
            }

            // Set collider size và offset
            Vector2 size = max - min;
            Vector2 centerOffset = (min + max) / 2f;

            collider.size = size;
            collider.offset = centerOffset;
        }

        private void OnMouseDown()
        {
            if (!isPlaced)
            {
                isDragging = true;
                transform.localScale = Vector3.one; // Scale up khi drag
                SetSortingOrder(3); // Nâng lên cao nhất khi drag
                
                // Phát âm thanh select block
                if (Utils.AudioManager.Instance != null)
                {
                    Utils.AudioManager.Instance.PlaySelectBlock();
                }
            }
        }

        private void OnMouseDrag()
        {
            if (isDragging)
            {
                Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                mousePos.z = 0;
                transform.position = mousePos;
            }
        }

        private void OnMouseUp()
        {
            if (isDragging)
            {
                // DOTween scale back
                transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.InBack);
                // Logic đặt block sẽ được xử lý bởi GameManager
            }
        }

        public void PlaceSuccess()
        {
            isPlaced = true;
            OnBlockPlaced?.Invoke(this);
            
            // Tắt ngay lập tức để không che hiệu ứng clear
            gameObject.SetActive(false);
        }

        public void ReturnToOriginalPosition()
        {
            // Animation quay về vị trí ban đầu
            transform.position = originalPosition;
        }

        public bool IsDragging => isDragging;

        public Vector3 OriginalPosition => originalPosition;

        public Vector3 GetPivotOffset() => pivotOffset;

        private void SetSortingOrder(int order)
        {
            foreach (GameObject cellObj in cellVisuals)
            {
                SpriteRenderer sr = cellObj.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.sortingOrder = order;
                }
            }
        }
        
        private void OnDestroy()
        {
            DOTween.Kill(transform);
        }
    }
}
