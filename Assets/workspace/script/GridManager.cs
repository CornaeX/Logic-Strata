using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [Header("Grid Settings")]
    [SerializeField] public float cellSize = 1.0f;
    [SerializeField] private GameObject groundChunkPrefab;
    [SerializeField] private int chunkSize = 10; // 10x10 cells per chunk

    private Dictionary<Vector2Int, GameObject> gridObjects = new Dictionary<Vector2Int, GameObject>();
    private Dictionary<Vector2Int, GameObject> activeChunks = new Dictionary<Vector2Int, GameObject>();

    private void Awake()
    {
        Instance = this;
        EnsureChunkExists(Vector2Int.zero);
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

    public void RegisterObject(Vector2Int gridPos, GameObject obj)
    {
        gridObjects[gridPos] = obj;
        EnsureChunkExists(GridToChunkPosition(gridPos));
    }

    public void UnregisterObject(Vector2Int gridPos)
    {
        if (gridObjects.ContainsKey(gridPos))
        {
            gridObjects.Remove(gridPos);
            // Removed CheckAndCleanupUnusedChunks() from here
        }
    }

    // Convert grid cell coordinates to chunk grid coordinates
    private Vector2Int GridToChunkPosition(Vector2Int gridPos)
    {
        int chunkX = Mathf.FloorToInt((float)gridPos.x / chunkSize);
        int chunkY = Mathf.FloorToInt((float)gridPos.y / chunkSize);
        return new Vector2Int(chunkX, chunkY);
    }

    // Spawn chunk ground tile if it doesn't exist yet
    public void EnsureChunkExists(Vector2Int chunkPos)
    {
        if (!activeChunks.ContainsKey(chunkPos))
        {
            // Spawn origin centered at chunk coordinates
            Vector3 worldPos = new Vector3(
                (chunkPos.x * chunkSize + chunkSize / 2f) * cellSize,
                0f,
                (chunkPos.y * chunkSize + chunkSize / 2f) * cellSize
            );

            GameObject newChunk = Instantiate(groundChunkPrefab, worldPos, Quaternion.identity, transform);
            newChunk.name = $"GroundChunk_{chunkPos.x}_{chunkPos.y}";
            
            // Adjust scale matching chunk dimensions (Unity default plane is 10x10 units)
            float scale = (chunkSize * cellSize) / 10f;
            newChunk.transform.localScale = new Vector3(scale, 1f, scale);

            activeChunks[chunkPos] = newChunk;
        }
    }

    // Iterate active chunks and delete any empty ones (except origin 0,0)
    public void CheckAndCleanupUnusedChunks()
    {
        HashSet<Vector2Int> requiredChunks = new HashSet<Vector2Int>();
        requiredChunks.Add(Vector2Int.zero); // Keep origin chunk always active

        foreach (var gridPos in gridObjects.Keys)
        {
            requiredChunks.Add(GridToChunkPosition(gridPos));
        }

        List<Vector2Int> chunksToRemove = new List<Vector2Int>();

        foreach (var chunk in activeChunks)
        {
            if (!requiredChunks.Contains(chunk.Key))
            {
                chunksToRemove.Add(chunk.Key);
            }
        }

        foreach (var chunkPos in chunksToRemove)
        {
            Destroy(activeChunks[chunkPos]);
            activeChunks.Remove(chunkPos);
            Debug.Log($"Cleaned up unused Ground Chunk at {chunkPos}");
        }
    }
}