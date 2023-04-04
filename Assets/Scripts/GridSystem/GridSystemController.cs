using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GridSystemController : MonoBehaviour {
    public int Width;

    public int Height;

    public GameObject EdgeWallObject;

    public GameObject WallHolder;

    public GameObject[] Cells;

    // Walls
    private readonly List<GameObject> EdgeWallPieces = new();

    private void Awake() {

        UpdateGridScale();
        UpdateCellsPosition();

        CellController.CellDragEvent = null;
        CellController.CellDragEvent += CheckCellSwap;
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
        int oldWidth = Width, oldHeight = Height;
        Width = Mathf.RoundToInt(transform.localScale.x);
        Height = Mathf.RoundToInt(transform.localScale.z);

        transform.localScale = new() {
            x = Width,
            y = GridConfig.DEFAULT_Y_SYSTEM_SCALE,
            z = Height
        };

        if ((oldWidth != Width) || (oldHeight != Height)) {
            GenerateWalls();
            UpdateCellsPosition();
        }
    }

    private void CheckCellSwap() {
        for (int i = 0; i < Cells.Length; i++) {
            if (Cells[i].GetComponent<CellController>().onDragging)
                for (int j = 0; j < Cells.Length; j++)
                    if (
                        (i != j) &&
                        (!Cells[i].GetComponent<CellController>().isLocked) &&
                        (!Cells[j].GetComponent<CellController>().isLocked) &&
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

    private void GenerateWalls() {
        ClearWallPieces();

        // Calculate offsets
        Vector3 offsetX = new() {
            x = GridConfig.DEFAULT_X_WALL_OFFSET + GridConfig.CELL_TO_GRID_SCALE/2 + GridConfig.DEFAULT_X_WALL_SCALE / 2,
            y = 0,
            z = 0
        };

        Vector3 offsetY = new() {
            x = 0,
            y = GridConfig.DEFAULT_Y_WALL_OFFSET,
            z = 0
        };

        Vector3 offsetZ = new() {
            x = 0,
            y = 0,
            z = GridConfig.DEFAULT_Z_WALL_OFFSET + GridConfig.CELL_TO_GRID_SCALE/2 + GridConfig.DEFAULT_Z_WALL_SCALE / 2
        };

        Vector3 offsetCorner = new() {
            x = GridConfig.DEFAULT_X_WALL_SCALE / 2 + GridConfig.DEFAULT_X_WALL_SCALE * GridConfig.NUMBER_OF_WALLS_PIECE_EACH_CELL / 2 ,
            y = 0,
            z = 0,
        };

        // Generate walls at the top side of GridSystem
        for (int i = 0; i < Width; i++) {
            Vector3 cellPos = CalculateCellPosition(i);
            if (i == 0) {
                GenerateWallGroup(cellPos + offsetZ + offsetY, GridConfig.WallGroupDirection.HORIZONTAL);
                GenerateWallPiece(cellPos + offsetZ + offsetY - offsetCorner);
            } else if (i == Width - 1) {
                GenerateWallGroup(cellPos + offsetZ + offsetY, GridConfig.WallGroupDirection.HORIZONTAL);
                GenerateWallPiece(cellPos + offsetZ + offsetY + offsetCorner);
            } else
                GenerateWallGroup(cellPos + offsetZ + offsetY, GridConfig.WallGroupDirection.HORIZONTAL);
        }

        // Generate walls at the bottom side of GridSystem
        for (int i = 0; i < Width; i++) {
            Vector3 cellPos = CalculateCellPosition(Width * (Height - 1) + i);
            if (i == 0) {
                GenerateWallGroup(cellPos - offsetZ + offsetY, GridConfig.WallGroupDirection.HORIZONTAL);
                GenerateWallPiece(cellPos - offsetZ + offsetY - offsetCorner);
            } else if (i == Width - 1) {
                GenerateWallGroup(cellPos - offsetZ + offsetY, GridConfig.WallGroupDirection.HORIZONTAL);
                GenerateWallPiece(cellPos - offsetZ + offsetY + offsetCorner);
            } else
                GenerateWallGroup(cellPos - offsetZ + offsetY, GridConfig.WallGroupDirection.HORIZONTAL);
        }

        // Generate walls at the left and right side of GridSystem
        for (int i = 0; i < Height; i++) {
            int l = Width * i, r = Width * (i + 1) - 1;
            GenerateWallGroup(CalculateCellPosition(l) - offsetX + offsetY, GridConfig.WallGroupDirection.VERTICAL);
            GenerateWallGroup(CalculateCellPosition(r) + offsetX + offsetY, GridConfig.WallGroupDirection.VERTICAL);
        }
    }

    private void GenerateWallGroup(Vector3 centerPos, GridConfig.WallGroupDirection direction) {
        switch (direction) {
            case GridConfig.WallGroupDirection.HORIZONTAL:
                for (int i = 1; i <= GridConfig.NUMBER_OF_WALLS_PIECE_EACH_CELL; i++) {
                    GenerateWallPiece(new() {
                        x = centerPos.x - GridConfig.NUMBER_OF_WALLS_PIECE_EACH_CELL * GridConfig.DEFAULT_X_WALL_SCALE / 2 + i * GridConfig.DEFAULT_X_WALL_SCALE - GridConfig.DEFAULT_X_WALL_SCALE / 2,
                        y = centerPos.y,
                        z = centerPos.z,
                    });
                }
                break;

            case GridConfig.WallGroupDirection.VERTICAL:
                for (int i = 1; i <= GridConfig.NUMBER_OF_WALLS_PIECE_EACH_CELL; i++) {
                    GenerateWallPiece(new() {
                        x = centerPos.x,
                        y = centerPos.y,
                        z = centerPos.z - GridConfig.NUMBER_OF_WALLS_PIECE_EACH_CELL * GridConfig.DEFAULT_Z_WALL_SCALE / 2 + i * GridConfig.DEFAULT_Z_WALL_SCALE - GridConfig.DEFAULT_Z_WALL_SCALE / 2,
                    });
                }
                break;
        }
    }

    private void GenerateWallPiece(Vector3 localPos) {
        GameObject newWallPiece = Instantiate(EdgeWallObject);
        newWallPiece.transform.localPosition = localPos;
        newWallPiece.transform.parent = WallHolder.transform;

        EdgeWallPieces.Add(newWallPiece);
    }

    private void ClearWallPieces() {
        EdgeWallPieces.ForEach(delegate (GameObject piece) {
            DestroyImmediate(piece);
        });

        EdgeWallPieces.Clear();
    }
}