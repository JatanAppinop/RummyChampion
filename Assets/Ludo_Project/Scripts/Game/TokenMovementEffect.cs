using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TokenMovementEffect : MonoBehaviour
{
    private Image image;


    private void Awake()
    {
        image = this.gameObject.GetComponent<Image>();
        image.enabled = false;
    }

    public void ShowEffect(float duration = 0)
    {
        StartCoroutine(PlayEffect(duration));
    }

    IEnumerator PlayEffect(float duration)
    {
        image.enabled = true;
        yield return new WaitForSeconds(0.1f);
        image.transform.DOScale(Vector3.one * 0.4f, duration > 0 ? duration : Appinop.Constants.TTokenEffectTimeout);
        yield return new WaitForSeconds(0.2f);
        image.DOFade(0, Appinop.Constants.TTokenEffectTimeout).OnComplete(() => Destroy(this.gameObject, 0.01f)); ;
    }
}
