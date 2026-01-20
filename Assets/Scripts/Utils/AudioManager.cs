using UnityEngine;

namespace BlockBlast.Utils
{
    /// <summary>
    /// Quản lý âm thanh trong game
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Clips")]
        [SerializeField] private AudioClip blockPlaceSound;
        [SerializeField] private AudioClip lineClearSound;
        [SerializeField] private AudioClip comboSound;
        [SerializeField] private AudioClip gameOverSound;
        [SerializeField] private AudioClip buttonClickSound;

        [Header("Audio Sources")]
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource musicSource;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void PlayBlockPlace()
        {
            PlaySFX(blockPlaceSound);
        }

        public void PlayLineClear()
        {
            PlaySFX(lineClearSound);
        }

        public void PlayCombo()
        {
            PlaySFX(comboSound);
        }

        public void PlayGameOver()
        {
            PlaySFX(gameOverSound);
        }

        public void PlayButtonClick()
        {
            PlaySFX(buttonClickSound);
        }

        private void PlaySFX(AudioClip clip)
        {
            if (clip != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(clip);
            }
        }

        public void SetSFXVolume(float volume)
        {
            if (sfxSource != null)
                sfxSource.volume = volume;
        }

        public void SetMusicVolume(float volume)
        {
            if (musicSource != null)
                musicSource.volume = volume;
        }
    }
}
