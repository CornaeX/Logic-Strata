using UnityEngine;

public class ComponentInteractionController : MonoBehaviour
{
    [SerializeField] private LayerMask groundLayer;

    private GameObject activeComponent;
    private Vector2Int originalGridPos;
    private Vector3 targetWorldPosition;
    private bool isDragging = false;
    private bool justPickedUp = false;

    void Update()
    {
        if (isDragging && activeComponent != null)
        {
            UpdateDragPosition();

            // Instant position snapping (smooth movement removed)
            activeComponent.transform.position = targetWorldPosition;

            // Skip input check on the frame object is picked up
            if (justPickedUp)
            {
                if (Input.GetMouseButtonUp(0))
                {
                    justPickedUp = false;
                }
                return;
            }

            // Confirm Placement (Left Click or G)
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.G))
            {
                TryPlaceComponent();
            }
            // Cancel Placement (Right Click or Escape)
            else if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
            {
                CancelPlacement();
            }
        }
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

        // FIRST PASS: Check if the mouse is hovering over ANY interactive knob/button
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.CompareTag("InteractiveKnob"))
            {
                Debug.Log($"[DEBUG] Direct click on Knob '{hit.collider.name}'. Blocked object movement.");
                return; // Exit immediately so the object IS NOT picked up
            }
        }

        // SECOND PASS: If no knob was clicked, check for body component pickup
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.CompareTag("CircuitComponent"))
            {
                activeComponent = hit.collider.transform.root.gameObject;

                if (GridManager.Instance != null)
                {
                    originalGridPos = GridManager.Instance.WorldToGridPosition(activeComponent.transform.position);
                    GridManager.Instance.UnregisterObject(originalGridPos);
                }

                targetWorldPosition = activeComponent.transform.position;
                
                Collider[] colliders = activeComponent.GetComponentsInChildren<Collider>();
                foreach (Collider c in colliders)
                {
                    c.enabled = false;
                }

                isDragging = true;
                justPickedUp = true;
                Debug.Log($"[DEBUG] Successfully picked up root '{activeComponent.name}'");
                return;
            }
        }
    }

    private void UpdateDragPosition()
    {
        if (activeComponent == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        // Increased raycast distance from 10f to 1000f
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, groundLayer))
        {
            if (GridManager.Instance != null)
            {
                Vector2Int currentGridPos = GridManager.Instance.WorldToGridPosition(hit.point);
                targetWorldPosition = GridManager.Instance.GridToWorldPosition(currentGridPos);
            }
            else
            {
                targetWorldPosition = hit.point;
            }
        }
    }

    private void TryPlaceComponent()
    {
        if (activeComponent == null) return;

        if (GridManager.Instance != null)
        {
            Vector2Int currentGridPos = GridManager.Instance.WorldToGridPosition(targetWorldPosition);

            if (!GridManager.Instance.IsCellOccupied(currentGridPos))
            {
                activeComponent.transform.position = targetWorldPosition;
                GridManager.Instance.RegisterObject(currentGridPos, activeComponent);
                
                Debug.Log($"[DEBUG] Placed '{activeComponent.name}' at grid cell {currentGridPos}");
                FinishPlacement();
            }
            else
            {
                Debug.LogWarning($"[DEBUG WARNING] Grid cell {currentGridPos} is occupied!");
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
            Vector3 originalWorldPos = GridManager.Instance.GridToWorldPosition(originalGridPos);
            activeComponent.transform.position = originalWorldPos;
            GridManager.Instance.RegisterObject(originalGridPos, activeComponent);
        }

        Debug.Log("[DEBUG] Placement cancelled.");
        FinishPlacement();
    }

    private void FinishPlacement()
    {
        if (activeComponent != null)
        {
            Collider[] colliders = activeComponent.GetComponentsInChildren<Collider>();
            foreach (Collider c in colliders)
            {
                c.enabled = true;
            }
        }

        activeComponent = null;
        isDragging = false;

        // Clean up ground chunks only after object is dropped
        if (GridManager.Instance != null)
        {
            GridManager.Instance.CheckAndCleanupUnusedChunks();
        }
    }
}