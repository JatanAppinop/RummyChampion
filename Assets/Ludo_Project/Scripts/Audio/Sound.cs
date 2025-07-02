using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class Sound
{

    [Header("Audio Source")]
    public string name;
    public AudioClip clip;

    [Header("Volume")]
    public float volume = 0.5f;

    // [HideInInspector]
    // public AudioSource source;

}
