using UnityEngine;
using UnityEngine.UI;

public class LevelNumberSelector : MonoBehaviour
{
    public static LevelNumberSelector Instance;
    [SerializeField] public int multiLevelNum;
    [SerializeField] public int addLevelNum;
    [SerializeField] public int subtractLevelNum;
    [SerializeField] public int divisionLevelNum;

   public int commonLevelNum;


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
    public void MultiWhichLevelIsSelected(int level)
    {

        multiLevelNum = level;
        commonLevelNum = level;
    }
    public void AddWhichLevelIsSelected(int level)
    {

        addLevelNum = level;
        commonLevelNum = level;
    }



    public void SubtractWhichLevelIsSelected(int level)
    {

        subtractLevelNum = level;
        commonLevelNum = level;
    }

    public void DivisionWhichLevelIsSelected(int level)
    {

        divisionLevelNum = level;
        commonLevelNum = level;
    }


}
