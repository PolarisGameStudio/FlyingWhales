﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;  
using Traits;

public class Invite : GoapAction {

    public Invite() : base(INTERACTION_TYPE.INVITE) {
        actionIconString = GoapActionStateDB.Flirt_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.DEMON };
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.INVITED, target = GOAP_EFFECT_TARGET.TARGET });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Invite Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return 1;
    }
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        if (goapActionInvalidity.isInvalid == false) {
            Character targetCharacter = poiTarget as Character;
            if (actor is SeducerSummon) {
                SeducerSummon seducer = actor as SeducerSummon;
                if (UnityEngine.Random.Range(0, 100) > seducer.seduceChance || targetCharacter.ownParty.isCarryingAnyPOI
                     || targetCharacter.stateComponent.currentState != null || targetCharacter.IsAvailable() == false) {
                    goapActionInvalidity.isInvalid = true;
                    goapActionInvalidity.stateName = "Invite Fail";
                }
            } else {
                int acceptChance = 100;
                if (targetCharacter.traitContainer.GetNormalTrait<Trait>("Chaste") != null) {
                    acceptChance = 25;
                }
                if (UnityEngine.Random.Range(0, 100) > acceptChance || targetCharacter.needsComponent.isStarving || targetCharacter.needsComponent.isExhausted
                || targetCharacter.traitContainer.GetNormalTrait<Trait>("Annoyed") != null || targetCharacter.ownParty.isCarryingAnyPOI
                || targetCharacter.stateComponent.currentState != null || targetCharacter.IsAvailable() == false) {
                    goapActionInvalidity.isInvalid = true;
                    goapActionInvalidity.stateName = "Invite Fail";
                }
            }
        }
        return goapActionInvalidity;
    }
    public override void OnInvalidAction(ActualGoapNode node) {
        base.OnInvalidAction(node);
        if (node.actor is SeducerSummon) {
            Character target = node.poiTarget as Character;
            target.marker.AddHostileInRange(node.actor, false);
        }
    }
    #endregion

    #region Effects
    public void PreInviteSuccess(ActualGoapNode goapNode) {
        goapNode.poiTarget.traitContainer.AddTrait(goapNode.poiTarget, "Wooed", goapNode.actor);
        goapNode.actor.ownParty.AddPOI(goapNode.poiTarget);
    }
    //public void PreInviteFail(ActualGoapNode goapNode) {
    //    currentState.SetIntelReaction(InviteFailReactions);
    //}
    public void AfterInviteFail(ActualGoapNode goapNode) {
        if (goapNode.actor is SeducerSummon) {
            //Start Combat between actor and target
            Character target = goapNode.poiTarget as Character;
            target.marker.AddHostileInRange(goapNode.actor, false);
        } else {
            //**After Effect 1**: Actor gains Annoyed trait.
            goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Annoyed");
        }
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
                return false;
            }
            Character target = poiTarget as Character;
            if (target == actor) {
                return false;
            }
            //if (target.currentAlterEgoName != CharacterManager.Original_Alter_Ego) { //do not woo characters that have transformed to other alter egos
            //    return false;
            //}
            if (target.traitContainer.HasTraitOf(TRAIT_TYPE.DISABLER, TRAIT_EFFECT.NEGATIVE)) {
                return false;
            }
            if (target.stateComponent.currentState is CombatState) { //do not invite characters that are currently in combat
                return false;
            }
            if (target.returnedToLife) { //do not woo characters that have been raised from the dead
                return false;
            }
            if (target.currentParty.icon.isTravellingOutside || target.currentRegion != actor.currentRegion) {
                return false; //target is outside the map
            }
            return target.IsInOwnParty();
        }
        return false;
    }
    #endregion
}

public class InviteData : GoapActionData {
    public InviteData() : base(INTERACTION_TYPE.INVITE) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
            return false;
        }
        Character target = poiTarget as Character;
        if (target == actor) {
            return false;
        }
        //if (target.currentAlterEgoName != CharacterManager.Original_Alter_Ego) {
        //    return false;
        //}
        if (target.traitContainer.HasTraitOf(TRAIT_TYPE.DISABLER, TRAIT_EFFECT.NEGATIVE)) {
            return false;
        }
        if (target.stateComponent.currentState is CombatState) { //do not invite characters that are currently in combat
            return false;
        }
        return target.IsInOwnParty();
    }
}
