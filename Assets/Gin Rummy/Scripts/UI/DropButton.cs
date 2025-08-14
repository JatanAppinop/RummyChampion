using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DropButton : MonoBehaviour
{
    [Header("UI References")]
    public Button dropButton;
    public TextMeshProUGUI dropButtonText;
    
    private GameManager gameManager;
    private Player currentPlayer;
    
    private void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
        
        if (dropButton == null)
            dropButton = GetComponent<Button>();
        
        if (dropButtonText == null)
            dropButtonText = GetComponentInChildren<TextMeshProUGUI>();
        
        dropButton.onClick.AddListener(OnDropButtonClicked);
    }
    
    private void Start()
    {
        // Initially hide the drop button
        gameObject.SetActive(false);
    }
    
    public void UpdateDropButtonVisibility()
    {
        bool shouldShow = ShouldShowDropButton();
        gameObject.SetActive(shouldShow);
        
        if (shouldShow)
        {
            UpdateDropButtonText();
        }
    }
    
    private bool ShouldShowDropButton()
    {
        if (gameManager == null || gameManager.gameMode != GameMode.Pool)
            return false;
        
        Player thisPlayer = gameManager.thisPlayerHand?.playerOfThisHand;
        if (thisPlayer == null)
            return false;
        
        // Show drop button only if:
        // 1. It's the player's turn
        // 2. Player hasn't already dropped
        // 3. Player isn't eliminated
        // 4. Game is in progress
        return gameManager.IsThisGamePlayer(thisPlayer) && 
               thisPlayer.CanDrop() && 
               !gameManager.IsGameEnded();
    }
    
    private void UpdateDropButtonText()
    {
        if (dropButtonText == null) return;
        
        Player thisPlayer = gameManager.thisPlayerHand?.playerOfThisHand;
        if (thisPlayer == null) return;
        
        // Show appropriate penalty based on whether player has picked a card
        int penalty = thisPlayer.hasPickedCardThisTurn ? Constants.MID_DROP_PENALTY : Constants.FULL_DROP_PENALTY;
        dropButtonText.text = $"Drop ({penalty} pts)";
    }
    
    private void OnDropButtonClicked()
    {
        Player thisPlayer = gameManager.thisPlayerHand?.playerOfThisHand;
        if (thisPlayer == null || !thisPlayer.CanDrop())
        {
            Debug.LogWarning("Cannot drop - player not eligible");
            return;
        }
        
        // Show confirmation popup
        ShowDropConfirmation(thisPlayer);
    }
    
    private void ShowDropConfirmation(Player player)
    {
        int penalty = player.hasPickedCardThisTurn ? Constants.MID_DROP_PENALTY : Constants.FULL_DROP_PENALTY;
        string message = $"Are you sure you want to drop from this game?\n\nYou will receive {penalty} penalty points.";
        
        if (PopupMessage.instance != null)
        {
            PopupMessage.instance.Show(message, "Drop", () => ConfirmDrop(player));
        }
        else
        {
            // Fallback - directly drop if no popup system
            Debug.LogWarning("No popup system found, dropping directly");
            ConfirmDrop(player);
        }
    }
    
    private void ConfirmDrop(Player player)
    {
        try
        {
            player.DropFromGame(player.hasPickedCardThisTurn);
            gameManager.OnPlayerDropped(player);
            
            // Hide the drop button after dropping
            gameObject.SetActive(false);
            
            Debug.Log($"Player {player.name} has dropped from the game");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error while dropping player: {e.Message}");
        }
    }
    
    // Call this when game phase changes
    public void OnGamePhaseChanged()
    {
        UpdateDropButtonVisibility();
    }
    
    // Call this when turn changes
    public void OnTurnChanged()
    {
        UpdateDropButtonVisibility();
    }
} 