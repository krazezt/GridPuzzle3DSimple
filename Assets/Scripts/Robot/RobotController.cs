using UnityEngine;

public class RobotController : MonoBehaviour {
    private Animator RobotAnimator;

    // Consts
    private readonly Vector3 DIR_TOP = new(0, 0, 1);

    private readonly Vector3 DIR_BOTTOM = new(0, 0, -1);
    private readonly Vector3 DIR_RIGHT = new(1, 0, 0);
    private readonly Vector3 DIR_LEFT = new(-1, 0, 0);

    private readonly Vector3 ROT_TOP = new(0, 0, 0);
    private readonly Vector3 ROT_BOTTOM = new(0, 180, 0);
    private readonly Vector3 ROT_RIGHT = new(0, 90, 0);
    private readonly Vector3 ROT_LEFT = new(0, -90, 0);

    // States
    private bool isPlaying = false;

    private GameplayConfig.MoveDirection CurrentMoveDirection;
    private Vector3 CurrentRotation;

    private float wallCollideDeltaTime = 0f;

    private float platformRotateDeltaTime = 0f;
    private float platformRotateOffset = 0f;
    private float platformYOffset = 0f;
    private float platformYAxisRotateDeltaDegree = 0f;
    private Vector3 platformPosisionSnap;
    private bool onRotating;

    // Variables
    public CharacterController controller;

    [SerializeField]
    private float RotateSpeed = 40f;

    [SerializeField]
    private float MovementSpeed = 0.02f;

    private Vector3 CurrentDir;

    private void Awake() {
        RobotAnimator = gameObject.GetComponent<Animator>();
        CurrentRotation = transform.eulerAngles;
        CurrentDir = DIR_RIGHT;
        CurrentMoveDirection = GameplayConfig.MoveDirection.RIGHT;
        onRotating = false;

        gameObject.transform.eulerAngles = CurrentRotation;
    }

    // Update is called once per frame
    private void Update() {
        // Preprocess
        CheckKey();
        wallCollideDeltaTime += Time.deltaTime;
        platformRotateDeltaTime += Time.deltaTime;

        // Process
        transform.eulerAngles = CurrentRotation;

        switch (isPlaying) {
            case true:  // Game is on Playing
                if (!onRotating)
                    MoveForward();
                break;

            case false: // Game is on Starting or Ending
                if (!RobotAnimator.GetCurrentAnimatorStateInfo(0).IsName(AnimationConfig.ANIMATION_NAME_OPENING)) {
                    RobotAnimator.SetBool(AnimationConfig.WALKING_ANIMATION_STATE, true);

                    isPlaying = true;
                }
                break;
        }
    }

    private void MoveForward() {
        controller.Move(CurrentDir * MovementSpeed);
    }

    private void OnTriggerEnter(Collider other) {
        // Debug.Log("Trigger detected: " + other.tag);
    }

    private void OnCollisionEnter(Collision collision) {
        Collider selfCollider = collision.GetContact(0).thisCollider;
        // Debug.Log(collision.gameObject.name + " - " + selfCollider.gameObject.name);

        switch (collision.gameObject.tag) {
            case GameplayConfig.TAG_WALL:
                if (selfCollider.gameObject.name == CollisionConfig.HITBOX_NAME_FRONT && wallCollideDeltaTime >= GameplayConfig.WALL_COLLIDE_DELAY) {
                    TurnBack();
                    ResetWallCollideDeltatime();
                }
                break;

            case GameplayConfig.TAG_GAMECONTROLLER:
                switch (collision.gameObject.name) {
                    case GameplayConfig.NAME_PLATFORM:
                        platformYOffset = Mathf.Abs(collision.gameObject.transform.position.y - transform.position.y);
                        break;

                    default:
                        break;
                };
                break;

            default:
                break;
        }
    }

    private void OnCollisionStay(Collision collision) {
        switch (collision.gameObject.tag) {
            case GameplayConfig.TAG_GAMECONTROLLER:
                switch (collision.gameObject.name) {
                    case GameplayConfig.NAME_PLATFORM:
                        if (Vector3.Distance(transform.position, collision.gameObject.transform.position) - platformYOffset <= GameplayConfig.PLATFORM_SNAP_DISTANCE &&
                            platformRotateDeltaTime >= GameplayConfig.PLATFORM_ROTATE_DELAY
                            ) {
                            // Calculate offset
                            if (!onRotating) {
                                // Start rotate
                                RobotAnimator.SetBool(AnimationConfig.WALKING_ANIMATION_STATE, false);
                                transform.position = collision.gameObject.transform.position + new Vector3(0, platformYOffset, 0);

                                platformYAxisRotateDeltaDegree = 0f;
                                platformRotateOffset = collision.gameObject.transform.eulerAngles.y - transform.eulerAngles.y;
                                onRotating = true;
                            } else {
                                // On rotate
                                platformYAxisRotateDeltaDegree += Mathf.Abs(CurrentRotation.y - (collision.gameObject.transform.eulerAngles.y - platformRotateOffset));

                                // Euler angles changed from 180 to -180 or vice versa.
                                if (platformYAxisRotateDeltaDegree > 360f)
                                    platformYAxisRotateDeltaDegree -= 360f;

                                CurrentRotation.y = collision.gameObject.transform.eulerAngles.y - platformRotateOffset;

                                // Stop rotate
                                if (platformYAxisRotateDeltaDegree >= GameplayConfig.PLATFORM_ROTATE_MAX) {
                                    platformYAxisRotateDeltaDegree = 0f;
                                    platformRotateDeltaTime = 0f;
                                    SnapRotation();
                                    RobotAnimator.SetBool(AnimationConfig.WALKING_ANIMATION_STATE, true);
                                    onRotating = false;
                                }
                            }
                        }
                        break;

                    default:
                        break;
                };
                break;

            default:
                break;
        }
    }

    private void TurnBack() {
        switch (CurrentMoveDirection) {
            case GameplayConfig.MoveDirection.TOP:
                Rotate(GameplayConfig.MoveDirection.BOTTOM);
                break;

            case GameplayConfig.MoveDirection.BOTTOM:
                Rotate(GameplayConfig.MoveDirection.TOP);
                break;

            case GameplayConfig.MoveDirection.RIGHT:
                Rotate(GameplayConfig.MoveDirection.LEFT);
                break;

            case GameplayConfig.MoveDirection.LEFT:
                Rotate(GameplayConfig.MoveDirection.RIGHT);
                break;
        }
    }

    private void SnapRotation() {
        // Standard the CurrentRotation vector
        while (CurrentRotation.y >= 180 + GameplayConfig.PLATFORM_SNAP_ROTATE)
            CurrentRotation.y -= 360;
        while (CurrentRotation.y <= -90 - GameplayConfig.PLATFORM_SNAP_ROTATE)
            CurrentRotation.y += 360;

        if (Vector3.Distance(CurrentRotation, ROT_TOP) <= GameplayConfig.PLATFORM_SNAP_ROTATE)
            Rotate(GameplayConfig.MoveDirection.TOP);
        else if (Vector3.Distance(CurrentRotation, ROT_BOTTOM) <= GameplayConfig.PLATFORM_SNAP_ROTATE)
            Rotate(GameplayConfig.MoveDirection.BOTTOM);
        else if (Vector3.Distance(CurrentRotation, ROT_RIGHT) <= GameplayConfig.PLATFORM_SNAP_ROTATE)
            Rotate(GameplayConfig.MoveDirection.RIGHT);
        else if (Vector3.Distance(CurrentRotation, ROT_LEFT) <= GameplayConfig.PLATFORM_SNAP_ROTATE)
            Rotate(GameplayConfig.MoveDirection.LEFT);
    }

    private void Rotate(GameplayConfig.MoveDirection newDirection) {
        switch (newDirection) {
            case GameplayConfig.MoveDirection.TOP:
                CurrentMoveDirection = GameplayConfig.MoveDirection.TOP;
                CurrentDir = DIR_TOP;
                CurrentRotation = ROT_TOP;
                break;

            case GameplayConfig.MoveDirection.BOTTOM:
                CurrentMoveDirection = GameplayConfig.MoveDirection.BOTTOM;
                CurrentDir = DIR_BOTTOM;
                CurrentRotation = ROT_BOTTOM;
                break;

            case GameplayConfig.MoveDirection.RIGHT:
                CurrentMoveDirection = GameplayConfig.MoveDirection.RIGHT;
                CurrentDir = DIR_RIGHT;
                CurrentRotation = ROT_RIGHT;
                break;

            case GameplayConfig.MoveDirection.LEFT:
                CurrentMoveDirection = GameplayConfig.MoveDirection.LEFT;
                CurrentDir = DIR_LEFT;
                CurrentRotation = ROT_LEFT;
                break;
        }
    }

    private void ResetWallCollideDeltatime() {
        wallCollideDeltaTime = 0f;
    }

    private void CheckKey() {
        // Walk
        if (Input.GetKey(KeyCode.W)) {
            RobotAnimator.SetBool(AnimationConfig.WALKING_ANIMATION_STATE, true);
        } else if (Input.GetKeyUp(KeyCode.W)) {
            RobotAnimator.SetBool(AnimationConfig.WALKING_ANIMATION_STATE, false);
        }

        // Rotate Left
        if (Input.GetKey(KeyCode.A)) {
            CurrentRotation[1] -= RotateSpeed * Time.fixedDeltaTime;
        }

        // Rotate Right
        if (Input.GetKey(KeyCode.D)) {
            CurrentRotation[1] += RotateSpeed * Time.fixedDeltaTime;
        }

        // Roll
        if (Input.GetKeyDown(KeyCode.Space)) {
            if (RobotAnimator.GetBool(AnimationConfig.ROLLING_ANIMATION_STATE)) {
                RobotAnimator.SetBool(AnimationConfig.ROLLING_ANIMATION_STATE, false);
            } else {
                RobotAnimator.SetBool(AnimationConfig.ROLLING_ANIMATION_STATE, true);
            }
        }

        // Close
        if (Input.GetKeyDown(KeyCode.LeftControl)) {
            if (!RobotAnimator.GetBool(AnimationConfig.OPENING_ANIMATION_STATE)) {
                RobotAnimator.SetBool(AnimationConfig.OPENING_ANIMATION_STATE, true);
            } else {
                RobotAnimator.SetBool(AnimationConfig.OPENING_ANIMATION_STATE, false);
            }
        }
    }
}