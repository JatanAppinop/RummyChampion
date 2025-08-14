using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RummyTestManager : MonoBehaviour
{
    [Header("Test Controls")]
    public Button testDropButton;
    public Button testEliminationButton;
    public Button testDealsButton;
    public Button testPoolButton;
    public Button testPointsButton;
    public TextMeshProUGUI testStatusText;
    
    [Header("Test Configuration")]
    public bool enableTestMode = true;
    public GameMode testGameMode = GameMode.Pool;
    
    private GameManager gameManager;
    private DropButton dropButton;
    private GameModeIndicator gameModeIndicator;
    private DealsRummyManager dealsRummyManager;
    
    private void Awake()
    {
        if (enableTestMode)
        {
            gameManager = FindObjectOfType<GameManager>();
            dropButton = FindObjectOfType<DropButton>();
            gameModeIndicator = FindObjectOfType<GameModeIndicator>();
            dealsRummyManager = FindObjectOfType<DealsRummyManager>();
            
            SetupTestButtons();
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
    
    private void SetupTestButtons()
    {
        if (testDropButton != null)
            testDropButton.onClick.AddListener(TestDropFunctionality);
        
        if (testEliminationButton != null)
            testEliminationButton.onClick.AddListener(TestEliminationSystem);
        
        if (testDealsButton != null)
            testDealsButton.onClick.AddListener(TestDealsRummy);
        
        if (testPoolButton != null)
            testPoolButton.onClick.AddListener(TestPoolRummy);
        
        if (testPointsButton != null)
            testPointsButton.onClick.AddListener(TestPointsRummy);
        
        UpdateTestStatus("Test Manager Initialized - Ready for Testing");
    }
    
    #region Test Drop Functionality
    public void TestDropFunctionality()
    {
        UpdateTestStatus("Testing Drop Functionality...");
        
        if (gameManager == null || gameManager.playerList.Count == 0)
        {
            UpdateTestStatus("ERROR: No game manager or players found!");
            return;
        }
        
        Player testPlayer = gameManager.thisPlayerHand?.playerOfThisHand;
        if (testPlayer == null)
        {
            UpdateTestStatus("ERROR: No test player found!");
            return;
        }
        
        // Test drop scenarios
        StartCoroutine(TestDropScenarios(testPlayer));
    }
    
    private IEnumerator TestDropScenarios(Player player)
    {
        // Test 1: Drop without picking a card (should get FULL_DROP_PENALTY)
        UpdateTestStatus($"Test 1: Drop without picking card (Penalty: {Constants.FULL_DROP_PENALTY})");
        player.hasPickedCardThisTurn = false;
        int scoreBefore = player.cumulativeScore;
        player.DropFromGame(false);
        
        yield return new WaitForSeconds(1f);
        
        int expectedScore = scoreBefore + Constants.FULL_DROP_PENALTY;
        if (player.cumulativeScore == expectedScore && player.hasDropped)
        {
            UpdateTestStatus("✓ Test 1 PASSED: Full drop penalty applied correctly");
        }
        else
        {
            UpdateTestStatus($"✗ Test 1 FAILED: Expected {expectedScore}, got {player.cumulativeScore}");
        }
        
        yield return new WaitForSeconds(2f);
        
        // Reset for next test
        player.ResetState();
        
        // Test 2: Drop after picking a card (should get MID_DROP_PENALTY)
        UpdateTestStatus($"Test 2: Drop after picking card (Penalty: {Constants.MID_DROP_PENALTY})");
        player.MarkCardPickedThisTurn();
        scoreBefore = player.cumulativeScore;
        player.DropFromGame(true);
        
        yield return new WaitForSeconds(1f);
        
        expectedScore = scoreBefore + Constants.MID_DROP_PENALTY;
        if (player.cumulativeScore == expectedScore && player.hasDropped)
        {
            UpdateTestStatus("✓ Test 2 PASSED: Mid drop penalty applied correctly");
        }
        else
        {
            UpdateTestStatus($"✗ Test 2 FAILED: Expected {expectedScore}, got {player.cumulativeScore}");
        }
        
        UpdateTestStatus("Drop Functionality Tests Complete!");
    }
    #endregion
    
    #region Test Elimination System
    public void TestEliminationSystem()
    {
        UpdateTestStatus("Testing Elimination System...");
        
        if (gameManager == null)
        {
            UpdateTestStatus("ERROR: No game manager found!");
            return;
        }
        
        StartCoroutine(TestEliminationScenarios());
    }
    
    private IEnumerator TestEliminationScenarios()
    {
        Player testPlayer = gameManager.thisPlayerHand?.playerOfThisHand;
        if (testPlayer == null)
        {
            UpdateTestStatus("ERROR: No test player found!");
            yield break;
        }
        
        // Set game mode to Pool for elimination testing
        gameManager.gameMode = GameMode.Pool;
        
        // Test elimination at 101 points (Pool 101)
        if (gameManager.tableData != null)
            gameManager.tableData.gameType = "Pool101";
        
        UpdateTestStatus("Test: Player elimination at 101 points");
        
        // Add points close to elimination threshold
        testPlayer.AddToCumulativeScore(95);
        yield return new WaitForSeconds(1f);
        
        if (!testPlayer.isEliminated)
        {
            UpdateTestStatus("✓ Player not eliminated at 95 points (correct)");
        }
        
        // Add more points to trigger elimination
        testPlayer.AddToCumulativeScore(10); // Total: 105 points
        yield return new WaitForSeconds(1f);
        
        if (testPlayer.isEliminated)
        {
            UpdateTestStatus("✓ Test PASSED: Player eliminated at 105 points");
        }
        else
        {
            UpdateTestStatus("✗ Test FAILED: Player should be eliminated at 105 points");
        }
        
        UpdateTestStatus("Elimination System Tests Complete!");
    }
    #endregion
    
    #region Test Deals Rummy
    public void TestDealsRummy()
    {
        UpdateTestStatus("Testing Deals Rummy...");
        
        if (gameManager == null)
        {
            UpdateTestStatus("ERROR: No game manager found!");
            return;
        }
        
        // Set game mode to Deals
        gameManager.gameMode = GameMode.Deals;
        
        StartCoroutine(TestDealsScenarios());
    }
    
    private IEnumerator TestDealsScenarios()
    {
        UpdateTestStatus("Initializing Deals Rummy with 3 deals");
        
        // Initialize deals rummy
        gameManager.InitializeDealsRummy(3);
        yield return new WaitForSeconds(1f);
        
        if (gameManager.GetTotalDeals() == 3 && gameManager.GetCurrentDealNumber() == 1)
        {
            UpdateTestStatus("✓ Deals Rummy initialized correctly");
        }
        else
        {
            UpdateTestStatus("✗ Deals Rummy initialization failed");
        }
        
        // Simulate completing deals
        for (int deal = 1; deal <= 3; deal++)
        {
            UpdateTestStatus($"Simulating Deal {deal}");
            
            Player winner = gameManager.playerList[deal % 2]; // Alternate winners
            if (dealsRummyManager != null)
            {
                dealsRummyManager.CompleteDeal(winner, gameManager.playerList);
            }
            
            yield return new WaitForSeconds(2f);
        }
        
        UpdateTestStatus("Deals Rummy Tests Complete!");
    }
    #endregion
    
    #region Test Pool Rummy
    public void TestPoolRummy()
    {
        UpdateTestStatus("Testing Pool Rummy...");
        
        if (gameManager == null)
        {
            UpdateTestStatus("ERROR: No game manager found!");
            return;
        }
        
        // Set game mode to Pool
        gameManager.gameMode = GameMode.Pool;
        
        StartCoroutine(TestPoolScenarios());
    }
    
    private IEnumerator TestPoolScenarios()
    {
        if (gameManager.tableData != null)
            gameManager.tableData.gameType = "Pool101";
        
        UpdateTestStatus("Testing Pool 101 functionality");
        
        // Test elimination threshold
        int threshold = gameManager.GetEliminationThreshold();
        if (threshold == Constants.POOL_101_ELIMINATION_SCORE)
        {
            UpdateTestStatus("✓ Pool 101 elimination threshold correct (101)");
        }
        else
        {
            UpdateTestStatus($"✗ Pool 101 threshold incorrect: {threshold}");
        }
        
        yield return new WaitForSeconds(1f);
        
        // Test active players count
        int activePlayers = gameManager.GetActivePlayersCount();
        UpdateTestStatus($"Active players: {activePlayers}");
        
        // Test pool winning amount calculation
        int poolAmount = gameManager.GetPoolWinningAmount();
        UpdateTestStatus($"Pool winning amount: ₹{poolAmount}");
        
        UpdateTestStatus("Pool Rummy Tests Complete!");
    }
    #endregion
    
    #region Test Points Rummy
    public void TestPointsRummy()
    {
        UpdateTestStatus("Testing Points Rummy...");
        
        if (gameManager == null)
        {
            UpdateTestStatus("ERROR: No game manager found!");
            return;
        }
        
        // Set game mode to Points
        gameManager.gameMode = GameMode.Points;
        
        StartCoroutine(TestPointsScenarios());
    }
    
    private IEnumerator TestPointsScenarios()
    {
        UpdateTestStatus("Testing Points Rummy calculations");
        
        // Test per-point value calculation
        double perPointValue = gameManager.GetPerPointValue();
        UpdateTestStatus($"Per point value: ₹{perPointValue:F2}");
        
        yield return new WaitForSeconds(1f);
        
        // Test platform fee calculation
        int testAmount = 100;
        int platformFee = gameManager.GetPlatformFee(testAmount);
        UpdateTestStatus($"Platform fee for ₹{testAmount}: ₹{platformFee}");
        
        UpdateTestStatus("Points Rummy Tests Complete!");
    }
    #endregion
    
    #region UI Integration Tests
    public void TestUIIntegration()
    {
        UpdateTestStatus("Testing UI Integration...");
        
        // Test drop button visibility
        if (dropButton != null)
        {
            dropButton.UpdateDropButtonVisibility();
            UpdateTestStatus("✓ Drop button updated");
        }
        
        // Test game mode indicator
        if (gameModeIndicator != null)
        {
            gameModeIndicator.UpdateGameModeDisplay();
            UpdateTestStatus("✓ Game mode indicator updated");
        }
        
        // Test player UI updates
        foreach (Player player in gameManager.playerList)
        {
            player.playerUI?.UpdateGameModeSpecificUI(gameManager.gameMode);
        }
        UpdateTestStatus("✓ Player UIs updated");
        
        UpdateTestStatus("UI Integration Tests Complete!");
    }
    #endregion
    
    private void UpdateTestStatus(string status)
    {
        if (testStatusText != null)
        {
            testStatusText.text = status;
        }
        Debug.Log($"[RummyTest] {status}");
    }
    
    // Quick test for all functionality
    public void RunAllTests()
    {
        StartCoroutine(RunAllTestsCoroutine());
    }
    
    private IEnumerator RunAllTestsCoroutine()
    {
        UpdateTestStatus("Running All Tests...");
        
        TestDropFunctionality();
        yield return new WaitForSeconds(3f);
        
        TestEliminationSystem();
        yield return new WaitForSeconds(3f);
        
        TestDealsRummy();
        yield return new WaitForSeconds(3f);
        
        TestPoolRummy();
        yield return new WaitForSeconds(3f);
        
        TestPointsRummy();
        yield return new WaitForSeconds(3f);
        
        TestUIIntegration();
        
        UpdateTestStatus("All Tests Complete! Check console for detailed results.");
    }
} 