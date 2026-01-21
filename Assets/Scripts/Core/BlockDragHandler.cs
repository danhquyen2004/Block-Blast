using System;
using UnityEngine;
using BlockBlast.Data;
using DG.Tweening;

namespace BlockBlast.Core
{
    /// <summary>
    /// Xử lý việc drag và drop block lên board
    /// </summary>
    public class BlockDragHandler : MonoBehaviour
    {
        public event Action<Block, Vector2Int> OnBlockPlacedSuccessfully;

        [SerializeField] private BoardManager boardManager;
        [SerializeField] private GameConfig config;

        private Block currentDraggingBlock;
        private Camera mainCamera;
        private bool isProcessingPlacement = false;

        private void Awake()
        {
            mainCamera = Camera.main;
        }

        private void OnDestroy()
        {
            DOTween.Kill(this);
        }

        private void Update()
        {
            if (isProcessingPlacement)
                return;

            // Tìm block đang được drag
            if (currentDraggingBlock == null)
            {
                CheckForBlockDrag();
            }

            if (currentDraggingBlock != null && currentDraggingBlock.IsDragging)
            {
                HandleDragging();
            }
        }

        private void CheckForBlockDrag()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

                if (hit.collider != null)
                {
                    Block block = hit.collider.GetComponent<Block>();
                    if (block != null)
                    {
                        currentDraggingBlock = block;
                    }
                }
            }
        }

        private void HandleDragging()
        {
            if (Input.GetMouseButtonUp(0))
            {
                HandleDrop();
            }
            else
            {
                // Preview vị trí đặt
                Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                
                // Adjust for block's pivot offset to get accurate grid position
                Vector3 adjustedPos = mousePos - currentDraggingBlock.GetPivotOffset();
                Vector2Int gridPos = boardManager.GetGridPosition(adjustedPos);

                // Highlight các ô trên board
                bool canPlace = boardManager.CanPlaceBlock(currentDraggingBlock.Shape, gridPos);
                boardManager.HighlightPreview(currentDraggingBlock.Shape, gridPos, canPlace, currentDraggingBlock.StoneSprite);
            }
        }

        private void HandleDrop()
        {
            if (isProcessingPlacement)
                return;

            Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            
            // Adjust for block's pivot offset
            Vector3 adjustedPos = mousePos - currentDraggingBlock.GetPivotOffset();
            Vector2Int gridPos = boardManager.GetGridPosition(adjustedPos);

            // Prepare clear effect TRƯỚC khi clear preview
            if (boardManager.CanPlaceBlock(currentDraggingBlock.Shape, gridPos))
            {
                boardManager.PrepareClearEffect(currentDraggingBlock.Shape, gridPos, currentDraggingBlock.StoneSprite);
            }

            // Clear preview highlight
            boardManager.ClearPreview();

            if (TryPlaceBlock(currentDraggingBlock, gridPos))
            {
                // Đặt thành công
                isProcessingPlacement = true;
                AnimatePlacement(currentDraggingBlock, gridPos);
            }
            else
            {
                // Đặt thất bại, quay về vị trí ban đầu
                AnimateReturn(currentDraggingBlock);
            }

            currentDraggingBlock = null;
        }

        private bool TryPlaceBlock(Block block, Vector2Int position)
        {
            return boardManager.CanPlaceBlock(block.Shape, position);
        }

        private void AnimatePlacement(Block block, Vector2Int gridPosition)
        {
            Vector3 targetPos = boardManager.GetWorldPosition(gridPosition);
            
            // Adjust for block's pivot offset (block pivot is at center, not bottom-left)
            targetPos += block.GetPivotOffset();
            
            // DOTween animation với bounce effect
            block.transform
                .DOMove(targetPos, config.blockPlacementDuration)
                .SetEase(Ease.OutBack)
                .OnComplete(() => 
                {
                    // Phát âm thanh drop block
                    if (Utils.AudioManager.Instance != null)
                    {
                        Utils.AudioManager.Instance.PlayDropBlock();
                    }
                    
                    // Thông báo cho GameManager
                    OnBlockPlacedSuccessfully?.Invoke(block, gridPosition);
                    isProcessingPlacement = false;
                });
        }

        private void AnimateReturn(Block block)
        {
            Vector3 targetPos = block.OriginalPosition;
            
            // DOTween animation với smooth effect
            Sequence sequence = DOTween.Sequence();
            sequence.Append(block.transform.DOMove(targetPos, config.blockReturnDuration).SetEase(Ease.OutQuad));
            sequence.Join(block.transform.DOScale(block.SizeBlockInitial, config.blockReturnDuration).SetEase(Ease.OutQuad));
            sequence.OnComplete(() => block.ReturnToOriginalPosition());
        }

        public void SetBoardManager(BoardManager manager)
        {
            boardManager = manager;
        }
    }
}
