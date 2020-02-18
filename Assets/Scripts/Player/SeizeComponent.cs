﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;

public class SeizeComponent {
    public IPointOfInterest seizedPOI { get; private set; }
    public bool isPreparingToBeUnseized { get; private set; }

    private Vector3 followOffset;

    #region getters
    public bool hasSeizedPOI => seizedPOI != null;
    #endregion

    public SeizeComponent() {
        followOffset = new Vector3(1f, -1f, 10f); // new Vector3(5, -5, 0f);
    }

    public void SeizePOI(IPointOfInterest poi) {
        // int manaCost = GetManaCost(poi);
        // if (PlayerManager.Instance.player.mana < manaCost) {
        //     PlayerUI.Instance.ShowGeneralConfirmation("ERROR", "Not enough mana! You need " + manaCost + " mana to seize this object.");
        //     return;
        // }
        if (seizedPOI == null) {
            if(poi.isBeingCarriedBy != null) {
                poi.isBeingCarriedBy.UncarryPOI();
            }
            if (poi.gridTileLocation != null) {
                poi.OnSeizePOI();
                Messenger.Broadcast(Signals.ON_SEIZE_POI, poi);
                //if(poi.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
                //} else {
                //    poi.gridTileLocation.structure.RemovePOI(poi);
                //}
            } else {
                Debug.LogError($"Cannot seize. {poi.name} has no tile");
                return;
            }
            seizedPOI = poi;
            PrepareToUnseize();
            // PlayerManager.Instance.player.AdjustMana(-manaCost);
            //PlayerUI.Instance.ShowSeizedObjectUI();
        } else {
            PlayerUI.Instance.ShowGeneralConfirmation("ERROR", "Already have a seized object. You need to drop the currently seized object first.");
        }
    }
    // public void PrepareToUnseize() {
    //     if (!isPreparingToBeUnseized) {
    //         isPreparingToBeUnseized = true;
    //         CursorManager.Instance.AddLeftClickAction(UnseizePOI);
    //     }
    // }
    private void PrepareToUnseize() {
        isPreparingToBeUnseized = true;
        Messenger.AddListener<KeyCode>(Signals.KEY_DOWN, OnReceiveKeyCodeSignal);
    }
    private void DoneUnseize() {
        isPreparingToBeUnseized = false;
        Messenger.RemoveListener<KeyCode>(Signals.KEY_DOWN, OnReceiveKeyCodeSignal);
    }
    private void OnReceiveKeyCodeSignal(KeyCode keyCode) {
        if(keyCode == KeyCode.Mouse0) {
            TryToUnseize();
        }
    }
    private void TryToUnseize() {
        if (isPreparingToBeUnseized) {
            bool hasUnseized = UnseizePOI();
            if (hasUnseized) {
                DoneUnseize();
            }
        }
    }
    private bool UnseizePOI() {
        if(seizedPOI == null) {
            //Debug.LogError("Cannot unseize. Not holding seized object");
            return false;
        }
        if (!InnerMapManager.Instance.isAnInnerMapShowing || UIManager.Instance.IsMouseOnUIOrMapObject()) {
            return false;
        }
        // isPreparingToBeUnseized = false;

        LocationGridTile hoveredTile = InnerMapManager.Instance.GetTileFromMousePosition();
        if(hoveredTile.objHere != null) {
            return false;
        }
        DisableFollowMousePosition();
        seizedPOI.OnUnseizePOI(hoveredTile);
        Messenger.Broadcast(Signals.ON_UNSEIZE_POI, seizedPOI);
        seizedPOI = null;
        //PlayerUI.Instance.HideSeizedObjectUI();
        CursorManager.Instance.SetCursorTo(CursorManager.Cursor_Type.Default);
        return true;
    }
    private int GetManaCost(IPointOfInterest poi) {
        if(poi.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
            return 50;
        }
        return 20;
    }

    #region Follow Mouse
    public void EnableFollowMousePosition() {
        if (seizedPOI.visualGO.activeSelf) {
            return;
        }
        seizedPOI.visualGO.transform.position = InnerMapManager.Instance.currentlyShowingMap.worldUiCanvas.worldCamera.ScreenToWorldPoint(Input.mousePosition) + followOffset;
        seizedPOI.visualGO.SetActive(true);
    }
    public void FollowMousePosition() {
        if (!seizedPOI.visualGO.activeSelf) {
            return;
        }
        if (!InnerMapManager.Instance.isAnInnerMapShowing) {
            return;
        }
        Vector3 targetPos = InnerMapManager.Instance.currentlyShowingMap.worldUiCanvas.worldCamera.ScreenToWorldPoint(Input.mousePosition) + followOffset;
        iTween.MoveUpdate(seizedPOI.visualGO, targetPos, 1f);
    }
    public void DisableFollowMousePosition() {
        if (!seizedPOI.visualGO.activeSelf) {
            return;
        }
        seizedPOI.visualGO.SetActive(false);
        iTween.Stop(seizedPOI.visualGO);
    }
    #endregion
}
