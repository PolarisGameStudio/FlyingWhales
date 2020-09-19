﻿using System.Collections;
using System.Collections.Generic;
using Logs;
using UnityEngine;
using Traits;

namespace Interrupts {
    public class InviteToMakeLove : Interrupt {
        public InviteToMakeLove() : base(INTERRUPT.Invite_To_Make_Love) {
            duration = 0;
            isSimulateneous = true;
            interruptIconString = GoapActionStateDB.Flirt_Icon;
            logTags = new[] {LOG_TAG.Social, LOG_TAG.Needs};
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder,
            ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            if(interruptHolder.target is Character targetCharacter) {
                string debugLog = $"{interruptHolder.actor.name} invite to make love interrupt with {targetCharacter.name}";
                Character actor = interruptHolder.actor;
                if (actor.reactionComponent.disguisedCharacter != null) {
                    actor = actor.reactionComponent.disguisedCharacter;
                }
                if (targetCharacter.reactionComponent.disguisedCharacter != null) {
                    targetCharacter = targetCharacter.reactionComponent.disguisedCharacter;
                }
                //if (targetCharacter.traitContainer.GetNormalTrait<Trait>("Unconscious") != null 
                //    || targetCharacter.combatComponent.isInCombat
                //    || (targetCharacter.stateComponent.currentState != null && targetCharacter.stateComponent.currentState.characterState == CHARACTER_STATE.DOUSE_FIRE)
                //    || (targetCharacter.interruptComponent.isInterrupted && targetCharacter.interruptComponent.currentInterrupt.interrupt == INTERRUPT.Cowering)) {
                //    debugLog += $"{targetCharacter.name} is unconscious/in combat/in douse fire state/cowering. Invite rejected.";
                //    actor.logComponent.PrintLogIfActive(debugLog);


                //    overrideEffectLog = new Log(GameManager.Instance.Today(), "Interrupt", "Invite To Make Love", "Reject");
                //    overrideEffectLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                //    overrideEffectLog.AddToFillers(target, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                //    actor.currentJob.CancelJob(false);
                //    return false;
                //}
                string chosen = "Reject";
                if (!interruptHolder.target.traitContainer.HasTrait("Unconscious")) {
                    WeightedDictionary<string> weights = new WeightedDictionary<string>();
                    int acceptWeight = 20;
                    int rejectWeight = 10;
                    Character targetLover = targetCharacter.relationshipContainer.GetFirstCharacterWithRelationship(RELATIONSHIP_TYPE.LOVER);
                    if (targetLover != null && targetLover != actor) {
                        //Target has a different lover
                        debugLog += $"\n-Target has different lover";
                        acceptWeight = 0;
                        rejectWeight = 50;
                        debugLog += $"\n-Base accept weight: {acceptWeight}";
                        debugLog += $"\n-Base reject weight: {rejectWeight}";

                        if (targetCharacter.traitContainer.HasTrait("Unfaithful")) {
                            acceptWeight += 200;
                            debugLog += $"\n-Target is unfaithful: +200 to Accept Weight";
                            if (targetCharacter.traitContainer.HasTrait("Drunk")) {
                                acceptWeight += 100;
                                debugLog += $"\n-Target is drunk: +100 to Accept Weight";
                            }
                        } else {
                            if (targetCharacter.traitContainer.HasTrait("Treacherous", "Psychopath")) {
                                acceptWeight += 50;
                                debugLog += $"\n-Target is not unfaithful but treacherous/psychopath: +50 to Accept Weight";
                            } else {
                                rejectWeight += 100;
                                debugLog += $"\n-Target is not unfaithful/treacherous/psychopath: +100 to Reject Weight";
                            }
                            if (targetCharacter.traitContainer.HasTrait("Drunk")) {
                                acceptWeight += 50;
                                debugLog += $"\n-Target is drunk: +50 to Accept Weight";
                            }
                        }
                    } else {
                        debugLog += $"\n-Base accept weight: {acceptWeight}";
                        debugLog += $"\n-Base reject weight: {rejectWeight}";

                        //int targetOpinionToActor = 0;
                        //if (targetCharacter.relationshipContainer.HasRelationshipWith(actor)) {
                        //    targetOpinionToActor = targetCharacter.relationshipContainer.GetTotalOpinion(actor);
                        //}
                        int compatibility = RelationshipManager.Instance.GetCompatibilityBetween(targetCharacter, actor);
                        acceptWeight += (10 * compatibility);
                        debugLog += $"\n-Target compatibility towards Actor: +(10 x {compatibility}) to Accept Weight";

                        if (targetCharacter.traitContainer.HasTrait("Drunk")) {
                            acceptWeight += 100;
                            debugLog += $"\n-Target is drunk: +100 to Accept Weight";
                        }
                    }

                    if (targetCharacter.traitContainer.HasTrait("Lustful")) {
                        acceptWeight += 100;
                        debugLog += "\n-Target is Lustful: +100 to Accept Weight";
                    } else if (targetCharacter.traitContainer.HasTrait("Chaste")) {
                        rejectWeight += 300;
                        debugLog += "\n-Target is Chaste: +300 to Reject Weight";
                    }

                    if (targetCharacter.moodComponent.moodState == MOOD_STATE.Bad) {
                        rejectWeight += 50;
                        debugLog += "\n-Target is Low mood: +50 to Reject Weight";
                    } else if (targetCharacter.moodComponent.moodState == MOOD_STATE.Critical) {
                        rejectWeight += 200;
                        debugLog += "\n-Target is Crit mood: +200 to Reject Weight";
                    }

                    weights.AddElement("Accept", acceptWeight);
                    weights.AddElement("Reject", rejectWeight);

                    debugLog += $"\n\n{weights.GetWeightsSummary("FINAL WEIGHTS")}";

                    chosen = weights.PickRandomElementGivenWeights();
                } else {
                    debugLog += "\n-Target is Unconscious: SURE REJECT";
                }
                debugLog += $"\n\nCHOSEN RESPONSE: {chosen}";
                interruptHolder.actor.logComponent.PrintLogIfActive(debugLog);


                overrideEffectLog = new Log(GameManager.Instance.Today(), "Interrupt", "Invite To Make Love", chosen, null, logTags);
                overrideEffectLog.AddToFillers(interruptHolder.actor, interruptHolder.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                overrideEffectLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                //actor.logComponent.RegisterLogAndShowNotifToThisCharacterOnly(log, onlyClickedCharacter: false);

                interruptHolder.SetIdentifier(chosen);
                if (chosen == "Reject") {
                    interruptHolder.actor.relationshipContainer.AdjustOpinion(interruptHolder.actor, targetCharacter, "Base", -3, "rejected sexual advances");
                    interruptHolder.actor.traitContainer.AddTrait(interruptHolder.actor, "Annoyed");
                    interruptHolder.actor.currentJob.CancelJob(false);
                    if(interruptHolder.actor.faction == FactionManager.Instance.disguisedFaction) {
                        interruptHolder.actor.ChangeFactionTo(PlayerManager.Instance.player.playerFaction);
                        if (!targetCharacter.marker.HasUnprocessedPOI(interruptHolder.actor)) {
                            targetCharacter.marker.AddUnprocessedPOI(interruptHolder.actor);
                        }
                    }
                    return false;
                }
            }
            return true;
        }
        public override string ReactionToActor(Character actor, IPointOfInterest target,
            Character witness,
            InterruptHolder interrupt, REACTION_STATUS status) {
            string response = base.ReactionToActor(actor, target, witness, interrupt, status);
            if (target != witness && target is Character targetCharacter) {
                bool isActorLoverOrAffairOfWitness = witness.relationshipContainer.HasRelationshipWith(actor, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.AFFAIR);
                bool isTargetLoverOrAffairOfWitness = witness.relationshipContainer.HasRelationshipWith(targetCharacter, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.AFFAIR);

                if (isActorLoverOrAffairOfWitness) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Rage, witness, actor, status);
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Betrayal, witness, actor, status);
                } else if (isTargetLoverOrAffairOfWitness) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Rage, witness, actor, status);
                    //response += CharacterManager.Instance.TriggerEmotion(EMOTION.Betrayal, witness, actor, status);
                    if (witness.relationshipContainer.IsFriendsWith(actor) || witness.relationshipContainer.IsFamilyMember(actor)) {
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Betrayal, witness, actor, status);
                    }
                } else {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Embarassment, witness, actor, status);
                    Character loverOfActor = actor.relationshipContainer.GetFirstCharacterWithRelationship(RELATIONSHIP_TYPE.LOVER);
                    if (loverOfActor != null && loverOfActor != targetCharacter) {
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disapproval, witness, actor, status);
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disgust, witness, actor, status);
                    } else if (witness.relationshipContainer.IsFriendsWith(actor)) {
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Scorn, witness, actor, status);
                    }
                }
            }
            return response;
        }
        #endregion
    }
}
