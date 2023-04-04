using System.Collections;
using UnityEngine;

[ExecuteInEditMode]
public class Star : MonoBehaviour {
    public float rotateSpeed = 60f;
    public float sineWaveMultiply = 0.1f;
    public float sineWaveSpeed = 4f;

    public Vector3 centerPos = Vector3.zero;

    public float collectAnimDuration = 2f;

    private void Update() {
        transform.Rotate(new(0, rotateSpeed * Time.deltaTime, 0));
        transform.localPosition = new(centerPos.x, centerPos.y + Mathf.Sin(Time.time * sineWaveSpeed) * sineWaveMultiply, centerPos.z);
    }

    public IEnumerator Collect() {
        float deltaTime = 0f;
        Vector3 unit3 = new(1, 1, 1);

        while (deltaTime <= collectAnimDuration) {
            transform.localScale = unit3 * (1 - Mathf.Clamp01(deltaTime / collectAnimDuration));
            transform.localPosition += new Vector3(0, Mathf.Clamp01(deltaTime / collectAnimDuration) * sineWaveMultiply * 10, 0);

            deltaTime += Time.deltaTime;

            yield return null;
        }

        gameObject.SetActive(false);
    }
}