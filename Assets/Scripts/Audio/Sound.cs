using UnityEngine;

[System.Serializable]
public class Sound {
    public string name;

    public AudioClip clip;

    [HideInInspector]
    public float volume;

    [Range(0.1f, 3f)]
    public float pitch = 1;

    public bool loop;

    [HideInInspector]
    public AudioSource audioSource;
}