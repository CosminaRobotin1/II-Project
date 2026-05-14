using NUnit.Framework;
using UnityEngine;
using System.Reflection;

public class SoundManagerTest {
    [SetUp]
    public void SetUp() { // Resets SoundManager before each test
        DestroyExistingSoundManagers();
        SoundManager.Instance = null;
    }
    [TearDown]
    public void TearDown() { // Cleans SoundManager objects after each test
        DestroyExistingSoundManagers();
        SoundManager.Instance = null;
    }
    private void DestroyExistingSoundManagers() { // Removes old SoundManager objects from previous tests
        SoundManager[] managers = UnityEngine.Object.FindObjectsByType<SoundManager>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );
        foreach (SoundManager manager in managers) {
            if (manager != null) {
                UnityEngine.Object.DestroyImmediate(manager.gameObject);
            }
        }
    }
    private void SetPrivateField(object target, string fieldName, object value) { // Sets a private field using reflection
        target.GetType()
            .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(target, value);
    }
    private T GetPrivateField<T>(object target, string fieldName) { // Reads a private field using reflection
        return (T)target.GetType()
            .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
            .GetValue(target);
    }
    private AudioSource CreateAudioSource(string name) { // Creates an AudioSource for testing
        GameObject audioObject = new GameObject(name);
        return audioObject.AddComponent<AudioSource>();
    }
    private AudioClip CreateAudioClip() { // Creates a small test audio clip
        return AudioClip.Create("TestClip", 44100, 1, 44100, false);
    }
    private SoundManager CreateManager(AudioSource musicSource, AudioSource sfxSource, AudioClip backgroundMusic = null) { // Creates an inactive SoundManager with test references
        GameObject managerObject = new GameObject("SoundManager_TestObject");
        managerObject.SetActive(false); // Prevents Awake from running before private fields are assigned.
        SoundManager manager = managerObject.AddComponent<SoundManager>();
        SetPrivateField(manager, "musicSource", musicSource); // Assign private music source.
        SetPrivateField(manager, "sfxSource", sfxSource); // Assign private sound effect source.
        SetPrivateField(manager, "backgroundMusic", backgroundMusic); // Assign private background music clip.
        return manager;
    }
    private void ActivateManager(SoundManager manager) { // Activates the manager so Awake runs
        manager.gameObject.SetActive(true);
    }
    [Test]
    public void AwakeConfiguresAudioSourcesAndInstance() { // Tests that Awake sets the singleton and configures audio sources
        AudioSource musicSource = CreateAudioSource("MusicSource");
        AudioSource sfxSource = CreateAudioSource("SFXSource");
        SoundManager manager = CreateManager(musicSource, sfxSource);
        ActivateManager(manager); // Awake runs here.
        Assert.AreEqual(manager, SoundManager.Instance); // Instance should point to the first manager.
        Assert.IsTrue(musicSource.loop); // Music should loop.
        Assert.IsFalse(sfxSource.loop); // SFX should not loop.
        Assert.AreEqual(0.5f, musicSource.volume); // Default music volume should be applied.
        Assert.AreEqual(1f, sfxSource.volume); // Default SFX volume should be applied.
    }
    [Test]
    public void SetMusicVolumeUpdatesMusicSourceVolume() { // Tests that music volume changes the AudioSource volume
        AudioSource musicSource = CreateAudioSource("MusicSource");
        AudioSource sfxSource = CreateAudioSource("SFXSource");
        SoundManager manager = CreateManager(musicSource, sfxSource);
        manager.SetMusicVolume(0.25f);
        Assert.AreEqual(0.25f, musicSource.volume); // Music source should receive the new volume.
        Assert.AreEqual(0.25f, GetPrivateField<float>(manager, "musicVolume")); // Private value should also update
    }
    [Test]
    public void SetSFXVolumeUpdatesSFXSourceVolume() { // Tests that SFX volume changes the AudioSource volume
        AudioSource musicSource = CreateAudioSource("MusicSource");
        AudioSource sfxSource = CreateAudioSource("SFXSource");
        SoundManager manager = CreateManager(musicSource, sfxSource);
        manager.SetSFXVolume(0.4f);
        Assert.AreEqual(0.4f, sfxSource.volume); // SFX source should receive the new volume
        Assert.AreEqual(0.4f, GetPrivateField<float>(manager, "sfxVolume")); // Private value should also update
    }
    [Test]
    public void PlayBackgroundMusicWithMissingSourceOrClip() { // Tests that missing music setup is handled safely
        SoundManager managerWithoutSource = CreateManager(null, null, null);
        Assert.DoesNotThrow(() => {
            managerWithoutSource.PlayBackgroundMusic(); // Missing source and clip should return safely
        });
        AudioSource musicSource = CreateAudioSource("MusicSource");
        SoundManager managerWithoutClip = CreateManager(musicSource, null, null);
        Assert.DoesNotThrow(() => {
            managerWithoutClip.PlayBackgroundMusic(); // Missing clip should return safely
        });
    }
    [Test]
    public void PlayBackgroundMusicAssignsBackgroundClip() { // Tests that background music clip is assigned to the music source
        AudioSource musicSource = CreateAudioSource("MusicSource");
        AudioSource sfxSource = CreateAudioSource("SFXSource");
        AudioClip clip = CreateAudioClip();
        SoundManager manager = CreateManager(musicSource, sfxSource, clip);
        manager.PlayBackgroundMusic();
        Assert.AreEqual(clip, musicSource.clip); // Music source should use the background music clip
    }
    [Test]
    public void StopPauseResumeBackgroundMusicWithNull() { // Tests that missing music source is handled safely
        SoundManager manager = CreateManager(null, null, null);
        Assert.DoesNotThrow(() => {
            manager.StopBackgroundMusic(); // Null music source should return safely
        });
        Assert.DoesNotThrow(() => {
            manager.PauseBackgroundMusic(); // Null music source should return safely
        });
        Assert.DoesNotThrow(() => {
            manager.ResumeBackgroundMusic(); // Null music source should return safely
        });
    }
    [Test]
    public void PlaySFXWithNullSourceOrClipDoesNotThrow() { // Tests that missing SFX setup is handled safely
        SoundManager managerWithoutSource = CreateManager(null, null, null);
        Assert.DoesNotThrow(() => {
            managerWithoutSource.PlaySFX(CreateAudioClip()); // Null SFX source should return safely
        });
        AudioSource sfxSource = CreateAudioSource("SFXSource");
        SoundManager managerWithoutClip = CreateManager(null, sfxSource, null);
        Assert.DoesNotThrow(() => {
            managerWithoutClip.PlaySFX(null); // Null clip should return safely
        });
    }
    [Test]
    public void SetMusicVolumeAcceptsOutOfRangeValueCurrentBug() { // Tests that music volume is not clamped by SoundManager
        AudioSource musicSource = CreateAudioSource("MusicSource");
        AudioSource sfxSource = CreateAudioSource("SFXSource");
        SoundManager manager = CreateManager(musicSource, sfxSource);
        manager.SetMusicVolume(2f); // This value is outside the intended 0 to 1 range
        Assert.AreEqual(2f, GetPrivateField<float>(manager, "musicVolume")); // SoundManager stores the invalid value
    }
    [Test]
    public void SetSFXVolumeAcceptsOutOfRangeValueCurrentBug() { // Tests that SFX volume is not clamped by SoundManager
        AudioSource musicSource = CreateAudioSource("MusicSource");
        AudioSource sfxSource = CreateAudioSource("SFXSource");
        SoundManager manager = CreateManager(musicSource, sfxSource);
        manager.SetSFXVolume(-1f); // This value is outside the intended 0 to 1 range
        Assert.AreEqual(-1f, GetPrivateField<float>(manager, "sfxVolume")); // SoundManager stores the invalid value
    }
}
