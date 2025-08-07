using UnityEngine;

public class Cloud : MonoBehaviour
{
    [SerializeField] float minSpeed = 1f;
    [SerializeField] float maxSpeed = 5f;

    float speed;

    void Start()
    {
        speed = Random.Range(minSpeed, maxSpeed);
    }

    void Update()
    {
        transform.Translate(Vector3.left * speed * Time.deltaTime);
    }
}
