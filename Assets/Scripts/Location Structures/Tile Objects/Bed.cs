﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Bed : TileObject {
    private Character[] bedUsers; //array of characters, currently using the bed

    public override Character[] users {
        get { return bedUsers.Where(x => x != null).ToArray(); }
    }

    public Bed(LocationStructure location) {
        SetStructureLocation(location);
        poiGoapActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.SLEEP, INTERACTION_TYPE.TILE_OBJECT_DESTROY, INTERACTION_TYPE.NAP, INTERACTION_TYPE.REPAIR_TILE_OBJECT };
        Initialize(TILE_OBJECT_TYPE.BED);
        bedUsers = new Character[2];
    }
    public Bed(SaveDataTileObject data) {
        poiGoapActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.SLEEP, INTERACTION_TYPE.TILE_OBJECT_DESTROY, INTERACTION_TYPE.NAP, INTERACTION_TYPE.REPAIR_TILE_OBJECT };
        Initialize(data);
        bedUsers = new Character[2];
    }

    #region Overrides
    public override string ToString() {
        return "Bed " + id.ToString();
    }
    public override void OnClickAction() {
        //base.OnClickAction();
        //cycle through characters in bed, and show the chosen characters ui
        if (UIManager.Instance.characterInfoUI.isShowing) {
            if (IsInThisBed(UIManager.Instance.characterInfoUI.activeCharacter)) {
                Character nextCharacter = GetNextCharacterInCycle(UIManager.Instance.characterInfoUI.activeCharacter);
                UIManager.Instance.ShowCharacterInfo(nextCharacter);
            } else {
                if (GetActiveUserCount() > 0) {
                    UIManager.Instance.ShowCharacterInfo(GetNextCharacterInCycle(null));
                }
            }
        } else {
            if (GetActiveUserCount() > 0) {
                UIManager.Instance.ShowCharacterInfo(GetNextCharacterInCycle(null));
            }
        }

    }
    public override void SetPOIState(POI_STATE state) {
        base.SetPOIState(state);
        if (IsSlotAvailable()) {
            if (GetActiveUserCount() > 0) {
                UpdateUsedBedAsset();
            } else {
                if (gridTileLocation != null) {
                    gridTileLocation.parentAreaMap.UpdateTileObjectVisual(this); //update visual based on state
                }
            }
        }
    }
    public override void OnDoActionToObject(GoapAction action) {
        base.OnDoActionToObject(action);
        switch (action.goapType) {
            case INTERACTION_TYPE.SLEEP:
            case INTERACTION_TYPE.NAP:
                AddUser(action.actor);
                break;
            case INTERACTION_TYPE.MAKE_LOVE:
                MakeLove makeLove = action as MakeLove;
                AddUser(makeLove.actor);
                AddUser(makeLove.targetCharacter);
                break;
        }
    }
    public override void OnDoneActionToObject(GoapAction action) {
        base.OnDoneActionToObject(action);
        switch (action.goapType) {
            case INTERACTION_TYPE.SLEEP:
            case INTERACTION_TYPE.NAP:
                RemoveUser(action.actor);
                break;
            case INTERACTION_TYPE.MAKE_LOVE:
                MakeLove makeLove = action as MakeLove;
                RemoveUser(makeLove.actor);
                RemoveUser(makeLove.targetCharacter);
                break;
        }
    }
    public override void OnCancelActionTowardsObject(GoapAction action) {
        base.OnCancelActionTowardsObject(action);
        switch (action.goapType) {
            case INTERACTION_TYPE.SLEEP:
            case INTERACTION_TYPE.NAP:
                RemoveUser(action.actor);
                break;
            case INTERACTION_TYPE.MAKE_LOVE:
                MakeLove makeLove = action as MakeLove;
                RemoveUser(makeLove.actor);
                RemoveUser(makeLove.targetCharacter);
                break;
        }
    }
    protected override void OnTileObjectGainedTrait(Trait trait) {
        base.OnTileObjectGainedTrait(trait);
        if (trait.name == "Burning") {
            for (int i = 0; i < bedUsers.Length; i++) {
                if (bedUsers[i] != null) {
                    Character currUser = bedUsers[i];
                    Messenger.Broadcast(Signals.CANCEL_CURRENT_ACTION, currUser, "bed is burning");
                }
            }
        }
    }
    public override bool CanBeReplaced() {
        return true;
    }
    #endregion

    #region Users
    private bool IsSlotAvailable() {
        for (int i = 0; i < bedUsers.Length; i++) {
            if (bedUsers[i] == null) {
                return true; //there is an available slot
            }
        }
        return false;
    }
    public override void AddUser(Character character) {
        for (int i = 0; i < bedUsers.Length; i++) {
            if (bedUsers[i] == null) {
                bedUsers[i] = character;
                character.SetTileObjectLocation(this);
                UpdateUsedBedAsset();
                if (!IsSlotAvailable()) {
                    SetPOIState(POI_STATE.INACTIVE); //if all slots in the bed are occupied, set it as inactive
                }
                //disable the character's marker
                character.marker.SetVisualState(false);
                break;
            }
        }
    }
    protected override void RemoveUser(Character character) {
        for (int i = 0; i < bedUsers.Length; i++) {
            if (bedUsers[i] == character) {
                bedUsers[i] = null;
                character.SetTileObjectLocation(null);
                UpdateUsedBedAsset();
                if (IsSlotAvailable()) {
                    SetPOIState(POI_STATE.ACTIVE); //if a slots in the bed is unoccupied, set it as active
                }
                //enable the character's marker
                character.marker.SetVisualState(true);
                break;
            }
        }
    }
    public int GetActiveUserCount() {
        int count = 0;
        for (int i = 0; i < bedUsers.Length; i++) {
            if (bedUsers[i] != null) {
                count++;
            }
        }
        return count;
    }
    private bool IsInThisBed(Character character) {
        for (int i = 0; i < bedUsers.Length; i++) {
            if (bedUsers[i] == character) {
                return true;
            }
        }
        return false;
    }
    private Character GetNextCharacterInCycle(Character startingPoint) {
        int startingIndex = 0;
        int currIndex = 0;
        if (startingPoint != null) {
            for (int i = 0; i < bedUsers.Length; i++) {
                Character currUser = bedUsers[i];
                if (currUser == startingPoint) {
                    startingIndex = i;
                    break;
                }
            }
            currIndex = startingIndex + 1;
        }
       
        
        while (true) {
            if (currIndex == bedUsers.Length) {
                currIndex = 0;
            }
            if (bedUsers[currIndex] != null) {
                return bedUsers[currIndex];
            }
            currIndex++;
        }

    }
    #endregion

    private void UpdateUsedBedAsset() {
        if (gridTileLocation == null) {
            return;
        }
        int userCount = GetActiveUserCount();
        if (userCount == 1) {
            gridTileLocation.parentAreaMap.UpdateTileObjectVisual(this, gridTileLocation.parentAreaMap.bed1SleepingVariant);
        } else if (userCount == 2) {
            gridTileLocation.parentAreaMap.UpdateTileObjectVisual(this, gridTileLocation.parentAreaMap.bed2SleepingVariant);
        }
        //the asset will revert to no one sleeping once the bed is set to active again
    }
}
