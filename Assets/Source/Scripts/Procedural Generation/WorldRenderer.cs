using System.Collections.Generic;
using UnityEngine;

public class WorldRenderer : MonoBehaviour
{
    public ChunkRenderer chunkPrefab;
    [SerializeField] private Transform _parent;
    public Queue<ChunkRenderer> chunkPool = new Queue<ChunkRenderer>();

    ChunkRenderer newChunk;

    public void Clear(WorldData worldData)
    {
        foreach (var item in worldData.chunkDictionary.Values)
        {
            Destroy(item.gameObject);
        }
        chunkPool.Clear();
    }

    public void Init(WorldData worldData, Vector3Int position)
    {
        newChunk.InitializeChunk(worldData.chunkDataDictionary[position]);
    }

    internal ChunkRenderer RenderChunk(WorldData worldData, Vector3Int position, MeshData meshData)
    {
        newChunk = null;

        if (chunkPool.Count > 0)
        {
            newChunk = chunkPool.Dequeue();
            newChunk.transform.position = position;
        }
        else
        {
            newChunk = Instantiate(chunkPrefab, position, Quaternion.identity, _parent);
            newChunk.Init();
        }

        newChunk.InitializeChunk(worldData.chunkDataDictionary[position]);
        newChunk.UpdateChunk(meshData);
        newChunk.gameObject.SetActive(true);
        return newChunk;
    }

    public void RemoveChunk(ChunkRenderer chunk)
    {
        chunk.gameObject.SetActive(false);
        chunkPool.Enqueue(chunk);
    }
}