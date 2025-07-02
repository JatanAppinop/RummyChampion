using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameoverListCell : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI Rank;
    [SerializeField] TextMeshProUGUI Name;
    [SerializeField] TextMeshProUGUI WinAmt;
    [SerializeField] Image photo;

    private UserData userData;

    public void UpdateData(UserData data)
    {
        userData = data;
        Name.text = userData.username;
        photo.sprite = ProfilePhotoHelper.Instance.GetProfileSprite(userData.profilePhotoIndex);
    }

    public void UpdateRank(double r) => Rank.text = r.ToString() + ".";
    public void UpdateWinAmt(double a) => WinAmt.text = "₹" + a.ToString();
}
