using UnityEngine;

public class ComponentInteractionController : MonoBehaviour
{
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float smoothSpeed = 25f; // Higher = faster snap, Lower = smoother drag

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

            // Smoothly lerp towards target grid coordinate position
            activeComponent.transform.position = Vector3.Lerp(
                activeComponent.transform.position, 
                targetWorldPosition, 
                Time.deltaTime * smoothSpeed
            );

            // Ignore input on the exact frame component was picked up
            if (justPickedUp)
            {
                justPickedUp = false;
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

                targetWorldPosition = activeComponent.transform.position;
                
                Collider componentCollider = activeComponent.GetComponent<Collider>();
                if (componentCollider != null)
                {
                    componentCollider.enabled = false;
                }

                isDragging = true;
                justPickedUp = true;
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
                targetWorldPosition = GridManager.Instance.GridToWorldPosition(currentGridPos);
                
                // Trigger infinite ground expansion if near workspace bounds
                GridManager.Instance.CheckAndExpandGrid(currentGridPos);
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
            Vector3 originalWorldPos = GridManager.Instance.GridToWorldPosition(originalGridPos);
            activeComponent.transform.position = originalWorldPos;
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