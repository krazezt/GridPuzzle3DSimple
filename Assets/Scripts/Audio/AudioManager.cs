using UnityEngine;

public class AudioManager : MonoBehaviour {

    [HideInInspector]
    public static AudioManager instance;

    [Range(0f, 1f)]
    public float BGMVolume;

    [Range(0f, 1f)]
    public float SFXVolume;

    public Sound[] BGMs;
    public Sound[] SFXs;

    private void Awake() {
        if (instance == null)
            instance = this;
        else {
            Destroy(this);
            return;
        }

        DontDestroyOnLoad(gameObject);

        foreach (var bgm in BGMs) {
            bgm.audioSource = gameObject.AddComponent<AudioSource>();
            bgm.audioSource.clip = bgm.clip;
            bgm.audioSource.volume = BGMVolume;
            bgm.audioSource.pitch = bgm.pitch;
            bgm.audioSource.loop = bgm.loop;
        }

        foreach (var sfx in SFXs) {
            sfx.audioSource = gameObject.AddComponent<AudioSource>();
            sfx.audioSource.clip = sfx.clip;
            sfx.audioSource.volume = SFXVolume;
            sfx.audioSource.pitch = sfx.pitch;
            sfx.audioSource.loop = sfx.loop;
        }
    }

    public void SetBGMVolume(float value) {
        BGMVolume = value;
        UpdateBGMVolume();
    }

    public void SetSFXVolume(float value) {
        BGMVolume = value;
        UpdateSFXVolume();
    }

    private void UpdateBGMVolume() {
        foreach (var bgm in BGMs) {
            bgm.audioSource.volume = BGMVolume;
        }
    }

    private void UpdateSFXVolume() {
        foreach (var sfx in SFXs) {
            sfx.audioSource.volume = SFXVolume;
        }
    }
}