using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.UI;
using System;

public class ButtonAnimation : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public Vector3 defaultPos { get; private set; }
    private float defaultScale;
    public float shakePower =5;
    public float offsetValue=1;
    private RectTransform rt;
    private Button btn;
    bool inited;
    float resetTimer = 1f;

    void Start()
    {
        defaultScale = transform.localScale.x;
        rt = transform as RectTransform;
        btn = GetComponent<Button>();
        ResetButtonState();
    }

    void GetDefaultPos()
    {
        if (rt == null)
            rt = transform as RectTransform;
        defaultPos = transform.position;

        inited = true;
        ResetBtnPos();
    }

    private void OnEnable()
    {
        ResetBtnPos();
    }

    void LateUpdate()
    {
        if (resetTimer > 0)
        {
            resetTimer -= Time.deltaTime;
            return;
        }

        if (!inited)
            GetDefaultPos();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (btn != null && btn.interactable == false)
            return;
        if(CanPlayAnimation())
            transform.DOScale(defaultScale * 1.1f, 0.5f);
    }

  
    public void OnPointerExit(PointerEventData eventData)
    {
        if (btn != null && btn.interactable == false)
            return;
        if (CanPlayAnimation())
            transform.DOScale(defaultScale * 1.0f, 0.5f);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (btn != null && btn.interactable == false)
            return;
      
		//if (CanPlayAnimation())
        //    transform.DOMoveY(transform.position.y - offsetValue, 0.2f);
    }



    public void OnPointerUp(PointerEventData eventData)
    {
        if (btn != null && btn.interactable == false)
            return;
        if (CanPlayAnimation())
        {
            ResetBtnPos();
            //transform.DOMoveY(transform.position.y, 0.2f);
        }
    }

    public void ResetBtnPos()
    {
        if (inited)
        {
            transform.DOKill();
            transform.position = defaultPos;
            transform.localScale = new Vector3(defaultScale,defaultScale,defaultScale);
        }
    }

    bool lockRaycast;

    public void LockRaycasts()
    {
        lockRaycast = true;
    }

    private bool CanPlayAnimation()
    {
        return inited && !lockRaycast;
    }


    public void ResetButtonState()
    {

        resetTimer = 1f;
        inited = false;
        lockRaycast = false;
    }

}