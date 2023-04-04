public class GridConfig {
    public const float DEFAULT_Y_SYSTEM_SCALE = 1f;
    public const float DEFAULT_Y_CELL_POS = 2f;

    public const float CELL_TO_GRID_SCALE = 10f;
    public const float CELL_SNAP_DISTANCE = 11f;

    public const float DEFAULT_X_WALL_OFFSET = 0f;
    public const float DEFAULT_Y_WALL_OFFSET = -2f;
    public const float DEFAULT_Z_WALL_OFFSET = 0f;
    public const float DEFAULT_X_WALL_SCALE = 2.5f;
    public const float DEFAULT_Z_WALL_SCALE = 2.5f;

    public const float OFFSET_X_WHILE_DRAG = 0f;
    public const float OFFSET_Y_WHILE_DRAG = 10f;
    public const float OFFSET_Z_WHILE_DRAG = 0f;

    public const int NUMBER_OF_WALLS_PIECE_EACH_CELL = 4;

    public const string CEllS_NAME_POSFIX = "Cell_";

    public enum WallGroupDirection {
        VERTICAL,
        HORIZONTAL
    }
}