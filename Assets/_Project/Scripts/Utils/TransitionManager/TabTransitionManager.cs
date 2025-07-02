using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

class TabTransitionManager
{
    private List<AnimatedView> _animatedViews;

    private int currentViewIndex = 0;

    private Action _onComplete;

    private float _duration = 0.2f;

    public TabTransitionManager(List<AnimatedView> animatedViews, int defaultView = 0)
    {
        _animatedViews = animatedViews;
        InitializeViews(defaultView);
    }

    private void InitializeViews(int defaultView)
    {
        currentViewIndex = defaultView;
        for (int i = 0; i < _animatedViews.Count; i++)
        {
            if (i == defaultView)
            {

                _animatedViews[i].rectTransform.gameObject.SetActive(true);
            }
            else
            {
                // _animatedViews[i].rectTransform.anchoredPosition = new Vector2(Screen.width, 0);
                _animatedViews[i].rectTransform.gameObject.SetActive(false);
            }

        }
    }

    public TabTransitionManager MoveToView(string viewName)
    {
        int targetIndex = _animatedViews.FindIndex(v => v.ViewName.Equals(viewName, System.StringComparison.OrdinalIgnoreCase));
        if (targetIndex == -1 || targetIndex == currentViewIndex) return this;


        if (targetIndex > currentViewIndex)
        {
            MoveOutCurrentViewToLeft();
            MoveInViewfromRight(targetIndex);
        }
        else
        {
            MoveOutCurrentViewToRight();
            MoveInViewfromLeft(targetIndex);
        }
        currentViewIndex = targetIndex;

        return this;
    }

    public void OnComplete(Action action)
    {
        _onComplete = action;
    }

    private void MoveOutCurrentViewToLeft()
    {
        int viewIndex = currentViewIndex;
        _animatedViews[viewIndex].rectTransform.DOAnchorPos(new Vector2(-Screen.width, 0), _duration)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                _animatedViews[viewIndex].rectTransform.anchoredPosition = Vector2.zero;
                _animatedViews[viewIndex].rectTransform.gameObject.SetActive(false);
            });
    }

    private void MoveOutCurrentViewToRight()
    {
        int viewIndex = currentViewIndex;
        _animatedViews[viewIndex].rectTransform.DOAnchorPos(new Vector2(Screen.width, 0), _duration)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                _animatedViews[viewIndex].rectTransform.anchoredPosition = Vector2.zero;
                _animatedViews[viewIndex].rectTransform.gameObject.SetActive(false);
            });
    }
    private void MoveInViewfromLeft(int index)
    {
        RectTransform rectTransform = _animatedViews[index].rectTransform;
        rectTransform.gameObject.SetActive(true);
        rectTransform.anchoredPosition = new Vector2(-Screen.width, 0);
        rectTransform.DOAnchorPos(Vector2.zero, _duration).SetEase(Ease.Linear).OnComplete(() =>
        {
            _onComplete?.Invoke();
            _onComplete = null;
        });
    }

    private void MoveInViewfromRight(int index)
    {
        RectTransform rectTransform = _animatedViews[index].rectTransform;
        rectTransform.gameObject.SetActive(true);
        rectTransform.anchoredPosition = new Vector2(Screen.width, 0);
        rectTransform.DOAnchorPos(Vector2.zero, _duration).SetEase(Ease.Linear).OnComplete(() =>
        {
            _onComplete?.Invoke();
            _onComplete = null;
        });
    }

}