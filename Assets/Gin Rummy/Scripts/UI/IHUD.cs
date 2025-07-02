using UnityEngine;
using System.Collections;
using System;
using DG.Tweening;
[RequireComponent(typeof(CanvasGroup))]
public class IHUD : MonoBehaviour
{
    protected CanvasGroup cg;
    protected GameManager gameManager;

    protected virtual void Awake()
    {
        cg = GetComponent<CanvasGroup>();
        gameManager = FindObjectOfType<GameManager>();
    }

    public virtual void TurnOnScreen()
    {
        SetActive(true);
    }

    public void TurnOffScreen()
    {
        SetActive(false);
    }

    public void SetActive(bool state)
    {
        gameObject.SetActive(state);
    }

    private void Update()
    {
        if(Input.GetKeyUp(KeyCode.Escape))
        {
            OnBack();
        }
    }

    public virtual void OnBack()
    {
      
    }

    
}
