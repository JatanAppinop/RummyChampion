using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupMessage : WindowBehaviour {
	public struct Message {
		public Action closePopupAction;
		public string message;
		public string btnTxt;

		public Message(Action closePopupAction, string message, string btnTxt) {
			this.closePopupAction = closePopupAction;
			this.message = message;
			this.btnTxt = btnTxt;
		}
	}

	public static PopupMessage instance;

	private List<Message> _messages = new List<Message>();
	private Action _closePopupAction;
	private Text _txt;
	private Text _btnText;
	private Button _btn;

	protected override void Awake() {
		instance = this;
		Init();
	}

	protected void Init() {
		_txt = transform.Find("Panel/Text").GetComponent<Text>();
		_btn = GetComponentInChildren<Button>();

		Transform btnTextT = transform.Find("Panel/Close/Text");
		if(btnTextT != null)
			_btnText = btnTextT.GetComponent<Text>();
		
		base.Awake();
	}

	public void Show(Message msg) {
		Show(msg.message, msg.btnTxt, msg.closePopupAction);
	}

	public void Show (string message, string buttonTxt = "Okay", Action closePopupAction = null)  {
		if (isOpen) {
			_messages.Add(new Message(closePopupAction, message, buttonTxt));
			return;
		}

		_closePopupAction = closePopupAction;
		_txt.text = message;

		if(_btnText != null)
			_btnText.text = buttonTxt;
		
		base.ShowWindow();	
	}
		
	public override void CloseWindow() {
		base.CloseWindow();
		_closePopupAction.RunAction();
		_closePopupAction = null;

		if(_messages.Count > 0) {
			Show(_messages[0]);
			_messages.RemoveAt(0);
		}
	}

	public Transform GetConfirmButtonPosition() {
		return _btn.transform;
	}
}
