using UnityEngine;
using UnityEngine.UI;

public class OverlayMenuController : MonoBehaviour {
    public GameObject OverlayMenu;
    public Image BackDropPanel;
    public GameObject MainMenuPanel;
    public GameObject SettingMenuPanel;

    [Header("Sound setting control")]
    public Slider BGMSlider;

    public Slider SFXSlider;

    public void ToggleOverlayMenu() {
        if (OverlayMenu != null) {
            if (OverlayMenu.GetComponent<CanvasGroup>().alpha == 0)
                ShowOverlayMenu();
            else
                CloseOverlayMenu();
        }
    }

    public void ShowOverlayMenu() {
        if (OverlayMenu != null) {
            BackDropPanel.raycastTarget = true;
            ShowCanvas(OverlayMenu);
            ActiveMainMenu();
            TimeManager.instance.GetComponent<TimeManager>().StopTime();
        }
    }

    public void CloseOverlayMenu() {
        if (OverlayMenu != null) {
            BackDropPanel.raycastTarget = false;
            HideCanvas(OverlayMenu);
            TimeManager.instance.GetComponent<TimeManager>().UnstopTime();
        }
    }

    public void ActiveMainMenu() {
        HideCanvas(SettingMenuPanel);
        ShowCanvas(MainMenuPanel);
    }

    public void ActiveSettingMenu() {
        BGMSlider.value = FindObjectOfType<AudioManager>().BGMVolume;
        SFXSlider.value = FindObjectOfType<AudioManager>().SFXVolume;

        HideCanvas(MainMenuPanel);
        ShowCanvas(SettingMenuPanel);
    }

    public void QuitGame() {
        TimeManager.instance.GetComponent<TimeManager>().UnstopTime();
        GameManager.instance.NavigateScene(GameConfig.SCENE_NAME_MAIN_MENU);
    }

    public void PlayButtonClickSFX() {
        FindObjectOfType<AudioManager>().PlaySFX_ButtonClick();
    }

    public void ChangeBGMVolume(Slider slider) {
        FindObjectOfType<AudioManager>().SetBGMVolume(slider.value);
    }

    public void ChangeSFXVolume(Slider slider) {
        FindObjectOfType<AudioManager>().SetSFXVolume(slider.value);
        FindObjectOfType<AudioManager>().PlaySFX_ButtonClick();
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