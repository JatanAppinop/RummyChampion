using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TabGroup : MonoBehaviour
{
    [Header("Setup")]
    [Range(1, 5)]
    public int startingPage = 3;

    [Header("Tab Buttons")]
    [SerializeField] List<TabButton> tabs;
    public List<TabButton> Tabs => tabs;


    [Header("Tab Views")]
    [SerializeField] List<AnimatedView> pages;

    [Header("Marker")]
    [SerializeField] RectTransform marker;
    //-Private Variables
    [SerializeField] TabButton selectedTab;
    public TabButton GetSelectedTab => selectedTab;
    private TabTransitionManager transitionManager;

    IEnumerator Start()
    {

        selectedTab = tabs[startingPage - 1];
        transitionManager = new TabTransitionManager(pages, startingPage - 1);
        selectedTab.Select(false);
        yield return null;
        MoveMarker(selectedTab, false);
    }
    public void onTabSelected(TabButton button, bool forced = false)
    {
        if (button != selectedTab || forced)
        {
            selectedTab = button;
            DeSelectAllButtons();
            MoveMarker(button);
            button.Select();
            transitionManager.MoveToView(tabs.IndexOf(selectedTab).ToString()).OnComplete(() =>
            {
                RectTransform page = pages[tabs.IndexOf(selectedTab)].rectTransform;
                if (page.GetComponent<PageController>())
                {
                    page.GetComponent<PageController>().OnShown();

                }
            });
        }

    }

    public void NavigateTo(string tabName)
    {
        onTabSelected(tabs.Find(t => t.gameObject.name == tabName));
    }
    private void MoveMarker(TabButton button, bool animate = true)
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