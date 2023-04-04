public class GameplayConfig {
    public const float WALL_COLLIDE_DELAY = 0.1f;
    public const float PLATFORM_ROTATE_DELAY = 0.5f;
    public const float PLATFORM_ROTATE_DELAY_ON_ROLLING = 0.25f;

    public const float PLATFORM_SNAP_DISTANCE = 0.45f;
    public const float PLATFORM_SNAP_ROTATE = 10f;
    public const float PLATFORM_ROTATE_MAX = 90f;

    public const string TAG_CELL = "Cell";
    public const string TAG_WALL = "Wall";
    public const string TAG_GAMECONTROLLER = "GameController";

    public const string NAME_PLATFORM = "Platform";
    public const string NAME_ACCELERATE_UP = "AccelerateUpArea";
    public const string NAME_ACCELERATE_DOWN = "AccelerateDownArea";
    public const string NAME_ELECTRIC_ARC = "ElectricArc";
    public const string NAME_FIRE_AREA = "FireArea";
    public const string NAME_STAR = "Star";
    public const string NAME_GOAL_POINT = "GoalPoint";

    public enum MoveDirection {
        TOP,
        BOTTOM,
        LEFT,
        RIGHT
    }

    public enum PlayState {
        LOSING,
        PLAYING,
        WINNING
    }
}