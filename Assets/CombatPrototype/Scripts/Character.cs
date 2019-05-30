﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Character : ICharacter, ILeader, IInteractable, IPointOfInterest {
    public delegate void OnCharacterDeath();
    public OnCharacterDeath onCharacterDeath;

    public delegate void DailyAction();
    public DailyAction onDailyAction;

    protected string _name;
    protected string _firstName;
    protected string _characterColorCode;
    protected int _id;
    protected int _gold;
    protected int _currentInteractionTick;
    protected int _doNotDisturb;
    protected int _doNotGetHungry;
    protected int _doNotGetTired;
    protected int _doNotGetLonely;
    protected int _numOfWaitingForGoapThread;
    protected float _actRate;
    protected bool _isDead;
    protected bool _isFainted;
    protected bool _isInCombat;
    protected bool _isBeingInspected;
    protected bool _hasBeenInspected;
    protected bool _alreadyTargetedByGrudge;
    protected bool _isTracked;
    protected bool _activateDailyGoapPlanInteraction;
    protected bool _hasAlreadyAskedForPlan;
    protected bool _isChatting;
    protected GENDER _gender;
    public SEXUALITY sexuality { get; private set; }
    protected MODE _currentMode;
    protected CharacterClass _characterClass;
    protected RaceSetting _raceSetting;
    protected CharacterRole _role;
    protected Job _job;
    protected Faction _faction;
    protected CharacterParty _ownParty;
    protected CharacterParty _currentParty;
    protected Region _currentRegion;
    protected Weapon _equippedWeapon;
    protected Armor _equippedArmor;
    protected Item _equippedAccessory;
    protected Item _equippedConsumable;
    protected PortraitSettings _portraitSettings;
    protected Color _characterColor;
    protected Minion _minion;
    protected CombatCharacter _currentCombatCharacter;
    protected PairCombatStats[] _pairCombatStats;
    protected List<Skill> _skills;
    protected List<Log> _history;
    protected List<Trait> _normalTraits; //List of traits that are just normal Traits (Not including relationships)
    protected Dictionary<ELEMENT, float> _elementalWeaknesses;
    protected Dictionary<ELEMENT, float> _elementalResistances;
    protected PlayerCharacterItem _playerCharacterItem;

    //Stats
    protected SIDES _currentSide;
    protected int _currentHP;
    protected int _currentRow;
    protected int _level;
    protected int _experience;
    protected int _maxExperience;
    protected int _sp;
    protected int _maxSP;
    protected int _attackPowerMod;
    protected int _speedMod;
    protected int _maxHPMod;
    protected int _attackPowerPercentMod;
    protected int _speedPercentMod;
    protected int _maxHPPercentMod;
    protected int _combatBaseAttack;
    protected int _combatBaseSpeed;
    protected int _combatBaseHP;
    protected int _combatAttackFlat;
    protected int _combatAttackMultiplier;
    protected int _combatSpeedFlat;
    protected int _combatSpeedMultiplier;
    protected int _combatHPFlat;
    protected int _combatHPMultiplier;
    protected int _combatPowerFlat;
    protected int _combatPowerMultiplier;

    public Area homeArea { get; protected set; }
    public Dwelling homeStructure { get; protected set; }
    public LocationStructure currentStructure { get; private set; } //what structure is this character currently in.
    public Area defendingArea { get; private set; }
    public MORALITY morality { get; private set; }
    public CharacterToken characterToken { get; private set; }
    public Dictionary<AlterEgoData, CharacterRelationshipData> relationships {
        get {
            return currentAlterEgo?.relationships ?? null;
        }
    }
    public List<INTERACTION_TYPE> currentInteractionTypes { get; private set; }
    public Dictionary<POINT_OF_INTEREST_TYPE, List<IAwareness>> awareness {
        get {
            return currentAlterEgo.awareness;
        }
    }
    public List<INTERACTION_TYPE> poiGoapActions { get; private set; }
    public List<GoapPlan> allGoapPlans { get; private set; }
    public GoapPlanner planner { get; set; }
    public int supply { get; set; }
    public int isWaitingForInteraction { get; private set; }
    public CharacterMarker marker { get; private set; }
    public GoapAction currentAction { get; private set; }
    public GoapAction previousCurrentAction { get; private set; }
    public bool hasAssaultPlan { get; private set; }
    public Character lastAssaultedCharacter { get; private set; }
    public List<GoapAction> targettedByAction { get; private set; }
    public CharacterStateComponent stateComponent { get; private set; }
    public List<SpecialToken> items { get; private set; }
    public JobQueue jobQueue { get; private set; }
    public JobQueueItem currentJob { get; private set; }
    public List<JobQueueItem> allJobsTargettingThis { get; private set; }
    public int moodValue { get; private set; }
    public bool isCombatant { get; private set; } //This should only be a getter but since we need to know when the value changes it now has a setter
    public List<Trait> traitsNeededToBeRemoved { get; private set; }
    //public Memories memories { get; private set; }
    public TrapStructure trapStructure { get; private set; }
    public bool isDisabledByPlayer { get; protected set; }

    private List<System.Action> onLeaveAreaActions;
    private LocationGridTile tile; //what tile in the structure is this character currently in.
    private POI_STATE _state;

    private Dictionary<STAT, float> _buffs;
    public Dictionary<int, Combat> combatHistory;

    //Needs
    public int tiredness { get; private set; }
    private const int TIREDNESS_DEFAULT = 1200;
    private const int TIREDNESS_THRESHOLD_1 = 1040;
    private const int TIREDNESS_THRESHOLD_2 = 880;
    public int fullness { get; private set; }
    private const int FULLNESS_DEFAULT = 1440;
    private const int FULLNESS_THRESHOLD_1 = 1390;
    private const int FULLNESS_THRESHOLD_2 = 1200;
    public int happiness { get; private set; }
    private const int HAPPINESS_DEFAULT = 240;
    private const int HAPPINESS_THRESHOLD_1 = 160;
    private const int HAPPINESS_THRESHOLD_2 = 0;

    //portrait
    public float hSkinColor { get; private set; }
    public float hHairColor { get; private set; }
    public float demonColor { get; private set; }

    //hostility
    public int ignoreHostility { get; private set; }

    //alter egos
    public string currentAlterEgoName { get; private set; } //this character's currently active alter ego. Usually just Original.
    public Dictionary<string, AlterEgoData> alterEgos { get; private set; }

    //For Testing
    public List<string> locationHistory { get; private set; }
    public List<string> actionHistory { get; private set; }

    #region getters / setters
    public string firstName {
        get { return _firstName; }
    }
    public virtual string name {
        get {
            //if(_minion != null) {
            //    return _minion.name;
            //}
            return _firstName;
        }
    }
    public string coloredName {
        get { return "<color=#" + this._characterColorCode + ">" + name + "</color>"; }
    }
    public string urlName {
        get { return "<link=" + '"' + this._id.ToString() + "_character" + '"' + ">" + name + "</link>"; }
    }
    public string coloredUrlName {
        get { return "<link=" + '"' + this._id.ToString() + "_character" + '"' + ">" + "<color=#" + this._characterColorCode + ">" + name + "</color></link>"; }
    }
    public string raceClassName {
        get {
            if (Utilities.IsRaceBeast(race)) {
                return Utilities.NormalizeString(race.ToString()) + " " + role.name;
            }
            //if(role.name == characterClass.className) {
            return Utilities.GetNormalizedRaceAdjective(race) + " " + characterClass.className;
            //}
            //return Utilities.GetNormalizedRaceAdjective(race) + " " + role.name + " " + characterClass.className;
        }
    }
    public int id {
        get { return _id; }
    }
    public bool isDead {
        get { return this._isDead; }
    }
    public bool isFainted {
        get { return this._isFainted; }
    }
    public bool isInCombat {
        get {
            return _isInCombat;
        }
    }
    public bool isFactionless { //is the character part of the neutral faction? or no faction?
        get {
            if (faction == null || FactionManager.Instance.neutralFaction.id == faction.id) {
                return true;
            } else {
                return false;
            }
        }
    }
    public bool isIdle {
        get { return _doNotDisturb <= 0 && IsInOwnParty() && !currentParty.icon.isTravelling; }
    }
    public bool isBeingInspected {
        get { return _isBeingInspected; }
    }
    public bool hasBeenInspected {
        get { return _hasBeenInspected; }
    }
    public bool alreadyTargetedByGrudge {
        get { return _alreadyTargetedByGrudge; }
    }
    public bool isLeader {
        get { return job.jobType == JOB.LEADER; }
    }
    public bool isHoldingItem {
        get { return items.Count > 0; }
    }
    public bool isAtHomeStructure {
        get { return currentStructure == homeStructure; }
    }
    public bool isAtHomeArea {
        get { return specificLocation.id == homeArea.id; }
    }
    public bool isTracked {
        get {
            if (GameManager.Instance.inspectAll) {
                return true;
            }
            return _isTracked;
        }
    }
    public bool isChatting {
        get { return _isChatting; }
    }
    public GENDER gender {
        get { return _gender; }
    }
    public MODE currentMode {
        get { return _currentMode; }
    }
    public RACE race {
        get { return _raceSetting.race; }
    }
    public CharacterClass characterClass {
        get { return this._characterClass; }
    }
    public RaceSetting raceSetting {
        get { return _raceSetting; }
    }
    public CharacterRole role {
        get { return _role; }
    }
    public Job job {
        get { return _job; }
    }
    public Faction faction {
        get { return _faction; }
    }
    public Faction factionOwner {
        get { return _faction; }
    }
    public virtual Party ownParty {
        get { return _ownParty; }
    }
    public CharacterParty party {
        get { return _ownParty; }
    }
    public virtual Party currentParty {
        get { return _currentParty; }
    }
    public HexTile currLocation {
        get { return (_currentParty.specificLocation != null ? _currentParty.specificLocation.coreTile : null); }
    }
    public Area specificLocation {
        get { return _currentParty.specificLocation; }
    }
    public List<Skill> skills {
        get { return this._skills; }
    }
    public int currentRow {
        get { return this._currentRow; }
    }
    public SIDES currentSide {
        get { return this._currentSide; }
    }
    public Color characterColor {
        get { return _characterColor; }
    }
    public string characterColorCode {
        get { return _characterColorCode; }
    }
    public List<Log> history {
        get { return this._history; }
    }
    public int gold {
        get { return _gold; }
    }
    public PortraitSettings portraitSettings {
        get { return _portraitSettings; }
    }
    public int level {
        get { return _level; }
    }
    public int currentSP {
        get { return _sp; }
    }
    public int maxSP {
        get { return _maxSP; }
    }
    public int experience {
        get { return _experience; }
    }
    public int maxExperience {
        get { return _maxExperience; }
    }
    public int speed {
        get {
            int total = (int)((_characterClass.baseSpeed + _speedMod) * (1f + ((_raceSetting.speedModifier + _speedPercentMod) / 100f)));
            if (total < 0) {
                return 1;
            }
            return total;
        }
    }
    public int attackPower {
        get {
            int total = (int)((_characterClass.baseAttackPower + _attackPowerMod) * (1f + ((_raceSetting.attackPowerModifier + _attackPowerPercentMod) / 100f)));
            if (total < 0) {
                return 1;
            }
            return total;
        }
    }
    public int maxHP {
        get {
            int total = (int)((_characterClass.baseHP + _maxHPMod) * (1f + ((_raceSetting.hpModifier + _maxHPPercentMod) / 100f)));
            if (total < 0) {
                return 1;
            }
            return total;
        }
    }
    //public int combatBaseAttack {
    //    get { return _combatBaseAttack; }
    //    set { _combatBaseAttack = value; }
    //}
    //public int combatBaseSpeed {
    //    get { return _combatBaseSpeed; }
    //    set { _combatBaseSpeed = value; }
    //}
    //public int combatBaseHP {
    //    get { return _combatBaseHP; }
    //    set { _combatBaseHP = value; }
    //}
    //public int combatAttackFlat {
    //    get { return _combatAttackFlat; }
    //    set { _combatAttackFlat = value; }
    //}
    //public int combatAttackMultiplier {
    //    get { return _combatAttackMultiplier; }
    //    set { _combatAttackMultiplier = value; }
    //}
    //public int combatSpeedFlat {
    //    get { return _combatSpeedFlat; }
    //    set { _combatSpeedFlat = value; }
    //}
    //public int combatSpeedMultiplier {
    //    get { return _combatSpeedMultiplier; }
    //    set { _combatSpeedMultiplier = value; }
    //}
    //public int combatHPFlat {
    //    get { return _combatHPFlat; }
    //    set { _combatHPFlat = value; }
    //}
    //public int combatHPMultiplier {
    //    get { return _combatHPMultiplier; }
    //    set { _combatHPMultiplier = value; }
    //}
    //public int combatPowerFlat {
    //    get { return _combatPowerFlat; }
    //    set { _combatPowerFlat = value; }
    //}
    //public int combatPowerMultiplier {
    //    get { return _combatPowerMultiplier; }
    //    set { _combatPowerMultiplier = value; }
    //}
    public int currentHP {
        get { return this._currentHP; }
    }
    //public PairCombatStats[] pairCombatStats {
    //    get { return _pairCombatStats; }
    //    set { _pairCombatStats = value; }
    //}
    public Dictionary<ELEMENT, float> elementalWeaknesses {
        get { return _elementalWeaknesses; }
    }
    public Dictionary<ELEMENT, float> elementalResistances {
        get { return _elementalResistances; }
    }
    public float actRate {
        get { return _actRate; }
        set { _actRate = value; }
    }
    public Weapon equippedWeapon {
        get { return _equippedWeapon; }
    }
    public Armor equippedArmor {
        get { return _equippedArmor; }
    }
    public Item equippedAccessory {
        get { return _equippedAccessory; }
    }
    public Item equippedConsumable {
        get { return _equippedConsumable; }
    }
    public float computedPower {
        get { return GetComputedPower(); }
    }
    public ICHARACTER_TYPE icharacterType {
        get { return ICHARACTER_TYPE.CHARACTER; }
    }
    public Minion minion {
        get { return _minion; }
    }
    public int doNotDisturb {
        get { return _doNotDisturb; }
    }
    public QUEST_GIVER_TYPE questGiverType {
        get { return QUEST_GIVER_TYPE.CHARACTER; }
    }
    public bool isDefender {
        get { return defendingArea != null; }
    }
    //returns all traits, including relationships from this character's current alter ego
    public List<Trait> allTraits {
        get {
            List<Trait> combined = new List<Trait>(_normalTraits);
            for (int i = 0; i < relationships.Values.Count; i++) {
                combined.AddRange(relationships.Values.ElementAt(i).rels);
            }
            return combined;
        }
    }
    public List<Trait> normalTraits {
        get {
            return _normalTraits;
        }
    }
    public Dictionary<STAT, float> buffs {
        get { return _buffs; }
    }
    public PlayerCharacterItem playerCharacterItem {
        get { return _playerCharacterItem; }
    }
    public int currentInteractionTick {
        get { return _currentInteractionTick; }
    }
    public CombatCharacter currentCombatCharacter {
        get { return _currentCombatCharacter; }
    }
    public POINT_OF_INTEREST_TYPE poiType {
        get { return POINT_OF_INTEREST_TYPE.CHARACTER; }
    }
    public LocationGridTile gridTileLocation {
        get {
            return GetLocationGridTileByXY(gridTilePosition.x, gridTilePosition.y);
            //if (tile == null) {
            //    LocationGridTile gridTile = specificLocation.areaMap.map[(int) marker.anchoredPos.x, (int) marker.anchoredPos.y];
            //    return gridTile;
            //}
            //return tile;
        }
    }
    public Vector2Int gridTilePosition {
        get { return new Vector2Int((int) marker.anchoredPos.x, (int) marker.anchoredPos.y); }
    }
    public POI_STATE state {
        get { return _state; }
    }
    public POICollisionTrigger collisionTrigger {
        get { return marker.collisionTrigger; }
    }
    public CHARACTER_MOOD currentMoodType {
        get { return ConvertCurrentMoodValueToType(); }
    }
    public bool isStarving { get { return fullness <= FULLNESS_THRESHOLD_2; } }
    public bool isExhausted { get { return tiredness <= TIREDNESS_THRESHOLD_2; } }
    public bool isForlorn { get { return happiness <= HAPPINESS_THRESHOLD_2; } }
    public Tombstone grave { get; private set; }
    public AlterEgoData currentAlterEgo {
        get {
            if (alterEgos == null || !alterEgos.ContainsKey(currentAlterEgoName)) {
                Debug.LogWarning(this.name + " Alter Ego Relationship Problem! Current alter ego is: " + currentAlterEgoName);
                return null;
            }
            return alterEgos[currentAlterEgoName];
        }
    }
    #endregion

    public Character(CharacterRole role, RACE race, GENDER gender) : this() {
        _id = Utilities.SetID(this);
        _gender = gender;
        RaceSetting raceSetting = RaceManager.Instance.racesDictionary[race.ToString()];
        _raceSetting = raceSetting.CreateNewCopy();
        AssignRole(role, false);
        _characterClass = CharacterManager.Instance.CreateNewCharacterClass(GetClassForRole(role));
        SetName(RandomNameGenerator.Instance.GenerateRandomName(_raceSetting.race, _gender));
        AssignRandomJob();
        SetMorality(MORALITY.GOOD);
        GenerateSexuality();
        ResetToFullHP();
        InitializeAlterEgos();
    }
    public Character(CharacterRole role, string className, RACE race, GENDER gender) : this() {
        _id = Utilities.SetID(this);
        _gender = gender;
        RaceSetting raceSetting = RaceManager.Instance.racesDictionary[race.ToString()];
        _raceSetting = raceSetting.CreateNewCopy();
        AssignRole(role, false);
        _characterClass = CharacterManager.Instance.CreateNewCharacterClass(className);
        SetName(RandomNameGenerator.Instance.GenerateRandomName(_raceSetting.race, _gender));
        AssignRandomJob();
        SetMorality(MORALITY.GOOD);
        GenerateSexuality();
        ResetToFullHP();
        InitializeAlterEgos();
    }
    public Character(CharacterSaveData data) : this() {
        _id = Utilities.SetID(this, data.id);
        _gender = data.gender;
        RaceSetting raceSetting = RaceManager.Instance.racesDictionary[race.ToString()];
        _raceSetting = raceSetting.CreateNewCopy();
        AssignRole(data.role, false);
        _characterClass = CharacterManager.Instance.CreateNewCharacterClass(data.className);
        SetName(data.name);
        AssignRandomJob();
        SetMorality(data.morality);
        GenerateSexuality();
        ResetToFullHP();
        InitializeAlterEgos();
    }
    public Character() {
        SetIsDead(false);
        _isFainted = false;
        _history = new List<Log>();
        _normalTraits = new List<Trait>();

        //RPG
        _level = 1;
        _experience = 0;
        _elementalWeaknesses = new Dictionary<ELEMENT, float>(CharacterManager.Instance.elementsChanceDictionary);
        _elementalResistances = new Dictionary<ELEMENT, float>(CharacterManager.Instance.elementsChanceDictionary);
        combatHistory = new Dictionary<int, Combat>();
        currentInteractionTypes = new List<INTERACTION_TYPE>();
        characterToken = new CharacterToken(this);
        poiGoapActions = new List<INTERACTION_TYPE>();
        allGoapPlans = new List<GoapPlan>();
        hasAssaultPlan = false;
        targettedByAction = new List<GoapAction>();
        stateComponent = new CharacterStateComponent(this);
        items = new List<SpecialToken>();
        jobQueue = new JobQueue(this);
        allJobsTargettingThis = new List<JobQueueItem>();
        traitsNeededToBeRemoved = new List<Trait>();
        onLeaveAreaActions = new List<Action>();
        //memories = new Memories();
        trapStructure = new TrapStructure();
        SetPOIState(POI_STATE.ACTIVE);
        
        //for testing
        locationHistory = new List<string>();
        actionHistory = new List<string>();
        //If this is a minion, this should not be initiated
        //awareness = new Dictionary<POINT_OF_INTEREST_TYPE, List<IAwareness>>();
        planner = new GoapPlanner(this);
       
        //hostiltiy
        ignoreHostility = 0;
    }
    /// <summary>
    /// Initialize data for this character that is not safe to put in the constructor.
    /// Usually this is data that is dependent on the character being fully constructed.
    /// </summary>
    public void Initialize() {
        OnUpdateRace();
        OnUpdateCharacterClass();
        UpdateIsCombatantState();

        SetMoodValue(90);

        tiredness = TIREDNESS_DEFAULT;
        //Fullness value between 1300 and 1440.
        SetFullness(UnityEngine.Random.Range(1300, FULLNESS_DEFAULT + 1));
        //Happiness value between 100 and 240.
        SetHappiness(UnityEngine.Random.Range(100, HAPPINESS_DEFAULT + 1));

        hSkinColor = UnityEngine.Random.Range(-360f, 360f);
        hHairColor = UnityEngine.Random.Range(-360f, 360f);
        demonColor = UnityEngine.Random.Range(-144f, 144f);

        //supply
        SetSupply(UnityEngine.Random.Range(10, 61)); //Randomize initial supply per character (Random amount between 10 to 60.)
        GetRandomCharacterColor();

        ConstructInitialGoapAdvertisementActions();
        SubscribeToSignals();
        GameDate gameDate = GameManager.Instance.Today();
        gameDate.AddTicks(1);
        SchedulingManager.Instance.AddEntry(gameDate, () => PlanGoapActions());
    }

    #region Signals
    private void SubscribeToSignals() {
        Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnOtherCharacterDied);
        //Messenger.AddListener(Signals.TICK_STARTED, DailyInteractionGeneration);
        Messenger.AddListener(Signals.TICK_STARTED, DailyGoapPlanGeneration);
        Messenger.AddListener(Signals.HOUR_STARTED, DecreaseNeeds);
        //Messenger.AddListener<Character, Area, Area>(Signals.CHARACTER_MIGRATED_HOME, OnCharacterMigratedHome);
        Messenger.AddListener<Character, GoapAction, string>(Signals.CHARACTER_FINISHED_ACTION, OnCharacterFinishedAction);
        Messenger.AddListener<Party>(Signals.PARTY_STARTED_TRAVELLING, OnLeaveArea);
        Messenger.AddListener<Party>(Signals.PARTY_DONE_TRAVELLING, OnArrivedAtArea);
        Messenger.AddListener<Character, string>(Signals.CANCEL_CURRENT_ACTION, CancelCurrentAction);
        Messenger.AddListener<GoapAction, GoapActionState>(Signals.ACTION_STATE_SET, OnActionStateSet);

    }
    public void UnsubscribeSignals() {
        Messenger.RemoveListener<Character>(Signals.CHARACTER_DEATH, OnOtherCharacterDied);
        //Messenger.RemoveListener(Signals.TICK_STARTED, DailyInteractionGeneration);
        Messenger.RemoveListener(Signals.TICK_STARTED, DailyGoapPlanGeneration);
        Messenger.RemoveListener(Signals.HOUR_STARTED, DecreaseNeeds);
        //Messenger.RemoveListener<Character, Area, Area>(Signals.CHARACTER_MIGRATED_HOME, OnCharacterMigratedHome);
        Messenger.RemoveListener<Character, GoapAction, string>(Signals.CHARACTER_FINISHED_ACTION, OnCharacterFinishedAction);
        Messenger.RemoveListener<Party>(Signals.PARTY_STARTED_TRAVELLING, OnLeaveArea);
        Messenger.RemoveListener<Party>(Signals.PARTY_DONE_TRAVELLING, OnArrivedAtArea);
        Messenger.RemoveListener<Character, string>(Signals.CANCEL_CURRENT_ACTION, CancelCurrentAction);
        Messenger.RemoveListener<GoapAction, GoapActionState>(Signals.ACTION_STATE_SET, OnActionStateSet);
    }
    #endregion

    #region Sexuality
    private void GenerateSexuality() {
        if (role.roleType == CHARACTER_ROLE.BEAST) {
            //For beasts:
            //100 % straight
            sexuality = SEXUALITY.STRAIGHT;
        } else {
            //For sapient creatures:
            //80 % straight
            //10 % bisexual
            //10 % gay
            int chance = UnityEngine.Random.Range(0, 100);
            if (chance < 80) {
                sexuality = SEXUALITY.STRAIGHT;
            } else if (chance >= 80 && chance < 90) {
                sexuality = SEXUALITY.BISEXUAL;
            } else {
                sexuality = SEXUALITY.GAY;
            }
        }
    }
    #endregion

    #region Marker
    public void CreateMarker() {
        GameObject portraitGO = ObjectPoolManager.Instance.InstantiateObjectFromPool("CharacterMarker", Vector3.zero, Quaternion.identity, InteriorMapManager.Instance.transform);
        //RectTransform rect = portraitGO.transform as RectTransform;
        //portraitGO.transform.localPosition = pos;
        SetCharacterMarker(portraitGO.GetComponent<CharacterMarker>());
        marker.SetCharacter(this);
#if UNITY_EDITOR
        marker.SetHoverAction(ShowTileData, InteriorMapManager.Instance.HideTileData);
#endif
    }
    public void DestroyMarker() {
        ObjectPoolManager.Instance.DestroyObject(marker.gameObject);
        gridTileLocation.RemoveCharacterHere(this);
    }
    #endregion

    public void SetCharacterMarker(CharacterMarker marker) {
        this.marker = marker;
    }
    public void ShowTileData(Character character, LocationGridTile location) {
        InteriorMapManager.Instance.ShowTileData(this, gridTileLocation);
    }
    //Changes row number of this character
    public void SetRowNumber(int rowNumber) {
        this._currentRow = rowNumber;
    }
    //Changes character's side
    public void SetSide(SIDES side) {
        this._currentSide = side;
    }
    //Adjust current HP based on specified paramater, but HP must not go below 0
    public virtual void AdjustHP(int amount, bool triggerDeath = false) {
        int previous = this._currentHP;
        this._currentHP += amount;
        this._currentHP = Mathf.Clamp(this._currentHP, 0, maxHP);
        Messenger.Broadcast(Signals.ADJUSTED_HP, this);
        if (triggerDeath && previous != this._currentHP) {
            if (this._currentHP == 0) {
                Death();
            }
        }
    }

    private string GetFaintOrDeath() {
        return "die";
    }
    //When character will faint
    internal void Faint() {
        if (!_isFainted) {
            _isFainted = true;
            SetHP(1);
            ////Set Task to Fainted
            //Faint faintTask = new Faint(this);
            //faintTask.OnChooseTask(this)
            ; }
    }
    internal void Unfaint() {
        if (_isFainted) {
            _isFainted = false;
            SetHP(1);
        }
    }
    //Character's death
    public void SetIsDead(bool isDead) {
        _isDead = isDead;
    }
    public void ReturnToLife() {
        if (_isDead) {
            SetIsDead(false);
            SubscribeToSignals();
            SetPOIState(POI_STATE.ACTIVE);
            _ownParty.ReturnToLife();
        }
    }
    public void Death(string cause = "normal") {
        if (!_isDead) {
            SetIsDead(true);
            UnsubscribeSignals();
            SetPOIState(POI_STATE.INACTIVE);
            CombatManager.Instance.ReturnCharacterColorToPool(_characterColor);

            if (currentParty.specificLocation == null) {
                throw new Exception("Specific location of " + this.name + " is null! Please use command /l_character_location_history [Character Name/ID] in console menu to log character's location history. (Use '~' to show console menu)");
            }
            if (stateComponent.currentState != null) {
                stateComponent.currentState.OnExitThisState();
            }
            CancelAllJobsTargettingThisCharacter("target is already dead", false);
            Messenger.Broadcast(Signals.CANCEL_CURRENT_ACTION, this, "target is already dead");
            if (currentAction != null && !currentAction.cannotCancelAction) {
                currentAction.StopAction();
            }
            if (ownParty.specificLocation != null && isHoldingItem) {
                DropAllTokens(ownParty.specificLocation, currentStructure, true);
            }

            //clear traits that need to be removed
            traitsNeededToBeRemoved.Clear();

            Area deathLocation = ownParty.specificLocation;
            LocationStructure deathStructure = currentStructure;
            LocationGridTile deathTile = gridTileLocation;

            if (!IsInOwnParty()) {
                _currentParty.RemoveCharacter(this);
            }
            _ownParty.PartyDeath();

            //if (this.race != RACE.SKELETON) {
            //    deathLocation.AddCorpse(this, deathStructure, deathTile);
            //}

            if (this._faction != null) {
                this._faction.RemoveCharacter(this); //remove this character from it's factions list of characters
            }

            if (_role != null) {
                _role.OnDeath(this);
            }
            if (homeArea != null) {
                Area home = homeArea;
                Dwelling homeStructure = this.homeStructure;
                homeArea.RemoveResident(this);
                SetHome(home); //keep this data with character to prevent errors
                SetHomeStructure(homeStructure); //keep this data with character to prevent errors
            }

            //List<Character> characterRels = new List<Character>(this.relationships.Keys.ToList());
            //for (int i = 0; i < characterRels.Count; i++) {
            //    RemoveRelationship(characterRels[i]);
            //}

            if (_minion != null) {
                PlayerManager.Instance.player.RemoveMinion(_minion);
            }

            //ObjectPoolManager.Instance.DestroyObject(marker.gameObject);
            //deathTile.RemoveCharacterHere(this);

            for (int i = 0; i < allTraits.Count; i++) {
                allTraits[i].OnDeath();
            }

            marker.OnDeath(deathTile);

            if (onCharacterDeath != null) {
                onCharacterDeath();
            }
            onCharacterDeath = null;
            Messenger.Broadcast(Signals.CHARACTER_DEATH, this);

            Debug.Log(GameManager.Instance.TodayLogString() + this.name + " died of " + cause);
            Log log = new Log(GameManager.Instance.Today(), "Character", "Generic", "death_" + cause);
            log.AddToFillers(this, name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            AddHistory(log);
            specificLocation.AddHistory(log);

        }
    }
    public void Assassinate(Character assassin) {
        Debug.Log(assassin.name + " assassinated " + name);
        Death();
    }
    internal void AddActionOnDeath(OnCharacterDeath onDeathAction) {
        onCharacterDeath += onDeathAction;
    }
    internal void RemoveActionOnDeath(OnCharacterDeath onDeathAction) {
        onCharacterDeath -= onDeathAction;
    }
    public void SetGrave(Tombstone grave) {
        this.grave = grave;
    }

    #region Items
    public void EquipItem(string itemName) {
        Item item = ItemManager.Instance.CreateNewItemInstance(itemName);
        if (item != null) {
            EquipItem(item);
        }
    }
    /*
        this is the real way to equip an item
        this will return a boolean whether the character successfully equipped
        the item or not.
            */
    internal bool EquipItem(Item item) {
        bool hasEquipped = false;
        if (item.itemType == ITEM_TYPE.WEAPON) {
            Weapon weapon = item as Weapon;
            hasEquipped = TryEquipWeapon(weapon);
        } else if (item.itemType == ITEM_TYPE.ARMOR) {
            Armor armor = item as Armor;
            hasEquipped = TryEquipArmor(armor);
        } else if (item.itemType == ITEM_TYPE.ACCESSORY) {
            hasEquipped = TryEquipAccessory(item);
        } else if (item.itemType == ITEM_TYPE.CONSUMABLE) {
            hasEquipped = TryEquipConsumable(item);
        }
        if (hasEquipped) {
            if (item.attributeNames != null) {
                for (int i = 0; i < item.attributeNames.Count; i++) {
                    Trait newTrait = AttributeManager.Instance.allTraits[item.attributeNames[i]];
                    AddTrait(newTrait);
                }
            }
#if !WORLD_CREATION_TOOL
            Log equipLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "equip_item");
            equipLog.AddToFillers(this, this.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            equipLog.AddToFillers(null, item.itemName, LOG_IDENTIFIER.ITEM_1);
            AddHistory(equipLog);
#endif
            Messenger.Broadcast(Signals.ITEM_EQUIPPED, item, this);
        }
        return hasEquipped;
    }
    //Unequips an item of a character, whether it's a weapon, armor, etc.
    public void UnequipItem(Item item) {
        if (item.itemType == ITEM_TYPE.WEAPON) {
            UnequipWeapon(item as Weapon);
        } else if (item.itemType == ITEM_TYPE.ARMOR) {
            UnequipArmor(item as Armor);
        } else if (item.itemType == ITEM_TYPE.ACCESSORY) {
            UnequipAccessory(item);
        } else if (item.itemType == ITEM_TYPE.CONSUMABLE) {
            UnequipConsumable(item);
        }
        if (item.attributeNames != null) {
            for (int i = 0; i < item.attributeNames.Count; i++) {
                Trait newTrait = AttributeManager.Instance.allTraits[item.attributeNames[i]];
                RemoveTrait(newTrait);
            }
        }
        Messenger.Broadcast(Signals.ITEM_UNEQUIPPED, item, this);
    }
    //Own an Item
    internal void OwnItem(Item item) {
        item.SetOwner(this);
    }
    //Transfer item ownership
    internal void TransferItemOwnership(Item item, Character newOwner) {
        newOwner.OwnItem(item);
    }
    //Try to equip a weapon to a body part of this character and add it to the list of items this character have
    internal bool TryEquipWeapon(Weapon weapon) {
        //if (!_characterClass.allowedWeaponTypes.Contains(weapon.weaponType)) {
        //    return false;
        //}
        _equippedWeapon = weapon;
        weapon.SetEquipped(true);
        return true;
    }
    //Unequips weapon of a character
    private void UnequipWeapon(Weapon weapon) {
        weapon.SetEquipped(false);
        _equippedWeapon = null;
    }
    //Try to equip an armor to a body part of this character and add it to the list of items this character have
    internal bool TryEquipArmor(Armor armor) {
        armor.SetEquipped(true);
        _equippedArmor = armor;
        return true;
    }
    //Unequips armor of a character
    private void UnequipArmor(Armor armor) {
        armor.SetEquipped(false);
        _equippedArmor = null;
    }
    //Try to equip an accessory
    internal bool TryEquipAccessory(Item accessory) {
        accessory.SetEquipped(true);
        _equippedAccessory = accessory;
        return true;
    }
    //Unequips accessory of a character
    private void UnequipAccessory(Item accessory) {
        accessory.SetEquipped(false);
        _equippedAccessory = null;
    }
    //Try to equip an consumable
    internal bool TryEquipConsumable(Item consumable) {
        consumable.SetEquipped(true);
        _equippedConsumable = consumable;
        return true;
    }
    //Unequips consumable of a character
    private void UnequipConsumable(Item consumable) {
        consumable.SetEquipped(false);
        _equippedConsumable = null;
    }
    internal bool HasItem(string itemName) {
        if (_equippedWeapon != null && _equippedWeapon.itemName == itemName) {
            return true;
        } else if (_equippedArmor != null && _equippedArmor.itemName == itemName) {
            return true;
        } else if (_equippedAccessory != null && _equippedAccessory.itemName == itemName) {
            return true;
        } else if (_equippedConsumable != null && _equippedConsumable.itemName == itemName) {
            return true;
        }
        return false;
    }
    internal bool HasItem(Item item) {
        if (_equippedWeapon != null && _equippedWeapon.itemName == item.itemName) {
            return true;
        } else if (_equippedArmor != null && _equippedArmor.itemName == item.itemName) {
            return true;
        } else if (_equippedAccessory != null && _equippedAccessory.itemName == item.itemName) {
            return true;
        } else if (_equippedConsumable != null && _equippedConsumable.itemName == item.itemName) {
            return true;
        }
        return false;
    }
    /*
        Does this character have an item that is like the required item.
        For example, if you want to check if the character has any scrolls,
        without specifying the types of scrolls.
            */
    internal bool HasItemLike(string itemName, int quantity) {
        int counter = 0;
        if (_equippedWeapon != null && _equippedWeapon.itemName == itemName) {
            counter++;
        } else if (_equippedArmor != null && _equippedArmor.itemName == itemName) {
            counter++;
        } else if (_equippedAccessory != null && _equippedAccessory.itemName == itemName) {
            counter++;
        } else if (_equippedConsumable != null && _equippedConsumable.itemName == itemName) {
            counter++;
        }
        if (counter >= quantity) {
            return true;
        } else {
            return false;
        }
    }
    public List<Item> GetItemsLike(string itemName) {
        List<Item> items = new List<Item>();
        if (_equippedWeapon != null && _equippedWeapon.itemName == itemName) {
            items.Add(_equippedWeapon);
        } else if (_equippedArmor != null && _equippedArmor.itemName == itemName) {
            items.Add(_equippedArmor);
        } else if (_equippedAccessory != null && _equippedAccessory.itemName == itemName) {
            items.Add(_equippedAccessory);
        } else if (_equippedConsumable != null && _equippedConsumable.itemName == itemName) {
            items.Add(_equippedConsumable);
        }
        return items;
    }
    #endregion

    #region Skills
    //private List<Skill> GetGeneralSkills(){
    //          List<Skill> allGeneralSkills = new List<Skill>();
    //          foreach (Skill skill in SkillManager.Instance.generalSkills.Values) {
    //              allGeneralSkills.Add(skill.CreateNewCopy());
    //          }
    //          return allGeneralSkills;
    //}
    //public List<Skill> GetClassSkills() {
    //    List<Skill> skills = new List<Skill>();
    //    for (int i = 0; i < level; i++) {
    //        if (i < characterClass.skillsPerLevel.Count) {
    //            if (characterClass.skillsPerLevel[i] != null) {
    //                for (int j = 0; j < characterClass.skillsPerLevel[i].Length; j++) {
    //                    Skill skill = characterClass.skillsPerLevel[i][j];
    //                    skills.Add(skill);
    //                }
    //            }
    //        }
    //    }
    //    return skills;
    //}
    //public List<AttackSkill> GetClassAttackSkills() {
    //    List<AttackSkill> skills = new List<AttackSkill>();
    //    for (int i = 0; i < level; i++) {
    //        if (i < characterClass.skillsPerLevel.Count) {
    //            if (characterClass.skillsPerLevel[i] != null) {
    //                for (int j = 0; j < characterClass.skillsPerLevel[i].Length; j++) {
    //                    Skill skill = characterClass.skillsPerLevel[i][j];
    //                    if(skill is AttackSkill) {
    //                        skills.Add(skill as AttackSkill);
    //                    }
    //                }
    //            }
    //        }
    //    }
    //    return skills;
    //}
    //private List<Skill> GetBodyPartSkills(){
    //	List<Skill> allBodyPartSkills = new List<Skill>();
    //	foreach (Skill skill in SkillManager.Instance.bodyPartSkills.Values) {
    //		bool requirementsPassed = true;
    //		//Skill skill	= SkillManager.Instance.bodyPartSkills [skillName];
    //		for (int j = 0; j < skill.skillRequirements.Length; j++) {
    //			if(!HasAttribute(skill.skillRequirements[j].attributeRequired, skill.skillRequirements[j].itemQuantity)){
    //				requirementsPassed = false;
    //				break;
    //			}
    //		}
    //		if(requirementsPassed){
    //			allBodyPartSkills.Add (skill.CreateNewCopy ());
    //		}
    //	}
    //	return allBodyPartSkills;
    //}
    #endregion

    #region Roles
    public void AssignRole(CharacterRole role, bool updateCombatantState = true) {
        bool wasRoleChanged = false;
        if (_role != null) {
            if (_role.roleType == role.roleType) {
                //If character role is being changed to same role, do not change it
                return;
            }
            _role.OnChange(this);
            wasRoleChanged = true;
        }
        _role = role;
        if (_role != null) {
            _role.OnAssign(this);
        }
        if (wasRoleChanged) {
            Messenger.Broadcast(Signals.ROLE_CHANGED, this);
        }
        if (updateCombatantState) {
            UpdateIsCombatantState();
        }
    }
    #endregion

    #region Character Class
    private string GetClassForRole(CharacterRole role) {
        if (role == CharacterRole.BEAST) {
            return Utilities.GetRespectiveBeastClassNameFromByRace(race);
        } else {
            string className = CharacterManager.Instance.GetRandomClassByIdentifier(role.classNameOrIdentifier);
            if (className != string.Empty) {
                return className;
            } else {
                return role.classNameOrIdentifier;
            }
        }
    }
    public void AssignClassByRole(CharacterRole role) {
        AssignClass(GetClassForRole(role));
    }
    public void RemoveClass() {
        if (_characterClass == null) { return; }
        RemoveTraitsFromClass();
        _characterClass = null;
    }
    private void AssignClass(string className) {
        if (CharacterManager.Instance.classesDictionary.ContainsKey(className)) {
            if (_characterClass != null) {
                //This means that the character currently has a class and it will be replaced with a new class
                RemoveTraitsFromClass();
            }
            _characterClass = CharacterManager.Instance.CreateNewCharacterClass(className);
            //_skills = new List<Skill>();
            //_skills.Add(_characterClass.skill);
            //EquipItemsByClass();
            OnUpdateCharacterClass();
        } else {
            throw new Exception("There is no class named " + className + " but it is being assigned to " + this.name);
        }
    }
    private void OnUpdateCharacterClass() {
        SetTraitsFromClass();
    }
    public void AssignClass(CharacterClass characterClass) {
        if (_characterClass != null) {
            //This means that the character currently has a class and it will be replaced with a new class
            RemoveTraitsFromClass();
        }
        _characterClass = characterClass;
        OnUpdateCharacterClass();
    }
    #endregion

    #region Jobs
    private void AssignRandomJob() {
        if (CharacterManager.Instance.IsClassADeadlySin(_characterClass.className)) {
            AssignJob(_characterClass.jobType);
        } else {
            JOB[] jobs = new JOB[] { JOB.DIPLOMAT, JOB.DEBILITATOR, JOB.EXPLORER, JOB.INSTIGATOR, JOB.RAIDER, JOB.RECRUITER, JOB.SPY };
            AssignJob(jobs[UnityEngine.Random.Range(0, jobs.Length)]);
            //AssignJob(JOB.RAIDER);
        }
    }
    public void AssignJob(JOB jobType) {
        switch (jobType) {
            case JOB.SPY:
                _job = new Spy(this);
                break;
            case JOB.RAIDER:
                _job = new Raider(this);
                break;
            case JOB.INSTIGATOR:
                _job = new Instigator(this);
                break;
            case JOB.EXPLORER:
                _job = new Explorer(this);
                break;
            case JOB.DEBILITATOR:
                _job = new Dissuader(this);
                break;
            case JOB.DIPLOMAT:
                _job = new Diplomat(this);
                break;
            case JOB.RECRUITER:
                _job = new Recruiter(this);
                break;
            case JOB.LEADER:
                _job = new LeaderJob(this);
                break;
            case JOB.WORKER:
                _job = new Worker(this);
                break;
            default:
                _job = new Job(this, JOB.NONE);
                break;
        }
        _job.OnAssignJob();
    }
    public void SetCurrentJob(JobQueueItem job) {
        currentJob = job;
    }
    public void AddJobTargettingThisCharacter(JobQueueItem job) {
        allJobsTargettingThis.Add(job);
    }
    public bool RemoveJobTargettingThisCharacter(JobQueueItem job) {
        return allJobsTargettingThis.Remove(job);
    }
    public void CancelAllJobsTargettingThisCharacter(string jobName) {
        for (int i = 0; i < allJobsTargettingThis.Count; i++) {
            JobQueueItem job = allJobsTargettingThis[i];
            if (job.name == jobName) {
                if (job.jobQueueParent.CancelJob(job)) {
                    i--;
                }
            }
        }
    }
    public void CancelAllJobsTargettingThisCharacter(string jobName, JobQueueItem except) {
        for (int i = 0; i < allJobsTargettingThis.Count; i++) {
            JobQueueItem job = allJobsTargettingThis[i];
            if (job.name == jobName && job != except) {
                if (job.jobQueueParent.CancelJob(job)) {
                    i--;
                }
            }
        }
    }
    public void CancelAllJobsTargettingThisCharacter(string jobName, object conditionKey) {
        for (int i = 0; i < allJobsTargettingThis.Count; i++) {
            if (allJobsTargettingThis[i] is GoapPlanJob) {
                GoapPlanJob job = allJobsTargettingThis[i] as GoapPlanJob;
                if (job.name == jobName && job.targetEffect.conditionKey == conditionKey) {
                    if (job.jobQueueParent.CancelJob(job)) {
                        i--;
                    }
                }
            }
        }
    }
    public void CancelAllJobsTargettingThisCharacter(string cause = "", bool shouldDoAfterEffect = true) {
        for (int i = 0; i < allJobsTargettingThis.Count; i++) {
            JobQueueItem job = allJobsTargettingThis[i];
            if (job.jobQueueParent.CancelJob(job, cause, shouldDoAfterEffect)) {
                i--;
            }
        }
    }
    public bool HasJobTargettingThisCharacter(string jobName) {
        for (int i = 0; i < allJobsTargettingThis.Count; i++) {
            JobQueueItem job = allJobsTargettingThis[i];
            if (job.name == jobName) {
                return true;
            }
        }
        return false;
    }
    public int GetNumOfJobsTargettingThisCharacter(string jobName) {
        int count = 0;
        for (int i = 0; i < allJobsTargettingThis.Count; i++) {
            JobQueueItem job = allJobsTargettingThis[i];
            if (job.name == jobName) {
                count++;
            }
        }
        return count;
    }
    public bool HasJobTargettingThisCharacter(string jobName, object conditionKey) {
        for (int i = 0; i < allJobsTargettingThis.Count; i++) {
            if (allJobsTargettingThis[i] is GoapPlanJob) {
                GoapPlanJob job = allJobsTargettingThis[i] as GoapPlanJob;
                if (job.name == jobName && job.targetEffect.conditionKey == conditionKey) {
                    return true;
                }
            }
        }
        return false;
    }
    public GoapPlanJob GetJobTargettingThisCharacter(string jobName, object conditionKey) {
        for (int i = 0; i < allJobsTargettingThis.Count; i++) {
            if (allJobsTargettingThis[i] is GoapPlanJob) {
                GoapPlanJob job = allJobsTargettingThis[i] as GoapPlanJob;
                if (job.name == jobName && job.targetEffect.conditionKey == conditionKey) {
                    return job;
                }
            }
        }
        return null;
    }
    public List<GoapPlanJob> GetJobsTargettingThisCharacter(string jobName, object conditionKey) {
        List<GoapPlanJob> jobs = new List<GoapPlanJob>();
        for (int i = 0; i < allJobsTargettingThis.Count; i++) {
            if (allJobsTargettingThis[i] is GoapPlanJob) {
                GoapPlanJob job = allJobsTargettingThis[i] as GoapPlanJob;
                if (job.name == jobName && job.targetEffect.conditionKey == conditionKey) {
                    jobs.Add(job);
                }
            }
        }
        return jobs;
    }
    private void CheckApprehendRelatedJobsOnLeaveLocation() {
        CancelAllJobsTargettingThisCharacter("Apprehend");

        //All apprehend jobs that are being done by this character must be unassigned
        for (int i = 0; i < allGoapPlans.Count; i++) {
            GoapPlan plan = allGoapPlans[i];
            if (plan.job != null && plan.job.name == "Apprehend") {
                plan.job.UnassignJob();
                i--;
            }
        }
    }
    private void CancelOrUnassignRemoveTraitRelatedJobs() {
        CancelAllJobsTargettingThisCharacter("Remove Trait");

        //All remove trait jobs that are being done by this character must be unassigned
        for (int i = 0; i < allGoapPlans.Count; i++) {
            GoapPlan plan = allGoapPlans[i];
            if (plan.job != null && plan.job.name == "Remove Trait") {
                plan.job.UnassignJob();
                i--;
            }
        }
    }
    public bool CreateJobsOnEnterVisionWith(Character targetCharacter) {
        bool hasCreatedJob = false;
        if (CreateRemoveTraitJobs(targetCharacter)) {
            hasCreatedJob = true;
        }
        if (CreateUndermineJob(targetCharacter)) {
            hasCreatedJob = true;
        }
        if (CreateBuryJob(targetCharacter)) {
            hasCreatedJob = true;
        }
        if (role.roleType == CHARACTER_ROLE.SOLDIER && isAtHomeArea && targetCharacter.isAtHomeArea && !targetCharacter.isDead) {
            if (GetRelationshipEffectWith(targetCharacter) != RELATIONSHIP_EFFECT.POSITIVE) {
                if (targetCharacter.HasTraitOf(TRAIT_TYPE.CRIMINAL)) {
                    if (CreateApprehendJobFor(targetCharacter) != null) {
                        hasCreatedJob = true;
                    }
                }
            }
        }
        GoapPlanJob restrainJob = CreateRestrainJob(targetCharacter);

        //GoapPlanJob assaultJob = CreateAssaultJob(targetCharacter);
        //if (assaultJob != null) {
        //    hasCreatedJob = true;
        //    if (overrideCurrentAction) {
        //        assaultJob.SetWillImmediatelyBeDoneAfterReceivingPlan(true);
        //        homeArea.jobQueue.AssignCharacterToJob(assaultJob, this);
        //    }
        //}
        return hasCreatedJob;
    }
    public bool CreateRemoveTraitJobs(Character targetCharacter) {
        if (targetCharacter.traitsNeededToBeRemoved.Count <= 0 || targetCharacter.isDead || !isAtHomeArea) {
            return false;
        }
        if (targetCharacter.GetTraitOf(TRAIT_TYPE.CRIMINAL) == null && CanCharacterTakeRemoveTraitJob(this, targetCharacter, null)) {
            bool hasCreatedJob = false;
            bool hasAssignedJob = false;
            for (int i = 0; i < targetCharacter.traitsNeededToBeRemoved.Count; i++) {
                Trait trait = targetCharacter.traitsNeededToBeRemoved[i];
                if(trait.name == "Restrained" && !targetCharacter.isAtHomeArea) {
                    GoapPlanJob job = CreateSaveCharacterJob(targetCharacter, true, false);
                    if (job != null && !hasAssignedJob) {
                        hasAssignedJob = jobQueue.ProcessFirstJobInQueue(this);
                    }
                } else {
                    if (!targetCharacter.HasJobTargettingThisCharacter("Remove Trait", trait.name)) {
                        if (trait.responsibleCharacter != null && trait.responsibleCharacter == this) {
                            continue;
                        }
                        if (trait.responsibleCharacters != null && trait.responsibleCharacters.Contains(this)) {
                            continue;
                        }
                        GoapEffect goapEffect = new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = trait.name, targetPOI = targetCharacter };
                        GoapPlanJob job = new GoapPlanJob("Remove Trait", goapEffect);
                        job.SetWillImmediatelyBeDoneAfterReceivingPlan(true);
                        job.SetCanTakeThisJobChecker(CanCharacterTakeRemoveTraitJob);
                        homeArea.jobQueue.AddJobInQueue(job, true);
                        //job.SetCancelOnFail(true);
                        if (!hasAssignedJob) {
                            hasAssignedJob = homeArea.jobQueue.AssignCharacterToJob(job, this);
                        }
                        hasCreatedJob = true;
                    }
                }
            }
            return hasCreatedJob;
        }
        return false;
    }
    private bool CanCharacterTakeRemoveTraitJob(Character character, Character targetCharacter, JobQueueItem job) {
        if (character != targetCharacter && character.faction.id == targetCharacter.faction.id) {
            if (job != null && job is GoapPlanJob) {
                GoapPlanJob planJob = job as GoapPlanJob;
                string traitName = planJob.targetEffect.conditionKey as string;
                Trait trait = targetCharacter.GetTrait(traitName);
                if (trait != null) {
                    if (trait.responsibleCharacter != null && trait.responsibleCharacter == this) {
                        return false;
                    }
                    if (trait.responsibleCharacters != null && trait.responsibleCharacters.Contains(this)) {
                        return false;
                    }
                }
            }
            if (character.faction.id == FactionManager.Instance.neutralFaction.id) {
                return character.race == targetCharacter.race && character.homeArea == targetCharacter.homeArea && !targetCharacter.HasRelationshipOfTypeWith(character, RELATIONSHIP_TRAIT.ENEMY);
            }
            return !character.HasRelationshipOfTypeWith(targetCharacter, RELATIONSHIP_TRAIT.ENEMY);
        }
        return false;
    }
    private bool CreateUndermineJob(Character targetCharacter) {
        if (!targetCharacter.isDead && HasRelationshipOfTypeWith(targetCharacter, RELATIONSHIP_TRAIT.ENEMY) && !jobQueue.HasJob("Undermine Enemy", targetCharacter)) {
            int chance = UnityEngine.Random.Range(0, 100);
            int value = 0;
            CHARACTER_MOOD currentMood = currentMoodType;
            if (currentMood == CHARACTER_MOOD.DARK) {
                value = 20;
            } else if (currentMood == CHARACTER_MOOD.BAD) {
                value = 10;
            }
            if (chance < value) {
                return CreateUndermineJobOnly(targetCharacter);
            }
        }
        return false;
    }
    /// <summary>
    /// Force this character to create an undermine job towards the target character.
    /// Only cases that this will not happen is:
    /// - if target character is already dead
    /// - this character already has an undermine job in his/her job queue
    /// </summary>
    /// <param name="targetCharacter">The character to undermine.</param>
    public void ForceCreateUndermineJob(Character targetCharacter) {
        if (!targetCharacter.isDead && !jobQueue.HasJob("Undermine Enemy", targetCharacter)) {
            CreateUndermineJobOnly(targetCharacter);
        }
    }
    public bool CreateUndermineJobOnly(Character targetCharacter) {
        WeightedDictionary<string> undermineWeights = new WeightedDictionary<string>();
        undermineWeights.AddElement("negative trait", 50);

        bool hasFriend = false;
        for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
            Character currCharacter = CharacterManager.Instance.allCharacters[i];
            if (currCharacter != targetCharacter && currCharacter != this) {
                if (currCharacter.HasRelationshipOfTypeWith(targetCharacter, RELATIONSHIP_TRAIT.FRIEND)) {
                    hasFriend = true;
                    break;
                }
            }
        }
        if (hasFriend) {
            undermineWeights.AddElement("destroy friendship", 20);
        }

        bool hasLoverOrParamour = false;
        for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
            Character currCharacter = CharacterManager.Instance.allCharacters[i];
            if (currCharacter != targetCharacter && currCharacter != this) {
                if (currCharacter.HasRelationshipOfTypeWith(targetCharacter, false, RELATIONSHIP_TRAIT.LOVER, RELATIONSHIP_TRAIT.PARAMOUR)) {
                    hasLoverOrParamour = true;
                    break;
                }
            }
        }
        if (hasLoverOrParamour) {
            undermineWeights.AddElement("destroy love", 20);
        }

        if (undermineWeights.Count > 0) {
            string result = undermineWeights.PickRandomElementGivenWeights();
            GoapPlanJob job = null;
            if (result == "negative trait") {
                job = new GoapPlanJob("Undermine Enemy", new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT_EFFECT, conditionKey = "Negative", targetPOI = targetCharacter });
            } else if (result == "destroy friendship") {
                job = new GoapPlanJob("Undermine Enemy", new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TARGET_REMOVE_RELATIONSHIP, conditionKey = "Friend", targetPOI = targetCharacter },
                    new Dictionary<INTERACTION_TYPE, object[]>() { { INTERACTION_TYPE.NONE, new object[] { targetCharacter } }, });
            } else if (result == "destroy love") {
                job = new GoapPlanJob("Undermine Enemy", new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TARGET_REMOVE_RELATIONSHIP, conditionKey = "Lover", targetPOI = targetCharacter },
                    new Dictionary<INTERACTION_TYPE, object[]>() { { INTERACTION_TYPE.NONE, new object[] { targetCharacter } }, });
            }

            job.SetCannotOverrideJob(true);
            Debug.LogWarning(GameManager.Instance.TodayLogString() + "Added an UNDERMINE ENEMY Job: " + result + " to " + this.name + " with target " + targetCharacter.name);
            job.SetWillImmediatelyBeDoneAfterReceivingPlan(true);
            jobQueue.AddJobInQueue(job, true, false);
            jobQueue.ProcessFirstJobInQueue(this);

            Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "saw_and_undermine");
            log.AddToFillers(this, name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            AddHistory(log);
            return true;
        }
        return false;
    }
    public void CreateAssaultJobs(Character targetCharacter, int amount) {
        if (isAtHomeArea && !targetCharacter.isDead && !targetCharacter.isAtHomeArea && !targetCharacter.HasTraitOf(TRAIT_TYPE.DISABLER, "Combat Recovery") && !this.HasTraitOf(TRAIT_TYPE.CRIMINAL)) {
            for (int i = 0; i < amount; i++) {
                GoapPlanJob job = new GoapPlanJob("Assault", new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT_EFFECT, conditionKey = "Negative", targetPOI = targetCharacter });
                job.SetCanTakeThisJobChecker(CanCharacterTakeAssaultJob);
                homeArea.jobQueue.AddJobInQueue(job);
            }
            //return job;
        }
        //return null;
    }
    private bool CanCharacterTakeAssaultJob(Character character, Character targetCharacter, JobQueueItem job) {
        return character.role.roleType == CHARACTER_ROLE.SOLDIER || character.role.roleType == CHARACTER_ROLE.ADVENTURER; // && !HasRelationshipOfEffectWith(targetCharacter, TRAIT_EFFECT.POSITIVE)
    }
    private bool CreateBuryJob(Character targetCharacter) {
        if (targetCharacter.isDead && targetCharacter.race != RACE.SKELETON && !HasTraitOf(TRAIT_TYPE.CRIMINAL) && this.isAtHomeArea) {
            //check first if the target character already has a bury job in this location
            GoapPlanJob buryJob = homeArea.jobQueue.GetJob("Bury", targetCharacter) as GoapPlanJob;
            if (buryJob == null) {
                //if none, create one
                buryJob = new GoapPlanJob("Bury", INTERACTION_TYPE.BURY_CHARACTER, targetCharacter);
                buryJob.AddForcedInteraction(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.IN_PARTY, targetPOI = targetCharacter }, INTERACTION_TYPE.CARRY_CORPSE);
                buryJob.SetCanTakeThisJobChecker(CanTakeBuryJob);
                buryJob.AllowDeadTargets();
                homeArea.jobQueue.AddJobInQueue(buryJob, true, false);
            }
            //if the character is a soldier or civilian, and the bury job is currently unassigned, take the job
            if (buryJob.assignedCharacter == null && (role.roleType == CHARACTER_ROLE.SOLDIER || role.roleType == CHARACTER_ROLE.CIVILIAN)) {
                buryJob.SetWillImmediatelyBeDoneAfterReceivingPlan(true);
                homeArea.jobQueue.AssignCharacterToJob(buryJob, this);
                //if (overrideCurrentAction) {
                //    buryJob.SetWillImmediatelyBeDoneAfterReceivingPlan(true);
                //    homeArea.jobQueue.AssignCharacterToJob(buryJob, this);
                //} else {
                //    homeArea.jobQueue.AssignCharacterToJob(buryJob, this);
                //}
            }
            return true;
        }
        return false;
    }
    private bool CanTakeBuryJob(Character character, JobQueueItem job) {
        return character.role.roleType == CHARACTER_ROLE.SOLDIER || character.role.roleType == CHARACTER_ROLE.CIVILIAN;
    }
    public GoapPlanJob CreateRestrainJob(Character targetCharacter) {
        if (isAtHomeArea && !targetCharacter.isDead && !targetCharacter.isAtHomeArea && !this.HasTraitOf(TRAIT_TYPE.CRIMINAL) && (role.roleType == CHARACTER_ROLE.SOLDIER || role.roleType == CHARACTER_ROLE.CIVILIAN || role.roleType == CHARACTER_ROLE.ADVENTURER)) {
            if (targetCharacter.GetTrait("Unconscious") != null && targetCharacter.GetTrait("Restrained") == null && GetRelationshipEffectWith(targetCharacter) != RELATIONSHIP_EFFECT.POSITIVE && !targetCharacter.HasJobTargettingThisCharacter("Restrain")) {
                GoapPlanJob job = new GoapPlanJob("Restrain", new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_FROM_PARTY, conditionKey = specificLocation, targetPOI = targetCharacter });
                job.AddForcedInteraction(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Restrained", targetPOI = targetCharacter }, INTERACTION_TYPE.RESTRAIN_CHARACTER);
                job.SetCanTakeThisJobChecker(CanCharacterTakeRestrainJob);
                job.SetWillImmediatelyBeDoneAfterReceivingPlan(true);
                homeArea.jobQueue.AddJobInQueue(job, true);
                homeArea.jobQueue.AssignCharacterToJob(job, this);
                return job;
            }
        }
        return null;
    }
    private bool CanCharacterTakeRestrainJob(Character character, Character targetCharacter, JobQueueItem job) {
        return (character.role.roleType == CHARACTER_ROLE.SOLDIER || character.role.roleType == CHARACTER_ROLE.CIVILIAN) && character.GetRelationshipEffectWith(targetCharacter) != RELATIONSHIP_EFFECT.POSITIVE; // || character.role.roleType == CHARACTER_ROLE.ADVENTURER
    }
    /// <summary>
    /// Make this character create an apprehend job at his home location targetting a specific character.
    /// </summary>
    /// <param name="targetCharacter">The character to be apprehended.</param>
    /// <returns>The created job.</returns>
    public GoapPlanJob CreateApprehendJobFor(Character targetCharacter, bool assignSelfToJob = false) {
        //if (homeArea.id == specificLocation.id) {
        if (!targetCharacter.HasJobTargettingThisCharacter("Apprehend") && targetCharacter.GetTrait("Restrained") == null && !this.HasTraitOf(TRAIT_TYPE.CRIMINAL)) {
            GoapEffect goapEffect = new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_FROM_PARTY, conditionKey = homeArea, targetPOI = targetCharacter };
            GoapPlanJob job = new GoapPlanJob("Apprehend", goapEffect);
            job.AddForcedInteraction(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Restrained", targetPOI = targetCharacter }, INTERACTION_TYPE.RESTRAIN_CHARACTER);
            job.SetCanTakeThisJobChecker(CanCharacterTakeApprehendJob);
            job.SetWillImmediatelyBeDoneAfterReceivingPlan(true);
            homeArea.jobQueue.AddJobInQueue(job, true);
            if(assignSelfToJob) {
                homeArea.jobQueue.AssignCharacterToJob(job, this);
            }
            return job;
        }
        return null;
        //}
    }
    //public GoapPlanJob CreateApprehendJobForThisCharacter(Character actor) {
    //    //if (homeArea.id == specificLocation.id) {
    //    if (!HasJobTargettingThisCharacter("Apprehend") && GetTrait("Restrained") == null && !this.HasTraitOf(TRAIT_TYPE.CRIMINAL)) {
    //        GoapEffect goapEffect = new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_FROM_PARTY, conditionKey = homeArea, targetPOI = this };
    //        GoapPlanJob job = new GoapPlanJob("Apprehend", goapEffect);
    //        job.AddForcedInteraction(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Restrained", targetPOI = this }, INTERACTION_TYPE.RESTRAIN_CHARACTER);
    //        job.SetCanTakeThisJobChecker(CanCharacterTakeApprehendJob);
    //        job.SetWillImmediatelyBeDoneAfterReceivingPlan(true);
    //        homeArea.jobQueue.AddJobInQueue(job, true);
    //        if (actor != null) {
    //            homeArea.jobQueue.AssignCharacterToJob(job, actor);
    //        }
    //        return job;
    //    }
    //    return null;
    //    //}
    //}
    private bool CanCharacterTakeApprehendJob(Character character, Character targetCharacter, JobQueueItem job) {
        return character.role.roleType == CHARACTER_ROLE.SOLDIER && character.GetRelationshipEffectWith(targetCharacter) != RELATIONSHIP_EFFECT.POSITIVE;
    }
    public void CreatePersonalJobs() {
        //Claim Item Job
        bool hasCreatedJob = false;
        //if (faction.id != FactionManager.Instance.neutralFaction.id && !jobQueue.HasJob("Claim Item")) {
        //    int numOfItemsOwned = GetNumOfItemsOwned();
        //    if (numOfItemsOwned < 3) {
        //        int chance = UnityEngine.Random.Range(0, 100);
        //        int value = 10 - (numOfItemsOwned * 3);
        //        if (chance < value) {
        //            List<SpecialToken> tokens = new List<SpecialToken>();
        //            for (int i = 0; i < specificLocation.possibleSpecialTokenSpawns.Count; i++) {
        //                SpecialToken currToken = specificLocation.possibleSpecialTokenSpawns[i];
        //                if (currToken.characterOwner == null) {
        //                    if (currToken.factionOwner != null && faction.id != currToken.factionOwner.id) {
        //                        continue;
        //                    }
        //                    if (currToken.gridTileLocation != null && currToken.gridTileLocation.structure != null
        //                        && currToken.gridTileLocation.structure.structureType == STRUCTURE_TYPE.WAREHOUSE && GetToken(currToken) == null && !currToken.HasJobTargettingThis("Claim Item")
        //                        && GetAwareness(currToken) != null) {
        //                        tokens.Add(currToken);
        //                    }
        //                }
        //            }
        //            if (tokens.Count > 0) {
        //                SpecialToken chosenToken = tokens[UnityEngine.Random.Range(0, tokens.Count)];
        //                GoapPlanJob job = new GoapPlanJob("Claim Item", INTERACTION_TYPE.PICK_ITEM, chosenToken);
        //                job.SetCancelOnFail(true);
        //                //GameManager.Instance.SetPausedState(true);
        //                Debug.LogWarning(GameManager.Instance.TodayLogString() + "Added a Claim Item Job to " + this.name + " with target " + chosenToken.name + " in " + chosenToken.gridTileLocation.ToString());
        //                jobQueue.AddJobInQueue(job);
        //                hasCreatedJob = true;
        //            }
        //        }
        //    }
        //}

        //Undermine Enemy Job
        if (!hasCreatedJob && HasRelationshipTraitOf(RELATIONSHIP_TRAIT.ENEMY)) {
            int chance = UnityEngine.Random.Range(0, 100);
            int value = 3;
            CHARACTER_MOOD currentMood = currentMoodType;
            if (currentMood == CHARACTER_MOOD.DARK) {
                value += 1;
            } else if (currentMood == CHARACTER_MOOD.GOOD) {
                value -= 1;
            } else if (currentMood == CHARACTER_MOOD.GREAT) {
                value -= 3;
            }
            if (chance < value) {
                List<Character> enemyCharacters = GetCharactersWithRelationship(RELATIONSHIP_TRAIT.ENEMY);
                Character chosenCharacter = null;
                while (chosenCharacter == null && enemyCharacters.Count > 0) {
                    int index = UnityEngine.Random.Range(0, enemyCharacters.Count);
                    Character character = enemyCharacters[index];
                    if (character.HasJobTargettingThisCharacter("Undermine Enemy") || jobQueue.HasJob("Undermine Enemy", character)) {
                        enemyCharacters.RemoveAt(index);
                    } else {
                        chosenCharacter = character;
                    }
                }
                if (chosenCharacter != null) {
                    GoapPlanJob job = new GoapPlanJob("Undermine Enemy", new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT_EFFECT, conditionKey = "Negative", targetPOI = chosenCharacter });
                    job.SetCancelOnFail(true);
                    job.SetCannotOverrideJob(true);
                    //GameManager.Instance.SetPausedState(true);
                    Debug.LogWarning(GameManager.Instance.TodayLogString() + "Added an UNDERMINE ENEMY Job to " + this.name + " with target " + chosenCharacter.name);
                    jobQueue.AddJobInQueue(job);
                    hasCreatedJob = true;
                }
            }
        }

        if (!hasCreatedJob && currentStructure is Dwelling) {
            Dwelling dwelling = currentStructure as Dwelling;
            if (dwelling.HasPositiveRelationshipWithAnyResident(this) && dwelling.HasUnoccupiedFurnitureSpot() && poiGoapActions.Contains(INTERACTION_TYPE.CRAFT_FURNITURE)) {
                //- if the character is in a Dwelling structure and he or someone he has a positive relationship with owns it, 
                //and the Dwelling still has an unoccupied Furniture Spot, 5% chance to add a Build Furniture Job.
                if (UnityEngine.Random.Range(0, 100) < 5) {
                    FACILITY_TYPE mostNeededFacility = dwelling.GetMostNeededValidFacility();
                    if (mostNeededFacility != FACILITY_TYPE.NONE) {
                        List<LocationGridTile> validSpots = dwelling.GetUnoccupiedFurnitureSpotsThatCanProvide(mostNeededFacility);
                        LocationGridTile chosenTile = validSpots[UnityEngine.Random.Range(0, validSpots.Count)];
                        FURNITURE_TYPE furnitureToCreate = chosenTile.GetFurnitureThatCanProvide(mostNeededFacility);
                        //check first if the character can build that specific type of furniture
                        if (furnitureToCreate.ConvertFurnitureToTileObject().CanBeCraftedBy(this)) {
                            GoapPlanJob job = new GoapPlanJob("Build Furniture", INTERACTION_TYPE.CRAFT_FURNITURE, this, new Dictionary<INTERACTION_TYPE, object[]>() {
                                { INTERACTION_TYPE.CRAFT_FURNITURE, new object[] { chosenTile, furnitureToCreate } }
                            });
                            job.SetCancelOnFail(true);
                            job.SetCannotOverrideJob(true);
                            jobQueue.AddJobInQueue(job);
                            Debug.Log(this.name + " created a new build furniture job targetting tile " + chosenTile.ToString() + " with furniture type " + furnitureToCreate.ToString());
                        }
                    }
                }
            }
        }
    }
    public Character troubledCharacter { get; private set; }
    public void CreateAskForHelpSaveCharacterJob(Character troubledCharacter) {
        if (troubledCharacter != null && troubledCharacter != this) {
            this.troubledCharacter = troubledCharacter;
            Character targetCharacter = null;
            List<Character> positiveCharacters = GetCharactersWithRelationship(TRAIT_EFFECT.POSITIVE);
            positiveCharacters.Remove(troubledCharacter);
            if (positiveCharacters.Count > 0) {
                targetCharacter = positiveCharacters[UnityEngine.Random.Range(0, positiveCharacters.Count)];
            } else {
                List<Character> nonEnemyCharacters = GetCharactersWithoutRelationship(RELATIONSHIP_TRAIT.ENEMY).Where(x => x.faction.id == faction.id).ToList();
                nonEnemyCharacters.Remove(troubledCharacter);
                if (nonEnemyCharacters.Count > 0) {
                    targetCharacter = nonEnemyCharacters[UnityEngine.Random.Range(0, nonEnemyCharacters.Count)];
                }
            }
            if (targetCharacter != null) {
                GoapPlanJob job = new GoapPlanJob("Ask For Help Save Character", INTERACTION_TYPE.ASK_FOR_HELP_SAVE_CHARACTER, targetCharacter);
                jobQueue.AddJobInQueue(job);
            } else {
                RegisterLogAndShowNotifToThisCharacterOnly("Generic", "ask_for_help_fail", troubledCharacter, troubledCharacter.name);
            }
        } else {
            if (troubledCharacter == null) {
                Debug.LogError(name + " cannot create ask for help save character job because troubled character is null!");
            } else {
                Debug.LogError(name + " cannot create ask for help save character job for " + troubledCharacter.name);
            }
        }
    }
    public void CreateAskForHelpJob(Character troubledCharacter, INTERACTION_TYPE helpType, params object[] otherData) {
        //&& troubledCharacter != this
        if (troubledCharacter != null) {
            this.troubledCharacter = troubledCharacter;
            Character targetCharacter = null;
            List<Character> positiveCharacters = GetCharactersWithRelationship(TRAIT_EFFECT.POSITIVE);
            positiveCharacters.Remove(troubledCharacter);
            if (positiveCharacters.Count > 0) {
                targetCharacter = positiveCharacters[UnityEngine.Random.Range(0, positiveCharacters.Count)];
            } else {
                List<Character> nonEnemyCharacters = GetCharactersWithoutRelationship(RELATIONSHIP_TRAIT.ENEMY).Where(x => x.faction.id == faction.id).ToList();
                nonEnemyCharacters.Remove(troubledCharacter);
                if (nonEnemyCharacters.Count > 0) {
                    targetCharacter = nonEnemyCharacters[UnityEngine.Random.Range(0, nonEnemyCharacters.Count)];
                }
            }
            if (targetCharacter != null) {
                GoapPlanJob job = new GoapPlanJob("Ask For Help " + helpType.ToString(), helpType, targetCharacter, new Dictionary<INTERACTION_TYPE, object[]>() {
                    { helpType, otherData }
                });
                jobQueue.AddJobInQueue(job);
            } else {
                RegisterLogAndShowNotifToThisCharacterOnly("Generic", "ask_for_help_fail", troubledCharacter, troubledCharacter.name);
            }
        } else {
            if (troubledCharacter == null) {
                Debug.LogError(name + " cannot create ask for help save character job because troubled character is null!");
            } else {
                Debug.LogError(name + " cannot create ask for help save character job for " + troubledCharacter.name);
            }
        }
    }
    public GoapPlanJob CreateSaveCharacterJob(Character targetCharacter, bool isPriority = false, bool processLogicForPersonalJob = true) {
        if (targetCharacter != null && targetCharacter != this) {
            if (!targetCharacter.HasJobTargettingThisCharacter("Save Character")) {
                GoapPlanJob job = new GoapPlanJob("Save Character", new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_FROM_PARTY, conditionKey = targetCharacter.homeArea, targetPOI = targetCharacter });
                jobQueue.AddJobInQueue(job, isPriority, processLogicForPersonalJob);
                return job;
            }
        } else {
            if (targetCharacter == null) {
                Debug.LogError(name + " cannot create save character job because troubled character is null!");
            } else {
                Debug.LogError(name + " cannot create save character job for " + targetCharacter.name);
            }
        }
        return null;
    }
    public GoapPlanJob CreateBreakupJob(Character targetCharacter) {
        GoapPlanJob job = new GoapPlanJob("Break Up with " + targetCharacter.name, INTERACTION_TYPE.BREAK_UP, targetCharacter);
        jobQueue.AddJobInQueue(job);
        return job;
    }
    public void CancelAllJobsAndPlans() {
        AdjustIsWaitingForInteraction(1);
        for (int i = 0; i < jobQueue.jobsInQueue.Count; i++) {
            if (jobQueue.CancelJob(jobQueue.jobsInQueue[i])) {
                i--;
            }
        }

        StopCurrentAction(false);
        for (int i = 0; i < allGoapPlans.Count; i++) {
            if (DropPlan(allGoapPlans[i])) {
                i--;
            }
        }
        AdjustIsWaitingForInteraction(-1);
    }
    #endregion

    #region Faction
    public void SetFaction(Faction newFaction) {
        if (_faction != null
            && newFaction != null
            && _faction.id == newFaction.id) {
            //ignore change, because character is already part of that faction
            return;
        }
        _faction = newFaction;
        currentAlterEgo.SetFaction(faction);
        OnChangeFaction();
        UpdateTokenOwner();
        if (_faction != null) {
            Messenger.Broadcast<Character>(Signals.FACTION_SET, this);
        }
    }
    public void ChangeFactionTo(Faction newFaction) {
        if (this.faction.id == newFaction.id) {
            return; //if the new faction is the same, ignore change
        }
        faction.RemoveCharacter(this);
        newFaction.AddNewCharacter(this);
    }
    private void OnChangeFaction() {
        //check if this character has a Criminal Trait, if so, remove it
        Trait criminal = GetTrait("Criminal");
        if (criminal != null) {
            RemoveTrait(criminal, false);
        }
    }
    public void FoundFaction(string factionName, Area location) {
        MigrateHomeTo(location);
        Faction newFaction = FactionManager.Instance.GetFactionBasedOnName(factionName);
        newFaction.SetLeader(this);
        ChangeFactionTo(newFaction);
        FactionManager.Instance.neutralFaction.RemoveFromOwnedAreas(location);
        LandmarkManager.Instance.OwnArea(newFaction, race, location);
        newFaction.SetFactionActiveState(true);
    }
    #endregion

    #region Party
    /*
        Create a new Party with this character as the leader.
            */
    public virtual Party CreateOwnParty() {
        if (_ownParty != null) {
            _ownParty.RemoveCharacter(this);
        }
        CharacterParty newParty = new CharacterParty(this);
        SetOwnedParty(newParty);
        newParty.AddCharacter(this, true);
        //newParty.CreateCharacterObject();
        return newParty;
    }
    public virtual void SetOwnedParty(Party party) {
        _ownParty = party as CharacterParty;
    }
    public virtual void SetCurrentParty(Party party) {
        _currentParty = party as CharacterParty;
    }
    public void OnRemovedFromParty() {
        SetCurrentParty(ownParty); //set the character's party to it's own party
        //if (ownParty is CharacterParty) {
        //    if ((ownParty as CharacterParty).actionData.currentAction != null) {
        //        (ownParty as CharacterParty).actionData.currentAction.EndAction(ownParty, (ownParty as CharacterParty).actionData.currentTargetObject);
        //    }
        //}
        if (this.minion != null) {
            this.minion.SetEnabledState(true); //reenable this minion, since it could've been disabled because it was part of another party
        }
    }
    public void OnAddedToParty() {
        if (currentParty.id != ownParty.id) {
            ownParty.specificLocation.RemoveCharacterFromLocation(this);
            //ownParty.icon.SetVisualState(false);
        }
    }
    public bool IsInParty() {
        if (currentParty.characters.Count > 1) {
            return true; //if the character is in a party that has more than 1 characters
        }
        return false;
    }
    public bool IsInPartyOf(Character character) {
        return currentParty == character.ownParty;
    }
    public bool IsInOwnParty() {
        if (currentParty.id == ownParty.id) {
            return true;
        }
        return false;
    }
    public bool HasOtherCharacterInParty() {
        return ownParty.characters.Count > 1;
    }
    #endregion

    #region Location
    public bool IsCharacterInAdjacentRegionOfThis(Character targetCharacter) {
        for (int i = 0; i < _currentRegion.adjacentRegionsViaRoad.Count; i++) {
            if (targetCharacter.party.currentRegion.id == _currentRegion.adjacentRegionsViaRoad[i].id) {
                return true;
            }
        }
        return false;
    }
    public void SetCurrentStructureLocation(LocationStructure currentStructure, bool broadcast = true) {
        if (currentStructure == this.currentStructure) {
            return; //ignore change;
        }
        LocationStructure previousStructure = this.currentStructure;
        this.currentStructure = currentStructure;
        //if (marker != null && currentStructure != null) {
        //    marker.RevalidatePOIsInVisionRange(); //when the character changes structures, revalidate pois in range
        //}
        string summary = string.Empty;
        if (currentStructure != null) {
            summary = GameManager.Instance.TodayLogString() + "Arrived at <color=\"green\">" + currentStructure.ToString() + "</color>";
        } else {
            summary = GameManager.Instance.TodayLogString() + "Left <color=\"red\">" + previousStructure.ToString() + "</color>";
        }
        locationHistory.Add(summary);
        if (locationHistory.Count > 80) {
            locationHistory.RemoveAt(0);
        }

        if (currentStructure != null && broadcast) {
            Messenger.Broadcast(Signals.CHARACTER_ARRIVED_AT_STRUCTURE, this, currentStructure);
        }
    }
    /// <summary>
    /// Move this character to another structure in the same area.
    /// </summary>
    /// <param name="newStructure">New structure the character is going to.</param>
    /// <param name="destinationTile">LocationGridTile where the character will go to (Must be inside the new structure).</param>
    /// <param name="targetPOI">The Point of Interest this character will interact with</param>
    /// <param name="arrivalAction">What should this character do when it reaches its target tile?</param>
    public void MoveToAnotherStructure(LocationStructure newStructure, LocationGridTile destinationTile, IPointOfInterest targetPOI = null, Action arrivalAction = null) {
        //if the character is already at the destination tile, just do the specified arrival action, if any.
        if (gridTileLocation == destinationTile) {
            if (arrivalAction != null) {
                arrivalAction();
            }
            //marker.PlayIdle();
        } else {
            if (destinationTile == null) {
                if (targetPOI != null) {
                    //if destination tile is null, make the charater marker use target poi logic (Usually used for moving targets)
                    marker.GoTo(targetPOI, arrivalAction);
                } else {
                    if (arrivalAction != null) {
                        arrivalAction();
                    }
                }
            } else {
                //if destination tile is not null, got there, regardless of target poi
                marker.GoTo(destinationTile, arrivalAction);
            }

        }
    }
    public void SetGridTileLocation(LocationGridTile tile) {
        this.tile = tile;
        string summary = string.Empty;
        if (tile == null) {
            summary = GameManager.Instance.TodayLogString() + "Set tile location to null";
        } else {
            summary = GameManager.Instance.TodayLogString() + "Set tile location to " + tile.localPlace.ToString();
        }
        locationHistory.Add(summary);
        if (locationHistory.Count > 80) {
            locationHistory.RemoveAt(0);
        }
    }
    public LocationGridTile GetNearestUnoccupiedTileFromThis() {
        if (!isDead && gridTileLocation != null) {
            List<LocationGridTile> unoccupiedNeighbours = gridTileLocation.UnoccupiedNeighbours;
            if (unoccupiedNeighbours.Count == 0) {
                return null;
            } else {
                return unoccupiedNeighbours[UnityEngine.Random.Range(0, unoccupiedNeighbours.Count)];
            }
        }
        return null;
    }
    public LocationGridTile GetNearestUnoccupiedEdgeTileFromThis() {
        LocationGridTile currentGridTile = gridTileLocation;
        if (currentGridTile.IsAtEdgeOfWalkableMap() && currentGridTile.structure != null) {
            return currentGridTile;
        }

        LocationGridTile nearestEdgeTile = null;
        List<LocationGridTile> neighbours = gridTileLocation.neighbourList;
        for (int i = 0; i < neighbours.Count; i++) {
            if (neighbours[i].IsAtEdgeOfWalkableMap() && neighbours[i].structure != null && !neighbours[i].isOccupied) {
                nearestEdgeTile = neighbours[i];
                break;
            }
        }
        if (nearestEdgeTile == null) {
            float nearestDist = -999f;
            for (int i = 0; i < specificLocation.areaMap.allEdgeTiles.Count; i++) {
                LocationGridTile currTile = specificLocation.areaMap.allEdgeTiles[i];
                float dist = Vector2.Distance(currTile.localLocation, currentGridTile.localLocation);
                if (nearestDist == -999f || dist < nearestDist) {
                    if(currTile.structure != null) {
                        nearestEdgeTile = currTile;
                        nearestDist = dist;
                    }
                }
            }
        }
        return nearestEdgeTile;
    }
    private void OnLeaveArea(Party party) {
        if (currentParty == party) {
            CheckApprehendRelatedJobsOnLeaveLocation();
            CancelOrUnassignRemoveTraitRelatedJobs();
            marker.ClearTerrifyingCharacters();
            ExecuteLeaveAreaActions();
        } else {
            if (marker.terrifyingCharacters.Count > 0) {
                for (int i = 0; i < party.characters.Count; i++) {
                    marker.RemoveTerrifyingCharacter(party.characters[i]);
                }
            }
        }
    }
    private void OnArrivedAtArea(Party party) {
        //if (currentParty == party) {
        //    if (isAtHomeArea) {
        //        if (HasTraitOf(TRAIT_TYPE.CRIMINAL)) {
        //            CreateApprehendJob();
        //        }
        //        //for (int i = 0; i < traits.Count; i++) {
        //        //    if (traits[i].name == "Cursed" || traits[i].name == "Sick"
        //        //        || traits[i].name == "Injured" || traits[i].name == "Unconscious") {
        //        //        CreateRemoveTraitJob(traits[i].name);
        //        //    }
        //        //}
        //    }
        //}
    }
    public void OnArriveAtAreaStopMovement() {
        currentParty.icon.SetTarget(null, null, null, null);
        currentParty.icon.SetOnPathFinished(null);
    }
    public void AddOnLeaveAreaAction(System.Action onLeaveAreaAction) {
        onLeaveAreaActions.Add(onLeaveAreaAction);
    }
    private void ExecuteLeaveAreaActions() {
        for (int i = 0; i < onLeaveAreaActions.Count; i++) {
            onLeaveAreaActions[i].Invoke();
        }
        onLeaveAreaActions.Clear();
    }
    #endregion

    #region Utilities
    public void ChangeGender(GENDER gender) {
        _gender = gender;
        Messenger.Broadcast(Signals.GENDER_CHANGED, this, gender);
    }
    public void ChangeRace(RACE race) {
        if (_raceSetting != null) {
            if (_raceSetting.race == race) {
                return; //current race is already the new race, no change
            }
            RemoveTraitsFromRace();
        }
        RaceSetting raceSetting = RaceManager.Instance.racesDictionary[race.ToString()];
        _raceSetting = raceSetting.CreateNewCopy();
        OnUpdateRace();
        Messenger.Broadcast(Signals.CHARACTER_CHANGED_RACE, this);
    }
    private void OnUpdateRace() {
        SetTraitsFromRace();
        //Update Portrait to use new race
        _portraitSettings = CharacterManager.Instance.GenerateRandomPortrait(race, gender);
        if (marker != null) {
            marker.UpdateMarkerVisuals();
        }
    }
    public void RemoveRace() {
        if (_raceSetting == null) {
            return;
        }
        RemoveTraitsFromRace();
        _raceSetting = null;
    }
    public void SetRace(RACE race) {
        RaceSetting raceSetting = RaceManager.Instance.racesDictionary[race.ToString()];
        _raceSetting = raceSetting.CreateNewCopy();
        OnUpdateRace();
    }
    public void ChangeClass(string className) {
        string previousClassName = _characterClass.className;
        AssignClass(className);
        //_characterClass = charClass.CreateNewCopy();
        OnCharacterClassChange();

#if !WORLD_CREATION_TOOL
        //_homeLandmark.tileLocation.areaOfTile.excessClasses.Remove(previousClassName);
        //_homeLandmark.tileLocation.areaOfTile.missingClasses.Remove(_characterClass.className);

        //Log log = new Log(GameManager.Instance.Today(), "CharacterActions", "ChangeClassAction", "change_class");
        //log.AddToFillers(this, this.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        //log.AddToFillers(null, previousClassName, LOG_IDENTIFIER.STRING_1);
        //log.AddToFillers(null, _characterClass.className, LOG_IDENTIFIER.STRING_2);
        //AddHistory(log);
        //check equipped items
#endif

    }
    public void SetName(string newName) {
        _name = newName;
        _firstName = _name.Split(' ')[0];
    }
    //If true, character can't do daily action (onDailyAction), i.e. actions, needs
    //public void SetIsIdle(bool state) {
    //    _isIdle = state;
    //}
    //public bool HasPathToParty(Party partyToJoin) {
    //    return PathGenerator.Instance.GetPath(currLocation, partyToJoin.currLocation, PATHFINDING_MODE.PASSABLE, _faction) != null;
    //}
    public void CenterOnCharacter(bool clickTravelLine = true) {
        if (!isDead && minion == null) {
            if (currentParty.icon.isTravelling) {
                if (currentParty.icon.travelLine != null) {
                    if (specificLocation.areaMap.isShowing) {
                        InteriorMapManager.Instance.HideAreaMap();
                        if (clickTravelLine) {
                            currentParty.icon.travelLine.OnClickTravelLine();
                        }
                    }
                    CameraMove.Instance.CenterCameraOn(currentParty.icon.travelLine.iconImg.gameObject);
                } else {
                    bool instantCenter = InteriorMapManager.Instance.currentlyShowingArea != specificLocation;
                    if (!specificLocation.areaMap.isShowing) {
                        InteriorMapManager.Instance.ShowAreaMap(specificLocation, false);
                    }
                    AreaMapCameraMove.Instance.CenterCameraOn(marker.gameObject, instantCenter);
                }
            } else {
                bool instantCenter = InteriorMapManager.Instance.currentlyShowingArea != specificLocation;
                if (!specificLocation.areaMap.isShowing) {
                    InteriorMapManager.Instance.ShowAreaMap(specificLocation, false);
                }
                AreaMapCameraMove.Instance.CenterCameraOn(marker.gameObject, instantCenter);
            }
        }
    }
    private void GetRandomCharacterColor() {
        _characterColor = CombatManager.Instance.UseRandomCharacterColor();
        _characterColorCode = ColorUtility.ToHtmlStringRGBA(_characterColor).Substring(0, 6);
    }
    private void OnOtherCharacterDied(Character characterThatDied) {
        if (characterThatDied.id != this.id) {
            //RemoveRelationship(characterThatDied); //do not remove relationships when dying
            marker.OnOtherCharacterDied(characterThatDied);
        }
    }
    public void SetMode(MODE mode) {
        _currentMode = mode;
    }
    public void AdjustDoNotDisturb(int amount) {
        _doNotDisturb += amount;
        _doNotDisturb = Math.Max(_doNotDisturb, 0);
    }
    public void AdjustDoNotGetHungry(int amount) {
        _doNotGetHungry += amount;
        _doNotGetHungry = Math.Max(_doNotGetHungry, 0);
    }
    public void AdjustDoNotGetLonely(int amount) {
        _doNotGetLonely += amount;
        _doNotGetLonely = Math.Max(_doNotGetLonely, 0);
    }
    public void AdjustDoNotGetTired(int amount) {
        _doNotGetTired += amount;
        _doNotGetTired = Math.Max(_doNotGetTired, 0);
    }
    public void ReturnToOriginalHomeAndFaction(Area ogHome, Faction ogFaction) {
        //first, check if the character's original faction is still alive
        if (!ogFaction.isDestroyed) { //if it is, 
            this.ChangeFactionTo(ogFaction);  //transfer the character to his original faction
            if (ogFaction.id == FactionManager.Instance.neutralFaction.id) { //if the character's original faction is the neutral faction
                if (ogHome.owner == null && !ogHome.IsResidentsFull()) { //check if his original home is still unowned
                    //if it is and it has not reached it's resident capacity, return him to his original home
                    MigrateHomeTo(ogHome);
                } else { //if it does not meet those requirements
                    //check if the neutral faction still has any available areas that have not reached capacity yet
                    List<Area> validNeutralAreas = FactionManager.Instance.neutralFaction.ownedAreas.Where(x => !x.IsResidentsFull()).ToList();
                    if (validNeutralAreas.Count > 0) {
                        //if it does, pick randomly from those
                        Area chosenArea = validNeutralAreas[UnityEngine.Random.Range(0, validNeutralAreas.Count)];
                        MigrateHomeTo(chosenArea);
                    }
                    //if not, keep the characters current home
                }
            } else { //if it is not, check if his original home is still owned by that faction and it has not yet reached it's resident capacity
                if (ogHome.owner == ogFaction && !ogHome.IsResidentsFull()) {
                    //if it meets those requirements, return the character's home to that location
                    MigrateHomeTo(ogHome);
                } else { //if not, get another area owned by his faction that has not yet reached capacity
                    List<Area> validAreas = ogFaction.ownedAreas.Where(x => !x.IsResidentsFull()).ToList();
                    if (validAreas.Count > 0) {
                        Area chosenArea = validAreas[UnityEngine.Random.Range(0, validAreas.Count)];
                        MigrateHomeTo(chosenArea);
                    }
                    //if there are still no areas that can be his home, keep his current one.
                }
            }
        } else { //if not
            //transfer the character to the neutral faction instead
            this.ChangeFactionTo(FactionManager.Instance.neutralFaction);
            List<Area> validNeutralAreas = FactionManager.Instance.neutralFaction.ownedAreas.Where(x => !x.IsResidentsFull()).ToList();
            if (validNeutralAreas.Count > 0) {  //then check if the neutral faction has any owned areas that have not yet reached area capacity
                //if it does, pick from any of those, then set it as the characters new home
                Area chosenArea = validNeutralAreas[UnityEngine.Random.Range(0, validNeutralAreas.Count)];
                MigrateHomeTo(chosenArea);
            }
            //if it does not, keep the characters current home
        }
    }
    public override string ToString() {
        return name;
    }
    public void SetTracked(bool state) {
        _isTracked = state;
        if (_isTracked) {
            Messenger.Broadcast(Signals.CHARACTER_TRACKED, this);
        }
    }
    public void AdjustIsWaitingForInteraction(int amount) {
        isWaitingForInteraction += amount;
        if (isWaitingForInteraction < 0) {
            isWaitingForInteraction = 0;
        }
    }
    public LocationGridTile GetLocationGridTileByXY(int x, int y, bool throwOnException = true) {
        try {
            if (throwOnException) {
                return specificLocation.areaMap.map[x, y];
            } else {
                if (Utilities.IsInRange(x, 0, specificLocation.areaMap.map.GetUpperBound(0) + 1) &&
                    Utilities.IsInRange(y, 0, specificLocation.areaMap.map.GetUpperBound(1) + 1)) {
                    return specificLocation.areaMap.map[x, y];
                }
                return null;
            }
        } catch (Exception e) {
            throw new Exception(e.Message + "\n " + this.name + "(" + x.ToString() + ", " + y.ToString() + ")");
        }

    }
    public bool IsDoingCombatActionTowards(Character otherCharacter) {
        //if (marker.currentlyEngaging == otherCharacter) {
        //    return true;
        //}
        if (currentAction != null) {
            return currentAction.goapType == INTERACTION_TYPE.ASSAULT_ACTION_NPC && currentAction.poiTarget == otherCharacter;
        }
        return false;
    }
    public bool IsNear(IPointOfInterest poi) {
        if (poi is Character) {
            if (ownParty.characters.Contains(poi as Character)) {
                return true;
            }
        }
        return gridTileLocation == poi.gridTileLocation || gridTileLocation.IsAdjacentTo(poi);
    }
    public void UpdateIsCombatantState() {
        bool state = false;
        if (_role.roleType == CHARACTER_ROLE.CIVILIAN || _role.roleType == CHARACTER_ROLE.LEADER || _role.roleType == CHARACTER_ROLE.NOBLE) {
            state = true;
        } else if (GetTrait("Injured") != null) {
            state = true;
        }
        if (isCombatant != state) {
            isCombatant = state;
            if (isCombatant && marker != null) {
                marker.ClearTerrifyingCharacters();
            }
        }
    }
    #endregion

    #region Relationships
    public void AddRelationship(Character character, RelationshipTrait newRel) {
        AddRelationship(character.currentAlterEgo, newRel);
    }
    public void AddRelationship(AlterEgoData alterEgo, RelationshipTrait newRel) {
        currentAlterEgo.AddRelationship(alterEgo, newRel);

        //if (!relationships.ContainsKey(alterEgo)) {
        //    relationships.Add(alterEgo, new CharacterRelationshipData(this, alterEgo.owner, alterEgo));
        //}
        //relationships[alterEgo].AddRelationship(newRel);
        //OnRelationshipWithCharacterAdded(alterEgo.owner, newRel);
        //Messenger.Broadcast(Signals.RELATIONSHIP_ADDED, this, newRel);
    }
    //public void RemoveRelationship(Character character) {
    //    if (relationships.ContainsKey(character.currentAlterEgo)) {
    //        List<Trait> traits = relationships[character.currentAlterEgo].rels.Select(x => x as Trait).ToList();
    //        relationships[character.currentAlterEgo].RemoveListeners();
    //        relationships.Remove(character.currentAlterEgo);
    //        RemoveTrait(traits);
    //    }
    //}
    //public bool RemoveRelationship(Character character, RelationshipTrait rel) {
    //    if (relationships.ContainsKey(character.currentAlterEgo)) {
    //        return relationships[character.currentAlterEgo].RemoveRelationship(rel);
    //        //not removing relationship data now when rel count is 0. Because relationship data can contain other data that is still valid even without relationships
    //        //if (relationships[character].rels.Count == 0) {
    //        //    RemoveRelationship(character);
    //        //}
    //    }
    //    return false;
    //}
    public void RemoveRelationshipWith(Character character, RELATIONSHIP_TRAIT rel) {
        //if (relationships.ContainsKey(character.currentAlterEgo)) {
        //    relationships[character.currentAlterEgo].RemoveRelationship(rel);
        //}
        RemoveRelationshipWith(character.currentAlterEgo, rel);
    }
    public void RemoveRelationshipWith(AlterEgoData alterEgo, RELATIONSHIP_TRAIT rel) {
        //if (relationships.ContainsKey(alterEgo)) {
        //    relationships[alterEgo].RemoveRelationship(rel);
        //}
        currentAlterEgo.RemoveRelationship(alterEgo, rel);
    }
    //public void RemoveAllRelationships(bool triggerOnRemove = true) {
    //    List<Character> targetCharacters = relationships.Keys.Select(x => x.owner).ToList();
    //    while (targetCharacters.Count > 0) {
    //        CharacterManager.Instance.RemoveRelationshipBetween(this, targetCharacters[0], triggerOnRemove);
    //        targetCharacters.RemoveAt(0);
    //    }
    //}
    public RelationshipTrait GetRelationshipTraitWith(Character character, RELATIONSHIP_TRAIT type, bool useDisabled = false) {
        return GetRelationshipTraitWith(character.currentAlterEgo, type, useDisabled);
    }
    public RelationshipTrait GetRelationshipTraitWith(AlterEgoData alterEgo, RELATIONSHIP_TRAIT type, bool useDisabled = false) {
        if (HasRelationshipWith(alterEgo, useDisabled)) {
            return relationships[alterEgo].GetRelationshipTrait(type);
        }
        return null;
    }
    public List<RelationshipTrait> GetAllRelationshipTraitWith(Character character) {
        return GetAllRelationshipTraitWith(character.currentAlterEgo);
    }
    public List<RelationshipTrait> GetAllRelationshipTraitWith(AlterEgoData alterEgo) {
        if (HasRelationshipWith(alterEgo)) {
            return relationships[alterEgo].GetAllRelationshipTraits();
        }
        return null;
    }
    public List<RelationshipTrait> GetAllRelationshipOfEffectWith(Character character, TRAIT_EFFECT effect) {
        return GetAllRelationshipOfEffectWith(character.currentAlterEgo, effect);
    }
    public List<RelationshipTrait> GetAllRelationshipOfEffectWith(AlterEgoData alterEgo, TRAIT_EFFECT effect) {
        List<RelationshipTrait> rels = new List<RelationshipTrait>();
        if (HasRelationshipWith(alterEgo)) {
            for (int i = 0; i < relationships[alterEgo].rels.Count; i++) {
                RelationshipTrait currTrait = relationships[alterEgo].rels[i];
                if (currTrait.effect == effect && !currTrait.isDisabled) {
                    rels.Add(currTrait);
                }
            }
        }
        return rels;
    }
    public List<RELATIONSHIP_TRAIT> GetAllRelationshipTraitTypesWith(Character character) {
        return GetAllRelationshipTraitTypesWith(character.currentAlterEgo);
    }
    public List<RELATIONSHIP_TRAIT> GetAllRelationshipTraitTypesWith(AlterEgoData alterEgo) {
        if (HasRelationshipWith(alterEgo)) {
            return new List<RELATIONSHIP_TRAIT>(relationships[alterEgo].rels.Where(x => !x.isDisabled).Select(x => x.relType));
        }
        return null;
    }
    public List<Character> GetCharactersWithRelationship(RELATIONSHIP_TRAIT type) {
        List<Character> characters = new List<Character>();
        foreach (KeyValuePair<AlterEgoData, CharacterRelationshipData> kvp in relationships) {
            if (!kvp.Value.isDisabled && kvp.Value.HasRelationshipTrait(type)) {
                if (characters.Contains(kvp.Key.owner)) {
                    continue;
                }
                characters.Add(kvp.Key.owner);
            }
        }
        return characters;
    }
    public List<Character> GetCharactersWithoutRelationship(RELATIONSHIP_TRAIT type) {
        List<Character> characters = new List<Character>();
        foreach (KeyValuePair<AlterEgoData, CharacterRelationshipData> kvp in relationships) {
            if (!kvp.Value.isDisabled && !kvp.Value.HasRelationshipTrait(type)) {
                if (characters.Contains(kvp.Key.owner)) {
                    continue;
                }
                characters.Add(kvp.Key.owner);
            }
        }
        return characters;
    }
    public List<Character> GetCharactersWithRelationship(TRAIT_EFFECT effect) {
        List<Character> characters = new List<Character>();
        foreach (KeyValuePair<AlterEgoData, CharacterRelationshipData> kvp in relationships) {
            if (!kvp.Value.isDisabled && kvp.Value.HasRelationshipOfEffect(effect)) {
                if (characters.Contains(kvp.Key.owner)) {
                    continue;
                }
                characters.Add(kvp.Key.owner);
            }
        }
        return characters;
    }
    public Character GetCharacterWithRelationship(RELATIONSHIP_TRAIT type) {
        foreach (KeyValuePair<AlterEgoData, CharacterRelationshipData> kvp in relationships) {
            if (!kvp.Value.isDisabled && kvp.Value.HasRelationshipTrait(type)) {
                return kvp.Key.owner;
            }
        }
        return null;
    }
    public List<Character> GetAllCharactersThatHasRelationship() {
        List<Character> characters = new List<Character>();
        foreach (KeyValuePair<AlterEgoData, CharacterRelationshipData> kvp in relationships) {
            if (!kvp.Value.isDisabled) {
                if (characters.Contains(kvp.Key.owner)) {
                    continue;
                }
                characters.Add(kvp.Key.owner);
            }
        }
        return characters;
    }
    public bool CanHaveRelationshipWith(RELATIONSHIP_TRAIT type, Character target) {
        //NOTE: This is only one way checking. This character will only check itself, if he/she meets the requirements of a given relationship
        List<RELATIONSHIP_TRAIT> relationshipsWithTarget = GetAllRelationshipTraitTypesWith(target);
        //if(relationshipsWithTarget == null) { return true; }
        switch (type) {
            case RELATIONSHIP_TRAIT.ENEMY:
                return relationshipsWithTarget == null || (relationshipsWithTarget != null && !relationshipsWithTarget.Contains(RELATIONSHIP_TRAIT.ENEMY) && !relationshipsWithTarget.Contains(RELATIONSHIP_TRAIT.FRIEND)); //check that the target character is not already this characters enemy and that this character is also not his friend
            case RELATIONSHIP_TRAIT.FRIEND:
                return relationshipsWithTarget == null || (relationshipsWithTarget != null && !relationshipsWithTarget.Contains(RELATIONSHIP_TRAIT.FRIEND) && !relationshipsWithTarget.Contains(RELATIONSHIP_TRAIT.ENEMY)); //check that the target character is not already this characters friend and that this character is also not his enemy
            case RELATIONSHIP_TRAIT.LOVER:
                //- **Lover:** Positive, Permanent (Can only have 1)
                //check if this character already has a lover and that the target character is not his/her paramour
                if (GetCharacterWithRelationship(type) != null) {
                    return false;
                }
                if (relationshipsWithTarget != null && relationshipsWithTarget.Contains(RELATIONSHIP_TRAIT.PARAMOUR)) {
                    return false;
                }
                return true;

                //if (GetCharacterWithRelationship(type) == null
                //    && !relationshipsWithTarget.Contains(RELATIONSHIP_TRAIT.PARAMOUR)) {
                //    return true;
                //}
                //return false;
            case RELATIONSHIP_TRAIT.PARAMOUR:
                //- **Paramour:** Positive, Transient (Can only have 1)
                //check if this character already has a paramour and that the target character is not his/her lover
                //Comment Reason: Allowed multiple paramours
                //if (GetCharacterWithRelationship(type) != null) {
                //    return false;
                //}
                if (relationshipsWithTarget != null && relationshipsWithTarget.Contains(RELATIONSHIP_TRAIT.LOVER)) {
                    return false;
                }
                //one of the characters must have a lover
                if (!target.HasRelationshipTraitOf(RELATIONSHIP_TRAIT.LOVER) && !HasRelationshipTraitOf(RELATIONSHIP_TRAIT.LOVER)) {
                    return false;
                }

                return true;
            //if (GetCharacterWithRelationship(type) == null 
            //    && !relationshipsWithTarget.Contains(RELATIONSHIP_TRAIT.LOVER)) { 
            //    return true;
            //}
            //return false;
            case RELATIONSHIP_TRAIT.MASTER:
                //this means that the target character will be this characters master, therefore making this character his/her servant
                //so check if this character isn't already serving a master, or that this character is not a master himself
                if (GetCharacterWithRelationship(RELATIONSHIP_TRAIT.MASTER) == null
                    && GetCharacterWithRelationship(RELATIONSHIP_TRAIT.SERVANT) == null) {
                    return true;
                }
                return false;
            case RELATIONSHIP_TRAIT.SERVANT:
                //this means that the target character will be this characters servant, therefore making this character his/her master
                //so check that this character is not a servant
                if (GetCharacterWithRelationship(RELATIONSHIP_TRAIT.MASTER) == null) {
                    return true;
                }
                return false;
        }
        return true;
    }
    internal void OnRelationshipWithCharacterAdded(Character targetCharacter, RelationshipTrait newRel) {
        //check if they share the same home, then migrate them accordingly
        if (newRel.relType == RELATIONSHIP_TRAIT.LOVER
            && this.homeArea.id == targetCharacter.homeArea.id
            && this.homeStructure != targetCharacter.homeStructure) {
            //Lover conquers all, even if one character is factionless they will be together, meaning the factionless character will still have home structure
            homeArea.AssignCharacterToDwellingInArea(this, targetCharacter.homeStructure);
            //homeArea.AssignCharacterToDwellingInArea(targetCharacter);
        }
    }
    /// <summary>
    /// Does this character have a relationship of effect with the provided character?
    /// </summary>
    /// <param name="character">Other character.</param>
    /// <param name="effect">Relationship effect (Positive, Negative, Neutral)</param>
    /// <param name="include">Relationship type to exclude from checking</param>
    /// <returns></returns>
    public bool HasRelationshipOfEffectWith(Character character, TRAIT_EFFECT effect, RELATIONSHIP_TRAIT include = RELATIONSHIP_TRAIT.NONE) {
        return HasRelationshipOfEffectWith(character.currentAlterEgo, effect, include);
    }
    public bool HasRelationshipOfEffectWith(AlterEgoData alterEgo, TRAIT_EFFECT effect, RELATIONSHIP_TRAIT include = RELATIONSHIP_TRAIT.NONE) {
        if (HasRelationshipWith(alterEgo)) {
            for (int i = 0; i < relationships[alterEgo].rels.Count; i++) {
                RelationshipTrait currTrait = relationships[alterEgo].rels[i];
                if ((currTrait.effect == effect || currTrait.relType == include) && !currTrait.isDisabled) {
                    return true;
                }
            }
        }
        return false;
    }
    /// <summary>
    /// Does this character have any relationship type with a target character among the given relationship types.
    /// </summary>
    /// <param name="character">The target character.</param>
    /// <param name="useDisabled">Use disabled relationships?</param>
    /// <param name="objs">The list of relationships.</param>
    /// <returns>true or false.</returns>
    public bool HasAnyRelationshipOfTypeWith(Character character, bool useDisabled = false, params RELATIONSHIP_TRAIT[] objs) {
        return HasAnyRelationshipOfTypeWith(character.currentAlterEgo, useDisabled, objs);
    }
    public bool HasAnyRelationshipOfTypeWith(AlterEgoData alterEgo, bool useDisabled = false, params RELATIONSHIP_TRAIT[] objs) {
        if (HasRelationshipWith(alterEgo, useDisabled)) {
            List<RELATIONSHIP_TRAIT> rels = objs.ToList();
            for (int i = 0; i < relationships[alterEgo].rels.Count; i++) {
                RelationshipTrait currTrait = relationships[alterEgo].rels[i];
                if (rels.Contains(currTrait.relType)) {
                    if (!currTrait.isDisabled || (currTrait.isDisabled && useDisabled)) {
                        return true;
                    }
                }
            }
        }
        return false;
    }
    public bool HasRelationshipOfTypeWith(Character character, RELATIONSHIP_TRAIT relType, bool useDisabled = false) {
        return HasRelationshipOfTypeWith(character.currentAlterEgo, relType, useDisabled);
    }
    public bool HasRelationshipOfTypeWith(AlterEgoData alterEgo, RELATIONSHIP_TRAIT relType, bool useDisabled = false) {
        if (HasRelationshipWith(alterEgo, useDisabled)) {
            for (int i = 0; i < relationships[alterEgo].rels.Count; i++) {
                RelationshipTrait currTrait = relationships[alterEgo].rels[i];
                if (currTrait.relType == relType && !currTrait.isDisabled) {
                    return true;
                }
            }
        }
        return false;
    }
    public bool HasRelationshipOfTypeWith(Character character, bool useDisabled = false, params RELATIONSHIP_TRAIT[] rels) {
        return HasRelationshipOfTypeWith(character.currentAlterEgo, useDisabled, rels);
    }
    public bool HasRelationshipOfTypeWith(AlterEgoData alterEgo, bool useDisabled = false, params RELATIONSHIP_TRAIT[] rels) {
        if (HasRelationshipWith(alterEgo)) {
            for (int i = 0; i < relationships[alterEgo].rels.Count; i++) {
                RelationshipTrait currTrait = relationships[alterEgo].rels[i];
                if (!useDisabled && currTrait.isDisabled) {
                    continue; //skip
                }
                for (int j = 0; j < rels.Length; j++) {
                    if (rels[j] == currTrait.relType) {
                        return true;
                    }
                }
            }
        }
        return false;
    }
    public CharacterRelationshipData GetCharacterRelationshipData(Character character) {
        return GetCharacterRelationshipData(character.currentAlterEgo);
    }
    public CharacterRelationshipData GetCharacterRelationshipData(AlterEgoData alterEgo) {
        if (HasRelationshipWith(alterEgo)) {
            return relationships[alterEgo];
        }
        return null;
    }
    public bool HasRelationshipWith(Character character, bool useDisabled = false) {
        return HasRelationshipWith(character.currentAlterEgo);
    }
    public bool HasRelationshipWith(AlterEgoData alterEgo, bool useDisabled = false) {
        return currentAlterEgo.HasRelationshipWith(alterEgo, useDisabled);
    }
    public int GetAllRelationshipCountExcept(List<RELATIONSHIP_TRAIT> except = null) {
        int count = 0;
        for (int i = 0; i < allTraits.Count; i++) {
            if (allTraits[i] is RelationshipTrait) {
                RelationshipTrait relTrait = allTraits[i] as RelationshipTrait;
                if (except != null) {
                    if (except.Contains(relTrait.relType)) {
                        continue; //skip
                    }
                }
                count++;
            }
        }
        return count;
    }
    public void FlirtWith(Character otherCharacter) {
        if (!relationships.ContainsKey(otherCharacter.currentAlterEgo)) {
            relationships.Add(otherCharacter.currentAlterEgo, new CharacterRelationshipData(this, otherCharacter, otherCharacter.currentAlterEgo));
        }
        relationships[otherCharacter.currentAlterEgo].IncreaseFlirtationCount();
    }
    /// <summary>
    /// Get the type of relationship that this character has with the other character.
    /// </summary>
    /// <param name="otherCharacter">Character to check</param>
    /// <param name="useDisabled">Should this checking use disabled relationships?</param>
    /// <returns>POSITIVE, NEGATIVE, NONE</returns>
    public RELATIONSHIP_EFFECT GetRelationshipEffectWith(Character otherCharacter, bool useDisabled = false) {
        return GetRelationshipEffectWith(otherCharacter.currentAlterEgo, useDisabled);
    }
    public RELATIONSHIP_EFFECT GetRelationshipEffectWith(AlterEgoData alterEgo, bool useDisabled = false) {
        if (HasRelationshipWith(alterEgo, useDisabled)) {
            for (int i = 0; i < relationships[alterEgo].rels.Count; i++) {
                RelationshipTrait currTrait = relationships[alterEgo].rels[i];
                if (currTrait.effect == TRAIT_EFFECT.NEGATIVE) {
                    return RELATIONSHIP_EFFECT.NEGATIVE;
                }
            }
            return RELATIONSHIP_EFFECT.POSITIVE;
        }
        return RELATIONSHIP_EFFECT.NONE;
    }
    #endregion

    #region History/Logs
    public void AddHistory(Log log) {
        if (!_history.Contains(log)) {
            _history.Add(log);
            //if (UIManager.Instance.characterInfoUI.currentlyShowingCharacter != null && this.id == UIManager.Instance.characterInfoUI.currentlyShowingCharacter.id) {
            //    Debug.Log("Added log to history of " + this.name + ". " + log.isInspected);
            //}
            if (this._history.Count > 300) {
                this._history.RemoveAt(0);
            }
            Messenger.Broadcast(Signals.HISTORY_ADDED, this as object);
        }
    }
    //Add log to this character and show notif of that log only if this character is clicked or tracked, otherwise, add log only
    public void RegisterLogAndShowNotifToThisCharacterOnly(string fileName, string key, object target = null, string targetName = "", GoapAction goapAction = null) {
        if (!GameManager.Instance.gameHasStarted) {
            return;
        }
        if (key == "remove_trait" && isDead) {
            return;
        }
        Log addLog = new Log(GameManager.Instance.Today(), "Character", fileName, key, goapAction);
        addLog.AddToFillers(this, this.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        if (targetName != "") {
            addLog.AddToFillers(target, targetName, LOG_IDENTIFIER.TARGET_CHARACTER);
        }
        addLog.AddLogToInvolvedObjects();
        if (PlayerManager.Instance.player.ShouldShowNotificationFrom(this, true)) {
            PlayerManager.Instance.player.ShowNotification(addLog);
        }
    }
    private void OnActionStateSet(GoapAction action, GoapActionState state) {
        if (action.actor != this && action.poiTarget != this) {
            if (marker.inVisionPOIs.Contains(action.actor)) {
                if (GetTrait("Unconscious", "Resting") != null) {
                    return;
                }
                CreateWitnessedEventLog(action);
            }
        }
    }
    public void ThisCharacterSaw(Character target) {
        if (target.currentAction != null && target.currentAction.isPerformingActualAction && !target.currentAction.isDone) {
            if(target.currentAction.actor != this && target.currentAction.poiTarget != this) {
                if(GetTrait("Unconscious", "Resting") != null) {
                    return;
                }
                CreateWitnessedEventLog(target.currentAction);
            }
        }
    }
    public List<Log> GetMemories(int dayFrom, int dayTo, bool eventMemoriesOnly = false){
        List<Log> memories = new List<Log>();
        if (eventMemoriesOnly) {
            for (int i = 0; i < _history.Count; i++) {
                if (_history[i].goapAction != null) {
                    if (_history[i].day >= dayFrom && _history[i].day <= dayTo) {
                        memories.Add(_history[i]);
                    }
                }
            }
        } else {
            for (int i = 0; i < _history.Count; i++) {
                if(_history[i].day >= dayFrom && _history[i].day <= dayTo) {
                    memories.Add(_history[i]);
                }
            }
        }
        return memories;
    }
    public List<Log> GetWitnessOrInformedMemories(int dayFrom, int dayTo) {
        List<Log> memories = new List<Log>();
        for (int i = 0; i < _history.Count; i++) {
            Log historyLog = _history[i];
            if (historyLog.goapAction != null && (historyLog.key == "witness_event" || historyLog.key == "informed_event")) {
                if (historyLog.day >= dayFrom && historyLog.day <= dayTo) {
                    memories.Add(historyLog);
                }
            }
        }
        return memories;
    }
    public List<Log> GetCrimeMemories(int dayFrom, int dayTo, Character involvedCharacter = null) {
        List<Log> memories = new List<Log>();
        for (int i = 0; i < _history.Count; i++) {
            Log historyLog = _history[i];
            if (historyLog.goapAction != null && historyLog.goapAction.committedCrime != CRIME.NONE) {
                if (historyLog.day >= dayFrom && historyLog.day <= dayTo) {
                    if(involvedCharacter != null) {
                        for (int j = 0; j < historyLog.goapAction.crimeCommitters.Length; j++) {
                            Character criminal = historyLog.goapAction.crimeCommitters[j];
                            if (criminal == involvedCharacter) {
                                memories.Add(historyLog);
                                break;
                            }
                        }
                    } else {
                        memories.Add(historyLog);
                    }
                }
            }
        }
        return memories;
    }
    public void CreateInformedEventLog(GoapAction eventToBeInformed) {
        Log informedLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "informed_event", eventToBeInformed);
        informedLog.AddToFillers(eventToBeInformed.currentState.descriptionLog.fillers);
        informedLog.AddToFillers(this, this.name, LOG_IDENTIFIER.OTHER);
        informedLog.AddToFillers(null, Utilities.LogDontReplace(eventToBeInformed.currentState.descriptionLog), LOG_IDENTIFIER.APPEND);
        AddHistory(informedLog);

        //If a character sees or informed about a lover performing Making Love or Ask to Make Love, they will feel Betrayed
        if (eventToBeInformed.actor != this && eventToBeInformed.poiTarget != this) {
            Character target = eventToBeInformed.poiTarget as Character;
            if (eventToBeInformed.goapType == INTERACTION_TYPE.MAKE_LOVE) {
                target = (eventToBeInformed as MakeLove).targetCharacter;
                if (HasRelationshipOfTypeWith(eventToBeInformed.actor, RELATIONSHIP_TRAIT.LOVER) || HasRelationshipOfTypeWith(target, RELATIONSHIP_TRAIT.LOVER)) {
                    Betrayed betrayed = new Betrayed();
                    AddTrait(betrayed);
                    CharacterManager.Instance.RelationshipDegradation(eventToBeInformed.actor, this, eventToBeInformed);
                    CharacterManager.Instance.RelationshipDegradation(target, this, eventToBeInformed);
                }
            } else if (eventToBeInformed.goapType == INTERACTION_TYPE.INVITE_TO_MAKE_LOVE) {
                if (HasRelationshipOfTypeWith(eventToBeInformed.actor, RELATIONSHIP_TRAIT.LOVER)) {
                    Betrayed betrayed = new Betrayed();
                    AddTrait(betrayed);
                    CharacterManager.Instance.RelationshipDegradation(eventToBeInformed.actor, this, eventToBeInformed);
                    CharacterManager.Instance.RelationshipDegradation(target, this, eventToBeInformed);
                } else if (HasRelationshipOfTypeWith(target, RELATIONSHIP_TRAIT.LOVER)) {
                    if (eventToBeInformed.currentState.name == "Invite Success") {
                        Betrayed betrayed = new Betrayed();
                        AddTrait(betrayed);
                        CharacterManager.Instance.RelationshipDegradation(eventToBeInformed.actor, this, eventToBeInformed);
                        CharacterManager.Instance.RelationshipDegradation(target, this, eventToBeInformed);
                    }
                }
            }
        }
    }
    public void CreateWitnessedEventLog(GoapAction witnessedEvent) {
        Log witnessLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "witness_event", witnessedEvent);
        witnessLog.AddToFillers(marker.character, marker.character.name, LOG_IDENTIFIER.OTHER);
        witnessLog.AddToFillers(null, Utilities.LogDontReplace(witnessedEvent.currentState.descriptionLog), LOG_IDENTIFIER.APPEND);
        witnessLog.AddToFillers(witnessedEvent.currentState.descriptionLog.fillers);
        AddHistory(witnessLog);

        //If a character sees or informed about a lover performing Making Love or Ask to Make Love, they will feel Betrayed
        if (witnessedEvent.actor != this && witnessedEvent.poiTarget != this) {
            Character target = witnessedEvent.poiTarget as Character;
            if (witnessedEvent.goapType == INTERACTION_TYPE.MAKE_LOVE) {
                target = (witnessedEvent as MakeLove).targetCharacter;
                if (HasRelationshipOfTypeWith(witnessedEvent.actor, RELATIONSHIP_TRAIT.LOVER) || HasRelationshipOfTypeWith(target, RELATIONSHIP_TRAIT.LOVER)) {
                    Betrayed betrayed = new Betrayed();
                    AddTrait(betrayed);
                    CharacterManager.Instance.RelationshipDegradation(witnessedEvent.actor, this, witnessedEvent);
                    CharacterManager.Instance.RelationshipDegradation(target, this, witnessedEvent);
                } 
            } else if (witnessedEvent.goapType == INTERACTION_TYPE.INVITE_TO_MAKE_LOVE) {
                if (HasRelationshipOfTypeWith(witnessedEvent.actor, RELATIONSHIP_TRAIT.LOVER)) {
                    Betrayed betrayed = new Betrayed();
                    AddTrait(betrayed);
                    CharacterManager.Instance.RelationshipDegradation(witnessedEvent.actor, this, witnessedEvent);
                    CharacterManager.Instance.RelationshipDegradation(target, this, witnessedEvent);
                } else if (HasRelationshipOfTypeWith(target, RELATIONSHIP_TRAIT.LOVER)) {
                    if (witnessedEvent.currentState.name == "Invite Success") {
                        Betrayed betrayed = new Betrayed();
                        AddTrait(betrayed);
                        CharacterManager.Instance.RelationshipDegradation(witnessedEvent.actor, this, witnessedEvent);
                        CharacterManager.Instance.RelationshipDegradation(target, this, witnessedEvent);
                    }
                }
            }
        }
    }
    #endregion

    #region Character
    public STANCE GetCurrentStance() {
        return STANCE.NEUTRAL;
    }
    #endregion

    #region Combat Handlers
    public void SetIsInCombat(bool state) {
        _isInCombat = state;
    }
    public void SetCombatCharacter(CombatCharacter combatCharacter) {
        _currentCombatCharacter = combatCharacter;
    }
    #endregion

    #region Portrait Settings
    public void SetPortraitSettings(PortraitSettings settings) {
        _portraitSettings = settings;
    }
    #endregion

    #region RPG
    private bool hpMagicRangedStatMod;
    public void LevelUp() {
        //Only level up once per day
        //if (_lastLevelUpDay == GameManager.Instance.continuousDays) {
        //    return;
        //}
        //_lastLevelUpDay = GameManager.Instance.continuousDays;
        if (_level < CharacterManager.Instance.maxLevel) {
            _level += 1;
            //Add stats per level from class
            if (_characterClass.attackType == ATTACK_TYPE.PHYSICAL && _characterClass.rangeType == RANGE_TYPE.MELEE) {
                AdjustAttackMod(_characterClass.attackPowerPerLevel);
                AdjustSpeedMod(_characterClass.speedPerLevel);
                AdjustMaxHPMod(_characterClass.hpPerLevel);
            } else if (_characterClass.attackType == ATTACK_TYPE.PHYSICAL && _characterClass.rangeType == RANGE_TYPE.RANGED) {
                if (_level % 2 == 0) {
                    //even
                    AdjustMaxHPMod(_characterClass.hpPerLevel);
                } else {
                    //odd
                    AdjustAttackMod(_characterClass.attackPowerPerLevel);
                }
                AdjustSpeedMod(_characterClass.speedPerLevel);
            } else if (_characterClass.attackType == ATTACK_TYPE.MAGICAL && _characterClass.rangeType == RANGE_TYPE.RANGED) {
                if (!hpMagicRangedStatMod) {
                    AdjustAttackMod(_characterClass.attackPowerPerLevel);
                } else {
                    AdjustMaxHPMod(_characterClass.hpPerLevel);
                }
                if ((_level - 1) % 2 == 0) {
                    hpMagicRangedStatMod = !hpMagicRangedStatMod;
                }
                AdjustSpeedMod(_characterClass.speedPerLevel);
            }

            //Reset to full health and sp
            ResetToFullHP();

            if (_playerCharacterItem != null) {
                _playerCharacterItem.UpdateMinionItem();
                Messenger.Broadcast(Signals.CHARACTER_LEVEL_CHANGED, this);
            }
        }
    }
    public void LevelUp(int amount) {
        //Only level up once per day
        //if (_lastLevelUpDay == GameManager.Instance.continuousDays) {
        //    return;
        //}
        //_lastLevelUpDay = GameManager.Instance.continuousDays;
        int supposedLevel = _level + amount;
        if (supposedLevel > CharacterManager.Instance.maxLevel) {
            amount = CharacterManager.Instance.maxLevel - level;
        } else if (supposedLevel < 0) {
            amount -= (supposedLevel - 1);
        }
        //Add stats per level from class
        if (_characterClass.attackType == ATTACK_TYPE.PHYSICAL && _characterClass.rangeType == RANGE_TYPE.MELEE) {
            AdjustAttackMod(_characterClass.attackPowerPerLevel * amount);
            AdjustSpeedMod(_characterClass.speedPerLevel * amount);
            AdjustMaxHPMod(_characterClass.hpPerLevel * amount);
        } else if (_characterClass.attackType == ATTACK_TYPE.PHYSICAL && _characterClass.rangeType == RANGE_TYPE.RANGED) {
            int multiplier = (amount < 0 ? -1 : 1);
            int range = amount * multiplier;
            for (int i = 0; i < range; i++) {
                if (i % 2 == 0) {
                    //even
                    AdjustMaxHPMod(_characterClass.hpPerLevel * multiplier);
                } else {
                    //odd
                    AdjustAttackMod(_characterClass.attackPowerPerLevel * multiplier);
                }
            }
            AdjustSpeedMod(_characterClass.speedPerLevel * amount);
        } else if (_characterClass.attackType == ATTACK_TYPE.MAGICAL && _characterClass.rangeType == RANGE_TYPE.RANGED) {
            int multiplier = (amount < 0 ? -1 : 1);
            int range = amount * multiplier;
            for (int i = _level; i <= _level + range; i++) {
                if (!hpMagicRangedStatMod) {
                    AdjustAttackMod(_characterClass.attackPowerPerLevel * multiplier);
                } else {
                    AdjustMaxHPMod(_characterClass.hpPerLevel * multiplier);
                }
                if (i != 1 && (i - 1) % 2 == 0) {
                    hpMagicRangedStatMod = !hpMagicRangedStatMod;
                }
            }
            AdjustSpeedMod(_characterClass.speedPerLevel * amount);
        }
        _level += amount;

        //Reset to full health and sp
        ResetToFullHP();
        //ResetToFullSP();
        Messenger.Broadcast(Signals.CHARACTER_LEVEL_CHANGED, this);
        if (_playerCharacterItem != null) {
            _playerCharacterItem.UpdateMinionItem();
        }
    }
    public void SetLevel(int amount) {
        int previousLevel = _level;
        _level = amount;
        if (_level < 1) {
            _level = 1;
        } else if (_level > CharacterManager.Instance.maxLevel) {
            _level = CharacterManager.Instance.maxLevel;
        }

        //Add stats per level from class
        int difference = _level - previousLevel;
        if (_characterClass.attackType == ATTACK_TYPE.PHYSICAL && _characterClass.rangeType == RANGE_TYPE.MELEE) {
            AdjustAttackMod(_characterClass.attackPowerPerLevel * difference);
            AdjustSpeedMod(_characterClass.speedPerLevel * difference);
            AdjustMaxHPMod(_characterClass.hpPerLevel * difference);
        } else if (_characterClass.attackType == ATTACK_TYPE.PHYSICAL && _characterClass.rangeType == RANGE_TYPE.RANGED) {
            int multiplier = (difference < 0 ? -1 : 1);
            int range = difference * multiplier;
            for (int i = 0; i < range; i++) {
                if (i % 2 == 0) {
                    //even
                    AdjustMaxHPMod(_characterClass.hpPerLevel * multiplier);
                } else {
                    //odd
                    AdjustAttackMod(_characterClass.attackPowerPerLevel * multiplier);
                }
            }
            AdjustSpeedMod(_characterClass.speedPerLevel * difference);
        } else if (_characterClass.attackType == ATTACK_TYPE.MAGICAL && _characterClass.rangeType == RANGE_TYPE.RANGED) {
            int multiplier = (difference < 0 ? -1 : 1);
            int range = difference * multiplier;
            for (int i = _level; i <= _level + range; i++) {
                if (!hpMagicRangedStatMod) {
                    AdjustAttackMod(_characterClass.attackPowerPerLevel * multiplier);
                } else {
                    AdjustMaxHPMod(_characterClass.hpPerLevel * multiplier);
                }
                if (i != 1 && (i - 1) % 2 == 0) {
                    hpMagicRangedStatMod = !hpMagicRangedStatMod;
                }
            }
            AdjustSpeedMod(_characterClass.speedPerLevel * difference);
        }

        //Reset to full health and sp
        ResetToFullHP();
        //ResetToFullSP();
        Messenger.Broadcast(Signals.CHARACTER_LEVEL_CHANGED, this);
        if (_playerCharacterItem != null) {
            _playerCharacterItem.UpdateMinionItem();
        }

        //Reset Experience
        //_experience = 0;
        //RecomputeMaxExperience();
    }
    public void OnCharacterClassChange() {
        if (_currentHP > _maxHPMod) {
            _currentHP = _maxHPMod;
        }
        if (_sp > _maxSP) {
            _sp = _maxSP;
        }
    }
    public void AdjustExperience(int amount) {
        _experience += amount;
        if (_experience >= _maxExperience) {
            _experience = 0;
            //LevelUp();
        }
    }
    public void AdjustElementalWeakness(ELEMENT element, float amount) {
        _elementalWeaknesses[element] += amount;
    }
    public void AdjustElementalResistance(ELEMENT element, float amount) {
        _elementalResistances[element] += amount;
    }
    public void AdjustSP(int amount) {
        _sp += amount;
        _sp = Mathf.Clamp(_sp, 0, _maxSP);
    }
    private void RecomputeMaxExperience() {
        _maxExperience = Mathf.CeilToInt(100f * ((Mathf.Pow((float) _level, 1.25f)) / 1.1f));
    }
    public void ResetToFullHP() {
        SetHP(maxHP);
    }
    public void ResetToFullSP() {
        AdjustSP(_maxSP);
    }
    private float GetComputedPower() {
        float compPower = 0f;
        for (int i = 0; i < currentParty.characters.Count; i++) {
            compPower += currentParty.characters[i].attackPower;
        }
        return compPower;
    }
    public void SetHP(int amount) {
        this._currentHP = amount;
    }
    public void SetMaxHPMod(int amount) {
        int previousMaxHP = maxHP;
        _maxHPMod = amount;
        int currentMaxHP = maxHP;
        if (_currentHP > currentMaxHP || _currentHP == previousMaxHP) {
            _currentHP = currentMaxHP;
        }
    }
    public void AdjustAttackMod(int amount) {
        _attackPowerMod += amount;
    }
    public void AdjustAttackPercentMod(int amount) {
        _attackPowerPercentMod += amount;
    }
    public void AdjustMaxHPMod(int amount) {
        int previousMaxHP = maxHP;
        _maxHPMod += amount;
        int currentMaxHP = maxHP;
        if (_currentHP > currentMaxHP || _currentHP == previousMaxHP) {
            _currentHP = currentMaxHP;
        }
    }
    public void AdjustMaxHPPercentMod(int amount) {
        int previousMaxHP = maxHP;
        _maxHPPercentMod += amount;
        int currentMaxHP = maxHP;
        if (_currentHP > currentMaxHP || _currentHP == previousMaxHP) {
            _currentHP = currentMaxHP;
        }
    }
    public void AdjustSpeedMod(int amount) {
        _speedMod += amount;
    }
    public void AdjustSpeedPercentMod(int amount) {
        _speedPercentMod += amount;
    }
    public bool IsHealthFull() {
        return _currentHP >= maxHP;
    }
    #endregion

    #region Home
    /// <summary>
    /// Set this character's home area data.(Does nothing else)
    /// </summary>
    /// <param name="newHome">The character's new home</param>
    public void SetHome(Area newHome) {
        this.homeArea = newHome;
    }
    public void SetHomeStructure(Dwelling homeStructure) {
        this.homeStructure = homeStructure;
        currentAlterEgo.SetHomeStructure(homeStructure);
    }
    public bool IsLivingWith(RELATIONSHIP_TRAIT type) {
        if (homeStructure != null && homeStructure.residents.Count > 1) {
            Character relTarget = GetCharacterWithRelationship(type);
            if (homeStructure.residents.Contains(relTarget)) {
                return true;
            }
        }
        return false;
    }
    public void MigrateHomeTo(Area newHomeArea, Dwelling homeStructure = null, bool broadcast = true) {
        Area previousHome = null;
        if (homeArea != null) {
            previousHome = homeArea;
            homeArea.RemoveResident(this);
        }
        newHomeArea.AddResident(this, homeStructure);
        if (broadcast) {
            Messenger.Broadcast(Signals.CHARACTER_MIGRATED_HOME, this, previousHome, newHomeArea);
        }
    }
    public void MigrateHomeStructureTo(Dwelling dwelling) {
        if (this.homeStructure != null) {
            if (this.homeStructure == dwelling) {
                return; //ignore change
            }
            //remove character from his/her old home
            this.homeStructure.RemoveResident(this);
        }
        dwelling.AddResident(this);
    }
    //private void OnCharacterMigratedHome(Character character, Area previousHome, Area homeArea) {
    //    if (character.id != this.id && this.homeArea.id == homeArea.id) {
    //        if (GetAllRelationshipTraitWith(character) != null) {
    //            this.homeArea.AssignCharacterToDwellingInArea(this); //redetermine home, in case new character with relationship has moved area to same area as this character
    //        }
    //    }
    //}
    #endregion

    #region IInteractable
    public void SetIsBeingInspected(bool state) {
        _isBeingInspected = state;
        if (_currentParty.icon != null) {
            _currentParty.icon.UpdateVisualState();
        }
        //if (_currentParty.specificLocation != null && _currentParty.specificLocation.coreTile.landmarkOnTile != null) {
        //    _currentParty.specificLocation.coreTile.landmarkOnTile.landmarkVisual.ToggleCharactersVisibility();
        //}
    }
    public void SetHasBeenInspected(bool state) {
        _hasBeenInspected = state;
    }
    public void EndedInspection() {

    }
    #endregion

    #region Defender
    public void OnSetAsDefender(Area defending) {
        defendingArea = defending;
        //this.ownParty.specificLocation.RemoveCharacterFromLocation(this.ownParty, false);
        //ownParty.SetSpecificLocation(defending.coreTile.landmarkOnTile);
    }
    public void OnRemoveAsDefender() {
        //defendingArea.coreTile.landmarkOnTile.AddCharacterToLocation(this.ownParty);
        defendingArea = null;
    }
    public bool IsDefending(BaseLandmark landmark) {
        if (defendingArea != null && defendingArea.id == landmark.id) {
            return true;
        }
        return false;
    }
    #endregion

    #region Traits
    public void CreateInitialTraitsByClass() {
        //Attack Type
        if (characterClass.attackType == ATTACK_TYPE.PHYSICAL) {
            AddTrait("Physical Attacker");
        } else if (characterClass.attackType == ATTACK_TYPE.MAGICAL) {
            AddTrait("Magic User");
        }

        //Range Type
        if (characterClass.rangeType == RANGE_TYPE.MELEE) {
            AddTrait("Melee Attack");
        } else if (characterClass.rangeType == RANGE_TYPE.RANGED) {
            AddTrait("Ranged Attack");
        }

        //Combat Position
        if (characterClass.combatPosition == COMBAT_POSITION.FRONTLINE) {
            AddTrait("Frontline Combatant");
        } else if (characterClass.combatPosition == COMBAT_POSITION.BACKLINE) {
            AddTrait("Backline Combatant");
        }

        ////Class Name
        //if (characterClass.className == "Knight" || characterClass.className == "Marauder" || characterClass.className == "Barbarian") {
        //    AddTrait(AttributeManager.Instance.allTraits["Melee Trait"]);
        //} else if (characterClass.className == "Stalker" || characterClass.className == "Archer" || characterClass.className == "Hunter") {
        //    AddTrait(AttributeManager.Instance.allTraits["Ranged Trait"]);
        //} else if (characterClass.className == "Druid" || characterClass.className == "Mage" || characterClass.className == "Shaman") {
        //    AddTrait(AttributeManager.Instance.allTraits["Magic Trait"]);
        //} else if (characterClass.className == "Spinner" || characterClass.className == "Abomination") {
        //    AddTrait(AttributeManager.Instance.allTraits["Melee Vulnerable"]);
        //} else if (characterClass.className == "Ravager") {
        //    AddTrait(AttributeManager.Instance.allTraits["Ranged Vulnerable"]);
        //} else if (characterClass.className == "Dragon") {
        //    AddTrait(AttributeManager.Instance.allTraits["Dragon Trait"]);
        //} else if (characterClass.className == "Greed") {
        //    AddTrait(AttributeManager.Instance.allTraits["Greed Trait"]);
        //} else if (characterClass.className == "Lust") {
        //    AddTrait(AttributeManager.Instance.allTraits["Lust Trait"]);
        //} else if (characterClass.className == "Envy") {
        //    AddTrait(AttributeManager.Instance.allTraits["Envy Trait"]);
        //}

        //Random Traits
        int chance = UnityEngine.Random.Range(0, 100);
        if (chance < 35) {
            AddTrait(new Craftsman());
        }
    }
    public void CreateInitialTraitsByRace() {
        if (race == RACE.HUMANS) {
            AddTrait("Beast Slayer");
        } else if (race == RACE.ELVES) {
            AddTrait("Anti Magic Aura");
        } else if (race == RACE.GOBLIN) {
            AddTrait("Soft Target");
        } else if (race == RACE.FAERY) {
            AddTrait("Melee Slayer");
        } else if (race == RACE.SKELETON) {
            AddTrait("Brittle Bones");
        } else if (race == RACE.DRAGON) {
            AddTrait("Steely Hide");
        } else if (race == RACE.SPIDER) {
            AddTrait("Faery Slayer");
        } else if (race == RACE.WOLF) {
            AddTrait("Goblin Slayer");
        } else if (race == RACE.ABOMINATION) {
            AddTrait("Elf Slayer");
        }
    }
    public bool AddTrait(string traitName, Character characterResponsible = null, System.Action onRemoveAction = null, GoapAction gainedFromDoing = null, bool triggerOnAdd = true) {
        return AddTrait(AttributeManager.Instance.allTraits[traitName], characterResponsible, onRemoveAction, gainedFromDoing, triggerOnAdd);
    }
    public bool AddTrait(Trait trait, Character characterResponsible = null, System.Action onRemoveAction = null, GoapAction gainedFromDoing = null, bool triggerOnAdd = true) {
        if (trait.IsUnique()) {
            Trait oldTrait = GetTrait(trait.name);
            if(oldTrait != null) {
                oldTrait.SetCharacterResponsibleForTrait(characterResponsible);
                oldTrait.AddCharacterResponsibleForTrait(characterResponsible);
                if (oldTrait.broadcastDuplicates) {
                    Messenger.Broadcast(Signals.TRAIT_ADDED, this, oldTrait);
                }
                return false;
            }
        }
        if (!(trait is RelationshipTrait)) {
            //Not adding relationship traits to the list of traits, since the getter will combine the traits list from this list and the relationships dictionary.
            //Did this so that relationships can be swappable without having to call RemoveTrait.
            _normalTraits.Add(trait); 
        }
        trait.SetGainedFromDoing(gainedFromDoing);
        trait.SetOnRemoveAction(onRemoveAction);
        trait.SetCharacterResponsibleForTrait(characterResponsible);
        trait.AddCharacterResponsibleForTrait(characterResponsible);
        ApplyTraitEffects(trait);
        ApplyPOITraitInteractions(trait);
        if (trait.daysDuration > 0) {
            GameDate removeDate = GameManager.Instance.Today();
            removeDate.AddTicks(trait.daysDuration);
            SchedulingManager.Instance.AddEntry(removeDate, () => RemoveTraitOnSchedule(trait));
        }
        if (triggerOnAdd) {
            trait.OnAddTrait(this);
        }
        Messenger.Broadcast(Signals.TRAIT_ADDED, this, trait);

        if (GameManager.Instance.gameHasStarted) {
            if (trait.name == "Hungry" || trait.name == "Starving") {
                Debug.Log("Planning fullness recovery from gain trait");
                PlanFullnessRecoveryActions();
            } else if (trait.name == "Lonely" || trait.name == "Forlorn") {
                Debug.Log("Planning happiness recovery from gain trait");
                PlanHappinessRecoveryActions();
            } else if (trait.name == "Tired" || trait.name == "Exhausted") {
                Debug.Log("Planning tiredness recovery from gain trait");
                PlanTirednessRecoveryActions();
            }
        }

        //if (trait is RelationshipTrait) {
        //    RelationshipTrait rel = trait as RelationshipTrait;
        //    AddRelationship(rel.targetCharacter, rel);
        //} else 
        if (trait.type == TRAIT_TYPE.CRIMINAL 
            || (trait.effect == TRAIT_EFFECT.NEGATIVE && trait.type == TRAIT_TYPE.DISABLER)) {
            //when a character gains a criminal trait, drop all location jobs that this character is assigned to
            homeArea.jobQueue.UnassignAllJobsTakenBy(this);
        }
        return true;
    }
    private bool RemoveTraitOnSchedule(Trait trait, bool triggerOnRemove = true) {
        if (isDead) {
            return false;
        }
        return RemoveTrait(trait, triggerOnRemove);
    }
    public bool RemoveTrait(Trait trait, bool triggerOnRemove = true) {
        bool removed = false;
        if (trait is RelationshipTrait) {
            removed = true;
        } else {
            removed = _normalTraits.Remove(trait);
        } 
        if (removed) {
            UnapplyTraitEffects(trait);
            UnapplyPOITraitInteractions(trait);
            if (triggerOnRemove) {
                trait.OnRemoveTrait(this);
            }
            Messenger.Broadcast(Signals.TRAIT_REMOVED, this, trait);
            //if (trait is RelationshipTrait) {
            //    RelationshipTrait rel = trait as RelationshipTrait;
            //    RemoveRelationship(rel.targetCharacter, rel);
            //}
        }
        return removed;
    }
    public bool RemoveTrait(string traitName, bool triggerOnRemove = true) {
        Trait trait = GetTrait(traitName);
        if (trait != null) {
            return RemoveTrait(trait, triggerOnRemove);
        }
        return false;
    }
    public void RemoveTrait(List<Trait> traits) {
        for (int i = 0; i < traits.Count; i++) {
            RemoveTrait(traits[i]);
        }
    }
    public void RemoveAllTraits(string traitNameException = "") {
        if (traitNameException == "") {
            while (allTraits.Count > 0) {
                RemoveTrait(allTraits[0]);
            }
        } else {
            for (int i = 0; i < allTraits.Count; i++) {
                if (allTraits[i].name != traitNameException) {
                    RemoveTrait(allTraits[i]);
                    i--;
                }
            }
        }
    }
    public void RemoveAllNonRelationshipTraits(string traitNameException = "") {
        if (traitNameException == "") {
            for (int i = 0; i < allTraits.Count; i++) {
                if (!(allTraits[i] is RelationshipTrait)) {
                    RemoveTrait(allTraits[i]);
                    i--;
                }
            }
        } else {
            for (int i = 0; i < allTraits.Count; i++) {
                if (allTraits[i].name != traitNameException && !(allTraits[i] is RelationshipTrait)) {
                    RemoveTrait(allTraits[i]);
                    i--;
                }
            }
        }
    }
    public Trait GetTrait(params string[] traitNames) {
    	for (int i = 0; i < allTraits.Count; i++) {
            Trait trait = allTraits[i];
            for (int j = 0; j < traitNames.Length; j++) {
                if (trait.name == traitNames[j] && !trait.isDisabled) {
                    return trait;
                }
            }
        }
        return null;
    }

    /// <summary>
    /// Remove all traits that are not persistent.
    /// NOTE: This does NOT remove relationships!
    /// </summary>
    public void RemoveAllNonPersistentTraits() {
        List<Trait> allTraits = new List<Trait>(this.allTraits);
        for (int i = 0; i < allTraits.Count; i++) {
            Trait currTrait = allTraits[i];
            if (currTrait is RelationshipTrait) {
                continue; //skip
            }
            if (!currTrait.isPersistent) {
                RemoveTrait(currTrait);
            }
        }
    }
    public Trait GetTrait(string traitName) {
        for (int i = 0; i < allTraits.Count; i++) {
            if (allTraits[i].name == traitName && !allTraits[i].isDisabled) {
                return allTraits[i];
            }
        }
        return null;
    }

    public Trait GetTraitOr(string traitName1, string traitName2) {
        for (int i = 0; i < allTraits.Count; i++) {
            if ((allTraits[i].name == traitName1 || allTraits[i].name == traitName2) && !allTraits[i].isDisabled) {
                return allTraits[i];
            }
        }
        return null;
    }

    public Trait GetTraitOr(string traitName1, string traitName2, string traitName3) {
	for (int i = 0; i < allTraits.Count; i++) {
            if ((allTraits[i].name == traitName1 || allTraits[i].name == traitName2 || allTraits[i].name == traitName3) && !allTraits[i].isDisabled) {
                return allTraits[i];
            }
        }
        return null;
    }
    public bool HasTraitOf(TRAIT_TYPE traitType, string traitException = "") {
        for (int i = 0; i < allTraits.Count; i++) {
            if (traitException != "" && allTraits[i].name == traitException) { continue; }
            if (allTraits[i].type == traitType && !allTraits[i].isDisabled) {
                return true;
            }
        }
        return false;
    }
    public bool HasTraitOf(TRAIT_EFFECT effect, TRAIT_TYPE type) {
        for (int i = 0; i < allTraits.Count; i++) {
            Trait currTrait = allTraits[i];
            if (currTrait.effect == effect && currTrait.type == type && !currTrait.isDisabled) {
                return true;
            }
        }
        return false;
    }
    public bool HasTraitOf(TRAIT_EFFECT effect1, TRAIT_EFFECT effect2, TRAIT_TYPE type) {
        for (int i = 0; i < allTraits.Count; i++) {
            Trait currTrait = allTraits[i];
            if ((currTrait.effect == effect1 || currTrait.effect == effect2) && currTrait.type == type && !currTrait.isDisabled) {
                return true;
            }
        }
        return false;
    }
    public bool HasTraitOf(TRAIT_EFFECT effect) {
        for (int i = 0; i < allTraits.Count; i++) {
            Trait currTrait = allTraits[i];
            if (currTrait.effect == effect && !currTrait.isDisabled) {
                return true;
            }
        }
        return false;
    }
    public Trait GetTraitOf(TRAIT_EFFECT effect, TRAIT_TYPE type) {
        for (int i = 0; i < allTraits.Count; i++) {
            Trait currTrait = allTraits[i];
            if (currTrait.effect == effect && currTrait.type == type && !currTrait.isDisabled) {
                return currTrait;
            }
        }
        return null;
    }
    public Trait GetTraitOf(TRAIT_TYPE type) {
        for (int i = 0; i < allTraits.Count; i++) {
            Trait currTrait = allTraits[i];
            if (currTrait.type == type && !currTrait.isDisabled) {
                return currTrait;
            }
        }
        return null;
    }
    public bool HasTraitOf(System.Type traitType) {
        for (int i = 0; i < allTraits.Count; i++) {
            System.Type type = allTraits[i].GetType();
            if (type == traitType) {
                return true;
            }
        }
        return false;
    }
    public List<Trait> RemoveAllTraitsByType(TRAIT_TYPE traitType) {
        List<Trait> removedTraits = new List<Trait>();
        for (int i = 0; i < allTraits.Count; i++) {
            if (allTraits[i].type == traitType) {
                removedTraits.Add(allTraits[i]);
                RemoveTrait(allTraits[i]);
                i--;
            }
        }
        return removedTraits;
    }
    public Trait GetRandomTrait(TRAIT_EFFECT effect) {
        List<Trait> negativeTraits = new List<Trait>();
        for (int i = 0; i < allTraits.Count; i++) {
            if (allTraits[i].effect == effect && !allTraits[i].isDisabled) {
                negativeTraits.Add(allTraits[i]);
            }
        }
        if (negativeTraits.Count > 0) {
            return negativeTraits[UnityEngine.Random.Range(0, negativeTraits.Count)];
        }
        return null;
    }
    private void ApplyTraitEffects(Trait trait) {
        if (trait.type == TRAIT_TYPE.DISABLER) {
            AdjustDoNotDisturb(1);
            if (trait.effect == TRAIT_EFFECT.NEGATIVE) {
                AdjustIgnoreHostilities(1);
            }
            _ownParty.RemoveAllOtherCharacters();
            if (trait.name != "Combat Recovery") {
                CancelAllJobsTargettingThisCharacter("Assault");
            }
        } else if (trait.type == TRAIT_TYPE.CRIMINAL) {
            CancelOrUnassignRemoveTraitRelatedJobs();
        }
        if (trait.name == "Abducted" || trait.name == "Restrained") {
            AdjustDoNotGetTired(1);
        } else if (trait.name == "Packaged" || trait.name == "Hibernating" || trait.name == "Reanimated") {
            AdjustDoNotGetTired(1);
            AdjustDoNotGetHungry(1);
            AdjustDoNotGetLonely(1);
        } else if (trait.name == "Eating") {
            AdjustDoNotGetHungry(1);
        } else if (trait.name == "Resting") {
            AdjustDoNotGetTired(1);
            AdjustDoNotGetHungry(1);
            AdjustDoNotGetLonely(1);
        } else if (trait.name == "Charmed") {
            AdjustDoNotGetLonely(1);
        } else if (trait.name == "Daydreaming") {
            AdjustDoNotGetTired(1);
            AdjustDoNotGetLonely(1);
        } else if (trait.name == "Forlorn") {
            AdjustMoodValue(-35, trait.gainedFromDoing);
        } else if (trait.name == "Lonely") {
            AdjustMoodValue(-20, trait.gainedFromDoing);
        } else if (trait.name == "Exhausted") {
            marker.AdjustUseWalkSpeed(1);
            AdjustMoodValue(-35, trait.gainedFromDoing);
        } else if (trait.name == "Tired") {
            marker.AdjustSpeedModifier(-0.2f);
            AdjustMoodValue(-10, trait.gainedFromDoing);
        } else if (trait.name == "Starving") {
            AdjustMoodValue(-25, trait.gainedFromDoing);
        } else if (trait.name == "Hungry") {
            AdjustMoodValue(-10, trait.gainedFromDoing);
        } else if (trait.name == "Injured") {
            AdjustMoodValue(-15, trait.gainedFromDoing);
        } else if (trait.name == "Cursed") {
            AdjustMoodValue(-25, trait.gainedFromDoing);
        } else if (trait.name == "Sick") {
            AdjustMoodValue(-15, trait.gainedFromDoing);
        } else if (trait.name == "Cheery") {
            AdjustMoodValue(15, trait.gainedFromDoing);
        } else if (trait.name == "Annoyed") {
            AdjustMoodValue(-15, trait.gainedFromDoing);
        } else if (trait.name == "Lethargic") {
            AdjustMoodValue(-20, trait.gainedFromDoing);
        } else if (trait.name == "Heartbroken") {
            AdjustMoodValue(-35, trait.gainedFromDoing);
        }
        //else if (trait.name == "Hungry") {
        //    CreateFeedJob();
        //} else if (trait.name == "Starving") {
        //    MoveFeedJobToTopPriority();
        //}
        for (int i = 0; i < trait.effects.Count; i++) {
            TraitEffect traitEffect = trait.effects[i];
            if (!traitEffect.hasRequirement && traitEffect.target == TRAIT_REQUIREMENT_TARGET.SELF) {
                if (traitEffect.isPercentage) {
                    if (traitEffect.stat == STAT.ATTACK) {
                        AdjustAttackPercentMod((int) traitEffect.amount);
                    } else if (traitEffect.stat == STAT.HP) {
                        AdjustMaxHPPercentMod((int) traitEffect.amount);
                    } else if (traitEffect.stat == STAT.SPEED) {
                        AdjustSpeedPercentMod((int) traitEffect.amount);
                    }
                } else {
                    if (traitEffect.stat == STAT.ATTACK) {
                        AdjustAttackMod((int) traitEffect.amount);
                    } else if (traitEffect.stat == STAT.HP) {
                        AdjustMaxHPMod((int) traitEffect.amount);
                    } else if (traitEffect.stat == STAT.SPEED) {
                        AdjustSpeedMod((int) traitEffect.amount);
                    }
                }
            }
        }
    }
    private void UnapplyTraitEffects(Trait trait) {
        if (trait.type == TRAIT_TYPE.DISABLER) {
            AdjustDoNotDisturb(-1);
            if (trait.effect == TRAIT_EFFECT.NEGATIVE) {
                AdjustIgnoreHostilities(-1);
            }
        }
        //else if(trait.type == TRAIT_TYPE.CRIMINAL) {
        //    if(GetTraitOf(TRAIT_TYPE.CRIMINAL) == null) {
        //        for (int i = 0; i < traits.Count; i++) {
        //            if (traits[i].name == "Cursed" || traits[i].name == "Sick"
        //                || traits[i].name == "Injured" || traits[i].name == "Unconscious") {
        //                CreateRemoveTraitJob(traits[i].name);
        //            }
        //        }
        //    }
        //}
        if (trait.name == "Abducted" || trait.name == "Restrained") {
            AdjustDoNotGetTired(-1);
        } else if (trait.name == "Packaged" || trait.name == "Hibernating" || trait.name == "Reanimated") {
            AdjustDoNotGetTired(-1);
            AdjustDoNotGetHungry(-1);
            AdjustDoNotGetLonely(-1);
        } else if (trait.name == "Eating") {
            AdjustDoNotGetHungry(-1);
        } else if (trait.name == "Resting") {
            AdjustDoNotGetTired(-1);
            AdjustDoNotGetHungry(-1);
            AdjustDoNotGetLonely(-1);
        } else if (trait.name == "Charmed") {
            AdjustDoNotGetLonely(-1);
        } else if (trait.name == "Daydreaming") {
            AdjustDoNotGetTired(-1);
            AdjustDoNotGetLonely(-1);
        } else if (trait.name == "Forlorn") {
            AdjustMoodValue(35, trait.gainedFromDoing);
        } else if (trait.name == "Lonely") {
            AdjustMoodValue(20, trait.gainedFromDoing);
        } else if (trait.name == "Exhausted") {
            marker.AdjustUseWalkSpeed(-1);
            AdjustMoodValue(35, trait.gainedFromDoing);
        } else if (trait.name == "Tired") {
            marker.AdjustSpeedModifier(0.2f);
            AdjustMoodValue(10, trait.gainedFromDoing);
        } else if (trait.name == "Starving") {
            AdjustMoodValue(25, trait.gainedFromDoing);
        } else if (trait.name == "Hungry") {
            AdjustMoodValue(10, trait.gainedFromDoing);
        } else if (trait.name == "Injured") {
            AdjustMoodValue(15, trait.gainedFromDoing);
        } else if (trait.name == "Cursed") {
            AdjustMoodValue(25, trait.gainedFromDoing);
        } else if (trait.name == "Sick") {
            AdjustMoodValue(15, trait.gainedFromDoing);
        } else if (trait.name == "Cheery") {
            AdjustMoodValue(-15, trait.gainedFromDoing);
        } else if (trait.name == "Annoyed") {
            AdjustMoodValue(15, trait.gainedFromDoing);
        } else if (trait.name == "Lethargic") {
            AdjustMoodValue(20, trait.gainedFromDoing);
        }
        for (int i = 0; i < trait.effects.Count; i++) {
            TraitEffect traitEffect = trait.effects[i];
            if (!traitEffect.hasRequirement && traitEffect.target == TRAIT_REQUIREMENT_TARGET.SELF) {
                if (traitEffect.isPercentage) {
                    if (traitEffect.stat == STAT.ATTACK) {
                        AdjustAttackPercentMod(-(int) traitEffect.amount);
                    } else if (traitEffect.stat == STAT.HP) {
                        AdjustMaxHPPercentMod(-(int) traitEffect.amount);
                    } else if (traitEffect.stat == STAT.SPEED) {
                        AdjustSpeedPercentMod(-(int) traitEffect.amount);
                    }
                } else {
                    if (traitEffect.stat == STAT.ATTACK) {
                        AdjustAttackMod(-(int) traitEffect.amount);
                    } else if (traitEffect.stat == STAT.HP) {
                        AdjustMaxHPMod(-(int) traitEffect.amount);
                    } else if (traitEffect.stat == STAT.SPEED) {
                        AdjustSpeedMod(-(int) traitEffect.amount);
                    }
                }
            }
        }
    }
    private void ApplyPOITraitInteractions(Trait trait) {
        if (trait.advertisedInteractions != null) {
            for (int i = 0; i < trait.advertisedInteractions.Count; i++) {
                poiGoapActions.Add(trait.advertisedInteractions[i]);
            }
        }
    }
    private void UnapplyPOITraitInteractions(Trait trait) {
        if (trait.advertisedInteractions != null) {
            for (int i = 0; i < trait.advertisedInteractions.Count; i++) {
                poiGoapActions.Remove(trait.advertisedInteractions[i]);
            }
        }
    }
    private void SetTraitsFromClass() {
        if (_characterClass.traitNames != null) {
            for (int i = 0; i < _characterClass.traitNames.Length; i++) {
                Trait trait = AttributeManager.Instance.allTraits[_characterClass.traitNames[i]];
                AddTrait(trait);
            }
        }
    }
    public void RemoveTraitsFromClass() {
        if (_characterClass.traitNames != null) {
            for (int i = 0; i < _characterClass.traitNames.Length; i++) {
                Trait trait = AttributeManager.Instance.allTraits[_characterClass.traitNames[i]];
                RemoveTrait(trait);
            }
        }
    }
    private void SetTraitsFromRace() {
        if (_raceSetting.traitNames != null) {
            for (int i = 0; i < _raceSetting.traitNames.Length; i++) {
                Trait trait = AttributeManager.Instance.allTraits[_raceSetting.traitNames[i]];
                AddTrait(trait);
            }
        }
    }
    public void RemoveTraitsFromRace() {
        if (_raceSetting.traitNames != null) {
            for (int i = 0; i < _raceSetting.traitNames.Length; i++) {
                Trait trait = AttributeManager.Instance.allTraits[_raceSetting.traitNames[i]];
                RemoveTrait(trait);
            }
        }
    }
    public bool HasRelationshipTraitOf(RELATIONSHIP_TRAIT relType, bool includeDead = true) {
        for (int i = 0; i < allTraits.Count; i++) {
            if (allTraits[i] is RelationshipTrait && !allTraits[i].isDisabled) {
                RelationshipTrait currTrait = allTraits[i] as RelationshipTrait;
                if (currTrait.targetCharacter.isDead && !includeDead) {
                    continue; //skip dead characters
                }
                if (currTrait.relType == relType) {
                    return true;
                }
            }
        }
        return false;
    }
    public bool HasRelationshipTraitOf(RELATIONSHIP_TRAIT relType, Faction except) {
        for (int i = 0; i < allTraits.Count; i++) {
            if (allTraits[i] is RelationshipTrait && !allTraits[i].isDisabled) {
                RelationshipTrait currTrait = allTraits[i] as RelationshipTrait;
                if (currTrait.relType == relType
                    && currTrait.targetCharacter.faction.id != except.id) {
                    return true;
                }
            }
        }
        return false;
    }
    public bool ReleaseFromAbduction() {
        Trait trait = GetTrait("Abducted");
        if (trait != null) {
            Abducted abductedTrait = trait as Abducted;
            RemoveTrait(abductedTrait);
            ReturnToOriginalHomeAndFaction(abductedTrait.originalHome, this.faction);
            //MigrateTo(abductedTrait.originalHomeLandmark);

            //Interaction interactionAbducted = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.MOVE_TO_RETURN_HOME, specificLocation);
            //InduceInteraction(interactionAbducted);
            return true;
        }
        return false;
    }
    public SpecialToken CraftAnItem() {
        Craftsman craftsmanTrait = GetTrait("Craftsman") as Craftsman;
        if (craftsmanTrait != null) {
            //SpecialTokenSettings settings = TokenManager.Instance.GetTokenSettings(craftsmanTrait.craftedItemName);
            //return TokenManager.Instance.CreateSpecialToken(craftsmanTrait.craftedItemName); //, settings.appearanceWeight
        }
        return null;
    }
    public void AddTraitNeededToBeRemoved(Trait trait) {
        traitsNeededToBeRemoved.Add(trait);
    }
    public void RemoveTraitNeededToBeRemoved(Trait trait) {
        traitsNeededToBeRemoved.Remove(trait);
    }
    #endregion

    #region Morality
    public void SetMorality(MORALITY morality) {
        this.morality = morality;
    }
    #endregion

    #region Minion
    public void SetMinion(Minion minion) {
        _minion = minion;
        UnsubscribeSignals();
    }
    public void RecruitAsMinion() {
        if (!IsInOwnParty()) {
            _currentParty.RemoveCharacter(this);
        }
        MigrateHomeTo(PlayerManager.Instance.player.playerArea);

        specificLocation.RemoveCharacterFromLocation(this.currentParty);
        //PlayerManager.Instance.player.demonicPortal.AddCharacterToLocation(this.currentParty);

        ChangeFactionTo(PlayerManager.Instance.player.playerFaction);

        Minion newMinion = PlayerManager.Instance.player.CreateNewMinion(this);
        PlayerManager.Instance.player.AddMinion(newMinion);

        if (!characterToken.isObtainedByPlayer) {
            PlayerManager.Instance.player.AddToken(characterToken);
        }
    }
    #endregion

    #region Buffs
    public void ConstructBuffs() {
        _buffs = new Dictionary<STAT, float>();
        STAT[] stats = Utilities.GetEnumValues<STAT>();
        for (int i = 0; i < stats.Length; i++) {
            _buffs.Add(stats[i], 0f);
        }
    }
    public void AddBuff(Buff buff) {
        if (_buffs.ContainsKey(buff.buffedStat)) {
            _buffs[buff.buffedStat] += buff.percentage;
        }
    }
    public void RemoveBuff(Buff buff) {
        if (_buffs.ContainsKey(buff.buffedStat)) {
            _buffs[buff.buffedStat] -= buff.percentage;
        }
    }
    #endregion

    public void SetPlayerCharacterItem(PlayerCharacterItem item) {
        _playerCharacterItem = item;
    }

    #region Interaction
    public void AddInteractionType(INTERACTION_TYPE type) {
        if (!currentInteractionTypes.Contains(type)) {
            currentInteractionTypes.Add(type);
        }
    }
    public void RemoveInteractionType(INTERACTION_TYPE type) {
        currentInteractionTypes.Remove(type);
    }
    private int GetMonthInteractionTick() {
        int daysInMonth = 15; //GameManager.daysInMonth[GameManager.Instance.month]
        int remainingDaysInMonth = GameManager.Instance.continuousDays % daysInMonth;
        int startDay = GameManager.Instance.continuousDays + remainingDaysInMonth + 1;
        return UnityEngine.Random.Range(startDay, startDay + daysInMonth);
    }
    public void SetDailyInteractionGenerationTick() {
        //if(specificLocation == null || specificLocation.id == homeArea.id) {
        //    _currentInteractionTick = GetMonthInteractionTick();
        //} else {
        //    int remainingDaysInWeek = GameManager.Instance.continuousDays % 7;
        //    int startDay = GameManager.Instance.continuousDays + remainingDaysInWeek + 1;
        //    _currentInteractionTick = UnityEngine.Random.Range(startDay, startDay + 7);
        //}
        int remainingDaysInWeek = GameManager.ticksPerTimeInWords - (GameManager.Instance.tick % GameManager.ticksPerTimeInWords);
        if (remainingDaysInWeek == GameManager.ticksPerTimeInWords) {
            remainingDaysInWeek = 0;
        }
        int startDay = GameManager.Instance.tick + remainingDaysInWeek + 1;
        if (startDay > GameManager.ticksPerDay) {
            startDay -= GameManager.ticksPerDay;
        }
        //if (startDay < 25) {
        //    startDay = 25;
        //}
        _currentInteractionTick = UnityEngine.Random.Range(startDay, startDay + GameManager.ticksPerTimeInWords);
    }
    public void AdjustDailyInteractionGenerationTick() {
        //Adjusts the tick trigger for this character to compensate for the 5 day delay interaction trigger
        //Characters trigger AI once every 12 ticks but exclude any tick that will be reached by the 5-tick wait time of the preceding tick.
        int remainingDaysInWeek = GameManager.ticksPerTimeInWords - (GameManager.Instance.tick % GameManager.ticksPerTimeInWords);
        if (remainingDaysInWeek < GameManager.ticksPerTimeInWords) {
            int startDay = GameManager.Instance.tick + remainingDaysInWeek + 1;
            _currentInteractionTick = UnityEngine.Random.Range(GameManager.Instance.tick + 1, startDay);
        }
    }
    public void SetDailyInteractionGenerationTick(int tick) {
        _currentInteractionTick = tick;
    }
    private void DailyGoapPlanGeneration() {
        //Check Trap Structure
        trapStructure.IncrementCurrentDuration(1);

        //This is to ensure that this character will not be idle forever
        //If at the start of the tick, the character is not currently doing any action, and is not waiting for any new plans, it means that the character will no longer perform any actions
        //so start doing actions again
        SetHasAlreadyAskedForPlan(false);
        if (currentAction == null && _numOfWaitingForGoapThread <= 0) {
            PlanGoapActions();
        }
    }
    public void SetDailyGoapGenerationTick() {
        int remainingDaysInWeek = 6 - (GameManager.Instance.tick % 6);
        if (remainingDaysInWeek == 6) {
            remainingDaysInWeek = 0;
        }
        int startDay = GameManager.Instance.tick + remainingDaysInWeek + 1;
        if (startDay > GameManager.ticksPerDay) {
            startDay -= GameManager.ticksPerDay;
        }
        _currentInteractionTick = UnityEngine.Random.Range(startDay, startDay + 6);
    }
    public void PlanGoapActions() {
        if (isDead) {
            //StopDailyGoapPlanGeneration();
            return;
        }
        if (minion != null || !IsInOwnParty() || isDefender || ownParty.icon.isTravelling || _doNotDisturb > 0 || _job == null || isWaitingForInteraction > 0 || marker.pathfindingThread != null) {
            return; //if this character is not in own party, is a defender or is travelling or cannot be disturbed, do not generate interaction
        }
        if (stateComponent.currentState != null) {
            Debug.LogWarning("Currently in " + stateComponent.currentState.stateName + " state, can't plan actions!");
            return;
        }
        //if(specificAction != null) {
        //    WillAboutToDoAction(specificAction);
        //    return;
        //}
        if (allGoapPlans.Count > 0) {
            //StopDailyGoapPlanGeneration();
            PerformGoapPlans();
            //SchedulePerformGoapPlans();
        } else {
            IdlePlans();
        }
    }
    private void IdlePlans() {
        if (_hasAlreadyAskedForPlan) {
            return;
        }
        SetHasAlreadyAskedForPlan(true);
        if (!PlanJobQueueFirst()) {
            if (!PlanFullnessRecoveryActions()) {
                if (!PlanTirednessRecoveryActions()) {
                    if (!PlanHappinessRecoveryActions()) {
                        if (!PlanWorkActions()) {
                            string idleLog = OtherIdlePlans();
                            PrintLogIfActive(idleLog);
                        }
                    }
                }
            }
        }
    }
    private bool OtherPlanCreations() {
        int chance = UnityEngine.Random.Range(0, 100);
        if (GetTrait("Berserker") != null) {
            if (chance < 15) {
                Character target = specificLocation.GetRandomCharacterAtLocationExcept(this);
                if (target != null) {
                    StartGOAP(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_NON_POSITIVE_TRAIT, conditionKey = "Disabler", targetPOI = target }, target, GOAP_CATEGORY.NONE);
                    return true;
                }
            } else {
                chance = UnityEngine.Random.Range(0, 100);
                if (chance < 15) {
                    IPointOfInterest target = specificLocation.GetRandomTileObject();
                    if (target != null) {
                        StartGOAP(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.DESTROY, conditionKey = target, targetPOI = target }, target, GOAP_CATEGORY.NONE);
                        return true;
                    }
                }
            }
        }
        return false;
    }
    public bool PlanFullnessRecoveryActions() {
        TIME_IN_WORDS currentTimeInWords = GameManager.GetCurrentTimeInWordsOfTick();
        Trait hungryOrStarving = GetTrait("Starving", "Hungry");

        if (hungryOrStarving != null) {
            if (!jobQueue.HasJob("Fullness")) {
                int chance = UnityEngine.Random.Range(0, 100);
                int value = 0;
                if (hungryOrStarving.name == "Starving") {
                    value = 100;
                } else {
                    if (currentTimeInWords == TIME_IN_WORDS.MORNING) {
                        value = 50;
                    } else if (currentTimeInWords == TIME_IN_WORDS.EARLY_NIGHT) {
                        value = 50;
                    }
                }
                if (chance < value) {
                    GoapPlanJob job = new GoapPlanJob("Fullness", new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = null, targetPOI = this });
                    if(GetTrait("Vampiric") != null) {
                        job.AddForcedInteraction(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = null, targetPOI = this }, INTERACTION_TYPE.DRINK_BLOOD);
                    }
                    if (hungryOrStarving.name == "Starving") {
                        job.SetCannotOverrideJob(true);
                        job.SetWillImmediatelyBeDoneAfterReceivingPlan(true);
                    }
                    jobQueue.AddJobInQueue(job, true, false);
                    jobQueue.ProcessFirstJobInQueue(this);
                    //StartGOAP(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = null, targetPOI = this }, this, GOAP_CATEGORY.FULLNESS, true);
                    return true;
                }
            } else {
                if (hungryOrStarving.name == "Starving") {
                    GoapPlanJob job = jobQueue.GetJob("Fullness") as GoapPlanJob;
                    job.SetCannotOverrideJob(true);
                }
            }
        }
        return false;
    }
    public bool PlanTirednessRecoveryActions() {
        TIME_IN_WORDS currentTimeInWords = GameManager.GetCurrentTimeInWordsOfTick();
        Trait tiredOrExhausted = GetTrait("Exhausted", "Tired");

        if (tiredOrExhausted != null) {
            if (!jobQueue.HasJob("Tiredness")) {
                int chance = UnityEngine.Random.Range(0, 100);
                int value = 0;
                if (tiredOrExhausted.name == "Exhausted") {
                    value = 100;
                } else {
                    if (isAtHomeArea) {
                        if (currentTimeInWords == TIME_IN_WORDS.EARLY_NIGHT) {
                            value = 15;
                        } else if (currentTimeInWords == TIME_IN_WORDS.LATE_NIGHT) {
                            value = 65;
                        } else if (currentTimeInWords == TIME_IN_WORDS.AFTER_MIDNIGHT) {
                            value = 65;
                        }
                    }
                }
                if (chance < value) {
                    GoapPlanJob job = new GoapPlanJob("Tiredness", new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TIREDNESS_RECOVERY, conditionKey = null, targetPOI = this });
                    if (tiredOrExhausted.name == "Exhausted") {
                        job.SetCannotOverrideJob(true);
                        job.SetWillImmediatelyBeDoneAfterReceivingPlan(true);
                    }
                    jobQueue.AddJobInQueue(job, true, false);
                    jobQueue.ProcessFirstJobInQueue(this);
                    //StartGOAP(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TIREDNESS_RECOVERY, conditionKey = null, targetPOI = this }, this, GOAP_CATEGORY.TIREDNESS, true);
                    return true;
                }
            } else {
                if (tiredOrExhausted.name == "Exhausted") {
                    GoapPlanJob job = jobQueue.GetJob("Tiredness") as GoapPlanJob;
                    job.SetCannotOverrideJob(true);
                }
            }
        }
        return false;
    }
    public bool PlanHappinessRecoveryActions() {
        TIME_IN_WORDS currentTimeInWords = GameManager.GetCurrentTimeInWordsOfTick();
        Trait lonelyOrForlorn = GetTrait("Forlorn", "Lonely");

        if (lonelyOrForlorn != null) {
            if (!jobQueue.HasJob("Happiness")) {
                int chance = UnityEngine.Random.Range(0, 100);
                int value = 0;
                if (lonelyOrForlorn.name == "Forlorn") {
                    value = 100;
                } else {
                    if (currentTimeInWords == TIME_IN_WORDS.MORNING) {
                        value = 30;
                    } else if (currentTimeInWords == TIME_IN_WORDS.AFTERNOON) {
                        value = 45;
                    } else if (currentTimeInWords == TIME_IN_WORDS.EARLY_NIGHT) {
                        value = 45;
                    } else if (currentTimeInWords == TIME_IN_WORDS.LATE_NIGHT) {
                        value = 30;
                    }
                }
                if (chance < value) {
                    GoapPlanJob job = new GoapPlanJob("Happiness", new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, conditionKey = null, targetPOI = this });
                    if (lonelyOrForlorn.name == "Forlorn") {
                        job.SetCannotOverrideJob(true);
                        job.SetWillImmediatelyBeDoneAfterReceivingPlan(true);
                    }
                    jobQueue.AddJobInQueue(job, true, false);
                    jobQueue.ProcessFirstJobInQueue(this);
                    //StartGOAP(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, conditionKey = null, targetPOI = this }, this, GOAP_CATEGORY.HAPPINESS, true);
                    return true;
                }
            } else {
                if (lonelyOrForlorn.name == "Forlorn") {
                    GoapPlanJob job = jobQueue.GetJob("Happiness") as GoapPlanJob;
                    job.SetCannotOverrideJob(true);
                }
            }
        }
        return false;
    }
    private bool PlanWorkActions() { //ref bool hasAddedToGoapPlans
        if (GetPlanByCategory(GOAP_CATEGORY.WORK) == null) {
            if (!jobQueue.ProcessFirstJobInQueue(this)) {
                if (isAtHomeArea) { //&& this.faction.id != FactionManager.Instance.neutralFaction.id
                    return homeArea.jobQueue.ProcessFirstJobInQueue(this);
                } else {
                    return false;
                }
            } else {
                return true;
            }
        }
        return false;
    }
    private bool PlanJobQueueFirst() {
        if (GetPlanByCategory(GOAP_CATEGORY.WORK) == null && !isStarving && !isExhausted && !isForlorn) {
            if (!jobQueue.ProcessFirstJobInQueue(this)) {
                if (isAtHomeArea) {
                    return homeArea.jobQueue.ProcessFirstJobInQueue(this);
                } else {
                    return false;
                }
            } else {
                return true;
            }
        }
        return false;
    }
    public bool PlanIdleStroll(LocationStructure targetStructure, LocationGridTile targetTile = null) {
        if (currentStructure == targetStructure) {
            stateComponent.SwitchToState(CHARACTER_STATE.STROLL);
        } else {
            MoveToAnotherStructure(targetStructure, targetTile, null, () => stateComponent.SwitchToState(CHARACTER_STATE.STROLL));
        }
        return true;
    }
    public bool PlanIdleStrollOutside(LocationStructure targetStructure, LocationGridTile targetTile = null) {
        if (currentStructure == targetStructure) {
            stateComponent.SwitchToState(CHARACTER_STATE.STROLL_OUTSIDE);
        } else {
            MoveToAnotherStructure(targetStructure, targetTile, null, () => stateComponent.SwitchToState(CHARACTER_STATE.STROLL_OUTSIDE));
        }
        return true;
    }
    public bool PlanIdleReturnHome() {
        //if (GetTrait("Berserker") != null) {
        //    //Return home becomes stroll if the character has berserker trait
        //    PlanIdleStroll(currentStructure);
        //} else {
        GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.RETURN_HOME, this, this);
        goapAction.SetTargetStructure();
        GoapNode goalNode = new GoapNode(null, goapAction.cost, goapAction);
        GoapPlan goapPlan = new GoapPlan(goalNode, new GOAP_EFFECT_CONDITION[] { GOAP_EFFECT_CONDITION.NONE }, GOAP_CATEGORY.IDLE);
        goapPlan.ConstructAllNodes();
        //AddPlan(goapPlan, true);
        AddPlan(goapPlan);
        //PlanGoapActions(goapAction);
        //}
        return true;
    }
    private string OtherIdlePlans() {
        string log = GameManager.Instance.TodayLogString() + " IDLE PLAN FOR " + name;
        if (faction.id != FactionManager.Instance.neutralFaction.id) {
            CreatePersonalJobs();
            //NPC with Faction Idle
            log += "\n-" + name + " has a faction";
            if (previousCurrentAction != null && previousCurrentAction.goapType == INTERACTION_TYPE.RETURN_HOME && currentStructure == homeStructure) {
                log += "\n-" + name + " is in home structure and just returned home";
                TileObject deskOrTable = currentStructure.GetUnoccupiedTileObject(TILE_OBJECT_TYPE.DESK, TILE_OBJECT_TYPE.TABLE);
                log += "\n-Sit if there is still an unoccupied Table or Desk in the current location";
                if (deskOrTable != null) {
                    log += "\n  -" + name + " will do action Sit on " + deskOrTable.ToString();
                    PlanIdle(INTERACTION_TYPE.SIT, deskOrTable);
                } else {
                    log += "\n-Otherwise, stand idle";
                    log += "\n  -" + name + " will do action Stand";
                    PlanIdle(INTERACTION_TYPE.STAND, this);
                }
                return log;
            } else if (currentStructure == homeStructure) {
                log += "\n-" + name + " is in home structure and previous action is not returned home";
                TIME_IN_WORDS currentTimeOfDay = GameManager.GetCurrentTimeInWordsOfTick();

                log += "\n-If it is Early Night, 35% chance to go to the current Inn and then set it as the Base Structure for 2.5 hours";
                if (currentTimeOfDay == TIME_IN_WORDS.EARLY_NIGHT) {
                    log += "\n  -Time of Day: " + currentTimeOfDay.ToString();
                    int chance = UnityEngine.Random.Range(0, 100);
                    log += "\n  -RNG roll: " + chance;
                    if (chance < 35) {
                        //StartGOAP(INTERACTION_TYPE.DRINK, null, GOAP_CATEGORY.IDLE);
                        LocationStructure structure = specificLocation.GetRandomStructureOfType(STRUCTURE_TYPE.INN);
                        if(structure != null) {
                            log += "\n  -Early Night: " + name + " will go to Inn and set Base Structure for 2.5 hours";
                            LocationGridTile gridTile = structure.GetRandomTile();
                            marker.GoTo(gridTile, () => trapStructure.SetStructureAndDuration(structure, GameManager.Instance.GetTicksBasedOnHour(2) + GameManager.Instance.GetTicksBasedOnMinutes(30)));
                            return log;
                        } else {
                            log += "\n  -No Inn Structure in the area";
                        }
                    }
                } else {
                    log += "\n  -Time of Day: " + currentTimeOfDay.ToString();
                }
                log += "\n-Otherwise, if it is Afternoon, 25% chance to nap if there is still an unoccupied Bed in the house";
                if (currentTimeOfDay == TIME_IN_WORDS.AFTERNOON) {
                    log += "\n  -Time of Day: " + currentTimeOfDay.ToString();
                    int chance = UnityEngine.Random.Range(0, 100);
                    log += "\n  -RNG roll: " + chance;
                    if (chance < 25) {
                        TileObject bed = currentStructure.GetUnoccupiedTileObject(TILE_OBJECT_TYPE.BED);
                        if (bed != null) {
                            log += "\n  -Afternoon: " + name + " will do action Nap on " + bed.ToString();
                            PlanIdle(INTERACTION_TYPE.NAP, bed);
                            return log;
                        } else {
                            log += "\n  -No unoccupied bed in the current structure";
                        }
                    }
                } else {
                    log += "\n  -Time of Day: " + currentTimeOfDay.ToString();
                }
                log += "\n-Otherwise, if it is Morning or Afternoon or Early Night, 25% chance to enter Stroll Outside Mode for 1 hour";
                if (currentTimeOfDay == TIME_IN_WORDS.MORNING || currentTimeOfDay == TIME_IN_WORDS.AFTERNOON || currentTimeOfDay == TIME_IN_WORDS.EARLY_NIGHT) {
                    log += "\n  -Time of Day: " + currentTimeOfDay.ToString();
                    int chance = UnityEngine.Random.Range(0, 100);
                    log += "\n  -RNG roll: " + chance;
                    if (chance < 25) {
                        log += "\n  -Morning, Afternoon, or Early Night: " + name + " will enter Stroll Outside Mode";
                        PlanIdleStrollOutside(currentStructure);
                        return log;
                    }
                } else {
                    log += "\n  -Time of Day: " + currentTimeOfDay.ToString();
                }
                log += "\n-Otherwise, if it is Morning or Afternoon, 25% chance to someone with a positive relationship in current location and then set it as the Base Structure for 2.5 hours";
                if (currentTimeOfDay == TIME_IN_WORDS.MORNING || currentTimeOfDay == TIME_IN_WORDS.AFTERNOON) {
                    log += "\n  -Time of Day: " + currentTimeOfDay.ToString();
                    int chance = UnityEngine.Random.Range(0, 100);
                    log += "\n  -RNG roll: " + chance;
                    if (chance < 25) {
                        List<Character> positiveCharacters = GetCharactersWithRelationship(TRAIT_EFFECT.POSITIVE);
                        if(positiveCharacters.Count > 0) {
                            Character chosenCharacter = positiveCharacters[UnityEngine.Random.Range(0, positiveCharacters.Count)];
                            if (chosenCharacter.homeStructure != null) {
                                log += "\n  -Morning or Afternoon: " + name + " will go to dwelling of character with positive relationship and set Base Structure for 2.5 hours";
                                LocationGridTile gridTile = chosenCharacter.homeStructure.GetRandomTile();
                                marker.GoTo(gridTile, () => trapStructure.SetStructureAndDuration(chosenCharacter.homeStructure, GameManager.Instance.GetTicksBasedOnHour(2) + GameManager.Instance.GetTicksBasedOnMinutes(30)));
                                return log;
                            } else {
                                log += "\n  -Chosen Character: " + chosenCharacter.name + " has no home structure";
                            }
                        } else {
                            log += "\n  -No character with positive relationship";
                        }
                    }
                } else {
                    log += "\n  -Time of Day: " + currentTimeOfDay.ToString();
                }
                //if (currentTimeOfDay == TIME_IN_WORDS.MORNING || currentTimeOfDay == TIME_IN_WORDS.AFTERNOON) {
                //    int chance = UnityEngine.Random.Range(0, 100);
                //    if (chance < 15) {
                //        log += "\n-Morning or Afternoon: " + name + " will do action Daydream";
                //        PlanIdle(INTERACTION_TYPE.DAYDREAM, this);
                //        return log;
                //    }
                //}
                //int guitarChance = UnityEngine.Random.Range(0, 100);
                //if (guitarChance < 15) {
                //    TileObject guitar = GetUnoccupiedHomeTileObject(TILE_OBJECT_TYPE.GUITAR);
                //    if (guitar != null) {
                //        log += "\n-" + name + " will do action Play Guitar on " + guitar.ToString();
                //        PlanIdle(INTERACTION_TYPE.PLAY_GUITAR, guitar);
                //        return log;
                //    }
                //}

                log += "\n-Otherwise, sit if there is still an unoccupied Table or Desk";
                TileObject deskOrTable = currentStructure.GetUnoccupiedTileObject(TILE_OBJECT_TYPE.DESK, TILE_OBJECT_TYPE.TABLE);
                if (deskOrTable != null) {
                    log += "\n  -" + name + " will do action Sit on " + deskOrTable.ToString();
                    PlanIdle(INTERACTION_TYPE.SIT, deskOrTable);
                    return log;
                } else {
                    log += "\n  -No unoccupied Table or Desk";
                }

                log += "\n-Otherwise, stand idle";
                log += "\n  -" + name + " will do action Stand";
                PlanIdle(INTERACTION_TYPE.STAND, this);
                //PlanIdleStroll(currentStructure);
                return log;
            } else if ((currentStructure.structureType == STRUCTURE_TYPE.WORK_AREA || currentStructure.structureType == STRUCTURE_TYPE.WILDERNESS || currentStructure.structureType == STRUCTURE_TYPE.CEMETERY) && specificLocation == homeArea) {
                log += "\n-" + name + " is in the Work Area/Wilderness/Cemetery of home location";

                log += "\n-If it is Morning or Afternoon, 25% chance to enter Stroll Outside Mode";
                TIME_IN_WORDS currentTimeOfDay = GameManager.GetCurrentTimeInWordsOfTick();
                if (currentTimeOfDay == TIME_IN_WORDS.MORNING || currentTimeOfDay == TIME_IN_WORDS.AFTERNOON) {
                    log += "\n  -Time of Day: " + currentTimeOfDay.ToString();
                    int chance = UnityEngine.Random.Range(0, 100);
                    log += "\n  -RNG roll: " + chance;
                    if (chance < 25) {
                        log += "\n  -Morning or Afternoon: " + name + " will enter Stroll Outside State";
                        PlanIdleStrollOutside(currentStructure);
                        return log;
                    }
                } else {
                    log += "\n  -Time of Day: " + currentTimeOfDay.ToString();
                }
                //log += "\n-Otherwise, if it is Early Night, 35% chance to drink at the Inn";
                //if (currentTimeOfDay == TIME_IN_WORDS.EARLY_NIGHT) {
                //    log += "\n  -Time of Day: " + currentTimeOfDay.ToString();
                //    int chance = UnityEngine.Random.Range(0, 100);
                //    log += "\n  -RNG roll: " + chance;
                //    if (chance < 35) {
                //        log += "\n  -Early Night: " + name + " will do action Drink (multithreaded)";
                //        StartGOAP(INTERACTION_TYPE.DRINK, null, GOAP_CATEGORY.IDLE);
                //        return log;
                //    }
                //} else {
                //    log += "\n  -Time of Day: " + currentTimeOfDay.ToString();
                //}
                log += "\n-Otherwise, return home";
                log += "\n  -" + name + " will do action Return Home";
                PlanIdleReturnHome();
                return log;
            } else if (trapStructure.structure != null && currentStructure == trapStructure.structure) {
                log += "\n-" + name + "'s Base Structure is not empty and current structure is the Base Structure";
                log += "\n-15% chance to trigger a Chat conversation if there is anyone chat-compatible in range";
                int chance = UnityEngine.Random.Range(0, 100);
                log += "\n  -RNG roll: " + chance;
                if (chance < 15) {
                    if(marker.inVisionPOIs.Count > 0) {
                        bool hasForcedChat = false;
                        for (int i = 0; i < marker.inVisionPOIs.Count; i++) {
                            if (marker.inVisionPOIs[i] is Character) {
                                Character targetCharacter = marker.inVisionPOIs[i] as Character;
                                if (marker.visionCollision.ForceChatHandling(targetCharacter)) {
                                    log += "\n  -Chat with: " + targetCharacter.name;
                                    hasForcedChat = true;
                                    break;
                                }
                            }
                        }
                        if (hasForcedChat) {
                            return log;
                        } else {
                            log += "\n  -Could not chat with anyone in vision";
                        }
                    } else {
                        log += "\n  -No characters in vision";
                    }
                }
                log += "\n-Sit if there is still an unoccupied Table or Desk";
                TileObject deskOrTable = currentStructure.GetUnoccupiedTileObject(TILE_OBJECT_TYPE.DESK, TILE_OBJECT_TYPE.TABLE);
                if (deskOrTable != null) {
                    log += "\n  -" + name + " will do action Sit on " + deskOrTable.ToString();
                    PlanIdle(INTERACTION_TYPE.SIT, deskOrTable);
                    return log;
                } else {
                    log += "\n  -No unoccupied Table or Desk";
                }
                log += "\n-Otherwise, stand idle";
                log += "\n  -" + name + " will do action Stand";
                PlanIdle(INTERACTION_TYPE.STAND, this);
                return log;
            } else if (((currentStructure.structureType == STRUCTURE_TYPE.DWELLING && currentStructure != homeStructure) 
                || currentStructure.structureType == STRUCTURE_TYPE.INN 
                || currentStructure.structureType == STRUCTURE_TYPE.WAREHOUSE 
                || currentStructure.structureType == STRUCTURE_TYPE.CEMETERY) 
                && trapStructure.structure == null) {
                log += "\n-" + name + " is in another Dwelling/Inn/Warehouse/Cemetery and Base Structure is empty";
                log += "\n-100% chance to return home";
                PlanIdleReturnHome();
                return log;
            } else if (specificLocation != homeArea && trapStructure.structure == null) {
                log += "\n-" + name + " is in another area and Base Structure is empty";
                log += "\n-100% chance to return home";
                PlanIdleReturnHome();
                return log;
            }
        } else {
            //Unaligned NPC Idle
            log += "\n-" + name + " has no faction";
            if (!isAtHomeArea) {
                log += "\n-" + name + " is in another area";
                log += "\n-100% chance to return home";
                PlanIdleReturnHome();
                return log;
            } else {
                log += "\n-" + name + " is in home area";
                log += "\n-If it is Morning or Afternoon, 25% chance to play";
                TIME_IN_WORDS currentTimeOfDay = GameManager.GetCurrentTimeInWordsOfTick();
                if (currentTimeOfDay == TIME_IN_WORDS.MORNING || currentTimeOfDay == TIME_IN_WORDS.AFTERNOON) {
                    log += "\n  -Time of Day: " + currentTimeOfDay.ToString();
                    int chance = UnityEngine.Random.Range(0, 100);
                    log += "\n  -RNG roll: " + chance;
                    if (chance < 25) {
                        log += "\n  -Morning or Afternoon: " + name + " will do action Play";
                        PlanIdle(INTERACTION_TYPE.PLAY, this);
                        return log;
                    }
                } else {
                    log += "\n  -Time of Day: " + currentTimeOfDay.ToString();
                }

                log += "\n-Otherwise, enter stroll mode";
                log += "\n  -" + name + " will enter Stroll Mode";
                PlanIdleStroll(currentStructure);
                return log;
            }
        }
        return log;

        //if (homeStructure != null) {
        //    if(currentStructure.structureType == STRUCTURE_TYPE.DWELLING) {
        //        if(currentStructure != homeStructure) {
        //            PlanIdleReturnHome();
        //        } else {
        //            PlanIdleStroll(currentStructure);
        //        }
        //    } else {
        //        int chance = UnityEngine.Random.Range(0, 100);
        //        int returnHomeChance = 0;
        //        if (specificLocation == homeArea && currentStructure.structureType == STRUCTURE_TYPE.WORK_AREA) {
        //            returnHomeChance = 25;
        //        } else {
        //            returnHomeChance = 80;
        //        }
        //        if (chance < returnHomeChance) {
        //            PlanIdleReturnHome();
        //        } else {
        //            PlanIdleStroll(currentStructure);
        //        }
        //    }
        //} else {
        //    PlanIdleStroll(currentStructure);
        //}
    }
    private void PlanIdle(INTERACTION_TYPE type, IPointOfInterest target) {
        if (target != this) {
            AddAwareness(target);
        }
        GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(type, this, target);
        GoapNode goalNode = new GoapNode(null, goapAction.cost, goapAction);
        GoapPlan goapPlan = new GoapPlan(goalNode, new GOAP_EFFECT_CONDITION[] { GOAP_EFFECT_CONDITION.NONE }, GOAP_CATEGORY.IDLE);
        goapPlan.ConstructAllNodes();
        AddPlan(goapPlan);
        //PlanGoapActions(goapAction);
    }
    public bool AssaultCharacter(Character target) {
        //Debug.Log(this.name + " will assault " + target.name);
        hasAssaultPlan = true;
        lastAssaultedCharacter = target;
        //Debug.Log("---------" + GameManager.Instance.TodayLogString() + "CREATING IDLE STROLL ACTION FOR " + name + "-------------");
        //if(currentStructure.unoccupiedTiles.Count > 0) {
        GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.ASSAULT_ACTION_NPC, this, target);
        goapAction.SetEndAction(OnAssaultActionReturn);
        //goapAction.SetTargetStructure(target);
        GoapNode goalNode = new GoapNode(null, goapAction.cost, goapAction);
        GoapPlan goapPlan = new GoapPlan(goalNode, new GOAP_EFFECT_CONDITION[] { GOAP_EFFECT_CONDITION.DEATH }, GOAP_CATEGORY.REACTION);
        goapPlan.ConstructAllNodes();
        goapPlan.SetDoNotRecalculate(true);
        AddPlan(goapPlan, true);
        if (currentAction != null) {
            currentAction.StopAction();
        }
        return true;
        //}
        //return false;
    }
    private void SetLastAssaultedCharacter(Character character) {
        lastAssaultedCharacter = character;
        if (character != null) {
            //cooldown
            GameDate dueDate = GameManager.Instance.Today().AddTicks(GameManager.ticksPerHour);
            SchedulingManager.Instance.AddEntry(dueDate, () => RemoveLastAssaultedCharacter(character));
        }
    }
    private void RemoveLastAssaultedCharacter(Character characterToRemove) {
        if (lastAssaultedCharacter == characterToRemove) {
            SetLastAssaultedCharacter(null);
        }
    }
    private void OnAssaultActionReturn(string result, GoapAction action) {
        hasAssaultPlan = false;
        SetLastAssaultedCharacter(action.poiTarget as Character);
        GoapActionResult(result, action);
        //if (action == currentAction) {
        //    SetCurrentAction(null);
        //}
        //if (isDead) {
        //    return;
        //}
        //GoapPlan plan = GetPlanWithAction(action);
        //if (action.isStopped) {
        //    if (!DropPlan(plan)) {
        //        PlanGoapActions();
        //    }
        //    return;
        //}
        //if (plan == null) {
        //    PlanGoapActions();
        //    return;
        //}
        //if (result == InteractionManager.Goap_State_Success) {
        //    plan.SetNextNode();
        //    if (plan.currentNode == null) {
        //        //this means that this is the end goal so end this plan now
        //        if (!DropPlan(plan)) {
        //            PlanGoapActions();
        //        }
        //    } else {
        //        PlanGoapActions();
        //    }
        //} else if (result == InteractionManager.Goap_State_Fail) {
        //    if (plan.endNode.action == action) {
        //        if (!DropPlan(plan)) {
        //            PlanGoapActions();
        //        }
        //    } else {
        //        if (plan.doNotRecalculate) {
        //            if (!DropPlan(plan)) {
        //                PlanGoapActions();
        //            }
        //        } else {
        //            RecalculatePlan(plan);
        //        }
        //    }
        //}
    }
    public void ChatCharacter(Character targetCharacter) {
        ChatCharacter chatAction = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.CHAT_CHARACTER, this, targetCharacter) as ChatCharacter;
        chatAction.PerformActualAction();
    }
    public float GetFlirtationWeightWith(Character targetCharacter, CharacterRelationshipData relData, params CHARACTER_MOOD[] moods) {
        float positiveFlirtationWeight = 0;
        float negativeFlirtationWeight = 0;
        for (int i = 0; i < moods.Length; i++) {
            CHARACTER_MOOD mood = moods[i];
            switch (mood) {
                case CHARACTER_MOOD.DARK:
                    //-100 Weight per Dark Mood
                    negativeFlirtationWeight -= 100;
                    break;
                case CHARACTER_MOOD.BAD:
                    //-50 Weight per Bad Mood
                    negativeFlirtationWeight -= 50;
                    break;
                case CHARACTER_MOOD.GOOD:
                    //+10 Weight per Good Mood
                    positiveFlirtationWeight += 10;
                    break;
                case CHARACTER_MOOD.GREAT:
                    //+30 Weight per Great Mood
                    positiveFlirtationWeight += 30;
                    break;
            }
        }
        if (relData != null) {
            //+10 Weight per previous flirtation
            positiveFlirtationWeight += 10 * relData.flirtationCount;
        }
        //x2 all positive modifiers per Drunk
        if (GetTrait("Drunk") != null) {
            positiveFlirtationWeight *= 2;
        }
        if (targetCharacter.GetTrait("Drunk") != null) {
            positiveFlirtationWeight *= 2;
        }

        //x0.5 all positive modifiers per negative relationship
        if (GetRelationshipEffectWith(targetCharacter) == RELATIONSHIP_EFFECT.NEGATIVE) {
            positiveFlirtationWeight *= 0.5f;
        }
        if (targetCharacter.GetRelationshipEffectWith(this) == RELATIONSHIP_EFFECT.NEGATIVE) {
            positiveFlirtationWeight *= 0.5f;
        }
        //x0.1 all positive modifiers per sexually incompatible
        if (!CharacterManager.Instance.IsSexuallyCompatibleOneSided(this, targetCharacter)) {
            positiveFlirtationWeight *= 0.1f;
        } 
        // x6 if initiator is Unfaithful and already has a lover
        else if ((GetTrait("Unfaithful") != null) && (relData == null || !relData.HasRelationshipTrait(RELATIONSHIP_TRAIT.LOVER))) {
            positiveFlirtationWeight *= 6f;
        }
        if (!CharacterManager.Instance.IsSexuallyCompatibleOneSided(targetCharacter, this)) {
            positiveFlirtationWeight *= 0.1f;
        }
        return positiveFlirtationWeight + negativeFlirtationWeight;
    }
    public float GetBecomeLoversWeightWith(Character targetCharacter, CharacterRelationshipData relData, params CHARACTER_MOOD[] moods) {
        float positiveWeight = 0;
        float negativeWeight = 0;
        if (GetRelationshipEffectWith(targetCharacter) != RELATIONSHIP_EFFECT.NEGATIVE && targetCharacter.GetRelationshipEffectWith(targetCharacter) != RELATIONSHIP_EFFECT.NEGATIVE
            && CanHaveRelationshipWith(RELATIONSHIP_TRAIT.LOVER, targetCharacter) && targetCharacter.CanHaveRelationshipWith(RELATIONSHIP_TRAIT.LOVER, this)
            && role.roleType != CHARACTER_ROLE.BEAST && targetCharacter.role.roleType != CHARACTER_ROLE.BEAST) {
            for (int i = 0; i < moods.Length; i++) {
                CHARACTER_MOOD mood = moods[i];
                switch (mood) {
                    case CHARACTER_MOOD.DARK:
                        //-30 Weight per Dark Mood
                        negativeWeight -= 30;
                        break;
                    case CHARACTER_MOOD.BAD:
                        //-10 Weight per Bad Mood
                        negativeWeight -= 10;
                        break;
                    case CHARACTER_MOOD.GOOD:
                        //+5 Weight per Good Mood
                        positiveWeight += 5;
                        break;
                    case CHARACTER_MOOD.GREAT:
                        //+10 Weight per Great Mood
                        positiveWeight += 10;
                        break;
                }
            }
            if (relData != null) {
                //+30 Weight per previous flirtation
                positiveWeight += 30 * relData.flirtationCount;
            }
            //x2 all positive modifiers per Drunk
            if (GetTrait("Drunk") != null) {
                positiveWeight *= 2;
            }
            if (targetCharacter.GetTrait("Drunk") != null) {
                positiveWeight *= 2;
            }
            //x0.1 all positive modifiers per sexually incompatible
            if (!CharacterManager.Instance.IsSexuallyCompatibleOneSided(this, targetCharacter)) {
                positiveWeight *= 0.1f;
            }
            if (!CharacterManager.Instance.IsSexuallyCompatibleOneSided(targetCharacter, this)) {
                positiveWeight *= 0.1f;
            }
            //x0 if a character is a beast
            //added to initial checking instead.
        }
        return positiveWeight + negativeWeight;
    }
    public float GetBecomeParamoursWeightWith(Character targetCharacter, CharacterRelationshipData relData, params CHARACTER_MOOD[] moods) {
        //**if they dont have a negative relationship and at least one of them has a lover, they may become paramours**
        float positiveWeight = 0;
        float negativeWeight = 0;
        if (GetRelationshipEffectWith(targetCharacter) != RELATIONSHIP_EFFECT.NEGATIVE && targetCharacter.GetRelationshipEffectWith(this) != RELATIONSHIP_EFFECT.NEGATIVE
            && CanHaveRelationshipWith(RELATIONSHIP_TRAIT.PARAMOUR, targetCharacter) && targetCharacter.CanHaveRelationshipWith(RELATIONSHIP_TRAIT.PARAMOUR, this)
            && role.roleType != CHARACTER_ROLE.BEAST && targetCharacter.role.roleType != CHARACTER_ROLE.BEAST) {
            for (int i = 0; i < moods.Length; i++) {
                CHARACTER_MOOD mood = moods[i];
                switch (mood) {
                    case CHARACTER_MOOD.DARK:
                        //-30 Weight per Dark Mood
                        negativeWeight -= 30;
                        break;
                    case CHARACTER_MOOD.BAD:
                        //-10 Weight per Bad Mood
                        negativeWeight -= 10;
                        break;
                    case CHARACTER_MOOD.GOOD:
                        //+5 Weight per Good Mood
                        positiveWeight += 5;
                        break;
                    case CHARACTER_MOOD.GREAT:
                        //+10 Weight per Great Mood
                        positiveWeight += 20;
                        break;
                }
            }
            if (relData != null) {
                //+30 Weight per previous flirtation
                positiveWeight += 50 * relData.flirtationCount;
            }
            //x2 all positive modifiers per Drunk
            if (GetTrait("Drunk") != null) {
                positiveWeight *= 2.5f;
            }
            if (targetCharacter.GetTrait("Drunk") != null) {
                positiveWeight *= 2.5f;
            }
            //x0.1 all positive modifiers per sexually incompatible
            if (!CharacterManager.Instance.IsSexuallyCompatibleOneSided(this, targetCharacter)) {
                positiveWeight *= 0.1f;
            }
            // x4 if initiator is Unfaithful and already has a lover
            else if ((GetTrait("Unfaithful") != null) && (relData == null || !relData.HasRelationshipTrait(RELATIONSHIP_TRAIT.LOVER)))
            {
                positiveWeight *= 4f;
            }

            if (!CharacterManager.Instance.IsSexuallyCompatibleOneSided(targetCharacter, this)) {
                positiveWeight *= 0.1f;
            }
            Character lover = GetCharacterWithRelationship(RELATIONSHIP_TRAIT.LOVER);
            //x3 all positive modifiers if character considers lover as Enemy
            if (lover != null && HasRelationshipOfTypeWith(lover, RELATIONSHIP_TRAIT.ENEMY)) {
                positiveWeight *= 3f;
            }
            if (HasRelationshipOfTypeWith(targetCharacter, RELATIONSHIP_TRAIT.RELATIVE)) {
                positiveWeight *= 0.01f;
            }
            //x0 if a character has a lover and does not have the Unfaithful trait
            if ((HasRelationshipTraitOf(RELATIONSHIP_TRAIT.LOVER, false) && GetTrait("Unfaithful") == null) 
                || (targetCharacter.HasRelationshipTraitOf(RELATIONSHIP_TRAIT.LOVER, false) && targetCharacter.GetTrait("Unfaithful") == null)) {
                positiveWeight *= 0;
                negativeWeight *= 0;
            }
            //x0 if a character is a beast
            //added to initial checking instead.
        }
        return positiveWeight + negativeWeight;
    }
    public void EndChatCharacter(Character targetCharacter) {
        SetIsChatting(false);
        targetCharacter.SetIsChatting(false);
        marker.UpdateActionIcon();
        targetCharacter.marker.UpdateActionIcon();
    }
    public void SetIsChatting(bool state) {
        _isChatting = state;
    }
    public void ClaimReward(Reward reward) {
        switch (reward.rewardType) {
            case REWARD.LEVEL:
                LevelUp(reward.amount);
                break;
            case REWARD.SUPPLY:
                if (minion != null) {
                    PlayerManager.Instance.player.AdjustCurrency(CURRENCY.SUPPLY, reward.amount);
                } else {
                    homeArea.AdjustSuppliesInBank(reward.amount);
                }
                break;
            default:
                break;
        }
    }
    public bool HasPlanWithType(INTERACTION_TYPE type) {
        for (int i = 0; i < allGoapPlans.Count; i++) {
            GoapPlan currPlan = allGoapPlans[i];
            if (currPlan.endNode.action != null && currPlan.endNode.action.goapType == type) {
                return true;
            }
        }
        return false;
    }
    /// <summary>
    /// This should only be used for plans that come/constructed from the outside.
    /// </summary>
    /// <param name="plan">Plan to be added</param>
    public void AddPlan(GoapPlan plan, bool isPriority = false) {
        if (!allGoapPlans.Contains(plan)) {
            plan.SetPriorityState(isPriority);
            if (isPriority) {
                //if the plan is a priority, place it after all other plans that are not overridable
                int indexToInsert = 0;
                for (int i = 0; i < allGoapPlans.Count; i++) {
                    GoapPlan currPlan = allGoapPlans[i];
                    if (!(currPlan.job != null && currPlan.job.cannotOverrideJob)) {
                        indexToInsert = i; //this is the first plan in the list that is overridable, place the priority plan before the current item.
                        break;
                    }
                }
                allGoapPlans.Insert(indexToInsert, plan);
            } else {
                allGoapPlans.Add(plan);
            }
            //If a character is strolling or idly returning home and a plan is added to this character, end the action/state
            if (stateComponent.currentState != null && (stateComponent.currentState.characterState == CHARACTER_STATE.STROLL 
                || stateComponent.currentState.characterState == CHARACTER_STATE.STROLL_OUTSIDE 
                || stateComponent.currentState.characterState == CHARACTER_STATE.PATROL)) {
                stateComponent.currentState.OnExitThisState();
            } else if (currentAction != null && currentAction.goapType == INTERACTION_TYPE.RETURN_HOME) {
                if (currentAction.parentPlan == null || currentAction.parentPlan.category == GOAP_CATEGORY.IDLE) {
                    currentAction.StopAction();
                }
            }
        }
    }
    #endregion

    #region Token Inventory
    public bool ObtainTokenFrom(Character target, SpecialToken token, bool changeCharacterOwnership = true) {
        if (target.UnobtainToken(token)) {
            ObtainToken(token, changeCharacterOwnership);
            return true;
        }
        return false;
    }
    public bool ObtainToken(SpecialToken token, bool changeCharacterOwnership = true) {
        if (AddToken(token)) {
            token.SetOwner(this.faction);
            token.OnObtainToken(this);
            if (changeCharacterOwnership) {
                token.SetCharacterOwner(this);
            } else {
                if (token.characterOwner == null) {
                    token.SetCharacterOwner(this);
                }
            }
            return true;
        }
        return false;
        //token.AdjustQuantity(-1);
    }
    public bool UnobtainToken(SpecialToken token) {
        if (RemoveToken(token)) {
            token.OnUnobtainToken(this);
            return true;
        }
        return false;
    }
    public bool ConsumeToken(SpecialToken token) {
        if (RemoveToken(token)) {
            token.OnConsumeToken(this);
            return true;
        }
        return false;
    }
    private bool AddToken(SpecialToken token) {
        if (!items.Contains(token)) {
            items.Add(token);
            Messenger.Broadcast(Signals.CHARACTER_OBTAINED_ITEM, token, this);
            return true;
        }
        return false;
    }
    private bool RemoveToken(SpecialToken token) {
        if (items.Remove(token)) {
            Messenger.Broadcast(Signals.CHARACTER_LOST_ITEM, token, this);
            return true;
        }
        return false;
    }
    public void DropToken(SpecialToken token, Area location, LocationStructure structure, LocationGridTile gridTile = null, bool clearOwner = true) {
        if (UnobtainToken(token)) {
            if(location.AddSpecialTokenToLocation(token, structure, gridTile)) {
                //When items are dropped into the warehouse, make all residents aware of it.
                if (structure.structureType == STRUCTURE_TYPE.WAREHOUSE) {
                    for (int i = 0; i < structure.location.areaResidents.Count; i++) {
                        Character resident = structure.location.areaResidents[i];
                        resident.AddAwareness(token);
                    }
                }
            }
            //if (structure != homeStructure) {
            //    //if this character drops this at a structure that is not his/her home structure, set the owner of the item to null
            //    token.SetCharacterOwner(null);
            //}
            if (clearOwner) {
                token.SetCharacterOwner(null);
            }
        }
    }
    public void DropAllTokens(Area location, LocationStructure structure, bool removeFactionOwner = false) {
        while (isHoldingItem) {
            SpecialToken token = items[0];
            if (UnobtainToken(token)) {
                if (removeFactionOwner) {
                    token.SetOwner(null);
                }
                location.AddSpecialTokenToLocation(token, structure);
                if (structure != homeStructure) {
                    //if this character drops this at a structure that is not his/her home structure, set the owner of the item to null
                    token.SetCharacterOwner(null);
                }
            }
        }
    }
    public void PickUpToken(SpecialToken token, bool changeCharacterOwnership = true) {
        if (ObtainToken(token, changeCharacterOwnership)) {
            token.gridTileLocation.structure.location.RemoveSpecialTokenFromLocation(token);
        }
    }
    //public void PickUpRandomToken(Area location) {
    //    WeightedDictionary<SpecialToken> pickWeights = new WeightedDictionary<SpecialToken>();
    //    for (int i = 0; i < location.possibleSpecialTokenSpawns.Count; i++) {
    //        SpecialToken token = location.possibleSpecialTokenSpawns[i];
    //        if (token.npcAssociatedInteractionType != INTERACTION_TYPE.USE_ITEM_ON_SELF) {
    //            pickWeights.AddElement(token, 60);
    //        } else if (token.CanBeUsedBy(this)) {
    //            pickWeights.AddElement(token, 100);
    //        }
    //    }
    //    if (pickWeights.Count > 0) {
    //        SpecialToken chosenToken = pickWeights.PickRandomElementGivenWeights();
    //        PickUpToken(chosenToken);
    //    }
    //}
    public void DestroyToken(SpecialToken token) {
        token.gridTileLocation.structure.location.RemoveSpecialTokenFromLocation(token);
    }
    private void UpdateTokenOwner() {
        for (int i = 0; i < items.Count; i++) {
            SpecialToken token = items[i];
            token.SetOwner(this.faction);
        }
    }
    public SpecialToken GetToken(SpecialToken token) {
        for (int i = 0; i < items.Count; i++) {
            if (items[i] == token) {
                return items[i];
            }
        }
        return null;
    }
    public SpecialToken GetToken(SPECIAL_TOKEN token) {
        for (int i = 0; i < items.Count; i++) {
            if (items[i].specialTokenType == token) {
                return items[i];
            }
        }
        return null;
    }
    public SpecialToken GetToken(string tokenName) {
        for (int i = 0; i < items.Count; i++) {
            if (items[i].tokenName.ToLower() == tokenName.ToLower()) {
                return items[i];
            }
        }
        return null;
    }
    public List<SpecialToken> GetItemsOwned() {
        List<SpecialToken> itemsOwned = new List<SpecialToken>();
        for (int i = 0; i < homeArea.possibleSpecialTokenSpawns.Count; i++) {
            SpecialToken token = homeArea.possibleSpecialTokenSpawns[i];
            if (token.characterOwner == this) {
                itemsOwned.Add(token);
            }
        }
        for (int i = 0; i < items.Count; i++) {
            SpecialToken token = items[i];
            if (token.characterOwner == this) {
                itemsOwned.Add(token);
            }
        }
        return itemsOwned;
    }
    public int GetNumOfItemsOwned() {
        int count = 0;
        for (int i = 0; i < homeArea.possibleSpecialTokenSpawns.Count; i++) {
            SpecialToken token = homeArea.possibleSpecialTokenSpawns[i];
            if (token.characterOwner == this) {
                count++;
            }
        }
        for (int i = 0; i < items.Count; i++) {
            SpecialToken token = items[i];
            if (token.characterOwner == this) {
                count++;
            }
        }
        return count;
    }
    public bool HasToken(SPECIAL_TOKEN tokenType) {
        for (int i = 0; i < items.Count; i++) {
            if (items[i].specialTokenType == tokenType) {
                return true;
            }
        }
        return false;
    }
    #endregion

    #region Needs
    private void DecreaseNeeds() {
        if (race == RACE.SKELETON) {
            return;
        }
        if (_doNotGetHungry <= 0) {
            AdjustFullness(-10);
        }
        if (_doNotGetTired <= 0) {
            AdjustTiredness(-10);
        }
        if (_doNotGetLonely <= 0) {
            AdjustHappiness(-13);
        }
    }
    public string GetNeedsSummary() {
        string summary = "Fullness: " + fullness.ToString() + "/" + FULLNESS_DEFAULT.ToString();
        summary += "\nTiredness: " + tiredness + "/" + TIREDNESS_DEFAULT.ToString();
        summary += "\nHappiness: " + happiness + "/" + HAPPINESS_DEFAULT.ToString();
        return summary;
    }
    #endregion

    #region Tiredness
    public void ResetTirednessMeter() {
        tiredness = TIREDNESS_DEFAULT;
        RemoveTiredOrExhausted();
    }
    public void AdjustTiredness(int adjustment) {
        tiredness += adjustment;
        tiredness = Mathf.Clamp(tiredness, 0, TIREDNESS_DEFAULT);
        if (tiredness == 0) {
            Death("exhaustion");
        } else if (isExhausted) {
            RemoveTrait("Tired");
            if (AddTrait("Exhausted")) {
                RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "add_trait", null, "exhausted");
            }
            //PlanTirednessRecoveryActions();
        } else if (tiredness <= TIREDNESS_THRESHOLD_1) {
            RemoveTrait("Exhausted");
            if (AddTrait("Tired")) {
                RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "add_trait", null, "tired");
            }
            //PlanTirednessRecoveryActions();
        } else {
            //tiredness is higher than both thresholds
            RemoveTiredOrExhausted();
        }
    }
    public void DecreaseTirednessMeter() { //this is used for when tiredness is only decreased by 1 (I did this for optimization, so as not to check for traits everytime)
        tiredness -= 1;
        tiredness = Mathf.Clamp(tiredness, 0, TIREDNESS_DEFAULT);
        if (tiredness == 0) {
            Death("exhaustion");
        } else if (isExhausted) {
            if (tiredness == TIREDNESS_THRESHOLD_2) {
                RemoveTrait("Tired");
                if (AddTrait("Exhausted")) {
                    RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "add_trait", null, "exhausted");
                }
            }
            //PlanTirednessRecoveryActions();
        } else if (tiredness <= TIREDNESS_THRESHOLD_1) {
            if (tiredness == TIREDNESS_THRESHOLD_1) {
                if (AddTrait("Tired")) {
                    RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "add_trait", null, "tired");
                }
            }
            //PlanTirednessRecoveryActions();
        }
    }
    public void SetTiredness(int amount) {
        tiredness = amount;
        tiredness = Mathf.Clamp(tiredness, 0, TIREDNESS_DEFAULT);
        if (tiredness == 0) {
            Death("exhaustion");
        } else if (isExhausted) {
            RemoveTrait("Tired");
            if (AddTrait("Exhausted")) {
                RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "add_trait", null, "exhausted");
            }
            //PlanTirednessRecoveryActions();
        } else if (tiredness <= TIREDNESS_THRESHOLD_1) {
            RemoveTrait("Exhausted");
            if (AddTrait("Tired")) {
                RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "add_trait", null, "tired");
            }
            //PlanTirednessRecoveryActions();
        } else {
            //tiredness is higher than both thresholds
            RemoveTiredOrExhausted();
        }
    }
    private void RemoveTiredOrExhausted() {
        if (!RemoveTrait("Tired")) {
            if (RemoveTrait("Exhausted")) {
                RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "remove_trait", null, "exhausted");
            }
        } else {
            RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "remove_trait", null, "tired");
        }
    }
    #endregion

    #region Fullness
    public void ResetFullnessMeter() {
        fullness = FULLNESS_DEFAULT;
        RemoveHungryOrStarving();
    }
    public void AdjustFullness(int adjustment) {
        fullness += adjustment;
        fullness = Mathf.Clamp(fullness, 0, FULLNESS_DEFAULT);
        if (fullness == 0) {
            Death("starvation");
        } else if (isStarving) {
            RemoveTrait("Hungry");
            if (AddTrait("Starving")) {
                RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "add_trait", null, "starving");
            }
            //PlanFullnessRecoveryActions();
        } else if (fullness <= FULLNESS_THRESHOLD_1) {
            RemoveTrait("Starving");
            if (AddTrait("Hungry")) {
                RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "add_trait", null, "hungry");
            }
            //PlanFullnessRecoveryActions();
        } else {
            //fullness is higher than both thresholds
            RemoveHungryOrStarving();
        }
    }
    public void DecreaseFullnessMeter() { //this is used for when fullness is only decreased by 1 (I did this for optimization, so as not to check for traits everytime)
        fullness -= 1;
        fullness = Mathf.Clamp(fullness, 0, FULLNESS_DEFAULT);
        if (fullness == 0) {
            Death("starvation");
        } else if (isStarving) {
            if (fullness == FULLNESS_THRESHOLD_2) {
                RemoveTrait("Hungry");
                if (AddTrait("Starving")) {
                    RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "add_trait", null, "starving");
                }
            }
            //PlanFullnessRecoveryActions();
        } else if (fullness <= FULLNESS_THRESHOLD_1) {
            if (fullness == FULLNESS_THRESHOLD_1) {
                if (AddTrait("Hungry")) {
                    RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "add_trait", null, "hungry");
                }
            }
            //PlanFullnessRecoveryActions();
        }
    }
    public void SetFullness(int amount) {
        fullness = amount;
        fullness = Mathf.Clamp(fullness, 0, FULLNESS_DEFAULT);
        if (fullness == 0) {
            Death("starvation");
        } else if (isStarving) {
            RemoveTrait("Hungry");
            if (AddTrait("Starving")) {
                RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "add_trait", null, "starving");
            }
            //PlanFullnessRecoveryActions();
        } else if (fullness <= FULLNESS_THRESHOLD_1) {
            RemoveTrait("Starving");
            if (AddTrait("Hungry")) {
                RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "add_trait", null, "hungry");
            }
            //PlanFullnessRecoveryActions();
        } else {
            //fullness is higher than both thresholds
            RemoveHungryOrStarving();
        }
    }
    private void RemoveHungryOrStarving() {
        if (!RemoveTrait("Hungry")) {
            if (RemoveTrait("Starving")) {
                RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "remove_trait", null, "starving");
            }
        } else {
            RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "remove_trait", null, "hungry");
        }
    }
    #endregion

    #region Happiness
    public void ResetHappinessMeter() {
        happiness = HAPPINESS_DEFAULT;
        RemoveLonelyOrForlorn();
    }
    public void AdjustHappiness(int adjustment) {
        happiness += adjustment;
        happiness = Mathf.Clamp(happiness, 0, HAPPINESS_DEFAULT);
        if (isForlorn) {
            RemoveTrait("Lonely");
            if (AddTrait("Forlorn")) {
                RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "add_trait", null, "forlorn");
            }
            //PlanHappinessRecoveryActions();
        } else if (happiness <= HAPPINESS_THRESHOLD_1) {
            RemoveTrait("Forlorn");
            if (AddTrait("Lonely")) {
                RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "add_trait", null, "lonely");
            }
            //PlanHappinessRecoveryActions();
        } else {
            RemoveLonelyOrForlorn();
        }
    }
    public void SetHappiness(int amount) {
        happiness = amount;
        happiness = Mathf.Clamp(happiness, 0, HAPPINESS_DEFAULT);
        if (isForlorn) {
            RemoveTrait("Lonely");
            if (AddTrait("Forlorn")) {
                RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "add_trait", null, "depressed");
            }
            //PlanHappinessRecoveryActions();
        } else if (happiness <= HAPPINESS_THRESHOLD_1) {
            RemoveTrait("Forlorn");
            if (AddTrait("Lonely")) {
                RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "add_trait", null, "lonely");
            }
            //PlanHappinessRecoveryActions();
        } else {
            RemoveLonelyOrForlorn();
        }
    }
    private void RemoveLonelyOrForlorn() {
        if (!RemoveTrait("Lonely")) {
            if (RemoveTrait("Forlorn")) {
                RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "remove_trait", null, "depressed");
            }
        } else {
            RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "remove_trait", null, "lonely");
        }
    }
    #endregion

    #region Share Intel
    public List<string> ShareIntel(Intel intel) {
        List<string> dialogReactions = new List<string>();
        if (intel is TileObjectIntel) {
            TileObjectIntel toi = intel as TileObjectIntel;
            if (GetAwareness(toi.obj) != null) {
                dialogReactions.Add("I already know about that.");
            } else {
                AddAwareness(toi.obj);
                dialogReactions.Add("There is an " + toi.obj.name + " in " + toi.knownLocation.structure.location.name + "? Thanks for letting me know.");
            }
        } else if (intel is EventIntel) {
            EventIntel ei = intel as EventIntel;
            if (ei.action.endedAtState != null && ei.action.endedAtState.shareIntelReaction != null) {
                dialogReactions.AddRange(ei.action.endedAtState.shareIntelReaction.Invoke(this, ei));
            }
            //if the determined reactions list is empty, check the default reactions
            if (dialogReactions.Count == 0) {
                bool doesNotConcernMe = false;
                //If the event's actor and target do not have any relationship with the recipient and are not part of his faction, 
                //and if no item involved is owned by the recipient: "This does not concern me."
                if (!this.HasRelationshipWith(ei.action.actor)
                    && ei.action.actor.faction != this.faction) {
                    if (ei.action.poiTarget is Character) {
                        Character otherCharacter = ei.action.poiTarget as Character;
                        if (!this.HasRelationshipWith(otherCharacter)
                            && otherCharacter.faction != this.faction) {
                            doesNotConcernMe = true;
                        }
                    } else if (ei.action.poiTarget is TileObject) {
                        TileObject obj = ei.action.poiTarget as TileObject;
                        if (!obj.IsOwnedBy(this)) {
                            doesNotConcernMe = true;
                        }
                    } else if (ei.action.poiTarget is SpecialToken) {
                        SpecialToken obj = ei.action.poiTarget as SpecialToken;
                        if (obj.characterOwner != this) {
                            doesNotConcernMe = true;
                        }
                    }
                }

                if (ei.action.actor == this) {
                    //If the actor and the recipient is the same: "I know what I did."
                    dialogReactions.Add("I know what I did.");
                } else {
                    if (doesNotConcernMe) {
                        //The following events are too unimportant to merit any meaningful response: "What will I do with this random tidbit?"
                        //-character picked up an item(not stealing)
                        //-character prayed, daydreamed, played
                        //- character slept
                        //- character mined or chopped wood
                        switch (ei.action.goapType) {
                            case INTERACTION_TYPE.PICK_ITEM:
                            case INTERACTION_TYPE.PRAY:
                            case INTERACTION_TYPE.DAYDREAM:
                            case INTERACTION_TYPE.PLAY:
                            case INTERACTION_TYPE.SLEEP:
                            case INTERACTION_TYPE.SLEEP_OUTSIDE:
                            case INTERACTION_TYPE.MINE_ACTION:
                            case INTERACTION_TYPE.CHOP_WOOD:
                                dialogReactions.Add("What will I do with this random tidbit?");
                                break;
                            default:
                                dialogReactions.Add("This does not concern me.");
                                break;
                        }

                    } else {
                        //Otherwise: "A proper response to this information has not been implemented yet."
                        dialogReactions.Add("A proper response to this information has not been implemented yet.");
                    }
                }
            }
            CreateInformedEventLog(intel.intelLog.goapAction);
        }
        PlayerManager.Instance.player.RemoveIntel(intel);
        return dialogReactions;
        //if (relationships.ContainsKey(intel.actor)) {
        //    if (!intel.isCompleted) {
        //        relationships[intel.actor].SetPlannedActionIntel(intel);
        //    } else {
        //        Debug.Log(GameManager.Instance.TodayLogString() + "The intel given to " + this.name + " regarding " + intel.actor.name + " has already been completed, not setting planned action...");
        //    }
        //    relationships[intel.actor].OnIntelGivenToCharacter(intel);
        //    PlayerManager.Instance.player.RemoveIntel(intel);
        //} else {
        //    Debug.Log(GameManager.Instance.TodayLogString() + this.name + " does not have a relationship with " + intel.actor.name + ". He/she doesn't care about any intel you give that is about " + intel.actor.name);
        //}
        //if (intel.target is Character) {
        //    Character target = intel.target as Character;
        //    if (relationships.ContainsKey(target)) {
        //        relationships[target].OnIntelGivenToCharacter(intel);
        //        PlayerManager.Instance.player.RemoveIntel(intel);
        //    }
        //}

    }
    #endregion

    #region Awareness
    public IAwareness AddAwareness(IPointOfInterest pointOfInterest) {
        return currentAlterEgo.AddAwareness(pointOfInterest);
        //IAwareness iawareness = GetAwareness(pointOfInterest);
        //if (iawareness == null) {
        //    iawareness = CreateNewAwareness(pointOfInterest);
        //    if (iawareness != null) {
        //        if (!awareness.ContainsKey(pointOfInterest.poiType)) {
        //            awareness.Add(pointOfInterest.poiType, new List<IAwareness>());
        //        }
        //        awareness[pointOfInterest.poiType].Add(iawareness);
        //        iawareness.OnAddAwareness(this);
        //    }
        //} else {
        //    if (pointOfInterest.gridTileLocation != null) {
        //        //if already has awareness for that poi, just update it's known location. 
        //        //Except if it's null, because if it's null, it ususally means the poi is travelling 
        //        //and setting it's location to null should be ignored to prevent unexpected behaviour.
        //        iawareness.SetKnownGridLocation(pointOfInterest.gridTileLocation);
        //    }

        //}
        //return iawareness;
    }
    public void RemoveAwareness(IPointOfInterest pointOfInterest) {
        currentAlterEgo.RemoveAwareness(pointOfInterest);
        //if (awareness.ContainsKey(pointOfInterest.poiType)) {
        //    List<IAwareness> awarenesses = awareness[pointOfInterest.poiType];
        //    for (int i = 0; i < awarenesses.Count; i++) {
        //        IAwareness iawareness = awarenesses[i];
        //        if (iawareness.poi == pointOfInterest) {
        //            awarenesses.RemoveAt(i);
        //            iawareness.OnRemoveAwareness(this);
        //            break;
        //        }
        //    }
        //}
    }
    public IAwareness GetAwareness(IPointOfInterest poi) {
        return currentAlterEgo.GetAwareness(poi);
        //if (awareness.ContainsKey(poi.poiType)) {
        //    List<IAwareness> awarenesses = awareness[poi.poiType];
        //    for (int i = 0; i < awarenesses.Count; i++) {
        //        IAwareness iawareness = awarenesses[i];
        //        if (iawareness.poi == poi) {
        //            return iawareness;
        //        }
        //    }
        //    return null;
        //}
        //return null;
    }
    //private IAwareness CreateNewAwareness(IPointOfInterest poi) {
    //    if (poi.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
    //        return new CharacterAwareness(poi as Character);
    //    } else if (poi.poiType == POINT_OF_INTEREST_TYPE.ITEM) {
    //        return new ItemAwareness(poi as SpecialToken);
    //    } else if (poi.poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
    //        return new TileObjectAwareness(poi);
    //    }//TODO: Structure Awareness
    //    return null;
    //}
    public void AddInitialAwareness() {
        AddAwareness(this);
        if (faction == FactionManager.Instance.neutralFaction) {
            foreach (List<LocationStructure> structures in specificLocation.structures.Values) {
                for (int i = 0; i < structures.Count; i++) {
                    for (int j = 0; j < structures[i].pointsOfInterest.Count; j++) {
                        IPointOfInterest poi = structures[i].pointsOfInterest[j];
                        if (poi != this) {
                            AddAwareness(poi);
                        }
                    }
                }
            }
        } else {
            foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> keyValuePair in specificLocation.structures) {
                for (int i = 0; i < keyValuePair.Value.Count; i++) {
                    LocationStructure structure = keyValuePair.Value[i];
                    for (int j = 0; j < structure.pointsOfInterest.Count; j++) {
                        IPointOfInterest poi = structure.pointsOfInterest[j];
                        if (poi != this && !(poi is Tree)) {
                            AddAwareness(poi);
                        }
                    }
                }
            }
        }
    }
    public void LogAwarenessList() {
        string log = "--------------AWARENESS LIST OF " + name + "-----------------";
        foreach (KeyValuePair<POINT_OF_INTEREST_TYPE, List<IAwareness>> kvp in awareness) {
            log += "\n" + kvp.Key.ToString() + ": ";
            for (int i = 0; i < kvp.Value.Count; i++) {
                if (i > 0) {
                    log += ", ";
                }
                log += kvp.Value[i].poi.name;
            }
        }
        Debug.Log(log);
    }
    public void CopyAwareness(Dictionary<POINT_OF_INTEREST_TYPE, List<IAwareness>> newAwareness) {
        currentAlterEgo.SetAwareness(newAwareness);
    }
    #endregion

    #region Point Of Interest
    public List<GoapAction> AdvertiseActionsToActor(Character actor, List<INTERACTION_TYPE> actorAllowedInteractions) {
        if (poiGoapActions != null && poiGoapActions.Count > 0 && IsAvailable() && !isDead) {
            List<GoapAction> usableActions = new List<GoapAction>();
            for (int i = 0; i < poiGoapActions.Count; i++) {
                INTERACTION_TYPE currType = poiGoapActions[i];
                if (actorAllowedInteractions.Contains(currType)) {
                    if (currType == INTERACTION_TYPE.CRAFT_ITEM) {
                        Craftsman craftsman = GetTrait("Craftsman") as Craftsman;
                        for (int j = 0; j < craftsman.craftedItemNames.Length; j++) {
                            CraftItemGoap goapAction = InteractionManager.Instance.CreateNewGoapInteraction(currType, actor, this, false) as CraftItemGoap;
                            goapAction.SetCraftedItem(craftsman.craftedItemNames[j]);
                            goapAction.Initialize();
                            if (goapAction.CanSatisfyRequirements() && goapAction.CanSatisfyRequirementOnBuildGoapTree()) {
                                usableActions.Add(goapAction);
                            }
                        }
                    } else {
                        GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(currType, actor, this);
                        if (goapAction == null) {
                            throw new Exception("Goap action " + currType.ToString() + " is null!");
                        }
                        if (goapAction.CanSatisfyRequirements() && goapAction.CanSatisfyRequirementOnBuildGoapTree()) {
                            usableActions.Add(goapAction);
                        }
                    }
                }
            }
            return usableActions;
        }
        return null;
    }
    public List<GoapAction> AdvertiseActionsToActorFromDeadCharacter(Character actor, List<INTERACTION_TYPE> actorAllowedInteractions) {
        if (poiGoapActions != null && poiGoapActions.Count > 0) {
            List<GoapAction> usableActions = new List<GoapAction>();
            for (int i = 0; i < poiGoapActions.Count; i++) {
                INTERACTION_TYPE currType = poiGoapActions[i];
                if (actorAllowedInteractions.Contains(currType)) {
                    GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(currType, actor, this);
                    if (goapAction == null) {
                        throw new Exception("Goap action " + currType.ToString() + " is null!");
                    }
                    if (goapAction.CanSatisfyRequirements() && goapAction.CanSatisfyRequirementOnBuildGoapTree()) {
                        usableActions.Add(goapAction);
                    }
                    //if (currType == INTERACTION_TYPE.CRAFT_ITEM) {
                    //    Craftsman craftsman = GetTrait("Craftsman") as Craftsman;
                    //    for (int j = 0; j < craftsman.craftedItemNames.Length; j++) {
                    //        CraftItemGoap goapAction = InteractionManager.Instance.CreateNewGoapInteraction(currType, actor, this, false) as CraftItemGoap;
                    //        goapAction.SetCraftedItem(craftsman.craftedItemNames[j]);
                    //        goapAction.Initialize();
                    //        if (goapAction.CanSatisfyRequirements()) {
                    //            usableActions.Add(goapAction);
                    //        }
                    //    }
                    //} else {

                    //}
                }
            }
            return usableActions;
        }
        return null;
    }
    public void SetPOIState(POI_STATE state) {
        _state = state;
    }
    public bool IsAvailable() {
        return _state != POI_STATE.INACTIVE && !isDisabledByPlayer;
    }
    public void SetIsDisabledByPlayer(bool state) {
        isDisabledByPlayer = state;
    }
    #endregion

    #region Goap
    private void ConstructInitialGoapAdvertisementActions() {
        poiGoapActions = new List<INTERACTION_TYPE>();
        poiGoapActions.Add(INTERACTION_TYPE.CARRY_CHARACTER);
        poiGoapActions.Add(INTERACTION_TYPE.ASSAULT_ACTION_NPC);
        poiGoapActions.Add(INTERACTION_TYPE.DROP_CHARACTER);
        poiGoapActions.Add(INTERACTION_TYPE.ABDUCT_ACTION);
        poiGoapActions.Add(INTERACTION_TYPE.RESTRAIN_CHARACTER);
        //poiGoapActions.Add(INTERACTION_TYPE.STROLL);
        poiGoapActions.Add(INTERACTION_TYPE.DAYDREAM);
        poiGoapActions.Add(INTERACTION_TYPE.SLEEP_OUTSIDE);
        poiGoapActions.Add(INTERACTION_TYPE.PRAY);
        //poiGoapActions.Add(INTERACTION_TYPE.EXPLORE);
        //poiGoapActions.Add(INTERACTION_TYPE.PATROL);
        //poiGoapActions.Add(INTERACTION_TYPE.TRAVEL);
        poiGoapActions.Add(INTERACTION_TYPE.RETURN_HOME_LOCATION);
        //poiGoapActions.Add(INTERACTION_TYPE.HUNT_ACTION);
        poiGoapActions.Add(INTERACTION_TYPE.PLAY);
        poiGoapActions.Add(INTERACTION_TYPE.REPORT_CRIME);
        poiGoapActions.Add(INTERACTION_TYPE.REPORT_HOSTILE);
        poiGoapActions.Add(INTERACTION_TYPE.STEAL_CHARACTER);
        poiGoapActions.Add(INTERACTION_TYPE.JUDGE_CHARACTER);
        poiGoapActions.Add(INTERACTION_TYPE.CURSE_CHARACTER);
        poiGoapActions.Add(INTERACTION_TYPE.ASK_FOR_HELP_SAVE_CHARACTER);
        poiGoapActions.Add(INTERACTION_TYPE.ASK_FOR_HELP_REMOVE_POISON_TABLE);
        poiGoapActions.Add(INTERACTION_TYPE.BURY_CHARACTER);
        poiGoapActions.Add(INTERACTION_TYPE.CARRY_CORPSE);
        poiGoapActions.Add(INTERACTION_TYPE.DROP_ITEM_WAREHOUSE);
        poiGoapActions.Add(INTERACTION_TYPE.INVITE_TO_MAKE_LOVE);
        poiGoapActions.Add(INTERACTION_TYPE.DRINK_BLOOD);
        poiGoapActions.Add(INTERACTION_TYPE.REPLACE_TILE_OBJECT);
        poiGoapActions.Add(INTERACTION_TYPE.TANTRUM);
        poiGoapActions.Add(INTERACTION_TYPE.SPREAD_RUMOR_REMOVE_FRIENDSHIP);
        poiGoapActions.Add(INTERACTION_TYPE.SPREAD_RUMOR_REMOVE_LOVE);
        poiGoapActions.Add(INTERACTION_TYPE.BREAK_UP);
        poiGoapActions.Add(INTERACTION_TYPE.SHARE_INFORMATION);
    }
    public void StartGOAP(GoapEffect goal, IPointOfInterest target, GOAP_CATEGORY category, bool isPriority = false, List<Character> otherCharactePOIs = null, bool isPersonalPlan = true, GoapPlanJob job = null, Dictionary<INTERACTION_TYPE, object[]> otherData = null, bool allowDeadTargets = false) {
        List<CharacterAwareness> characterTargetsAwareness = new List<CharacterAwareness>();
        if (target.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
            CharacterAwareness characterAwareness = AddAwareness(target) as CharacterAwareness;
            if (characterAwareness != null) {
                characterTargetsAwareness.Add(characterAwareness);
            }
        }

        if (otherCharactePOIs != null) {
            for (int i = 0; i < otherCharactePOIs.Count; i++) {
                CharacterAwareness characterAwareness = AddAwareness(otherCharactePOIs[i]) as CharacterAwareness;
                if (characterAwareness != null) {
                    characterTargetsAwareness.Add(characterAwareness);
                }
            }
        }
        if (job != null) {
            job.SetAssignedPlan(null);
        }
        _numOfWaitingForGoapThread++;
        //Debug.LogWarning(name + " sent a plan to other thread(" + _numOfWaitingForGoapThread + ")");
        MultiThreadPool.Instance.AddToThreadPool(new GoapThread(this, target, goal, category, isPriority, characterTargetsAwareness, isPersonalPlan, job, allowDeadTargets, otherData));
    }
    public void StartGOAP(GoapAction goal, IPointOfInterest target, GOAP_CATEGORY category, bool isPriority = false, List<Character> otherCharactePOIs = null, bool isPersonalPlan = true, GoapPlanJob job = null, bool allowDeadTargets = false) {
        List<CharacterAwareness> characterTargetsAwareness = new List<CharacterAwareness>();
        if (target.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
            CharacterAwareness characterAwareness = AddAwareness(target) as CharacterAwareness;
            if (characterAwareness != null) {
                characterTargetsAwareness.Add(characterAwareness);
            }
        }

        if (otherCharactePOIs != null) {
            for (int i = 0; i < otherCharactePOIs.Count; i++) {
                CharacterAwareness characterAwareness = AddAwareness(otherCharactePOIs[i]) as CharacterAwareness;
                if (characterAwareness != null) {
                    characterTargetsAwareness.Add(characterAwareness);
                }
            }
        }
        if (job != null) {
            job.SetAssignedPlan(null);
        }
        _numOfWaitingForGoapThread++;
        MultiThreadPool.Instance.AddToThreadPool(new GoapThread(this, target, goal, category, isPriority, characterTargetsAwareness, isPersonalPlan, job, allowDeadTargets));
    }
    public void StartGOAP(INTERACTION_TYPE goalType, IPointOfInterest target, GOAP_CATEGORY category, bool isPriority = false, List<Character> otherCharactePOIs = null, bool isPersonalPlan = true, GoapPlanJob job = null, Dictionary<INTERACTION_TYPE, object[]> otherData = null, bool allowDeadTargets = false) {
        List<CharacterAwareness> characterTargetsAwareness = new List<CharacterAwareness>();
        if (target != null && target.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
            CharacterAwareness characterAwareness = AddAwareness(target) as CharacterAwareness;
            if (characterAwareness != null) {
                characterTargetsAwareness.Add(characterAwareness);
            }
        }
        if (job != null) {
            job.SetAssignedPlan(null);
        }
        _numOfWaitingForGoapThread++;
        MultiThreadPool.Instance.AddToThreadPool(new GoapThread(this, goalType, target, category, isPriority, characterTargetsAwareness, isPersonalPlan, job, allowDeadTargets, otherData));
    }
    public void RecalculatePlan(GoapPlan currentPlan) {
        currentPlan.SetIsBeingRecalculated(true);
        MultiThreadPool.Instance.AddToThreadPool(new GoapThread(this, currentPlan));
    }
    public void ReceivePlanFromGoapThread(GoapThread goapThread) {
        //string log = name + " received a plan from other thread(" + _numOfWaitingForGoapThread + ")";
        //if(goapThread.recalculationPlan == null) {
        //    log += " - thread has no recalculation plan";
        //}
        //Debug.LogWarning(log);
        if (isDead) {
            return;
        }
        if (goapThread.recalculationPlan != null && goapThread.recalculationPlan.isEnd) {
            return;
        }
        if (goapThread.recalculationPlan == null) {
            _numOfWaitingForGoapThread--;
        }
        PrintLogIfActive(goapThread.log);
        if (goapThread.createdPlan != null) {
            if (goapThread.recalculationPlan == null) {
                if (HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER)) {
                    PrintLogIfActive(GameManager.Instance.TodayLogString() + name + " is scrapping plan since " + name + " has a negative disabler trait. " + goapThread.job.name + " is the job.");
                    if (goapThread.job != null) {
                        if (goapThread.job.assignedCharacter == this) {
                            goapThread.job.SetAssignedCharacter(null);
                            goapThread.job.SetAssignedPlan(null);
                            goapThread.job.jobQueueParent.RemoveJobInQueue(goapThread.job);
                        }
                    }
                    return;
                }
                if (goapThread.job != null) {
                    if (goapThread.job.assignedCharacter != this) {
                        PrintLogIfActive(GameManager.Instance.TodayLogString() + name + " is scrapping plan since " + goapThread.job.name + " job's assigned character is no longer him/her. New assigned character is " + (goapThread.job.assignedCharacter != null ? goapThread.job.assignedCharacter.name : "None"));
                        return;
                    }
                    goapThread.job.SetAssignedPlan(goapThread.createdPlan);
                    for (int i = 0; i < goapThread.createdPlan.allNodes.Count; i++) {
                        if (goapThread.createdPlan.allNodes[i].action.goapType == INTERACTION_TYPE.CARRY_CHARACTER
                            || goapThread.createdPlan.allNodes[i].action.goapType == INTERACTION_TYPE.CARRY_CORPSE) {
                            goapThread.createdPlan.job.SetCannotOverrideJob(true);
                            break;
                        }
                    }
                    bool overrideCurrentAction = !(currentAction != null && currentAction.parentPlan != null && currentAction.parentPlan.job != null && currentAction.parentPlan.job.cannotOverrideJob);
                    if (goapThread.job.willImmediatelyBeDoneAfterReceivingPlan && overrideCurrentAction) {
                        AddPlan(goapThread.createdPlan, true);

                        if (stateComponent.currentState != null) {
                            if (stateComponent.currentState.characterState != CHARACTER_STATE.ENGAGE && stateComponent.currentState.characterState != CHARACTER_STATE.FLEE) {
                                stateComponent.currentState.OnExitThisState();
                            }
                        } else {
                            if (currentParty.icon.isTravelling) {
                                if (currentParty.icon.travelLine == null) {
                                    marker.StopMovement();
                                } else {
                                    currentParty.icon.SetOnArriveAction(() => OnArriveAtAreaStopMovement());
                                }
                            }

                            AdjustIsWaitingForInteraction(1);
                            StopCurrentAction(false);
                            AdjustIsWaitingForInteraction(-1);
                        }
                        return;
                    }
                }
                bool isPriority = goapThread.isPriority || (goapThread.job != null ? goapThread.job.isPriority : false);
                AddPlan(goapThread.createdPlan, isPriority);
                //PlanGoapActions();
            } else {
                //Receive plan recalculation
                goapThread.createdPlan.SetIsBeingRecalculated(false);
                if (HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER)) {
                    PrintLogIfActive(GameManager.Instance.TodayLogString() + name + " is scrapping recalculated plan since " + name + " has a negative disabler trait. " + goapThread.job.name + " is the job.");
                    DropPlan(goapThread.recalculationPlan, true);
                    return;
                }
                if (goapThread.createdPlan.job != null) {
                    if (goapThread.createdPlan.job.assignedCharacter != this) {
                        PrintLogIfActive(GameManager.Instance.TodayLogString() + name + " is scrapping recalculated plan since " + goapThread.createdPlan.job.name + " job's assigned character is no longer him/her. New assigned character is " + (goapThread.createdPlan.job.assignedCharacter != null ? goapThread.createdPlan.job.assignedCharacter.name : "None"));
                        DropPlan(goapThread.recalculationPlan, true);
                        return;
                    }
                }
            }
            //if (allGoapPlans.Count == 1) {
            //    //Start this plan immediately since this is the only plan
            //    SchedulePerformGoapPlans();
            //} else {
            //    StartDailyGoapPlanGeneration();
            //}
        } else {
            if (goapThread.recalculationPlan != null) {
                //This means that the recalculation has failed
                DropPlan(goapThread.recalculationPlan);
            } else {
                if (goapThread.job != null) {
                    goapThread.job.SetAssignedCharacter(null);
                    if (goapThread.job.jobQueueParent.character != null) {
                        goapThread.job.jobQueueParent.RemoveJobInQueue(goapThread.job);
                        RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "cancel_job_no_plan");
                    } else {
                        goapThread.job.AddBlacklistedCharacter(this);
                    }
                    //if (goapThread.job.cancelJobOnFail) {
                    //    goapThread.job.jobQueueParent.RemoveJobInQueue(goapThread.job);
                    //}
                }
                //if (allGoapPlans.Count <= 0) {
                //    //StartDailyGoapPlanGeneration();
                //    PlanGoapActions();
                //}
            }
        }
    }
    public bool IsPOIInCharacterAwarenessList(IPointOfInterest poi, List<CharacterAwareness> awarenesses) {
        if (awarenesses != null && awarenesses.Count > 0) {
            for (int i = 0; i < awarenesses.Count; i++) {
                if (awarenesses[i].poi == poi) {
                    return true;
                }
            }
        }
        return false;
    }
    //private void SchedulePerformGoapPlans() {
    //    if (!IsInOwnParty() || isDefender || ownParty.icon.isTravelling || _doNotDisturb > 0 || _job == null || isWaitingForInteraction > 0) {
    //        StartDailyGoapPlanGeneration();
    //        return;
    //    }
    //    if (allGoapPlans.Count > 0) {
    //        StopDailyGoapPlanGeneration();
    //        GameDate dueDate = GameManager.Instance.Today();
    //        dueDate.AddTicks(1);
    //        SchedulingManager.Instance.AddEntry(dueDate, PerformGoapPlans);
    //    }
    //}
    public void PerformGoapPlans() {
        string log = GameManager.Instance.TodayLogString() + "PERFORMING GOAP PLANS OF " + name;
        if (currentAction != null) {
            log += "\n" + name + " can't perform another action because he/she is currently performing " + currentAction.goapName;
            PrintLogIfActive(log);
            return;
        }
        List<INTERACTION_TYPE> actorAllowedActions = RaceManager.Instance.GetNPCInteractionsOfRace(this);
        bool willGoIdleState = true;
        for (int i = 0; i < allGoapPlans.Count; i++) {
            GoapPlan plan = allGoapPlans[i];
            log += "\n" + plan.currentNode.action.goapName;
            if (plan.isBeingRecalculated) {
                log += "\n - Plan is currently being recalculated, skipping...";
                continue; //skip plan
            }
            if (actorAllowedActions.Contains(plan.currentNode.action.goapType) && plan.currentNode.action.CanSatisfyRequirements()) {
                //if (plan.isBeingRecalculated) {
                //    log += "\n - Plan is currently being recalculated, skipping...";
                //    continue; //skip plan
                //}
                if (IsPlanCancelledDueToInjury(plan.currentNode.action)) {
                    log += "\n - Action's plan is cancelled due to injury, dropping plan...";
                    PrintLogIfActive(log);
                    if (allGoapPlans.Count == 1) {
                        DropPlan(plan, true);
                        willGoIdleState = false;
                        break;
                    } else {
                        DropPlan(plan, true);
                        i--;
                        continue;
                    }
                }
                if (plan.currentNode.action.IsHalted()) {
                    log += "\n - Action " + plan.currentNode.action.goapName + " is waiting, skipping...";
                    continue;
                }
                bool preconditionsSatisfied = plan.currentNode.action.CanSatisfyAllPreconditions();
                if (!preconditionsSatisfied) {
                    log += "\n - Action's preconditions are not all satisfied, trying to recalculate plan...";
                    if (plan.doNotRecalculate) {
                        log += "\n - Action's plan has doNotRecalculate state set to true, dropping plan...";
                        PrintLogIfActive(log);
                        if (allGoapPlans.Count == 1) {
                            DropPlan(plan);
                            willGoIdleState = false;
                            break;
                        } else {
                            DropPlan(plan);
                            i--;
                        }
                    } else {
                        PrintLogIfActive(log);
                        RecalculatePlan(plan);
                        willGoIdleState = false;
                    }
                } else {
                    //Do not perform action if the target character is still in another character's party, this means that he/she is probably being abducted
                    //Wait for the character to be in its own party before doing the action
                    if (plan.currentNode.action.poiTarget.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
                        Character targetCharacter = plan.currentNode.action.poiTarget as Character;
                        if (!targetCharacter.IsInOwnParty() && targetCharacter.currentParty != _ownParty) {
                            log += "\n - " + targetCharacter.name + " is not in its own party, waiting and skipping...";
                            PrintLogIfActive(log);
                            continue;
                        }
                    }
                    log += "\n - Action's preconditions are all satisfied, doing action...";
                    PrintLogIfActive(log);
                    Messenger.Broadcast(Signals.CHARACTER_WILL_DO_PLAN, this, plan);
                    plan.currentNode.action.DoAction();
                    willGoIdleState = false;
                    break;
                }
            } else {
                log += "\n - Action did not meet current requirements and allowed actions, dropping plan...";
                PrintLogIfActive(log);
                if (allGoapPlans.Count == 1) {
                    DropPlan(plan);
                    willGoIdleState = false;
                    break;
                } else {
                    DropPlan(plan);
                    i--;
                }
            }
        }
        if (willGoIdleState) {
            log += "\n - Character will go into idle state";
            PrintLogIfActive(log);
            IdlePlans();
        }
    }
    private bool IsPlanCancelledDueToInjury(GoapAction action) {
        if (action.poiTarget.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
            Character target = action.poiTarget as Character;
            if (IsHostileWith(target) && GetTrait("Injured") != null) {
                AdjustIsWaitingForInteraction(1);
                DropPlan(action.parentPlan);
                AdjustIsWaitingForInteraction(-1);

                Log addLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "plan_cancelled_injury");
                addLog.AddToFillers(this, this.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                addLog.AddLogToInvolvedObjects();
                PlayerManager.Instance.player.ShowNotificationFrom(this, addLog);
                return true;
            }
        }
        return false;
    }
    public void PerformGoapAction() {
        string log = string.Empty;
        if (currentAction == null) {
            log = GameManager.Instance.TodayLogString() + name + " cannot PerformGoapAction because there is no current action!";
            PrintLogIfActive(log);
            //Debug.LogError(log);
            //if (!DropPlan(plan)) {
            //    //PlanGoapActions();
            //}
            //StartDailyGoapPlanGeneration();
            return;
        }
        log = GameManager.Instance.TodayLogString() + name + " is performing goap action: " + currentAction.goapName;
        FaceTarget(currentAction.poiTarget);
        if (currentAction.isStopped) {
            log += "\n Action is stopped! Dropping plan...";
            PrintLogIfActive(log);
            SetCurrentAction(null);
            if (!DropPlan(currentAction.parentPlan)) {
                //PlanGoapActions();
            }
        } else {
            if (currentAction.IsHalted()) {
                log += "\n Action is waiting! Not doing action...";
                //if (currentAction.poiTarget.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
                //    Character targetCharacter = currentAction.poiTarget as Character;
                //    targetCharacter.AdjustIsWaitingForInteraction(-1);
                //}
                SetCurrentAction(null);
                return;
            }
            if (currentAction.CanSatisfyRequirements() && currentAction.CanSatisfyAllPreconditions()) {
                log += "\nAction satisfies all requirements and preconditions, proceeding to perform actual action: " + currentAction.goapName + " to " + currentAction.poiTarget.name + " at " + currentAction.poiTarget.gridTileLocation?.ToString() ?? "No Tile Location";
                PrintLogIfActive(log);
                currentAction.PerformActualAction();
            } else {
                log += "\nAction did not meet all requirements and preconditions. Will try to recalculate plan...";
                //if (currentAction.poiTarget.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
                //    Character targetCharacter = currentAction.poiTarget as Character;
                //    targetCharacter.AdjustIsWaitingForInteraction(-1);
                //}
                GoapPlan plan = currentAction.parentPlan;
                SetCurrentAction(null);
                if (plan.doNotRecalculate) {
                    log += "\n - Action's plan has doNotRecalculate state set to true, dropping plan...";
                    PrintLogIfActive(log);
                    if (!DropPlan(plan)) {
                        //PlanGoapActions();
                    }
                } else {
                    PrintLogIfActive(log);
                    RecalculatePlan(plan);
                    //IdlePlans();
                }
            }
        }
    }
    public void GoapActionResult(string result, GoapAction action) {
        string log = GameManager.Instance.TodayLogString() + name + " is done performing goap action: " + action.goapName;
        //Debug.Log(log);
        if (action == currentAction) {
            SetCurrentAction(null);
        }
        if (action.poiTarget.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
            Character targetCharacter = action.poiTarget as Character;
            targetCharacter.RemoveTargettedByAction(action);
        }
        GoapPlan plan = action.parentPlan;
        if (isDead) {
            log += "\n" + name + " is dead!";
            if (plan.job != null) {
                if (result == InteractionManager.Goap_State_Success) {
                    if (plan.currentNode.parent == null) {
                        log += "This plan has a job and the result of action " + action.goapName + " is " + result + " and this is the last action for this plan, removing job in job queue...";
                        plan.job.jobQueueParent.RemoveJobInQueue(plan.job);
                    }
                }
                log += "\nSetting assigned character and plan to null...";
                plan.job.SetAssignedCharacter(null);
                plan.job.SetAssignedPlan(null);
            }
            PrintLogIfActive(log);
            DropPlan(plan);
            return;
        }
        //if (plan == null) {
        //    log += "\nAction " + action.goapName + " no longer has a plan, current plans are: ";
        //    for (int i = 0; i < allGoapPlans.Count; i++) {
        //        if(i > 0) {
        //            log += ", ";
        //        }
        //        log += allGoapPlans[i].endNode.action.goapName;
        //    }
        //    PrintLogIfActive(log);
        //    PlanGoapActions();
        //    return;
        //}
        if (action.isStopped) {
            log += "\nAction is stopped!";
            PrintLogIfActive(log);
            if (!DropPlan(plan)) {
                //PlanGoapActions();
            }
            return;
        }
        if (plan.state == GOAP_PLAN_STATE.CANCELLED) {
            log += "\nPlan was cancelled.";
            if (plan.job != null) {
                log += "\nRemoving job in queue...";
                plan.job.jobQueueParent.RemoveJobInQueue(plan.job);
            }
            PrintLogIfActive(log);
            if (!DropPlan(plan)) {
                //PlanGoapActions();
            }
            return;
        }

        //Reason: https://trello.com/c/58aGENsO/1867-attempt-to-find-another-nearby-chair-first-instead-of-dropping-drink-eat-sit-down-actions
        if (result == InteractionManager.Goap_State_Fail) {
            //if the last action of the plan failed and that action type can be replaced
            if (action.goapType.CanBeReplaced()) {
                //find a similar action that is advertised by another object, in the same structure
                //if there is any, insert that action into the current plan, then do that next
                List<TileObject> objs = currentStructure.GetTileObjectsThatAdvertise(action.goapType);
                if (objs.Count > 0) {
                    TileObject chosenObject = objs[UnityEngine.Random.Range(0, objs.Count)];
                    GoapAction newAction = chosenObject.Advertise(action.goapType, this);
                    if (newAction != null) {
                        plan.InsertAction(newAction);
                    } else {
                        Debug.LogWarning(chosenObject.ToString() + " did not return an action of type " + action.goapType.ToString());
                    }
                }
            }
        }
        log += "\nPlan is setting next action to be done...";
        plan.SetNextNode();
        if (plan.currentNode == null) {
            log += "\nThis action is the end of plan.";
            if (plan.job != null) {
                log += "\nRemoving job in queue...";
                plan.job.jobQueueParent.RemoveJobInQueue(plan.job);
            }
            PrintLogIfActive(log);
            //this means that this is the end goal so end this plan now
            if (!DropPlan(plan)) {
                //PlanGoapActions();
            }
        } else {
            log += "\nNext action for this plan: " + plan.currentNode.action.goapName;
            if (plan.job != null) {
                log += "\nPlan has a job: " + plan.job.name + ". Assigned character " + (plan.job.assignedCharacter != null ? plan.job.assignedCharacter.name : "None") + " does not match with " + name + ".";
                log += "Drop plan because this character is no longer the one assigned";
                DropPlan(plan);
            }
            PrintLogIfActive(log);
            //PlanGoapActions();
        }
        
        //if (result == InteractionManager.Goap_State_Success) {
        //    log += "\nAction performed is a success!";
        //    plan.SetNextNode();
        //    if (plan.currentNode == null) {
        //        log += "\nThis action is the end of plan.";
        //        if (plan.job != null) {
        //            log += "\nRemoving job in queue...";
        //            plan.job.jobQueueParent.RemoveJobInQueue(plan.job);
        //        }
        //        PrintLogIfActive(log);
        //        //this means that this is the end goal so end this plan now
        //        if (!DropPlan(plan)) {
        //            //PlanGoapActions();
        //        }
        //    } else {
        //        log += "\nNext action for this plan: " + plan.currentNode.action.goapName;
        //        PrintLogIfActive(log);
        //        //PlanGoapActions();
        //    }
        //} else if(result == InteractionManager.Goap_State_Fail) {
        //    if(plan.endNode.action == action) {
        //        log += "\nAction performed has failed. Since this action is the end/goal action, it will not recalculate anymore. Dropping plan...";
        //        PrintLogIfActive(log);
        //        if (!DropPlan(plan)) {
        //            //PlanGoapActions();
        //        }
        //    } else {
        //        log += "\nAction performed has failed. Will try to recalculate plan...";
        //        if (plan.doNotRecalculate) {
        //            log += "\n - Action's plan has doNotRecalculate state set to true, dropping plan...";
        //            PrintLogIfActive(log);
        //            if (!DropPlan(plan)) {
        //                //PlanGoapActions();
        //            }
        //        } else {
        //            PrintLogIfActive(log);
        //            RecalculatePlan(plan);
        //            //IdlePlans();
        //        }
        //    }
        //}
        action.OnResultReturnedToActor();
    }
    public bool DropPlan(GoapPlan plan, bool forceCancelJob = false) {
        if (allGoapPlans.Remove(plan)) {
            plan.EndPlan();
            if (plan.job != null) {
                if (plan.job.cancelJobOnFail || forceCancelJob) {
                    plan.job.jobQueueParent.RemoveJobInQueue(plan.job);
                }
                plan.job.SetAssignedCharacter(null);
                plan.job.SetAssignedPlan(null);
            }
            //if (allGoapPlans.Count <= 0) {
            //    PlanGoapActions();
            //    return true;
            //    //StartDailyGoapPlanGeneration();
            //}
            return true;
        }
        return false;
    }
    public void DropAllPlans(GoapPlan planException = null) {
        if (planException == null) {
            while (allGoapPlans.Count > 0) {
                DropPlan(allGoapPlans[0]);
            }
        } else {
            for (int i = 0; i < allGoapPlans.Count; i++) {
                if (allGoapPlans[i] != planException) {
                    DropPlan(allGoapPlans[i]);
                    i--;
                }
            }
        }
    }
    public GoapPlan GetPlanWithGoalEffect(GOAP_EFFECT_CONDITION goalEffect) {
        for (int i = 0; i < allGoapPlans.Count; i++) {
            if (allGoapPlans[i].goalEffects.Contains(goalEffect)) {
                return allGoapPlans[i];
            }
        }
        return null;
    }
    public GoapPlan GetPlanByCategory(GOAP_CATEGORY category) {
        for (int i = 0; i < allGoapPlans.Count; i++) {
            if (allGoapPlans[i].category == category) {
                return allGoapPlans[i];
            }
        }
        return null;
    }
    //For testing: Drop Character
    public void DropACharacter() {
        if (awareness.ContainsKey(POINT_OF_INTEREST_TYPE.CHARACTER)) {
            List<IAwareness> characterAwarenesses = awareness[POINT_OF_INTEREST_TYPE.CHARACTER];
            Character randomTarget = characterAwarenesses[UnityEngine.Random.Range(0, characterAwarenesses.Count)].poi as Character;
            GoapEffect goapEffect = new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_FROM_PARTY, conditionKey = homeArea, targetPOI = randomTarget };
            StartGOAP(goapEffect, randomTarget, GOAP_CATEGORY.REACTION);
        }
    }
    public GoapPlan GetPlanWithAction(GoapAction action) {
        for (int i = 0; i < allGoapPlans.Count; i++) {
            for (int j = 0; j < allGoapPlans[i].allNodes.Count; j++) {
                if (allGoapPlans[i].allNodes[j].action == action) {
                    return allGoapPlans[i];
                }
            }
        }
        return null;
    }
    public void OnCharacterDoAction(GoapAction action) {
        Messenger.Broadcast(Signals.CHARACTER_DID_ACTION, this, action);
    }
    public void FaceTarget(IPointOfInterest target) {
        if (this != target && !this.isDead && gridTileLocation != null) {
            if (target is Character) {
                Character targetCharacter = target as Character;
                if (targetCharacter.isDead) {
                    return;
                }
                CharacterMarker lookAtMarker = targetCharacter.currentParty.owner.marker;
                if (lookAtMarker.character != this) {
                    marker.LookAt(lookAtMarker.transform.position);
                }
            } else {
                if (target.gridTileLocation == null) {
                    return;
                }
                marker.LookAt(target.gridTileLocation.centeredWorldLocation);
            }
        }
    }
    public void SetCurrentAction(GoapAction action) {
        //if(currentAction != null && action == null) {
        //    //This means that the current action of this character is being set to null, when this happens, the poi target of the current action must not wait for interaction anymore
        //    if (currentAction.poiTarget.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
        //        Character targetCharacter = currentAction.poiTarget as Character;
        //        if (targetCharacter != currentAction.actor) {
        //            targetCharacter.AdjustIsWaitingForInteraction(-1);
        //            targetCharacter.RemoveTargettedByAction(currentAction);
        //        }
        //    }
        //}
        if (currentAction != null) {
            previousCurrentAction = currentAction;
        }
        currentAction = action;
        if (currentAction != null) {
            PrintLogIfActive(GameManager.Instance.TodayLogString() + this.name + " will do action " + action.goapType.ToString() + " to " + action.poiTarget.ToString());
            if (currentAction.goapType.IsCombatAction()) { //if the character will do a combat action, remove all ignore hostilities value
                ClearIgnoreHostilities();
            }
            stateComponent.SetStateToDo(null);
        }
        string summary = GameManager.Instance.TodayLogString() + "Set current action to ";
        if (currentAction == null) {
            summary += "null";
        } else {
            summary += currentAction.goapName + " targetting " + currentAction.poiTarget.name;
        }
        summary += "\n StackTrace: " + StackTraceUtility.ExtractStackTrace();

        actionHistory.Add(summary);
        if (actionHistory.Count > 10) {
            actionHistory.RemoveAt(0);
        }

    }
    public void SetHasAlreadyAskedForPlan(bool state) {
        _hasAlreadyAskedForPlan = state;
    }
    public void PrintLogIfActive(string log) {
        if (InteriorMapManager.Instance.currentlyShowingArea == specificLocation) {//UIManager.Instance.characterInfoUI.isShowing && UIManager.Instance.characterInfoUI.activeCharacter == this
            Debug.Log(log);
        }
    }
    public void OnTargettedByAction(GoapAction action) {
        targettedByAction.Add(action);
        if (marker != null) {
            marker.OnCharacterTargettedByAction(action);
        }
    }
    public void RemoveTargettedByAction(GoapAction action) {
        if (targettedByAction.Remove(action)) {
            if (marker != null) {
                marker.OnCharacterRemovedTargettedByAction(action);
            }
        }
    }
    private void OnCharacterFinishedAction(Character character, GoapAction action, string result) {
        
    }
    private void CancelCurrentAction(Character target, string cause) {
        if (this != target && !isDead && currentAction != null && currentAction.poiTarget == target && !currentAction.cannotCancelAction) {
            RegisterLogAndShowNotifToThisCharacterOnly("Generic", "action_cancelled_cause", null, cause);
            currentAction.StopAction();
        }
    }
    //This will only stop the current action of this character, this is different from StopAction because this will not drop the plan if the actor is not performing it but is on the way
    //This does not stop the movement of this character, call StopMovement separately to stop movement
    public void StopCurrentAction(bool shouldDoAfterEffect = true) {
        if(currentAction != null) {
            if (currentAction.isPerformingActualAction && !currentAction.isDone) {
                if (!shouldDoAfterEffect) {
                    currentAction.OnStopActionDuringCurrentState();
                }
                currentAction.currentState.EndPerTickEffect(shouldDoAfterEffect);
            } else {
                SetCurrentAction(null);
            }
        }
    }
    #endregion

    #region Supply
    public void AdjustSupply(int amount) {
        supply += amount;
        if (supply < 0) {
            supply = 0;
        }
    }
    public void SetSupply(int amount) {
        supply = amount;
        if (supply < 0) {
            supply = 0;
        }
    }
    #endregion

    #region Collision
    //Most of collision is handled by the character's marker
    public void InitializeCollisionTrigger() { }
    public void PlaceCollisionTriggerAt(LocationGridTile tile) { }
    public void DisableCollisionTrigger() { }
    public void SetCollisionTrigger(POICollisionTrigger trigger) { }
    public void PlaceGhostCollisionTriggerAt(LocationGridTile tile) { }
    #endregion

    #region Hostility
    /// <summary>
    /// Function to encapsulate, whether or not this character treats another character as hostile.
    /// </summary>
    /// <param name="character">Character in question.</param>
    public bool IsHostileWith(Character character) {
        //return true;
        if (character.isDead || this.isDead) {
            return false;
        }

        //if (character.ignoreHostility > 0) {
        //    //if the other character is set to ignore hostilities, check if the character's current action is a combat action or state is a combat state
        //    //if it is, waive the ignore hostility value
        //    if (true) {

        //    }
        //}

        if (character.ignoreHostility > 0 || this.ignoreHostility > 0) {
            //if either the character in question or this character should ignore hostility, return false.
            return false;
        }
        if (this.faction.id == FactionManager.Instance.neutralFaction.id) {
            //this character is unaligned
            //if unaligned, hostile to all other characters, except those of same race
            return character.race != this.race;
        } else {
            //this character has a faction
            //if has a faction, is hostile to characters of every other faction
            return this.faction.id != character.faction.id;
        }
    }
    /// <summary>
    /// Adjusts value that ignores hostility. This character will ignore hostiles and will be ignored by other hostiles if
    /// the value is greater than 0.
    /// </summary>
    /// <param name="amount">Amount to increase/decrease</param>
    public void AdjustIgnoreHostilities(int amount) {
        ignoreHostility += amount;
        ignoreHostility = Mathf.Max(0, ignoreHostility);
    }
    public void ClearIgnoreHostilities() {
        ignoreHostility = 0;
    }
    #endregion

    #region Crime System
    /// <summary>
    /// Make this character react to a crime that he/she witnessed.
    /// </summary>
    /// <param name="witnessedCrime">Witnessed Crime.</param>
    public void ReactToCrime(GoapAction witnessedCrime) {
        ReactToCrime(witnessedCrime.committedCrime, witnessedCrime.actorAlterEgo, witnessedCrime);
        witnessedCrime.OnWitnessedBy(this);
    }
    /// <summary>
    /// Base function for crime reactions
    /// </summary>
    /// <param name="committedCrime">The type of crime that was committed.</param>
    /// <param name="criminal">The character that committed the crime</param>
    /// <param name="witnessedCrime">The crime witnessed by this character, if this is null, character was only informed of the crime by someone else.</param>
    /// <param name="informedCrime">The crime this character was informed of. NOTE: Should only have value if Share Intel</param>
    public void ReactToCrime(CRIME committedCrime, AlterEgoData criminal, GoapAction witnessedCrime = null, GoapAction informedCrime = null) {
        //NOTE: Moved this to be per action specific. See GoapAction.IsConsideredACrimeBy and GoapAction.CanReactToThisCrime for necessary mechanics.
        //if (witnessedCrime != null) {
        //    //if the action that should be considered a crime is part of a job from this character's area, do not consider it a crime
        //    if (witnessedCrime.parentPlan.job != null
        //        && homeArea.jobQueue.jobsInQueue.Contains(witnessedCrime.parentPlan.job)) {
        //        return;
        //    }
        //    //if the witnessed crime is targetting this character, this character should not react to the crime if the crime's doesNotStopTargetCharacter is true
        //    if (witnessedCrime.poiTarget == this && witnessedCrime.doesNotStopTargetCharacter) {
        //        return;
        //    }
        //}

        string reactSummary = GameManager.Instance.TodayLogString() + this.name + " will react to crime committed by " + criminal.owner.name;
        Log witnessLog = null;
        Log reportLog = null;
        RELATIONSHIP_EFFECT relationshipEfffectWithCriminal = GetRelationshipEffectWith(criminal);
        CRIME_CATEGORY category = committedCrime.GetCategory();

        //If character witnessed an Infraction crime:
        if (category == CRIME_CATEGORY.INFRACTIONS) {
            //-Witness Log: "[Character Name] saw [Criminal Name] committing [Theft/Assault/Murder]."
            //- Report / Share Intel Log: "[Character Name] saw [Criminal Name] committing [Theft/Assault/Murder]."
            //- no additional response
            reactSummary += "\nCrime committed is infraction.";
            witnessLog = new Log(GameManager.Instance.Today(), "Character", "CrimeSystem", "witnessed");
            reportLog = new Log(GameManager.Instance.Today(), "Character", "CrimeSystem", "witnessed");
        }
        //If character has a positive relationship (Friend, Lover, Paramour) with the criminal
        else if (relationshipEfffectWithCriminal == RELATIONSHIP_EFFECT.POSITIVE) {
            reactSummary += "\n" + this.name + " has a positive relationship with " + criminal.owner.name;
            //and crime severity is a Misdemeanor:
            if (category == CRIME_CATEGORY.MISDEMEANOR) {
                reactSummary += "\nCrime committed is misdemeanor.";
                //- Witness Log: "[Character Name] saw [Criminal Name] committing [Theft/Assault/Murder] but did not do anything due to their relationship."
                //-Report / Share Intel Log: "[Character Name] was informed that [Criminal Name] committed [Theft/Assault/Murder] but did not do anything due to their relationship."
                witnessLog = new Log(GameManager.Instance.Today(), "Character", "CrimeSystem", "do_nothing");
                reportLog = new Log(GameManager.Instance.Today(), "Character", "CrimeSystem", "report_do_nothing");
            }
            //and crime severity is Serious Crimes or worse:
            else if (category.IsGreaterThanOrEqual(CRIME_CATEGORY.SERIOUS)) {
                reactSummary += "\nCrime committed is serious or worse. Removing positive relationships.";
                //- Relationship Degradation between Character and Criminal
                CharacterManager.Instance.RelationshipDegradation(criminal.owner, this, witnessedCrime);
                witnessLog = new Log(GameManager.Instance.Today(), "Character", "CrimeSystem", "witnessed");
                reportLog = new Log(GameManager.Instance.Today(), "Character", "CrimeSystem", "report_witnessed");
                PerRoleCrimeReaction(committedCrime, criminal, witnessedCrime, informedCrime);
            }
        }
        //If character has no relationships with the criminal or they are enemies and the crime is a Misdemeanor or worse:
        else if ((!this.HasRelationshipWith(criminal) || this.HasRelationshipOfTypeWith(criminal, RELATIONSHIP_TRAIT.ENEMY)) 
            && category.IsGreaterThanOrEqual(CRIME_CATEGORY.MISDEMEANOR)) {
            reactSummary += "\n" + this.name + " does not have a relationship with or is an enemy of " + criminal.name + " and the committed crime is misdemeanor or worse";
            //- Relationship Degradation between Character and Criminal
            CharacterManager.Instance.RelationshipDegradation(criminal.owner, this, witnessedCrime);
            //- Witness Log: "[Character Name] saw [Criminal Name] committing [Theft/Assault/Murder]!"
            witnessLog = new Log(GameManager.Instance.Today(), "Character", "CrimeSystem", "witnessed");
            reportLog = new Log(GameManager.Instance.Today(), "Character", "CrimeSystem", "report_witnessed");
            PerRoleCrimeReaction(committedCrime, criminal, witnessedCrime, informedCrime);
        }

        if (witnessedCrime != null) {
            if (witnessLog != null) {
                witnessLog.AddToFillers(this, this.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                witnessLog.AddToFillers(criminal.owner, criminal.owner.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                witnessLog.AddToFillers(null, Utilities.NormalizeStringUpperCaseFirstLetters(committedCrime.ToString()), LOG_IDENTIFIER.STRING_1);
                PlayerManager.Instance.player.ShowNotificationFrom(this, witnessLog);
            }
        } else {
            if (reportLog != null) {
                reportLog.AddToFillers(this, this.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                reportLog.AddToFillers(criminal.owner, criminal.owner.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                reportLog.AddToFillers(null, Utilities.NormalizeStringUpperCaseFirstLetters(committedCrime.ToString()), LOG_IDENTIFIER.STRING_1);
                PlayerManager.Instance.player.ShowNotificationFrom(this, reportLog);
            }
        }

        Debug.Log(reactSummary);
    }
    /// <summary>
    /// Crime reactions per role.
    /// </summary>
    /// <param name="committedCrime">The type of crime that was committed.</param>
    /// <param name="criminal">The character that committed the crime</param>
    /// <param name="witnessedCrime">The crime witnessed by this character, if this is null, character was only informed of the crime by someone else.</param>
    /// <param name="informedCrime">The crime this character was informed of. NOTE: Should only have value if Share Intel</param>
    private void PerRoleCrimeReaction(CRIME committedCrime, AlterEgoData criminal, GoapAction witnessedCrime = null, GoapAction informedCrime = null) {
        GoapPlanJob job = null;
        switch (role.roleType) {
            case CHARACTER_ROLE.CIVILIAN:
            case CHARACTER_ROLE.ADVENTURER:
                //- If the character is a Civilian or Adventurer, he will enter Flee mode (fleeing the criminal) and will create a Report Crime Job Type in his personal job queue
                if (this.faction != FactionManager.Instance.neutralFaction && criminal.faction == this.faction) {
                    //only make character flee, if he/she actually witnessed the crime (not share intel)
                    GoapAction crimeToReport = informedCrime;
                    if (witnessedCrime != null) {
                        crimeToReport = witnessedCrime;
                        this.marker.AddHostileInRange(criminal.owner, CHARACTER_STATE.FLEE);
                    }
                    job = new GoapPlanJob("Report Crime", INTERACTION_TYPE.REPORT_CRIME, new Dictionary<INTERACTION_TYPE, object[]>() {
                        { INTERACTION_TYPE.REPORT_CRIME,  new object[] { committedCrime, criminal, crimeToReport }}
                    });
                    job.SetCannotOverrideJob(true);
                    jobQueue.AddJobInQueue(job, true);
                }
                break;
            case CHARACTER_ROLE.LEADER:
            case CHARACTER_ROLE.NOBLE:
                //- If the character is a Noble or Faction Leader, the criminal will gain the relevant Crime-type trait
                //If he is a Noble or Faction Leader, he will create the Apprehend Job Type in the Location job queue instead.
                if (this.faction != FactionManager.Instance.neutralFaction && criminal.faction == this.faction) {
                    //only add apprehend job if the criminal is part of this characters faction
                    criminal.owner.AddCriminalTrait(committedCrime);
                    CreateApprehendJobFor(criminal.owner);
                    //job = new GoapPlanJob("Apprehend", new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_FROM_PARTY, conditionKey = homeArea, targetPOI = actor });
                    //job.AddForcedInteraction(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Restrained", targetPOI = actor }, INTERACTION_TYPE.RESTRAIN_CHARACTER);
                    //job.SetCanTakeThisJobChecker(CanCharacterTakeApprehendJob);
                    //homeArea.jobQueue.AddJobInQueue(job);
                }

                break;
            case CHARACTER_ROLE.SOLDIER:
            case CHARACTER_ROLE.BANDIT:
                //- If the character is a Soldier, the criminal will gain the relevant Crime-type trait
                if (this.faction != FactionManager.Instance.neutralFaction && criminal.faction == this.faction) {
                    //only add apprehend job if the criminal is part of this characters faction
                    criminal.owner.AddCriminalTrait(committedCrime);
                    //- If the character is a Soldier, he will also create an Apprehend Job Type in his personal job queue.
                    //job = new GoapPlanJob("Apprehend", new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_FROM_PARTY, conditionKey = homeArea, targetPOI = actor });
                    //job.AddForcedInteraction(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Restrained", targetPOI = actor }, INTERACTION_TYPE.RESTRAIN_CHARACTER);
                    //job.SetCanTakeThisJobChecker(CanCharacterTakeApprehendJob);
                    //homeArea.jobQueue.AddJobInQueue(job);
                    job = CreateApprehendJobFor(criminal.owner);
                    if (job != null) {
                        homeArea.jobQueue.ForceAssignCharacterToJob(job, this);
                    }
                }
                break;
            default:
                break;
        }
    }
    public void AddCriminalTrait(CRIME crime) {
        Trait trait = null;
        switch (crime) {
            case CRIME.THEFT:
                trait = new Thief();
                break;
            case CRIME.ASSAULT:
                trait = new Assaulter();
                break;
            case CRIME.MURDER:
                trait = new Murderer();
                break;
            case CRIME.ATTEMPTED_MURDER:
                trait = new AttemptedMurderer();
                break;
            case CRIME.ABERRATION:
                trait = new Aberration();
                break;
            default:
                break;
        }
        if (trait != null) {
            AddTrait(trait);
        }
    }
    public bool CanReactToCrime() {
        if (stateComponent.currentState == null) {
            return true;
        } else {
            if (stateComponent.currentState.characterState == CHARACTER_STATE.FLEE || stateComponent.currentState.characterState == CHARACTER_STATE.ENGAGE) {
                return false;
            }
        }
        return true;
    }
    #endregion

    #region Mood
    public void SetMoodValue(int amount) {
        moodValue = amount;
        moodValue = Mathf.Clamp(moodValue, 1, 100);
    }
    public void AdjustMoodValue(int amount, GoapAction triggerAction = null) {
        moodValue += amount;
        moodValue = Mathf.Clamp(moodValue, 1, 100);
        if(triggerAction != null) {
            if(amount < 0 && currentMoodType == CHARACTER_MOOD.DARK) {
                if(currentAction != null && currentAction.goapType == INTERACTION_TYPE.TANTRUM) {
                    return;
                }
                int chance = UnityEngine.Random.Range(0, 100);
                if(chance < 20) {
                    CancelAllJobsAndPlans();
                    //Create Tantrum action
                    GoapPlanJob tantrum = new GoapPlanJob("Tantrum", INTERACTION_TYPE.TANTRUM, this, new Dictionary<INTERACTION_TYPE, object[]>() {
                        { INTERACTION_TYPE.TANTRUM, new object[] { triggerAction } }
                    });
                    tantrum.SetCannotOverrideJob(true);
                    tantrum.SetWillImmediatelyBeDoneAfterReceivingPlan(true);
                    tantrum.SetIsPriority(true);
                    jobQueue.AddJobInQueue(tantrum, true);
                    jobQueue.ProcessFirstJobInQueue(this);
                    Debug.Log(this.name + " is started having a tantrum!");
                }
            }
        }
    }
    public CHARACTER_MOOD ConvertCurrentMoodValueToType() {
        return ConvertMoodValueToType(moodValue);
    }
    public CHARACTER_MOOD ConvertMoodValueToType(int amount) {
        if (amount >= 1 && amount < 26) {
            return CHARACTER_MOOD.DARK;
        } else if (amount >= 26 && amount < 51) {
            return CHARACTER_MOOD.BAD;
        } else if (amount >= 51 && amount < 76) {
            return CHARACTER_MOOD.GOOD;
        } else {
            return CHARACTER_MOOD.GREAT;
        }
    }
    #endregion

    #region Explore Items
    public ExploreState lastExploreState { get; private set; }
    /// <summary>
    /// Set the last explore state that ths character did.
    /// </summary>
    /// <param name="es">The explore state.</param>
    public void SetLastExploreState(ExploreState es) {
        lastExploreState = es;
    }
    public void OnReturnHome() {
        if (lastExploreState != null && lastExploreState.itemsCollected.Count > 0 && role.roleType == CHARACTER_ROLE.ADVENTURER) {
            //create deliver treasure job that will deposit the items that the character collected during his/her last explore action.
            lastExploreState.CreateDeliverTreasureJob();
        }
    }
    #endregion

    #region Pathfinding
    public List<LocationGridTile> GetTilesInRadius(int radius, int radiusLimit = 0, bool includeCenterTile = false, bool includeTilesInDifferentStructure = false) {
        List<LocationGridTile> tiles = new List<LocationGridTile>();
        int mapSizeX = specificLocation.areaMap.map.GetUpperBound(0);
        int mapSizeY = specificLocation.areaMap.map.GetUpperBound(1);
        int x = gridTileLocation.localPlace.x;
        int y = gridTileLocation.localPlace.y;
        if (includeCenterTile) {
            tiles.Add(gridTileLocation);
        }
        int xLimitLower = x - radiusLimit;
        int xLimitUpper = x + radiusLimit;
        int yLimitLower = y - radiusLimit;
        int yLimitUpper = y + radiusLimit;


        for (int dx = x - radius; dx <= x + radius; dx++) {
            for (int dy = y - radius; dy <= y + radius; dy++) {
                if (dx >= 0 && dx <= mapSizeX && dy >= 0 && dy <= mapSizeY) {
                    if (dx == x && dy == y) {
                        continue;
                    }
                    if (radiusLimit > 0 && dx > xLimitLower && dx < xLimitUpper && dy > yLimitLower && dy < yLimitUpper) {
                        continue;
                    }
                    LocationGridTile result = specificLocation.areaMap.map[dx, dy];
                    if (!includeTilesInDifferentStructure && result.structure != gridTileLocation.structure) { continue; }
                    tiles.Add(result);
                }
            }
        }
        return tiles;
    }
    #endregion

    #region States
    public void OnCharacterEnteredState(CharacterState state) {
        if (state.characterState.IsCombatState()) {
            ClearIgnoreHostilities();
        }
        marker.UpdateActionIcon();
    }
    #endregion

    #region Alter Egos
    private void InitializeAlterEgos() {
        alterEgos = new Dictionary<string, AlterEgoData>();
        alterEgos.Add(CharacterManager.Original_Alter_Ego, new AlterEgoData(this, CharacterManager.Original_Alter_Ego));
        currentAlterEgoName = CharacterManager.Original_Alter_Ego;
        currentAlterEgo.SetFaction(faction);
        currentAlterEgo.SetCharacterClass(characterClass);
        currentAlterEgo.SetRace(race);
        currentAlterEgo.SetRole(role);
        currentAlterEgo.SetHomeStructure(homeStructure);
    }
    public AlterEgoData CreateNewAlterEgo(string alterEgoName) {
        if (alterEgos.ContainsKey(alterEgoName)) {
            throw new Exception(this.name + " already has an alter ego named " + alterEgoName + " but something is trying to create a new one!");
        }
        AlterEgoData newData = new AlterEgoData(this, alterEgoName);
        alterEgos.Add(alterEgoName, newData);
        return newData;
    }
    public void RemoveAlterEgo(string alterEgoName) {
        if (alterEgoName == CharacterManager.Original_Alter_Ego) {
            throw new Exception("Something is trying to remove " + this.name + "'s original alter ego! This should not happen!");
        }
        if (currentAlterEgoName == alterEgoName) {
            //switch to the original alter ego
            SwitchAlterEgo(CharacterManager.Original_Alter_Ego);
        }
        if (alterEgos.ContainsKey(alterEgoName)) {
            alterEgos.Remove(alterEgoName);
        }
    }
    public bool isSwitchingAlterEgo { get; private set; } //is this character in the process of switching alter egos?
    public void SwitchAlterEgo(string alterEgoName) {
        if (currentAlterEgoName == alterEgoName) {
            return; //ignore change
        }
        if (alterEgos.ContainsKey(alterEgoName)) {
            isSwitchingAlterEgo = true;
            //apply all alter ego changes here
            AlterEgoData alterEgoData = alterEgos[alterEgoName];
            currentAlterEgo.CopySpecialTraits();

            //Drop all plans except for the current action
            AdjustIsWaitingForInteraction(1);
            DropAllPlans(currentAction.parentPlan);
            AdjustIsWaitingForInteraction(-1);

            ResetFullnessMeter();
            ResetHappinessMeter();
            ResetTirednessMeter();
            RemoveAllNonPersistentTraits();

            SetHomeStructure(alterEgoData.homeSturcture);
            ChangeFactionTo(alterEgoData.faction);
            ChangeRace(alterEgoData.race);
            AssignRole(alterEgoData.role);
            AssignClass(alterEgoData.characterClass);

            CancelAllJobsTargettingThisCharacter("target is not found", false);
            Messenger.Broadcast(Signals.CANCEL_CURRENT_ACTION, this, "target is not found");

            for (int i = 0; i < alterEgoData.traits.Count; i++) {
                AddTrait(alterEgoData.traits[i]);
            }
            currentAlterEgoName = alterEgoName;
            isSwitchingAlterEgo = false;
            Messenger.Broadcast(Signals.CHARACTER_SWITCHED_ALTER_EGO, this);
        } else {
            throw new Exception(this.name + " is trying to switch to alter ego " + alterEgoName + " but doesn't have an alter ego of that name!");
        }
    }
    public AlterEgoData GetAlterEgoData(string alterEgoName) {
        if (alterEgos.ContainsKey(alterEgoName)) {
            return alterEgos[alterEgoName];
        }
        return null;
    }
    #endregion
}