﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class DrinkBlood : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.CONSUME; } }

    public DrinkBlood() : base(INTERACTION_TYPE.DRINK_BLOOD) {
        actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
        actionIconString = GoapActionStateDB.Drink_Blood_Icon;
        doesNotStopTargetCharacter = true;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Unconscious", target = GOAP_EFFECT_TARGET.TARGET }, HasUnconsciousOrRestingTarget);
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Lethargic", target = GOAP_EFFECT_TARGET.TARGET });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Drink Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, object[] otherData) {
        return 1;
    }
    public override void OnStopWhilePerforming(Character actor, IPointOfInterest target, object[] otherData) {
        base.OnStopWhilePerforming(actor, target, otherData);
        actor.AdjustDoNotGetHungry(-1);
    }
    public override GoapActionInvalidity IsInvalid(Character actor, IPointOfInterest target, object[] otherData) {
        GoapActionInvalidity actionInvalidity = base.IsInvalid(actor, target, otherData);
        if (actionInvalidity.isInvalid == false) {
            Character targetCharacter = target as Character;
            if (targetCharacter.canMove || targetCharacter.canWitness || targetCharacter.IsAvailable() == false) {
                actionInvalidity.isInvalid = true;
                actionInvalidity.stateName = "Drink Fail";
            }
        }
        return actionInvalidity;
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            if (actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
                return false;
            }
            return actor != poiTarget;
        }
        return false;
    }
    #endregion

    #region Preconditions
    private bool HasUnconsciousOrRestingTarget(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        Character target = poiTarget as Character;
        return target.traitContainer.GetNormalTrait("Unconscious", "Resting") != null;
    }
    #endregion

    #region Effects
    private void PreDrinkSuccess(ActualGoapNode goapNode) {
        goapNode.actor.AdjustDoNotGetHungry(1);
        //TODO: currentState.SetIntelReaction(DrinkBloodSuccessIntelReaction);
    }
    private void PerTickDrinkSuccess(ActualGoapNode goapNode) {
        goapNode.actor.AdjustFullness(1100);
    }
    private void AfterDrinkSuccess(ActualGoapNode goapNode) {
        //poiTarget.SetPOIState(POI_STATE.ACTIVE);
        goapNode.actor.AdjustDoNotGetHungry(-1);
        int chance = UnityEngine.Random.Range(0, 100);
        if(chance < 85) {
            Lethargic lethargic = new Lethargic();
            goapNode.poiTarget.traitContainer.AddTrait(goapNode.poiTarget, lethargic, goapNode.actor);
        } else {
            Vampiric vampiric = new Vampiric();
            goapNode.poiTarget.traitContainer.AddTrait(goapNode.poiTarget, vampiric, goapNode.actor);
            Log log = new Log(GameManager.Instance.Today(), "GoapAction", GetType().ToString(), "contracted", goapNode);
            log.AddToFillers(goapNode.actor, goapNode.actor.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            log.AddToFillers(goapNode.poiTarget, goapNode.poiTarget.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddLogToInvolvedObjects();
            PlayerManager.Instance.player.ShowNotification(log);
        }
    }
    #endregion

    //#region Intel Reactions
    //private List<string> DrinkBloodSuccessIntelReaction(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
    //    List<string> reactions = new List<string>();
    //    Character targetCharacter = poiTarget as Character;
    //    bool isRecipientVampire = recipient.traitContainer.GetNormalTrait("Vampiric") != null;

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
    //                            recipient.marker.AddAvoidInRange(actor, reason: "saw something shameful");
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

public class DrinkBloodData : GoapActionData {
    public DrinkBloodData() : base(INTERACTION_TYPE.DRINK_BLOOD) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (poiTarget.gridTileLocation == null || (actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure)) {
            return false;
        }
        if (actor != poiTarget) {
            return true;
        }
        return false;
    }
}
