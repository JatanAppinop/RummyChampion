using System.Collections;
using System.Collections.Generic;


public class OddsController
{
    public static void SetOdds(LudoPlayer player, DiceController diceController)
    {
        int Tokens = player.tokens.Count;

        int TokenAtHouse = player.tokens.FindAll((t) => t.index == 0).Count;

        if (TokenAtHouse > Tokens - 1)
        {
            diceController.Odds = DiceController.DiceOdds.PreOpening;
            return;
        }

        int TokenNearFinish = player.tokens.FindAll((t) => t.index > 51).Count;

        if (TokenNearFinish > Tokens - 1)
        {
            diceController.Odds = DiceController.DiceOdds.NearClosing;
            return;
        }

        diceController.Odds = DiceController.DiceOdds.Standard;
        return;

    }
}
