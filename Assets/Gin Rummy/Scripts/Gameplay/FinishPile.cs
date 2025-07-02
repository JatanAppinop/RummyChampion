using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class FinishPile : DropZone, IPointerDownHandler
{
    public override void OnDrop(Card card)
    {

        if (card != null && card.previousParent != transform)
        {
            float cardRotation = 0;
            CardDestination cardDestination = new CardDestination(transform, transform.position, cardRotation);
            card.SetReturnPoint(cardDestination);
            card.SetCardHolderAsParent();
        }
        gameManager.currentPlayer.DiscardZoomedCard();

        Debug.LogWarning("Game Finished");
        gameManager.FinishGameFromFinishPile();
    }

    public void OnPointerDown(PointerEventData eventData)
    {

    }
}
