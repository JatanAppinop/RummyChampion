using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class EditProfileTabGroup : MonoBehaviour
{
    [Header("Tab Buttons")]
    [SerializeField] List<EditProfileTabButton> tabs;

    public List<EditProfileTabButton> Tabs => tabs;


    [Header("Tab Views")]
    [SerializeField] List<AnimatedView> pages;

    [Header("Marker")]
    [SerializeField] RectTransform marker;
    //-Private Variables
    [SerializeField] EditProfileTabButton selectedTab;
    public EditProfileTabButton GetSelectedTab => selectedTab;
    private TabTransitionManager transitionManager;

    private void Start()
    {

        selectedTab = tabs[0];
        transitionManager = new TabTransitionManager(pages, 0);
        selectedTab.Select(false);

        foreach (var tab in tabs)
        {
            if (tab != selectedTab)
            {
                tab.DeSelect();
            }
        }

        //Change Market Width
        RectTransform marketParentRect = marker.parent as RectTransform;
        float newWidth = (marketParentRect.rect.width / 2) - 10;
        Vector2 newSizeDelta = new Vector2(newWidth, marker.sizeDelta.y);
        marker.sizeDelta = newSizeDelta;

    }
    public void onTabSelected(EditProfileTabButton button, bool forced = false)
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
        selectedTab.Select();
        transitionManager.MoveToView(pages[tabs.IndexOf(selectedTab)].ViewName);
        MoveMarker(selectedTab, false);

    }

    private void MoveMarker(EditProfileTabButton button, bool animate = true)
    {
        RectTransform buttonRect = button.transform as RectTransform;
        var localSpacePos = marker.InverseTransformVector(buttonRect.position);
        if (animate)
            marker.DOAnchorPosX(localSpacePos.x, 0.2f).SetEase(Ease.OutBack);
        else
            marker.position = buttonRect.position;
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