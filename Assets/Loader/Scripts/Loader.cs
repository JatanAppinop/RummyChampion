using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loader : MonoBehaviour
{

    private static Loader instance;
    [SerializeField] private static SpinnerView spinnerView;

    public static Loader Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<Loader>();
                if (instance == null)
                {
                    spinnerView = Instantiate(Resources.Load<SpinnerView>("LoaderCanvas"));
                    instance = spinnerView.gameObject.AddComponent<Loader>();
                    spinnerView.name = "[ LoaderView ]";
                }
            }
            return instance;
        }
    }

    private void FindSpinnerView()
    {
        spinnerView = FindObjectOfType<SpinnerView>();
        if (spinnerView == null)
        {
            spinnerView = Resources.Load<SpinnerView>("LoaderCanvas");
        }
    }
    public void Show(string text = "")
    {
        // FindSpinnerView();
        spinnerView.showSpinnerView(text);
    }

    public void Hide()
    {
        spinnerView.HideSpinnerView();
    }

    public void UpdateStatus(string text = "")
    {
        spinnerView.showLoadingText(text);
    }

}
