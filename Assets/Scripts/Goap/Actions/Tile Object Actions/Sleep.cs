﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class Sleep : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.DIRECT; } }

    public Sleep() : base(INTERACTION_TYPE.SLEEP) {
        actionIconString = GoapActionStateDB.Sleep_Icon;
        shouldIntelNotificationOnlyIfActorIsActive = true;
        isNotificationAnIntel = false;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
    }


    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TIREDNESS_RECOVERY, conditionKey = string.Empty, target = GOAP_EFFECT_TARGET.ACTOR });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Rest Success", goapNode); 
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, object[] otherData) {
        LocationStructure targetStructure = target.gridTileLocation.structure;
        if (targetStructure.structureType == STRUCTURE_TYPE.DWELLING) {
            Dwelling dwelling = targetStructure as Dwelling;
            if (dwelling.IsResident(actor)) {
                return 1;
            } else {
                for (int i = 0; i < dwelling.residents.Count; i++) {
                    Character resident = dwelling.residents[i];
                    if (resident != actor) {
                        if (actor.opinionComponent.HasOpinion(resident) && actor.opinionComponent.GetTotalOpinion(resident) > 0) {
                            return 30;
                        }
                    }
                }
                return 60;
            }
        } else if (targetStructure.structureType == STRUCTURE_TYPE.INN) {
            return 60;
        }
        return 50;
    }
    public override void OnStopWhilePerforming(ActualGoapNode node) {
        base.OnStopWhilePerforming(node);
        Character actor = node.actor;
        actor.traitContainer.RemoveTrait(actor, "Resting");
    }
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        if (goapActionInvalidity.isInvalid == false) {
            if (CanSleepInBed(actor, poiTarget as TileObject) == false) {
                goapActionInvalidity.isInvalid = true;
                goapActionInvalidity.stateName = "Rest Fail";
            } else if (poiTarget.IsAvailable() == false) {
                goapActionInvalidity.isInvalid = true;
            }
        }
        return goapActionInvalidity;
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            //if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
            //    return false;
            //}
            if (CanSleepInBed(actor, poiTarget as TileObject) == false) {
                return false;
            }
            return poiTarget.IsAvailable() && poiTarget.gridTileLocation != null;
        }
        return false;
    }
    #endregion

    #region State Effects
    public void PreRestSuccess(ActualGoapNode goapNode) {
        //goapNode.descriptionLog.AddToFillers(goapNode.targetStructure.location, goapNode.targetStructure.GetNameRelativeTo(goapNode.actor), LOG_IDENTIFIER.LANDMARK_1);
        goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Resting");
        goapNode.actor.CancelAllJobsExceptForCurrent(false);
        //goapNode.action.states[goapNode.currentStateName].OverrideDuration(goapNode.actor.currentSleepTicks);
    }
    public void PerTickRestSuccess(ActualGoapNode goapNode) {
        CharacterNeedsComponent needsComponent = goapNode.actor.needsComponent;
        if (needsComponent.currentSleepTicks == 1) { //If sleep ticks is down to 1 tick left, set current duration to end duration so that the action will end now, we need this because the character must only sleep the remaining hours of his sleep if ever that character is interrupted while sleeping
            goapNode.OverrideCurrentStateDuration(goapNode.currentState.duration);
        }
        needsComponent.AdjustTiredness(75);
        needsComponent.AdjustSleepTicks(-1);
    }
    public void AfterRestSuccess(ActualGoapNode goapNode) {
        goapNode.actor.traitContainer.RemoveTrait(goapNode.actor, "Resting");
    }
    //public void PreRestFail(ActualGoapNode goapNode) {
    //    if (parentPlan != null && parentPlan.job != null && parentPlan.job.id == actor.sleepScheduleJobID) {
    //        actor.SetHasCancelledSleepSchedule(true);
    //    }
    //    goapNode.descriptionLog.AddToFillers(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    //}
    //public void PreTargetMissing() {
    //    goapNode.descriptionLog.AddToFillers(actor.currentStructure.location, actor.currentStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    //}
    #endregion

    private bool CanSleepInBed(Character character, TileObject tileObject) {
        for (int i = 0; i < tileObject.users.Length; i++) {
            if (tileObject.users[i] != null) {
                Character user = tileObject.users[i];
                RELATIONSHIP_EFFECT relEffect = character.opinionComponent.GetRelationshipEffectWith(user);
                if(character.relationshipContainer.HasRelationshipWith(user, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.PARAMOUR) == false
                   && relEffect != RELATIONSHIP_EFFECT.POSITIVE) {
                    //if the bed has a user that is not the actors lover/paramour/positive opinion
                    //do not allow actor to sleep in this bed.
                    return false;
                }
            }
        }
        return true;
    }
}

public class SleepData : GoapActionData {
    public SleepData() : base(INTERACTION_TYPE.SLEEP) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
            return false;
        }
        return poiTarget.IsAvailable() && poiTarget.gridTileLocation != null;
    }
}