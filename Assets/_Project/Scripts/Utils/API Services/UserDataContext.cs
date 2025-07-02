using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class UserDataContext : SingletonWithGameobject<UserDataContext>
{
    public UserData UserData;
    public ProfileData profileData = new ProfileData();

    [HideInInspector]
    public UnityEvent<ProfileData> ProfileDataChanged;
    [HideInInspector]
    public UnityEvent<UserData> UserdataChanged;

    public async Task<bool> Initialize()
    {
        return await fetchData();
    }

    public async Task<bool> RefreshData()
    {
        return await fetchData();
    }

    private async Task<bool> fetchData()
    {
        var responce = await APIServices.Instance.GetAsync<User>(APIEndpoints.userProfile, includeAuthorization: true);
        if (responce != null && responce.success)
        {
            UserData = responce.data;
            UserdataChanged?.Invoke(UserData);
            UpdateProfileData();
            return true;
        }
        else
        {
            Debug.LogError("Unable to Get User Data in Context");
            UnityNativeToastsHelper.ShowShortText("Something went Wrong.");
            return false;
        }
    }

    private void UpdateProfileData()
    {
        profileData.photo = ProfilePhotoHelper.Instance.GetProfileSprite(this.UserData.profilePhotoIndex);
        profileData.backgroundColor = ProfilePhotoHelper.Instance.GetBackdropColor(this.UserData.backdropIndex);
        profileData.fullName = this.UserData.username;
        profileData.phNo = this.UserData.mobileNumber;
        ProfileDataChanged?.Invoke(profileData);
    }

    private void OnDestroy()
    {
        ProfileDataChanged.RemoveAllListeners();
        UserdataChanged.RemoveAllListeners();
    }

}

public class ProfileData
{
    public Sprite photo;
    public Color backgroundColor;
    public string fullName;
    public string phNo;
}
