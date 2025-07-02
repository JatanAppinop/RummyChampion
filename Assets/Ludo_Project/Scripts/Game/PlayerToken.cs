using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PlayerToken : MonoBehaviour
{

    [Serializable]
    internal class TokenSprite
    {
        public string name;
        public Sprite sprite;
    }

    public int index = 0;
    public int ID = 0;
    [field: SerializeField] public BoardColorsUtils.BoardColors tokenColor { get; private set; }
    public bool canMove = false;
    public bool isMoving = false;
    [Header("Token Sprites")]
    [SerializeField] List<TokenSprite> tokenSprites;
    [Header("Token Image")]
    [SerializeField] Image tokenImage;
    [SerializeField] Image highlightImage;
    [SerializeField] Button tokenButton;

    private Vector2 originalScale;

    private Tween imageScaleTween;

    public UnityEvent<PlayerToken, bool> onTokenClicked;

    private void Awake()
    {
        tokenButton.interactable = false;
        highlightImage.gameObject.SetActive(false);
    }

    public void SetTokenColor(BoardColorsUtils.BoardColors _tokenColor)
    {
        tokenColor = _tokenColor;
        Sprite spr = tokenSprites.Find((ts) => ts.name.Equals(tokenColor.ToString())).sprite;
        tokenImage.sprite = spr;

    }

    public void ActivateToken(bool interactable = true)
    {
        // Debug.Log("Token Activated");
        originalScale = this.transform.localScale;
        this.transform.DOScale(Vector2.one * 1f, 0.1f);
        imageScaleTween = tokenImage.GetComponent<RectTransform>().DOScale(Vector2.one * 1.3f, 0.3f).SetLoops(-1, LoopType.Yoyo);
        highlightImage.gameObject.SetActive(true);
        canMove = true;
        tokenButton.interactable = interactable;

    }
    public void DeactivateToken()
    {
        // Debug.Log("Token Deactivated");
        if (imageScaleTween != null && imageScaleTween.active)
        {
            imageScaleTween.Kill(true);
        }
        tokenImage.GetComponent<RectTransform>().DOScale(Vector2.one, 0.1f);

        if (!isMoving)
        {
            this.transform.localScale = originalScale;
        }

        highlightImage.gameObject.SetActive(false);
        canMove = false;
        isMoving = false;
        tokenButton.interactable = false;

    }
    public void onClicked()
    {
        if (canMove)
        {
            isMoving = true;
            onTokenClicked?.Invoke(this, false);
        }
    }
    public void Highlight(bool b)
    {
        if (b)
        {
            highlightImage.gameObject.SetActive(true);
        }
        else
        {
            highlightImage.gameObject.SetActive(false);
        }
    }

}
