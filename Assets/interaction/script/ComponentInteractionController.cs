using UnityEngine;

public class ComponentInteractionController : MonoBehaviour
{
    [SerializeField] private LayerMask groundLayer;

    private GameObject activeComponent;
    private Vector2Int originalGridPos;
    private bool isDragging = false;

    void Update()
    {
        // Dragging Phase
        if (isDragging && activeComponent != null)
        {
            UpdateDragPosition();

            // Confirm Placement (Left-Click or 'G')
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.G))
            {
                TryPlaceComponent();
            }
            // Cancel Placement (Right-Click or Escape)
            else if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
            {
                CancelPlacement();
            }
        }
        // Selection / Pick Up Phase (uses 'else if' to prevent triggering on the same frame as placement)
        else if (!isDragging)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.G))
            {
                TryPickUpComponent();
            }
        }
    }

    private void TryPickUpComponent()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray, 1000f);

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.CompareTag("CircuitComponent"))
            {
                activeComponent = hit.collider.gameObject;

                if (GridManager.Instance != null)
                {
                    originalGridPos = GridManager.Instance.WorldToGridPosition(activeComponent.transform.position);
                    GridManager.Instance.UnregisterObject(originalGridPos);
                }
                
                // Disable object collider during drag so raycast passes through to the ground plane
                Collider componentCollider = activeComponent.GetComponent<Collider>();
                if (componentCollider != null)
                {
                    componentCollider.enabled = false;
                }

                isDragging = true;
                Debug.Log($"Picked up {activeComponent.name} successfully!");
                return;
            }
        }
    }

    private void UpdateDragPosition()
    {
        if (activeComponent == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, groundLayer))
        {
            if (GridManager.Instance != null)
            {
                Vector2Int currentGridPos = GridManager.Instance.WorldToGridPosition(hit.point);
                activeComponent.transform.position = GridManager.Instance.GridToWorldPosition(currentGridPos);
            }
            else
            {
                activeComponent.transform.position = hit.point;
            }
        }
    }

    private void TryPlaceComponent()
    {
        if (activeComponent == null) return; // Prevent NullReferenceException

        if (GridManager.Instance != null)
        {
            Vector2Int currentGridPos = GridManager.Instance.WorldToGridPosition(activeComponent.transform.position);

            if (!GridManager.Instance.IsCellOccupied(currentGridPos))
            {
                GridManager.Instance.RegisterObject(currentGridPos, activeComponent);
                FinishPlacement();
                Debug.Log($"Placed {activeComponent.name} at grid cell {currentGridPos}");
            }
            else
            {
                Debug.LogWarning($"Cell {currentGridPos} is already occupied!");
            }
        }
        else
        {
            FinishPlacement();
        }
    }

    private void CancelPlacement()
    {
        if (activeComponent == null) return;

        if (GridManager.Instance != null)
        {
            activeComponent.transform.position = GridManager.Instance.GridToWorldPosition(originalGridPos);
            GridManager.Instance.RegisterObject(originalGridPos, activeComponent);
        }

        FinishPlacement();
        Debug.Log("Placement cancelled.");
    }

    private void FinishPlacement()
    {
        if (activeComponent != null)
        {
            Collider componentCollider = activeComponent.GetComponent<Collider>();
            if (componentCollider != null)
            {
                componentCollider.enabled = true;
            }
        }

        activeComponent = null;
        isDragging = false;
    }
}