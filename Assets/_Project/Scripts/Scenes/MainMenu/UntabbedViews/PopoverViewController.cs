using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Rummy;
using UnityEngine;

public class PopoverViewController : SingletonWithoutGameobject<PopoverViewController>
{
    [field: SerializeField] public EditProfilePopoverView EditProfilePopover { get; private set; }
    [field: SerializeField] public SettingsPopoverView SettingsPopover { get; private set; }

    [field: Header("Settings Sub Pages")]
    [field: SerializeField] public ChangeLanguagePopoverView ChangeLanguagePopover { get; private set; }
    [field: SerializeField] public CompleteKYCPopoverView completeKYCPopover { get; private set; }
    [field: SerializeField] public PanCardKYCPopoverView panCardKYCPopover { get; private set; }
    [field: SerializeField] public DLKYCPopoverView dLKYCPopoverView { get; private set; }
    [field: SerializeField] public AadharKYCPopoverView aadharKYCPopoverView { get; private set; }

    [field: Space]
    [field: SerializeField] public LudoModesPopoverView ludoModesPopover { get; private set; }

    [field: Header("Ludo Sub Pages")]
    [field: SerializeField] public LudoContestPopoverView ludoContestPopover { get; private set; }
    [field: SerializeField] public LudoConfirmationPopoverView ludoConfirmationPopover { get; private set; }
    [field: SerializeField] public LudoMatchFindingPopoverView ludoMatchFindingPopover { get; private set; }
    [field: Header("Rummy Sub Pages")]
    [field: SerializeField] public RummyModesPopoverView rummyModesPopover { get; private set; }
    [field: SerializeField] public RummyContestPopoverView rummyContestPopover { get; private set; }

    [field: Space]
    [field: SerializeField] public AddNamePopoverView addNamePopover { get; private set; }
    [field: SerializeField] public GameHistoryPopoverView gameHistoryPopover { get; private set; }
    [field: SerializeField] public AllGameHistoryPopover allGameHistoryPopover { get; private set; }

    [field: Space]
    [field: SerializeField] public TransactionsPopoverView transactionsPopover { get; private set; }
    [field: SerializeField] public WithdrawPopoverView withdrawPopover { get; private set; }
    [field: SerializeField] public AddBankAccPopoverView addBankAccPopover { get; private set; }
    [field: SerializeField] public AddUPIAccPopoverView addUPIAccPopover { get; private set; }
    [field: SerializeField] public LudoGameRulesPopover ludoGameRules { get; private set; }
    [field: SerializeField] public DepositCalcPopover depositCalc { get; private set; }
    [field: SerializeField] public WithdrawCalcPopover withdrawCalc { get; private set; }
    private Stack<PopoverView> viewStack = new Stack<PopoverView>();
    private List<PopoverView> views = new List<PopoverView>();

    public int ViewStackCount => viewStack.Count;

    private void Awake()
    {
        PopoverView[] _views = FindObjectsByType(typeof(PopoverView), FindObjectsInactive.Include, FindObjectsSortMode.None) as PopoverView[];
        views.AddRange(_views.ToList());
        views.ForEach(view => view.gameObject.SetActive(false));
    }
    public void GoBack(bool dataUpdated = false)
    {
        if (viewStack.Count > 0)
        {
            viewStack.Pop().Hide();
        }

        if (viewStack.Count > 0)
        {
            viewStack.Last().OnFocus(dataUpdated);
        }
    }

    public void GoBackTo(int level, bool dataUpdated = false)
    {
        // Ensure levels is a valid number
        if (level <= 0)
        {
            Debug.LogWarning("Levels should be greater than 0", this.gameObject);
            return;
        }

        for (int i = 0; i < level; i++)
        {
            if (viewStack.Count > 0)
            {
                viewStack.Pop().Hide();
            }
            else
            {
                break;
            }
        }

        if (viewStack.Count > 0)
        {
            viewStack.Last().OnFocus(dataUpdated);
        }
    }

    public void Show(PopoverView view)
    {
        view.Show();
        viewStack.Push(view);
    }

    public void Show(PopoverView view, params KeyValuePair<string, object>[] args)
    {
        view.Show(args);
        viewStack.Push(view);
    }
}

