using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MovesLeftController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI movesLabel;
    public void Initialize()
    {
        this.gameObject.SetActive(true);
    }


    public void UpdateMoves(int value)
    {
        movesLabel.text = "Moves Left : " + value;
    }
}
