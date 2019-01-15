﻿using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using System.Linq;

public class Player : ILeader {

    private const int MAX_IMPS = 5;

    public Faction playerFaction { get; private set; }
    public Area playerArea { get; private set; }
    public int maxImps { get; private set; }

    private int _lifestones;
    private float _currentLifestoneChance;
    private BaseLandmark _demonicPortal;
    private List<Token> _tokens;
    private List<Minion> _minions;
    private Dictionary<CURRENCY, int> _currencies;
    public List<Character> otherCharacters;

    public Dictionary<JOB, Character> roleSlots { get; private set; }
    public CombatGrid attackGrid { get; private set; }
    public CombatGrid defenseGrid { get; private set; }

    #region getters/setters
    public int id {
        get { return -645; }
    }
    public string name {
        get { return "Player"; }
    }
    public int lifestones {
        get { return _lifestones; }
    }
    public float currentLifestoneChance {
        get { return _currentLifestoneChance; }
    }
    public RACE race {
        get { return RACE.HUMANS; }
    }
    public BaseLandmark demonicPortal {
        get { return _demonicPortal; }
    }
    public ILocation specificLocation {
        get { return _demonicPortal; }
    }
    public List<Token> tokens {
        get { return _tokens; }
    }
    public Dictionary<CURRENCY, int> currencies {
        get { return _currencies; }
    }
    public List<Minion> minions {
        get { return _minions; }
    }
    public List<Character> allOwnedCharacters {
        get { return minions.Select(x => x.character).Concat(otherCharacters).ToList(); } //TODO: Optimize this!
    }
    #endregion

    public Player() {
        playerArea = null;
        _tokens = new List<Token>();
        otherCharacters = new List<Character>();
        attackGrid = new CombatGrid();
        defenseGrid = new CombatGrid();
        attackGrid.Initialize();
        defenseGrid.Initialize();
        maxImps = 5;
        SetCurrentLifestoneChance(25f);
        ConstructCurrencies();
        ConstructRoleSlots();
        Messenger.AddListener<Area, HexTile>(Signals.AREA_TILE_REMOVED, OnTileRemovedFromPlayerArea);
        Messenger.AddListener(Signals.DAY_STARTED, EverydayAction);
        AddWinListener();
    }

    private void EverydayAction() {
        //DepleteThreatLevel();
    }

    #region ILeader
    public void LevelUp() {
        //Not applicable
    }
    #endregion

    #region Area
    public void CreatePlayerArea(HexTile chosenCoreTile) {
        chosenCoreTile.SetCorruption(true);
        Area playerArea = LandmarkManager.Instance.CreateNewArea(chosenCoreTile, AREA_TYPE.DEMONIC_INTRUSION);
        playerArea.LoadAdditionalData();
        _demonicPortal = LandmarkManager.Instance.CreateNewLandmarkOnTile(chosenCoreTile, LANDMARK_TYPE.DEMONIC_PORTAL);
        Biomes.Instance.CorruptTileVisuals(chosenCoreTile);
        SetPlayerArea(playerArea);
        //ActivateMagicTransferToPlayer();
        _demonicPortal.tileLocation.ScheduleCorruption();
        //OnTileAddedToPlayerArea(playerArea, chosenCoreTile);
    }
    public void CreatePlayerArea(BaseLandmark portal) {
        _demonicPortal = portal;
        Area playerArea = LandmarkManager.Instance.CreateNewArea(portal.tileLocation, AREA_TYPE.DEMONIC_INTRUSION);
        playerArea.LoadAdditionalData();
        Biomes.Instance.CorruptTileVisuals(portal.tileLocation);
        portal.tileLocation.SetCorruption(true);
        SetPlayerArea(playerArea);
        //ActivateMagicTransferToPlayer();
        _demonicPortal.tileLocation.ScheduleCorruption();
    }
    public void LoadPlayerArea(Area area) {
        _demonicPortal = area.coreTile.landmarkOnTile;
        Biomes.Instance.CorruptTileVisuals(_demonicPortal.tileLocation);
        _demonicPortal.tileLocation.SetCorruption(true);
        SetPlayerArea(area);
        _demonicPortal.tileLocation.ScheduleCorruption();

    }
    private void SetPlayerArea(Area area) {
        playerArea = area;
        area.SetSuppliesInBank(_currencies[CURRENCY.SUPPLY]);
        area.StopSupplyLine();
    }
    private void OnTileRemovedFromPlayerArea(Area affectedArea, HexTile removedTile) {
        if (playerArea != null && affectedArea.id == playerArea.id) {
            Biomes.Instance.UpdateTileVisuals(removedTile);
        }
    }
    #endregion

    #region Faction
    public void CreatePlayerFaction() {
        Faction playerFaction = FactionManager.Instance.CreateNewFaction(true);
        playerFaction.SetLeader(this);
        playerFaction.SetEmblem(FactionManager.Instance.GetFactionEmblem(6));
        SetPlayerFaction(playerFaction);
    }
    private void SetPlayerFaction(Faction faction) {
        playerFaction = faction;
    }
    #endregion

    #region Token
    public void AddToken(Token token) {
        if (!_tokens.Contains(token)) {
            if (token is CharacterToken && (token as CharacterToken).character.minion != null) {

            } else {
                _tokens.Add(token);
                Debug.Log("Added token " + token.ToString());
                Messenger.Broadcast(Signals.TOKEN_ADDED, token);
            }
            token.SetObtainedState(true);
            if (token is CharacterToken) {
                Messenger.Broadcast(Signals.CHARACTER_TOKEN_ADDED, token as CharacterToken);
            } 
            //else if (token is SpecialToken) {
            //    (token as SpecialToken).AdjustQuantity(-1);
            //}
        }
    }
    public bool RemoveToken(Token token) {
        if (_tokens.Remove(token)) {
            token.SetObtainedState(false);
            Debug.Log("Removed token " + token.ToString());
            return true;
        }
        return false;
    }
    public Token GetToken(Token token) {
        for (int i = 0; i < _tokens.Count; i++) {
            if(_tokens[i] == token) {
                return _tokens[i];
            }
        }
        return null;
    }
    public bool HasSpecialToken(string tokenName) {
        for (int i = 0; i < _tokens.Count; i++) {
            if (_tokens[i].tokenName == tokenName) {
                return true;
            }
        }
        return false;
    }
    #endregion

    #region Lifestone
    public void DecreaseLifestoneChance() {
        if(_currentLifestoneChance > 2f) {
            float decreaseRate = 5f;
            if(_currentLifestoneChance <= 15f) {
                decreaseRate = 1f;
            }
            _currentLifestoneChance -= decreaseRate;
        }
    }
    public void SetCurrentLifestoneChance(float amount) {
        _currentLifestoneChance = amount;
    }
    public void SetLifestone(int amount) {
        _lifestones = amount;
    }
    public void AdjustLifestone(int amount) {
        _lifestones += amount;
        Debug.Log("Adjusted player lifestones by: " + amount + ". New total is " + _lifestones);
    }
    #endregion

    #region Minions
    public void CreateInitialMinions() {
        PlayerUI.Instance.ResetAllMinionItems();
        _minions = new List<Minion>();
        for (int i = 0; i < 20; i++) {
            AddMinion(CreateNewMinion(CharacterManager.Instance.GetDeadlySinsClassNameFromRotation(), RACE.DEMON, false));
        }
        //AddMinion(CreateNewMinion(CharacterManager.Instance.GetRandomDeadlySinsClassName(), RACE.DEMON, false));
        //AddMinion(CreateNewMinion(CharacterManager.Instance.GetRandomDeadlySinsClassName(), RACE.DEMON, false));
        //AddMinion(CreateNewMinion(CharacterManager.Instance.GetRandomDeadlySinsClassName(), RACE.DEMON, false));
        //AddMinion(CreateNewMinion(CharacterManager.Instance.GetRandomDeadlySinsClassName(), RACE.DEMON, false));
        //AddMinion(CreateNewMinion(CharacterManager.Instance.GetRandomDeadlySinsClassName(), RACE.DEMON, false));

        //UpdateMinions();
        PlayerUI.Instance.minionsScrollRect.verticalNormalizedPosition = 1f;
        PlayerUI.Instance.OnStartMinionUI();
    }
    public Minion CreateNewMinion(Character character) {
        return new Minion(character, true);
    }
    public Minion CreateNewMinion(string className, RACE race, bool isArmy) {
        Minion minion = null;
        if (isArmy) {
            minion = new Minion(CharacterManager.Instance.CreateCharacterArmyUnit(className, race, playerFaction, _demonicPortal), false);
        } else {
            minion = new Minion(CharacterManager.Instance.CreateNewCharacter(className, race, GENDER.MALE, playerFaction, _demonicPortal, false), false);
        }
        return minion;
    }
    public void UpdateMinions() {
        for (int i = 0; i < _minions.Count; i++) {
            RearrangeMinionItem(_minions[i].minionItem, i);
        }
    }
    private void RearrangeMinionItem(PlayerCharacterItem minionItem, int index) {
        //if (minionItem.transform.GetSiblingIndex() != index) {
            //minionItem.transform.SetSiblingIndex(index);
            minionItem.supposedIndex = index;
            Vector3 to = PlayerUI.Instance.minionsContentTransform.GetChild(index).transform.localPosition;
            Vector3 from = minionItem.transform.localPosition;
            minionItem.tweenPos.from = from;
            minionItem.tweenPos.to = to;
            minionItem.tweenPos.ResetToBeginning();
            minionItem.tweenPos.PlayForward();
        //}
    }
    public void SortByLevel() {
        _minions = _minions.OrderBy(x => x.lvl).ToList();
        //for (int i = 0; i < PlayerUI.Instance.minionItems.Length; i++) {
        //    MinionItem minionItem = PlayerUI.Instance.minionItems[i];
        //    if (i < _minions.Count) {
        //        minionItem.SetMinion(_minions[i]);
        //    } else {
        //        minionItem.SetMinion(null);
        //    }
        //}
        UpdateMinions();
    }
    public void SortByClass() {
        _minions = _minions.OrderBy(x => x.character.characterClass.className).ToList();
        //for (int i = 0; i < PlayerUI.Instance.minionItems.Length; i++) {
        //    MinionItem minionItem = PlayerUI.Instance.minionItems[i];
        //    if (i < _minions.Count) {
        //        minionItem.SetMinion(_minions[i]);
        //    } else {
        //        minionItem.SetMinion(null);
        //    }
        //}
        UpdateMinions();
    }
    public void SortByDefault() {
        _minions = _minions.OrderBy(x => x.indexDefaultSort.ToString()).ToList();
        UpdateMinions();
    }
    public void AddMinion(Minion minion) {
        minion.SetIndexDefaultSort(_minions.Count);
        //MinionItem minionItem = PlayerUI.Instance.minionItems[_minions.Count];
        PlayerCharacterItem item = PlayerUI.Instance.CreateMinionItem();
        item.SetCharacter(minion.character);

        if (PlayerUI.Instance.minionSortType == MINIONS_SORT_TYPE.LEVEL) {
            for (int i = 0; i < _minions.Count; i++) {
                if (minion.lvl <= _minions[i].lvl) {
                    _minions.Insert(i, minion);
                    item.transform.SetSiblingIndex(i);
                    break;
                }
            }
        } else if (PlayerUI.Instance.minionSortType == MINIONS_SORT_TYPE.TYPE) {
            string strMinionType = minion.character.characterClass.className;
            for (int i = 0; i < _minions.Count; i++) {
                int compareResult = string.Compare(strMinionType, minion.character.characterClass.className);
                if (compareResult == -1 || compareResult == 0) {
                    _minions.Insert(i, minion);
                    item.transform.SetSiblingIndex(i);
                    break;
                }
            }
        } else {
            _minions.Add(minion);
        }
    }
    public void RemoveMinion(Minion minion) {
        if(_minions.Remove(minion)){
            PlayerUI.Instance.RemoveCharacterItem(minion.minionItem);
            if (minion.currentlyExploringArea != null) {
                minion.currentlyExploringArea.areaInvestigation.CancelInvestigation("explore");
            }
            if (minion.currentlyAttackingArea != null) {
                minion.currentlyAttackingArea.areaInvestigation.CancelInvestigation("attack");
            }
        }
    }
    //public void AdjustMaxMinions(int adjustment) {
    //    _maxMinions += adjustment;
    //    _maxMinions = Mathf.Max(0, _maxMinions);
    //    PlayerUI.Instance.OnMaxMinionsChanged();
    //}
    //public void SetMaxMinions(int value) {
    //    _maxMinions = value;
    //    _maxMinions = Mathf.Max(0, _maxMinions);
    //    PlayerUI.Instance.OnMaxMinionsChanged();
    //}
    #endregion

    #region Currencies
    private void ConstructCurrencies() {
        _currencies = new Dictionary<CURRENCY, int>();
        _currencies.Add(CURRENCY.IMP, 0);
        _currencies.Add(CURRENCY.MANA, 0);
        _currencies.Add(CURRENCY.SUPPLY, 0);
        AdjustCurrency(CURRENCY.IMP, maxImps);
        AdjustCurrency(CURRENCY.SUPPLY, 5000);
        AdjustCurrency(CURRENCY.MANA, 5000);
    }
    public void AdjustCurrency(CURRENCY currency, int amount) {
        _currencies[currency] += amount;
        if(currency == CURRENCY.IMP) {
            _currencies[currency] = Mathf.Clamp(_currencies[currency], 0, maxImps);
        }else if (currency == CURRENCY.SUPPLY) {
            _currencies[currency] = Mathf.Max(_currencies[currency], 0);
            if (playerArea != null) {
                playerArea.SetSuppliesInBank(_currencies[currency]);
            }
        } else if (currency == CURRENCY.MANA) {
            _currencies[currency] = Mathf.Max(_currencies[currency], 0); //maybe 999?
        }
        Messenger.Broadcast(Signals.UPDATED_CURRENCIES);
    }
    public void SetMaxImps(int imps) {
        maxImps = imps;
        _currencies[CURRENCY.IMP] = Mathf.Clamp(_currencies[CURRENCY.IMP], 0, maxImps);
    }
    public void AdjustMaxImps(int adjustment) {
        maxImps += adjustment;
        AdjustCurrency(CURRENCY.IMP, adjustment);
    }
    #endregion

    #region Rewards
    public void ClaimReward(Reward reward) {
        switch (reward.rewardType) {
            case REWARD.SUPPLY:
                AdjustCurrency(CURRENCY.SUPPLY, reward.amount);
                break;
            case REWARD.MANA:
                AdjustCurrency(CURRENCY.MANA, reward.amount);
                break;
            //case REWARD.EXP:
            //    state.assignedMinion.AdjustExp(reward.amount);
            //    break;
            default:
                break;
        }
    }
    #endregion

    #region Other Characters/Units
    public void AddNewCharacter(Character character) {
        if (!otherCharacters.Contains(character)) {
            otherCharacters.Add(character);
            character.OnAddedToPlayer();
            PlayerCharacterItem item = PlayerUI.Instance.GetUnoccupiedCharacterItem();
            item.SetCharacter(character);
        }
    }
    public void RemoveCharacter(Character character) {
        if (otherCharacters.Remove(character)) {
            PlayerUI.Instance.RemoveCharacterItem(character.playerCharacterItem);
        }
    }
    #endregion

    #region Win/Lose Conditions
    private void AddWinListener() {
        Messenger.AddListener<Faction>(Signals.FACTION_LEADER_DIED, OnFactionLeaderDied);
    }
    private void OnFactionLeaderDied(Faction faction) {
        Faction fyn = FactionManager.Instance.GetFactionBasedOnName("Fyn");
        Faction orelia = FactionManager.Instance.GetFactionBasedOnName("Orelia");
        if (fyn.isDestroyed && orelia.isDestroyed) {
            Debug.LogError("Fyn and Orelia factions are destroyed! Player won!");
        }
    }
    public void OnPlayerLandmarkRuined(BaseLandmark landmark) {
        switch (landmark.specificLandmarkType) {
            case LANDMARK_TYPE.DWELLINGS:
                //add 2 minion slots
                //AdjustMaxMinions(-2);
                break;
            case LANDMARK_TYPE.IMP_KENNEL:
                //adds 1 Imp capacity
                //AdjustMaxImps(-1);
                break;
            case LANDMARK_TYPE.DEMONIC_PORTAL:
                //player loses if the Portal is destroyed
                throw new System.Exception("Demonic Portal Was Destroyed! Game Over!");
            case LANDMARK_TYPE.RAMPART:
                //remove bonus 25% HP to all Defenders
                //for (int i = 0; i < playerArea.landmarks.Count; i++) {
                //    BaseLandmark currLandmark = playerArea.landmarks[i];
                //    currLandmark.RemoveDefenderBuff(new Buff() { buffedStat = STAT.HP, percentage = 0.25f });
                //    //if (currLandmark.defenders != null) {
                //    //    currLandmark.defenders.RemoveBuff(new Buff() { buffedStat = STAT.HP, percentage = 0.25f });
                //    //}
                //}
                break;
            default:
                break;
        }
    }
    #endregion

    #region Role Slots
    public void ConstructRoleSlots() {
        roleSlots = new Dictionary<JOB, Character>();
        roleSlots.Add(JOB.SPY, null);
        roleSlots.Add(JOB.RECRUITER, null);
        roleSlots.Add(JOB.DIPLOMAT, null);
        roleSlots.Add(JOB.INSTIGATOR, null);
        roleSlots.Add(JOB.DEBILITATOR, null);
    }
    public List<JOB> GetValidJobForCharacter(Character character) {
        List<JOB> validJobs = new List<JOB>();
        if (character.minion != null) {
            switch (character.characterClass.className) {
                case "Envy":
                    validJobs.Add(JOB.SPY);
                    validJobs.Add(JOB.RECRUITER);
                    break;
                case "Lust":
                    validJobs.Add(JOB.DIPLOMAT);
                    validJobs.Add(JOB.RECRUITER);
                    break;
                case "Pride":
                    validJobs.Add(JOB.DIPLOMAT);
                    validJobs.Add(JOB.INSTIGATOR);
                    break;
                case "Greed":
                    validJobs.Add(JOB.SPY);
                    validJobs.Add(JOB.INSTIGATOR);
                    break;
                case "Guttony":
                    validJobs.Add(JOB.SPY);
                    validJobs.Add(JOB.RECRUITER);
                    break;
                case "Wrath":
                    validJobs.Add(JOB.INSTIGATOR);
                    validJobs.Add(JOB.DEBILITATOR);
                    break;
                case "Sloth":
                    validJobs.Add(JOB.DEBILITATOR);
                    validJobs.Add(JOB.DIPLOMAT);
                    break;
            }
        } else {
            switch (character.race) {
                case RACE.HUMANS:
                    validJobs.Add(JOB.DIPLOMAT);
                    validJobs.Add(JOB.RECRUITER);
                    break;
                case RACE.ELVES:
                    validJobs.Add(JOB.SPY);
                    validJobs.Add(JOB.DIPLOMAT);
                    break;
                case RACE.GOBLIN:
                    validJobs.Add(JOB.INSTIGATOR);
                    validJobs.Add(JOB.RECRUITER);
                    break;
                case RACE.FAERY:
                    validJobs.Add(JOB.SPY);
                    validJobs.Add(JOB.DEBILITATOR);
                    break;
                case RACE.SKELETON:
                    validJobs.Add(JOB.DEBILITATOR);
                    validJobs.Add(JOB.INSTIGATOR);
                    break;
            }
        }
        return validJobs;
    }
    public bool CanAssignCharacterToJob(JOB job, Character character) {
        List<JOB> jobs = GetValidJobForCharacter(character);
        return jobs.Contains(job);
    }
    public bool CanAssignCharacterToAttack(Character character) {
        return !roleSlots.ContainsValue(character) && !defenseGrid.IsCharacterInGrid(character);
    }
    public bool CanAssignCharacterToDefend(Character character) {
        return !roleSlots.ContainsValue(character) && !attackGrid.IsCharacterInGrid(character);
    }
    public void AssignCharacterToJob(JOB job, Character character) {
        if (!roleSlots.ContainsKey(job)) {
            Debug.LogWarning("There is something trying to assign a character to " + job.ToString() + " but the player doesn't have a slot for it.");
            return;
        }
        if (roleSlots[job] != null) {
            UnassignCharacterFromJob(job);
        }
        JOB charactersCurrentJob = GetCharactersCurrentJob(character);
        if (charactersCurrentJob != JOB.NONE) {
            UnassignCharacterFromJob(charactersCurrentJob);
        }

        roleSlots[job] = character;
        Messenger.Broadcast(Signals.CHARACTER_ASSIGNED_TO_JOB, job, character);
    }
    public void UnassignCharacterFromJob(JOB job) {
        if (!roleSlots.ContainsKey(job)) {
            Debug.LogWarning("There is something trying to unassign a character from " + job.ToString() + " but the player doesn't have a slot for it.");
            return;
        }
        if (roleSlots[job] == null) {
            return; //ignore command
        }
        Character character = roleSlots[job];
        roleSlots[job] = null;
        Messenger.Broadcast(Signals.CHARACTER_UNASSIGNED_FROM_JOB, job, character);
    }
    public void AssignAttackGrid(CombatGrid grid) {
        attackGrid = grid;
    }
    public void AssignDefenseGrid(CombatGrid grid) {
        defenseGrid = grid;
    }
    public JOB GetCharactersCurrentJob(Character character) {
        foreach (KeyValuePair<JOB, Character> keyValuePair in roleSlots) {
            if (keyValuePair.Value != null && keyValuePair.Value.id == character.id) {
                return keyValuePair.Key;
            }
        }
        return JOB.NONE;
    }
    #endregion
}
