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
        StartCoroutine(LoadSceneAsynchronously(name));
    }

    public void BackScene() {
        sceneStack.Pop();
        StartCoroutine(LoadSceneAsynchronously(sceneStack.Peek()));
    }

    private IEnumerator LoadSceneAsynchronously(string name) {
        loadProgress = 0f;
        sceneFadeAlpha = 0f;
        float deltaTime = 0f;

        while (deltaTime <= GameConfig.SCENE_FADE_DURATION) {
            sceneFadeAlpha = Mathf.Clamp01(deltaTime / GameConfig.SCENE_FADE_DURATION);

            deltaTime += Time.unscaledDeltaTime;
            yield return null;
        }

        SceneManager.LoadScene(GameConfig.SCENE_NAME_LOADING);
        AsyncOperation operation = SceneManager.LoadSceneAsync(name);

        while (!operation.isDone) {
            loadProgress = Mathf.Clamp01(operation.progress / 0.9f);
            Debug.Log(loadProgress);
            yield return null;
        }
        loadProgress = 0f;

        deltaTime = 0f;
        while (deltaTime <= GameConfig.SCENE_FADE_DURATION) {
            sceneFadeAlpha = 1 - Mathf.Clamp01(deltaTime / GameConfig.SCENE_FADE_DURATION);

            if (Time.unscaledDeltaTime <= GameConfig.SCENE_LOAD_LAG_DURATION_MAX)
                deltaTime += Time.unscaledDeltaTime;
            yield return null;
        }

        yield return null;
    }
}