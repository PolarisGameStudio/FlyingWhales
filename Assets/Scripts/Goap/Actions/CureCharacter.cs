﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class CureCharacter : GoapAction {

    public CureCharacter() : base(INTERACTION_TYPE.CURE_CHARACTER) {
        actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
        actionIconString = GoapActionStateDB.FirstAid_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, };
        isNotificationAnIntel = true;
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddPrecondition(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_ITEM, SPECIAL_TOKEN.HEALING_POTION.ToString(), false, GOAP_EFFECT_TARGET.ACTOR), HasItemInInventory);
        AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.REMOVE_TRAIT, "Sick", false, GOAP_EFFECT_TARGET.TARGET));
        AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.REMOVE_TRAIT, "Infected", false, GOAP_EFFECT_TARGET.TARGET));
        AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.REMOVE_TRAIT, "Plagued", false, GOAP_EFFECT_TARGET.TARGET));

    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Cure Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        string costLog = "\n" + name + " " + target.nameWithID + ": +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
        return 10;
    }
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
        IPointOfInterest poiTarget = node.poiTarget;
        if (goapActionInvalidity.isInvalid == false) {
            if ((poiTarget as Character).IsInOwnParty() == false) {
                goapActionInvalidity.isInvalid = true;
            }
        }
        return goapActionInvalidity;
    }
    public override string ReactionToActor(Character witness, ActualGoapNode node) {
        string response = base.ReactionToActor(witness, node);
        Character actor = node.actor;
        IPointOfInterest target = node.poiTarget;
        Character targetCharacter = target as Character;
        string opinionLabel = witness.opinionComponent.GetOpinionLabel(targetCharacter);
        if (opinionLabel == OpinionComponent.Friend || opinionLabel == OpinionComponent.Close_Friend) {
            if (!witness.traitContainer.HasTrait("Psychopath")) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Gratefulness, witness, actor);
            }
        } else if (opinionLabel == OpinionComponent.Rival) {
            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disapproval, witness, actor);
        }
        return response;
    }
    public override string ReactionOfTarget(ActualGoapNode node) {
        string response = base.ReactionOfTarget(node);
        Character actor = node.actor;
        IPointOfInterest target = node.poiTarget;
        Character targetCharacter = target as Character;
        if (!targetCharacter.traitContainer.HasTrait("Psychopath")) {
            if (targetCharacter.opinionComponent.IsEnemiesWith(actor)) {
                if(UnityEngine.Random.Range(0, 100) < 30) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Gratefulness, targetCharacter, actor);
                }
                if (UnityEngine.Random.Range(0, 100) < 20) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Embarassment, targetCharacter, actor);
                }
            } else {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Gratefulness, targetCharacter, actor);
            }
        }
        return response;
    }
    #endregion

    #region State Effects
    //public void PreCureSuccess(ActualGoapNode goapNode) { }
    public void AfterCureSuccess(ActualGoapNode goapNode) {
        Character targetCharacter = goapNode.poiTarget as Character;
        targetCharacter.opinionComponent.AdjustOpinion(goapNode.actor, "Base", 3);
        goapNode.poiTarget.traitContainer.RemoveTraitAndStacks(goapNode.poiTarget, "Sick", goapNode.actor);
        goapNode.poiTarget.traitContainer.RemoveTraitAndStacks(goapNode.poiTarget, "Plagued", goapNode.actor);
        goapNode.poiTarget.traitContainer.RemoveTraitAndStacks(goapNode.poiTarget, "Infected", goapNode.actor);
        //**After Effect 2**: Remove Healing Potion from Actor's Inventory
        goapNode.actor.ConsumeToken(goapNode.actor.GetToken(SPECIAL_TOKEN.HEALING_POTION));
        //**After Effect 3**: Allow movement of Target
    }
    #endregion

    #region Preconditions
    private bool HasItemInInventory(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return actor.HasTokenInInventory(SPECIAL_TOKEN.HEALING_POTION);
        //return true;
    }
    #endregion

    //#region Intel Reactions
    //private List<string> CureSuccessReactions(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
    //    List<string> reactions = new List<string>();
    //    Character targetCharacter = poiTarget as Character;

    //    if (isOldNews) {
    //        //Old News
    //        reactions.Add("This is old news.");
    //    } else {
    //        //Not Yet Old News
    //        if (awareCharactersOfThisAction.Contains(recipient)) {
    //            //- If Recipient is Aware
    //            reactions.Add("I know that already.");
    //        } else {
    //            //- Recipient is Actor
    //            if (recipient == actor) {
    //                reactions.Add("I know what I did.");
    //            }
    //            //- Recipient is Target
    //            else if (recipient == targetCharacter) {
    //                reactions.Add(string.Format("I am grateful for {0}'s help.", actor.name));
    //            }
    //            //- Recipient Has Positive Relationship with Target
    //            else if (recipient.relationshipContainer.GetRelationshipEffectWith(targetCharacter.currentAlterEgo) == RELATIONSHIP_EFFECT.POSITIVE) {
    //                reactions.Add(string.Format("I am grateful that {0} helped {1}.", actor.name, targetCharacter.name));
    //            }
    //            //- Recipient Has Negative Relationship with Target
    //            else if (recipient.relationshipContainer.GetRelationshipEffectWith(targetCharacter.currentAlterEgo) == RELATIONSHIP_EFFECT.NEGATIVE) {
    //                reactions.Add(string.Format("{0} is such a chore.", targetCharacter.name));
    //            }
    //            //- Recipient Has No Relationship with Target
    //            else {
    //                reactions.Add(string.Format("That was nice of {0}.", Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.OBJECTIVE, false)));
    //            }
    //        }
    //    }
    //    return reactions;
    //}
    //#endregion
}

public class CureCharacterData : GoapActionData {
    public CureCharacterData() : base(INTERACTION_TYPE.CURE_CHARACTER) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, };
        requirementAction = Requirement;
    }
    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return poiTarget.traitContainer.HasTrait("Sick", "Infected", "Plagued");
    }
}
