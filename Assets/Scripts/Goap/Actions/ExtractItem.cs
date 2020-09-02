﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;

public class ExtractItem : GoapAction {

    public ExtractItem() : base(INTERACTION_TYPE.EXTRACT_ITEM) {
        actionIconString = GoapActionStateDB.Work_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES };
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        //AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_POI, "Ember", false, GOAP_EFFECT_TARGET.ACTOR));
        //AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_POI, "Water Flask", false, GOAP_EFFECT_TARGET.ACTOR));
        //AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_POI, "Ice", false, GOAP_EFFECT_TARGET.ACTOR));
        AddPossibleExpectedEffectForTypeAndTargetMatching(new GoapEffectConditionTypeAndTargetType(GOAP_EFFECT_CONDITION.HAS_POI, GOAP_EFFECT_TARGET.ACTOR));
    }
    protected override List<GoapEffect> GetExpectedEffects(Character actor, IPointOfInterest target, OtherData[] otherData) {
        List<GoapEffect> ee = base.GetExpectedEffects(actor, target, otherData);
        if (target.traitContainer.HasTrait("Wet")) {
            ee.Add(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_POI, "Water Flask", false, GOAP_EFFECT_TARGET.ACTOR));
        }
        if (target.traitContainer.HasTrait("Burning")) {
            ee.Add(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_POI, "Ember", false, GOAP_EFFECT_TARGET.ACTOR));
        }
        if (target.traitContainer.HasTrait("Frozen") || target is SnowMound) {
            ee.Add(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_POI, "Ice", false, GOAP_EFFECT_TARGET.ACTOR));
        }
        return ee;
    }
    //protected override void ConstructBasePreconditionsAndEffects() {
    //    AddPossibleExpectedEffectForTypeAndTargetMatching(new GoapEffectConditionTypeAndTargetType(GOAP_EFFECT_CONDITION.HAS_POI, GOAP_EFFECT_TARGET.ACTOR));
    //}
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Extract Success", goapNode);
    }
    public override void AddFillersToLog(Log log, ActualGoapNode node) {
        base.AddFillersToLog(log, node);
        TileObject obj = node.poiTarget as TileObject;
        IPointOfInterest target = node.poiTarget;
        string text = string.Empty;
        if (target.traitContainer.HasTrait("Wet")) {
            text += "Water Flask";
        }
        if (target.traitContainer.HasTrait("Burning")) {
            if(text != string.Empty) {
                text += ", ";
            }
            text += "Ember";
        }
        if (target is SnowMound || target.traitContainer.HasTrait("Frozen")) {
            if (text != string.Empty) {
                text += ", ";
            }
            text += "Ice";
        }
        string article = UtilityScripts.Utilities.GetArticleForWord(text);
        text = article + " " + text;
        log.AddToFillers(null, text, LOG_IDENTIFIER.STRING_1);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}:";
        int cost = 250;
        costLog += $" +{cost}(Initial)";
        actor.logComponent.AppendCostLog(costLog);
        return cost;
    }
    #endregion

    #region State Effects
    public void PreExtractSuccess(ActualGoapNode goapNode) {
        IPointOfInterest target = goapNode.poiTarget;
        string text = string.Empty;
        if (target.traitContainer.HasTrait("Wet")) {
            text += "Water Flask";
        }
        if (target.traitContainer.HasTrait("Burning")) {
            if(text != string.Empty) {
                text += ", ";
            }
            text += "Ember";
        }
        if (target is SnowMound || target.traitContainer.HasTrait("Frozen")) {
            if (text != string.Empty) {
                text += ", ";
            }
            text += "Ice";
        }
        string article = UtilityScripts.Utilities.GetArticleForWord(text);
        text = article + " " + text;
        goapNode.descriptionLog.AddToFillers(null, text, LOG_IDENTIFIER.STRING_1);
        goapNode.thoughtBubbleLog?.AddToFillers(null, text, LOG_IDENTIFIER.STRING_1);
        goapNode.thoughtBubbleMovingLog?.AddToFillers(null, text, LOG_IDENTIFIER.STRING_1);
    }
    public void AfterExtractSuccess(ActualGoapNode goapNode) {
        Character actor = goapNode.actor;
        IPointOfInterest target = goapNode.poiTarget;
        if (target.traitContainer.HasTrait("Wet")) {
            if (goapNode.associatedJobType == JOB_TYPE.DOUSE_FIRE) {
                actor.ObtainItem(InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.WATER_FLASK));
                actor.ObtainItem(InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.WATER_FLASK));
                actor.ObtainItem(InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.WATER_FLASK));
            } else {
                actor.ObtainItem(InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.WATER_FLASK));
            }
        }
        if (target.traitContainer.HasTrait("Burning")) {
            actor.ObtainItem(InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.EMBER));
        }
        if (target is SnowMound || target.traitContainer.HasTrait("Frozen")) {
            actor.ObtainItem(InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.ICE));
        }
    }
    #endregion

    #region Preconditions
    private bool HasHerbPlant(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return actor.HasItem("Herb Plant");
    }
    private bool HasWaterFlask(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return actor.HasItem("Water Flask");
    }
    #endregion

    #region Requirement
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            return actor != poiTarget && (poiTarget.traitContainer.HasTrait("Wet", "Burning", "Frozen") || poiTarget is SnowMound);
        }
        return false;
    }
    #endregion
}