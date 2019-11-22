﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class PrayTileObject : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.DIRECT; } }

    public PrayTileObject() : base(INTERACTION_TYPE.PRAY_TILE_OBJECT) {
        this.goapName = "Pray";
        actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
        actionIconString = GoapActionStateDB.Pray_Icon;
        shouldIntelNotificationOnlyIfActorIsActive = true;
        isNotificationAnIntel = false;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
    }

    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Pray Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, object[] otherData) {
        //**Cost**: randomize between 15 - 55
        return Utilities.rng.Next(15, 56);
    }
    public override void AddFillersToLog(Log log, ActualGoapNode node) {
        base.AddFillersToLog(log, node);
        IPointOfInterest poiTarget = node.poiTarget;
        TileObject obj = poiTarget as TileObject;
        log.AddToFillers(poiTarget, Utilities.NormalizeStringUpperCaseFirstLetters(obj.tileObjectType.ToString()), LOG_IDENTIFIER.TARGET_CHARACTER);
    }
    #endregion

    #region State Effects
    public void PrePraySuccess(ActualGoapNode goapNode) {
        TileObject obj = goapNode.poiTarget as TileObject;
        goapNode.descriptionLog.AddToFillers(goapNode.poiTarget, Utilities.NormalizeStringUpperCaseFirstLetters(obj.tileObjectType.ToString()), LOG_IDENTIFIER.TARGET_CHARACTER);
    }
    public void AfterPraySuccess(ActualGoapNode goapNode) {
        if (goapNode.poiTarget is GoddessStatue) {
            //Speed up divine intervention by 4 hours
            PlayerManager.Instance.player.AdjustDivineInterventionDuration(-GameManager.Instance.GetTicksBasedOnHour(4));
        }
    }
    #endregion

    #region Requirement
   protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
                return false;
            }
            return true;
        }
        return false;
    }
    #endregion
}

public class PrayTileObjectData : GoapActionData {
    public PrayTileObjectData() : base(INTERACTION_TYPE.PRAY_TILE_OBJECT) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
            return false;
        }
        return true;
    }
}
