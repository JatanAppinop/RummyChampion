using UnityEngine;
using TMPro;
using System;

public class AdvancedInputField : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private GameObject errorIcon;
    [SerializeField] private TextMeshProUGUI errorText;

    // Public events to mirror TMP_InputField's events
    public event Action<string> OnValueChanged;
    public event Action<string> OnFocus;
    public event Action<string> OnFocusLost;

    // Public accessor to directly get or set the input field's text
    public string text
    {
        get => inputField.text;
        set => inputField.text = value;
    }

    void Awake()
    {
        inputField.onValueChanged.AddListener(text =>
        {
            OnValueChanged?.Invoke(text);
            HandleValueChanged(text);
        });
        inputField.onSelect.AddListener(text =>
        {
            OnFocus?.Invoke(text);
            HandleFocus(text);
        });
        inputField.onDeselect.AddListener(text =>
        {
            OnFocusLost?.Invoke(text);
            HandleDeselect(text);
        });
        HideError(); // Optionally hide error on focus
    }

    private void HandleValueChanged(string text)
    {
        // Additional internal logic if needed
    }

    private void HandleFocus(string text)
    {
        HideError(); // Optionally hide error on focus
    }

    private void HandleDeselect(string text)
    {
        // Additional internal logic if needed on focus lost
    }

    // Public methods to show and hide errors
    public void ShowError(string message = "")
    {
        if (!string.IsNullOrEmpty(message))
            errorText.text = message;
        errorIcon.SetActive(true);
        errorText.gameObject.SetActive(true);
    }

    public void HideError()
    {
        errorIcon.SetActive(false);
        errorText.gameObject.SetActive(false);
    }

    public void Focus()
    {
        inputField.Select();
    }

    void OnDestroy()
    {
        // Clean up listeners
        inputField.onValueChanged.RemoveListener(HandleValueChanged);
        inputField.onSelect.RemoveListener(HandleFocus);
        inputField.onDeselect.RemoveListener(HandleDeselect);
    }
}

