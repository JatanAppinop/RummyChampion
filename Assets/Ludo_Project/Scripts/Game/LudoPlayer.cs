using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class LudoPlayer : MonoBehaviour
{
    // Public Attributes
    [field: SerializeField] public string id { get; private set; }
    public void SetId(string _id) => id = _id;
    public BoardColorsUtils.BoardColors color;
    public PlayerSection section;
    public PlayerControls playerControls;
    public int Mistries = 0;
    public LudoPlayerState currentState;
    public PlayerType playerType;
    public List<PlayerToken> tokens = new List<PlayerToken>();
    public UserData userData;
    public int moves = 0;
    public int points = 0;

    public int oldPoints = 0;

    private bool markNext = false;
    public enum LudoPlayerState
    {
        Waiting,
        Turn,
        Playing
    }
    public enum PlayerType
    {
        Human,
        Multiplayer,
        Bot
    }

    public void CreateTokens()
    {
        for (int i = 0; i < section.tokenPositions.Count; i++)
        {
            RectTransform transform = section.tokenPositions[i];
            PlayerToken playerToken = Instantiate(Resources.Load<PlayerToken>("Token"), transform, false);
            playerToken.gameObject.name = ("P " + (this.id + 1) + " Token " + (i + 1));
            playerToken.SetTokenColor(color);
            playerToken.ID = i;
            RectTransform tokenTransform = playerToken.GetComponent<RectTransform>();
            tokenTransform.localPosition = Vector2.zero;
            tokenTransform.localScale = Vector2.zero;
            tokens.Add(playerToken);
        }
    }

    public void AnimateToken()
    {
        StartCoroutine(StartPlayerTokensAnimation());
    }

    IEnumerator StartPlayerTokensAnimation()
    {
        yield return new WaitForSeconds(0.5f);
        foreach (var token in tokens)
        {
            token.GetComponent<RectTransform>().DOScale(Vector3.one, 0.2f).SetEase(Ease.InBounce);
            yield return new WaitForSeconds(0.05f);
        }
    }

    public void StartPlayerTurn()
    {

        Debug.Log("Total Moves :" + moves + name);

        if (Gamemanager.Instance.gameMode == Appinop.Constants.GameMode.Turbo)
        {
            if (moves >= Appinop.Constants.TotalNumberMoved)
            {
                Gamemanager.Instance.TurnFinished(this);
                return;
            }
        }


        Gamemanager.Instance.movesLeftController.UpdateMoves(Gamemanager.Instance.diceDrawerController.GetRemainingMoves());
        currentState = LudoPlayerState.Turn;
        markNext = true;
        playerControls.StartTurn();
        section.HighlightSection();
        moves++;

    }

    public void FinishPlayerTurn()
    {

        if (Gamemanager.Instance.gameMode == Appinop.Constants.GameMode.Turbo && playerControls.hasAuthority && markNext)
        {
            Debug.Log("Mark Next for : " + this.name);
            Gamemanager.Instance.diceDrawerController.MarkNextActive();
            markNext = false;
            Debug.Log("Mark Next False");

        }

        currentState = LudoPlayerState.Waiting;

        section.StopHighlight();
        Debug.Log(playerControls.hasAuthority ? "Player Turn Finished" : "Opponent Turn Finished");

    }

    public void UpdatePoints()
    {
        CalculatePoints();
        section.UpdatePoints(points, playerControls.GetDiceRollVals().Last());
    }

    public void CalculatePoints()
    {
        oldPoints = points;
        points = 0;
        tokens.ForEach(t => points += (t.index - 1));
        if (tokens.Count < 4)
        {
            points += (4 - tokens.Count) * 56 * 2;
        }

    }

    public async Task GetPlayerData()
    {
        var userResponce = await APIServices.Instance.GetAsync<User>(APIEndpoints.getUser + id);
        if (userResponce != null && userResponce.success)
        {
            userData = userResponce.data;
            playerControls.SetName(userData.username);
        }
        else
        {
            UnityNativeToastsHelper.ShowShortText(userResponce.message);
        }

    }

    public void UpdatePlayerData()
    {
        playerControls.SetName(userData.username);
        playerControls.SetPhoto(ProfilePhotoHelper.Instance.GetProfileSprite(userData.profilePhotoIndex));
        playerControls.SetBackdrop(ProfilePhotoHelper.Instance.GetBackdropColor(userData.backdropIndex));
    }
    public int ReduceTry()
    {
        return ++Mistries;
    }
}
