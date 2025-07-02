using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardDetailView : MonoBehaviour
{

    [Header("References")]
    [SerializeField] LeaderboardCell cell;
    [SerializeField] List<ProfilePhoto> top3Photos;
    [SerializeField] List<LeaderboardCell> cells;
    [SerializeField] ScrollRect scrollRect;
    [SerializeField] GameObject nothingToShowLabel;

    public void Reset()
    {
        nothingToShowLabel.SetActive(true);
        scrollRect.gameObject.SetActive(false);
    }
    public void UpdateData(List<LeaderboardItem> leaderboardItems)
    {
        nothingToShowLabel.SetActive(true);
        nothingToShowLabel.GetComponent<TextMeshProUGUI>().SetText("Loading....");
        scrollRect.gameObject.SetActive(false);

        cells.ForEach(x => Destroy(x.gameObject));
        cells.Clear();

        if (leaderboardItems.Count > 0)
        {

            for (int i = 0; i < Math.Min(3, leaderboardItems.Count); i++)
            {

                if (leaderboardItems[i] != null)
                {
                    ProfileData userData = new ProfileData();
                    userData.fullName = leaderboardItems[i].username;
                    userData.photo = ProfilePhotoHelper.Instance.GetProfileSprite(leaderboardItems[i].profilePhotoIndex);
                    userData.backgroundColor = ProfilePhotoHelper.Instance.GetBackdropColor(leaderboardItems[i].backdropIndex);
                    top3Photos[i].UpdateData(userData);

                }

            }

            int objToSpawn = 0;
            if (leaderboardItems.Count > 10)
            {
                objToSpawn = 10;
            }
            else
            {
                objToSpawn = leaderboardItems.Count;
            }

            for (int i = 0; i < objToSpawn; i++)
            {
                LeaderboardCell newChild = Instantiate(cell, scrollRect.content);
                newChild.UpdateData(leaderboardItems[i], i + 1);
                cells.Add(newChild);
            }
            scrollRect.content.localPosition = Vector2.zero;
            nothingToShowLabel.SetActive(false);
            scrollRect.gameObject.SetActive(true);
        }
        else
        {
            nothingToShowLabel.GetComponent<TextMeshProUGUI>().SetText("Nothing to Show.");

        }

    }
}
