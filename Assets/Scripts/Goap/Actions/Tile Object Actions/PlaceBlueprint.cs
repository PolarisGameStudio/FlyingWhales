﻿using System.Collections;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using Logs;
using UnityEngine;

public class PlaceBlueprint : GoapAction {

    public PlaceBlueprint() : base(INTERACTION_TYPE.PLACE_BLUEPRINT) {
        actionIconString = GoapActionStateDB.Work_Icon;
        showNotification = true;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
        logTags = new[] {LOG_TAG.Work};
    }

    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Place Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
        return 3;
    }
    public override void AddFillersToLog(ref Log log, ActualGoapNode goapNode) {
        base.AddFillersToLog(ref log, goapNode);
        StructureSetting structureSetting = (StructureSetting)goapNode.otherData[2].obj;
        log.AddToFillers(null, UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(structureSetting.structureType.ToString()), LOG_IDENTIFIER.STRING_1);
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            if (poiTarget.gridTileLocation == null) {
                return false;
            }
            return true;
        }
        return false;
    }
    #endregion

    #region State Effects
    public void PrePlaceSuccess(ActualGoapNode goapNode) {
        string prefabName = (string)goapNode.otherData[0].obj;
        StructureSetting structureSetting = (StructureSetting)goapNode.otherData[2].obj;
        if (goapNode.poiTarget is GenericTileObject genericTileObject) {
            if (genericTileObject.PlaceBlueprintOnTile(prefabName)) {
                //create new build job at npcSettlement
                GoapPlanJob buildJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.BUILD_BLUEPRINT, INTERACTION_TYPE.BUILD_BLUEPRINT, goapNode.poiTarget, goapNode.actor.homeSettlement);
                buildJob.AddOtherData(INTERACTION_TYPE.TAKE_RESOURCE, new object[] { genericTileObject.blueprintOnTile.craftCost });
                // buildJob.SetCanTakeThisJobChecker(InteractionManager.Instance.CanCharacterTakeBuildJob);
                goapNode.actor.homeSettlement.AddToAvailableJobs(buildJob);
                goapNode.descriptionLog.AddToFillers(null, UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(structureSetting.structureType.ToString()), LOG_IDENTIFIER.STRING_1);
            } else {
                Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "GoapAction", "Place Blueprint", "fail", goapNode, LOG_TAG.Work);
                log.AddToFillers(goapNode.actor, goapNode.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                log.AddToFillers(null, UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(structureSetting.structureType.ToString()), LOG_IDENTIFIER.STRING_1);
                goapNode.OverrideDescriptionLog(log);
            }
        }
    }
    #endregion
}
