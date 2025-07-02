using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotEasy : AIPlayer {

    public BotEasy(Deck deck) : base(deck)
    {
        SayBotDecision("Play with Bot - Easy");
    }

    public override void Discard()
    {
        if (myHand.CanWinThisGame() == false)
        {
            cardOnAction = GetRandomNotSequencedCard();
            ZoomOrDiscardCard();
        }
    }

    public override void PassOrTake()
    {
        if (Randomizer.GetRandomDecision())
        {
            FullscreenTextMessage.instance.ShowText("Opponent passed");
            gameManager.OnPass();
        }
        else
        {
            TakeCard(discardPile);
        }
    }
    
}
