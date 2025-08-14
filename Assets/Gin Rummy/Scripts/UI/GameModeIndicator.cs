using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameModeIndicator : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI gameModeText;
    public TextMeshProUGUI gameInfoText;
    public TextMeshProUGUI dealCounterText;
    public Image backgroundPanel;
    
    private GameManager gameManager;
    
    private void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
    }
    
    private void Start()
    {
        UpdateGameModeDisplay();
    }
    
    public void UpdateGameModeDisplay()
    {
        if (gameManager == null) return;
        
        UpdateGameModeText();
        UpdateGameInfoText();
        UpdateDealCounter();
    }
    
    private void UpdateGameModeText()
    {
        if (gameModeText == null) return;
        
        string modeText = "";
        Color modeColor = Color.white;
        
        switch (gameManager.gameMode)
        {
            case GameMode.Points:
                modeText = "POINTS RUMMY";
                modeColor = new Color(0.2f, 0.8f, 0.2f); // Green
                break;
            case GameMode.Deals:
                modeText = "DEALS RUMMY";
                modeColor = new Color(0.2f, 0.6f, 0.9f); // Blue
                break;
            case GameMode.Pool:
                if (gameManager.tableData != null)
                {
                    if (gameManager.tableData.gameType == "Pool101")
                        modeText = "POOL 101";
                    else if (gameManager.tableData.gameType == "Pool201")
                        modeText = "POOL 201";
                    else
                        modeText = "POOL RUMMY";
                }
                else
                {
                    modeText = "POOL RUMMY";
                }
                modeColor = new Color(0.9f, 0.4f, 0.1f); // Orange
                break;
            default:
                modeText = "RUMMY";
                break;
        }
        
        gameModeText.text = modeText;
        gameModeText.color = modeColor;
    }
    
    private void UpdateGameInfoText()
    {
        if (gameInfoText == null) return;
        
        string infoText = "";
        
        switch (gameManager.gameMode)
        {
            case GameMode.Points:
                double perPointValue = gameManager.GetPerPointValue();
                infoText = $"Per Point: ₹{perPointValue:F2}";
                break;
                
            case GameMode.Deals:
                int totalDeals = gameManager.GetTotalDeals();
                int currentDeal = gameManager.GetCurrentDealNumber();
                infoText = $"Deal {currentDeal} of {totalDeals}";
                break;
                
            case GameMode.Pool:
                int eliminationScore = gameManager.GetEliminationThreshold();
                int activePlayers = gameManager.GetActivePlayersCount();
                infoText = $"Elimination: {eliminationScore} pts | Active: {activePlayers}";
                break;
        }
        
        gameInfoText.text = infoText;
    }
    
    private void UpdateDealCounter()
    {
        if (dealCounterText == null) return;
        
        if (gameManager.gameMode == GameMode.Deals)
        {
            dealCounterText.gameObject.SetActive(true);
            int currentDeal = gameManager.GetCurrentDealNumber();
            int totalDeals = gameManager.GetTotalDeals();
            dealCounterText.text = $"Deal {currentDeal}/{totalDeals}";
        }
        else
        {
            dealCounterText.gameObject.SetActive(false);
        }
    }
    
    public void OnDealChanged()
    {
        UpdateDealCounter();
        UpdateGameInfoText();
    }
    
    public void OnPlayerEliminated()
    {
        UpdateGameInfoText();
    }
    
    public void OnGameModeChanged()
    {
        UpdateGameModeDisplay();
    }
    
    // Update display when game state changes
    public void OnGameStateChanged()
    {
        UpdateGameModeDisplay();
    }
} 