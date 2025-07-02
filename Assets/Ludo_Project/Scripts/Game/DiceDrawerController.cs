using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class DiceDrawerController : MonoBehaviour
{

    public int currentMove = 0;
    public int totalMoves = Appinop.Constants.TotalNumberMoved;
    [Header("References")]
    [SerializeField] List<DiceDrawerCell> cells;
    [SerializeField] List<GameObject> rows;
    [SerializeField] RectTransform drawerRect;
    [SerializeField] Image blocker;
    [SerializeField] Vector2 closedDrawerPosition;
    private bool isOpen = false;


    public void Initialize()
    {
        this.gameObject.transform.parent.gameObject.SetActive(true);
        this.gameObject.SetActive(true);
        closeDrawer();
        MarkCurrentActive();
    }
    public void onOpenDrawerPressed()
    {
        if (isOpen)
        {
            //Close Drawer
            closeDrawer();
        }
        else
        {
            //opem Drawer
            openDrawer();
        }
    }

    public void onBlockerClicked()
    {
        closeDrawer();
    }

    private void openDrawer()
    {
        showallRows();
        isOpen = true;
        drawerRect.DOAnchorPos(Vector3.zero, 0.1f).SetEase(Ease.InSine);
        blocker.enabled = true;
        blocker.DOFade(0.5f, 0.1f);
    }
    private void closeDrawer()
    {
        isOpen = false;
        drawerRect.DOAnchorPos(closedDrawerPosition, 0.1f).SetEase(Ease.OutSine);
        blocker.DOFade(0, 0.1f).OnComplete(() => blocker.enabled = false);
        showCurrentRow();

    }

    private void showCurrentRow()
    {
        if (isOpen)
            closeDrawer();

        if (currentMove < 10)
        {
            showRow(1);
        }
        else if (currentMove >= 10 && currentMove < 20)
        {
            showRow(2);
        }
        else if (currentMove >= 20 && currentMove < 30)
        {
            showRow(3);
        }
    }

    private void showRow(int row)
    {
        for (int i = 0; i < rows.Count; i++)
        {
            if (i == row - 1)
                rows[i].SetActive(true);
            else
                rows[i].SetActive(false);
        }
    }

    private void showallRows()
    {
        rows.ForEach(r => r.SetActive(true));
    }


    public void GenerateNumbers()
    {
        List<int> values = GenerateNumberList(cells.Count, 5);
        for (int i = 0; i < cells.Count; i++)
        {
            cells[i].SetValue(values[i]);
        }
    }

    public int GetCurrentValue() => cells[currentMove].Value;
    public int GetRemainingMoves() => totalMoves - currentMove;
    private void MarkCurrentActive()
    {
        cells[currentMove].SetActive();
    }
    public void MarkNextActive()
    {

        cells[currentMove].SetDisabled();
        currentMove++;
        if (currentMove < totalMoves)
        {
            cells[currentMove].SetActive();
        }
        else
        {
            Debug.LogError("Current Move Out of bound : " + currentMove);
        }
        showCurrentRow();
    }
    private static List<int> GenerateNumberList(int count, int maxValue)
    {
        List<int> numberList = new List<int>();

        // Calculate how many times each number should repeat
        int repeats = count / maxValue;

        // Fill the list with numbers from 1 to maxValue, each repeating 'repeats' times
        for (int i = 1; i <= maxValue; i++)
        {
            for (int j = 0; j < repeats; j++)
            {
                numberList.Add(i);
            }
        }

        // Shuffle the list to randomize the order
        numberList.Shuffle();

        return numberList;
    }



}
