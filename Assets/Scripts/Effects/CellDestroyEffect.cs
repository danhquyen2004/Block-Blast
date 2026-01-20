using System.Collections;
using UnityEngine;
using BlockBlast.Utils;

namespace BlockBlast.Effects
{
    /// <summary>
    /// Hiệu ứng vỡ khi xóa các ô
    /// </summary>
    public class CellDestroyEffect : MonoBehaviour
    {
        [SerializeField] private ParticleSystem particleEffect;
        [SerializeField] private AudioSource destroySound;

        public void PlayEffect(Vector3 position, Sprite stoneSprite)
        {
            transform.position = position;

            if (particleEffect != null)
            {
                // Lấy màu từ sprite để dùng cho particle
                Color spriteColor = SpriteHelper.GetAverageColor(stoneSprite);
                var main = particleEffect.main;
                main.startColor = spriteColor;
                
                particleEffect.Play();
            }

            if (destroySound != null)
            {
                destroySound.Play();
            }

            // Tự hủy sau khi effect xong
            StartCoroutine(DestroyAfterDelay(2f));
        }

        private IEnumerator DestroyAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            Destroy(gameObject);
        }
    }
}
