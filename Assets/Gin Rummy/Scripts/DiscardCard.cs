using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rummy
{

    public class DiscardCard : MonoBehaviour
    {
        [SerializeField] GameManager gameManager;

        private void Awake()
        {
            gameManager = GameManager.instance;
            gameManager.gameStateUpdated.AddListener(OnGameStateUpdated);
            OnGameStateUpdated();
        }

        private void OnGameStateUpdated()
        {
            if (gameManager.currentPlayer != null && gameManager.currentPlayer.playerId == UserDataContext.Instance.UserData._id && gameManager.IsValidTimeToDiscard())
            {
                Debug.Log("Discard Card");
                this.gameObject.SetActive(true);
            }
            else
            {

                this.gameObject.SetActive(false);
            }
        }

        public void OnDiscardButtonPressed()
        {
            Card card = gameManager.currentPlayer.GetZoomedCard;

            if (card != null)
            {
                card.Unzoom(null);
                GameManager.instance.TryDiscardCard(card);
            }
        }
    }
}
