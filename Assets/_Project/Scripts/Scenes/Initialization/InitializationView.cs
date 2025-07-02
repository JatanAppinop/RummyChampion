using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InitializationView : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private TextMeshProUGUI loadStatusLabel;


    public void SetStatus(int value, string Status = "")
    {
        if (value <= 100 && value >= 0)
        {
            slider.value = value;
        }
        else
        {
            throw new InvalidDataException("Invalid Slider Range : " + value);
        }

        if (!String.IsNullOrEmpty(Status))
        {
            loadStatusLabel.SetText(Status);
        }
    }

}
