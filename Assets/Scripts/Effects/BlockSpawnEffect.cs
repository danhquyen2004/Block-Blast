using System.Collections;
using UnityEngine;

namespace BlockBlast.Effects
{
    /// <summary>
    /// Hiệu ứng khi block spawn
    /// </summary>
    public class BlockSpawnEffect : MonoBehaviour
    {
        [SerializeField] private ParticleSystem particleEffect;
        [SerializeField] private AudioSource spawnSound;

        public void PlayEffect(Vector3 position)
        {
            transform.position = position;
            gameObject.SetActive(true);

            if (particleEffect != null)
            {
                particleEffect.Play();
            }

            if (spawnSound != null)
            {
                spawnSound.Play();
            }

            // Tự return về pool sau khi effect xong
            StartCoroutine(ReturnToPoolAfterDelay(2f));
        }

        private IEnumerator ReturnToPoolAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            gameObject.SetActive(false);
        }
    }
}
