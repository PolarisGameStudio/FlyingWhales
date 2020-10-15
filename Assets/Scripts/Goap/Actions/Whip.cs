﻿using System;
using Traits;
using Random = UnityEngine.Random;

public class Whip : GoapAction {
    public Whip() : base(INTERACTION_TYPE.WHIP) {
        actionIconString = GoapActionStateDB.Hostile_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
        logTags = new[] {LOG_TAG.Crimes, LOG_TAG.Work, LOG_TAG.Life_Changes};
    }
    
    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_TRAIT, "Injured", false, GOAP_EFFECT_TARGET.TARGET));
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Whip Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
        return 10;
    }
    public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness) {
        if (node.target is Character target && target.traitContainer.HasTrait("Criminal") == false) {
            return REACTABLE_EFFECT.Negative;
        }
        return REACTABLE_EFFECT.Positive;
    }
    public override string ReactionToActor(Character actor, IPointOfInterest target, Character witness,
        ActualGoapNode node, REACTION_STATUS status) {
        string response = base.ReactionToActor(actor, target, witness, node, status);
        Character targetCharacter = target as Character;
        if (targetCharacter.crimeComponent.HasWantedCrime() && targetCharacter.crimeComponent.IsTargetOfACrime(witness)) {
            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Approval, witness, node.actor, status, node);
        } else {
            if (witness.relationshipContainer.IsFriendsWith(targetCharacter)
                && witness.traitContainer.HasTrait("Psychopath") == false) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Resentment, witness, node.actor, status, node);
            }
            if (witness.traitContainer.HasTrait("Psychopath") == false) {
                if ((witness.traitContainer.HasTrait("Coward") && Random.Range(0, 100) < 75) ||
                    (witness.traitContainer.HasTrait("Coward") == false && Random.Range(0, 100) < 15)) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Fear, witness, node.actor, status, node);
                }
            }
        }
        return response;
    }
    public override string ReactionToTarget(Character actor, IPointOfInterest target, Character witness,
        ActualGoapNode node, REACTION_STATUS status) {
        string response = base.ReactionToTarget(actor, target, witness, node, status);
        Character targetCharacter = target as Character;
        if (witness.relationshipContainer.HasOpinionLabelWithCharacter(targetCharacter, RelationshipManager.Acquaintance)) {
            if (witness.traitContainer.HasTrait("Psychopath") == false && Random.Range(0, 100) < 50) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Concern, witness, targetCharacter, status, node);
            }
        } else if (witness.relationshipContainer.IsFriendsWith(targetCharacter)) {
            if (witness.traitContainer.HasTrait("Psychopath") == false) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Concern, witness, targetCharacter, status, node);
            }
        } else if (witness.relationshipContainer.IsEnemiesWith(targetCharacter)) {
            if (witness.traitContainer.HasTrait("Diplomatic") == false) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Scorn, witness, targetCharacter, status, node);
            }
        }
        return response;
    }
    public override string ReactionOfTarget(Character actor, IPointOfInterest target, ActualGoapNode node,
        REACTION_STATUS status) {
        string response = base.ReactionOfTarget(actor, target, node, status);
        Character targetCharacter = target as Character;
        if (Random.Range(0, 100) < 20) {
            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Resentment, targetCharacter, node.actor, status, node);
        }
        if (targetCharacter.traitContainer.HasTrait("Hothead") || Random.Range(0, 100) < 20) {
            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Anger, targetCharacter, node.actor, status, node);
        }
        return response;
    }
    #endregion

    #region State Effects
    public void AfterWhipSuccess(ActualGoapNode goapNode) {
        Character target = goapNode.target as Character;
        if (target.traitContainer.HasTrait("Criminal")) {
            Criminal criminalTrait = target.traitContainer.GetTraitOrStatus<Criminal>("Criminal");
            criminalTrait.SetIsImprisoned(false);
        }
        target.crimeComponent.SetDecisionAndJudgeToAllUnpunishedCrimesWantedBy(target.faction, CRIME_STATUS.Punished, goapNode.actor);
        target.crimeComponent.RemoveAllCrimesWantedBy(goapNode.actor.faction);
        //target.traitContainer.RemoveTrait(target, "Criminal", goapNode.actor);
        target.traitContainer.RemoveTrait(target, "Restrained", goapNode.actor);
        target.traitContainer.AddTrait(target, "Injured", goapNode.actor, goapNode);
    }
    #endregion
}