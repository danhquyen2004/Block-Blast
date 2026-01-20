using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float scale = Mathf.Lerp(1f, 0f, elapsed / duration);
                
                foreach (Vector2Int pos in cellsToClear)
                {
                    cells[pos.x, pos.y].SetScale(scale);
                }
                
                yield return null;
            }
        }

        private void FinishClearEffect(HashSet<Vector2Int> cellsToClear)
        {
            foreach (Vector2Int pos in cellsToClear)
            {
                Cell cell = cells[pos.x, pos.y];
                cell.SetFilled(false, null, cellBackgroundSprite);
                spawnDestroyEffect?.Invoke(pos.x, pos.y);
            }
        }
        #endregion
    }
}
