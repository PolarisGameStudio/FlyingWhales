﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using JetBrains.Annotations;
using UnityEngine.Assertions;

public class MakeLove : GoapAction {

    public MakeLove() : base(INTERACTION_TYPE.MAKE_LOVE) {
        actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
        actionIconString = GoapActionStateDB.Flirt_Icon;
        validTimeOfDays = new TIME_IN_WORDS[] { TIME_IN_WORDS.EARLY_NIGHT, TIME_IN_WORDS.LATE_NIGHT, TIME_IN_WORDS.AFTER_MIDNIGHT, };
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.LESSER_DEMON };
        isNotificationAnIntel = true;
        logTags = new[] {LOG_TAG.Needs, LOG_TAG.Social};
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.INVITED, target = GOAP_EFFECT_TARGET.TARGET }, IsTargetInvited);
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, conditionKey = string.Empty, target = GOAP_EFFECT_TARGET.ACTOR });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Make Love Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}:";
        if (actor.partyComponent.hasParty && actor.partyComponent.currentParty.isActive) {
            if (actor.partyComponent.isActiveMember) {
                costLog += $" +2000(Active Party, Cannot make love)";
                actor.logComponent.AppendCostLog(costLog);
                return 2000;
            }
        }
        int cost = UtilityScripts.Utilities.Rng.Next(80, 121);
        costLog += $" +{cost}(Initial)";
        Trait trait = actor.traitContainer.GetNormalTrait<Trait>("Chaste", "Lustful");
        if (trait != null && trait.name == "Chaste") {
            cost += 2000;
            costLog += " +2000(Chaste)";
        }
        if (trait != null && trait.name == "Lustful") {
            cost += -15;
            costLog += " -15(Lustful)";
        } else {
            int numOfTimesActionDone = actor.jobComponent.GetNumOfTimesActionDone(this);
            if (numOfTimesActionDone > 5) {
                cost += 2000;
                costLog += " +2000(Times Made Love > 5)";
            } else {
                int timesCost = 10 * numOfTimesActionDone;
                cost += timesCost;
                costLog += $" +{timesCost}(10 x Times Made Love)";
            }
        }
        actor.logComponent.AppendCostLog(costLog);
        return cost;
    }
    public override void OnStopWhilePerforming(ActualGoapNode node) {
        base.OnStopWhilePerforming(node);
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        Character targetCharacter = poiTarget as Character;
        actor.UncarryPOI(targetCharacter);
        actor.needsComponent.AdjustDoNotGetBored(-1);
        targetCharacter.needsComponent.AdjustDoNotGetBored(-1);

        Bed bed = actor.gridTileLocation.structure.GetTileObjectsOfType(TILE_OBJECT_TYPE.BED).FirstOrDefault() as Bed;
        bed?.OnDoneActionToObject(actor.currentActionNode);

        //targetCharacter.traitContainer.RemoveTrait(targetCharacter, "Wooed");
        if (targetCharacter.currentActionNode != null && targetCharacter.currentActionNode.action == this) {
            targetCharacter.SetCurrentActionNode(null, null, null);
        }
    }
    public override void OnStopWhileStarted(ActualGoapNode node) {
        base.OnStopWhileStarted(node);
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        Character targetCharacter = poiTarget as Character;
        actor.UncarryPOI(targetCharacter);
        actor.needsComponent.AdjustDoNotGetBored(-1);
        targetCharacter.needsComponent.AdjustDoNotGetBored(-1);

        //targetCharacter.traitContainer.RemoveTrait(targetCharacter, "Wooed");
        if (targetCharacter.currentActionNode != null && targetCharacter.currentActionNode.action == this) {
            targetCharacter.SetCurrentActionNode(null, null, null);
        }
    }
    public override IPointOfInterest GetTargetToGoTo(ActualGoapNode goapNode) {
        Character targetCharacter = goapNode.poiTarget as Character;
        Assert.IsNotNull(targetCharacter, $"Make love of {goapNode.actor.name} is not a character! {goapNode.poiTarget?.ToString() ?? "Null"}");
        return GetValidBedForActor(goapNode.actor, targetCharacter);
    }
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
        if (goapActionInvalidity.isInvalid == false) {
            Character targetCharacter = node.poiTarget as Character;
            Assert.IsNotNull(targetCharacter, $"Make love of {node.actor.name} is not a character! {node.poiTarget?.ToString() ?? "Null"}");
            Bed targetBed = node.actor.gridTileLocation.objHere as Bed;
            if (targetBed == null) {
                //check neighbours
                for (int i = 0; i < node.actor.gridTileLocation.neighbourList.Count; i++) {
                    LocationGridTile neighbour = node.actor.gridTileLocation.neighbourList[i];
                    if (neighbour.objHere is Bed bed) {
                        targetBed = bed;
                    }
                }
            }
            if (targetBed == null ||  targetBed.IsAvailable() == false || targetBed.GetActiveUserCount() > 0) {
                goapActionInvalidity.isInvalid = true;
                goapActionInvalidity.stateName = "Make Love Fail";
            }
        }
        return goapActionInvalidity;
    }
    public override void OnInvalidAction(ActualGoapNode node) {
        base.OnInvalidAction(node);
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        Character targetCharacter = poiTarget as Character;
        actor.UncarryPOI(targetCharacter);

        //targetCharacter.traitContainer.RemoveTrait(targetCharacter, "Wooed");
        if (targetCharacter.currentActionNode != null && targetCharacter.currentActionNode.action == this) {
            targetCharacter.SetCurrentActionNode(null, null, null);
        }
    }
    public override string ReactionToActor(Character actor, IPointOfInterest target, Character witness,
        ActualGoapNode node, REACTION_STATUS status) {
        string response = base.ReactionToActor(actor, target, witness, node, status);

        if (status == REACTION_STATUS.WITNESSED) {
            //If witnessed
            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, witness, actor, status, node);
        }

        if (target is Character targetCharacter) {
            if (actor.relationshipContainer.HasRelationshipWith(targetCharacter, RELATIONSHIP_TYPE.LOVER) == false) { //if actor and target are not lovers
                Character actorLover = CharacterManager.Instance.GetCharacterByID(actor.relationshipContainer
                    .GetFirstRelatableIDWithRelationship(RELATIONSHIP_TYPE.LOVER));
                if (actorLover != null) {
                    if (actorLover != targetCharacter) {
                        //if actor has a lover that is different from target
                        //actor considered Infraction.
                        //CrimeManager.Instance.ReactToCrime(witness, actor, node, node.associatedJobType, 
                        //    CRIME_SEVERITY.Infraction);    
                        CrimeManager.Instance.ReactToCrime(witness, actor, target, target.factionOwner, node.crimeType, node, status);
                    } else if (actorLover == witness) {
                        //if witness is lover of actor
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Betrayal, witness, actor, status, node);
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disapproval, witness, actor, status, node);
                    }
                        
                }
                
                Character targetLover = CharacterManager.Instance.GetCharacterByID(targetCharacter.relationshipContainer
                    .GetFirstRelatableIDWithRelationship(RELATIONSHIP_TYPE.LOVER));
                if (targetLover != null) {
                    if (witness == targetLover) {
                        //witness is lover of target
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Anger, witness, actor, status, node);
                    }
                    if (witness.relationshipContainer.IsFriendsWith(actor) || witness.relationshipContainer.IsFamilyMember(actor)) {
                        //if actor is friend/close friend or relative
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Betrayal, witness, actor, status, node);    
                    } else {
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Resentment, witness, actor, status, node);
                    }
                }
            } else {
                //actor and target are lovers
                if(witness.relationshipContainer.HasRelationshipWith(targetCharacter, RELATIONSHIP_TYPE.AFFAIR)) {
                    //if witness and target have an affair
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Resentment, witness, actor, status, node);
                }
            }
            
        }
        return response;
    }
    public override string ReactionToTarget(Character actor, IPointOfInterest target, Character witness,
        ActualGoapNode node, REACTION_STATUS status) {
       string response = base.ReactionToTarget(actor, target, witness, node, status);

        if (status == REACTION_STATUS.WITNESSED) {
            //If witnessed
            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, witness, actor, status, node);
        }

        if (target is Character targetCharacter) {
            if (actor.relationshipContainer.HasRelationshipWith(targetCharacter, RELATIONSHIP_TYPE.LOVER) == false) { //if actor and target are not lovers
                Character targetLover = CharacterManager.Instance.GetCharacterByID(targetCharacter.relationshipContainer
                    .GetFirstRelatableIDWithRelationship(RELATIONSHIP_TYPE.LOVER));
                if (targetLover != null) {
                    if (targetLover != actor) {
                        //if target has a lover that is different from actor
                        //target considered Infraction.
                        //CrimeManager.Instance.ReactToCrime(witness, targetCharacter, node, node.associatedJobType, 
                        //    CRIME_SEVERITY.Infraction);
                        CrimeManager.Instance.ReactToCrime(witness, targetCharacter, actor, actor.factionOwner, node.crimeType, node, status);
                    } else if (targetLover == witness) {
                        //if witness is lover of target
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Betrayal, witness, targetCharacter, status, node);
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disapproval, witness, targetCharacter, status, node);
                    }
                        
                }
                
                Character actorLover = CharacterManager.Instance.GetCharacterByID(actor.relationshipContainer
                    .GetFirstRelatableIDWithRelationship(RELATIONSHIP_TYPE.LOVER));
                if (actorLover != null) {
                    if (witness == targetLover) {
                        //witness is lover of actor
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Anger, witness, targetCharacter, status, node);
                    }
                    if (witness.relationshipContainer.IsFriendsWith(actor) || witness.relationshipContainer.IsFamilyMember(actor)) {
                        //if actor is friend/close friend or relative
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Betrayal, witness, targetCharacter, status, node);    
                    } else {
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Resentment, witness, targetCharacter, status, node);
                    }
                }
            } else {
                //actor and target are lovers
                if(witness.relationshipContainer.HasRelationshipWith(actor, RELATIONSHIP_TYPE.AFFAIR)) {
                    //if witness and actor have an affair
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Resentment, witness, targetCharacter, status, node);
                }
            }
            
        }
        return response;
    }
    public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness) {
        if (node.poiTarget is Character character) {
            if (node.actor.relationshipContainer.HasRelationshipWith(character, RELATIONSHIP_TYPE.LOVER) == false) {
                return REACTABLE_EFFECT.Negative;
            }
        }
        return REACTABLE_EFFECT.Neutral;
    }
    public override CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, ActualGoapNode crime) {
        if(target is Character targetCharacter) {
            if ((actor.relationshipContainer.HasRelationshipWith(targetCharacter, RELATIONSHIP_TYPE.LOVER) == false && actor.relationshipContainer.HasRelationship(RELATIONSHIP_TYPE.LOVER))
                || (targetCharacter.relationshipContainer.HasRelationshipWith(actor, RELATIONSHIP_TYPE.LOVER) == false && targetCharacter.relationshipContainer.HasRelationship(RELATIONSHIP_TYPE.LOVER))){
                return CRIME_TYPE.Infidelity;
            }
        }
        return base.GetCrimeType(actor, target, crime);
    }
    #endregion

    #region Effects
    public void PreMakeLoveSuccess(ActualGoapNode goapNode) {
        Character actor = goapNode.actor;
        Character targetCharacter = goapNode.poiTarget as Character;
        Assert.IsNotNull(targetCharacter, $"Target character of Make Love Action by {goapNode.actor.name} is not a character!");
        Bed bed = null;
        if (actor.tileObjectComponent.primaryBed != null) {
            if(actor.tileObjectComponent.primaryBed.gridTileLocation != null 
                && (actor.gridTileLocation == actor.tileObjectComponent.primaryBed.gridTileLocation || actor.gridTileLocation.IsNeighbour(actor.tileObjectComponent.primaryBed.gridTileLocation))) {
                bed = actor.tileObjectComponent.primaryBed;
            }
        } else if (targetCharacter.tileObjectComponent.primaryBed != null) {
            if (targetCharacter.tileObjectComponent.primaryBed.gridTileLocation != null
                && (actor.gridTileLocation == targetCharacter.tileObjectComponent.primaryBed.gridTileLocation || actor.gridTileLocation.IsNeighbour(targetCharacter.tileObjectComponent.primaryBed.gridTileLocation))) {
                bed = targetCharacter.tileObjectComponent.primaryBed;
            }
        }
        Assert.IsNotNull(bed, $"Target bed of Make Love Action by {goapNode.actor.name} targeting {goapNode.poiTarget.name} is null!");

        goapNode.actor.UncarryPOI(targetCharacter, dropLocation: bed.gridTileLocation);

        bed.OnDoActionToObject(goapNode);

        goapNode.actor.needsComponent.AdjustDoNotGetBored(1);
        targetCharacter.needsComponent.AdjustDoNotGetBored(1);

        goapNode.actor.jobComponent.IncreaseNumOfTimesActionDone(this);
        targetCharacter.jobComponent.IncreaseNumOfTimesActionDone(this);

        targetCharacter.SetCurrentActionNode(goapNode.actor.currentActionNode, goapNode.actor.currentJob, goapNode.actor.currentPlan);
        goapNode.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    }
    public void PerTickMakeLoveSuccess(ActualGoapNode goapNode) {
        Character targetCharacter = goapNode.poiTarget as Character;
        goapNode.actor.needsComponent.AdjustHappiness(4.1f);
        targetCharacter.needsComponent.AdjustHappiness(4.1f);
        //goapNode.actor.needsComponent.AdjustStamina(1f);
        //targetCharacter.needsComponent.AdjustStamina(1f);
    }
    public void AfterMakeLoveSuccess(ActualGoapNode goapNode) {
        Character actor = goapNode.actor;
        Character targetCharacter = goapNode.poiTarget as Character;
        Bed bed = null;
        if (actor.tileObjectComponent.primaryBed != null) {
            if (actor.tileObjectComponent.primaryBed.gridTileLocation != null
                && (actor.gridTileLocation == actor.tileObjectComponent.primaryBed.gridTileLocation || actor.gridTileLocation.IsNeighbour(actor.tileObjectComponent.primaryBed.gridTileLocation))) {
                bed = actor.tileObjectComponent.primaryBed;
            }
        } else if (targetCharacter.tileObjectComponent.primaryBed != null) {
            if (targetCharacter.tileObjectComponent.primaryBed.gridTileLocation != null
                && (actor.gridTileLocation == targetCharacter.tileObjectComponent.primaryBed.gridTileLocation || actor.gridTileLocation.IsNeighbour(targetCharacter.tileObjectComponent.primaryBed.gridTileLocation))) {
                bed = targetCharacter.tileObjectComponent.primaryBed;
            }
        }
        //Bed bed = goapNode.actor.gridTileLocation.structure.GetTileObjectsOfType(TILE_OBJECT_TYPE.BED).FirstOrDefault() as Bed;
        bed?.OnDoneActionToObject(goapNode);
        goapNode.actor.needsComponent.AdjustDoNotGetBored(-1);
        targetCharacter.needsComponent.AdjustDoNotGetBored(-1);

        //**After Effect 1**: If Actor and Target are Lovers, they both gain Cheery trait. If Actor and Target are Affairs, they both gain Ashamed trait.
        if (actor is SeducerSummon) {
            //kill the target character
            targetCharacter.Death("seduced", goapNode, actor);
        }

        //if (goapNode.actor.relationshipContainer.HasRelationshipWith(targetCharacter, RELATIONSHIP_TYPE.LOVER)) {
        //    goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Satisfied", targetCharacter);
        //    targetCharacter.traitContainer.AddTrait(targetCharacter, "Satisfied", goapNode.actor);
        //} else if (goapNode.actor.relationshipContainer.HasRelationshipWith(targetCharacter, RELATIONSHIP_TYPE.AFFAIR)) {
        //    goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Ashamed", targetCharacter);
        //    targetCharacter.traitContainer.AddTrait(targetCharacter, "Ashamed", goapNode.actor);
        //}
        //goapNode.actor.ownParty.RemovePOI(targetCharacter);
        //targetCharacter.traitContainer.RemoveTrait(targetCharacter, "Wooed");

        //targetCharacter.RemoveTargettedByAction(this);
        if (targetCharacter.currentActionNode.action == this) {
            targetCharacter.SetCurrentActionNode(null, null, null);
        }
    }
    #endregion

    #region Preconditions
    private bool IsTargetInvited(Character actor, IPointOfInterest poiTarget, object[] otherData, JOB_TYPE jobType) {
        return actor.carryComponent.IsPOICarried(poiTarget);
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            if (poiTarget.gridTileLocation != null && actor.trapStructure.IsTrappedAndTrapStructureIsNot(poiTarget.gridTileLocation.structure)) {
                return false;
            }
            if (poiTarget.gridTileLocation != null && poiTarget.gridTileLocation.collectionOwner.isPartOfParentRegionMap && actor.trapStructure.IsTrappedAndTrapHexIsNot(poiTarget.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner)) {
                return false;
            }
            Character target = poiTarget as Character;
            if (target == actor) {
                return false;
            }
            //if (target.currentAlterEgoName != CharacterManager.Original_Alter_Ego) { //do not woo characters that have transformed to other alter egos
            //    return false;
            //}
            if (!target.canPerform) { //target.traitContainer.HasTraitOf(TRAIT_TYPE.DISABLER, TRAIT_EFFECT.NEGATIVE)
                return false;
            }
            if (target.stateComponent.currentState is CombatState) { //do not invite characters that are currently in combat
                return false;
            }
            if (target.returnedToLife) { //do not woo characters that have been raised from the dead
                return false;
            }
            if (target.carryComponent.masterCharacter.movementComponent.isTravellingInWorld || target.currentRegion != actor.currentRegion) {
                return false; //target is outside the map
            }
            if (GetValidBedForActor(actor, target) == null) {
                return false;/**/
            }
            if (!(actor is SeducerSummon)) { //ignore relationships if succubus
                if (!actor.relationshipContainer.HasRelationshipWith(target, RELATIONSHIP_TYPE.LOVER) && !actor.relationshipContainer.HasRelationshipWith(target, RELATIONSHIP_TYPE.AFFAIR)) {
                    return false; //only lovers and affairs can make love
                }
            }
            return true;
        }
        return false;
    }
    #endregion

    private Bed GetValidBedForActor(Character actor, [NotNull]Character target) {
        Bed bedToUse = null;
        if(actor.tileObjectComponent.primaryBed != null) {
            bedToUse = actor.tileObjectComponent.primaryBed;
        } else if (target.tileObjectComponent.primaryBed != null) {
            bedToUse = target.tileObjectComponent.primaryBed;
        }
        return bedToUse;
        //if (actor is Summon) {
        //    //check un owned dwellings for possible beds
        //    //Bed nearestBed = null;
        //    //if (target.homeSettlement != null) {
        //    //    List<Bed> beds = target.homeSettlement.GetTileObjectsOfTypeThatMeetCriteria<Bed>(b => b.mapObjectState == MAP_OBJECT_STATE.BUILT && b.IsAvailable() && b.GetActiveUserCount() == 0);
        //    //    float nearestDistance = 0f;
        //    //    for (int i = 0; i < beds.Count; i++) {
        //    //        Bed bed = beds[i];
        //    //        float distanceFromActor = actor.gridTileLocation.GetDistanceTo(bed.gridTileLocation);
        //    //        if (nearestBed == null || distanceFromActor < nearestDistance) {
        //    //            nearestBed = bed;
        //    //            nearestDistance = distanceFromActor;
        //    //        }
        //    //    }
        //    //}
        //    List<Dwelling> dwellings =
        //        actor.currentRegion.GetStructuresAtLocation<Dwelling>(STRUCTURE_TYPE.DWELLING);
        //    Bed nearestBed = null;
        //    float nearestDistance = 0f;
        //    for (int i = 0; i < dwellings.Count; i++) {
        //        Dwelling currDwelling = dwellings[i];
        //        Bed dwellingBed = currDwelling.GetTileObjectOfType<Bed>(TILE_OBJECT_TYPE.BED);
        //        if (dwellingBed != null && dwellingBed.mapObjectState == MAP_OBJECT_STATE.BUILT && dwellingBed.IsAvailable() && dwellingBed.GetActiveUserCount() == 0) {
        //            float distanceFromActor = actor.gridTileLocation.GetDistanceTo(dwellingBed.gridTileLocation);
        //            if (nearestBed == null || distanceFromActor < nearestDistance) {
        //                nearestBed = dwellingBed;
        //                nearestDistance = distanceFromActor;
        //            }
        //        }
        //    }
        //    return nearestBed;
        //} else {
        //    if(actor.homeStructure != null) {
        //        Bed actorBed = actor.homeStructure.GetTileObjectOfType<Bed>(TILE_OBJECT_TYPE.BED);
        //        if (actorBed != null && actorBed.GetActiveUserCount() == 0) {
        //            return actorBed;
        //        } else if (target.homeStructure != null){
        //            Bed targetBed = target.homeStructure.GetTileObjectOfType<Bed>(TILE_OBJECT_TYPE.BED);
        //            if (targetBed != null && targetBed.GetActiveUserCount() == 0) {
        //                return targetBed;
        //            }
        //        }
        //    }
        //    return null;
        //}   
    }
    
    //#region Intel Reactions
    //private List<string> MakeLoveSuccessReactions(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
    //    List<string> reactions = new List<string>();
    //    Character target = targetCharacter;
    //    //RELATIONSHIP_EFFECT recipientRelationshipWithActor = recipient.GetRelationshipEffectWith(actor);
    //    //RELATIONSHIP_EFFECT recipientRelationshipWithTarget = recipient.GetRelationshipEffectWith(target);
    //    Relatable actorLover = actor.relationshipContainer.GetFirstRelatableWithRelationship(RELATIONSHIP_TRAIT.LOVER);
    //    Relatable targetLover = target.relationshipContainer.GetFirstRelatableWithRelationship(RELATIONSHIP_TRAIT.LOVER);
    //    Relatable actorParamour = actor.relationshipContainer.GetFirstRelatableWithRelationship(RELATIONSHIP_TRAIT.AFFAIR);
    //    Relatable targetParamour = target.relationshipContainer.GetFirstRelatableWithRelationship(RELATIONSHIP_TRAIT.AFFAIR);


    //    bool hasFled = false;
    //    if (isOldNews) {
    //        reactions.Add("This is old news.");
    //        if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //            hasFled = true;
    //            recipient.combatComponent.AddAvoidInRange(actor, reason: "saw something shameful");
    //        }
    //    } else {
    //        //- Recipient is the Actor
    //        if(recipient == actor) {
    //            if(targetLover == recipient.currentAlterEgo) {
    //                reactions.Add("That's private!");
    //            } else if (targetParamour == recipient.currentAlterEgo) {
    //                reactions.Add("Don't tell anyone. *wink**wink*");
    //            }
    //        }
    //        //- Recipient is the Target
    //        else if (recipient == target) {
    //            if (actorLover == recipient.currentAlterEgo) {
    //                reactions.Add("That's private!");
    //            } else if (actorParamour == recipient.currentAlterEgo) {
    //                reactions.Add("Don't you dare judge me!");
    //            }
    //        }
    //        //- Recipient is Actor's Lover
    //        else if (recipient.currentAlterEgo == actorLover) {
    //            string response = string.Empty;
    //            if (RelationshipManager.Instance.RelationshipDegradation(actor, recipient, this) ) {
    //                response = string.Format("I've had enough of {0}'s shenanigans!", actor.name);
    //                recipient.CreateUndermineJobOnly(actor, "informed", status);
    //            } else {
    //                response = string.Format("I'm still the one {0} comes home to.", actor.name);
    //                if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                    hasFled = true;
    //                    recipient.combatComponent.AddAvoidInRange(actor, reason: "saw something shameful");
    //                }
    //            }
    //            if(recipient.relationshipContainer.HasRelationshipWith(target.currentAlterEgo, RELATIONSHIP_TRAIT.AFFAIR)) {
    //                if (RelationshipManager.Instance.RelationshipDegradation(target, recipient, this)) {
    //                    response += string.Format(" {0} seduced both of us. {1} must pay for this.", target.name, Utilities.GetPronounString(target.gender, PRONOUN_TYPE.SUBJECTIVE, true));
    //                    recipient.CreateUndermineJobOnly(target, "informed", status);
    //                } else {
    //                    response += string.Format(" I already know that {0} is a harlot.", target.name);
    //                    if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                        hasFled = true;
    //                        recipient.combatComponent.AddAvoidInRange(actor, reason: "saw something shameful");
    //                    }
    //                }
    //            }else if (recipient.relationshipContainer.HasRelationshipWith(target.currentAlterEgo, RELATIONSHIP_TRAIT.RELATIVE)) {
    //                if (RelationshipManager.Instance.RelationshipDegradation(target, recipient, this)) {
    //                    response += string.Format(" {0} is a snake! I can't believe {1} would do this to me.", target.name, Utilities.GetPronounString(target.gender, PRONOUN_TYPE.SUBJECTIVE, false));
    //                    recipient.CreateUndermineJobOnly(target, "informed", status);
    //                } else {
    //                    response += string.Format(" {0} is my blood. Blood is thicker than water.", target.name);
    //                    if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                        hasFled = true;
    //                        recipient.combatComponent.AddAvoidInRange(actor, reason: "saw something shameful");
    //                    }
    //                }
    //            } else if (recipient.relationshipContainer.HasRelationshipWith(target.currentAlterEgo, RELATIONSHIP_TRAIT.FRIEND)) {
    //                if (RelationshipManager.Instance.RelationshipDegradation(target, recipient, this)) {
    //                    response += string.Format(" {0} is a snake! I can't believe {1} would do this to me.", target.name, Utilities.GetPronounString(target.gender, PRONOUN_TYPE.SUBJECTIVE, false));
    //                    recipient.CreateUndermineJobOnly(target, "informed", status);
    //                } else {
    //                    response += string.Format(" My friendship with {0} is much stronger than this incident.", target.name);
    //                    if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                        hasFled = true;
    //                        recipient.combatComponent.AddAvoidInRange(actor, reason: "saw something shameful");
    //                    }
    //                }
    //            } else if (recipient.relationshipContainer.HasRelationshipWith(target.currentAlterEgo, RELATIONSHIP_TRAIT.ENEMY)) {
    //                response += string.Format(" I always knew that {0} is a snake. {1} must pay for this!", target.name, Utilities.GetPronounString(target.gender, PRONOUN_TYPE.SUBJECTIVE, true));
    //                recipient.CreateUndermineJobOnly(target, "informed", status);
    //            } else if (!recipient.relationshipContainer.HasRelationshipWith(target.currentAlterEgo)) {
    //                if (RelationshipManager.Instance.RelationshipDegradation(target, recipient, this)) {
    //                    response += string.Format(" {0} is a snake. {1} must pay for this!", target.name, Utilities.GetPronounString(target.gender, PRONOUN_TYPE.SUBJECTIVE, true));
    //                    recipient.CreateUndermineJobOnly(target, "informed", status);
    //                } else {
    //                    response += string.Format(" I'm not even going to bother myself with {0}.", target.name);
    //                    if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                        hasFled = true;
    //                        recipient.combatComponent.AddAvoidInRange(actor, reason: "saw something shameful");
    //                    }
    //                }
    //            }
    //            reactions.Add(response);
    //        }
    //        //- Recipient is Target's Lover
    //        else if (recipient.currentAlterEgo == targetLover) {
    //            string response = string.Empty;
    //            if (RelationshipManager.Instance.RelationshipDegradation(target, recipient, this)) {
    //                response = string.Format("I've had enough of {0}'s shenanigans!", target.name);
    //                recipient.CreateUndermineJobOnly(target, "informed", status);
    //            } else {
    //                response = string.Format("I'm still the one {0} comes home to.", target.name);
    //                if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                    hasFled = true;
    //                    recipient.combatComponent.AddAvoidInRange(target, reason: "saw something shameful");
    //                }
    //            }
    //            if (recipient.relationshipContainer.HasRelationshipWith(actor.currentAlterEgo, RELATIONSHIP_TRAIT.AFFAIR)) {
    //                if (RelationshipManager.Instance.RelationshipDegradation(actor, recipient, this)) {
    //                    response += string.Format(" {0} seduced both of us. {1} must pay for this.", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, true));
    //                    recipient.CreateUndermineJobOnly(actor, "informed", status);
    //                } else {
    //                    response += string.Format(" I already know that {0} is a harlot.", actor.name);
    //                    if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                        hasFled = true;
    //                        recipient.combatComponent.AddAvoidInRange(target, reason: "saw something shameful");
    //                    }
    //                }
    //            } else if (recipient.relationshipContainer.HasRelationshipWith(actor.currentAlterEgo, RELATIONSHIP_TRAIT.RELATIVE)) {
    //                if (RelationshipManager.Instance.RelationshipDegradation(actor, recipient, this)) {
    //                    response += string.Format(" {0} is a snake! I can't believe {1} would do this to me.", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, false));
    //                    recipient.CreateUndermineJobOnly(actor, "informed", status);
    //                } else {
    //                    response += string.Format(" {0} is my blood. Blood is thicker than water.", actor.name);
    //                    if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                        hasFled = true;
    //                        recipient.combatComponent.AddAvoidInRange(target, reason: "saw something shameful");
    //                    }
    //                }
    //            } else if (recipient.relationshipContainer.HasRelationshipWith(actor.currentAlterEgo, RELATIONSHIP_TRAIT.FRIEND)) {
    //                if (RelationshipManager.Instance.RelationshipDegradation(actor, recipient, this)) {
    //                    response += string.Format(" {0} is a snake! I can't believe {1} would do this to me.", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, false));
    //                    recipient.CreateUndermineJobOnly(actor, "informed", status);
    //                } else {
    //                    response += string.Format(" My friendship with {0} is much stronger than this incident.", actor.name);
    //                    if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                        hasFled = true;
    //                        recipient.combatComponent.AddAvoidInRange(target, reason: "saw something shameful");
    //                    }
    //                }
    //            } else if (recipient.relationshipContainer.HasRelationshipWith(actor.currentAlterEgo, RELATIONSHIP_TRAIT.ENEMY)) {
    //                response += string.Format(" I always knew that {0} is a snake. {1} must pay for this!", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, true));
    //                recipient.CreateUndermineJobOnly(actor, "informed", status);
    //            } else if (!recipient.relationshipContainer.HasRelationshipWith(actor.currentAlterEgo)) {
    //                if (RelationshipManager.Instance.RelationshipDegradation(actor, recipient, this)) {
    //                    response += string.Format(" {0} is a snake. {1} must pay for this!", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, true));
    //                    recipient.CreateUndermineJobOnly(actor, "informed", status);
    //                } else {
    //                    response += string.Format(" I'm not even going to bother myself with {0}.", actor.name);
    //                    if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                        hasFled = true;
    //                        recipient.combatComponent.AddAvoidInRange(target, reason: "saw something shameful");
    //                    }
    //                }
    //            }
    //            reactions.Add(response);
    //        }
    //        //- Recipient is Actor/Target's Paramour
    //        else if (recipient.currentAlterEgo == actorParamour || recipient.currentAlterEgo == targetParamour) {
    //            reactions.Add("I have no right to complain. Bu..but I wish that we could be like that.");
    //            AddTraitTo(recipient, "Heartbroken");
    //        }
    //        //- Recipient has a positive relationship with Actor's Lover and Actor's Lover is not the Target
    //        else if (actorLover != null && recipient.relationshipContainer.GetRelationshipEffectWith(actorLover) == RELATIONSHIP_EFFECT.POSITIVE && actorLover != target.currentAlterEgo) {
    //            if (RelationshipManager.Instance.RelationshipDegradation(actor, recipient, this)) {
    //                AlterEgoData ego = actorLover as AlterEgoData;
    //                reactions.Add(string.Format("{0} is cheating on {1}?! I must let {2} know.", actor.name, actorLover.relatableName, Utilities.GetPronounString(ego.owner.gender, PRONOUN_TYPE.OBJECTIVE, false)));
    //                recipient.CreateShareInformationJob(ego.owner, this);
    //            } else {
    //                reactions.Add(string.Format("{0} is cheating on {1}? I don't want to get involved.", actor.name, actorLover.relatableName));
    //                if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                    hasFled = true;
    //                    recipient.combatComponent.AddAvoidInRange(actor, reason: "saw something shameful");
    //                }
    //            }
    //        }
    //        //- Recipient has a positive relationship with Target's Lover and Target's Lover is not the Actor
    //        else if (targetLover != null && recipient.relationshipContainer.GetRelationshipEffectWith(targetLover) == RELATIONSHIP_EFFECT.POSITIVE && targetLover != actor.currentAlterEgo) {
    //            if (RelationshipManager.Instance.RelationshipDegradation(target, recipient, this)) {
    //                AlterEgoData ego = targetLover as AlterEgoData;
    //                reactions.Add(string.Format("{0} is cheating on {1}?! I must let {2} know.", target.name, targetLover.relatableName, Utilities.GetPronounString(ego.owner.gender, PRONOUN_TYPE.OBJECTIVE, false)));
    //                recipient.CreateShareInformationJob(ego.owner, this);
    //            } else {
    //                reactions.Add(string.Format("{0} is cheating on {1}? I don't want to get involved.", target.name, targetLover.relatableName));
    //                if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                    hasFled = true;
    //                    recipient.combatComponent.AddAvoidInRange(target, reason: "saw something shameful");
    //                }
    //            }
    //        }
    //        //- Recipient has a negative relationship with Actor's Lover and Actor's Lover is not the Target
    //        else if (actorLover != null && recipient.relationshipContainer.GetRelationshipEffectWith(actorLover) == RELATIONSHIP_EFFECT.NEGATIVE && actorLover != target.currentAlterEgo) {
    //            AlterEgoData ego = actorLover as AlterEgoData;
    //            reactions.Add(string.Format("{0} is cheating on {1}? {2} got what {3} deserves.", actor.name, actorLover.relatableName, Utilities.GetPronounString(ego.owner.gender, PRONOUN_TYPE.SUBJECTIVE, true), Utilities.GetPronounString(ego.owner.gender, PRONOUN_TYPE.SUBJECTIVE, false)));
    //            if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                hasFled = true;
    //                recipient.combatComponent.AddAvoidInRange(actor, reason: "saw something shameful");
    //            }
    //        }
    //        //- Recipient has a negative relationship with Target's Lover and Target's Lover is not the Actor
    //        else if (targetLover != null && recipient.relationshipContainer.GetRelationshipEffectWith(targetLover) == RELATIONSHIP_EFFECT.NEGATIVE && targetLover != actor.currentAlterEgo) {
    //            AlterEgoData ego = targetLover as AlterEgoData;
    //            reactions.Add(string.Format("{0} is cheating on {1}? {2} got what {3} deserves.", target.name, targetLover.relatableName, Utilities.GetPronounString(ego.owner.gender, PRONOUN_TYPE.SUBJECTIVE, true), Utilities.GetPronounString(ego.owner.gender, PRONOUN_TYPE.SUBJECTIVE, false)));
    //            if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                hasFled = true;
    //                recipient.combatComponent.AddAvoidInRange(target, reason: "saw something shameful");
    //            }
    //        }
    //        //- Recipient has a no relationship with Actor's Lover and Actor's Lover is not the Target
    //        else if (actorLover != null && recipient.relationshipContainer.GetRelationshipEffectWith(actorLover) == RELATIONSHIP_EFFECT.NONE && actorLover != target.currentAlterEgo) {
    //            reactions.Add(string.Format("{0} is cheating on {1}? I don't want to get involved.", actor.name, actorLover.relatableName));
    //            RelationshipManager.Instance.RelationshipDegradation(actor, recipient, this);
    //        }
    //        //- Recipient has no relationship with Target's Lover and Target's Lover is not the Actor
    //        else if (targetLover != null && recipient.relationshipContainer.GetRelationshipEffectWith(targetLover) == RELATIONSHIP_EFFECT.NONE && targetLover != actor.currentAlterEgo) {
    //            reactions.Add(string.Format("{0} is cheating on {1}? I don't want to get involved.", target.name, targetLover.relatableName));
    //            RelationshipManager.Instance.RelationshipDegradation(target, recipient, this);
    //        }
    //        //- Else Catcher
    //        else {
    //            reactions.Add("That is none of my business.");
    //            if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                hasFled = true;
    //                recipient.combatComponent.AddAvoidInRange(actor, reason: "saw something shameful");
    //            }
    //        }
    //    }

    //    if (status == SHARE_INTEL_STATUS.WITNESSED && !hasFled) {
    //        if (recipient.relationshipContainer.HasRelationshipWith(actor.currentAlterEgo, RELATIONSHIP_TRAIT.LOVER) 
    //            || recipient.relationshipContainer.HasRelationshipWith(actor.currentAlterEgo, RELATIONSHIP_TRAIT.AFFAIR)) {
    //            recipient.CreateWatchEvent(this, null, actor);
    //        } else if (recipient.relationshipContainer.HasRelationshipWith(target.currentAlterEgo, RELATIONSHIP_TRAIT.LOVER)
    //            || recipient.relationshipContainer.HasRelationshipWith(target.currentAlterEgo, RELATIONSHIP_TRAIT.AFFAIR)) {
    //            recipient.CreateWatchEvent(this, null, target);
    //        }
    //    }
    //    return reactions;
    //}
    //#endregion
}