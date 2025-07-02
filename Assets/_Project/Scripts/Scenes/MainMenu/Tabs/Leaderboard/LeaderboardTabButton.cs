using System.Collections;
using System.Collections.Generic;
using LottiePlugin.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LeaderboardTabButton : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private LeaderboardTabGroup tabBar;

    [Header("Leaderboard Specifics")]
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
            label.fontWeight = FontWeight.SemiBold;
        }
    }

    public void DeSelect()
    {
        if (isSelected)
        {
            isSelected = false;
            label.fontWeight = FontWeight.Medium;
            label.color = inactiveColor;
        }
    }
}
