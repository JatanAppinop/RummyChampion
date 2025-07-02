using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class TurnTimer : MonoBehaviour {

	public Action OnTimerFinishedCB;
	public Action OnTimerIsEndingCB;
	
	private CanvasGroup canvasGroup;
	// private Slider slider;
	[SerializeField] private Image slider;
	// private Image sliderFill;
	private float timer;
	private bool isTimerRunning;
	private bool isTimerEnding;
	// private Color fillColor;

	private void Awake()
	{
		canvasGroup = GetComponent<CanvasGroup>();
		// slider = GetComponent<Slider>();
		// slider.maxValue = Constants.TIME_PER_TURN;
		// sliderFill = transform.Find("Fill Area/Fill").GetComponent<Image>();
		// fillColor = transform.Find ("Fill Area/Fill").GetComponent<Image> ().color;
		canvasGroup.alpha = 0;
		GameManager.OnGameFinishedCB += StopTimer;
		
	}

	public void StartTimer()
	{
		timer = Constants.TIME_PER_TURN;
		isTimerRunning = true;
		canvasGroup.DOFade(1, Constants.QUICK_ANIM_TIME);
	}

	private void Update()
	{
		if(isTimerRunning)
		{
			timer -= Time.deltaTime;

			if(timer < 7 && !isTimerEnding)
			{
				isTimerEnding = true;
				new Timer(Randomizer.GetRandomNumber(2f, 4f), () => isTimerEnding = false);
				OnTimerIsEndingCB.RunAction();
			}

			if(timer <= 0)
			{
				timer = 0;
				StopTimer();
				OnTimerFinishedCB.RunAction();
			}
			slider.fillAmount = timer / Constants.TIME_PER_TURN;
			// sliderFill.color = Color.Lerp(Color.red, fillColor, timer / Constants.TIME_PER_TURN);
			// slider.value = timer;
		}
	}

	public void StopTimer()
	{
		isTimerRunning = false;
		isTimerEnding = false;
		canvasGroup.DOFade(0, Constants.QUICK_ANIM_TIME);
	}

	public bool TimeIsOver()
	{
		return timer <= 0;
	}
}
