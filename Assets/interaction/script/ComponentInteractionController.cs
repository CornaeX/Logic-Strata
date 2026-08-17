using UnityEngine;

public class ComponentInteractionController : MonoBehaviour
{
    [Header("Raycast")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float rayDistance = 1000f;

    [Header("Selection")]
    [SerializeField] private LayerMask componentLayer;
    [SerializeField] private RuntimeTransformGizmo transformGizmo;

    private GameObject activeComponent;

    private Vector2Int originalGridPos;
    private Vector3 originalWorldPosition;
    private Quaternion originalRotation;

    private Vector3 targetWorldPosition;

    private bool isDragging = false;
    private bool justPickedUp = false;

    private GameObject selectedComponent;

    private void Update()
    {
        HandleKeyboardSelection();

        if (isDragging && activeComponent != null)
        {
            UpdateDragPosition();

            activeComponent.transform.position = targetWorldPosition;

            if (justPickedUp)
            {
                if (Input.GetMouseButtonUp(0))
                {
                    justPickedUp = false;
                }

                return;
            }

            // Confirm placement
            if (Input.GetMouseButtonDown(0) ||
                Input.GetKeyDown(KeyCode.G))
            {
                TryPlaceComponent();
            }

            // Cancel placement
            else if (Input.GetMouseButtonDown(1) ||
                     Input.GetKeyDown(KeyCode.Escape))
            {
                CancelPlacement();
            }

            return;
        }

        HandleSelection();
    }

    // ============================================================
    // SELECTION
    // ============================================================

    private void HandleSelection()
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        // Don't select when clicking the gizmo
        if (RuntimeTransformGizmo.IsPointerOverGizmo)
            return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit[] hits = Physics.RaycastAll(
            ray,
            rayDistance
        );

        GameObject clickedComponent = null;

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.CompareTag("InteractiveKnob"))
            {
                // Let the knob system handle this.
                return;
            }

            if (hit.collider.CompareTag("CircuitComponent"))
            {
                clickedComponent = hit.collider.transform.root.gameObject;
                break;
            }
        }

        if (clickedComponent != null)
        {
            SelectComponent(clickedComponent);
        }
        else
        {
            // Click empty space
            DeselectComponent();
        }
    }

    private void SelectComponent(GameObject component)
    {
        if (selectedComponent == component)
            return;

        DeselectComponent();

        selectedComponent = component;

        ComponentSelectionHighlight highlight =
            selectedComponent.GetComponent<ComponentSelectionHighlight>();

        if (highlight == null)
        {
            highlight =
                selectedComponent.AddComponent<ComponentSelectionHighlight>();
        }

        highlight.SetSelected(true);

        if (transformGizmo != null)
        {
            transformGizmo.SetTarget(selectedComponent);
        }

        Debug.Log(
            $"[SELECT] Selected '{selectedComponent.name}'"
        );
    }

    public void DeselectComponent()
    {
        if (selectedComponent != null)
        {
            ComponentSelectionHighlight highlight =
                selectedComponent.GetComponent<ComponentSelectionHighlight>();

            if (highlight != null)
                highlight.SetSelected(false);
        }

        selectedComponent = null;

        if (transformGizmo != null)
        {
            transformGizmo.ClearTarget();
        }
    }

    // ============================================================
    // KEYBOARD
    // ============================================================

    private void HandleKeyboardSelection()
    {
        if (selectedComponent == null)
            return;

        // W = Move mode
        if (Input.GetKeyDown(KeyCode.W))
        {
            if (transformGizmo != null)
                transformGizmo.SetMoveMode();
        }

        // E = Rotate mode
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (transformGizmo != null)
                transformGizmo.SetRotateMode();
        }

        // X axis
        if (Input.GetKeyDown(KeyCode.X))
        {
            if (transformGizmo != null)
                transformGizmo.SetAxis(
                    RuntimeTransformGizmo.TransformAxis.X
                );
        }

        // Z axis
        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (transformGizmo != null)
                transformGizmo.SetAxis(
                    RuntimeTransformGizmo.TransformAxis.Z
                );
        }

        // R = rotate 90 degrees
        if (Input.GetKeyDown(KeyCode.R))
        {
            selectedComponent.transform.Rotate(
                Vector3.up,
                90f,
                Space.World
            );
        }

        // Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (transformGizmo != null)
                transformGizmo.CancelTransform();
        }
    }

    // ============================================================
    // PICKUP / PLACE
    // ============================================================

    public void TryPickUpComponent()
    {
        Ray ray = Camera.main.ScreenPointToRay(
            Input.mousePosition
        );

        RaycastHit[] hits = Physics.RaycastAll(
            ray,
            rayDistance
        );

        // FIRST PASS:
        // Don't pick up if clicking an interactive knob
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.CompareTag("InteractiveKnob"))
            {
                Debug.Log(
                    $"[DEBUG] Direct click on Knob '{hit.collider.name}'."
                );

                return;
            }
        }

        // SECOND PASS:
        // Find component
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.CompareTag("CircuitComponent"))
            {
                activeComponent =
                    hit.collider.transform.root.gameObject;

                originalWorldPosition =
                    activeComponent.transform.position;

                originalRotation =
                    activeComponent.transform.rotation;

                if (GridManager.Instance != null)
                {
                    originalGridPos =
                        GridManager.Instance.WorldToGridPosition(
                            activeComponent.transform.position
                        );

                    GridManager.Instance.UnregisterObject(
                        originalGridPos
                    );
                }

                targetWorldPosition =
                    activeComponent.transform.position;

                Collider[] colliders =
                    activeComponent.GetComponentsInChildren<Collider>();

                foreach (Collider c in colliders)
                {
                    c.enabled = false;
                }

                isDragging = true;
                justPickedUp = true;

                // Hide selection gizmo while placing
                if (transformGizmo != null)
                    transformGizmo.ClearTarget();

                Debug.Log(
                    $"[DEBUG] Picked up '{activeComponent.name}'"
                );

                return;
            }
        }
    }

    private void UpdateDragPosition()
    {
        if (activeComponent == null)
            return;

        Ray ray =
            Camera.main.ScreenPointToRay(
                Input.mousePosition
            );

        if (Physics.Raycast(
            ray,
            out RaycastHit hit,
            rayDistance,
            groundLayer))
        {
            if (GridManager.Instance != null)
            {
                Vector2Int gridPosition =
                    GridManager.Instance.WorldToGridPosition(
                        hit.point
                    );

                targetWorldPosition =
                    GridManager.Instance.GridToWorldPosition(
                        gridPosition
                    );
            }
            else
            {
                // IMPORTANT:
                // Keep original Y.
                targetWorldPosition = new Vector3(
                    hit.point.x,
                    originalWorldPosition.y,
                    hit.point.z
                );
            }
        }
    }

    private void TryPlaceComponent()
    {
        if (activeComponent == null)
            return;

        if (GridManager.Instance != null)
        {
            Vector2Int currentGridPos =
                GridManager.Instance.WorldToGridPosition(
                    targetWorldPosition
                );

            if (!GridManager.Instance.IsCellOccupied(
                currentGridPos))
            {
                activeComponent.transform.position =
                    targetWorldPosition;

                GridManager.Instance.RegisterObject(
                    currentGridPos,
                    activeComponent
                );

                FinishPlacement();

                return;
            }

            Debug.LogWarning(
                $"[GRID] Cell {currentGridPos} is occupied."
            );
        }
        else
        {
            FinishPlacement();
        }
    }

    private void CancelPlacement()
    {
        if (activeComponent == null)
            return;

        activeComponent.transform.position =
            originalWorldPosition;

        activeComponent.transform.rotation =
            originalRotation;

        if (GridManager.Instance != null)
        {
            GridManager.Instance.RegisterObject(
                originalGridPos,
                activeComponent
            );
        }

        Debug.Log("[DEBUG] Placement cancelled.");

        FinishPlacement();
    }

    private void FinishPlacement()
    {
        if (activeComponent != null)
        {
            Collider[] colliders =
                activeComponent.GetComponentsInChildren<Collider>();

            foreach (Collider c in colliders)
            {
                c.enabled = true;
            }
        }

        activeComponent = null;
        isDragging = false;
        justPickedUp = false;

        if (GridManager.Instance != null)
        {
            GridManager.Instance.CheckAndCleanupUnusedChunks();
        }
    }
}