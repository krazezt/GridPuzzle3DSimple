using UnityEngine;

[ExecuteInEditMode]
public class MainMenuController : MonoBehaviour {
    public GameObject MainMenuPanel;
    public GameObject SettingMenuPanel;

    private void Awake() {
        ActiveMainMenu();
    }

    public void ActiveMainMenu() {
        HideCanvas(SettingMenuPanel);
        ShowCanvas(MainMenuPanel);
    }

    public void ActiveSettingMenu() {
        HideCanvas(MainMenuPanel);
        ShowCanvas(SettingMenuPanel);
    }

    public void PlayGame() {
        GameManager.instance.NavigateScene(GameConfig.SCENE_NAME_GAMEPLAY);
    }

    private void HideCanvas(GameObject canvasObj) {
        CanvasGroup canvasGroup = canvasObj.GetComponent<CanvasGroup>();

        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    private void ShowCanvas(GameObject canvasObj) {
        CanvasGroup canvasGroup = canvasObj.GetComponent<CanvasGroup>();

        canvasGroup.alpha = 1;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }
}