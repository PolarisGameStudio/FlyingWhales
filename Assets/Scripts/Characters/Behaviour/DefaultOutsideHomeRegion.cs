﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;
using Inner_Maps;
using UtilityScripts;

public class DefaultOutsideHomeRegion : CharacterBehaviourComponent {
    public DefaultOutsideHomeRegion() {
        priority = 25;
    }
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
        if (!character.isAtHomeRegion) {
            log += $"\n-{character.name} is not in home region";
            TIME_IN_WORDS currentTimeOfDay = GameManager.GetCurrentTimeInWordsOfTick(null);
            log += $"\n  -Time of Day: {currentTimeOfDay}";
            if (currentTimeOfDay == TIME_IN_WORDS.MORNING || currentTimeOfDay == TIME_IN_WORDS.LUNCH_TIME || currentTimeOfDay == TIME_IN_WORDS.AFTERNOON || currentTimeOfDay == TIME_IN_WORDS.EARLY_NIGHT) {
                log += $"\n  -Morning/Lunch/Afternoon/Early Night: 35% to stroll";
                int chance = UnityEngine.Random.Range(0, 100);
                log += $"\n  -RNG roll: {chance}";
                if (chance < 35) {
                    log += $"\n  -Enter Stroll Outside State";
                    return character.jobComponent.PlanIdleStrollOutside(out producedJob);
                } else {
                    log += $"\n  -Otherwise: Return home";
                    return character.jobComponent.PlanIdleReturnHome(out producedJob);
                }
            } else {
                if(character.currentStructure != null && character.currentStructure.structureType == STRUCTURE_TYPE.TAVERN) {
                    log += $"\n  -Already in a tavern, 35% to roam";
                    int chance = UnityEngine.Random.Range(0, 100);
                    log += $"\n  -RNG roll: {chance}";
                    if (chance < 35) {
                        log += $"\n  -Roam";
                        return character.jobComponent.TriggerRoamAroundStructure(out producedJob);
                    } else {
                        log += $"\n  -Otherwise: Drink";
                        Table table = character.currentStructure.GetRandomTileObjectOfTypeThatMeetCriteria<Table>(t => t.gridTileLocation != null);
                        if(table != null) {
                            return character.jobComponent.TriggerDrinkJob(JOB_TYPE.IDLE, table, out producedJob);
                        } else {
                            log += $"\n  -No table available in tavern, stroll";
                            return character.jobComponent.PlanIdleStrollOutside(out producedJob);
                        }
                    }

                } else {
                    List<LocationStructure> taverns = character.currentRegion.GetStructuresAtLocation<LocationStructure>(STRUCTURE_TYPE.TAVERN);
                    if (taverns != null && taverns.Count > 0) {
                        log += $"\n  -Has tavern in region";
                        LocationStructure chosenTavern = null;
                        for (int i = 0; i < taverns.Count; i++) {
                            LocationStructure potentialTavern = taverns[i];
                            if (potentialTavern.settlementLocation == null || potentialTavern.settlementLocation.owner == null || character.faction == null || !potentialTavern.settlementLocation.owner.IsHostileWith(character.faction)) {
                                chosenTavern = potentialTavern;
                                break;
                            }
                        }
                        if (chosenTavern != null) {
                            log += $"\n  -Chosen tavern: " + chosenTavern.name;
                            LocationGridTile targetTile = CollectionUtilities.GetRandomElement(chosenTavern.passableTiles);
                            return character.jobComponent.CreateGoToJob(targetTile, out producedJob);
                        } else {
                            log += $"\n  -No tavern available for character, might be hostile with all the available taverns";
                        }
                    }
                }
                if (!character.currentStructure.isInterior) {
                    log += $"\n  -Character is in an exterior structure";
                    List<LocationStructure> structures = null;
                    HexTile currentHex = character.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner;
                    for (int i = 0; i < currentHex.AllNeighbours.Count; i++) {
                        HexTile hex = currentHex.AllNeighbours[i];
                        LocationGridTile centerTile = hex.GetCenterLocationGridTile();
                        //TODO: Enable digging
                        if (centerTile.structure.structureType.IsSpecialStructure() && centerTile.structure.isInterior && character.movementComponent.HasPathTo(centerTile)) {
                            if(structures == null) { structures = new List<LocationStructure>(); }
                            structures.Add(centerTile.structure);
                        }
                    }
                    if(structures != null && structures.Count > 0) {
                        log += $"\n  -Has an adjacent special structure that has a path to, go there";
                        LocationStructure specialStructureToGoTo = CollectionUtilities.GetRandomElement(structures);
                        log += $"\n  -Chosen special structure: " + specialStructureToGoTo.name;
                        LocationGridTile targetTile = CollectionUtilities.GetRandomElement(specialStructureToGoTo.passableTiles);
                        return character.jobComponent.CreateGoToJob(targetTile, out producedJob);
                    } else {
                        log += $"\n  -No adjacent special structure that has a path to";
                        if (character.currentSettlement != null) {
                            log += $"\n  -Character is inside settlement, go to adjacent plain hextile outside settlement";
                            HexTile chosenHex = character.currentSettlement.GetAPlainAdjacentHextile();
                            if(chosenHex != null) {
                                LocationGridTile targetTile = CollectionUtilities.GetRandomElement(chosenHex.locationGridTiles);
                                return character.jobComponent.CreateGoToJob(targetTile, out producedJob);
                            } else {
                                log += $"\n  -No adjacent plain hextile outside settlement, stroll";
                                return character.jobComponent.PlanIdleStrollOutside(out producedJob);
                            }
                        } else {
                            log += $"\n  -Outside settlement";
                            Campfire chosenCampfire = null;
                            List<Campfire> campfires = currentHex.GetTileObjectsInHexTile<Campfire>();
                            if(campfires != null && campfires.Count > 0) {
                                for (int i = 0; i < campfires.Count; i++) {
                                    Campfire campfire = campfires[i];
                                    if(campfire.characterOwner == null || campfire.IsOwnedBy(character) || (!character.IsHostileWith(campfire.characterOwner) && !character.relationshipContainer.IsEnemiesWith(campfire.characterOwner))){
                                        chosenCampfire = campfire;
                                        break;
                                    }
                                }
                            }
                            if(chosenCampfire != null) {
                                log += $"\n  -Has available campfire within hex, warm up: " + chosenCampfire.nameWithID + ", 25% chance to roam around";
                                int chance = UnityEngine.Random.Range(0, 100);
                                log += $"\n  -RNG roll: {chance}";
                                if(chance < 25) {
                                    log += $"\n  -Roam";
                                    return character.jobComponent.TriggerRoamAroundTile(out producedJob);
                                } else {
                                    log += $"\n  -Warm up";
                                    return character.jobComponent.TriggerWarmUp(chosenCampfire, out producedJob);
                                }
                            } else {
                                log += $"\n  -No available campfire within hex, create one";
                                return character.jobComponent.TriggerBuildCampfireJob(out producedJob);
                            }
                        }
                    }
                } else {
                    log += $"\n  -Character is in an interior structure: 35% to roam";
                    int chance = UnityEngine.Random.Range(0, 100);
                    log += $"\n  -RNG roll: {chance}";
                    if (chance < 35) {
                        log += $"\n  -Roam";
                        return character.jobComponent.TriggerRoamAroundStructure(out producedJob);
                    } else {
                        log += $"\n  -Otherwise: Stand";
                        return character.jobComponent.TriggerStand(out producedJob);
                    }
                }
            }
        }
        return false;
    }
}
