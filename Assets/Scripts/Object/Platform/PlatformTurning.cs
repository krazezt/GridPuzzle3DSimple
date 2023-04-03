using UnityEngine;

[ExecuteInEditMode]
public class PlatformTurning : MonoBehaviour {
    [SerializeField]
    private float TurnScale = 1f;

    // Update is called once per frame
    void Update() {
        Vector3 deltaRot = new(0, TurnScale * 90, 0);

        transform.Rotate(deltaRot * Time.deltaTime);
    }
}
