﻿using UnityEngine;
using Ruinarch.MVCFramework;
using System;
using System.Linq;

public class BiolabUIController : MVCUIController, BiolabUIView.IListener
{
	[SerializeField]
	private BiolabUIModel m_biolabUIModel;
	private BiolabUIView m_biolabUIView;

	public TransmissionUIController transmissionUIController;
	public LifeSpanUIController lifeSpanUIController;
	public FatalityUIController fatalityUIController;
	public SymptomsUIController symptomsUIController;
	public OnDeathUIController onDeathUIController;

	private Action onCloseBiolabUI;

	public void Init(Action p_onCloseBiolabUI = null) {
		InstantiateUI();
		HideUI();
		if (p_onCloseBiolabUI != null) {
			onCloseBiolabUI += p_onCloseBiolabUI;	
		}
	}
	
	public void Open() {
		ShowUI();
		Messenger.AddListener<int>(PlayerSignals.UPDATED_PLAGUE_POINTS, OnPlaguePointsUpdated);
	}

	#region Listeners
	private void OnPlaguePointsUpdated(int p_plaguePoints) {
		UpdateTopMenuSummary();
	}
	#endregion
	
	public override void ShowUI() {
		base.ShowUI();
		UpdateTopMenuSummary();
		ShowUI(transmissionUIController);
		m_biolabUIView.SetTransmissionTabIsOnWithoutNotify(true);
	}
	public override void HideUI() {
		base.HideUI();
		onCloseBiolabUI?.Invoke();
		Messenger.RemoveListener<int>(PlayerSignals.UPDATED_PLAGUE_POINTS, OnPlaguePointsUpdated);
	}
	private void OnDisable() {
		onDeathUIController.onUIINstantiated -= LastUIInstantiated;
	}

	//Call this function to Instantiate the UI, on the callback you can call initialization code for the said UI
	[ContextMenu("Instantiate UI")]
	public override void InstantiateUI() {
		BiolabUIView.Create(_canvas, m_biolabUIModel, (p_ui) => {
			m_biolabUIView = p_ui;
			m_biolabUIView.Subscribe(this);
			InitUI(p_ui.UIModel, p_ui);

			onDeathUIController.onUIINstantiated += LastUIInstantiated;

			transmissionUIController.InstantiateUI();
			lifeSpanUIController.InstantiateUI();
			fatalityUIController.InstantiateUI();
			symptomsUIController.InstantiateUI();
			onDeathUIController.InstantiateUI();
		});
	}

	void LastUIInstantiated() {
		transmissionUIController.SetParent(m_biolabUIView.GetTabParentTransform());
		lifeSpanUIController.SetParent(m_biolabUIView.GetTabParentTransform());
		fatalityUIController.SetParent(m_biolabUIView.GetTabParentTransform());
		symptomsUIController.SetParent(m_biolabUIView.GetTabParentTransform());
		onDeathUIController.SetParent(m_biolabUIView.GetTabParentTransform());
		ShowUI(transmissionUIController);
		UpdateTopMenuSummary();
	}

	void ShowUI(MVCUIController p_targetUIToShow) {
		transmissionUIController.HideUI();
		lifeSpanUIController.HideUI();
		fatalityUIController.HideUI();
		symptomsUIController.HideUI();
		onDeathUIController.HideUI();

		p_targetUIToShow.ShowUI();
	}

	private void UpdateTopMenuSummary() {
		m_biolabUIView.SetActiveCases(PlagueDisease.Instance.activeCases.ToString());
		m_biolabUIView.SetDeathCases(PlagueDisease.Instance.deaths.ToString());
		m_biolabUIView.SetRecoveriesCases(PlagueDisease.Instance.recoveries.ToString());
		if (PlayerManager.Instance != null && PlayerManager.Instance.player != null) {
			m_biolabUIView.SetPlaguePoints(PlayerManager.Instance.player.plagueComponent.plaguePoints.ToString());	
		}
		if (PlayerSkillManager.Instance != null) {
			m_biolabUIView.SetPlagueRats(PlayerSkillManager.Instance.GetSummonPlayerSkillData(SPELL_TYPE.PLAGUED_RAT).charges.ToString());
		}
	}
	

	#region BiolabUIView.IListener implementation
	public void OnTransmissionTabClicked(bool isOn) {
		if (isOn) {
			ShowUI(transmissionUIController);	
		}
	}
	public void OnLifeSpanTabClicked(bool isOn) {
		if (isOn) {
			ShowUI(lifeSpanUIController);
		}
	}
	public void OnFatalityTabClicked(bool isOn) {
		if (isOn) {
			ShowUI(fatalityUIController);
		}
	}
	public void OnSymptomsTabClicked(bool isOn) {
		if (isOn) {
			ShowUI(symptomsUIController);
		}
	}
	public void OnOnDeathClicked(bool isOn) {
		if (isOn) {
			ShowUI(onDeathUIController);
		}
	}
	public void OnCloseClicked() {
		HideUI();
	}
	#endregion
}