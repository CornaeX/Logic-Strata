using UnityEngine;

public class ComponentSpawner : MonoBehaviour
{
    [SerializeField] private GameObject componentPrefab;

    void Update()
    {
        // Press 'Space' to spawn a component at grid origin (0, 0, 0)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SpawnComponentAtOrigin();
        }
    }

    public void SpawnComponentAtOrigin()
    {
        Vector2Int originGrid = new Vector2Int(0, 0);

        if (GridManager.Instance != null && !GridManager.Instance.IsCellOccupied(originGrid))
        {
            Vector3 spawnWorldPos = GridManager.Instance.GridToWorldPosition(originGrid);
            GameObject newObj = Instantiate(componentPrefab, spawnWorldPos, Quaternion.identity);
            
            // Register to grid and set correct tag
            newObj.tag = "CircuitComponent";
            GridManager.Instance.RegisterObject(originGrid, newObj);
        }
        else
        {
            Debug.LogWarning("Origin grid cell (0,0) is already occupied or GridManager is missing!");
        }
    }
}