using UnityEngine;

public class RobotAnimController : MonoBehaviour {
    private Vector3 rot = Vector3.zero;
    private float rotSpeed = 40f;
    private Animator anim;

    // Use this for initialization
    private void Awake() {
        anim = gameObject.GetComponent<Animator>();
        gameObject.transform.eulerAngles = rot;
    }

    // Update is called once per frame
    private void Update() {
        CheckKey();
        gameObject.transform.eulerAngles = rot;
    }

    private void CheckKey() {
        // Walk
        if (Input.GetKey(KeyCode.W)) {
            anim.SetBool("Walk_Anim", true);
        } else if (Input.GetKeyUp(KeyCode.W)) {
            anim.SetBool("Walk_Anim", false);
        }

        // Rotate Left
        if (Input.GetKey(KeyCode.A)) {
            rot[1] -= rotSpeed * Time.fixedDeltaTime;
        }

        // Rotate Right
        if (Input.GetKey(KeyCode.D)) {
            rot[1] += rotSpeed * Time.fixedDeltaTime;
        }

        // Roll
        if (Input.GetKeyDown(KeyCode.Space)) {
            if (anim.GetBool("Roll_Anim")) {
                anim.SetBool("Roll_Anim", false);
            } else {
                anim.SetBool("Roll_Anim", true);
            }
        }

        // Close
        if (Input.GetKeyDown(KeyCode.LeftControl)) {
            if (!anim.GetBool("Open_Anim")) {
                anim.SetBool("Open_Anim", true);
            } else {
                anim.SetBool("Open_Anim", false);
            }
        }
    }
}