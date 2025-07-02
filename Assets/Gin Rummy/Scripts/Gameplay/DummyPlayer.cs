using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyPlayer : BotPlayer
{

    public DummyPlayer(Deck deck) : base(deck)
    {
    }

    private Queue<Action> opponentActionsQueue = new Queue<Action>();
    private bool isOnAction;

    void RunNextAction()
    {
        if(opponentActionsQueue.Count > 0 && isOnAction == false && gameManager.IsGameEnded() == false)
        {
            opponentActionsQueue.Dequeue().RunAction();
        }
    }




    private void PassAction()
    {
        gameManager.OnPass();
        FullscreenTextMessage.instance.ShowText("Opponent passed");
        FinishAction();
    }

    public override void PerformAction()
    {
        Debug.Log("Koniec czasu przeciwnika");
    }

    protected override void FinishMove()
    {
        base.FinishMove();
        FinishAction();
    }

    private void FinishAction()
    {
        isOnAction = false;
        RunNextAction();
    }

    public override void ResetState()
    {
        opponentActionsQueue.Clear();
        isOnAction = false;
    }
}
