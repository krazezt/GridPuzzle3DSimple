using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
    public static GameManager instance;

    [HideInInspector]
    public float loadProgress;

    [HideInInspector]
    public float sceneFadeAlpha;

    private readonly Stack<string> sceneStack = new();

    private void Awake() {
        if (instance == null)
            instance = this;
        else {
            Destroy(this);
            return;
        }

        DontDestroyOnLoad(gameObject);
        // First scene when the game open up.
        sceneStack.Push(SceneManager.GetActiveScene().name);
    }

    public void NavigateScene(string name) {
        sceneStack.Push(name);
        StartCoroutine(LoadSceneAsynchronously(name, LoadSceneMode.Single));
    }

    public void ReloadScene() {
        StartCoroutine(LoadSceneAsynchronously(SceneManager.GetActiveScene().name, LoadSceneMode.Single));
    }

    public void BackScene() {
        sceneStack.Pop();
        StartCoroutine(LoadSceneAsynchronously(sceneStack.Peek(), LoadSceneMode.Single));
    }

    private IEnumerator LoadSceneAsynchronously(string name, LoadSceneMode mode) {
        loadProgress = 0f;
        sceneFadeAlpha = 0f;
        float originalBGMVolume = FindObjectOfType<AudioManager>().BGMVolume;
        float deltaTime = 0f;

        while (deltaTime <= GameConfig.SCENE_FADE_DURATION) {
            sceneFadeAlpha = Mathf.Clamp01(deltaTime / GameConfig.SCENE_FADE_DURATION);
            FindObjectOfType<AudioManager>().SetBGMVolume(originalBGMVolume * (1 - sceneFadeAlpha));

            deltaTime += Time.unscaledDeltaTime;
            yield return null;
        }

        AsyncOperation loadingSceneOperation = SceneManager.LoadSceneAsync(GameConfig.SCENE_NAME_LOADING);
        while (!loadingSceneOperation.isDone)
            yield return null;

        AsyncOperation nextSceneOperation = SceneManager.LoadSceneAsync(name, mode);
        while (!nextSceneOperation.isDone) {
            loadProgress = Mathf.Clamp01(nextSceneOperation.progress / 0.9f);
            yield return null;
        }
        loadProgress = 0f;

        deltaTime = 0f;
        while (deltaTime <= GameConfig.SCENE_FADE_DURATION) {
            sceneFadeAlpha = 1 - Mathf.Clamp01(deltaTime / GameConfig.SCENE_FADE_DURATION);
            FindObjectOfType<AudioManager>().SetBGMVolume(originalBGMVolume * (1 - sceneFadeAlpha));

            if (Time.unscaledDeltaTime <= GameConfig.SCENE_LOAD_LAG_DURATION_MAX)
                deltaTime += Time.unscaledDeltaTime;
            yield return null;
        }

        yield return null;
    }
}