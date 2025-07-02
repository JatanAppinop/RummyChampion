using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DiceController : MonoBehaviour
{

    public List<int> diceRollVals = new List<int>();

    [SerializeField] private List<Sprite> images;
    [SerializeField] private Image diceImage;
    private Sprite dicePlaceHolder;
    [SerializeField] private RectTransform DiceAnimation;
    private IEnumerator AnimateDiceRoll;


    public enum DiceOdds
    {
        Standard,
        PreOpening,
        NearClosing,
    }

    private readonly int[] StandardOdds = { 1, 2, 3, 4, 5, 6 };
    private readonly int[] PreOpeningOdds = { 5, 6 };
    private readonly int[] NearClosingOdds = { 1, 2, 3 };

    [HideInInspector]
    public DiceOdds Odds = DiceOdds.Standard;


    [HideInInspector]
    public UnityEvent<int> diceRolled;
    public UnityEvent diceClicked;

    private bool isActive = false;
    bool isBusy = false;

    private float duration = 0.5f;


    private void Start()
    {
        dicePlaceHolder = diceImage.sprite;
        diceImage.GetComponent<Button>().interactable = false;
    }

    public void ActivateDice(bool authority = false)
    {
        isActive = true;
        if (authority)
        {
            diceImage.GetComponent<Button>().interactable = true;
        }

    }
    public void ResetDice()
    {
        diceImage.sprite = dicePlaceHolder;
    }
    public void onClicked()
    {
        diceClicked?.Invoke();
    }
    public void onForceClicked(int value)
    {

        if (isActive && !isBusy)
        {
            isBusy = true;
            if (AnimateDiceRoll != null)
                StopCoroutine(AnimateDiceRoll);
            RollDice(value, (int v) =>
            {
                isBusy = false;
                isActive = false;
                diceImage.GetComponent<Button>().interactable = false;
                diceRolled?.Invoke(v);
            });
        }
    }



    public void RollDice(int value, Action<int> action)
    {
        AnimateDiceRoll = AnimateDiceRollRoutine(value, action);
        StartCoroutine(AnimateDiceRoll);
    }

    private IEnumerator AnimateDiceRollRoutine(int result, Action<int> action)
    {
        Debug.Log("Dice Roll animated");
        diceRollVals.Add(result);
        Debug.Log("Dice Value Added : " + result);

        Sequence diceRollSeq = DOTween.Sequence();
        diceRollSeq.Append(DiceAnimation.DOScale((Vector2.one * 1.3f), duration * 0.5f).OnComplete(() => AudioManager.Instance.PlayEffect("Dice_Roll")));
        diceRollSeq.Append(DiceAnimation.DOScale((Vector2.one), duration * 0.4f));

        DiceAnimation.gameObject.SetActive(true);

        yield return new WaitForSeconds(duration);

        diceImage.sprite = images[result - 1];
        DiceAnimation.gameObject.SetActive(false);

        yield return new WaitForSeconds(0.4f);
        action?.Invoke(result);
    }

    public int GetDiceRollValue()
    {

        int Index = UnityEngine.Random.Range(0, GetOdds(Odds).Length);

        List<int> last2Vals = new List<int>();
        if (diceRollVals.Count > 1)
        {
            last2Vals = diceRollVals.Skip(Math.Max(0, diceRollVals.Count - 2)).ToList();
        }

        if (last2Vals.Count == 2)
        {

            while (last2Vals.All(val => val == GetOdds(Odds)[Index]))
            {
                Index = UnityEngine.Random.Range(0, GetOdds(Odds).Length);
                Debug.LogWarning("New Roll Value Fetched");
            }

        }

        return GetOdds(Odds)[Index];
    }

    private int[] GetOdds(DiceOdds odds)
    {
        switch (odds)
        {
            case DiceOdds.PreOpening:
                return PreOpeningOdds;

            case DiceOdds.NearClosing:
                return NearClosingOdds;

            default:
                return StandardOdds;
        }
    }
}
