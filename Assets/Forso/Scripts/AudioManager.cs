using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    #region Singleton & Persistence
    public static AudioManager Instance { get; private set; }

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
    #endregion

    #region Audio Sources
    [Header("Audio Sources")]
    [Tooltip("The source dedicated to looping background music.")]
    public AudioSource musicSource;
    
    [Tooltip("The source dedicated to playing overlapping sound effects.")]
    public AudioSource sfxSource;
    #endregion

    #region Music Logic
    public void PlayMusic(AudioClip clip, float volume = 1f)
    {
        if (clip == null || musicSource == null) return;

        // Prevent the music from restarting awkwardly if the same track is already playing
        if (musicSource.clip == clip && musicSource.isPlaying) return;

        musicSource.clip = clip;
        musicSource.volume = volume;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource != null) musicSource.Stop();
    }
    #endregion

    #region SFX Logic
    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, volume);
    }

    // AAA UPGRADE: Built-in pitch randomization for rapid-fire weapons or repetitive impacts!
    public void PlaySFXRandomPitch(AudioClip clip, float volume = 1f, float minPitch = 0.9f, float maxPitch = 1.1f)
    {
        if (clip == null || sfxSource == null) return;

        // Remember the original pitch
        float originalPitch = sfxSource.pitch;
        
        // Randomize it slightly
        sfxSource.pitch = Random.Range(minPitch, maxPitch);
        sfxSource.PlayOneShot(clip, volume);
        sfxSource.pitch = originalPitch; 
    }
    #endregion
}