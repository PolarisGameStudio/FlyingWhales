﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Inner_Maps;
using Traits;
using Archetype;
using Locations.Settlements;
using Ruinarch;
using UtilityScripts;
using Random = UnityEngine.Random;
using Inner_Maps.Location_Structures;
// ReSharper disable Unity.NoNullPropagation

public class Player : ILeader, IObjectManipulator {
    //public PlayerArchetype archetype { get; private set; }
    public Faction playerFaction { get; private set; }
    public PlayerSettlement playerSettlement { get; private set; }
    public int mana { get; private set; }
    public int experience { get; private set; }
    public List<IIntel> allIntel { get; private set; }
    public List<Minion> minions { get; private set; }
    public List<Summon> summons { get; private set; }
    //public List<Artifact> artifacts { get; private set; }
    //private int currentCorruptionDuration { get; set; }
    //private int currentCorruptionTick { get; set; }
    //private bool isTileCurrentlyBeingCorrupted { get; set; }
    //public HexTile currentTileBeingCorrupted { get; private set; }
    public CombatAbility currentActiveCombatAbility { get; private set; }
    public IIntel currentActiveIntel { get; private set; }
    //public int maxSummonSlots { get; private set; } //how many summons can the player have
    //public int maxArtifactSlots { get; private set; } //how many artifacts can the player have
    //public PlayerJobActionSlot[] interventionAbilitySlots { get; }
    public HexTile portalTile { get; private set; }
    //public float constructionRatePercentageModifier { get; private set; }
    public List<SPELL_TYPE> unlearnedSpells { get; }
    public List<SPELL_TYPE> unlearnedAfflictions { get; }

    //Components
    public SeizeComponent seizeComponent { get; }
    public ThreatComponent threatComponent { get; }
    public PlayerSkillComponent playerSkillComponent { get; }

    #region getters/setters
    public int id => -645;
    public string name => "Player";
    public RACE race => RACE.HUMANS;
    public GENDER gender => GENDER.MALE;
    public Region currentRegion => null;
   
    public Region homeRegion => null;
    #endregion

    public Player() {
        allIntel = new List<IIntel>();
        minions = new List<Minion>();
        summons = new List<Summon>();
        //artifacts = new List<Artifact>();
        //interventionAbilitySlots = new PlayerJobActionSlot[PlayerDB.MAX_INTERVENTION_ABILITIES];
        //maxSummonSlots = 0;
        //maxArtifactSlots = 0;
        unlearnedSpells = new List<SPELL_TYPE>(PlayerDB.spells);
        unlearnedAfflictions = new List<SPELL_TYPE>(PlayerDB.afflictions);
        AdjustMana(EditableValuesManager.Instance.startingMana);
        seizeComponent = new SeizeComponent();
        threatComponent = new ThreatComponent(this);
        playerSkillComponent = new PlayerSkillComponent(this);
        //ConstructAllInterventionAbilitySlots();
        AddListeners();
        currentActiveItem = TILE_OBJECT_TYPE.NONE;
    }
    public Player(SaveDataPlayerGame data) {
        allIntel = new List<IIntel>();
        unlearnedSpells = new List<SPELL_TYPE>(PlayerDB.spells);
        unlearnedAfflictions = new List<SPELL_TYPE>(PlayerDB.afflictions);
        seizeComponent = new SeizeComponent();
        threatComponent = new ThreatComponent(this);
        playerSkillComponent = new PlayerSkillComponent(this);

        //TODO: Load Minions/Summons
        AdjustMana(data.mana);
        SetPortalTile(GridMap.Instance.map[data.portalTileXCoordinate, data.portalTileYCoordinate]);
        threatComponent.AdjustThreat(data.threat);
        playerSkillComponent.LoadSkills(data.skills);

        AddListeners();
        currentActiveItem = TILE_OBJECT_TYPE.NONE;
    }

    public void LoadPlayerData(SaveDataPlayer save) {
        if(save != null) {
            experience = save.exp;
            playerSkillComponent.LoadPlayerSkillTreeOrLoadout(save);
            //playerSkillComponent.LoadSummons(save);
        }
    }

    public void SetPortalTile(HexTile tile) {
        portalTile = tile;
    }

    #region Listeners
    private void AddListeners() {
        AddWinListener();
        //goap
        // Messenger.AddListener<string, ActualGoapNode>(Signals.AFTER_ACTION_STATE_SET, OnAfterActionStateSet);
        // Messenger.AddListener<Character, ActualGoapNode>(Signals.CHARACTER_DOING_ACTION, OnCharacterDoingAction);
        Messenger.AddListener<Region>(Signals.LOCATION_MAP_OPENED, OnInnerMapOpened);
        Messenger.AddListener<Region>(Signals.LOCATION_MAP_CLOSED, OnInnerMapClosed);

        //minions
        Messenger.AddListener<Minion, BaseLandmark>(Signals.MINION_ASSIGNED_PLAYER_LANDMARK, OnMinionAssignedToPlayerLandmark);
        Messenger.AddListener<Minion, BaseLandmark>(Signals.MINION_UNASSIGNED_PLAYER_LANDMARK, OnMinionUnassignedFromPlayerLandmark);
        Messenger.AddListener<Minion>(Signals.SUMMON_MINION, OnSummonMinion);
        Messenger.AddListener<Minion>(Signals.UNSUMMON_MINION, OnUnsummonMinion);

        Messenger.AddListener<Character, Faction>(Signals.CHARACTER_ADDED_TO_FACTION, OnCharacterAddedToFaction);
        Messenger.AddListener<Character, Faction>(Signals.CHARACTER_REMOVED_FROM_FACTION, OnCharacterRemovedFromFaction);
        Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);

        Messenger.AddListener(Signals.HOUR_STARTED, OnHourStarted);
    }
    #endregion

    #region ILeader
    //public void LevelUp() { }
    #endregion

    #region Settlement
    public PlayerSettlement CreatePlayerSettlement(BaseLandmark portal) {
        PlayerSettlement npcSettlement = LandmarkManager.Instance.CreateNewPlayerSettlement(portal.tileLocation);
        npcSettlement.SetName("Demonic Intrusion");
        SetPlayerArea(npcSettlement);
        // portal.tileLocation.InstantlyCorruptAllOwnedInnerMapTiles();
        return npcSettlement;
    }
    public void LoadPlayerArea(PlayerSettlement npcSettlement) {
        //Biomes.Instance.CorruptTileVisuals(npcSettlement.coreTile);
        //npcSettlement.coreTile.tileLocation.SetCorruption(true);
        SetPlayerArea(npcSettlement);
        //_demonicPortal.tileLocation.ScheduleCorruption();
    }
    public void SetPlayerArea(PlayerSettlement npcSettlement) {
        playerSettlement = npcSettlement;
    }
    private void OnInnerMapOpened(Region area) {
        //for (int i = 0; i < minions.Count; i++) {
        //    minions[i].ResetCombatAbilityCD();
        //}
        //ResetInterventionAbilitiesCD();
        //currentTargetFaction = npcSettlement.owner;
    }
    private void OnInnerMapClosed(Region area) {
        //currentTargetFaction = null;
    }
    #endregion

    #region Faction
    public void CreatePlayerFaction() {
        Faction faction = FactionManager.Instance.CreateNewFaction(FACTION_TYPE.Demons, "Demons");
        faction.SetLeader(this);
        faction.SetEmblem(FactionManager.Instance.GetFactionEmblem(6));
        SetPlayerFaction(faction);
    }
    //public void CreatePlayerFaction(SaveDataPlayer data) {
    //    Faction faction = FactionManager.Instance.GetFactionBasedOnID(data.playerFactionID);
    //    faction.SetLeader(this);
    //    SetPlayerFaction(faction);
    //}
    private void SetPlayerFaction(Faction faction) {
        playerFaction = faction;
    }
    //public void SetPlayerTargetFaction(Faction faction) {
    //    currentTargetFaction = faction;
    //}
    private void OnCharacterAddedToFaction(Character character, Faction faction) {
        if(faction == playerFaction) {
            //if(character.minion != null) {
            //    AddMinion(character.minion);
            //} 
            //else if(character is Summon summon) {
            //    AddSummon(summon);
            //}
            if (!string.IsNullOrEmpty(character.characterClass.traitNameOnTamedByPlayer)) {
                character.traitContainer.AddTrait(character, character.characterClass.traitNameOnTamedByPlayer);
            }
        }
    }
    private void OnCharacterRemovedFromFaction(Character character, Faction faction) {
        if (faction == playerFaction) {
            //if (character.minion != null) {
            //    RemoveMinion(character.minion);
            //} 
            //else if (character is Summon summon) {
            //    RemoveSummon(summon);
            //}
            if (!string.IsNullOrEmpty(character.characterClass.traitNameOnTamedByPlayer)) {
                character.traitContainer.RemoveTrait(character, character.characterClass.traitNameOnTamedByPlayer);
            }
        }
    }
    #endregion

    #region Minions
    public Minion CreateNewMinion(Character character, bool initialize = true) {
        Minion minion = new Minion(character, true);
        if (initialize) {
            InitializeMinion(minion);
        }
        return minion;
    }
    public Minion CreateNewMinion(SaveDataMinion data) {
        Minion minion = new Minion(data);
        InitializeMinion(data, minion);
        return minion;
    }
    public Minion CreateNewMinion(string className, RACE race, bool initialize = true) {
        Minion minion = new Minion(CharacterManager.Instance.CreateNewCharacter(className, race, GENDER.MALE, playerFaction, playerSettlement, portalTile.region), false);
        if (initialize) {
            InitializeMinion(minion);
        }
        return minion;
    }
    private void InitializeMinion(Minion minion) { }
    private void InitializeMinion(SaveDataMinion data, Minion minion) {
        data.combatAbility.Load(minion);
    }
    public void AddMinion(Minion minion) {
        if (!minions.Contains(minion)) {
            minions.Add(minion);
            Messenger.Broadcast(Signals.PLAYER_GAINED_MINION, minion);
        }
    }
    public void RemoveMinion(Minion minion) {
        if (minions.Remove(minion)) {
            Messenger.Broadcast(Signals.PLAYER_LOST_MINION, minion);
        }
    }
    private void OnSummonMinion(Minion minion) {
        AddMinion(minion);
    }
    private void OnUnsummonMinion(Minion minion) {
        RemoveMinion(minion);
    }
    //public void SetMinionLeader(Minion minion) {
    //    currentMinionLeader = minion;
    //}
    //private void ReplaceMinion(object objToReplace, object objToAdd) {
    //    Minion minionToBeReplaced = objToReplace as Minion;
    //    Minion minionToBeAdded = objToAdd as Minion;

    //    for (int i = 0; i < minions.Count; i++) {
    //        if(minions[i] == minionToBeReplaced) {
    //            minionToBeAdded.SetIndexDefaultSort(i);
    //            minions[i] = minionToBeAdded;
    //            if(currentMinionLeader == minionToBeReplaced) {
    //                SetMinionLeader(minionToBeAdded);
    //            }
    //            break;
    //        }
    //    }
    //}
    private void RejectMinion(object obj) { }
    #endregion

    #region Win/Lose Conditions
    private void AddWinListener() {
        Messenger.AddListener<Faction>(Signals.FACTION_LEADER_DIED, OnFactionLeaderDied);
    }
    private void OnFactionLeaderDied(Faction faction) {
        List<Faction> allUndestroyedFactions = FactionManager.Instance.allFactions.Where(
            x => x != FactionManager.Instance.neutralFaction
            && !x.isPlayerFaction
            && x.isActive && !x.isDestroyed).ToList();
        if (allUndestroyedFactions.Count == 0) {
            Debug.LogError("All factions are destroyed! Player won!");
        }        
    }
    #endregion

    #region Role Actions
    public SpellData currentActivePlayerSpell { get; private set; }
    public void SetCurrentlyActivePlayerSpell(SpellData action) {
        if(currentActivePlayerSpell != action) {
            SpellData previousActiveAction = currentActivePlayerSpell;
            currentActivePlayerSpell = action;
            if (currentActivePlayerSpell == null) {
                UIManager.Instance.SetTempDisableShowInfoUI(false); //allow UI clicks again after active spell has been set to null
                Messenger.RemoveListener<KeyCode>(Signals.KEY_DOWN, OnSpellCast);
            	InputManager.Instance.SetCursorTo(InputManager.Cursor_Type.Default);
                previousActiveAction.UnhighlightAffectedTiles();
                Messenger.Broadcast(Signals.PLAYER_NO_ACTIVE_SPELL, previousActiveAction);
            } else {
            	InputManager.Instance.SetCursorTo(InputManager.Cursor_Type.Cross);
                Messenger.AddListener<KeyCode>(Signals.KEY_DOWN, OnSpellCast);
                Messenger.Broadcast(Signals.PLAYER_SET_ACTIVE_SPELL, currentActivePlayerSpell);
            }
        }
    }
    private void OnSpellCast(KeyCode key) {
        if (key == KeyCode.Mouse0) {
            TryExecuteCurrentActiveSpell();
        }
    }

    private void TryExecuteCurrentActiveSpell() {
        if (UIManager.Instance.IsMouseOnUI() || !InnerMapManager.Instance.isAnInnerMapShowing) {
            return; //clicked on UI;
        }
        bool activatedAction = false;
        for (int i = 0; i < currentActivePlayerSpell.targetTypes.Length; i++) {
            LocationGridTile hoveredTile = null;
            switch (currentActivePlayerSpell.targetTypes[i]) {
                case SPELL_TARGET.NONE:
                    break;
                case SPELL_TARGET.CHARACTER:
                    if (InnerMapManager.Instance.currentlyShowingMap != null && InnerMapManager.Instance.currentlyHoveredPoi is Character) {
                        if (currentActivePlayerSpell.CanPerformAbilityTowards(InnerMapManager.Instance.currentlyHoveredPoi)) {
                            currentActivePlayerSpell.ActivateAbility(InnerMapManager.Instance.currentlyHoveredPoi);
                            activatedAction = true;
                        } else {
                        }
                        UIManager.Instance.SetTempDisableShowInfoUI(true);
                    }
                    break;
                case SPELL_TARGET.TILE_OBJECT:
                    if (InnerMapManager.Instance.currentlyHoveredPoi is TileObject) {
                        if (currentActivePlayerSpell.CanPerformAbilityTowards(InnerMapManager.Instance.currentlyHoveredPoi)) {
                            currentActivePlayerSpell.ActivateAbility(InnerMapManager.Instance.currentlyHoveredPoi);
                            activatedAction = true;
                        }
                        UIManager.Instance.SetTempDisableShowInfoUI(true);
                    }
                    break;
                case SPELL_TARGET.TILE:
                    hoveredTile = InnerMapManager.Instance.GetTileFromMousePosition();
                    if (hoveredTile != null) {
                        if (currentActivePlayerSpell.CanPerformAbilityTowards(hoveredTile)) {
                            currentActivePlayerSpell.ActivateAbility(hoveredTile);
                            activatedAction = true;
                        } 
                        UIManager.Instance.SetTempDisableShowInfoUI(true);
                    }
                    break;
                case SPELL_TARGET.HEX:
                    hoveredTile = InnerMapManager.Instance.GetTileFromMousePosition();
                    if (hoveredTile != null && hoveredTile.collectionOwner.partOfHextile.hexTileOwner) {
                        if (currentActivePlayerSpell.CanPerformAbilityTowards(hoveredTile.collectionOwner.partOfHextile.hexTileOwner)) {
                            currentActivePlayerSpell.ActivateAbility(hoveredTile.collectionOwner.partOfHextile.hexTileOwner);
                            activatedAction = true;
                        } 
                        UIManager.Instance.SetTempDisableShowInfoUI(true);
                    }
                    break;
                default:
                    break;
            }
            if (activatedAction) {
                break;
            }
        }
        
        InputManager.Instance.SetCursorTo(InputManager.Cursor_Type.Default);
        if (currentActivePlayerSpell.CanPerformAbility() == false || 
            (currentActivePlayerSpell is DemonicStructurePlayerSkill && activatedAction)) {
            //if player can no longer cast spell after casting it, set active spell to null.
            SetCurrentlyActivePlayerSpell(null);
        }
        //Debug.Log(GameManager.Instance.TodayLogString() + summary);
    }
    #endregion

    #region Intel
    public void AddIntel(IIntel newIntel) {
        if (!allIntel.Contains(newIntel)) {
            allIntel.Add(newIntel);
            if (allIntel.Count > PlayerDB.MAX_INTEL) {
                RemoveIntel(allIntel[0]);
            }
            Messenger.Broadcast(Signals.PLAYER_OBTAINED_INTEL, newIntel);
        }
    }
    private void RemoveIntel(IIntel intel) {
        if (allIntel.Remove(intel)) {
            Messenger.Broadcast(Signals.PLAYER_REMOVED_INTEL, intel);
        }
    }
    public void LoadIntels(SaveDataPlayer data) {
        //for (int i = 0; i < data.allIntel.Count; i++) {
        //    AddIntel(data.allIntel[i].Load());
        //}
    }
    public void SetCurrentActiveIntel(IIntel intel) {
        if (currentActiveIntel == intel) {
            //Do not process when setting the same intel
            return;
        }
        IIntel previousIntel = currentActiveIntel;
        currentActiveIntel = intel;
        if(previousIntel != null) {
            IntelItem intelItem = PlayerUI.Instance.GetIntelItemWithIntel(previousIntel);
            intelItem?.SetClickedState(false);
            Messenger.RemoveListener<KeyCode>(Signals.KEY_DOWN, OnIntelCast);
            InputManager.Instance.SetCursorTo(InputManager.Cursor_Type.Default);
        }
        if (currentActiveIntel != null) {
            Messenger.Broadcast(Signals.ACTIVE_INTEL_SET, currentActiveIntel);
            IntelItem intelItem = PlayerUI.Instance.GetIntelItemWithIntel(currentActiveIntel);
            intelItem?.SetClickedState(true);
            InputManager.Instance.SetCursorTo(InputManager.Cursor_Type.Cross);
            Messenger.AddListener<KeyCode>(Signals.KEY_DOWN, OnIntelCast);
        }
    }
    private void OnIntelCast(KeyCode keyCode) {
        if (keyCode == KeyCode.Mouse0) {
            TryExecuteCurrentActiveIntel();
        }
    }
    private void TryExecuteCurrentActiveIntel() {
        string hoverText = string.Empty;
        if (CanShareIntel(InnerMapManager.Instance.currentlyHoveredPoi, ref hoverText)) {
            Character targetCharacter = InnerMapManager.Instance.currentlyHoveredPoi as Character;
            UIManager.Instance.OpenShareIntelMenu(targetCharacter, null, currentActiveIntel);
            RemoveIntel(currentActiveIntel);
            SetCurrentActiveIntel(null);
        }
    }
    public bool CanShareIntel(IPointOfInterest poi, ref string hoverText) {
        if(poi is Character character) {
            if (!character.isNormalCharacter) {
                return false;
            }
            hoverText = string.Empty;
            if(character.traitContainer.HasTrait("Catatonic")) { //"Blessed"
                hoverText = "Catatonic characters cannot be targeted."; //Blessed/
                return false;
            }
            if(character.traitContainer.HasTrait("Resting")) { 
                hoverText = "Sleeping characters cannot be targeted.";
                return false;
            }
            if (character.traitContainer.HasTrait("Unconscious")) {
                hoverText = "Unconscious characters cannot be targeted.";
                return false;
            }
            if (!character.faction.isPlayerFaction && !GameUtilities.IsRaceBeast(character.race)) { //character.role.roleType != CHARACTER_ROLE.BEAST && character.role.roleType != CHARACTER_ROLE.PLAYER
                return true;
            }
        }
        return false;
    }
    #endregion

    #region Player Notifications
    private bool ShouldShowNotificationFrom(Region location) {
        return location.canShowNotifications;
    }
    private bool ShouldShowNotificationFrom(Character character) {
        // if (!onlyClickedCharacter && InnerMapCameraMove.Instance.gameObject.activeSelf) { //&& !character.isDead
        //     if ((UIManager.Instance.characterInfoUI.isShowing && UIManager.Instance.characterInfoUI.activeCharacter.id == character.id) || (character.marker &&  InnerMapCameraMove.Instance.CanSee(character.marker.gameObject))) {
        //         return true;
        //     }
        // } else if (onlyClickedCharacter && UIManager.Instance.characterInfoUI.isShowing && UIManager.Instance.characterInfoUI.activeCharacter.id == character.id) {
        //     return true;
        // }
        // return false;
        return character.currentRegion != null && ShouldShowNotificationFrom(character.currentRegion);
    }
    private bool ShouldShowNotificationFrom(Character character, Log log) {
        if (ShouldShowNotificationFrom(character)) {
            return true;
        } else {
            return ShouldShowNotificationFrom(log.fillers.Where(x => x.obj is Character).Select(x => x.obj as Character).ToArray())
                || ShouldShowNotificationFrom(log.fillers.Where(x => x.obj is Region).Select(x => x.obj as Region).ToArray());
        }
    }
    private bool ShouldShowNotificationFrom(Region location, Log log) {
        if (ShouldShowNotificationFrom(location)) {
            return true;
        } else {
            return ShouldShowNotificationFrom(log.fillers.Where(x => x.obj is Character).Select(x => x.obj as Character).ToArray())
                   || ShouldShowNotificationFrom(log.fillers.Where(x => x.obj is Region).Select(x => x.obj as Region).ToArray());
        }
    }
    private bool ShouldShowNotificationFrom(Character[] characters) {
        for (int i = 0; i < characters.Length; i++) {
            if (ShouldShowNotificationFrom(characters[i])) {
                return true;
            }
        }
        return false;
    }
    private bool ShouldShowNotificationFrom(Region[] locations) {
        for (int i = 0; i < locations.Length; i++) {
            if (ShouldShowNotificationFrom(locations[i])) {
                return true;
            }
        }
        return false;
    }
    private bool ShouldShowNotificationFrom(Character[] characters, Log log) {
        for (int i = 0; i < characters.Length; i++) {
            if (ShouldShowNotificationFrom(characters[i], log)) {
                return true;
            }
        }
        return false;
    }
    
    public bool ShowNotificationFrom(Region location, Log log) {
        if (ShouldShowNotificationFrom(location, log)) {
            ShowNotification(log);
            return true;
        }
        return false;
    }
    public bool ShowNotificationFrom(Character character, Log log) {
        if (ShouldShowNotificationFrom(character, log)) {
            ShowNotification(log);
            return true;
        }
        return false;
    }
    public bool ShowNotificationFrom(Character character, IIntel intel) {
        if (ShouldShowNotificationFrom(character)) {
            ShowNotification(intel);
            return true;
        }
        return false;
    }
    public void ShowNotificationFrom(Log log, params Character[] characters) {
        if (ShouldShowNotificationFrom(characters, log)) {
            ShowNotification(log);
        }
    }
    public void ShowNotificationFrom(Log log, Character character, bool onlyClickedCharacter) {
        if (ShouldShowNotificationFrom(character)) {
            ShowNotification(log);
        }
    }
    public void ShowNotificationFromPlayer(Log log) {
        ShowNotification(log);
    }
    
    private void ShowNotification(Log log) {
        Messenger.Broadcast(Signals.SHOW_PLAYER_NOTIFICATION, log);
    }
    private void ShowNotification(IIntel intel) {
        Messenger.Broadcast(Signals.SHOW_INTEL_NOTIFICATION, intel);
    }
    #endregion

    #region Tile Corruption
    //public void InvadeATile() {
    //    //currentCorruptionDuration = currentTileBeingCorrupted.corruptDuration;
    //    //if(currentCorruptionDuration == 0) {
    //    //    Debug.LogError("Cannot corrupt a tile with 0 corruption duration");
    //    //} else {
    //    //    GameManager.Instance.SetOnlyTickDays(true);
    //    //    currentTileBeingCorrupted.StartCorruptionAnimation();
    //    //    currentCorruptionTick = 0;
    //    //    Messenger.AddListener(Signals.DAY_STARTED, CorruptTilePerTick);
    //    //    UIManager.Instance.Unpause();
    //    //    isTileCurrentlyBeingCorrupted = true;
    //    //}
    //    currentTileBeingCorrupted.region.InvadeActions();
    //    //TODO:
    //    // LandmarkManager.Instance.OwnRegion(PlayerManager.Instance.player.playerFaction, RACE.DEMON, currentTileBeingCorrupted.region);
    //    //PlayerManager.Instance.AddTileToPlayerArea(currentTileBeingCorrupted);
    //}
    //private void CorruptTilePerTick() {
    //    currentCorruptionTick ++;
    //    if(currentCorruptionTick >= currentCorruptionDuration) {
    //        TileIsCorrupted();
    //    }
    //}
    //private void TileIsCorrupted() {
    //    isTileCurrentlyBeingCorrupted = false;
    //    Messenger.RemoveListener(Signals.DAY_STARTED, CorruptTilePerTick);
    //    UIManager.Instance.Pause();
    //    //TODO:
    //    // LandmarkManager.Instance.OwnRegion(PlayerManager.Instance.player.playerFaction, RACE.DEMON, currentTileBeingCorrupted.region);
    //    //PlayerManager.Instance.AddTileToPlayerArea(currentTileBeingCorrupted);
    //}
    #endregion

    #region Settlement Corruption
    private NPCSettlement AreaIsCorrupted() {
        //TODO:
        // isTileCurrentlyBeingCorrupted = false;
        // GameManager.Instance.SetPausedState(true);
        // NPCSettlement corruptedSettlement = currentTileBeingCorrupted.settlementOfTile;
        // LandmarkManager.Instance.OwnRegion(PlayerManager.Instance.player.playerFaction, RACE.DEMON, currentTileBeingCorrupted.region);
        // //PlayerManager.Instance.AddTileToPlayerArea(currentTileBeingCorrupted);
        // return corruptedSettlement;
        return null;
    }
    #endregion

    #region Summons
    private void AddSummon(Summon summon) {
        if (!summons.Contains(summon)) {
            summons.Add(summon);
            Messenger.Broadcast(Signals.PLAYER_GAINED_SUMMON, summon);
        }
    }
    private void RemoveSummon(Summon summon) {
        if (summons.Remove(summon)) {
            Messenger.Broadcast(Signals.PLAYER_LOST_SUMMON, summon);
        }
    }
    private void OnCharacterDied(Character character) {
        if (character.faction == playerFaction && character is Summon summon) {
            RemoveSummon(summon);
        }
    }
    #endregion

    #region Artifacts
    public ARTIFACT_TYPE currentActiveArtifact { get; private set; }
    public void SetCurrentlyActiveArtifact(ARTIFACT_TYPE artifact) {
        if (currentActiveArtifact != artifact) {
            ARTIFACT_TYPE previousActiveArtifact = currentActiveArtifact;
            currentActiveArtifact = artifact;
            if (currentActiveArtifact == ARTIFACT_TYPE.None) {
                Messenger.RemoveListener<KeyCode>(Signals.KEY_DOWN, OnArtifactCast);
                InputManager.Instance.SetCursorTo(InputManager.Cursor_Type.Default);
                Messenger.Broadcast(Signals.PLAYER_NO_ACTIVE_ARTIFACT, previousActiveArtifact);
            } else {
                InputManager.Instance.SetCursorTo(InputManager.Cursor_Type.Check);
                Messenger.AddListener<KeyCode>(Signals.KEY_DOWN, OnArtifactCast);
            }
        }
    }
    private void OnArtifactCast(KeyCode key) {
        if (key == KeyCode.Mouse0) {
            TrySpawnArtifact();
        }
    }
    private void TrySpawnArtifact() {
        if (UIManager.Instance.IsMouseOnUI() || !InnerMapManager.Instance.isAnInnerMapShowing) {
            return; //clicked on UI;
        }
        LocationGridTile hoveredTile = InnerMapManager.Instance.GetTileFromMousePosition();
        if (hoveredTile != null && hoveredTile.objHere == null) {
            Artifact artifact = InnerMapManager.Instance.CreateNewArtifact(currentActiveArtifact);
            hoveredTile.structure.AddPOI(artifact, hoveredTile);
        }
    }
    //public bool HasArtifact(string artifactName) {
    //    for (int i = 0; i < artifacts.Count; i++) {
    //        Artifact currArtifact = artifacts[i];
    //        if (currArtifact.name == artifactName) {
    //            return true;
    //        }
    //    }
    //    return false;
    //}
    //private Artifact GetArtifactOfType(ARTIFACT_TYPE type) {
    //    for (int i = 0; i < artifacts.Count; i++) {
    //        Artifact currArtifact = artifacts[i];
    //        if (currArtifact.type == type) {
    //            return currArtifact;
    //        }
    //    }
    //    return null;
    //}
    #endregion

    //#region Invasion
    //public void SetConstructionRatePercentageModifier(float amount) {
    //    constructionRatePercentageModifier = amount;
    //}
    //#endregion

    #region Combat Ability
    public void SetCurrentActiveCombatAbility(CombatAbility ability) {
        if(currentActiveCombatAbility == ability) {
            //Do not process when setting the same combat ability
            return;
        }
        CombatAbility previousAbility = currentActiveCombatAbility;
        currentActiveCombatAbility = ability;
        if (currentActiveCombatAbility == null) {
            InputManager.Instance.SetCursorTo(InputManager.Cursor_Type.Default);
            InnerMapManager.Instance.UnhighlightTiles();
            // InputManager.Instance.ClearLeftClickActions();
            //GameManager.Instance.SetPausedState(false);
        } else {
            //change the cursor
            InputManager.Instance.SetCursorTo(InputManager.Cursor_Type.Cross);
            // InputManager.Instance.AddLeftClickAction(TryExecuteCurrentActiveCombatAbility);
            // InputManager.Instance.AddLeftClickAction(() => SetCurrentActiveCombatAbility(null));
            //GameManager.Instance.SetPausedState(true);
        }
    }
    private void TryExecuteCurrentActiveCombatAbility() {
        //string summary = "Mouse was clicked. Will try to execute " + currentActiveCombatAbility.name;
        if (currentActiveCombatAbility.abilityRadius == 0) {
            if (currentActiveCombatAbility.CanTarget(InnerMapManager.Instance.currentlyHoveredPoi)) {
                currentActiveCombatAbility.ActivateAbility(InnerMapManager.Instance.currentlyHoveredPoi);
            }
        } else {
            List<LocationGridTile> highlightedTiles = InnerMapManager.Instance.currentlyHighlightedTiles;
            if (highlightedTiles != null) {
                List<IPointOfInterest> poisInHighlightedTiles = new List<IPointOfInterest>();
                for (int i = 0; i < InnerMapManager.Instance.currentlyShowingLocation.charactersAtLocation.Count; i++) {
                    Character currCharacter = InnerMapManager.Instance.currentlyShowingLocation.charactersAtLocation[i];
                    if (highlightedTiles.Contains(currCharacter.gridTileLocation)) {
                        poisInHighlightedTiles.Add(currCharacter);
                    }
                }
                for (int i = 0; i < highlightedTiles.Count; i++) {
                    if(highlightedTiles[i].objHere != null) {
                        poisInHighlightedTiles.Add(highlightedTiles[i].objHere);
                    }
                }
                currentActiveCombatAbility.ActivateAbility(poisInHighlightedTiles);
            }
        }
        //Debug.Log(GameManager.Instance.TodayLogString() + summary);
    }
    #endregion

    //#region Intervention Ability
    //private void ConstructAllInterventionAbilitySlots() {
    //    for (int i = 0; i < interventionAbilitySlots.Length; i++) {
    //        if (interventionAbilitySlots[i] == null) {
    //            interventionAbilitySlots[i] = new PlayerJobActionSlot();
    //        }
    //    }
    //}
    //public void GainNewInterventionAbility(SPELL_TYPE ability, bool showNewAbilityUI = false) {
    //    PlayerSpell playerSpell = PlayerManager.Instance.CreateNewInterventionAbility(ability);
    //    GainNewInterventionAbility(playerSpell, showNewAbilityUI);
    //}
    //public void GainNewInterventionAbility(PlayerSpell ability, bool showNewAbilityUI = false) {
    //    if (!HasEmptyInterventionSlot()) {
    //        PlayerUI.Instance.replaceUI.ShowReplaceUI(GetAllInterventionAbilities(), ability, OnReplaceInterventionAbility, OnRejectInterventionAbility);
    //    } else {
    //        for (int i = 0; i < interventionAbilitySlots.Length; i++) {
    //            if (interventionAbilitySlots[i].ability == null) {
    //                interventionAbilitySlots[i].SetAbility(ability);
    //                Messenger.Broadcast(Signals.PLAYER_LEARNED_INTERVENE_ABILITY, ability);
    //                if (showNewAbilityUI) {
    //                    PlayerUI.Instance.newAbilityUI.ShowNewAbilityUI(null, ability);
    //                }
    //                break;
    //            }
    //        }
    //    }
    //}
    //public void ConsumeAbility(PlayerSpell ability) {
    //    for (int i = 0; i < interventionAbilitySlots.Length; i++) {
    //        if (interventionAbilitySlots[i].ability == ability) {
    //            interventionAbilitySlots[i].SetAbility(null);
    //            Messenger.Broadcast(Signals.PLAYER_CONSUMED_INTERVENE_ABILITY, ability);
    //            break;
    //        }
    //    }
    //}
    //private void OnReplaceInterventionAbility(object objToReplace, object objToAdd) {
    //    PlayerSpell replace = objToReplace as PlayerSpell;
    //    PlayerSpell add = objToAdd as PlayerSpell;
    //    for (int i = 0; i < interventionAbilitySlots.Length; i++) {
    //        if (interventionAbilitySlots[i].ability == replace) {
    //            interventionAbilitySlots[i].SetAbility(add);
    //            Messenger.Broadcast(Signals.PLAYER_LEARNED_INTERVENE_ABILITY, add);
    //            break;
    //        }
    //    }
    //}
    //private void OnRejectInterventionAbility(object rejectedObj) { }
    //private int GetInterventionAbilityCount() {
    //    int count = 0;
    //    for (int i = 0; i < interventionAbilitySlots.Length; i++) {
    //        if (interventionAbilitySlots[i].ability != null) {
    //            count++;
    //        }
    //    }
    //    return count;
    //}
    //public bool HasEmptyInterventionSlot() {
    //    for (int i = 0; i < interventionAbilitySlots.Length; i++) {
    //        if (interventionAbilitySlots[i].ability == null) {
    //            return true;
    //        }
    //    }
    //    return false;
    //}
    //public List<PlayerSpell> GetAllInterventionAbilities() {
    //    List<PlayerSpell> abilities = new List<PlayerSpell>();
    //    for (int i = 0; i < interventionAbilitySlots.Length; i++) {
    //        if (interventionAbilitySlots[i].ability != null) {
    //            abilities.Add(interventionAbilitySlots[i].ability);
    //        }
    //    }
    //    return abilities;
    //}
    //public void ResetInterventionAbilitiesCD() {
    //    for (int i = 0; i < interventionAbilitySlots.Length; i++) {
    //        if (interventionAbilitySlots[i].ability != null) {
    //            interventionAbilitySlots[i].ability.InstantCooldown();
    //        }
    //    }
    //}
    //public bool AreAllInterventionSlotsMaxLevel() {
    //    for (int i = 0; i < interventionAbilitySlots.Length; i++) {
    //        if (interventionAbilitySlots[i].level < PlayerDB.MAX_LEVEL_INTERVENTION_ABILITY) {
    //            return false;
    //        }
    //    }
    //    return true;
    //}
    //public void LoadResearchNewInterventionAbility(SaveDataPlayer data) {
    //    if(data.interventionAbilityToResearch != INTERVENTION_ABILITY.NONE) {
    //        currentNewInterventionAbilityCycleIndex = data.currentNewInterventionAbilityCycleIndex;
    //        currentInterventionAbilityTimerTick = data.currentInterventionAbilityTimerTick;
    //        NewCycleForNewInterventionAbility(data.interventionAbilityToResearch);
    //    } else {
    //        StartResearchNewInterventionAbility();
    //    }
    //}
    //private void InitializeNewInterventionAbilityCycle() {
    //    currentNewInterventionAbilityCycleIndex = -1;
    //    newInterventionAbilityTimerTicks = GameManager.Instance.GetTicksBasedOnHour(8);
    //    interventionAbilityToResearch = INTERVENTION_ABILITY.NONE;
    //}
    //public void StartResearchNewInterventionAbility() {
    //    currentInterventionAbilityTimerTick = 0;
    //    currentNewInterventionAbilityCycleIndex++;
    //    if (currentNewInterventionAbilityCycleIndex > 3) {
    //        currentNewInterventionAbilityCycleIndex = 0;
    //    }

    //    int tier = GetTierBasedOnCycle();
    //    List<INTERVENTION_ABILITY> abilities = PlayerManager.Instance.GetAbilitiesByTier(tier);

    //    int index1 = UnityEngine.Random.Range(0, abilities.Count);
    //    INTERVENTION_ABILITY ability1 = abilities[index1];
    //    abilities.RemoveAt(index1);

    //    if (abilities.Count <= 0) {
    //        abilities = PlayerManager.Instance.GetAbilitiesByTier(tier);
    //    }
    //    int index2 = UnityEngine.Random.Range(0, abilities.Count);
    //    INTERVENTION_ABILITY ability2 = abilities[index2];
    //    abilities.RemoveAt(index2);

    //    if (abilities.Count <= 0) {
    //        abilities = PlayerManager.Instance.GetAbilitiesByTier(tier);
    //    }
    //    int index3 = UnityEngine.Random.Range(0, abilities.Count);
    //    INTERVENTION_ABILITY ability3 = abilities[index3];

    //    PlayerUI.Instance.researchInterventionAbilityUI.SetAbility1(ability1);
    //    PlayerUI.Instance.researchInterventionAbilityUI.SetAbility2(ability2);
    //    PlayerUI.Instance.researchInterventionAbilityUI.SetAbility3(ability3);
    //    PlayerUI.Instance.researchInterventionAbilityUI.ShowResearchUI();
    //}
    //private void PerTickInterventionAbility() {
    //    currentInterventionAbilityTimerTick++;
    //    if (currentInterventionAbilityTimerTick >= newInterventionAbilityTimerTicks) {
    //        Messenger.RemoveListener(Signals.TICK_STARTED, PerTickInterventionAbility);
    //        GainNewInterventionAbility(interventionAbilityToResearch, true);
    //        StartResearchNewInterventionAbility();
    //    }
    //}
    //public void NewCycleForNewInterventionAbility(INTERVENTION_ABILITY interventionAbilityToResearch) {
    //    if (!isNotFirstResearch) {
    //        isNotFirstResearch = true;
    //    }
    //    this.interventionAbilityToResearch = interventionAbilityToResearch;
    //    TimerHubUI.Instance.AddItem("Research for " + Utilities.NormalizeStringUpperCaseFirstLetters(interventionAbilityToResearch.ToString()), newInterventionAbilityTimerTicks - currentInterventionAbilityTimerTick, null);
    //    Messenger.AddListener(Signals.TICK_STARTED, PerTickInterventionAbility);
    //}
    //private int GetTierBasedOnCycle() {
    //    //Tier Cycle - 3, 3, 2, 1
    //    if (currentNewInterventionAbilityCycleIndex == 0) return 3;
    //    else if (currentNewInterventionAbilityCycleIndex == 1) return 3;
    //    else if (currentNewInterventionAbilityCycleIndex == 2) return 2;
    //    else if (currentNewInterventionAbilityCycleIndex == 3) return 1;
    //    return 3;
    //}
    //#endregion

    #region The Eye
    private void OnMinionAssignedToPlayerLandmark(Minion minion, BaseLandmark landmark) { }
    private void OnMinionUnassignedFromPlayerLandmark(Minion minion, BaseLandmark landmark) { }
    #endregion

    #region Mana
    public void AdjustMana(int amount) {
        mana += amount;
        mana = Mathf.Clamp(mana, 0, EditableValuesManager.Instance.maximumMana);
        Messenger.Broadcast(Signals.PLAYER_ADJUSTED_MANA, amount);
        Messenger.Broadcast(Signals.FORCE_RELOAD_PLAYER_ACTIONS);
    }
    public int GetManaCostForInterventionAbility(SPELL_TYPE ability) {
        int tier = PlayerManager.Instance.GetSpellTier(ability);
        return PlayerManager.Instance.GetManaCostForSpell(tier);
    }
    private void RegenManaProcess() {
        if(mana < 20) {
            AdjustMana(20);
        }
    }
    #endregion

    //#region Archetype
    //public void SetArchetype(PLAYER_ARCHETYPE type) {
    //    if(archetype == null || archetype.type != type) {
    //        archetype = PlayerManager.CreateNewArchetype(type);
    //        for (int i = 0; i < archetype.spells.Count; i++) {
    //            unlearnedSpells.Remove(archetype.spells[i]);
    //        }
    //        for (int i = 0; i < archetype.afflictions.Count; i++) {
    //            unlearnedAfflictions.Remove(archetype.afflictions[i]);
    //        }
    //    }
    //}
    //public void LearnSpell(SPELL_TYPE type) {
    //    archetype.AddSpell(type);
    //    unlearnedSpells.Remove(type);
    //}
    //public void LearnAffliction(SPELL_TYPE affliction) {
    //    archetype.AddAffliction(affliction);
    //    unlearnedAfflictions.Remove(affliction);
    //}
    //#endregion

    #region Utilities
    /// <summary>
    /// Is the player currently doing something?
    /// (Casting a spell, seizing an object, etc.)
    /// </summary>
    /// <returns>True or false.</returns>
    public bool IsPerformingPlayerAction() {
        return PlayerManager.Instance.player.currentActivePlayerSpell != null
               || PlayerManager.Instance.player.seizeComponent.hasSeizedPOI
               || PlayerManager.Instance.player.currentActiveIntel != null
               || PlayerManager.Instance.player.currentActiveItem != TILE_OBJECT_TYPE.NONE
               || PlayerManager.Instance.player.currentActiveArtifact != ARTIFACT_TYPE.None;
    }
    private void OnHourStarted() {
        RegenManaProcess();
    }
    #endregion

    #region Tile Objects
    public TILE_OBJECT_TYPE currentActiveItem { get; private set; }
    public void SetCurrentlyActiveItem(TILE_OBJECT_TYPE item) {
        if (currentActiveItem != item) {
            TILE_OBJECT_TYPE previousActiveItem = currentActiveItem;
            currentActiveItem = item;
            if (currentActiveItem == TILE_OBJECT_TYPE.NONE) {
                Messenger.RemoveListener<KeyCode>(Signals.KEY_DOWN, OnItemCast);
                InputManager.Instance.SetCursorTo(InputManager.Cursor_Type.Default);
                Messenger.Broadcast(Signals.PLAYER_NO_ACTIVE_ITEM, previousActiveItem);
            } else {
                InputManager.Instance.SetCursorTo(InputManager.Cursor_Type.Check);
                Messenger.AddListener<KeyCode>(Signals.KEY_DOWN, OnItemCast);
            }
        }
    }
    private void OnItemCast(KeyCode key) {
        if (key == KeyCode.Mouse0) {
            TrySpawnItem();
        }
    }
    private void TrySpawnItem() {
        if (UIManager.Instance.IsMouseOnUI() || !InnerMapManager.Instance.isAnInnerMapShowing) {
            return; //clicked on UI;
        }
        LocationGridTile hoveredTile = InnerMapManager.Instance.GetTileFromMousePosition();
        if (hoveredTile != null && hoveredTile.objHere == null) {
            TileObject item = InnerMapManager.Instance.CreateNewTileObject<TileObject>(currentActiveItem);
            hoveredTile.structure.AddPOI(item, hoveredTile);
        }
    }
    #endregion

    #region Experience
    public void AdjustExperience(int amount) {
        experience += amount;
        if(experience < 0) {
            experience = 0;
        }
        SaveExp();
    }
    public void SetExperience(int amount) {
        experience = amount;
        if (experience < 0) {
            experience = 0;
        }
        SaveExp();
    }
    #endregion

    #region Saving
    private void SaveExp() {
        SaveManager.Instance.currentSaveDataPlayer.SetExp(experience);
    }
    //public void SaveSummons() {
    //    List<LocationStructure> kennels = playerSettlement.GetStructuresOfType(STRUCTURE_TYPE.KENNEL);
    //    List<Summon> kennelSummons = null;
    //    if(kennels != null) {
    //        for (int i = 0; i < kennels.Count; i++) {
    //            LocationStructure currKennel = kennels[i];
    //            for (int j = 0; j < currKennel.charactersHere.Count; j++) {
    //                Character character = currKennel.charactersHere[j];
    //                if(character is Summon summon) {
    //                    if(summon.gridTileLocation != null && summon.marker) {
    //                        if(kennelSummons == null) {
    //                            kennelSummons = new List<Summon>();
    //                        }
    //                        kennelSummons.Add(summon);
    //                    }
    //                }
    //            }
    //        }
    //    }
    //    if(kennelSummons != null) {
    //        SaveManager.Instance.currentSaveDataPlayer.SaveSummons(kennelSummons);
    //    }
    //}
    public void SaveTileObjects() {
        List<LocationStructure> crypts = playerSettlement.GetStructuresOfType(STRUCTURE_TYPE.CRYPT);
        List<TileObject> cryptTileObjects = null;
        if (crypts != null) {
            for (int i = 0; i < crypts.Count; i++) {
                LocationStructure currCrypt = crypts[i];
                foreach (TileObjectsAndCount tileObjectsAndCount in currCrypt.groupedTileObjects.Values) {
                    if(tileObjectsAndCount.tileObjects != null && tileObjectsAndCount.tileObjects.Count > 0) {
                        for (int j = 0; j < tileObjectsAndCount.tileObjects.Count; j++) {
                            TileObject tileObject = tileObjectsAndCount.tileObjects[j];
                            if(tileObject.gridTileLocation != null 
                                && tileObject.tileObjectType != TILE_OBJECT_TYPE.BLOCK_WALL
                                && tileObject.preplacedLocationStructure != currCrypt
                                && !tileObject.isSaved) {
                                if (cryptTileObjects == null) {
                                    cryptTileObjects = new List<TileObject>();
                                }
                                cryptTileObjects.Add(tileObject);
                            }
                        }
                    }
                }
            }
        }
        if (cryptTileObjects != null) {
            SaveManager.Instance.currentSaveDataPlayer.SaveTileObjects(cryptTileObjects);
        }
    }
    #endregion
}

[System.Serializable]
public struct DemonicLandmarkBuildingData {
    public LANDMARK_TYPE landmarkType;
    public string landmarkName;
    public int buildDuration;
    public int currentDuration;
}

[System.Serializable]
public struct DemonicLandmarkInvasionData {
    public bool beingInvaded;
    public int currentDuration;
}