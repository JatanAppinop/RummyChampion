using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using DG.Tweening;


public class DeadwoodArea : CardsArea {
    private ScoreBall scoreBall;

    protected  void Awake()
    {
        scoreBall = transform.parent.Find("CircleBG").GetComponent<ScoreBall>();
        scoreBall.Hide();
    }

    public int CalculateDeadwood()
    {
        Card[] cards = transform.GetComponentsInChildren<Card>();
        int deadwoodPoints = 0;
        for (int i = 0; i < cards.Length; i++)
        {
                deadwoodPoints += cards[i].GetCardPointsValue();
        }
        scoreBall.ShowWithAppearAnim(deadwoodPoints);
        return deadwoodPoints;
    }

    public void UpdateBallPoints(int layoffPoints)
    {
        scoreBall.UpdatePoints(layoffPoints);
    }
    
    public IEnumerator AnimateDeadwoodCards()
    { 
        scoreBall.Show();

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform cardTransform = transform.GetChild(i);
            cardTransform.transform.DOJump(cardTransform.transform.position, 4, 1, 1);
           
            Card card = cardTransform.GetComponent<Card>();
            if(card != null)
            {
                GameObject scoreBallGO = Instantiate(Resources.Load<GameObject>("CircleBG"), cardTransform.transform.position, Quaternion.identity, cardTransform.transform);
                ScoreBall sb = scoreBallGO.GetComponent<ScoreBall>();
                sb.ShowJumpAndAnimateToMasterBall(card.GetCardPointsValue(), scoreBall);
            }
            yield return Constants.delayBetweenDeadwoodCardAnim;
        }
    }
}
