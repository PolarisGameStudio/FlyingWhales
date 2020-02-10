﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class Strangle : GoapAction {

    public Strangle() : base(INTERACTION_TYPE.STRANGLE) {
        actionIconString = GoapActionStateDB.Sleep_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.RANDOM_LOCATION;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
        validTimeOfDays = new TIME_IN_WORDS[] { TIME_IN_WORDS.EARLY_NIGHT, TIME_IN_WORDS.LATE_NIGHT, TIME_IN_WORDS.AFTER_MIDNIGHT, };
        isNotificationAnIntel = true;
    }

    #region Override
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.DEATH, target = GOAP_EFFECT_TARGET.ACTOR });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Strangle Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        string costLog = "\n" + name + " " + target.nameWithID + ": +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
        return 10;
    }
    public override LocationStructure GetTargetStructure(ActualGoapNode node) {
        Character actor = node.actor;
        if (actor.homeStructure != null) {
            return actor.homeStructure.GetLocationStructure();
        } else {
            return actor.currentRegion.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS);
        }
    }
    public override string ReactionToActor(Character witness, ActualGoapNode node) {
        string response = base.ReactionToActor(witness, node);
        Character actor = node.actor;
        IPointOfInterest target = node.poiTarget;
        if (target is Character) {
            Character targetCharacter = target as Character;
            if(actor != targetCharacter) {
                if (witness.traitContainer.HasTrait("Coward")) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Fear, witness, actor);
                } else {
                    string opinionLabel = witness.relationshipContainer.GetOpinionLabel(targetCharacter);
                    if(opinionLabel == OpinionComponent.Rival) {
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Approval, witness, actor);
                    } else if (opinionLabel == OpinionComponent.Friend || opinionLabel == OpinionComponent.Close_Friend) {
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Anger, witness, actor);
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Threatened, witness, actor);
                    } else {
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, witness, actor);
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disapproval, witness, actor);
                    }
                }
                CrimeManager.Instance.ReactToCrime(witness, actor, node, node.associatedJobType, CRIME_TYPE.SERIOUS);
            } else {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, witness, actor);
                if (witness.traitContainer.HasTrait("Serial Killer") || witness.relationshipContainer.IsEnemiesWith(actor)) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Scorn, witness, actor);
                } else {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disapproval, witness, actor);
                }
            }
        }
        return response;
    }
    public override string ReactionToTarget(Character witness, ActualGoapNode node) {
        string response = base.ReactionToTarget(witness, node);
        Character actor = node.actor;
        IPointOfInterest target = node.poiTarget;
        if (target is Character) {
            Character targetCharacter = target as Character;
            if (actor != targetCharacter) {
                string opinionLabel = witness.relationshipContainer.GetOpinionLabel(targetCharacter);
                if (opinionLabel == OpinionComponent.Rival) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Scorn, witness, targetCharacter);
                } else {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Concern, witness, targetCharacter);
                }
            }
        }
        return response;
    }
    public override string ReactionOfTarget(ActualGoapNode node) {
        string response = base.ReactionOfTarget(node);
        Character actor = node.actor;
        IPointOfInterest target = node.poiTarget;
        if (target is Character) {
            Character targetCharacter = target as Character;
            if (actor != targetCharacter) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Anger, targetCharacter, actor);
                if (targetCharacter.relationshipContainer.IsFriendsWith(actor) && !targetCharacter.traitContainer.HasTrait("Serial Killer")) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Betrayal, targetCharacter, actor);
                }
                CrimeManager.Instance.ReactToCrime(targetCharacter, actor, node, node.associatedJobType, CRIME_TYPE.SERIOUS);
            }
        }
        return response;
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            return poiTarget == actor && poiTarget.IsAvailable() && poiTarget.gridTileLocation != null;
        }
        return false;
    }
    #endregion

    #region State Effects
    public void PerTickStrangleSuccess(ActualGoapNode goapNode) {
        goapNode.actor.AdjustHP(-(int)(goapNode.actor.maxHP * 0.18f));
    }
    public void AfterStrangleSuccess(ActualGoapNode goapNode) {
        //Character target = goapNode.poiTarget as Character;
        //string deathReason = string.Empty;
        //if(target == goapNode.actor) {
        //    deathReason = "suicide";
        //} else {
        //    deathReason = "murder";
        //}
        //target.Death("suicide", goapNode, _deathLog: goapNode.action.states[goapNode.currentStateName].descriptionLog);
        goapNode.actor.Death("suicide", goapNode, _deathLog: goapNode.descriptionLog);

    }
    #endregion
}

public class StrangleData : GoapActionData {
    public StrangleData() : base(INTERACTION_TYPE.STRANGLE) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return poiTarget == actor && poiTarget.IsAvailable() && poiTarget.gridTileLocation != null;
    }
}