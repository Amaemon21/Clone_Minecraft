using System.Linq;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class ChunkRenderer : MonoBehaviour
{
    MeshFilter meshFilter; // Фильтр меша
    MeshCollider meshCollider; // Коллайдер меша
    Mesh mesh; // Меш
    public bool showGizmo = false; // Показывать ли гизмо

    public ChunkData ChunkData { get; private set; } // Данные чанка

    // Флаг, указывающий, изменен ли чанк игроком
    public bool ModifiedByThePlayer
    {
        get
        {
            return ChunkData.modifiedByThePlayer;
        }
        set
        {
            ChunkData.modifiedByThePlayer = value;
        }
    }

    // Инициализация при запуске
    public void Awake()
    {
        Init();
    }

    // Инициализация компонентов
    public void Init()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();
        mesh = meshFilter.mesh;
    }

    // Инициализация чанка
    public void InitializeChunk(ChunkData data)
    {
        this.ChunkData = data;
    }

    // Рендеринг меша
    private void RenderMesh(MeshData meshData)
    {
        mesh.Clear();

        mesh.subMeshCount = 2;
        mesh.vertices = meshData.vertices.Concat(meshData.waterMesh.vertices).ToArray();

        mesh.SetTriangles(meshData.triangles.ToArray(), 0);
        mesh.SetTriangles(meshData.waterMesh.triangles.Select(val => val + meshData.vertices.Count).ToArray(), 1);

        mesh.uv = meshData.uv.Concat(meshData.waterMesh.uv).ToArray();
        mesh.Optimize();
        mesh.RecalculateNormals();

        meshCollider.sharedMesh = null;
        Mesh collisionMesh = new Mesh();
        collisionMesh.vertices = meshData.colliderVertices.ToArray();
        collisionMesh.triangles = meshData.colliderTriangles.ToArray();
        collisionMesh.Optimize();
        collisionMesh.RecalculateNormals();

        meshCollider.sharedMesh = collisionMesh;

        GetComponent<MeshCollider>().sharedMesh = collisionMesh;
    }

    // Обновление чанка
    [ContextMenu("Test")]
    public void UpdateChunk()
    {
        RenderMesh(Chunk.GetChunkMeshData(ChunkData));
    }

    // Обновление чанка с заданными данными меша
    public void UpdateChunk(MeshData data)
    {
        RenderMesh(data);
    }

#if UNITY_EDITOR
    // Отрисовка гизмо в редакторе
    private void OnDrawGizmos()
    {
        if (showGizmo)
        {
            if (Application.isPlaying && ChunkData != null)
            {
                if (Selection.activeObject == gameObject)
                    Gizmos.color = new Color(0, 1, 0, 0.4f);
                else
                    Gizmos.color = new Color(1, 0, 1, 0.4f);

                Gizmos.DrawCube(transform.position + new Vector3(ChunkData.chunkSize / 2f, ChunkData.chunkHeight / 2f, ChunkData.chunkSize / 2f), new Vector3(ChunkData.chunkSize, ChunkData.chunkHeight, ChunkData.chunkSize));
            }
        }
    }
#endif
}