﻿using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;  
using Traits;

public class Scrap : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.DIRECT; } }

    public Scrap() : base(INTERACTION_TYPE.SCRAP) {
        validTimeOfDays = new TIME_IN_WORDS[] {
            TIME_IN_WORDS.MORNING,
            TIME_IN_WORDS.LUNCH_TIME,
            TIME_IN_WORDS.AFTERNOON,
            TIME_IN_WORDS.EARLY_NIGHT,
        };
        actionIconString = GoapActionStateDB.Work_Icon;
        isNotificationAnIntel = false;
        //actionLocationType = ACTION_LOCATION_TYPE.ON_TARGET;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.ITEM };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, };
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.PRODUCE_STONE, conditionKey = string.Empty, isKeyANumber = false, target = GOAP_EFFECT_TARGET.ACTOR });
    }
    //protected override List<GoapEffect> GetExpectedEffects(Character actor, IPointOfInterest target, object[] otherData) {
    //    List <GoapEffect> ee = base.GetExpectedEffects(actor, target, otherData);
    //    SpecialToken item = target as SpecialToken;
    //    ee.Add(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_WOOD, conditionKey = "0", isKeyANumber = true, target = GOAP_EFFECT_TARGET.ACTOR });
    //    return ee;
    //}
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Scrap Success", goapNode);
    }
<<<<<<< Updated upstream
    protected override int GetBaseCost(Character actor, IPointOfInterest target, object[] otherData) {
        return Utilities.rng.Next(15, 31);
=======
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        return Ruinarch.Utilities.rng.Next(15, 31);
>>>>>>> Stashed changes
    }
    #endregion

    #region Requirements
   protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            if (poiTarget is SpecialToken) {
                SpecialToken token = poiTarget as SpecialToken;
                if(token.characterOwner != null && token.characterOwner != actor) {
                    return false;
                }
                if (token.gridTileLocation == null) {
                    return false;
                }
                if (token.gridTileLocation.structure.location.IsRequiredByLocation(token)) {
                    return false;
                }
                return true;
            }
            //if (poiTarget.gridTileLocation != null) {
            //    if (poiTarget.factionOwner != null) {
            //        if (actor.faction == poiTarget.factionOwner) {
            //            return true;
            //        }
            //    } else {
            //        return true;
            //    }
            //}
        }
        return false;
    }
    #endregion

    #region State Effects
    //public void PreScrapSuccess(ActualGoapNode goapNode) {
    //    SpecialToken item = goapNode.poiTarget as SpecialToken;
    //    GoapActionState currentState = goapNode.action.states[goapNode.currentStateName];
    //    //goapNode.descriptionLog.AddToFillers(goapNode.targetStructure.location, goapNode.targetStructure.GetNameRelativeTo(goapNode.actor), LOG_IDENTIFIER.LANDMARK_1);
    //    goapNode.descriptionLog.AddToFillers(item, item.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    //    goapNode.descriptionLog.AddToFillers(null, TokenManager.Instance.itemData[item.specialTokenType].supplyValue.ToString(), LOG_IDENTIFIER.STRING_1);
    //}
    public void AfterScrapSuccess(ActualGoapNode goapNode) {
        SpecialToken item = goapNode.poiTarget as SpecialToken;
        LocationGridTile tile = item.gridTileLocation;
        int craftCost = item.craftCost;
        //goapNode.actor.AdjustSupply(TokenManager.Instance.itemData[item.specialTokenType].supplyValue);
        goapNode.actor.DestroyToken(item);

        StonePile stonePile = InnerMapManager.Instance.CreateNewTileObject<StonePile>(TILE_OBJECT_TYPE.STONE_PILE);
        stonePile.SetResourceInPile(Mathf.CeilToInt(craftCost * 0.5f));
        tile.structure.AddPOI(stonePile, tile);
        stonePile.gridTileLocation.SetReservedType(TILE_OBJECT_TYPE.STONE_PILE);
    }
    #endregion
}

public class ScrapData : GoapActionData {
    public ScrapData() : base(INTERACTION_TYPE.SCRAP) {
        //racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (poiTarget is SpecialToken) {
            SpecialToken token = poiTarget as SpecialToken;
            if (token.gridTileLocation != null && token.gridTileLocation.structure.location.IsRequiredByLocation(token)) {
                return false;
            }
        }
        if (poiTarget.gridTileLocation != null) {
            if (poiTarget.factionOwner != null) {
                if (actor.faction == poiTarget.factionOwner) {
                    return true;
                }
            } else {
                return true;
            }
        }
        return false;
    }
}
