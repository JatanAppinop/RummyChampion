using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class LeaderboardTabGroup : MonoBehaviour
{
    [Header("Setup")]
    [Range(1, 3)]
    public int startingPage = 1;

    [Header("Tab Buttons")]
    [SerializeField] List<LeaderboardTabButton> tabs;


    [Header("Tab Views")]
    [SerializeField] List<AnimatedView> pages;

    [Header("Marker")]
    [SerializeField] RectTransform marker;
    //-Private Variables
    [SerializeField] LeaderboardTabButton selectedTab;
    public LeaderboardTabButton GetSelectedTab => selectedTab;
    private TabTransitionManager transitionManager;

    IEnumerator Start()
    {

        selectedTab = tabs[startingPage - 1];
        transitionManager = new TabTransitionManager(pages, startingPage - 1);
        selectedTab.Select(false);
        yield return null;
    }
    public void onTabSelected(LeaderboardTabButton button, bool forced = false)
    {
        if (button != selectedTab || forced)
        {
            selectedTab = button;
            DeSelectAllButtons();
            MoveMarker(button);
            button.Select();
            transitionManager.MoveToView(pages[tabs.IndexOf(selectedTab)].ViewName);
        }

    }

    private void MoveMarker(LeaderboardTabButton button, bool animate = true)
    {
        RectTransform buttonRect = button.transform as RectTransform;
        var localSpacePos = marker.InverseTransformVector(buttonRect.position);
        if (animate)
            marker.DOAnchorPosX(localSpacePos.x, 0.2f).SetEase(Ease.OutBack);
        else
            marker.anchoredPosition = new Vector2(localSpacePos.x, marker.anchoredPosition.y);
    }

    private void DeSelectAllButtons()
    {
        foreach (var TabButton in tabs)
        {
            if (TabButton != selectedTab)
            {
                TabButton.DeSelect();
            }
        }
    }
}