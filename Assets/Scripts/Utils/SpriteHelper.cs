using UnityEngine;

namespace BlockBlast.Utils
{
    /// <summary>
    /// Helper để quản lý và xử lý sprites
    /// </summary>
    public static class SpriteHelper
    {
        /// <summary>
        /// Lấy màu trung bình từ sprite (để dùng cho particle effects)
        /// </summary>
        public static Color GetAverageColor(Sprite sprite)
        {
            if (sprite == null || sprite.texture == null)
                return Color.white;

            Texture2D texture = sprite.texture;
            Color[] pixels = texture.GetPixels(
                (int)sprite.textureRect.x,
                (int)sprite.textureRect.y,
                (int)sprite.textureRect.width,
                (int)sprite.textureRect.height
            );

            float r = 0, g = 0, b = 0;
            int validPixels = 0;

            foreach (Color pixel in pixels)
            {
                if (pixel.a > 0.1f) // Bỏ qua pixel trong suốt
                {
                    r += pixel.r;
                    g += pixel.g;
                    b += pixel.b;
                    validPixels++;
                }
            }

            if (validPixels == 0)
                return Color.white;

            return new Color(r / validPixels, g / validPixels, b / validPixels);
        }

        /// <summary>
        /// Scale sprite renderer để fit vào size mong muốn
        /// </summary>
        public static void ScaleToFit(SpriteRenderer renderer, float targetSize)
        {
            if (renderer == null || renderer.sprite == null)
                return;

            float spriteWidth = renderer.sprite.bounds.size.x;
            float scale = targetSize / spriteWidth;
            renderer.transform.localScale = Vector3.one * scale;
        }

        /// <summary>
        /// Tạo sprite atlas runtime (để optimize draw calls)
        /// </summary>
        public static Sprite[] PackSprites(Sprite[] sprites, out Texture2D atlas)
        {
            atlas = null;
            
            if (sprites == null || sprites.Length == 0)
                return sprites;

            // Implementation for runtime sprite packing
            // (Phức tạp hơn, có thể implement sau nếu cần optimize)
            
            return sprites;
        }
    }
}
