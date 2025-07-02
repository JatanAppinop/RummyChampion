using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DecisionPopup : PopupMessage {
    public static new DecisionPopup instance;
    protected Action onYesAction;
    protected Action onNoAction;

    protected override void Awake() {
        instance = this;
        Init();    
    }
		
    public virtual void Show(string message, Action onYesAction, Action onNoAction) {
        base.Show(message);
        this.onYesAction = onYesAction;
        this.onNoAction = onNoAction;
    }
    
    public virtual void OnYes() {
        onYesAction.RunAction();
        CloseWindow();
    }

    public virtual void OnNo() {
        onNoAction.RunAction();
        CloseWindow();
    }
}
