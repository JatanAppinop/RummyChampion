using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ContestsTabButton : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private ContestsTabGroup tabBar;

    [Header("Edit Profile Specifics")]
    [SerializeField] TextMeshProUGUI label;
    [SerializeField] Color activeColor;
    [SerializeField] Color inactiveColor;

    [field: Header("Data")]
    [field: SerializeField] public Appinop.Constants.PlayerCounts playerCounts { get; private set; }

    private bool isSelected;

    private void Awake()
    {
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        tabBar.onTabSelected(this);
    }
    public void Select(bool animate = true)
    {
        if (!isSelected)
        {
            isSelected = true;
            label.color = activeColor;
        }
    }

    public void DeSelect()
    {
        if (isSelected)
        {
            isSelected = false;
            label.color = inactiveColor;
        }
    }
}