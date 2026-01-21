using UnityEngine;

namespace BlockBlast.Utils
{
    /// <summary>
    /// Quản lý âm thanh trong game
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Clear Sounds (Combo 1-9)")]
        [SerializeField] private AudioClip clear1Sound;
        [SerializeField] private AudioClip clear2Sound;
        [SerializeField] private AudioClip clear3Sound;
        [SerializeField] private AudioClip clear4Sound;
        [SerializeField] private AudioClip clear5Sound;
        [SerializeField] private AudioClip clear6Sound;
        [SerializeField] private AudioClip clear7Sound;
        [SerializeField] private AudioClip clear8Sound;
        [SerializeField] private AudioClip clear9Sound;

        [Header("Block Sounds")]
        [SerializeField] private AudioClip selectBlockSound;
        [SerializeField] private AudioClip dropdownBlockSound;

        [Header("Other Sounds")]
        [SerializeField] private AudioClip gameOverSound;
        [SerializeField] private AudioClip buttonClickSound;

        [Header("Audio Sources")]
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource musicSource;

        private AudioClip[] clearSounds;

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
                return;
            }

            // Tạo AudioSource nếu chưa có
            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
                sfxSource.volume = 1f;
                Debug.Log("AudioManager: Created SFX AudioSource");
            }

            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.playOnAwake = false;
                musicSource.loop = true;
                musicSource.volume = 0.5f;
                Debug.Log("AudioManager: Created Music AudioSource");
            }

            // Lưu array của clear sounds để dễ truy cập
            clearSounds = new AudioClip[]
            {
                clear1Sound, clear2Sound, clear3Sound, clear4Sound, clear5Sound,
                clear6Sound, clear7Sound, clear8Sound, clear9Sound
            };
            
            Debug.Log($"AudioManager initialized. SFX Source: {sfxSource != null}, Select Sound: {selectBlockSound != null}, Drop Sound: {dropdownBlockSound != null}");
        }

        /// <summary>
        /// Phát âm thanh khi select block
        /// </summary>
        public void PlaySelectBlock()
        {
            Debug.Log("AudioManager: PlaySelectBlock called");
            PlaySFX(selectBlockSound);
        }

        /// <summary>
        /// Phát âm thanh khi drop block xuống board
        /// </summary>
        public void PlayDropBlock()
        {
            Debug.Log("AudioManager: PlayDropBlock called");
            PlaySFX(dropdownBlockSound);
        }

        /// <summary>
        /// Phát âm thanh clear dựa theo combo hiện tại (1-9+)
        /// </summary>
        /// <param name="comboCount">Số combo hiện tại, nếu >9 sẽ dùng sound của combo 9</param>
        public void PlayClearSound(int comboCount)
        {
            // Clamp combo từ 1-9
            int index = Mathf.Clamp(comboCount, 1, 9) - 1; // Array index 0-8
            
            Debug.Log($"AudioManager: PlayClearSound called with combo {comboCount}, using index {index}");
            
            if (clearSounds != null && index >= 0 && index < clearSounds.Length)
            {
                PlaySFX(clearSounds[index]);
            }
            else
            {
                Debug.LogWarning($"AudioManager: Clear sound at index {index} is null or out of range");
            }
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
                Debug.Log($"AudioManager: Playing sound {clip.name}");
            }
            else
            {
                if (clip == null)
                    Debug.LogWarning("AudioManager: AudioClip is null!");
                if (sfxSource == null)
                    Debug.LogWarning("AudioManager: sfxSource is null!");
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
