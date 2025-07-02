using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPlayer : BotPlayer {

    public AIPlayer(Deck deck) : base(deck)
    {
        avatarID = Randomizer.GetRandomNumber(0, Constants.MAX_AVATAR_ID);
        SetPlayerAvatar(avatarID);
        name = Randomizer.GetRandomName();
    }
    
    protected void ZoomOrDiscardCard()
    {
        if (Randomizer.GetRandomDecision())
        {
            cardOnAction.ZoomCard(true);
            new Timer(Constants.QUICK_ANIM_TIME + 0.2f, DiscardSelectedCard);
        }
        else
            DiscardSelectedCard();
    }
}
