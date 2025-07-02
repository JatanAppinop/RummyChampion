using System;
using System.Collections;
using System.Collections.Generic;
using SecPlayerPrefs;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : SingletonWithoutGameobject<AudioManager>
{
    [Header("Audio Sources")]
    [SerializeField] Sound[] BGMSounds;
    [SerializeField] Sound[] SoundEffects;

    [Header("Audio Sources Instances")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    private bool isMusicMuted = false;
    private bool isSfxMuted = false;



    public void Initialize()
    {
        DontDestroyOnLoad(this);
        isMusicMuted =PlayerPrefs.GetInt("MusicMuted", 0) == 1;
        isSfxMuted = PlayerPrefs.GetInt("SFXMuted", 0) == 1;
        musicSource.mute = isMusicMuted;
        sfxSource.mute = isSfxMuted;

    }

    public void PlayMusic(string name)
    {

        Sound sound = System.Array.Find(BGMSounds, s => s.name == name);
        if (sound == null)
        {
            Debug.LogWarning("Music sound not found: " + name);
            return;
        }

        musicSource.clip = sound.clip;
        musicSource.volume = sound.volume;
        musicSource.loop = true;
        musicSource.Play();

    }

    public void PlayEffect(string name)
    {

        Sound sound = System.Array.Find(SoundEffects, s => s.name == name);
        if (sound == null)
        {
            Debug.LogWarning("SFX sound not found: " + name);
            return;
        }

        sfxSource.PlayOneShot(sound.clip, sound.volume);

    }

    public void PlayEffect(string name, int loops)
    {

        Sound sound = System.Array.Find(SoundEffects, s => s.name == name);
        if (sound == null)
        {
            Debug.LogWarning("SFX sound not found: " + name);
            return;
        }

        sfxSource.clip = sound.clip;
        sfxSource.volume = sound.volume;
        if (loops > 0)
        {
            sfxSource.loop = true;
            StartCoroutine(stopEffect(sfxSource, loops));
        }
        sfxSource.Play();

    }

    IEnumerator stopEffect(AudioSource source, int loops)
    {
        yield return new WaitForSeconds(source.clip.length * loops);
        source.Stop();

    }

    public void ToggleMusic()
    {
        isMusicMuted = !isMusicMuted;
        musicSource.mute = isMusicMuted;
        PlayerPrefs.SetInt("MusicMuted", isMusicMuted ? 1 : 0);  // Save as 1 (muted) or 0 (unmuted)
        PlayerPrefs.Save();
    }

    public void ToggleSFX()
    {
        isSfxMuted = !isSfxMuted;
        sfxSource.mute = isSfxMuted;
        PlayerPrefs.SetInt("SFXMuted", isSfxMuted ? 1 : 0);  // Save as 1 (muted) or 0 (unmuted)
        PlayerPrefs.Save();
    }

    public bool IsMusicMuted()
    {
        return isMusicMuted;
    }

    public bool IsSFXMuted()
    {
        return isSfxMuted;
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

}
