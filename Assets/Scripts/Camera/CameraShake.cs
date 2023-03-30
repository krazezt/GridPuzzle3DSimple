using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour {
    public float totalDuration = 0.5f;

    [Space]
    public Vector3 shakeStrength = new(1f, 1f, 1f);

    public AnimationCurve shakeCurve = AnimationCurve.Linear(0, 1, 1, 0);

    private Vector3 originalPos = Vector3.zero;
    private float elapsed, delta;
    private Vector3 randomVec, scaledRandomVec, shakeVec;

    public void StartShake() {
        StartCoroutine(ShakeCoroutine());
    }

    private IEnumerator ShakeCoroutine() {
        originalPos = transform.localPosition;

        while (elapsed < totalDuration) {
            Shake();
            yield return null;
        }

        transform.localPosition = originalPos;
    }

    private void Shake() {
        delta = Mathf.Clamp01(elapsed / totalDuration);

        randomVec = new Vector3(Random.value, Random.value, Random.value);
        scaledRandomVec = Vector3.Scale(randomVec, shakeStrength) * (Random.value > 0.5f ? -1 : 1);
        shakeVec = scaledRandomVec * shakeCurve.Evaluate(delta);

        transform.localPosition = originalPos + shakeVec;

        elapsed += Time.deltaTime;
    }
}