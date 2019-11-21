﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;  
using Traits;
public class BuryCharacter : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.DIRECT; } }

    public BuryCharacter() : base(INTERACTION_TYPE.BURY_CHARACTER) {
        actionLocationType = ACTION_LOCATION_TYPE.RANDOM_LOCATION_B;
        actionIconString = GoapActionStateDB.Work_Icon;
        canBeAdvertisedEvenIfActorIsUnavailable = true;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
    }

    #region Overrides
    public override LocationStructure GetTargetStructure(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (otherData != null && otherData.Length == 1 && otherData[0] is LocationStructure) {
            return otherData[0] as LocationStructure;
        } else {
            return actor.specificLocation.GetRandomStructureOfType(STRUCTURE_TYPE.CEMETERY);
        }
    }
    protected override void ConstructBasePreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.IN_PARTY, conditionKey = string.Empty, isKeyANumber = false, target = GOAP_EFFECT_TARGET.TARGET }, IsInActorParty);
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_FROM_PARTY, conditionKey = string.Empty, isKeyANumber = false, target = GOAP_EFFECT_TARGET.TARGET });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Bury Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, object[] otherData) {
        return 1;
    }
    public override void OnStopWhileStarted(Character actor, IPointOfInterest target, object[] otherData) {
        base.OnStopWhileStarted(actor, target, otherData);
        Character targetCharacter = target as Character;
        actor.ownParty.RemoveCharacter(targetCharacter, false);
        targetCharacter.SetCurrentStructureLocation(targetCharacter.gridTileLocation.structure, false);
    }
    public override GoapActionInvalidity IsInvalid(Character actor, IPointOfInterest target, object[] otherData) {
        string stateName = "Target Missing";
        bool defaultTargetMissing = false;
        GoapActionInvalidity goapActionInvalidity = new GoapActionInvalidity(defaultTargetMissing, stateName);
        //bury cannot be invalid because all cases are handled by the requirements of the action
        return goapActionInvalidity;
    }
    #endregion

    #region State Effects
    public void PreBurySuccess(ActualGoapNode goapNode) {
        //TODO:
        //if (parentPlan.job != null) {
        //    if (parentPlan.job.jobType == JOB_TYPE.BURY) {
        //        currentState.SetIntelReaction(NormalBurySuccessIntelReaction);
        //    } else if (parentPlan.job.jobType == JOB_TYPE.HUNT_SERIAL_KILLER_VICTIM) {
        //        currentState.SetIntelReaction(SerialKillerBurySuccessIntelReaction);
        //    }
        //}
    }
    public void AfterBurySuccess(ActualGoapNode goapNode) {
        //if (parentPlan.job != null) {
        //    parentPlan.job.SetCannotCancelJob(true);
        //}
        //SetCannotCancelAction(true);

        Character targetCharacter = goapNode.poiTarget as Character;
        //**After Effect 1**: Remove Target from Actor's Party.
        goapNode.actor.ownParty.RemoveCharacter(targetCharacter, false);
        //**After Effect 2**: Place a Tombstone tile object in adjacent unoccupied tile, link it with Target.
        LocationGridTile chosenLocation = goapNode.actor.gridTileLocation;
        if (chosenLocation.isOccupied) {
            List<LocationGridTile> choices = goapNode.actor.gridTileLocation.UnoccupiedNeighbours.Where(x => x.structure == goapNode.actor.currentStructure).ToList();
            if (choices.Count > 0) {
                chosenLocation = choices[Random.Range(0, choices.Count)];
            }
            
        }
        Tombstone tombstone = new Tombstone(goapNode.actor.currentStructure);
        tombstone.SetCharacter(targetCharacter);
        goapNode.actor.currentStructure.AddPOI(tombstone, chosenLocation);
        //TODO: targetCharacter.CancelAllJobsTargettingThisCharacterExcept(JOB_TYPE.BURY, goapNode.actor);
        List<Character> characters = targetCharacter.relationshipContainer.relationships.Keys.Where(x => x is AlterEgoData).Select(x => (x as AlterEgoData).owner).ToList();
        if(characters != null) {
            for (int i = 0; i < characters.Count; i++) {
                characters[i].AddAwareness(tombstone);
            }
        }
    }
    #endregion

    #region Preconditions
    private bool IsInActorParty(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        Character target = poiTarget as Character;
        return target.currentParty == actor.currentParty;
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            Character targetCharacter = poiTarget as Character;
            //target character must be dead
            if (!targetCharacter.isDead) {
                return false;
            }
            //check that the charcater has been buried (has a grave)
            if (targetCharacter.grave != null) {
                return false;
            }
            return true;
        }
        return false;
    }
    #endregion

    //#region Intel Reactions
    //private List<string> NormalBurySuccessIntelReaction(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
    //    List<string> reactions = new List<string>();
    //    Character targetCharacter = poiTarget as Character;

    //    RELATIONSHIP_EFFECT relWithActor = recipient.relationshipContainer.GetRelationshipEffectWith(actor.currentAlterEgo);
    //    RELATIONSHIP_EFFECT relWithTarget = recipient.relationshipContainer.GetRelationshipEffectWith(targetCharacter.currentAlterEgo);

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
    //                reactions.Add("It is always sad to bury a fellow resident.");
    //            }
    //            //- Positive Relationship with Target
    //            else if (relWithTarget == RELATIONSHIP_EFFECT.POSITIVE) {
    //                recipient.traitContainer.AddTrait(recipient, "Heartbroken");
    //                if (UnityEngine.Random.Range(0, 2) == 0) {
    //                    bool triggerBrokenhearted = false;
    //                    Heartbroken heartbroken = recipient.traitContainer.GetNormalTrait("Heartbroken") as Heartbroken;
    //                    if (heartbroken != null) {
    //                        triggerBrokenhearted = UnityEngine.Random.Range(0, 100) < 20;
    //                    }
    //                    if (!triggerBrokenhearted) {
    //                        GoapPlanJob job = new GoapPlanJob(JOB_TYPE.HAPPINESS_RECOVERY, INTERACTION_TYPE.REMEMBER_FALLEN, targetCharacter.grave);
    //                        job.SetCancelOnFail(true);
    //                        recipient.jobQueue.AddJobInQueue(job);
    //                    } else {
    //                        heartbroken.TriggerBrokenhearted();
    //                    }
    //                }
    //                reactions.Add("Another good one bites the dust.");
    //            }
    //            //- Negative Relationship with Target
    //            else if (relWithTarget == RELATIONSHIP_EFFECT.POSITIVE) {
    //                recipient.traitContainer.AddTrait(recipient, "Satisfied");
    //                if (UnityEngine.Random.Range(0, 2) == 0) {
    //                    bool triggerBrokenhearted = false;
    //                    Heartbroken heartbroken = recipient.traitContainer.GetNormalTrait("Heartbroken") as Heartbroken;
    //                    if (heartbroken != null) {
    //                        triggerBrokenhearted = UnityEngine.Random.Range(0, 100) < 20;
    //                    }
    //                    if (!triggerBrokenhearted) {
    //                        GoapPlanJob job = new GoapPlanJob(JOB_TYPE.HAPPINESS_RECOVERY, INTERACTION_TYPE.SPIT, targetCharacter.grave);
    //                        job.SetCancelOnFail(true);
    //                        recipient.jobQueue.AddJobInQueue(job);
    //                    } else {
    //                        heartbroken.TriggerBrokenhearted();
    //                    }
    //                }
    //                reactions.Add(string.Format("Is it terrible to think that {0} deserved that?", targetCharacter.name));
    //            }
    //            //-  Positive or No Relationship with Actor, Actor has a positive relationship with Target
    //            else if ((relWithActor == RELATIONSHIP_EFFECT.POSITIVE || relWithActor == RELATIONSHIP_EFFECT.NONE) && actor.relationshipContainer.GetRelationshipEffectWith(targetCharacter.currentAlterEgo) == RELATIONSHIP_EFFECT.POSITIVE) {
    //                reactions.Add(string.Format("Burying someone close to you is probably the worst feeling ever. I feel for {0}.", actor.name));
    //            }
    //            //- Others
    //            else {
    //                reactions.Add("This isn't relevant to me.");
    //            }
    //        }
    //    }
    //    return reactions;
    //}
    //private List<string> SerialKillerBurySuccessIntelReaction(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
    //    List<string> reactions = new List<string>();
    //    Character targetCharacter = poiTarget as Character;

    //    RELATIONSHIP_EFFECT relWithActor = recipient.relationshipContainer.GetRelationshipEffectWith(actor.currentAlterEgo);
    //    RELATIONSHIP_EFFECT relWithTarget = recipient.relationshipContainer.GetRelationshipEffectWith(targetCharacter.currentAlterEgo);

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
    //                reactions.Add("Do not tell anybody, please!");
    //            }
    //            //- Positive Relationship with Actor
    //            else if (relWithActor == RELATIONSHIP_EFFECT.POSITIVE) {
    //                if (relWithTarget == RELATIONSHIP_EFFECT.POSITIVE) {
    //                    recipient.ReactToCrime(CRIME.MURDER, this, actorAlterEgo, status);
    //                    if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                        recipient.marker.AddAvoidInRange(actor, reason: "saw something shameful");
    //                    }
    //                    reactions.Add(string.Format("{0} is dear to me but {1} must be punished for killing {2}!", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, false), targetCharacter.name));
    //                } else if (relWithTarget == RELATIONSHIP_EFFECT.NEGATIVE) {
    //                    if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                        recipient.marker.AddAvoidInRange(actor, reason: "saw something shameful");
    //                    }
    //                    reactions.Add(string.Format("{0} is a terrible person so I'm sure there was a reason {1} offed {2}.", targetCharacter.name, actor.name, Utilities.GetPronounString(targetCharacter.gender, PRONOUN_TYPE.OBJECTIVE, false)));
    //                } else {
    //                    recipient.ReactToCrime(CRIME.MURDER, this, actorAlterEgo, status);
    //                    if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                        recipient.marker.AddAvoidInRange(actor, reason: "saw something shameful");
    //                    }
    //                    reactions.Add(string.Format("{0} is dear to me but {1} must be punished for killing {2}!", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, false), targetCharacter.name));
    //                }
    //            }
    //            //- Negative Relationship with Actor
    //            else if (relWithActor == RELATIONSHIP_EFFECT.NEGATIVE) {
    //                if (relWithTarget == RELATIONSHIP_EFFECT.POSITIVE) {
    //                    recipient.ReactToCrime(CRIME.MURDER, this, actorAlterEgo, status);
    //                    if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                        recipient.CreateKnockoutJob(actor);
    //                    } else if (status == SHARE_INTEL_STATUS.INFORMED) {
    //                        recipient.CreateUndermineJobOnly(actor, "informed");
    //                    }
    //                    reactions.Add(string.Format("{0} is the worst! {1} must be punished for killing {2}!", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, true), targetCharacter.name));
    //                } else if (relWithTarget == RELATIONSHIP_EFFECT.NEGATIVE) {
    //                    recipient.ReactToCrime(CRIME.MURDER, this, actorAlterEgo, status);
    //                    if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                        recipient.marker.AddAvoidInRange(actor, reason: "saw something shameful");
    //                    }
    //                    reactions.Add(string.Format("I may not like {0} but {1} must still be punished for killing {2}!", targetCharacter.name, actor.name, Utilities.GetPronounString(targetCharacter.gender, PRONOUN_TYPE.OBJECTIVE, false)));
    //                } else {
    //                    recipient.ReactToCrime(CRIME.MURDER, this, actorAlterEgo, status);
    //                    if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                        recipient.CreateKnockoutJob(actor);
    //                    } else if (status == SHARE_INTEL_STATUS.INFORMED) {
    //                        recipient.CreateUndermineJobOnly(actor, "informed");
    //                    }
    //                    reactions.Add(string.Format("{0} is the worst! {1} must be punished for killing {2}!", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, true), targetCharacter.name));
    //                }
    //            }
    //            //- No Relationship with Actor
    //            else if (relWithActor == RELATIONSHIP_EFFECT.NONE) {
    //                if (relWithTarget == RELATIONSHIP_EFFECT.POSITIVE) {
    //                    recipient.ReactToCrime(CRIME.MURDER, this, actorAlterEgo, status);
    //                    if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                        recipient.CreateKnockoutJob(actor);
    //                    } else if (status == SHARE_INTEL_STATUS.INFORMED) {
    //                        recipient.CreateUndermineJobOnly(actor, "informed");
    //                    }
    //                    reactions.Add(string.Format("{0} must be punished for killing {1}!", actor.name, targetCharacter.name));
    //                } else if (relWithTarget == RELATIONSHIP_EFFECT.NEGATIVE) {
    //                    recipient.ReactToCrime(CRIME.MURDER, this, actorAlterEgo, status);
    //                    if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                        recipient.marker.AddAvoidInRange(actor, reason: "saw something shameful");
    //                    }
    //                    reactions.Add(string.Format("I may not like {0} but {1} must still be punished for killing {2}!", targetCharacter.name, actor.name, Utilities.GetPronounString(targetCharacter.gender, PRONOUN_TYPE.OBJECTIVE, false)));
    //                } else {
    //                    recipient.ReactToCrime(CRIME.MURDER, this, actorAlterEgo, status);
    //                    if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                        recipient.marker.AddAvoidInRange(actor, reason: "saw something shameful");
    //                    }
    //                    reactions.Add(string.Format("{0} must be punished for killing {1}!", actor.name, targetCharacter.name));
    //                }
    //            }
    //        }
    //    }
    //    return reactions;
    //}
    //#endregion
}

public class BuryCharacterData : GoapActionData {
    public BuryCharacterData() : base(INTERACTION_TYPE.BURY_CHARACTER) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        Character targetCharacter = poiTarget as Character;
        //target character must be dead
        if (!targetCharacter.isDead) {
            return false;
        }
        //check that the charcater has been buried (has a grave)
        if (targetCharacter.grave != null) {
            return false;
        }
        return true;
    }
}
