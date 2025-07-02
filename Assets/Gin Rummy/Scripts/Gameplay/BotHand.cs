using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotHand : Hand {


    protected override void Awake()
    {
        base.Awake();
    }

    protected override void ShowCard(Transform child)
    {
#if !TEST_MODE
        Card card = child.GetComponent<Card>();
        if (card != null)
        {
            card.isReversed = true;
        }
#else
        base.ShowCard(child);
#endif
    }


    protected override int GetLastAvailableBeforeSequencesPosition()
    {
//#if !TEST_MODE
        return transform.childCount;
//#else
//       return base.GetLastAvailableBeforeSequencesPosition();
//#endif
    }

    public override void UnlockZone()
    {
        base.LockZone();
    }

    protected override float GetVerticalOffset(bool isSequencedCard)
    {
        return 0;
    }

    protected override float GetScaleFactor(bool isSequencedCard)
    {
        return 1;
    }

    protected override float GetPositionFactor(bool isSequencedCard)
    {
        return 1;
    }
}
