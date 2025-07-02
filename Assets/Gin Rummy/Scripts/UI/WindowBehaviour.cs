using UnityEngine;
using System.Collections;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class WindowBehaviour : IHUD
{
    Animator animator;
    private readonly int hideWindowParameter = Animator.StringToHash("Hide");
    private readonly int showWindowParameter = Animator.StringToHash("Show");

    protected float animTime = 0.35f;
    protected bool isOpen;

    protected override void Awake()
    {
        base.Awake();
        cg.alpha = 0;
        animator = GetComponent<Animator>();
        SwitchCanvasGroup(false);
    }

    private void OnDisable()
    {
        CloseWindow();
    }

    protected virtual void SwitchCanvasGroup(bool state)
    {
        cg.interactable = state;
        cg.blocksRaycasts = state;
    }

    public virtual void ShowWindow()
    {
        if (!isOpen)
        {
            if (animator != null)
                animator.SetTrigger(showWindowParameter);
            isOpen = true;
            TurnOnScreen();
            SwitchCanvasGroup(true);
            cg.DOKill();
            cg.DOFade(1, animTime);
        }
    }

    public virtual void CloseWindow()
    {
        if (cg != null && isOpen)
        {
            if (animator != null)
                animator.SetTrigger(hideWindowParameter);

            cg.DOKill();
            isOpen = false;
            SwitchCanvasGroup(false);
            cg.DOFade(0, animTime).OnComplete(TurnOffScreen);
        }
    }
}
