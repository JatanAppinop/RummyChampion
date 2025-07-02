using System.Collections;
using System.Collections.Generic;
using Appinop;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class NoInternetView : MonoBehaviour
{

    [Header("References")]
    [SerializeField] private Button RetryBtn;
    [SerializeField] private Image Spinner;
    [SerializeField] private Image overlay;
    [SerializeField] private RectTransform Popup;

    [SerializeField] private Vector3 popupPosition;



    private IEnumerator spinnerRoutine;


    [HideInInspector]
    public UnityEvent RetryPressed;


    private void Awake()
    {
        overlay.color = Color.clear;
        Spinner.gameObject.SetActive(false);
        RetryBtn.gameObject.SetActive(true);
        popupPosition = (Popup.transform as RectTransform).anchoredPosition;
        Popup.MoveOutOfScreen();

    }

    private void Start()
    {
        Popup.MoveToPosition(popupPosition);
        overlay.DOColor(new Color(0f, 0f, 0f, 0.7f), 0.2f);
    }
    public void OnRetryPressed()
    {
        disableBtn();
        StartCoroutine(SendRetryEvent());
    }

    public void EnableRetryButton()
    {
        enableBtn();
    }

    public void HidePopup()
    {
        overlay.DOColor(Color.clear, 0.2f);
        Popup.AnimateToBottom();
        Destroy(this, 0.6f);
    }

    private void enableBtn()
    {
        RetryBtn.gameObject.SetActive(true);
        Spinner.gameObject.SetActive(false);
        if (spinnerRoutine != null)
        {
            StopCoroutine(spinnerRoutine);
        }
    }

    private void disableBtn()
    {
        RetryBtn.gameObject.SetActive(false);
        Spinner.gameObject.SetActive(true);
        spinnerRoutine = SpinSpinner();
        StartCoroutine(spinnerRoutine);
    }
    IEnumerator SpinSpinner()
    {
        while (true)
        {
            Spinner.rectTransform.Rotate(Vector3.back * Time.deltaTime * 100f);
            yield return new WaitForSeconds(0.001f);
        }
    }

    IEnumerator SendRetryEvent()
    {
        yield return new WaitForSeconds(1f);
        RetryPressed?.Invoke();
    }
}
