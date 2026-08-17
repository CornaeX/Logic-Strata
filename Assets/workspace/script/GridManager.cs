using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [Header("Grid Movement & Snapping")]
    [SerializeField] public float cellSize = 0.001f; // Now this can safely be 0.001 for fine movement!

    [Header("Ground Chunk Settings")]
    [SerializeField] private GameObject groundChunkPrefab;
    [SerializeField] private int chunkSize = 10; 
    [SerializeField] private float chunkVisualScale = 1.0f; // Set this in Inspector to whatever size your plane prefab needs to be (e.g., 1 or 10)

    private Dictionary<Vector2Int, GameObject> gridObjects = new Dictionary<Vector2Int, GameObject>();
    private Dictionary<Vector2Int, GameObject> activeChunks = new Dictionary<Vector2Int, GameObject>();

    private void Awake()
    {
        Instance = this;

        EnsureChunkExists(new Vector2Int(0, 0));  
        EnsureChunkExists(new Vector2Int(-1, 0)); 
        EnsureChunkExists(new Vector2Int(0, -1)); 
        EnsureChunkExists(new Vector2Int(-1, -1)); 
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
        }
    }

    private Vector2Int GridToChunkPosition(Vector2Int gridPos)
    {
        int chunkX = Mathf.FloorToInt((float)gridPos.x / chunkSize);
        int chunkY = Mathf.FloorToInt((float)gridPos.y / chunkSize);
        return new Vector2Int(chunkX, chunkY);
    }

    public void EnsureChunkExists(Vector2Int chunkPos)
    {
        if (!activeChunks.ContainsKey(chunkPos))
        {
            Vector3 worldPos = new Vector3(
                (chunkPos.x * chunkSize + chunkSize / 2f) * cellSize,
                0f,
                (chunkPos.y * chunkSize + chunkSize / 2f) * cellSize
            );

            GameObject newChunk = Instantiate(groundChunkPrefab, worldPos, Quaternion.identity, transform);
            newChunk.name = $"GroundChunk_{chunkPos.x}_{chunkPos.y}";
            
            // Uses independent visual scale so your plane won't shrink to nothing
            newChunk.transform.localScale = new Vector3(chunkVisualScale, 1f, chunkVisualScale);

            activeChunks[chunkPos] = newChunk;
        }
    }

    public void CheckAndCleanupUnusedChunks()
    {
        HashSet<Vector2Int> requiredChunks = new HashSet<Vector2Int>
        {
            new Vector2Int(0, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, -1),
            new Vector2Int(-1, -1)
        };

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