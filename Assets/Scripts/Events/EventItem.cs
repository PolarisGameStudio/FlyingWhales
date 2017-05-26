﻿using UnityEngine;
using System.Collections;

public class EventItem : MonoBehaviour {

	public delegate void OnClickEvent(object obj);
	public OnClickEvent onClickEvent;

	public GameEvent gameEvent;
	public UI2DSprite eventIcon;

	private bool isHovering;
	private bool isPaused;
	private string toolTip;
	private float timeElapsed;

	void Start(){
		this.isHovering = false;
		this.isPaused = true;
		this.toolTip = string.Empty;
		this.timeElapsed = 0f;
		UIManager.Instance.onPauseEventExpiration += PauseExpirationTimer;
	}
	void Update(){
		if (this.isHovering) {
			UIManager.Instance.ShowSmallInfo (this.toolTip, this.transform);
		}
		if(!this.isPaused){
			this.timeElapsed += Time.deltaTime * 1f;
			if(this.timeElapsed >= 10f){
				HasExpired ();
			}
		}
	}

	public void SetEvent(GameEvent gameEvent){
		this.gameEvent = gameEvent;
	}

	public void SetSpriteIcon(Sprite sprite){
		eventIcon.sprite2D = sprite;
		eventIcon.MakePixelPerfect();
	}
	internal void StartExpirationTimer(){
		this.isPaused = false;
	}
	private void PauseExpirationTimer(bool state){
		this.isPaused = state;
	}
	private void HasExpired(){
		this.isPaused = true;
		UIManager.Instance.HideSmallInfo ();
		Destroy (this.gameObject);
	}
	public IEnumerator StartExpiration(){
		yield return new WaitForSeconds (10);
		UIManager.Instance.HideSmallInfo ();
		Destroy (this.gameObject);
	}

	void OnHover(bool isOver){
		if (isOver) {
			this.isHovering = true;
			this.toolTip = Utilities.LogReplacer (this.gameEvent.logs [0]);
			UIManager.Instance.ShowSmallInfo (this.toolTip, this.transform);
		}else{
			this.isHovering = false;
			UIManager.Instance.HideSmallInfo ();
		}
	}
	void OnClick(){
		if (onClickEvent != null) {
			onClickEvent(this.gameEvent);
		}
	}

	void OnDestroy(){
		UIManager.Instance.onPauseEventExpiration -= PauseExpirationTimer;
		if(UIManager.Instance.gameObject.activeSelf){
			UIManager.Instance.RepositionGridCallback (UIManager.Instance.gameEventsOfTypeGrid);
		}
	}
}
