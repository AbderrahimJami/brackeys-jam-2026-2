using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField]
    Vector3 offset = Vector3.zero;
    Transform toFollow;

    Transform parent;
    void Start()
    {
        toFollow = FindObjectsByType<PlayerController>()[0].transform.Find("CameraPosition");
        parent = transform.parent;
    }

    void Update(){
        parent.transform.position = toFollow.position + offset;
    }
}
