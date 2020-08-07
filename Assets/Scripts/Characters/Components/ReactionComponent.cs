﻿using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Traits;
using Inner_Maps;
using Interrupts;
using Inner_Maps.Location_Structures;
using UnityEngine.Assertions;
using Tutorial;
using UtilityScripts;
using Random = System.Random;

public class ReactionComponent {
    public Character owner { get; private set; }

    private List<Character> _assumptionSuspects;
    public List<Character> charactersThatSawThisDead { get; private set; }
    public bool isHidden { get; private set; }
    public Character disguisedCharacter { get; private set; }

    public ReactionComponent(Character owner) {
        this.owner = owner;
        _assumptionSuspects = new List<Character>();
        charactersThatSawThisDead = new List<Character>();
    }

    #region Processes
    public void ReactTo(IPointOfInterest target, ref string debugLog) {
        Character actor = owner;
        //if (actor.reactionComponent.disguisedCharacter != null) {
        //    actor = actor.reactionComponent.disguisedCharacter;
        //}
        if (target.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
            Character targetCharacter = target as Character; 
            Assert.IsNotNull(targetCharacter);
            ReactTo(actor, targetCharacter, ref debugLog);

            //If reacting to a disguised character, checking the carried poi must be from the disguised one, but the reaction must to the one he is disguised as.
            if (targetCharacter.carryComponent.carriedPOI is TileObject tileObject) {
                ReactToCarriedObject(actor, tileObject, targetCharacter, ref debugLog);
            }
        } else if (target.poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
            ReactTo(actor, target as TileObject, ref debugLog);
        } 
        // else if (targetTileObject.poiType == POINT_OF_INTEREST_TYPE.ITEM) {
        //     ReactTo(targetTileObject as SpecialToken, ref debugLog);
        // }
        if (!actor.isNormalCharacter /*|| owner.race == RACE.SKELETON*/) {
            //Minions or Summons cannot react to its own traits
            return;
        }
        if (actor.combatComponent.isInActualCombat) {
            return;
        }
        debugLog += "\n-Character will loop through all his/her traits to react to Target";
        List<Trait> traitOverrideFunctions = actor.traitContainer.GetTraitOverrideFunctions(TraitManager.See_Poi_Trait);
        if (traitOverrideFunctions != null) {
            for (int i = 0; i < traitOverrideFunctions.Count; i++) {
                Trait trait = traitOverrideFunctions[i];
                debugLog += $"\n - {trait.name}";
                if (trait.OnSeePOI(target, actor)) {
                    debugLog += ": triggered";
                } else {
                    debugLog += ": not triggered";
                }
            }
        }
    }
    public void ReactToDisguised(Character targetCharacter, Character copiedCharacter, ref string debugLog) {
        if(owner == copiedCharacter) {
            debugLog += $"{owner.name} is reacting to a copy of himself/herself";
            debugLog += $"Surprise interrupt and Fight response";
            owner.combatComponent.Fight(targetCharacter, CombatManager.Hostility);
            owner.interruptComponent.TriggerInterrupt(INTERRUPT.Surprised, targetCharacter, reason: Surprised.Copycat_Reason);
        } else {
            ReactTo(targetCharacter, ref debugLog);
            return;
        }

        //If reacting to a disguised character, checking the carried poi must be from the disguised one, but the reaction must to the one he is disguised as.
        if (targetCharacter.carryComponent.carriedPOI is TileObject tileObject) {
            ReactToCarriedObject(owner, tileObject, copiedCharacter, ref debugLog);
        }
        if (!owner.isNormalCharacter /*|| owner.race == RACE.SKELETON*/) {
            //Minions or Summons cannot react to its own traits
            return;
        }
        if (owner.combatComponent.isInActualCombat) {
            return;
        }
        debugLog += "\n-Character will loop through all his/her traits to react to Target";
        List<Trait> traitOverrideFunctions = owner.traitContainer.GetTraitOverrideFunctions(TraitManager.See_Poi_Trait);
        if (traitOverrideFunctions != null) {
            for (int i = 0; i < traitOverrideFunctions.Count; i++) {
                Trait trait = traitOverrideFunctions[i];
                debugLog += $"\n - {trait.name}";
                if (trait.OnSeePOI(copiedCharacter, owner)) {
                    debugLog += ": triggered";
                } else {
                    debugLog += ": not triggered";
                }
            }
        }
    }
    public string ReactTo(IReactable reactable, REACTION_STATUS status, bool addLog = true) {
        if (!owner.isNormalCharacter /*|| owner.race == RACE.SKELETON*/) {
            //Minions or Summons cannot react to actions
            return string.Empty;
        }
        if (reactable.awareCharacters.Contains(owner)) {
            return "aware";
        }
        reactable.AddAwareCharacter(owner);
        if (status == REACTION_STATUS.WITNESSED) {
            ReactToWitnessedReactable(reactable, addLog);
        } else {
            return ReactToInformedReactable(reactable, addLog);
        }
        return string.Empty;
    }
    private void ReactToWitnessedReactable(IReactable reactable, bool addLog) {
        if (owner.combatComponent.isInActualCombat) {
            return;
        }

        Character actor = reactable.actor;
        IPointOfInterest target = reactable.target;
        //Whenever a disguised character is being set as actor/target, set the original as the actor/target, as if they are the ones who did it
        if (actor.reactionComponent.disguisedCharacter != null) {
            actor = actor.reactionComponent.disguisedCharacter;
        }
        if (target is Character targetCharacter && targetCharacter.reactionComponent.disguisedCharacter != null) {
            target = targetCharacter.reactionComponent.disguisedCharacter;
        }

        if (owner.faction != null && actor.faction != null && owner.faction != actor.faction && owner.faction.IsHostileWith(actor.faction)) {
            //Must not react if the faction of the actor of witnessed action is hostile with the faction of the witness
            return;
        }
        //if (witnessedEvent.currentStateName == null) {
        //    throw new System.Exception(GameManager.Instance.TodayLogString() + this.name + " witnessed event " + witnessedEvent.action.goapName + " by " + witnessedEvent.actor.name + " but it does not have a current state!");
        //}
        //if (string.IsNullOrEmpty(reactable.currentStateName)) {
        //    return;
        //}
        if (reactable.informationLog == null) {
            throw new Exception(
                $"{GameManager.Instance.TodayLogString()}{owner.name} witnessed event {reactable.name} by {reactable.actor.name} does not have a log!");
        }
        if(reactable.target is TileObject item && reactable is ActualGoapNode node) {
            if (node.action.goapType == INTERACTION_TYPE.STEAL) {
                if (item.isBeingCarriedBy != null) {
                    target = item.isBeingCarriedBy;
                }
            }
        }
        if(actor != owner && target != owner) {
            if (addLog) {
                //Only log witness event if event is not an action. If it is an action, the CharacterManager.Instance.CanAddCharacterLogOrShowNotif must return true
                if (reactable is ActualGoapNode action && (!action.action.shouldAddLogs || !CharacterManager.Instance.CanAddCharacterLogOrShowNotif(action.goapType))) {
                    //Should not add witness log if the action log itself is not added to the actor
                } else {
                    Log witnessLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "witness_event", reactable as ActualGoapNode);
                    witnessLog.SetLogType(LOG_TYPE.Witness);
                    witnessLog.AddToFillers(owner, owner.name, LOG_IDENTIFIER.PARTY_1); //Used Party 1 identifier so there will be no conflict if reactable.informationLog is a Rumor
                    witnessLog.AddToFillers(null, UtilityScripts.Utilities.LogDontReplace(reactable.informationLog), LOG_IDENTIFIER.APPEND);
                    witnessLog.AddToFillers(reactable.informationLog.fillers);
                    owner.logComponent.AddHistory(witnessLog);
                }
            }
            string emotionsToActor = reactable.ReactionToActor(actor, target, owner, REACTION_STATUS.WITNESSED);
            if(emotionsToActor != string.Empty) {
                if (!CharacterManager.Instance.EmotionsChecker(emotionsToActor)) {
                    string error = "Action Error in Witness Reaction To Actor (Duplicate/Incompatible Emotions Triggered)";
                    error += $"\n-Witness: {owner}";
                    error += $"\n-Action: {reactable.name}";
                    error += $"\n-Actor: {actor.name}";
                    error += $"\n-Target: {reactable.target.nameWithID}";
                    owner.logComponent.PrintLogErrorIfActive(error);
                } else {
                    //add log of emotions felt
                    Log log = new Log(GameManager.Instance.Today(), "Character", "Generic", "emotions_reaction_witness");
                    log.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                    log.AddToFillers(null, UtilityScripts.Utilities.GetFirstFewEmotionsAndComafy(emotionsToActor, 2), LOG_IDENTIFIER.STRING_1);
                    log.AddLogToInvolvedObjects();
                }
            }
            string emotionsToTarget = reactable.ReactionToTarget(actor, target, owner, REACTION_STATUS.WITNESSED);
            if (emotionsToTarget != string.Empty) {
                if (!CharacterManager.Instance.EmotionsChecker(emotionsToTarget)) {
                    string error = "Action Error in Witness Reaction To Target (Duplicate/Incompatible Emotions Triggered)";
                    error += $"\n-Witness: {owner}";
                    error += $"\n-Action: {reactable.name}";
                    error += $"\n-Actor: {actor.name}";
                    error += $"\n-Target: {reactable.target.nameWithID}";
                    owner.logComponent.PrintLogErrorIfActive(error);
                } else {
                    //add log of emotions felt
                    Log log = new Log(GameManager.Instance.Today(), "Character", "Generic", "emotions_reaction_witness");
                    log.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    log.AddToFillers(reactable.target, reactable.target.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                    log.AddToFillers(null, UtilityScripts.Utilities.Comafy(emotionsToTarget), LOG_IDENTIFIER.STRING_1);
                    log.AddLogToInvolvedObjects();
                }
            }
            string response =
                $"Witness action reaction of {owner.name} to {reactable.name} of {actor.name} with target {reactable.target.name}: {emotionsToActor}{emotionsToTarget}";
            owner.logComponent.PrintLogIfActive(response);
        } else if (reactable.target == owner) {
            if (!reactable.isStealth || reactable.target.traitContainer.HasTrait("Vigilant")) {
                string emotionsOfTarget = reactable.ReactionOfTarget(actor, reactable.target, REACTION_STATUS.WITNESSED);
                if (emotionsOfTarget != string.Empty) {
                    if (!CharacterManager.Instance.EmotionsChecker(emotionsOfTarget)) {
                        string error = "Action Error in Witness Reaction Of Target (Duplicate/Incompatible Emotions Triggered)";
                        error += $"\n-Witness: {owner}";
                        error += $"\n-Action: {reactable.name}";
                        error += $"\n-Actor: {actor.name}";
                        error += $"\n-Target: {reactable.target.nameWithID}";
                        owner.logComponent.PrintLogErrorIfActive(error);
                    } else {
                        //add log of emotions felt
                        Log log = new Log(GameManager.Instance.Today(), "Character", "Generic", "emotions_reaction_witness");
                        log.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                        log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                        log.AddToFillers(null, UtilityScripts.Utilities.Comafy(emotionsOfTarget), LOG_IDENTIFIER.STRING_1);
                        log.AddLogToInvolvedObjects();
                    }
                }
                string response =
                    $"Witness action reaction of {owner.name} to {reactable.name} of {actor.name} with target {reactable.target.name}: {emotionsOfTarget}";
                owner.logComponent.PrintLogIfActive(response);
            }
        }

        //CRIME_TYPE crimeType = CrimeManager.Instance.GetCrimeTypeConsideringAction(node);
        //if (crimeType != CRIME_TYPE.NONE) {
        //    CrimeManager.Instance.ReactToCrime(owner, node, node.associatedJobType, crimeType);
        //}
    }
    private string ReactToInformedReactable(IReactable reactable, bool addLog) {
        if (reactable.informationLog == null) {
            throw new Exception(
                $"{GameManager.Instance.TodayLogString()}{owner.name} informed event {reactable.name} by {reactable.actor.name} does not have a log!");
        }

        Character actor = reactable.actor;
        IPointOfInterest target = reactable.target;
        //Whenever a disguised character is being set as actor/target, set the original as the actor/target, as if they are the ones who did it
        if (actor.reactionComponent.disguisedCharacter != null) {
            actor = actor.reactionComponent.disguisedCharacter;
        }
        if (target is Character targetCharacter && targetCharacter.reactionComponent.disguisedCharacter != null) {
            target = targetCharacter.reactionComponent.disguisedCharacter;
        }

        if (addLog) {
            Log informedLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "informed_event", reactable as ActualGoapNode);
            informedLog.SetLogType(LOG_TYPE.Informed);
            informedLog.AddToFillers(reactable.informationLog.fillers);
            informedLog.AddToFillers(owner, owner.name, LOG_IDENTIFIER.PARTY_1); //Used Party 1 identifier so there will be no conflict if reactable.informationLog is a Rumor
            informedLog.AddToFillers(null, UtilityScripts.Utilities.LogDontReplace(reactable.informationLog), LOG_IDENTIFIER.APPEND);
            owner.logComponent.AddHistory(informedLog);
        }

        string response = string.Empty;
        if (actor != owner && target != owner) {
            string emotionsToActor = reactable.ReactionToActor(actor, target, owner, REACTION_STATUS.INFORMED);
            if (emotionsToActor != string.Empty) {
                if (!CharacterManager.Instance.EmotionsChecker(emotionsToActor)) {
                    string error = "Action Error in Witness Reaction To Actor (Duplicate/Incompatible Emotions Triggered)";
                    error += $"\n-Witness: {owner}";
                    error += $"\n-Action: {reactable.name}";
                    error += $"\n-Actor: {actor.name}";
                    error += $"\n-Target: {target.nameWithID}";
                    owner.logComponent.PrintLogErrorIfActive(error);
                } else {
                    //add log of emotions felt
                    Log log = new Log(GameManager.Instance.Today(), "Character", "Generic", "emotions_reaction");
                    log.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                    log.AddToFillers(null, UtilityScripts.Utilities.GetFirstFewEmotionsAndComafy(emotionsToActor, 2), LOG_IDENTIFIER.STRING_1);
                    log.AddLogToInvolvedObjects();
                }
            }
            string emotionsToTarget = reactable.ReactionToTarget(actor, target, owner, REACTION_STATUS.INFORMED);
            if (emotionsToTarget != string.Empty) {
                if (!CharacterManager.Instance.EmotionsChecker(emotionsToTarget)) {
                    string error = "Action Error in Witness Reaction To Target (Duplicate/Incompatible Emotions Triggered)";
                    error += $"\n-Witness: {owner}";
                    error += $"\n-Action: {reactable.name}";
                    error += $"\n-Actor: {actor.name}";
                    error += $"\n-Target: {target.nameWithID}";
                    owner.logComponent.PrintLogErrorIfActive(error);
                } else {
                    //add log of emotions felt
                    Log log = new Log(GameManager.Instance.Today(), "Character", "Generic", "emotions_reaction");
                    log.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    log.AddToFillers(target, target.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                    log.AddToFillers(null, UtilityScripts.Utilities.GetFirstFewEmotionsAndComafy(emotionsToTarget, 2), LOG_IDENTIFIER.STRING_1);
                    log.AddLogToInvolvedObjects();
                }
            }
            response += $"{emotionsToActor}/{emotionsToTarget}";
        } else if(reactable.target == owner && reactable.target is Character) {
            string emotionsOfTarget = reactable.ReactionOfTarget(actor, reactable.target, REACTION_STATUS.INFORMED);
            if (emotionsOfTarget != string.Empty) {
                if (!CharacterManager.Instance.EmotionsChecker(emotionsOfTarget)) {
                    string error = "Action Error in Witness Reaction Of Target (Duplicate/Incompatible Emotions Triggered)";
                    error += $"\n-Witness: {owner}";
                    error += $"\n-Action: {reactable.name}";
                    error += $"\n-Actor: {actor.name}";
                    error += $"\n-Target: {reactable.target.nameWithID}";
                    owner.logComponent.PrintLogErrorIfActive(error);
                } else {
                    //add log of emotions felt
                    Log log = new Log(GameManager.Instance.Today(), "Character", "Generic", "emotions_reaction");
                    log.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                    log.AddToFillers(null, UtilityScripts.Utilities.GetFirstFewEmotionsAndComafy(emotionsOfTarget, 2), LOG_IDENTIFIER.STRING_1);
                    log.AddLogToInvolvedObjects();
                }
            }
            response = emotionsOfTarget;
        }
        // else if (node.actor == owner) {
        //     response = "I know what I did.";
        // }
        return response;
        //CRIME_TYPE crimeType = CrimeManager.Instance.GetCrimeTypeConsideringAction(node);
        //if (crimeType != CRIME_TYPE.NONE) {
        //    CrimeManager.Instance.ReactToCrime(owner, node, node.associatedJobType, crimeType);
        //}
    }
    //public string ReactTo(Interrupt interrupt, Character actor, IPointOfInterest target, Log log, REACTION_STATUS status) {
    //    if (owner.combatComponent.isInActualCombat) {
    //        return string.Empty;
    //    }
    //    if (!owner.isNormalCharacter /*|| owner.race == RACE.SKELETON*/) {
    //        //Minions or Summons cannot react to interrupts
    //        return string.Empty;
    //    }
    //    if (owner.faction != actor.faction && owner.faction.IsHostileWith(actor.faction)) {
    //        //Must not react if the faction of the actor of witnessed action is hostile with the faction of the witness
    //        return string.Empty;
    //    }
    //    if(status == REACTION_STATUS.WITNESSED) {
    //        ReactToWitnessedInterrupt(interrupt, actor, target, log);
    //    } else if (status == REACTION_STATUS.INFORMED) {
    //        return ReactToInformedInterrupt(interrupt, actor, target, log);
    //    }
    //    return string.Empty;
    //}
    //private void ReactToWitnessedInterrupt(Interrupt interrupt, Character actor, IPointOfInterest target, Log log) {
    //    if (actor != owner && target != owner) {
    //        if (actor.interruptComponent.currentInterrupt == interrupt && log != null) {
    //            Log witnessLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "witness_event");
    //            witnessLog.SetLogType(LOG_TYPE.Witness);
    //            witnessLog.AddToFillers(owner, owner.name, LOG_IDENTIFIER.PARTY_1); //Used Party 1 identifier so there will be no conflict if reactable.informationLog is a Rumor
    //            witnessLog.AddToFillers(null, UtilityScripts.Utilities.LogDontReplace(log), LOG_IDENTIFIER.APPEND);
    //            witnessLog.AddToFillers(log.fillers);
    //            owner.logComponent.AddHistory(witnessLog);
    //        }
    //        string emotionsToActor = interrupt.ReactionToActor(owner, actor, target, interrupt, REACTION_STATUS.WITNESSED);
    //        if (emotionsToActor != string.Empty) {
    //            if (!CharacterManager.Instance.EmotionsChecker(emotionsToActor)) {
    //                string error = "Interrupt Error in Witness Reaction To Actor (Duplicate/Incompatible Emotions Triggered)";
    //                error += $"\n-Witness: {owner}";
    //                error += $"\n-Interrupt: {interrupt.name}";
    //                error += $"\n-Actor: {actor.name}";
    //                error += $"\n-Target: {target.nameWithID}";
    //                owner.logComponent.PrintLogErrorIfActive(error);
    //            } else {
    //                Log emotionsLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "emotions_reaction_witness");
    //                emotionsLog.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
    //                emotionsLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    //                emotionsLog.AddToFillers(null, UtilityScripts.Utilities.GetFirstFewEmotionsAndComafy(emotionsToActor, 2), LOG_IDENTIFIER.STRING_1);
    //                emotionsLog.AddLogToInvolvedObjects();
    //            }
    //        }
    //        string emotionsToTarget = interrupt.ReactionToTarget(owner, actor, target, interrupt, REACTION_STATUS.WITNESSED);
    //        if (emotionsToTarget != string.Empty) {
    //            if (!CharacterManager.Instance.EmotionsChecker(emotionsToTarget)) {
    //                string error = "Interrupt Error in Witness Reaction To Actor (Duplicate/Incompatible Emotions Triggered)";
    //                error += $"\n-Witness: {owner}";
    //                error += $"\n-Interrupt: {interrupt.name}";
    //                error += $"\n-Actor: {actor.name}";
    //                error += $"\n-Target: {target.nameWithID}";
    //                owner.logComponent.PrintLogErrorIfActive(error);
    //            } else {
    //                Log emotionsLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "emotions_reaction_witness");
    //                emotionsLog.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
    //                emotionsLog.AddToFillers(target, target.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    //                emotionsLog.AddToFillers(null, UtilityScripts.Utilities.GetFirstFewEmotionsAndComafy(emotionsToTarget, 2), LOG_IDENTIFIER.STRING_1);
    //                emotionsLog.AddLogToInvolvedObjects();
    //            }
    //        }
    //        string response =
    //            $"Witness interrupt reaction of {owner.name} to {interrupt.name} of {actor.name} with target {target.name}: {emotionsToActor}{emotionsToTarget}";
    //        owner.logComponent.PrintLogIfActive(response);
    //    } else if (target == owner) {
    //        string emotionsOfTarget = interrupt.ReactionOfTarget(actor, target, interrupt, REACTION_STATUS.WITNESSED);
    //        if (emotionsOfTarget != string.Empty) {
    //            if (!CharacterManager.Instance.EmotionsChecker(emotionsOfTarget)) {
    //                string error = "Interrupt Error in Witness Reaction To Actor (Duplicate/Incompatible Emotions Triggered)";
    //                error += $"\n-Witness: {owner}";
    //                error += $"\n-Interrupt: {interrupt.name}";
    //                error += $"\n-Actor: {actor.name}";
    //                error += $"\n-Target: {target.nameWithID}";
    //                owner.logComponent.PrintLogErrorIfActive(error);
    //            } else {
    //                Log emotionsLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "emotions_reaction_witness");
    //                emotionsLog.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
    //                emotionsLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    //                emotionsLog.AddToFillers(null, UtilityScripts.Utilities.GetFirstFewEmotionsAndComafy(emotionsOfTarget, 2), LOG_IDENTIFIER.STRING_1);
    //                emotionsLog.AddLogToInvolvedObjects();
    //            }
    //        }
    //        string response =
    //            $"Witness interrupt reaction of {owner.name} to {interrupt.name} of {actor.name} with target {target.name}: {emotionsOfTarget}";
    //        owner.logComponent.PrintLogIfActive(response);
    //    }
    //}
    //public string ReactToInformedInterrupt(Interrupt interrupt, Character actor, IPointOfInterest target, Log log) {
    //    if (log == null) {
    //        throw new Exception(
    //            $"{GameManager.Instance.TodayLogString()}{owner.name} informed interrupt {interrupt.name} by {actor.name} with target {target.name} but it does not have a log!");
    //    }
    //    Log informedLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "informed_event");
    //    informedLog.SetLogType(LOG_TYPE.Informed);
    //    informedLog.AddToFillers(log.fillers);
    //    informedLog.AddToFillers(owner, owner.name, LOG_IDENTIFIER.PARTY_1); //Used Party 1 identifier so there will be no conflict if reactable.informationLog is a Rumor
    //    informedLog.AddToFillers(null, UtilityScripts.Utilities.LogDontReplace(log), LOG_IDENTIFIER.APPEND);
    //    owner.logComponent.AddHistory(informedLog);

    //    string response = string.Empty;
    //    if (actor != owner && target != owner) {
    //        string emotionsToActor = interrupt.ReactionToActor(owner, actor, target, interrupt, REACTION_STATUS.INFORMED);
    //        if (emotionsToActor != string.Empty) {
    //            if (!CharacterManager.Instance.EmotionsChecker(emotionsToActor)) {
    //                string error = "Interrupt Error in Witness Reaction To Actor (Duplicate/Incompatible Emotions Triggered)";
    //                error += $"\n-Witness: {owner}";
    //                error += $"\n-Interrupt: {interrupt.name}";
    //                error += $"\n-Actor: {actor.name}";
    //                error += $"\n-Target: {target.nameWithID}";
    //                owner.logComponent.PrintLogErrorIfActive(error);
    //            } else {
    //                Log emotionsLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "emotions_reaction");
    //                emotionsLog.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
    //                emotionsLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    //                emotionsLog.AddToFillers(null, UtilityScripts.Utilities.GetFirstFewEmotionsAndComafy(emotionsToActor, 2), LOG_IDENTIFIER.STRING_1);
    //                emotionsLog.AddLogToInvolvedObjects();
    //            }
    //        }
    //        string emotionsToTarget = interrupt.ReactionToTarget(owner, actor, target, interrupt, REACTION_STATUS.INFORMED);
    //        if (emotionsToTarget != string.Empty) {
    //            if (!CharacterManager.Instance.EmotionsChecker(emotionsToTarget)) {
    //                string error = "Interrupt Error in Witness Reaction To Actor (Duplicate/Incompatible Emotions Triggered)";
    //                error += $"\n-Witness: {owner}";
    //                error += $"\n-Interrupt: {interrupt.name}";
    //                error += $"\n-Actor: {actor.name}";
    //                error += $"\n-Target: {target.nameWithID}";
    //                owner.logComponent.PrintLogErrorIfActive(error);
    //            } else {
    //                Log emotionsLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "emotions_reaction");
    //                emotionsLog.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
    //                emotionsLog.AddToFillers(target, target.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    //                emotionsLog.AddToFillers(null, UtilityScripts.Utilities.GetFirstFewEmotionsAndComafy(emotionsToTarget, 2), LOG_IDENTIFIER.STRING_1);
    //                emotionsLog.AddLogToInvolvedObjects();
    //            }
    //        }
    //        response += $"{emotionsToActor}/{emotionsToTarget}";
    //        owner.logComponent.PrintLogIfActive(response);
    //    } else if (target == owner) {
    //        string emotionsOfTarget = interrupt.ReactionOfTarget(actor, target, interrupt, REACTION_STATUS.INFORMED);
    //        if (emotionsOfTarget != string.Empty) {
    //            if (!CharacterManager.Instance.EmotionsChecker(emotionsOfTarget)) {
    //                string error = "Interrupt Error in Witness Reaction To Actor (Duplicate/Incompatible Emotions Triggered)";
    //                error += $"\n-Witness: {owner}";
    //                error += $"\n-Interrupt: {interrupt.name}";
    //                error += $"\n-Actor: {actor.name}";
    //                error += $"\n-Target: {target.nameWithID}";
    //                owner.logComponent.PrintLogErrorIfActive(error);
    //            } else {
    //                Log emotionsLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "emotions_reaction");
    //                emotionsLog.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
    //                emotionsLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    //                emotionsLog.AddToFillers(null, UtilityScripts.Utilities.GetFirstFewEmotionsAndComafy(emotionsOfTarget, 2), LOG_IDENTIFIER.STRING_1);
    //                emotionsLog.AddLogToInvolvedObjects();
    //            }
    //        }
    //        response = emotionsOfTarget;
    //        owner.logComponent.PrintLogIfActive(response);
    //    }
    //    // else if (node.actor == owner) {
    //    //     response = "I know what I did.";
    //    // }
    //    return response;
    //    //CRIME_TYPE crimeType = CrimeManager.Instance.GetCrimeTypeConsideringAction(node);
    //    //if (crimeType != CRIME_TYPE.NONE) {
    //    //    CrimeManager.Instance.ReactToCrime(owner, node, node.associatedJobType, crimeType);
    //    //}
    //}

    private void ReactTo(Character actor, Character targetCharacter, ref string debugLog) {
        debugLog += $"{actor.name} is reacting to {targetCharacter.name}";
        Character disguisedActor = actor;
        Character disguisedTarget = targetCharacter;
        if (actor.reactionComponent.disguisedCharacter != null) {
            disguisedActor = actor.reactionComponent.disguisedCharacter;
        }
        if (targetCharacter.reactionComponent.disguisedCharacter != null) {
            disguisedTarget = targetCharacter.reactionComponent.disguisedCharacter;
        }
        bool isHostile = false;
        if (disguisedTarget != targetCharacter && targetCharacter is SeducerSummon && !disguisedActor.isNormalCharacter) {
            isHostile = disguisedActor.IsHostileWith(targetCharacter);
        } else {
            isHostile = disguisedActor.IsHostileWith(disguisedTarget);
        }
        if (isHostile) {
            debugLog += "\n-Target is hostile";
            if(disguisedActor is Troll && disguisedTarget.isNormalCharacter && disguisedActor.homeStructure != null) {
                debugLog += "\n-Actor is a Troll and target is a Villager and actor has a home structure";
                if (targetCharacter.currentStructure != disguisedActor.homeStructure) {
                    debugLog += "\n-Will engage in combat and move it to its home";
                    if (!actor.jobQueue.HasJob(JOB_TYPE.MOVE_CHARACTER)) {
                        actor.jobComponent.TryTriggerMoveCharacter(targetCharacter, actor.homeStructure, true);
                    }
                } else {
                    debugLog += "\n-Will engage in combat and restrain it";
                    actor.jobComponent.TriggerRestrainJob(targetCharacter);
                }
            } else if (disguisedActor.traitContainer.HasTrait("Cultist") && (disguisedTarget.faction.isPlayerFaction || disguisedTarget.traitContainer.HasTrait("Cultist"))) {
                debugLog += $"\n-{disguisedActor.name} is a cultist and {disguisedTarget.name} is part of the demon faction or is also a cultist.";
                int roll = UnityEngine.Random.Range(0, 100);
                int inspireChance = 30;
                if (roll < inspireChance) {
                    debugLog += $"\n-{actor.name} triggered inspired.";
                    actor.interruptComponent.TriggerInterrupt(INTERRUPT.Inspired, targetCharacter);
                } else {
                    //pray
                    debugLog += $"\n-{actor.name} triggered pray.";
                    actor.jobComponent.TriggerPray();
                }
            } else if (!disguisedTarget.isDead && disguisedTarget.combatComponent.combatMode != COMBAT_MODE.Passive) {
                debugLog += "\n-If Target is alive and not in Passive State:";
                debugLog += "\n-Fight or Flight response";
                //Fight or Flight
                if (disguisedActor.combatComponent.combatMode == COMBAT_MODE.Aggressive) {
                    //If the source is harassing or defending, combat should not be lethal
                    //There is a special case, even if the source is defending if he/she is a demon and the target is an angel and vice versa, make the combat lethal
                    bool isLethal = (!disguisedActor.behaviourComponent.isHarassing && !disguisedActor.behaviourComponent.isDefending)
                        || ((disguisedActor.race == RACE.DEMON && disguisedTarget.race == RACE.ANGEL) || (disguisedActor.race == RACE.ANGEL && disguisedTarget.race == RACE.DEMON));
                    bool isTopPrioJobLethal = actor.jobQueue.jobsInQueue.Count <= 0 || actor.jobQueue.jobsInQueue[0].jobType.IsJobLethal();
                    if (actor.jobQueue.jobsInQueue.Count > 0) {
                        debugLog += $"\n-{actor.jobQueue.jobsInQueue[0].jobType}";
                    }
                    //NOTE: Added checking for webbed so that spiders won't attack characters that they've webbed up
                    if (disguisedActor.race == RACE.SPIDER && targetCharacter.traitContainer.HasTrait("Webbed")) {
                        debugLog += "\nActor is a spider and target is webbed, did not trigger Fight or Flight response.";
                        return;
                    }
                    
                    //If the target is already unconscious (it cannot fight back), attack it again only if this character's top priority job is considered lethal
                    if (!targetCharacter.traitContainer.HasTrait("Unconscious") || (isLethal && isTopPrioJobLethal)) {
                        //Determine whether to fight or flight.
                        CombatReaction combatReaction = actor.combatComponent.GetFightOrFlightReaction(targetCharacter, CombatManager.Hostility);
                        if (combatReaction.reaction == COMBAT_REACTION.Flight) {
                            //if flight was decided
                            //if target is restrained or resting, do nothing
                            if (targetCharacter.traitContainer.HasTrait("Restrained", "Resting") == false) {
                                actor.combatComponent.FightOrFlight(targetCharacter, combatReaction, isLethal: isLethal);    
                            }
                        } else {
                            actor.combatComponent.FightOrFlight(targetCharacter, combatReaction, isLethal: isLethal);    
                        }
                    }
                }
            } else {
                debugLog += "\n-Target is dead or is passive";
                debugLog += "\n-Do nothing";
            }
        } else if (!actor.combatComponent.isInActualCombat) {
            debugLog += "\n-Target is not hostile and Character is not in combat";
            if (disguisedActor.isNormalCharacter && !IsPOICurrentlyTargetedByAPerformingAction(targetCharacter)) {
                debugLog += "\n-Character is a villager and Target is not being targeted by an action, continue reaction";
                if (!targetCharacter.isDead) {
                    debugLog += "\n-Target is not dead";
                    if (!actor.isConversing && !targetCharacter.isConversing && actor.nonActionEventsComponent.CanInteract(targetCharacter) 
                        //only allow chat if characters current action is not have affair or if his action is have affair but the character he is reacting to is not the target of that action.
                        && (actor.currentActionNode == null || (actor.currentActionNode.action.goapType != INTERACTION_TYPE.HAVE_AFFAIR || actor.currentActionNode.poiTarget != targetCharacter))) {
                        debugLog += "\n-Character and Target are not Chatting or Flirting and Character can interact with Target, has 3% chance to Chat";
                        int chance = UnityEngine.Random.Range(0, 100);
                        debugLog += $"\n-Roll: {chance.ToString()}";
                        if (chance < 3) {
                            debugLog += "\n-Chat triggered";
                            actor.interruptComponent.TriggerInterrupt(INTERRUPT.Chat, targetCharacter);
                        } else {
                            debugLog += "\n-Chat did not trigger, will now trigger Flirt if Character is Sexually Compatible with Target and Character is Unfaithful, or Target is Lover or Affair, or Character has no Lover";
                            Trait angry = actor.traitContainer.GetNormalTrait<Trait>("Angry");
                            bool isAngryWithTarget = angry?.responsibleCharacters != null && angry.responsibleCharacters.Contains(disguisedTarget);
                            if (RelationshipManager.IsSexuallyCompatibleOneSided(disguisedActor.sexuality, disguisedTarget.sexuality, disguisedActor.gender, disguisedTarget.gender)
                                && disguisedActor.relationshipContainer.IsFamilyMember(disguisedTarget) == false && isAngryWithTarget == false) {
                                
                                if (disguisedActor.relationshipContainer.HasRelationshipWith(disguisedTarget, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.AFFAIR)
                                    || disguisedActor.relationshipContainer.GetFirstRelatableIDWithRelationship(RELATIONSHIP_TYPE.LOVER) == -1
                                    || disguisedActor.traitContainer.HasTrait("Unfaithful")) {
                                    debugLog += "\n-Flirt has 1% (multiplied by Compatibility value) chance to trigger";
                                    int compatibility = RelationshipManager.Instance.GetCompatibilityBetween(disguisedActor, disguisedTarget);
                                    int baseChance = 1;
                                    if (actor.moodComponent.moodState == MOOD_STATE.Normal) {
                                        debugLog += "\n-Flirt has +2% chance to trigger because character is in a normal mood";
                                        baseChance += 2;
                                    }

                                    int flirtChance;
                                    if (compatibility != -1) {
                                        flirtChance = baseChance * compatibility;
                                        debugLog += $"\n-Chance: {flirtChance.ToString()}";
                                    } else {
                                        flirtChance = baseChance * 2;
                                        debugLog += $"\n-Chance: {flirtChance.ToString()} (No Compatibility)";
                                    }
                                    int flirtRoll = UnityEngine.Random.Range(0, 100);
                                    debugLog += $"\n-Roll: {flirtRoll.ToString()}";
                                    if (flirtRoll < flirtChance) {
                                        actor.interruptComponent.TriggerInterrupt(INTERRUPT.Flirt, targetCharacter);
                                    } else {
                                        debugLog += "\n-Flirt did not trigger";
                                    }
                                } else {
                                    debugLog += "\n-Flirt did not trigger";
                                }
                            }
                        }
                    }

                    if (disguisedTarget.isNormalCharacter && disguisedActor.isNormalCharacter && disguisedTarget.traitContainer.HasTrait("Criminal")
                        && (!disguisedTarget.traitContainer.HasTrait("Restrained") || !(disguisedTarget.currentSettlement != null && disguisedTarget.currentSettlement is NPCSettlement npcSettlement && disguisedTarget.currentStructure == npcSettlement.prison))) {
                        debugLog += "\n-Target Character is a criminal";
                        bool cannotReactToCriminal = false;
                        if (actor.currentJob != null && actor.currentJob is GoapPlanJob planJob) {
                            cannotReactToCriminal = planJob.jobType == JOB_TYPE.APPREHEND && planJob.targetPOI == targetCharacter;
                            debugLog += "\n-Character is current job is already apprehend targeting target";
                        }
                        if (!cannotReactToCriminal) {
                            string opinionLabel = disguisedActor.relationshipContainer.GetOpinionLabel(disguisedTarget);
                            if ((opinionLabel == RelationshipManager.Friend || opinionLabel == RelationshipManager.Close_Friend)
                                || ((disguisedActor.relationshipContainer.IsFamilyMember(disguisedTarget) || disguisedActor.relationshipContainer.HasRelationshipWith(disguisedTarget, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.AFFAIR))
                                && opinionLabel != RelationshipManager.Rival)) {
                                debugLog += "\n-Character is friends/close friend/family member/lover/affair/not rival with target";
                                Criminal criminalTrait = disguisedTarget.traitContainer.GetNormalTrait<Criminal>("Criminal");
                                if (!criminalTrait.HasCharacterThatIsAlreadyWorried(disguisedActor)) {
                                    debugLog += "\n-Character will worry";
                                    criminalTrait.AddCharacterThatIsAlreadyWorried(disguisedActor);
                                    actor.interruptComponent.TriggerInterrupt(INTERRUPT.Worried, targetCharacter);
                                } else {
                                    debugLog += "\n-Character already worried about this target";
                                }
                            } else {
                                debugLog += "\n-Character is not friends with target";
                                debugLog += "\n-Character will try to apprehend";
                                bool canDoJob = false;
                                if (!targetCharacter.HasJobTargetingThis(JOB_TYPE.APPREHEND)) {
                                    actor.jobComponent.TryCreateApprehend(targetCharacter, ref canDoJob);
                                }
                                if (!canDoJob) {
                                    debugLog += "\n-Character cannot do apprehend, will flee instead";
                                    actor.combatComponent.Flight(targetCharacter, "saw criminal " + targetCharacter.name);
                                }
                            }
                        }
                    }

                    if (disguisedActor.faction == disguisedTarget.faction || disguisedActor.homeSettlement == disguisedTarget.homeSettlement) {
                        debugLog += "\n-Character and Target are with the same faction or npcSettlement";
                        if (disguisedActor.relationshipContainer.IsEnemiesWith(disguisedTarget)) {
                            debugLog += "\n-Character considers Target as Enemy or Rival";
                            if ((!targetCharacter.canMove || !targetCharacter.canPerform)) {
                                debugLog += "\n-Target can neither move or perform";
                                if (disguisedActor.moodComponent.moodState == MOOD_STATE.Bad || disguisedActor.moodComponent.moodState == MOOD_STATE.Critical) {
                                    debugLog += "\n-Actor is in Bad or Critical mood";
                                    if (UnityEngine.Random.Range(0, 2) == 0) {
                                        debugLog += "\n-Character triggered Mock interrupt";
                                        actor.interruptComponent.TriggerInterrupt(INTERRUPT.Mock, targetCharacter);
                                    } else {
                                        debugLog += "\n-Character triggered Laugh At interrupt";
                                        actor.interruptComponent.TriggerInterrupt(INTERRUPT.Laugh_At, targetCharacter);
                                    }
                                } else {
                                    debugLog += "\n-Actor is in Normal mood, will trigger shocked interrupt";
                                    actor.interruptComponent.TriggerInterrupt(INTERRUPT.Shocked, targetCharacter);
                                }
                            }
                        } else if (!disguisedActor.traitContainer.HasTrait("Psychopath")) {
                            debugLog += "\n-Character is not Psychopath and does not consider Target as Enemy or Rival";
                            bool targetIsParalyzedOrEnsnared =
                                targetCharacter.traitContainer.HasTrait("Paralyzed", "Ensnared");
                            bool targetIsRestrainedCriminal =
                                (targetCharacter.traitContainer.HasTrait("Restrained") &&
                                 disguisedTarget.traitContainer.HasTrait("Criminal"));
                            if (targetIsParalyzedOrEnsnared || targetIsRestrainedCriminal) {
                                debugLog += $"\n-Target is Restrained Criminal({targetIsRestrainedCriminal.ToString()}) or is Paralyzed or Ensnared({targetIsParalyzedOrEnsnared.ToString()})";
                                if (targetCharacter.needsComponent.isHungry || targetCharacter.needsComponent.isStarving) {
                                    debugLog += "\n-Target is hungry or starving, will create feed job";
                                    actor.jobComponent.TryTriggerFeed(targetCharacter);
                                } else if ((targetCharacter.needsComponent.isTired || targetCharacter.needsComponent.isExhausted) && targetIsParalyzedOrEnsnared) {
                                    debugLog += "\n-Target is tired or exhausted, will create Move Character job to bed if Target has a home and an available bed";
                                    if (disguisedTarget.homeStructure != null) {
                                        Bed bed = disguisedTarget.homeStructure.GetUnoccupiedTileObject(TILE_OBJECT_TYPE.BED) as Bed;
                                        if (bed != null && bed.gridTileLocation != targetCharacter.gridTileLocation) {
                                            debugLog += "\n-Target has a home and an available bed, will trigger Move Character job to bed";
                                            actor.jobComponent.TryTriggerMoveCharacter(targetCharacter, disguisedTarget.homeStructure, bed.gridTileLocation);
                                        } else {
                                            debugLog += "\n-Target has a home but does not have an available bed or already in bed, will not trigger Move Character job";
                                        }
                                    } else {
                                        debugLog += "\n-Target does not have a home, will not trigger Move Character job";
                                    }
                                } else if ((targetCharacter.needsComponent.isBored || targetCharacter.needsComponent.isSulking) && targetIsParalyzedOrEnsnared) {
                                    debugLog += "\n-Target is bored or sulking, will trigger Move Character job if character is not in the right place to do Daydream or Pray";
                                    if (UnityEngine.Random.Range(0, 2) == 0 && disguisedTarget.homeStructure != null) {
                                        //Pray
                                        if (targetCharacter.currentStructure != disguisedTarget.homeStructure) {
                                            debugLog += "\n-Target chose Pray and is not inside his/her house, will trigger Move Character job";
                                            actor.jobComponent.TryTriggerMoveCharacter(targetCharacter, disguisedTarget.homeStructure);
                                        } else {
                                            debugLog += "\n-Target chose Pray but is already inside his/her house, will not trigger Move Character job";
                                        }
                                    } else {
                                        //Daydream
                                        if (!targetCharacter.currentStructure.structureType.IsOpenSpace()) {
                                            debugLog += "\n-Target chose Daydream and is not in an open space structure, will trigger Move Character job";
                                            actor.jobComponent.TryTriggerMoveCharacter(targetCharacter, targetCharacter.currentRegion.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS));
                                        } else {
                                            debugLog += "\n-Target chose Daydream but is already in an open space structure, will not trigger Move Character job";
                                        }
                                    }
                                }
                            }

                            //Add personal Remove Status - Restrained job when seeing a restrained non-enemy villager
                            //https://trello.com/c/Pe6wuHQc/1197-add-personal-remove-status-restrained-job-when-seeing-a-restrained-non-enemy-villager
                            if (disguisedActor.isNormalCharacter && disguisedTarget.isNormalCharacter && targetCharacter.traitContainer.HasTrait("Restrained") && !disguisedTarget.traitContainer.HasTrait("Criminal")) {
                                actor.jobComponent.TriggerRemoveStatusTarget(targetCharacter, "Restrained");
                            }

                        }
                    }
                } else {
                    debugLog += "\n-Target is dead";
                    //Dead targetDeadTrait = targetCharacter.traitContainer.GetNormalTrait<Dead>("Dead");
                    if(!targetCharacter.reactionComponent.charactersThatSawThisDead.Contains(disguisedActor)) { //targetDeadTrait != null && !targetDeadTrait.charactersThatSawThisDead.Contains(owner)
                        targetCharacter.reactionComponent.AddCharacterThatSawThisDead(disguisedActor);
                        debugLog += "\n-Target saw dead for the first time";
                        string opinionLabel = disguisedActor.relationshipContainer.GetOpinionLabel(disguisedTarget);
                        if(opinionLabel == RelationshipManager.Friend || opinionLabel == RelationshipManager.Close_Friend) {
                            debugLog += "\n-Target is Friend/Close Friend";
                            if (UnityEngine.Random.Range(0, 2) == 0) {
                                debugLog += "\n-Target will Cry";
                                actor.interruptComponent.TriggerInterrupt(INTERRUPT.Cry, targetCharacter, "saw dead " + disguisedTarget.name);
                            } else {
                                debugLog += "\n-Target will Puke";
                                actor.interruptComponent.TriggerInterrupt(INTERRUPT.Puke, targetCharacter, "saw dead " + disguisedTarget.name);
                            }
                        } else if ((disguisedActor.relationshipContainer.IsFamilyMember(disguisedTarget) || 
                                    disguisedActor.relationshipContainer.HasRelationshipWith(disguisedTarget, RELATIONSHIP_TYPE.AFFAIR)) && 
                                  !disguisedActor.relationshipContainer.HasOpinionLabelWithCharacter(disguisedTarget, BaseRelationshipContainer.Rival)) {
                            debugLog += "\n-Target is Relative, Lover or Affair and not Rival";
                            // if Actor is Relative, Lover, Affair and not a Rival
                            if (UnityEngine.Random.Range(0, 2) == 0) {
                                debugLog += "\n-Target will Cry";
                                actor.interruptComponent.TriggerInterrupt(INTERRUPT.Cry, targetCharacter, "saw dead " + disguisedTarget.name);
                            } else {
                                debugLog += "\n-Target will Puke";
                                actor.interruptComponent.TriggerInterrupt(INTERRUPT.Puke, targetCharacter, "saw dead " + disguisedTarget.name);
                            }
                        } else if (opinionLabel == RelationshipManager.Rival || opinionLabel == RelationshipManager.Enemy) {
                            debugLog += "\n-Target is Rival/Enemy";
                            if (UnityEngine.Random.Range(0, 2) == 0) {
                                debugLog += "\n-Target will Mock";
                                actor.interruptComponent.TriggerInterrupt(INTERRUPT.Mock, targetCharacter);
                            } else {
                                debugLog += "\n-Target will Laugh At";
                                actor.interruptComponent.TriggerInterrupt(INTERRUPT.Laugh_At, targetCharacter);
                            }
                        }

                        if (actor.marker && disguisedTarget.isNormalCharacter) {
                            if(disguisedActor.traitContainer.HasTrait("Suspicious") 
                                || actor.moodComponent.moodState == MOOD_STATE.Critical 
                                || (actor.moodComponent.moodState == MOOD_STATE.Bad && UnityEngine.Random.Range(0, 2) == 0)
                                || UnityEngine.Random.Range(0, 100) < 15) {
                                debugLog += "\n-Owner is Suspicious or Critical Mood or Low Mood";

                                _assumptionSuspects.Clear();
                                for (int i = 0; i < actor.marker.inVisionCharacters.Count; i++) {
                                    Character inVision = actor.marker.inVisionCharacters[i];
                                    if (inVision != targetCharacter && inVision.relationshipContainer.IsEnemiesWith(disguisedTarget)) {
                                        if(inVision.currentJob != null && inVision.currentJob.jobType == JOB_TYPE.BURY) {
                                            //If the in vision character is going to bury the dead, do not assume
                                            continue;
                                        }
                                        _assumptionSuspects.Add(inVision);
                                    }
                                }
                                if(_assumptionSuspects.Count > 0) {
                                    debugLog += "\n-There are in vision characters that considers target character as Enemy/Rival";
                                    Character chosenSuspect = _assumptionSuspects[UnityEngine.Random.Range(0, _assumptionSuspects.Count)];

                                    debugLog += "\n-Will create Murder assumption on " + chosenSuspect.name;
                                    actor.assumptionComponent.CreateAndReactToNewAssumption(chosenSuspect, disguisedTarget, INTERACTION_TYPE.MURDER, REACTION_STATUS.WITNESSED);
                                }
                            }

                        }
                    }
                }
            } else {
                debugLog += "\n-Character is minion or summon or Target is currently being targeted by an action, not going to react";
            }
        }
    }
    private void ReactTo(Character actor, TileObject targetTileObject, ref string debugLog) {
        if(actor is Troll) {
            if(targetTileObject is BallLightningTileObject || targetTileObject.traitContainer.HasTrait("Lightning Remnant")) {
                actor.combatComponent.Flight(targetTileObject, "saw something frightening");
            } else if(targetTileObject is WoodPile || targetTileObject is StonePile || targetTileObject is MetalPile || targetTileObject is Gold || targetTileObject is Diamond) {
                if (targetTileObject.gridTileLocation.structure != actor.homeStructure && !actor.jobQueue.HasJob(JOB_TYPE.DROP_ITEM)) {
                    actor.jobComponent.CreateHoardItemJob(targetTileObject, actor.homeStructure, true);
                }
            }
        }
        if (!actor.isNormalCharacter /*|| owner.race == RACE.SKELETON*/) {
            //Minions or Summons cannot react to objects
            return;
        }
        debugLog += $"{actor.name} is reacting to {targetTileObject.nameWithID}";
        if (!actor.combatComponent.isInActualCombat && !actor.hasSeenFire) {
            if (targetTileObject.traitContainer.HasTrait("Burning")
                && targetTileObject.gridTileLocation != null
                && actor.homeSettlement != null
                && targetTileObject.gridTileLocation.IsPartOfSettlement(actor.homeSettlement)
                && !actor.traitContainer.HasTrait("Pyrophobic")
                && !actor.traitContainer.HasTrait("Dousing")
                && actor.jobQueue.HasJob(JOB_TYPE.DOUSE_FIRE) == false) {
                debugLog += "\n-Target is Burning and Character is not Pyrophobic";
                actor.SetHasSeenFire(true);
                actor.homeSettlement.settlementJobTriggerComponent.TriggerDouseFire();
                if (actor.homeSettlement.HasJob(JOB_TYPE.DOUSE_FIRE) == false) {
                    Debug.LogWarning($"{actor.name} saw a fire in a settlement but no douse fire jobs were created.");
                }

                List<JobQueueItem> douseFireJobs = actor.homeSettlement.GetJobs(JOB_TYPE.DOUSE_FIRE)
                    .Where(j => j.assignedCharacter == null && actor.jobQueue.CanJobBeAddedToQueue(j)).ToList();

                if (douseFireJobs.Count > 0) {
                    actor.jobQueue.AddJobInQueue(douseFireJobs[0]);
                } else {
                    if (actor.combatComponent.combatMode == COMBAT_MODE.Aggressive) {
                        actor.combatComponent.Flight(targetTileObject, "saw fire");
                    }
                }

                // for (int i = 0; i < owner.homeSettlement.availableJobs.Count; i++) {
                //     JobQueueItem job = owner.homeSettlement.availableJobs[i];
                //     if (job.jobType == JOB_TYPE.DOUSE_FIRE) {
                //         if (job.assignedCharacter == null && owner.jobQueue.CanJobBeAddedToQueue(job)) {
                //             owner.jobQueue.AddJobInQueue(job);
                //         } else {
                //             if (owner.combatComponent.combatMode == COMBAT_MODE.Aggressive) {
                //                 owner.combatComponent.Flight(targetTileObject, "saw fire");
                //             }
                //         }
                //     }
                // }
            }
        }
        if (!actor.combatComponent.isInActualCombat && !actor.hasSeenWet) {
            if (targetTileObject.traitContainer.HasTrait("Wet")
                && targetTileObject.gridTileLocation != null
                && actor.homeSettlement != null
                && targetTileObject.gridTileLocation.IsPartOfSettlement(actor.homeSettlement)
                && !actor.jobQueue.HasJob(JOB_TYPE.DRY_TILES)) {
                debugLog += "\n-Target is Wet";
                actor.SetHasSeenWet(true);
                actor.homeSettlement.settlementJobTriggerComponent.TriggerDryTiles();
                for (int i = 0; i < actor.homeSettlement.availableJobs.Count; i++) {
                    JobQueueItem job = actor.homeSettlement.availableJobs[i];
                    if (job.jobType == JOB_TYPE.DRY_TILES) {
                        if (job.assignedCharacter == null && actor.jobQueue.CanJobBeAddedToQueue(job)) {
                            actor.jobQueue.AddJobInQueue(job);
                        }
                    }
                }
            }
        }
        if (!actor.combatComponent.isInActualCombat && !actor.hasSeenPoisoned) {
            if (targetTileObject.traitContainer.HasTrait("Poisoned")
                && targetTileObject.gridTileLocation != null
                && actor.homeSettlement != null
                && targetTileObject.gridTileLocation.IsPartOfSettlement(actor.homeSettlement)
                && !actor.jobQueue.HasJob(JOB_TYPE.CLEANSE_TILES)) {
                debugLog += "\n-Target is Poisoned";
                actor.SetHasSeenPoisoned(true);
                actor.homeSettlement.settlementJobTriggerComponent.TriggerCleanseTiles();
                for (int i = 0; i < actor.homeSettlement.availableJobs.Count; i++) {
                    JobQueueItem job = actor.homeSettlement.availableJobs[i];
                    if (job.jobType == JOB_TYPE.CLEANSE_TILES) {
                        if (job.assignedCharacter == null && actor.jobQueue.CanJobBeAddedToQueue(job)) {
                            actor.jobQueue.AddJobInQueue(job);
                        }
                    }
                }
            }
        }
        if (targetTileObject.traitContainer.HasTrait("Dangerous") && targetTileObject.gridTileLocation != null) {
            if (targetTileObject is TornadoTileObject || actor.currentStructure == targetTileObject.gridTileLocation.structure || (!actor.currentStructure.isInterior && !targetTileObject.gridTileLocation.structure.isInterior)) {
                if (actor.traitContainer.HasTrait("Berserked")) {
                    actor.combatComponent.FightOrFlight(targetTileObject, CombatManager.Berserked);
                } else if (actor.stateComponent.currentState == null || actor.stateComponent.currentState.characterState != CHARACTER_STATE.FOLLOW) {
                    if (actor.traitContainer.HasTrait("Suicidal")) {
                        if (!actor.jobQueue.HasJob(JOB_TYPE.SUICIDE_FOLLOW)) {
                            CharacterStateJob job = JobManager.Instance.CreateNewCharacterStateJob(JOB_TYPE.SUICIDE_FOLLOW, CHARACTER_STATE.FOLLOW, targetTileObject, actor);
                            actor.jobQueue.AddJobInQueue(job);
                        }
                    } else if (actor.moodComponent.moodState == MOOD_STATE.Normal) {
                        string neutralizingTraitName = TraitManager.Instance.GetNeutralizingTraitFor(targetTileObject);
                        if (neutralizingTraitName != string.Empty) {
                            if (actor.traitContainer.HasTrait(neutralizingTraitName)) {
                                if (!actor.jobQueue.HasJob(JOB_TYPE.NEUTRALIZE_DANGER, targetTileObject)) {
                                    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.NEUTRALIZE_DANGER,
                                        INTERACTION_TYPE.NEUTRALIZE, targetTileObject, actor);
                                    actor.jobQueue.AddJobInQueue(job);
                                }
                            } else {
                                actor.combatComponent.Flight(targetTileObject, "saw a " + targetTileObject.name);
                            }
                        } else {
                            throw new Exception("Trying to neutralize " + targetTileObject.nameWithID + " but it does not have a neutralizing trait!");
                        }
                    } else {
                        actor.combatComponent.Flight(targetTileObject, "saw a " + targetTileObject.name);
                    }
                }
            }
        }
        //if (targetTileObject.tileObjectType.IsTileObjectAnItem()) {
        //    if (targetTileObject.gridTileLocation != null && owner.homeSettlement != null
        //        && targetTileObject.gridTileLocation.structure != owner.homeSettlement.mainStorage
        //        && !(targetTileObject.gridTileLocation.structure is Dwelling) 
        //        && !owner.IsInventoryAtFullCapacity()
        //        && (owner.jobQueue.jobsInQueue.Count == 0 || owner.jobQueue.jobsInQueue[0].priority < JOB_TYPE.TAKE_ITEM.GetJobTypePriority())) {
        //        owner.jobComponent.CreateTakeItemJob(targetTileObject);
        //    }
        //}
        if (targetTileObject.traitContainer.HasTrait("Danger Remnant", "Lightning Remnant")) {
            if (!actor.traitContainer.HasTrait("Berserked")) {
                if (targetTileObject.gridTileLocation != null && targetTileObject.gridTileLocation.isCorrupted) {
                    CharacterManager.Instance.TriggerEmotion(EMOTION.Fear, actor, targetTileObject, REACTION_STATUS.WITNESSED);
                } else {
                    if (actor.traitContainer.HasTrait("Coward")) {
                        CharacterManager.Instance.TriggerEmotion(EMOTION.Fear, actor, targetTileObject, REACTION_STATUS.WITNESSED);
                    } else {
                        int shockChance = 30;
                        if (actor.traitContainer.HasTrait("Combatant")) {
                            shockChance = 70;
                        }
                        if (UnityEngine.Random.Range(0, 100) < shockChance) {
                            CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, actor, targetTileObject, REACTION_STATUS.WITNESSED);
                        } else {
                            CharacterManager.Instance.TriggerEmotion(EMOTION.Fear, actor, targetTileObject, REACTION_STATUS.WITNESSED);
                        }
                    }
                }
                
            }
        }
        if (targetTileObject.traitContainer.HasTrait("Surprised Remnant")) {
            if (!actor.traitContainer.HasTrait("Berserked")) {
                if (targetTileObject.gridTileLocation != null && targetTileObject.gridTileLocation.isCorrupted) {
                    CharacterManager.Instance.TriggerEmotion(EMOTION.Fear, actor, targetTileObject, REACTION_STATUS.WITNESSED);
                } else {
                    if (actor.traitContainer.HasTrait("Coward")) {
                        CharacterManager.Instance.TriggerEmotion(EMOTION.Fear, actor, targetTileObject, REACTION_STATUS.WITNESSED);
                    } else {
                        if (UnityEngine.Random.Range(0, 100) < 95) {
                            CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, actor, targetTileObject, REACTION_STATUS.WITNESSED);
                        } else {
                            CharacterManager.Instance.TriggerEmotion(EMOTION.Fear, actor, targetTileObject, REACTION_STATUS.WITNESSED);
                        }
                    }
                }
            }
        }


        if (targetTileObject is Tombstone tombstone) {
            Character targetCharacter = tombstone.character;
            //Dead targetDeadTrait = targetCharacter.traitContainer.GetNormalTrait<Dead>("Dead");
            if (!targetCharacter.reactionComponent.charactersThatSawThisDead.Contains(actor)) { //targetDeadTrait != null && !targetDeadTrait.charactersThatSawThisDead.Contains(owner)
                targetCharacter.reactionComponent.AddCharacterThatSawThisDead(actor);
                debugLog += "\n-Target saw dead for the first time";
                string opinionLabel = actor.relationshipContainer.GetOpinionLabel(targetCharacter);
                if (opinionLabel == RelationshipManager.Friend || opinionLabel == RelationshipManager.Close_Friend) {
                    debugLog += "\n-Target is Friend/Close Friend";
                    if (UnityEngine.Random.Range(0, 2) == 0) {
                        debugLog += "\n-Target will Cry";
                        actor.interruptComponent.TriggerInterrupt(INTERRUPT.Cry, targetCharacter, "saw dead " + targetCharacter.name);
                    } else {
                        debugLog += "\n-Target will Puke";
                        actor.interruptComponent.TriggerInterrupt(INTERRUPT.Puke, targetCharacter, "saw dead " + targetCharacter.name);
                    }
                } else if (opinionLabel == RelationshipManager.Rival) {
                    debugLog += "\n-Target is Rival";
                    if (UnityEngine.Random.Range(0, 2) == 0) {
                        debugLog += "\n-Target will Mock";
                        actor.interruptComponent.TriggerInterrupt(INTERRUPT.Mock, targetCharacter);
                    } else {
                        debugLog += "\n-Target will Laugh At";
                        actor.interruptComponent.TriggerInterrupt(INTERRUPT.Laugh_At, targetCharacter);
                    }
                }
            }
        }

        if (targetTileObject.IsOwnedBy(actor)
            && targetTileObject.gridTileLocation != null 
            && targetTileObject.gridTileLocation.structure != null
            && targetTileObject.gridTileLocation.structure is Dwelling
            && targetTileObject.gridTileLocation.structure != actor.homeStructure) {

            if (targetTileObject.gridTileLocation.structure.residents.Count > 0 && !targetTileObject.HasCharacterAlreadyAssumed(actor)) {
                if (actor.traitContainer.HasTrait("Suspicious")
                || actor.moodComponent.moodState == MOOD_STATE.Critical
                || (actor.moodComponent.moodState == MOOD_STATE.Bad && UnityEngine.Random.Range(0, 2) == 0)
                || UnityEngine.Random.Range(0, 100) < 15
                || TutorialManager.Instance.IsTutorialCurrentlyActive(TutorialManager.Tutorial.Frame_Up)) {
                    debugLog += "\n-Owner is Suspicious or Critical Mood or Low Mood";

                    debugLog += "\n-There is at least 1 resident of the structure";
                    _assumptionSuspects.Clear();
                    for (int i = 0; i < targetTileObject.gridTileLocation.structure.residents.Count; i++) {
                        Character resident = targetTileObject.gridTileLocation.structure.residents[i];
                        AWARENESS_STATE awarenessState = actor.relationshipContainer.GetAwarenessState(resident);
                        if (awarenessState == AWARENESS_STATE.Available) {
                            _assumptionSuspects.Add(resident);
                        } else if (awarenessState == AWARENESS_STATE.None) {
                            if (!resident.isDead) {
                                _assumptionSuspects.Add(resident);
                            }
                        }
                    }
                    if(_assumptionSuspects.Count > 0) {
                        Character chosenSuspect = _assumptionSuspects[UnityEngine.Random.Range(0, _assumptionSuspects.Count)];
                        debugLog += "\n-Will create Steal assumption on " + chosenSuspect.name;
                        actor.assumptionComponent.CreateAndReactToNewAssumption(chosenSuspect, targetTileObject, INTERACTION_TYPE.STEAL, REACTION_STATUS.WITNESSED);
                    }
                }
            }
            if(targetTileObject.tileObjectType.IsTileObjectAnItem() && 
               !actor.jobQueue.HasJob(JOB_TYPE.TAKE_ITEM, targetTileObject) && 
               targetTileObject.Advertises(INTERACTION_TYPE.PICK_UP)) {
                actor.jobComponent.CreateTakeItemJob(targetTileObject);
            }
        }

        if (targetTileObject is CultistKit && targetTileObject.IsOwnedBy(actor) == false) {
            debugLog += "\n-Object is a cultist kit";
            if (targetTileObject.gridTileLocation != null) {
                if (targetTileObject.structureLocation is ManMadeStructure && 
                    targetTileObject.structureLocation.GetNumberOfResidentsExcluding(out var validResidents,actor) > 0) {
                    debugLog += "\n-Cultist kit is at structure with residents excluding the witness";
                    int chanceToCreateAssumption = 0;
                    if (actor.traitContainer.HasTrait("Suspicious") || actor.moodComponent.moodState == MOOD_STATE.Critical) {
                        chanceToCreateAssumption = 100;
                    } else if (actor.moodComponent.moodState == MOOD_STATE.Bad) {
                        chanceToCreateAssumption = 50;
                    } else {
                        chanceToCreateAssumption = 15;
                    }
                    debugLog += "\n-Rolling for chance to create assumption";
                    if (GameUtilities.RollChance(chanceToCreateAssumption, ref debugLog)) {
                        _assumptionSuspects.Clear();
                        if(validResidents != null) {
                            for (int i = 0; i < validResidents.Count; i++) {
                                Character resident = validResidents[i];
                                AWARENESS_STATE awarenessState = actor.relationshipContainer.GetAwarenessState(resident);
                                if (awarenessState == AWARENESS_STATE.Available) {
                                    _assumptionSuspects.Add(resident);
                                } else if (awarenessState == AWARENESS_STATE.None) {
                                    if (!resident.isDead) {
                                        _assumptionSuspects.Add(resident);
                                    }
                                }
                            }
                        }
                        Character chosenTarget = CollectionUtilities.GetRandomElement(_assumptionSuspects);
                        if(chosenTarget != null) {
                            actor.assumptionComponent.CreateAndReactToNewAssumption(chosenTarget, targetTileObject, INTERACTION_TYPE.IS_CULTIST, REACTION_STATUS.WITNESSED);
                        }
                    }
                }
            } 
        }

        if (targetTileObject is ResourcePile resourcePile && actor.homeSettlement != null) {
            //if character sees a resource pile that is outside his/her home settlement or
            //is not at his/her settlement's main storage
            if (resourcePile.gridTileLocation.IsPartOfSettlement(actor.homeSettlement) == false || 
                resourcePile.gridTileLocation.structure != actor.homeSettlement.mainStorage) {
                //do not create haul job for human and elven meat
                if (resourcePile.tileObjectType != TILE_OBJECT_TYPE.ELF_MEAT && 
                    resourcePile.tileObjectType != TILE_OBJECT_TYPE.HUMAN_MEAT) {
                    actor.homeSettlement.settlementJobTriggerComponent.TryCreateHaulJob(resourcePile);
                }
            }
        }
    }
    private void ReactToCarriedObject(Character actor, TileObject targetTileObject, Character carrier, ref string debugLog) {
        debugLog += $"{actor.name} is reacting to {targetTileObject.nameWithID} carried by {carrier.name}";
        if (targetTileObject is CultistKit) {
            debugLog += $"Object is cultist kit, creating assumption...";
            Character disguisedTarget = carrier;
            if (carrier.reactionComponent.disguisedCharacter != null) {
                disguisedTarget = carrier.reactionComponent.disguisedCharacter;
            }
            if (!disguisedTarget.isDead) {
                actor.assumptionComponent.CreateAndReactToNewAssumption(disguisedTarget, targetTileObject, INTERACTION_TYPE.IS_CULTIST, REACTION_STATUS.WITNESSED);
            }
        }
    }
    //The reason why we pass the character that was hit instead of just getting the current closest hostile in combat state is because 
    public void ReactToCombat(CombatState combat, IPointOfInterest poiHit) {
        Character attacker = combat.stateComponent.character;
        Character reactor = owner;
        if (reactor.combatComponent.isInCombat) {
            string inCombatLog = reactor.name + " is in combat and reacting to combat of " + attacker.name + " against " + poiHit.nameWithID;
            if (reactor == poiHit) {
                inCombatLog += "\n-Reactor is the Hit Character";
                CombatState reactorCombat = reactor.stateComponent.currentState as CombatState;
                if (reactorCombat.isAttacking && reactorCombat.currentClosestHostile != null && reactorCombat.currentClosestHostile != attacker) {
                    inCombatLog += "\n-Reactor is currently attacking another character";
                    if (reactorCombat.currentClosestHostile is Character currentPursuingCharacter) {
                        if (currentPursuingCharacter.combatComponent.isInCombat && (currentPursuingCharacter.stateComponent.currentState as CombatState).isAttacking == false) {
                            inCombatLog += "\n-Character that is being attacked by reactor is currently fleeing";
                            inCombatLog += "\n-Reactor will determine combat reaction";
                            reactor.combatComponent.SetWillProcessCombat(true);
                            //if (reactor.combatComponent.hostilesInRange.Contains(attacker) || reactor.combatComponent.avoidInRange.Contains(attacker)) {
                            //log += "\n-Attacker of reactor is in hostile/avoid list of the reactor, rector will determine combat reaction";
                            //}
                        }
                    }
                }
            }
            reactor.logComponent.PrintLogIfActive(inCombatLog);
            return;
        }
        if (!owner.isNormalCharacter /*|| owner.race == RACE.SKELETON*/) {
            //Minions or Summons cannot react to objects
            return;
        }
        if(owner.isDead || !owner.canPerform) {
            return;
        }
        string log = reactor.name + " is reacting to combat of " + attacker.name + " against " + poiHit.nameWithID;
        if (reactor.IsHostileWith(attacker)) {
            log += "\n-Hostile with attacker, will skip processing";
            reactor.logComponent.PrintLogIfActive(log);
            return;
        }
        if (combat.DidCharacterAlreadyReactToThisCombat(reactor)) {
            log += "\n-Already reacted to the combat, will skip processing";
            reactor.logComponent.PrintLogIfActive(log);
            return;
        }
        if (poiHit is Character targetHit && reactor.IsHostileWith(targetHit)) {
            log += "\n-Reactor is hostile with the hit character, will skip processing";
            reactor.logComponent.PrintLogIfActive(log);
            return;
        }
        combat.AddCharacterThatReactedToThisCombat(reactor);
        if(poiHit is Character characterHit) {
            if (combat.currentClosestHostile != characterHit) {
                log += "\n-Hit Character is not the same as the actual target which is: " + combat.currentClosestHostile?.name;
                if (characterHit.combatComponent.isInCombat) {
                    log += "\n-Hit Character is in combat";
                    log += "\n-Do nothing";
                } else {
                    log += "\n-Reactor felt Shocked";
                    CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, reactor, attacker, REACTION_STATUS.WITNESSED);
                }
            } else {
                CombatData combatDataAgainstCharacterHit = attacker.combatComponent.GetCombatData(characterHit);
                if (combatDataAgainstCharacterHit != null && combatDataAgainstCharacterHit.connectedAction != null && combatDataAgainstCharacterHit.connectedAction.associatedJobType == JOB_TYPE.APPREHEND) {
                    log += "\n-Combat is part of Apprehend Job";
                    log += "\n-Reactor felt Shocked";
                    CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, reactor, attacker, REACTION_STATUS.WITNESSED);
                } else {
                    if (characterHit == reactor) {
                        log += "\n-Hit Character is the Reactor";
                        if (characterHit.relationshipContainer.IsFriendsWith(attacker)) {
                            log += "\n-Hit Character is Friends/Close Friends with Attacker";
                            log += "\n-Reactor felt Betrayal";
                            CharacterManager.Instance.TriggerEmotion(EMOTION.Betrayal, reactor, attacker, REACTION_STATUS.WITNESSED);
                        } else if (characterHit.relationshipContainer.IsEnemiesWith(attacker)) {
                            log += "\n-Hit Character is Enemies/Rivals with Attacker";
                            log += "\n-Reactor felt Anger";
                            CharacterManager.Instance.TriggerEmotion(EMOTION.Anger, reactor, attacker, REACTION_STATUS.WITNESSED);
                        }
                    } else {
                        log += "\n-Hit Character is NOT the Reactor";
                        if (reactor.relationshipContainer.IsFriendsWith(characterHit)) {
                            log += "\n-Reactor is Friends/Close Friends with Hit Character";
                            if (reactor.relationshipContainer.IsFriendsWith(attacker)) {
                                log += "\n-Reactor is Friends/Close Friends with Attacker";
                                log += "\n-Reactor felt Shock, Disappointment";
                                CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, reactor, attacker, REACTION_STATUS.WITNESSED);
                                CharacterManager.Instance.TriggerEmotion(EMOTION.Disappointment, reactor, attacker, REACTION_STATUS.WITNESSED);
                            } else if (reactor.relationshipContainer.IsEnemiesWith(attacker)) {
                                log += "\n-Reactor is Enemies/Rivals with Attacker";
                                log += "\n-Reactor felt Rage";
                                CharacterManager.Instance.TriggerEmotion(EMOTION.Rage, reactor, attacker, REACTION_STATUS.WITNESSED);
                            } else {
                                log += "\n-Reactor felt Anger";
                                CharacterManager.Instance.TriggerEmotion(EMOTION.Anger, reactor, attacker, REACTION_STATUS.WITNESSED);
                            }
                        } else if (reactor.relationshipContainer.IsEnemiesWith(characterHit)) {
                            log += "\n-Reactor is Enemies/Rivals with Hit Character";
                            if (reactor.relationshipContainer.IsFriendsWith(attacker)) {
                                log += "\n-Reactor is Friends/Close Friends with Attacker";
                                log += "\n-Reactor felt Approval";
                                CharacterManager.Instance.TriggerEmotion(EMOTION.Approval, reactor, attacker, REACTION_STATUS.WITNESSED);
                            } else if (reactor.relationshipContainer.IsEnemiesWith(attacker)) {
                                log += "\n-Reactor is Enemies/Rivals with Attacker";
                                log += "\n-Reactor felt Shock";
                                CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, reactor, attacker, REACTION_STATUS.WITNESSED);
                            } else {
                                log += "\n-Reactor felt Approval";
                                CharacterManager.Instance.TriggerEmotion(EMOTION.Approval, reactor, attacker, REACTION_STATUS.WITNESSED);
                            }
                        } else {
                            log += "\n-Reactor felt Shock";
                            CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, reactor, attacker, REACTION_STATUS.WITNESSED);
                        }
                    }
                }
            }
            //Check for crime
            if ((reactor.faction != null && reactor.faction == attacker.faction) || (reactor.homeSettlement != null && reactor.homeSettlement == attacker.homeSettlement)) {
                log += "\n-Reactor is the same faction/home settlement as Attacker";
                log += "\n-Reactor is checking for crime";
                CombatData combatDataAgainstPOIHit = attacker.combatComponent.GetCombatData(characterHit);
                if (combatDataAgainstPOIHit != null && combatDataAgainstPOIHit.connectedAction != null) {
                    ActualGoapNode possibleCrimeAction = combatDataAgainstPOIHit.connectedAction;
                    CRIME_TYPE crimeType = CrimeManager.Instance.GetCrimeTypeConsideringAction(possibleCrimeAction);
                    log += "\n-Crime committed is: " + crimeType.ToString();
                    if (crimeType != CRIME_TYPE.NONE) {
                        log += "\n-Reactor will react to crime";
                        CrimeManager.Instance.ReactToCrime(reactor, attacker, possibleCrimeAction, possibleCrimeAction.associatedJobType, crimeType);
                    }
                }
            }

        } else if (poiHit is TileObject objectHit) {
            if(!objectHit.IsOwnedBy(attacker)) {
                //CrimeManager.Instance.ReactToCrime()
                log += "\n-Object Hit is not owned by the Attacker";
                log += "\n-Reactor is checking for crime";
                CombatData combatDataAgainstPOIHit = attacker.combatComponent.GetCombatData(objectHit);
                if (combatDataAgainstPOIHit != null && combatDataAgainstPOIHit.connectedAction != null) {
                    ActualGoapNode possibleCrimeAction = combatDataAgainstPOIHit.connectedAction;
                    CRIME_TYPE crimeType = CrimeManager.Instance.GetCrimeTypeConsideringAction(possibleCrimeAction);
                    log += "\n-Crime committed is: " + crimeType.ToString();
                    if (crimeType != CRIME_TYPE.NONE) {
                        log += "\n-Reactor will react to crime";
                        CrimeManager.Instance.ReactToCrime(reactor, attacker, possibleCrimeAction, possibleCrimeAction.associatedJobType, crimeType);
                    }
                }
            }
        }

        reactor.logComponent.PrintLogIfActive(log);
    }
    // private void ReactTo(SpecialToken targetItem, ref string debugLog) {
    //     if (owner.minion != null || owner is Summon) {
    //         //Minions or Summons cannot react to items
    //         return;
    //     }
    //     debugLog += owner.name + " is reacting to " + targetItem.nameWithID;
    //     if (!owner.hasSeenFire) {
    //         if (targetItem.traitContainer.HasTrait("Burning")
    //             && targetItem.gridTileLocation != null
    //             && targetItem.gridTileLocation.IsPartOfSettlement(owner.homeNpcSettlement)
    //             && !owner.traitContainer.HasTrait("Pyrophobic")) {
    //             debugLog += "\n-Target is Burning and Character is not Pyrophobic";
    //             owner.SetHasSeenFire(true);
    //             owner.homeNpcSettlement.settlementJobTriggerComponent.TriggerDouseFire();
    //             for (int i = 0; i < owner.homeNpcSettlement.availableJobs.Count; i++) {
    //                 JobQueueItem job = owner.homeNpcSettlement.availableJobs[i];
    //                 if (job.jobType == JOB_TYPE.DOUSE_FIRE) {
    //                     if (job.assignedCharacter == null && owner.jobQueue.CanJobBeAddedToQueue(job)) {
    //                         owner.jobQueue.AddJobInQueue(job);
    //                     } else {
    //                         owner.combatComponent.Flight(targetItem);
    //                     }
    //                     return;
    //                 }
    //             }
    //         }
    //     }
    // }
    #endregion

    #region General
    private bool IsPOICurrentlyTargetedByAPerformingAction(IPointOfInterest poi) {
        for (int i = 0; i < poi.allJobsTargetingThis.Count; i++) {
            if(poi.allJobsTargetingThis[i] is GoapPlanJob) {
                GoapPlanJob planJob = poi.allJobsTargetingThis[i] as GoapPlanJob;
                if(planJob.assignedPlan != null && planJob.assignedPlan.currentActualNode.actionStatus == ACTION_STATUS.PERFORMING) {
                    return true;
                }
            }
        }
        return false;
    }
    public void AddCharacterThatSawThisDead(Character character) {
        charactersThatSawThisDead.Add(character);
    }
    public void SetIsHidden(bool state) {
        if(isHidden != state) {
            isHidden = state;
            owner.OnSetIsHidden();
            if (!isHidden) {
                //If character comes out from being hidden, all characters in vision should process this character
                if (owner.marker) {
                    for (int i = 0; i < owner.marker.inVisionCharacters.Count; i++) {
                        Character inVision = owner.marker.inVisionCharacters[i];
                        inVision.marker.AddUnprocessedPOI(owner);
                    }
                }
            }
            
        }
    }
    public void SetDisguisedCharacter(Character character) {
        if(disguisedCharacter != character) {
            disguisedCharacter = character;
            if (disguisedCharacter != null) {
                owner.visuals.UpdateAllVisuals(owner);
                Messenger.Broadcast(Signals.CHARACTER_DISGUISED, owner, character);
            } else {
                owner.visuals.UpdateAllVisuals(owner);
                if (!owner.isDead && owner.marker) {
                    for (int i = 0; i < owner.marker.inVisionCharacters.Count; i++) {
                        Character inVisionCharacter = owner.marker.inVisionCharacters[i];
                        if (!inVisionCharacter.isDead && inVisionCharacter.marker) {
                            inVisionCharacter.marker.AddUnprocessedPOI(owner);
                        }
                    }
                }
            }
        }
    }
    #endregion
}
