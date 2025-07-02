using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
public class ColorSelectionButton : MultiToggleButton
{
    [SerializeField] Button button;

    [SerializeField] GameObject tickmark;


    [field: Header("Data")]
    [field: SerializeField] public BoardColorsUtils.BoardColors color { get; private set; }
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
        tickmark.SetActive(true);
        if (!forced)
            OnSelect.Invoke(this);
    }
    public override void Deselect()
    {
        isSelected = false;
        tickmark.SetActive(false);
    }

    private void OnDestroy()
    {
        button.onClick.RemoveListener(onButtonSelect);

    }
}
