using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Appinop;
using TMPro;
using UnityEngine;

public class EditProfilePopoverView : PopoverView
{
    [SerializeField] private bool isLoaded = false;

    [Header("Views")]
    [SerializeField] private List<RectTransform> views;

    [Header("References")]
    [SerializeField] EditProfileTabGroup tabGroup;
    [SerializeField] ProfilePhoto profilePhoto;
    [SerializeField] List<EditProfilePhotoButton> profileButtons;
    [SerializeField] List<EditProfilePhotoButton> backdropButtons;
    [SerializeField] TMP_InputField nameLabel;
    [SerializeField] TMP_InputField phNoLabel;
    [SerializeField] PrimaryButton saveButton;
    [SerializeField] AdvancedInputField nameinput;

    public int profilePhotoIndex = 0;
    public int backdropIndex = 0;

    private RectTransform rectTransform;

    private void Awake()
    {
        this.rectTransform = this.transform as RectTransform;

        views[0].gameObject.SetActive(true);
        views[1].gameObject.SetActive(false);

    }
    private void Start()
    {
        // Debug.Log("Edit Profile Started");
    }
    public override void Hide()
    {
        rectTransform.MoveOutOfScreen(direction: Appinop.RectTransformExtensions.Direction.Left, Animate: true, onComplete: () =>
        {
            ResetView();
            this.gameObject.SetActive(false);

            //Remove
            profileButtons.ForEach(button => button.onSelect.RemoveListener(OnProfileButtonClicked));
            backdropButtons.ForEach(button => button.onSelect.RemoveListener(OnBackdropButtonClicked));
        });
    }

    public override void Show()
    {
        if (!isLoaded)
        {
            isLoaded = true;
            LoadProfileData();
        }

        if (!rectTransform)
        {
            rectTransform = this.transform as RectTransform;
        }

        rectTransform.MoveOutOfScreen(direction: Appinop.RectTransformExtensions.Direction.Left);
        this.gameObject.SetActive(true);
        rectTransform.MoveToPosition(Vector2.zero, duration: 0.2f);

        profilePhoto.UpdateData(UserDataContext.Instance.profileData);

        //AddListeners
        profileButtons.ForEach(button => button.onSelect.AddListener(OnProfileButtonClicked));
        backdropButtons.ForEach(button => button.onSelect.AddListener(OnBackdropButtonClicked));

        int userPhoto = UserDataContext.Instance.UserData.profilePhotoIndex;
        int userBackdrop = UserDataContext.Instance.UserData.backdropIndex;

        OnProfileButtonClicked(profileButtons[userPhoto]);
        OnBackdropButtonClicked(backdropButtons[userBackdrop]);

        nameLabel.text = UserDataContext.Instance.UserData.username;
        phNoLabel.text = Extentions.MaskMobile(UserDataContext.Instance.UserData.mobileNumber);

    }

    private void LoadProfileData()
    {

        for (int i = 0; i < profileButtons.Count(); i++)
        {
            profileButtons[i].UpdateBackdrop(ProfilePhotoHelper.Instance.GetBackdropColor(i));
            profileButtons[i].UpdateSprite(ProfilePhotoHelper.Instance.GetProfileSprite(i));
        }

        for (int i = 0; i < backdropButtons.Count(); i++)
        {
            backdropButtons[i].UpdateBackdrop(ProfilePhotoHelper.Instance.GetBackdropColor(i));
        }

    }


    private void OnProfileButtonClicked(EditProfilePhotoButton pressedButton)
    {

        for (int i = 0; i < profileButtons.Count; i++)
        {
            if (profileButtons[i] != pressedButton)
            {
                profileButtons[i].Deselect();
            }
            else
            {
                profileButtons[i].MarkSelect();
                profilePhotoIndex = i;
                profilePhoto.UpdatePhoto(ProfilePhotoHelper.Instance.GetProfileSprite(i));
            }

        }
    }
    private void OnBackdropButtonClicked(EditProfilePhotoButton pressedButton)
    {
        for (int i = 0; i < backdropButtons.Count; i++)
        {
            if (backdropButtons[i] != pressedButton)
            {
                backdropButtons[i].Deselect();
            }
            else
            {
                backdropButtons[i].MarkSelect();
                backdropIndex = i;
                profilePhoto.UpdateBackdrop(ProfilePhotoHelper.Instance.GetBackdropColor(i));

                for (int j = 0; j < profileButtons.Count(); j++)
                {
                    profileButtons[j].UpdateBackdrop(ProfilePhotoHelper.Instance.GetBackdropColor(i));
                }
            }
        }

    }
    private void ResetView()
    {
        views[0].gameObject.SetActive(true);
        views[0].anchoredPosition = Vector2.zero;
        views[1].gameObject.SetActive(false);
        saveButton.SetInteractable(true);
        tabGroup.ResetTab();
    }
    public void OnNextButtonPressed()
    {
        views[0].MoveOutOfScreen(Appinop.RectTransformExtensions.Direction.Left, true, () => views[0].gameObject.SetActive(false));
        views[1].MoveOutOfScreen(Appinop.RectTransformExtensions.Direction.Right);
        views[1].gameObject.SetActive(true);
        views[1].MoveToPosition(Vector2.zero, duration: 0.3f);
    }

    public async void OnSaveButtonPressed()
    {
        saveButton.SetInteractable(false);

        if (nameinput.text == "")
        {
            nameinput.ShowError("Enter your Name");
            saveButton.SetInteractable(true);
            return;
        }

        string requestBodyJson = "{\"username\":\"" + nameinput.text + "\", \"profilePhotoIndex\":\"" + profilePhotoIndex.ToString() + "\", \"backdropIndex\":\"" + backdropIndex.ToString() + "\"}";

        var responce = await APIServices.Instance.PutAsync<SimpleBaseModel>(APIEndpoints.editProfile, requestBodyJson);

        if (responce != null && responce.success)
        {
            UnityNativeToastsHelper.ShowShortText("Profile Updated.");
            Debug.Log("Update Profile successful : " + responce.message);
            await UserDataContext.Instance.RefreshData();
            PopoverViewController.Instance.GoBack();
        }
        else
        {
            string errorMessage = responce != null ? responce.message : "Failed to Update Profile";
            UnityNativeToastsHelper.ShowShortText(errorMessage);
            Debug.LogError("Update Profile Unsuccessfull. Error : " + errorMessage);
            saveButton.SetInteractable(true);
        }

    }

    public override void OnFocus(bool dataUpdated = false)
    {
    }

}
