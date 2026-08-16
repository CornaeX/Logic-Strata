using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [SerializeField] public float cellSize = 1.0f;
    [SerializeField] private Transform groundPlaneTransform;
    [SerializeField] private int initialGridRadius = 50;
    
    private Dictionary<Vector2Int, GameObject> gridObjects = new Dictionary<Vector2Int, GameObject>();
    private int currentGridRadius;

    private void Awake()
    {
        Instance = this;
        currentGridRadius = initialGridRadius;
        UpdateGroundPlaneScale();
    }

    public Vector2Int WorldToGridPosition(Vector3 worldPos)
    {
        int x = Mathf.RoundToInt(worldPos.x / cellSize);
        int z = Mathf.RoundToInt(worldPos.z / cellSize);
        return new Vector2Int(x, z);
    }

    public Vector3 GridToWorldPosition(Vector2Int gridPos)
    {
        return new Vector3(gridPos.x * cellSize, 0f, gridPos.y * cellSize);
    }

    public bool IsCellOccupied(Vector2Int gridPos) => gridObjects.ContainsKey(gridPos);

    public void RegisterObject(Vector2Int gridPos, GameObject obj) => gridObjects[gridPos] = obj;

    public void UnregisterObject(Vector2Int gridPos)
    {
        if (gridObjects.ContainsKey(gridPos)) gridObjects.Remove(gridPos);
    }

    // Checks distance to edge and expands the ground plane dynamically
    public void CheckAndExpandGrid(Vector2Int currentPos)
    {
        int maxCoord = Mathf.Max(Mathf.Abs(currentPos.x), Mathf.Abs(currentPos.y));
        if (maxCoord >= currentGridRadius - 5)
        {
            currentGridRadius += 25;
            UpdateGroundPlaneScale();
            Debug.Log($"Ground Expanded! New Radius: {currentGridRadius}");
        }
    }

    private void UpdateGroundPlaneScale()
    {
        if (groundPlaneTransform != null)
        {
            // Unity plane default size is 10x10 units per scale factor of 1
            float requiredScale = (currentGridRadius * 2f * cellSize) / 10f;
            groundPlaneTransform.localScale = new Vector3(requiredScale, 1f, requiredScale);
        }
    }
}