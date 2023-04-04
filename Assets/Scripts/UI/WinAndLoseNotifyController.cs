using UnityEngine;
using UnityEngine.UI;

public class WinAndLoseNotifyController : MonoBehaviour {
    public GameObject player;
    public GameObject WinAndLoseNotify;
    public Image BackDropPanel;
    public GameObject WinPanel;
    public GameObject LosePanel;

    private RobotController robotController;
    private bool isGameEnded;

    private void Awake() {
        robotController = player.GetComponent<RobotController>();
        isGameEnded = false;
    }

    private void Update() {
        if ((robotController.playState == GameplayConfig.PlayState.PLAYING) || (isGameEnded))
            return;

        switch (robotController.playState) {
            case GameplayConfig.PlayState.WINNING:
                ShowWinMenu();
                break;

            case GameplayConfig.PlayState.LOSING:
                ShowLoseMenu();
                break;

            default:
                break;
        }
    }

    public void ShowWinMenu() {
        BackDropPanel.raycastTarget = true;
        ShowCanvas(WinAndLoseNotify);

        HideCanvas(LosePanel);
        ShowCanvas(WinPanel);

        isGameEnded = true;
    }

    public void ShowLoseMenu() {
        BackDropPanel.raycastTarget = true;
        ShowCanvas(WinAndLoseNotify);

        HideCanvas(WinPanel);
        ShowCanvas(LosePanel);

        isGameEnded = true;
    }

    public void RestartGame() {
        GameManager.instance.ReloadScene();
    }

    public void QuitGame() {
        TimeManager.instance.GetComponent<TimeManager>().UnstopTime();
        GameManager.instance.NavigateScene(GameConfig.SCENE_NAME_MAIN_MENU);
    }

    public void PlayButtonClickSFX() {
        GameObject.FindObjectOfType<AudioManager>().PlaySFX_ButtonClick();
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