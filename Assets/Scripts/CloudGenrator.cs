
using UnityEngine;

public class CloudGenrator : MonoBehaviour
{
    [SerializeField] GameObject[] clouds;
    [SerializeField] GameObject[] parentCloud;

    [SerializeField] GameObject destroyer;



    void Start()
    {
        InvokeRepeating(nameof(CloudGenerator), 2, 5);
    }


    public void CloudGenerator()
    {
        int randomCloud = UnityEngine.Random.Range(0, clouds.Length - 1);
        int randomParent = UnityEngine.Random.Range(0, parentCloud.Length - 1);

        GameObject cloud = Instantiate(clouds[randomCloud], parentCloud[randomParent].transform);
        

        
        
        

    }

}
