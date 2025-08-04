using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIButtonStates : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    public void OnPointerEnter(PointerEventData eventData)
    {
       Debug.Log("Mouse is over Play Button");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("Mouse is over Play Button");
    }
}
