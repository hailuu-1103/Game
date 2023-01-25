namespace Controllers.Game
{
    using UnityEngine;

    public class SoundManager : MonoBehaviour
    {
        [SerializeField] private AudioClip   buttonClickAudio;
        [SerializeField] private AudioClip   correctWordAudio;
        [SerializeField] private AudioClip   backgroundAudio;
        [SerializeField] private AudioSource backgroundAudioSource;
        [SerializeField] private AudioSource sfxAudioSource;

        public static SoundManager Instance;

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
    }
}