﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.UI;

using UnityEngine.UI.Extensions;
using EZObjectPools;
using Locations.Settlements;

public class FactionInfoUI : InfoUIBase {

    [Space(10)]
    [Header("Content")]
    [SerializeField] private TextMeshProUGUI factionNameLbl;
    [SerializeField] private TextMeshProUGUI factionTypeLbl;
    [SerializeField] private FactionEmblem emblem;

    [Space(10)]
    [Header("Overview")]
    [SerializeField] private TextMeshProUGUI overviewFactionNameLbl;
    [SerializeField] private TextMeshProUGUI overviewFactionTypeLbl;
    [SerializeField] private CharacterNameplateItem leaderNameplateItem;
    [SerializeField] private TextMeshProUGUI ideologyLbl;
    
    [Space(10)]
    [Header("Characters")]
    [SerializeField] private GameObject characterItemPrefab;
    [SerializeField] private ScrollRect charactersScrollView;
    private List<CharacterNameplateItem> _characterItems;

    [Space(10)]
    [Header("Regions")]
    [SerializeField] private ScrollRect regionsScrollView;
    [SerializeField] private GameObject regionNameplatePrefab;
    private List<RegionNameplateItem> locationItems;

    [Space(10)]
    [Header("Relationships")]
    [SerializeField] private RectTransform relationshipsParent;
    [SerializeField] private GameObject relationshipPrefab;
    
    [Space(10)] [Header("Logs")]
    [SerializeField] private GameObject logHistoryPrefab;
    [SerializeField] private ScrollRect historyScrollView;
    [SerializeField] private UIHoverPosition logHoverPosition;
    private LogHistoryItem[] logHistoryItems;
    
    internal Faction currentlyShowingFaction => _data as Faction;
    private Faction activeFaction { get; set; }

    internal override void Initialize() {
        base.Initialize();
        _characterItems = new List<CharacterNameplateItem>();
        locationItems = new List<RegionNameplateItem>();
        Messenger.AddListener(Signals.INSPECT_ALL, OnInspectAll);
        Messenger.AddListener<Character, Faction>(Signals.CHARACTER_ADDED_TO_FACTION, OnCharacterAddedToFaction);
        Messenger.AddListener<Character, Faction>(Signals.CHARACTER_REMOVED_FROM_FACTION, OnCharacterRemovedFromFaction);
        Messenger.AddListener<Faction, BaseSettlement>(Signals.FACTION_OWNED_REGION_ADDED, OnFactionRegionAdded);
        Messenger.AddListener<Faction, BaseSettlement>(Signals.FACTION_OWNED_REGION_REMOVED, OnFactionRegionRemoved);
        Messenger.AddListener<FactionRelationship>(Signals.FACTION_RELATIONSHIP_CHANGED, OnFactionRelationshipChanged);
        Messenger.AddListener<Faction>(Signals.FACTION_ACTIVE_CHANGED, OnFactionActiveChanged);
        Messenger.AddListener<Character, ILeader>(Signals.ON_SET_AS_FACTION_LEADER, OnFactionLeaderChanged);
        Messenger.AddListener<Faction, ILeader>(Signals.ON_FACTION_LEADER_REMOVED, OnFactionLeaderRemoved);
        Messenger.AddListener<Faction>(Signals.FACTION_LOG_ADDED, UpdateHistory);
        InitializeLogsMenu();
    }
    public override void OpenMenu() {
        Faction previousArea = activeFaction;
        activeFaction = _data as Faction;
        base.OpenMenu();
        if (UIManager.Instance.IsShareIntelMenuOpen()) {
            backButton.interactable = false;
        }
        UpdateOverview();
        UpdateFactionInfo();
        UpdateAllCharacters();
        UpdateRegions();
        UpdateAllRelationships();
        UpdateAllHistoryInfo();
        ResetScrollPositions();
    }
    public override void CloseMenu() {
        base.CloseMenu();
        activeFaction = null;
    }

    public void UpdateFactionInfo() {
        if (activeFaction == null) {
            return;
        }
        UpdateBasicInfo();
        //ResetScrollPositions();
    }

    #region Basic Info
    private void UpdateBasicInfo() {
        factionNameLbl.text = activeFaction.name;
        factionTypeLbl.text = activeFaction.GetRaceText();
        emblem.SetFaction(activeFaction);
    }
    #endregion

    #region Characters
    private void UpdateAllCharacters() {
        UtilityScripts.Utilities.DestroyChildren(charactersScrollView.content);
        _characterItems.Clear();

        for (int i = 0; i < activeFaction.characters.Count; i++) {
            Character currCharacter = activeFaction.characters[i];
            CreateNewCharacterItem(currCharacter, false);
        }
        OrderCharacterItems();
    }
    private CharacterNameplateItem GetItem(Character character) {
        CharacterNameplateItem[] items = UtilityScripts.GameUtilities.GetComponentsInDirectChildren<CharacterNameplateItem>(charactersScrollView.content.gameObject);
        for (int i = 0; i < items.Length; i++) {
            CharacterNameplateItem item = items[i];
            if (item.character != null) {
                if (item.character.id == character.id) {
                    return item;
                }
            }
        }
        return null;
    }
    private CharacterNameplateItem CreateNewCharacterItem(Character character, bool autoSort = true) {
        GameObject characterGO = UIManager.Instance.InstantiateUIObject(characterItemPrefab.name, charactersScrollView.content);
        CharacterNameplateItem item = characterGO.GetComponent<CharacterNameplateItem>();
        item.SetObject(character);
        item.SetAsDefaultBehaviour();
        _characterItems.Add(item);
        if (autoSort) {
            OrderCharacterItems();
        }
        return item;
    }
    private void OrderCharacterItems() {
        if (activeFaction.leader != null && activeFaction.leader is Character leader) {
            CharacterNameplateItem leaderItem = GetItem(leader);
            if (leaderItem == null) {
                throw new System.Exception($"Leader item in {activeFaction.name}'s UI is null! Leader is {leader.name}");
            }
            leaderItem.transform.SetAsFirstSibling();
        }
    }
    private void OnCharacterAddedToFaction(Character character, Faction faction) {
        if (isShowing && activeFaction.id == faction.id) {
            CreateNewCharacterItem(character);
        }
    }
    private void OnCharacterRemovedFromFaction(Character character, Faction faction) {
        if (isShowing && activeFaction != null && activeFaction.id == faction.id) {
            CharacterNameplateItem item = GetItem(character);
            if (item != null) {
                _characterItems.Remove(item);
                ObjectPoolManager.Instance.DestroyObject(item);
                OrderCharacterItems();
            }
        }
    }
    #endregion

    #region Regions
    private void UpdateRegions() {
        UtilityScripts.Utilities.DestroyChildren(regionsScrollView.content);
        locationItems.Clear();
    }
    private void CreateNewRegionItem(Region region) {
        GameObject characterGO = UIManager.Instance.InstantiateUIObject(regionNameplatePrefab.name, regionsScrollView.content);
        RegionNameplateItem item = characterGO.GetComponent<RegionNameplateItem>();
        item.SetObject(region);
        locationItems.Add(item);
    }
    private RegionNameplateItem GetLocationItem(Region region) {
        for (int i = 0; i < locationItems.Count; i++) {
            RegionNameplateItem locationPortrait = locationItems[i];
            if (locationPortrait.obj.id == region.id) {
                return locationPortrait;
            }
        }
        return null;
    }
    private void DestroyLocationItem(Region region) {
        RegionNameplateItem item = GetLocationItem(region);
        if (item != null) {
            locationItems.Remove(item);
            ObjectPoolManager.Instance.DestroyObject(item);
        }
    }
    private void OnFactionRegionAdded(Faction faction, BaseSettlement region) {
        if (isShowing && activeFaction.id == faction.id) {
        }
    }
    private void OnFactionRegionRemoved(Faction faction, BaseSettlement region) {
        if (isShowing && activeFaction.id == faction.id) {
        }
    }
    #endregion

    #region Relationships
    private void UpdateAllRelationships() {
        UtilityScripts.Utilities.DestroyChildren(relationshipsParent);

        foreach (KeyValuePair<Faction, FactionRelationship> keyValuePair in activeFaction.relationships) {
            if (keyValuePair.Key.isActive) {
                GameObject relGO = UIManager.Instance.InstantiateUIObject(relationshipPrefab.name, relationshipsParent);
                FactionRelationshipItem item = relGO.GetComponent<FactionRelationshipItem>();
                item.SetData(keyValuePair.Key, keyValuePair.Value);
            }
        }
    }
    private void OnFactionRelationshipChanged(FactionRelationship rel) {
        if (isShowing && (rel.faction1.id == activeFaction.id || rel.faction2.id == activeFaction.id)) {
            UpdateAllRelationships();
        }
    }
    private void OnFactionActiveChanged(Faction faction) {
        if (isShowing) {
            UpdateAllRelationships();
        }
    }
    #endregion

    #region Utilities
    public void OnClickCloseBtn() {
        CloseMenu();
    }
    private void ResetScrollPositions() {
        charactersScrollView.verticalNormalizedPosition = 1;
        regionsScrollView.verticalNormalizedPosition = 1;
        historyScrollView.verticalNormalizedPosition = 1;
    }
    private void OnInspectAll() {
        if (isShowing && activeFaction != null) {
            UpdateAllCharacters();
            //UpdateHiddenUI();
        }
    }
    public void ShowFactionTestingInfo() {
        string summary = $"Faction Type: {activeFaction.factionType.type.ToString()}";
        for (int i = 0; i < activeFaction.ideologyComponent.currentIdeologies.Count; i++) {
            FactionIdeology ideology = activeFaction.ideologyComponent.currentIdeologies[i];
            if (ideology != null) {
                summary += $"\n{ideology.name}";
                summary += "\nRequirements for joining:";
                summary += $"\n\t{ideology.GetRequirementsForJoiningAsString()}";    
            }
        }
        summary += $"\n{name} Faction Job Queue:";
        if (activeFaction.availableJobs.Count > 0) {
            for (int j = 0; j < activeFaction.availableJobs.Count; j++) {
                JobQueueItem jqi = activeFaction.availableJobs[j];
                if (jqi is GoapPlanJob gpj) {
                    summary += $"\n<b>{gpj.name} Targeting {gpj.targetPOI?.ToString() ?? "None"}</b>" ;
                } else {
                    summary += $"\n<b>{jqi.name}</b>";
                }
                summary += $"\n Assigned Character: {jqi.assignedCharacter?.name}";
            }
        } else {
            summary += "\nNone";
        }

        UIManager.Instance.ShowSmallInfo(summary);
    }
    public void HideFactionTestingInfo() {
        UIManager.Instance.HideSmallInfo();
    }
    #endregion

    #region Overview
    private void OnFactionLeaderChanged(Character character, ILeader previousLeader) {
        if (isShowing) {
            UpdateOverview();
        }
    }
    private void OnFactionLeaderRemoved(Faction faction, ILeader previousLeader) {
        if (isShowing && faction == activeFaction) {
            UpdateOverview();
        }
    }
    private void UpdateOverview() {
        overviewFactionNameLbl.text = activeFaction.name;
        overviewFactionTypeLbl.text = activeFaction.factionType.name;

        if (activeFaction.leader is Character leader) {
            leaderNameplateItem.gameObject.SetActive(true);
            leaderNameplateItem.SetObject(leader);
        } else {
            leaderNameplateItem.gameObject.SetActive(false);
        }

        ideologyLbl.text = string.Empty;
        for (int i = 0; i < activeFaction.factionType.ideologies.Count; i++) {
            FactionIdeology ideology = activeFaction.factionType.ideologies[i];
            ideologyLbl.text += $"<sprite=\"Text_Sprites\" name=\"Arrow_Icon\">   <link=\"{i}\">{ideology.GetIdeologyDescription()}</link>\n";
        }
    }
    public void OnHoverIdeology(object obj) {
        if (obj is string text) {
            int index = int.Parse(text);
            FactionIdeology ideology = activeFaction.factionType.ideologies[index];
            UIManager.Instance.ShowSmallInfo(ideology.name);
        }
    }
    public void OnHoverOutIdeology() {
        UIManager.Instance.HideSmallInfo();
    }
    #endregion
    
    #region History
    private void InitializeLogsMenu() {
        logHistoryItems = new LogHistoryItem[Faction.MAX_HISTORY_LOGS];
        for (int i = 0; i < Faction.MAX_HISTORY_LOGS; i++) {
            GameObject newLogItem = ObjectPoolManager.Instance.InstantiateObjectFromPool(logHistoryPrefab.name, Vector3.zero, Quaternion.identity, historyScrollView.content);
            logHistoryItems[i] = newLogItem.GetComponent<LogHistoryItem>();
            newLogItem.transform.localScale = Vector3.one;
            newLogItem.SetActive(true);
        }
        for (int i = 0; i < logHistoryItems.Length; i++) {
            logHistoryItems[i].gameObject.SetActive(false);
        }
    }
    private void UpdateHistory(Faction faction) {
        if (isShowing && faction == activeFaction) {
            UpdateAllHistoryInfo();
        }
    }
    private void UpdateAllHistoryInfo() {
        int historyCount = activeFaction.history.Count;
        int historyLastIndex = historyCount - 1;
        for (int i = 0; i < logHistoryItems.Length; i++) {
            LogHistoryItem currItem = logHistoryItems[i];
            if(i < historyCount) {
                Log currLog = activeFaction.history[historyLastIndex - i];
                currItem.gameObject.SetActive(true);
                currItem.SetLog(currLog);
                currItem.SetHoverPosition(logHoverPosition);
            } else {
                currItem.gameObject.SetActive(false);
            }
        }
    }
    #endregion   
}
