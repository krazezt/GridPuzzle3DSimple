using UnityEngine;

public class TimeManager : MonoBehaviour {
    private float normalFactor = 1.0f;
    private float stopFactor = 0.0f;
    public float slowdownFactor = 0.05f;
    public float slowdownLength = 2f;
    public float fixedUpdateRate = 50f;

    public Material[] UsingUnscaledTimeMaterials;

    public void StartSlowmotion() {
        Time.timeScale = slowdownFactor;
        Time.fixedDeltaTime = Time.timeScale * (1 / fixedUpdateRate);
    }

    public void StopSlowmotion() {
        Time.timeScale = 1f;
    }

    public void StopTime() {
        Time.timeScale = stopFactor;
    }

    public void UnstopTime() {
        Time.timeScale = normalFactor;
    }

    private void Update() {
        foreach (Material ele in UsingUnscaledTimeMaterials) {
            ele.SetFloat(ShaderConfig.UNSCALED_TIME_REFERENCE, Time.unscaledTime);
        }
    }
}