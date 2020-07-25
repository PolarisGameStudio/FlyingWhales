﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using Locations.Tile_Features;

public class PlayerBuildLandmarkUI : MonoBehaviour {

    public Region targetRegion { get; private set; }

    #region General
    private void OnHoverExitMinion(Character character) {
        UIManager.Instance.HideSmallInfo();
    }
    private bool CanChooseLandmark(string landmarkName) {
        if (landmarkName == "The Pit") {
            return false;
        }
        if(landmarkName == "The Kennel" && !UIManager.Instance.regionInfoUI.activeRegion.HasTileWithFeature(TileFeatureDB.Summons_Feature)) {
            return false;
        }
        if (landmarkName == "The Crypt" && (!UIManager.Instance.regionInfoUI.activeRegion.HasTileWithFeature(TileFeatureDB.Artifact_Feature) || PlayerManager.Instance.player.playerFaction.HasOwnedRegionWithLandmarkType(LANDMARK_TYPE.CRYPT))) {
            return false;
        }
        return true;
    }
    private void OnHoverLandmarkChoice(string landmarkName) {
        LandmarkData data = LandmarkManager.Instance.GetLandmarkData(landmarkName);
        string info = data.description;
        if (info != string.Empty) {
            info += "\n";
        }
        info += $"Duration: {GameManager.Instance.GetCeilingHoursBasedOnTicks(data.buildDuration).ToString()} hours";
        UIManager.Instance.ShowSmallInfo(info);
    }
    private void OnHoverExitLandmarkChoice(string landmarkName) {
        UIManager.Instance.HideSmallInfo();
    }
    private void StartBuild(object minionObj, object landmarkObj) {
        string landmarkTypeName = landmarkObj as string;
        Debug.Log($"Chose to build {landmarkTypeName}");
        // LandmarkData landmarkData = LandmarkManager.Instance.GetLandmarkData(landmarkObj as string);
        // targetRegion.StartBuildingStructure(landmarkData.landmarkType, (minionObj as Character).minion);
        // UIManager.Instance.regionInfoUI.UpdateInfo();
        // Messenger.Broadcast<Region>(Signals.REGION_INFO_UI_UPDATE_APPROPRIATE_CONTENT, targetRegion);
    }
    #endregion
}
