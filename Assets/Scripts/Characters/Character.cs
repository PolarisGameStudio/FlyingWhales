﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Actionables;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
using Traits;
using UnityEngine.Assertions;
using Interrupts;
using UnityEngine.EventSystems;
using UtilityScripts;

public class Character : Relatable, ILeader, IPointOfInterest, IJobOwner, IPlayerActionTarget {

    protected string _name;
    protected string _firstName;
    protected string _surName;
    protected int _id;
    protected float _actRate;
    protected bool _isDead;
    //protected bool _isFlirting;
    protected GENDER _gender;
    protected CharacterClass _characterClass;
    protected RaceSetting _raceSetting;
    // protected CharacterRole _role;
    protected Faction _faction;
    protected Minion _minion;
    protected LocationStructure _currentStructure; //what structure is this character currently in.
    protected Region _currentRegion;

    //Stats
    //protected SIDES _currentSide;
    protected int _currentHP;
    protected int _maxHP;
    protected int _level;
    protected int _experience;
    protected int _maxExperience;
    protected int _sp;

    //visuals
    public CharacterVisuals visuals { get; private set; }
    public BaseMapObjectVisual mapObjectVisual => marker;
    public int doNotRecoverHP { get; protected set; }
    public SEXUALITY sexuality { get; private set; }
    public int attackPowerMod { get; protected set; }
    public int speedMod { get; protected set; }
    public int maxHPMod { get; protected set; }
    public int attackPowerPercentMod { get; protected set; }
    public int speedPercentMod { get; protected set; }
    public int maxHPPercentMod { get; protected set; }
    public Region homeRegion { get; protected set; }
    public Settlement homeSettlement => homeStructure?.settlementLocation ?? null;
    public IDwelling homeStructure { get; protected set; }
    //public IRelationshipContainer relationshipContainer => currentAlterEgo.relationshipContainer;
    //public IRelationshipValidator relationshipValidator => currentAlterEgo.relationshipValidator;
    public List<INTERACTION_TYPE> advertisedActions { get; private set; }
    public int supply { get; set; }
    public int food { get; set; }
    public CharacterMarker marker { get; private set; }
    public JobQueueItem currentJob { get; private set; }
    public GoapPlan currentPlan { get; private set; }
    public ActualGoapNode currentActionNode { get; private set; }
    public ActualGoapNode previousCurrentActionNode { get; private set; }
    public Character lastAssaultedCharacter { get; private set; }
    public List<TileObject> items { get; private set; }
    public JobQueue jobQueue { get; private set; }
    public List<JobQueueItem> allJobsTargetingThis { get; private set; }
    public bool canCombat { get; private set; } //This should only be a getter but since we need to know when the value changes it now has a setter
    public List<Trait> traitsNeededToBeRemoved { get; private set; }
    public TrapStructure trapStructure { get; private set; }
    public bool isDisabledByPlayer { get; protected set; }
    public float speedModifier { get; private set; }
    public string deathStr { get; private set; }
    public TileObject tileObjectLocation { get; private set; }
    public CharacterTrait defaultCharacterTrait { get; private set; }
    public int isStoppedByOtherCharacter { get; private set; } //this is increased, when the action of another character stops this characters movement
    public Party ownParty { get; protected set; }
    public Party currentParty { get; protected set; }
    public Dictionary<RESOURCE, int> storedResources { get; protected set; }
    public int currentMissingTicks { get; protected set; }
    public bool isSettlementRuler { get; protected set; }
    public bool hasUnresolvedCrime { get; protected set; }
    public bool isConversing { get; protected set; }
    public bool isInLimbo { get; protected set; }
    public bool isLimboCharacter { get; protected set; }
    public bool hasSeenFire { get; protected set; }
    public LycanthropeData lycanData { get; protected set; }
    public List<JobQueueItem> forcedCancelJobsOnTickEnded { get; private set; }

    private List<System.Action> onLeaveAreaActions;
    private POI_STATE _state;
    // public Dictionary<int, Combat> combatHistory;

    //limiters
    private int _canWitnessValue; //if this is >= 0 then character can witness events
    private int _canMoveValue; //if this is >= 0 then character can move
    private int _canBeAtttackedValue; //if this is >= 0 then character can be attacked
    private int _canPerformValue; //if this is >= 0 then character can perform
    private int canTakeJobsValue; //if this is >= 0 then character can take jobs

    public bool canWitness => _canWitnessValue >= 0;
    public bool canMove => _canMoveValue >= 0;
    public bool canBeAtttacked => _canBeAtttackedValue >= 0;
    public bool canPerform => _canPerformValue >= 0;
    public bool canTakeJobs => canTakeJobsValue >= 0;

    //Needs
    public CharacterNeedsComponent needsComponent { get; private set; }

    //hostility
    public virtual int ignoreHostility { get; protected set; }

    //alter egos
    //public string currentAlterEgoName { get; private set; } //this character's currently active alter ego. Usually just Original.
    //public Dictionary<string, AlterEgoData> alterEgos { get; private set; }
    //public string originalClassName { get; private set; } //the class that this character started with
    private List<Action> pendingActionsAfterMultiThread; //List of actions to perform after a character is finished with all his/her multithread processing (This is to prevent errors while the character has a thread running)

    //misc
    public bool isFollowingPlayerInstruction { get; private set; } //is this character moving/attacking because of the players instruction
    public bool returnedToLife { get; private set; }
    public Tombstone grave { get; private set; }
    private WeightedDictionary<string> combatResultWeights;
    private readonly List<string> _overrideThoughts;

    //For Testing
    public List<string> locationHistory { get; private set; }
    public List<string> actionHistory { get; private set; }

    //Components / Managers
    public GoapPlanner planner { get; private set; }
    public BuildStructureComponent buildStructureComponent { get; private set; }
    public CharacterStateComponent stateComponent { get; private set; }
    public NonActionEventsComponent nonActionEventsComponent { get; private set; }
    [Obsolete("opinionComponent is obsolete and has been moved to the BaseRelationshipContainer")]
    public OpinionComponent opinionComponent { get; private set; }
    public InterruptComponent interruptComponent { get; private set; }
    public BehaviourComponent behaviourComponent { get; private set; }
    public MoodComponent moodComponent { get; private set; }
    public CharacterJobTriggerComponent jobComponent { get; private set; }
    public ReactionComponent reactionComponent { get; private set; }
    public LogComponent logComponent { get; private set; }
    public CombatComponent combatComponent { get; private set; }

    #region getters / setters
    public override string relatableName => _firstName;
    public virtual string name => _firstName;
    public string fullname => $"{_firstName} {_surName}";
    public string nameWithID => name;
    public string raceClassName {
        get {
            // if (Utilities.IsRaceBeast(race)) {
            //     return Utilities.NormalizeStringUpperCaseFirstLetterOnly(race.ToString()) + " " + role.name;
            // }
            //if(role.name == characterClass.className) {
            return $"{UtilityScripts.GameUtilities.GetNormalizedRaceAdjective(race)} {characterClass.className}";
            //}
            //return Utilities.GetNormalizedRaceAdjective(race) + " " + role.name + " " + characterClass.className;
        }
    }
    public override int id => _id;
    public bool isDead => this._isDead;
    public bool isFactionless { //is the character part of the neutral faction? or no faction?
        get {
            if (faction == null || FactionManager.Instance.neutralFaction == faction) {
                return true;
            } else {
                return false;
            }
        }
    }
    public bool isFriendlyFactionless { //is the character part of the friendly neutral faction? or no faction?
        get {
            if (faction == null || FactionManager.Instance.friendlyNeutralFaction == faction) {
                return true;
            } else {
                return false;
            }
        }
    }
    public bool isFactionLeader => faction != null && faction.leader == this;
    public bool isHoldingItem => items.Count > 0;
    public bool isAtHomeRegion => currentRegion == homeRegion && !currentParty.icon.isTravellingOutside;
    public bool isPartOfHomeFaction => homeRegion != null && faction != null && homeRegion.IsFactionHere(faction); //is this character part of the faction that owns his home settlement
    //public bool isFlirting => _isFlirting;
    public override GENDER gender => _gender;
    public RACE race => _raceSetting.race;
    public CharacterClass characterClass => _characterClass;
    public RaceSetting raceSetting => _raceSetting;
    // public CharacterRole role => _role;
    public Faction faction => _faction;
    public Faction factionOwner => _faction;
    //public Settlement currentArea => currentSettlement;
    public Region currentRegion {
        get {
            if (!IsInOwnParty()) {
                return currentParty.owner.currentRegion;
            }
            return _currentRegion;
        }
    }
    public Settlement currentSettlement => gridTileLocation != null 
        && gridTileLocation.buildSpotOwner.hexTileOwner != null ? 
        gridTileLocation.buildSpotOwner.hexTileOwner.settlementOnTile : null;
    public int level => _level;
    public int experience => _experience;
    public int maxExperience => _maxExperience;
    public int speed {
        get {
            int total = (int) ((_characterClass.baseSpeed + speedMod) * (1f + ((_raceSetting.speedModifier + speedPercentMod) / 100f)));
            if (total < 0) {
                return 1;
            }
            return total;
        }
    }
    public int attackPower {
        get {
            int total = (int) ((_characterClass.baseAttackPower + attackPowerMod) * (1f + ((_raceSetting.attackPowerModifier + attackPowerPercentMod) / 100f)));
            if (total < 0) {
                return 1;
            }
            return total;
        }
    }
    public int maxHP => _maxHP;
    public int currentHP => this._currentHP;
    public int attackSpeed => _characterClass.baseAttackSpeed; //in milliseconds, The lower the amount the faster the attack rate
    public Minion minion => _minion;
    public POINT_OF_INTEREST_TYPE poiType => POINT_OF_INTEREST_TYPE.CHARACTER;
    public LocationGridTile gridTileLocation {
        get {
            if (marker == null) {
                return null;
            }
            if (!IsInOwnParty()) {
                return currentParty.owner.gridTileLocation;
            }
            return GetLocationGridTileByXY(gridTilePosition.x, gridTilePosition.y);
        }
    }
    public Vector2Int gridTilePosition {
        get {
            if (marker == null) {
                throw new Exception($"{this.name} marker is null!");
            }
            return new Vector2Int(Mathf.FloorToInt(marker.anchoredPos.x), Mathf.FloorToInt(marker.anchoredPos.y));
        }
    }
    public POI_STATE state => _state;
    //public AlterEgoData currentAlterEgo {
    //    get {
    //        if (alterEgos == null || !alterEgos.ContainsKey(currentAlterEgoName)) {
    //            Debug.LogWarning(this.name + " Alter Ego Relationship Problem! Current alter ego is: " + currentAlterEgoName);
    //            return null;
    //        }
    //        return alterEgos[currentAlterEgoName];
    //    }
    //}
    public LocationStructure currentStructure {
        get {
            if (!IsInOwnParty()) {
                return currentParty.owner.currentStructure;
            }
            return _currentStructure;
        }
    }
    public float walkSpeed => raceSetting.walkSpeed + (raceSetting.walkSpeed * characterClass.walkSpeedMod);
    public float runSpeed => raceSetting.runSpeed + (raceSetting.runSpeed * characterClass.runSpeedMod);
    public Vector3 worldPosition => marker.transform.position;
    public Vector2 selectableSize => Vector2Int.one;
    public ProjectileReceiver projectileReceiver => marker.collisionTrigger.projectileReceiver;
    public JOB_OWNER ownerType => JOB_OWNER.CHARACTER;
    public bool isInCombat => stateComponent.currentState != null && stateComponent.currentState.characterState == CHARACTER_STATE.COMBAT;
    public Transform worldObject => marker.transform;
    public bool isStillConsideredAlive => minion == null /*&& !(this is Summon)*/ && !faction.isPlayerFaction;
    public Character isBeingCarriedBy => IsInOwnParty() ? null : currentParty.owner;
    public bool isMissing => currentMissingTicks > CharacterManager.Instance.CHARACTER_MISSING_THRESHOLD;
    public bool isBeingSeized => PlayerManager.Instance.player != null && PlayerManager.Instance.player.seizeComponent.seizedPOI == this;
    public bool isLycanthrope => lycanData != null;
    //public JobQueueItem currentJob => jobQueue.jobsInQueue.Count > 0 ? jobQueue.jobsInQueue[0] : null; //The current job is always the top of the queue
    public JobTriggerComponent jobTriggerComponent => jobComponent;
    public GameObject visualGO => marker.gameObject;
    #endregion
    
    public Character(string className, RACE race, GENDER gender) : this() {
        _id = UtilityScripts.Utilities.SetID(this);
        _gender = gender;
        RaceSetting raceSetting = RaceManager.Instance.racesDictionary[race.ToString()];
        _raceSetting = raceSetting.CreateNewCopy();
        AssignClass(className, true);
        SetName(RandomNameGenerator.GenerateRandomName(_raceSetting.race, _gender));
        GenerateSexuality();
        StartingLevel();
        visuals = new CharacterVisuals(this);
    }
    public Character(string className, RACE race, GENDER gender, SEXUALITY sexuality, int id = -1) : this() {
        _id = id == -1 ? UtilityScripts.Utilities.SetID(this) : id;
        _gender = gender;
        RaceSetting raceSetting = RaceManager.Instance.racesDictionary[race.ToString()];
        _raceSetting = raceSetting.CreateNewCopy();
        AssignClass(className, true);
        SetName(RandomNameGenerator.GenerateRandomName(_raceSetting.race, _gender));
        SetSexuality(sexuality);
        StartingLevel();
        visuals = new CharacterVisuals(this);
    }
    public Character(SaveDataCharacter data) {
        _id = UtilityScripts.Utilities.SetID(this, data.id);
        _gender = data.gender;
        SetSexuality(data.sexuality);
        AssignClass(data.className, true);
        RaceSetting raceSetting = RaceManager.Instance.racesDictionary[data.race.ToString()];
        _raceSetting = raceSetting.CreateNewCopy();
        SetName(data.name);
        visuals = new CharacterVisuals(data);
        
        isStoppedByOtherCharacter = data.isStoppedByOtherCharacter;

        _overrideThoughts = new List<string>();
        advertisedActions = new List<INTERACTION_TYPE>();
        stateComponent = new CharacterStateComponent(this);
        items = new List<TileObject>();
        jobQueue = new JobQueue(this);
        allJobsTargetingThis = new List<JobQueueItem>();
        traitsNeededToBeRemoved = new List<Trait>();
        onLeaveAreaActions = new List<Action>();
        pendingActionsAfterMultiThread = new List<Action>();
        trapStructure = new TrapStructure();
        //for testing
        locationHistory = new List<string>();
        actionHistory = new List<string>();
        planner = new GoapPlanner(this);

        SetIsDead(data.isDead);
    }
    public Character() {
        SetIsDead(false);
        _overrideThoughts = new List<string>();
        
        //Needs
        needsComponent = new CharacterNeedsComponent(this);
        
        //RPG
        SetExperience(0);

        //Traits
        CreateTraitContainer();

        // combatHistory = new Dictionary<int, Combat>();
        advertisedActions = new List<INTERACTION_TYPE>();
        items = new List<TileObject>();
        allJobsTargetingThis = new List<JobQueueItem>();
        traitsNeededToBeRemoved = new List<Trait>();
        onLeaveAreaActions = new List<Action>();
        pendingActionsAfterMultiThread = new List<Action>();
        forcedCancelJobsOnTickEnded = new List<JobQueueItem>();
        SetPOIState(POI_STATE.ACTIVE);
        needsComponent.ResetSleepTicks();
        ConstructResources();

        //for testing
        locationHistory = new List<string>();
        actionHistory = new List<string>();

        //hostiltiy
        ignoreHostility = 0;

        //Components
        stateComponent = new CharacterStateComponent(this);
        jobQueue = new JobQueue(this);
        trapStructure = new TrapStructure();
        planner = new GoapPlanner(this);
        nonActionEventsComponent = new NonActionEventsComponent(this);
        opinionComponent = new OpinionComponent(this);
        interruptComponent = new InterruptComponent(this);
        behaviourComponent = new BehaviourComponent(this);
        moodComponent = new MoodComponent(this);
        jobComponent = new CharacterJobTriggerComponent(this);
        reactionComponent = new ReactionComponent(this);
        logComponent = new LogComponent(this);
        combatComponent = new CombatComponent(this);
    }

    //This is done separately after all traits have been loaded so that the data will be accurate
    //It is because all traits are added again, this would mean that OnAddedTrait will also be called
    //Some values of character are modified by adding traits, so since adding trait will still be processed, it will get modified twice or more
    //For example, the Glutton trait adds fullnessDecreaseRate by 50%
    //Now when the fullnessDecreaseRate value is loaded the value of it already includes the Glutton trait modification
    //But since the Glutton trait will process the add trait function, fullnessDecreaseRate will add by 50% again
    //So for example if the saved value is 150, then the loaded value will be 300 (150+150)
    public void LoadAllStatsOfCharacter(SaveDataCharacter data) {
        //_doNotDisturb = data.doNotDisturb;
        //_doNotGetHungry = data.doNotGetHungry;
        //_doNotGetLonely = data.doNotGetLonely;
        //_doNotGetTired = data.doNotGetTired;

        _maxHP = data.maxHP;
        _currentHP = data.currentHP;
        _level = data.level;
        _experience = data.experience;
        _maxExperience = data.maxExperience;
        attackPowerMod = data.attackPowerMod;
        speedMod = data.speedMod;
        maxHPMod = data.maxHPMod;
        attackPowerPercentMod = data.attackPowerPercentMod;
        speedPercentMod = data.speedPercentMod;
        maxHPPercentMod = data.maxHPPercentMod;

        //currentInteractionTypes = data.currentInteractionTypes;
        supply = data.supply;
        canCombat = data.isCombatant;
        isDisabledByPlayer = data.isDisabledByPlayer;
        speedModifier = data.speedModifier;
        deathStr = data.deathStr;
        _state = data.state;

        moodComponent.Load(data);
        needsComponent.LoadAllStatsOfCharacter(data);

        ignoreHostility = data.ignoreHostility;
        returnedToLife = data.returnedToLife;
        
    }
    /// <summary>
    /// Initialize data for this character that is not safe to put in the constructor.
    /// Usually this is data that is dependent on the character being fully constructed.
    /// </summary>
    public virtual void Initialize() {
        ConstructDefaultActions();
        OnUpdateRace();
        OnUpdateCharacterClass();

        moodComponent.SetMoodValue(50);
        CreateOwnParty();

        needsComponent.Initialize();
        
        //supply
        SetSupply(UnityEngine.Random.Range(10, 61)); //Randomize initial supply per character (Random amount between 10 to 60.)
    }
    public virtual void InitialCharacterPlacement(LocationGridTile tile) {
        needsComponent.InitialCharacterPlacement();
        ConstructInitialGoapAdvertisementActions();
        marker.InitialPlaceMarkerAt(tile, false); //since normal characters are already placed in their areas.
        //AddInitialAwareness();
        SubscribeToSignals();
        for (int i = 0; i < traitContainer.allTraits.Count; i++) {
            traitContainer.allTraits[i].OnOwnerInitiallyPlaced(this);
        }
    }
    public void LoadInitialCharacterPlacement(LocationGridTile tile) {
        ConstructInitialGoapAdvertisementActions();
        //#if !WORLD_CREATION_TOOL
        //        GameDate gameDate = GameManager.Instance.Today();
        //        gameDate.AddTicks(1);
        //        SchedulingManager.Instance.AddEntry(gameDate, () => PlanGoapActions(), this);
        //#endif
        marker.InitialPlaceMarkerAt(tile, false); //since normal characters are already placed in their areas.
        //AddInitialAwareness();
        SubscribeToSignals();
        for (int i = 0; i < traitContainer.allTraits.Count; i++) {
            traitContainer.allTraits[i].OnOwnerInitiallyPlaced(this);
        }
    }

    #region Signals
    public void SubscribeToSignals() {
        if (minion != null) {
            logComponent.PrintLogErrorIfActive($"{name} is a minion and has subscribed to the signals!");
        }
        Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnOtherCharacterDied);
        Messenger.AddListener(Signals.TICK_STARTED, OnTickStarted);
        Messenger.AddListener(Signals.TICK_ENDED, OnTickEnded);
        Messenger.AddListener(Signals.HOUR_STARTED, OnHourStarted);
        Messenger.AddListener(Signals.DAY_STARTED, DailyGoapProcesses);
        Messenger.AddListener<Party>(Signals.PARTY_STARTED_TRAVELLING, OnLeaveArea);
        Messenger.AddListener<Party>(Signals.PARTY_DONE_TRAVELLING, OnArrivedAtArea);
        Messenger.AddListener<IPointOfInterest, string>(Signals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI, ForceCancelAllJobsTargetingPOI);
        Messenger.AddListener<IPointOfInterest, string, JOB_TYPE>(Signals.FORCE_CANCEL_ALL_JOB_TYPES_TARGETING_POI, ForceCancelJobTypesTargetingPOI);
        Messenger.AddListener<Settlement>(Signals.SUCCESS_INVASION_AREA, OnSuccessInvadeArea);
        Messenger.AddListener<Character, CharacterState>(Signals.CHARACTER_STARTED_STATE, OnCharacterStartedState);
        Messenger.AddListener<Character, CharacterState>(Signals.CHARACTER_ENDED_STATE, OnCharacterEndedState);
        Messenger.AddListener<Character>(Signals.SCREAM_FOR_HELP, HeardAScream);
        Messenger.AddListener<ActualGoapNode>(Signals.ACTION_PERFORMED, OnActionPerformed);
        Messenger.AddListener<Character, IPointOfInterest, Interrupt>(Signals.INTERRUPT_STARTED, OnInterruptStarted);
        Messenger.AddListener<IPointOfInterest>(Signals.ON_SEIZE_POI, OnSeizePOI);
        //Messenger.AddListener<Character>(Signals.ON_SEIZE_CHARACTER, OnSeizeOtherCharacter);
        //Messenger.AddListener<TileObject>(Signals.ON_SEIZE_TILE_OBJECT, OnSeizeTileObject);
        Messenger.AddListener<Character>(Signals.CHARACTER_MISSING, OnCharacterMissing);
        Messenger.AddListener<Character>(Signals.CHARACTER_NO_LONGER_MISSING, OnCharacterNoLongerMissing);
        //Messenger.AddListener<ActualGoapNode>(Signals.ACTION_PERFORMED, OnCharacterPerformedAction);
        needsComponent.SubscribeToSignals();
        jobComponent.SubscribeToListeners();
    }
    public virtual void UnsubscribeSignals() {
        Messenger.RemoveListener<Character>(Signals.CHARACTER_DEATH, OnOtherCharacterDied);
        Messenger.RemoveListener(Signals.TICK_STARTED, OnTickStarted);
        Messenger.RemoveListener(Signals.TICK_ENDED, OnTickEnded);
        Messenger.RemoveListener(Signals.HOUR_STARTED, OnHourStarted);
        Messenger.RemoveListener(Signals.DAY_STARTED, DailyGoapProcesses);
        Messenger.RemoveListener<Party>(Signals.PARTY_STARTED_TRAVELLING, OnLeaveArea);
        Messenger.RemoveListener<Party>(Signals.PARTY_DONE_TRAVELLING, OnArrivedAtArea);
        Messenger.RemoveListener<IPointOfInterest, string>(Signals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI, ForceCancelAllJobsTargetingPOI);
        Messenger.RemoveListener<IPointOfInterest, string, JOB_TYPE>(Signals.FORCE_CANCEL_ALL_JOB_TYPES_TARGETING_POI, ForceCancelJobTypesTargetingPOI);
        Messenger.RemoveListener<Settlement>(Signals.SUCCESS_INVASION_AREA, OnSuccessInvadeArea);
        Messenger.RemoveListener<Character, CharacterState>(Signals.CHARACTER_STARTED_STATE, OnCharacterStartedState);
        Messenger.RemoveListener<Character, CharacterState>(Signals.CHARACTER_ENDED_STATE, OnCharacterEndedState);
        Messenger.RemoveListener<Character>(Signals.SCREAM_FOR_HELP, HeardAScream);
        Messenger.RemoveListener<ActualGoapNode>(Signals.ACTION_PERFORMED, OnActionPerformed);
        Messenger.RemoveListener<Character, IPointOfInterest, Interrupt>(Signals.INTERRUPT_STARTED, OnInterruptStarted);
        Messenger.RemoveListener<IPointOfInterest>(Signals.ON_SEIZE_POI, OnSeizePOI);
        //Messenger.RemoveListener<Character>(Signals.ON_SEIZE_CHARACTER, OnSeizeOtherCharacter);
        //Messenger.RemoveListener<TileObject>(Signals.ON_SEIZE_TILE_OBJECT, OnSeizeTileObject);
        Messenger.RemoveListener<Character>(Signals.CHARACTER_MISSING, OnCharacterMissing);
        Messenger.RemoveListener<Character>(Signals.CHARACTER_NO_LONGER_MISSING, OnCharacterNoLongerMissing);
        //Messenger.RemoveListener<ActualGoapNode>(Signals.ACTION_PERFORMED, OnCharacterPerformedAction);
        needsComponent.UnsubscribeToSignals();
        jobComponent.UnsubscribeListeners();
    }
    #endregion

    #region Listeners
    private void OnCharacterExitedArea(Settlement settlement, Character character) {
        if (character.id == this.id) {
            //Clear terrifying characters of this character if he/she leaves the settlement
            //marker.ClearTerrifyingObjects();
        } else {
            if (marker == null) {
                throw new Exception($"Marker of {this.name} is null!");
            }
            //remove the character that left the settlement from anyone elses list of terrifying characters.
            //if (marker.terrifyingObjects.Count > 0) {
            //    if (character.IsInOwnParty()) {
            //        marker.RemoveTerrifyingObject(character);
            //        if (character.ownParty.isCarryingAnyPOI) {
            //            marker.RemoveTerrifyingObject(character.ownParty.carriedPOI);
            //        }
            //    } else {
            //        marker.RemoveTerrifyingObject(character.currentParty.owner);
            //        if (character.currentParty.isCarryingAnyPOI) {
            //            marker.RemoveTerrifyingObject(character.currentParty.carriedPOI);
            //        }
            //    }
            //    //for (int i = 0; i < party.characters.Count; i++) {
            //    //    marker.RemoveTerrifyingObject(party.characters[i]);
            //    //}
            //}
        }
    }
    /// <summary>
    /// Listener for when the player successfully invades an settlement. And this character is still alive.
    /// </summary>
    /// <param name="settlementinvaded settlement.</param>
    protected virtual void OnSuccessInvadeArea(Settlement settlement) {
        if (currentSettlement == settlement && minion == null) {
            StopCurrentActionNode(false);
            if (stateComponent.currentState != null) {
                stateComponent.ExitCurrentState();
            }
            //else if (stateComponent.stateToDo != null) {
            //    stateComponent.SetStateToDo(null);
            //}
            currentRegion.RemoveCharacterFromLocation(this);
            //combatComponent.ClearAvoidInRange(false);
            //combatComponent.ClearHostilesInRange(false);
            //marker.ClearPOIsInVisionRange();

            UnsubscribeSignals();
            traitContainer.RemoveAllNonPersistentTraits(this);
            //ClearAllAwareness();
            CancelAllJobs();
            SchedulingManager.Instance.ClearAllSchedulesBy(this);
            if (marker != null) {
                DestroyMarker();
            }
        }
    }
    #endregion

    #region Sexuality
    private void GenerateSexuality() {
        if (UtilityScripts.GameUtilities.IsRaceBeast(race)) {
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
    public void SetSexuality(SEXUALITY sexuality) {
        this.sexuality = sexuality;
    }
    #endregion

    #region Marker
    public void CreateMarker() {
        GameObject portraitGO = ObjectPoolManager.Instance.InstantiateObjectFromPool("CharacterMarker", Vector3.zero, Quaternion.identity, InnerMapManager.Instance.transform);
        CharacterMarker _marker = portraitGO.GetComponent<CharacterMarker>();
        _marker.SetCharacter(this);
        SetCharacterMarker(_marker);
    }
    public void DestroyMarker(LocationGridTile destroyedAt = null) {
        if (destroyedAt == null) {
            gridTileLocation.RemoveCharacterHere(this);
        } else {
            destroyedAt.RemoveCharacterHere(this);
        }
        ObjectPoolManager.Instance.DestroyObject(marker);
        SetCharacterMarker(null);
        Messenger.Broadcast(Signals.CHECK_APPLICABILITY_OF_ALL_JOBS_TARGETING, this as IPointOfInterest);
    }
    public void DisableMarker() {
        marker.gameObject.SetActive(false);
        gridTileLocation.RemoveCharacterHere(this);
    }
    public void EnableMarker() {
        marker.gameObject.SetActive(true);
    }
    private void SetCharacterMarker(CharacterMarker marker) {
        this.marker = marker;
    }
    public void AdjustSpeedModifier(float amount) {
        speedModifier += amount;
        if (marker != null) {
            marker.UpdateSpeed();
        }
    }
    public void PerTickDuringMovement() {
        for (int i = 0; i < traitContainer.allTraits.Count; i++) {
            Traits.Trait trait = traitContainer.allTraits[i];
            if (trait.PerTickOwnerMovement()) {
                break;
            }
        }
    }
    #endregion

    //Changes character's side
    //public void SetSide(SIDES side) {
    //    this._currentSide = side;
    //}
    //Character's death
    public void SetIsDead(bool isDead) {
        _isDead = isDead;
    }
    public void RaiseFromDeath(int level = 1, System.Action<Character> onReturnToLifeAction = null, Faction faction = null, RACE race = RACE.SKELETON, string className = "") {
        if (faction == null) {
            GameManager.Instance.StartCoroutine(Raise(this, level, onReturnToLifeAction, FactionManager.Instance.neutralFaction, race, className));
        } else {
            GameManager.Instance.StartCoroutine(Raise(this, level, onReturnToLifeAction, faction, race, className));
        }
    }
    private IEnumerator Raise(Character target, int level, System.Action<Character> onReturnToLifeAction, Faction faction, RACE race, string className) {
        target.marker.PlayAnimation("Raise Dead");
        yield return new WaitForSeconds(0.7f);
        target.ReturnToLife(faction, race, className);
        target.SetLevel(level);
        yield return null;
        onReturnToLifeAction?.Invoke(this);
    }
    private void ReturnToLife(Faction faction, RACE race, string className) {
        if (_isDead) {
            returnedToLife = true;
            SetIsDead(false);
            SubscribeToSignals();
            ResetToFullHP();
            SetPOIState(POI_STATE.ACTIVE);
            ChangeFactionTo(faction);
            ChangeRace(race);
            // AssignRole(CharacterRole.SOLDIER);
            // if (string.IsNullOrEmpty(className)) {
            //     AssignClassByRole(this.role);
            // } else {
            //     AssignClass(className);
            // }
            AssignClass(className);
            needsComponent.ResetFullnessMeter();
            needsComponent.ResetTirednessMeter();
            needsComponent.ResetHappinessMeter();
            ownParty.ReturnToLife();
            marker.OnReturnToLife();
            if (grave != null) {
                marker.PlaceMarkerAt(grave.gridTileLocation);
                grave.gridTileLocation.structure.RemovePOI(grave);
                SetGrave(null);
            }
            traitContainer.RemoveTrait(this, "Dead");
            for (int i = 0; i < traitContainer.allTraits.Count; i++) {
                traitContainer.allTraits[i].OnReturnToLife(this);
            }
            //RemoveAllNonPersistentTraits();
            //ClearAllAwareness();
            //Settlement gloomhollow = LandmarkManager.Instance.GetAreaByName("Gloomhollow");
            MigrateHomeStructureTo(null);
            needsComponent.SetTirednessForcedTick(0);
            needsComponent.SetFullnessForcedTick(0);
            needsComponent.SetHasCancelledSleepSchedule(false);
            needsComponent.ResetSleepTicks();
            Messenger.Broadcast(Signals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI, this as IPointOfInterest, "");
            //MigrateHomeTo(null);
            //AddInitialAwareness(gloomhollow);
            Messenger.Broadcast(Signals.CHARACTER_RETURNED_TO_LIFE, this);
        }
    }
    public virtual void Death(string cause = "normal", ActualGoapNode deathFromAction = null, Character responsibleCharacter = null, Log _deathLog = null, LogFiller[] deathLogFillers = null) {
        if (minion != null) {
            minion.Death(cause, deathFromAction, responsibleCharacter, _deathLog, deathLogFillers);
            return;
        }
        if (!_isDead) {
            //if (currentAlterEgoName != CharacterManager.Original_Alter_Ego) {
            //    SwitchAlterEgo(CharacterManager.Original_Alter_Ego); //revert the character to his/her original alter ego
            //}
            SetIsConversing(false);
            //SetIsFlirting(false);
            Region deathLocation = currentRegion;
            LocationStructure deathStructure = currentStructure;
            LocationGridTile deathTile = gridTileLocation;
            for (int i = 0; i < traitContainer.allTraits.Count; i++) {
                if (traitContainer.allTraits[i].OnDeath(this)) {
                    i--;
                }
            }
            if (lycanData != null) {
                lycanData.LycanDies(this, cause, deathFromAction, responsibleCharacter, _deathLog, deathLogFillers);
            }
            //------------------------ Things that are above this line are called before letting the character die so that if we need things done before actually setting the death of character we can do it here like cleaning up necessary things, etc.
            SetIsDead(true);
            if (isLimboCharacter && isInLimbo) {
                //If a limbo character dies while in limbo, that character should not process death, instead he/she will be removed from the list
                CharacterManager.Instance.RemoveLimboCharacter(this);
                return;
            }
            UnsubscribeSignals();
            SetPOIState(POI_STATE.INACTIVE);
            //CombatManager.Instance.ReturnCharacterColorToPool(_characterColor);

            if (currentRegion == null) {
                throw new Exception(
                    $"Current Region Location of {this.name} is null! Please use command /l_character_location_history [Character Name/ID] in console menu to log character's location history. (Use '~' to show console menu)");
            }
            if (stateComponent.currentState != null) {
                stateComponent.ExitCurrentState();
                //if (stateComponent.currentState != null) {
                //    stateComponent.currentState.OnExitThisState();
                //}
            }
            //else if (stateComponent.stateToDo != null) {
            //    stateComponent.SetStateToDo(null);
            //}
            //if (deathFromAction != null) { //if this character died from an action, do not cancel the action that he/she died from. so that the action will just end as normal.
            //    CancelAllJobsTargettingThisCharacterExcept(deathFromAction, "target is already dead", false);
            //} else {
            //    CancelAllJobsTargettingThisCharacter("target is already dead", false);
            //}
            //StopCurrentActionNode();
            //ForceCancelAllJobsTargettingCharacter(false, "target is already dead");
            ////Messenger.Broadcast(Signals.CANCEL_CURRENT_ACTION, this, "target is already dead");
            //if (jobQueue.jobsInQueue.Count > 0) {
            //    jobQueue.CancelAllJobs();
            //}
            Messenger.Broadcast(Signals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI, this as IPointOfInterest, "target is already dead");
            CancelAllJobs();

            if (currentSettlement != null && isHoldingItem) {
                DropAllItems(deathTile);
            } else {
                for (int i = 0; i < items.Count; i++) {
                    if (RemoveItem(i)) {
                        i--;
                    }
                }
            }
            //if (currentRegion == null) {
            //    if (currentArea != null && isHoldingItem) {
            //        DropAllTokens(currentArea, currentStructure, deathTile, true);
            //    } else {
            //        for (int i = 0; i < items.Count; i++) {
            //            if (RemoveToken(i)) {
            //                i--;
            //            }
            //        }
            //    }
            //} else {
            //    List<SpecialToken> all = new List<SpecialToken>(items);
            //    for (int i = 0; i < all.Count; i++) {
            //        RemoveToken(all[i]);
            //    }
            //}


            //clear traits that need to be removed
            traitsNeededToBeRemoved.Clear();

            bool wasOutsideSettlement = currentSettlement == null;

            //bool wasOutsideSettlement = false;
            //if (currentRegion != null) {
            //    wasOutsideSettlement = true;
            //    currentRegion.RemoveCharacterFromLocation(this);
            //}

            Character carrier = isBeingCarriedBy;
            if (carrier != null) {
                carrier.UncarryPOI(this);
            }
            ownParty.PartyDeath();
            currentRegion?.RemoveCharacterFromLocation(this);
            SetRegionLocation(deathLocation); //set the specific location of this party, to the location it died at
            SetCurrentStructureLocation(deathStructure, false);

            //if (this.race != RACE.SKELETON) {
            //    deathLocation.AddCorpse(this, deathStructure, deathTile);
            //}


            //if (faction != null) {
            //    faction.LeaveFaction(this); //remove this character from it's factions list of characters
            //}
            //if (faction != null && faction.leader == this) {
            //    faction.SetNewLeader();
            //}

            // if (_role != null) {
            //     _role.OnDeath(this);
            // }

            if (homeRegion != null) {
                Region home = homeRegion;
                IDwelling homeStructure = this.homeStructure;
                homeRegion.RemoveResident(this);
                SetHomeRegion(home); //keep this data with character to prevent errors
                SetHomeStructure(homeStructure); //keep this data with character to prevent errors
            }
            //if (homeSettlement != null) {
            //    Settlement home = homeSettlement;
            //    Dwelling homeStructure = this.homeStructure;
            //    homeSettlement.RemoveResident(this);
            //    SetHome(home); //keep this data with character to prevent errors
            //    SetHomeStructure(homeStructure); //keep this data with character to prevent errors
            //}

            //List<Character> characterRels = new List<Character>(this.relationships.Keys.ToList());
            //for (int i = 0; i < characterRels.Count; i++) {
            //    RemoveRelationship(characterRels[i]);
            //}

            //if (_minion != null) {
            //    PlayerManager.Instance.player.RemoveMinion(_minion);
            //}

            //ObjectPoolManager.Instance.DestroyObject(marker.gameObject);
            //deathTile.RemoveCharacterHere(this);

            //RemoveAllTraitsByType(TRAIT_TYPE.CRIMINAL); //remove all criminal type traits

            //RemoveAllNonPersistentTraits();

            SetHP(0);

            marker.OnDeath(deathTile);

            //SetNumWaitingForGoapThread(0); //for raise dead
            //Dead dead = new Dead();
            //dead.SetCharacterResponsibleForTrait(responsibleCharacter);
            traitContainer.AddTrait(this, "Dead", responsibleCharacter, gainedFromDoing: deathFromAction);

            logComponent.PrintLogIfActive($"{name} died of {cause}");
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
                //AddHistory(deathLog);
                deathLog.AddLogToInvolvedObjects();
                //specificLocation.AddHistory(deathLog);
                PlayerManager.Instance.player.ShowNotificationFrom(this, deathLog);
            } else {
                deathLog = _deathLog;
            }
            deathStr = UtilityScripts.Utilities.LogReplacer(deathLog);
            Messenger.Broadcast(Signals.CHARACTER_DEATH, this);

            //for (int i = 0; i < traitContainer.allTraits.Count; i++) {
            //    if (traitContainer.allTraits[i].OnAfterDeath(this, cause, deathFromAction, responsibleCharacter, _deathLog, deathLogFillers)) {
            //        i--;
            //    }
            //}
        }
    }
    public void SetGrave(Tombstone grave) {
        this.grave = grave;
    }

    #region Roles
    // public void AssignRole(CharacterRole role, bool updateCombatantState = true) {
    //     bool wasRoleChanged = false;
    //     if (_role != null) {
    //         if (_role.roleType == role.roleType) {
    //             //If character role is being changed to same role, do not change it
    //             return;
    //         }
    //         _role.OnChange(this);
    //         wasRoleChanged = true;
    //     }
    //     _role = role;
    //     if (_role != null) {
    //         _role.OnAssign(this);
    //     }
    //     if (wasRoleChanged) {
    //         Messenger.Broadcast(Signals.ROLE_CHANGED, this);
    //     }
    //     //if (updateCombatantState) {
    //     //    UpdateCanCombatantState();
    //     //}
    // }
    #endregion

    #region Character Class
    public virtual string GetClassForRole(CharacterRole role) {
        if (role == CharacterRole.BEAST) {
            return UtilityScripts.GameUtilities.GetRespectiveBeastClassNameFromByRace(race);
        } else {
            string className = CharacterManager.Instance.GetRandomClassByIdentifier(role.classNameOrIdentifier);
            if (className != string.Empty) {
                return className;
            } else {
                return role.classNameOrIdentifier;
            }
        }
    }
    public void RemoveClass() {
        if (_characterClass == null) { return; }
        traitContainer.RemoveTrait(this, traitContainer.GetNormalTrait<Trait>(_characterClass.traitNames)); //Remove traits from class
        _characterClass = null;
    }
    public void AssignClass(string className, bool isInitial = false) {
        if (CharacterManager.Instance.HasCharacterClass(className)) {
            AssignClass(CharacterManager.Instance.CreateNewCharacterClass(className), isInitial);
        } else {
            throw new Exception($"There is no class named {className} but it is being assigned to {this.name}");
        }
    }
    protected void OnUpdateCharacterClass() {
        if (_currentHP > maxHP) {
            _currentHP = maxHP;
        }
        //if (_sp > _maxSP) {
        //    _sp = _maxSP;
        //}
        for (int i = 0; i < _characterClass.traitNames.Length; i++) {
            traitContainer.AddTrait(this, _characterClass.traitNames[i]);
        }
        visuals.UpdateAllVisuals(this);
        if (minion != null) {
            minion.SetAssignedDeadlySinName(_characterClass.className);
        }
        UpdateCanCombatState();
    }
    public void AssignClass(CharacterClass characterClass, bool isInitial = false) {
        CharacterClass previousClass = _characterClass;
        if (previousClass != null) {
            //This means that the character currently has a class and it will be replaced with a new class
            for (int i = 0; i < previousClass.traitNames.Length; i++) {
                traitContainer.RemoveTrait(this, previousClass.traitNames[i]); //Remove traits from class
            }
        }
        _characterClass = characterClass;
        behaviourComponent.OnChangeClass(_characterClass, previousClass);
        if (!isInitial) {
            OnUpdateCharacterClass();
            Messenger.Broadcast(Signals.CHARACTER_CLASS_CHANGE, this, previousClass, _characterClass);
        }
    }
    #endregion

    #region Jobs
    public void SetCurrentJob(JobQueueItem job) {
        currentJob = job;
    }
    //public JobQueueItem GetCurrentJob() {
    //    if(currentActionNode != null && jobQueue.jobsInQueue.Count > 0) {
    //        return jobQueue.jobsInQueue[0];
    //    }
    //    return null;
    //}
    public void AddJobTargetingThis(JobQueueItem job) {
        allJobsTargetingThis.Add(job);
    }
    public bool RemoveJobTargetingThis(JobQueueItem job) {
        return allJobsTargetingThis.Remove(job);
    }
    public void ForceCancelAllJobsTargettingThisCharacter() {
        for (int i = 0; i < allJobsTargetingThis.Count; i++) {
            JobQueueItem job = allJobsTargetingThis[i];
            if (job.ForceCancelJob()) {
                i--;
            }
        }
    }
    public void ForceCancelAllJobsTargettingThisCharacter(JOB_TYPE jobType) {
        for (int i = 0; i < allJobsTargetingThis.Count; i++) {
            JobQueueItem job = allJobsTargetingThis[i];
            if (job.jobType == jobType) {
                if (job.ForceCancelJob()) {
                    i--;
                }

            }
        }
    }
    public void ForceCancelAllJobsTargettingThisCharacterExcept(JOB_TYPE jobType, string conditionKey, Character otherCharacter) {
        for (int i = 0; i < allJobsTargetingThis.Count; i++) {
            if (allJobsTargetingThis[i] is GoapPlanJob) {
                GoapPlanJob job = allJobsTargetingThis[i] as GoapPlanJob;
                if (job.jobType == jobType && job.assignedCharacter != otherCharacter && job.HasGoalConditionKey(conditionKey)) {
                    if (job.ForceCancelJob()) {
                        i--;
                    }
                }
            }
        }
    }
    private void ForceCancelAllJobsTargetingPOI(IPointOfInterest target, string reason) {
        for (int i = 0; i < jobQueue.jobsInQueue.Count; i++) {
            JobQueueItem job = jobQueue.jobsInQueue[i];
            if (job is GoapPlanJob) {
                GoapPlanJob goapJob = job as GoapPlanJob;
                if (goapJob.targetPOI == target) {
                    if (goapJob.ForceCancelJob(false, reason)) {
                        i--;
                    }
                }
            }
        }
    }
    private void ForceCancelJobTypesTargetingPOI(IPointOfInterest target, string reason, JOB_TYPE jobType) {
        for (int i = 0; i < jobQueue.jobsInQueue.Count; i++) {
            JobQueueItem job = jobQueue.jobsInQueue[i];
            if (job.jobType == jobType && job is GoapPlanJob) {
                GoapPlanJob goapJob = job as GoapPlanJob;
                if (goapJob.targetPOI == target) {
                    if (goapJob.ForceCancelJob(false, reason)) {
                        i--;
                    }
                }
            }
        }
    }
    public bool HasJobTargetingThis(params JOB_TYPE[] jobTypes) {
        for (int i = 0; i < allJobsTargetingThis.Count; i++) {
            JobQueueItem job = allJobsTargetingThis[i];
            for (int j = 0; j < jobTypes.Length; j++) {
                if (job.jobType == jobTypes[j]) {
                    return true;
                }
            }
        }
        return false;
    }
    public int GetNumOfJobsTargettingThisCharacter(JOB_TYPE jobType) {
        int count = 0;
        for (int i = 0; i < allJobsTargetingThis.Count; i++) {
            JobQueueItem job = allJobsTargetingThis[i];
            if (job.jobType == jobType) {
                count++;
            }
        }
        return count;
    }
    public GoapPlanJob GetJobTargettingThisCharacter(JOB_TYPE jobType, string conditionKey) {
        for (int i = 0; i < allJobsTargetingThis.Count; i++) {
            if (allJobsTargetingThis[i] is GoapPlanJob) {
                GoapPlanJob job = allJobsTargetingThis[i] as GoapPlanJob;
                if (job.jobType == jobType && job.HasGoalConditionKey(conditionKey)) {
                    return job;
                }
            }
        }
        return null;
    }
    public GoapPlanJob GetJobTargettingThisCharacter(JOB_TYPE jobType) {
        for (int i = 0; i < allJobsTargetingThis.Count; i++) {
            if (allJobsTargetingThis[i] is GoapPlanJob) {
                GoapPlanJob job = allJobsTargetingThis[i] as GoapPlanJob;
                if (job.jobType == jobType) {
                    return job;
                }
            }
        }
        return null;
    }
    private void CheckApprehendRelatedJobsOnLeaveLocation() {
        ForceCancelAllJobsTargettingThisCharacter(JOB_TYPE.APPREHEND);
        CancelAllJobs(JOB_TYPE.APPREHEND);
        //All apprehend jobs that are being done by this character must be unassigned
        //for (int i = 0; i < allGoapPlans.Count; i++) {
        //    GoapPlan plan = allGoapPlans[i];
        //    if (plan.job != null && plan.job.jobType == JOB_TYPE.APPREHEND) {
        //        plan.job.UnassignJob();
        //        i--;
        //    }
        //}
    }
    public void CancelOrUnassignRemoveTraitRelatedJobs() {
        ForceCancelAllJobsTargettingThisCharacter(JOB_TYPE.REMOVE_STATUS);
        CancelAllJobs(JOB_TYPE.REMOVE_STATUS);
        //TODO:
        //All remove trait jobs that are being done by this character must be unassigned
        //for (int i = 0; i < allGoapPlans.Count; i++) {
        //    GoapPlan plan = allGoapPlans[i];
        //    if (plan.job != null && plan.job.jobType == JOB_TYPE.REMOVE_TRAIT) {
        //        plan.job.UnassignJob();
        //        i--;
        //    }
        //}
    }
    private bool CreateJobsOnEnterVisionWithCharacter(Character targetCharacter) {
        string log = $"{name} saw {targetCharacter.name}, will try to create jobs on enter vision...";
        if (!CanCharacterReact(targetCharacter)) {
            log += "\nCharacter cannot react!";
            logComponent.PrintLogIfActive(log);
            return true;
        }
        bool hasCreatedJob = false;
        log += "\nChecking source character traits...";
        for (int i = 0; i < traitContainer.allTraits.Count; i++) {
            log += $"\n- {traitContainer.allTraits[i].name}";
            if (traitContainer.allTraits[i].OnSeePOI(targetCharacter, this)) {
                log += ": created a job!";
                hasCreatedJob = true;
            } else {
                log += ": did not create a job!";
            }
        }

        log += "\nChecking target character traits...";
        for (int i = 0; i < targetCharacter.traitContainer.allTraits.Count; i++) {
            log += $"\n- {targetCharacter.traitContainer.allTraits[i].name}";
            if (targetCharacter.traitContainer.allTraits[i].CreateJobsOnEnterVisionBasedOnTrait(targetCharacter, this)) {
                hasCreatedJob = true;
                log += ": created a job!";
            } else {
                log += ": did not create a job!";
            }
        }
        logComponent.PrintLogIfActive(log);
        return hasCreatedJob;
    }
    public bool CreateJobsOnEnterVisionWith(IPointOfInterest targetPOI) {
        if (targetPOI is Character) {
            return CreateJobsOnEnterVisionWithCharacter(targetPOI as Character);
        }
        string log = $"{name} saw {targetPOI.name}, will try to create jobs on enter vision...";
        if (!CanCharacterReact(targetPOI)) {
            log += "\nCharacter cannot react!";
            logComponent.PrintLogIfActive(log);
            return true;
        }
        bool hasCreatedJob = false;
        log += "\nChecking source character traits...";
        for (int i = 0; i < traitContainer.allTraits.Count; i++) {
            log += $"\n- {traitContainer.allTraits[i].name}";
            if (traitContainer.allTraits[i].OnSeePOI(targetPOI, this)) {
                log += ": created a job!";
                hasCreatedJob = true;
            } else {
                log += ": did not create a job!";
            }
        }
        log += "\nChecking target poi traits...";
        for (int i = 0; i < targetPOI.traitContainer.allTraits.Count; i++) {
            log += $"\n- {targetPOI.traitContainer.allTraits[i].name}";
            if (targetPOI.traitContainer.allTraits[i].CreateJobsOnEnterVisionBasedOnTrait(targetPOI, this)) {
                log += ": created a job!";
                hasCreatedJob = true;
            } else {
                log += ": did not create a job!";
            }
        }

        logComponent.PrintLogIfActive(log);
        return hasCreatedJob;
    }
    public bool CreateJobsOnTargetGainTrait(IPointOfInterest targetPOI, Trait traitGained) {
        string log = $"{targetPOI.name} gained trait {traitGained.name}, will try to create jobs based on it...";
        if (!CanCharacterReact(targetPOI)) {
            log += "\nCharacter cannot react!";
            logComponent.PrintLogIfActive(log);
            return true;
        }
        bool hasCreatedJob = false;
        log += "\nChecking trait...";
        if (traitGained.CreateJobsOnEnterVisionBasedOnTrait(targetPOI, this)) {
            log += ": created a job!";
            hasCreatedJob = true;
        } else {
            log += ": did not create a job!";
        }

        logComponent.PrintLogIfActive(log);
        return hasCreatedJob;
    }
    /// <summary>
    /// Force this character to create an undermine job towards the target character.
    /// Only cases that this will not happen is:
    /// - if target character is already dead
    /// - this character already has an undermine job in his/her job queue
    /// </summary>
    /// <param name="targetCharacter">The character to undermine.</param>
    public void ForceCreateUndermineJob(Character targetCharacter, string reason) {
        if (!targetCharacter.isDead) {
            CreateUndermineJobOnly(targetCharacter, reason);
        }
    }
    public bool CreateUndermineJobOnly(Character targetCharacter, string reason, SHARE_INTEL_STATUS status = SHARE_INTEL_STATUS.INFORMED) {
        if (jobQueue.HasJob(JOB_TYPE.UNDERMINE, targetCharacter)) {
            return false;
        }
        if (traitContainer.HasTrait("Diplomatic")) {
            return false;
        }
        // if (status == SHARE_INTEL_STATUS.WITNESSED) {
        //     //When creating undermine job and the creator of the job witnessed the event that caused him/her to undermine, mutate undermine job to knockout job
        //     //This means that all undermine jobs that are caused by witnessing an event will become knockout jobs
        //     return CreateKnockoutJob(targetCharacter);
        // }
        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.UNDERMINE, new GoapEffect(GOAP_EFFECT_CONDITION.HAS_TRAIT_EFFECT, "Negative", false, GOAP_EFFECT_TARGET.TARGET), targetCharacter, this);
        logComponent.PrintLogIfActive(
            $"Added an UNDERMINE ENEMY Job: negative trait to {this.name} with target {targetCharacter.name}");
        jobQueue.AddJobInQueue(job);

        Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", $"{reason}_and_undermine");
        log.AddToFillers(this, name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        // log.AddToFillers(null, currentMoodType.ToString().ToLower(), LOG_IDENTIFIER.STRING_1);
        logComponent.AddHistory(log);

        // PlayerManager.Instance.player.ShowNotificationFrom(log, this, false);
        return true;
    }
    public void CancelAllJobs(string reason = "") {
        //AdjustIsWaitingForInteraction(1);
        //StopCurrentActionNode(reason: reason);
        for (int i = 0; i < jobQueue.jobsInQueue.Count; i++) {
            if (jobQueue.jobsInQueue[i].CancelJob(reason: reason)) {
                i--;
            }
        }
        //if (homeSettlement != null) {
        //    homeSettlement.jobQueue.UnassignAllJobsTakenBy(this);
        //}

        //StopCurrentAction(false, reason: reason);
        //for (int i = 0; i < allGoapPlans.Count; i++) {
        //    if (DropPlan(allGoapPlans[i])) {
        //        i--;
        //    }
        //}
        //AdjustIsWaitingForInteraction(-1);
    }
    public void CancelAllJobs(JOB_TYPE jobType) {
        for (int i = 0; i < jobQueue.jobsInQueue.Count; i++) {
            JobQueueItem job = jobQueue.jobsInQueue[i];
            if (job.jobType == jobType) {
                if (job.CancelJob()) {
                    i--;
                }
            }
        }
    }
    public void CancelAllJobsExceptForCurrent(bool shouldDoAfterEffect = true) {
        if (currentJob != null) {
            for (int i = 0; i < jobQueue.jobsInQueue.Count; i++) {
                JobQueueItem job = jobQueue.jobsInQueue[i];
                if (job != currentJob) {
                    if (job.CancelJob(shouldDoAfterEffect)) {
                        i--;
                    }
                }
            }
        }
    }
    public bool CanCurrentJobBeOverriddenByJob(JobQueueItem job) {
        return false;
        ////GENERAL RULE: Plans/States that have no jobs are always the lowest priority
        ////Current job cannot be overriden by null job
        //if (job == null) {
        //    return false;
        //}
        //if (GetNormalTrait<Trait>("Berserked") != null /*||(stateComponent.stateToDo != null && stateComponent.stateToDo.characterState == CHARACTER_STATE.BERSERKED)*/) {
        //    //Berserked state cannot be overriden
        //    return false;
        //}
        //if (stateComponent.currentState == null && this.marker != null && this.marker.hasFleePath) {
        //    return false; //if the character is only fleeing, but is not in combat state, do not allow overriding.
        //}
        //if (stateComponent.currentState != null) {
        //    if (stateComponent.currentState.characterState == CHARACTER_STATE.COMBAT) {
        //        //Only override flee or engage state if the job is Berserked State, Berserk overrides all
        //        if (job is CharacterStateJob) {
        //            CharacterStateJob stateJob = job as CharacterStateJob;
        //            if (stateJob.targetState == CHARACTER_STATE.BERSERKED) {
        //                return true;
        //            }
        //        }
        //        return false;
        //    } else {
        //        //If current state is not Flee or Engage, it is certainly one of the major states since the only minor states are Flee and Engage
        //        //If current state has no job, it is automatically overridable, otherwise, if the current state's job has a lower job priority (higher number) than the parameter job, it is overridable
        //        if (stateComponent.currentState.job != null && !stateComponent.currentState.job.cannotOverrideJob && job.priority < stateComponent.currentState.job.priority) {
        //            return true;
        //        } else if (stateComponent.currentState.job == null) {
        //            return true;
        //        }
        //        return false;
        //    }
        //} 
        ////else if (stateComponent.stateToDo != null) {
        ////    if (stateComponent.stateToDo.characterState == CHARACTER_STATE.COMBAT) {
        ////        //Only override flee or engage state if the job is Berserked State, Berserk overrides all
        ////        if (job is CharacterStateJob) {
        ////            CharacterStateJob stateJob = job as CharacterStateJob;
        ////            if (stateJob.targetState == CHARACTER_STATE.BERSERKED) {
        ////                return true;
        ////            }
        ////        }
        ////        return false;
        ////    } else {
        ////        //If current state is not Flee or Engage, it is certainly one of the major states since the only minor states are Flee and Engage
        ////        //If current state has no job, it is automatically overridable, otherwise, if the current state's job has a lower job priority (higher number) than the parameter job, it is overridable
        ////        if (stateComponent.stateToDo.job != null && !stateComponent.stateToDo.job.cannotOverrideJob && job.priority < stateComponent.stateToDo.job.priority) {
        ////            return true;
        ////        } else if (stateComponent.stateToDo.job == null) {
        ////            return true;
        ////        }
        ////        return false;
        ////    }
        ////}
        ////Cannot override when resting
        //if (traitContainer.GetNormalTrait<Trait>("Resting") != null) {
        //    return false;
        //}
        ////If there is no current state then check the current action
        ////Same process applies that if the current action's job has a lower job priority (higher number) than the parameter job, it is overridable
        //if(currentActionNode != null && currentActionNode.goapType == INTERACTION_TYPE.MAKE_LOVE && !currentActionNode.isDone) {
        //    //Cannot override make love
        //    return false;
        //}
        //if (currentActionNode != null && currentActionNode.parentPlan != null) {
        //    if(currentActionNode.parentPlan.job != null && !currentActionNode.parentPlan.job.cannotOverrideJob
        //               && job.priority < currentActionNode.parentPlan.job.priority) {
        //        return true;
        //    } else if (currentActionNode.parentPlan.job == null) {
        //        return true;
        //    }
        //    return false;
        //}

        ////If nothing applies, always overridable
        //return true;
    }
    /// <summary>
    /// Gets the current priority of the character's current action or state.
    /// If he/she has none, this will return a very high number.
    /// </summary>
    /// <returns></returns>
    public int GetCurrentPriorityValue() {
        if (stateComponent.currentState != null && stateComponent.currentState.job != null) {
            return stateComponent.currentState.job.priority;
        }
        //else if (stateComponent.stateToDo != null && stateComponent.stateToDo.job != null) {
        //    return stateComponent.stateToDo.job.priority;
        //}
        else {
            JobQueueItem job = currentJob;
            if (job != null) {
                return job.priority;
            } else {
                return 999999;
            }
        }

        //else if (currentActionNode != null && currentActionNode.parentPlan != null && currentActionNode.parentPlan.job != null) {
        //    return currentActionNode.parentPlan.job.priority;
        //} else {
        //    return 999999;
        //}
    }
    //public void CreateReplaceTileObjectJob(TileObject removedObj, LocationGridTile removedFrom) {
    //    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.REPLACE_TILE_OBJECT, INTERACTION_TYPE.REPLACE_TILE_OBJECT, new Dictionary<INTERACTION_TYPE, object[]>() {
    //                    { INTERACTION_TYPE.REPLACE_TILE_OBJECT, new object[]{ removedObj, removedFrom } },
    //    });
    //    job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanCharacterTakeReplaceTileObjectJob);
    //    job.SetCancelOnFail(false);
    //    job.SetCancelJobOnDropPlan(false);
    //    jobQueue.AddJobInQueue(job);
    //}
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
        //currentAlterEgo.SetFaction(faction);
        OnChangeFaction();
        // UpdateItemFactionOwner();
        if (_faction != null) {
            Messenger.Broadcast<Character>(Signals.FACTION_SET, this);
        }
    }
    public bool ChangeFactionTo(Faction newFaction) {
        if (this.faction == newFaction) {
            return false; //if the new faction is the same, ignore change
        }
        if (faction != null) {
            faction.LeaveFaction(this);
        }
        newFaction.JoinFaction(this);
        return true;
    }
    private void OnChangeFaction() {
        //check if this character has a Criminal Trait, if so, remove it
        Trait criminal = traitContainer.GetNormalTrait<Trait>("Criminal");
        if (criminal != null) {
            traitContainer.RemoveTrait(this, criminal); //TODO: RemoveTrait(criminal, false); do not trigger on remove
        }
        // if (PlayerManager.Instance.player != null && this.faction == PlayerManager.Instance.player.playerFaction) {
        //     ClearPlayerActions();
        // }
    }
    private void OnChangeFactionRelationship(Faction faction1, Faction faction2, FACTION_RELATIONSHIP_STATUS newStatus, FACTION_RELATIONSHIP_STATUS oldStatus) {
        if(faction1 == faction) {
            if(newStatus == FACTION_RELATIONSHIP_STATUS.HOSTILE) {
                //If at war with another faction, decrease hope 
                needsComponent.AdjustHope(-5f);
            }else if(oldStatus == FACTION_RELATIONSHIP_STATUS.HOSTILE && newStatus != FACTION_RELATIONSHIP_STATUS.HOSTILE) {
                //If no longer at war with another faction, increase hope
                needsComponent.AdjustHope(-5f);
            }
        }
    }
    #endregion

    #region Party
    /*
        Create a new Party with this character as the leader.
            */
    public virtual Party CreateOwnParty() {
        //if (_ownParty != null) {
        //    _ownParty.RemoveCharacter(this);
        //}
        Party newParty = new Party(this);
        SetOwnedParty(newParty);
        SetCurrentParty(newParty);
        //newParty.AddCharacter(this, true);
        //newParty.CreateCharacterObject();
        return newParty;
    }
    public virtual void SetOwnedParty(Party party) {
        ownParty = party;
    }
    public virtual void SetCurrentParty(Party party) {
        currentParty = party;
    }
    public void OnRemovedFromParty() {
        SetCurrentParty(ownParty); //set the character's party to it's own party
        //if (ownParty is CharacterParty) {
        //    if ((ownParty as CharacterParty).actionData.currentAction != null) {
        //        (ownParty as CharacterParty).actionData.currentAction.EndAction(ownParty, (ownParty as CharacterParty).actionData.currentTargetObject);
        //    }
        //}
        marker.collisionTrigger.SetCollidersState(true);
        marker.UpdateAnimation();
        //if (this.minion != null) {
        //    this.minion.SetEnabledState(true); //reenable this minion, since it could've been disabled because it was part of another party
        //}
    }
    public void OnAddedToParty() {
        if (currentParty.id != ownParty.id) {
            //currentRegion.RemoveCharacterFromLocation(this); //Why are we removing the character from location if it is added to a party
            //ownParty.specificLocation.RemoveCharacterFromLocation(this);
            //ownParty.icon.SetVisualState(false);
            marker.collisionTrigger.SetCollidersState(false);
            marker.UpdateAnimation();
        }
    }
    public bool IsInParty() {
        return currentParty.isCarryingAnyPOI;
        //if (currentParty.characters.Count > 1) {
        //    return true; //if the character is in a party that has more than 1 characters
        //}
        //return false;
    }
    public bool IsInOwnParty() {
        if (currentParty.id == ownParty.id) {
            return true;
        }
        return false;
    }
    public void CarryPOI(IPointOfInterest poi, bool changeOwnership = false) {
        if (poi.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
            ownParty.AddPOI(poi);
        } else if (poi.poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
            PickUpItem(poi as TileObject, changeOwnership);
        }
    }
    public bool IsPOICarriedOrInInventory(IPointOfInterest poi) {
        if (poi.poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
            return HasItem(poi as TileObject);
        }
        return ownParty.IsPOICarried(poi);
    }
    public void UncarryPOI(IPointOfInterest poi, bool bringBackToInventory = false, bool addToLocation = true, LocationGridTile dropLocation = null) {
        if (poi.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
            ownParty.RemovePOI(poi, addToLocation, dropLocation);
        } else if (poi.poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
            TileObject item = poi as TileObject;
            ownParty.RemovePOI(poi, false);
            if (!bringBackToInventory) {
                if (addToLocation) {
                    DropItem(item, dropLocation);
                } else {
                    UnobtainItem(item);
                }
            }
        }
    }
    public void UncarryPOI(bool bringBackToInventory = false, bool addToLocation = true, LocationGridTile dropLocation = null) {
        if(ownParty.isCarryingAnyPOI) {
            IPointOfInterest poi = ownParty.carriedPOI;
            UncarryPOI(poi, bringBackToInventory, addToLocation, dropLocation);
        }
    }
    public void ShowItemVisualCarryingPOI(TileObject item) {
        if (HasItem(item)) {
            ownParty.AddPOI(item);
        }
    }
    //public bool HasOtherCharacterInParty() {
    //    return ownParty.characters.Count > 1;
    //}
    #endregion

    #region Location
    public void SetCurrentStructureLocation(LocationStructure newStructure, bool broadcast = true) {
        if (newStructure == this.currentStructure) {
            return; //ignore change;
        }
        LocationStructure previousStructure = this.currentStructure;
        _currentStructure = newStructure;
        //if (marker != null && currentStructure != null) {
        //    marker.RevalidatePOIsInVisionRange(); //when the character changes structures, revalidate pois in range
        //}
        var summary = newStructure != null ? $"{GameManager.Instance.TodayLogString()}Arrived at <color=\"green\">{newStructure}</color>" 
            : $"{GameManager.Instance.TodayLogString()}Left <color=\"red\">{previousStructure}</color>";
        locationHistory.Add(summary);
        if (locationHistory.Count > 80) {
            locationHistory.RemoveAt(0);
        }

        if (broadcast) {
            if (newStructure != null) {
                Messenger.Broadcast(Signals.CHARACTER_ARRIVED_AT_STRUCTURE, this, newStructure);
            }
            if (previousStructure != null) {
                Messenger.Broadcast(Signals.CHARACTER_LEFT_STRUCTURE, this, previousStructure);
            }
        }
    }
    /// <summary>
    /// Move this character to another structure in the same settlement.
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
                    marker.GoToPOI(targetPOI, arrivalAction);
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
        //NOTE: Tile location is being computed every time.
        //this.tile = tile;
        //string summary = string.Empty;
        //if (tile == null) {
        //    summary = GameManager.Instance.TodayLogString() + "Set tile location to null";
        //} else {
        //    summary = GameManager.Instance.TodayLogString() + "Set tile location to " + tile.localPlace.ToString();
        //}
        //locationHistory.Add(summary);
        //if (locationHistory.Count > 80) {
        //    locationHistory.RemoveAt(0);
        //}
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
            for (int i = 0; i < gridTileLocation.parentMap.allEdgeTiles.Count; i++) {
                LocationGridTile currTile = gridTileLocation.parentMap.allEdgeTiles[i];
                float dist = Vector2.Distance(currTile.localLocation, currentGridTile.localLocation);
                if (nearestDist == -999f || dist < nearestDist) {
                    if (currTile.structure != null) {
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
            //CheckApprehendRelatedJobsOnLeaveLocation();
            //CancelOrUnassignRemoveTraitRelatedJobs();
            Messenger.Broadcast(Signals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI, this as IPointOfInterest, "");
            CancelAllJobsExceptForCurrent(false);
            //marker.ClearTerrifyingObjects();
            ExecuteLeaveAreaActions();
            needsComponent.OnCharacterLeftLocation(currentRegion);
        } else {
            //if (marker.terrifyingObjects.Count > 0) {
            //    marker.RemoveTerrifyingObject(party.owner);
            //    if (party.isCarryingAnyPOI) {
            //        marker.RemoveTerrifyingObject(party.carriedPOI);
            //    }
            //}
        }
    }
    private void OnArrivedAtArea(Party party) {
        if (currentParty == party) {
            //if (isAtHomeArea) {
            //    if (HasTraitOf(TRAIT_TYPE.CRIMINAL)) {
            //        CreateApprehendJob();
            //    }
            //    //for (int i = 0; i < traits.Count; i++) {
            //    //    if (traits[i].name == "Cursed" || traits[i].name == "Sick"
            //    //        || traits[i].name == "Injured" || traits[i].name == "Unconscious") {
            //    //        CreateRemoveTraitJob(traits[i].name);
            //    //    }
            //    //}
            //}
            needsComponent.OnCharacterArrivedAtLocation(currentRegion);
        } else {
            //AddAwareness(party.owner);
            //if (party.isCarryingAnyPOI) {
            //    AddAwareness(party.carriedPOI);
            //}
            //for (int i = 0; i < party.characters.Count; i++) {
            //    Character character = party.characters[i];
            //    AddAwareness(character); //become re aware of character
            //}
        }
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
    public void SetRegionLocation(Region region) {
        _currentRegion = region;
    }
    public bool IsInHomeSettlement() {
        if (isAtHomeRegion) {
            Settlement currentSettlement = this.currentSettlement;
            Settlement home = homeSettlement;
            if(home != null) {
                return currentSettlement == home;
            }
        }
        return false;
    } 
    public void SetIsSettlementRuler(bool state) {
        if(isSettlementRuler != state) {
            isSettlementRuler = state;
            if (isSettlementRuler) {
                AssignBuildStructureComponent();
                behaviourComponent.AddBehaviourComponent(typeof(SettlementRulerBehaviour));
            } else {
                UnassignBuildStructureComponent();
                behaviourComponent.RemoveBehaviourComponent(typeof(SettlementRulerBehaviour));
            }
        }
    }
    #endregion

    #region Utilities
    private bool ChangeRace(RACE race) {
        if (_raceSetting != null) {
            if (_raceSetting.race == race) {
                return false; //current race is already the new race, no change
            }
            for (int i = 0; i < _raceSetting.traitNames.Length; i++) {
                traitContainer.RemoveTrait(this, _raceSetting.traitNames[i]); //Remove traits from race
            }
        }
        RaceSetting rs = RaceManager.Instance.racesDictionary[race.ToString()];
        _raceSetting = rs.CreateNewCopy();
        OnUpdateRace();
        Messenger.Broadcast(Signals.CHARACTER_CHANGED_RACE, this);
        return true;
    }
    protected void OnUpdateRace() {
        for (int i = 0; i < _raceSetting.traitNames.Length; i++) {
            traitContainer.AddTrait(this, _raceSetting.traitNames[i]);
        }
        //Update Portrait to use new race
        visuals.UpdateAllVisuals(this);
        //update goap interactions that should no longer be valid
        if (race == RACE.SKELETON) {
            advertisedActions.Remove(INTERACTION_TYPE.DRINK_BLOOD);
            advertisedActions.Remove(INTERACTION_TYPE.SHARE_INFORMATION);
        }
    }
    private void ChangeClass(string className) {
        //string previousClassName = _characterClass.className;
        AssignClass(className);
        //_characterClass = charClass.CreateNewCopy();

    }
    public void SetName(string newName) {
        _name = newName;
        string[] split = _name.Split(' '); 
        _firstName = split[0];
        if (split.Length > 1) {
            _surName = split[1];    
        }
        RandomNameGenerator.RemoveNameAsAvailable(this.gender, this.race, newName);
    }
    public void CenterOnCharacter() {
        if (marker != null) {
            if (currentParty.icon.isTravellingOutside) {
                if (InnerMapManager.Instance.isAnInnerMapShowing) {
                    InnerMapManager.Instance.HideAreaMap();
                }
                //CameraMove.Instance.CenterCameraOn(currentParty.icon.travelLine.iconImg.gameObject);
                CameraMove.Instance.CenterCameraOn(currentParty.icon.targetLocation.coreTile.gameObject);
            } else if (currentParty.icon.isTravelling) {
                if (marker.gameObject.activeInHierarchy) {
                    bool instantCenter = !InnerMapManager.Instance.IsShowingInnerMap(currentRegion);
                    if (currentRegion != null && instantCenter) {
                        InnerMapManager.Instance.ShowInnerMap(currentRegion, false);
                    }
                    InnerMapCameraMove.Instance.CenterCameraOn(marker.gameObject, instantCenter);
                }
            } else if (currentRegion != null) {
                bool instantCenter = !InnerMapManager.Instance.IsShowingInnerMap(currentRegion);
                if (instantCenter) {
                    InnerMapManager.Instance.ShowInnerMap(currentRegion, false);
                }
                InnerMapCameraMove.Instance.CenterCameraOn(marker.gameObject, instantCenter);

            } else {
                if (InnerMapManager.Instance.isAnInnerMapShowing) {
                    InnerMapManager.Instance.HideAreaMap();
                }
                CameraMove.Instance.CenterCameraOn(currentRegion.coreTile.gameObject);
            }
        } else {
            if (InnerMapManager.Instance.isAnInnerMapShowing) {
                InnerMapManager.Instance.HideAreaMap();
            }
            CameraMove.Instance.CenterCameraOn(currentRegion.coreTile.gameObject);
        }
    }
    private void OnOtherCharacterDied(Character characterThatDied) {
        if (characterThatDied.id != this.id) {
            string opinionLabel = relationshipContainer.GetOpinionLabel(characterThatDied);
            if (opinionLabel == OpinionComponent.Friend) {
                needsComponent.AdjustHope(-5f);
            } else if (opinionLabel == OpinionComponent.Close_Friend) {
                needsComponent.AdjustHope(-10f);
                if (!traitContainer.HasTrait("Psychopath")) {
                    traitContainer.AddTrait(this, "Griefstricken");
                }
            }

            if (characterThatDied.currentRegion == homeRegion) {
                //if a hostile character has been killed within the character's home settlement, Hope increases by XX amount.
                if (IsHostileWith(characterThatDied)) {
                    needsComponent.AdjustHope(5f);
                }
            }
            //RemoveRelationship(characterThatDied); //do not remove relationships when dying
            marker.OnOtherCharacterDied(characterThatDied);
        }
    }
    private void OnSeizePOI(IPointOfInterest poi) {
        if(poi is Character) {
            OnSeizeCharacter(poi as Character);
        } else if (poi is TileObject) {
            OnSeizeTileObject(poi as TileObject);
        }
    }
    private void OnSeizeCharacter(Character character) {
        if (character.id != this.id) {
            //RemoveRelationship(characterThatDied); //do not remove relationships when dying
            marker.OnSeizeOtherCharacter(character);
        }
    }
    private void OnSeizeTileObject(TileObject tileObject) {
        if(currentActionNode != null && currentActionNode.poiTarget == tileObject) {
            StopCurrentActionNode();
        }
        Character[] currentUsers = tileObject.users;
        if (currentUsers != null && currentUsers.Length > 0) {
            for (int i = 0; i < currentUsers.Length; i++) {
                currentUsers[i].StopCurrentActionNode();
                tileObject.RemoveUser(currentUsers[i]);
                //i--;
            }
        }
    }
    public void AdjustDoNotRecoverHP(int amount) {
        doNotRecoverHP += amount;
        doNotRecoverHP = Math.Max(doNotRecoverHP, 0);
    }
    public override string ToString() {
        return name;
    }
    private LocationGridTile GetLocationGridTileByXY(int x, int y, bool throwOnException = true) {
        return currentRegion.innerMap.map[x, y];
    }
    public void UpdateCanCombatState() {
        bool combatState = traitContainer.HasTrait("Combatant") && !traitContainer.HasTrait("Injured");
        if (canCombat != combatState) {
            canCombat = combatState;
            //if (canCombat && marker != null) {
            //    marker.ClearTerrifyingObjects();
            //}
        }
    }
    private bool CanCharacterReact(IPointOfInterest targetPOI = null) {
        if (!canWitness || !canPerform) {
            return false; //this character cannot witness
        }
        if (interruptComponent.isInterrupted) {
            //Cannot react if interrupted
            return false;
        }
        if (this is Summon || minion != null) {
            //Cannot react if summon or minion
            return false;
        }
        if (defaultCharacterTrait.hasSurvivedApprehension && !isAtHomeRegion) {
            return false; //Must not react because he will only have one thing to do and that is to return home
        }
        if (stateComponent.currentState != null && !stateComponent.currentState.isDone) {
            if (stateComponent.currentState.characterState == CHARACTER_STATE.COMBAT) {
                //Character must not react if he/she is in flee or engage state
                return false;
            }
            if (stateComponent.currentState.characterState == CHARACTER_STATE.DOUSE_FIRE) {
                //Character must not react if he/she is in douse fire state
                return false;
            }
        }
        //if (traitContainer.GetNormalTrait<Trait>("Berserked") != null) { //|| (stateComponent.stateToDo != null && stateComponent.stateToDo.characterState == CHARACTER_STATE.BERSERKED && !stateComponent.stateToDo.isDone)
        //    //Character must not react if he/she is in berserked state
        //    //Returns true so that it will create an impression that the character actually created a job even if he/she didn't, so that the character will not chat, etc.
        //    return false;
        //}
        if (traitContainer.HasTraitOf(TRAIT_TYPE.DISABLER, TRAIT_EFFECT.NEGATIVE)) {
            return false;
        }
        if (targetPOI != null && targetPOI is Character) {
            Character target = targetPOI as Character;
            if (target.faction != null && target.faction.IsHostileWith(faction)) {
                //Cannot react if target charcter is from a hostile faction
                //Only combat those characters that's why they cannot react to their traits, actions, etc.
                return false;
            }
        }
        return true;
    }
    public bool IsAble() {
        return currentHP > 0 && !isDead && !traitContainer.HasTraitOf(TRAIT_TYPE.DISABLER, TRAIT_EFFECT.NEGATIVE) && !isDead && characterClass.className != "Zombie";
    }
    public void SetIsFollowingPlayerInstruction(bool state) {
        isFollowingPlayerInstruction = state;
    }
    /// <summary>
    /// Can this character be instructed by the player?
    /// </summary>
    /// <returns>True or false.</returns>
    public virtual bool CanBeInstructedByPlayer() {
        if (!faction.isPlayerFaction) {
            return false;
        }
        if (isDead) {
            return false;
        }
        if (stateComponent.currentState is CombatState && !(stateComponent.currentState as CombatState).isAttacking) {
            return false; //character is fleeing
        }
        if (traitContainer.HasTraitOf(TRAIT_TYPE.DISABLER, TRAIT_EFFECT.NEGATIVE)) {
            return false;
        }
        return true;
    }
    public void SetTileObjectLocation(TileObject tileObject) {
        tileObjectLocation = tileObject;
    }
    public void AdjustIsStoppedByOtherCharacter(int amount) {
        isStoppedByOtherCharacter += amount;
        isStoppedByOtherCharacter = Mathf.Max(0, isStoppedByOtherCharacter);
        if (marker != null) {
            marker.UpdateAnimation();
        }
    }
    public virtual bool IsValidCombatTarget() {
        return traitContainer.HasTraitOf(TRAIT_TYPE.DISABLER, TRAIT_EFFECT.NEGATIVE) == false;
    }
    public void ExecutePendingActionsAfterMultithread() {
        for (int i = 0; i < pendingActionsAfterMultiThread.Count; i++) {
            pendingActionsAfterMultiThread[i].Invoke();
        }
        pendingActionsAfterMultiThread.Clear();
    }
    public void SetHaUnresolvedCrime(bool state) {
        hasUnresolvedCrime = state;
    }
    public void SetIsInLimbo(bool state) {
        isInLimbo = state;
    }
    public void SetIsLimboCharacter(bool state) {
        isLimboCharacter = state;
    }
    public void SetHasSeenFire(bool state) {
        hasSeenFire = state;
    }
    /// <summary>
    /// Is this character and NPC?
    /// (Characters that can normally be manipulated by the player)
    /// </summary>
    /// <returns></returns>
    public bool IsNPC() {
        return (this is Summon) == false && minion == null;
    }
    #endregion    

    #region History/Logs
    //Add log to this character and show notif of that log only if this character is clicked or tracked, otherwise, add log only
    public void RegisterLog(string fileName, string key, object target = null, string targetName = "", ActualGoapNode node = null, bool onlyClickedCharacter = true) {
        if (!GameManager.Instance.gameHasStarted) {
            return;
        }
        if (key == "remove_trait" && isDead) {
            return;
        }
        Log addLog = new Log(GameManager.Instance.Today(), "Character", fileName, key, node);
        addLog.AddToFillers(this, this.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        if (targetName != "") {
            addLog.AddToFillers(target, targetName, LOG_IDENTIFIER.TARGET_CHARACTER);
        }
        addLog.AddLogToInvolvedObjects();
        // PlayerManager.Instance.player.ShowNotificationFrom(addLog, this, onlyClickedCharacter);
    }

    public virtual void OnActionPerformed(ActualGoapNode node) {
        ///Moved all needed checking <see cref="CharacterManager.OnActionStateSet(GoapAction, GoapActionState)"/>
        if (isDead || !canWitness) {
            return;
        }
        //if (node.action.goapType == INTERACTION_TYPE.WATCH) {
        //    //Cannot witness/watch a watch action
        //    return;
        //}
        if (node.actor == this) {
            //Cannot witness if character is part of the action
            return;
        }
        //if (!node.action.shouldAddLogs) {
        //    return;
        //}

        //Instead of witnessing the action immediately, it needs to be pooled to avoid duplicates, so add the supposed to be witnessed action to the list and let ProcessAllUnprocessedVisionPOIs in CharacterMarker do its thing
        if (marker != null) { //&& !marker.actionsToWitness.Contains(node)
            if (marker.inVisionCharacters.Contains(node.actor)) {
                //marker.actionsToWitness.Add(node);
                //This is done so that the character will react again
                marker.AddUnprocessedPOI(node.actor, true);
            } else if (marker.inVisionPOIs.Contains(node.poiTarget)) {
                //marker.actionsToWitness.Add(node);
                //This is done so that the character will react again
                marker.AddUnprocessedPOI(node.poiTarget, true);
            }
        }

        //ThisCharacterWitnessedEvent(action);
        //ThisCharacterWatchEvent(null, action, state);
    }
    public virtual void OnInterruptStarted(Character actor, IPointOfInterest target, Interrupt interrupt) {
        if (isDead || !canWitness) {
            return;
        }
        if (actor == this) {
            return;
        }
        if (marker != null) {
            if (marker.inVisionCharacters.Contains(actor)) {
                //This is done so that the character will react again
                marker.AddUnprocessedPOI(actor, true);
            } 
            //else if (marker.inVisionPOIs.Contains(target)) {
            //    //This is done so that the character will react again
            //    marker.unprocessedVisionPOIs.Add(target);
            //}
        }
    }
    public void AddOverrideThought(string log) {
        _overrideThoughts.Add(log);
    }
    public void RemoveOverrideThought(string log) {
        _overrideThoughts.Remove(log);
    }
    public string GetThoughtBubble(out Log log) {
        log = null;
        if (minion != null) {
            return string.Empty;
        }
        if (_overrideThoughts.Count > 0) {
            return _overrideThoughts[0];
        }
        if (isDead) {
            return $"{name} has died.";
        }
        if (minion != null) {
            if (minion.busyReasonLog != null) {
                log = minion.busyReasonLog;
                return UtilityScripts.Utilities.LogReplacer(minion.busyReasonLog);
            } else {
                return $"{name} is ready to do your bidding.";
            }
            //return string.Empty; 
        }
        //Interrupt
        if (interruptComponent.isInterrupted && interruptComponent.thoughtBubbleLog != null) {
            log = interruptComponent.thoughtBubbleLog;
            return UtilityScripts.Utilities.LogReplacer(interruptComponent.thoughtBubbleLog);
        }

        //Action
        if (currentActionNode != null) {
            Log currentLog = currentActionNode.GetCurrentLog();
            log = currentLog;
            return UtilityScripts.Utilities.LogReplacer(currentLog);
        }

        //Character State
        if (stateComponent.currentState != null) {
            log = stateComponent.currentState.thoughtBubbleLog;
            return UtilityScripts.Utilities.LogReplacer(stateComponent.currentState.thoughtBubbleLog);
        }
        //fleeing
        if (marker != null && marker.hasFleePath) {
            return $"{name} is fleeing.";
        }

        //Travelling
        if (currentParty.icon.isTravelling) {
            if (currentParty.owner.marker.destinationTile != null) {
                return $"{name} is going to {currentParty.owner.marker.destinationTile.structure.GetNameRelativeTo(this)}";
            }
        }

        //Default - Do nothing/Idle
        if (currentStructure != null) {
            return $"{name} is in {currentStructure.GetNameRelativeTo(this)}";
        }

        return $"{name} is in {currentRegion.name}";
    }
    //Returns the list of goap actions to be witnessed by this character
    public void ThisCharacterSaw(IPointOfInterest target, bool reactToActionOnly = false) {
        //if (isDead) {
        //    return;
        //}

        for (int i = 0; i < traitContainer.allTraits.Count; i++) {
            traitContainer.allTraits[i].OnSeePOIEvenCannotWitness(target, this);
        }
        // for (int i = 0; i < target.traitContainer.onOthersSeeEvenCannotWitnessTraits.Count; i++) {
        //     target.traitContainer.onOthersSeeEvenCannotWitnessTraits[i].OnOthersSeeThisEvenCannotWitness(this, target);
        // }

        if (!canWitness) {
            return;
        }

        if (currentActionNode != null && currentActionNode.actionStatus == ACTION_STATUS.STARTED && currentActionNode.isStealth) {
            if (currentActionNode.poiTarget == target) {
                //Upon seeing the target while performing a stealth job action, check if it can do the action
                if (!marker.CanDoStealthActionToTarget(target)) {
                    currentJob.CancelJob(reason: "There is a witness around");
                }
            } else {
                //Upon seeing other characters while target of stealth action is already in vision, automatically cancel job
                if (target is Character) {
                    if (marker.inVisionCharacters.Contains(currentActionNode.poiTarget)) {
                        currentJob.CancelJob(reason: "There is a witness around");
                    }
                }
            }
        }

        //React To Actions
        ActualGoapNode targetCharacterCurrentActionNode = null;
        Character targetCharacter = null;
        if (target is Character) {
            targetCharacter = target as Character;
            //React To Interrupt
            if (targetCharacter.interruptComponent.isInterrupted) {
                reactionComponent.ReactTo(targetCharacter.interruptComponent.currentInterrupt, targetCharacter, targetCharacter.interruptComponent.currentTargetPOI);
            } else {
                //targetCharacter.OnSeenBy(this); //trigger that the target character was seen by this character.
                targetCharacterCurrentActionNode = targetCharacter.currentActionNode;
                if (targetCharacterCurrentActionNode != null /*&& node.action.shouldAddLogs*/ && targetCharacterCurrentActionNode.actionStatus == ACTION_STATUS.PERFORMING && targetCharacterCurrentActionNode.actor != this) {
                    if (!targetCharacterCurrentActionNode.awareCharacters.Contains(this)) {
                        reactionComponent.ReactTo(targetCharacterCurrentActionNode, SHARE_INTEL_STATUS.WITNESSED);
                        targetCharacterCurrentActionNode.AddAwareCharacter(this);
                    }
                } else if (targetCharacter.isInCombat && targetCharacter.jobQueue.jobsInQueue.Count > 1) {
                    if (targetCharacter.jobQueue.jobsInQueue[1] is GoapPlanJob) {
                        GoapPlanJob planJob = targetCharacter.jobQueue.jobsInQueue[1] as GoapPlanJob;
                        if (planJob.assignedPlan != null && planJob.assignedPlan.currentActualNode != null) {
                            targetCharacterCurrentActionNode = planJob.assignedPlan.currentActualNode;
                            if (!targetCharacterCurrentActionNode.awareCharacters.Contains(this)) {
                                reactionComponent.ReactTo(targetCharacterCurrentActionNode, SHARE_INTEL_STATUS.WITNESSED);
                                targetCharacterCurrentActionNode.AddAwareCharacter(this);
                            }
                        }
                    }
                }
            }
        }
        if (target.allJobsTargetingThis.Count > 0) {
            //We get the actions targeting the target character because a character must also witness an action even if he only sees the target and not the actor
            for (int i = 0; i < target.allJobsTargetingThis.Count; i++) {
                if (target.allJobsTargetingThis[i] is GoapPlanJob) {
                    GoapPlanJob job = target.allJobsTargetingThis[i] as GoapPlanJob;
                    GoapPlan plan = job.assignedPlan;
                    if (plan != null /*&& plan.currentActualNode.action.shouldAddLogs*/ 
                        && plan.currentActualNode.actionStatus == ACTION_STATUS.PERFORMING
                        && plan.currentActualNode != targetCharacterCurrentActionNode && plan.currentActualNode.actor != this) {
                        if (!plan.currentActualNode.awareCharacters.Contains(this)) {
                            reactionComponent.ReactTo(plan.currentActualNode, SHARE_INTEL_STATUS.WITNESSED);
                            plan.currentActualNode.AddAwareCharacter(this);
                        }
                    }
                }
            }
        }
        if (!reactToActionOnly) {
            //React To Character, Object, and Item
            string debugLog = string.Empty;
            reactionComponent.ReactTo(target, ref debugLog);
            logComponent.PrintLogIfActive(debugLog);
 
            if(targetCharacter != null) {
                ThisCharacterWatchEvent(targetCharacter, null, null);
            }
        }
    }
    //public List<Log> GetWitnessOrInformedMemories(int dayFrom, int dayTo, Character involvedCharacter = null) {
    //    List<Log> memories = new List<Log>();
    //    for (int i = 0; i < _history.Count; i++) {
    //        Log historyLog = _history[i];
    //        if (historyLog.goapAction != null && (historyLog.key == "witness_event" || historyLog.key == "informed_event")) {
    //            if (historyLog.day >= dayFrom && historyLog.day <= dayTo) {
    //                if(involvedCharacter != null) {
    //                    if (historyLog.goapAction.actor == involvedCharacter || historyLog.goapAction.IsTarget(involvedCharacter)) {
    //                        memories.Add(historyLog);
    //                    }
    //                } else {
    //                    memories.Add(historyLog);
    //                }
    //            }
    //        }
    //    }
    //    return memories;
    //}
    //public List<Log> GetCrimeMemories(int dayFrom, int dayTo, Character involvedCharacter = null) {
    //    List<Log> memories = new List<Log>();
    //    for (int i = 0; i < _history.Count; i++) {
    //        Log historyLog = _history[i];
    //        if (historyLog.goapAction != null && historyLog.goapAction.awareCharactersOfThisAction.Contains(this) && historyLog.goapAction.committedCrime != CRIME.NONE) {
    //            if (historyLog.day >= dayFrom && historyLog.day <= dayTo) {
    //                if(involvedCharacter != null) {
    //                    for (int j = 0; j < historyLog.goapAction.crimeCommitters.Length; j++) {
    //                        Character criminal = historyLog.goapAction.crimeCommitters[j];
    //                        if (criminal == involvedCharacter) {
    //                            memories.Add(historyLog);
    //                            break;
    //                        }
    //                    }
    //                } else {
    //                    memories.Add(historyLog);
    //                }
    //            }
    //        }
    //    }
    //    return memories;
    //}
    //public void ThisCharacterWitnessedEvent(ActualGoapNode witnessedEvent) {
    //    //if (isDead || !canWitness) {
    //    //    return;
    //    //}
    //    if (faction != witnessedEvent.actor.faction && //only check faction relationship if involved characters are of different factions
    //        faction.IsHostileWith(witnessedEvent.actor.faction)) {
    //        //Must not react if the faction of the actor of witnessed action is hostile with the faction of the witness
    //        return;
    //    }


    //    if (witnessedEvent.currentStateName == null) {
    //        throw new System.Exception(GameManager.Instance.TodayLogString() + this.name + " witnessed event " + witnessedEvent.action.goapName + " by " + witnessedEvent.actor.name + " but it does not have a current state!");
    //    }
    //    if (witnessedEvent.descriptionLog == null) {
    //        throw new Exception(GameManager.Instance.TodayLogString() + this.name + " witnessed event " + witnessedEvent.action.goapName + " by " + witnessedEvent.actor.name + " with state " + witnessedEvent.currentStateName + " but it does not have a description log!");
    //    }
    //    Log witnessLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "witness_event", witnessedEvent);
    //    witnessLog.AddToFillers(this, name, LOG_IDENTIFIER.OTHER);
    //    witnessLog.AddToFillers(null, Utilities.LogDontReplace(witnessedEvent.descriptionLog), LOG_IDENTIFIER.APPEND);
    //    witnessLog.AddToFillers(witnessedEvent.descriptionLog.fillers);
    //    AddHistory(witnessLog);

    //    CRIME_TYPE crimeType = CrimeManager.Instance.GetCrimeTypeConsideringAction(witnessedEvent);
    //    if (crimeType != CRIME_TYPE.NONE) {
    //        CrimeManager.Instance.ReactToCrime(this, witnessedEvent, witnessedEvent.associatedJobType, crimeType);
    //    }

    //    //if (faction.isPlayerFaction) {
    //    //    //Player characters cannot react to witnessed events
    //    //    return;
    //    //}
    //    //if (witnessedEvent.currentState.shareIntelReaction != null && !isFactionless) { //Characters with no faction cannot witness react
    //    //    List<string> reactions = witnessedEvent.currentState.shareIntelReaction.Invoke(this, null, SHARE_INTEL_STATUS.WITNESSED);
    //    //    if(reactions != null) {
    //    //        string reactionLog = name + " witnessed event: " + witnessedEvent.goapName;
    //    //        reactionLog += "\nREACTION:";
    //    //        for (int i = 0; i < reactions.Count; i++) {
    //    //            reactionLog += "\n" + reactions[i];
    //    //        }
    //    //        PrintLogIfActive(reactionLog);
    //    //    }
    //    //}
    //    //witnessedEvent.AddAwareCharacter(this);

    //    //If a character sees or informed about a lover performing Making Love or Ask to Make Love, they will feel Betrayed
    //    //if (witnessedEvent.actor != this && !witnessedEvent.IsTarget(this)) {
    //    //    Character target = witnessedEvent.poiTarget as Character;
    //    //    if (witnessedEvent.goapType == INTERACTION_TYPE.MAKE_LOVE) {
    //    //        target = (witnessedEvent as MakeLove).targetCharacter;
    //    //        if (relationshipContainer.HasRelationshipWith(witnessedEvent.actor.currentAlterEgo, RELATIONSHIP_TRAIT.LOVER) || relationshipContainer.HasRelationshipWith(target.currentAlterEgo, RELATIONSHIP_TRAIT.LOVER)) {
    //    //            Betrayed betrayed = new Betrayed();
    //    //            traitContainer.AddTrait(this, betrayed);
    //    //            //RelationshipManager.Instance.RelationshipDegradation(witnessedEvent.actor, this, witnessedEvent);
    //    //            //RelationshipManager.Instance.RelationshipDegradation(target, this, witnessedEvent);
    //    //        } 
    //    //    } else if (witnessedEvent.goapType == INTERACTION_TYPE.INVITE) {
    //    //        if (relationshipContainer.HasRelationshipWith(witnessedEvent.actor.currentAlterEgo, RELATIONSHIP_TRAIT.LOVER)) {
    //    //            Betrayed betrayed = new Betrayed();
    //    //            traitContainer.AddTrait(this, betrayed);
    //    //            //RelationshipManager.Instance.RelationshipDegradation(witnessedEvent.actor, this, witnessedEvent);
    //    //            //RelationshipManager.Instance.RelationshipDegradation(target, this, witnessedEvent);
    //    //        } else if (relationshipContainer.HasRelationshipWith(target.currentAlterEgo, RELATIONSHIP_TRAIT.LOVER)) {
    //    //            if (witnessedEvent.currentState.name == "Invite Success") {
    //    //                Betrayed betrayed = new Betrayed();
    //    //                traitContainer.AddTrait(this, betrayed);
    //    //                //RelationshipManager.Instance.RelationshipDegradation(witnessedEvent.actor, this, witnessedEvent);
    //    //                //RelationshipManager.Instance.RelationshipDegradation(target, this, witnessedEvent);
    //    //            }
    //    //        }
    //    //    }
    //    //}
    //}
    /// <summary>
    /// This character watched an action happen.
    /// </summary>
    /// <param name="targetCharacter">The character that was performing the action.</param>
    /// <param name="action">The action that was performed.</param>
    /// <param name="state">The state the action was in when this character watched it.</param>
    public void ThisCharacterWatchEvent(Character targetCharacter, GoapAction action, GoapActionState state) {
        if (faction.isPlayerFaction) {
            //Player characters cannot watch events
            return;
        }
        if (action == null) {
            if (targetCharacter != null && targetCharacter.isInCombat) {
                CombatState targetCombatState = targetCharacter.stateComponent.currentState as CombatState;
                if (targetCombatState.currentClosestHostile != null && targetCombatState.currentClosestHostile != this) {
                    if (targetCombatState.currentClosestHostile is Character) {
                        Character currentHostileOfTargetCharacter = targetCombatState.currentClosestHostile as Character;
                        RELATIONSHIP_EFFECT relEffectTowardsTargetOfCombat = relationshipContainer.GetRelationshipEffectWith(currentHostileOfTargetCharacter);
                        if (relEffectTowardsTargetOfCombat == RELATIONSHIP_EFFECT.POSITIVE) {
                            if (!targetCombatState.allCharactersThatDegradedRel.Contains(this)) {
                                relationshipContainer.AdjustOpinion(this, targetCharacter, "Base", -10);
                                targetCombatState.AddCharacterThatDegradedRel(this);
                            }
                        }
                        if (targetCharacter.faction == faction) {
                            if (currentHostileOfTargetCharacter.stateComponent.currentState != null
                                && !currentHostileOfTargetCharacter.stateComponent.currentState.isDone
                                && currentHostileOfTargetCharacter.stateComponent.currentState.characterState == CHARACTER_STATE.COMBAT) {
                                CombatState combatStateOfCurrentHostileOfTargetCharacter = currentHostileOfTargetCharacter.stateComponent.currentState as CombatState;
                                if (combatStateOfCurrentHostileOfTargetCharacter.currentClosestHostile != null
                                    && combatStateOfCurrentHostileOfTargetCharacter.currentClosestHostile == this) {
                                    //If character 1 is supposed to watch/join the combat of character 2 against character 3
                                    //but character 3 is already in combat and his current target is already character 1
                                    //then character 1 should not react
                                    return;
                                }
                            }
                            if (currentHostileOfTargetCharacter.faction == faction) {
                                RELATIONSHIP_EFFECT relEffectTowardsTarget = relationshipContainer.GetRelationshipEffectWith(targetCharacter);

                                if (relEffectTowardsTarget == RELATIONSHIP_EFFECT.POSITIVE) {
                                    if (relEffectTowardsTargetOfCombat == RELATIONSHIP_EFFECT.POSITIVE) {
                                        CreateWatchEvent(null, targetCombatState, targetCharacter);
                                    } else {
                                        if (combatComponent.Fight(targetCombatState.currentClosestHostile, targetCharacter.combatComponent.IsLethalCombatForTarget(currentHostileOfTargetCharacter))) {
                                            //if (!combatComponent.avoidInRange.Contains(targetCharacter)) {
                                                //Do process combat behavior first for this character, if the current closest hostile
                                                //of the combat state of this character is also the targetCombatState.currentClosestHostile
                                                //Then that's only when we apply the join combat log and notif
                                                //Because if not, it means that this character is already in combat with someone else, and thus
                                                //should not product join combat log anymore
                                                Log joinLog = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "join_combat");
                                                joinLog.AddToFillers(this, this.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                                                joinLog.AddToFillers(targetCombatState.currentClosestHostile, targetCombatState.currentClosestHostile.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                                                joinLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.CHARACTER_3);
                                                joinLog.AddToFillers(null, this.relationshipContainer.GetRelationshipNameWith(targetCharacter), LOG_IDENTIFIER.STRING_1);
                                                joinLog.AddLogToSpecificObjects(LOG_IDENTIFIER.ACTIVE_CHARACTER, LOG_IDENTIFIER.TARGET_CHARACTER);
                                                // PlayerManager.Instance.player.ShowNotificationFrom(this, joinLog);
                                            //}
                                            //combatComponent.ProcessCombatBehavior();
                                        }
                                    }
                                } else {
                                    if (relEffectTowardsTargetOfCombat == RELATIONSHIP_EFFECT.POSITIVE) {
                                        //if (combatComponent.AddHostileInRange(targetCharacter, false, targetCharacter.combatComponent.IsLethalCombatForTarget(currentHostileOfTargetCharacter))) {
                                        //    if (!combatComponent.avoidInRange.Contains(targetCharacter)) {
                                        //        //Do process combat behavior first for this character, if the current closest hostile
                                        //        //of the combat state of this character is also the targetCombatState.currentClosestHostile
                                        //        //Then that's only when we apply the join combat log and notif
                                        //        //Because if not, it means that this character is already in combat with someone else, and thus
                                        //        //should not product join combat log anymore
                                        //        Log joinLog = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "join_combat");
                                        //        joinLog.AddToFillers(this, this.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                                        //        joinLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                                        //        joinLog.AddToFillers(targetCombatState.currentClosestHostile, targetCombatState.currentClosestHostile.name, LOG_IDENTIFIER.CHARACTER_3);
                                        //        joinLog.AddToFillers(null, this.relationshipContainer.GetRelationshipName(currentHostileOfTargetCharacter), LOG_IDENTIFIER.STRING_1);
                                        //        joinLog.AddLogToSpecificObjects(LOG_IDENTIFIER.ACTIVE_CHARACTER, LOG_IDENTIFIER.TARGET_CHARACTER);
                                        //        PlayerManager.Instance.player.ShowNotificationFrom(this, joinLog);
                                        //    }
                                        //    //combatComponent.ProcessCombatBehavior();
                                        //}
                                    } else {
                                        CreateWatchEvent(null, targetCombatState, targetCharacter);
                                    }
                                }
                            } else {
                                //the target of the combat state is not part of this character's faction
                                if (combatComponent.Fight(targetCombatState.currentClosestHostile, targetCharacter.combatComponent.IsLethalCombatForTarget(currentHostileOfTargetCharacter))) {
                                    //if (!combatComponent.avoidInRange.Contains(targetCharacter)) {
                                        //TODO: Do process combat behavior first for this character, if the current closest hostile
                                        //of the combat state of this character is also the targetCombatState.currentClosestHostile
                                        //Then that's only when we apply the join combat log and notif
                                        //Because if not, it means that this character is already in combat with someone else, and thus
                                        //should not product join combat log anymore
                                        Log joinLog = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "join_combat_faction");
                                        joinLog.AddToFillers(this, this.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                                        joinLog.AddToFillers(targetCombatState.currentClosestHostile, targetCombatState.currentClosestHostile.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                                        joinLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.CHARACTER_3);
                                        joinLog.AddLogToSpecificObjects(LOG_IDENTIFIER.ACTIVE_CHARACTER, LOG_IDENTIFIER.TARGET_CHARACTER);
                                        // PlayerManager.Instance.player.ShowNotificationFrom(this, joinLog);
                                    //}
                                    //combatComponent.ProcessCombatBehavior();
                                }
                            }
                        }
                    }
                }
            }
        }
        //else if (!action.isDone) {
        //    if (action.goapType == INTERACTION_TYPE.MAKE_LOVE && state.name == "Make Love Success") {
        //        MakeLove makeLove = action as MakeLove;
        //        Character target = makeLove.targetCharacter;
        //        if (HasRelationshipOfTypeWith(action.actor, false, RELATIONSHIP_TRAIT.LOVER, RELATIONSHIP_TRAIT.AFFAIR)) {
        //            CreateWatchEvent(action, null, action.actor);
        //        } else if (HasRelationshipOfTypeWith(target, false, RELATIONSHIP_TRAIT.LOVER, RELATIONSHIP_TRAIT.AFFAIR)) {
        //            CreateWatchEvent(action, null, target);
        //        } else {
        //            combatComponent.AddAvoidInRange(action.actor, false);
        //            combatComponent.AddAvoidInRange(target);
        //        }
        //    } else if (action.goapType == INTERACTION_TYPE.PLAY_GUITAR && state.name == "Play Success" && GetNormalTrait<Trait>("MusicHater") == null) {
        //        int chance = UnityEngine.Random.Range(0, 100);
        //        if (chance < 25) { //25
        //            if (!HasRelationshipOfTypeWith(action.actor, RELATIONSHIP_TRAIT.ENEMY)) {
        //                CreateWatchEvent(action, null, action.actor);
        //            }
        //        }
        //    } else if (action.goapType == INTERACTION_TYPE.TABLE_POISON) {
        //        int chance = UnityEngine.Random.Range(0, 100);
        //        if (chance < 35) {
        //            CreateWatchEvent(action, null, action.actor);
        //        }
        //    } else if (action.goapType == INTERACTION_TYPE.CURSE_CHARACTER && state.name == "Curse Success") {
        //        int chance = UnityEngine.Random.Range(0, 100);
        //        if (chance < 35) {
        //            CreateWatchEvent(action, null, action.actor);
        //        }
        //    } else if ((action.goapType == INTERACTION_TYPE.TRANSFORM_TO_WOLF_FORM || action.goapType == INTERACTION_TYPE.REVERT_TO_NORMAL_FORM) && state.name == "Transform Success") {
        //        if (faction == action.actor.faction) {
        //            CreateWatchEvent(action, null, action.actor);
        //        }
        //    }
        //}
    }
    //In watch event, it's either the character watch an action or combat state, it cannot be both. (NOTE: Since 9/2/2019 Enabled watching of other states other than Combat)
    public void CreateWatchEvent(ActualGoapNode actionToWatch, CharacterState stateToWatch, Character targetCharacter) {
        string summary = $"Creating watch event for {name} with target {targetCharacter.name}";
        if (actionToWatch != null) {
            summary += $" involving {actionToWatch.goapName}";
        } else if (stateToWatch != null) {
            if (stateToWatch is CombatState) {
                summary += " involving Combat";
            } 
            //else if (stateToWatch is DouseFireState) {
            //    summary += " involving Douse Fire";
            //}

        }
        //if (currentActionNode != null && !currentActionNode.isDone && currentActionNode.action.goapType == INTERACTION_TYPE.WATCH) {
        //    summary += "\n-Already watching an action, will not watch another one...";
        //    PrintLogIfActive(summary);
        //    return;
        //}
        if (stateComponent.currentState != null/* && (stateComponent.currentState.characterState == CHARACTER_STATE.COMBAT || stateComponent.currentState.characterState == CHARACTER_STATE.BERSERKED || stateComponent.currentState.characterState == CHARACTER_STATE.DOUSE_FIRE)*/) {
            summary += "\n-In a state, must not watch...";
            logComponent.PrintLogIfActive(summary);
            return;
        }
        interruptComponent.TriggerInterrupt(INTERRUPT.Watch, targetCharacter);
        //if (!jobQueue.IsJobTopTypePriorityWhenAdded(JOB_TYPE.WATCH)) {
        //    summary += "\n-Watch job will not be top priority in queue in when added, will not watch...";
        //    return;
        //}
        //summary += "\nWatch event created.";
        //PrintLogIfActive(summary);
        //ActualGoapNode node = null;
        //if (actionToWatch != null) {
        //    node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.WATCH], this, targetCharacter, new object[] { actionToWatch }, 0);
        //} else if (stateToWatch != null) {
        //    node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.WATCH], this, targetCharacter, new object[] { stateToWatch }, 0);
        //}
        //GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, targetCharacter);
        //GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.WATCH, INTERACTION_TYPE.WATCH, targetCharacter, this);
        //goapPlan.SetDoNotRecalculate(true);
        //job.SetCannotBePushedBack(true);
        //job.SetAssignedPlan(goapPlan);

        //jobQueue.AddJobInQueue(job);
    }
    #endregion

    #region Combat Handlers
    //public void SetCombatCharacter(CombatCharacter combatCharacter) {
    //    _currentCombatCharacter = combatCharacter;
    //}
    /// <summary>
    /// This character was hit by an attack.
    /// </summary>
    /// <param name="characterThatAttacked">The character that attacked this.</param>
    /// <param name="state">The combat state that the attacker is in.</param>
    /// <param name="attackSummary">reference log of what happened.</param>
    public void OnHitByAttackFrom(Character characterThatAttacked, CombatState state, ref string attackSummary) {
        ELEMENTAL_TYPE elementalType = characterThatAttacked.combatComponent.elementalDamage.type;
        CombatManager.Instance.CreateHitEffectAt(this, elementalType);
        if (this.currentHP <= 0) {
            return; //if hp is already 0, do not deal damage
        }

        //If someone is attacked, relationship should deteriorate
        //TODO: SAVE THE allCharactersThatDegradeRel list so when loaded they will not be able to degrade rel again
        Character responsibleCharacter = null;
        if (state != null) {
            if (state.currentClosestHostile == this) {
                //Do not set as responsible character for unconscious trait if character is hit unintentionally
                //So, only set responsible character if currentClosestHostile is this character, meaning, this character is really the target
                responsibleCharacter = characterThatAttacked;
                //if (!state.allCharactersThatDegradedRel.Contains(this)) {
                //    RelationshipManager.Instance.RelationshipDegradation(characterThatAttacked, this);
                //    state.AddCharacterThatDegradedRel(this);
                //}
            }
        }

        this.AdjustHP(-characterThatAttacked.attackPower, elementalType, source: characterThatAttacked);
        attackSummary += $"\nDealt damage {stateComponent.character.attackPower}";
        //If the hostile reaches 0 hp, evalueate if he/she dies, get knock out, or get injured
        if (this.currentHP <= 0) {
            attackSummary += $"\n{this.name}'s hp has reached 0.";
            if (!characterThatAttacked.combatComponent.IsLethalCombatForTarget(this)) {
                traitContainer.AddTrait(this, "Unconscious", responsibleCharacter);
            } else {
                if(combatResultWeights == null) {
                    combatResultWeights = new WeightedDictionary<string>();
                } else {
                    combatResultWeights.Clear();
                }
                int deathWeight = 70; //70
                int unconsciousWeight = 30; //30
                // if (!characterThatAttacked.combatComponent.IsLethalCombatForTarget(this)) {
                //     deathWeight = 5;
                //     unconsciousWeight = 95;
                // }
                string rollLog =
                    $"{characterThatAttacked.name} attacked {name}, death weight: {deathWeight}, unconscious weight: {unconsciousWeight}, isLethal: {characterThatAttacked.combatComponent.IsLethalCombatForTarget(this)}";

                if (minion == null && !this.traitContainer.HasTrait("Unconscious")) {
                    combatResultWeights.AddElement("Unconscious", unconsciousWeight);
                    rollLog += "\n- Unconscious weight will be added";
                }
                //if (currentClosestHostile.GetNormalTrait<Trait>("Injured") == null) {
                //    loserResults.AddElement("Injured", 10);
                //}
                if (!isDead) {
                    combatResultWeights.AddElement("Death", deathWeight);
                    rollLog += "\n- Death weight will be added";
                }

                if (combatResultWeights.Count > 0) {
                    string result = combatResultWeights.PickRandomElementGivenWeights();
                    rollLog += $"\n- Pick result is: {result}";
                    characterThatAttacked.logComponent.PrintLogIfActive(rollLog);
                    attackSummary += $"\ncombat result is {result}"; ;
                    switch (result) {
                        case "Unconscious":
                            //Unconscious unconscious = new Unconscious();
                            traitContainer.AddTrait(this, "Unconscious", responsibleCharacter);
                            break;
                        case "Injured":
                            //Injured injured = new Injured();
                            traitContainer.AddTrait(this, "Injured", responsibleCharacter);
                            break;
                        case "Death":
                            string deathReason = "attacked";
                            if (!characterThatAttacked.combatComponent.IsLethalCombatForTarget(this)) {
                                deathReason = "accidental_attacked";
                            }
                            this.Death(deathReason, responsibleCharacter: responsibleCharacter);
                            break;
                    }
                } else {
                    rollLog += "\n- Dictionary is empty, no result!";
                    characterThatAttacked.logComponent.PrintLogIfActive(rollLog);
                }
            }

        }
        //else {
        //    Invisible invisible = characterThatAttacked.GetNormalTrait<Trait>("Invisible") as Invisible;
        //    if (invisible != null) {
        //        if (invisible.level == 1) {
        //            //Level 1 = remove invisible trait
        //            characterThatAttacked.RemoveTrait(invisible);
        //        } else if (invisible.level == 2) {
        //            //Level 2 = attacked character will be the only character to see
        //            invisible.AddCharacterThatCanSee(this);
        //        }
        //        //Level 3 = will not be seen forever
        //    }
        //}

        Messenger.Broadcast(Signals.CHARACTER_WAS_HIT, this, characterThatAttacked);
    }
    /// <summary>
    /// Function to check what a character will do when he/she sees a hostile.
    /// </summary>
    /// <returns>True or False(True if the character will not flee, and false if otherwise).</returns>
    public bool IsCombatReady() {
        if (IsHealthCriticallyLow()) {
            return false;
        }
        if (needsComponent.isStarving && !traitContainer.HasTrait("Vampiric")) {
            return false; //only characters that are not vampires will flee if they are starving
        }
        if (needsComponent.isExhausted) {
            return false;
        }
        return true;
    }
    #endregion

    #region RPG
    private void StartingLevel() {
        _level = 1;
        UpdateMaxHP();
        ResetToFullHP();
    }

    private bool hpMagicRangedStatMod;
    public virtual void LevelUp() {
        //Only level up once per day
        if (_level < CharacterManager.Instance.maxLevel) {
            _level += 1;
            //Add stats per level from class
            if (_characterClass.rangeType == RANGE_TYPE.MELEE) {//_characterClass.attackType == ATTACK_TYPE.PHYSICAL &&  = Commented this because if the character is a MAGICAL MELEE, he cannot level up
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
            UpdateMaxHP();
            //Reset to full health and sp
            ResetToFullHP();

            Messenger.Broadcast(Signals.CHARACTER_LEVEL_CHANGED, this);
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
        if (_characterClass.rangeType == RANGE_TYPE.MELEE) {//_characterClass.attackType == ATTACK_TYPE.PHYSICAL &&  = Commented this because if the character is a MAGICAL MELEE, he cannot level up
            AdjustAttackMod(_characterClass.attackPowerPerLevel * amount);
            AdjustSpeedMod(_characterClass.speedPerLevel * amount);
            AdjustMaxHPMod(_characterClass.hpPerLevel * amount);
        } else if (_characterClass.attackType == ATTACK_TYPE.PHYSICAL && _characterClass.rangeType == RANGE_TYPE.RANGED) {
            int multiplier = (amount < 0 ? -1 : 1);
            int range = amount * multiplier;
            if (range > 0) {
                for (int i = 0; i < range; i++) {
                    if (i % 2 == 0) {
                        //even
                        AdjustMaxHPMod(_characterClass.hpPerLevel * multiplier);
                    } else {
                        //odd
                        AdjustAttackMod(_characterClass.attackPowerPerLevel * multiplier);
                    }
                }
            }
            AdjustSpeedMod(_characterClass.speedPerLevel * amount);
        } else if (_characterClass.attackType == ATTACK_TYPE.MAGICAL && _characterClass.rangeType == RANGE_TYPE.RANGED) {
            int multiplier = (amount < 0 ? -1 : 1);
            int range = amount * multiplier;
            if (range > 0) {
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
            }
            AdjustSpeedMod(_characterClass.speedPerLevel * amount);
        }
        _level += amount;

        UpdateMaxHP();
        //Reset to full health and sp
        ResetToFullHP();
        //ResetToFullSP();
        Messenger.Broadcast(Signals.CHARACTER_LEVEL_CHANGED, this);
        //if (_playerCharacterItem != null) {
        //    _playerCharacterItem.UpdateMinionItem();
        //}
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
        if (_characterClass.rangeType == RANGE_TYPE.MELEE) {//_characterClass.attackType == ATTACK_TYPE.PHYSICAL &&  = Commented this because if the character is a MAGICAL MELEE, he cannot level up
            AdjustAttackMod(_characterClass.attackPowerPerLevel * difference);
            AdjustSpeedMod(_characterClass.speedPerLevel * difference);
            AdjustMaxHPMod(_characterClass.hpPerLevel * difference);
        } else if (_characterClass.attackType == ATTACK_TYPE.PHYSICAL && _characterClass.rangeType == RANGE_TYPE.RANGED) {
            int multiplier = (difference < 0 ? -1 : 1);
            int range = difference * multiplier;
            if (range > 0) {
                for (int i = 0; i < range; i++) {
                    if (i % 2 == 0) {
                        //even
                        AdjustMaxHPMod(_characterClass.hpPerLevel * multiplier);
                    } else {
                        //odd
                        AdjustAttackMod(_characterClass.attackPowerPerLevel * multiplier);
                    }
                }
            }
            AdjustSpeedMod(_characterClass.speedPerLevel * difference);
        } else if (_characterClass.attackType == ATTACK_TYPE.MAGICAL && _characterClass.rangeType == RANGE_TYPE.RANGED) {
            int multiplier = (difference < 0 ? -1 : 1);
            int range = difference * multiplier;
            if (range > 0) {
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
            }
            AdjustSpeedMod(_characterClass.speedPerLevel * difference);
        }

        UpdateMaxHP();
        //Reset to full health and sp
        ResetToFullHP();
        //ResetToFullSP();
        Messenger.Broadcast(Signals.CHARACTER_LEVEL_CHANGED, this);
        //if (_playerCharacterItem != null) {
        //    _playerCharacterItem.UpdateMinionItem();
        //}

        //Reset Experience
        //_experience = 0;
        //RecomputeMaxExperience();
    }
    public void AdjustExperience(int amount) {
        _experience += amount;
        if (_experience >= _maxExperience) {
            SetExperience(0);
            //LevelUp();
        }
    }
    public void SetExperience(int amount) {
        _experience = amount;
    }
    private void RecomputeMaxExperience() {
        _maxExperience = Mathf.CeilToInt(100f * ((Mathf.Pow((float) _level, 1.25f)) / 1.1f));
    }
    public void SetMaxExperience(int amount) {
        _maxExperience = amount;
    }
    //public void AdjustElementalWeakness(ELEMENT element, float amount) {
    //    _elementalWeaknesses[element] += amount;
    //}
    //public void AdjustElementalResistance(ELEMENT element, float amount) {
    //    _elementalResistances[element] += amount;
    //}
    //public void AdjustSP(int amount) {
    //    _sp += amount;
    //    _sp = Mathf.Clamp(_sp, 0, _maxSP);
    //}
    public void ResetToFullHP() {
        SetHP(maxHP);
    }
    //public void ResetToFullSP() {
    //    AdjustSP(_maxSP);
    //}
    //private float GetComputedPower() {
    //    float compPower = 0f;
    //    for (int i = 0; i < currentParty.characters.Count; i++) {
    //        compPower += currentParty.characters[i].attackPower;
    //    }
    //    return compPower;
    //}
    public void SetHP(int amount) {
        this._currentHP = amount;
    }
    //Adjust current HP based on specified paramater, but HP must not go below 0
    public virtual void AdjustHP(int amount, ELEMENTAL_TYPE elementalDamageType, bool triggerDeath = false, object source = null) {
        int previous = this._currentHP;
        this._currentHP += amount;
        this._currentHP = Mathf.Clamp(this._currentHP, 0, maxHP);
        Messenger.Broadcast(Signals.ADJUSTED_HP, this);
        if (marker != null) {
            if (marker.hpBarGO.activeSelf) {
                marker.UpdateHP();
            } else {
                if (amount <= 0) {
                    if (_currentHP > 0) {
                        //only show hp bar if hp was reduced and hp is greater than 0
                        marker.QuickShowHPBar();
                    }
                    Character responsibleCharacter = null;
                    if (source is Character) {
                        responsibleCharacter = source as Character;
                    }
                    CombatManager.Instance.ApplyElementalDamage(elementalDamageType, this, responsibleCharacter);
                }
            }
        }
        if (triggerDeath && previous != this._currentHP && this._currentHP <= 0) {
            if(source != null) {
                if (source is Character) {
                    Character character = source as Character;
                    Death("attacked", responsibleCharacter: character);
                } else {
                    string cause = "attacked";
                    if (source != null) {
                        cause += $"_{source}";
                    }
                    Death(cause);
                }
            } else {
                Death();
            }
        } else if (amount < 0 && IsHealthCriticallyLow()) {
            combatComponent.FlightAll();
            // Messenger.Broadcast(Signals.TRANSFER_ENGAGE_TO_FLEE_LIST, this, "critically low health");
        }
    }
    public void AdjustAttackMod(int amount) {
        attackPowerMod += amount;
        //currentAlterEgo.SetAttackPowerMod(attackPowerMod);
    }
    public void AdjustAttackPercentMod(int amount) {
        attackPowerPercentMod += amount;
        //currentAlterEgo.SetAttackPowerPercentMod(attackPowerPercentMod);
    }
    public void SetAttackMod(int amount) {//, bool includeAlterEgo = true
        attackPowerMod = amount;
        //if (includeAlterEgo) {
        //currentAlterEgo.SetAttackPowerMod(attackPowerMod);
        //}
    }
    public void SetAttackPercentMod(int amount) {//, bool includeAlterEgo = true
        attackPowerPercentMod = amount;
        //if (includeAlterEgo) {
        //currentAlterEgo.SetAttackPowerPercentMod(attackPowerPercentMod);
        //}
    }
    public void AdjustMaxHPMod(int amount) {
        int previousMaxHP = maxHP;
        maxHPMod += amount;
        //currentAlterEgo.SetMaxHPMod(maxHPMod);
        UpdateMaxHP();
        int currentMaxHP = maxHP;
        if (_currentHP > currentMaxHP || _currentHP == previousMaxHP) {
            _currentHP = currentMaxHP;
        }
    }
    public void AdjustMaxHPPercentMod(int amount) {
        int previousMaxHP = maxHP;
        maxHPPercentMod += amount;
        //currentAlterEgo.SetMaxHPPercentMod(maxHPPercentMod);
        UpdateMaxHP();
        int currentMaxHP = maxHP;
        if (_currentHP > currentMaxHP || _currentHP == previousMaxHP) {
            _currentHP = currentMaxHP;
        }
    }
    public void SetMaxHPMod(int amount) {//, bool includeAlterEgo = true
        int previousMaxHP = maxHP;
        maxHPMod = amount;
        //if (includeAlterEgo) {
        //currentAlterEgo.SetMaxHPMod(maxHPMod);
        //}
        UpdateMaxHP();
        int currentMaxHP = maxHP;
        if (_currentHP > currentMaxHP || _currentHP == previousMaxHP) {
            _currentHP = currentMaxHP;
        }
    }
    public void SetMaxHPPercentMod(int amount) {//, bool includeAlterEgo = true
        int previousMaxHP = maxHP;
        maxHPPercentMod = amount;
        //if (includeAlterEgo) {
        //currentAlterEgo.SetMaxHPPercentMod(maxHPPercentMod);
        //}
        UpdateMaxHP();
        int currentMaxHP = maxHP;
        if (_currentHP > currentMaxHP || _currentHP == previousMaxHP) {
            _currentHP = currentMaxHP;
        }
    }
    public void UpdateMaxHP() {
        _maxHP = (int) (((_characterClass.baseHP + maxHPMod) * (1f + ((_raceSetting.hpModifier + maxHPPercentMod) / 100f))) * 4f);
        if (_maxHP < 0) {
            _maxHP = 1;
        }
    }
    public void HPRecovery(float maxHPPercentage) {
        if (doNotRecoverHP <= 0 && currentHP < maxHP && currentHP > 0) {
            AdjustHP(Mathf.CeilToInt(maxHPPercentage * maxHP), ELEMENTAL_TYPE.Normal);
        }
    }
    public void AdjustSpeedMod(int amount) {
        speedMod += amount;
        //currentAlterEgo.SetSpeedMod(speedMod);
    }
    public void AdjustSpeedPercentMod(int amount) {
        speedPercentMod += amount;
        //currentAlterEgo.SetSpeedPercentMod(speedPercentMod);
    }
    public void SetSpeedMod(int amount) { //, bool includeAlterEgo = true
        speedMod = amount;
        //if (includeAlterEgo) {
        //currentAlterEgo.SetSpeedMod(speedMod);
        //}
    }
    public void SetSpeedPercentMod(int amount) { //, bool includeAlterEgo = true
        speedPercentMod = amount;
        //if (includeAlterEgo) {
        //currentAlterEgo.SetSpeedPercentMod(speedPercentMod);
        //}
    }
    public bool IsHealthFull() {
        return _currentHP >= maxHP;
    }
    public bool IsHealthCriticallyLow() {
        //chance based dependent on the character
        return currentHP < (maxHP * 0.2f);
    }
    #endregion

    #region Home
    /// <summary>
    /// Set this character's home settlement data.(Does nothing else)
    /// </summary>
    /// <param name="newHome">The character's new home</param>
    public void SetHomeRegion(Region newHome) {
        Region previousHome = homeRegion;
        homeRegion = newHome;

        //If a character sets his home, add his faction to the factions in the region
        //Subsequently, if character loses his home, remove his faction from the region only if there are no co faction resident in the region anymore
        if (faction != null) {
            if (newHome == null) {
                //If character loses home and he has previous home, remove him faction
                if (previousHome != null) {
                    bool removeFaction = true;
                    for (int i = 0; i < previousHome.residents.Count; i++) {
                        Character resident = previousHome.residents[i];
                        if (resident != this && resident.faction == faction) {
                            removeFaction = false;
                            break;
                        }
                    }
                    if (removeFaction) {
                        previousHome.RemoveFactionHere(faction);
                    }
                }
            } else {
                newHome.AddFactionHere(faction);
            }
        }
    }
    public void SetHomeStructure(IDwelling homeStructure) {
        this.homeStructure = homeStructure;
        //currentAlterEgo.SetHomeStructure(homeStructure);
    }
    public bool MigrateHomeTo(Settlement newHomeSettlement, IDwelling homeStructure = null, bool broadcast = true) {
        Settlement previousHome = null;
        if (homeSettlement != null) {
            previousHome = homeSettlement;
            previousHome.RemoveResident(this);
        }
        if (newHomeSettlement.AddResident(this, homeStructure)) {
            if (broadcast) {
                Messenger.Broadcast(Signals.CHARACTER_MIGRATED_HOME, this, previousHome, newHomeSettlement);
            }
            return true;
        }
        return false;
    }
    public void MigrateHomeStructureTo(IDwelling dwelling) {
        if (this.homeStructure != null) {
            if (this.homeStructure == dwelling) {
                return; //ignore change
            }
            //remove character from his/her old home
            this.homeStructure.RemoveResident(this);
        }
        if (dwelling != null) {
            //Added checking, because character can sometimes change home from dwelling to nothing.
            dwelling.AddResident(this);
        }
#if !WORLD_CREATION_TOOL
        //Debug.Log(GameManager.Instance.TodayLogString() + this.name + " changed home structure to " + dwelling?.ToString() ?? "None");
#endif
    }
    #endregion

    #region Traits
    public ITraitContainer traitContainer { get; private set; }
    public TraitProcessor traitProcessor => TraitManager.characterTraitProcessor;
    public void CreateTraitContainer() {
        traitContainer = new TraitContainer();
    }
    public void CreateInitialTraitsByClass() {
        if (minion == null && race != RACE.DEMON && !(this is Summon)) { //only generate buffs and flaws for non minion characters. Reference: https://trello.com/c/pC9hBih0/2781-demonic-minions-should-not-have-pregenerated-buff-and-flaw-traits
 
            List<string> buffTraits = new List<string>(TraitManager.Instance.buffTraitPool);
            List<string> flawTraits = new List<string>(TraitManager.Instance.flawTraitPool);
            List<string> neutralTraits = new List<string>(TraitManager.Instance.neutralTraitPool);

            //First trait is random buff trait
            string chosenBuffTraitName;
            if (buffTraits.Count > 0) {
                int index = UnityEngine.Random.Range(0, buffTraits.Count);
                chosenBuffTraitName = buffTraits[index];
                buffTraits.RemoveAt(index);
            } else {
                throw new Exception("There are no buff traits!");
            }


            traitContainer.AddTrait(this, chosenBuffTraitName);
            Trait buffTrait = traitContainer.GetNormalTrait<Trait>(chosenBuffTraitName);
            if (buffTrait.mutuallyExclusive != null) {
                buffTraits = CollectionUtilities.RemoveElements(ref buffTraits, buffTrait.mutuallyExclusive); //update buff traits pool to accomodate new trait
                neutralTraits = CollectionUtilities.RemoveElements(ref neutralTraits, buffTrait.mutuallyExclusive); //update neutral traits pool to accomodate new trait
                flawTraits = CollectionUtilities.RemoveElements(ref flawTraits, buffTrait.mutuallyExclusive); //update flaw traits pool to accomodate new trait
            }


            //Second trait is a random buff or neutral trait
            string chosenBuffOrNeutralTraitName;
            if (buffTraits.Count > 0 && neutralTraits.Count > 0) {
                if (UnityEngine.Random.Range(0, 2) == 0) {
                    //Buff trait
                    int index = UnityEngine.Random.Range(0, buffTraits.Count);
                    chosenBuffOrNeutralTraitName = buffTraits[index];
                    buffTraits.RemoveAt(index);
                } else {
                    //Neutral trait
                    int index = UnityEngine.Random.Range(0, neutralTraits.Count);
                    chosenBuffOrNeutralTraitName = neutralTraits[index];
                    neutralTraits.RemoveAt(index);
                }
            } else {
                if (buffTraits.Count > 0) {
                    int index = UnityEngine.Random.Range(0, buffTraits.Count);
                    chosenBuffOrNeutralTraitName = buffTraits[index];
                    buffTraits.RemoveAt(index);
                } else {
                    int index = UnityEngine.Random.Range(0, neutralTraits.Count);
                    chosenBuffOrNeutralTraitName = neutralTraits[index];
                    neutralTraits.RemoveAt(index);
                }
            }

            traitContainer.AddTrait(this, chosenBuffOrNeutralTraitName);
            Trait buffOrNeutralTrait = traitContainer.GetNormalTrait<Trait>(chosenBuffOrNeutralTraitName);
            if (buffOrNeutralTrait.mutuallyExclusive != null) {
                buffTraits = CollectionUtilities.RemoveElements(ref buffTraits, buffOrNeutralTrait.mutuallyExclusive); //update buff traits pool to accomodate new trait
                neutralTraits = CollectionUtilities.RemoveElements(ref neutralTraits, buffOrNeutralTrait.mutuallyExclusive); //update neutral traits pool to accomodate new trait
                flawTraits = CollectionUtilities.RemoveElements(ref flawTraits, buffOrNeutralTrait.mutuallyExclusive); //update flaw traits pool to accomodate new trait
            }


            //Third trait is a random neutral or flaw traits
            string chosenFlawOrNeutralTraitName;
            if (flawTraits.Count > 0 && neutralTraits.Count > 0) {
                if (UnityEngine.Random.Range(0, 2) == 0) {
                    //Buff trait
                    int index = UnityEngine.Random.Range(0, flawTraits.Count);
                    chosenFlawOrNeutralTraitName = flawTraits[index];
                    flawTraits.RemoveAt(index);
                } else {
                    //Neutral trait
                    int index = UnityEngine.Random.Range(0, neutralTraits.Count);
                    chosenFlawOrNeutralTraitName = neutralTraits[index];
                    neutralTraits.RemoveAt(index);
                }
            } else {
                if (flawTraits.Count > 0) {
                    int index = UnityEngine.Random.Range(0, flawTraits.Count);
                    chosenFlawOrNeutralTraitName = flawTraits[index];
                    flawTraits.RemoveAt(index);
                } else {
                    int index = UnityEngine.Random.Range(0, neutralTraits.Count);
                    chosenFlawOrNeutralTraitName = neutralTraits[index];
                    neutralTraits.RemoveAt(index);
                }
            }
            traitContainer.AddTrait(this, chosenFlawOrNeutralTraitName);
        }

        traitContainer.AddTrait(this, "Character Trait");
        traitContainer.AddTrait(this, "Flammable");
        //traitContainer.AddTrait(this, "Drunkard");
        //traitContainer.AddTrait(this, "Accident Prone");

        defaultCharacterTrait = traitContainer.GetNormalTrait<CharacterTrait>("Character Trait");
    }
    public void AddTraitNeededToBeRemoved(Trait trait) {
        traitsNeededToBeRemoved.Add(trait);
    }
    public void RemoveTraitNeededToBeRemoved(Trait trait) {
        traitsNeededToBeRemoved.Remove(trait);
    }
    protected void ProcessTraitsOnTickStarted() {
        if (!interruptComponent.isInterrupted) {
            traitContainer.ProcessOnTickStarted(this);
        }
    }
    private void ProcessTraitsOnTickEnded() {
        if (!interruptComponent.isInterrupted) {
            traitContainer.ProcessOnTickEnded(this);
        }
    }
    private void ProcessTraitsOnHourStarted() {
        if (!interruptComponent.isInterrupted) {
            traitContainer.ProcessOnHourStarted(this);
        }
    }
    #endregion

    #region Minion
    public void SetMinion(Minion minion) {
        if (_minion != null && minion == null) {
            Messenger.Broadcast(Signals.CHARACTER_BECOMES_NON_MINION_OR_SUMMON, this);
            moodComponent.OnCharacterNoLongerMinionOrSummon();
            Assert.IsTrue(moodComponent.executeMoodChangeEffects);
        } else if (_minion == null && minion != null) {
            Messenger.Broadcast(Signals.CHARACTER_BECOMES_MINION_OR_SUMMON, this);
            moodComponent.OnCharacterBecomeMinionOrSummon();
            Assert.IsFalse(moodComponent.executeMoodChangeEffects);
        }
        _minion = minion;
        visuals.CreateWholeImageMaterial();
    }
    public void RecruitAsMinion(UnsummonedMinionData minionData) {
        if (stateComponent.currentState != null) {
            stateComponent.ExitCurrentState();
        }
        //else if (stateComponent.stateToDo != null) {
        //    stateComponent.SetStateToDo(null);
        //}

        //ForceCancelAllJobsTargettingCharacter(false, "target became a minion");
        //StopCurrentActionNode(reason: "Became a minion");
        //if (currentActionNode != null && !currentActionNode.cannotCancelAction) {
        //    currentActionNode.StopAction(reason: "Became a minion");
        //}
        Messenger.Broadcast(Signals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI, this as IPointOfInterest, "target became a minion");
        CancelAllJobs();

        Character carrier = isBeingCarriedBy;
        if (carrier != null) {
            carrier.UncarryPOI(this);
        }
        // if (ownParty.isCarryingAnyPOI) {
        //     ownParty.RemovePOI();
        // }
        ChangeFactionTo(PlayerManager.Instance.player.playerFaction);
        MigrateHomeTo(PlayerManager.Instance.player.playerSettlement);

        //currentArea.RemoveCharacterFromLocation(this.currentParty);
        currentRegion?.RemoveCharacterFromLocation(this);

        needsComponent.ResetFullnessMeter();
        needsComponent.ResetHappinessMeter();
        needsComponent.ResetTirednessMeter();
        //PlayerManager.Instance.player.demonicPortal.AddCharacterToLocation(this.currentParty);

        Minion newMinion = PlayerManager.Instance.player.CreateNewMinion(this, false);
        newMinion.character.SetName(minionData.minionName);
        ChangeRace(RACE.DEMON);
        ChangeClass(minionData.className);
        // AssignRole(CharacterRole.MINION);
        newMinion.SetCombatAbility(minionData.combatAbility);


        PlayerManager.Instance.player.playerSettlement.region.AddCharacterToLocation(this);

        if (PlayerManager.Instance.player.minions.Count < PlayerDB.MAX_MINIONS) {
            PlayerManager.Instance.player.AddMinion(newMinion);
            UIManager.Instance.ShowImportantNotification(GameManager.Instance.Today(), "Gained new Minion!", null);
        } else {
            UIManager.Instance.ShowImportantNotification(GameManager.Instance.Today(), "Gained new Minion!", () => PlayerManager.Instance.player.AddMinion(newMinion, true));
        }
    }
    #endregion

    #region Interaction
    //public void AddInteractionType(INTERACTION_TYPE type) {
    //    if (!currentInteractionTypes.Contains(type)) {
    //        currentInteractionTypes.Add(type);
    //    }
    //}
    //public void RemoveInteractionType(INTERACTION_TYPE type) {
    //    currentInteractionTypes.Remove(type);
    //}
    #endregion

    #region Action Planning and Job Processing
    private void DailyGoapProcesses() {
        needsComponent.DailyGoapProcesses();
    }
    public virtual void OnTickStartedWhileSeized() {
        CheckMissing();
    }
    protected virtual void OnTickStarted() {
        //What happens every start of tick

        //Check Trap Structure
        trapStructure.IncrementCurrentDuration(1);

        //Out of combat hp recovery
        if (!isDead && !isInCombat) {
            HPRecovery(0.0025f);
        }
        CheckMissing();
        ProcessTraitsOnTickStarted();
        StartTickGoapPlanGeneration();
    }
    protected virtual void OnTickEnded() {
        ProcessForcedCancelJobsOnTickEnded();
        interruptComponent.OnTickEnded();
        stateComponent.OnTickEnded();
        ProcessTraitsOnTickEnded();
        EndTickPerformJobs();
    }
    protected virtual void OnHourStarted() {
        ProcessTraitsOnHourStarted();
    }
    protected void StartTickGoapPlanGeneration() {
        //This is to ensure that this character will not be idle forever
        //If at the start of the tick, the character is not currently doing any action, and is not waiting for any new plans, it means that the character will no longer perform any actions
        //so start doing actions again
        //SetHasAlreadyAskedForPlan(false);
        needsComponent.PlanScheduledFullnessRecovery(this);
        needsComponent.PlanScheduledTirednessRecovery(this);
        if (CanPlanGoap()) {
            PerStartTickActionPlanning();
        }
    }
    public bool CanPlanGoap() {
        //If there is no settlement, it means that there is no inner map, so character must not do goap actions, jobs, and plans
        //characters that cannot witness, cannot plan actions.
        //minion == null &&
        return !isDead && isStoppedByOtherCharacter <= 0 && canPerform
            && currentActionNode == null && planner.status == GOAP_PLANNING_STATUS.NONE && jobQueue.jobsInQueue.Count <= 0
            && !marker.hasFleePath && stateComponent.currentState == null && IsInOwnParty() && !interruptComponent.isInterrupted;
    }
    public void EndTickPerformJobs() {
        if (CanPerformEndTickJobs()) {
            if (!jobQueue.jobsInQueue[0].ProcessJob()) {
                PerformTopPriorityJob();
            }
        }
    }
    public bool CanPerformEndTickJobs() {
        return !isDead && isStoppedByOtherCharacter <= 0 /*&& canWitness*/
            && currentActionNode == null && planner.status == GOAP_PLANNING_STATUS.NONE && jobQueue.jobsInQueue.Count > 0 
            && currentParty.icon.isTravellingOutside == false && !marker.hasFleePath 
            && stateComponent.currentState == null && IsInOwnParty() && !interruptComponent.isInterrupted; //minion == null && doNotDisturb <= 0
    }
    //public void PlanGoapActions() {
    //    if (!IsInOwnParty() || ownParty.icon.isTravelling || _doNotDisturb > 0 /*|| isWaitingForInteraction > 0 */ || isDead || marker.hasFleePath) {
    //        return; //if this character is not in own party, is a defender or is travelling or cannot be disturbed, do not generate interaction
    //    }
    //    if (stateComponent.currentState != null) {
    //        //Debug.LogWarning(name + " is currently in " + stateComponent.currentState.stateName + " state, can't plan actions!");
    //        return;
    //    }
    //    //if (stateComponent.stateToDo != null) {
    //    //    Debug.LogWarning("Will about to do " + stateComponent.stateToDo.stateName + " state, can't plan actions!");
    //    //    return;
    //    //}
    //    //if(specificAction != null) {
    //    //    WillAboutToDoAction(specificAction);
    //    //    return;
    //    //}
    //    if (allGoapPlans.Count > 0) {
    //        //StopDailyGoapPlanGeneration();
    //        PerformGoapPlans();
    //        //SchedulePerformGoapPlans();
    //    } else {
    //        PlanPerTickGoap();
    //    }
    //}
    public void PerStartTickActionPlanning() {
        //if (_hasAlreadyAskedForPlan || isDead) {
        //    return;
        //}
        //SetHasAlreadyAskedForPlan(true);
        if (returnedToLife) {
            //characters that have returned to life will just stroll.
            PlanIdleStrollOutside(); //currentStructure
            return;
        }
        string idleLog = OtherIdlePlans();
        logComponent.PrintLogIfActive(idleLog);
        //if (!PlanJobQueueFirst()) {
        //    if (!PlanFullnessRecoveryActions()) {
        //        if (!PlanTirednessRecoveryActions()) {
        //            if (!PlanHappinessRecoveryActions()) {
        //                if (!PlanWorkActions()) {
        //                    string idleLog = OtherIdlePlans();
        //                    PrintLogIfActive(idleLog);
        //                }
        //            }
        //        }
        //    }
        //}
    }
    //private bool OtherPlanCreations() {
    //    int chance = UnityEngine.Random.Range(0, 100);
    //    if (traitContainer.GetNormalTrait<Trait>("Berserked") != null) {
    //        if (chance < 15) {
    //            Character target = specificLocation.GetRandomCharacterAtLocationExcept(this);
    //            if (target != null) {
    //                StartGOAP(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_NON_POSITIVE_TRAIT, conditionKey = "Disabler", targetPOI = target }, target, GOAP_CATEGORY.NONE);
    //                return true;
    //            }
    //        } else {
    //            chance = UnityEngine.Random.Range(0, 100);
    //            if (chance < 15) {
    //                IPointOfInterest target = specificLocation.GetRandomTileObject();
    //                if (target != null) {
    //                    StartGOAP(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.DESTROY, conditionKey = target, targetPOI = target }, target, GOAP_CATEGORY.NONE);
    //                    return true;
    //                }
    //            }
    //        }
    //    }
    //    return false;
    //}

    public bool PlanIdleStroll(LocationStructure targetStructure, LocationGridTile targetTile = null) {
        CharacterStateJob job = JobManager.Instance.CreateNewCharacterStateJob(JOB_TYPE.STROLL, CHARACTER_STATE.STROLL, this);
        jobQueue.AddJobInQueue(job);
        //if (currentStructure == targetStructure) {
        //    stateComponent.SwitchToState(CHARACTER_STATE.STROLL);
        //} else {
        //    MoveToAnotherStructure(targetStructure, targetTile, null, () => stateComponent.SwitchToState(CHARACTER_STATE.STROLL));
        //}
        return true;
    }
    public bool PlanIdleStrollOutside() {
        CharacterStateJob job = JobManager.Instance.CreateNewCharacterStateJob(JOB_TYPE.STROLL, CHARACTER_STATE.STROLL_OUTSIDE, this);
        jobQueue.AddJobInQueue(job);
        //if (currentStructure == targetStructure) {
        //    stateComponent.SwitchToState(CHARACTER_STATE.STROLL_OUTSIDE);
        //} else {
        //    MoveToAnotherStructure(targetStructure, targetTile, null, () => stateComponent.SwitchToState(CHARACTER_STATE.STROLL_OUTSIDE));
        //}
        return true;
    }
    public bool PlanIdleReturnHome() { //bool forceDoAction = false
        ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.RETURN_HOME], this, this, null, 0);
        GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, this);
        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.IDLE_RETURN_HOME, INTERACTION_TYPE.RETURN_HOME, this, this);
        goapPlan.SetDoNotRecalculate(true);
        job.SetCannotBePushedBack(true);
        job.SetAssignedPlan(goapPlan);
        jobQueue.AddJobInQueue(job);
        //if (GetTrait("Berserker") != null) {
        //    //Return home becomes stroll if the character has berserker trait
        //    PlanIdleStroll(currentStructure);
        //} else {
        //GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.RETURN_HOME, this, this);
        //goapAction.SetTargetStructure();
        //GoapNode goalNode = new GoapNode(null, goapAction.cost, goapAction);
        //GoapPlan goapPlan = new GoapPlan(goalNode, new GOAP_EFFECT_CONDITION[] { GOAP_EFFECT_CONDITION.NONE }, GOAP_CATEGORY.IDLE);
        //goapPlan.ConstructAllNodes();
        ////AddPlan(goapPlan, true);
        //AddPlan(goapPlan);
        //if (forceDoAction) {
        //    PerformTopPriorityJob();
        //}
        //PlanGoapActions(goapAction);
        //}
        return true;
    }
    private string OtherIdlePlans() {
        string log = $" IDLE PLAN FOR {name}";
        if (isDead) {
            log += $"{this.name} is already dead not planning other idle plans.";
            return log;
        }
        if (!isFactionless) { }
        string classIdlePlanLog = behaviourComponent.RunBehaviour();
        log += $"\n{classIdlePlanLog}";
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
        //        if (specificLocation == homeSettlement && currentStructure.structureType == STRUCTURE_TYPE.WORK_AREA) {
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
    public void PlanIdle(JOB_TYPE jobType, INTERACTION_TYPE type, IPointOfInterest target, object[] otherData = null) {
        ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[type], this, target, otherData, 0);
        GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, target);
        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, type, target, this);
        goapPlan.SetDoNotRecalculate(true);
        job.SetCannotBePushedBack(true);
        job.SetAssignedPlan(goapPlan);
        jobQueue.AddJobInQueue(job);

        //GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(type, this, target);
        //GoapNode goalNode = new GoapNode(null, goapAction.cost, goapAction);
        //GoapPlan goapPlan = new GoapPlan(goalNode, new GOAP_EFFECT_CONDITION[] { GOAP_EFFECT_CONDITION.NONE }, GOAP_CATEGORY.IDLE);
        //goapPlan.ConstructAllNodes();
        //AddPlan(goapPlan);
        //PlanGoapActions(goapAction);
    }
    public void PlanIdle(JOB_TYPE jobType, GoapEffect effect, IPointOfInterest target) {
        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, effect, target, this);
        jobQueue.AddJobInQueue(job);
        //if (effect.targetPOI != null && effect.targetPOI != this) {
        //    AddAwareness(effect.targetPOI);
        //}
        //StartGOAP(effect, effect.targetPOI, GOAP_CATEGORY.IDLE);

        //GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(type, this, target);
        //GoapNode goalNode = new GoapNode(null, goapAction.cost, goapAction);
        //GoapPlan goapPlan = new GoapPlan(goalNode, new GOAP_EFFECT_CONDITION[] { GOAP_EFFECT_CONDITION.NONE }, GOAP_CATEGORY.IDLE);
        //goapPlan.ConstructAllNodes();
        //AddPlan(goapPlan);
        //PlanGoapActions(goapAction);
    }
    public void PlanAction(JOB_TYPE jobType, INTERACTION_TYPE type, IPointOfInterest target, object[] otherData = null) {
        ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[type], this, target, otherData, 0);
        GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, target);
        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, type, target, this);
        goapPlan.SetDoNotRecalculate(true);
        job.SetCannotBePushedBack(true);
        job.SetAssignedPlan(goapPlan);
        jobQueue.AddJobInQueue(job);
    }
    public Character GetDisabledCharacterToCheckOut() {
        //List<Character> charactersWithRel = relationshipContainer.relationships.Keys.Where(x => x is AlterEgoData).Select(x => (x as AlterEgoData).owner).ToList();
        List<Character> charactersWithRel = relationshipContainer.charactersWithOpinion;
        if (charactersWithRel.Count > 0) {
            List<Character> positiveCharacters = new List<Character>();
            for (int i = 0; i < charactersWithRel.Count; i++) {
                Character character = charactersWithRel[i];
                if(character.isDead || character.isMissing || homeStructure == character.homeStructure) {
                    continue;
                }
                if (relationshipContainer.HasOpinionLabelWithCharacter(character, OpinionComponent.Acquaintance, 
                    OpinionComponent.Friend, OpinionComponent.Close_Friend)) {
                    if (character.traitContainer.HasTrait("Paralyzed", "Catatonic")) {
                        positiveCharacters.Add(character);
                    }
                }
            }
            if (positiveCharacters.Count > 0) {
                return positiveCharacters[UnityEngine.Random.Range(0, positiveCharacters.Count)];
            }
        }
        return null;
    }
    private void SetLastAssaultedCharacter(Character character) {
        lastAssaultedCharacter = character;
        if (character != null) {
            //cooldown
            GameDate dueDate = GameManager.Instance.Today().AddTicks(GameManager.ticksPerHour);
            SchedulingManager.Instance.AddEntry(dueDate, () => RemoveLastAssaultedCharacter(character), this);
        }
    }
    private void RemoveLastAssaultedCharacter(Character characterToRemove) {
        if (lastAssaultedCharacter == characterToRemove) {
            SetLastAssaultedCharacter(null);
        }
    }
    public void SetIsConversing(bool state) {
        isConversing = state;
        if(marker != null) {
            marker.UpdateActionIcon();
        }
    }
    //public void SetIsFlirting(bool state) {
    //    _isFlirting = state;
    //}
    public void AddAdvertisedAction(INTERACTION_TYPE type) {
        advertisedActions.Add(type);
    }
    public void RemoveAdvertisedAction(INTERACTION_TYPE type) {
        advertisedActions.Remove(type);
    }
    #endregion

    #region Inventory
    //public bool ObtainTokenFrom(Character target, SpecialToken token, bool changeCharacterOwnership = true) {
    //    if (target.UnobtainToken(token)) {
    //        ObtainToken(token, changeCharacterOwnership);
    //        return true;
    //    }
    //    return false;
    //}
    public bool ObtainItem(TileObject item, bool changeCharacterOwnership = true) {
        if (AddItem(item)) {
            // item.SetFactionOwner(this.faction);
            item.SetInventoryOwner(this);
            if (changeCharacterOwnership) {
                item.SetCharacterOwner(this);
            } else {
                if (item.characterOwner == null) {
                    item.SetCharacterOwner(this);
                }
            }
            return true;
        }
        return false;
        //token.AdjustQuantity(-1);
    }
    public bool UnobtainItem(TileObject item) {
        if (RemoveItem(item)) {
            item.SetInventoryOwner(null);
            return true;
        }
        return false;
    }
    public bool UnobtainItem(TILE_OBJECT_TYPE itemType) {
        TileObject removedItem = RemoveItem(itemType);
        if (removedItem != null) {
            removedItem.SetInventoryOwner(null);
            return true;
        }
        return false;
    }
    //public bool ConsumeToken(SpecialToken token) {
    //    token.OnConsumeToken(this);
    //    if (token.uses <= 0) {
    //        return RemoveToken(token);
    //    }
    //    return false;
    //}
    private bool AddItem(TileObject item) {
        if (!items.Contains(item)) {
            items.Add(item);
            Messenger.Broadcast(Signals.CHARACTER_OBTAINED_ITEM, item, this);
            return true;
        }
        return false;
    }
    private bool RemoveItem(TileObject item) {
        if (items.Remove(item)) {
            Messenger.Broadcast(Signals.CHARACTER_LOST_ITEM, item, this);
            return true;
        }
        return false;
    }
    private TileObject RemoveItem(TILE_OBJECT_TYPE itemType) {
        TileObject removedItem = null;
        for (int i = 0; i < items.Count; i++) {
            if (items[i].tileObjectType == itemType) {
                removedItem = items[i];
                RemoveItem(i);
            }
        }
        return removedItem;
    }
    private bool RemoveItem(int index) {
        TileObject item = items[index];
        if (item != null) {
            items.RemoveAt(index);
            Messenger.Broadcast(Signals.CHARACTER_LOST_ITEM, item, this);
            return true;
        }
        return false;
    }
    public void DropItem(TileObject item, LocationGridTile gridTile = null, bool clearOwner = true) {
        if (UnobtainItem(item)) {
            //if (item.specialTokenType.CreatesObjectWhenDropped()) {
            //    structure.AddItem(item, gridTile);
            //    //location.AddSpecialTokenToLocation(token, structure, gridTile);
            //}
            LocationGridTile targetTile = gridTile;
            if (targetTile == null || targetTile.objHere != null) {
                targetTile = gridTileLocation.GetNearestUnoccupiedTileFromThis();
            }
            if (targetTile != null) {
                targetTile.structure.AddPOI(item, targetTile);
                if (clearOwner) {
                    item.SetCharacterOwner(null);
                }
            } else {
                logComponent.PrintLogErrorIfActive(
                    $"Cannot drop {item.nameWithID} of {name} because there is no target tile.");
            }
        }
    }
    public void DropAllItems(LocationGridTile tile) { //, bool removeFactionOwner = false
        while (isHoldingItem) {
            TileObject item = items[0];
            if (UnobtainItem(item)) {
                // if (removeFactionOwner) {
                //     item.SetFactionOwner(null);
                // }
                LocationGridTile targetTile = tile;
                if (targetTile == null || targetTile.objHere != null) {
                    targetTile = gridTileLocation.GetNearestUnoccupiedTileFromThis();
                }
                if (targetTile != null) {
                    targetTile.structure.AddPOI(item, targetTile);
                } else {
                    logComponent.PrintLogErrorIfActive(
                        $"Cannot drop {item.nameWithID} of {name} because there is no target tile.");
                }
                item.SetCharacterOwner(null);
                //if (item.specialTokenType.CreatesObjectWhenDropped()) {
                //    LocationGridTile targetTile = tile.GetNearestUnoccupiedTileFromThis();
                //    targetTile.structure.AddItem(item, targetTile);
                //    //location.AddSpecialTokenToLocation(token, structure, targetTile);
                //    if (structure != homeStructure) {
                //        //if this character drops this at a structure that is not his/her home structure, set the owner of the item to null
                //        item.SetCharacterOwner(null);
                //    }
                //} else {
                //    item.SetCharacterOwner(null);
                //}
            }
        }
    }
    public void PickUpItem(TileObject item, bool changeCharacterOwnership = true) {
        if (item.carriedByCharacter != null) {
            item.carriedByCharacter.UnobtainItem(item);
        }
        if (ObtainItem(item, changeCharacterOwnership)) {
            if (item.gridTileLocation != null) {
                item.gridTileLocation.structure.RemovePOI(item);
            }
        }
    }
    public void DestroyItem(TileObject item) {
        item.structureLocation.RemovePOI(item);
        //token.gridTileLocation.structure.location.RemoveSpecialTokenFromLocation(token);
    }
    // private void UpdateItemFactionOwner() {
    //     for (int i = 0; i < items.Count; i++) {
    //         TileObject item = items[i];
    //         item.SetFactionOwner(this.faction);
    //     }
    // }
    public TileObject GetItem(TileObject item) {
        for (int i = 0; i < items.Count; i++) {
            if (items[i] == item) {
                return items[i];
            }
        }
        return null;
    }
    public TileObject GetItem(TILE_OBJECT_TYPE itemType) {
        for (int i = 0; i < items.Count; i++) {
            if (items[i].tileObjectType == itemType) {
                return items[i];
            }
        }
        return null;
    }
    public TileObject GetItem(string itemName) {
        for (int i = 0; i < items.Count; i++) {
            if (items[i].name == itemName) {
                return items[i];
            }
        }
        return null;
    }
    public bool HasItem(TileObject item) {
        return GetItem(item) != null;
    }
    public bool HasItem(TILE_OBJECT_TYPE itemType) {
        return GetItem(itemType) != null;
    }
    public bool HasItem(string itemName) {
        return GetItem(itemName) != null;
    }
    // public List<TileObject> GetItemsOwned() {
    //     List<TileObject> itemsOwned = new List<TileObject>();
    //     //for (int i = 0; i < homeSettlement.possibleSpecialTokenSpawns.Count; i++) {
    //     //    SpecialToken token = homeSettlement.possibleSpecialTokenSpawns[i];
    //     //    if (token.characterOwner == this) {
    //     //        itemsOwned.Add(token);
    //     //    }
    //     //}
    //     if(homeStructure == null) {
    //         logComponent.PrintLogErrorIfActive(name + " error in GetItemsOwned no homestructure!");
    //     }
    //     for (int i = 0; i < homeStructure.itemsInStructure.Count; i++) {
    //         SpecialToken token = homeStructure.itemsInStructure[i];
    //         if (token.characterOwner == this) {
    //             itemsOwned.Add(token);
    //         }
    //     }
    //     for (int i = 0; i < items.Count; i++) {
    //         SpecialToken token = items[i];
    //         if (token.characterOwner == this) {
    //             itemsOwned.Add(token);
    //         }
    //     }
    //     return itemsOwned;
    // }
    // public int GetNumOfItemsOwned() {
    //     int count = 0;
    //     //for (int i = 0; i < homeSettlement.possibleSpecialTokenSpawns.Count; i++) {
    //     //    SpecialToken token = homeSettlement.possibleSpecialTokenSpawns[i];
    //     //    if (token.characterOwner == this) {
    //     //        count++;
    //     //    }
    //     //}
    //     for (int i = 0; i < homeStructure.itemsInStructure.Count; i++) {
    //         SpecialToken token = homeStructure.itemsInStructure[i];
    //         if (token.characterOwner == this) {
    //             count++;
    //         }
    //     }
    //     
    //     for (int i = 0; i < items.Count; i++) {
    //         SpecialToken token = items[i];
    //         if (token.characterOwner == this) {
    //             count++;
    //         }
    //     }
    //     return count;
    // }
    // public int GetTokenCountInInventory(SPECIAL_TOKEN tokenType) {
    //     int count = 0;
    //     for (int i = 0; i < items.Count; i++) {
    //         if (items[i].specialTokenType == tokenType) {
    //             count++;
    //         }
    //     }
    //     return count;
    // }
    // public bool HasExtraTokenInInventory(SPECIAL_TOKEN tokenType) {
    //     if (role.IsRequiredItem(tokenType)) {
    //         //if the specified token type is required by this character's role, check if this character has any extras
    //         int requiredAmount = role.GetRequiredItemAmount(tokenType);
    //         if (GetTokenCountInInventory(tokenType) > requiredAmount) {
    //             return true;
    //         }
    //         return false;
    //     } else {
    //         return HasTokenInInventory(tokenType);
    //     }
    // }
    // public bool OwnsItemOfType(SPECIAL_TOKEN tokenType) {
    //     for (int i = 0; i < homeStructure.itemsInStructure.Count; i++) {
    //         SpecialToken token = homeStructure.itemsInStructure[i];
    //         if (token.characterOwner == this && token.specialTokenType == tokenType) {
    //             return true;
    //         }
    //     }
    //     for (int i = 0; i < items.Count; i++) {
    //         SpecialToken token = items[i];
    //         if (token.characterOwner == this && token.specialTokenType == tokenType) {
    //             return true;
    //         }
    //     }
    //     return false;
    // }
    #endregion

    #region Share Intel
    //public List<string> ShareIntel(Intel intel) {
    //    List<string> dialogReactions = new List<string>();
    //    //if (intel is EventIntel) {
    //    //    EventIntel ei = intel as EventIntel;
    //    //    if (ei.action.currentState != null && ei.action.currentState.shareIntelReaction != null) {
    //    //        dialogReactions.AddRange(ei.action.currentState.shareIntelReaction.Invoke(this, ei, SHARE_INTEL_STATUS.INFORMED));
    //    //    }
    //    //    //if the determined reactions list is empty, check the default reactions
    //    //    if (dialogReactions.Count == 0) {
    //    //        bool doesNotConcernMe = false;
    //    //        //If the event's actor and target do not have any relationship with the recipient and are not part of his faction, 
    //    //        //and if no item involved is owned by the recipient: "This does not concern me."
    //    //        if (!this.relationshipContainer.HasRelationshipWith(ei.action.actorAlterEgo)
    //    //            && ei.action.actor.faction != this.faction) {
    //    //            if (ei.action.poiTarget is Character) {
    //    //                Character otherCharacter = ei.action.poiTarget as Character;
    //    //                if (!this.relationshipContainer.HasRelationshipWith(ei.action.poiTargetAlterEgo)
    //    //                    && otherCharacter.faction != this.faction) {
    //    //                    doesNotConcernMe = true;
    //    //                }
    //    //            } else if (ei.action.poiTarget is TileObject) {
    //    //                TileObject obj = ei.action.poiTarget as TileObject;
    //    //                if (!obj.IsOwnedBy(this)) {
    //    //                    doesNotConcernMe = true;
    //    //                }
    //    //            } else if (ei.action.poiTarget is SpecialToken) {
    //    //                SpecialToken obj = ei.action.poiTarget as SpecialToken;
    //    //                if (obj.characterOwner != this) {
    //    //                    doesNotConcernMe = true;
    //    //                }
    //    //            }
    //    //        }

    //    //        if (ei.action.actor == this) {
    //    //            //If the actor and the recipient is the same: "I know what I did."
    //    //            dialogReactions.Add("I know what I did.");
    //    //        } else {
    //    //            if (doesNotConcernMe) {
    //    //                //The following events are too unimportant to merit any meaningful response: "What will I do with this random tidbit?"
    //    //                //-character picked up an item(not stealing)
    //    //                //-character prayed, daydreamed, played
    //    //                //- character slept
    //    //                //- character mined or chopped wood
    //    //                switch (ei.action.goapType) {
    //    //                    case INTERACTION_TYPE.PICK_UP:
    //    //                    case INTERACTION_TYPE.PRAY:
    //    //                    case INTERACTION_TYPE.DAYDREAM:
    //    //                    case INTERACTION_TYPE.PLAY:
    //    //                    case INTERACTION_TYPE.SLEEP:
    //    //                    case INTERACTION_TYPE.SLEEP_OUTSIDE:
    //    //                    case INTERACTION_TYPE.MINE:
    //    //                    case INTERACTION_TYPE.CHOP_WOOD:
    //    //                        dialogReactions.Add("What will I do with this random tidbit?");
    //    //                        break;
    //    //                    default:
    //    //                        dialogReactions.Add("This does not concern me.");
    //    //                        break;
    //    //                }

    //    //            } else {
    //    //                //Otherwise: "A proper response to this information has not been implemented yet."
    //    //                dialogReactions.Add("A proper response to this information has not been implemented yet.");
    //    //            }
    //    //        }
    //    //    }
    //    //    CreateInformedEventLog(intel.intelLog.node, false);
    //    //}
    //    //PlayerManager.Instance.player.RemoveIntel(intel);
    //    dialogReactions.Add("A proper response to this information has not been implemented yet.");
    //    return dialogReactions;
    //    //if (relationships.ContainsKey(intel.actor)) {
    //    //    if (!intel.isCompleted) {
    //    //        relationships[intel.actor].SetPlannedActionIntel(intel);
    //    //    } else {
    //    //        Debug.Log(GameManager.Instance.TodayLogString() + "The intel given to " + this.name + " regarding " + intel.actor.name + " has already been completed, not setting planned action...");
    //    //    }
    //    //    relationships[intel.actor].OnIntelGivenToCharacter(intel);
    //    //    PlayerManager.Instance.player.RemoveIntel(intel);
    //    //} else {
    //    //    Debug.Log(GameManager.Instance.TodayLogString() + this.name + " does not have a relationship with " + intel.actor.name + ". He/she doesn't care about any intel you give that is about " + intel.actor.name);
    //    //}
    //    //if (intel.target is Character) {
    //    //    Character target = intel.target as Character;
    //    //    if (relationships.ContainsKey(target)) {
    //    //        relationships[target].OnIntelGivenToCharacter(intel);
    //    //        PlayerManager.Instance.player.RemoveIntel(intel);
    //    //    }
    //    //}
    //}
    public string ShareIntel(Intel intel) {
        // PlayerManager.Instance.player.RemoveIntel(intel);
        if (!intel.node.awareCharacters.Contains(this)) {
            string reaction = reactionComponent.ReactTo(intel.node, SHARE_INTEL_STATUS.INFORMED);
            intel.node.AddAwareCharacter(this);
            return reaction;
        }
        return "aware";
    }
    #endregion

    #region Awareness
    public void LogAwarenessList() {
        string log = $"--------------AWARENESS LIST OF {name}-----------------";
        foreach (KeyValuePair<POINT_OF_INTEREST_TYPE, List<IPointOfInterest>> kvp in currentRegion.awareness) {
            log += $"\n{kvp.Key}: ";
            for (int i = 0; i < kvp.Value.Count; i++) {
                if (i > 0) {
                    log += ", ";
                }
                log += kvp.Value[i].ToString();
            }
        }
        logComponent.PrintLogIfActive(log);
    }
    #endregion

    #region Point Of Interest
    //Returns the chosen action for the plan
    public GoapAction AdvertiseActionsToActor(Character actor, GoapEffect precondition, JobQueueItem job,
        Dictionary<INTERACTION_TYPE, object[]> otherData, ref int cost, ref string log) {
        GoapAction chosenAction = null;
        if (advertisedActions != null && advertisedActions.Count > 0) {//&& IsAvailable()
            bool isCharacterAvailable = IsAvailable();
            //List<GoapAction> usableActions = new List<GoapAction>();
            GoapAction lowestCostAction = null;
            int currentLowestCost = 0;
            log += $"\n--Choices for {precondition}";
            log += "\n--";
            for (int i = 0; i < advertisedActions.Count; i++) {
                INTERACTION_TYPE currType = advertisedActions[i];
                GoapAction action = InteractionManager.Instance.goapActionData[currType];
                if (!isCharacterAvailable && !action.canBeAdvertisedEvenIfActorIsUnavailable) {
                    //if this character is not available, check if the current action type can be advertised even when the character is inactive.
                    continue; //skip
                }
                if (PathfindingManager.Instance.HasPath(actor.gridTileLocation, gridTileLocation) && RaceManager.Instance.CanCharacterDoGoapAction(actor, currType)) {
                    object[] data = null;
                    if (otherData != null) {
                        if (otherData.ContainsKey(currType)) {
                            data = otherData[currType];
                        } else if (otherData.ContainsKey(INTERACTION_TYPE.NONE)) {
                            data = otherData[INTERACTION_TYPE.NONE];
                        }
                    }
                    //object[] otherActionData = null;
                    //if (otherData.ContainsKey(currType)) {
                    //    otherActionData = otherData[currType];
                    //}
                    if (action.CanSatisfyRequirements(actor, this, data)
                        && action.WillEffectsSatisfyPrecondition(precondition, actor, this, data)) { //&& InteractionManager.Instance.CanSatisfyGoapActionRequirementsOnBuildTree(currType, actor, this, data)
                        int actionCost = action.GetCost(actor, this, job, data);
                        log += $"({actionCost}){action.goapName}-{nameWithID}, ";
                        if (lowestCostAction == null || actionCost < currentLowestCost) {
                            lowestCostAction = action;
                            currentLowestCost = actionCost;
                        }
                        //GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(currType, actor, this);
                        //if (goapAction != null) {
                        //    if (data != null) {
                        //        goapAction.InitializeOtherData(data);
                        //    }
                        //    usableActions.Add(goapAction);
                        //} else {
                        //    throw new System.Exception("Goap action " + currType.ToString() + " is null!");
                        //}
                    }
                }
            }
            cost = currentLowestCost;
            chosenAction = lowestCostAction;
            //return usableActions;
        }
        return chosenAction;
    }
    public bool CanAdvertiseActionToActor(Character actor, GoapAction action, JobQueueItem job,
        Dictionary<INTERACTION_TYPE, object[]> otherData, ref int cost) {
        if((IsAvailable() || action.canBeAdvertisedEvenIfActorIsUnavailable) 
            && advertisedActions != null && advertisedActions.Contains(action.goapType)
            && actor.trapStructure.SatisfiesForcedStructure(this)
            && RaceManager.Instance.CanCharacterDoGoapAction(actor, action.goapType)
            && PathfindingManager.Instance.HasPath(actor.gridTileLocation, gridTileLocation)) {
            object[] data = null;
            if (otherData != null) {
                if (otherData.ContainsKey(action.goapType)) {
                    data = otherData[action.goapType];
                } else if (otherData.ContainsKey(INTERACTION_TYPE.NONE)) {
                    data = otherData[INTERACTION_TYPE.NONE];
                }
            }
            if (action.CanSatisfyRequirements(actor, this, data)) {
                cost = action.GetCost(actor, this, job, data);
                return true;
            }
        }
        return false;
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
    public void OnPlacePOI() { /*FOR INTERFACE ONLY*/ }
    public void OnDestroyPOI() { /*FOR INTERFACE ONLY*/ }
    public virtual bool IsStillConsideredPartOfAwarenessByCharacter(Character character) {
        if(character.currentRegion == currentRegion && !isBeingSeized && !isMissing) {
            if (!isDead && currentParty.icon.isTravellingOutside) {
                return false;
            }
            if (isDead && marker == null) {
                return false;
            }
            return true;
        }
        return false;
    }
    #endregion

    #region Goap
    //public void SetNumWaitingForGoapThread(int amount) {
    //    _numOfWaitingForGoapThread = amount;
    //}
    public void ConstructInitialGoapAdvertisementActions() {
        //poiGoapActions = new List<INTERACTION_TYPE>();
        advertisedActions.Add(INTERACTION_TYPE.ASSAULT);
        advertisedActions.Add(INTERACTION_TYPE.RESTRAIN_CHARACTER);
        //poiGoapActions.Add(INTERACTION_TYPE.STROLL);
        advertisedActions.Add(INTERACTION_TYPE.DAYDREAM);
        advertisedActions.Add(INTERACTION_TYPE.SLEEP_OUTSIDE);
        advertisedActions.Add(INTERACTION_TYPE.PRAY);
        //poiGoapActions.Add(INTERACTION_TYPE.EXPLORE);
        //poiGoapActions.Add(INTERACTION_TYPE.PATROL);
        //poiGoapActions.Add(INTERACTION_TYPE.TRAVEL);
        //poiGoapActions.Add(INTERACTION_TYPE.RETURN_HOME_LOCATION);
        //poiGoapActions.Add(INTERACTION_TYPE.HUNT_ACTION);
        advertisedActions.Add(INTERACTION_TYPE.PLAY);
        advertisedActions.Add(INTERACTION_TYPE.JUDGE_CHARACTER);
        advertisedActions.Add(INTERACTION_TYPE.CURSE_CHARACTER);
        advertisedActions.Add(INTERACTION_TYPE.ASK_FOR_HELP_SAVE_CHARACTER);
        advertisedActions.Add(INTERACTION_TYPE.ASK_FOR_HELP_REMOVE_POISON_TABLE);
        advertisedActions.Add(INTERACTION_TYPE.BURY_CHARACTER);
        advertisedActions.Add(INTERACTION_TYPE.INVITE);
        advertisedActions.Add(INTERACTION_TYPE.MAKE_LOVE);
        advertisedActions.Add(INTERACTION_TYPE.TANTRUM);
        //advertisedActions.Add(INTERACTION_TYPE.BREAK_UP);
        //advertisedActions.Add(INTERACTION_TYPE.PUKE);
        //advertisedActions.Add(INTERACTION_TYPE.SEPTIC_SHOCK);
        advertisedActions.Add(INTERACTION_TYPE.CARRY);
        advertisedActions.Add(INTERACTION_TYPE.DROP);
        //advertisedActions.Add(INTERACTION_TYPE.RESOLVE_CONFLICT);
        advertisedActions.Add(INTERACTION_TYPE.ASK_TO_STOP_JOB);
        advertisedActions.Add(INTERACTION_TYPE.STRANGLE);
        //advertisedActions.Add(INTERACTION_TYPE.SHOCK);
        advertisedActions.Add(INTERACTION_TYPE.CRY);
        //advertisedActions.Add(INTERACTION_TYPE.HAVE_AFFAIR);
        advertisedActions.Add(INTERACTION_TYPE.SLAY_CHARACTER);
        //advertisedActions.Add(INTERACTION_TYPE.FEELING_CONCERNED);
        //advertisedActions.Add(INTERACTION_TYPE.LAUGH_AT);
        advertisedActions.Add(INTERACTION_TYPE.TEASE);
        //advertisedActions.Add(INTERACTION_TYPE.FEELING_SPOOKED);
        //advertisedActions.Add(INTERACTION_TYPE.FEELING_BROKENHEARTED);
        //advertisedActions.Add(INTERACTION_TYPE.GRIEVING);
        advertisedActions.Add(INTERACTION_TYPE.DANCE);
        advertisedActions.Add(INTERACTION_TYPE.SING);
        advertisedActions.Add(INTERACTION_TYPE.GO_TO);
        advertisedActions.Add(INTERACTION_TYPE.SCREAM_FOR_HELP);
        advertisedActions.Add(INTERACTION_TYPE.RESOLVE_COMBAT);
        advertisedActions.Add(INTERACTION_TYPE.RETURN_HOME);
        //advertisedActions.Add(INTERACTION_TYPE.RETURN_HOME_LOCATION);
        advertisedActions.Add(INTERACTION_TYPE.CHAT_CHARACTER);
        //advertisedActions.Add(INTERACTION_TYPE.TRANSFORM_TO_WOLF_FORM);
        //advertisedActions.Add(INTERACTION_TYPE.REVERT_TO_NORMAL_FORM);
        advertisedActions.Add(INTERACTION_TYPE.CHANGE_CLASS);
        advertisedActions.Add(INTERACTION_TYPE.STUDY_MONSTER);
        //advertisedActions.Add(INTERACTION_TYPE.ZOMBIE_DEATH);
        //advertisedActions.Add(INTERACTION_TYPE.STAND);
        //advertisedActions.Add(INTERACTION_TYPE.VISIT);
        //advertisedActions.Add(INTERACTION_TYPE.CRAFT_FURNITURE);

        if (race != RACE.SKELETON) {
            advertisedActions.Add(INTERACTION_TYPE.SHARE_INFORMATION);
            advertisedActions.Add(INTERACTION_TYPE.DRINK_BLOOD);
            advertisedActions.Add(INTERACTION_TYPE.KNOCKOUT_CHARACTER);
            advertisedActions.Add(INTERACTION_TYPE.BUTCHER);
        }
    }
    /// <summary>
    /// This should only be used for plans that come/constructed from the outside.
    /// </summary>
    /// <param name="plan">Plan to be added</param>
    //public void AddPlan(GoapPlan plan, bool isPriority = false, bool processPlanSpecialCases = true) {
    //    if (!allGoapPlans.Contains(plan)) {
    //        plan.SetPriorityState(isPriority);
    //        if (isPriority) {
    //            allGoapPlans.Insert(0, plan);
    //        } else {
    //            bool hasBeenInserted = false;
    //            if (plan.job != null) {
    //                for (int i = 0; i < allGoapPlans.Count; i++) {
    //                    GoapPlan currentPlan = allGoapPlans[i];
    //                    if (currentPlan.isPriority) {
    //                        continue;
    //                    }
    //                    if (currentPlan.job == null || plan.job.priority < currentPlan.job.priority) {
    //                        allGoapPlans.Insert(i, plan);
    //                        hasBeenInserted = true;
    //                        break;
    //                    }
    //                }
    //            }
    //            if (!hasBeenInserted) {
    //                allGoapPlans.Add(plan);
    //            }
    //        }

    //        if (processPlanSpecialCases) {
    //            ////If a character is strolling or idly returning home and a plan is added to this character, end the action/state
    //            //if (stateComponent.currentState != null && (stateComponent.currentState.characterState == CHARACTER_STATE.STROLL
    //            //    || stateComponent.currentState.characterState == CHARACTER_STATE.STROLL_OUTSIDE
    //            //    || stateComponent.currentState.characterState == CHARACTER_STATE.PATROL)) {
    //            //    stateComponent.currentState.OnExitThisState();
    //            //} else if (stateComponent.stateToDo != null && (stateComponent.stateToDo.characterState == CHARACTER_STATE.STROLL
    //            //     || stateComponent.stateToDo.characterState == CHARACTER_STATE.STROLL_OUTSIDE
    //            //     || stateComponent.stateToDo.characterState == CHARACTER_STATE.PATROL)) {
    //            //    stateComponent.SetStateToDo(null);
    //            //} else if (currentAction != null && currentAction.goapType == INTERACTION_TYPE.RETURN_HOME) {
    //            //    if (currentAction.parentPlan == null || currentAction.parentPlan.category == GOAP_CATEGORY.IDLE) {
    //            //        currentAction.StopAction();
    //            //    }
    //            //}

    //            if (plan.job != null && (plan.job.jobType.IsNeedsTypeJob() || plan.job.jobType.IsEmergencyTypeJob())) {
    //                //Unassign Location Job if character decides to rest, eat or have fun.
    //                homeSettlement.jobQueue.UnassignAllJobsTakenBy(this);
    //                faction.activeQuest?.jobQueue.UnassignAllJobsTakenBy(this);
    //            }
    //        }
    //    }
    //}
    //public void RecalculateJob(GoapPlanJob job) {
    //    if(job.assignedPlan != null) {
    //        job.assignedPlan.SetIsBeingRecalculated(true);
    //        MultiThreadPool.Instance.AddToThreadPool(new GoapThread(this, job.assignedPlan, job));
    //    }
    //}
    //public bool IsPOIInCharacterAwarenessList(IPointOfInterest poi, List<IPointOfInterest> awarenesses) {
    //    if (awarenesses != null && awarenesses.Count > 0) {
    //        for (int i = 0; i < awarenesses.Count; i++) {
    //            if (awarenesses[i] == poi) {
    //                return true;
    //            }
    //        }
    //    }
    //    return false;
    //}
    public void PerformTopPriorityJob() {
        string log = $"PERFORMING GOAP PLANS OF {name}";
        if (currentActionNode != null) {
            log +=
                $"\n{name} can't perform another action because he/she is currently performing {currentActionNode.action.goapName}";
            logComponent.PrintLogIfActive(log);
            return;
        }
        
        //List<INTERACTION_TYPE> actorAllowedActions = RaceManager.Instance.GetNPCInteractionsOfCharacter(this);
        //bool willGoIdleState = true;
        if(jobQueue.jobsInQueue[0] is GoapPlanJob) {
            GoapPlanJob currentTopPrioJob = jobQueue.jobsInQueue[0] as GoapPlanJob;
            if(currentTopPrioJob.assignedPlan != null) {
                GoapPlan plan = currentTopPrioJob.assignedPlan;
                ActualGoapNode currentNode = plan.currentActualNode;
                if (RaceManager.Instance.CanCharacterDoGoapAction(this, currentNode.action.goapType)
                && InteractionManager.Instance.CanSatisfyGoapActionRequirements(currentNode.action.goapType, currentNode.actor, currentNode.poiTarget, currentNode.otherData)) {
                    bool preconditionsSatisfied = plan.currentActualNode.action.CanSatisfyAllPreconditions(currentNode.actor, currentNode.poiTarget, currentNode.otherData);
                    if (!preconditionsSatisfied) {
                        log += "\n - Action's preconditions are not all satisfied, trying to recalculate plan...";
                        if (plan.doNotRecalculate) {
                            log += "\n - Action's plan has doNotRecalculate state set to true, dropping plan...";
                            logComponent.PrintLogIfActive(log);
                            currentTopPrioJob.CancelJob(false);
                        } else {
                            logComponent.PrintLogIfActive(log);
                            planner.RecalculateJob(currentTopPrioJob);
                        }
                    } else {
                        //Do not perform action if the target character is still in another character's party, this means that he/she is probably being abducted
                        //Wait for the character to be in its own party before doing the action
                        if (currentNode.poiTarget.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
                            Character targetCharacter = currentNode.poiTarget as Character;
                            if (!targetCharacter.IsInOwnParty() && targetCharacter.currentParty != ownParty) {
                                log += $"\n - {targetCharacter.name} is not in its own party, waiting and skipping...";
                                logComponent.PrintLogIfActive(log);
                                return;
                            }
                        }
                        if (currentNode.poiTarget != this && currentNode.isStealth) {
                            //When performing a stealth job action to a character check if that character is already in vision range, if it is, check if the character doesn't have anyone other than this character in vision, if it is, skip it
                            if (marker.inVisionPOIs.Contains(currentNode.poiTarget) && !marker.CanDoStealthActionToTarget(currentNode.poiTarget)) {
                                log += "\n - Action is stealth and character cannot do stealth action right now...";
                                logComponent.PrintLogIfActive(log);
                                return;
                            }
                        }
                        if(traitContainer.HasTrait("Lazy")) {
                            log += "\n - Character is lazy, has 20% chance to not perform job if it is a needs type job...";
                            if (currentTopPrioJob.jobType.IsNeedsTypeJob()) {
                                int chance = UnityEngine.Random.Range(0, 100);
                                log += $"\n - Roll: {chance}";
                                if (chance < 20) {
                                    Lazy lazy = traitContainer.GetNormalTrait<Lazy>("Lazy");
                                    if (lazy.TriggerLazy()) {
                                        log += "\n - Character triggered lazy, not going to do job";
                                        logComponent.PrintLogIfActive(log);
                                        return;
                                    } else {
                                        log += "\n - Character did not trigger lazy, continue to do action";
                                    }
                                }
                            } else {
                                log += "\n - Job is not a needs type job, continue to do job";
                            }
                        }
                        log += "\n - Action's preconditions are all satisfied, doing action...";
                        logComponent.PrintLogIfActive(log);
                        Messenger.Broadcast(Signals.CHARACTER_WILL_DO_PLAN, this, plan);
                        currentNode.DoAction(currentTopPrioJob, plan);
                    }
                } else {
                    log += "\n - Action did not meet current requirements and allowed actions, dropping plan...";
                    logComponent.PrintLogIfActive(log);
                    currentTopPrioJob.CancelJob(false);
                }
            }
            
        }

        //for (int i = 0; i < allGoapPlans.Count; i++) {
        //    GoapPlan plan = allGoapPlans[i];
        //    if (plan.currentNode == null) {
        //        throw new Exception(this.name + "'s current node in plan is null! Plan is: " + plan.name + "\nCall stack: " + plan.setPlanStateCallStack + "\n");
        //    }
        //    log += "\n" + plan.currentNode.action.goapName;
        //    if (plan.isBeingRecalculated) {
        //        log += "\n - Plan is currently being recalculated, skipping...";
        //        continue; //skip plan
        //    }
        //    if (RaceManager.Instance.CanCharacterDoGoapAction(this, plan.currentNode.action.goapType) 
        //        && InteractionManager.Instance.CanSatisfyGoapActionRequirements(plan.currentNode.action.goapType, plan.currentNode.action.actor, plan.currentNode.action.poiTarget, plan.currentNode.action.otherData)) {
        //        //if (plan.isBeingRecalculated) {
        //        //    log += "\n - Plan is currently being recalculated, skipping...";
        //        //    continue; //skip plan
        //        //}
        //        //if (IsPlanCancelledDueToInjury(plan.currentNode.action)) {
        //        //    log += "\n - Action's plan is cancelled due to injury, dropping plan...";
        //        //    PrintLogIfActive(log);
        //        //    if (allGoapPlans.Count == 1) {
        //        //        DropPlan(plan, true);
        //        //        willGoIdleState = false;
        //        //        break;
        //        //    } else {
        //        //        DropPlan(plan, true);
        //        //        i--;
        //        //        continue;
        //        //    }
        //        //}
        //        if (plan.currentNode.action.IsHalted()) {
        //            log += "\n - Action " + plan.currentNode.action.goapName + " is waiting, skipping...";
        //            continue;
        //        }
        //        bool preconditionsSatisfied = plan.currentNode.action.CanSatisfyAllPreconditions();
        //        if (!preconditionsSatisfied) {
        //            log += "\n - Action's preconditions are not all satisfied, trying to recalculate plan...";
        //            if (plan.doNotRecalculate) {
        //                log += "\n - Action's plan has doNotRecalculate state set to true, dropping plan...";
        //                PrintLogIfActive(log);
        //                if (allGoapPlans.Count == 1) {
        //                    DropPlan(plan);
        //                    willGoIdleState = false;
        //                    break;
        //                } else {
        //                    DropPlan(plan);
        //                    i--;
        //                }
        //            } else {
        //                PrintLogIfActive(log);
        //                planner.RecalculateJob(plan);
        //                willGoIdleState = false;
        //            }
        //        } else {
        //            //Do not perform action if the target character is still in another character's party, this means that he/she is probably being abducted
        //            //Wait for the character to be in its own party before doing the action
        //            if (plan.currentNode.action.poiTarget.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
        //                Character targetCharacter = plan.currentNode.action.poiTarget as Character;
        //                if (!targetCharacter.IsInOwnParty() && targetCharacter.currentParty != _ownParty) {
        //                    log += "\n - " + targetCharacter.name + " is not in its own party, waiting and skipping...";
        //                    PrintLogIfActive(log);
        //                    continue;
        //                }
        //            }
        //            if(plan.currentNode.action.poiTarget != this && plan.currentNode.action.isStealth) {
        //                //When performing a stealth job action to a character check if that character is already in vision range, if it is, check if the character doesn't have anyone other than this character in vision, if it is, skip it
        //                if (marker.inVisionPOIs.Contains(plan.currentNode.action.poiTarget) && !marker.CanDoStealthActionToTarget(plan.currentNode.action.poiTarget)) {
        //                    log += "\n - Action is stealth and character cannot do stealth action right now...";
        //                    continue;
        //                }
        //            }
        //            log += "\n - Action's preconditions are all satisfied, doing action...";
        //            PrintLogIfActive(log);
        //            Messenger.Broadcast(Signals.CHARACTER_WILL_DO_PLAN, this, plan);
        //            //if (plan.currentNode.parent != null && plan.currentNode.parent.action.CanSatisfyAllPreconditions() && plan.currentNode.parent.action.CanSatisfyRequirements()) {
        //            //    log += "\n - All Preconditions of next action in plan already met, skipping action: " + plan.currentNode.action.goapName;
        //            //    //set next node to parent node instead
        //            //    plan.SetNextNode();
        //            //    log += "\n - Next action is: " + plan.currentNode.action.goapName;
        //            //}
        //            plan.currentNode.action.DoAction();
        //            willGoIdleState = false;
        //            break;
        //        }
        //    } else {
        //        log += "\n - Action did not meet current requirements and allowed actions, dropping plan...";
        //        PrintLogIfActive(log);
        //        if (allGoapPlans.Count == 1) {
        //            DropPlan(plan);
        //            willGoIdleState = false;
        //            break;
        //        } else {
        //            DropPlan(plan);
        //            i--;
        //        }
        //    }
        //}
        //if (willGoIdleState) {
        //    log += "\n - Character will go into idle state";
        //    PrintLogIfActive(log);
        //    PerStartTickActionPlanning();
        //}
    }
    public void PerformGoapAction() {
        string log = string.Empty;
        if (currentActionNode == null) {
            log = $"{name} cannot PerformGoapAction because there is no current action!";
            logComponent.PrintLogIfActive(log);
            //Debug.LogError(log);
            //if (!DropPlan(plan)) {
            //    //PlanGoapActions();
            //}
            //StartDailyGoapPlanGeneration();
            return;
        }
        log = $"{name} is performing goap action: {currentActionNode.action.goapName}";
        FaceTarget(currentActionNode.poiTarget);
        bool willStillContinueAction = true;
        OnStartPerformGoapAction(currentActionNode, ref willStillContinueAction);
        if (!willStillContinueAction) {
            return;
        }
        if (InteractionManager.Instance.CanSatisfyGoapActionRequirements(currentActionNode.action.goapType, currentActionNode.actor, currentActionNode.poiTarget, currentActionNode.otherData)
            && currentActionNode.action.CanSatisfyAllPreconditions(currentActionNode.actor, currentActionNode.poiTarget, currentActionNode.otherData)) {
            log +=
                $"\nAction satisfies all requirements and preconditions, proceeding to perform actual action: {currentActionNode.action.goapName} to {currentActionNode.poiTarget.name} at {currentActionNode.poiTarget.gridTileLocation}" ?? "No Tile Location";
            logComponent.PrintLogIfActive(log);
            currentActionNode.PerformAction();
        } else {
            log += "\nAction did not meet all requirements and preconditions. Will try to recalculate plan...";
            GoapPlan plan = currentPlan;
            if (plan.doNotRecalculate) {
                log += "\n - Action's plan has doNotRecalculate state set to true, dropping plan...";
                logComponent.PrintLogIfActive(log);
                currentJob.CancelJob(false);
            } else {
                logComponent.PrintLogIfActive(log);
                UnityEngine.Assertions.Assert.IsNotNull(currentJob);
                UnityEngine.Assertions.Assert.IsTrue(currentJob is GoapPlanJob);
                planner.RecalculateJob(currentJob as GoapPlanJob);
            }
            SetCurrentActionNode(null, null, null);
        }
        //if (currentActionNode.isStopped) {
        //    log += "\n Action is stopped! Dropping plan...";
        //    PrintLogIfActive(log);
        //    SetCurrentActionNode(null);
        //    if (!DropPlan(currentActionNode.parentPlan)) {
        //        //PlanGoapActions();
        //    }
        //} else {
        //    bool willStillContinueAction = true;
        //    OnStartPerformGoapAction(currentActionNode, ref willStillContinueAction);
        //    if (!willStillContinueAction) {
        //        return;
        //    }
        //    if (currentActionNode.IsHalted()) {
        //        log += "\n Action is waiting! Not doing action...";
        //        //if (currentAction.poiTarget.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
        //        //    Character targetCharacter = currentAction.poiTarget as Character;
        //        //    targetCharacter.AdjustIsWaitingForInteraction(-1);
        //        //}
        //        SetCurrentActionNode(null);
        //        return;
        //    }
        //    if (InteractionManager.Instance.CanSatisfyGoapActionRequirements(currentActionNode.goapType, currentActionNode.actor, currentActionNode.poiTarget, currentActionNode.otherData) 
        //        && currentActionNode.CanSatisfyAllPreconditions()) {
        //        //if (currentAction.poiTarget != this && currentAction.isStealth) {
        //        //    //When performing a stealth job action to a character check if that character is already in vision range, if it is, check if the character doesn't have anyone other than this character in vision, if it is, skip it
        //        //    if (marker.inVisionPOIs.Contains(currentAction.poiTarget) && !marker.CanDoStealthActionToTarget(currentAction.poiTarget)) {
        //        //        log += "\n - Action is stealth and character cannot do stealth action right now...";
        //        //        PrintLogIfActive(log);
        //        //        return;
        //        //    }
        //        //}
        //        log += "\nAction satisfies all requirements and preconditions, proceeding to perform actual action: " + currentActionNode.goapName + " to " + currentActionNode.poiTarget.name + " at " + currentActionNode.poiTarget.gridTileLocation?.ToString() ?? "No Tile Location";
        //        PrintLogIfActive(log);
        //        currentActionNode.Perform();
        //    } else {
        //        log += "\nAction did not meet all requirements and preconditions. Will try to recalculate plan...";
        //        //if (currentAction.poiTarget.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
        //        //    Character targetCharacter = currentAction.poiTarget as Character;
        //        //    targetCharacter.AdjustIsWaitingForInteraction(-1);
        //        //}
        //        GoapPlan plan = currentActionNode.parentPlan;
        //        SetCurrentActionNode(null);
        //        if (plan.doNotRecalculate) {
        //            log += "\n - Action's plan has doNotRecalculate state set to true, dropping plan...";
        //            PrintLogIfActive(log);
        //            if (!DropPlan(plan)) {
        //                //PlanGoapActions();
        //            }
        //        } else {
        //            PrintLogIfActive(log);
        //            planner.RecalculateJob(plan);
        //            //IdlePlans();
        //        }
        //    }
        //}
    }
    public void GoapActionResult(string result, ActualGoapNode actionNode) {
        string log = $"{name} is done performing goap action: {actionNode.action.goapName}";
        //Debug.Log(log);
        GoapPlan plan = currentPlan;
        GoapPlanJob job = currentJob as GoapPlanJob;

        if (actionNode == currentActionNode) {
            SetCurrentActionNode(null, null, null);
        }

        if (isDead || !canPerform) {
            log += $"\n{name} is dead or cannot perform! Do not do GoapActionResult, automatically CancelJob";
            logComponent.PrintLogIfActive(log);
            job.CancelJob(false);
            return;
        }

        if(result == InteractionManager.Goap_State_Success) {
            log += "\nPlan is setting next action to be done...";
            Messenger.Broadcast(Signals.CHARACTER_DID_ACTION_SUCCESSFULLY, this, actionNode);
            plan.SetNextNode();
            if (plan.currentNode == null) {
                log += "\nThis action is the end of plan.";
                //if (job.originalOwner.ownerType != JOB_OWNER.CHARACTER && traitContainer.GetNormalTrait<Trait>("Hardworking") != null) {
                //    log += "\nFinished a settlement job and character is hardworking, increase happiness by 3000...";
                //    needsComponent.AdjustHappiness(3000); //TODO: Move this to hardworking trait.
                //}
                logComponent.PrintLogIfActive(log);
                //bool forceRemoveJobInQueue = true;
                ////If an action is stopped as current action (meaning it was cancelled) and it is a settlement/faction job, do not remove it from the queue
                //if (actionNode.isStoppedAsCurrentAction && plan != null && plan.job != null && plan.job.jobQueueParent.isAreaOrQuestJobQueue) {
                //    forceRemoveJobInQueue = false;
                //}
                Messenger.Broadcast(Signals.CHARACTER_FINISHED_JOB_SUCCESSFULLY, this, job);
                
                //this means that this is the end goal so end this plan now
                job.ForceCancelJob(false);
            } else {
                log += $"\nNext action for this plan: {plan.currentActualNode.goapName}";
                //if (plan.job != null && plan.job.assignedCharacter != this) {
                //    log += "\nPlan has a job: " + plan.job.name + ". Assigned character " + (plan.job.assignedCharacter != null ? plan.job.assignedCharacter.name : "None") + " does not match with " + name + ".";
                //    log += "Drop plan because this character is no longer the one assigned";
                //    DropPlan(plan);
                //}
                logComponent.PrintLogIfActive(log);
                //PlanGoapActions();
            }
        }
        //Reason: https://trello.com/c/58aGENsO/1867-attempt-to-find-another-nearby-chair-first-instead-of-dropping-drink-eat-sit-down-actions
        else if (result == InteractionManager.Goap_State_Fail) {
            if (plan.doNotRecalculate) {
                job.CancelJob(false);
            } else {
                planner.RecalculateJob(job as GoapPlanJob);
            }
        }
    }
    //public bool DropPlan(GoapPlan plan, bool forceRemoveJob = false, bool forceProcessPlanJob = false) {
    //    bool hasBeenRemoved = false;
    //    if (allGoapPlans.Remove(plan)) {
    //        Debug.Log(GameManager.Instance.TodayLogString() + plan.name + " was removed from " + this.name + "'s plan list");
    //        plan.EndPlan();
    //        hasBeenRemoved = true;
    //    }
    //    if(hasBeenRemoved || forceProcessPlanJob) {
    //        if (plan.job != null) {
    //            if (plan.job.cancelJobOnFail || plan.job.cancelJobOnDropPlan || forceRemoveJob) {
    //                plan.job.jobQueueParent.RemoveJobInQueue(plan.job);
    //            }
    //            plan.job.SetAssignedCharacter(null);
    //            plan.job.SetAssignedPlan(null);
    //        }
    //    }
    //    return hasBeenRemoved;
    //}
    //public bool JustDropPlan(GoapPlan plan, bool forceRemoveJob = false, bool forceProcessPlanJob = false) {
    //    bool hasBeenRemoved = false;
    //    if (allGoapPlans.Remove(plan)) {
    //        Debug.Log(GameManager.Instance.TodayLogString() + plan.name + " was removed from " + this.name + "'s plan list");
    //        plan.EndPlan();
    //        hasBeenRemoved = true;
    //    }
    //    if (hasBeenRemoved || forceProcessPlanJob) {
    //        if (plan.job != null) {
    //            if (plan.job.cancelJobOnDropPlan || forceRemoveJob) {
    //                plan.job.jobQueueParent.RemoveJobInQueue(plan.job);
    //            }
    //            plan.job.SetAssignedCharacter(null);
    //            plan.job.SetAssignedPlan(null);
    //        }
    //    }
    //    return hasBeenRemoved;
    //}
    //public void DropAllPlans(GoapPlan planException = null) {
    //    if (planException == null) {
    //        while (allGoapPlans.Count > 0) {
    //            DropPlan(allGoapPlans[0]);
    //        }
    //    } else {
    //        for (int i = 0; i < allGoapPlans.Count; i++) {
    //            if (allGoapPlans[i] != planException) {
    //                DropPlan(allGoapPlans[i]);
    //                i--;
    //            }
    //        }
    //    }
    //}
    //public void JustDropAllPlansOfType(INTERACTION_TYPE type) {
    //    for (int i = 0; i < allGoapPlans.Count; i++) {
    //        GoapPlan currPlan = allGoapPlans[i];
    //        if (currPlan.endNode.action != null && currPlan.endNode.action.goapType == type) {
    //            if (JustDropPlan(currPlan)) {
    //                i--;
    //            }
    //        }
    //    }
    //}
    //public void DropAllPlansOfType(INTERACTION_TYPE type) {
    //    for (int i = 0; i < allGoapPlans.Count; i++) {
    //        GoapPlan currPlan = allGoapPlans[i];
    //        if (currPlan.endNode.action != null && currPlan.endNode.action.goapType == type) {
    //            if (DropPlan(currPlan)) {
    //                i--;
    //            }
    //        }
    //    }
    //}
    //public bool HasPlanWithType(INTERACTION_TYPE type) {
    //    for (int i = 0; i < allGoapPlans.Count; i++) {
    //        GoapPlan currPlan = allGoapPlans[i];
    //        if (currPlan.endNode.action != null && currPlan.endNode.action.goapType == type) {
    //            return true;
    //        }
    //    }
    //    return false;
    //}
    //public GoapPlan GetPlanWithGoalEffect(GOAP_EFFECT_CONDITION goalEffect) {
    //    for (int i = 0; i < allGoapPlans.Count; i++) {
    //        if (allGoapPlans[i].goalEffects.Contains(goalEffect)) {
    //            return allGoapPlans[i];
    //        }
    //    }
    //    return null;
    //}
    //public GoapPlan GetPlanByCategory(GOAP_CATEGORY category) {
    //    for (int i = 0; i < allGoapPlans.Count; i++) {
    //        if (allGoapPlans[i].category == category) {
    //            return allGoapPlans[i];
    //        }
    //    }
    //    return null;
    //}
    //For testing: Drop Character
    //public void DropACharacter() {
    //    if (awareness.ContainsKey(POINT_OF_INTEREST_TYPE.CHARACTER)) {
    //        List<IPointOfInterest> characterAwarenesses = awareness[POINT_OF_INTEREST_TYPE.CHARACTER];
    //        Character randomTarget = characterAwarenesses[UnityEngine.Random.Range(0, characterAwarenesses.Count)] as Character;
    //        GoapEffect goapEffect = new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_FROM_PARTY, conditionKey = homeRegion, targetPOI = randomTarget };
    //        StartGOAP(goapEffect, randomTarget, GOAP_CATEGORY.REACTION);
    //    }
    //}
    //public GoapPlan GetPlanWithAction(GoapAction action) {
    //    for (int i = 0; i < allGoapPlans.Count; i++) {
    //        for (int j = 0; j < allGoapPlans[i].allNodes.Count; j++) {
    //            if (allGoapPlans[i].allNodes[j].actionType == action) {
    //                return allGoapPlans[i];
    //            }
    //        }
    //    }
    //    return null;
    //}
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
    public void SetCurrentActionNode(ActualGoapNode actionNode, JobQueueItem job, GoapPlan plan) {
        if (currentActionNode != null) {
            previousCurrentActionNode = currentActionNode;
        }
        currentActionNode = actionNode;
        if (currentActionNode != null) {
            logComponent.PrintLogIfActive(
                $"{this.name} will do action {actionNode.action.goapType} to {actionNode.poiTarget}");
            if (currentActionNode.action.goapType.IsHostileAction()) { //if the character will do a combat action, remove all ignore hostilities value
                ClearIgnoreHostilities();
            }
            //stateComponent.SetStateToDo(null, stopMovement: false);

            //Current Job must always be the job in the top prio, if there is inconsistency with the currentActionNode, then the problem lies on what you set as the currentActionNode
        }
        SetCurrentJob(job);
        SetCurrentPlan(plan);
        
        string summary = $"{GameManager.Instance.TodayLogString()}Set current action to ";
        if (currentActionNode == null) {
            summary += "null";
        } else {
            summary += $"{currentActionNode.action.goapName} targetting {currentActionNode.poiTarget.name}";
        }
        //summary += "\n StackTrace: " + StackTraceUtility.ExtractStackTrace();

        actionHistory.Add(summary);
        if (actionHistory.Count > 10) {
            actionHistory.RemoveAt(0);
        }
    }
    public void SetCurrentPlan(GoapPlan plan) {
        currentPlan = plan;
    }
    //Only stop an action node if it is the current action node
    ///Stopping action node does not mean that the job will be cancelled, if you want to cancel job at the same time call <see cref="StopCurrentActionNodeAndCancelItsJob">
    public bool StopCurrentActionNode(bool shouldDoAfterEffect = false, string reason = "") {
        if(currentActionNode == null) {
            return false;
        }
        bool shouldLogReason = true;
        if (reason != "" && currentActionNode.poiTarget != this/* && currentJob.jobType != JOB_TYPE.WATCH*/) {
            //if(currentActionNode.poiTarget is Character) {
            //    Trait targetDeadTrait = currentActionNode.poiTarget.traitContainer.GetNormalTrait<Trait>("Dead");
            //    if(targetDeadTrait.gainedFromDoing == currentActionNode) {
            //        shouldLogReason = false;
            //    }
            //}
        } else {
            shouldLogReason = false;
        }
        if (shouldLogReason) {
            Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "current_action_abandoned_reason");
            log.AddToFillers(this, name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddToFillers(null, currentActionNode.action.goapName, LOG_IDENTIFIER.STRING_1);
            log.AddToFillers(null, reason, LOG_IDENTIFIER.STRING_2);
            logComponent.RegisterLog(log, onlyClickedCharacter: false);
        }
        //if (actor.currentAction != null && actor.currentAction.parentPlan != null && actor.currentAction.parentPlan.job != null && actor.currentAction == this) {
        //    if (reason != "") {
        //        Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "current_action_abandoned_reason");
        //        log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        //        log.AddToFillers(null, actor.currentAction.goapName, LOG_IDENTIFIER.STRING_1);
        //        log.AddToFillers(null, reason, LOG_IDENTIFIER.STRING_2);
        //        actor.RegisterLogAndShowNotifToThisCharacterOnly(log, onlyClickedCharacter: false);
        //    }
        //}
        if (currentParty.icon.isTravelling) {
            if (ReferenceEquals(currentParty.icon.travelLine, null)) {
                //This means that the actor currently travelling to another tile in tilemap
                marker.StopMovement();
            } else {
                //This means that the actor is currently travelling to another settlement
                currentParty.icon.SetOnArriveAction(() => OnArriveAtAreaStopMovement());
            }
        }
        //if (poiTarget.poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
        //    Messenger.RemoveListener<TileObject, Character, LocationGridTile>(Signals.TILE_OBJECT_REMOVED, OnTileObjectRemoved);
        //    Messenger.RemoveListener<TileObject, Character>(Signals.TILE_OBJECT_DISABLED, OnTileObjectDisabled);
        //}

        //SetIsStopped(true);
        currentActionNode.StopActionNode(shouldDoAfterEffect);
        SetCurrentActionNode(null, null, null);
        
        //Every time current node is stopped, drop carried poi
        if (IsInOwnParty()) {
            if (ownParty.isCarryingAnyPOI) {
                IPointOfInterest carriedPOI = ownParty.carriedPOI;
                string log = $"Dropping carried POI: {carriedPOI.name} because current action is stopped!";
                log += "\nAdditional Info:";
                if (carriedPOI is ResourcePile) {
                    ResourcePile pile = carriedPOI as ResourcePile;
                    log += $"\n-Stored resources on drop: {pile.resourceInPile} {pile.providedResource}";
                } else if (carriedPOI is Table) {
                    Table table = carriedPOI as Table;
                    log += $"\n-Stored resources on drop: {table.food} Food.";
                }

                logComponent.PrintLogIfActive(log);
                UncarryPOI();
            }
        }
        //JobQueueItem job = parentPlan.job;

        //Remove job in queue if job is personal job and removeJobInQueue value is true
        //if (removeJobInQueue && job != null && !job.jobQueueParent.isAreaOrQuestJobQueue) {
        //    job.jobQueueParent.RemoveJobInQueue(job);
        //}
        if (UIManager.Instance.characterInfoUI.isShowing) {
            UIManager.Instance.characterInfoUI.UpdateBasicInfo();
        }
        //Messenger.Broadcast<GoapAction>(Signals.STOP_ACTION, this);
        logComponent.PrintLogIfActive(
            $"Stopped action of {name} which is {previousCurrentActionNode.action.goapName} targetting {previousCurrentActionNode.poiTarget.name}!");
        return true;
    }
    //public void SetHasAlreadyAskedForPlan(bool state) {
    //    _hasAlreadyAskedForPlan = state;
    //}
    //public void AddTargettedByAction(GoapAction action) {
    //    if (this != action.actor) { // && !isDead
    //        targettedByAction.Add(action);
    //        if (marker != null) {
    //            marker.OnCharacterTargettedByAction(action);
    //        }
    //    }
    //}
    //public void RemoveTargettedByAction(GoapAction action) {
    //    if (targettedByAction.Remove(action)) {
    //        if (marker != null) {
    //            marker.OnCharacterRemovedTargettedByAction(action);
    //        }
    //    }
    //}
    //This will only stop the current action of this character, this is different from StopAction because this will not drop the plan if the actor is not performing it but is on the way
    //This does not stop the movement of this character, call StopMovement separately to stop movement
    //public void StopCurrentAction(bool shouldDoAfterEffect = true, string reason = "") {
    //    if(currentActionNode != null) {
    //        //Debug.Log("Stopped action of " + name + " which is " + currentAction.goapName);
    //        if (currentActionNode.parentPlan != null && currentActionNode.parentPlan.job != null && reason != "") {
    //            Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "current_action_abandoned_reason");
    //            log.AddToFillers(this, name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
    //            log.AddToFillers(null, currentActionNode.goapName, LOG_IDENTIFIER.STRING_1);
    //            log.AddToFillers(null, reason, LOG_IDENTIFIER.STRING_2);
    //            RegisterLogAndShowNotifToThisCharacterOnly(log, onlyClickedCharacter: false);
    //        }
    //        if (currentActionNode.isPerformingActualAction && !currentActionNode.isDone) {
    //            if (!shouldDoAfterEffect) {
    //                currentActionNode.OnStopActionDuringCurrentState();
    //            }
    //            if(currentActionNode.currentState != null) {
    //                currentActionNode.SetIsStoppedAsCurrentAction(true);
    //                currentActionNode.currentState.EndPerTickEffect(shouldDoAfterEffect);
    //            } else {
    //                SetCurrentActionNode(null);
    //            }
    //        } else {
    //            currentActionNode.OnStopActionWhileTravelling();
    //            SetCurrentActionNode(null);
    //        }
    //    }
    //}
    public void OnStartPerformGoapAction(ActualGoapNode node, ref bool willStillContinueAction) {
        bool stillContinueCurrentAction = true;
        for (int i = 0; i < traitContainer.allTraits.Count; i++) {
            Trait trait = traitContainer.allTraits[i];
            if (trait.OnStartPerformGoapAction(node, ref stillContinueCurrentAction)) {
                willStillContinueAction = stillContinueCurrentAction;
                break;
            } else {
                stillContinueCurrentAction = true;
            }
        }
    }
    private void HeardAScream(Character characterThatScreamed) {
        if(!canPerform || !canWitness) {
            //Do not react to scream if character has disabler trait
            return;
        }
        if(currentRegion != characterThatScreamed.currentRegion) {
            return;
        }
        if(gridTileLocation != null && characterThatScreamed.gridTileLocation != null) {
            float dist = gridTileLocation.GetDistanceTo(characterThatScreamed.gridTileLocation);
            logComponent.PrintLogIfActive($"{name} distance to {characterThatScreamed.name} is {dist}");
            float distanceChecker = 10f;
            //if (currentStructure != characterThatScreamed.currentStructure) {
            //    distanceChecker = 2f;
            //}
            if (dist > distanceChecker) {
                //Do not react to scream if character is too far
                return;
            }
        }
        if(jobQueue.HasJob(JOB_TYPE.GO_TO, characterThatScreamed)) {
            //Do not react if character will already react to a scream;
            return;
        }
        if (!CanCharacterReact(characterThatScreamed)) {
            return;
        }
        ReactToScream(characterThatScreamed);
    }
    private void ReactToScream(Character characterThatScreamed) {
        //If you are wondering why the job is not just simply added to the job queue and let the job queue processing work if the job can override current job, is because
        //in this situation, we want the job not to be added to the job queue if it cannot override the current job
        //If we let the AddJobInQueue simply process the job, it will still be added regardless if it cannot override the current job, it means that it will just be pushed back in queue and will be done by the character when the time comes
        //We don't want that because we want to have a spontaneous reaction from this character, so the only way that the character will react is if he can do it immediately

        string log = $"{name} heard the scream of {characterThatScreamed.name}, reacting...";

        bool canReact = true;
        int reactJobPriority = JOB_TYPE.GO_TO.GetJobTypePriority();
        if (stateComponent.currentState != null && stateComponent.currentState.job != null && stateComponent.currentState.job.priority >= reactJobPriority) {
            canReact = false;
        } 
        //else if (stateComponent.stateToDo != null && stateComponent.stateToDo.job != null && stateComponent.stateToDo.job.priority <= reactJobPriority) {
        //    canReact = false;
        //} 
        else if (currentJob != null && currentJob.priority >= reactJobPriority) {
            canReact = false;
        }
        if (canReact) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.GO_TO, INTERACTION_TYPE.GO_TO, characterThatScreamed, this);
            jobQueue.AddJobInQueue(job);
            //if (CanCurrentJobBeOverriddenByJob(job)) {
            //    jobQueue.AddJobInQueue(job, false);
            //    jobQueue.CurrentTopPriorityIsPushedBackBy(job, this);
            //    log += "\n" + name + " will go to " + characterThatScreamed.name;
            //} else {
            //    log += "\n" + name + " cannot react because there is still something important that he/she will do.";
            //}
        } else {
            log += $"\n{name} cannot react because there is still something important that he/she will do.";
        }

        logComponent.PrintLogIfActive(log);
    }

    //Can only be seized if poi has tile location
    public void OnSeizePOI() {
        if (UIManager.Instance.characterInfoUI.isShowing && UIManager.Instance.characterInfoUI.activeCharacter == this) {
            UIManager.Instance.characterInfoUI.CloseMenu();
        }
        if (ownParty.icon.isTravelling) {
            marker.StopMovement();
        }
        if (trapStructure.structure != null) {
            trapStructure.SetStructureAndDuration(null, 0);
        }
        minion?.OnSeize();
        Messenger.Broadcast(Signals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI, this as IPointOfInterest, "");
        //ForceCancelAllJobsTargettingThisCharacter();
        //marker.ClearTerrifyingObjects();
        needsComponent.OnCharacterLeftLocation(currentRegion);

        CancelAllJobs();
        UnsubscribeSignals();
        SetIsConversing(false);
        SetPOIState(POI_STATE.INACTIVE);
        SchedulingManager.Instance.ClearAllSchedulesBy(this);
        if (marker != null) {
            //DestroyMarker();
            //marker.collisionTrigger.SetCollidersState(false);
            marker.OnSeize();
            DisableMarker();
            Messenger.Broadcast(Signals.CHECK_APPLICABILITY_OF_ALL_JOBS_TARGETING, this as IPointOfInterest);
        }
        Messenger.AddListener(Signals.TICK_STARTED, OnTickStartedWhileSeized);
        //Messenger.Broadcast(Signals.ON_SEIZE_CHARACTER, this);
    }
    public void OnUnseizePOI(LocationGridTile tileLocation) {
        Messenger.RemoveListener(Signals.TICK_STARTED, OnTickStartedWhileSeized);
        needsComponent.OnCharacterArrivedAtLocation(tileLocation.structure.location.coreTile.region);
        if (minion == null) {
            SubscribeToSignals();    
        }
        SetPOIState(POI_STATE.ACTIVE);
        if (marker == null) {
            CreateMarker();
        } else {
            marker.SetCharacter(this);
        }
        //marker.SetAllColliderStates(true);
        EnableMarker();
        marker.OnUnseize();
        minion?.OnUnseize();
        if(tileLocation.structure.location.coreTile.region != currentRegion) {
            currentRegion.RemoveCharacterFromLocation(this);
            marker.InitialPlaceMarkerAt(tileLocation);
        } else {
            marker.InitialPlaceMarkerAt(tileLocation, false);
        }
        needsComponent.CheckExtremeNeeds();
        //Messenger.Broadcast(Signals.ON_UNSEIZE_CHARACTER, this);
    }
    #endregion

    #region Resources
    public void ConstructResources() {
        storedResources = new Dictionary<RESOURCE, int>() {
            { RESOURCE.FOOD, 0 },
            { RESOURCE.WOOD, 0 },
            { RESOURCE.STONE, 0 },
            { RESOURCE.METAL, 0 },
        };
    }
    public void SetResource(RESOURCE resourceType, int amount) {
        int currentAmount = storedResources[resourceType];
        storedResources[resourceType] = amount;
        storedResources[resourceType] = Mathf.Max(0, currentAmount);
    }
    public void AdjustResource(RESOURCE resourceType, int amount) {
        int currentAmount = storedResources[resourceType];
        storedResources[resourceType] += amount;
        storedResources[resourceType] = Mathf.Max(0, currentAmount);
    }
    public bool HasResourceAmount(RESOURCE resourceType, int amount) {
        return storedResources[resourceType] >= amount;
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

    #region Food
    public void AdjustFood(int amount) {
        food += amount;
        if (food < 0) {
            food = 0;
        }
    }
    public void SetFood(int amount) {
        food = amount;
        if (food < 0) {
            food = 0;
        }
    }
    #endregion

    #region Hostility
    /// <summary>
    /// Function to encapsulate, whether or not this character treats another character as hostile.
    /// </summary>
    /// <param name="character">Character in question.</param>
    public bool IsHostileWith(Character character, bool checkIgnoreHostility = true) {
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

        if (checkIgnoreHostility && (character.ignoreHostility > 0 || this.ignoreHostility > 0)) {
            //if either the character in question or this character should ignore hostility, return false.
            return false;
        }
        if (isFactionless || character.isFactionless) {
            //this character is unaligned
            //if unaligned, hostile to all other characters, except those of same race
            return character.race != this.race;
        } else {
            //this character has a faction
            //if has a faction, is hostile to characters of every other faction
            //return this.faction.id != character.faction.id;
            
            return faction.IsHostileWith(character.faction);
            // if(faction == character.faction) {
            //     return false;
            // }
            // return faction.GetRelationshipWith(character.faction).relationshipStatus == FACTION_RELATIONSHIP_STATUS.HOSTILE;
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
        //Debug.Log(GameManager.Instance.TodayLogString() + "Adjusted " + name + "'s ignore hostilities by " + amount + ". Ignore hostiles value is " + ignoreHostility.ToString());
    }
    public void SetIgnoreHostilities(int amount) {
        ignoreHostility = amount;
        ignoreHostility = Mathf.Max(0, ignoreHostility);
        //Debug.Log(GameManager.Instance.TodayLogString() + "Adjusted " + name + "'s ignore hostilities by " + amount + ". Ignore hostiles value is " + ignoreHostility.ToString());
    }
    public void ClearIgnoreHostilities() {
        ignoreHostility = 0;
        //Debug.Log(GameManager.Instance.TodayLogString() + name + " clreared ignore hostiles.");
    }
    /// <summary>
    /// Is the other character an outsider. (Not part of this character's faction)
    /// </summary>
    /// <param name="otherCharacter"></param>
    /// <returns></returns>
    public bool IsHostileOutsider(Character otherCharacter) {
        //return true;
        if (otherCharacter.isDead || this.isDead) {
            return false;
        }
        return this.faction.id != otherCharacter.faction.id;
    }
    #endregion

    #region Crime System
    /// <summary>
    /// Make this character react to a crime that he/she witnessed.
    /// </summary>
    /// <param name="witnessedCrime">Witnessed Crime.</param>
    //public void ReactToCrime(GoapAction witnessedCrime, ref bool hasRelationshipDegraded) {
    //    //TODO: Crime System Handling!
    //    //ReactToCrime(witnessedCrime.committedCrime, witnessedCrime, witnessedCrime.actorAlterEgo, ref hasRelationshipDegraded, witnessedCrime);
    //    //witnessedCrime.OnWitnessedBy(this);
    //}
    /// <summary>
    /// A variation of react to crime in which the parameter SHARE_INTEL_STATUS will be the one to determine if it is informed or witnessed crime
    /// Returns true or false, if the relationship between the reactor and the criminal has degraded
    /// </summary>
    //public bool ReactToCrime(CRIME committedCrime, ActualGoapNode crimeAction, Character criminal, SHARE_INTEL_STATUS status) {
    //    bool hasRelationshipDegraded = false;
    //    if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //        ReactToCrime(committedCrime, crimeAction, criminal, ref hasRelationshipDegraded, crimeAction, null);
    //    }else if (status == SHARE_INTEL_STATUS.INFORMED) {
    //        ReactToCrime(committedCrime, crimeAction, criminal, ref hasRelationshipDegraded, null, crimeAction);
    //    } else {
    //        logComponent.PrintLogErrorIfActive("The share intel status is neither INFORMED or WITNESSED");
    //    }
    //    return hasRelationshipDegraded;
    //}
    /// <summary>
    /// Base function for crime reactions
    /// </summary>
    /// <param name="committedCrime">The type of crime that was committed.</param>
    /// <param name="criminal">The character that committed the crime</param>
    /// <param name="witnessedCrime">The crime witnessed by this character, if this is null, character was only informed of the crime by someone else.</param>
    /// <param name="informedCrime">The crime this character was informed of. NOTE: Should only have value if Share Intel</param>
    //public void ReactToCrime(CRIME committedCrime, ActualGoapNode crimeAction, Character criminal, ref bool hasRelationshipDegraded, ActualGoapNode witnessedCrime = null, ActualGoapNode informedCrime = null) {
    //    //NOTE: Moved this to be per action specific. See GoapAction.IsConsideredACrimeBy and GoapAction.CanReactToThisCrime for necessary mechanics.
    //    //if (witnessedCrime != null) {
    //    //    //if the action that should be considered a crime is part of a job from this character's settlement, do not consider it a crime
    //    //    if (witnessedCrime.parentPlan.job != null
    //    //        && homeSettlement.jobQueue.jobsInQueue.Contains(witnessedCrime.parentPlan.job)) {
    //    //        return;
    //    //    }
    //    //    //if the witnessed crime is targetting this character, this character should not react to the crime if the crime's doesNotStopTargetCharacter is true
    //    //    if (witnessedCrime.poiTarget == this && witnessedCrime.doesNotStopTargetCharacter) {
    //    //        return;
    //    //    }
    //    //}
        
    //    string reactSummary = this.name + " will react to crime committed by " + criminal.name;
    //    if(committedCrime == CRIME.NONE) {
    //        reactSummary += "\nNo reaction because committed crime is " + committedCrime.ToString();
    //        logComponent.PrintLogIfActive(reactSummary);
    //        return;
    //    }
    //    //Log witnessLog = null;
    //    //Log reportLog = null;
    //    RELATIONSHIP_EFFECT relationshipEfffectWithCriminal = opinionComponent.GetRelationshipEffectWith(criminal);
    //    CRIME_TYPE category = committedCrime.GetCategory();

    //    //If character witnessed an Infraction crime:
    //    if (category == CRIME_TYPE.INFRACTION) {
    //        //-Witness Log: "[Character Name] saw [Criminal Name] committing [Theft/Assault/Murder]."
    //        //- Report / Share Intel Log: "[Character Name] saw [Criminal Name] committing [Theft/Assault/Murder]."
    //        //- no additional response
    //        reactSummary += "\nCrime committed is infraction.";
    //        //witnessLog = new Log(GameManager.Instance.Today(), "Character", "CrimeSystem", "witnessed");
    //        //reportLog = new Log(GameManager.Instance.Today(), "Character", "CrimeSystem", "witnessed");
    //    }
    //    //If character has a positive relationship (Friend, Lover, Paramour) with the criminal
    //    else if (relationshipEfffectWithCriminal == RELATIONSHIP_EFFECT.POSITIVE) {
    //        reactSummary += "\n" + this.name + " has a positive relationship with " + criminal.name;
    //        //and crime severity is a Misdemeanor:
    //        if (category == CRIME_TYPE.MISDEMEANOR) {
    //            reactSummary += "\nCrime committed is misdemeanor.";
    //            //- Witness Log: "[Character Name] saw [Criminal Name] committing [Theft/Assault/Murder] but did not do anything due to their relationship."
    //            //-Report / Share Intel Log: "[Character Name] was informed that [Criminal Name] committed [Theft/Assault/Murder] but did not do anything due to their relationship."
    //            //witnessLog = new Log(GameManager.Instance.Today(), "Character", "CrimeSystem", "do_nothing");
    //            //reportLog = new Log(GameManager.Instance.Today(), "Character", "CrimeSystem", "report_do_nothing");
    //        }
    //        //and crime severity is Serious Crimes or worse:
    //        else if (category.IsGreaterThanOrEqual(CRIME_TYPE.SERIOUS)) {
    //            reactSummary += "\nCrime committed is serious or worse. Removing positive relationships.";
    //            //- Relationship Degradation between Character and Criminal
    //            hasRelationshipDegraded = RelationshipManager.Instance.RelationshipDegradation(criminal, this, witnessedCrime);
    //            if (hasRelationshipDegraded) {
    //                //witnessLog = new Log(GameManager.Instance.Today(), "Character", "CrimeSystem", "witnessed_degraded");
    //                //reportLog = new Log(GameManager.Instance.Today(), "Character", "CrimeSystem", "report_witnessed_degraded");
    //                PerRoleCrimeReaction(committedCrime, crimeAction, criminal, witnessedCrime, informedCrime);
    //            } else {
    //                if (witnessedCrime != null) {
    //                    if (marker.inVisionCharacters.Contains(criminal)) {
    //                        combatComponent.AddAvoidInRange(criminal);
    //                    }
    //                }
    //                //witnessLog = new Log(GameManager.Instance.Today(), "Character", "CrimeSystem", "do_nothing");
    //                //reportLog = new Log(GameManager.Instance.Today(), "Character", "CrimeSystem", "report_do_nothing");
    //            }

    //        }
    //    }
    //    //If character has no relationships with the criminal or they are enemies and the crime is a Misdemeanor or worse:
    //    else if ((!this.relationshipContainer.HasRelationshipWith(criminal) || this.opinionComponent.IsEnemiesWith(criminal)) 
    //        && category.IsGreaterThanOrEqual(CRIME_TYPE.MISDEMEANOR)) {
    //        reactSummary += "\n" + this.name + " does not have a relationship with or is an enemy of " + criminal.name + " and the committed crime is misdemeanor or worse";
    //        //- Relationship Degradation between Character and Criminal
    //        hasRelationshipDegraded = RelationshipManager.Instance.RelationshipDegradation(criminal, this, witnessedCrime);
    //        //- Witness Log: "[Character Name] saw [Criminal Name] committing [Theft/Assault/Murder]!"
    //        if (hasRelationshipDegraded) {
    //            //witnessLog = new Log(GameManager.Instance.Today(), "Character", "CrimeSystem", "witnessed_degraded");
    //            //reportLog = new Log(GameManager.Instance.Today(), "Character", "CrimeSystem", "report_witnessed_degraded");
    //            PerRoleCrimeReaction(committedCrime, crimeAction, criminal, witnessedCrime, informedCrime);
    //        } else {
    //            if (witnessedCrime != null) {
    //                if (marker.inVisionCharacters.Contains(criminal)) {
    //                    combatComponent.AddAvoidInRange(criminal);
    //                }
    //            }
    //            //witnessLog = new Log(GameManager.Instance.Today(), "Character", "CrimeSystem", "do_nothing");
    //            //reportLog = new Log(GameManager.Instance.Today(), "Character", "CrimeSystem", "report_do_nothing");
    //        }
    //    }

    //    //if (witnessedCrime != null) {
    //    //    if (witnessLog != null) {
    //    //        witnessLog.AddToFillers(this, this.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
    //    //        witnessLog.AddToFillers(criminal.owner, criminal.owner.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    //    //        witnessLog.AddToFillers(null, Utilities.NormalizeStringUpperCaseFirstLetters(committedCrime.ToString()), LOG_IDENTIFIER.STRING_1);
    //    //        if (this != witnessedCrime.poiTarget) {
    //    //            PlayerManager.Instance.player.ShowNotificationFrom(this, witnessLog);
    //    //        }
    //    //    }
    //    //} else {
    //    //    if (reportLog != null) {
    //    //        reportLog.AddToFillers(this, this.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
    //    //        reportLog.AddToFillers(criminal.owner, criminal.owner.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    //    //        reportLog.AddToFillers(null, Utilities.NormalizeStringUpperCaseFirstLetters(committedCrime.ToString()), LOG_IDENTIFIER.STRING_1);
    //    //        PlayerManager.Instance.player.ShowNotificationFrom(this, reportLog);
    //    //    }
    //    //}

    //    logComponent.PrintLogIfActive(reactSummary);
    //}
    /// <summary>
    /// Crime reactions per role.
    /// </summary>
    /// <param name="committedCrime">The type of crime that was committed.</param>
    /// <param name="criminal">The character that committed the crime</param>
    /// <param name="witnessedCrime">The crime witnessed by this character, if this is null, character was only informed of the crime by someone else.</param>
    /// <param name="informedCrime">The crime this character was informed of. NOTE: Should only have value if Share Intel</param>
    //private void PerRoleCrimeReaction(CRIME committedCrime, ActualGoapNode crimeAction, Character criminal, ActualGoapNode witnessedCrime = null, ActualGoapNode informedCrime = null) {
    //    //GoapPlanJob job = null;
    //    switch (role.roleType) {
    //        case CHARACTER_ROLE.CIVILIAN:
    //        case CHARACTER_ROLE.ADVENTURER:
    //            //- If the character is a Civilian or Adventurer, he will enter Flee mode (fleeing the criminal) and will create a Report Crime Job Type in his personal job queue
    //            //if (this.faction != FactionManager.Instance.neutralFaction && criminal.faction == this.faction) {
    //            //    //only make character flee, if he/she actually witnessed the crime (not share intel)
    //            //    //GoapAction crimeToReport = informedCrime;
    //            //    //if (witnessedCrime != null) {
    //            //    //    crimeToReport = witnessedCrime;
    //            //    //    ////if a character has no negative disabler traits. Do not Flee. This is so that the character will not also add a Report hostile job
    //            //    //    //if (!this.HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER)) { 
    //            //    //    //    this.combatComponent.AddHostileInRange(criminal.owner, false);
    //            //    //    //}
    //            //    //}
    //            //    //TODO: job = CreateReportCrimeJob(committedCrime, crimeToReport, criminal);
    //            //}
    //            break;
    //        case CHARACTER_ROLE.LEADER:
    //        case CHARACTER_ROLE.NOBLE:
    //            //- If the character is a Noble or Faction Leader, the criminal will gain the relevant Crime-type trait
    //            //If he is a Noble or Faction Leader, he will create the Apprehend Job Type in the Location job queue instead.
    //            if (!isFactionless && criminal.faction == this.faction) {
    //                //only add apprehend job if the criminal is part of this characters faction
    //                criminal.AddCriminalTrait(committedCrime, crimeAction);
    //                // CreateApprehendJobFor(criminal);
    //                //crimeAction.OnReportCrime();
    //                //job = JobManager.Instance.CreateNewGoapPlanJob("Apprehend", new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_FROM_PARTY, conditionKey = homeSettlement, targetPOI = actor });
    //                //job.AddForcedInteraction(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Restrained", targetPOI = actor }, INTERACTION_TYPE.RESTRAIN_CHARACTER);
    //                //job.SetCanTakeThisJobChecker(CanCharacterTakeApprehendJob);
    //                //homeSettlement.jobQueue.AddJobInQueue(job);
    //            }

    //            break;
    //        case CHARACTER_ROLE.SOLDIER:
    //        case CHARACTER_ROLE.BANDIT:
    //            //- If the character is a Soldier, the criminal will gain the relevant Crime-type trait
    //            if (!isFactionless && criminal.faction == this.faction) {
    //                //only add apprehend job if the criminal is part of this characters faction
    //                criminal.AddCriminalTrait(committedCrime, crimeAction);
    //                //- If the character is a Soldier, he will also create an Apprehend Job Type in his personal job queue.
    //                //job = JobManager.Instance.CreateNewGoapPlanJob("Apprehend", new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_FROM_PARTY, conditionKey = homeSettlement, targetPOI = actor });
    //                //job.AddForcedInteraction(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Restrained", targetPOI = actor }, INTERACTION_TYPE.RESTRAIN_CHARACTER);
    //                //job.SetCanTakeThisJobChecker(CanCharacterTakeApprehendJob);
    //                //homeSettlement.jobQueue.AddJobInQueue(job);
    //                // CreateApprehendJobFor(criminal, true); //job =
    //                //if (job != null) {
    //                //    homeSettlement.jobQueue.ForceAssignCharacterToJob(job, this);
    //                //}
    //                //crimeAction.OnReportCrime();
    //            }
    //            break;
    //        default:
    //            break;
    //    }
    //}
    //public void AddCriminalTrait(CRIME crime, ActualGoapNode crimeAction) {
    //    Trait trait = null;
    //    switch (crime) {
    //        case CRIME.THEFT:
    //            trait = new Thief();
    //            break;
    //        case CRIME.ASSAULT:
    //            trait = new Assaulter();
    //            break;
    //        case CRIME.MURDER:
    //            trait = new Murderer();
    //            break;
    //        case CRIME.ATTEMPTED_MURDER:
    //            trait = new AttemptedMurderer();
    //            break;
    //        case CRIME.ABERRATION:
    //            trait = new Aberration();
    //            break;
    //        case CRIME.HERETIC:
    //            trait = new Heretic();
    //            break;
    //        default:
    //            break;
    //    }
    //    if (trait != null) {
    //        traitContainer.AddTrait(this, trait, null, crimeAction);
    //    }
    //}
    //private void OnCharacterPerformedAction(ActualGoapNode node) {
    //    if (canWitness && marker != null && node.actor != this && node.poiTarget != this) {
    //        bool isInVision = false;
    //        for (int i = 0; i < marker.inVisionCharacters.Count; i++) {
    //            Character characterInVision = marker.inVisionCharacters[i];
    //            if(node.actor == characterInVision || node.poiTarget == characterInVision) {
    //                isInVision = true;
    //                break;
    //            }
    //        }
    //        if (isInVision) {
    //            CRIME_TYPE crimeType = CrimeManager.Instance.GetCrimeTypeConsideringAction(this, node);
    //            if (crimeType != CRIME_TYPE.NONE) {
    //                CrimeManager.Instance.ReactToCrime(this, node, node.actor.currentJob as GoapPlanJob, crimeType);
    //            }
    //        }
    //    }
    //}
    #endregion

    #region Pathfinding
    public List<LocationGridTile> GetTilesInRadius(int radius, int radiusLimit = 0, bool includeCenterTile = false, bool includeTilesInDifferentStructure = false) {
        if(currentSettlement == null) { return null; }
        List<LocationGridTile> tiles = new List<LocationGridTile>();
        int mapSizeX = currentSettlement.innerMap.map.GetUpperBound(0);
        int mapSizeY = currentSettlement.innerMap.map.GetUpperBound(1);
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
                    LocationGridTile result = currentSettlement.innerMap.map[dx, dy];
                    if (!includeTilesInDifferentStructure && result.structure != gridTileLocation.structure) { continue; }
                    tiles.Add(result);
                }
            }
        }
        return tiles;
    }
    #endregion

    #region States
    private const float Combat_Signalled_Distance = 4f;
    private void OnCharacterStartedState(Character characterThatStartedState, CharacterState state) {
        if (characterThatStartedState == this) {
            marker.UpdateActionIcon();
            if (state.characterState.IsCombatState()) {
                ClearIgnoreHostilities();
            }
        } else {
            //if (state.characterState == CHARACTER_STATE.COMBAT && traitContainer.GetNormalTrait<Trait>("Unconscious", "Resting") == null && isAtHomeRegion && !ownParty.icon.isTravellingOutside) {
            //    //Reference: https://trello.com/c/2ZppIBiI/2428-combat-available-npcs-should-be-able-to-be-aware-of-hostiles-quickly
            //    CombatState combatState = state as CombatState;
            //    float distance = Vector2.Distance(this.marker.transform.position, characterThatStartedState.marker.transform.position);
            //    Character targetCharacter = null;
            //    if (combatState.isAttacking && combatState.currentClosestHostile is Character) {
            //        targetCharacter = combatState.currentClosestHostile as Character;
            //    }
            //    //Debug.Log(this.name + " distance with " + characterThatStartedState.name + " is " + distance.ToString());
            //    if (targetCharacter != null && this.isPartOfHomeFaction && characterThatStartedState.isAtHomeRegion && characterThatStartedState.isPartOfHomeFaction && this.IsCombatReady()
            //        && this.IsHostileOutsider(targetCharacter) && (opinionComponent.GetRelationshipEffectWith(characterThatStartedState) == RELATIONSHIP_EFFECT.POSITIVE || characterThatStartedState.role.roleType == CHARACTER_ROLE.SOLDIER)
            //        && distance <= Combat_Signalled_Distance) {
            //        if (combatComponent.AddHostileInRange(targetCharacter, false)) {
            //            if (!combatComponent.avoidInRange.Contains(targetCharacter)) {
            //                Log joinLog = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "join_combat_signaled");
            //                joinLog.AddToFillers(this, this.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            //                joinLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            //                joinLog.AddToFillers(characterThatStartedState, characterThatStartedState.name, LOG_IDENTIFIER.CHARACTER_3);
            //                joinLog.AddLogToSpecificObjects(LOG_IDENTIFIER.ACTIVE_CHARACTER, LOG_IDENTIFIER.TARGET_CHARACTER);
            //                PlayerManager.Instance.player.ShowNotificationFrom(this, joinLog);
            //            }
            //            //combatComponent.ProcessCombatBehavior();
            //            return; //do not do watch.
            //        }
            //    }
            //    if (marker.inVisionCharacters.Contains(characterThatStartedState)) {
            //        ThisCharacterWatchEvent(characterThatStartedState, null, null);
            //    }
            //}
        }
    }
    public void OnCharacterEndedState(Character character, CharacterState state) {
        if (character == this) {
            if (state is CombatState && marker != null) {
                combatComponent.OnThisCharacterEndedCombatState();
            }
        }
    }
    #endregion

    #region Alter Egos
    //private void InitializeAlterEgos() {
    //    alterEgos = new Dictionary<string, AlterEgoData> {
    //        {CharacterManager.Original_Alter_Ego, new AlterEgoData(this, CharacterManager.Original_Alter_Ego)}
    //    };
    //    currentAlterEgoName = CharacterManager.Original_Alter_Ego;
    //    currentAlterEgo.SetFaction(faction);
    //    currentAlterEgo.SetCharacterClass(characterClass);
    //    currentAlterEgo.SetRace(race);
    //    currentAlterEgo.SetRole(role);
    //    currentAlterEgo.SetHomeStructure(homeStructure);
    //}
    //public AlterEgoData CreateNewAlterEgo(string alterEgoName) {
    //    if (alterEgos.ContainsKey(alterEgoName)) {
    //        throw new Exception(this.name + " already has an alter ego named " + alterEgoName + " but something is trying to create a new one!");
    //    }
    //    AlterEgoData newData = new AlterEgoData(this, alterEgoName);
    //    AddAlterEgo(newData);
    //    return newData;
    //}
    //private void AddAlterEgo(AlterEgoData data) {
    //    if (!alterEgos.ContainsKey(data.name)) {
    //        alterEgos.Add(data.name, data);
    //    }
    //}
    //public void RemoveAlterEgo(string alterEgoName) {
    //    if (alterEgoName == CharacterManager.Original_Alter_Ego) {
    //        throw new Exception("Something is trying to remove " + this.name + "'s original alter ego! This should not happen!");
    //    }
    //    if (currentAlterEgoName == alterEgoName) {
    //        //switch to the original alter ego
    //        SwitchAlterEgo(CharacterManager.Original_Alter_Ego);
    //    }
    //    if (alterEgos.ContainsKey(alterEgoName)) {
    //        alterEgos.Remove(alterEgoName);
    //    }
    //}
    //public bool isSwitchingAlterEgo { get; private set; } //is this character in the process of switching alter egos?
    //public void SwitchAlterEgo(string alterEgoName) {
    //    if (currentAlterEgoName == alterEgoName) {
    //        return; //ignore change
    //    }
    //    if (alterEgos.ContainsKey(alterEgoName)) {
    //        isSwitchingAlterEgo = true;
    //        //for (int i = 0; i < traitContainer.allTraits.Count; i++) {
    //        //    Trait currTrait = traitContainer.allTraits[i];
    //        //    if (currTrait.isRemovedOnSwitchAlterEgo) {
    //        //        if (traitContainer.RemoveTrait(this, currTrait)) {
    //        //            i--;
    //        //        }
    //        //    }
    //        //}
    //        //apply all alter ego changes here
    //        AlterEgoData alterEgoData = alterEgos[alterEgoName];
    //        //currentAlterEgo.CopySpecialTraits();

    //        //Drop all plans except for the current action
    //        Messenger.Broadcast(Signals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI, this as IPointOfInterest, "target is not found");
    //        if (currentActionNode != null) {
    //            CancelAllJobsExceptForCurrent();
    //        } else {
    //            CancelAllJobs();
    //        }

    //        if(alterEgoName == "Lycanthrope") {
    //            needsComponent.hasForcedTiredness = true;
    //        }
    //        needsComponent.SetHasCancelledSleepSchedule(false);
    //        needsComponent.ResetSleepTicks();
    //        needsComponent.ResetFullnessMeter();
    //        needsComponent.ResetHappinessMeter();
    //        needsComponent.ResetTirednessMeter();

    //        SetHomeStructure(alterEgoData.homeStructure);
    //        ChangeFactionTo(alterEgoData.faction);
    //        AssignRole(alterEgoData.role);
    //        AssignClass(alterEgoData.characterClass);
    //        ChangeRace(alterEgoData.race);
    //        SetLevel(alterEgoData.level);
    //        SetMaxHPMod(alterEgoData.maxHPMod);
    //        SetMaxHPPercentMod(alterEgoData.maxHPPercentMod);
    //        SetAttackMod(alterEgoData.attackPowerMod);
    //        SetAttackPercentMod(alterEgoData.attackPowerPercentMod);
    //        SetSpeedMod(alterEgoData.speedMod);
    //        SetSpeedPercentMod(alterEgoData.speedPercentMod);
    //        traitContainer.RemoveAllNonPersistentTraits(this); //remove all non persistent traits (include alter ego: false)

    //        //ForceCancelAllJobsTargettingCharacter(false, "target is not found");

    //        for (int i = 0; i < alterEgoData.traits.Count; i++) {
    //            traitContainer.AddTrait(this, alterEgoData.traits[i]);
    //        }
    //        currentAlterEgoName = alterEgoName;
    //        isSwitchingAlterEgo = false;
    //        visuals.UpdateAllVisuals(this);
    //        Messenger.Broadcast(Signals.CHARACTER_SWITCHED_ALTER_EGO, this);
    //    } else {
    //        throw new Exception(this.name + " is trying to switch to alter ego " + alterEgoName + " but doesn't have an alter ego of that name!");
    //    }
    //}
    //public AlterEgoData GetAlterEgoData(string alterEgoName) {
    //    if (alterEgos.ContainsKey(alterEgoName)) {
    //        return alterEgos[alterEgoName];
    //    }
    //    return null;
    //}
    #endregion

    #region Converters
    //public static implicit operator Relatable(Character d) => d.currentAlterEgo;
    #endregion

    #region Limiters
    public void IncreaseCanWitness() {
        _canWitnessValue++;
    }
    public void DecreaseCanWitness() {
        _canWitnessValue--;
    }
    public void IncreaseCanMove() {
        bool couldNotMoveBefore = canMove == false;
        _canMoveValue++;
        if (couldNotMoveBefore && canMove) {
            //character could not move before adjustment, but can move after adjustment
            Messenger.Broadcast(Signals.CHARACTER_CAN_MOVE_AGAIN, this);
        }
    }
    public void DecreaseCanMove() {
        bool couldMoveBefore = canMove;
        _canMoveValue--;
        if (couldMoveBefore && canMove == false) {
            //character could move before adjustment, but cannot move after adjustment
            Messenger.Broadcast(Signals.CHARACTER_CAN_NO_LONGER_MOVE, this);
        }
    }
    public void IncreaseCanBeAttacked() {
        _canBeAtttackedValue++;
    }
    public void DecreaseCanBeAttacked() {
        _canBeAtttackedValue--;
    }
    public void IncreaseCanPerform() {
        bool couldNotPerformBefore = canPerform == false;
        _canPerformValue++;
        if (couldNotPerformBefore && canPerform) {
            //character could not perform before adjustment, but can perform after adjustment
            Messenger.Broadcast(Signals.CHARACTER_CAN_PERFORM_AGAIN, this);
        }
    }
    public void DecreaseCanPerform() {
        bool couldPerformBefore = canPerform;
        _canPerformValue--;
        if (couldPerformBefore && canPerform == false) {
            //character could perform before adjustment, but cannot perform after adjustment
            Messenger.Broadcast(Signals.CHARACTER_CAN_NO_LONGER_PERFORM, this);
        }
    }
    public void IncreaseCanTakeJobs() {
        canTakeJobsValue++;
    }
    public void DecreaseCanTakeJobs() {
        canTakeJobsValue--;
    }
    #endregion

    #region IJobOwner
    public void OnJobAddedToCharacterJobQueue(JobQueueItem job, Character character) {
    }
    public void OnJobRemovedFromCharacterJobQueue(JobQueueItem job, Character character) {
        JobManager.Instance.OnFinishJob(job);
    }
    public bool ForceCancelJob(JobQueueItem job) {
        //JobManager.Instance.OnFinishGoapPlanJob(job);
        return true;
    }
    public void AddForcedCancelJobsOnTickEnded(JobQueueItem job) {
        if (!forcedCancelJobsOnTickEnded.Contains(job)) {
            forcedCancelJobsOnTickEnded.Add(job);
        }
    }
    public void ProcessForcedCancelJobsOnTickEnded() {
        if (forcedCancelJobsOnTickEnded.Count > 0) {
            for (int i = 0; i < forcedCancelJobsOnTickEnded.Count; i++) {
                forcedCancelJobsOnTickEnded[i].ForceCancelJob(false);
            }
            forcedCancelJobsOnTickEnded.Clear();
        }
    }
    #endregion

    #region Build Structure Component
    public void AssignBuildStructureComponent() {
        buildStructureComponent = new BuildStructureComponent(this);
    }
    public void UnassignBuildStructureComponent() {
        buildStructureComponent = null;
    }
    #endregion

    #region IDamageable
    public bool CanBeDamaged() {
        return true;
    }
    #endregion

    #region Missing
    public void CheckMissing() {
        if (!isDead) {
            if (marker != null && gridTileLocation != null && isAtHomeRegion && gridTileLocation.buildSpotOwner.isPartOfParentRegionMap 
                && gridTileLocation.IsPartOfSettlement()) {
                if (currentMissingTicks > CharacterManager.Instance.CHARACTER_MISSING_THRESHOLD) {
                    currentMissingTicks = 0;
                    Messenger.Broadcast(Signals.CHARACTER_NO_LONGER_MISSING, this);
                }
            } else {
                //If not home region, increment missing ticks
                if (currentMissingTicks <= CharacterManager.Instance.CHARACTER_MISSING_THRESHOLD) {
                    currentMissingTicks++;
                    if (currentMissingTicks > CharacterManager.Instance.CHARACTER_MISSING_THRESHOLD) {
                        Messenger.Broadcast(Signals.CHARACTER_MISSING, this);
                    }
                }
            }
        }
    }
    private void OnCharacterMissing(Character missingCharacter) {
        if(missingCharacter != this) {
            string opinionLabel = relationshipContainer.GetOpinionLabel(missingCharacter);
            if(opinionLabel == OpinionComponent.Friend) {
                needsComponent.AdjustHope(-5f);
            }else if (opinionLabel == OpinionComponent.Close_Friend) {
                needsComponent.AdjustHope(-10f);
            }
        }
    }
    private void OnCharacterNoLongerMissing(Character missingCharacter) {
        if (missingCharacter != this) {
            string opinionLabel = relationshipContainer.GetOpinionLabel(missingCharacter);
            if (opinionLabel == OpinionComponent.Friend) {
                needsComponent.AdjustHope(5f);
            } else if (opinionLabel == OpinionComponent.Close_Friend) {
                needsComponent.AdjustHope(10f);
            }
        }
    }
    #endregion

    #region Lycanthropy
    //NOTE: This might a bad practice since we have a special case here for lycanthrope, but I see no other way to easily know if the character is a lycan or not
    //This way we can easily know and access the lycan data
    public void SetLycanthropeData(LycanthropeData data) {
        lycanData = data;
    }
    #endregion

    #region Player Action Target
    public List<PlayerAction> actions { get; protected set; }
    public virtual void ConstructDefaultActions() {
        actions = new List<PlayerAction>();

        if (race == RACE.DEMON) {
            PlayerAction stopAction = new PlayerAction(PlayerDB.Stop_Action, 
                () => true,
                jobComponent.TriggerStopJobs);
            PlayerAction returnAction = new PlayerAction(PlayerDB.Return_To_Portal_Action, 
                () => true,
                () => jobComponent.TriggerReturnPortal());
            PlayerAction combatModeAction = new PlayerAction(PlayerDB.Combat_Mode_Action,
                () => true,
                UIManager.Instance.characterInfoUI.ShowSwitchCombatModeUI);
            combatModeAction.SetLabelText(combatModeAction.actionName + ": " + UtilityScripts.Utilities.NotNormalizedConversionEnumToString(combatComponent.combatMode.ToString()));

            AddPlayerAction(stopAction);
            AddPlayerAction(returnAction);
            AddPlayerAction(combatModeAction);
        } else {
            PlayerAction afflictAction = new PlayerAction(PlayerDB.Afflict_Action, 
                () => true,
                UIManager.Instance.characterInfoUI.ShowAfflictUI);
            PlayerAction zapAction = new PlayerAction(PlayerDB.Zap_Action, 
                () => PlayerManager.Instance.allSpellsData[SPELL_TYPE.ZAP].CanPerformAbilityTowards(this),
                () => PlayerManager.Instance.allSpellsData[SPELL_TYPE.ZAP].ActivateAbility(this));
            PlayerAction seizeAction = new PlayerAction(PlayerDB.Seize_Character_Action, 
                () => !PlayerManager.Instance.player.seizeComponent.hasSeizedPOI && !this.traitContainer.HasTrait("Leader", "Blessed"), 
                () => PlayerManager.Instance.player.seizeComponent.SeizePOI(this));
            // PlayerAction shareIntelAction = new PlayerAction("Share Intel", () => false, null);

            AddPlayerAction(afflictAction);
            AddPlayerAction(zapAction);
            AddPlayerAction(seizeAction);
        }
        // AddPlayerAction(shareIntelAction);
    }
    public void AddPlayerAction(PlayerAction action) {
        if (actions.Contains(action) == false) {
            actions.Add(action);
            Messenger.Broadcast(Signals.PLAYER_ACTION_ADDED_TO_TARGET, action, this as IPlayerActionTarget);    
        }
    }
    public void RemovePlayerAction(PlayerAction action) {
        if (actions.Remove(action)) {
            Messenger.Broadcast(Signals.PLAYER_ACTION_REMOVED_FROM_TARGET, action, this as IPlayerActionTarget);
        }
    }
    public void RemovePlayerAction(string actionName) {
        for (int i = 0; i < actions.Count; i++) {
            PlayerAction action = actions[i];
            if (action.actionName == actionName) {
                actions.RemoveAt(i);
                Messenger.Broadcast(Signals.PLAYER_ACTION_REMOVED_FROM_TARGET, action, this as IPlayerActionTarget);
            }
        }
    }
    public void ClearPlayerActions() {
        actions.Clear();
    }
    #endregion
    
    #region Selectable
    public bool IsCurrentlySelected() {
        return UIManager.Instance.characterInfoUI.isShowing &&
               UIManager.Instance.characterInfoUI.activeCharacter == this;
    }
    public void LeftSelectAction() {
        mapObjectVisual.ExecuteClickAction(PointerEventData.InputButton.Left);
        // UIManager.Instance.ShowCharacterInfo(this);
    }
    public void RightSelectAction() {
        mapObjectVisual.ExecuteClickAction(PointerEventData.InputButton.Right);
        // UIManager.Instance.ShowCharacterInfo(this);
    }
    #endregion
}