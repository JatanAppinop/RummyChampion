using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SortingBtn : MonoBehaviour
{

    [SerializeField] Text btnText;
    SortingMethod sortingMethod = SortingMethod.Values;
    Hand playerHand;
    int currentSortingType;

    private void Awake()
    {
        playerHand = GameObject.FindGameObjectWithTag("PlayerHand").GetComponent<Hand>();
        currentSortingType = (int)sortingMethod;
        RefreshBtnText();
    }

    public void NextSorting()
    {
        currentSortingType = (currentSortingType + 1) % 2;
        sortingMethod = (SortingMethod)currentSortingType;
        playerHand.ChangeSortingType(sortingMethod);
        RefreshBtnText();
    }

    private void RefreshBtnText()
    {
        string desc = "";
        switch (sortingMethod)
        {
            case SortingMethod.Values:
                desc = "7777";
                break;
            case SortingMethod.Colors:
                desc = "A234";
                break;
            default:
                break;
        }
        btnText.text = desc;
    }
}
