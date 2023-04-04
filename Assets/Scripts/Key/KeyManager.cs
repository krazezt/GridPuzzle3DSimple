using UnityEngine;

public class KeyManager : MonoBehaviour {
    public GameObject UIController;

    private void Update() {
        if (Input.GetKeyDown(KeyConfig.KEY_TOGGLE_OVERLAY_MENU)) {
            UIController.GetComponent<OverlayMenuController>().ToggleOverlayMenu();
        }
    }
}