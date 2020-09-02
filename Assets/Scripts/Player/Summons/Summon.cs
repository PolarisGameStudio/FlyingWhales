﻿using System;
using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Traits;
using UnityEngine;
using Interrupts;

public class Summon : Character {

	public SUMMON_TYPE summonType { get; }
    private bool showNotificationOnDeath { get; set; }
    public virtual SUMMON_TYPE adultSummonType => SUMMON_TYPE.None;
    public virtual COMBAT_MODE defaultCombatMode => COMBAT_MODE.Aggressive;
    public virtual bool defaultDigMode => false;
    public virtual string bredBehaviour => characterClass.traitNameOnTamedByPlayer;

    protected Summon(SUMMON_TYPE summonType, string className, RACE race, GENDER gender) : base(className, race, gender) {
        this.summonType = summonType;
        showNotificationOnDeath = true;
    }
    protected Summon(SaveDataCharacter data) : base(data) {
        //this.summonType = data.summonType;
    }

    #region Overrides
    public override void Initialize() {
        ConstructDefaultActions();
        OnUpdateRace();
        OnUpdateCharacterClass();

        moodComponent.OnCharacterBecomeMinionOrSummon();
        moodComponent.SetMoodValue(50);

        //CreateOwnParty();
        
        needsComponent.Initialize();
        
        advertisedActions.Clear(); //This is so that any advertisements from OnUpdateRace will be negated. TODO: Make updating advertisements better.
        //TODO: Put this in a system
        AddAdvertisedAction(INTERACTION_TYPE.ABSORB_LIFE);
        AddAdvertisedAction(INTERACTION_TYPE.ABSORB_POWER);
        ConstructInitialGoapAdvertisementActions();
        needsComponent.SetFullnessForcedTick(0);
        needsComponent.SetTirednessForcedTick(0);
        behaviourComponent.ChangeDefaultBehaviourSet(CharacterManager.Default_Monster_Behaviour);
        //SubscribeToSignals(); //NOTE: Only made characters subscribe to signals when their npcSettlement is the one that is currently active. TODO: Also make sure to unsubscribe a character when the player has completed their map.
    }
    public override void OnActionPerformed(ActualGoapNode node) { } //overridden OnActionStateSet so that summons cannot witness other events.
    public override void Death(string cause = "normal", ActualGoapNode deathFromAction = null, Character responsibleCharacter = null, Log _deathLog = null, LogFiller[] deathLogFillers = null, Interrupt interrupt = null) {
        if (!_isDead) {
            Region deathLocation = currentRegion;
            LocationStructure deathStructure = currentStructure;
            LocationGridTile deathTile = gridTileLocation;

            if (lycanData != null) {
                lycanData.LycanDies(this, cause, deathFromAction, responsibleCharacter, _deathLog, deathLogFillers);
            }
            
            SetIsDead(true);
            if (isLimboCharacter && isInLimbo) {
                //If a limbo character dies while in limbo, that character should not process death, instead he/she will be removed from the list
                CharacterManager.Instance.RemoveLimboCharacter(this);
                return;
            }
            UnsubscribeSignals();
            //behaviourComponent.SetIsHarassing(false, null);
            //behaviourComponent.SetIsInvading(false, null);
            //behaviourComponent.SetIsDefending(false, null);

            //if (currentParty.specificLocation == null) {
            //    throw new Exception("Specific location of " + this.name + " is null! Please use command /l_character_location_history [Character Name/ID] in console menu to log character's location history. (Use '~' to show console menu)");
            //}
            if (stateComponent.currentState != null) {
                stateComponent.ExitCurrentState();
            }
            //else if (stateComponent.stateToDo != null) {
            //    stateComponent.SetStateToDo(null);
            //}
            //if (deathFromAction != null) { //if this character died from an action, do not cancel the action that he/she died from. so that the action will just end as normal.
            //    CancelAllJobsTargettingThisCharacterExcept(deathFromAction, "target is already dead", false);
            //} else {
            //    CancelAllJobsTargettingThisCharacter("target is already dead", false);
            //}
            //ForceCancelAllJobsTargettingCharacter(false, "target is already dead");
            Messenger.Broadcast(Signals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI, this as IPointOfInterest, "target is already dead");
            jobQueue.CancelAllJobs();
            //Messenger.Broadcast(Signals.CANCEL_CURRENT_ACTION, this as Character, "target is already dead");
            //if (currentActionNode != null) {
            //    currentActionNode.StopActionNode(false);
            //}
            //if (currentSettlement != null && isHoldingItem) {
            //    DropAllItems(deathTile);
            //}
            DropAllItems(deathTile);
            UnownOrTransferOwnershipOfAllItems();

            reactionComponent.SetIsHidden(false);
            //if (ownParty.specificLocation != null && isHoldingItem) {
            //    DropAllTokens(ownParty.specificLocation, currentStructure, deathTile, true);
            //}

            //clear traits that need to be removed
            traitsNeededToBeRemoved.Clear();

            UncarryPOI();
            Character carrier = isBeingCarriedBy;
            if (carrier != null) {
                carrier.UncarryPOI(this);
            }
            //ownParty.PartyDeath();

            //No longer remove from region list even if character died to prevent inconsistency in data because if a dead character is picked up and dropped, he will be added in the structure location list again but wont be in region list
            //https://trello.com/c/WTiGxjrK/1786-inconsistent-characters-at-location-list-in-region-with-characters-at-structure
            //currentRegion?.RemoveCharacterFromLocation(this);
            //SetRegionLocation(deathLocation); //set the specific location of this party, to the location it died at
            //SetCurrentStructureLocation(deathStructure, false);

            // if (_role != null) {
            //     _role.OnDeath(this);
            // }

            if (homeRegion != null) {
                Region home = homeRegion;
                LocationStructure homeStructure = this.homeStructure;
                homeRegion.RemoveResident(this);
                SetHomeRegion(home); //keep this data with character to prevent errors
                SetHomeStructure(homeStructure); //keep this data with character to prevent errors
            }
            //if (homeNpcSettlement != null) {
            //    NPCSettlement home = homeNpcSettlement;
            //    Dwelling homeStructure = this.homeStructure;
            //    homeNpcSettlement.RemoveResident(this);
            //    SetHome(home); //keep this data with character to prevent errors
            //    SetHomeStructure(homeStructure); //keep this data with character to prevent errors
            //}

            traitContainer.RemoveAllTraitsAndStatusesByName(this, "Criminal"); //remove all criminal type traits

            List<Trait> traitOverrideFunctions = traitContainer.GetTraitOverrideFunctions(TraitManager.Death_Trait);
            if (traitOverrideFunctions != null) {
                for (int i = 0; i < traitOverrideFunctions.Count; i++) {
                    Trait trait = traitOverrideFunctions[i];
                    if (trait.OnDeath(this)) {
                        i--;
                    }
                }
            }
            //for (int i = 0; i < traitContainer.allTraitsAndStatuses.Count; i++) {
            //    if (traitContainer.allTraitsAndStatuses[i].OnDeath(this)) {
            //        i--;
            //    }
            //}

            marker?.OnDeath(deathTile);

            if (interruptComponent.isInterrupted && interruptComponent.currentInterrupt.interrupt != interrupt) {
                interruptComponent.ForceEndNonSimultaneousInterrupt();
            }

            Dead dead = new Dead();
            dead.AddCharacterResponsibleForTrait(responsibleCharacter);
            traitContainer.AddTrait(this, dead, gainedFromDoing: deathFromAction);
            Messenger.Broadcast(Signals.CHARACTER_DEATH, this as Character);

            behaviourComponent.OnDeath();
            jobQueue.CancelAllJobs();

            //Debug.Log(GameManager.Instance.TodayLogString() + this.name + " died of " + cause);
            Log deathLog;
            if (_deathLog == null) {
                deathLog = new Log(GameManager.Instance.Today(), "Character", "Generic", $"death_{cause}");
                deathLog.AddToFillers(this, name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                if (responsibleCharacter != null) {
                    deathLog.AddToFillers(responsibleCharacter, responsibleCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                }
                if (deathLogFillers != null) {
                    for (int i = 0; i < deathLogFillers.Length; i++) {
                        deathLog.AddToFillers(deathLogFillers[i]);
                    }
                }
                //will only add death log to history if no death log is provided. NOTE: This assumes that if a death log is provided, it has already been added to this characters history.
                logComponent.AddHistory(deathLog);
                if (showNotificationOnDeath) {
                    PlayerManager.Instance.player.ShowNotificationFrom(this, deathLog);    
                }
            } else {
                deathLog = _deathLog;
            }
            SetDeathLog(deathLog);
            AfterDeath(deathTile);
        }
    }
    protected override void OnTickStarted() {
        ProcessTraitsOnTickStarted();
        StartTickGoapPlanGeneration();
    }
    public override void OnUnseizePOI(LocationGridTile tileLocation) {
        base.OnUnseizePOI(tileLocation);
        //If you drop a monster at a structure that is not yet full and not occupied by villagers, they will set their home to that place.
        if(tileLocation.structure != null && tileLocation.structure.structureType != STRUCTURE_TYPE.WILDERNESS && !(tileLocation.structure is DemonicStructure)) {
            if(!tileLocation.structure.HasReachedMaxResidentCapacity() && !tileLocation.structure.HasResidentThatMeetCriteria(r => r.isNormalCharacter)) {
                MigrateHomeStructureTo(tileLocation.structure);
            }
        }
    }
    #endregion

    #region Virtuals
    /// <summary>
    /// What should a summon do when it is placed.
    /// </summary>
    /// <param name="tile">The tile the summon was placed on.</param>
    public virtual void OnPlaceSummon(LocationGridTile tile) {
        SubscribeToSignals();
        Messenger.RemoveListener(Signals.HOUR_STARTED, () => needsComponent.DecreaseNeeds()); //do not make summons decrease needs
        movementComponent.UpdateSpeed();
        behaviourComponent.OnSummon(tile);
    }
    protected virtual void AfterDeath(LocationGridTile deathTileLocation) {
        if (marker == null && destroyMarkerOnDeath/* && (behaviourComponent.isInvading || behaviourComponent.isDefending || behaviourComponent.isHarassing)*/) {
            GameManager.Instance.CreateParticleEffectAt(deathTileLocation, PARTICLE_EFFECT.Minion_Dissipate);
        }
        behaviourComponent.SetIsHarassing(false, null);
        behaviourComponent.SetIsInvading(false, null);
        behaviourComponent.SetIsDefending(false, null);
    }
    public virtual void OnSummonAsPlayerMonster() {
        combatComponent.SetCombatMode(COMBAT_MODE.Aggressive);
    }
    #endregion

    #region Player Action Target
    public override void ConstructDefaultActions() {
        if (actions == null) {
            actions = new List<SPELL_TYPE>();
        } else {
            actions.Clear();
        }
        //PlayerAction seizeAction = new PlayerAction(PlayerDB.Seize_Character_Action,
        //() => !PlayerManager.Instance.player.seizeComponent.hasSeizedPOI && !this.traitContainer.HasTrait("Leader", "Blessed"),
        //null,
        //() => PlayerManager.Instance.player.seizeComponent.SeizePOI(this));

        AddPlayerAction(SPELL_TYPE.SEIZE_MONSTER);
        AddPlayerAction(SPELL_TYPE.BREED_MONSTER);
        AddPlayerAction(SPELL_TYPE.AGITATE);
        AddPlayerAction(SPELL_TYPE.SNATCH);
        AddPlayerAction(SPELL_TYPE.SACRIFICE);
    }
    #endregion

    #region Selecatble
    public override bool IsCurrentlySelected() {
        Character characterToSelect = this;
        if (lycanData != null) {
            characterToSelect = lycanData.activeForm;
        }
        return UIManager.Instance.monsterInfoUI.isShowing &&
               UIManager.Instance.monsterInfoUI.activeMonster == characterToSelect;
    }
    #endregion

    #region Utilities
    public void SetShowNotificationOnDeath(bool showNotificationOnDeath) {
        this.showNotificationOnDeath = showNotificationOnDeath;
    }
    #endregion
}

public class SummonSlot {
    public int level;
    public Summon summon;
    public bool isLocked {
        get { return false; }
    }

    public SummonSlot() {
        level = 1;
        summon = null;
    }

    public void SetSummon(Summon summon) {
        this.summon = summon;
        //if (this.summon != null) {
        //    this.summon.StartingLevel();
        //}
    }

    //public void LevelUp() {
    //    level++;
    //    level = Mathf.Clamp(level, 1, PlayerDB.MAX_LEVEL_SUMMON);
    //    if (this.summon != null) {
    //        this.summon.SetLevel(level);
    //    }
    //    Messenger.Broadcast(Signals.PLAYER_GAINED_SUMMON_LEVEL, this);
    //}
    //public void SetLevel(int amount) {
    //    level = amount;
    //    level = Mathf.Clamp(level, 1, PlayerDB.MAX_LEVEL_SUMMON);
    //    if (this.summon != null) {
    //        this.summon.SetLevel(level);
    //    }
    //    Messenger.Broadcast(Signals.PLAYER_GAINED_SUMMON_LEVEL, this);
    //}
}
