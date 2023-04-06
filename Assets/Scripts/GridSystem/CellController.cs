using UnityEngine;
using UnityEngine.EventSystems;

[ExecuteInEditMode]
public class CellController : MonoBehaviour {

    public delegate void CellDragEventListener();

    public static CellDragEventListener CellDragEvent;

    // States
    [SerializeField]
    private GameObject[] RulerSides;

    public GameObject blockUpperSideQuad;   // The Quad/Plane in the Block's upper side to make player know which Cell is being locked.
    public bool isLocked;

    [HideInInspector]
    public bool onDragging;

    // Variables
    private Vector3 offsetVector;

    private Vector3 firstPosition;

    private Vector3 offsetYDragAddition = new(GridConfig.OFFSET_X_WHILE_DRAG, GridConfig.OFFSET_Y_WHILE_DRAG, GridConfig.OFFSET_Z_WHILE_DRAG);

    private bool tmpLocked;

    private void Awake() {
        firstPosition = transform.position;
        onDragging = false;
        tmpLocked = isLocked;

        if (blockUpperSideQuad == null)
            return;
        else {
            if (isLocked)
                blockUpperSideQuad.GetComponent<MeshRenderer>().enabled = true;
            else
                blockUpperSideQuad.GetComponent<MeshRenderer>().enabled = false;
        }
    }

    public void LockCellTmp() {
        tmpLocked = true;
        blockUpperSideQuad.GetComponent<MeshRenderer>().enabled = true;
    }

    public void UnlockCellTmp() {
        tmpLocked = false;
        if (!isLocked)
            blockUpperSideQuad.GetComponent<MeshRenderer>().enabled = false;
    }

    private void OnMouseDown() {
        if (isLocked || tmpLocked || !Application.isPlaying || IsMouseOverUI()) {
            if (!IsMouseOverUI())
                FindObjectOfType<AudioManager>().PlaySFX(AudioConfig.SFX_CELL_LOCKED);
            return;
        }

        FindObjectOfType<AudioManager>().PlaySFX(AudioConfig.SFX_CELL_DRAG);
        TimeManager.instance.StopTime();
        ShowRuler();
        offsetVector = transform.position - MouseWorldPositionStart();
        onDragging = true;
    }

    private void OnMouseDrag() {
        if (isLocked || tmpLocked || !Application.isPlaying || IsMouseOverUI())
            return;

        Vector3 fixedPosition = MouseWorldPositionDrag() + offsetVector;
        fixedPosition.y = firstPosition.y;
        fixedPosition += offsetYDragAddition;

        transform.position = fixedPosition;
    }

    private void OnMouseUp() {
        if (isLocked || tmpLocked || !Application.isPlaying || IsMouseOverUI())
            return;

        FindObjectOfType<AudioManager>().PlaySFX(AudioConfig.SFX_CELL_DRAG);
        TimeManager.instance.UnstopTime();
        HideRuler();
        CellDragEvent();
        onDragging = false;
    }

    private bool IsMouseOverUI() {
        return EventSystem.current.IsPointerOverGameObject();
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