using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource SFXsource;
    [SerializeField] private AudioSource crowdSource;

    [Header("BGM Clips")]
    public AudioClip mainMenuMusic;
    public AudioClip gameBGM;
    public AudioClip victoryBGM;

    [Header("SFX Clips")]
    public AudioClip gravityShift;
    public AudioClip jump;
    public AudioClip boxCollision;
    public AudioClip deathSFX;
    public AudioClip gameOverSFX;
    public AudioClip levelCompleteSFX;
    public AudioClip crowdCheeringSFX;

    public static AudioManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        PlayMainMenuMusic(); 
    }

    public void PlayMainMenuMusic()
    {
        musicSource.clip = mainMenuMusic;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlayGameBGM()
    {
        musicSource.clip = gameBGM;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlayWinBGM()
    {
        musicSource.clip = victoryBGM;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
        SFXsource.Stop();
    }

    
    public void PlaySFX(AudioClip clip)
    {
        SFXsource.PlayOneShot(clip);
    }

    public void PlayGameOverSFX()
    {
        PlaySFX(gameOverSFX);
    }

    public void PlayLevelCompleteSFX()
    {
        PlaySFX(levelCompleteSFX);
    }

    public void PlayCrowdCheer() => crowdSource.PlayOneShot(crowdCheeringSFX);

    public void SetMusicVolume(float volume)
    {
        musicSource.volume = volume;
    }

    public void SetSFXVolume(float volume)
    {
        SFXsource.volume = volume;
    }
}
