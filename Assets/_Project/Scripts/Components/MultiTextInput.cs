using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class MultiTextInput : MonoBehaviour
{
    [SerializeField] List<TextMeshProUGUI> inputFields;
    [SerializeField] TextMeshProUGUI errorText;
    private TouchScreenKeyboard keyboard;
    public string text = "";
    // public string text
    // {
    //     set
    //     {
    //         text = value;
    //         UpdateDigitDisplays(value);
    //     }
    //     get => text;
    // }

    private void Awake()
    {
        HideError();
    }
    void Start()
    {
        Debug.Log("OTP Keyboard Opened");
        OpenKeyboard();
    }

    private void FixedUpdate()
    {
        if (keyboard != null && keyboard.status == TouchScreenKeyboard.Status.Visible)
        {
            if (text != keyboard.text)
            {
                text = keyboard.text;
                UpdateDigitDisplays(text);
            }
        }
    }


    public void InputTouched()
    {
        ToggleKeyboard();
    }

    public void Focus()
    {
        OpenKeyboard();
    }
    private void OpenKeyboard()
    {
        TouchScreenKeyboard.hideInput = true;
        keyboard = TouchScreenKeyboard.Open(text, TouchScreenKeyboardType.NumbersAndPunctuation, false, false, false, false, "Enter OTP", 6);
    }

    public void CloseKeyboard()
    {
        keyboard.active = false;
    }

    public void ToggleKeyboard()
    {
        if (keyboard != null && keyboard.active)
        {
            CloseKeyboard();
        }
        else
        {
            OpenKeyboard();
        }
    }

    public void SetText(string text)
    {
        this.text = text;
        UpdateDigitDisplays(text);
    }

    private void UpdateDigitDisplays(string input)
    {
        for (int i = 0; i < inputFields.Count; i++)
        {
            if (i < input.Length)
            {
                inputFields[i].text = input[i].ToString();
            }
            else
            {
                inputFields[i].text = "";
            }
        }
    }

    public void ResetInput()
    {
        text = "";
        UpdateDigitDisplays(text);
        OpenKeyboard();
    }

    public void ShowError(string message = "")
    {
        if (!string.IsNullOrEmpty(message))
            errorText.text = message;
        errorText.gameObject.SetActive(true);
    }

    public void HideError()
    {
        errorText.gameObject.SetActive(false);
    }
}
