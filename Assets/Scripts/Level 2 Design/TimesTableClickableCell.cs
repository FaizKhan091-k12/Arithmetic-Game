using UnityEngine;
using UnityEngine.EventSystems;

/// Attach automatically by the manager. For UI cells only.
public class TimesTableClickableCell : MonoBehaviour, IPointerClickHandler
{
    private TimesTableGridPopulator manager;
    private int row, col;
    private bool disabled;

 
    public void Init(TimesTableGridPopulator mgr, int r, int c, bool isDisabled)
    {
        manager = mgr;
        row = r;
        col = c;
        disabled = isDisabled;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (disabled || manager == null) return;

        // If reveal succeeds, disable further clicks for this cell
        if (manager.RevealCell(row, col))
        {
            disabled = true;
            TimesTableGridPopulator.Instance.clickSound.Play();
        }
    }
}
