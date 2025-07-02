using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class QuitConfirmation : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Image blocker;
    [SerializeField] RectTransform drawerRect;
    [SerializeField] Vector2 closedDrawerPosition;


    private void Awake()
    {
    }
    public void Show()
    {
        closeDrawer();
        this.gameObject.transform.parent.gameObject.SetActive(true);
        this.gameObject.SetActive(true);
        openDrawer();
    }
    private void openDrawer()
    {
        drawerRect.DOAnchorPos(Vector3.zero, 0.1f).SetEase(Ease.InSine);
        blocker.enabled = true;
        blocker.DOFade(0.5f, 0.1f);
    }
    private void closeDrawer()
    {
        drawerRect.DOAnchorPos(closedDrawerPosition, 0.1f).SetEase(Ease.OutSine);
        blocker.DOFade(0, 0.1f).OnComplete(() => blocker.enabled = false);
    }

    public void onYesPressed()
    {
        SocketServer.Instance.Disconnect();
        SceneManager.LoadScene((int)Scenes.MainMenu);
    }
    public void onNoPressed()
    {
        closeDrawer();
    }

}
