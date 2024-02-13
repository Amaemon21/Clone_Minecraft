using UnityEngine;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

public class RaycastFromCenter : MonoBehaviour
{
    public float maxDistance = 5;
    public LayerMask layerMask;
    public Vector3 startOffset = Vector3.zero;
    public Vector3 endOffset = Vector3.forward;
    private Camera mainCamera;
    public Vector3 test;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        RaycastHit hit;
        Vector3 startPoint = mainCamera.transform.position + startOffset;
        Vector3 endPoint = mainCamera.transform.position + endOffset * maxDistance;

        Ray ray = mainCamera.ViewportPointToRay(test);

        if (Physics.Raycast(startPoint, endOffset, out hit, maxDistance, layerMask))
        {
            Debug.DrawLine(startPoint, hit.point, Color.green);
        }
        else
        {
            Debug.DrawRay(startPoint, endOffset * maxDistance, Color.red);
        }
    }
}
