﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;
using UtilityScripts;

public class DrinkBlood : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.CONSUME; } }

    public DrinkBlood() : base(INTERACTION_TYPE.DRINK_BLOOD) {
        actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
        actionIconString = GoapActionStateDB.Drink_Blood_Icon;
        doesNotStopTargetCharacter = true;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
        isNotificationAnIntel = true;
        logTags = new[] {LOG_TAG.Crimes, LOG_TAG.Needs};
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Unconscious", target = GOAP_EFFECT_TARGET.TARGET }, HasUnconsciousOrRestingTarget);
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Lethargic", target = GOAP_EFFECT_TARGET.TARGET });
        AddPossibleExpectedEffectForTypeAndTargetMatching(new GoapEffectConditionTypeAndTargetType(GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, GOAP_EFFECT_TARGET.ACTOR));
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Drink Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}:";
        int cost = 0;
        //if (actor.moodComponent.moodState == MOOD_STATE.Normal) {
        //    cost = UtilityScripts.Utilities.Rng.Next(50, 61);
        //    costLog += $" +{cost}(Normal Mood)";
        //} else if (actor.moodComponent.moodState == MOOD_STATE.Bad) {
        //    cost = UtilityScripts.Utilities.Rng.Next(20, 31);
        //    costLog += $" +{cost}(Low Mood)";
        //} else if (actor.moodComponent.moodState == MOOD_STATE.Critical) {
        //    cost = UtilityScripts.Utilities.Rng.Next(0, 11);
        //    costLog += $" +{cost}(Critical Mood)";
        //}
        if (target is Character targetCharacter) {
            if (targetCharacter.traitContainer.HasTrait("Vampire")) {
                cost += 2000;
                costLog += " +2000(Vampire)";
                actor.logComponent.AppendCostLog(costLog);
                //Skip further cost processing
                return cost;
            }
            if (!actor.isVagrant) {
                AWARENESS_STATE awarenessState = actor.relationshipContainer.GetAwarenessState(targetCharacter);
                if(actor.currentRegion != targetCharacter.currentRegion || awarenessState == AWARENESS_STATE.Missing || awarenessState == AWARENESS_STATE.Presumed_Dead
                    || targetCharacter.partyComponent.isMemberTheJoinedQuest) {
                    cost += 2000;
                    costLog += " +2000(not Vagrant and not Same Region/Missing/Presumed Dead/Joined a Party Quest)";
                    actor.logComponent.AppendCostLog(costLog);
                    //Skip further cost processing
                    return cost;
                }
            }
            if (targetCharacter.canPerform && targetCharacter.canMove) {
                cost += 80;
                costLog += " +80(Can Perform and Move)";
            }
            if (!targetCharacter.race.IsSapient()) {
                cost += 200;
                costLog += " +200(Not Sapient)";
            }
            if (actor.needsComponent.isHungry || (!actor.needsComponent.isHungry && !actor.needsComponent.isStarving)) {
                //if (actor.currentRegion != targetCharacter.currentRegion) {
                //    cost += 2000;
                //    costLog += " +2000(Starving, Diff Region)";
                //    actor.logComponent.AppendCostLog(costLog);
                //    //Skip further cost processing
                //    return cost;
                //}
                string opinionLabel = actor.relationshipContainer.GetOpinionLabel(targetCharacter);
                if (opinionLabel == RelationshipManager.Friend || opinionLabel == RelationshipManager.Close_Friend) {
                    cost += 2000;
                    costLog += " +2000(Hungry, Friend/Close)";
                    actor.logComponent.AppendCostLog(costLog);
                    //Skip further cost processing
                    return cost;
                } else if (actor.homeStructure != null && targetCharacter.currentStructure == actor.homeStructure && targetCharacter.traitContainer.HasTrait("Prisoner")) {
                    cost += 0;
                    costLog += " +0(Hungry, Prisoner and inside actor home)";
                } else if (opinionLabel == RelationshipManager.Rival) {
                    cost += 20;
                    costLog += " +20(Hungry, Rival)";
                } else if (opinionLabel == RelationshipManager.Enemy) {
                    cost += 40;
                    costLog += " +40(Hungry, Enemy)";
                } else if (opinionLabel == RelationshipManager.Acquaintance) {
                    cost += 75;
                    costLog += " +75(Hungry, Acquaintance)";
                } else {
                    cost += 40;
                    costLog += " +40(Hungry, Other)";
                }
            } else if (actor.needsComponent.isStarving) {
                //if (actor.currentRegion != targetCharacter.currentRegion) {
                //    cost += 2000;
                //    costLog += " +2000(Starving, Diff Region)";
                //    actor.logComponent.AppendCostLog(costLog);
                //    //Skip further cost processing
                //    return cost;
                //}
                string opinionLabel = actor.relationshipContainer.GetOpinionLabel(targetCharacter);
                if (actor.homeStructure != null && targetCharacter.currentStructure == actor.homeStructure && targetCharacter.traitContainer.HasTrait("Prisoner")) {
                    cost += 0;
                    costLog += " +0(Starving, Prisoner and inside actor home)";
                } else if (opinionLabel == RelationshipManager.Close_Friend) {
                    cost += 400;
                    costLog += " +400(Starving, Close Friend)";
                } else if (opinionLabel == RelationshipManager.Friend) {
                    cost += 300;
                    costLog += " +300(Starving, Friend)";
                } else if (opinionLabel == RelationshipManager.Rival) {
                    cost += 20;
                    costLog += " +20(Starving, Rival)";
                } else if (opinionLabel == RelationshipManager.Enemy) {
                    cost += 40;
                    costLog += " +40(Starving, Enemy)";
                } else if (opinionLabel == RelationshipManager.Acquaintance) {
                    cost += 75;
                    costLog += " +75(Starving, Acquaintance)";
                } else {
                    cost += 40;
                    costLog += " +40(Starving, Other)";
                }
            }
        }
        actor.logComponent.AppendCostLog(costLog);
        return cost;
    }
    public override void OnStopWhilePerforming(ActualGoapNode node) {
        base.OnStopWhilePerforming(node);
        Character actor = node.actor;
        actor.needsComponent.AdjustDoNotGetHungry(-1);
    }
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        GoapActionInvalidity actionInvalidity = base.IsInvalid(node);
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        if (actionInvalidity.isInvalid == false) {
            Character targetCharacter = poiTarget as Character;
            if (targetCharacter.canMove && targetCharacter.canPerform/*|| targetCharacter.canWitness || targetCharacter.IsAvailable() == false*/) {
                actionInvalidity.isInvalid = true;
                actionInvalidity.stateName = "Drink Fail";
            }
        }
        return actionInvalidity;
    }
    public override string ReactionToActor(Character actor, IPointOfInterest target, Character witness,
        ActualGoapNode node, REACTION_STATUS status) {
        string response = base.ReactionToActor(actor, target, witness, node, status);

        Vampire vampire = actor.traitContainer.GetTraitOrStatus<Vampire>("Vampire");
        if(vampire != null) {
            vampire.AddAwareCharacter(witness);
        }

        CrimeManager.Instance.ReactToCrime(witness, actor, target, target.factionOwner, node.crimeType, node, status);

        CRIME_SEVERITY severity = CrimeManager.Instance.GetCrimeSeverity(witness, actor, target, CRIME_TYPE.Vampire);
        if (severity != CRIME_SEVERITY.None && severity != CRIME_SEVERITY.Unapplicable) {
            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, witness, actor, status, node);
            string opinionLabel = witness.relationshipContainer.GetOpinionLabel(actor);
            if (opinionLabel == RelationshipManager.Acquaintance || opinionLabel == RelationshipManager.Friend || opinionLabel == RelationshipManager.Close_Friend) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Despair, witness, actor, status, node);
            }
            if (witness.traitContainer.HasTrait("Coward")) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Fear, witness, actor, status, node);
            } else if (!witness.traitContainer.HasTrait("Psychopath")) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Threatened, witness, actor, status, node);
            }
            if (target is Character targetCharacter) {
                string opinionToTarget = witness.relationshipContainer.GetOpinionLabel(targetCharacter);
                if (opinionToTarget == RelationshipManager.Friend || opinionToTarget == RelationshipManager.Close_Friend) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disapproval, witness, actor, status, node);
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Anger, witness, actor, status, node);
                } else if ((witness.relationshipContainer.IsFamilyMember(targetCharacter) || witness.relationshipContainer.HasRelationshipWith(targetCharacter, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.AFFAIR))
                     && opinionToTarget != RelationshipManager.Rival) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disapproval, witness, actor, status, node);
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Anger, witness, actor, status, node);
                } else if (opinionToTarget == RelationshipManager.Acquaintance || witness.faction == targetCharacter.faction || witness.homeSettlement == targetCharacter.homeSettlement) {
                    if (!witness.traitContainer.HasTrait("Psychopath")) {
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Anger, witness, actor, status, node);
                    }
                }
            }
        } else {
            if (witness.traitContainer.HasTrait("Hemophiliac")) {
                if(RelationshipManager.IsSexuallyCompatible(witness.sexuality, actor.sexuality, witness.gender, actor.gender)) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Arousal, witness, actor, status, node);
                } else {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Approval, witness, actor, status, node);
                }
            } else if (witness.traitContainer.HasTrait("Hemophobic")) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Threatened, witness, actor, status, node);
            }
        }
        //if (!witness.traitContainer.HasTrait("Vampire")) {
        //    //CrimeManager.Instance.ReactToCrime(witness, actor, node, node.associatedJobType, CRIME_SEVERITY.Heinous);
        //    CrimeManager.Instance.ReactToCrime(witness, actor, target, target.factionOwner, node.crimeType, node, status);
        //    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, witness, actor, status, node);

        //    string opinionLabel = witness.relationshipContainer.GetOpinionLabel(actor);
        //    if (opinionLabel == RelationshipManager.Acquaintance || opinionLabel == RelationshipManager.Friend || opinionLabel == RelationshipManager.Close_Friend) {
        //        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Despair, witness, actor, status, node);
        //    }
        //    if(witness.traitContainer.HasTrait("Coward")) {
        //        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Fear, witness, actor, status, node);
        //    } else if (!witness.traitContainer.HasTrait("Psychopath")) {
        //        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Threatened, witness, actor, status, node);
        //    }
        //}
        //if(target is Character) {
        //    Character targetCharacter = target as Character;
        //    string opinionLabel = witness.relationshipContainer.GetOpinionLabel(targetCharacter);
        //    if (opinionLabel == RelationshipManager.Friend || opinionLabel == RelationshipManager.Close_Friend) {
        //        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disapproval, witness, actor, status, node);
        //        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Anger, witness, actor, status, node);
        //    } else if ((witness.relationshipContainer.IsFamilyMember(targetCharacter) || witness.relationshipContainer.HasRelationshipWith(targetCharacter, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.AFFAIR))
        //         && opinionLabel != RelationshipManager.Rival) {
        //        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disapproval, witness, actor, status, node);
        //        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Anger, witness, actor, status, node);
        //    } else if (opinionLabel == RelationshipManager.Acquaintance || witness.faction == targetCharacter.faction || witness.homeSettlement == targetCharacter.homeSettlement) {
        //        if (!witness.traitContainer.HasTrait("Psychopath")) {
        //            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Anger, witness, actor, status, node);
        //        }
        //    }
        //}
        return response;
    }
    public override string ReactionOfTarget(Character actor, IPointOfInterest target, ActualGoapNode node,
        REACTION_STATUS status) {
        string response = base.ReactionOfTarget(actor, target, node, status);
        if (target is Character targetCharacter) {
            Vampire vampire = actor.traitContainer.GetTraitOrStatus<Vampire>("Vampire");
            if (vampire != null) {
                vampire.AddAwareCharacter(targetCharacter);
            }

            CrimeManager.Instance.ReactToCrime(targetCharacter, actor, targetCharacter, target.factionOwner, node.crimeType, node, status);

            CRIME_SEVERITY severity = CrimeManager.Instance.GetCrimeSeverity(targetCharacter, actor, target, CRIME_TYPE.Vampire);
            if (severity != CRIME_SEVERITY.None && severity != CRIME_SEVERITY.Unapplicable) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, targetCharacter, actor, status, node);
                string opinionLabel = targetCharacter.relationshipContainer.GetOpinionLabel(actor);
                if (targetCharacter.traitContainer.HasTrait("Coward", "Hemophobic")) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Fear, targetCharacter, actor, status, node);
                } else {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Threatened, targetCharacter, actor, status, node);
                }
                if (opinionLabel == RelationshipManager.Friend || opinionLabel == RelationshipManager.Close_Friend) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Betrayal, targetCharacter, actor, status, node);
                }
            } else {
                if (targetCharacter.traitContainer.HasTrait("Hemophiliac")) {
                    if (RelationshipManager.IsSexuallyCompatible(targetCharacter.sexuality, actor.sexuality, targetCharacter.gender, actor.gender)) {
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Arousal, targetCharacter, actor, status, node);
                    } else {
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Approval, targetCharacter, actor, status, node);
                    }
                } else if (targetCharacter.traitContainer.HasTrait("Hemophobic")) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Threatened, targetCharacter, actor, status, node);
                }
            }
        }
        //if (target is Character targetCharacter) {
        //    //CrimeManager.Instance.ReactToCrime(targetCharacter, actor, node, node.associatedJobType, CRIME_SEVERITY.Heinous);
        //    CrimeManager.Instance.ReactToCrime(targetCharacter, actor, target, target.factionOwner, node.crimeType, node, status);
        //    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, targetCharacter, actor, status, node);
        //    if (targetCharacter.traitContainer.HasTrait("Coward")) {
        //        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Fear, targetCharacter, actor, status, node);
        //    } else {
        //        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Threatened, targetCharacter, actor, status, node);
        //    }
        //    if (targetCharacter.relationshipContainer.IsFriendsWith(actor)) {
        //        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Betrayal, targetCharacter, actor, status, node);
        //    }
        //}
        return response;
    }
    public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness) {
        return REACTABLE_EFFECT.Negative;
    }
    public override CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, ActualGoapNode crime) {
        return CRIME_TYPE.Vampire;
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            //if (actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
            //    return false;
            //}
            if(poiTarget is Character targetCharacter) {
                return actor != targetCharacter && actor.traitContainer.HasTrait("Vampire") && !targetCharacter.isDead && targetCharacter.carryComponent.IsNotBeingCarried();
            }
            return actor != poiTarget && actor.traitContainer.HasTrait("Vampire");
        }
        return false;
    }
    #endregion

    #region Preconditions
    private bool HasUnconsciousOrRestingTarget(Character actor, IPointOfInterest poiTarget, object[] otherData, JOB_TYPE jobType) {
        Character target = poiTarget as Character;
        return target.traitContainer.HasTrait("Unconscious", "Resting");
    }
    #endregion

    #region Effects
    public void PreDrinkSuccess(ActualGoapNode goapNode) {
        goapNode.actor.needsComponent.AdjustDoNotGetHungry(1);
    }
    public void PerTickDrinkSuccess(ActualGoapNode goapNode) {
        goapNode.actor.needsComponent.AdjustFullness(34f);
    }
    public void AfterDrinkSuccess(ActualGoapNode goapNode) {
        //poiTarget.SetPOIState(POI_STATE.ACTIVE);
        Character actor = goapNode.actor;
        actor.needsComponent.AdjustDoNotGetHungry(-1);

        if (goapNode.poiTarget is Character targetCharacter) {
            if (targetCharacter.HasItem("Phylactery")) {
                Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "GoapAction", goapName, "activate_phylactery", goapNode, LOG_TAG.Misc);
                log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                log.AddLogToDatabase();

                targetCharacter.UnobtainItem("Phylactery");
                actor.AdjustHP(-500, ELEMENTAL_TYPE.Normal);
                if(actor.currentHP <= 0) {
                    actor.Death(deathFromAction: goapNode, responsibleCharacter: targetCharacter, _deathLog: goapNode.descriptionLog);
                } else {
                    actor.traitContainer.AddTrait(actor, "Unconscious", targetCharacter, goapNode);
                }
            } else {
                if (!targetCharacter.race.IsSapient()) {
                    actor.traitContainer.AddTrait(actor, "Poor Meal", targetCharacter);
                }
                if (GameUtilities.RollChance(98)) {
                    Lethargic lethargic = TraitManager.Instance.CreateNewInstancedTraitClass<Lethargic>("Lethargic");
                    targetCharacter.traitContainer.AddTrait(targetCharacter, lethargic, actor, goapNode);
                } else {
                    Vampire vampire = TraitManager.Instance.CreateNewInstancedTraitClass<Vampire>("Vampire");
                    targetCharacter.traitContainer.AddTrait(targetCharacter, vampire, actor);
                    Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "GoapAction", goapName, "contracted", goapNode, LOG_TAG.Life_Changes);
                    // if(goapNode != null) {
                    //     log.SetLogType(LOG_TYPE.Action);
                    // }
                    log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                    log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    log.AddLogToDatabase();
                    PlayerManager.Instance.player.ShowNotificationFrom(actor, log);
                }
            }
        }

        Infected infected = goapNode.poiTarget.traitContainer.GetTraitOrStatus<Infected>("Infected");
        infected?.InfectTarget(actor);
    }
    #endregion

    //#region Intel Reactions
    //private List<string> DrinkBloodSuccessIntelReaction(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
    //    List<string> reactions = new List<string>();
    //    Character targetCharacter = poiTarget as Character;
    //    bool isRecipientVampire = recipient.traitContainer.GetNormalTrait<Trait>("Vampire") != null;

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
    //                if (isRecipientVampire) {
    //                    //- Recipient is a Vampire
    //                    reactions.Add(string.Format("{0} must be the one that turned me into this...", actor.name));
    //                } else {
    //                    //- Recipient is NOT a Vampire
    //                    RELATIONSHIP_EFFECT relationshipWithActorBeforeDegradation = recipient.relationshipContainer.GetRelationshipEffectWith(actor.currentAlterEgo);
    //                    bool hasRelationshipDegraded = false;
    //                    if (!hasCrimeBeenReported) {
    //                        hasRelationshipDegraded = recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
    //                    }
    //                    if (relationshipWithActorBeforeDegradation == RELATIONSHIP_EFFECT.POSITIVE) {
    //                        if (hasRelationshipDegraded) {
    //                            reactions.Add("Vampires are cursed beings that must be destroyed!");
    //                        } else {
    //                            reactions.Add(string.Format("I don't believe you! {0} is not a vampire.", actor.name));
    //                        }
    //                    } else if (relationshipWithActorBeforeDegradation == RELATIONSHIP_EFFECT.NEGATIVE) {
    //                        reactions.Add(string.Format("I knew something is off with that {0}!", actor.name));
    //                    } else {
    //                        reactions.Add("Vampires are cursed beings that must be destroyed!");
    //                    }
    //                }
    //            }
    //            //- Recipient Has Positive Relationship with Target
    //            else if (recipient.relationshipContainer.GetRelationshipEffectWith(targetCharacter.currentAlterEgo) == RELATIONSHIP_EFFECT.POSITIVE) {
    //                if (isRecipientVampire) {
    //                    //- Recipient is a Vampire
    //                    RELATIONSHIP_EFFECT relationshipWithActorBeforeDegradation = recipient.relationshipContainer.GetRelationshipEffectWith(actor.currentAlterEgo);
    //                    bool hasRelationshipDegraded = RelationshipManager.Instance.RelationshipDegradation(actor, recipient, this);
    //                    if (relationshipWithActorBeforeDegradation == RELATIONSHIP_EFFECT.POSITIVE) {
    //                        if (hasRelationshipDegraded) {
    //                            reactions.Add(string.Format("I am also a vampire so I understand {0}'s unique needs but {1} shouldn't have hurt {2}!", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, false), targetCharacter.name));
    //                            if(status != SHARE_INTEL_STATUS.WITNESSED) {
    //                                recipient.CreateKnockoutJob(actor);
    //                            }
    //                        } else {
    //                            reactions.Add(string.Format("I am also a vampire so I understand {0}'s unique needs.", actor.name));
    //                        }
    //                    } else if (relationshipWithActorBeforeDegradation == RELATIONSHIP_EFFECT.NEGATIVE) {
    //                        reactions.Add(string.Format("I am also a vampire so I understand {0}'s unique needs but {1} shouldn't have hurt {2}!", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, false), targetCharacter.name));
    //                        if (status != SHARE_INTEL_STATUS.WITNESSED) {
    //                            recipient.CreateKnockoutJob(actor);
    //                        }
    //                    } else {
    //                        reactions.Add(string.Format("I am also a vampire so I understand {0}'s unique needs but {1} shouldn't have hurt {2}!", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, false), targetCharacter.name));
    //                        if (status != SHARE_INTEL_STATUS.WITNESSED) {
    //                            recipient.CreateKnockoutJob(actor);
    //                        }
    //                    }
    //                } else {
    //                    //- Recipient is NOT a Vampire
    //                    RELATIONSHIP_EFFECT relationshipWithActorBeforeDegradation = recipient.relationshipContainer.GetRelationshipEffectWith(actor.currentAlterEgo);
    //                    bool hasRelationshipDegraded = false;
    //                    if (!hasCrimeBeenReported) {
    //                        hasRelationshipDegraded = recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
    //                    }
    //                    if (relationshipWithActorBeforeDegradation == RELATIONSHIP_EFFECT.POSITIVE) {
    //                        if (hasRelationshipDegraded) {
    //                            reactions.Add("Vampires are cursed beings that must be destroyed!");
    //                        } else {
    //                            reactions.Add(string.Format("I don't believe you! {0} is not a vampire.", actor.name));
    //                        }
    //                    } else if (relationshipWithActorBeforeDegradation == RELATIONSHIP_EFFECT.NEGATIVE) {
    //                        reactions.Add(string.Format("I knew something is off with that {0}!", actor.name));
    //                    } else {
    //                        reactions.Add("Vampires are cursed beings that must be destroyed!");
    //                    }
    //                }
    //            }
    //            //- Recipient Has Negative Relationship with Target
    //            else if (recipient.relationshipContainer.GetRelationshipEffectWith(targetCharacter.currentAlterEgo) == RELATIONSHIP_EFFECT.NEGATIVE) {
    //                if (isRecipientVampire) {
    //                    //- Recipient is a Vampire
    //                    if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                        if (recipient.marker.inVisionCharacters.Contains(actor)) {
    //                            recipient.combatComponent.AddAvoidInRange(actor, reason: "saw something shameful");
    //                        }
    //                    }
    //                    RELATIONSHIP_EFFECT relationshipWithActor = recipient.relationshipContainer.GetRelationshipEffectWith(actor.currentAlterEgo);
    //                    if (relationshipWithActor == RELATIONSHIP_EFFECT.POSITIVE) {
    //                        reactions.Add(string.Format("I am also a vampire so I understand {0}'s unique needs.", actor.name));
    //                    } else if (relationshipWithActor == RELATIONSHIP_EFFECT.NEGATIVE) {
    //                        reactions.Add(string.Format("I may dislike {0} but I can't report a fellow vampire.", actor.name));
    //                    } else {
    //                        reactions.Add(string.Format("I am also a vampire so I understand {0}'s unique needs.", actor.name));
    //                    }
    //                } else {
    //                    //- Recipient is NOT a Vampire
    //                    RELATIONSHIP_EFFECT relationshipWithActorBeforeDegradation = recipient.relationshipContainer.GetRelationshipEffectWith(actor.currentAlterEgo);
    //                    bool hasRelationshipDegraded = false;
    //                    if (!hasCrimeBeenReported) {
    //                        hasRelationshipDegraded = recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
    //                    }
    //                    if (relationshipWithActorBeforeDegradation == RELATIONSHIP_EFFECT.POSITIVE) {
    //                        if (hasRelationshipDegraded) {
    //                            reactions.Add("Vampires are cursed beings that must be destroyed!");
    //                        } else {
    //                            reactions.Add(string.Format("I don't believe you! {0} is not a vampire.", actor.name));
    //                        }
    //                    } else if (relationshipWithActorBeforeDegradation == RELATIONSHIP_EFFECT.NEGATIVE) {
    //                        reactions.Add("Vampires are cursed beings that must be destroyed!");
    //                    } else {
    //                        reactions.Add("Vampires are cursed beings that must be destroyed!");
    //                    }
    //                }
    //            }
    //            //- Recipient Has No Relationship with Target
    //            else {
    //                if (isRecipientVampire) {
    //                    //- Recipient is a Vampire
    //                    RELATIONSHIP_EFFECT relationshipWithActor = recipient.relationshipContainer.GetRelationshipEffectWith(actor.currentAlterEgo);
    //                    if (relationshipWithActor == RELATIONSHIP_EFFECT.POSITIVE) {
    //                        reactions.Add(string.Format("I am also a vampire so I understand {0}'s unique needs.", actor.name));
    //                    } else if (relationshipWithActor == RELATIONSHIP_EFFECT.NEGATIVE) {
    //                        reactions.Add(string.Format("I may dislike {0} but I can't report a fellow vampire.", actor.name));
    //                    } else {
    //                        reactions.Add(string.Format("I am also a vampire so I understand {0}'s unique needs.", actor.name));
    //                    }
    //                } else {
    //                    //- Recipient is NOT a Vampire
    //                    RELATIONSHIP_EFFECT relationshipWithActorBeforeDegradation = recipient.relationshipContainer.GetRelationshipEffectWith(actor.currentAlterEgo);
    //                    bool hasRelationshipDegraded = false;
    //                    if (!hasCrimeBeenReported) {
    //                        hasRelationshipDegraded = recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
    //                    }
    //                    if (relationshipWithActorBeforeDegradation == RELATIONSHIP_EFFECT.POSITIVE) {
    //                        if (hasRelationshipDegraded) {
    //                            reactions.Add("Vampires are cursed beings that must be destroyed!");
    //                        } else {
    //                            reactions.Add(string.Format("I don't believe you! {0} is not a vampire.", actor.name));
    //                        }
    //                    } else if (relationshipWithActorBeforeDegradation == RELATIONSHIP_EFFECT.NEGATIVE) {
    //                        reactions.Add("Vampires are cursed beings that must be destroyed!");
    //                    } else {
    //                        reactions.Add("Vampires are cursed beings that must be destroyed!");
    //                    }
    //                }
    //            }
    //        }
    //    }
    //    return reactions;
    //}
    //#endregion
}