using Ricimi;
using UnityEngine;
using UnityEngine.UI;

public class HowManyStars : MonoBehaviour
{
    [SerializeField] GameObject yellow_Star1, yellow_Star2, yellow_Star3;
    [SerializeField] GameObject gray_star1, gray_star2, gray_start3;

    [SerializeField] Button startLevelBtn;
    [SerializeField] int stars_Count;
    

    void OnEnable()
    {
        StarsProvider();
    }

    void StarsProvider()
    {
        if (yellow_Star1.activeInHierarchy)
        {
            stars_Count = 1;
        }
         if (yellow_Star1.activeInHierarchy && yellow_Star2.activeInHierarchy)
        {
            stars_Count = 2;
        }
         if (yellow_Star1.activeInHierarchy && yellow_Star2.activeInHierarchy && yellow_Star3.activeInHierarchy)
        {
            stars_Count = 3;
        }

        startLevelBtn.GetComponent<PlayPopupOpener>().starsObtained = stars_Count;

    }

}
