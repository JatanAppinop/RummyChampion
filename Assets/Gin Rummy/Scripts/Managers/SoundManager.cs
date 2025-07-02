using UnityEngine;
using System.Collections;
using System;

public class SoundManager : MonoBehaviour {
    public static SoundManager instance;

    private AudioSource audioSource;
    private AudioSource musicSource;

    public AudioClip reverseCardSound;
    public AudioClip dropCardSound;
    public AudioClip pickCardSound;
    public AudioClip clickSound;
    public AudioClip obtainMedalSound;
    
    private bool locked = false;
    private bool isMuted;

    void Awake()
    {
        instance = this;
        audioSource = GetComponent<AudioSource>();
        musicSource = transform.Find("MusicManager").GetComponent<AudioSource>();
    }

    private void Start()
    {
        if (Application.isEditor)
            SwitchMusicState(false);
    }

    public void PlayClickSound()
    {
        PlaySound(clickSound, true);
    }

    public void PlayReverseCardSound()
    {
        PlaySound(reverseCardSound);
    }

    public void PlayPickCardSound()
    {
        PlaySound(pickCardSound);
    }

    public void PlayDropCardSound()
    {
        PlaySound(dropCardSound);
    }

    public void PlayGetMedalSound()
    {
        PlaySound(obtainMedalSound);
    }

    private void PlaySound(AudioClip audioClip, bool isPrioritySound = false)
    {
        if ((!locked || isPrioritySound)&& !isMuted)
        {
            locked = true;
            audioSource.PlayOneShot(audioClip);
            Invoke("Unlock", Constants.SOUND_LOCK_TIME);
        }
    }

    void Unlock()
    {
        locked = false;
    }

    public void SwitchSoundState(bool value)
    {
        isMuted = !value;
    }

    public void SwitchMusicState(bool value)
    {
        musicSource.mute = !value;
    }
}