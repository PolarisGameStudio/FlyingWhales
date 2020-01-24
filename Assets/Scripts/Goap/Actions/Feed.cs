﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class Feed : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.DIRECT; } }

    public Feed() : base(INTERACTION_TYPE.FEED) {
        actionIconString = GoapActionStateDB.FirstAid_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TAKE_FOOD, conditionKey = "0", isKeyANumber = true, target = GOAP_EFFECT_TARGET.ACTOR }, ActorHasFood);
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = string.Empty, isKeyANumber = false, target = GOAP_EFFECT_TARGET.TARGET });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Feed Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, object[] otherData) {
        string costLog = "\n" + name + ": +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
        return 10;
    }
    public override void OnStopWhileStarted(ActualGoapNode node) {
        base.OnStopWhileStarted(node);
        Character actor = node.actor;
        actor.ownParty.RemoveCarriedPOI();
    }
    public override void OnStopWhilePerforming(ActualGoapNode node) {
        base.OnStopWhilePerforming(node);
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        actor.ownParty.RemoveCarriedPOI();
        (poiTarget as Character).needsComponent.AdjustDoNotGetHungry(-1);
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
        if(target is Character) {
            Character targetCharacter = target as Character;
            string opinionLabel = witness.opinionComponent.GetOpinionLabel(targetCharacter);
            if (opinionLabel == OpinionComponent.Friend || opinionLabel == OpinionComponent.Close_Friend) {
                if (!witness.isSerialKiller) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Gratefulness, witness, actor);
                }
            } else if (opinionLabel == OpinionComponent.Rival) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disapproval, witness, actor);
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
            if (!targetCharacter.isSerialKiller) {
                if (targetCharacter.opinionComponent.IsEnemiesWith(actor)) {
                    if (UnityEngine.Random.Range(0, 100) < 30) {
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Gratefulness, targetCharacter, actor);
                    }
                    if (UnityEngine.Random.Range(0, 100) < 30) {
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Embarassment, targetCharacter, actor);
                    }
                } else {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Gratefulness, targetCharacter, actor);
                }
            }
        }
        return response;
    }
    #endregion

    #region Effects
    public void PreFeedSuccess(ActualGoapNode goapNode) {
        Character targetCharacter = goapNode.poiTarget as Character;
        targetCharacter.needsComponent.AdjustDoNotGetHungry(1);
        if(goapNode.actor.ownParty.carriedPOI != null) {
            ResourcePile carriedPile = goapNode.actor.ownParty.carriedPOI as ResourcePile;
            carriedPile.AdjustResourceInPile(-12);
            targetCharacter.AdjustResource(RESOURCE.FOOD, 12);
        }
        //goapNode.actor.AdjustFood(-20);
        //TODO: goapNode.action.states[goapNode.currentStateName].SetIntelReaction(FeedSuccessReactions);
    }
    public void PerTickFeedSuccess(ActualGoapNode goapNode) {
        Character targetCharacter = goapNode.poiTarget as Character;
        targetCharacter.needsComponent.AdjustFullness(8.5f);
        targetCharacter.AdjustResource(RESOURCE.FOOD, -1);
    }
    public void AfterFeedSuccess(ActualGoapNode goapNode) {
        Character targetCharacter = goapNode.poiTarget as Character;
        targetCharacter.needsComponent.AdjustDoNotGetHungry(-1);
        targetCharacter.opinionComponent.AdjustOpinion(goapNode.actor, "Base", 3);
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            if (poiTarget.gridTileLocation != null && actor != poiTarget) { //actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure
                return true;
            }
        }
        return false;
    }
    #endregion

    //#region Intel Reactions
    //private List<string> FeedSuccessReactions(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
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
    //                if(targetCharacter.isAtHomeRegion) {
    //                    reactions.Add("I am paying for my mistakes.");
    //                } else {
    //                    reactions.Add("Please help me!");
    //                }
    //            }
    //            //- Recipient Has Positive Relationship with Target
    //            else if (recipient.relationshipContainer.GetRelationshipEffectWith(targetCharacter.currentAlterEgo) == RELATIONSHIP_EFFECT.POSITIVE) {
    //                if (targetCharacter.isAtHomeRegion) {
    //                    reactions.Add(string.Format("{0} is paying for {1} mistakes.", targetCharacter.name, Utilities.GetPronounString(targetCharacter.gender, PRONOUN_TYPE.POSSESSIVE, false)));
    //                } else {
    //                    reactions.Add(string.Format("I've got to figure out how to save {0}!", targetCharacter.name));
    //                    recipient.CreateSaveCharacterJob(targetCharacter);
    //                }
    //            }
    //            //- Recipient Has Negative Relationship with Target
    //            else if (recipient.relationshipContainer.GetRelationshipEffectWith(targetCharacter.currentAlterEgo) == RELATIONSHIP_EFFECT.NEGATIVE) {
    //                //if (targetCharacter.isAtHomeArea) {
    //                //    reactions.Add(string.Format("I hope {0} rots in there!", targetCharacter.name));
    //                //    AddTraitTo(recipient, "Satisfied");
    //                //} else {
    //                //    reactions.Add(string.Format("I hope {0} rots in there!", targetCharacter.name));
    //                //    AddTraitTo(recipient, "Satisfied");
    //                //}
    //                reactions.Add(string.Format("I hope {0} rots in there!", targetCharacter.name));
    //                AddTraitTo(recipient, "Satisfied");
    //            }
    //            //- Recipient Has No Relationship with Target
    //            else {
    //                if(recipient.faction.id == targetCharacter.faction.id) {
    //                    if (targetCharacter.isAtHomeRegion) {
    //                        reactions.Add(string.Format("{0} is a criminal!", targetCharacter.name));
    //                    } else {
    //                        reactions.Add(string.Format("I've got to figure out how to save {0}!", targetCharacter.name));
    //                        recipient.CreateSaveCharacterJob(targetCharacter);
    //                    }
    //                } else {
    //                    //if (targetCharacter.isAtHomeArea) {
    //                    //    reactions.Add("This isn't relevant to me.");
    //                    //} else {
    //                    //    reactions.Add("This isn't relevant to me.");
    //                    //}
    //                    reactions.Add("This isn't relevant to me.");
    //                }
    //            }
    //        }
    //    }
    //    return reactions;
    //}
    //#endregion

    #region Preconditions
    private bool ActorHasFood(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (poiTarget.HasResourceAmount(RESOURCE.FOOD, 12)) {
            return true;
        }
        if (actor.ownParty.isCarryingAnyPOI && actor.ownParty.carriedPOI is ResourcePile) {
            ResourcePile carriedPile = actor.ownParty.carriedPOI as ResourcePile;
            return carriedPile.resourceInPile >= 12;
        }
        return false;
        //return actor.supply >= 20;
    }
    #endregion
}

public class FeedData : GoapActionData {
    public FeedData() : base(INTERACTION_TYPE.FEED) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
            return false;
        }
        return true;
    }
}
