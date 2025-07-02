using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ScoreAnimation : MonoBehaviour {

    private Text scoreTxt;

    private void Awake()
    {
        Destroy(gameObject, 4);
    }



    public void SetScore(int score)
    {
        scoreTxt = GetComponentInChildren<Text>();
        scoreTxt.text = "+"+score.ToString();
        transform.SetParent(transform.parent.parent.parent);
        transform.DOScale(1, 1);
        transform.DOMoveY(transform.position.y + 10, 2);
        transform.DOScale(0, 1).SetDelay(3);
    }

   
   
}
