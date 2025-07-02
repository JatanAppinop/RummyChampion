using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class LudoBoard : MonoBehaviour
{
    public List<PlayerSection> sections;
    public RectTransform pinContainer;
    public RectTransform effectContainer;
    [SerializeField] Image curtain;

    private CanvasGroup canvasGroup;
    private void Awake()
    {
        canvasGroup = gameObject.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0;
    }

    public void Initialize()
    {
        StartCoroutine(InitializeAnimation());
    }

    IEnumerator InitializeAnimation()
    {
        yield return new WaitForEndOfFrame();
        canvasGroup.DOFade(1, 0.1f).OnComplete(() => curtain.DOFade(1f, 0.2f).OnComplete(() => curtain.gameObject.SetActive(false)));
    }

}
