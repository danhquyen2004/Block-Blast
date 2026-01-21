using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace BlockBlast.Core
{
    /// <summary>
    /// Xử lý hiệu ứng khi clear hàng/cột
    /// </summary>
    public class BoardClearEffectHandler
    {
        private readonly Cell[,] cells;
        private readonly Action<int, int> spawnDestroyEffect;
        private readonly float defaultDuration;
        private readonly Sprite cellBackgroundSprite;

        private Sprite clearEffectSprite;
        private HashSet<Vector2Int> cellsPendingClear = new HashSet<Vector2Int>();

        public BoardClearEffectHandler(Cell[,] cells, float duration, Sprite cellBackgroundSprite, Action<int, int> spawnDestroyEffect)
        {
            this.cells = cells;
            this.defaultDuration = duration;
            this.cellBackgroundSprite = cellBackgroundSprite;
            this.spawnDestroyEffect = spawnDestroyEffect;
        }

        /// <summary>
        /// Các cells đang chờ được clear (để preview không reset chúng)
        /// </summary>
        public HashSet<Vector2Int> CellsPendingClear => cellsPendingClear;

        /// <summary>
        /// Sprite hiện tại dùng cho hiệu ứng clear
        /// </summary>
        public Sprite ClearEffectSprite
        {
            get => clearEffectSprite;
            set => clearEffectSprite = value;
        }

        /// <summary>
        /// Chuẩn bị hiệu ứng clear với các cells đã cho
        /// </summary>
        public void PrepareClearEffect(HashSet<Vector2Int> cellsToClear, Sprite sprite)
        {
            cellsPendingClear = cellsToClear;
            clearEffectSprite = sprite;
        }

        /// <summary>
        /// Chạy hiệu ứng clear: sprite change → glow → scale animation → clear
        /// </summary>
        public IEnumerator PlayClearEffect(
            HashSet<Vector2Int> cellsToClear,
            List<int> rows,
            List<int> columns,
            Action<List<int>> onRowsCleared,
            Action<List<int>> onColumnsCleared)
        {
            ApplyClearEffectVisuals(cellsToClear);
            yield return AnimateScale(cellsToClear, defaultDuration);
            FinishClearEffect(cellsToClear);

            onRowsCleared?.Invoke(rows);
            onColumnsCleared?.Invoke(columns);

            Reset();
        }

        /// <summary>
        /// Reset state sau khi clear
        /// </summary>
        public void Reset()
        {
            cellsPendingClear.Clear();
            clearEffectSprite = null;
        }

        #region Private Methods
        private void ApplyClearEffectVisuals(HashSet<Vector2Int> cellsToClear)
        {
            foreach (Vector2Int pos in cellsToClear)
            {
                Cell cell = cells[pos.x, pos.y];
                if (clearEffectSprite != null)
                {
                    cell.SetClearEffectSprite(clearEffectSprite);
                }
                cell.SetGlow(true);
            }
        }

        private IEnumerator AnimateScale(HashSet<Vector2Int> cellsToClear, float duration)
        {
            // Hiệu ứng "vỡ ra": Punch scale → Scale to 0 + Rotate (chỉ stone, không touch background)
            foreach (Vector2Int pos in cellsToClear)
            {
                Cell cell = cells[pos.x, pos.y];
                Transform stoneTransform = cell.GetStoneTransform();
                
                if (stoneTransform == null) continue;
                
                // Sequence: Nảy to → Xoay vỡ → Biến mất
                Sequence sequence = DOTween.Sequence();
                
                // Bước 1: Punch scale (nảy to)
                sequence.Append(stoneTransform.DOPunchScale(Vector3.one * 0.2f, duration * 0.3f, 5, 0.5f));
                
                // Bước 2: Scale về 0 + xoay (vỡ)
                sequence.Append(stoneTransform.DOScale(0f, duration * 0.7f).SetEase(Ease.InBack));
                sequence.Join(stoneTransform.DORotate(new Vector3(0, 0, 180), duration * 0.7f, RotateMode.FastBeyond360).SetEase(Ease.InQuad));
                
                // Delay ngẫu nhiên để các ô không vỡ cùng lúc
                sequence.SetDelay(UnityEngine.Random.Range(0f, 0.05f));
            }
            
            // Đợi hết duration
            yield return new WaitForSeconds(duration);
        }

        private void FinishClearEffect(HashSet<Vector2Int> cellsToClear)
        {
            foreach (Vector2Int pos in cellsToClear)
            {
                Cell cell = cells[pos.x, pos.y];
                
                // Reset stone transform về ban đầu (background không bị ảnh hưởng)
                Transform stoneTransform = cell.GetStoneTransform();
                if (stoneTransform != null)
                {
                    stoneTransform.localScale = Vector3.one;
                    stoneTransform.localRotation = Quaternion.identity;
                }
                
                cell.SetFilled(false, null, cellBackgroundSprite, 0);
                spawnDestroyEffect?.Invoke(pos.x, pos.y);
            }
        }
        #endregion
    }
}
