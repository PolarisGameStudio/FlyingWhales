﻿using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using TMPro;
using UnityEngine;

public class ItemInfoUI : UIMenu {

    [Space(10)]
    [Header("Basic Info")]
    [SerializeField] private TextMeshProUGUI nameLbl;
    public SpecialToken activeItem { get; private set; }

    #region Overrides
    public override void CloseMenu() {
        base.CloseMenu();
        if (activeItem != null && activeItem.mapVisual != null) {
            activeItem.mapVisual.UnlockHoverObject();
            activeItem.mapVisual.SetHoverObjectState(false);
            if (InnerMapCameraMove.Instance.target == activeItem.mapObjectVisual.gameObject.transform) {
                InnerMapCameraMove.Instance.CenterCameraOn(null);
            }
        }
        activeItem = null;
    }
    public override void OpenMenu() {
        SpecialToken previousItem = activeItem;
        if (previousItem != null) {
            previousItem.mapVisual.UnlockHoverObject();
            previousItem.mapVisual.SetHoverObjectState(false);    
        }

        activeItem = _data as SpecialToken;
        if (activeItem.gridTileLocation != null) {
            bool instantCenter = !InnerMapManager.Instance.IsShowingInnerMap(activeItem.currentRegion);
            InnerMapCameraMove.Instance.CenterCameraOn(activeItem.mapObjectVisual.gameObject, instantCenter);
        }
        activeItem.mapVisual.SetHoverObjectState(true);
        activeItem.mapVisual.LockHoverObject();
        base.OpenMenu();
        UIManager.Instance.HideObjectPicker();
        //UpdateBasicInfo();
        UpdateInfo();
    }
    #endregion

    public void UpdateInfo() {
        if (activeItem == null) {
            return;
        }
        UpdateBasicInfo();
        //UpdateCharacters();
    }
    private void UpdateBasicInfo() {
        nameLbl.text = activeItem.name;
        if (activeItem.isDisabledByPlayer) {
            nameLbl.text += " (Disabled)";
        }
    }

    #region Destroy
    protected void Destroy() {
        SpecialToken item = activeItem;
        CloseMenu();
        PlayerManager.Instance.allSpellsData[SPELL_TYPE.DESTROY].ActivateAbility(item);
    }
    protected bool CanBeDestroyed() {
        return PlayerManager.Instance.allSpellsData[SPELL_TYPE.DESTROY].CanPerformAbilityTowards(activeItem);
    }
    #endregion
}