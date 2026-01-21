using UnityEngine;

namespace BlockBlast.Core
{
    /// <summary>
    /// Một ô trên bảng chơi
    /// </summary>
    public class Cell : MonoBehaviour
    {
        #region Serialized Fields
        [SerializeField] private SpriteRenderer backgroundRenderer;
        [SerializeField] private SpriteRenderer stoneRenderer;
        [SerializeField] private Material blockCellMaterial;
        #endregion

        #region Properties
        public int X { get; private set; }
        public int Y { get; private set; }
        public bool IsFilled { get; private set; }
        public int CurrentSpriteIndex { get; private set; } // Lưu index của sprite để restore sau khi load game
        #endregion

        #region Private Fields
        private Sprite currentStoneSprite;
        private Sprite originalSpriteBeforePreview;
        #endregion

        #region Initialization
        public void Initialize(int x, int y, bool filled)
        {
            X = x;
            Y = y;
            IsFilled = filled;

            SetupRenderers();
        }

        private void SetupRenderers()
        {
            if (backgroundRenderer == null)
            {
                backgroundRenderer = GetComponent<SpriteRenderer>();
                if (backgroundRenderer != null)
                    backgroundRenderer.sortingOrder = 0;
            }
            
            if (stoneRenderer == null)
            {
                GameObject stoneObj = new GameObject("Stone");
                stoneObj.transform.SetParent(transform);
                stoneObj.transform.localPosition = Vector3.zero;
                stoneRenderer = stoneObj.AddComponent<SpriteRenderer>();
                stoneRenderer.sortingOrder = 1;
                
                if (blockCellMaterial != null)
                {
                    stoneRenderer.material = new Material(blockCellMaterial);
                }
            }
        }
        #endregion

        #region Fill State
        public void SetFilled(bool filled, Sprite stoneSprite, Sprite backgroundSprite, int spriteIndex = 0)
        {
            IsFilled = filled;
            currentStoneSprite = stoneSprite;
            CurrentSpriteIndex = spriteIndex;
            
            if (backgroundRenderer != null)
                backgroundRenderer.sprite = backgroundSprite;
            
            if (stoneRenderer != null)
            {
                stoneRenderer.sprite = filled ? stoneSprite : null;
                stoneRenderer.enabled = filled;
                ResetVisuals();
            }
            
            originalSpriteBeforePreview = null;
        }

        public Sprite GetCurrentStoneSprite()
        {
            return currentStoneSprite;
        }
        #endregion

        #region Preview System
        public void ShowPreview(Sprite previewSprite, bool isValid)
        {
            if (stoneRenderer != null && !IsFilled && isValid)
            {
                stoneRenderer.sprite = previewSprite;
                stoneRenderer.enabled = true;
                stoneRenderer.color = new Color(1f, 1f, 1f, 0.5f);
            }
        }

        public void SetPreviewSpriteForFilledCell(Sprite previewSprite)
        {
            if (stoneRenderer != null && IsFilled)
            {
                if (originalSpriteBeforePreview == null)
                {
                    originalSpriteBeforePreview = stoneRenderer.sprite;
                }
                stoneRenderer.sprite = previewSprite;
            }
        }

        public void ClearPreview()
        {
            // Clear preview cho cells trống
            if (stoneRenderer != null && !IsFilled)
            {
                stoneRenderer.enabled = false;
                stoneRenderer.color = Color.white;
            }
            
            // Restore sprite gốc cho cells đã filled
            if (IsFilled && stoneRenderer != null && originalSpriteBeforePreview != null)
            {
                stoneRenderer.sprite = originalSpriteBeforePreview;
                originalSpriteBeforePreview = null;
            }
            
            SetGlow(false);
        }
        #endregion

        #region Clear Effect
        public void SetClearEffectSprite(Sprite effectSprite)
        {
            if (stoneRenderer != null)
            {
                stoneRenderer.sprite = effectSprite;
                currentStoneSprite = effectSprite;
                stoneRenderer.enabled = true;
                stoneRenderer.color = Color.white;
            }
        }

        public void SetScale(float scale)
        {
            if (stoneRenderer != null)
            {
                stoneRenderer.transform.localScale = Vector3.one * scale;
            }
        }

        public void SetGlow(bool enabled)
        {
            SetRendererGlow(stoneRenderer, enabled && IsFilled);
            SetRendererGlow(backgroundRenderer, enabled);
        }

        private void SetRendererGlow(SpriteRenderer renderer, bool enabled)
        {
            if (renderer != null && renderer.material != null)
            {
                if (enabled)
                    renderer.material.EnableKeyword("GLOW_ON");
                else
                    renderer.material.DisableKeyword("GLOW_ON");
            }
        }

        /// <summary>
        /// Reset tất cả visual state về mặc định
        /// </summary>
        public void ResetVisuals()
        {
            if (stoneRenderer != null)
            {
                stoneRenderer.color = Color.white;
                stoneRenderer.transform.localScale = Vector3.one;
                stoneRenderer.transform.localRotation = Quaternion.identity;
                
                if (stoneRenderer.material != null)
                {
                    stoneRenderer.material.DisableKeyword("GLOW_ON");
                }
            }
        }

        /// <summary>
        /// Lấy transform của stone để animate
        /// </summary>
        public Transform GetStoneTransform()
        {
            return stoneRenderer?.transform;
        }
        #endregion

        #region Deprecated
        public void Highlight(bool enabled, bool isValid)
        {
            // Không dùng nữa - preview sẽ dùng sprite thay vì màu background
        }
        #endregion
    }
}
