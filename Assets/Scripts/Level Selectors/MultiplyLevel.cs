using UnityEngine;
using UnityEngine.UI;

public class MultiplyLevel : MonoBehaviour
{
    public static MultiplyLevel Instance;
    [SerializeField] public int levelNum;


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void WhichLevelIsSelected(int level)
    {

        levelNum = level;
    }





}
