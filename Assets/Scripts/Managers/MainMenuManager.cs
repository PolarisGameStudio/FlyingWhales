﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour {

    [Header("Main Menu")]
    [SerializeField] private Button loadGameButton;

    [Space(10)]
    [Header("World Configurations Menu")]
    [SerializeField] private GameObject worldConfigsMenuGO;
    [SerializeField] private GameObject worldConfigPrefab;
    [SerializeField] private GameObject worldConfigContent;
    [SerializeField] private ContentSorter worldConfigContentSorter;

    public void OnClickPlayGame() {
        //PlayGame();
        ShowWorldConfigurations();
    }

    private void ShowWorldConfigurations() {
        worldConfigsMenuGO.SetActive(true);
        LoadWorldConfigurations();
    }

    private void LoadWorldConfigurations() {
        //initial templates
        Directory.CreateDirectory(Utilities.worldConfigsTemplatesPath);
        DirectoryInfo templateDirInfo = new DirectoryInfo(Utilities.worldConfigsTemplatesPath);
        FileInfo[] templateFiles = templateDirInfo.GetFiles("*.worldConfig");
        for (int i = 0; i < templateFiles.Length; i++) {
            FileInfo currFile = templateFiles[i];
            GameObject configGO = GameObject.Instantiate(worldConfigPrefab, worldConfigContent.transform);
            configGO.transform.localScale = Vector3.one;
            WorldConfigItem item = configGO.GetComponent<WorldConfigItem>();
            item.SetFile(currFile);
        }

        //custom maps
        Directory.CreateDirectory(Utilities.worldConfigsSavePath);
        DirectoryInfo customMapDirInfo = new DirectoryInfo(Utilities.worldConfigsSavePath);
        FileInfo[] customMapFiles = customMapDirInfo.GetFiles("*.worldConfig");
        for (int i = 0; i < customMapFiles.Length; i++) {
            FileInfo currFile = customMapFiles[i];
            GameObject configGO = GameObject.Instantiate(worldConfigPrefab, worldConfigContent.transform);
            configGO.transform.localScale = Vector3.one;
            WorldConfigItem item = configGO.GetComponent<WorldConfigItem>();
            item.SetFile(currFile);
        }
    }

    private void PlayGame() {
        LevelLoaderManager.Instance.LoadLevel("Main");
    }
}
