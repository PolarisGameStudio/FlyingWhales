﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;

namespace Interrupts {
    public class InviteToMakeLove : Interrupt {
        public InviteToMakeLove() : base(INTERRUPT.Invite_To_Make_Love) {
            duration = 0;
            isSimulateneous = true;
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(Character actor, IPointOfInterest target) {
            if(target is Character) {
                string debugLog = actor.name + " invite to make love interrupt with " + target.name;

                Character targetCharacter = target as Character;
                WeightedDictionary<string> weights = new WeightedDictionary<string>();
                int acceptWeight = 50;
                int rejectWeight = 10;
                debugLog += "\n-Base accept weight: " + acceptWeight;
                debugLog += "\n-Base reject weight: " + rejectWeight;


                int targetOpinionToActor = 0;
                if (targetCharacter.opinionComponent.HasOpinion(actor)) {
                    targetOpinionToActor = targetCharacter.opinionComponent.GetTotalOpinion(actor);
                }
                acceptWeight += (3 * targetOpinionToActor);
                debugLog += "\n-Target opinion towards Actor: +(3 x " + targetOpinionToActor + ") to Accept Weight";

                Trait trait = target.traitContainer.GetNormalTrait<Trait>("Chaste", "Lustful");
                if(trait != null) {
                    if(trait.name == "Lustful") {
                        acceptWeight += 100;
                        debugLog += "\n-Target is Lustful: +100 to Accept Weight";
                    } else {
                        rejectWeight += 100;
                        debugLog += "\n-Target is Lustful: +100 to Reject Weight";
                    }
                }

                if(targetCharacter.moodComponent.moodState == MOOD_STATE.LOW) {
                    rejectWeight += 50;
                    debugLog += "\n-Target is Low mood: +50 to Reject Weight";
                } else if (targetCharacter.moodComponent.moodState == MOOD_STATE.CRITICAL) {
                    rejectWeight += 200;
                    debugLog += "\n-Target is Crit mood: +200 to Reject Weight";
                }

                weights.AddElement("Accept", acceptWeight);
                weights.AddElement("Reject", rejectWeight);

                debugLog += "\n\n" + weights.GetWeightsSummary("FINAL WEIGHTS");

                string chosen = weights.PickRandomElementGivenWeights();
                debugLog += "\n\nCHOSEN RESPONSE: " + chosen;
                actor.logComponent.PrintLogIfActive(debugLog);


                Log log = new Log(GameManager.Instance.Today(), "Interrupt", "Invite To Make Love", chosen);
                log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                log.AddToFillers(target, target.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                actor.logComponent.RegisterLogAndShowNotifToThisCharacterOnly(log, onlyClickedCharacter: false);

                if (chosen == "Reject") {
                    actor.opinionComponent.AdjustOpinion(targetCharacter, "Base", -3, "rejected sexual advances");
                    actor.traitContainer.AddTrait(actor, "Annoyed");
                    actor.currentJob.CancelJob(false);
                    return false;
                }
            }
            return true;
        }
        #endregion
    }
}
