using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using DG.Tweening;

public class PlayerControls : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nameTag;
    [SerializeField] Image profilePhoto;
    [SerializeField] Image Backdrop;
    [SerializeField] DiceController diceController;
    [SerializeField] List<Image> mistryIndicators;
    [SerializeField] Sprite missImage;
    [SerializeField] Image timerImage;
    [SerializeField] Image skullImage;
    [SerializeField] RectTransform handImage;
    private Vector2 handPos;

    [HideInInspector]
    private LudoPlayer player;

    public bool isLocalPlayer;

    public bool hasAuthority;

    public bool ExtraTurn = false;

    private bool diceRolled = false;
    private bool tokenMoved = false;
    private bool isBot = false;

    private IEnumerator timerRoutine;
    private Tween handTween;

    // Events------

    public List<int> GetDiceRollVals() => diceController.diceRollVals;
    private void Awake()
    {
        timerImage.gameObject.SetActive(false);
        handImage.gameObject.SetActive(false);
        skullImage.gameObject.SetActive(false);
        handPos = handImage.localPosition;
    }

    public void Initialize(LudoPlayer player, bool isLocal)
    {
        this.player = player;
        isLocalPlayer = isLocal;

        if (isLocalPlayer)
        {
            hasAuthority = true;
            diceController.diceRolled.AddListener(onDiceRolled);
            diceController.diceClicked.AddListener(onDiceClicked);
        }
        else
        {
            hasAuthority = false;
            SocketServer.Instance.onDiceClicked.AddListener(onSocketDiceClicked);
            //SocketServer.Instance.onDiceRolled.AddListener(onSocketDiceRolled);
            SocketServer.Instance.onTokenMoved.AddListener(onSocketTokenMoved);
            diceController.diceRolled.AddListener(onDiceRolled);
        }
    }
    public void SetName(string name) => nameTag.text = name;
    public void SetPhoto(Sprite photo) => profilePhoto.sprite = photo;
    public void SetBackdrop(Color backdrop) => Backdrop.color = backdrop;

    public void StartTurn()
    {

        if (Gamemanager.Instance.gameMode == Appinop.Constants.GameMode.Classic || Gamemanager.Instance.gameMode == Appinop.Constants.GameMode.Timer)
        {

            if (hasAuthority)
            {
                diceRolled = false;
                tokenMoved = false;
                StartTimer();
                ShowHand();
                OddsController.SetOdds(player, diceController);
                diceController.ActivateDice(true);
            }
            else
            {
                diceRolled = false;
                tokenMoved = false;
                StartTimer();
                ShowHand();
                OddsController.SetOdds(player, diceController);
                diceController.ActivateDice(false);
            }

        }
        else if (Gamemanager.Instance.gameMode == Appinop.Constants.GameMode.Turbo)
        {
            if (hasAuthority)
            {
                diceRolled = false;
                tokenMoved = false;
                diceController.ActivateDice(false);
                timerRunout();
                StartTimer();
                ShowHand();
            }
            else
            {
                diceRolled = false;
                tokenMoved = false;
                diceController.ActivateDice(false);
                StartTimer();
                ShowHand();
            }
        }
    }

    public void StartTimer()
    {
        timerImage.gameObject.SetActive(true);
        timerImage.fillAmount = 0;
        timerRoutine = startTimerRoutine();
        StartCoroutine(timerRoutine);
    }

    IEnumerator startTimerRoutine()
    {
        float timeElapsed = 0f;
        float updateInterval = 0.1f;
        bool warning = false;
        float timeout = Gamemanager.Instance.DevMode ? 2.0f : Appinop.Constants.TTurnTimeout;


        while (timeElapsed < timeout)
        {
            yield return new WaitForSeconds(updateInterval);
            timerImage.fillAmount = ((1f / timeout) * timeElapsed);
            timeElapsed += updateInterval;


            if (!warning && timeElapsed > (timeout - 3f))
            {
                warning = true;
                AudioManager.Instance.PlayEffect("Warning", 3);
            }
        }

        timerImage.fillAmount = 1f;
        timerRunout();
    }
    public void StopTimer()
    {
        if (timerRoutine != null)
        {

            StopCoroutine(timerRoutine);
            timerImage.gameObject.SetActive(false);
        }
    }

    private async void onDiceClicked()
    {
        if (hasAuthority)
        {
            int value;
            if (Gamemanager.Instance.gameMode == Appinop.Constants.GameMode.Turbo)
            {
                value = Gamemanager.Instance.diceDrawerController.GetCurrentValue();
            }
            else
            {
                value = diceController.GetDiceRollValue();
            }

            Debug.Log("Player Dice Value : " + value);
            diceController.onForceClicked(value);
            Dictionary<string, string> data = new Dictionary<string, string>() {
                { "playerId", player.id },
                { "value", value.ToString() }
            };
            await SocketServer.Instance.SendEvent("Dice_Clicked", data);
        }
    }

    public void EndTurn()
    {
        skullImage.color = Color.clear;
        skullImage.gameObject.SetActive(true);
        skullImage.DOColor(Color.white, 0.2f);

        SetName("Disconnected");

        profilePhoto.sprite = null;
        profilePhoto.DOColor(Color.black, 0.2f);

        diceController.ResetDice();
        HideHand();
        StopTimer();
        DeactivateTokens();
        diceRolled = false;
        tokenMoved = true;
    }
    private void onDiceRolled(int value)
    {

        diceRolled = true;
        StopTimer();

        //diceController.diceRolled.RemoveListener(onDiceRolled);
        ActivateTokens(value);

        if (GetActiveTokenCount() == 0)
        {
            diceController.ResetDice();
            player.FinishPlayerTurn();
            Gamemanager.Instance.TurnFinished(player);

        }
        else if (GetActiveTokenCount() == 1)
        {
            onMoveToken(GetForceToken(), true);
        }

        else
        {
            StartTimer();
        }

        HideHand();
    }

    private void onMoveToken(PlayerToken token, bool forced = false)
    {
        if (hasAuthority && !forced)
        {
            Dictionary<string, string> data = new Dictionary<string, string>() {
                { "playerId", player.id },
                { "tokenId", token.ID.ToString() }
            };
            SocketServer.Instance.SendEvent("MoveToken", data);
        }

        StopTimer();
        DeactivateTokens();
        diceRolled = false;
        tokenMoved = true;
        // Debug.Log("Token Move : " + diceController.diceRollVals.Last());
        //Move Token
        TokenMovement.Instance.MoveToken(player, token, diceController.diceRollVals.Last(), () => diceController.ResetDice());

    }

    private async void timerRunout(bool miss = true)
    {
        if (!diceRolled)
        {
            diceController.onClicked();
        }


        if (diceRolled && !tokenMoved)
        {

            // Debug.Log("Force Move Token");
            onMoveToken(GetForceToken());
            if (miss)
            {
                if (hasAuthority)
                {
                    ReduceMisstry();
                    await SocketServer.Instance.SendEvent("Turn_Missed", new Dictionary<string, string>() { { "playerId", player.id } });
                }
            }
        }
    }

    public void ReduceMisstry()
    {
        UpdateMissTries(player.ReduceTry());
    }
    private PlayerToken GetForceToken()
    {
        if (player.tokens.Any())
        {
            PlayerToken forceToken = player.tokens.OrderByDescending(token => token.index).FirstOrDefault(t => t.canMove);
            return forceToken;
        }

        return null;
    }
    private int GetActiveTokenCount()
    {
        return player.tokens.FindAll(t => t.canMove).Count;
    }
    private void ActivateTokens(int value)
    {
        foreach (var token in player.tokens)
        {
            if (value < 6)
            {
                if (token.index > 0 && token.index + value <= player.section.playerPath.Count)
                {
                    token.ActivateToken(hasAuthority);
                    token.onTokenClicked.AddListener(onMoveToken);
                }
            }
            else
            {
                if (token.index + value <= player.section.playerPath.Count)
                {
                    token.ActivateToken(hasAuthority);
                    token.onTokenClicked.AddListener(onMoveToken);
                }
            }
        }
    }
    public void DeactivateTokens()
    {
        foreach (var token in player.tokens)
        {
            token.DeactivateToken();
            token.onTokenClicked.RemoveAllListeners();
        }
    }

    private void ShowHand()
    {
        handImage.gameObject.SetActive(true);
        handTween = handImage.DOLocalMoveX(handPos.x - 20f, Appinop.Constants.THandMovement);
        handTween.SetLoops(-1, LoopType.Yoyo);
    }
    private void HideHand()
    {
        handImage.gameObject.SetActive(false);
        handImage.localPosition = handPos;
        handTween.Kill();
    }

    private void UpdateMissTries(int val)
    {
        AudioManager.Instance.PlayEffect("Beep");

        int exc = 3 - val;

        for (int i = 1; i <= mistryIndicators.Count; i++)
        {
            if (i > exc)
            {

                mistryIndicators[i - 1].sprite = missImage;
            }
        }
    }

    private void onSocketDiceClicked(Dictionary<string, string> data)
    {
        if (player.currentState == LudoPlayer.LudoPlayerState.Turn)
        {
            diceRolled = true;
            int value = int.Parse(data["value"]);
            diceController.onForceClicked(value);
            Debug.Log("Opponent Dice Value : " + data["value"]);
        }
    }

    private void onSocketDiceRolled(Dictionary<string, string> data)
    {
        if (player.currentState == LudoPlayer.LudoPlayerState.Turn)
        {
            Debug.Log("Dice Clicked : " + data["playerId"]);
        }

    }
    private void onSocketTokenMoved(Dictionary<string, string> data)
    {
        if (player.currentState == LudoPlayer.LudoPlayerState.Turn && data["playerId"] == player.id)
        {
            PlayerToken token = player.tokens.Find(t => t.ID == int.Parse(data["tokenId"]));
            onMoveToken(token);
        }

    }

}