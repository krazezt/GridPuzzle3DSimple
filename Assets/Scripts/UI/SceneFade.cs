using UnityEngine;

public class SceneFade : MonoBehaviour {
    private CanvasGroup canvasGroup;
    private void Awake() {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Update() {
        canvasGroup.alpha = GameManager.instance.sceneFadeAlpha;
    }
}
