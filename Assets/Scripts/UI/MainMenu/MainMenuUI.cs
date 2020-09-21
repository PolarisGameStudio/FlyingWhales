﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Managers;
using Settings;
using TMPro;

public class MainMenuUI : MonoBehaviour {

    public static MainMenuUI Instance = null;

    [SerializeField] private EasyTween buttonsTween;
    [SerializeField] private EasyTween titleTween;

    [SerializeField] private EasyTween glowTween;
    [SerializeField] private EasyTween glow2Tween;

    [SerializeField] private Image bg;

    [Header("Buttons")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button newGameButton;

    [Header("Steam")]
    [SerializeField] private TextMeshProUGUI steamName;
    
    [Header("Version")]
    [SerializeField] private TextMeshProUGUI version;
    
    [Header("Yes/No Confirmation")]
    public YesNoConfirmation yesNoConfirmation;
    
    [Header("Load Game")]
    [SerializeField] private Button loadGameButton;
    [SerializeField] private LoadWindow loadWindow;
    
    private void Awake() {
        Instance = this;
    }
    private void Start() {
        if (!SaveManager.Instance.savePlayerManager.hasSavedDataPlayer) {
            SaveManager.Instance.savePlayerManager.CreateNewSaveDataPlayer();
        }
        newGameButton.interactable = true;
        steamName.text = $"Logged in as: <b>{SteamworksManager.Instance.GetSteamName()}</b>";
        version.text = $"Version: {Application.version}";
        //Set current save data to null everytime this is loaded, this is so that the previous save file is not loaded if new game was clicked
        SaveManager.Instance.saveCurrentProgressManager.SetCurrentSaveDataPath(string.Empty); 
        UpdateButtonStates();
        Messenger.AddListener<string>(Signals.SAVE_FILE_DELETED, OnSaveFileDeleted);
    }
    public void ShowMenuButtons() {
        titleTween.OnValueChangedAnimation(true);
        glowTween.OnValueChangedAnimation(true);
        buttonsTween.OnValueChangedAnimation(true);
    }
    private void HideMenuButtons() {
        buttonsTween.OnValueChangedAnimation(false);
    }
    public void ExitGame() {
        Application.Quit();
    }
    public void OnClickPlayGame() {
        WorldSettings.Instance.Open(); 
    }
    public void OnClickContinue() {
        //Load latest save
        string latestFile = SaveManager.Instance.saveCurrentProgressManager.GetLatestSaveFile();
        if (!string.IsNullOrEmpty(latestFile)) {
            SaveManager.Instance.saveCurrentProgressManager.SetCurrentSaveDataPath(latestFile);
            MainMenuManager.Instance.StartGame();
        } else {
            //in case no latest file was found, doubt that this will happen.
            OnClickPlayGame();
        }
        
    }
    public void OnClickSettings() {
        SettingsManager.Instance.OpenSettings();
    }
    public void OnClickDiscord() {
        Application.OpenURL("http://discord.ruinarch.com/");
    }
    private void UpdateButtonStates() {
        bool hasSaves = SaveManager.Instance.saveCurrentProgressManager.HasAnySaveFiles();
        continueButton.interactable = hasSaves;
        loadGameButton.interactable = hasSaves;
    }

    #region Load Game
    private void OnSaveFileDeleted(string saveFileDeleted) {
        UpdateButtonStates();
        if (!SaveManager.Instance.saveCurrentProgressManager.HasAnySaveFiles()) {
            //automatically close load window when all saves have been deleted.
            loadWindow.Close();
        }
    }
    public void OnClickLoadGame() {
        loadWindow.Open();
    }
    #endregion
}
