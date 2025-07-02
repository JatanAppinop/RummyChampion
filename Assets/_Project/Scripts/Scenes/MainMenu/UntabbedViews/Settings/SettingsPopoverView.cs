using System;
using System.Collections;
using System.Collections.Generic;
using Appinop;
using SecPlayerPrefs;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingsPopoverView : PopoverView
{
    [SerializeField] private bool isLoaded = false;
    [SerializeField] TextMeshProUGUI versionLabel;
    [SerializeField] TextMeshProUGUI companyName;

    [SerializeField] ToggleButton soundEffectToggleBtn;
    [SerializeField] ToggleButton musicToggleBtn;
    private RectTransform rectTransform;

    private void Awake()
    {
        this.rectTransform = this.transform as RectTransform;
        versionLabel.SetText("Version : " + Application.version);
    }
    private void Start()
    {
        Debug.Log("Settings Page Started");

    }

    private void UpdateLabels()
    {
        string text = companyName.text;

        text = text.Replace("{{companyName}}", Appinop.Constants.CompanyName);
        companyName.SetText(text);
    }
    public override void Hide()
    {
        rectTransform.MoveOutOfScreen(direction: Appinop.RectTransformExtensions.Direction.Left, Animate: true, onComplete: () =>
        {
            this.gameObject.SetActive(false);
            ResetView();
        });
    }


    public override void Show()
    {
        if (!isLoaded)
        {
            isLoaded = true;
            UpdateLabels();
        }

        if (!rectTransform)
        {
            rectTransform = this.transform as RectTransform;
        }

        rectTransform.MoveOutOfScreen(direction: Appinop.RectTransformExtensions.Direction.Left);
        this.gameObject.SetActive(true);
        rectTransform.MoveToPosition(Vector2.zero, duration: 0.2f);

        musicToggleBtn.updateButton(!AudioManager.Instance.IsMusicMuted());
        soundEffectToggleBtn.updateButton(!AudioManager.Instance.IsSFXMuted());
    }

    public override void OnFocus(bool dataUpdated = false)
    {
        Debug.Log("Focused Back. Data Update : " + dataUpdated);
    }

    private void ResetView()
    {

    }


    public void onHelpButtonPressed()
    {
        Application.OpenURL(Appinop.Constants.HelpURL);
    }
    public void onAboutButtonPressed()
    {
        Application.OpenURL(Appinop.Constants.AboutUrl);
    }
    public void onLogoutButtonPressed()
    {
        Debug.Log("Logout Pressed");
        AlertSlider.Instance.Show("Are you sure you want to Logout ?", "Logout", "Cancel")
        .OnPrimaryAction(() =>
        {
            OnlinePlayersContext.Instance.OnDataUpdated.RemoveAllListeners();
            UserDataContext.Instance.ProfileDataChanged.RemoveAllListeners();
            UserDataContext.Instance.UserdataChanged.RemoveAllListeners();
            DataContext.Instance.OnContestDataUpdated.RemoveAllListeners();
            SecurePlayerPrefs.SetString(Appinop.Constants.KUserToken, "");
            SceneManager.LoadScene(0);
        });

    }
    public void onDeleteButtonPressed()
    {
        AlertSlider.Instance.Show($"Are you sure you want to Delete your Account ? \nThis Action is irreversible. Your Account will be deleted in 15 Days.", "Delete Account", "Cancel")
        .OnPrimaryAction(() =>
        {
            OnlinePlayersContext.Instance.OnDataUpdated.RemoveAllListeners();
            UserDataContext.Instance.ProfileDataChanged.RemoveAllListeners();
            UserDataContext.Instance.UserdataChanged.RemoveAllListeners();
            DataContext.Instance.OnContestDataUpdated.RemoveAllListeners();
            SecurePlayerPrefs.SetString(Appinop.Constants.KUserToken, "");
            SceneManager.LoadScene(0);
        });

    }


    public void onMusicToggleClicked()
    {
        AudioManager.Instance.ToggleMusic();
    }
    public void onSoundEffectToggleClicked()
    {
        AudioManager.Instance.ToggleSFX();
    }

}
