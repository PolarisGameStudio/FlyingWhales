﻿using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class DefaultHomeless : CharacterBehaviourComponent {
    public DefaultHomeless() {
        priority = 26;
        attributes = new BEHAVIOUR_COMPONENT_ATTRIBUTE[] { BEHAVIOUR_COMPONENT_ATTRIBUTE.DO_NOT_SKIP_PROCESSING/*, BEHAVIOUR_COMPONENT_ATTRIBUTE.ONCE_PER_DAY*/ };
    }

    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
        if (character.homeStructure == null || character.homeStructure.hasBeenDestroyed) {
            log += $"\n-{character.name} is homeless, 25% chance to find home";
            if (GameUtilities.RollChance(25)) {
                log += $"\n-Character will try to set home";
                character.interruptComponent.TriggerInterrupt(INTERRUPT.Set_Home, null);
                return true;
            }
        }
        if(character.characterClass.className != "Vampire Lord") {
            if (GameUtilities.RollChance(5)) {
                if (character.homeStructure != null) {
                    if(!(character.homeStructure is Dwelling) && !character.isVagrantOrFactionless) {
                        log += $"\n-{character.name} has a home but his home is not a house and character is not vagrant, 5% chance to find home";
                        log += $"\n-Character will try to set home";
                        character.interruptComponent.TriggerInterrupt(INTERRUPT.Set_Home, null);
                        return true;
                    } else if (character.homeStructure.structureType == STRUCTURE_TYPE.VAMPIRE_CASTLE) {
                        bool hasSpecialRelationshipWithVampireLord = false;
                        for (int i = 0; i < character.homeStructure.residents.Count; i++) {
                            Character resident = character.homeStructure.residents[i];
                            if(character != resident && resident.characterClass.className == "Vampire Lord") {
                                if (character.relationshipContainer.HasSpecialPositiveRelationshipWith(resident)) {
                                    hasSpecialRelationshipWithVampireLord = true;
                                    break;
                                }
                            }
                        }
                        if (!hasSpecialRelationshipWithVampireLord) {
                            log += $"\n-{character.name} has a home but his home is vampire castle and has no special relationship with a vampire lord resident";
                            log += $"\n-Character will try to set home";
                            character.interruptComponent.TriggerInterrupt(INTERRUPT.Set_Home, null);
                            return true;
                        }
                    }

                }
            }
        }
        return false;
    }
}
