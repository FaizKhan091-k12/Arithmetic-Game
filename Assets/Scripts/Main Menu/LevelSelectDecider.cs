using UnityEngine;
using UnityEngine.UI;

public class LevelSelectDecider : MonoBehaviour
{
    public static LevelSelectDecider Instance;

    public enum ArithmeticLevel
    {
        AdditionLevel,
        SubtractionLevel,

        MultiplyLevel,

        DivisionLevel

    }

    public ArithmeticLevel arithmeticLevel;
    Button btn;
    public MainMenuBehaviour mainMenu;

    void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(() => LevelSelected());
     
    }

    public void LevelSelected()
    {
        mainMenu.OnLevelSelector(arithmeticLevel);
    }

   


}
