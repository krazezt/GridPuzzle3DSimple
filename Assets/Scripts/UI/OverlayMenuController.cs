using UnityEngine;

public class OverlayMenuController : MonoBehaviour {
    public GameObject OverlayMenu;
    public GameObject MainMenuPanel;
    public GameObject SettingMenuPanel;

    public void ToggleOverlayMenu() {
        if (OverlayMenu != null) {
            if (OverlayMenu.GetComponent<CanvasGroup>().alpha == 0) {
                ShowCanvas(OverlayMenu);
                ActiveMainMenu();
                TimeManager.instance.GetComponent<TimeManager>().StopTime();
            } else {
                HideCanvas(OverlayMenu);
                TimeManager.instance.GetComponent<TimeManager>().UnstopTime();
            }
        }
    }

    public void CloseOverlayMenu() {
        if (OverlayMenu != null) {
            HideCanvas(OverlayMenu);
            TimeManager.instance.GetComponent<TimeManager>().UnstopTime();
        }
    }

    public void ActiveMainMenu() {
        HideCanvas(SettingMenuPanel);
        ShowCanvas(MainMenuPanel);
    }

    public void ActiveSettingMenu() {
        HideCanvas(MainMenuPanel);
        ShowCanvas(SettingMenuPanel);
    }

    public void QuitGame() {
        TimeManager.instance.GetComponent<TimeManager>().UnstopTime();
        GameManager.instance.NavigateScene(GameConfig.SCENE_NAME_MAIN_MENU);
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