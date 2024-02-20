using System.Collections;
using UnityEngine;

public class Creator : MonoBehaviour
{
    [ContextMenu("Start Test")]
    void Test()
    {
        StartCoroutine(Corutine());
    }

    IEnumerator Corutine()
    {
        yield return new WaitForSeconds(2f);
        Debug.Log("+");
    }
}