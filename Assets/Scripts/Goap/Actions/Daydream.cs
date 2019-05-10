﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Daydream : GoapAction {

    //private LocationStructure _targetStructure;

    //public override LocationStructure targetStructure { get { return _targetStructure; } }

    protected override string failActionState { get { return "Daydream Failed"; } }

    public Daydream(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.DAYDREAM, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        shouldIntelNotificationOnlyIfActorIsActive = true;
        actionLocationType = ACTION_LOCATION_TYPE.NEARBY;
        validTimeOfDays = new TIME_IN_WORDS[] {
            TIME_IN_WORDS.MORNING,
            TIME_IN_WORDS.AFTERNOON,
        };
        actionIconString = GoapActionStateDB.Entertain_Icon;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, targetPOI = actor });
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        //if (targetTile.occupant != null && targetTile.occupant != actor) {
        //    SetState("Daydream Failed");
        //} else {
        SetState("Daydream Success");
        //}
    }
    protected override int GetCost() {
        //**Cost**: randomize between 5-12
        return Utilities.rng.Next(25, 35);
    }
    //public override void FailAction() {
    //    base.FailAction();
    //    SetState("Daydream Failed");
    //}
    //public override void SetTargetStructure() {
    //    List<LocationStructure> choices = actor.specificLocation.GetStructuresOfType(STRUCTURE_TYPE.WILDERNESS).ToList();
    //    if (actor.specificLocation.HasStructure(STRUCTURE_TYPE.WORK_AREA)) {
    //        choices.AddRange(actor.specificLocation.GetStructuresOfType(STRUCTURE_TYPE.WORK_AREA));
    //    }
    //    if (choices.Count > 0) {
    //        _targetStructure = choices[Utilities.rng.Next(0, choices.Count)];
    //    }
    //    base.SetTargetStructure();
    //}
    public override LocationGridTile GetTargetLocationTile() {
        return InteractionManager.Instance.GetTargetLocationTile(actionLocationType, actor, null, targetStructure);
    }
    public override void OnStopActionDuringCurrentState() {
        if (currentState.name == "Daydream Success") {
            actor.AdjustDoNotGetLonely(-1);
            actor.AdjustDoNotGetTired(-1);
        }
    }
    #endregion

    #region Effects
    private void PreDaydreamSuccess() {
        actor.AdjustDoNotGetLonely(1);
        actor.AdjustDoNotGetTired(1);
    }
    private void PerTickDaydreamSuccess() {
        actor.AdjustHappiness(5);
    }
    private void AfterDaydreamSuccess() {
        actor.AdjustDoNotGetLonely(-1);
        actor.AdjustDoNotGetTired(-1);
    }
    #endregion

    #region Requirement
    protected bool Requirement() {
        return actor == poiTarget;
        //if (actor == poiTarget) {
        //    //actor should be non-beast
        //    return actor.role.roleType != CHARACTER_ROLE.BEAST;
        //}
        //return false;
    }
    #endregion
}
