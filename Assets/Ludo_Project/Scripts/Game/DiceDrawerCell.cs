using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DiceDrawerCell : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI label;
    [SerializeField] Image background;
    [SerializeField] Sprite selectedSprite;
    [SerializeField] Sprite normalSprite;

    public bool isUsed;

    public int Value { get; private set; }

    public void SetValue(int val)
    {
        Value = val;
        label.text = val.ToString();
    }

    public void SetActive()
    {
        background.sprite = selectedSprite;
    }

    public void SetDisabled()
    {
        Color transparentColor = Color.white;
        transparentColor.a = 0.5f;
        background.color = transparentColor;
        background.sprite = normalSprite;
    }
}
