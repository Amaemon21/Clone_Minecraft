using UnityEngine;

public class Character : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float maxDistance = 5;
    [SerializeField] private float minDistance = 5;
    [SerializeField] private LayerMask groundMask;

    private World world;

    [System.Obsolete]
    private void Awake()
    {
        world = FindObjectOfType<World>();
    }

    private void Update()
    {
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        RaycastHit hit;

        if (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Mouse1))
        {
            bool isDestroing = Input.GetKeyDown(KeyCode.Mouse0);

            if (isDestroing)
            {
                if (Physics.Raycast(ray, out hit, maxDistance, groundMask))
                {
                    world.isSpawn = false;
                    world.SetBlock(hit, BlockType.Air);
                }    
            }
            else
            {
                if (Physics.Raycast(ray, out hit, maxDistance, groundMask))
                {
                    if (hit.distance >= minDistance)
                    {
                        world.isSpawn = true;
                        world.SetBlock(hit, BlockType.Sand);
                    }
                }
            }
        }
    }
}   