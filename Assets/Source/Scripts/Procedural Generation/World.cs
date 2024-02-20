using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class World : MonoBehaviour
{
    // Переменные для определения свойств мира и чанков
    public int mapSizeInChunks = 6;  // Количество чанков в мире
    public int chunkSize = 16;  // Размер каждого чанка
    public int chunkHeight = 100;  // Высота каждого чанка
    public int chunkDrawingRange = 8;  // Радиус вокруг игрока для отрисовки чанков

    // Префабы и менеджеры
    public GameObject chunkPrefab;  // Префаб для игрового объекта чанка
    public WorldRenderer worldRenderer;  // Рендерер мира
    public BlockDataManager blockDataManager;  // Менеджер для данных блоков
    public TerrainGenerator terrainGenerator;  // Логика генерации местности
    public Vector2Int mapSeedOffset;  // Смещение для сид мира

    // Токен отмены для асинхронных задач
    CancellationTokenSource taskTokenSource = new CancellationTokenSource();

    // События, вызываемые при создании мира и генерации новых чанков
    public UnityEvent OnWorldCreated, OnNewChunksGenerated;

    public bool isSpawn;

    // Словарь для хранения данных сеток чанков
    ConcurrentDictionary<Vector3Int, MeshData> meshDataDictionary = new ConcurrentDictionary<Vector3Int, MeshData>();

    // Свойства для доступа к данным мира
    public WorldData worldData { get; private set; }  // Данные, представляющие мир
    public bool IsWorldCreated { get; private set; }  // Флаг, указывающий, создан ли мир

    // Логика инициализации
    private void Awake()
    {
        // Создание экземпляра данных мира
        worldData = new WorldData
        {
            chunkHeight = this.chunkHeight,
            chunkSize = this.chunkSize,
            chunkDataDictionary = new Dictionary<Vector3Int, ChunkData>(),
            chunkDictionary = new Dictionary<Vector3Int, ChunkRenderer>()
        };

        // Инициализация менеджера данных блоков
        blockDataManager.Init();
    }

    // Метод для генерации мира
    [ContextMenu("GenerateWorld")]
    public async void GenerateWorld()
    {
        await GenerateWorld(Vector3Int.zero);
    }

    // Асинхронный метод для генерации мира
    private async Task GenerateWorld(Vector3Int position)
    {
        terrainGenerator.GenerateBiomePoints(position, chunkDrawingRange, chunkSize, mapSeedOffset);

        // Вычисление данных генерации мира в отдельной задаче
        WorldGenerationData worldGenerationData = await Task.Run(() => GetPositionsThatPlayerSees(position), taskTokenSource.Token);

        // Удаление ненужных чанков и данных чанков
        foreach (Vector3Int pos in worldGenerationData.chunkPositionsToRemove)
            WorldDataHelper.RemoveChunk(this, pos);

        foreach (Vector3Int pos in worldGenerationData.chunkDataToRemove)
            WorldDataHelper.RemoveChunkData(this, pos);

        ConcurrentDictionary<Vector3Int, ChunkData> dataDictionary = null;

        try
        {
            // Вычисление данных чанков асинхронно
            dataDictionary = await CalculateWorldChunkData(worldGenerationData.chunkDataPositionsToCreate);
        }
        catch (Exception)
        {
            Debug.Log("Задача отменена");
            return;
        }

        // Добавление вычисленных данных чанков в данные мира
        foreach (var calculatedData in dataDictionary)
            worldData.chunkDataDictionary.Add(calculatedData.Key, calculatedData.Value);

        // Добавление блоков деревьев к данным чанков
        foreach (var chunkData in worldData.chunkDataDictionary.Values)
            AddTreeLeafs(chunkData);

        // Получение данных для отрисовки чанков
        List<ChunkData> dataToRender = worldData.chunkDataDictionary
            .Where(keyvaluepair => worldGenerationData.chunkPositionsToCreate.Contains(keyvaluepair.Key))
            .Select(keyvalpair => keyvalpair.Value)
            .ToList();

        ConcurrentDictionary<Vector3Int, MeshData> meshDataDictionary = new ConcurrentDictionary<Vector3Int, MeshData>();

        try
        {
            // Генерация данных сеток асинхронно
            meshDataDictionary = await CreateMeshDataAsync(dataToRender);
        }
        catch (Exception)
        {
            Debug.Log("Задача отменена");
            return;
        }

        // Запуск корутины для создания чанков
        StartCoroutine(ChunkCreationCoroutine(meshDataDictionary));
    }

    // Метод для добавления блоков деревьев к данным чанков
    private void AddTreeLeafs(ChunkData chunkData)
    {
        foreach (var treeLeafes in chunkData.treeData.treeLeafesSolid)
            Chunk.SetBlock(chunkData, treeLeafes, BlockType.TreeLeafsSolid);
    }

    // Асинхронный метод для создания данных сеток для отрисовки чанков
    private Task<ConcurrentDictionary<Vector3Int, MeshData>> CreateMeshDataAsync(List<ChunkData> dataToRender)
    {
        ConcurrentDictionary<Vector3Int, MeshData> dictionary = new ConcurrentDictionary<Vector3Int, MeshData>();

        return Task.Run(() =>
        {
            foreach (ChunkData data in dataToRender)
            {
                if (taskTokenSource.Token.IsCancellationRequested)
                    taskTokenSource.Token.ThrowIfCancellationRequested();

                MeshData meshData = Chunk.GetChunkMeshData(data);
                dictionary.TryAdd(data.worldPosition, meshData);
            }

            return dictionary;
        }, taskTokenSource.Token);
    }

    // Асинхронный метод для вычисления данных чанков мира
    private Task<ConcurrentDictionary<Vector3Int, ChunkData>> CalculateWorldChunkData(List<Vector3Int> chunkDataPositionsToCreate)
    {
        ConcurrentDictionary<Vector3Int, ChunkData> dictionary = new ConcurrentDictionary<Vector3Int, ChunkData>();

        return Task.Run(() =>
        {
            foreach (Vector3Int pos in chunkDataPositionsToCreate)
            {
                if (taskTokenSource.Token.IsCancellationRequested)
                    taskTokenSource.Token.ThrowIfCancellationRequested();

                ChunkData data = new ChunkData(chunkSize, chunkHeight, this, pos);
                ChunkData newData = terrainGenerator.GenerateChunkData(data, mapSeedOffset);
                dictionary.TryAdd(pos, newData);
            }
            return dictionary;
        },
        taskTokenSource.Token);
    }

    IEnumerator ChunkCreationCoroutine(ConcurrentDictionary<Vector3Int, MeshData> meshDataDictionary)
    {
        foreach (var item in meshDataDictionary)
        {
            CreateChunk(worldData, item.Key, item.Value);
            yield return new WaitForEndOfFrame();
        }

        if (IsWorldCreated == false)
        {
            IsWorldCreated = true;
            OnWorldCreated?.Invoke();
        }
    }

    private void CreateChunk(WorldData worldData, Vector3Int position, MeshData meshData)
    {
        ChunkRenderer chunkRenderer = worldRenderer.RenderChunk(worldData, position, meshData);
        worldData.chunkDictionary.Add(position, chunkRenderer);
    }

    internal bool SetBlock(RaycastHit hit, BlockType blockType)
    {
        ChunkRenderer chunk = hit.collider.GetComponent<ChunkRenderer>();

        if (chunk == null)
        {
            Debug.Log("Chunk NULL");
            return false;
        }

        Vector3Int pos = GetBlockPos(hit);

        WorldDataHelper.SetBlock(chunk.ChunkData.worldReference, pos, blockType);
        chunk.ModifiedByThePlayer = true;
        if (Chunk.IsOnEdge(chunk.ChunkData, pos))
        {
            List<ChunkData> neighbourDataList = Chunk.GetEdgeNeighbourChunk(chunk.ChunkData, pos);
            foreach (ChunkData neighbourData in neighbourDataList)
            {
                //neighbourData.modifiedByThePlayer = true;
                ChunkRenderer chunkToUpdate = WorldDataHelper.GetChunk(neighbourData.worldReference, neighbourData.worldPosition);
                if (chunkToUpdate != null)
                    chunkToUpdate.UpdateChunk();
            }
        }

        chunk.UpdateChunk();
        return true;
    }

    internal bool SetBlock(RaycastHit hit, BlockType blockType, ChunkRenderer OtherChunkRenderer)
    {
        ChunkRenderer chunk = hit.collider.GetComponent<ChunkRenderer>();

        if (chunk == null)
        {
            Debug.Log("Chunk NULL");
            return false;
        }

        Vector3Int pos = GetBlockPos(hit);

        WorldDataHelper.SetBlock(chunk.ChunkData.worldReference, pos, blockType);
        chunk.ModifiedByThePlayer = true;
        chunk.UpdateChunk();
        OtherChunkRenderer.UpdateChunk();
        return true;
    }

    private Vector3Int GetBlockPos(RaycastHit hit)
    {
        Vector3 pos = new Vector3(
             GetBlockPositionIn(hit.point.x, hit.normal.x),
             GetBlockPositionIn(hit.point.y, hit.normal.y),
             GetBlockPositionIn(hit.point.z, hit.normal.z)
             );

        return Vector3Int.RoundToInt(pos);
    }

    private float GetBlockPositionIn(float pos, float normal)
    {
        if (isSpawn)
            pos += (normal / 2);
        else
            pos -= (normal / 2);

        return (float)pos;
    }

    private WorldGenerationData GetPositionsThatPlayerSees(Vector3Int playerPosition)
    {
        List<Vector3Int> allChunkPositionsNeeded = WorldDataHelper.GetChunkPositionsAroundPlayer(this, playerPosition);

        List<Vector3Int> allChunkDataPositionsNeeded = WorldDataHelper.GetDataPositionsAroundPlayer(this, playerPosition);

        List<Vector3Int> chunkPositionsToCreate = WorldDataHelper.SelectPositonsToCreate(worldData, allChunkPositionsNeeded, playerPosition);
        List<Vector3Int> chunkDataPositionsToCreate = WorldDataHelper.SelectDataPositonsToCreate(worldData, allChunkDataPositionsNeeded, playerPosition);

        List<Vector3Int> chunkPositionsToRemove = WorldDataHelper.GetUnnededChunks(worldData, allChunkPositionsNeeded);
        List<Vector3Int> chunkDataToRemove = WorldDataHelper.GetUnnededData(worldData, allChunkDataPositionsNeeded);

        WorldGenerationData data = new WorldGenerationData
        {
            chunkPositionsToCreate = chunkPositionsToCreate,
            chunkDataPositionsToCreate = chunkDataPositionsToCreate,
            chunkPositionsToRemove = chunkPositionsToRemove,
            chunkDataToRemove = chunkDataToRemove,
            chunkPositionsToUpdate = new List<Vector3Int>()
        };

        return data;
    }

    internal async void LoadAdditionalChunksRequest(Character player)
    {
        Debug.Log("Load more chunks");
        await GenerateWorld(Vector3Int.RoundToInt(player.transform.position));
        OnNewChunksGenerated?.Invoke();
    }

    internal BlockType GetBlockFromChunkCoordinates(ChunkData chunkData, int x, int y, int z)
    {
        Vector3Int pos = Chunk.ChunkPositionFromBlockCoords(this, x, y, z);
        ChunkData containerChunk = null;

        worldData.chunkDataDictionary.TryGetValue(pos, out containerChunk);

        if (containerChunk == null)
            return BlockType.Nothing;

        Vector3Int blockInCHunkCoordinates = Chunk.GetBlockInChunkCoordinates(containerChunk, new Vector3Int(x, y, z));
        return Chunk.GetBlockFromChunkCoordinates(containerChunk, blockInCHunkCoordinates);
    }

    public void OnDisable()
    {
        taskTokenSource.Cancel();
    }

    public struct WorldGenerationData
    {
        public List<Vector3Int> chunkPositionsToCreate;
        public List<Vector3Int> chunkDataPositionsToCreate;
        public List<Vector3Int> chunkPositionsToRemove;
        public List<Vector3Int> chunkDataToRemove;
        public List<Vector3Int> chunkPositionsToUpdate;
    }
}
public struct WorldData
{
    public Dictionary<Vector3Int, ChunkData> chunkDataDictionary;
    public Dictionary<Vector3Int, ChunkRenderer> chunkDictionary;
    public int chunkSize;
    public int chunkHeight;
}