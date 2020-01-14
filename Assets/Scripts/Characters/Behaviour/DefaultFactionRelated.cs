﻿using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;

public class DefaultFactionRelated : CharacterBehaviourComponent {
    public DefaultFactionRelated() {
        attributes = new BEHAVIOUR_COMPONENT_ATTRIBUTE[] { BEHAVIOUR_COMPONENT_ATTRIBUTE.DO_NOT_SKIP_PROCESSING/*, BEHAVIOUR_COMPONENT_ATTRIBUTE.ONCE_PER_DAY*/ };
    }

    public override bool TryDoBehaviour(Character character, ref string log) {
        if(UnityEngine.Random.Range(0, 100) < 15) {
            if (character.isFriendlyFactionless) {
                log += "\n-" + character.name + " is factionless, 15% chance to join faction";
                List<Faction> viableFactions = new List<Faction>();
                if (character.currentRegion != null) {
                    Settlement potentialSettlement = character.currentSettlement;
                    log += "\n-" + character.name + " is factionless and in a settlement: " + potentialSettlement.name + ", will try to join a faction...";
                    Faction potentialFaction = potentialSettlement.owner;
                    if (!potentialFaction.isPlayerFaction
                        && !potentialSettlement.owner.IsCharacterBannedFromJoining(character) 
                        && potentialFaction.ideologyComponent.DoesCharacterFitCurrentIdeologies(character)) {
                        if (!viableFactions.Contains(potentialFaction)) {
                            viableFactions.Add(potentialFaction);
                        }
                    }
                } 
                if (viableFactions.Count > 0) {
                    Faction chosenFaction = viableFactions[UnityEngine.Random.Range(0, viableFactions.Count)];
                    character.interruptComponent.TriggerInterrupt(INTERRUPT.Join_Faction, chosenFaction.leader as Character, "join_faction_normal");
                    //character.ChangeFactionTo(chosenFaction);
                    log += "\n-Chosen faction to join: " + chosenFaction.name;
                } else {
                    log += "\n-No available faction that the character fits the ideology";
                }
            }
            return true;
        } else if (UnityEngine.Random.Range(0, 100) < 10) {
            if (character.isFriendlyFactionless) {
                log += "\n-" + character.name + " is factionless, 10% chance to create faction";
                if (character.traitContainer.GetNormalTrait<Trait>("Inspiring", "Ambitious") != null) {
                    log += "\n-" + character.name + " is Ambitious or Inspiring, creating new faction...";
                    character.interruptComponent.TriggerInterrupt(INTERRUPT.Create_Faction, character);
                }
            }
            return true;
        }

        return false;
    }
}
