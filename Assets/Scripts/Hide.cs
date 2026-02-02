using UnityEngine;
using System.Collections;

public class Hide : MonoBehaviour
{
    public float waitTime = 2f;
    void Start()
    {
        StartCoroutine(HideObject());
    }

    IEnumerator HideObject()
    {
        yield return new WaitForSeconds(waitTime);
        gameObject.SetActive(false);
    }
}
