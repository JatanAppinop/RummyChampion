using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
public class PlayersSelectButton : MultiToggleButton
{
    [SerializeField] Button button;

    [Header("Background")]
    [SerializeField] Image background;
    [SerializeField] Color activeBGColor;
    [SerializeField] Color inactiveBGColor;
    [Header("Label")]
    [SerializeField] TextMeshProUGUI label;
    [SerializeField] Color activeLabelColor;
    [SerializeField] Color inactiveLabelColor;

    [field: Header("Data")]
    [field: SerializeField] public Appinop.Constants.PlayerCounts playerCounts { get; private set; }
    public override bool IsSelected { get { return isSelected; } }


    [SerializeField] private bool isSelected;
    private void Awake()
    {
        button.onClick.AddListener(onButtonSelect);
    }

    private void onButtonSelect()
    {
        Select();
    }
    public override void Select(bool forced = false)
    {
        isSelected = true;
        background.color = activeBGColor;
        label.color = activeLabelColor;
        if (!forced)
            OnSelect.Invoke(this);
    }
    public override void Deselect()
    {
        isSelected = false;
        background.color = inactiveBGColor;
        label.color = inactiveLabelColor;
    }

    private void OnDestroy()
    {
        button.onClick.RemoveListener(onButtonSelect);

    }
}
