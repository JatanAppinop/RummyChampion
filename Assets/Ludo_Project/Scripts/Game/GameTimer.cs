using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class GameTimer : MonoBehaviour
{
    [HideInInspector] public UnityEvent onTimerFinished;
    [SerializeField] TextMeshProUGUI timerLabel;
    [SerializeField] CanvasGroup timeLeftNoti;

    private float _timer;

    private IEnumerator timerRoutine;
    private Color startingColor;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    private void Awake()
    {
        startingColor = timerLabel.color;

    }
    public void Initialize(float timer)
    {

        this.gameObject.SetActive(true);
        _timer = timer;

        if (startingColor != null && startingColor != timerLabel.color)
            timerLabel.color = startingColor;
    }

    public void StartTimer()
    {
        timerRoutine = startTimer();
        StartCoroutine(timerRoutine);
    }

    public void StopTimer()
    {
        timerLabel.text = "0:00";
        if (timerRoutine != null)
            StopCoroutine(timerRoutine);
    }

    IEnumerator startTimer()
    {
        float elapsedTime = _timer;
        while (elapsedTime > 0)
        {
            // Calculate minutes and seconds
            int minutes = Mathf.FloorToInt(elapsedTime / 60);
            int seconds = Mathf.FloorToInt(elapsedTime % 60);

            // Update timer label
            timerLabel.text = string.Format("{0:00}:{1:00}", minutes, seconds);

            if (elapsedTime == 60)
            {
                timerLabel.color = Color.red;
                ShowTimeLeftLabel();
            }
            yield return new WaitForSeconds(1f);
            elapsedTime--;

        }
        onTimerFinished?.Invoke();
    }

    private void ShowTimeLeftLabel()
    {

        RectTransform rt = timeLeftNoti.transform as RectTransform;

        Sequence seq = DOTween.Sequence();

        seq.OnStart(() =>
        {
            timeLeftNoti.gameObject.SetActive(true);
        });

        seq.Append(timeLeftNoti.DOFade(1f, 0.5f))
           .Join(rt.DOAnchorPosY(rt.anchoredPosition.y - 145f, 0.5f));

        seq.AppendInterval(3f);

        seq.Append(rt.DOAnchorPosY(0f, 0.5f))
           .Join(timeLeftNoti.DOFade(0f, 0.5f));

        seq.OnComplete(() =>
        {
            timeLeftNoti.gameObject.SetActive(false);
        });

        seq.Play();
    }
}
