﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Steal : GoapAction {

    public Steal(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.STEAL, INTERACTION_ALIGNMENT.EVIL, actor, poiTarget) {
        //validTimeOfDays = new TIME_IN_WORDS[] {
        //    TIME_IN_WORDS.EARLY_NIGHT,
        //    TIME_IN_WORDS.LATE_NIGHT,
        //    TIME_IN_WORDS.AFTER_MIDNIGHT,
        //};
        actionIconString = GoapActionStateDB.Hostile_Icon;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        SpecialToken token = poiTarget as SpecialToken;
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_ITEM, conditionKey = poiTarget, targetPOI = actor });
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_ITEM, conditionKey = token.specialTokenType.ToString(), targetPOI = actor });
        if (actor.GetTrait("Kleptomaniac") != null) {
            AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, targetPOI = actor });
        }
        //AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, targetPOI = actor });
        //AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TIREDNESS_RECOVERY, targetPOI = actor });
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        if(!isTargetMissing) {
            SetState("Steal Success");
        } else {
            SetState("Target Missing");
        }
    }
    protected override int GetCost() {
        if (actor.GetTrait("Kleptomaniac") != null) {
            return 4;
        }
        return 12;
    }
    #endregion

    #region Requirements
    protected bool Requirement() {
        //IAwareness awareness = actor.GetAwareness(poiTarget);
        //if (awareness == null) {
        //    return false;
        //}
        //LocationGridTile knownLoc = awareness.knownGridLocation;
        ////&& !actor.isHoldingItem 
        //if (knownLoc != null && poiTarget.factionOwner != null) {
        //    if(actor.faction.id != poiTarget.factionOwner.id) {
        //        return true;
        //    }
        //}
        if(poiTarget.gridTileLocation != null) {
            //return true;
            SpecialToken token = poiTarget as SpecialToken;
            return token.characterOwner != null && token.characterOwner != actor;
        }
        return false;
    }
    #endregion

    #region State Effects
    private void PreStealSuccess() {
        //**Note**: This is a Theft crime
        SetCommittedCrime(CRIME.THEFT);
        currentState.AddLogFiller(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
        currentState.AddLogFiller(poiTarget as SpecialToken, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        currentState.SetIntelReaction(State1Reactions);
    }
    private void AfterStealSuccess() {
        actor.PickUpToken(poiTarget as SpecialToken, false);
        if (actor.GetTrait("Kleptomaniac") != null) {
            actor.AdjustHappiness(60);
        }
    }
    private void PreTargetMissing() {
        currentState.AddLogFiller(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
        currentState.AddLogFiller(poiTarget as SpecialToken, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    }
    public void AfterTargetMissing() {
        actor.RemoveAwareness(poiTarget);
    }
    #endregion

    #region Intel Reactions
    private List<string> State1Reactions(Character recipient, Intel sharedIntel) {
        List<string> reactions = new List<string>();
        SpecialToken stolenItem = poiTarget as SpecialToken;
        //Recipient is the owner of the item:
        if (recipient == stolenItem.characterOwner) {
            //- **Recipient Response Text**: "[Actor Name] stole from me? What a horrible person."
            reactions.Add(string.Format("{0} stole from me? What a horrible person.", actor.name));
            //- **Recipient Effect**: Remove Friend/Lover/Paramour relationship between Actor and Recipient.Apply Crime System handling as if the Recipient witnessed Actor commit Theft.
            recipient.ReactToCrime(CRIME.THEFT, actor);
            List<RelationshipTrait> traitsToRemove = recipient.GetAllRelationshipOfEffectWith(actor, TRAIT_EFFECT.POSITIVE);
            CharacterManager.Instance.RemoveRelationshipBetween(recipient, actor, traitsToRemove);
        }

        //Recipient and Actor is the same:
        else if (recipient == actor) {
            //- **Recipient Response Text**: "I know what I did."
            reactions.Add("I know what I did.");
            //-**Recipient Effect**: no effect
        }

        //Recipient and Actor have a positive relationship:
        else if (recipient.HasRelationshipOfEffectWith(actor, TRAIT_EFFECT.POSITIVE, RELATIONSHIP_TRAIT.RELATIVE)) {
            //- **Recipient Response Text**: "[Actor Name] may have committed theft but I know that [he/she] is a good person."
            reactions.Add(string.Format("{0} may have committed theft but I know that {1} is a good person.", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, false)));
            //-**Recipient Effect**: no effect
        }
        //Recipient and Actor have a negative relationship:
        else if (recipient.HasRelationshipOfEffectWith(actor, TRAIT_EFFECT.NEGATIVE)) {
            //- **Recipient Response Text**: "[Actor Name] committed theft!? Why am I not surprised."
            reactions.Add(string.Format("{0} committed theft!? Why am I not surprised.", actor.name));
            //-**Recipient Effect**: Apply Crime System handling as if the Recipient witnessed Actor commit Theft.
            recipient.ReactToCrime(CRIME.THEFT, actor);
        }
        //Recipient and Actor have no relationship but are from the same faction:
        else if (!recipient.HasRelationshipWith(actor) && recipient.faction == actor.faction) {
            //- **Recipient Response Text**: "[Actor Name] committed theft!? That's illegal."
            reactions.Add(string.Format("{0} committed theft!? That's illegal.", actor.name));
            //- **Recipient Effect**: Apply Crime System handling as if the Recipient witnessed Actor commit Theft.
            recipient.ReactToCrime(CRIME.THEFT, actor);
        }
        return reactions;
    }
    #endregion
}
