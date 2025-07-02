using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class SplashScreen : MonoBehaviour {
	private Text textField;
    private int minYear = 2016;

	private void Start() {
        int year = DateTime.Now.Year;
        year = Mathf.Max(year, minYear);
        textField = transform.Find("Text").GetComponent<Text>();
		textField.text = Application.productName + " is trademark of University of Games. \n © 2014-" + year + " University of Games Sp. z o.o. All rights reserved.";
        Invoke("LoadScene", 2.0f);
    }

    private void LoadScene()
    {
        SceneManager.LoadScene(1);
    }
}
