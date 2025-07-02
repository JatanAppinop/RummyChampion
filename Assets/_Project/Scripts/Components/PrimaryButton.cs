using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PrimaryButton : MonoBehaviour
{
    [SerializeField] Button button;
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] Image spinner;

    private IEnumerator spinnerRoutine;

    [Header("Public Event")]
    public UnityEvent OnButtonPressed;

    private void Awake()
    {
        button.onClick.AddListener(ButtonPressed);
        SetInteractable(true);
    }

    public void ButtonPressed()
    {
        OnButtonPressed?.Invoke();
    }


    public void SetInteractable(bool b)
    {

        spinner.gameObject.SetActive(!b);

        if (b)
        {
            button.interactable = true;
            text.gameObject.SetActive(true);
            spinner.gameObject.SetActive(false);
            button.interactable = true;
            spinner.gameObject.SetActive(false);
            if (spinnerRoutine != null)
                StopCoroutine(spinnerRoutine);
        }
        else
        {
            button.interactable = false;
            text.gameObject.SetActive(false);
            spinner.gameObject.SetActive(true);
            spinnerRoutine = SpinSpinner();
            if (gameObject.activeInHierarchy)
                StartCoroutine(spinnerRoutine);
        }
    }

    IEnumerator SpinSpinner()
    {
        while (true)
        {
            spinner.rectTransform.Rotate(Vector3.back * Time.deltaTime * 100f);
            yield return new WaitForSeconds(0.001f);
        }
    }
}
