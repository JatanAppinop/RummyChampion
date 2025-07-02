using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class ContestsTabGroup : MonoBehaviour
{
    [Header("Tab Buttons")]
    [SerializeField] List<ContestsTabButton> tabs;

    public List<ContestsTabButton> Tabs => tabs;


    [Header("Tab Views")]
    [SerializeField] List<AnimatedView> pages;

    [Header("Marker")]
    [SerializeField] RectTransform marker;
    //-Private Variables
    [SerializeField] ContestsTabButton selectedTab;
    public ContestsTabButton GetSelectedTab => selectedTab;
    private TabTransitionManager transitionManager;

    private void Start()
    {

        selectedTab = tabs.First();

        transitionManager = new TabTransitionManager(pages, 0);
        selectedTab.Select(false);

        foreach (var tab in tabs)
        {
            if (tab != selectedTab)
            {
                tab.DeSelect();
            }
        }

        for (int i = 1; i < pages.Count(); i++)
        {
            pages[i].rectTransform.gameObject.SetActive(false);
        }

        // //Change Market Width
        // RectTransform marketParentRect = marker.parent as RectTransform;
        // float newWidth = (marketParentRect.rect.width / 2) - 10;
        // Vector2 newSizeDelta = new Vector2(newWidth, marker.sizeDelta.y);
        // marker.sizeDelta = newSizeDelta;

        MoveMarker(selectedTab, false);
    }
    public void onTabSelected(ContestsTabButton button, bool forced = false)
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

    public void ResetTab()
    {
        selectedTab = tabs.First();
        DeSelectAllButtons();
        MoveMarker(selectedTab, false);
        selectedTab.Select();
        transitionManager.MoveToView(pages[tabs.IndexOf(selectedTab)].ViewName);

    }

    private void MoveMarker(ContestsTabButton button, bool animate = true)
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