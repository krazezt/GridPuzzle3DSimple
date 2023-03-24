using UnityEngine;

[ExecuteInEditMode]
public class GridSystemController : MonoBehaviour {

    [SerializeField]
    private int Width;

    [SerializeField]
    private int Height;

    [SerializeField]
    private GameObject[] Cells;

    private void Awake() {
        UpdateGridScale();
        UpdateCellsPosition();

        CellDragAndDrop.CellDragEvent += CheckCellSwap;
    }

    private void LateUpdate() {
        UpdateGridScale();
    }

    private Vector3 CalculateCellPosition(int cellIndex) {
        Vector3 result = new() {
            x = Mathf.RoundToInt(cellIndex % Width) * GridConfig.CELL_TO_GRID_SCALE - Mathf.RoundToInt((float)Width / 2 * GridConfig.CELL_TO_GRID_SCALE) + Mathf.RoundToInt(GridConfig.CELL_TO_GRID_SCALE / 2),
            y = GridConfig.DEFAULT_Y_CELL_POS,
            z = - Mathf.RoundToInt(cellIndex / Width) * GridConfig.CELL_TO_GRID_SCALE + Mathf.RoundToInt((float)Height / 2 * GridConfig.CELL_TO_GRID_SCALE) - Mathf.RoundToInt(GridConfig.CELL_TO_GRID_SCALE / 2)
        };

        return result;
    }

    private void UpdateCellsPosition() {
        for (int i = 0; i < Cells.Length; i++)
            Cells[i].transform.localPosition = CalculateCellPosition(i);
    }

    private void UpdateGridScale() {
        Vector3 fixedScale = new();
        int oldWidth = Width, oldHeight = Height;

        Width = Mathf.RoundToInt(transform.localScale.x);
        Height = Mathf.RoundToInt(transform.localScale.z);

        fixedScale.x = Width;
        fixedScale.y = GridConfig.DEFAULT_Y_SYSTEM_SCALE;
        fixedScale.z = Height;

        transform.localScale = fixedScale;

        if ((oldWidth != Width) || (oldHeight != Height))
            UpdateCellsPosition();
    }

    private void CheckCellSwap() {
        for (int i = 0; i < Cells.Length; i++) {
            if (Cells[i].GetComponent<CellDragAndDrop>().onDragging)
                for (int j = 0; j < Cells.Length; j++)
                    if (
                        (i != j) &&
                        (!Cells[i].GetComponent<CellDragAndDrop>().isLocked) &&
                        (!Cells[j].GetComponent<CellDragAndDrop>().isLocked) &&
                        (Vector3.Distance(Cells[i].transform.position, Cells[j].transform.position) <= GridConfig.CELL_SNAP_DISTANCE)
                        ) {
                        SwapCells(i, j);
                        UpdateCellsPosition();
                        return;
                    }
        }

        UpdateCellsPosition();
    }

    private void SwapCells(int index1, int index2) {
        (Cells[index2], Cells[index1]) = (Cells[index1], Cells[index2]);
    }
}