using System.Collections;
using UnityEngine;

public class RobotController : MonoBehaviour {
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

    private Animator robotAnimator;
    private GameplayConfig.MoveDirection currentMoveDirection;
    private Vector3 currentRotation;

    private float wallCollideDeltaTime = 0f;

    private float platformRotateDeltaTime = 0f;
    private float platformRotateOffset = 0f;
    private float platformYOffset = 0f;
    private float platformYAxisRotateDeltaDegree = 0f;

    private bool onStarting;
    private bool onRotating;
    private bool onRolling;
    private bool onStartRolling;
    private bool onStopRolling;

    public GameObject robotBody;
    public CharacterController controller;
    public ParticleSystem ExplosionVFX;
    public ParticleSystem ElectrifiedVFX;

    [HideInInspector]
    public GameplayConfig.PlayState playState;

    [SerializeField]
    private float MovementSpeed = 0.02f;

    [SerializeField]
    private float MSMultiplyOnRolling = 5f;

    [SerializeField]
    private float MSMultiplyOnStartRolling = 0.5f;

    [SerializeField]
    private float MSMultiplyOnStopRolling = 0.0f;

    private Vector3 currentDir;
    private Vector3 tmpMoveVector;
    private bool isStarCollected;

    private void Awake() {
        robotAnimator = gameObject.GetComponent<Animator>();
        currentRotation = transform.eulerAngles;
        currentDir = DIR_RIGHT;
        currentMoveDirection = GameplayConfig.MoveDirection.RIGHT;
        playState = GameplayConfig.PlayState.PLAYING;
        onRotating = false;
        onRolling = false;
        onStartRolling = false;
        onStopRolling = false;
        isStarCollected = false;

        onStarting = true;
        isPlaying = false;

        gameObject.transform.eulerAngles = currentRotation;
    }

    // Update is called once per frame
    private void Update() {
        //Preprocess
        wallCollideDeltaTime += Time.deltaTime;
        platformRotateDeltaTime += Time.deltaTime;

        // Process
        transform.eulerAngles = currentRotation;
        switch (isPlaying) {
            case true:  // Game is on Playing
                if (!onRotating)
                    MoveForward();

                break;

            case false: // Game is on Starting or Ending
                if (onStarting && !robotAnimator.GetCurrentAnimatorStateInfo(0).IsName(AnimationConfig.ANIMATION_NAME_OPENING)) {
                    robotAnimator.SetBool(AnimationConfig.WALKING_ANIMATION_STATE, true);

                    onStarting = false;
                    isPlaying = true;
                }

                break;
        }
    }

    private void MoveForward() {
        tmpMoveVector = Vector3.zero;

        tmpMoveVector = currentDir * MovementSpeed;

        if (onRolling)
            tmpMoveVector *= MSMultiplyOnRolling;
        if (onStartRolling)
            tmpMoveVector *= MSMultiplyOnStartRolling;
        if (onStopRolling)
            tmpMoveVector *= MSMultiplyOnStopRolling;

        controller.Move(tmpMoveVector * Time.timeScale);
    }

    private void OnTriggerEnter(Collider other) {
        // Debug.Log("Trigger detected: " + other.tag);

        switch (other.gameObject.tag) {
            case GameplayConfig.TAG_GAMECONTROLLER:
                // Debug.Log("GameController: " + collision.gameObject.name);
                switch (other.gameObject.name) {
                    case GameplayConfig.NAME_FIRE_AREA:
                        StartCoroutine(StartDyingFire());
                        break;

                    case GameplayConfig.NAME_STAR:
                        FindObjectOfType<AudioManager>().PlaySFX(AudioConfig.SFX_STAR_COLLECTED);
                        StartCoroutine(other.gameObject.GetComponent<Star>().Collect());
                        isStarCollected = true;
                        break;

                    case GameplayConfig.NAME_GOAL_POINT:
                        if (isStarCollected)
                            StartCoroutine(Win());
                        break;

                    default:
                        break;
                };
                break;

            case GameplayConfig.TAG_CELL:
                CellController cellController;
                if (other.TryGetComponent(out cellController))
                    cellController.LockCellTmp();
                break;

            default:
                break;
        }
    }

    private void OnTriggerExit(Collider other) {
        // Debug.Log("Trigger exited: " + other.tag);

        switch (other.gameObject.tag) {
            case GameplayConfig.TAG_CELL:
                CellController cellController;
                if (other.TryGetComponent(out cellController))
                    cellController.UnlockCellTmp();
                break;

            default:
                break;
        }
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
                // Debug.Log("GameController: " + collision.gameObject.name);
                switch (collision.gameObject.name) {
                    case GameplayConfig.NAME_PLATFORM:
                        platformYOffset = Mathf.Abs(collision.gameObject.transform.position.y - transform.position.y);
                        break;

                    case GameplayConfig.NAME_ACCELERATE_UP:
                        if (!onRolling)
                            StartCoroutine(StartRolling());
                        break;

                    case GameplayConfig.NAME_ACCELERATE_DOWN:
                        if (onRolling)
                            StartCoroutine(StopRolling());
                        break;

                    case GameplayConfig.NAME_ELECTRIC_ARC:
                        StartCoroutine(StartDyingElectric());
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
                        // Debug.Log("onRotating: " + onRotating + ", Position: " +
                        // transform.position + ", Distance: " +
                        // (Vector3.Distance(transform.position,
                        // collision.gameObject.transform.position) - platformYOffset));

                        if (Vector3.Distance(transform.position, collision.gameObject.transform.position) - platformYOffset <= GameplayConfig.PLATFORM_SNAP_DISTANCE &&
                            ((platformRotateDeltaTime >= GameplayConfig.PLATFORM_ROTATE_DELAY && !onRolling) || (platformRotateDeltaTime >= GameplayConfig.PLATFORM_ROTATE_DELAY_ON_ROLLING && onRolling))
                            ) {
                            if (!onRotating) {
                                // Start rotate
                                robotAnimator.SetBool(AnimationConfig.WALKING_ANIMATION_STATE, false);
                                transform.position = collision.gameObject.transform.position + new Vector3(0, platformYOffset, 0);

                                platformRotateOffset = collision.gameObject.transform.eulerAngles.y - transform.eulerAngles.y;
                                onRotating = true;
                            } else {
                                // On rotate
                                platformYAxisRotateDeltaDegree += Mathf.Abs(currentRotation.y - (collision.gameObject.transform.eulerAngles.y - platformRotateOffset));

                                // Euler angles changed from 180 to -180 or vice versa.
                                if (platformYAxisRotateDeltaDegree > 360f - GameplayConfig.PLATFORM_SNAP_ROTATE)
                                    platformYAxisRotateDeltaDegree -= 360f;

                                currentRotation.y = collision.gameObject.transform.eulerAngles.y - platformRotateOffset;

                                // Stop rotate
                                if (platformYAxisRotateDeltaDegree >= GameplayConfig.PLATFORM_ROTATE_MAX) {
                                    // Debug.Log("platformYAxisRotateDeltaDegree = " + platformYAxisRotateDeltaDegree);
                                    platformYAxisRotateDeltaDegree = 0f;
                                    platformRotateDeltaTime = 0f;
                                    SnapRotation();
                                    robotAnimator.SetBool(AnimationConfig.WALKING_ANIMATION_STATE, true);
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
        switch (currentMoveDirection) {
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
        // Standard the currentRotation vector
        while (currentRotation.y >= 180 + GameplayConfig.PLATFORM_SNAP_ROTATE)
            currentRotation.y -= 360;
        while (currentRotation.y <= -90 - GameplayConfig.PLATFORM_SNAP_ROTATE)
            currentRotation.y += 360;

        if (Vector3.Distance(currentRotation, ROT_TOP) <= GameplayConfig.PLATFORM_SNAP_ROTATE)
            Rotate(GameplayConfig.MoveDirection.TOP);
        else if (Vector3.Distance(currentRotation, ROT_BOTTOM) <= GameplayConfig.PLATFORM_SNAP_ROTATE)
            Rotate(GameplayConfig.MoveDirection.BOTTOM);
        else if (Vector3.Distance(currentRotation, ROT_RIGHT) <= GameplayConfig.PLATFORM_SNAP_ROTATE)
            Rotate(GameplayConfig.MoveDirection.RIGHT);
        else if (Vector3.Distance(currentRotation, ROT_LEFT) <= GameplayConfig.PLATFORM_SNAP_ROTATE)
            Rotate(GameplayConfig.MoveDirection.LEFT);
    }

    private void Rotate(GameplayConfig.MoveDirection newDirection) {
        switch (newDirection) {
            case GameplayConfig.MoveDirection.TOP:
                currentMoveDirection = GameplayConfig.MoveDirection.TOP;
                currentDir = DIR_TOP;
                currentRotation = ROT_TOP;
                break;

            case GameplayConfig.MoveDirection.BOTTOM:
                currentMoveDirection = GameplayConfig.MoveDirection.BOTTOM;
                currentDir = DIR_BOTTOM;
                currentRotation = ROT_BOTTOM;
                break;

            case GameplayConfig.MoveDirection.RIGHT:
                currentMoveDirection = GameplayConfig.MoveDirection.RIGHT;
                currentDir = DIR_RIGHT;
                currentRotation = ROT_RIGHT;
                break;

            case GameplayConfig.MoveDirection.LEFT:
                currentMoveDirection = GameplayConfig.MoveDirection.LEFT;
                currentDir = DIR_LEFT;
                currentRotation = ROT_LEFT;
                break;
        }
    }

    private void ResetWallCollideDeltatime() {
        wallCollideDeltaTime = 0f;
    }

    private IEnumerator StartRolling() {
        FindObjectOfType<AudioManager>().PlaySFX(AudioConfig.SFX_ACCELERATE_UP);
        robotAnimator.SetBool(AnimationConfig.ROLLING_ANIMATION_STATE, true);
        onStartRolling = true;

        yield return new WaitForSeconds(AnimationConfig.ANIMATION_DELAY_START_ROLL);

        onStartRolling = false;
        onRolling = true;
    }

    private IEnumerator StopRolling() {
        FindObjectOfType<AudioManager>().PlaySFX(AudioConfig.SFX_ACCELERATE_DOWN);
        robotAnimator.SetBool(AnimationConfig.ROLLING_ANIMATION_STATE, false);
        onRolling = false;

        yield return new WaitForSeconds(AnimationConfig.ANIMATION_DELAY_STOP_ROLL);

        onStopRolling = true;

        yield return new WaitForSeconds(AnimationConfig.ANIMATION_DELAY_START_WALK_AFTER_ROLL);

        onStopRolling = false;
    }

    private IEnumerator StartDyingFire() {
        if (!onRolling) {
            robotAnimator.SetBool(AnimationConfig.OPENING_ANIMATION_STATE, false);
            yield return new WaitForSeconds(AnimationConfig.ANIMATION_DELAY_CLOSE);
        } else
            yield return new WaitForSeconds(AnimationConfig.ANIMATION_DELAY_EXPLODE_FIRE);

        isPlaying = false;
        robotBody.SetActive(false);
        FindObjectOfType<AudioManager>().PlaySFX(AudioConfig.SFX_EXPLOSION);
        ExplosionVFX.Play();
        if (Camera.main.TryGetComponent<CameraShake>(out var cameraShake))
            cameraShake.StartShake();

        StartCoroutine(Lose());
    }

    private IEnumerator StartDyingElectric() {
        isPlaying = false;
        robotAnimator.speed = 0f;

        yield return new WaitForSeconds(AnimationConfig.ANIMATION_DELAY_ELECTRIFIED);
        FindObjectOfType<AudioManager>().PlaySFX(AudioConfig.SFX_ELECTRIFIED);
        ElectrifiedVFX.Play();

        yield return new WaitForSeconds(AnimationConfig.ANIMATION_DELAY_EXPLODE_ELECCTRIC);
        ElectrifiedVFX.Stop();

        robotBody.SetActive(false);
        FindObjectOfType<AudioManager>().PlaySFX(AudioConfig.SFX_EXPLOSION);
        ExplosionVFX.Play();
        if (Camera.main.TryGetComponent<CameraShake>(out var cameraShake))
            cameraShake.StartShake();

        StartCoroutine(Lose());
    }

    private IEnumerator Win() {
        isPlaying = false;
        if (onRolling) {
            robotAnimator.SetBool(AnimationConfig.ROLLING_ANIMATION_STATE, false);
            yield return new WaitForSeconds(AnimationConfig.ANIMATION_DELAY_STOP_ROLL);
        }
        robotAnimator.SetBool(AnimationConfig.WINNING_ANIMATION_STATE, true);

        yield return new WaitForSeconds(AnimationConfig.ANIMATION_DELAY_WINNING);
        playState = GameplayConfig.PlayState.WINNING;
        FindObjectOfType<AudioManager>().PlaySFX(AudioConfig.SFX_WIN);
    }

    private IEnumerator Lose() {
        isPlaying = false;

        yield return new WaitForSeconds(AnimationConfig.ANIMATION_DELAY_WINNING);
        playState = GameplayConfig.PlayState.LOSING;
        FindObjectOfType<AudioManager>().PlaySFX(AudioConfig.SFX_LOSE);
    }
}