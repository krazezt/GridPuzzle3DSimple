using UnityEngine;

[ExecuteInEditMode]
public class PlatformTurning : MonoBehaviour {
    [SerializeField]
    private float turnScale = 1f;

    // Update is called once per frame
    void Update() {
        Vector3 deltaRot = new(0, turnScale * 90, 0);

        transform.Rotate(deltaRot * Time.deltaTime);
    }
}
