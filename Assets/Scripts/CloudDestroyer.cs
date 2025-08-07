using UnityEngine;
using UnityEngine.Assertions.Must;

public class CloudDestroyer : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        Destroy(other.gameObject);
    }
}
