namespace Controllers.Game
{
    using UnityEngine;
    using UnityEngine.UI;

    public class SoundManager : MonoBehaviour
    {
        [SerializeField] private AudioClip   buttonClickAudio;
        [SerializeField] private AudioClip   correctWordAudio;
        [SerializeField] private AudioClip   backgroundAudio;
        [SerializeField] private AudioSource backgroundAudioSource;
        [SerializeField] private AudioSource sfxAudioSource;
        [SerializeField] public Slider volumeSlier;
        [SerializeField] public Image soundOnIcon;
        [SerializeField] public Image soundOffIcon;

        public static SoundManager Instance;
        private bool muted = false;
        private void Start()
        {
            if (!PlayerPrefs.HasKey("musicVolume"))
            {
                PlayerPrefs.SetFloat("musicVolume", 1);
                Load();
            }
            else
            {
                Load();
            }

            if (!PlayerPrefs.HasKey("muted"))
            {
                PlayerPrefs.SetInt("muted", 0);
                LoadSound();
            }
            else
            {
                LoadSound();
            }
            UpdateButtonIcon();
            AudioListener.pause = muted;
        }
        private void Awake()
        {
            Instance                    = this;
            this.backgroundAudioSource.clip = this.backgroundAudio;
        }

        public void PlaySfxSound(string sfxName)
        {
            switch (sfxName)
            {
                case "button_click":
                    this.sfxAudioSource.clip = this.buttonClickAudio;
                    this.sfxAudioSource.Play();
                    break;
                case "correct_word":
                    this.sfxAudioSource.clip = this.correctWordAudio;
                    this.sfxAudioSource.Play();
                    break;
            }
        }
        public void ChangeVolume()
        {
            AudioListener.volume = volumeSlier.value;
        }

        private void Load()
        {
            volumeSlier.value = PlayerPrefs.GetFloat("musicVolume");
        }

        private void LoadSound()
        {
            muted = PlayerPrefs.GetInt("muted") == 1;
        }

        private void SaveVolume()
        {
            PlayerPrefs.SetFloat("musicVolume", volumeSlier.value);
        }

        private void SaveSound()
        {
            PlayerPrefs.SetInt("muted", muted ? 1 : 0);
        }
        public void OnButtonPress()
        {
            if(muted == false)
            {
                muted = true;
                AudioListener.pause = true;
            }
            else
            {
                muted = false;
                AudioListener.pause = false;
            }
            SaveSound();
            UpdateButtonIcon();
        }

        private void UpdateButtonIcon()
        {
            if(muted == false)
            {
                soundOnIcon.enabled = true;
                soundOffIcon.enabled = false;
            }
            else
            {
                soundOnIcon.enabled = false;
                soundOffIcon.enabled = true;
            }  
        }
    }
}