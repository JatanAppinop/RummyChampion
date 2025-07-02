using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotPlayer : Player
{

    public BotPlayer(Deck deck) : base(deck)
    {
    }

    protected override void InitPlayerUI(string playerUIName = "EnemyUI")
    {
        playerUI = GameObject.Find("EnemyUI").GetComponent<PlayerUI>();
        playerUI.ResetUI(PerformAction);
    }

    protected void SayBotDecision(string decision)
    {
#if TEST_MODE
        Debug.Log(decision);
#endif
    }
}
