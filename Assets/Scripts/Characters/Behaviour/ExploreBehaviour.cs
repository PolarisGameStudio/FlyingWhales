﻿using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;

public class ExploreBehaviour : CharacterBehaviourComponent {
    public ExploreBehaviour() {
        priority = 450;
    }
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
        log += $"\n-Character is exploring";
        Party exploreParty = character.partyComponent.currentParty;
        if (!exploreParty.isWaitTimeOver) {
            log += $"\n-Party is waiting";
            if (character.homeSettlement != null) {
                log += $"\n-Character has home settlement";
                if (character.homeSettlement.locationType == LOCATION_TYPE.DUNGEON) {
                    log += $"\n-Character home settlement is a special structure";
                    character.jobComponent.TriggerRoamAroundStructure(out producedJob);
                } else {
                    log += $"\n-Character home settlement is a village";
                    LocationStructure targetStructure = null;
                    if(character.currentStructure.structureType == STRUCTURE_TYPE.INN) {
                        targetStructure = character.currentStructure;
                    } else {
                        targetStructure = character.homeSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.INN);
                    }
                    if (targetStructure == null) {
                        if (character.currentStructure.structureType == STRUCTURE_TYPE.CITY_CENTER) {
                            targetStructure = character.currentStructure;
                        } else {
                            targetStructure = character.homeSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.CITY_CENTER);
                        }
                    }

                    if(targetStructure != null) {
                        log += $"\n-Character will roam around " + targetStructure.name;
                        LocationGridTile targetTile = null;
                        if (character.currentStructure != targetStructure) {
                            targetTile = UtilityScripts.CollectionUtilities.GetRandomElement(targetStructure.passableTiles);
                        }
                        character.jobComponent.TriggerRoamAroundStructure(out producedJob, targetTile);
                    }
                }
            }
        } else {
            log += $"\n-Party is not waiting";
            if(character.currentStructure == exploreParty.target) {
                log += $"\n-Character is already in target structure";
                Character memberInCombat = exploreParty.GetMemberInCombatExcept(character);
                if (memberInCombat != null && memberInCombat.currentStructure == exploreParty.target) {
                    if (!character.marker.inVisionCharacters.Contains(memberInCombat)) {
                        log += $"\n-There is a party member in combat inside explore structure, go to it";
                        character.jobComponent.CreatePartyGoToJob(memberInCombat.gridTileLocation, out producedJob);
                    } else {
                        log += $"\n-Roam around";
                        character.jobComponent.TriggerRoamAroundStructure(out producedJob);
                    }
                } else {
                    log += $"\n-Roam around";
                    character.jobComponent.TriggerRoamAroundStructure(out producedJob);
                }
            } else {
                log += $"\n-Character is not in target structure, go to it";
                if (exploreParty.target is LocationStructure targetStructure) {
                    LocationGridTile targetTile = UtilityScripts.CollectionUtilities.GetRandomElement(targetStructure.passableTiles);
                    character.jobComponent.CreatePartyGoToJob(targetTile, out producedJob);
                }
            }
        }
        return true;
    }
}
