using System.Collections;
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

    private bool onStarting;
    private bool onRotating;
    private bool onRolling;
    private bool onStartRolling;
    private bool onStopRolling;

    // Variables
    public GameObject RobotBody;

    public CharacterController controller;

    [HideInInspector]
    public GameplayConfig.PlayState playState;

    public ParticleSystem ExplosionVFX;
    public ParticleSystem ElectrifiedVFX;

    [SerializeField]
    private float MovementSpeed = 0.02f;

    [SerializeField]
    private float MSMultiplyOnRolling = 5f;

    [SerializeField]
    private float MSMultiplyOnStartRolling = 0.5f;

    [SerializeField]
    private float MSMultiplyOnStopRolling = 0.0f;

    private Vector3 CurrentDir;
    private Vector3 tmpMoveVector;
    private bool isStarCollected;

    private void Awake() {
        RobotAnimator = gameObject.GetComponent<Animator>();
        CurrentRotation = transform.eulerAngles;
        CurrentDir = DIR_RIGHT;
        CurrentMoveDirection = GameplayConfig.MoveDirection.RIGHT;
        playState = GameplayConfig.PlayState.PLAYING;
        onRotating = false;
        onRolling = false;
        onStartRolling = false;
        onStopRolling = false;
        isStarCollected = false;

        onStarting = true;
        isPlaying = false;

        gameObject.transform.eulerAngles = CurrentRotation;
    }

    // Update is called once per frame
    private void Update() {
        //Preprocess
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
                if (onStarting && !RobotAnimator.GetCurrentAnimatorStateInfo(0).IsName(AnimationConfig.ANIMATION_NAME_OPENING)) {
                    RobotAnimator.SetBool(AnimationConfig.WALKING_ANIMATION_STATE, true);

                    onStarting = false;
                    isPlaying = true;
                }

                break;
        }
    }

    private void MoveForward() {
        tmpMoveVector = Vector3.zero;

        tmpMoveVector = CurrentDir * MovementSpeed;

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
                                RobotAnimator.SetBool(AnimationConfig.WALKING_ANIMATION_STATE, false);
                                transform.position = collision.gameObject.transform.position + new Vector3(0, platformYOffset, 0);

                                platformRotateOffset = collision.gameObject.transform.eulerAngles.y - transform.eulerAngles.y;
                                onRotating = true;
                            } else {
                                // On rotate
                                platformYAxisRotateDeltaDegree += Mathf.Abs(CurrentRotation.y - (collision.gameObject.transform.eulerAngles.y - platformRotateOffset));

                                // Euler angles changed from 180 to -180 or vice versa.
                                if (platformYAxisRotateDeltaDegree > 360f - GameplayConfig.PLATFORM_SNAP_ROTATE)
                                    platformYAxisRotateDeltaDegree -= 360f;

                                CurrentRotation.y = collision.gameObject.transform.eulerAngles.y - platformRotateOffset;

                                // Stop rotate
                                if (platformYAxisRotateDeltaDegree >= GameplayConfig.PLATFORM_ROTATE_MAX) {
                                    // Debug.Log("platformYAxisRotateDeltaDegree = " + platformYAxisRotateDeltaDegree);
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

    private IEnumerator StartRolling() {
        FindObjectOfType<AudioManager>().PlaySFX(AudioConfig.SFX_ACCELERATE_UP);
        RobotAnimator.SetBool(AnimationConfig.ROLLING_ANIMATION_STATE, true);
        onStartRolling = true;

        yield return new WaitForSeconds(AnimationConfig.ANIMATION_DELAY_START_ROLL);

        onStartRolling = false;
        onRolling = true;
    }

    private IEnumerator StopRolling() {
        FindObjectOfType<AudioManager>().PlaySFX(AudioConfig.SFX_ACCELERATE_DOWN);
        RobotAnimator.SetBool(AnimationConfig.ROLLING_ANIMATION_STATE, false);
        onRolling = false;

        yield return new WaitForSeconds(AnimationConfig.ANIMATION_DELAY_STOP_ROLL);

        onStopRolling = true;

        yield return new WaitForSeconds(AnimationConfig.ANIMATION_DELAY_START_WALK_AFTER_ROLL);

        onStopRolling = false;
    }

    private IEnumerator StartDyingFire() {
        if (!onRolling) {
            RobotAnimator.SetBool(AnimationConfig.OPENING_ANIMATION_STATE, false);
            yield return new WaitForSeconds(AnimationConfig.ANIMATION_DELAY_CLOSE);
        } else
            yield return new WaitForSeconds(AnimationConfig.ANIMATION_DELAY_EXPLODE_FIRE);

        isPlaying = false;
        RobotBody.SetActive(false);
        FindObjectOfType<AudioManager>().PlaySFX(AudioConfig.SFX_EXPLOSION);
        ExplosionVFX.Play();
        if (Camera.main.TryGetComponent<CameraShake>(out var cameraShake))
            cameraShake.StartShake();

        StartCoroutine(Lose());
    }

    private IEnumerator StartDyingElectric() {
        isPlaying = false;
        RobotAnimator.speed = 0f;

        yield return new WaitForSeconds(AnimationConfig.ANIMATION_DELAY_ELECTRIFIED);
        FindObjectOfType<AudioManager>().PlaySFX(AudioConfig.SFX_ELECTRIFIED);
        ElectrifiedVFX.Play();

        yield return new WaitForSeconds(AnimationConfig.ANIMATION_DELAY_EXPLODE_ELECCTRIC);
        ElectrifiedVFX.Stop();

        RobotBody.SetActive(false);
        FindObjectOfType<AudioManager>().PlaySFX(AudioConfig.SFX_EXPLOSION);
        ExplosionVFX.Play();
        if (Camera.main.TryGetComponent<CameraShake>(out var cameraShake))
            cameraShake.StartShake();

        StartCoroutine(Lose());
    }

    private IEnumerator Win() {
        isPlaying = false;
        if (onRolling) {
            RobotAnimator.SetBool(AnimationConfig.ROLLING_ANIMATION_STATE, false);
            yield return new WaitForSeconds(AnimationConfig.ANIMATION_DELAY_STOP_ROLL);
        }
        RobotAnimator.SetBool(AnimationConfig.WINNING_ANIMATION_STATE, true);

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