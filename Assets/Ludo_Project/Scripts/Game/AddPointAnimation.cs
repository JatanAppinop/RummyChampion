using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class AddPointAnimation : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI label;
    [SerializeField] RectTransform rect;
    private Color endColor;

    private void Awake()
    {
        endColor = label.color;
        label.color = Color.clear;
    }

    public void Show(int value)
    {
        if (value < 0)
        {

            label.text = value.ToString();
        }
        else
        {

            label.text = "+" + value;
        }

        rect.DOAnchorPosY(rect.anchoredPosition.y + 100, 1f);

        Sequence seq = DOTween.Sequence();
        seq.Append(label.DOColor(endColor, 0.7f));
        seq.Append(label.DOColor(Color.clear, 0.3f));
        seq.OnComplete(() => Destroy(this.gameObject, 0.1f));
    }


}
