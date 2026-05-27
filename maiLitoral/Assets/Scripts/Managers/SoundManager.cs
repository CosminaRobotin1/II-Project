using UnityEngine;

public class SoundManager : MonoBehaviour {

    /* Attributes */

    public static SoundManager Instance; // Current instance of sound manager

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Music")]
    [SerializeField] private AudioClip backgroundMusic;

    [Header("UI Sounds")]
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioClip backButtonSound;
    [SerializeField] private AudioClip searchSound;
    [SerializeField] private AudioClip successSound;
    [SerializeField] private AudioClip errorSound;

    [Header("Volume")]
    [Range(0f, 1f)]
    [SerializeField] private float musicVolume = 0.5f;

    [Range(0f, 1f)]
    [SerializeField] private float sfxVolume = 1f;

    /* Main Methods */

    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
            return;
        }
        if (musicSource != null) {
            musicSource.loop = true;
            musicSource.volume = musicVolume;
        }
        if (sfxSource != null) {
            sfxSource.loop = false;
            sfxSource.volume = sfxVolume;
        }
    }

    private void Start() {
        PlayBackgroundMusic();
    }

    /* Custom Methods */

    public void PlayBackgroundMusic() {
        if (musicSource == null || backgroundMusic == null) {
            Debug.LogError("Music source clip is invalid!");
            return;
        }
        if (musicSource.clip != backgroundMusic) {
            musicSource.clip = backgroundMusic;
        }
        if (!musicSource.isPlaying) {
            musicSource.Play();
        }
    }
    public void StopBackgroundMusic() {
        if (musicSource == null)
            return;
        musicSource.Stop();
    }
    public void PauseBackgroundMusic() {
        if (musicSource == null)
            return;
        musicSource.Pause();
    }

    public void ResumeBackgroundMusic() {
        if (musicSource == null)
            return;
        if (!musicSource.isPlaying) {
            musicSource.UnPause();
        }
    }
    public void SetMusicVolume(float value) {
        musicVolume = Mathf.Clamp(value, 0f, 1f);
        if (musicSource != null) {
            musicSource.volume = musicVolume;
        }
    }

    public void SetSFXVolume(float value) {
        sfxVolume = Mathf.Clamp(value, 0f, 1f);
        if (sfxSource != null) {
            sfxSource.volume = sfxVolume;
        }
    }
    public void PlaySFX(AudioClip clip) {
        if (sfxSource == null || clip == null) {
            Debug.LogError(clip.name + " is invalid!");
            return;
        }
        sfxSource.PlayOneShot(clip, sfxVolume);
    }
}