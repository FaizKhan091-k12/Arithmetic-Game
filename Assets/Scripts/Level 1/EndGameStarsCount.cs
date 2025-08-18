using UnityEngine;

public class EndGameStarsCount : MonoBehaviour
{

    public int stars_Count;

    void Update()
    {
        stars_Count = LevelDesigner.Instance.stars;        
    }
}
