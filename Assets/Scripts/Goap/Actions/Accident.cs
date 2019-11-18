﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class Accident : GoapAction {
    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.DIRECT; } }

    public Accident() : base(INTERACTION_TYPE.ACCIDENT) {
        actionIconString = GoapActionStateDB.No_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        isNotificationAnIntel = false;
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REDUCE_HP, target = GOAP_EFFECT_TARGET.ACTOR });
    }
    public override void Perform(ActualGoapNode actionNode) {
        base.Perform(actionNode);
        SetState("Drop Success", actionNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, object[] otherData) {
        return 5;
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            return actor == poiTarget;
        }
        return false;
    }
    #endregion

    #region State Effects
    private void PreAccidentSuccess(ActualGoapNode goapNode) {
        GoapAction actionToDo = goapNode.otherData[0] as GoapAction;
        GoapActionState currentState = this.states[goapNode.currentStateName];
        goapNode.descriptionLog.AddToFillers(actionToDo, actionToDo.goapName, LOG_IDENTIFIER.STRING_1);
    }
    private void AfterAccidentSuccess(ActualGoapNode goapNode) {
        goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Injured", gainedFromDoing: this);

        int randomHpToLose = UnityEngine.Random.Range(5, 26);
        float percentMaxHPToLose = randomHpToLose / 100f;
        int actualHPToLose = Mathf.CeilToInt(goapNode.actor.maxHP * percentMaxHPToLose);

        goapNode.actor.AdjustHP(-actualHPToLose);
        if (goapNode.actor.currentHP <= 0) {
            goapNode.actor.Death(deathFromAction: this);
        }
    }
    #endregion
}

public class AccidentData : GoapActionData {
    public AccidentData() : base(INTERACTION_TYPE.ACCIDENT) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.WOLF, RACE.SPIDER, RACE.DRAGON };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return actor == poiTarget;
    }
}