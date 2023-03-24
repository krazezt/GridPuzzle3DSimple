using UnityEngine;

public class CellDragAndDrop : MonoBehaviour {

    public delegate void CellDragEventListener();

    public static CellDragEventListener CellDragEvent;

    // States
    [SerializeField]
    private GameObject[] RulerSides;

    public bool isLocked;

    [HideInInspector]
    public bool onDragging;

    // Variables
    private Vector3 offsetVector;

    private Vector3 firstPosition;

    private Vector3 offsetYDragAddition = new(GridConfig.OFFSET_X_WHILE_DRAG, GridConfig.OFFSET_Y_WHILE_DRAG, GridConfig.OFFSET_Z_WHILE_DRAG);

    private void Awake() {
        firstPosition = transform.position;
        onDragging = false;
    }

    private void OnMouseDown() {
        if (isLocked)
            return;

        ShowRuler();
        offsetVector = transform.position - MouseWorldPositionStart();
        onDragging = true;
    }

    private void OnMouseDrag() {
        if (isLocked)
            return;

        Vector3 fixedPosition = MouseWorldPositionDrag() + offsetVector;
        fixedPosition.y = firstPosition.y;
        fixedPosition += offsetYDragAddition;

        transform.position = fixedPosition;
    }

    private void OnMouseUp() {
        if (isLocked)
            return;

        HideRuler();
        CellDragEvent();
        onDragging = false;
    }

    private Vector3 MouseWorldPositionStart() {
        Vector3 mouseScreenPos = Input.mousePosition;

        mouseScreenPos.z = Camera.main.WorldToScreenPoint(transform.position).z;
        return Camera.main.ScreenToWorldPoint(mouseScreenPos);
    }

    private Vector3 MouseWorldPositionDrag() {
        Vector3 mouseScreenPos = Input.mousePosition;

        mouseScreenPos.z = Camera.main.WorldToScreenPoint(transform.position - offsetYDragAddition).z;
        return Camera.main.ScreenToWorldPoint(mouseScreenPos);
    }

    public void ShowRuler() {
        for (int i = 0; i < RulerSides.Length; i++)
            RulerSides[i].GetComponent<MeshRenderer>().enabled = true;
    }

    public void HideRuler() {
        for (int i = 0; i < RulerSides.Length; i++)
            RulerSides[i].GetComponent<MeshRenderer>().enabled = false;
    }
}