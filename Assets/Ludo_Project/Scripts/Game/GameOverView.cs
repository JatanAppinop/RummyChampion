using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverView : MonoBehaviour
{



    public void OnBackPresses()
    {
        SceneManager.LoadScene((int)Scenes.MainMenu);
    }
}
