using UnityEngine;

public class ComponentInteractionController : MonoBehaviour
{
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float smoothSpeed = 25f;

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

            // Smooth movement towards grid target
            activeComponent.transform.position = Vector3.Lerp(
                activeComponent.transform.position, 
                targetWorldPosition, 
                Time.deltaTime * smoothSpeed
            );

            // Ignore input on the exact frame the item was picked up
            if (justPickedUp)
            {
                justPickedUp = false;
                return;
            }

            // Confirm Placement
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.G))
            {
                Debug.Log("[DEBUG] Confirm placement triggered.");
                TryPlaceComponent();
            }
            // Cancel Placement
            else if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
            {
                Debug.Log("[DEBUG] Cancel placement triggered.");
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

        Debug.Log($"[DEBUG] PickUp Raycast triggered. Total hits under cursor: {hits.Length}");

        foreach (RaycastHit hit in hits)
        {
            Debug.Log($"[DEBUG] Hit Object: '{hit.collider.name}' | Tag: '{hit.collider.tag}' | Parent: '{hit.collider.transform.root.name}'");

            // Ignore clicks directly on knobs
            if (hit.collider.CompareTag("Untagged"))
            {
                Debug.Log("[DEBUG] Clicked on an InteractiveKnob! Aborting pick-up.");
                return;
            }

            if (hit.collider.CompareTag("CircuitComponent"))
            {
                // Grabs the root parent object so everything moves together
                activeComponent = hit.collider.transform.root.gameObject;
                Debug.Log($"[DEBUG] Target object for movement set to ROOT: '{activeComponent.name}'");

                if (GridManager.Instance != null)
                {
                    originalGridPos = GridManager.Instance.WorldToGridPosition(activeComponent.transform.position);
                    GridManager.Instance.UnregisterObject(originalGridPos);
                    Debug.Log($"[DEBUG] Unregistered '{activeComponent.name}' from Grid cell {originalGridPos}");
                }
                else
                {
                    Debug.LogWarning("[DEBUG WARNING] GridManager.Instance is NULL!");
                }

                targetWorldPosition = activeComponent.transform.position;
                
                // Disable all colliders across parent and children during drag
                Collider[] colliders = activeComponent.GetComponentsInChildren<Collider>();
                Debug.Log($"[DEBUG] Disabling {colliders.Length} colliders on target object during drag.");
                foreach (Collider c in colliders)
                {
                    c.enabled = false;
                }

                isDragging = true;
                justPickedUp = true;
                Debug.Log($"[DEBUG] Successfully picked up '{activeComponent.name}'!");
                return;
            }
        }

        Debug.Log("[DEBUG] Raycast completed, but no object with tag 'CircuitComponent' was hit.");
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
                targetWorldPosition = GridManager.Instance.GridToWorldPosition(currentGridPos);
            }
            else
            {
                targetWorldPosition = hit.point;
            }
        }
        else
        {
            Debug.LogWarning("[DEBUG WARNING] Dragging active, but raycast is NOT hitting the 'Ground' layer!");
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
                FinishPlacement();
                Debug.Log($"[DEBUG] Placed '{activeComponent.name}' at grid cell {currentGridPos}");
            }
            else
            {
                Debug.LogWarning($"[DEBUG WARNING] Cell {currentGridPos} is occupied!");
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

        FinishPlacement();
        Debug.Log("[DEBUG] Placement cancelled.");
    }

    private void FinishPlacement()
    {
        if (activeComponent != null)
        {
            Collider[] colliders = activeComponent.GetComponentsInChildren<Collider>();
            Debug.Log($"[DEBUG] Re-enabling {colliders.Length} colliders on '{activeComponent.name}'.");
            foreach (Collider c in colliders)
            {
                c.enabled = true;
            }
        }

        activeComponent = null;
        isDragging = false;
    }
}