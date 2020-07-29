﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;  
using Traits;
using UtilityScripts;

public class Cook : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.DIRECT; } }

    public Cook() : base(INTERACTION_TYPE.COOK) {
        //actionLocationType = ACTION_LOCATION_TYPE.NEAR_OTHER_TARGET;
        actionIconString = GoapActionStateDB.Work_Icon;
        canBeAdvertisedEvenIfTargetIsUnavailable = true;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_POI, conditionKey = string.Empty, isKeyANumber = false, target = GOAP_EFFECT_TARGET.TARGET }, IsCarried);
        //AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_FROM_PARTY, conditionKey = string.Empty, isKeyANumber = false, target = GOAP_EFFECT_TARGET.TARGET });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Cook Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        return 1;
    }
    public override void OnStopWhileStarted(ActualGoapNode node) {
        base.OnStopWhileStarted(node);
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        Character targetCharacter = poiTarget as Character;
        actor.UncarryPOI(targetCharacter, addToLocation: false);
        targetCharacter.SetCurrentStructureLocation(targetCharacter.gridTileLocation.structure, false);
    }
    public override LocationStructure GetTargetStructure(ActualGoapNode node) {
        Character actor = node.actor;
        object[] otherData = node.otherData;
        if (otherData != null && otherData.Length == 1 && otherData[0] is TileObject) {
            return (otherData[0] as TileObject).structureLocation;
        }
        return base.GetTargetStructure(node);
    }
    public override IPointOfInterest GetTargetToGoTo(ActualGoapNode goapNode) {
        if (goapNode.otherData != null && goapNode.otherData.Length == 1 && goapNode.otherData[0] is TileObject) {
            return goapNode.otherData[0] as TileObject;
        }
        return base.GetTargetToGoTo(goapNode);
    }
    //public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
    //    string stateName = "Target Missing";
    //    GoapActionInvalidity goapActionInvalidity = new GoapActionInvalidity(false, stateName);
    //    //bury cannot be invalid because all cases are handled by the requirements of the action
    //    return goapActionInvalidity;
    //}
    #endregion

    #region State Effects
    public void PreCookSuccess(ActualGoapNode goapNode) {
        if(goapNode.poiTarget is Character targetCharacter) {
            goapNode.actor.UncarryPOI(addToLocation: false);
            targetCharacter.currentStructure.RemoveCharacterAtLocation(targetCharacter);
            targetCharacter.DestroyMarker();
        }
    }
    public void AfterCookSuccess(ActualGoapNode goapNode) {
        FoodPile foodPile = CharacterManager.Instance.CreateFoodPileForPOI(goapNode.poiTarget, goapNode.actor.gridTileLocation);
    }
    #endregion

    #region Preconditions
    private bool IsCarried(Character actor, IPointOfInterest poiTarget, object[] otherData, JOB_TYPE jobType) {
        // Character target = poiTarget as Character;
        // return target.currentParty == actor.currentParty;
        return actor.IsPOICarriedOrInInventory(poiTarget);
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            Character targetCharacter = poiTarget as Character;
            //target character must be dead
            if (!targetCharacter.isDead) {
                return false;
            }
            //if (targetCharacter.marker == null) {
            //    return false;
            //}
            if (targetCharacter.isBeingCarriedBy != null && targetCharacter.isBeingCarriedBy != actor) {
                return false;
            }
            return true;
        }
        return false;
    }
    #endregion
}