using UnityEngine;
using UnityEngine.UI;

public class LoadProgressBar : MonoBehaviour {
    private Slider slider;

    private void Awake() {
        slider = GetComponent<Slider>();
    }

    private void Update() {
        if (slider != null) {
            slider.value = GameManager.instance.loadProgress;
        }
    }
}
