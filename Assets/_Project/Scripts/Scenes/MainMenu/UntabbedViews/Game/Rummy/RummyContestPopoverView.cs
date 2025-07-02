using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Appinop;

public class RummyContestPopoverView : PopoverView
{
    [SerializeField] private bool isLoaded = false;
    private RectTransform rectTransform;

    [Header("References")]
    [SerializeField] TextMeshProUGUI GameTitle;
    [SerializeField] Image ModeIcon;
    [SerializeField] TextMeshProUGUI ModelTitle;
    [SerializeField] TextMeshProUGUI OnlinePlayersLabel;
    [SerializeField] List<ScrollRect> scrollViews;
    [Header("Tab Views")]
    [SerializeField] List<AnimatedView> pages;

    [Header("Contest Cells")]
    [SerializeField] List<LudoContestButton> TwoPlayerCells;
    [SerializeField] List<LudoContestButton> FourPlayerCells;
    [SerializeField] TextMeshProUGUI nothingToShowTwoPlayer;
    [SerializeField] TextMeshProUGUI nothingToShowFourPlayer;

    [Header("Players Choice")]
    [SerializeField] List<PlayersSelectButton> playerChoiceButtons;
    [SerializeField] PlayersSelectButton playerChoiceSelection;

    [Header("Game Modes")]
    [SerializeField] List<GameModeData> modes;

    private GameModeData currentGameMode;
    private string selectedMode;
    private TabTransitionManager transitionManager;


    private void Awake()
    {
        this.rectTransform = this.transform as RectTransform;
        transitionManager = new TabTransitionManager(pages, 0);


    }
    private void Start()
    {
    }
    public override void Hide()
    {
        rectTransform.MoveOutOfScreen(direction: Appinop.RectTransformExtensions.Direction.Left, Animate: true, onComplete: () =>
        {
            ResetView();
            this.gameObject.SetActive(false);
        });
    }

    public override void Show(params KeyValuePair<string, object>[] args)
    {
        if (!isLoaded)
        {
            isLoaded = true;

            OnlinePlayersContext.Instance.OnDataUpdated.AddListener(OnOnlinePlayersDataUpdated);

            playerChoiceButtons.ForEach((b) =>
            {
                b.OnSelect.AddListener(OnPlayerChoiceButtonSelected);
            });

        }

        this.gameObject.SetActive(true);
        rectTransform.MoveOutOfScreen(direction: Appinop.RectTransformExtensions.Direction.Left);
        this.gameObject.SetActive(true);
        rectTransform.MoveToPosition(Vector2.zero, duration: 0.2f);


        selectedMode = args.GetValueByKey<string>(Appinop.Constants.KModeSelect);



        playerChoiceButtons.ForEach((b) =>
            {
                b.Deselect();
            });

        playerChoiceSelection = playerChoiceButtons.First();
        playerChoiceSelection.Select(true);

        pages.ForEach((p) =>
            {
                p.rectTransform.gameObject.SetActive(false);
            });

        pages.First().rectTransform.gameObject.SetActive(true);

        UpdateTitles(selectedMode);

        UpdateList();
    }

    private void OnOnlinePlayersDataUpdated()
    {
        UpdateOnlineUsersData();
    }

    private void UpdateOnlineUsersData()
    {
        foreach (LudoContestButton playerCell in TwoPlayerCells)
        {
            if (playerCell.gameObject.activeSelf)
            {
                playerCell.UpdateOnlinePlayerCount(OnlinePlayersContext.Instance.GetOnlinePlayersCountForContest(playerCell.data._id));
            }
        }
        foreach (LudoContestButton playerCell in FourPlayerCells)
        {
            if (playerCell.gameObject.activeSelf)
            {
                playerCell.UpdateOnlinePlayerCount(OnlinePlayersContext.Instance.GetOnlinePlayersCountForContest(playerCell.data._id));
            }
        }

        OnlinePlayersLabel.SetText(OnlinePlayersContext.Instance.GetOnlinePlayersCountForMode(selectedMode).ToString());
    }

    private void UpdateTitles(string mode)
    {
        var selectedMode = modes.Find((m) => m.id.ToLower() == mode.ToLower());

        UpdateTitleObjects(selectedMode);
    }

    private void UpdateTitleObjects(GameModeData data)
    {
        currentGameMode = data;
        GameTitle.SetText(data.game);
        ModeIcon.sprite = data.icon;
        ModelTitle.SetText(data.titleName);
    }

    private void UpdateList()
    {
        string gameType;

        switch (selectedMode)
        {
            case "Points":
                gameType = "Points";
                break;
            case "Deals":
                gameType = "Deals";
                break;
            case "101":
                gameType = "101 Pool";
                break;
            case "201":
                gameType = "201 Pool";
                break;
            default:
                Debug.LogError($"Not able to Find the Mode : " + selectedMode);
                gameType = "Points";
                break;
        }


        List<TableData> filteredData = new List<TableData>();
        filteredData = DataContext.Instance.contestsData.FindAll(t => t.game == "Rummy" && t.gameMode == gameType && t.isActive);
        List<TableData> TwoPlayerData = filteredData.FindAll(t => t.gameType == "2 Player").OrderBy(t => t.totalBet).ToList();
        List<TableData> SixPlayerData = filteredData.FindAll(t => t.gameType == "6 Player").OrderBy(t => t.totalBet).ToList();

        UpdateTwoPlayerCells(TwoPlayerData);

        UpdateSixPlayerCells(SixPlayerData);

    }

    public void UpdateTwoPlayerCells(List<TableData> TwoPlayerData)
    {
        if (TwoPlayerData != null && TwoPlayerData.Count > 0)
        {
            int requiredButtonCount = TwoPlayerData.Count;

            int indexOffset = 0;
            if (TwoPlayerData.First().bet != 0)
            {
                TwoPlayerCells.First().gameObject.SetActive(false);
                indexOffset = 1;
            }

            if (requiredButtonCount < TwoPlayerCells.Count)
            {
                for (int i = (requiredButtonCount + indexOffset); i < TwoPlayerCells.Count; i++)
                {
                    TwoPlayerCells[i].gameObject.SetActive(false);
                }
            }
            else
            {

                Debug.LogError("Not Enough Cells for 2 Player");
            }


            for (int i = 0; i < TwoPlayerData.Count; i++)
            {
                TableData contest = TwoPlayerData[i];
                LudoContestButton button = TwoPlayerCells[i + indexOffset];
                button.onPressed.RemoveAllListeners();
                button.UpdateData(contest);
                button.onPressed.AddListener(ContestSelected);
                button.gameObject.SetActive(true);
            }
            nothingToShowTwoPlayer.gameObject.SetActive(false);


        }
        else
        {
            Debug.LogError("Data for 2 Player not Found");
            TwoPlayerCells.ForEach(x => x.gameObject.SetActive(false));
            nothingToShowTwoPlayer.gameObject.SetActive(true);
        }
    }
    public void UpdateSixPlayerCells(List<TableData> FourPlayerData)
    {
        if (FourPlayerData != null && FourPlayerData.Count > 0)
        {
            int requiredButtonCount = FourPlayerData.Count;

            int indexOffset = 0;
            if (FourPlayerData.First().bet != 0)
            {
                FourPlayerCells.First().gameObject.SetActive(false);
                indexOffset = 1;
            }

            if (requiredButtonCount < FourPlayerCells.Count)
            {
                for (int i = (requiredButtonCount + indexOffset); i < FourPlayerCells.Count; i++)
                {
                    FourPlayerCells[i].gameObject.SetActive(false);
                }
            }
            else
            {

                Debug.LogError("Not Enough Cells for 4 Player");
            }


            for (int i = 0; i < FourPlayerData.Count; i++)
            {
                TableData contest = FourPlayerData[i];
                LudoContestButton button = FourPlayerCells[i + indexOffset];
                button.onPressed.RemoveAllListeners();
                button.UpdateData(contest);
                button.onPressed.AddListener(ContestSelected);
                button.gameObject.SetActive(true);
            }
            nothingToShowFourPlayer.gameObject.SetActive(false);



        }
        else
        {
            Debug.LogError("Data for 4 Player not Found");
            FourPlayerCells.ForEach(x => x.gameObject.SetActive(false));
            nothingToShowFourPlayer.gameObject.SetActive(true);


        }
    }

    public void ContestSelected(LudoContestButton selectedButton)
    {
        var mMode = modes.Find((m) => m.id.ToLower() == selectedMode.ToLower());

        PopoverViewController.Instance.Show(PopoverViewController.Instance.ludoConfirmationPopover,
        new KeyValuePair<string, object>(Appinop.Constants.KContestId, selectedButton.data),
        new KeyValuePair<string, object>(Appinop.Constants.KGameMode, mMode.titleName)
        );
    }

    private void ResetView()
    {
        //Reset Scroll View Position
        scrollViews.ForEach(s => s.content.anchoredPosition = Vector2.zero);

    }

    private void OnPlayerChoiceButtonSelected(MultiToggleButton button)
    {
        if (playerChoiceSelection == button)
        {
            return;
        }
        playerChoiceSelection = button as PlayersSelectButton;

        playerChoiceButtons.ForEach((b) =>
        {
            if (b != button)
                b.Deselect();
        });

        transitionManager.MoveToView(pages[playerChoiceButtons.IndexOf(playerChoiceSelection)].ViewName);
        scrollViews[playerChoiceButtons.IndexOf(playerChoiceSelection)].content.anchoredPosition = Vector2.zero;

    }

    public void OnContestSelected(string contestID)
    {

        PopoverViewController.Instance.Show(PopoverViewController.Instance.ludoConfirmationPopover,
        new KeyValuePair<string, object>(Appinop.Constants.KContestId, contestID)
        );
    }

    public override void OnFocus(bool dataUpdated = false)
    {

    }

    private void RemoveListeners()
    {

    }

    [Serializable]
    internal class GameModeData
    {
        public string id;
        public string game;
        public string titleName;
        public Sprite icon;
    }
}
