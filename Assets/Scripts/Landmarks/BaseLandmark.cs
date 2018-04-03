﻿/*
 This is the base class for all landmarks.
 eg. Settlements(Cities), Resources, Dungeons, Lairs, etc.
 */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ECS;

public class BaseLandmark : ILocation, TaskCreator {
    protected int _id;
    protected HexTile _location;
    protected LANDMARK_TYPE _specificLandmarkType;
    protected List<object> _connections;
    protected bool _canBeOccupied; //can the landmark be occupied?
    protected bool _isOccupied;
    //protected bool _isHidden; //is landmark hidden or discovered?
    protected bool _isExplored; //has landmark been explored?
    protected string _landmarkName;
    protected Faction _owner;
    protected float _civilians; //This only contains the number of civilians (not including the characters) refer to totalPopulation to get the sum of the 2
	protected int _reservedCivilians;
    protected List<Character> _charactersWithHomeOnLandmark;
    //protected Dictionary<MATERIAL, MaterialValues> _materialsInventory; //list of materials in landmark
    //protected Dictionary<PRODUCTION_TYPE, MATERIAL> _neededMaterials; //list of materials in landmark
    protected MATERIAL _materialMadeOf; //What material is this landmark made of?
    protected Dictionary<RACE, int> _civiliansByRace;
    protected int _currDurability;
	protected int _totalDurability;
    protected List<TECHNOLOGY> _technologiesOnLandmark;
    protected Dictionary<TECHNOLOGY, bool> _technologies; //list of technologies and whether or not the landmark has that type of technology
    protected LandmarkObject _landmarkObject;
	protected List<Character> _prisoners; //list of prisoners on landmark
    protected List<Log> _history;
	protected int _combatHistoryID;
	protected Dictionary<int, CombatPrototype> _combatHistory;
    protected List<ICombatInitializer> _charactersAtLocation;
    protected CombatPrototype _currentCombat;
	//protected List<OldQuest.Quest> _activeQuests;
	protected List<Item> _itemsInLandmark;
	protected Dictionary<Character, GameDate> _characterTraces; //Lasts for 60 days

    private bool _hasScheduledCombatCheck = false;

    #region getters/setters
    public int id {
        get { return _id; }
    }
    public string locationName {
        get { return landmarkName; }
    }
    public string landmarkName {
		get { return _landmarkName; }
	}
	public string urlName {
		get { return "[url=" + this._id.ToString() + "_landmark]" + _landmarkName + "[/url]"; }
	}
    public LANDMARK_TYPE specificLandmarkType {
        get { return _specificLandmarkType; }
    }
    public List<object> connections {
        get { return _connections; }
    }
    public bool canBeOccupied {
        get { return _canBeOccupied; }
    }
    public bool isOccupied {
        get { return _isOccupied; }
    }
    //public bool isHidden {
    //    get { return _isHidden; }
    //}
    public bool isExplored {
        get { return _isExplored; }
    }
    public Faction owner {
        get { return _owner; }
    }
    public virtual int totalPopulation {
		get { return civilians + CharactersCount(); }
    }
	public int civilians {
		get { return _civiliansByRace.Sum(x => x.Value); }
    }
    public Dictionary<RACE, int> civiliansByRace {
        get { return _civiliansByRace; }
    }
  //  public Dictionary<MATERIAL, MaterialValues> materialsInventory {
		//get { return _materialsInventory; }
  //  }
    public Dictionary<TECHNOLOGY, bool> technologies {
        get { return _technologies; }
    }
    public LandmarkObject landmarkObject {
        get { return _landmarkObject; }
    }
	public List<Character> prisoners {
		get { return _prisoners; }
	}
	public List<Log> history{
		get { return this._history; }
	}
	public Dictionary<int, CombatPrototype> combatHistory {
		get { return _combatHistory; }
	}
    public List<ICombatInitializer> charactersAtLocation {
        get { return _charactersAtLocation; }
    }
	//public List<OldQuest.Quest> activeQuests {
	//	get { return _activeQuests; }
	//}
	public HexTile tileLocation{
		get { return _location; }
	}
	public LOCATION_IDENTIFIER locIdentifier{
		get { return LOCATION_IDENTIFIER.LANDMARK; }
	}
	public List<Item> itemsInLandmark {
		get { return _itemsInLandmark; }
	}
    public int currDurability {
        get { return _currDurability; }
    }
    public int totalDurability {
		get { return _totalDurability; }
    }
    public MATERIAL materialMadeOf {
        get { return _materialMadeOf; }
    }
	public Dictionary<Character, GameDate> characterTraces {
		get { return _characterTraces; }
	}
    #endregion

    public BaseLandmark(HexTile location, LANDMARK_TYPE specificLandmarkType, MATERIAL materialMadeOf = MATERIAL.NONE) {
        _id = Utilities.SetID(this);
        _location = location;
        _specificLandmarkType = specificLandmarkType;
        _connections = new List<object>();
        //_isHidden = true;
        _isExplored = false;
        _landmarkName = RandomNameGenerator.Instance.GetLandmarkName(specificLandmarkType);
//		_landmarkName = _specificLandmarkType.ToString ();
        _owner = null; //landmark has no owner yet
        _civilians = 0f;
        _charactersWithHomeOnLandmark = new List<Character>();
		_prisoners = new List<Character>();
		_history = new List<Log>();
		_combatHistory = new Dictionary<int, CombatPrototype>();
		_combatHistoryID = 0;
        _charactersAtLocation = new List<ICombatInitializer>();
		//_activeQuests = new List<OldQuest.Quest>();
		_itemsInLandmark = new List<Item> ();
		_characterTraces = new Dictionary<Character, GameDate> ();
        _materialMadeOf = materialMadeOf;
		_totalDurability = GetTotalDurability ();
		_currDurability = _totalDurability;
        ConstructTechnologiesDictionary();
		//ConstructMaterialValues();
        ConstructCiviliansDictionary();
        SpawnInitialLandmarkItems();
//        Initialize();
    }

    #region Virtuals
    public virtual void Initialize() {}
	public virtual void DestroyLandmark(bool putRuinStructure){}
    /*
     What should happen when a character searches this landmark
         */
    public virtual void SearchLandmark(Character character) { }
	#endregion

    public void SetLandmarkObject(LandmarkObject obj) {
        _landmarkObject = obj;
        _landmarkObject.SetLandmark(this);
        //if(this is ResourceLandmark) {
        //    SetHiddenState(false);
        //    SetExploredState(true);
        //}
    }

    #region Connections
    public void AddConnection(BaseLandmark connection) {
        if (!_connections.Contains(connection)) {
            _connections.Add(connection);
        }
    }
    public void AddConnection(Region connection) {
        if (!_connections.Contains(connection)) {
            _connections.Add(connection);
        }
    }
    #endregion

    #region Ownership
    public virtual void OccupyLandmark(Faction faction) {
        _owner = faction;
        _isOccupied = true;
        SetExploredState(true);
        _location.Occupy();
        EnableInitialTechnologies(faction);
    }
    public virtual void UnoccupyLandmark() {
        if(_owner == null) {
            throw new System.Exception("Landmark doesn't have an owner but something is trying to unoccupy it!");
        }
        _isOccupied = false;
        _location.Unoccupy();
        DisableInititalTechnologies(_owner);
        _owner = null;
    }
	public void ChangeOwner(Faction newOwner){
		_owner = newOwner;
		_isOccupied = true;
		_location.Occupy();
		EnableInitialTechnologies(newOwner);
	}
    #endregion

    #region Technologies
    /*
     Initialize the technologies dictionary with all the available technologies
     and set them as disabled.
         */
    private void ConstructTechnologiesDictionary() {
        TECHNOLOGY[] allTechnologies = Utilities.GetEnumValues<TECHNOLOGY>();
        _technologies = new Dictionary<TECHNOLOGY, bool>();
        for (int i = 0; i < allTechnologies.Length; i++) {
            TECHNOLOGY currTech = allTechnologies[i];
            _technologies.Add(currTech, false);
        }
    }
    /*
     Set the initial technologies of a faction as enabled on this landmark.
         */
    private void EnableInitialTechnologies(Faction faction) {
        SetTechnologyState(faction.initialTechnologies, true);
    }
    /*
     Set the initital technologies of a faction as disabled on this landmark.
         */
    private void DisableInititalTechnologies(Faction faction) {
        SetTechnologyState(faction.initialTechnologies, false);
    }
    /*
     Enable/Disable technologies in a landmark.
         */
    public void SetTechnologyState(TECHNOLOGY technology, bool state) {
        if (!state) {
            if (!_technologiesOnLandmark.Contains(technology)) {
                //technology is not inherent to the landmark, so allow action
                _technologies[technology] = state;
            }
        } else {
            _technologies[technology] = state;
        }
    }
    /*
     Set multiple technologies states.
         */
    public void SetTechnologyState(List<TECHNOLOGY> technology, bool state) {
        for (int i = 0; i < technology.Count; i++) {
            TECHNOLOGY currTech = technology[i];
            SetTechnologyState(currTech, state);
        }
    }
    /*
     Add a technology that is inherent to the current landmark.
         */
    public void AddTechnologyOnLandmark(TECHNOLOGY technology) {
        if (!_technologiesOnLandmark.Contains(technology)) {
            _technologiesOnLandmark.Add(technology);
            SetTechnologyState(technology, true);
        }
    }
    /*
     Remove a technology that is inherent to the current landmark.
         */
    public void RemoveTechnologyOnLandmark(TECHNOLOGY technology) {
        if (_technologiesOnLandmark.Contains(technology)) {
            _technologiesOnLandmark.Remove(technology);
            if(_owner != null && _owner.initialTechnologies.Contains(technology)) {
                //Do not disable technology, since the owner of the landmark has that technology inherent to itself
            } else {
                SetTechnologyState(technology, false);
            }
        }
    }
    /*
     Does this landmark have a specific technology?
         */
    public bool HasTechnology(TECHNOLOGY technology) {
        return technologies[technology];
    }
    #endregion

    #region Population
    private void ConstructCiviliansDictionary() {
        _civiliansByRace = new Dictionary<RACE, int>();
        RACE[] allRaces = Utilities.GetEnumValues<RACE>();
        for (int i = 0; i < allRaces.Length; i++) {
            RACE currRace = allRaces[i];
            if(currRace != RACE.NONE) {
                _civiliansByRace.Add(currRace, 0);
            }
        }
    }
    public void AdjustCivilians(RACE race, int amount, Character culprit = null) {
        _civiliansByRace[race] += amount;
        _civiliansByRace[race] = Mathf.Max(0, _civiliansByRace[race]);
		if(culprit != null){
			QuestManager.Instance.CreateHuntQuest (culprit);
		}
    }
    public void AdjustCivilians(Dictionary<RACE, int> civilians) {
        foreach (KeyValuePair<RACE, int> kvp in civilians) {
            AdjustCivilians(kvp.Key, kvp.Value);
        }
    }
    public Dictionary<RACE, int> ReduceCivilians(int amount) {
        Dictionary<RACE, int> reducedCivilians = new Dictionary<RACE, int>();
        for (int i = 0; i < Mathf.Abs(amount); i++) {
            RACE chosenRace = GetRaceBasedOnProportion();
            AdjustCivilians(chosenRace, -1);
            if (reducedCivilians.ContainsKey(chosenRace)) {
                reducedCivilians[chosenRace] += 1;
            } else {
                reducedCivilians.Add(chosenRace, 1);
            }
        }
        return reducedCivilians;
    }
	public void KillAllCivilians(){
		RACE[] races = _civiliansByRace.Keys.ToArray ();
		for (int i = 0; i < races.Length; i++) {
			_civiliansByRace [races [i]] = 0;
		}
	}
    //public void AdjustPopulation(float adjustment) {
    //    _civilians += adjustment;
    //}
	//public void AdjustReservedPopulation(int amount){
	//	_reservedCivilians += amount;
	//}
    protected RACE GetRaceBasedOnProportion() {
        WeightedDictionary<RACE> raceDict = new WeightedDictionary<RACE>(_civiliansByRace);
        if (raceDict.GetTotalOfWeights() > 0) {
            return raceDict.PickRandomElementGivenWeights();
        }
        throw new System.Exception("Cannot get race to produce!");
    }
    #endregion

    #region Characters
    /*
     Create a new character, given a role and class.
     This will also subtract from the civilian population.
         */
    public Character CreateNewCharacter(CHARACTER_ROLE charRole, string className, bool reduceCivilians = true, bool determineAction = true) {
        RACE raceOfChar = GetRaceBasedOnProportion();
        Character newCharacter = CharacterManager.Instance.CreateNewCharacter(charRole, className, raceOfChar, 0, _owner);
        //        newCharacter.AssignRole(charRole);
        //newCharacter.SetFaction(_owner);
        newCharacter.SetHome(this);
        if (reduceCivilians) {
            AdjustCivilians(raceOfChar, -1);
        }
        //this.AdjustPopulation(-1); //Adjust population by -1
        this.owner.AddNewCharacter(newCharacter);
        this.AddCharacterToLocation(newCharacter);
        this.AddCharacterHomeOnLandmark(newCharacter);
        if (charRole != CHARACTER_ROLE.FOLLOWER && determineAction) {
            newCharacter.DetermineAction();
        }
        UIManager.Instance.UpdateFactionSummary();
        return newCharacter;
    }
    /*
     Create a new character, given a role and class.
     This will also subtract from the civilian population.
         */
    public Character CreateNewCharacter(RACE raceOfChar, CHARACTER_ROLE charRole, string className, bool reduceCivilians = true, bool determineAction = true) {
        Character newCharacter = CharacterManager.Instance.CreateNewCharacter(charRole, className, raceOfChar);
        
        newCharacter.SetHome(this);
        if (reduceCivilians) {
            AdjustCivilians(raceOfChar, -1);
        }
        if (owner != null) {
            newCharacter.SetFaction(owner);
            owner.AddNewCharacter(newCharacter);
        }
        AddCharacterToLocation(newCharacter);
        AddCharacterHomeOnLandmark(newCharacter);
        if (determineAction) {
            newCharacter.DetermineAction();
        }
        UIManager.Instance.UpdateFactionSummary();
        return newCharacter;
    }
    /*
     Make a character consider this landmark as it's home.
         */
    public virtual void AddCharacterHomeOnLandmark(Character character) {
        if (!_charactersWithHomeOnLandmark.Contains(character)) {
            _charactersWithHomeOnLandmark.Add(character);
        }
    }
    public void RemoveCharacterHomeOnLandmark(Character character) {
        _charactersWithHomeOnLandmark.Remove(character);
    }
	public Character GetCharacterAtLocationByID(int id, bool includeTraces = false){
		for (int i = 0; i < _charactersAtLocation.Count; i++) {
			if(_charactersAtLocation[i]	is Character){
				if(((Character)_charactersAtLocation[i]).id == id){
					return (Character)_charactersAtLocation [i];
				}
			}else if(_charactersAtLocation[i] is Party){
				Party party = (Party)_charactersAtLocation [i];
				for (int j = 0; j < party.partyMembers.Count; j++) {
					if(party.partyMembers[j].id == id){
						return party.partyMembers [j];
					}
				}
			}
		}
		if(includeTraces){
			foreach (Character character in _characterTraces.Keys) {
				if(character.id == id){
					return character;
				}	
			}
		}
		return null;
	}
	public Party GetPartyAtLocationByLeaderID(int id){
		for (int i = 0; i < _charactersAtLocation.Count; i++) {
			if(_charactersAtLocation[i]	is Party){
				if(((Party)_charactersAtLocation[i]).partyLeader.id == id){
					return (Party)_charactersAtLocation [i];
				}
			}
		}
		return null;
	}
	public Character GetPrisonerByID(int id){
		for (int i = 0; i < _prisoners.Count; i++) {
			if (_prisoners [i].id == id){
				return _prisoners [i];
			}
		}
		return null;
	}
    /*
     Does the landmark have the required technology
     to produce a class?
         */
    public bool CanProduceClass(CHARACTER_CLASS charClass) {
        //if (_owner == null) {
        //    return false;
        //}
        TECHNOLOGY neededTech = Utilities.GetTechnologyForCharacterClass(charClass);
        if (neededTech == TECHNOLOGY.NONE) {
            return true;
        } else {
            return _technologies[neededTech];
        }
    }
    #endregion

    #region Party
    public List<Party> GetPartiesOnLandmark() {
        List<Party> parties = new List<Party>();
        for (int i = 0; i < _location.charactersAtLocation.Count; i++) {
			if(_location.charactersAtLocation[i] is Party){
				parties.Add((Party)_location.charactersAtLocation[i]);
			}
        }
        return parties;
    }
    #endregion

    #region Location
    public void AddCharacterToLocation(ICombatInitializer character) {
        if (!_charactersAtLocation.Contains(character)) {
            _charactersAtLocation.Add(character);
            if (character is Character) {
                Character currChar = character as Character;
				this.tileLocation.RemoveCharacterFromLocation(currChar);
                currChar.SetSpecificLocation(this);
            } else if (character is Party) {
                Party currParty = character as Party;
				this.tileLocation.RemoveCharacterFromLocation(currParty);
                currParty.SetSpecificLocation(this);
            }
            if (!_hasScheduledCombatCheck) {
                ScheduleCombatCheck();
            }
        }
    }
    public void RemoveCharacterFromLocation(ICombatInitializer character) {
        _charactersAtLocation.Remove(character);
        if (character is Character) {
            Character currChar = character as Character;
			currChar.SetSpecificLocation(this.tileLocation); //make the characters location, the hex tile that this landmark is on, meaning that the character exited the structure
        } else if (character is Party) {
            Party currParty = character as Party;
			currParty.SetSpecificLocation(this.tileLocation);//make the party's location, the hex tile that this landmark is on, meaning that the party exited the structure
        }
        if (_charactersAtLocation.Count == 0 && _hasScheduledCombatCheck) {
            UnScheduleCombatCheck();
        }
    }
    public void ReplaceCharacterAtLocation(ICombatInitializer characterToReplace, ICombatInitializer characterToAdd) {
        if (_charactersAtLocation.Contains(characterToReplace)) {
            int indexOfCharacterToReplace = _charactersAtLocation.IndexOf(characterToReplace);
            _charactersAtLocation.Insert(indexOfCharacterToReplace, characterToAdd);
            _charactersAtLocation.Remove(characterToReplace);
            if (characterToAdd is Character) {
                Character currChar = characterToAdd as Character;
				this.tileLocation.RemoveCharacterFromLocation(currChar);
                currChar.SetSpecificLocation(this);
            } else if (characterToAdd is Party) {
                Party currParty = characterToAdd as Party;
				this.tileLocation.RemoveCharacterFromLocation(currParty);
                currParty.SetSpecificLocation(this);
            }
            if (!_hasScheduledCombatCheck) {
                ScheduleCombatCheck();
            }
        }
    }
    public int CharactersCount(bool includeHostile = false) {
        int count = 0;
        for (int i = 0; i < _charactersAtLocation.Count; i++) {
			if (includeHostile && this._owner != null) {
				if(_charactersAtLocation[i].faction == null){
					continue;
				}else{
					FactionRelationship fr = this._owner.GetRelationshipWith (_charactersAtLocation [i].faction);
					if(fr != null && fr.relationshipStatus == RELATIONSHIP_STATUS.HOSTILE){
						continue;
					}
				}
			}
            if (_charactersAtLocation[i] is Party) {
                count += ((Party)_charactersAtLocation[i]).partyMembers.Count;
            } else {
                count += 1;
            }
        }
        return count;
    }
    #endregion

    #region Combat
    public void ScheduleCombatCheck() {
        _hasScheduledCombatCheck = true;
        Messenger.AddListener("OnDayStart", CheckForCombat);
    }
    public void UnScheduleCombatCheck() {
        _hasScheduledCombatCheck = false;
        Messenger.RemoveListener("OnDayStart", CheckForCombat);
    }
    /*
     Check this location for encounters, start if any.
     Mechanics can be found at https://trello.com/c/PgK25YvC/837-encounter-mechanics.
         */
    public void CheckForCombat() {
        //At the start of each day:
        if (HasHostilities()) {
            ////1. Attacking characters will attempt to initiate combat:
            //CheckAttackingGroupsCombat();
            ////2. Patrolling characters will attempt to initiate combat:
            //CheckPatrollingGroupsCombat();
            PairUpCombats();
        }
        //3. Pillaging and Hunting characters will perform their daily action if they havent been engaged in combat
        //4. Exploring and Stealing characters will perform their daily action if they havent been engaged in combat
        //5. Resting and Hibernating characters will recover HP if they havent been engaged in combat
        ContinueDailyActions();
    }
    private void PairUpCombats() {
        List<ICombatInitializer> combatInitializers = GetCharactersByCombatPriority();
        for (int i = 0; i < combatInitializers.Count; i++) {
            ICombatInitializer currInitializer = combatInitializers[i];
            Debug.Log("Finding combat pair for " + currInitializer.mainCharacter.name);
            if (currInitializer.isInCombat) {
                continue; //this current group is already in combat, skip it
            }
            //- If there are hostile parties in combat stance who are not engaged in combat, the attacking character will initiate combat with one of them at random
            List<ICombatInitializer> combatGroups = new List<ICombatInitializer>(GetGroupsBasedOnStance(STANCE.COMBAT, true, currInitializer).Where(x => x.IsHostileWith(currInitializer)));
            if (combatGroups.Count > 0) {
                ICombatInitializer chosenEnemy = combatGroups[Random.Range(0, combatGroups.Count)];
                StartCombatBetween(currInitializer, chosenEnemy);
                continue; //the attacking group has found an enemy! skip to the next group
            }

            //Otherwise, if there are hostile parties in neutral stance who are not engaged in combat, the attacking character will initiate combat with one of them at random
            List<ICombatInitializer> neutralGroups = new List<ICombatInitializer>(GetGroupsBasedOnStance(STANCE.NEUTRAL, true, currInitializer).Where(x => x.IsHostileWith(currInitializer)));
            if (neutralGroups.Count > 0) {
                ICombatInitializer chosenEnemy = neutralGroups[Random.Range(0, neutralGroups.Count)];
                StartCombatBetween(currInitializer, chosenEnemy);
                continue; //the attacking group has found an enemy! skip to the next group
            }

            //- Otherwise, if there are hostile parties in stealthy stance who are not engaged in combat, the attacking character will attempt to initiate combat with one of them at random.
            List<ICombatInitializer> stealthGroups = new List<ICombatInitializer>(GetGroupsBasedOnStance(STANCE.STEALTHY, true, currInitializer).Where(x => x.IsHostileWith(currInitializer)));
            if (stealthGroups.Count > 0) {
                //The chance of initiating combat is 35%
                if (Random.Range(0, 100) < 35) {
                    ICombatInitializer chosenEnemy = stealthGroups[Random.Range(0, stealthGroups.Count)];
                    StartCombatBetween(currInitializer, chosenEnemy);
                    continue; //the attacking group has found an enemy! skip to the next group
                }
            }
        }
    }
    private List<ICombatInitializer> GetCharactersByCombatPriority() {
        return _charactersAtLocation.Where(x => x.currentTask.combatPriority > 0).OrderByDescending(x => x.currentTask.combatPriority).ToList();
    }
    public void CheckAttackingGroupsCombat() {
        List<ICombatInitializer> attackingGroups = GetAttackingGroups();
        for (int i = 0; i < attackingGroups.Count; i++) {
            ICombatInitializer currAttackingGroup = attackingGroups[i];
            if (currAttackingGroup.isInCombat) {
                continue; //this current group is already in combat, skip it
            }
            //- If there are hostile parties in combat stance who are not engaged in combat, the attacking character will initiate combat with one of them at random
            List<ICombatInitializer> combatGroups = new List<ICombatInitializer>(GetGroupsBasedOnStance(STANCE.COMBAT, true, currAttackingGroup).Where(x => x.IsHostileWith(currAttackingGroup)));
            if (combatGroups.Count > 0) {
                ICombatInitializer chosenEnemy = combatGroups[Random.Range(0, combatGroups.Count)];
                StartCombatBetween(currAttackingGroup, chosenEnemy);
                continue; //the attacking group has found an enemy! skip to the next group
            }

            //Otherwise, if there are hostile parties in neutral stance who are not engaged in combat, the attacking character will initiate combat with one of them at random
            List<ICombatInitializer> neutralGroups = new List<ICombatInitializer>(GetGroupsBasedOnStance(STANCE.NEUTRAL, true, currAttackingGroup).Where(x => x.IsHostileWith(currAttackingGroup)));
            if (neutralGroups.Count > 0) {
                ICombatInitializer chosenEnemy = neutralGroups[Random.Range(0, neutralGroups.Count)];
                StartCombatBetween(currAttackingGroup, chosenEnemy);
                continue; //the attacking group has found an enemy! skip to the next group
            }

            //- Otherwise, if there are hostile parties in stealthy stance who are not engaged in combat, the attacking character will attempt to initiate combat with one of them at random.
            List<ICombatInitializer> stealthGroups = new List<ICombatInitializer>(GetGroupsBasedOnStance(STANCE.STEALTHY, true, currAttackingGroup).Where(x => x.IsHostileWith(currAttackingGroup)));
            if (stealthGroups.Count > 0) {
                //The chance of initiating combat is 35%
                if (Random.Range(0, 100) < 35) {
                    ICombatInitializer chosenEnemy = stealthGroups[Random.Range(0, stealthGroups.Count)];
                    StartCombatBetween(currAttackingGroup, chosenEnemy);
                    continue; //the attacking group has found an enemy! skip to the next group
                }
            }
        }
    }
    public void CheckPatrollingGroupsCombat() {
        List<ICombatInitializer> patrollingGroups = GetPatrollingGroups();
        for (int i = 0; i < patrollingGroups.Count; i++) {
            ICombatInitializer currPatrollingGroup = patrollingGroups[i];
            if (currPatrollingGroup.isInCombat) {
                continue; //this current group is already in combat, skip it
            }
            //- If there are hostile parties in combat stance who are not engaged in combat, the attacking character will initiate combat with one of them at random
            List<ICombatInitializer> combatGroups = new List<ICombatInitializer>(GetGroupsBasedOnStance(STANCE.COMBAT, true, currPatrollingGroup).Where(x => x.IsHostileWith(currPatrollingGroup)));
            if (combatGroups.Count > 0) {
                ICombatInitializer chosenEnemy = combatGroups[Random.Range(0, combatGroups.Count)];
                StartCombatBetween(currPatrollingGroup, chosenEnemy);
                continue; //the attacking group has found an enemy! skip to the next group
            }

            //Otherwise, if there are hostile parties in neutral stance who are not engaged in combat, the attacking character will initiate combat with one of them at random
            List<ICombatInitializer> neutralGroups = new List<ICombatInitializer>(GetGroupsBasedOnStance(STANCE.NEUTRAL, true, currPatrollingGroup).Where(x => x.IsHostileWith(currPatrollingGroup)));
            if (neutralGroups.Count > 0) {
                ICombatInitializer chosenEnemy = neutralGroups[Random.Range(0, neutralGroups.Count)];
                StartCombatBetween(currPatrollingGroup, chosenEnemy);
                continue; //the attacking group has found an enemy! skip to the next group
            }

            //- Otherwise, if there are hostile parties in stealthy stance who are not engaged in combat, the attacking character will attempt to initiate combat with one of them at random
            List<ICombatInitializer> stealthGroups = new List<ICombatInitializer>(GetGroupsBasedOnStance(STANCE.STEALTHY, true, currPatrollingGroup).Where(x => x.IsHostileWith(currPatrollingGroup)));
            if (stealthGroups.Count > 0) {
                //The chance of initiating combat is 35%
                if (Random.Range(0, 100) < 35) {
                    ICombatInitializer chosenEnemy = stealthGroups[Random.Range(0, stealthGroups.Count)];
                    StartCombatBetween(currPatrollingGroup, chosenEnemy);
                    continue; //the attacking group has found an enemy! skip to the next group
                }
            }
        }
    }
    public bool HasHostilities() {
        for (int i = 0; i < _charactersAtLocation.Count; i++) {
            ICombatInitializer currItem = _charactersAtLocation[i];
            for (int j = 0; j < _charactersAtLocation.Count; j++) {
                ICombatInitializer otherItem = _charactersAtLocation[j];
                if (currItem != otherItem) {
                    if (currItem.IsHostileWith(otherItem)) {
                        return true; //there are characters with hostilities
                    }
                }
            }
        }
        return false;
    }
    public bool HasHostilitiesWith(Character character) {
        for (int i = 0; i < _charactersAtLocation.Count; i++) {
            ICombatInitializer currItem = _charactersAtLocation[i];
            Faction factionOfItem = null;
            if (currItem is Character) {
                factionOfItem = (currItem as Character).faction;
            } else if (currItem is Party) {
                factionOfItem = (currItem as Party).faction;
            }
            if (factionOfItem == null || character.faction == null) {
                return true;
            } else {
                if (factionOfItem.id == character.faction.id) {
                    continue; //skip this item, since it has the same faction as the other faction
                }
                FactionRelationship rel = character.faction.GetRelationshipWith(factionOfItem);
                if (rel.relationshipStatus == RELATIONSHIP_STATUS.HOSTILE) {
                    return true;
                }
            }
        }
        return false;
    }
    public bool HasHostilitiesWith(Faction faction, bool withFactionOnly = false) {
        if (faction == null) {
            if(this.owner != null) {
                return true; //the passed faction is null (factionless), if this landmark is owned, the factionless are considered as hostile
            }
        } else {
            //the passed faction is not null, check if this landmark is owned
            if(this.owner != null) {
                //if this is owned, check if the 2 factions are not the same
                if(faction.id != this.owner.id) {
                    //if they are not the same, check if the relationship of the factions are hostile
                    FactionRelationship rel = faction.GetRelationshipWith(this.owner);
                    if (rel.relationshipStatus == RELATIONSHIP_STATUS.HOSTILE) {
                        return true; //the passed faction is hostile with the owner of this landmark
                    }
                }
            }
        }
        if (!withFactionOnly) {
            for (int i = 0; i < _charactersAtLocation.Count; i++) {
                ICombatInitializer currItem = _charactersAtLocation[i];
                Faction factionOfItem = null;
                if (currItem is Character) {
                    factionOfItem = (currItem as Character).faction;
                } else if (currItem is Party) {
                    factionOfItem = (currItem as Party).faction;
                }
                if (factionOfItem == null || faction == null) {
                    return true;
                } else {
                    if (factionOfItem.id == faction.id) {
                        continue; //skip this item, since it has the same faction as the other faction
                    }
                    FactionRelationship rel = faction.GetRelationshipWith(factionOfItem);
                    if (rel.relationshipStatus == RELATIONSHIP_STATUS.HOSTILE) {
                        return true;
                    }
                }
            }
        }
        return false;
    }
    public List<ICombatInitializer> GetAttackingGroups() {
        List<ICombatInitializer> groups = new List<ICombatInitializer>();
        for (int i = 0; i < _charactersAtLocation.Count; i++) {
            ICombatInitializer currGroup = _charactersAtLocation[i];
            if (currGroup.currentTask is Invade) {
                groups.Add(currGroup);
            }
        }
        return groups;
    }
    public List<ICombatInitializer> GetPatrollingGroups() {
        List<ICombatInitializer> groups = new List<ICombatInitializer>();
        for (int i = 0; i < _charactersAtLocation.Count; i++) {
            ICombatInitializer currGroup = _charactersAtLocation[i];
			if (currGroup.currentTask is Patrol) {
                groups.Add(currGroup);
            }
        }
        return groups;
    }
    public List<ICombatInitializer> GetGroupsBasedOnStance(STANCE stance, bool notInCombatOnly, ICombatInitializer except = null) {
        List<ICombatInitializer> groups = new List<ICombatInitializer>();
        for (int i = 0; i < _charactersAtLocation.Count; i++) {
            ICombatInitializer currGroup = _charactersAtLocation[i];
            if (notInCombatOnly) {
                if (currGroup.isInCombat) {
                    continue; //skip
                }
            }
            if (currGroup.GetCurrentStance() == stance) {
                if (except != null && currGroup == except) {
                    continue; //skip
                }
                groups.Add(currGroup);
            }
        }
        return groups;
    }
    public void StartCombatBetween(ICombatInitializer combatant1, ICombatInitializer combatant2) {
        CombatPrototype combat = new CombatPrototype(combatant1, combatant2, this);
        combatant1.SetIsInCombat(true);
        combatant2.SetIsInCombat(true);
        string combatant1Name = string.Empty;
        string combatant2Name = string.Empty;
        if (combatant1 is Party) {
            combatant1Name = (combatant1 as Party).name;
            combat.AddCharacters(SIDES.A, (combatant1 as Party).partyMembers);
        } else {
            combatant1Name = (combatant1 as Character).name;
            combat.AddCharacter(SIDES.A, combatant1 as Character);
        }
        if (combatant2 is Party) {
            combatant2Name = (combatant2 as Party).name;
            combat.AddCharacters(SIDES.B, (combatant2 as Party).partyMembers);
        } else {
            combatant2Name = (combatant2 as Character).name;
            combat.AddCharacter(SIDES.B, combatant2 as Character);
        }
        Log combatLog = new Log(GameManager.Instance.Today(), "General", "Combat", "start_combat");
        combatLog.AddToFillers(combatant1, combatant1Name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        combatLog.AddToFillers(combatant2, combatant2Name, LOG_IDENTIFIER.TARGET_CHARACTER);
        AddHistory(combatLog);
        combatant1.mainCharacter.AddHistory(combatLog);
        combatant2.mainCharacter.AddHistory(combatLog);
        Debug.Log("Starting combat between " + combatant1Name + " and  " + combatant2Name);

        //this.specificLocation.SetCurrentCombat(combat);
        CombatThreadPool.Instance.AddToThreadPool(combat);
    }
    public void ContinueDailyActions() {
        for (int i = 0; i < _charactersAtLocation.Count; i++) {
            ICombatInitializer currItem = _charactersAtLocation[i];
            if (!currItem.isInCombat) {
                currItem.ContinueDailyAction();
            }
        }
    }

    //public void StartCombatAtLocation() {
    //    if (!CombatAtLocation()) {
    //        this._currentCombat = null;
    //        for (int i = 0; i < _charactersAtLocation.Count; i++) {
    //            ICombatInitializer currItem = _charactersAtLocation[i];
    //            currItem.SetIsDefeated(false);
    //currItem.SetIsInCombat (false);
    //if(currItem.currentFunction != null){
    //	currItem.currentFunction ();
    //}
    //currItem.SetCurrentFunction(null);
    //        }
    //    } else {
    //        for (int i = 0; i < _charactersAtLocation.Count; i++) {
    //            ICombatInitializer currItem = _charactersAtLocation[i];
    //currItem.SetIsInCombat (true);
    //        }
    //    }
    //}
    //public bool CombatAtLocation() {
    //    for (int i = 0; i < _charactersAtLocation.Count; i++) {
    //        if (_charactersAtLocation[i].InitializeCombat()) {
    //            return true;
    //        }
    //    }
    //    return false;
    //}
    //public ICombatInitializer GetCombatEnemy(ICombatInitializer combatInitializer) {
    //    for (int i = 0; i < _charactersAtLocation.Count; i++) {
    //        if (_charactersAtLocation[i] != combatInitializer) {
    //            if (_charactersAtLocation[i] is Party) {
    //                if (((Party)_charactersAtLocation[i]).isDefeated) {
    //                    continue;
    //                }
    //            }
    //            if (combatInitializer.IsHostileWith(_charactersAtLocation[i])) {
    //                return _charactersAtLocation[i];
    //            }
    //        }
    //    }
    //    return null;
    //}
    //public void SetCurrentCombat(CombatPrototype combat) {
    //    _currentCombat = combat;
    //}
    #endregion

    #region Utilities
    public int GetTotalDurability() {
        int durabilityFromMaterial = 0;
        int durabilityModifierFromLandmarkType = 0;

        if(_materialMadeOf != MATERIAL.NONE) {
            durabilityFromMaterial = MaterialManager.Instance.GetMaterialData(_materialMadeOf).sturdiness;
        }

        LandmarkData landmarkData = LandmarkManager.Instance.GetLandmarkData(specificLandmarkType);
        if (landmarkData != null) {
            durabilityModifierFromLandmarkType = landmarkData.durabilityModifier;
        }

        if(durabilityModifierFromLandmarkType == 0) {
            return durabilityFromMaterial;
        }
        return durabilityFromMaterial * durabilityModifierFromLandmarkType;
    }
    public void SetExploredState(bool isExplored) {
        _isExplored = isExplored;
        //if (landmarkObject != null) {
        //    landmarkObject.UpdateLandmarkVisual();
        //}
    }
    internal bool IsBorder() {
        if (this.owner == null) {
            return false;
        }
		for (int i = 0; i < this.tileLocation.region.connections.Count; i++) {
			if (this.tileLocation.region.connections[i] is Region) {
				Region adjacentRegion = (Region)this.tileLocation.region.connections[i];
                if (adjacentRegion.centerOfMass.landmarkOnTile.owner != null && adjacentRegion.centerOfMass.landmarkOnTile.owner.id != this.owner.id) {
                    return true;
                }
            }
        }
        return false;
    }
    internal bool IsAdjacentToEnemyTribe() {
        if (this.owner == null || (this.owner != null && !(this.owner is Tribe))) {
            return false;
        }
		for (int i = 0; i < this.tileLocation.region.connections.Count; i++) {
			if (this.tileLocation.region.connections[i] is Region) {
				Region adjacentRegion = (Region)this.tileLocation.region.connections[i];
                if (adjacentRegion.centerOfMass.landmarkOnTile.owner != null && this.owner is Tribe && adjacentRegion.centerOfMass.landmarkOnTile.owner.id != this.owner.id) {
                    FactionRelationship factionRel = this._owner.GetRelationshipWith(adjacentRegion.centerOfMass.landmarkOnTile.owner);
                    if (factionRel != null && factionRel.isAtWar) {
                        return true;
                    }
                }
            }
        }
        return false;
    }
    internal bool HasWarlordOnAdjacentVillage() {
        if (this.owner == null) {
            return false;
        }
		for (int i = 0; i < this.tileLocation.region.connections.Count; i++) {
			if (this.tileLocation.region.connections[i] is Region) {
				Region adjacentRegion = (Region)this.tileLocation.region.connections[i];
                if (adjacentRegion.centerOfMass.landmarkOnTile.owner != null && adjacentRegion.centerOfMass.landmarkOnTile.owner.id != this.owner.id) {
                    FactionRelationship factionRel = this._owner.GetRelationshipWith(adjacentRegion.centerOfMass.landmarkOnTile.owner);
                    if (factionRel != null && factionRel.isAtWar) {
                        if (adjacentRegion.centerOfMass.landmarkOnTile.HasWarlord()) {
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }
    internal bool HasWarlord() {
        for (int i = 0; i < this._location.charactersAtLocation.Count; i++) {
            if (this._location.charactersAtLocation[i] is Character) {
                if (((Character)this._location.charactersAtLocation[i]).role.roleType == CHARACTER_ROLE.WARLORD) {
                    return true;
                }
            } else if (this._location.charactersAtLocation[i] is Party) {
                if (((Party)this._location.charactersAtLocation[i]).partyLeader.role.roleType == CHARACTER_ROLE.WARLORD) {
                    return true;
                }
            }
        }
        return false;
    }
    internal int GetTechnologyCount() {
        int count = 0;
        foreach (bool isTrue in _technologies.Values) {
            if (isTrue) {
                count += 1;
            }
        }
        return count;
    }
    internal bool HasAdjacentUnoccupiedTile() {
        for (int i = 0; i < this._location.region.connections.Count; i++) {
            if (this._location.region.connections[i] is Region) {
                Region adjacentRegion = (Region)this._location.region.connections[i];
                if (!adjacentRegion.centerOfMass.isOccupied) {
                    return true;
                }
            }
        }
        return false;
    }
    //internal HexTile GetRandomAdjacentUnoccupiedTile() {
    //    List<HexTile> allUnoccupiedCenterOfMass = new List<HexTile>();
    //    for (int i = 0; i < this._location.region.connections.Count; i++) {
    //        if (this._location.region.connections[i] is Region) {
    //            Region adjacentRegion = (Region)this._location.region.connections[i];
    //            if (!adjacentRegion.centerOfMass.isOccupied && !this.owner.internalQuestManager.AlreadyHasQuestOfType(QUEST_TYPE.EXPAND, adjacentRegion.centerOfMass)) {
    //                allUnoccupiedCenterOfMass.Add(adjacentRegion.centerOfMass);
    //            }
    //        }
    //    }
    //    if (allUnoccupiedCenterOfMass.Count > 0) {
    //        return allUnoccupiedCenterOfMass[UnityEngine.Random.Range(0, allUnoccupiedCenterOfMass.Count)];
    //    } else {
    //        return null;
    //    }
    //}
    internal int GetMinimumCivilianRequirement() {
        if (this is ResourceLandmark) {
            return 5;
        }else if(this is Settlement) {
            return 20;
        }
        return 0;
    }
	internal void ChangeLandmarkType(LANDMARK_TYPE newLandmarkType){
		_specificLandmarkType = newLandmarkType;
		Initialize ();
	}
    public void CenterOnLandmark() {
		CameraMove.Instance.CenterCameraOn(this.tileLocation.gameObject);
    }
    #endregion

    #region Prisoner
    internal void AddPrisoner(Character character){
		character.SetPrisoner (true, this);
		_prisoners.Add (character);
	}
	internal void RemovePrisoner(Character character){
		_prisoners.Remove (character);
		character.SetPrisoner (false, null);
	}
	#endregion

	#region History
    internal void AddHistory(Log log) {
        ////check if the new log is a duplicate of the latest log
        //Log latestLog = history.ElementAtOrDefault(history.Count - 1);
        //if (latestLog != null) {
        //    if (Utilities.AreLogsTheSame(log, latestLog)) {
        //        string text = landmarkName + " has duplicate logs!";
        //        text += "\n" + log.id + Utilities.LogReplacer(log) + " ST:" + log.logCallStack;
        //        text += "\n" + latestLog.id + Utilities.LogReplacer(latestLog) + " ST:" + latestLog.logCallStack;
        //        throw new System.Exception(text);
        //    }
        //}

        _history.Add(log);
        if (this._history.Count > 20) {
            this._history.RemoveAt(0);
        }
    }
    //internal void AddHistory(string text, object obj = null) {
    //    GameDate today = GameManager.Instance.Today();
    //    string date = "[" + ((MONTH)today.month).ToString() + " " + today.day + ", " + today.year + "]";
    //    if (obj != null) {
    //        if (obj is CombatPrototype) {
    //            CombatPrototype combat = (CombatPrototype)obj;
    //            if (this.combatHistory.Count > 20) {
    //                this.combatHistory.Remove(0);
    //            }
    //            _combatHistoryID += 1;
    //            combatHistory.Add(_combatHistoryID, combat);
    //            string combatText = "[url=" + _combatHistoryID.ToString() + "_combat]" + text + "[/url]";
    //            text = combatText;
    //        }
    //    }
    //    this._history.Insert(0, date + " " + text);
    //    if (this._history.Count > 20) {
    //        this._history.RemoveAt(this._history.Count - 1);
    //    }
    //}
    #endregion

    #region Materials
    public void AdjustDurability(int amount){
		_currDurability += amount;
		_currDurability = Mathf.Clamp (_currDurability, 0, _totalDurability);
	}
    #endregion

    #region Quests
 //   public void AddNewQuest(OldQuest.Quest quest) {
	//	if (!_activeQuests.Contains(quest)) {
	//		_activeQuests.Add(quest);
	//		_owner.AddNewQuest(quest);
	//		//if(quest.postedAt != null) {
	//		//	quest.postedAt.AddQuestToBoard(quest);
	//		//}
	//		//quest.ScheduleDeadline(); //Once a quest has been added to active quest, scedule it's deadline
	//	}
	//}
	//public void RemoveQuest(OldQuest.Quest quest) {
	//	_activeQuests.Remove(quest);
	//	_owner.RemoveQuest(quest);
	//}
	//public List<OldQuest.Quest> GetQuestsOfType(QUEST_TYPE questType) {
	//	List<OldQuest.Quest> quests = new List<OldQuest.Quest>();
	//	for (int i = 0; i < _activeQuests.Count; i++) {
	//		OldQuest.Quest currQuest = _activeQuests[i];
	//		if(currQuest.questType == questType) {
	//			quests.Add(currQuest);
	//		}
	//	}
	//	return quests;
	//}
	//public bool AlreadyHasQuestOfType(QUEST_TYPE questType, object identifier){
	//	for (int i = 0; i < _activeQuests.Count; i++) {
	//		OldQuest.Quest currQuest = _activeQuests[i];
	//		if(currQuest.questType == questType) {
	//			if(questType == QUEST_TYPE.EXPLORE_REGION){
	//				Region region = (Region)identifier;
	//				if(((ExploreRegion)currQuest).regionToExplore.id == region.id){
	//					return true;
	//				}
	//			} else if(questType == QUEST_TYPE.EXPAND){
	//				if(identifier is HexTile){
	//					HexTile hexTile = (HexTile)identifier;
	//					if(((Expand)currQuest).targetUnoccupiedTile.id == hexTile.id){
	//						return true;
	//					}
	//				}else if(identifier is BaseLandmark){
	//					BaseLandmark landmark = (BaseLandmark)identifier;
	//					if(((Expand)currQuest).originTile.id == landmark.tileLocation.id){
	//						return true;
	//					}
	//				}

	//			} else if (questType == QUEST_TYPE.BUILD_STRUCTURE) {
	//				BaseLandmark landmark = (BaseLandmark)identifier;
	//				if (((BuildStructure)currQuest).target.id == landmark.id) {
	//					return true;
	//				}
	//			} else if (questType == QUEST_TYPE.OBTAIN_MATERIAL) {
	//				MATERIAL material = (MATERIAL)identifier;
	//				if (((ObtainMaterial)currQuest).materialToObtain == material) {
	//					return true;
	//				}
	//			}
	//		}
	//	}
	//	return false;
	//}
    #endregion

    #region Items
    private void SpawnInitialLandmarkItems() {
        LandmarkData data = LandmarkManager.Instance.GetLandmarkData(_specificLandmarkType);
        for (int i = 0; i < data.itemData.Length; i++) {
            LandmarkItemData currItemData = data.itemData[i];
            Item createdItem = ItemManager.Instance.CreateNewItemInstance(currItemData.itemName);
            if (ItemManager.Instance.IsLootChest(createdItem)) {
                //chosen item is a loot crate, generate a random item
                string[] words = createdItem.itemName.Split(' ');
                int tier = System.Int32.Parse(words[1]);
                if (createdItem.itemName.Contains("Armor")) {
                    createdItem = ItemManager.Instance.GetRandomTier(tier, ITEM_TYPE.ARMOR);
                } else if (createdItem.itemName.Contains("Weapon")) {
                    createdItem = ItemManager.Instance.GetRandomTier(tier, ITEM_TYPE.WEAPON);
                }
                QUALITY equipmentQuality = GetEquipmentQuality();
                if (createdItem.itemType == ITEM_TYPE.ARMOR) {
                    ((Armor)createdItem).SetQuality(equipmentQuality);
                } else if (createdItem.itemType == ITEM_TYPE.WEAPON) {
                    ((Weapon)createdItem).SetQuality(equipmentQuality);
                }
            } else {
                //only set as unlimited if not from loot chest, since gear from loot chests are not unlimited
                createdItem.SetIsUnlimited(currItemData.isUnlimited);
            }
            createdItem.SetExploreWeight(currItemData.exploreWeight);
            AddItemInLandmark(createdItem);
        }
    }
    private QUALITY GetEquipmentQuality() {
        int crudeChance = 30;
        int exceptionalChance = crudeChance + 20;
        int chance = UnityEngine.Random.Range(0, 100);
        if (chance < crudeChance) {
            return QUALITY.CRUDE;
        } else if (chance >= crudeChance && chance < exceptionalChance) {
            return QUALITY.EXCEPTIONAL;
        }
        return QUALITY.NORMAL;
    }
    public void AddItemInLandmark(Item item){
        if (_itemsInLandmark.Contains(item)) {
            throw new System.Exception(this.landmarkName + " already has an instance of " + item.itemName);
        }
		_itemsInLandmark.Add (item);
        item.OnItemPlacedOnLandmark(this);
	}
	public void AddItemsInLandmark(List<Item> item){
		_itemsInLandmark.AddRange (item);
	}
	public void RemoveItemInLandmark(Item item){
		if(!item.isUnlimited){
			_itemsInLandmark.Remove (item);
		}
	}
    public void RemoveItemInLandmark(string itemName) {
        for (int i = 0; i < itemsInLandmark.Count; i++) {
            ECS.Item currItem = itemsInLandmark[i];
            if (currItem.itemName.Equals(itemName)) {
                RemoveItemInLandmark(currItem);
                break;
            }
        }
    }
    private WeightedDictionary<Item> GetExploreItemWeights() {
        WeightedDictionary<Item> itemWeights = new WeightedDictionary<Item>();
        for (int i = 0; i < _itemsInLandmark.Count; i++) {
            Item currItem = _itemsInLandmark[i];
            itemWeights.AddElement(currItem, currItem.exploreWeight);
        }
        return itemWeights;
    }
    ///*
    // What should happen when this landmark is explored?
    //     */
    //public virtual void ExploreLandmark(Character explorer) {
    //    //default behaviour is a random item will be given to the explorer based on the landmarks item weights
    //    Item generatedItem = GenerateRandomItem();
    //    if (generatedItem != null) {
    //        if (generatedItem.isObtainable) {
    //            if (!explorer.EquipItem(generatedItem)) {
    //                explorer.PickupItem(generatedItem);
    //            }
    //        } else {
    //            //item should only be interacted with
    //            StorylineManager.Instance.OnInteractWith(generatedItem, this, explorer);
    //            Log interactLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "interact_item");
    //            interactLog.AddToFillers(explorer, explorer.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
    //            interactLog.AddToFillers(null, generatedItem.interactString, LOG_IDENTIFIER.OTHER);
    //            interactLog.AddToFillers(null, generatedItem.nameWithQuality, LOG_IDENTIFIER.ITEM_1);
    //            AddHistory(interactLog);
    //            explorer.AddHistory(interactLog);
    //        }

    //    }
    //}
   // /*
   //  Generate a random item, given the data of this landmark type
   //      */
   // public Item GenerateRandomItem() {
   //     WeightedDictionary<Item> itemWeights = GetExploreItemWeights();
   //     if (itemWeights.GetTotalOfWeights() > 0) {
   //         Item chosenItem = itemWeights.PickRandomElementGivenWeights();
			////Remove item form weights if it is not unlimited
			//RemoveItemInLandmark(chosenItem);
   //         return chosenItem;
   //         //if (ItemManager.Instance.IsLootChest(chosenItem)) {
   //         //    //chosen item is a loot crate, generate a random item
   //         //    string[] words = chosenItem.itemName.Split(' ');
   //         //    int tier = System.Int32.Parse(words[1]);
   //         //    if (chosenItem.itemName.Contains("Armor")) {
   //         //        return ItemManager.Instance.GetRandomTier(tier, ITEM_TYPE.ARMOR);
   //         //    }else if (chosenItem.itemName.Contains("Weapon")) {
   //         //        return ItemManager.Instance.GetRandomTier(tier, ITEM_TYPE.WEAPON);
   //         //    }
   //         //} else {

   //         //}

   //     }
   //     return null;
   // }

	public void SpawnItemInLandmark(string itemName, int exploreWeight, bool isUnlimited){
		Item item = ItemManager.Instance.CreateNewItemInstance (itemName);
		item.exploreWeight = exploreWeight;
		item.isUnlimited = isUnlimited;
		AddItemInLandmark (item);
	}
	public void SpawnItemInLandmark(Item item, int exploreWeight, bool isUnlimited){
		Item newItem = item.CreateNewCopy();
		newItem.exploreWeight = exploreWeight;
		newItem.isUnlimited = isUnlimited;
		AddItemInLandmark (newItem);
	}
	public void SpawnItemInLandmark(string itemName){
		Item item = ItemManager.Instance.CreateNewItemInstance (itemName);
		AddItemInLandmark (item);
	}
	public void SpawnItemInLandmark(Item item){
		Item newItem = item.CreateNewCopy();
		AddItemInLandmark (newItem);
	}
	public bool HasItem(string itemName){
		for (int i = 0; i < _itemsInLandmark.Count; i++) {
			if (_itemsInLandmark [i].itemName == itemName) {
				return true;
			}
		}
		return false;
	}
    #endregion

	#region Traces
	public void AddTrace(Character character){
		GameDate expDate = GameManager.Instance.Today ();
		expDate.AddDays (90);
		if(!_characterTraces.ContainsKey(character)){
			_characterTraces.Add (character, expDate);
		}else{
			SchedulingManager.Instance.RemoveSpecificEntry (_characterTraces[character], () => RemoveTrace (character));
			_characterTraces [character] = expDate;
		}
		SchedulingManager.Instance.AddEntry (expDate, () => RemoveTrace (character));
	}
	public void RemoveTrace(Character character){
		if(_characterTraces.ContainsKey(character)){
			if(GameManager.Instance.Today().IsSameDate(_characterTraces[character])){
				_characterTraces.Remove (character);
			}
		}
	}
	#endregion
}
