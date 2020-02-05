﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class DropResource : GoapAction {

    public DropResource() : base(INTERACTION_TYPE.DROP_RESOURCE) {
        actionIconString = GoapActionStateDB.Work_Icon;
        
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
    }

    #region Overrides
    //protected override void ConstructBasePreconditionsAndEffects() {
    //    //AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_FOOD, conditionKey = "0", isKeyANumber = true, target = GOAP_EFFECT_TARGET.ACTOR }, HasHauledEnoughAmount);
    //    //AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_FOOD, conditionKey = "0", isKeyANumber = true, target = GOAP_EFFECT_TARGET.TARGET });
    //    AddPossibleExpectedEffectForTypeAndTargetMatching(new GoapEffectConditionTypeAndTargetType(GOAP_EFFECT_CONDITION.HAS_FOOD, GOAP_EFFECT_TARGET.TARGET));
    //    AddPossibleExpectedEffectForTypeAndTargetMatching(new GoapEffectConditionTypeAndTargetType(GOAP_EFFECT_CONDITION.HAS_WOOD, GOAP_EFFECT_TARGET.TARGET));
    //    AddPossibleExpectedEffectForTypeAndTargetMatching(new GoapEffectConditionTypeAndTargetType(GOAP_EFFECT_CONDITION.HAS_STONE, GOAP_EFFECT_TARGET.TARGET));
    //    AddPossibleExpectedEffectForTypeAndTargetMatching(new GoapEffectConditionTypeAndTargetType(GOAP_EFFECT_CONDITION.HAS_METAL, GOAP_EFFECT_TARGET.TARGET));
    //}
    protected override List<GoapEffect> GetExpectedEffects(Character actor, IPointOfInterest target, object[] otherData) {
        List<GoapEffect> ee = base.GetExpectedEffects(actor, target, otherData);
        if (target is Table) {
            ee.Add(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_FOOD, "0", true, GOAP_EFFECT_TARGET.TARGET));
        } else {
            ResourcePile pile = target as ResourcePile;
            switch (pile.providedResource) {
                case RESOURCE.FOOD:
                    ee.Add(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_FOOD, "0", true, GOAP_EFFECT_TARGET.TARGET));
                    break;
                case RESOURCE.WOOD:
                    ee.Add(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_WOOD, "0", true, GOAP_EFFECT_TARGET.TARGET));
                    break;
                case RESOURCE.STONE:
                    ee.Add(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_STONE, "0", true, GOAP_EFFECT_TARGET.TARGET));
                    break;
                case RESOURCE.METAL:
                    ee.Add(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_METAL, "0", true, GOAP_EFFECT_TARGET.TARGET));
                    break;
            }
        }
        return ee;
    }
    public override List<Precondition> GetPreconditions(Character actor, IPointOfInterest target, object[] otherData) {
        List<Precondition> p = new List<Precondition>(base.GetPreconditions(actor, target, otherData));
        if (target is Table) {
            p.Add(new Precondition(new GoapEffect(GOAP_EFFECT_CONDITION.TAKE_FOOD, "0" /*+ (int)otherData[0]*/, true, GOAP_EFFECT_TARGET.ACTOR), HasTakenEnoughAmount));
        } else {
            ResourcePile pile = target as ResourcePile;
            switch (pile.providedResource) {
                case RESOURCE.FOOD:
                    p.Add(new Precondition(new GoapEffect(GOAP_EFFECT_CONDITION.TAKE_FOOD, "0" /*+ (int) otherData[0]*/, true, GOAP_EFFECT_TARGET.ACTOR), HasTakenEnoughAmount));
                    break;
                case RESOURCE.WOOD:
                    p.Add(new Precondition(new GoapEffect(GOAP_EFFECT_CONDITION.TAKE_WOOD, "0" /*+ (int) otherData[0]*/, true, GOAP_EFFECT_TARGET.ACTOR), HasTakenEnoughAmount));
                    break;
                case RESOURCE.STONE:
                    p.Add(new Precondition(new GoapEffect(GOAP_EFFECT_CONDITION.TAKE_STONE, "0" /*+ (int) otherData[0]*/, true, GOAP_EFFECT_TARGET.ACTOR), HasTakenEnoughAmount));
                    break;
                case RESOURCE.METAL:
                    p.Add(new Precondition(new GoapEffect(GOAP_EFFECT_CONDITION.TAKE_METAL, "0" /*+ (int) otherData[0]*/, true, GOAP_EFFECT_TARGET.ACTOR), HasTakenEnoughAmount));
                    break;
            }
        }
        return p;
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Drop Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        string costLog = "\n" + name + " " + target.nameWithID + ": +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
        return 10;
    }
    public override void AddFillersToLog(Log log, ActualGoapNode node) {
        base.AddFillersToLog(log, node);
        ResourcePile pile = node.actor.ownParty.carriedPOI as ResourcePile;
        log.AddToFillers(null, UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetterOnly(pile.providedResource.ToString()), LOG_IDENTIFIER.STRING_2);
    }
    public override void OnStopWhileStarted(ActualGoapNode node) {
        base.OnStopWhileStarted(node);
        Character actor = node.actor;
        actor.ownParty.RemoveCarriedPOI();
    }
    public override void OnStopWhilePerforming(ActualGoapNode node) {
        base.OnStopWhilePerforming(node);
        Character actor = node.actor;
        actor.ownParty.RemoveCarriedPOI();
    }
    #endregion

    #region Preconditions
    private bool HasTakenEnoughAmount(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if(actor.ownParty.isCarryingAnyPOI && actor.ownParty.carriedPOI is ResourcePile) {
            return true;
        }
        //if(poiTarget is Table) {
        //    int neededFood = (int)otherData[0];
        //    //Table table = poiTarget as Table;
        //    if (actor.food >= neededFood) {
        //        return true;
        //    }
        //} else if (poiTarget is FoodPile) {
        //    //FoodPile foodPile = poiTarget as FoodPile;
        //    if (actor.food >= 10) {
        //        return true;
        //    }
        //}
        return false;
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            if (poiTarget.gridTileLocation == null) {
                return false;
            }
            return actor.homeRegion.IsSameCoreLocationAs(poiTarget.gridTileLocation.structure.location);
        }
        return false;
    }
    #endregion

    #region State Effects
    public void PreDropSuccess(ActualGoapNode goapNode) {
        //int givenFood = goapNode.actor.food;
        //GoapActionState currentState = goapNode.action.states[goapNode.currentStateName];
        ResourcePile pile = goapNode.actor.ownParty.carriedPOI as ResourcePile;
        goapNode.descriptionLog.AddToFillers(null, UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(pile.resourceInPile.ToString()), LOG_IDENTIFIER.STRING_1);
        //goapNode.descriptionLog.AddToFillers(null, pile.providedResource.ToString(), LOG_IDENTIFIER.STRING_2);
        //goapNode.descriptionLog.AddToFillers(goapNode.targetStructure, goapNode.targetStructure.GetNameRelativeTo(goapNode.actor), LOG_IDENTIFIER.LANDMARK_1);
    }
    public void AfterDropSuccess(ActualGoapNode goapNode) {
        ResourcePile pile = goapNode.actor.ownParty.carriedPOI as ResourcePile;
        if (goapNode.poiTarget is Table) {
            Table table = goapNode.poiTarget as Table;
            table.AdjustFood(pile.resourceInPile);
        } else if (goapNode.poiTarget is ResourcePile) {
            ResourcePile resourcePile = goapNode.poiTarget as ResourcePile;
            resourcePile.AdjustResourceInPile(pile.resourceInPile);
        }
        pile.AdjustResourceInPile(-pile.resourceInPile);

        //goapNode.actor.ownParty.RemoveCarriedPOI(false);
        //TODO: Moved to Drop Resource action
        //else if (poiTarget is FoodPile) {
        //    FoodPile foodPile = poiTarget as FoodPile;
        //    actor.AdjustFood(-givenFood);
        //    foodPile.AdjustFoodInPile(givenFood);
        //}
    }
    #endregion
}