using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Appinop;

public class AlertSlider : MonoBehaviour
{
    [SerializeField] Image backdrop;
    [SerializeField] Button backdropButton;
    [SerializeField] RectTransform drawer;
    [Header("Label")]
    [SerializeField] TextMeshProUGUI label;

    [Header("Primary Button")]
    [SerializeField] Button primaryButton;
    [SerializeField] TextMeshProUGUI primaryButtonLabel;

    [Header("Secondary Button")]
    [SerializeField] Button secondaryButton;
    [SerializeField] TextMeshProUGUI secondaryButtonLabel;

    private Action primaryAction;
    private Action secondaryAction;

    private static AlertSlider instance;

    public static AlertSlider Instance
    {
        get
        {
            instance = Instantiate(Resources.Load<AlertSlider>("AlertCanvas"));
            instance.name = "[ Alert Slider ]";
            // if (instance == null)
            // {
            //     instance = FindObjectOfType<AlertSlider>();
            //     if (instance == null)
            //     {
            //         instance = Instantiate(Resources.Load<AlertSlider>("AlertCanvas"));
            //         instance.name = "[ Alert Slider ]";
            //     }
            // }
            return instance;
        }
    }

    private void Awake()
    {

        primaryButton.onClick.AddListener(onPrimaryButtonPressed);
        secondaryButton.onClick.AddListener(onSecondaryButtonPressed);
        backdropButton.onClick.AddListener(onBackdropButtonPressed);

        drawer.anchoredPosition = new Vector2(0f, -drawer.rect.height);
        backdrop.color = Color.clear;
        backdrop.enabled = false;

    }

    public AlertSlider Show(string title, string primaryButton, string secondaryButton = "")
    {

        if (String.IsNullOrEmpty(secondaryButton))
        {
            secondaryButtonLabel.gameObject.SetActive(false);
            this.secondaryButton.gameObject.SetActive(false);
        }

        label.text = title;
        primaryButtonLabel.text = primaryButton;
        secondaryButtonLabel.text = secondaryButton;

        drawer.MoveToPosition(Vector2.zero, duration: 0.2f);
        backdrop.enabled = true;
        backdrop.DOColor(new Color(0f, 0f, 0f, 0.7f), 0.2f);
        return this;
    }

    public AlertSlider Hide()
    {
        drawer.MoveToPosition(new Vector2(0f, -drawer.rect.height), duration: 0.2f);
        backdrop.DOColor(Color.clear, 0.2f).OnComplete(() =>
        {
            backdrop.enabled = false;
            Destroy(this.gameObject, 0.01f);
        });
        return this;
    }

    public AlertSlider OnPrimaryAction(Action action)
    {
        primaryAction = action;
        return this;
    }
    public AlertSlider OnSecondaryAction(Action action)
    {
        secondaryAction = action;
        return this;
    }

    private void onPrimaryButtonPressed()
    {
        Hide();
        primaryAction?.Invoke();

    }

    private void onSecondaryButtonPressed()
    {
        Hide();
        secondaryAction?.Invoke();

    }
    private void onBackdropButtonPressed()
    {
        Hide();
    }


}
