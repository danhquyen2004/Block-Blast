using UnityEngine;

namespace BlockBlast.Core
{
    /// <summary>
    /// Một ô trên bảng
    /// </summary>
    public class Cell : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer backgroundRenderer;
        [SerializeField] private SpriteRenderer stoneRenderer;
        [SerializeField] private Material blockCellMaterial; // Material có GLOW_ON keyword
        
        public int X { get; private set; }
        public int Y { get; private set; }
        public bool IsFilled { get; private set; }
        private Sprite currentStoneSprite;
        private Sprite originalSpriteBeforePreview; // Lưu sprite gốc trước khi preview

        public void Initialize(int x, int y, bool filled)
        {
            X = x;
            Y = y;
            IsFilled = filled;

            if (backgroundRenderer == null)
            {
                backgroundRenderer = GetComponent<SpriteRenderer>();
                if (backgroundRenderer != null)
                    backgroundRenderer.sortingOrder = 0; // Thấp nhất - background
            }
            
            // Tạo stone renderer nếu chưa có
            if (stoneRenderer == null)
            {
                GameObject stoneObj = new GameObject("Stone");
                stoneObj.transform.SetParent(transform);
                stoneObj.transform.localPosition = Vector3.zero;
                stoneRenderer = stoneObj.AddComponent<SpriteRenderer>();
                stoneRenderer.sortingOrder = 1;
                
                // Sử dụng blockCellMaterial nếu có
                if (blockCellMaterial != null)
                {
                    stoneRenderer.material = new Material(blockCellMaterial);
                }
            }
        }

        public void SetFilled(bool filled, Sprite stoneSprite, Sprite backgroundSprite)
        {
            IsFilled = filled;
            currentStoneSprite = stoneSprite;
            
            if (backgroundRenderer != null)
                backgroundRenderer.sprite = backgroundSprite;
            
            if (stoneRenderer != null)
            {
                stoneRenderer.sprite = filled ? stoneSprite : null;
                stoneRenderer.enabled = filled;
            }
        }

        public void Highlight(bool enabled, bool isValid)
        {
            // Không dùng nữa - preview sẽ dùng sprite thay vì màu background
        }

        public void ShowPreview(Sprite previewSprite, bool isValid)
        {
            if (stoneRenderer != null && !IsFilled && isValid)
            {
                stoneRenderer.sprite = previewSprite;
                stoneRenderer.enabled = true;
                // Chỉ hiện preview với alpha thấp, không có màu tint
                stoneRenderer.color = new Color(1f, 1f, 1f, 0.5f);
            }
        }

        public void ClearPreview()
        {
            // Clear preview cho cells tr\u1ed1ng
            if (stoneRenderer != null && !IsFilled)
            {
                stoneRenderer.enabled = false;
                stoneRenderer.color = Color.white; // Reset v\u1ec1 m\u00e0u b\u00ecnh th\u01b0\u1eddng
            }
            
            // Restore sprite g\u1ed1c cho cells \u0111\u00e3 filled
            if (IsFilled && stoneRenderer != null && originalSpriteBeforePreview != null)
            {
                stoneRenderer.sprite = originalSpriteBeforePreview;
                originalSpriteBeforePreview = null;
            }
            
            SetGlow(false);
        }

        public void SetPreviewSpriteForFilledCell(Sprite previewSprite)
        {
            if (stoneRenderer != null && IsFilled)
            {
                // Lưu sprite hiện tại trước khi thay đổi
                if (originalSpriteBeforePreview == null)
                {
                    originalSpriteBeforePreview = stoneRenderer.sprite;
                }
                // Đổi sang sprite của block đang drag
                stoneRenderer.sprite = previewSprite;
            }
        }

        public void SetGlow(bool enabled)
        {
            // Set glow cho stone renderer (cells đã fill)
            if (stoneRenderer != null && IsFilled)
            {
                Material mat = stoneRenderer.material;
                if (mat != null)
                {
                    if (enabled)
                        mat.EnableKeyword("GLOW_ON");
                    else
                        mat.DisableKeyword("GLOW_ON");
                }
            }
            
            // Cũng có thể set glow cho background renderer
            if (backgroundRenderer != null)
            {
                Material mat = backgroundRenderer.material;
                if (mat != null)
                {
                    if (enabled)
                        mat.EnableKeyword("GLOW_ON");
                    else
                        mat.DisableKeyword("GLOW_ON");
                }
            }
        }

        public Sprite GetCurrentStoneSprite()
        {
            return currentStoneSprite;
        }
    }
}
