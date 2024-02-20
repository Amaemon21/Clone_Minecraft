using UnityEngine;

public class Character : MonoCache
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float maxDistance = 5;
    [SerializeField] private float minDistance = 5;
    [SerializeField] private LayerMask groundMask;

    [SerializeField] private World _world;
    private float rayDistance = 5f;
    private bool isDestroing;

    private Ray _ray;
    private RaycastHit _hit;

    public ChunkRenderer ChunkRenderer { get; private set; }

    public void Init(World world)
    {
        _world = world;
    }

    private void Update()
    {
        _ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));

        if (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Mouse1))
        {
            isDestroing = Input.GetKeyDown(KeyCode.Mouse0);
            _world.isSpawn = !isDestroing;

            if (Physics.Raycast(_ray, out _hit, maxDistance, groundMask))
            {
                if (isDestroing)
                {
                    _world.SetBlock(_hit, BlockType.Air);
                }
                else
                {
                    if (_hit.distance >= minDistance)
                    {
                        if (CheckChunkRenderer())
                        {
                            _world.SetBlock(_hit, BlockType.Stone, ChunkRenderer);
                        }
                    }
                }
            }
        }
    }

    public void SpawnBlock()
    {
        isDestroing = false;
        _world.isSpawn = !isDestroing;

        Ray _ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        RaycastHit _hit;

        if (Physics.Raycast(_ray, out _hit, maxDistance, groundMask))
        {
            if (_hit.distance >= minDistance) 
            { 
                if (CheckChunkRenderer())
                {
                    _world.SetBlock(_hit, BlockType.Stone, ChunkRenderer);
                }
            }
        }
    }

    public void DestroyBlock()
    {
        isDestroing = true;
        _world.isSpawn = !isDestroing;

        Ray _ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        RaycastHit _hit;

        if (Physics.Raycast(_ray, out _hit, maxDistance, groundMask))
        {
            _world.SetBlock(_hit, BlockType.Air);
        }
    }

    private bool CheckChunkRenderer()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, Vector3.down, out hit, rayDistance, groundMask))
        {
            var hitCollider = hit.collider;

            if (hitCollider.TryGetComponent<ChunkRenderer>(out ChunkRenderer chunkRenderer))
            {
                ChunkRenderer = chunkRenderer;
                return true;
            }
        }

        return false;
    }
}   