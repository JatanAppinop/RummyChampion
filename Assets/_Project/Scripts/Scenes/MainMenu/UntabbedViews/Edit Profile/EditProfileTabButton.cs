using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EditProfileTabButton : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private EditProfileTabGroup tabBar;

    [Header("Edit Profile Specifics")]
    [SerializeField] Image icon;
    [SerializeField] TextMeshProUGUI label;
    [SerializeField] Color activeColor;
    [SerializeField] Color inactiveColor;

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
            icon.color = activeColor;
        }
    }

    public void DeSelect()
    {
        if (isSelected)
        {
            isSelected = false;
            label.color = inactiveColor;
            icon.color = inactiveColor;
        }
    }
}