using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class SpinnerView : MonoBehaviour
{
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] TextMeshProUGUI loadingText;

    private void Awake()
    {
        canvasGroup.alpha = 0;
        loadingText.gameObject.SetActive(false);
    }

    public void showLoadingText(string text)
    {
        if (text.Length > 1)
        {
            loadingText.text = text;
            loadingText.gameObject.SetActive(true);
        }
    }

    public void showSpinnerView(string text)
    {
        canvasGroup.DOFade(1f, 0.2f);
        showLoadingText(text);
    }
    public void HideSpinnerView()
    {
        canvasGroup.DOFade(0f, 0.2f).OnComplete(() => Destroy(this.gameObject, 0.1f));
    }
}
