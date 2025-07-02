using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HomeScreenController : PageController
{
    [SerializeField] private bool isLoaded = false;
    [Space]
    [SerializeField] ProfilePhoto profilePhoto;

    private void Awake()
    {
        OnShown();
        ShowAddNamePopover();
    }


    public override void OnShown()
    {
        if (!isLoaded)
        {
            isLoaded = true;
            UserDataContext.Instance.ProfileDataChanged.AddListener(OnProfileDataChanged);
            UpdateProfile(UserDataContext.Instance.profileData);
            AudioManager.Instance.PlayMusic("MainMenu");
        }

        UserDataContext.Instance.RefreshData();
    }
    private void ShowAddNamePopover()
    {
        if (string.IsNullOrEmpty(UserDataContext.Instance.UserData.username))
        {
            PopoverViewController.Instance.Show(PopoverViewController.Instance.addNamePopover);
        }
    }
    private void OnProfileDataChanged(ProfileData data)
    {
        UpdateProfile(data);
    }

    private void UpdateProfile(ProfileData data)
    {
        profilePhoto.UpdateData(data);
    }

    public void OnRummyButtonPresses()
    {
        SceneManager.LoadScene((int)Scenes.Rummy);
    }

    public void ComingsoonButton()
    {
        AlertSlider.Instance.Show("Coming Soon", "OK");
    }
}
