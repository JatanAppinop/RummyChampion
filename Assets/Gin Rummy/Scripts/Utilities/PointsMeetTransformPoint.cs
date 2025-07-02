using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointsMeetTransformPoint : MonoBehaviour {
    
    private ScoreBall scoreBall;
    int scoreBallCounter;

    private void Awake()
    {
        scoreBall = transform.Find("CircleBG").GetComponent<ScoreBall>();
        scoreBall.Hide();
    }

 

    public void TryRunMyScoreBall()
    {
        scoreBallCounter++;
        if (scoreBallCounter == 2)
        {
            scoreBallCounter = 0;
            RoundEndScreen.instance.TransferPoints(scoreBall);
        }
    }


}
