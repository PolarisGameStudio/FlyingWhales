﻿using UnityEngine;
using Ruinarch.MVCFramework;
using System;

public class LifeSpanUIView : MVCUIView
{
	#region interface for listener

	public interface IListener
	{
		void OnObjectsUpgradeClicked();
		void OnElvesUpgradeClicked();
		void OnHumansUpgradeClicked();
		void OnMonstersUpgradeClicked();
		void OnUndeadUpgradeClicked();
	}
	#endregion
	#region MVC Properties and functions to override
	/*
	 * this will be the reference to the model 
	 * */
	public LifeSpanUIModel UIModel
	{
		get
		{
			return _baseAssetModel as LifeSpanUIModel;
		}
	}

	/*
	 * Call this Create method to Initialize and instantiate the UI.
	 * There's a callback on the controller if you want custom initialization
	 * */
	public static void Create(Canvas p_canvas, LifeSpanUIModel p_assets, Action<LifeSpanUIView> p_onCreate)
	{
		var go = new GameObject(typeof(LifeSpanUIView).ToString());
		var gui = go.AddComponent<LifeSpanUIView>();
		var assetsInstance = Instantiate(p_assets);
		gui.Init(p_canvas, assetsInstance);
		if (p_onCreate != null)
		{
			p_onCreate.Invoke(gui);
		}
	}
	#endregion

	#region Subscribe/Unsubscribe for IListener
	public void Subscribe(IListener p_listener)
	{
		UIModel.onObjectsUpgradeClicked += p_listener.OnObjectsUpgradeClicked;
		UIModel.onElvesUpgradeClicked += p_listener.OnElvesUpgradeClicked;
		UIModel.onHumansUpgradeClicked += p_listener.OnHumansUpgradeClicked;
		UIModel.onMonstersUpgradeClicked += p_listener.OnMonstersUpgradeClicked;
		UIModel.onUndeadUpgradeClicked += p_listener.OnUndeadUpgradeClicked;
	}

	public void Unsubscribe(IListener p_listener)
	{
		UIModel.onObjectsUpgradeClicked -= p_listener.OnObjectsUpgradeClicked;
		UIModel.onElvesUpgradeClicked -= p_listener.OnElvesUpgradeClicked;
		UIModel.onHumansUpgradeClicked -= p_listener.OnHumansUpgradeClicked;
		UIModel.onMonstersUpgradeClicked -= p_listener.OnMonstersUpgradeClicked;
		UIModel.onUndeadUpgradeClicked -= p_listener.OnUndeadUpgradeClicked;
	}
	#endregion
	
	public void UpdateTileObjectUpgradePrice(string p_newPrice) {
		UIModel.txtTileObjectCost.text = p_newPrice;
	}
	public void UpdateTileObjectInfectionTime(string p_rate) {
		UIModel.txtTileObjectInfectionTime.text = p_rate;
	}
	public void UpdateTileObjectUpgradeButtonInteractable(bool p_interactable) {
		UIModel.btnObjectsUpgrade.interactable = p_interactable;
	}
	public void UpdateTileObjectUpgradePriceState(bool p_state) {
		UIModel.txtTileObjectCost.gameObject.SetActive(p_state);
	}
	
	public void UpdateElvesUpgradePrice(string p_newPrice) {
		UIModel.txtElvesCost.text = p_newPrice;
	}
	public void UpdateElvesInfectionTime(string p_rate) {
		UIModel.txtElvesInfectionTime.text = p_rate;
	}
	public void UpdateElvesUpgradeButtonInteractable(bool p_interactable) {
		UIModel.btnElvesUpgrade.interactable = p_interactable;
	}
	public void UpdateElvesUpgradePriceState(bool p_state) {
		UIModel.txtElvesCost.gameObject.SetActive(p_state);
	}
	
	public void UpdateHumansUpgradePrice(string p_newPrice) {
		UIModel.txtHumansCost.text = p_newPrice;
	}
	public void UpdateHumansInfectionTime(string p_rate) {
		UIModel.txtHumansInfectionTime.text = p_rate;
	}
	public void UpdateHumansUpgradeButtonInteractable(bool p_interactable) {
		UIModel.btnHumansUpgrade.interactable = p_interactable;
	}
	public void UpdateHumansUpgradePriceState(bool p_state) {
		UIModel.txtHumansCost.gameObject.SetActive(p_state);
	}
	
	public void UpdateMonstersUpgradePrice(string p_newPrice) {
		UIModel.txtMonstersCost.text = p_newPrice;
	}
	public void UpdateMonstersInfectionTime(string p_rate) {
		UIModel.txtMonstersInfectionTime.text = p_rate;
	}
	public void UpdateMonstersUpgradeButtonInteractable(bool p_interactable) {
		UIModel.btnMonstersUpgrade.interactable = p_interactable;
	}
	public void UpdateMonstersUpgradePriceState(bool p_state) {
		UIModel.txtMonstersCost.gameObject.SetActive(p_state);
	}
	
	public void UpdateUndeadUpgradePrice(string p_newPrice) {
		UIModel.txtUndeadCost.text = p_newPrice;
	}
	public void UpdateUndeadInfectionTime(string p_rate) {
		UIModel.txtUndeadInfectionTime.text = p_rate;
	}
	public void UpdateUndeadUpgradeButtonInteractable(bool p_interactable) {
		UIModel.btnUndeadUpgrade.interactable = p_interactable;
	}
	public void UpdateUndeadUpgradePriceState(bool p_state) {
		UIModel.txtUndeadCost.gameObject.SetActive(p_state);
	}
}