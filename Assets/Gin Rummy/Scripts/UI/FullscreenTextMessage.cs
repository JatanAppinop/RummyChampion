using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FullscreenTextMessage : WindowBehaviour
{

    public static FullscreenTextMessage instance;
    [SerializeField] private Text myText;
    Timer currentTimer;

    protected override void Awake()
    {
        base.Awake();
        instance = this;

        if (myText == null)
            myText = GetComponentInChildren<Text>();
    }


    public void ShowText(string message, float time = 2f)
    {
        myText.text = message;
        ShowWindow();
        if (currentTimer != null)
            currentTimer.RestartTimer(time, CloseWindow);
        else
            currentTimer = new Timer(time, CloseWindow);
    }

    protected override void SwitchCanvasGroup(bool state)
    {
        cg.interactable = false;
        cg.blocksRaycasts = false;
    }
}
