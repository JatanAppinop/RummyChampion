using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProfilePhoto : MonoBehaviour
{
    [SerializeField] Image backdrop;
    [SerializeField] Image photo;
    [SerializeField] TextMeshProUGUI Label;
    [SerializeField] GameObject LabelObject;

    [SerializeField] bool showNameTag = true;

    private void Awake()
    {
        ShowNameTag(showNameTag);
    }

    public void UpdateData(ProfileData profileData)
    {
        backdrop.color = profileData.backgroundColor;
        photo.sprite = profileData.photo;
        backdrop.color = profileData.backgroundColor;
        backdrop.color = profileData.backgroundColor;

        UpdateBackdrop(profileData.backgroundColor);
        UpdatePhoto(profileData.photo);
        UpdateNameTag(profileData.fullName);
    }
    public void UpdateBackdrop(Color color)
    {
        backdrop.color = color;
    }

    public void UpdatePhoto(Sprite sprite)
    {
        photo.sprite = sprite;
    }
    public void UpdateNameTag(string name)
    {
        if (Label != null)
            Label.SetText(name);
    }

    public void ShowNameTag(bool show)
    {
        if (LabelObject != null)
            LabelObject.SetActive(show);
    }


}
