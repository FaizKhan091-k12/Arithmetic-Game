using UnityEngine;
using UnityEngine.EventSystems;

public class TimesTableClickableCell : MonoBehaviour,
    IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private TimesTableGridPopulator manager;
    private int row, col;
    private bool disabled;
    private RectTransform rt;

    public void Init(TimesTableGridPopulator mgr, int r, int c, bool isDisabled)
    {
        manager = mgr;
        row = r;
        col = c;
        disabled = isDisabled;
        rt = transform as RectTransform;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (disabled || manager == null) return;

        bool staysOpen = manager.OnCellClicked(row, col, rt);
        if (staysOpen) disabled = true;

        if (TimesTableGridPopulator.Instance?.clickSound) 
            TimesTableGridPopulator.Instance.clickSound.Play();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (disabled || manager == null) return;
        // ignore headers
        if (row == 0 || col == 0) return;
        manager.OnHoverEnter(row, col);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (manager == null) return;
        if (row == 0 || col == 0) return;
        manager.OnHoverExit(row, col);
    }
}
