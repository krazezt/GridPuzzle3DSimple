public class GameplayConfig {
    public const float WALL_COLLIDE_DELAY = 0.1f;
    public const float PLATFORM_ROTATE_DELAY = 1f;

    public const float PLATFORM_SNAP_DISTANCE = 0.42f;
    public const float PLATFORM_SNAP_ROTATE = 10f;
    public const float PLATFORM_ROTATE_MAX = 90f;

    public const string TAG_WALL = "Wall";
    public const string TAG_GAMECONTROLLER = "GameController";

    public const string NAME_PLATFORM = "Platform";

    public enum MoveDirection {
        TOP, BOTTOM, LEFT, RIGHT
    }
}