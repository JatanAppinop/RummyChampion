using UnityEngine;
using UnityEngine.UI;

public class MatchEndScreen : IHUD {
	public static MatchEndScreen instance;
	
	private Text winnnerNameTxt;
	private Text descTxt;
	private Button buttonBtn;
	
	protected override void Awake()
	{
		base.Awake();
		instance = this;
		gameManager = FindObjectOfType<GameManager>();
		winnnerNameTxt = transform.Find("WinnerNameTxt").GetComponent<Text>();
		descTxt = transform.Find("descTxt").GetComponent<Text>();
		buttonBtn = transform.Find("ReamatchBtn").GetComponent<Button>();
		buttonBtn.onClick.AddListener(OnClick);
	}
	
	public void SetWinnerName(string name, bool isThisPlayerWon, string desc = "The winner is:")
	{
		descTxt.text = desc;
		winnnerNameTxt.text = name;
	}

	public void OnClick()
	{
		TurnOffScreen();
		StartScreen.instance.TurnOnScreen();
	}
}
