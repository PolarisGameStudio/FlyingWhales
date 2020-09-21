﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;  
using Traits;

public class Daydream : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.DIRECT; } }

    public Daydream() : base(INTERACTION_TYPE.DAYDREAM) {
        showNotification = false;
        actionLocationType = ACTION_LOCATION_TYPE.NEARBY;
        validTimeOfDays = new TIME_IN_WORDS[] { TIME_IN_WORDS.MORNING,  TIME_IN_WORDS.LUNCH_TIME, TIME_IN_WORDS.AFTERNOON, };
        actionIconString = GoapActionStateDB.Happy_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
        logTags = new[] {LOG_TAG.Needs};
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, target = GOAP_EFFECT_TARGET.ACTOR });
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.STAMINA_RECOVERY, target = GOAP_EFFECT_TARGET.ACTOR });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Daydream Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}:";
        int cost = UtilityScripts.Utilities.Rng.Next(90, 131);
        costLog += $" +{cost}(Initial)";
        int numOfTimesActionDone = actor.jobComponent.GetNumOfTimesActionDone(this);
        if (numOfTimesActionDone > 5) {
            cost += 2000;
            costLog += " +2000(Times Daydreamed > 5)";
        } else {
            int timesCost = 10 * numOfTimesActionDone;
            cost += timesCost;
            costLog += $" +{timesCost}(10 x Times Daydreamed)";
        }
        actor.logComponent.AppendCostLog(costLog);
        return cost;
    }
    public override void OnStopWhilePerforming(ActualGoapNode node) {
        base.OnStopWhilePerforming(node);
        Character actor = node.actor;
        actor.needsComponent.AdjustDoNotGetBored(-1);
        actor.needsComponent.AdjustDoNotGetTired(-1);
    }
    #endregion

    #region Effects
    public void PreDaydreamSuccess(ActualGoapNode goapNode) {
        goapNode.actor.needsComponent.AdjustDoNotGetBored(1);
        goapNode.actor.needsComponent.AdjustDoNotGetTired(1);
        goapNode.actor.jobComponent.IncreaseNumOfTimesActionDone(this);
    }
    public void PerTickDaydreamSuccess(ActualGoapNode goapNode) {
        goapNode.actor.needsComponent.AdjustHappiness(5f);
        goapNode.actor.needsComponent.AdjustStamina(1f);
    }
    public void AfterDaydreamSuccess(ActualGoapNode goapNode) {
        goapNode.actor.needsComponent.AdjustDoNotGetBored(-1);
        goapNode.actor.needsComponent.AdjustDoNotGetTired(-1);
    }
    #endregion

    #region Requirement
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            if (poiTarget.gridTileLocation != null && actor.trapStructure.IsTrappedAndTrapStructureIsNot(poiTarget.gridTileLocation.structure)) {
                return false;
            }
            if (poiTarget.gridTileLocation != null && poiTarget.gridTileLocation.collectionOwner.isPartOfParentRegionMap && actor.trapStructure.IsTrappedAndTrapHexIsNot(poiTarget.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner)) {
                return false;
            }
            //if (actor.traitContainer.HasTrait("Disillusioned")) {
            //    return false;
            //}
            return actor == poiTarget;
        }
        return false;
    }
    #endregion
}