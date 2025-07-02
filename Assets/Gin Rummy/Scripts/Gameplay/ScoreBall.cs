using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ScoreBall : MonoBehaviour
{
    public Action OnAnimationFinishCB;
    private PointsMeetTransformPoint pointsMeetTransformPoint;
    private Text pointsTxt;
    private Vector3 startPos;
    private RectTransform rt;
    private Transform startParent;
    private bool inited;
    private int points;

    private void Awake()
    {
        Init();
    }

    void Init()
    {
        if(!inited)
        {
            rt = transform as RectTransform;
            startParent = transform.parent;
            pointsTxt = GetComponentInChildren<Text>();
            pointsMeetTransformPoint = FindObjectOfType<PointsMeetTransformPoint>();
            startPos = transform.position;
            inited = true;
        }
    }

    public void UpdatePoints(int layoffPoints)
    {
        pointsTxt.text = (points  - layoffPoints).ToString();
    }

    private void AddScore(int score)
    {
        points += score;
        pointsTxt.text = points.ToString();
    }

    public void Show()
    {
        SetScore(0);
        transform.DOScale(Constants.vectorOne, 0.5f);
    }

    private void CreateAppearAndMoveAnimationSequence()
    {
        Sequence mySequence = DOTween.Sequence();
        mySequence
            .Append(rt.DOAnchorPosY(90, 0.5f).OnComplete(ParentBallToAnimationHolder))
            .Append(transform.DOMove(pointsMeetTransformPoint.transform.position, 1.5f).OnComplete(OnAppearAnimationComplete))
            .Insert(0, transform.DOScale(Constants.vectorOne, 1));
    }

    private void OnAppearAnimationComplete()
    {
        pointsMeetTransformPoint.TryRunMyScoreBall();
        Hide();
    }

    private void ParentBallToAnimationHolder()
    {
        transform.SetParent(transform.root);
    }

    public void Hide()
    {
        Init();
        gameObject.SetActive(false);
        transform.SetParent(startParent);
        transform.position = startPos;
        transform.localScale = Constants.vectorZero;
    }

    public void ShowWithAppearAnim(int score)
    {
        SetScore(score);
        CreateAppearAndMoveAnimationSequence();
    }

    public void ShowWithMoveOnlyAnim(int score, Vector3 position, Action cb)
    {
        ParentBallToAnimationHolder();
        transform.localScale = Constants.vectorOne;
        SetScore(score);
        transform.DOMove(position, 1).OnComplete(() => { cb.RunAction(); Hide(); });
    }

    public void ShowJumpAndAnimateToMasterBall(int score, ScoreBall masterScoreBall)
    {
        SetScore(score);
        ParentBallToAnimationHolder();
        Sequence mySequence = DOTween.Sequence();
        mySequence.Append(rt.DOAnchorPosY(rt.anchoredPosition.y + 150,0.5f))
            .Append(transform.DOMove(masterScoreBall.transform.position, 1f)
            .OnComplete(() =>
            {
                masterScoreBall.AddScore(score);
                Destroy(gameObject);
            }))
            .Insert(0, transform.DOScale(Constants.vectorOne, 1));
    }

    private void SetScore(int score)
    {
        points = score;
        pointsTxt.text = score.ToString();
        gameObject.SetActive(true);
    }
}

