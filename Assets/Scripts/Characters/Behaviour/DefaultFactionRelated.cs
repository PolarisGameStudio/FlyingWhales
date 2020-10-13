﻿using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;
using UtilityScripts;

public class DefaultFactionRelated : CharacterBehaviourComponent {
    public DefaultFactionRelated() {
        priority = 26;
        attributes = new BEHAVIOUR_COMPONENT_ATTRIBUTE[] { BEHAVIOUR_COMPONENT_ATTRIBUTE.DO_NOT_SKIP_PROCESSING/*, BEHAVIOUR_COMPONENT_ATTRIBUTE.ONCE_PER_DAY*/ };
    }

    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
        if (character.isVagrantOrFactionless) {
            if(UnityEngine.Random.Range(0, 100) < 15) {
                log += $"\n-{character.name} is factionless, 15% chance to join faction";
                Faction chosenFaction = character.JoinFactionProcessing();
                if (chosenFaction != null) {
                    log += $"\n-Chosen faction to join: {chosenFaction.name}";
                } else {
                    log += "\n-No available faction that the character fits the ideology";
                }
                return true;
            } else {
                log += $"\nDid not meet chance to join faction. Checking if can create faction...";
                int villagerFactionCount = FactionManager.Instance.GetActiveVillagerFactionCount();
                log += $"\nActive villager faction count is {villagerFactionCount.ToString()}";
                if (villagerFactionCount < 20) {
                    log += $"\nActive villager factions is less than 20, rolling chances";
                    int createChance = 3;
                    if (character.traitContainer.HasTrait("Inspiring", "Ambitious")) {
                        createChance = 15;
                    }
                    if (GameUtilities.RollChance(createChance, ref log)) {
                        log += $"\nChance met, creating new faction";
                        character.interruptComponent.TriggerInterrupt(INTERRUPT.Create_Faction, character);
                        return true;
                    }
                }
            }
        }
        if(character.faction != null && character.faction.isMajorNonPlayer && !character.isFactionLeader && !character.isSettlementRuler) {
            int leaveFactionChance = 0;
            if(character.moodComponent.moodState == MOOD_STATE.Bad) {
                leaveFactionChance += 3;
            } else if (character.moodComponent.moodState == MOOD_STATE.Critical) {
                leaveFactionChance += 8;
            }
            if (character.traitContainer.HasTrait("Betrayed") && character.faction.leader != null) {
                Betrayed betrayed = character.traitContainer.GetTraitOrStatus<Betrayed>("Betrayed");
                if(betrayed.responsibleCharacter == character.faction.leader) {
                    leaveFactionChance += 30;
                }
            }
            if (GameUtilities.RollChance(leaveFactionChance)) {
                character.interruptComponent.TriggerInterrupt(INTERRUPT.Leave_Faction, character, "left_faction_normal");
            }
            for (int i = 0; i < character.crimeComponent.witnessedCrimes.Count; i++) {
                CrimeData crimeData = character.crimeComponent.witnessedCrimes[i];
                if (!crimeData.isRemoved) {
                    if (!character.crimeComponent.IsReported(crimeData)) {
                        if (character.jobComponent.TryCreateReportCrimeJob(crimeData.criminal, crimeData.target, crimeData, crimeData.crime)) {
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }
}
