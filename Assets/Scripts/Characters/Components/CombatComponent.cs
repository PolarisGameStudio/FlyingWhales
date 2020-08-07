﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;
using Inner_Maps;

public class CombatComponent {
	public Character owner { get; private set; }
    public int attack { get; private set; }
    public int attackModification { get; private set; }
    public int maxHP { get; private set; }
    public int maxHPModification { get; private set; }
    public int attackSpeed { get; private set; }  //in milliseconds, The lower the amount the faster the attack rate

    public COMBAT_MODE combatMode { get; private set; }
    public List<IPointOfInterest> hostilesInRange { get; private set; } //POI's in this characters hostility collider
    public List<IPointOfInterest> avoidInRange { get; private set; } //POI's in this characters hostility collider
    public List<IPointOfInterest> bannedFromHostileList { get; private set; }
    public Dictionary<IPointOfInterest, CombatData> fightCombatData { get; private set; }
    //public string avoidReason { get; private set; }
    public ElementalDamageData elementalDamage { get; private set; }
    //public ActualGoapNode actionThatTriggeredCombatState { get; private set; }
    //public GoapPlanJob jobThatTriggeredCombatState { get; private set; }
    // public ActualGoapNode combatConnectedActionNode { get; private set; }

    //delegates
    public delegate void OnProcessCombat(CombatState state);
    private OnProcessCombat onProcessCombat; //actions to be executed and cleared when a character processes combat.

    private bool _willProcessCombat;

    #region getters
    public bool isInCombat => owner.stateComponent.currentState != null && owner.stateComponent.currentState.characterState == CHARACTER_STATE.COMBAT;
    public bool isInActualCombat => IsInActualCombat();
    public int unModifiedMaxHP => Mathf.RoundToInt(owner.characterClass.baseHP *
                                                   (owner.raceSetting.hpMultiplier == 0f ? 
                                                       1f : owner.raceSetting.hpMultiplier));
    public int unModifiedAttack => Mathf.RoundToInt(owner.characterClass.baseAttackPower *
                                                    (owner.raceSetting.attackMultiplier == 0f
                                                        ? 1f : owner.raceSetting.attackMultiplier));
    #endregion

    public CombatComponent(Character owner) {
		this.owner = owner;
        hostilesInRange = new List<IPointOfInterest>();
        avoidInRange = new List<IPointOfInterest>();
        bannedFromHostileList = new List<IPointOfInterest>();
        fightCombatData = new Dictionary<IPointOfInterest, CombatData>();
        SetCombatMode(COMBAT_MODE.Aggressive);
        SetElementalType(ELEMENTAL_TYPE.Normal);
        //UpdateBasicData(true);
	}


    #region General
    public void OnThisCharacterEndedCombatState() {
        SetOnProcessCombatAction(null);
    }
    private void ProcessCombatBehavior() {
        string log = $"{owner.name} process combat switch is turned on, processing combat...";
        if (owner.interruptComponent.isInterrupted) {
            log +=
                $"\n-Character is interrupted: {owner.interruptComponent.currentInterrupt.name}, will not process combat";
        } else {
            if (owner.combatComponent.isInCombat) {
                log += "\n-Character is already in combat, determining combat action to do";
                Messenger.Broadcast(Signals.DETERMINE_COMBAT_REACTION, owner);
            } else {
                log += "\n-Character is not in combat, will add Combat job if there is a hostile or avoid in range";
                if (hostilesInRange.Count > 0 || avoidInRange.Count > 0) {
                    if (!owner.jobQueue.HasJob(JOB_TYPE.COMBAT)) {
                        log += "\n-No existing combat job, Combat job added";
                        CharacterStateJob job = JobManager.Instance.CreateNewCharacterStateJob(JOB_TYPE.COMBAT, CHARACTER_STATE.COMBAT, owner);
                        owner.jobQueue.AddJobInQueue(job);
                    } else {
                        log += "\n-Has existing combat job, no combat job added";
                    }
                } else {
                    log += "\n-Combat job not added";
                    if (owner.marker != null && owner.marker.hasFleePath && owner.combatComponent.isInCombat) {
                        CombatState combatState = owner.stateComponent.currentState as CombatState;
                        combatState.CheckFlee(ref log);
                    }
                }
            }
            //avoidReason = string.Empty;
        }
        owner.logComponent.PrintLogIfActive(log);
        //execute any external combat actions. This assumes that this character entered combat state.

        //NOTE: Commented this out temporarily because we no longer immediately switch the state of the character to combat, instead we create a job and add it to its job queue
        //This means that the character will always have a null current state in this tick
        //onProcessCombat?.Invoke(owner.stateComponent.currentState as CombatState);
        //SetOnProcessCombatAction(null);
    }
    public void AddOnProcessCombatAction(OnProcessCombat action) {
        onProcessCombat += action;
    }
    public void SetOnProcessCombatAction(OnProcessCombat action) {
        onProcessCombat = action;
    }
    public void CheckCombatPerTickEnded() {
        if (_willProcessCombat) {
            SetWillProcessCombat(false); //Moved this up here, because ProcessCombatBehavior can set process combat to true again, and we don't want to overwrite that.
            ProcessCombatBehavior();
        }
    }
    public void SetCombatMode(COMBAT_MODE mode) {
        combatMode = mode;
    }
    public void SetElementalType(ELEMENTAL_TYPE elementalType) {
        elementalDamage = ScriptableObjectsManager.Instance.GetElementalDamageData(elementalType);
    }
    public void UpdateElementalType() {
        bool hasSetElementalType = false;
        for (int i = (owner.traitContainer.traits.Count - 1); i >= 0; i--) {
            Trait currTrait = owner.traitContainer.traits[i];
            if (currTrait.elementalType != ELEMENTAL_TYPE.Normal) {
                SetElementalType(currTrait.elementalType);
                hasSetElementalType = true;
                break;
            }
        }
        if (!hasSetElementalType) {
            SetElementalType(owner.characterClass.elementalType);
        }
    }
    //public void SetActionAndJobThatTriggeredCombat(ActualGoapNode node, GoapPlanJob job) {
    //    actionThatTriggeredCombatState = node;
    //    jobThatTriggeredCombatState = job;
    //}
    public void SetWillProcessCombat(bool state) {
        _willProcessCombat = state;
    }
    private bool IsInActualCombat() {
        if(owner.marker && owner.stateComponent.currentState != null && owner.stateComponent.currentState is CombatState combatState) {
            if (combatState.isAttacking) {
                //Only become in "actual" combat in attacking mode if character is already in vision of the target or if the target is fleeing
                if(combatState.currentClosestHostile != null) {
                    if (owner.marker.inVisionPOIs.Contains(combatState.currentClosestHostile)) {
                        return true;
                    } else if (owner.marker.inVisionPOIsButDiffStructure.Contains(combatState.currentClosestHostile)) {
                        return true;
                    } else if (combatState.currentClosestHostile is Character currentHostileCharacter && currentHostileCharacter.combatComponent.isInCombat) {
                        CombatState combatStateOfCurrentHostile = currentHostileCharacter.stateComponent.currentState as CombatState;
                        if (!combatStateOfCurrentHostile.isAttacking) {
                            return true;
                        }
                    }
                }
            } else {
                //If character is fleeing, he/she is always in actual combat
                return true;
            }
        }
        return false;
    }
    public bool IsInActualCombatWith(IPointOfInterest target) {
        if (owner.marker && owner.stateComponent.currentState != null && owner.stateComponent.currentState is CombatState combatState) {
            if (combatState.isAttacking) {
                //Only become in "actual" combat in attacking mode if character is already in vision of the target or if the target is fleeing
                if (combatState.currentClosestHostile != null && combatState.currentClosestHostile == target) {
                    if(combatState.currentClosestHostile is Character hostileCharacter) {
                        if (owner.marker.inVisionCharacters.Contains(hostileCharacter)) {
                            return true;
                        }
                    } else if (combatState.currentClosestHostile is TileObject hostileTileObject) {
                        if (owner.marker.inVisionTileObjects.Contains(hostileTileObject)) {
                            return true;
                        }
                    }
                    if (owner.marker.inVisionPOIsButDiffStructure.Contains(combatState.currentClosestHostile)) {
                        return true;
                    }
                }
            }
        }
        return false;
    }
    #endregion

    #region Fight or Flight
    public CombatReaction GetFightOrFlightReaction(IPointOfInterest target, string fightReason) {
        string debugLog = $"FIGHT or FLIGHT response of {owner.name} against {target.nameWithID}";
        if (!owner.canPerform || !owner.canMove) {
            debugLog += "\n-Character cannot move/perform, will not fight or flight";
            owner.logComponent.PrintLogIfActive(debugLog);
            return new CombatReaction(COMBAT_REACTION.None);
        }
        if (hostilesInRange.Contains(target) || avoidInRange.Contains(target)) {
            debugLog += "\n-Target is already in hostile/avoid list, will no longer trigger fight or flight";
            owner.logComponent.PrintLogIfActive(debugLog);
            return new CombatReaction(COMBAT_REACTION.None);
        }
        if (owner.faction == FactionManager.Instance.undeadFaction || owner.race == RACE.SKELETON) {
            debugLog += "\n-Character is zombie";
            debugLog += "\n-FIGHT";
            owner.logComponent.PrintLogIfActive(debugLog);
            return new CombatReaction(COMBAT_REACTION.Fight, fightReason);
        }
        if (owner.behaviourComponent.HasBehaviour(typeof(DisablerBehaviour))) {
            debugLog += "\n-Character is a Disabler";
            debugLog += "\n-FLIGHT";
            owner.logComponent.PrintLogIfActive(debugLog);
            return new CombatReaction(COMBAT_REACTION.Flight);
        }
        if (target is Character) {
            debugLog += "\n-Target is character";
            Character targetCharacter = target as Character;
            if (owner.traitContainer.HasTrait("Coward")) {
                debugLog += "\n-Character is coward";
                if(owner.race == RACE.DEMON || owner.race == RACE.DRAGON) {
                    debugLog += "\n-Character is a demon";
                    debugLog += "\n-FIGHT";
                    owner.logComponent.PrintLogIfActive(debugLog);
                    return new CombatReaction(COMBAT_REACTION.Fight, fightReason);
                } else {
                    debugLog += "\n-FLIGHT";
                    owner.logComponent.PrintLogIfActive(debugLog);
                    return new CombatReaction(COMBAT_REACTION.Flight, "character is a coward");
                }
            } else {
                debugLog += "\n-Character is not coward";
                if (!owner.traitContainer.HasTrait("Combatant", "Royalty")) {
                    debugLog += "\n-Character is not combatant, 20% to Fight";
                    int chance = UnityEngine.Random.Range(0, 100);
                    debugLog += $"\n-Roll: {chance}";
                    if (chance < 20) {
                        debugLog += "\n-FIGHT";
                        owner.logComponent.PrintLogIfActive(debugLog);
                        return new CombatReaction(COMBAT_REACTION.Fight, fightReason);
                    } else {
                        if (owner.race == RACE.DEMON || owner.race == RACE.DRAGON) {
                            debugLog += "\n-Character is a demon";
                            debugLog += "\n-FIGHT";
                            owner.logComponent.PrintLogIfActive(debugLog);
                            return new CombatReaction(COMBAT_REACTION.Fight, fightReason);
                        } else {
                            debugLog += "\n-FLIGHT";
                            owner.logComponent.PrintLogIfActive(debugLog);
                            return new CombatReaction(COMBAT_REACTION.Flight, "got scared");
                        }
                    }
                } else {
                    debugLog += "\n-Character is combatant or royalty";
                    if (owner.currentHP > targetCharacter.currentHP) {
                        debugLog += "\n-Character hp is higher than target";
                        debugLog += "\n-FIGHT";
                        owner.logComponent.PrintLogIfActive(debugLog);
                        return new CombatReaction(COMBAT_REACTION.Fight, fightReason);
                    } else {
                        debugLog += "\n-Character hp is lower or equal than target";
                        if (CombatManager.Instance.IsImmuneToElement(targetCharacter, elementalDamage.type)) {
                            debugLog += "\n-Target is immune to character elemental damage";
                            if (owner.race == RACE.DEMON || owner.race == RACE.DRAGON) {
                                debugLog += "\n-Character is a demon";
                                debugLog += "\n-FIGHT";
                                owner.logComponent.PrintLogIfActive(debugLog);
                                return new CombatReaction(COMBAT_REACTION.Fight, fightReason);
                            } else {
                                debugLog += "\n-FLIGHT";
                                owner.logComponent.PrintLogIfActive(debugLog);
                                return new CombatReaction(COMBAT_REACTION.Flight, "got scared");
                            }
                        } else if (CombatManager.Instance.IsImmuneToElement(owner, targetCharacter.combatComponent.elementalDamage.type)) {
                            debugLog += "\n-Character is immune to target elemental damage";
                            debugLog += "\n-FIGHT";
                            owner.logComponent.PrintLogIfActive(debugLog);
                            return new CombatReaction(COMBAT_REACTION.Fight, fightReason);
                        } else {
                            if (owner.currentHP >= Mathf.CeilToInt(owner.maxHP * 0.3f)) {
                                debugLog += "\n-Character's hp is greater than or equal to 30% of its max hp";
                                debugLog += "\n-FIGHT";
                                owner.logComponent.PrintLogIfActive(debugLog);
                                return new CombatReaction(COMBAT_REACTION.Fight, fightReason);
                            } else {
                                int fightChance = 25;
                                for (int i = 0; i < owner.marker.inVisionCharacters.Count; i++) {
                                    if (owner.marker.inVisionCharacters[i].combatComponent.hostilesInRange.Contains(target)) {
                                        debugLog += "\n-Character has another character in vision who has the same target";
                                        fightChance = 75;
                                        break;
                                    }
                                }
                                debugLog += $"\n-Fight chance: {fightChance}";
                                int roll = UnityEngine.Random.Range(0, 100);
                                debugLog += $"\n-Roll: {roll}";
                                if (roll < fightChance) {
                                    debugLog += "\n-FIGHT";
                                    owner.logComponent.PrintLogIfActive(debugLog);
                                    return new CombatReaction(COMBAT_REACTION.Fight, fightReason);
                                } else {
                                    if (owner.race == RACE.DEMON || owner.race == RACE.DRAGON) {
                                        debugLog += "\n-Character is a demon";
                                        debugLog += "\n-FIGHT";
                                        owner.logComponent.PrintLogIfActive(debugLog);
                                        return new CombatReaction(COMBAT_REACTION.Fight, fightReason);
                                    } else {
                                        debugLog += "\n-FLIGHT";
                                        owner.logComponent.PrintLogIfActive(debugLog);
                                        return new CombatReaction(COMBAT_REACTION.Flight, "got scared");
                                    }
                                }
                            }
                        }
                    }
                }
            }
        } else if (target is TileObject tileObject) {
            debugLog += "\n-Target is object";
            if (owner.traitContainer.HasTrait("Coward")) {
                debugLog += "\n-Character is coward";
                if (owner.race == RACE.DEMON || owner.race == RACE.DRAGON) {
                    debugLog += "\n-Character is a demon";
                    debugLog += "\n-FIGHT";
                    owner.logComponent.PrintLogIfActive(debugLog);
                    return new CombatReaction(COMBAT_REACTION.Fight, fightReason);
                } else {
                    debugLog += "\n-FLIGHT";
                    owner.logComponent.PrintLogIfActive(debugLog);
                    return new CombatReaction(COMBAT_REACTION.Flight, "character is a coward");
                }
            } else if (tileObject.traitContainer.HasTrait("Dangerous")) {
                debugLog += "\n-Object is dangerous";
                if (string.IsNullOrEmpty(tileObject.neutralizer) == false && 
                    owner.traitContainer.HasTrait(tileObject.neutralizer)) {
                    debugLog += $"\n-Character has neutralizer trait {tileObject.neutralizer}";
                    owner.logComponent.PrintLogIfActive(debugLog);
                    return new CombatReaction(COMBAT_REACTION.Fight, fightReason);
                } else {
                    if (owner.race == RACE.DEMON || owner.race == RACE.DRAGON) {
                        debugLog += "\n-Character is a demon";
                        debugLog += "\n-FIGHT";
                        owner.logComponent.PrintLogIfActive(debugLog);
                        return new CombatReaction(COMBAT_REACTION.Fight, fightReason);
                    } else {
                        debugLog += "\n-FLIGHT";
                        owner.logComponent.PrintLogIfActive(debugLog);
                        return new CombatReaction(COMBAT_REACTION.Flight, "got scared");
                    }
                }
            } else {
                debugLog += "\n-Object is not dangerous";
                owner.logComponent.PrintLogIfActive(debugLog);
                return new CombatReaction(COMBAT_REACTION.Fight, fightReason);
            }
        }
        owner.logComponent.PrintLogIfActive(debugLog);
        return new CombatReaction(COMBAT_REACTION.None);
    }
    public void FightOrFlight(IPointOfInterest target, CombatReaction combatReaction, 
        ActualGoapNode connectedAction = null, bool isLethal = true) {
        if (combatReaction.reaction == COMBAT_REACTION.Fight) {
            Fight(target, combatReaction.reason, connectedAction, isLethal);
        } else if (combatReaction.reaction == COMBAT_REACTION.Flight) {
            if (owner.movementComponent.isStationary) {
                owner.logComponent.PrintLogIfActive($"Supposed to FLIGHT for {owner.name} against {target.nameWithID} but character is STATIONARY, fight insted");
                Fight(target, combatReaction.reason, connectedAction, isLethal);
            } else {
                Flight(target, combatReaction.reason);
            }
        }
    }
    public void FightOrFlight(IPointOfInterest target, string fightReason, ActualGoapNode connectedAction = null, bool isLethal = true) {
        CombatReaction combatReaction = GetFightOrFlightReaction(target, fightReason);
        FightOrFlight(target, combatReaction, connectedAction, isLethal);
    }
    public bool Fight(IPointOfInterest target, string reason, ActualGoapNode connectedAction = null, bool isLethal = true) {
        bool hasFought = false;
        bool cannotFight = reason == CombatManager.Hostility && bannedFromHostileList.Contains(target);
        if (!hostilesInRange.Contains(target) && !cannotFight) {
            string debugLog = $"Triggered FIGHT response for {owner.name} against {target.nameWithID}";
            hostilesInRange.Add(target);
            avoidInRange.Remove(target);
            SetWillProcessCombat(true);

            //CombatData newCombatData = ObjectPoolManager.Instance.CreateNewCombatData();
            //newCombatData.SetFightData(reason, connectedAction, isLethal);
            if (fightCombatData.ContainsKey(target)) {
                //CombatData prevCombatData = fightCombatData[target];
                //ObjectPoolManager.Instance.ReturnCombatDataToPool(prevCombatData);
                //fightCombatData[target] = newCombatData;
                fightCombatData[target].SetFightData(reason, connectedAction, isLethal);
            } else {
                CombatData newCombatData = ObjectPoolManager.Instance.CreateNewCombatData();
                newCombatData.SetFightData(reason, connectedAction, isLethal);
                fightCombatData.Add(target, newCombatData);
            }

            if (target is TileObject targetTileObject) {
                targetTileObject.AdjustRepairCounter(1);
            }
            target.CancelRemoveStatusFeedAndRepairJobsTargetingThis();
            debugLog += $"\n{target.name} was added to {owner.name}'s hostile range!";
            hasFought = true;
            owner.logComponent.PrintLogIfActive(debugLog);
        }
        return hasFought;
    }
    public bool Flight(IPointOfInterest target, string reason = "") {
        if (owner.movementComponent.isStationary) {
            owner.logComponent.PrintLogIfActive($"Triggered FLIGHT response for {owner.name} against {target.nameWithID} but character is STATIONARY, cannot flee");
            return false;
        }
        bool hasFled = false;
        if (hostilesInRange.Remove(target)) {
            //if (target.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
            //    fightCombatData.Remove(target as Character);
            //} else 
            if (target is TileObject targetTileObject) {
                targetTileObject.AdjustRepairCounter(-1);
            } else if (target is Character targetCharacter) {
                AddPOIToBannedFromHostile(targetCharacter);
                //if (fightCombatData.ContainsKey(targetCharacter) && fightCombatData[targetCharacter].reasonForCombat == CombatManager.Hostility) {
                //    AddPOIToBannedFromHostile(targetCharacter);
                //}
            }
        }
        if (!avoidInRange.Contains(target)) {
            string debugLog = $"Triggered FLIGHT response for {owner.name} against {target.nameWithID}";
            if (owner.marker.inVisionPOIs.Contains(target)) {
                avoidInRange.Add(target);
                SetWillProcessCombat(true);

                if (fightCombatData.ContainsKey(target)) {
                    fightCombatData[target].SetFlightData(reason);
                } else {
                    CombatData newCombatData = ObjectPoolManager.Instance.CreateNewCombatData();
                    newCombatData.SetFlightData(reason);
                    fightCombatData.Add(target, newCombatData);
                }

                debugLog += $"\n{target.name} was added to {owner.name}'s avoid range!";
                hasFled = true;
                if (target is Character) {
                    Character targetCharacter = target as Character;
                    if (targetCharacter.combatComponent.combatMode == COMBAT_MODE.Defend) {
                        targetCharacter.combatComponent.RemoveHostileInRange(owner);
                    }
                }
            }
            owner.logComponent.PrintLogIfActive(debugLog);
        }
        return hasFled;
    }
    public void FlightAll(string reason = "") {
        if (owner.movementComponent.isStationary) {
            owner.logComponent.PrintLogIfActive($"Triggered FLIGHT ALL response for {owner.name} but character is STATIONARY, cannot flee");
            return;
        }
        //Demons no longer trigger Flight
        //https://trello.com/c/D4bdwPhH/1104-demons-and-monsters-no-longer-trigger-flight
        if (owner.race == RACE.DEMON || owner.race == RACE.DRAGON) {
            return;
        }
        if (hostilesInRange.Count > 0) {
            if (owner.canMove) {
                for (int i = 0; i < hostilesInRange.Count; i++) {
                    IPointOfInterest hostile = hostilesInRange[i];
                    if (owner.marker.inVisionPOIs.Contains(hostile)) {
                        avoidInRange.Add(hostile);

                        if (fightCombatData.ContainsKey(hostile)) {
                            fightCombatData[hostile].SetFlightData(reason);
                        } else {
                            CombatData newCombatData = ObjectPoolManager.Instance.CreateNewCombatData();
                            newCombatData.SetFlightData(reason);
                            fightCombatData.Add(hostile, newCombatData);
                        }

                        if (hostile is Character) {
                            Character targetCharacter = hostile as Character;
                            if (targetCharacter.combatComponent.combatMode == COMBAT_MODE.Defend) {
                                targetCharacter.combatComponent.RemoveHostileInRange(owner);
                            }
                        }
                    }
                }
            }
            ClearHostilesInRange(false);
            SetWillProcessCombat(true);
            //avoidReason = reason;
        }
    }
    #endregion

    #region Hostiles
    //private bool AddHostileInRange(IPointOfInterest poi, bool processCombatBehaviour = true, bool isLethal = true) {
    //    //Not yet applicable
    //    //if (!hostilesInRange.Contains(poi)) {
    //    //    hostilesInRange.Add(poi);
    //    //    if (poi.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
    //    //        lethalCharacters.Add(poi as Character, isLethal);
    //    //    }
    //    //    owner.logComponent.PrintLogIfActive(poi.name + " was added to " + owner.name + "'s hostile range!");
    //    //    willProcessCombat = true;
    //    //}
    //    return false;
    //}
    public bool RemoveHostileInRange(IPointOfInterest poi, bool processCombatBehavior = true) {
        if (hostilesInRange.Remove(poi)) {
            //if (poi is Character character) {
            //    fightCombatData.Remove(character);
            //} else 
            if (poi is TileObject targetTileObject) {
                targetTileObject.AdjustRepairCounter(-1);
            } else if (poi is Character targetCharacter) {
                AddPOIToBannedFromHostile(targetCharacter);
                //if (fightCombatData.ContainsKey(targetCharacter) && fightCombatData[targetCharacter].reasonForCombat == CombatManager.Hostility) {
                //    AddPOIToBannedFromHostile(targetCharacter);
                //}
            }
            string removeHostileSummary = $"{poi.name} was removed from {owner.name}'s hostile range.";
            owner.logComponent.PrintLogIfActive(removeHostileSummary);
            //When removing hostile in range, check if character is still in combat state, if it is, reevaluate combat behavior, if not, do nothing
            if (processCombatBehavior) {
                if (owner.combatComponent.isInCombat) {
                    CombatState combatState = owner.stateComponent.currentState as CombatState;
                    if (combatState.forcedTarget == poi) {
                        combatState.SetForcedTarget(null);
                    }
                    if (combatState.currentClosestHostile == poi) {
                        combatState.ResetClosestHostile();
                    }
                    //Messenger.Broadcast(Signals.DETERMINE_COMBAT_REACTION, owner);
                }
                SetWillProcessCombat(true);
            }
            return true;
        }
        return false;
    }
    public void ClearHostilesInRange(bool processCombatBehavior = true) {
        if (hostilesInRange.Count > 0) {
            for (int i = 0; i < hostilesInRange.Count; i++) {
                if (hostilesInRange[i] is TileObject targetTileObject) {
                    targetTileObject.AdjustRepairCounter(-1);
                } else if (hostilesInRange[i] is Character targetCharacter) {
                    AddPOIToBannedFromHostile(targetCharacter);
                    //if (fightCombatData.ContainsKey(targetCharacter) && fightCombatData[targetCharacter].reasonForCombat == CombatManager.Hostility) {
                    //    AddPOIToBannedFromHostile(targetCharacter);
                    //}
                }
            }
            hostilesInRange.Clear();
            //fightCombatData.Clear();
            //When adding hostile in range, check if character is already in combat state, if it is, only reevaluate combat behavior, if not, enter combat state
            if (processCombatBehavior) {
                //if (owner.combatComponent.isInCombat) {
                //    Messenger.Broadcast(Signals.DETERMINE_COMBAT_REACTION, owner);
                //}
                SetWillProcessCombat(true);
            }
        }
    }
    public bool IsLethalCombatForTarget(Character character) {
        if (fightCombatData.ContainsKey(character)) {
            return fightCombatData[character].isLethal;
        }
        return true;
    }
    public bool HasLethalCombatTarget() {
        for (int i = 0; i < hostilesInRange.Count; i++) {
            IPointOfInterest poi = hostilesInRange[i];
            if (poi is Character) {
                Character hostile = poi as Character;
                if (IsLethalCombatForTarget(hostile)) {
                    return true;
                }
            }

        }
        return false;
    }
    public IPointOfInterest GetNearestValidHostile() {
        IPointOfInterest nearest = null;
        float nearestDist = 9999f;
        //first check only the hostiles that are in the same npcSettlement as this character
        for (int i = 0; i < hostilesInRange.Count; i++) {
            IPointOfInterest poi = hostilesInRange[i];
            if (poi.IsValidCombatTargetFor(owner)) {
                float dist = Vector2.Distance(owner.marker.transform.position, poi.worldPosition);
                if (nearest == null || dist < nearestDist) {
                    nearest = poi;
                    nearestDist = dist;
                }
            } else {
                //If poi in the list is no longer a valid combat target, remove it because there is no sense in keeping it in the list if you can't attack it
                if(RemoveHostileInRange(poi, false)) {
                    i--;
                }
            }
        }
        //if no character was returned, choose at random from the list, since we are sure that all characters in the list are not in the same npcSettlement as this character
        if (nearest == null) {
            //List<Character> hostileCharacters = hostilesInRange.Where(x => x.poiType == POINT_OF_INTEREST_TYPE.CHARACTER).Select(x => x as Character).ToList();
            //if (hostileCharacters.Count > 0) {
            //    nearest = hostileCharacters[UnityEngine.Random.Range(0, hostileCharacters.Count)];
            //}
            for (int i = 0; i < hostilesInRange.Count; i++) {
                IPointOfInterest poi = hostilesInRange[i];
                if(poi.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
                    if (poi.IsValidCombatTargetFor(owner)) {
                        nearest = hostilesInRange[i];
                        break;
                    } else {
                        //If poi in the list is no longer a valid combat target, remove it because there is no sense in keeping it in the list if you can't attack it
                        if (RemoveHostileInRange(poi, false)) {
                            i--;
                        }
                    }
                }
            }
        }
        return nearest;
    }
    public IPointOfInterest GetNearestValidHostilePriorityNotFleeing() {
        IPointOfInterest nearest = null;
        float nearestDist = 9999f;
        //first check only the hostiles that are in the same npcSettlement as this character
        for (int i = 0; i < hostilesInRange.Count; i++) {
            IPointOfInterest poi = hostilesInRange[i];
            if (poi.IsValidCombatTargetFor(owner)) {
                if(poi is Character hostileCharacter) {
                    if(hostileCharacter.combatComponent.isInCombat && (hostileCharacter.stateComponent.currentState as CombatState).isAttacking == false) {
                        continue;
                    }
                }
                float dist = Vector2.Distance(owner.marker.transform.position, poi.worldPosition);
                if (nearest == null || dist < nearestDist) {
                    nearest = poi;
                    nearestDist = dist;
                }
            } else {
                //If poi in the list is no longer a valid combat target, remove it because there is no sense in keeping it in the list if you can't attack it
                if (RemoveHostileInRange(poi, false)) {
                    i--;
                }
            }
        }
        if(nearest != null) {
            return nearest;
        }
        return GetNearestValidHostile();
    }
    //public void OnItemRemovedFromTile(SpecialToken token, LocationGridTile removedFrom) {
    //    if (hostilesInRange.Contains(token)) {
    //        RemoveHostileInRange(token);
    //    }
    //}
    private void AddPOIToBannedFromHostile(IPointOfInterest poi) {
        if (owner.movementComponent.isStationary) {
            //Stationary characters cannot ban hostiles because they can't move/pursue targets
            return;
        }
        if (poi is Character targetCharacter) {
            //Dead/unconscious characters should not be banned because they can no longer be attacked anyway
            if (targetCharacter.isDead || targetCharacter.traitContainer.HasTrait("Unconscious")) {
                return;
            }

            if (targetCharacter.combatComponent.isInCombat && !(targetCharacter.stateComponent.currentState as CombatState).isAttacking) {
                //Only ban characters that are already fleeing when you removed them from hostile list
            } else {
                return;
            }
        }
        if (!bannedFromHostileList.Contains(poi)) {
            
            bannedFromHostileList.Add(poi);
            GameDate dueDate = GameManager.Instance.Today();
            dueDate.AddTicks(4);
            SchedulingManager.Instance.AddEntry(dueDate, () => RemovePOIToBannedFromHostile(poi), owner);
        }
    }
    private bool RemovePOIToBannedFromHostile(IPointOfInterest poi) {
        return bannedFromHostileList.Remove(poi);
    }
    #endregion

    #region Avoid
    private bool AddAvoidInRange(IPointOfInterest poi, bool processCombatBehavior = true, string reason = "") {
        if (owner.canMove) {
        //if (!poi.isDead && !poi.traitContainer.HasTraitOf(TRAIT_TYPE.DISABLER, TRAIT_EFFECT.NEGATIVE) && character.traitContainer.GetNormalTrait<Trait>("Berserked") == null) {
            if (!avoidInRange.Contains(poi)) {
                avoidInRange.Add(poi);
                SetWillProcessCombat(true);
                //avoidReason = reason;
                return true;
            }
        }
        return false;
    }
    public void RemoveAvoidInRange(IPointOfInterest poi, bool processCombatBehavior = true) {
        if (avoidInRange.Remove(poi)) {
            if (processCombatBehavior) {
                SetWillProcessCombat(true);
                owner.logComponent.PrintLogIfActive($"{poi.name} was removed from {owner.name}'s avoid range!");
                //if (owner.combatComponent.isInCombat) {
                //    Messenger.Broadcast(Signals.DETERMINE_COMBAT_REACTION, owner);
                //}
            }
        }
    }
    public void RemoveAvoidInRangeSchedule(IPointOfInterest poi, bool processCombatBehavior = true) {
        if (avoidInRange.Contains(poi)) {
            GameDate dueDate = GameManager.Instance.Today();
            dueDate.AddTicks(3);
            SchedulingManager.Instance.AddEntry(dueDate, () => FinalCheckForRemoveAvoidSchedule(poi, processCombatBehavior), owner);
        }
    }
    private void FinalCheckForRemoveAvoidSchedule(IPointOfInterest poi, bool processCombatBehavior) {
        if (owner.marker) {
            if (!owner.marker.IsStillInRange(poi)) {
                RemoveAvoidInRange(poi, processCombatBehavior);
            }
        }
    }
    public void RemoveHostileInRangeSchedule(IPointOfInterest poi, bool processCombatBehavior = true) {
        if (hostilesInRange.Contains(poi)) {
            if(fightCombatData.ContainsKey(poi) && fightCombatData[poi].reasonForCombat != CombatManager.Demon_Kill && fightCombatData[poi].connectedAction == null) {
                GameDate dueDate = GameManager.Instance.Today();
                dueDate.AddTicks(2);
                SchedulingManager.Instance.AddEntry(dueDate, () => FinalCheckForRemoveHostileSchedule(poi, processCombatBehavior), owner);
            }
        }
    }
    private void FinalCheckForRemoveHostileSchedule(IPointOfInterest poi, bool processCombatBehavior) {
        if (owner.marker) {
            if (!owner.marker.IsStillInRange(poi)) {
                RemoveHostileInRange(poi, processCombatBehavior);
            }
        }
    }
    public void ClearAvoidInRange(bool processCombatBehavior = true) {
        if (avoidInRange.Count > 0) {
            avoidInRange.Clear();
            owner.logComponent.PrintLogIfActive($"{owner.name} cleared avoid range!");
            if (processCombatBehavior) {
                SetWillProcessCombat(true);
                //if (owner.combatComponent.isInCombat) {
                //    Messenger.Broadcast(Signals.DETERMINE_COMBAT_REACTION, owner);
                //}
            }
        }
    }
    #endregion

    #region Combat Data
    public CombatData GetCombatData(IPointOfInterest target) {
        if (fightCombatData.ContainsKey(target)) {
            return fightCombatData[target];
        }
        return null;
    }
    public void ClearCombatData() {
        foreach (CombatData combatData in fightCombatData.Values) {
            ObjectPoolManager.Instance.ReturnCombatDataToPool(combatData);
        }
        fightCombatData.Clear();
    }
    public string GetCombatLogKeyReason(IPointOfInterest target) {
        string key = string.Empty;
        CombatData combatData = GetCombatData(target);
        if (combatData != null) {
            key = combatData.reasonForCombat;
            if (key == CombatManager.Action) {
                switch (combatData.connectedAction.associatedJobType) {
                    case JOB_TYPE.RESTRAIN:
                        key = "Restrain";
                        break;
                    case JOB_TYPE.PRODUCE_FOOD:
                        key = "Butcher";
                        break;
                    case JOB_TYPE.APPREHEND:
                        key = "Apprehend";
                        break;
                    case JOB_TYPE.RITUAL_KILLING:
                        key = "Ritual Killing";
                        break;
                    case JOB_TYPE.BERSERK_ATTACK:
                        key = "Berserked";
                        break;
                    case JOB_TYPE.BRAWL:
                        key = "Brawl";
                        break;
                    case JOB_TYPE.DESTROY:
                        if (owner.traitContainer.HasTrait("Angry")) {
                            key = "Destroy_Angry";
                        } else if (owner.traitContainer.HasTrait("Suspicious")) {
                            key = "Destroy_Suspicious";
                        }
                        break;
                    case JOB_TYPE.MOVE_CHARACTER:
                        if (owner is Troll) {
                            key = "Abduct";
                        }
                        break;
                }
            } else {
                if(key == CombatManager.Anger) {
                    if (owner.traitContainer.HasTrait("Angry")) {
                        Trait trait = owner.traitContainer.GetNormalTrait<Trait>("Angry");
                        if(trait.responsibleCharacter == target) {
                            key = "Anger_Target";
                        }
                    }
                }
            }
        }
        return key;
    }
    #endregion

    #region Jobs
    public void OnJobRemovedFromQueue(JobQueueItem job) {
        //Dropped Combat Related Jobs should remove target from hostile list if not yet in actual combat with it?
        //https://trello.com/c/GXzaAAsP/1361-dropped-combat-related-jobs-should-remove-target-from-hostile-list-if-not-yet-in-actual-combat-with-it
        if (!job.finishedSuccessfully) {
            bool hasRemoved = false;
            foreach (KeyValuePair<IPointOfInterest, CombatData> kvp in fightCombatData) {
                if (kvp.Value.connectedAction != null && kvp.Value.connectedAction.associatedJob == job) {
                    if (!IsInActualCombatWith(kvp.Key)) {
                        if (RemoveHostileInRange(kvp.Key, false)){
                            hasRemoved = true;
                        }
                    }
                }
            }
            if (hasRemoved) {
                SetWillProcessCombat(true);
            }
        }
    }
    #endregion

    #region Basic Data
    public void UpdateBasicData(bool resetHP) {
        UpdateAttack();
        UpdateAttackSpeed();
        if (resetHP) {
            UpdateMaxHPAndReset();
        } else {
            UpdateMaxHP();
        }
    }
    private void UpdateAttack() {
        attack = Mathf.RoundToInt(owner.characterClass.baseAttackPower * (owner.raceSetting.attackMultiplier == 0f ? 1f : owner.raceSetting.attackMultiplier)) + attackModification;
    }
    private void UpdateMaxHP() {
        maxHP = Mathf.RoundToInt(owner.characterClass.baseHP * (owner.raceSetting.hpMultiplier == 0f ? 1f : owner.raceSetting.hpMultiplier)) + maxHPModification;
        if (maxHP < 0) {
            maxHP = 1;
        }
    }
    private void UpdateAttackSpeed() {
        attackSpeed = owner.characterClass.baseAttackSpeed;
        //attackSpeed = Mathf.RoundToInt(owner.characterClass.baseAttackSpeed * (owner.raceSetting.attackSpeedMultiplier == 0f ? 1f : owner.raceSetting.attackSpeedMultiplier));
    }
    public void UpdateMaxHPAndReset() {
        UpdateMaxHP();
        owner.ResetToFullHP();
    }
    public void AdjustMaxHPModifier(int modification) {
        maxHPModification += modification;
        UpdateMaxHP();
    }
    public void AdjustAttackModifier(int modification) {
        attackModification += modification;
        UpdateAttack();
    }
    #endregion

    #region Signals
    public void SubscribeToSignals() {
        //Messenger.AddListener<JobQueueItem, Character>(Signals.JOB_REMOVED_FROM_QUEUE, OnJobRemovedFromQueue);
    }
    public void UnsubscribeToSignals() {
        //Messenger.RemoveListener<JobQueueItem, Character>(Signals.JOB_REMOVED_FROM_QUEUE, OnJobRemovedFromQueue);
    }
    #endregion
}

public class CombatData {
    public string reasonForCombat;
    public string avoidReason;
    public ActualGoapNode connectedAction;
    public bool isLethal;

    public CombatData() {
        Initialize();
    }
    public void Initialize() {
        reasonForCombat = string.Empty;
        avoidReason = string.Empty;
        connectedAction = null;
        isLethal = false;
    }
    public void Reset() {
        Initialize();
    }
    public void SetFightData(string reasonForCombat, ActualGoapNode connectedAction, bool isLethal) {
        this.reasonForCombat = reasonForCombat;
        this.connectedAction = connectedAction;
        this.isLethal = isLethal;
    }
    public void SetFlightData(string avoidReason) {
        this.avoidReason = avoidReason;
    }
}

public struct CombatReaction {
    public COMBAT_REACTION reaction;
    public string reason;

    public CombatReaction(COMBAT_REACTION reaction, string reason = "") {
        this.reaction = reaction;
        this.reason = reason;
    }
}