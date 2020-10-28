﻿using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using TMPro;
using UnityEngine.UI;
using Traits;
using UnityEngine.Serialization;
using UtilityScripts;

public class CharacterInfoUI : InfoUIBase {
    
    [Header("Basic Info")]
    [SerializeField] private CharacterPortrait characterPortrait;
    [SerializeField] private TextMeshProUGUI nameLbl;
    [SerializeField] private TextMeshProUGUI lvlClassLbl;
    [SerializeField] private TextMeshProUGUI plansLbl;
    [SerializeField] private TextMeshProUGUI partyLbl;
    [SerializeField] private EventLabel partyEventLbl;
    [SerializeField] private LogItem plansLblLogItem;
    [SerializeField] private GameObject leaderIcon;

    [Space(10)] [Header("Location")]
    [SerializeField] private TextMeshProUGUI factionLbl;
    [SerializeField] private EventLabel factionEventLbl;
    [SerializeField] private TextMeshProUGUI currentLocationLbl;
    [SerializeField] private EventLabel currentLocationEventLbl;
    [SerializeField] private TextMeshProUGUI homeRegionLbl;
    [SerializeField] private EventLabel homeRegionEventLbl;
    [SerializeField] private TextMeshProUGUI houseLbl;
    [SerializeField] private EventLabel houseEventLbl;

    [Space(10)] [Header("Logs")] 
    [SerializeField] private LogsWindow _logsWindow;

    [Space(10)] [Header("Stats")]
    [SerializeField] private TextMeshProUGUI hpLbl;
    [SerializeField] private TextMeshProUGUI attackLbl;
    [SerializeField] private TextMeshProUGUI speedLbl;
    [SerializeField] private TextMeshProUGUI raceLbl;
    [SerializeField] private TextMeshProUGUI elementLbl;

    [Space(10)] [Header("Traits")]
    [SerializeField] private TextMeshProUGUI statusTraitsLbl;
    [SerializeField] private TextMeshProUGUI normalTraitsLbl;
    [SerializeField] private EventLabel statusTraitsEventLbl;
    [SerializeField] private EventLabel normalTraitsEventLbl;

    [Space(10)] [Header("Items")]
    [SerializeField] private TextMeshProUGUI itemsLbl;
    
    [Space(10)] [Header("Relationships")]
    [SerializeField] private EventLabel relationshipNamesEventLbl;
    [SerializeField] private TextMeshProUGUI relationshipTypesLbl;
    [SerializeField] private TextMeshProUGUI relationshipNamesLbl;
    [SerializeField] private TextMeshProUGUI relationshipValuesLbl;
    [SerializeField] private UIHoverPosition relationshipNameplateItemPosition;
    [SerializeField] private RelationshipFilterItem[] relationFilterItems;
    [SerializeField] private GameObject relationFiltersGO;
    [SerializeField] private Toggle allRelationshipFiltersToggle;
    
    [Space(10)] [Header("Mood")] 
    [SerializeField] private MarkedMeter moodMeter;
    [SerializeField] private TextMeshProUGUI moodSummary;
    
    [Space(10)] [Header("Needs")] 
    [SerializeField] private MarkedMeter energyMeter;
    [SerializeField] private MarkedMeter fullnessMeter;
    [SerializeField] private MarkedMeter happinessMeter;
    [SerializeField] private MarkedMeter hopeMeter;
    [SerializeField] private MarkedMeter staminaMeter;

    private Character _activeCharacter;
    private Character _previousCharacter;

    public Character activeCharacter => _activeCharacter;
    public Character previousCharacter => _previousCharacter;
    private List<SpellData> afflictions;
    private List<string> combatModes;
    private List<string> triggerFlawPool;
    private List<LogFiller> triggerFlawLogFillers;
    private bool aliveRelationsOnly;
    private List<RELATIONS_FILTER> filters;
    private RELATIONS_FILTER[] allFilters;

    internal override void Initialize() {
        base.Initialize();
        Messenger.AddListener<Log>(Signals.LOG_ADDED, UpdateHistory);
        Messenger.AddListener<Log>(Signals.LOG_IN_DATABASE_UPDATED, UpdateHistory);
        Messenger.AddListener<Character, Trait>(Signals.CHARACTER_TRAIT_ADDED, UpdateTraitsFromSignal);
        Messenger.AddListener<Character, Trait>(Signals.CHARACTER_TRAIT_REMOVED, UpdateTraitsFromSignal);
        Messenger.AddListener<Character, Trait>(Signals.CHARACTER_TRAIT_STACKED, UpdateTraitsFromSignal);
        Messenger.AddListener<Character, Trait>(Signals.CHARACTER_TRAIT_UNSTACKED, UpdateTraitsFromSignal);
        Messenger.AddListener<InfoUIBase>(Signals.MENU_OPENED, OnMenuOpened);
        Messenger.AddListener<InfoUIBase>(Signals.MENU_CLOSED, OnMenuClosed);
        Messenger.AddListener(Signals.ON_OPEN_SHARE_INTEL, OnOpenShareIntelMenu);
        Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        Messenger.AddListener<TileObject, Character>(Signals.CHARACTER_OBTAINED_ITEM, UpdateInventoryInfoFromSignal);
        Messenger.AddListener<TileObject, Character>(Signals.CHARACTER_LOST_ITEM, UpdateInventoryInfoFromSignal);
        Messenger.AddListener<Relatable, Relatable>(Signals.RELATIONSHIP_CREATED, OnRelationshipChanged);
        Messenger.AddListener<Relatable, Relatable>(Signals.RELATIONSHIP_TYPE_ADDED, OnRelationshipChanged);
        Messenger.AddListener<Character, Character>(Signals.OPINION_ADDED, OnOpinionChanged);
        Messenger.AddListener<Character, Character>(Signals.OPINION_REMOVED, OnOpinionChanged);
        Messenger.AddListener<Character, Character, string>(Signals.OPINION_INCREASED, OnOpinionChanged);
        Messenger.AddListener<Character, Character, string>(Signals.OPINION_DECREASED, OnOpinionChanged);

        Messenger.AddListener<Character>(Signals.UPDATE_THOUGHT_BUBBLE, UpdateThoughtBubbleFromSignal);
        Messenger.AddListener<MoodComponent>(Signals.MOOD_SUMMARY_MODIFIED, OnMoodModified);

        //normalTraitsEventLbl.SetOnClickAction(OnClickTrait);
        relationshipNamesEventLbl.SetOnClickAction(OnClickCharacter);
        
        factionEventLbl.SetOnClickAction(OnClickFaction);
        currentLocationEventLbl.SetOnClickAction(OnClickCurrentLocation);
        homeRegionEventLbl.SetOnClickAction(OnClickHomeLocation);
        houseEventLbl.SetOnClickAction(OnClickHomeStructure);
        partyEventLbl.SetOnClickAction(OnClickParty);

        moodMeter.ResetMarks();
        moodMeter.AddMark(EditableValuesManager.Instance.criticalMoodHighThreshold/100f, Color.red);
        moodMeter.AddMark(EditableValuesManager.Instance.lowMoodHighThreshold/100f, Color.yellow);

        energyMeter.ResetMarks();
        energyMeter.AddMark(CharacterNeedsComponent.REFRESHED_LOWER_LIMIT/100f, Color.green);
        energyMeter.AddMark(CharacterNeedsComponent.TIRED_UPPER_LIMIT/100f, Color.yellow);
        energyMeter.AddMark(CharacterNeedsComponent.EXHAUSTED_UPPER_LIMIT/100f, Color.red);
        
        fullnessMeter.ResetMarks();
        fullnessMeter.AddMark(CharacterNeedsComponent.FULL_LOWER_LIMIT/100f, Color.green);
        fullnessMeter.AddMark(CharacterNeedsComponent.HUNGRY_UPPER_LIMIT/100f, Color.yellow);
        fullnessMeter.AddMark(CharacterNeedsComponent.STARVING_UPPER_LIMIT/100f, Color.red);
        
        happinessMeter.ResetMarks();
        // happinessMeter.AddMark(CharacterNeedsComponent.ENTERTAINED_LOWER_LIMIT/100f, Color.green);
        happinessMeter.AddMark(CharacterNeedsComponent.BORED_UPPER_LIMIT/100f, Color.yellow);
        happinessMeter.AddMark(CharacterNeedsComponent.SULKING_UPPER_LIMIT/100f, Color.red);
        
        staminaMeter.ResetMarks();
        staminaMeter.AddMark(CharacterNeedsComponent.SPRIGHTLY_LOWER_LIMIT/100f, Color.green);
        staminaMeter.AddMark(CharacterNeedsComponent.SPENT_UPPER_LIMIT/100f, Color.yellow);
        staminaMeter.AddMark(CharacterNeedsComponent.DRAINED_UPPER_LIMIT/100f, Color.red);
        
        hopeMeter.ResetMarks();
        hopeMeter.AddMark(CharacterNeedsComponent.HOPEFUL_LOWER_LIMIT/100f, Color.green);
        hopeMeter.AddMark(CharacterNeedsComponent.DISCOURAGED_UPPER_LIMIT/100f, Color.yellow);
        hopeMeter.AddMark(CharacterNeedsComponent.HOPELESS_UPPER_LIMIT/100f, Color.red);

        _logsWindow.Initialize();

        InitializeRelationships();
        
        afflictions = new List<SpellData>();
        triggerFlawPool = new List<string>();
        triggerFlawLogFillers = new List<LogFiller>();
        ConstructCombatModes();
    }

    #region Overrides
    public override void CloseMenu() {
        base.CloseMenu();
        Selector.Instance.Deselect();
        Character character = _activeCharacter;
        _activeCharacter = null;
        if (character != null && ReferenceEquals(character.marker, null) == false) {
            if (InnerMapCameraMove.Instance != null && InnerMapCameraMove.Instance.target == character.marker.gameObject.transform) {
                InnerMapCameraMove.Instance.CenterCameraOn(null);
            }
            character.marker.UpdateNameplateElementsState();
        }
    }
    public override void OpenMenu() {
        _previousCharacter = _activeCharacter;
        _activeCharacter = _data as Character;
        base.OpenMenu();
        if (_previousCharacter != null && _previousCharacter.marker != null) {
            _previousCharacter.marker.UpdateNameplateElementsState();
        }
        if (UIManager.Instance.IsShareIntelMenuOpen()) {
            backButton.interactable = false;
        }
        if (UIManager.Instance.IsObjectPickerOpen()) {
            UIManager.Instance.HideObjectPicker();
        }
        if (_activeCharacter.marker && _activeCharacter.marker.transform != null) {
            Selector.Instance.Select(_activeCharacter, _activeCharacter.marker.transform);
            _activeCharacter.marker.UpdateNameplateElementsState();
        }
        UpdateCharacterInfo();
        UpdateTraits();
        UpdateRelationships();
        UpdateInventoryInfo();
        _logsWindow.OnParentMenuOpened(_activeCharacter.persistentID);
        UpdateAllHistoryInfo();
        ResetAllScrollPositions();
        UpdateMoodSummary();
    }
    protected override void OnExecutePlayerAction(PlayerAction action) {
        base.OnExecutePlayerAction(action);
        if(action.type == SPELL_TYPE.CHANGE_COMBAT_MODE) {
            SetCombatModeUIPosition(action);
        }
    }
    protected override void LoadActions(IPlayerActionTarget target) {
        UtilityScripts.Utilities.DestroyChildren(actionsTransform);
        activeActionItems.Clear();
        for (int i = 0; i < target.actions.Count; i++) {
            PlayerAction action = PlayerSkillManager.Instance.GetPlayerActionData(target.actions[i]);
            if (action.IsValid(target) && PlayerManager.Instance.player.playerSkillComponent.CanDoPlayerAction(action.type)) {
                //if (action.actionName == PlayerDB.Combat_Mode_Action) {
                //    action.SetLabelText(action.actionName + ": " + UtilityScripts.Utilities.NotNormalizedConversionEnumToString(activeCharacter.combatComponent.combatMode.ToString()));
                //}
                ActionItem actionItem = AddNewAction(action, target);
                actionItem.SetInteractable(action.CanPerformAbilityTo(target) && !PlayerManager.Instance.player.seizeComponent.hasSeizedPOI);
                actionItem.ForceUpdateCooldown();
            }
        }
    }
    #endregion

    #region Utilities
    // private void InitializeLogsMenu() {
    //     logHistoryItems = new LogHistoryItem[CharacterManager.MAX_HISTORY_LOGS];
    //     //populate history logs table
    //     for (int i = 0; i < CharacterManager.MAX_HISTORY_LOGS; i++) {
    //         GameObject newLogItem = ObjectPoolManager.Instance.InstantiateObjectFromPool(logHistoryPrefab.name, Vector3.zero, Quaternion.identity, historyScrollView.content);
    //         logHistoryItems[i] = newLogItem.GetComponent<LogHistoryItem>();
    //         newLogItem.transform.localScale = Vector3.one;
    //         newLogItem.SetActive(true);
    //     }
    //     for (int i = 0; i < logHistoryItems.Length; i++) {
    //         logHistoryItems[i].gameObject.SetActive(false);
    //     }
    // }
    private void ResetAllScrollPositions() {
        _logsWindow.ResetScrollPosition();
    }
    public void UpdateCharacterInfo() {
        if (_activeCharacter == null) {
            return;
        }
        UpdatePortrait();
        UpdateBasicInfo();
        UpdateStatInfo();
        UpdateLocationInfo();
        UpdateMoodMeter();
        UpdateNeedMeters();
        UpdatePartyInfo();
    }
    private void UpdatePortrait() {
        characterPortrait.GeneratePortrait(_activeCharacter);
    }
    public void UpdateBasicInfo() {
        nameLbl.text = _activeCharacter.visuals.GetNameplateName();
        lvlClassLbl.text = _activeCharacter.raceClassName;
        // leaderIcon.SetActive(_activeCharacter.isFactionLeader || _activeCharacter.isSettlementRuler);
        UpdateThoughtBubble();
    }
    public void UpdateThoughtBubble() {
        plansLbl.text = activeCharacter.visuals.GetThoughtBubble();
        // if (log != null) {
        //     plansLblLogItem.SetLog(log);
        // }
    }
    public void OnHoverLeaderIcon() {
        string message = string.Empty;
        if (activeCharacter.isSettlementRuler) {
            message = $"<b>{activeCharacter.name}</b> is the Settlement Ruler of <b>{activeCharacter.ruledSettlement.name}</b>\n";
        } 
        if (activeCharacter.isFactionLeader) {
            message += $"<b>{activeCharacter.name}</b> is the Faction Leader of <b>{activeCharacter.faction.name}</b>";
        }
        UIManager.Instance.ShowSmallInfo(message);
    }
    public void OnHoverExitLeaderIcon() {
        UIManager.Instance.HideSmallInfo();
    }
    #endregion

    #region Stats
    private void UpdateStatInfo() {
        hpLbl.text = $"{_activeCharacter.currentHP.ToString()}/{_activeCharacter.maxHP.ToString()}";
        attackLbl.text = $"{_activeCharacter.combatComponent.attack.ToString()}";
        speedLbl.text =  $"{_activeCharacter.combatComponent.attackSpeed / 1000f}s";
        raceLbl.text = $"{UtilityScripts.GameUtilities.GetNormalizedSingularRace(_activeCharacter.race)}";
        elementLbl.text = $"{_activeCharacter.combatComponent.elementalDamage.type.ToString()}";
        //if(characterPortrait.character != null) {
        //    characterPortrait.UpdateLvl();
        //}
    }
    #endregion

    #region Location
    private void UpdateLocationInfo() {
        factionLbl.text = _activeCharacter.faction != null ? $"<link=\"faction\">{UtilityScripts.Utilities.ColorizeAndBoldName(_activeCharacter.faction.name)}</link>" : "Factionless";
        currentLocationLbl.text = _activeCharacter.currentRegion != null ? $"{_activeCharacter.currentRegion.name}" : "None";
        homeRegionLbl.text = _activeCharacter.homeRegion != null ? $"{_activeCharacter.homeRegion.name}" : "Homeless";
        //currentLocationLbl.text = $"<link=\"currLocation\">{UtilityScripts.Utilities.ColorizeName(_activeCharacter.currentRegion.name)}</link>";
        //homeRegionLbl.text = _activeCharacter.homeRegion != null ? $"<link=\"home\">{UtilityScripts.Utilities.ColorizeName(_activeCharacter.homeRegion.name)}</link>" : "Homeless";
        houseLbl.text = _activeCharacter.homeStructure != null ? $"<link=\"house\">{UtilityScripts.Utilities.ColorizeAndBoldName(_activeCharacter.homeStructure.name)}</link>" : "Homeless";
    }
    private void OnClickFaction(object obj) {
        UIManager.Instance.ShowFactionInfo(activeCharacter.faction);
    }
    private void OnClickCurrentLocation(object obj) {
        UIManager.Instance.ShowRegionInfo(activeCharacter.currentRegion);
    }
    private void OnClickHomeLocation(object obj) {
        UIManager.Instance.ShowRegionInfo(activeCharacter.homeRegion);
    }
    private void OnClickHomeStructure(object obj) {
        if (activeCharacter.homeStructure != null) {
            activeCharacter.homeStructure.CenterOnStructure();
        }
        
    }
    #endregion

    #region Traits
    private void UpdateTraitsFromSignal(Character character, Trait trait) {
        if(_activeCharacter == null || _activeCharacter != character) {
            return;
        }
        UpdateTraits();
        UpdateThoughtBubble();
    }
    private void UpdateThoughtBubbleFromSignal(Character character) {
        if (isShowing && _activeCharacter == character) {
            UpdateThoughtBubble();
        }
    }
    private void UpdateTraits() {
        string statusTraits = string.Empty;
        string normalTraits = string.Empty;

        for (int i = 0; i < _activeCharacter.traitContainer.statuses.Count; i++) {
            Status currStatus = _activeCharacter.traitContainer.statuses[i];
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            // if (currStatus.isHidden) {
            //     continue; //skip
            // }
#else
            if (currStatus.isHidden) {
                continue; //skip
            }
#endif
            string color = UIManager.normalTextColor;
            if (currStatus.moodEffect > 0) {
                color = UIManager.buffTextColor;
            } else if (currStatus.moodEffect < 0) {
                color = UIManager.flawTextColor;
            }
            if (!string.IsNullOrEmpty(statusTraits)) {
                statusTraits = $"{statusTraits}, ";
            }
            statusTraits = $"{statusTraits}<b><color={color}><link=\"{i}\">{currStatus.GetNameInUI(activeCharacter)}</link></color></b>";
        }
        for (int i = 0; i < _activeCharacter.traitContainer.traits.Count; i++) {
            Trait currTrait = _activeCharacter.traitContainer.traits[i];
            if (currTrait.isHidden) {
                continue; //skip
            }
            string color = UIManager.normalTextColor;
            if (currTrait.type == TRAIT_TYPE.BUFF) {
                color = UIManager.buffTextColor;
            } else if (currTrait.type == TRAIT_TYPE.FLAW) {
                color = UIManager.flawTextColor;
            }
            if (!string.IsNullOrEmpty(normalTraits)) {
                normalTraits = $"{normalTraits}, ";
            }
            normalTraits = $"{normalTraits}<b><color={color}><link=\"{i}\">{currTrait.GetNameInUI(activeCharacter)}</link></color></b>";
        }

        statusTraitsLbl.text = string.Empty;
        if (string.IsNullOrEmpty(statusTraits) == false) {
            //character has status traits
            statusTraitsLbl.text = statusTraits; 
        }
        normalTraitsLbl.text = string.Empty;
        if (string.IsNullOrEmpty(normalTraits) == false) {
            //character has normal traits
            normalTraitsLbl.text = normalTraits;
        }
    }
    public void OnHoverTrait(object obj) {
        if (obj is string text) {
            int index = int.Parse(text);
            Trait trait = activeCharacter.traitContainer.traits.ElementAtOrDefault(index);
            if (trait != null) {
                string info = trait.descriptionInUI;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                info += $"\n{trait.GetTestingData(activeCharacter)}";
#endif
                UIManager.Instance.ShowSmallInfo(info);    
            }
        }
    }
    public void OnHoverStatus(object obj) {
        if (obj is string text) {
            int index = int.Parse(text);
            Trait trait = activeCharacter.traitContainer.statuses.ElementAtOrDefault(index);
            if (trait != null) {
                string info = trait.descriptionInUI;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                info += $"\n{trait.GetTestingData(activeCharacter)}";
#endif
                UIManager.Instance.ShowSmallInfo(info);    
            }
        }
    }
    public void OnHoverOutTrait() {
        UIManager.Instance.HideSmallInfo();
    }
    #endregion

    #region Items
    private void UpdateInventoryInfoFromSignal(TileObject item, Character character) {
        if (isShowing && _activeCharacter == character) {
            UpdateInventoryInfo();
        }
    }
    private void UpdateInventoryInfo() {
        itemsLbl.text = string.Empty;
        for (int i = 0; i < _activeCharacter.items.Count; i++) {
            TileObject currInventoryItem = _activeCharacter.items[i];
            itemsLbl.text = $"{itemsLbl.text}{currInventoryItem.name}";
            if (i < _activeCharacter.items.Count - 1) {
                itemsLbl.text = $"{itemsLbl.text}, ";
            }
        }
    }
    #endregion

    #region History
    private void UpdateHistory(Log log) {
        if (isShowing && log.IsInvolved(_activeCharacter)) {
            UpdateAllHistoryInfo();
        }
    }
    public void UpdateAllHistoryInfo() {
        _logsWindow.UpdateAllHistoryInfo();
    }
    #endregion   

    #region Listeners
    private void OnMenuOpened(InfoUIBase openedBase) {
        //if (this.isShowing) {
        //    if (openedMenu is PartyInfoUI) {
        //        CheckIfMenuShouldBeHidden();
        //    }
        //}
    }
    private void OnMenuClosed(InfoUIBase closedBase) {
        //if (this.isShowing) {
        //    if (closedMenu is PartyInfoUI) {
        //        CheckIfMenuShouldBeHidden();
        //    }
        //}
    }
    private void OnOpenShareIntelMenu() {
        backButton.interactable = false;
    }
    //private void OnCloseShareIntelMenu() { }
    //private void OnCharacterChangedAlterEgo(Character character) {
    //    if (isShowing && activeCharacter == character) {
    //        UpdateCharacterInfo();
    //        UpdateTraits();
    //    }
    //}
    private void OnCharacterDied(Character character) {
        if (isShowing) {
            if (activeCharacter.id == character.id) {
                InnerMapCameraMove.Instance.CenterCameraOn(null);    
            }
            if (activeCharacter.relationshipContainer.HasRelationshipWith(character)) {
                UpdateRelationships();
            }
        }
    }
    #endregion

    #region For Testing
    public void ShowCharacterTestingInfo() {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        string summary = $"Home structure: {activeCharacter.homeStructure?.ToString() ?? "None"}" ?? "None";
        summary = $"{summary} {$"Territories: {activeCharacter.territories?.Count.ToString() ?? "None"}"}";
        summary = $"{summary} {$"Current structure: {activeCharacter.currentStructure}" ?? "None"}";
        summary = $"{summary} {"POI State: " + activeCharacter.state.ToString()}";
        summary = $"{summary} {"Do Not Get Hungry: " + activeCharacter.needsComponent.doNotGetHungry.ToString()}";
        summary = $"{summary} {"Do Not Get Tired: " + activeCharacter.needsComponent.doNotGetTired.ToString()}";
        summary = $"{summary} {"Do Not Get Bored: " + activeCharacter.needsComponent.doNotGetBored.ToString()}";
        summary = $"{summary} {"Do Not Recover HP: " + activeCharacter.doNotRecoverHP.ToString()}";
        summary = $"{summary} {"Can Move: " + activeCharacter.canMove.ToString()}";
        summary = $"{summary} {"Can Witness: " + activeCharacter.canWitness.ToString()}";
        summary = $"{summary} {"Can Be Attacked: " + activeCharacter.canBeAttacked.ToString()}";
        summary = $"{summary} {"Can Perform: " + activeCharacter.canPerform.ToString()}";
        summary = $"{summary} {"Is Sociable: " + activeCharacter.isSociable.ToString()}";
        summary = $"{summary} {"Is Running: " + activeCharacter.movementComponent.isRunning.ToString()}";
        summary = $"{summary} {"POI State: " + activeCharacter.state.ToString()}";
        summary = $"{summary} {"Personal Religion: " + activeCharacter.religionComponent.religion.ToString()}";
        summary = $"{summary}{"\nFullness Time: " + (activeCharacter.needsComponent.fullnessForcedTick == 0 ? "N/A" : GameManager.ConvertTickToTime(activeCharacter.needsComponent.fullnessForcedTick))}";
        summary = $"{summary}{"\nTiredness Time: " + (activeCharacter.needsComponent.tirednessForcedTick == 0 ? "N/A" : GameManager.ConvertTickToTime(activeCharacter.needsComponent.tirednessForcedTick))}";
        summary = $"{summary}{"\nHappiness Time: " + (activeCharacter.needsComponent.happinessSecondForcedTick == 0 ? "N/A" : GameManager.ConvertTickToTime(activeCharacter.needsComponent.happinessSecondForcedTick))} - Satisfied Schedule Today ({activeCharacter.needsComponent.hasForcedSecondHappiness.ToString()})";
        summary = $"{summary}{"\nRemaining Sleep Ticks: " + activeCharacter.needsComponent.currentSleepTicks.ToString()}";
        //summary = $"{summary}{("\nFood: " + activeCharacter.food.ToString())}";
        summary = $"{summary}{"\nSexuality: " + activeCharacter.sexuality.ToString()}";
        // summary = $"{summary}{("\nMood: " + activeCharacter.moodComponent.moodValue + "/100" + "(" + activeCharacter.moodComponent.moodState.ToString() + ")")}";
        // summary = $"{summary}{("\nHP: " + activeCharacter.currentHP.ToString() + "/" + activeCharacter.maxHP.ToString())}";
        summary = $"{summary}{"\nAttack Range: " + activeCharacter.characterClass.attackRange.ToString(CultureInfo.InvariantCulture)}";
        summary = $"{summary}{"\nAttack Speed: " + activeCharacter.combatComponent.attackSpeed.ToString()}";
        summary = $"{summary}{"\nCombat Mode: " + activeCharacter.combatComponent.combatMode.ToString()}";
        summary = $"{summary}{"\nElemental Type: " + activeCharacter.combatComponent.elementalDamage.name}";
        summary = $"{summary}{"\nPrimary Job: " + activeCharacter.jobComponent.primaryJob.ToString()}";
        summary = $"{summary}{"\nPriority Jobs: " + activeCharacter.jobComponent.GetPriorityJobs()}";
        summary = $"{summary}{"\nSecondary Jobs: " + activeCharacter.jobComponent.GetSecondaryJobs()}";
        summary = $"{summary}{"\nAble Jobs: " + activeCharacter.jobComponent.GetAbleJobs()}";
        summary = $"{summary}{"\nAdditional Able Jobs: " + activeCharacter.jobComponent.GetAdditionalAbleJobs()}";
        summary = $"{summary}{("\nParty: " + (activeCharacter.partyComponent.hasParty ? activeCharacter.partyComponent.currentParty.partyName : "None") + ", State: " + activeCharacter.partyComponent.currentParty?.partyState.ToString() + ", Members: " + activeCharacter.partyComponent.currentParty?.members.Count)}";
        summary = $"{summary}{"\nPrimary Bed: " + (activeCharacter.tileObjectComponent.primaryBed != null ? activeCharacter.tileObjectComponent.primaryBed.name : "None")}";
        summary = $"{summary}{"\nEnable Digging: " + activeCharacter.movementComponent.enableDigging.ToString()}";
        summary = $"{summary}{"\nAvoid Settlements: " + activeCharacter.movementComponent.avoidSettlements.ToString()}";

        if (activeCharacter.stateComponent.currentState != null) {
            summary = $"{summary}\nCurrent State: {activeCharacter.stateComponent.currentState}";
            summary = $"{summary}\n\tDuration in state: {activeCharacter.stateComponent.currentState.currentDuration.ToString()}/{activeCharacter.stateComponent.currentState.duration.ToString()}";
        }
        
        summary += "\nBehaviour Components: ";
        for (int i = 0; i < activeCharacter.behaviourComponent.currentBehaviourComponents.Count; i++) {
            CharacterBehaviourComponent component = activeCharacter.behaviourComponent.currentBehaviourComponents[i];
            summary += $"{component}, ";
        }
        
        summary += "\nInterested Items: ";
        for (int i = 0; i < activeCharacter.interestedItemNames.Count; i++) {
            summary += $"{activeCharacter.interestedItemNames[i]}, ";
        }
        
        summary += "\nPersonal Job Queue: ";
        if (activeCharacter.jobQueue.jobsInQueue.Count > 0) {
            for (int i = 0; i < activeCharacter.jobQueue.jobsInQueue.Count; i++) {
                JobQueueItem poi = activeCharacter.jobQueue.jobsInQueue[i];
                summary += $"{poi}, ";
            }
        } else {
            summary += "None";
        }
        
        // summary += "\nCharacters with opinion: ";
        // if (activeCharacter.relationshipContainer.charactersWithOpinion.Count > 0) {
        //     for (int i = 0; i < activeCharacter.relationshipContainer.charactersWithOpinion.Count; i++) {
        //         Character characterWithOpinion = activeCharacter.relationshipContainer.charactersWithOpinion[i];
        //         summary += $"{characterWithOpinion}, ";
        //     }
        // } else {
        //     summary += "None";
        // }
        // summary += "\n" + activeCharacter.needsComponent.GetNeedsSummary();
        UIManager.Instance.ShowSmallInfo(summary);
#endif
    }
    public void HideCharacterTestingInfo() {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        UIManager.Instance.HideSmallInfo();
#endif
    }
    #endregion

    #region Relationships
    private void InitializeRelationships() {
        for (int i = 0; i < relationFilterItems.Length; i++) {
            RelationshipFilterItem item = relationFilterItems[i];
            item.Initialize(OnToggleRelationshipFilter);
            item.SetIsOnWithoutNotify(true);
        }
        allRelationshipFiltersToggle.SetIsOnWithoutNotify(true);
        allFilters = CollectionUtilities.GetEnumValues<RELATIONS_FILTER>();
        filters = new List<RELATIONS_FILTER>(allFilters);
        aliveRelationsOnly = true;
    }
    public void OnToggleShowOnlyAliveRelations(bool isOn) {
        aliveRelationsOnly = isOn;
        UpdateRelationships();
    }
    public void OnToggleShowAll(bool isOn) {
        filters.Clear();
        if (isOn) {
            filters.AddRange(allFilters);
        }
        for (int i = 0; i < relationFilterItems.Length; i++) {
            RelationshipFilterItem item = relationFilterItems[i];
            item.SetIsOnWithoutNotify(isOn);
        }
        UpdateRelationships();
    }
    private void OnToggleRelationshipFilter(bool isOn, RELATIONS_FILTER filter) {
        if (isOn) {
            filters.Add(filter);
        } else {
            filters.Remove(filter);
        }
        allRelationshipFiltersToggle.SetIsOnWithoutNotify(filters.Count == allFilters.Length);
        UpdateRelationships();
    }
    public void ToggleRelationFilters() {
        relationFiltersGO.SetActive(!relationFiltersGO.activeSelf);
    }
    private void UpdateRelationships() {
        relationshipTypesLbl.text = string.Empty;
        relationshipNamesLbl.text = string.Empty;
        relationshipValuesLbl.text = string.Empty;
        
        HashSet<int> filteredKeys = new HashSet<int>();
        foreach (var kvp in activeCharacter.relationshipContainer.relationships) {
            if (DoesRelationshipMeetFilters(kvp.Key, kvp.Value)) {
                filteredKeys.Add(kvp.Key);
            }
        }
        
        Dictionary<int, IRelationshipData> orderedRels = _activeCharacter.relationshipContainer.relationships
            .OrderByDescending(k => k.Value.opinions.totalOpinion)
            .ToDictionary(k => k.Key, v => v.Value);
        List<int> allKeys = _activeCharacter.relationshipContainer.relationships.Keys.ToList();
        
        for (int i = 0; i < orderedRels.Keys.Count; i++) {
            int targetID = orderedRels.Keys.ElementAt(i);
            if (filteredKeys.Contains(targetID)) {
                int actualIndex = allKeys.IndexOf(targetID);
                IRelationshipData relationshipData = _activeCharacter.relationshipContainer.GetRelationshipDataWith(targetID);
                string relationshipName = _activeCharacter.relationshipContainer.GetRelationshipNameWith(targetID);
                Character target = CharacterManager.Instance.GetCharacterByID(targetID);
                
                relationshipTypesLbl.text += $"{relationshipName}\n";
            
                int opinionOfOther = 0;
                string opinionText;
                if (target != null && target.relationshipContainer.HasRelationshipWith(activeCharacter)) {
                    opinionOfOther = target.relationshipContainer.GetTotalOpinion(activeCharacter);
                    opinionText = GetOpinionText(opinionOfOther);
                } else {
                    opinionText = "???";
                }
            
                relationshipNamesLbl.text += $"<link=\"{actualIndex.ToString()}\">{UtilityScripts.Utilities.ColorizeAndBoldName(relationshipData.targetName)}</link>\n";
                relationshipValuesLbl.text += $"<link=\"{actualIndex.ToString()}\">" +
                                              $"<color={BaseRelationshipContainer.OpinionColor(activeCharacter.relationshipContainer.GetTotalOpinion(targetID))}> " +
                                              $"{GetOpinionText(activeCharacter.relationshipContainer.GetTotalOpinion(targetID))}</color> " +
                                              $"<color={BaseRelationshipContainer.OpinionColor(opinionOfOther)}>({opinionText})</color></link>\n";
            }
        }
        
        // for (int i = 0; i < orderedRels.Keys.Count; i++) {
        //     int targetID = orderedRels.Keys.ElementAt(i);
        //     int actualIndex = keys.IndexOf(targetID);
        //     IRelationshipData relationshipData = _activeCharacter.relationshipContainer.GetRelationshipDataWith(targetID);
        //     string relationshipName = _activeCharacter.relationshipContainer.GetRelationshipNameWith(targetID);
        //     Character target = CharacterManager.Instance.GetCharacterByID(targetID);
        //
        //     
        //     //Hide relationship in UI if both consider each other an Acquaintance and no other special relationships (relative, lover, etc)
        //     //Reference: https://trello.com/c/7uR4Iwya/1874-hide-relationship-in-ui-if-both-consider-each-other-an-acquaintance-and-no-other-special-relationships-relative-lover-etc
        //     bool shouldShowRelationship = relationshipName != RelationshipManager.Acquaintance;
        //     if (!shouldShowRelationship) {
        //         //if active character considers target an acquaintance, then check if target also considers active character as an Acquaintance  
        //         if (target != null) {
        //             string targetRelationshipName = target.relationshipContainer.GetRelationshipNameWith(_activeCharacter.id);
        //             shouldShowRelationship = targetRelationshipName != RelationshipManager.Acquaintance;
        //         }    
        //     }
        //
        //     if (!shouldShowRelationship) {
        //         continue; //skip
        //     }
        //
        //     relationshipTypesLbl.text += $"{relationshipName}\n";
        //     
        //     int opinionOfOther = 0;
        //     string opinionText;
        //     if (target != null && target.relationshipContainer.HasRelationshipWith(activeCharacter)) {
        //         opinionOfOther = target.relationshipContainer.GetTotalOpinion(activeCharacter);
        //         opinionText = GetOpinionText(opinionOfOther);
        //     } else {
        //         opinionText = "???";
        //     }
        //     
        //     relationshipNamesLbl.text += $"<link=\"{actualIndex.ToString()}\">{UtilityScripts.Utilities.ColorizeAndBoldName(relationshipData.targetName)}</link>\n";
        //     relationshipValuesLbl.text +=
        //         $"<link=\"{actualIndex.ToString()}\"><color={BaseRelationshipContainer.OpinionColor(activeCharacter.relationshipContainer.GetTotalOpinion(targetID))}> {GetOpinionText(activeCharacter.relationshipContainer.GetTotalOpinion(targetID))}</color> <color={BaseRelationshipContainer.OpinionColor(opinionOfOther)}>({opinionText})</color></link>\n";
        // }
    }
    private bool DoesRelationshipMeetFilters(int id, IRelationshipData data) {
        Character target = CharacterManager.Instance.GetCharacterByID(id);
        if (target != null) {
            if (aliveRelationsOnly && target.isDead) {
                return false;
            }
            return DoesRelationshipMeetAnyFilter(data);
        } else {
            //did not check aliveRelationsOnly because unspawned characters will be shown regardless
            return DoesRelationshipMeetAnyFilter(data);
        }
        return true;
    }
    private bool DoesRelationshipMeetAnyFilter(IRelationshipData data) {
        if (filters.Count == 0) {
            return false; //if no filters were provided, then HIDE all relationships
        }
        //loop through enabled filters, if relationships meets any filter then return true.
        bool hasMetAFilter = false;
        string opinionLabel = data.opinions.GetOpinionLabel();
        for (int i = 0; i < filters.Count; i++) {
            RELATIONS_FILTER filter = filters[i];
            switch (filter) {
                case RELATIONS_FILTER.Enemies:
                    if (opinionLabel == RelationshipManager.Enemy) {
                        hasMetAFilter = true;
                    }
                    break;
                case RELATIONS_FILTER.Rivals:
                    if (opinionLabel == RelationshipManager.Rival) {
                        hasMetAFilter = true;
                    }
                    break;
                case RELATIONS_FILTER.Acquaintances:
                    if (opinionLabel == RelationshipManager.Acquaintance) {
                        hasMetAFilter = true;
                    }
                    break;
                case RELATIONS_FILTER.Friends:
                    if (opinionLabel == RelationshipManager.Friend) {
                        hasMetAFilter = true;
                    }
                    break;
                case RELATIONS_FILTER.Close_Friends:
                    if (opinionLabel == RelationshipManager.Close_Friend) {
                        hasMetAFilter = true;
                    }
                    break;
                case RELATIONS_FILTER.Relatives:
                    if (data.IsFamilyMember()) {
                        hasMetAFilter = true;
                    }
                    break;
                case RELATIONS_FILTER.Lovers:
                    if (data.IsLover()) {
                        hasMetAFilter = true;
                    }
                    break;
            }
            if (hasMetAFilter) {
                return true;
            }
        }
        return false; //no filters were met.
    }
    public void OnHoverRelationshipValue(object obj) {
        if (obj is string) {
            string text = (string)obj;
            int index = int.Parse(text);
            int id = _activeCharacter.relationshipContainer.relationships.Keys.ElementAtOrDefault(index);
            ShowOpinionData(id);
        }
    }
    public void OnHoverRelationshipName(object obj) {
        if (obj is string text) {
            int index = int.Parse(text);
            int characterID = _activeCharacter.relationshipContainer.relationships.Keys.ElementAtOrDefault(index);
            OnHoverCharacterNameInRelationships(characterID);
        }
    }
    private void OnOpinionChanged(Character owner, Character target, string reason) {
        if (isShowing && (owner == activeCharacter || target == activeCharacter)) {
            UpdateRelationships();
        }
    }
    private void OnOpinionChanged(Character owner, Character target) {
        if (isShowing && (owner == activeCharacter || target == activeCharacter)) {
            UpdateRelationships();
        }
    }
    private void OnRelationshipChanged(Relatable owner, Relatable target) {
        if (isShowing && (owner == activeCharacter || target == activeCharacter)) {
            UpdateRelationships();
        }
    }
    private void ShowOpinionData(int targetID) {
        IRelationshipData targetData = _activeCharacter.relationshipContainer.GetRelationshipDataWith(targetID);
        Character target = CharacterManager.Instance.GetCharacterByID(targetID);

        string summary = $"{activeCharacter.name}'s opinion of {targetData.targetName}";
        summary += "\n---------------------";
        Dictionary<string, int> opinions = activeCharacter.relationshipContainer.GetOpinionData(targetID).allOpinions;
        foreach (KeyValuePair<string, int> kvp in opinions) {
            summary += $"\n{kvp.Key}: <color={BaseRelationshipContainer.OpinionColorNoGray(kvp.Value)}>{GetOpinionText(kvp.Value)}</color>";
        }
        summary += "\n---------------------";
        summary +=
            $"\nTotal: <color={BaseRelationshipContainer.OpinionColorNoGray(targetData.opinions.totalOpinion)}>{GetOpinionText(activeCharacter.relationshipContainer.GetTotalOpinion(targetID))}</color>";
        if (target != null) {
            int opinionOfOther = target.relationshipContainer.GetTotalOpinion(activeCharacter);
            summary +=
                $"\n{targetData.targetName}'s opinion of {activeCharacter.name}: <color={BaseRelationshipContainer.OpinionColorNoGray(opinionOfOther)}>{GetOpinionText(opinionOfOther)}</color>";
        } else {
            summary += $"\n{targetData.targetName}'s opinion of {activeCharacter.name}: ???</color>";
        }
        
        summary +=
            $"\n\nCompatibility: {RelationshipManager.Instance.GetCompatibilityBetween(activeCharacter, targetID).ToString()}";
        summary +=
            $"\nState Awareness: {UtilityScripts.Utilities.NotNormalizedConversionEnumToString(targetData.awareness.state.ToString())}";
        UIManager.Instance.ShowSmallInfo(summary);
    }
    public void HideRelationshipData() {
        UIManager.Instance.HideSmallInfo();
    }
    private string GetOpinionText(int number) {
        if (number < 0) {
            return $"{number.ToString()}";
        }
        return $"+{number.ToString()}";
    }
    private void OnClickCharacter(object obj) {
        if (obj is string) {
            string text = (string)obj;
            int index = int.Parse(text);
            Character target = CharacterManager.Instance.GetCharacterByID(_activeCharacter.relationshipContainer
                .relationships.Keys.ElementAtOrDefault(index));
            if (target != null) {
                UIManager.Instance.ShowCharacterInfo(target,true);    
            }
        }
    }
    private void OnHoverCharacterNameInRelationships(int id) {
        Character target = CharacterManager.Instance.GetCharacterByID(id);
        if (target != null) {
            UIManager.Instance.HideSmallInfo();
            UIManager.Instance.ShowCharacterNameplateTooltip(target, relationshipNameplateItemPosition);
        } else {
            //character has not yet been spawned
            IRelationshipData relationshipData = _activeCharacter.relationshipContainer.relationships[id];
            UIManager.Instance.ShowSmallInfo($"{relationshipData.targetName} is not yet in this region.", relationshipNameplateItemPosition);
            UIManager.Instance.HideCharacterNameplateTooltip();
        }
    }
    public void HideRelationshipNameplate() {
        UIManager.Instance.HideSmallInfo();
        UIManager.Instance.HideCharacterNameplateTooltip();
    }
    #endregion

    #region Afflict
    public void ShowAfflictUI() {
        afflictions.Clear();
        List<SPELL_TYPE> afflictionTypes = PlayerManager.Instance.player.playerSkillComponent.afflictions;
        for (int i = 0; i < afflictionTypes.Count; i++) {
            SPELL_TYPE spellType = afflictionTypes[i];
            SpellData spellData = PlayerSkillManager.Instance.GetPlayerSpellData(spellType);
            afflictions.Add(spellData);
        }
        UIManager.Instance.ShowClickableObjectPicker(afflictions, ActivateAfflictionConfirmation, null, CanActivateAffliction,
            "Select Affliction", OnHoverAffliction, OnHoverOutAffliction, 
            portraitGetter: null, showCover: true, layer: 25, shouldShowConfirmationWindowOnPick: true, asButton: true);
    }
    private Sprite GetAfflictionPortrait(string str) {
        return PlayerManager.Instance.GetJobActionSprite(UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(str));
    }
    private void ActivateAfflictionConfirmation(object o) {
        SpellData affliction = (SpellData)o;
        SPELL_TYPE afflictionType = affliction.type;
        string afflictionName = UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(afflictionType.ToString());
        UIManager.Instance.ShowYesNoConfirmation("Affliction Confirmation",
            "Are you sure you want to afflict " + afflictionName + "?", () => ActivateAffliction(afflictionType),
            layer: 26, showCover: true, pauseAndResume: true);
    }
    private void ActivateAffliction(SPELL_TYPE afflictionType) {
        UIManager.Instance.HideObjectPicker();
        PlayerSkillManager.Instance.GetAfflictionData(afflictionType).ActivateAbility(activeCharacter);
        PlayerSkillManager.Instance.GetPlayerActionData(SPELL_TYPE.AFFLICT).OnExecuteSpellActionAffliction();
    }
    private bool CanActivateAffliction(SpellData spellData) {
        // if (WorldConfigManager.Instance.isTutorialWorld) {
        //     return WorldConfigManager.Instance.availableSpellsInTutorial.Contains(spellData.type) 
        //            && spellData.CanPerformAbilityTowards(activeCharacter);
        // }
        return spellData.CanPerformAbilityTowards(activeCharacter);
    }
    private void OnHoverAffliction(SpellData spellData) {
        PlayerUI.Instance.OnHoverSpell(spellData);
    }
    private void OnHoverOutAffliction(SpellData spellData) {
        UIManager.Instance.HideSmallInfo();
        PlayerUI.Instance.OnHoverOutSpell(null);
    }
    #endregion

    #region Trigger Flaw
    public void ShowTriggerFlawUI() {
        triggerFlawPool.Clear();
        for (int i = 0; i < activeCharacter.traitContainer.traits.Count; i++) {
            Trait trait = activeCharacter.traitContainer.traits[i];
            if(trait.type == TRAIT_TYPE.FLAW) {
                triggerFlawPool.Add(trait.name);
            }
        }
        UIManager.Instance.ShowClickableObjectPicker(triggerFlawPool, ActivateTriggerFlawConfirmation, null, CanActivateTriggerFlaw,
            $"Select Flaw ({PlayerSkillManager.Instance.GetPlayerActionData(SPELL_TYPE.TRIGGER_FLAW).manaCost.ToString()} {UtilityScripts.Utilities.ManaIcon()})", 
            OnHoverEnterFlaw, OnHoverExitFlaw, showCover: true, layer: 25, shouldShowConfirmationWindowOnPick: true, asButton: true, identifier: "Trigger Flaw");
    }
    private void ActivateTriggerFlawConfirmation(object o) {
        string traitName = (string) o;
        Trait trait = activeCharacter.traitContainer.GetTraitOrStatus<Trait>(traitName);
        string question = "Are you sure you want to trigger " + traitName + "?";
        string effect = $"<b>Effect</b>: {trait.GetTriggerFlawEffectDescription(activeCharacter, "flaw_effect")}";
        string manaCost = $"{PlayerSkillManager.Instance.GetPlayerActionData(SPELL_TYPE.TRIGGER_FLAW).manaCost.ToString()} {UtilityScripts.Utilities.ManaIcon()}";

        UIManager.Instance.ShowTriggerFlawConfirmation(question, effect, manaCost, () => ActivateTriggerFlaw(trait), layer: 26, showCover: true, pauseAndResume: true);
    }
    private void ActivateTriggerFlaw(Trait trait) {
        UIManager.Instance.HideObjectPicker();
        string result = trait.TriggerFlaw(activeCharacter);
        //When flaw is triggered, leave from party
        if (result == "flaw_effect") {
            if (activeCharacter.partyComponent.hasParty) {
                activeCharacter.partyComponent.currentParty.RemoveMemberThatJoinedQuest(activeCharacter);
            }
            PlayerSkillManager.Instance.GetPlayerActionData(SPELL_TYPE.TRIGGER_FLAW).OnExecuteSpellActionAffliction();
        } else {
            string log = "Failed to trigger flaw. Some requirements might be unmet.";
            if (LocalizationManager.Instance.HasLocalizedValue("Trigger Flaw", trait.name, result)) {
                triggerFlawLogFillers.Clear();
                triggerFlawLogFillers.Add(new LogFiller(activeCharacter, activeCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER));

                string reason = LocalizationManager.Instance.GetLocalizedValue("Trigger Flaw", trait.name, result);
                log = UtilityScripts.Utilities.StringReplacer(reason, triggerFlawLogFillers);
            }
            PlayerUI.Instance.ShowGeneralConfirmation("Trigger Flaw Failed", log);
        }
        Messenger.Broadcast(Signals.FLAW_TRIGGERED_BY_PLAYER, trait);
    }
    private bool CanActivateTriggerFlaw(string traitName) {
        Trait trait = activeCharacter.traitContainer.GetTraitOrStatus<Trait>(traitName);
        if (trait != null) {
            return trait.CanFlawBeTriggered(activeCharacter);
        }
        return false;
    }
    private void OnHoverEnterFlaw(string traitName) {
        Trait trait = activeCharacter.traitContainer.GetTraitOrStatus<Trait>(traitName);
        PlayerUI.Instance.skillDetailsTooltip.ShowPlayerSkillDetails(
            traitName, trait.GetTriggerFlawEffectDescription(activeCharacter, "flaw_effect"), 
            manaCost: PlayerSkillManager.Instance.GetPlayerActionData(SPELL_TYPE.TRIGGER_FLAW).manaCost
        );
    }
    private void OnHoverExitFlaw(string traitName) {
        PlayerUI.Instance.skillDetailsTooltip.HidePlayerSkillDetails();
    }
    //    private void OnClickTrait(object obj) {
//#if UNITY_EDITOR
//        if (obj is string text) {
//            int index = int.Parse(text);
//            Trait trait = activeCharacter.traitContainer.traits[index];
//            string traitDescription = trait.description;
//            if (trait.canBeTriggered) {
//                traitDescription +=
//                    $"\n{trait.GetRequirementDescription(activeCharacter)}\n\n<b>Effect</b>: {trait.GetTriggerFlawEffectDescription(activeCharacter, "flaw_effect")}";
//            }

//            StartCoroutine(HoverOutTraitAfterClick());//Quick fix because tooltips do not disappear. Issue with hover out action in label not being called when other collider goes over it.
//            UIManager.Instance.ShowYesNoConfirmation(trait.name, traitDescription,
//                onClickYesAction: () => OnClickTriggerFlaw(trait),
//                onClickNoAction: () => OnClickRemoveTrait(trait),
//                showCover: true, layer: 25,
//                yesBtnText: $"Trigger ({EditableValuesManager.Instance.triggerFlawManaCost.ToString()} Mana)",
//                noBtnText: "Remove Trait",
//                yesBtnInteractable: PlayerManager.Instance.player.playerSkillComponent.canTriggerFlaw && trait.canBeTriggered && trait.CanFlawBeTriggered(activeCharacter) && TraitManager.Instance.CanStillTriggerFlaws(activeCharacter),
//                noBtnInteractable: PlayerManager.Instance.player.playerSkillComponent.canRemoveTraits,
//                pauseAndResume: true,
//                //noBtnActive: false,
//                //yesBtnActive: trait.canBeTriggered,
//                yesBtnInactiveHoverAction: () => ShowCannotTriggerFlawReason(trait),
//                yesBtnInactiveHoverExitAction: UIManager.Instance.HideSmallInfo
//            );
//            normalTraitsEventLbl.ResetHighlightValues();
//            if (trait.type == TRAIT_TYPE.FLAW) {
//                Messenger.Broadcast(Signals.FLAW_CLICKED, trait);
//            }
//        }
//#endif
//    }
//    private IEnumerator HoverOutTraitAfterClick() {
//        yield return new WaitForEndOfFrame();
//        OnHoverOutTrait();
//    }
//    private void ShowCannotTriggerFlawReason(Trait trait) {
//        string reason = $"You cannot trigger {activeCharacter.name}'s flaw because: ";
//        List<string> reasons = trait.GetCannotTriggerFlawReasons(activeCharacter);
//        for (int i = 0; i < reasons.Count; i++) {
//            reason = $"{reason}\n\t- {reasons[i]}";
//        }
//        UIManager.Instance.ShowSmallInfo(reason);
//    }
//    private void OnClickTriggerFlaw(Trait trait) {
//        string logKey = trait.TriggerFlaw(activeCharacter);
//        int manaCost = EditableValuesManager.Instance.triggerFlawManaCost;
//        PlayerManager.Instance.player.AdjustMana(-manaCost);
//        if (logKey != "flaw_effect") {
//            UIManager.Instance.ShowYesNoConfirmation(
//                trait.name,
//                trait.GetTriggerFlawEffectDescription(activeCharacter, logKey),
//                showCover: true,
//                layer: 25,
//                yesBtnText: "OK",
//                pauseAndResume: true,
//                noBtnActive: false
//            );
//        }
//        Messenger.Broadcast(Signals.FLAW_TRIGGERED_BY_PLAYER);
//    }
    #endregion

    #region Mood
    private void OnMoodModified(MoodComponent moodComponent) {
        if (_activeCharacter != null && _activeCharacter.moodComponent == moodComponent) {
            UpdateMoodMeter();
            UpdateMoodSummary();
        }
    }
    private void UpdateMoodMeter() {
        moodMeter.SetFillAmount(_activeCharacter.moodComponent.moodValue/100f);
    }
    private void UpdateMoodSummary() {
        moodSummary.text = string.Empty;
        string summary = string.Empty;
        int index = 0;
        foreach (KeyValuePair<string, int> pair in _activeCharacter.moodComponent.moodModificationsSummary) {
            string color = "green";
            string text = "+" + pair.Value;
            if (pair.Value < 0) {
                color = "red";
                text = pair.Value.ToString();
            }
            summary += $"<color={color}>{text}</color> <link=\"{index}\">{pair.Key}</link>\n";
            index++;
        }
        moodSummary.text = summary;
    }
    public void OnHoverMoodEffect(object obj) {
        if (obj is string text) {
            int index = int.Parse(text);
            if (index < _activeCharacter.moodComponent.allMoodModifications.Count) {
                var kvp = _activeCharacter.moodComponent.allMoodModifications.ElementAt(index);
                MoodModification modifications = kvp.Value;
                int total = _activeCharacter.moodComponent.moodModificationsSummary[kvp.Key];
                string modificationSign = string.Empty;
                if (total > 0) {
                    modificationSign = "+";
                }
                string color = "green";
                if (total < 0) {
                    color = "red";
                }
                GameDate expiryDate = modifications.expiryDates.Last();
                string expiryText = string.Empty;
                if (expiryDate.hasValue) {
                    GameDate today = GameManager.Instance.Today();
                    int tickDiff = today.GetTickDifference(expiryDate);
                    if (tickDiff >= GameManager.ticksPerHour) {
                        int hours = GameManager.Instance.GetHoursBasedOnTicks(tickDiff);
                        if (hours > 1) {
                            expiryText = $"Lasts for: {hours.ToString()} hours";  //expiryDate.ConvertToContinuousDaysWithTime();    
                        } else {
                            expiryText = $"Lasts for: {hours.ToString()} hour";  //expiryDate.ConvertToContinuousDaysWithTime();
                        }
                    } else {
                        int minutes = GameManager.Instance.GetMinutesBasedOnTicks(tickDiff);
                        if (minutes > 1) {
                            expiryText = $"Lasts for: {minutes.ToString()} minutes";    
                        } else {
                            expiryText = $"Lasts for: {minutes.ToString()} minute";
                        }
                        
                    }
                } else {
                    expiryText = "Lasts until: Linked to Needs";
                }
                string summary = $"<color={color}>{modificationSign}{total.ToString()}</color> {expiryText}";
                // int dateIndex = modifications.expiryDates.Count - 1;
                // for (int i = 0; i < modifications.modifications.Count; i++) {
                //     int modificationValue = modifications.modifications[i];
                //     if (modificationValue != 0) { //do not show 0 values
                //         GameDate date = modifications.expiryDates[dateIndex];
                //         string modificationSign = string.Empty;
                //         if (modificationValue > 0) {
                //             modificationSign = "+";
                //         }
                //         string color = "green";
                //         if (modificationValue < 0) {
                //             color = "red";
                //         }
                //         summary = $"{summary} <color={color}>{modificationSign}{modificationValue.ToString()}</color> - {date.ConvertToContinuousDaysWithTime()}\n";
                //     }
                //     dateIndex--;
                // }
                UIManager.Instance.ShowSmallInfo(summary, autoReplaceText: false);    
            }
        }
    }
    public void OnHoverOutMoodEffect() {
        UIManager.Instance.HideSmallInfo();
    }
    public void ShowMoodTooltip() {
        string summary = $"Represents the Villagers' overall state of mind. Lower a Villagers' Mood to make them less effective and more volatile.\n\n" +
                         $"{_activeCharacter.moodComponent.moodValue.ToString()}/100\nBrainwash Success Rate: {DefilerRoom.GetBrainwashSuccessRate(_activeCharacter).ToString("N0")}%";
        // summary +=
        //     $"\nChance to trigger Major Mental Break {_activeCharacter.moodComponent.currentCriticalMoodEffectChance.ToString(CultureInfo.InvariantCulture)}";
        UIManager.Instance.ShowSmallInfo(summary, $"MOOD: {_activeCharacter.moodComponent.moodStateName}");
    }
    public void HideSmallInfo() {
        UIManager.Instance.HideSmallInfo();
    }
    #endregion

    #region Needs
    private void UpdateNeedMeters() {
        energyMeter.SetFillAmount(_activeCharacter.needsComponent.tiredness/CharacterNeedsComponent.TIREDNESS_DEFAULT);
        fullnessMeter.SetFillAmount(_activeCharacter.needsComponent.fullness/CharacterNeedsComponent.FULLNESS_DEFAULT);
        happinessMeter.SetFillAmount(_activeCharacter.needsComponent.happiness/CharacterNeedsComponent.HAPPINESS_DEFAULT);
        hopeMeter.SetFillAmount(_activeCharacter.needsComponent.hope/CharacterNeedsComponent.HOPE_DEFAULT);
        staminaMeter.SetFillAmount(_activeCharacter.needsComponent.stamina/CharacterNeedsComponent.STAMINA_DEFAULT);
    }
    public void ShowEnergyTooltip() {
        string summary = $"Villagers will become Unconscious once this Meter is empty. This is replenished through rest.\n\n" +
                         $"Value: {_activeCharacter.needsComponent.tiredness.ToString("N0")}/100";
        UIManager.Instance.ShowSmallInfo(summary, "ENERGY");
    }
    public void ShowFullnessTooltip() {
        string summary = $"Villagers will become Malnourished and eventually die once this Meter is empty. This is replenished through eating.\n\n" +
                         $"Value: {_activeCharacter.needsComponent.fullness.ToString("N0")}/100";
        UIManager.Instance.ShowSmallInfo(summary, "FULLNESS");
    }
    public void ShowHappinessTooltip() {
        string summary = $"Villager's Mood becomes significantly affected when this Meter goes down. This is replenished by doing fun activities.\n\n" +
                         $"Value: {_activeCharacter.needsComponent.happiness.ToString("N0")}/100";
        UIManager.Instance.ShowSmallInfo(summary, "ENTERTAINMENT");
    }
    public void ShowHopeTooltip() {
        string summary = $"How much this Villager trusts you. If this gets too low, they will be uncooperative towards you in various way.\n\n" +
                         $"Value: {_activeCharacter.needsComponent.hope.ToString("N0")}/100";
        UIManager.Instance.ShowSmallInfo(summary, "TRUST");
    }
    public void ShowStaminaTooltip() {
        string summary = $"Villagers will be unable to run when this Meter is empty. This is used up when the Villager is running and quickly replenished when he isn't.\n\n" +
                         $"Value: {_activeCharacter.needsComponent.stamina.ToString("N0")}/100";
        UIManager.Instance.ShowSmallInfo(summary, "STAMINA");
    }
    #endregion

    #region Combat Modes
    private void ConstructCombatModes() {
        combatModes = new List<string>();
        for (int i = 0; i < CharacterManager.Instance.combatModes.Length; i++) {
            combatModes.Add(UtilityScripts.Utilities.NotNormalizedConversionEnumToString(CharacterManager.Instance.combatModes[i].ToString()));
        }
    }
    public void ShowSwitchCombatModeUI() {
        UIManager.Instance.customDropdownList.ShowDropdown(combatModes, OnClickChooseCombatMode, CanChoostCombatMode);
    }
    private void SetCombatModeUIPosition(PlayerAction action) {
        ActionItem actionItem = GetActiveActionItem(action);
        if(actionItem != null) {
            Vector3 actionWorldPos = actionItem.transform.localPosition;
            UIManager.Instance.customDropdownList.SetPosition(new Vector3(actionWorldPos.x, actionWorldPos.y + 10f, actionWorldPos.z));
        }
    }
    private bool CanChoostCombatMode(string mode) {
        if(UtilityScripts.Utilities.NotNormalizedConversionEnumToString(activeCharacter.combatComponent.combatMode.ToString()) == mode) {
            return false;
        }
        return true;
    }
    private void OnClickChooseCombatMode(string mode) {
        COMBAT_MODE combatMode = (COMBAT_MODE) System.Enum.Parse(typeof(COMBAT_MODE), UtilityScripts.Utilities.NotNormalizedConversionStringToEnum(mode));
        UIManager.Instance.characterInfoUI.activeCharacter.combatComponent.SetCombatMode(combatMode);
        Messenger.Broadcast(Signals.RELOAD_PLAYER_ACTIONS, activeCharacter as IPlayerActionTarget);
        UIManager.Instance.customDropdownList.Close();
        PlayerSkillManager.Instance.GetPlayerActionData(SPELL_TYPE.CHANGE_COMBAT_MODE).OnExecuteSpellActionAffliction();
    }
    #endregion

    #region Tabs
    public void OnToggleInfo(bool isOn) {
        // if (isOn) {
        //     Messenger.Broadcast(Signals.TOGGLE_TURNED_ON, "CharacterInfo_Info");    
        // }
    }
    public void OnToggleMood(bool isOn) {
        // if (isOn) {
        //     Messenger.Broadcast(Signals.TOGGLE_TURNED_ON, "CharacterInfo_Mood");    
        // }
    }
    public void OnToggleRelations(bool isOn) {
        // if (isOn) {
        //     Messenger.Broadcast(Signals.TOGGLE_TURNED_ON, "CharacterInfo_Relations");    
        // }
    }
    public void OnToggleLogs(bool isOn) {
        // if (isOn) {
        //     Messenger.Broadcast(Signals.TOGGLE_TURNED_ON, "CharacterInfo_Logs");    
        // }
    }
    #endregion

    #region Party
    public void UpdatePartyInfo() {
        string text = "None";
        if (activeCharacter.partyComponent.hasParty) {
            text = $"<link=\"party\">{UtilityScripts.Utilities.ColorizeAndBoldName(activeCharacter.partyComponent.currentParty.partyName)}</link>";
        }
        partyLbl.text = text;
    }
    private void OnClickParty(object obj) {
        if (activeCharacter.partyComponent.hasParty) {
            UIManager.Instance.ShowPartyInfo(activeCharacter.partyComponent.currentParty);
        }
    }
    #endregion
}
