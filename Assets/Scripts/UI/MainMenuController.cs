using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class MainMenuController : MonoBehaviour {
    public GameObject MainMenuPanel;
    public GameObject SettingMenuPanel;

    [Header("Sound settings control")]
    public Slider BGMSlider;

    public Slider SFXSlider;

    private void Awake() {
        ActiveMainMenu();
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

    public void PlayGame() {
        FindObjectOfType<AudioManager>().PlaySFX(AudioConfig.SFX_PLAY_BUTTON_CLICK);
        GameManager.instance.NavigateScene(GameConfig.SCENE_NAME_GAMEPLAY);
    }

    public void ExitGame() {
        Application.Quit();
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