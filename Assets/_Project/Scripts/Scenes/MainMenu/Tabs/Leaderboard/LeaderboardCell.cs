using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LeaderboardCell : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nameLabel;
    [SerializeField] TextMeshProUGUI numLabel;
    [SerializeField] TextMeshProUGUI winLabel;

    public async void UpdateData(LeaderboardItem data, int index)
    {
        nameLabel.SetText(data.username);

        numLabel.SetText("#" + index.ToString());
        winLabel.SetText(data.wins.ToString());
    }

}
