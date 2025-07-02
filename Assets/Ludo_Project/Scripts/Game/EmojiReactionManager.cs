using System.Collections;
using System.Collections.Generic;
using AssetKits.ParticleImage;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.Events;
using System;

public class EmojiReactionManager : MonoBehaviour
{
    [SerializeField] List<Sprite> emojiSprites;

    [Header("References")]
    [SerializeField] List<ParticleImage> particleImages;
    [SerializeField] Image blocker;
    [SerializeField] RectTransform drawer;
    [SerializeField] Button emojiButton;
    [SerializeField] Image timerImage;
    [SerializeField] float timer;

    //Private Members
    [SerializeField] Vector2 closedDrawerPosition;




    public void Initialize()
    {
        this.gameObject.SetActive(true);
        drawer.gameObject.SetActive(true);
        _closeDrawer();
        StartCoroutine(StartEmojiButtonTimer());
        SocketServer.Instance.onEmojiReaction.AddListener(OnEmojiReactionReceived);
    }

    private void OnEmojiReactionReceived(Dictionary<string, string> data)
    {
        int index = int.Parse(data["value"]);
        string playerId = data["playerId"];
        LudoPlayer player = Gamemanager.Instance.ludoPlayers.Find(p => p.id == playerId);

        if (index < emojiSprites.Count && player != null)
        {

            int playerIndex = Gamemanager.Instance.playerControls.IndexOf(player.playerControls);
            Debug.LogWarning("Player Index : " + playerIndex);
            Debug.LogWarning("Imoji Index : " + index);

            if (playerIndex != -1)
            {
                particleImages[playerIndex].sprite = emojiSprites[index];
                particleImages[playerIndex].Play();
            }
        }
    }

    IEnumerator StartEmojiButtonTimer()
    {
        float timeElapsed = 0f;
        float updateInterval = 0.1f;
        timerImage.gameObject.SetActive(true);
        emojiButton.interactable = false;

        while (timeElapsed < timer)
        {
            yield return new WaitForSeconds(updateInterval);
            timerImage.fillAmount = ((1f / timer) * timeElapsed);
            timeElapsed += updateInterval;
        }

        timerImage.fillAmount = 1f;
        timerImage.gameObject.SetActive(false);
        emojiButton.interactable = true;

    }

    public void OpenDrawer()
    {
        _openDrawer();
    }
    public void ReactionClicked(int index)
    {
        Dictionary<string, string> data = new Dictionary<string, string>() {
                { "playerId", UserDataContext.Instance.UserData._id },
                { "value", index.ToString() }
            };

        SocketServer.Instance.SendEvent("emoji_sent", data);

        _closeDrawer();
        StartCoroutine(StartEmojiButtonTimer());
    }
    public void CloseDrawer()
    {
        _closeDrawer();
    }


    private void _openDrawer()
    {
        drawer.DOAnchorPos(new Vector2(0f, 60f + 300f), 0.1f).SetEase(Ease.InSine);
        blocker.enabled = true;
        blocker.DOFade(0.5f, 0.1f);
    }

    private void _closeDrawer()
    {
        drawer.DOAnchorPos(closedDrawerPosition, 0.1f).SetEase(Ease.OutSine);
        blocker.DOFade(0, 0.1f).OnComplete(() => blocker.enabled = false);
    }

}
