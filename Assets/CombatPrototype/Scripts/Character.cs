﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;

namespace ECS {
    public class Character : ICharacter, ILeader, IInteractable, IQuestGiver {
        public delegate void OnCharacterDeath();
        public OnCharacterDeath onCharacterDeath;

        public delegate void DailyAction();
        public DailyAction onDailyAction;

        protected string _name;
        protected string _characterColorCode;
        protected int _id;
        protected int _gold;
        protected float _actRate;
        protected bool _isDead;
        protected bool _isFainted;
        protected bool _isInCombat;
        protected bool _doNotDisturb;
        protected bool _isBeingInspected;
        protected bool _hasBeenInspected;
        protected GENDER _gender;
        protected MODE _currentMode;
        protected CharacterClass _characterClass;
        protected RaceSetting _raceSetting;
        protected CharacterRole _role;
        protected Faction _faction;
        protected CharacterParty _ownParty;
        protected CharacterParty _currentParty;
        protected BaseLandmark _homeLandmark;
        protected BaseLandmark _workplace;
        protected Region _currentRegion;
        protected Weapon _equippedWeapon;
        protected Armor _equippedArmor;
        protected Item _equippedAccessory;
        protected Item _equippedConsumable;
        protected CharacterBattleTracker _battleTracker;
        protected CharacterBattleOnlyTracker _battleOnlyTracker;
        protected PortraitSettings _portraitSettings;
        protected CharacterPortrait _characterPortrait;
        protected Color _characterColor;
        protected CharacterAction _genericWorkAction;
        protected HiddenDesire _hiddenDesire;
        protected Secret _currentlySelectedSecret;
        protected Squad _squad;
        protected Minion _minion;
        protected List<Secret> _secrets;
        protected List<STATUS_EFFECT> _statusEffects;
        protected List<Item> _inventory;
        protected List<Skill> _skills;
        protected List<Attribute> _attributes;
        protected List<Log> _history;
        protected List<BaseLandmark> _exploredLandmarks; //Currently only storing explored landmarks that were explored for the last 6 months
        protected List<CombatAttribute> _combatAttributes;
        protected CharacterActionQueue<ActionQueueItem> _actionQueue;
        protected List<CharacterAction> _miscActions;
        protected List<Interaction> _currentInteractions;
        protected Dictionary<Character, Relationship> _relationships;
        protected Dictionary<ELEMENT, float> _elementalWeaknesses;
        protected Dictionary<ELEMENT, float> _elementalResistances;
        protected Dictionary<Character, List<string>> _traceInfo;
        protected Dictionary<int, GAME_EVENT> _intelReactions; //int = intel id

        //Stats
        protected SIDES _currentSide;
        protected int _currentHP;
        protected int _maxHP;
        protected int _currentRow;
        protected int _level;
        protected int _experience;
        protected int _maxExperience;
        protected int _sp;
        protected int _maxSP;
        protected int _attackPower;
        protected int _speed;

        public CharacterSchedule schedule { get; private set; }
        public Quest currentQuest { get; private set; }
        public CharacterEventSchedule eventSchedule { get; private set; }
        public CharacterUIData uiData { get; private set; }
        public BaseLandmark defendingLandmark { get; private set; }
        public MORALITY morality { get; private set; }
        private Dictionary<STAT, float> _buffs;

        public Dictionary<int, Combat> combatHistory;

        #region getters / setters
        public string firstName {
            get { return name.Split(' ')[0]; }
        }
        public virtual string name {
            get { return this._name; }
        }
        public string coloredName {
            get { return "<color=#" + this._characterColorCode  + ">" + this._name + "</color>"; }
        }
        public string urlName {
            get { return "<link=" + '"' + this._id.ToString() + "_character" + '"' + ">" + this._name + "</link>"; }
        }
        public string coloredUrlName {
            get { return "<link=" + '"' + this._id.ToString() + "_character" + '"' + ">" + "<color=#" + this._characterColorCode + ">" + this._name + "</color></link>"; }
        }
        public int id {
            get { return _id; }
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
        public List<Attribute> attributes {
            get { return _attributes; }
        }
        public Dictionary<Character, Relationship> relationships {
            get { return _relationships; }
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
        public Faction faction {
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
        public CharacterAction genericWorkAction {
            get { return _genericWorkAction; }
        }
        //public List<CharacterQuestData> questData {
        //    get { return _questData; }
        //}
        public HexTile currLocation {
            get { return (_currentParty.specificLocation != null ? _currentParty.specificLocation.tileLocation : null); }
        }
        public ILocation specificLocation {
            get { return (_currentParty.specificLocation != null ? _currentParty.specificLocation : null); }
        }
        //public List<BodyPart> bodyParts {
        //    get { return this._bodyParts; }
        //}
        //public List<Item> equippedItems {
        //    get { return this._equippedItems; }
        //}
        public List<Item> inventory {
            get { return this._inventory; }
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
        public bool isDead {
            get { return this._isDead; }
        }
        public bool isFainted {
            get { return this._isFainted; }
        }
        public Color characterColor {
            get { return _characterColor; }
        }
        public string characterColorCode {
            get { return _characterColorCode; }
        }
        //public Area home {
        //    get { return _home; }
        //}
        public BaseLandmark homeLandmark {
            get { return _homeLandmark; }
        }
        //public StructureObj homeStructure {
        //    get { return _homeStructure; }
        //}
        public BaseLandmark workplace {
            get { return _workplace; }
        }
        public float remainingHP { //Percentage of remaining HP this character has
            get { return (float) currentHP / (float) maxHP; }
        }
        public int remainingHPPercent {
            get { return (int) (remainingHP * 100); }
        }
        public List<Log> history {
            get { return this._history; }
        }
        public int gold {
            get { return _gold; }
        }
        public List<BaseLandmark> exploredLandmarks {
            get { return _exploredLandmarks; }
        }
        public bool isInCombat {
            get {
                return _isInCombat;
            }
        }
        public bool isFactionless { //is the character part of the neutral faction? or no faction?
            get {
                if (FactionManager.Instance.neutralFaction == null) {
                    if (faction != null) {
                        return false;
                    } else {
                        return true;
                    }
                } else {
                    if (faction != null && FactionManager.Instance.neutralFaction.id == faction.id) {
                        return true;
                    } else {
                        return false;
                    }
                }
            }
        }
        public Dictionary<Character, List<string>> traceInfo {
            get { return _traceInfo; }
        }
        public PortraitSettings portraitSettings {
            get { return _portraitSettings; }
        }
        public CharacterPortrait characterPortrait {
            get { return _characterPortrait; }
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
        public virtual int speed {
            get { return _speed; }
        }
        public virtual int attackPower {
            get { return _attackPower; }
        }
        public virtual int maxHP {
            get { return this._maxHP; }
        }
        public int currentHP {
            get { return this._currentHP; }
        }
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
        public CharacterBattleTracker battleTracker {
            get { return _battleTracker; }
        }
        public CharacterBattleOnlyTracker battleOnlyTracker {
            get { return _battleOnlyTracker; }
        }
        public float computedPower {
            get { return GetComputedPower(); }
        }
        public ICHARACTER_TYPE icharacterType {
            get { return ICHARACTER_TYPE.CHARACTER; }
        }
        public List<CharacterAction> miscActions {
            get { return _miscActions; }
        }
        public Squad squad {
            get { return _squad; }
        }
        public Minion minion {
            get { return _minion; }
        }
        public CharacterActionQueue<ActionQueueItem> actionQueue {
            get { return _actionQueue; }
        }
        public bool doNotDisturb {
            get { return _doNotDisturb; }
        }
        public bool isBeingInspected {
            get { return _isBeingInspected; }
        }
        public bool hasBeenInspected {
            get { return _hasBeenInspected; }
        }
        public HiddenDesire hiddenDesire {
            get { return _hiddenDesire; }
        }
        public Secret currentlySelectedSecret {
            get { return _currentlySelectedSecret; }
        }
        public List<Secret> secrets {
            get { return _secrets; }
        }
        public Dictionary<int, GAME_EVENT> intelReactions {
            get { return _intelReactions; }
        }
        public IObject questGiverObj {
            get { return currentParty.icharacterObject; }
        }
        public QUEST_GIVER_TYPE questGiverType {
            get { return QUEST_GIVER_TYPE.CHARACTER; }
        }
        public bool isDefender {
            get { return defendingLandmark != null; }
        }
        public List<CombatAttribute> combatAttributes {
            get { return _combatAttributes; }
        }
        public List<Interaction> currentInteractions {
            get { return _currentInteractions; }
        }
        public Dictionary<STAT, float> buffs {
            get { return _buffs; }
        }
        #endregion

        public Character(string className, RACE race, GENDER gender) : this() {
            _id = Utilities.SetID(this);
			_characterClass = CharacterManager.Instance.classesDictionary[className].CreateNewCopy();
			_raceSetting = RaceManager.Instance.racesDictionary[race.ToString()].CreateNewCopy();
            _gender = gender;
            _name = RandomNameGenerator.Instance.GenerateRandomName(_raceSetting.race, _gender);
            if (this is CharacterArmyUnit) {
                _portraitSettings = CharacterManager.Instance.GenerateRandomPortrait(RACE.HUMANS, GENDER.MALE);
            } else {
                _portraitSettings = CharacterManager.Instance.GenerateRandomPortrait(race, gender);
            }
            SetMorality(MORALITY.GOOD);
            //_skills = GetGeneralSkills();
            _skills = new List<Skill>();
            _skills.Add(_characterClass.skill);
            //_bodyParts = new List<BodyPart>(_raceSetting.bodyParts);
            //ConstructBodyPartDict(_raceSetting.bodyParts);
            GenerateRaceAttributes();

            //AllocateStatPoints(10);
            //AllocateStats();

            CharacterSetup setup = CombatManager.Instance.GetBaseCharacterSetup(className);
            if(setup != null) {
                GenerateSetupAttributes(setup);
                if(setup.optionalRole != CHARACTER_ROLE.NONE) {
                    AssignRole(setup.optionalRole);
                }
            }
        }
        public Character(CharacterSaveData data) : this(){
            _id = Utilities.SetID(this, data.id);
            _characterClass = CharacterManager.Instance.classesDictionary[data.className].CreateNewCopy();
            _raceSetting = RaceManager.Instance.racesDictionary[data.race.ToString()].CreateNewCopy();
            _gender = data.gender;
            _name = data.name;
            //LoadRelationships(data.relationshipsData);
            _portraitSettings = data.portraitSettings;
            SetMorality(data.morality);
#if !WORLD_CREATION_TOOL
            GameObject portraitGO = UIManager.Instance.InstantiateUIObject(CharacterManager.Instance.characterPortraitPrefab.name, UIManager.Instance.characterPortraitsParent);
            _characterPortrait = portraitGO.GetComponent<CharacterPortrait>();
            _characterPortrait.GeneratePortrait(this, 36, true, data.role);
            portraitGO.SetActive(false);
#endif

            //_bodyParts = new List<BodyPart>(_raceSetting.bodyParts);
            //ConstructBodyPartDict(_raceSetting.bodyParts);
            //_skills = GetGeneralSkills();
            _skills = new List<Skill>();
            _skills.Add(_characterClass.skill);
            //_skills.AddRange (GetBodyPartSkills ());
            if (data.attributes != null) {
                AddAttributes(data.attributes);
            }
            //GenerateSetupTags(baseSetup);
            GenerateRaceAttributes();

            //AllocateStatPoints(10);
            //AllocateStats();

            //EquipPreEquippedItems(baseSetup);
            CharacterSetup setup = CombatManager.Instance.GetBaseCharacterSetup(data.className);
            if (setup != null) {
                GenerateSetupAttributes(setup);
                if (setup.optionalRole != CHARACTER_ROLE.NONE) {
                    AssignRole(setup.optionalRole);
                }
            }
            //DetermineAllowedMiscActions();
        }
        public Character() {
            _attributes = new List<Attribute>();
            _exploredLandmarks = new List<BaseLandmark>();
            _statusEffects = new List<STATUS_EFFECT>();
            _secrets = new List<Secret>();
            _intelReactions = new Dictionary<int, GAME_EVENT>();
            _isDead = false;
            _isFainted = false;
            //_isDefeated = false;
            //_isIdle = false;
            _traceInfo = new Dictionary<Character, List<string>>();
            _history = new List<Log>();
            //_questData = new List<CharacterQuestData>();
            _actionQueue = new CharacterActionQueue<ActionQueueItem>(this);
            //previousActions = new Dictionary<CharacterTask, string>();
            _relationships = new Dictionary<Character, Relationship>();
            _genericWorkAction = ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.WORKING);
            _combatAttributes = new List<CombatAttribute>();
            
            //_actionData = new ActionData(this);


            //RPG
            _level = 1;
            _experience = 0;
            _elementalWeaknesses = new Dictionary<ELEMENT, float>(CharacterManager.Instance.elementsChanceDictionary);
            _elementalResistances = new Dictionary<ELEMENT, float>(CharacterManager.Instance.elementsChanceDictionary);
            _battleTracker = new CharacterBattleTracker();
            _battleOnlyTracker = new CharacterBattleOnlyTracker();
            //_equippedItems = new List<Item>();
            _inventory = new List<Item>();
            combatHistory = new Dictionary<int, Combat>();
            eventSchedule = new CharacterEventSchedule(this);
            uiData = new CharacterUIData();
            AllocateStats();
            EquipItemsByClass();
            //ConstructBuffs();
            GetRandomCharacterColor();
            ConstructDefaultMiscActions();
            //_combatHistoryID = 0;
            SubscribeToSignals();
        }
        public void Initialize() { }

        #region Signals
        private void SubscribeToSignals() {
            //Messenger.AddListener<ECS.Character>(Signals.CHARACTER_SNATCHED, OnCharacterSnatched);
            Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnOtherCharacterDied);
            //Messenger.AddListener(Signals.HOUR_ENDED, EverydayAction);
            //Messenger.AddListener<StructureObj, int>("CiviliansDeath", CiviliansDiedReduceSanity);
            Messenger.AddListener<ECS.Character>(Signals.CHARACTER_REMOVED, RemoveRelationshipWith);
            //Messenger.AddListener<Area>(Signals.AREA_DELETED, OnAreaDeleted);
            Messenger.AddListener<BaseLandmark>(Signals.DESTROY_LANDMARK, OnDestroyLandmark);
            //Messenger.AddListener<ECS.Character>(Signals.CHARACTER_DEATH, RemoveRelationshipWith);
        }
        public void UnsubscribeSignals() {
            //Messenger.RemoveListener<ECS.Character>(Signals.CHARACTER_SNATCHED, OnCharacterSnatched);
            Messenger.RemoveListener<Character>(Signals.CHARACTER_DEATH, OnOtherCharacterDied);
            //Messenger.RemoveListener(Signals.HOUR_ENDED, EverydayAction);
            //Messenger.RemoveListener<StructureObj, int>("CiviliansDeath", CiviliansDiedReduceSanity);
            Messenger.RemoveListener<ECS.Character>(Signals.CHARACTER_REMOVED, RemoveRelationshipWith);
            //Messenger.RemoveListener<Area>(Signals.AREA_DELETED, OnAreaDeleted);
            Messenger.RemoveListener<BaseLandmark>(Signals.DESTROY_LANDMARK, OnDestroyLandmark);
            //Messenger.RemoveListener<ECS.Character>(Signals.CHARACTER_DEATH, RemoveRelationshipWith);
            if (Messenger.eventTable.ContainsKey(Signals.HOUR_ENDED)) {
                Messenger.RemoveListener(Signals.HOUR_ENDED, EventEveryTick);
            }
        }
        #endregion

        private void AllocateStats() {
            _attackPower = _raceSetting.baseAttackPower;
            _speed = _raceSetting.baseSpeed;
            SetMaxHP(_raceSetting.baseHP);
            _maxSP = _characterClass.baseSP;
        }
        //      private void AllocateStatPoints(int statAllocation){
        //          _baseStrength = 0;
        //          _baseIntelligence = 0;
        //          _baseAgility = 0;
        //          _baseVitality = 0;

        //	WeightedDictionary<string> statWeights = new WeightedDictionary<string> ();
        //	statWeights.AddElement ("strength", (int) _characterClass.strWeightAllocation);
        //	statWeights.AddElement ("intelligence", (int) _characterClass.intWeightAllocation);
        //	statWeights.AddElement ("agility", (int) _characterClass.agiWeightAllocation);
        //	statWeights.AddElement ("vitality", (int) _characterClass.vitWeightAllocation);

        //	if(statWeights.GetTotalOfWeights() > 0){
        //		string chosenStat = string.Empty;
        //		for (int i = 0; i < statAllocation; i++) {
        //			chosenStat = statWeights.PickRandomElementGivenWeights ();
        //			if (chosenStat == "strength") {
        //				_baseStrength += 1;
        //			}else if (chosenStat == "intelligence") {
        //				_baseIntelligence += 1;
        //			}else if (chosenStat == "agility") {
        //				_baseAgility += 1;
        //			}else if (chosenStat == "vitality") {
        //				_baseVitality += 1;
        //			}
        //		}
        //	}
        //}
        //Enables or Disables skills based on skill requirements
        public void EnableDisableSkills(Combat combat){
			//bool isAllAttacksInRange = true;
			//bool isAttackInRange = false;

            //Body part skills / general skills
			for (int i = 0; i < this._skills.Count; i++) {
				Skill skill = this._skills [i];
				skill.isEnabled = true;

    //            if (skill is AttackSkill){
    //                AttackSkill attackSkill = skill as AttackSkill;
    //                if(attackSkill.spCost > _sp) {
    //                    skill.isEnabled = false;
    //                    continue;
    //                }
				//} else 
                if (skill is FleeSkill) {
                    skill.isEnabled = false;
                    //if (this.currentHP >= (this.maxHP / 2)) {
                    //    skill.isEnabled = false;
                    //    continue;
                    //}
                }
            }

            //Character class skills
            //if(_equippedWeapon != null) {
            //    for (int i = 0; i < _level; i++) {
            //        if(i < _characterClass.skillsPerLevel.Count) {
            //            if (_characterClass.skillsPerLevel[i] != null) {
            //                for (int j = 0; j < _characterClass.skillsPerLevel[i].Length; j++) {
            //                    Skill skill = _characterClass.skillsPerLevel[i][j];
            //                    skill.isEnabled = true;

            //                    //Check for allowed weapon types
            //                    if (skill.allowedWeaponTypes != null) {
            //                        for (int k = 0; k < skill.allowedWeaponTypes.Length; k++) {
            //                            if (!skill.allowedWeaponTypes.Contains(_equippedWeapon.weaponType)) {
            //                                skill.isEnabled = false;
            //                                continue;
            //                            }
            //                        }
            //                    }

            //                    if (skill is AttackSkill) {
            //                        AttackSkill attackSkill = skill as AttackSkill;
            //                        if (attackSkill.spCost > _sp) {
            //                            skill.isEnabled = false;
            //                            continue;
            //                        }
            //                    }
            //                }
            //            }
            //        } else {
            //            break;
            //        }
            //    }

            //}

		}
        //Changes row number of this character
        public void SetRowNumber(int rowNumber){
			this._currentRow = rowNumber;
		}
		//Changes character's side
		public void SetSide(SIDES side){
			this._currentSide = side;
		}
        //Adjust current HP based on specified paramater, but HP must not go below 0
        public virtual void AdjustHP(int amount, ICharacter killer = null) {
            int previous = this._currentHP;
			this._currentHP += amount;
			this._currentHP = Mathf.Clamp(this._currentHP, 0, maxHP);
            if(previous != this._currentHP) {
                if (this._currentHP == 0) {
                    FaintOrDeath(killer);
                }
            }
		}
		public void SetHP(int amount){
			this._currentHP = amount;
		}
		private string GetFaintOrDeath(){
            return "die";
			//WeightedDictionary<string> faintDieDict = new WeightedDictionary<string> ();
			//int faintWeight = 100;
			//int dieWeight = 50;
			//if(HasTrait(TRAIT.GRITTY)){
			//	faintWeight += 50;
			//}
			//if(HasTrait(TRAIT.ROBUST)){
			//	faintWeight += 50;
			//}
			//if(HasTrait(TRAIT.FRAGILE)){
			//	dieWeight += 50;
			//}
			//faintDieDict.AddElement ("faint", 100);
			//faintDieDict.AddElement ("die", 50);

			//return faintDieDict.PickRandomElementGivenWeights ();
		}
		public void FaintOrDeath(ICharacter killer){
			string pickedWeight = GetFaintOrDeath ();
			if(pickedWeight == "faint"){
				if(currentParty.currentCombat == null){
					Faint ();
				}else{
                    currentParty.currentCombat.CharacterFainted(this);
                }
			}else if(pickedWeight == "die"){
                if (currentParty.currentCombat != null) {
                    currentParty.currentCombat.CharacterDeath(this, killer);
                }
                Death();
    //            if (this.currentCombat == null){
				//	Death ();
				//}else{
				//	this.currentCombat.CharacterDeath (this);
				//}
			}
		}
		//When character will faint
		internal void Faint(){
			if(!_isFainted){
				_isFainted = true;
				SetHP (1);
                ////Set Task to Fainted
                //Faint faintTask = new Faint(this);
                //faintTask.OnChooseTask(this)
;			}
		}
		internal void Unfaint(){
			if (_isFainted) {
				_isFainted = false;
				SetHP (1);
			}
		}
        public void Imprison() {
            //if(_ownParty.icharacters.Count > 1) {
            //    CreateOwnParty();
            //}
            if(_ownParty.characterObject.currentState.stateName != "Imprisoned") {
                ObjectState imprisonedState = _ownParty.characterObject.GetState("Imprisoned");
                _ownParty.characterObject.ChangeState(imprisonedState);

                _ownParty.SetIsIdle(true); //this makes the character not do any action, and needs are halted
                //Do other things when imprisoned
            }
        }
		//Character's death
		public void Death(){
			if(!_isDead){
				_isDead = true;
                UnsubscribeSignals();

                CombatManager.Instance.ReturnCharacterColorToPool (_characterColor);

                if (currentParty.specificLocation == null) {
                    throw new Exception("Specific location of " + this.name + " is null! Please use command /l_character_location_history [Character Name/ID] in console menu to log character's location history. (Use '~' to show console menu)");
                }

				if(currentParty.specificLocation != null && currentParty.specificLocation.locIdentifier == LOCATION_IDENTIFIER.LANDMARK){
                    Log deathLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "death");
                    deathLog.AddToFillers(this, this.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    AddHistory(deathLog);
                    (currentParty.specificLocation as BaseLandmark).AddHistory(deathLog);
				}
                
                //Drop all Items
    //            while (_equippedItems.Count > 0) {
				//	ThrowItem (_equippedItems [0]);
				//}
				while (_inventory.Count > 0) {
					ThrowItem (_inventory [0]);
				}

                if(IsInOwnParty()) {
                    if (_ownParty.actionData.currentAction != null) {
                        _ownParty.actionData.currentAction.EndAction(_ownParty, _ownParty.actionData.currentTargetObject);
                    }
                }
                _currentParty.RemoveCharacter(this);
                _ownParty.PartyDeath();
                //if (currentParty.id != _ownParty.id) {
                //}
                //_ownParty.PartyDeath();
                //Remove ActionData
                //_actionData.DetachActionData();

                //if(_home != null){
                //                //Remove character home on landmark
                //	_home.RemoveCharacterHomeOnLandmark (this);
                //}

                if (this._faction != null){
					this._faction.RemoveCharacter(this); //remove this character from it's factions list of characters
				}

                //if (_specificLocation != null) {
                //    _specificLocation.RemoveCharacterFromLocation(this);
                //}
                //if (_avatar != null) {
                //    if (_avatar.mainCharacter.id == this.id) {
                //        DestroyAvatar();
                //    } else {
                //        _avatar.RemoveCharacter(this); //if the character has an avatar, remove it from the list of characters
                //    }
                //}
                //if (_isPrisoner){
                //	PrisonerDeath ();
                //}
                if (_role != null){
					_role.DeathRole ();
				}
                if(_homeLandmark != null) {
                    _homeLandmark.RemoveCharacterHomeOnLandmark(this);
                }
                //while(_tags.Count > 0){
                //	RemoveCharacterAttribute (_tags [0]);
                //}
                //while (questData.Count != 0) {
                //    questData[0].AbandonQuest();
                //}
                //				if(Messenger.eventTable.ContainsKey("CharacterDeath")){
                //					Messenger.Broadcast ("CharacterDeath", this);
                //				}
                if (schedule != null) {
                    schedule.OnOwnerDied();
                }
                if (onCharacterDeath != null){
					onCharacterDeath();
				}
                onCharacterDeath = null;
                Messenger.Broadcast(Signals.CHARACTER_DEATH, this);
                //if (killer != null) {
                //    Messenger.Broadcast(Signals.CHARACTER_KILLED, killer, this);
                //}
                if(_characterPortrait != null) {
                    GameObject.Destroy(_characterPortrait.gameObject);
                    _characterPortrait = null;
                }
                //ObjectState deadState = _characterObject.GetState("Dead");
                //_characterObject.ChangeState(deadState);

                //GameObject.Destroy(_icon.gameObject);
                //_icon = null;

                Debug.Log(this.name + " died!");
            }
		}
        internal void AddActionOnDeath(OnCharacterDeath onDeathAction) {
            onCharacterDeath += onDeathAction;
        }
        internal void RemoveActionOnDeath(OnCharacterDeath onDeathAction) {
            onCharacterDeath -= onDeathAction;
        }

        #region Items
		//If a character picks up an item, it is automatically added to his/her inventory
		internal void PickupItem(Item item, bool broadcast = true){
			Item newItem = item;
            if (_inventory.Contains(newItem)) {
                throw new Exception(this.name + " already has an instance of " + newItem.itemName);
            }
			this._inventory.Add (newItem);
			//newItem.SetPossessor (this);
			if(newItem.owner == null){
				OwnItem (newItem);
			}
#if !WORLD_CREATION_TOOL
            Log obtainLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "obtain_item");
            obtainLog.AddToFillers(this, this.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            obtainLog.AddToFillers(null, item.itemName, LOG_IDENTIFIER.ITEM_1);
            AddHistory(obtainLog);
			if (_ownParty.specificLocation.locIdentifier == LOCATION_IDENTIFIER.LANDMARK) {
                (_ownParty.specificLocation as BaseLandmark).AddHistory(obtainLog);
            }
#endif
            if (broadcast) {
                Messenger.Broadcast(Signals.ITEM_OBTAINED, newItem, this);
            }
            newItem.OnItemPutInInventory(this);
        }
		internal void ThrowItem(Item item, bool addInLandmark = true){
			if(item.isEquipped){
				UnequipItem (item);
			}
			//item.SetPossessor (null);
			this._inventory.Remove (item);
			//item.exploreWeight = 15;
			if(addInLandmark){
				ILocation location = _ownParty.specificLocation;
				if(location != null && location.locIdentifier == LOCATION_IDENTIFIER.LANDMARK){
					BaseLandmark landmark = location as BaseLandmark;
					landmark.AddItem(item);
				}
			}
            Messenger.Broadcast(Signals.ITEM_THROWN, item, this);
        }
        internal void ThrowItem(string itemName, int quantity, bool addInLandmark = true) {
            for (int i = 0; i < quantity; i++) {
                if (HasItem(itemName)) {
                    ThrowItem(GetItemInInventory(itemName), addInLandmark);
                }
            }
        }
        internal void DropItem(Item item) {
            ThrowItem(item);
            ILocation location = _ownParty.specificLocation;
			if (location != null && location.locIdentifier == LOCATION_IDENTIFIER.LANDMARK) {
				//BaseLandmark landmark = location as BaseLandmark;
                Log dropLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "drop_item");
                dropLog.AddToFillers(this, this.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                dropLog.AddToFillers(null, item.itemName, LOG_IDENTIFIER.ITEM_1);
                dropLog.AddToFillers(location, location.locationName, LOG_IDENTIFIER.LANDMARK_1);
                AddHistory(dropLog);
                (location as BaseLandmark).AddHistory(dropLog);
            }

        }
        internal void CheckForItemDrop() {
            ILocation location = _ownParty.specificLocation;
            if (location != null && location.locIdentifier == LOCATION_IDENTIFIER.LANDMARK) {
                if (UnityEngine.Random.Range(0, 100) < 3) {
                    Dictionary<Item, Character> itemPool = new Dictionary<Item, Character>();
                    List<Character> charactersToCheck = new List<Character>();
                    charactersToCheck.Add(this);

                    for (int i = 0; i < charactersToCheck.Count; i++) {
                        ECS.Character currCharacter = charactersToCheck[i];
                        for (int j = 0; j < currCharacter.inventory.Count; j++) {
                            itemPool.Add(currCharacter.inventory[j], currCharacter);
                        }
                    }

                    //Drop a random item from the pool
                    if (itemPool.Count > 0) {
                        Item chosenItem = itemPool.Keys.ElementAt(UnityEngine.Random.Range(0, itemPool.Count));
                        Character owner = itemPool[chosenItem];
                        owner.DropItem(chosenItem);
                    }
                }
            }
        }
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
        internal bool EquipItem(Item item){
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
                        CombatAttribute newCombatAttribute = AttributeManager.Instance.allCombatAttributes[item.attributeNames[i]];
                        AddCombatAttribute(newCombatAttribute);
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
		public void UnequipItem(Item item){
			if (item.itemType == ITEM_TYPE.WEAPON) {
                UnequipWeapon(item as Weapon);
			} else if (item.itemType == ITEM_TYPE.ARMOR) {
                UnequipArmor(item as Armor);
            } else if (item.itemType == ITEM_TYPE.ACCESSORY) {
                UnequipAccessory(item);
            } else if (item.itemType == ITEM_TYPE.CONSUMABLE) {
                UnequipConsumable(item);
            }
            if(item.attributeNames != null) {
                for (int i = 0; i < item.attributeNames.Count; i++) {
                    CombatAttribute newCombatAttribute = AttributeManager.Instance.allCombatAttributes[item.attributeNames[i]];
                    RemoveCombatAttribute(newCombatAttribute);
                }
            }
            Messenger.Broadcast(Signals.ITEM_UNEQUIPPED, item, this);
        }
		//Own an Item
		internal void OwnItem(Item item){
			item.SetOwner (this);
		}
		//Transfer item ownership
		internal void TransferItemOwnership(Item item, Character newOwner){
			newOwner.OwnItem (item);
		}
		//Try to equip a weapon to a body part of this character and add it to the list of items this character have
		internal bool TryEquipWeapon(Weapon weapon){
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
		internal bool TryEquipArmor(Armor armor){
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
            if(_equippedWeapon != null && _equippedWeapon.itemName == itemName) {
                return true;
            } else if (_equippedArmor != null && _equippedArmor.itemName == itemName) {
                return true;
            } else if (_equippedAccessory != null && _equippedAccessory.itemName == itemName) {
                return true;
            } else if (_equippedConsumable != null && _equippedConsumable.itemName == itemName) {
                return true;
            }
            for (int i = 0; i < _inventory.Count; i++) {
                Item currItem = _inventory[i];
                if (currItem.itemName.Equals(itemName)) {
                    return true;
                }
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
            if (inventory.Contains(item)) {
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
            for (int i = 0; i < _inventory.Count; i++) {
                Item currItem = _inventory[i];
                if (currItem.itemName.Contains(itemName)) {
                    counter++;
                }
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
            for (int i = 0; i < _inventory.Count; i++) {
                Item currItem = _inventory[i];
                if (currItem.itemName.Contains(itemName)) {
                    items.Add(currItem);
                }
            }
            return items;
        }
        internal Item GetItemInInventory(string itemName){
			for (int i = 0; i < _inventory.Count; i++) {
				Item currItem = _inventory[i];
				if (currItem.itemName.Equals(itemName)) {
					return currItem;
				}
			}
			return null;
		}
        public void GiveItemsTo(List<Item> items, Character otherCharacter) {
            for (int i = 0; i < items.Count; i++) {
                Item currItem = items[i];
                if (this.HasItem(currItem)) { //check if the character still has the item that he wants to give
                    this.ThrowItem(currItem, false);
                    otherCharacter.PickupItem(currItem);
                    Debug.Log(this.name + " gave item " + currItem.itemName + " to " + otherCharacter.name);
                }
            }
        }
        private void EquipItemsByClass() {
            if(_characterClass != null) {
                if(_characterClass.weaponTierNames != null && _characterClass.weaponTierNames.Count > 0) {
                    EquipItem(_characterClass.weaponTierNames[0]);
                }
                if (_characterClass.armorTierNames != null && _characterClass.armorTierNames.Count > 0) {
                    EquipItem(_characterClass.armorTierNames[0]);
                }
                if (_characterClass.accessoryTierNames != null && _characterClass.accessoryTierNames.Count > 0) {
                    EquipItem(_characterClass.accessoryTierNames[0]);
                }
            }
        }
        public void UpgradeWeapon() {
            if (_characterClass != null && _equippedWeapon != null) {
                if (_characterClass.weaponTierNames != null && _characterClass.weaponTierNames.Count > 0) {
                    bool foundEquipped = false;
                    for (int i = 0; i < _characterClass.weaponTierNames.Count; i++) {
                        if (foundEquipped) {
                            //Found equipped item, now equip next on the list for upgrade
                            EquipItem(_characterClass.weaponTierNames[i]);
                            break;
                        } else {
                            if (_equippedWeapon.itemName == _characterClass.weaponTierNames[i]) {
                                foundEquipped = true;
                            }
                        }
                    }
                }
            }
        }
        public void UpgradeArmor() {
            if (_characterClass != null && _equippedArmor != null) {
                if (_characterClass.armorTierNames != null && _characterClass.armorTierNames.Count > 0) {
                    bool foundEquipped = false;
                    for (int i = 0; i < _characterClass.armorTierNames.Count; i++) {
                        if (foundEquipped) {
                            //Found equipped item, now equip next on the list for upgrade
                            EquipItem(_characterClass.armorTierNames[i]);
                            break;
                        } else {
                            if (_equippedArmor.itemName == _characterClass.armorTierNames[i]) {
                                foundEquipped = true;
                            }
                        }
                    }
                }
            }
        }
        public void UpgradeAccessory() {
            if (_characterClass != null && _equippedAccessory != null) {
                if (_characterClass.accessoryTierNames != null && _characterClass.accessoryTierNames.Count > 0) {
                    bool foundEquipped = false;
                    for (int i = 0; i < _characterClass.accessoryTierNames.Count; i++) {
                        if (foundEquipped) {
                            //Found equipped weapon, now equip next on the list for upgrade
                            EquipItem(_characterClass.accessoryTierNames[i]);
                            break;
                        } else {
                            if (_equippedAccessory.itemName == _characterClass.accessoryTierNames[i]) {
                                foundEquipped = true;
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region Status Effects
        internal void AddStatusEffect(STATUS_EFFECT statusEffect){
			this._statusEffects.Add (statusEffect);
		}
		internal void RemoveStatusEffect(STATUS_EFFECT statusEffect){
			this._statusEffects.Remove (statusEffect);
		}
        internal void CureStatusEffects(){
			for (int i = 0; i < _statusEffects.Count; i++) {
				STATUS_EFFECT statusEffect = _statusEffects[i];
				int chance = Utilities.rng.Next (0, 100);
				if (chance < 15) {
                    _ownParty.currentCombat.AddCombatLog(this.name + " is cured from " + statusEffect.ToString ().ToLower () + ".", this.currentSide);
					RemoveStatusEffect (statusEffect);
					i--;
				}
			}
		}
        internal bool HasStatusEffect(STATUS_EFFECT statusEffect) {
			if (_statusEffects.Contains(statusEffect)) {
				return true;
			}
            return false;
        }
        #endregion

        #region Skills
		private List<Skill> GetGeneralSkills(){
            List<Skill> allGeneralSkills = new List<Skill>();
            foreach (Skill skill in SkillManager.Instance.generalSkills.Values) {
                allGeneralSkills.Add(skill.CreateNewCopy());
            }
            return allGeneralSkills;
		}
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
        public void AssignRole(CHARACTER_ROLE role) {
            bool wasRoleChanged = false;
			if(_role != null){
				_role.ChangedRole ();
#if !WORLD_CREATION_TOOL
                Log roleChangeLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "change_role");
                roleChangeLog.AddToFillers(this, this.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                AddHistory(roleChangeLog);
#endif
                wasRoleChanged = true;
            }
			switch (role) {
		        case CHARACTER_ROLE.HERO:
			        _role = new Hero(this);
			        break;
		        case CHARACTER_ROLE.VILLAIN:
			        _role = new Villain(this);
			        break;
                case CHARACTER_ROLE.CIVILIAN:
                    _role = new Civilian(this);
                    break;
                case CHARACTER_ROLE.KING:
                    _role = new King(this);
                    break;
                case CHARACTER_ROLE.PLAYER:
                    _role = new PlayerRole(this);
                    break;
                case CHARACTER_ROLE.GUARDIAN:
                    _role = new Guardian(this);
                    break;
                default:
		            break;
			}
            if (_role != null) {
                _role.OnAssignRole();
            }
            if (wasRoleChanged) {
                Messenger.Broadcast(Signals.ROLE_CHANGED, this);
            }
		}
		public void ChangeRole(){
			//TODO: Things to do when a character changes role

//			AssignRole(role);
			if(_raceSetting.tags.Contains(ATTRIBUTE.SAPIENT)){
				CHARACTER_ROLE roleToCreate = CHARACTER_ROLE.HERO;
				WeightedDictionary<CHARACTER_ROLE> characterRoleProductionDictionary = LandmarkManager.Instance.GetCharacterRoleProductionDictionary();
				if (characterRoleProductionDictionary.GetTotalOfWeights () > 0) {
					roleToCreate = characterRoleProductionDictionary.PickRandomElementGivenWeights ();
				}
				AssignRole(roleToCreate);
			}
   //         else{
			//	AssignRole (CHARACTER_ROLE.TAMED_BEAST);
			//}
		}
        #endregion

        #region Character Class
		public void AssignClass(CharacterClass charClass) {
			_characterClass = charClass;
		}
        #endregion

        #region Character Tags
        //public void AssignInitialAttributes() {
        //    int tagChance = UnityEngine.Random.Range(0, 100);
        //    ATTRIBUTE[] initialTags = (ATTRIBUTE[])System.Enum.GetValues(typeof(ATTRIBUTE));
        //    for (int j = 0; j < initialTags.Length; j++) {
        //        ATTRIBUTE tag = initialTags[j];
        //        if (tagChance < Utilities.GetTagWorldGenChance(tag)) {
        //            AddAttribute(tag);
        //        }
        //    }
        //}
  //      public CharacterAttribute AddAttribute(ATTRIBUTE tag) {
		//	if(HasAttribute(tag)){
		//		return null;
		//	}
		//	CharacterAttribute charTag = null;
		//	switch (tag) {
  //          case ATTRIBUTE.HUNGRY:
  //              charTag = new Hungry(this);
  //              break;
  //          case ATTRIBUTE.FAMISHED:
  //              charTag = new Famished(this);
  //              break;
  //          case ATTRIBUTE.TIRED:
  //              charTag = new Tired(this);
  //              break;
  //          case ATTRIBUTE.EXHAUSTED:
  //              charTag = new Exhausted(this);
  //              break;
  //          case ATTRIBUTE.SAD:
  //              charTag = new Sad(this);
  //              break;
  //          case ATTRIBUTE.DEPRESSED:
  //              charTag = new Depressed(this);
  //              break;
  //          case ATTRIBUTE.ANXIOUS:
  //              charTag = new Anxious(this);
  //              break;
  //          case ATTRIBUTE.INSECURE:
  //              charTag = new Insecure(this);
  //              break;
  //          case ATTRIBUTE.DRUNK:
  //              charTag = new Drunk(this);
  //              break;
  //          case ATTRIBUTE.DISTURBED:
  //              charTag = new Disturbed(this);
  //              break;
  //          case ATTRIBUTE.CRAZED:
  //              charTag = new Crazed(this);
  //              break;
  //          case ATTRIBUTE.DEMORALIZED:
  //              charTag = new Demoralized(this);
  //              break;
  //          case ATTRIBUTE.STARVING:
  //              charTag = new Starving(this);
  //              break;
  //          case ATTRIBUTE.WOUNDED:
  //              charTag = new Wounded(this);
  //              break;
  //          case ATTRIBUTE.WRECKED:
  //              charTag = new Wrecked(this);
  //              break;
  //          case ATTRIBUTE.IMPULSIVE:
  //              charTag = new Impulsive(this);
  //              break;
  //          case ATTRIBUTE.BETRAYED:
  //              charTag = new Betrayed(this);
  //              break;
  //          case ATTRIBUTE.HEARTBROKEN:
  //              charTag = new Heartbroken(this);
  //              break;
  //          }
		//	if(charTag != null){
		//		AddCharacterAttribute (charTag);
		//	}
  //          return charTag;
		//}
        public Attribute CreateAttribute(ATTRIBUTE type) {
            switch (type) {
                case ATTRIBUTE.GREGARIOUS:
                return new Gregarious();
                case ATTRIBUTE.BOOKWORM:
                return new Bookworm();
                case ATTRIBUTE.SINGER:
                return new Singer();
                case ATTRIBUTE.DAYDREAMER:
                return new Daydreamer();
                case ATTRIBUTE.MEDITATOR:
                return new Meditator();
                case ATTRIBUTE.CLEANER:
                return new Cleaner();
                case ATTRIBUTE.INTROVERT:
                return new Introvert();
                case ATTRIBUTE.EXTROVERT:
                return new Extrovert();
                case ATTRIBUTE.BELLIGERENT:
                return new Belligerent();
                case ATTRIBUTE.LIBERATED:
                return new Liberated();
                case ATTRIBUTE.UNFAITHFUL:
                return new Unfaithful();
                case ATTRIBUTE.DEAFENED:
                return new Deafened();
                case ATTRIBUTE.MUTE:
                return new Mute();
                case ATTRIBUTE.ROYALTY:
                return new Royalty();
                case ATTRIBUTE.STALKER:
                return new Stalker();
                case ATTRIBUTE.SPOOKED:
                return new Spooked();
                case ATTRIBUTE.HUMAN:
                    return new Human();
                case ATTRIBUTE.HUNGRY:
                    return new Hungry();
                case ATTRIBUTE.FAMISHED:
                    return new Famished();
                case ATTRIBUTE.TIRED:
                    return new Tired();
                case ATTRIBUTE.EXHAUSTED:
                    return new Exhausted();
                case ATTRIBUTE.SAD:
                    return new Sad();
                case ATTRIBUTE.DEPRESSED:
                    return new Depressed();
                case ATTRIBUTE.ANXIOUS:
                    return new Anxious();
                case ATTRIBUTE.INSECURE:
                    return new Insecure();
                case ATTRIBUTE.DRUNK:
                    return new Drunk();
                case ATTRIBUTE.DISTURBED:
                    return new Disturbed();
                case ATTRIBUTE.CRAZED:
                    return new Crazed();
                case ATTRIBUTE.DEMORALIZED:
                    return new Demoralized();
                case ATTRIBUTE.STARVING:
                    return new Starving();
                case ATTRIBUTE.WOUNDED:
                    return new Wounded();
                case ATTRIBUTE.WRECKED:
                    return new Wrecked();
                case ATTRIBUTE.IMPULSIVE:
                    return new Impulsive();
                case ATTRIBUTE.BETRAYED:
                    return new Betrayed();
                case ATTRIBUTE.HEARTBROKEN:
                    return new Heartbroken();
                case ATTRIBUTE.MARKED:
                return new Marked();
            }
            return null;
        }
        private void GenerateRaceAttributes(){
			for (int i = 0; i < _raceSetting.tags.Count; i++) {
				AddAttribute (_raceSetting.tags [i]);
			}
		}
        private void GenerateSetupAttributes(CharacterSetup setup) {
            for (int i = 0; i < setup.tags.Count; i++) {
                AddAttribute(setup.tags[i]);
            }
        }
        public bool HasAttributes(ATTRIBUTE[] tagsToHave, bool mustHaveAll = false){
			return DoesHaveAttributes (this, tagsToHave, mustHaveAll);
		}
		private bool DoesHaveAttributes(Character currCharacter, ATTRIBUTE[] tagsToHave, bool mustHaveAll = false){
			if(mustHaveAll){
				int tagsCount = 0;
				for (int i = 0; i < currCharacter.attributes.Count; i++) {
					for (int j = 0; j < tagsToHave.Length; j++) {
						if(tagsToHave[j] == currCharacter.attributes[i].attribute) {
							tagsCount++;
							break;
						}
					}
					if(tagsCount >= tagsToHave.Length){
						return true;
					}
				}
			}else{
				for (int i = 0; i < currCharacter.attributes.Count; i++) {
					for (int j = 0; j < tagsToHave.Length; j++) {
						if(tagsToHave[j] == currCharacter.attributes[i].attribute) {
							return true;
						}
					}
				}
			}
			return false;
		}
        public Attribute GetAttribute(ATTRIBUTE attribute) {
            for (int i = 0; i < _attributes.Count; i++) {
                if (_attributes[i].attribute == attribute) {
                    return _attributes[i];
                }
            }
            return null;
        }
        public Attribute GetAttribute(string attribute) {
            for (int i = 0; i < _attributes.Count; i++) {
                if (_attributes[i].name.ToLower() == attribute.ToLower()) {
                    return _attributes[i];
                }
            }
            return null;
        }
        public Attribute AddAttribute(ATTRIBUTE attribute) {
            if(GetAttribute(attribute) == null) {
                Attribute newAttribute = CreateAttribute(attribute);
                _attributes.Add(newAttribute);
#if !WORLD_CREATION_TOOL
                newAttribute.OnAddAttribute(this);
#endif
                Messenger.Broadcast<Character, Attribute>(Signals.ATTRIBUTE_ADDED, this, newAttribute);
                return newAttribute;
            }
            return null;
        }
        public void AddAttributes(List<ATTRIBUTE> attributes) {
            for (int i = 0; i < attributes.Count; i++) {
                AddAttribute(attributes[i]);
            }
        }
        public void RemoveAttributes(List<ATTRIBUTE> attributes) {
            for (int i = 0; i < attributes.Count; i++) {
                RemoveAttribute(attributes[i]);
            }
        }
        public bool RemoveAttribute(ATTRIBUTE attribute) {
            for (int i = 0; i < _attributes.Count; i++) {
                if(_attributes[i].attribute == attribute) {
                    Attribute removedAttribute = _attributes[i];
#if !WORLD_CREATION_TOOL
                    removedAttribute.OnRemoveAttribute();
#endif
                    _attributes.RemoveAt(i);
                    Messenger.Broadcast<Character, Attribute>(Signals.ATTRIBUTE_REMOVED, this, removedAttribute);
                    return true;
                }
            }
            return false;
        }
        public bool RemoveAttribute(Attribute attribute) {
            Attribute attributeToBeRemoved = attribute;
            if (_attributes.Remove(attribute)) {
#if !WORLD_CREATION_TOOL
                attributeToBeRemoved.OnRemoveAttribute();
#endif
                Messenger.Broadcast<Character, Attribute>(Signals.ATTRIBUTE_REMOVED, this, attributeToBeRemoved);
                return true;
            }
            return false;
        }
        #endregion

        #region Faction
        public void SetFaction(Faction faction) {
			_faction = faction;
            Messenger.Broadcast<Character>(Signals.FACTION_SET, this);
		}
        #endregion

        #region Party
        /*
         Create a new Party with this character as the leader.
             */
        public virtual Party CreateOwnParty() {
            if(_ownParty != null) {
                _ownParty.RemoveCharacter(this);
            }
            CharacterParty newParty = new CharacterParty(this);
            SetOwnedParty(newParty);
            newParty.AddCharacter(this);
            newParty.CreateCharacterObject();
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
            if (ownParty is CharacterParty) {
                (ownParty as CharacterParty).actionData.currentAction.EndAction(ownParty, (ownParty as CharacterParty).actionData.currentTargetObject);
            }
        }
        public void OnAddedToParty() {
            if (currentParty.id != ownParty.id) {
                if (ownParty.specificLocation is BaseLandmark) {
                    ownParty.specificLocation.RemoveCharacterFromLocation(ownParty);
                }
                ownParty.icon.SetVisualState(false);
            }
        }
        public bool IsInParty() {
            if (currentParty.icharacters.Count > 1) {
                return true; //if the character is in a party that has more than 1 characters
            }
            return false;
        }
        public bool IsInOwnParty() {
            if (currentParty.id == ownParty.id) {
                return true;
            }
            return false;
        }
        public bool InviteToParty(ICharacter inviter) {
            if (IsInParty()) {
                return false;
            }
            if (party.isIdle) {
                return false;
            }
            if (this.party.actionData.currentAction == null || this.party.actionData.currentAction.actionData.actionType == ACTION_TYPE.WAIT_FOR_PARTY) {
                //accept invitation
                //this.actionQueue.Clear();
                this.party.actionData.ForceDoAction(inviter.ownParty.icharacterObject.currentState.GetAction(ACTION_TYPE.JOIN_PARTY), inviter.ownParty.icharacterObject);
                return true;
            }
            return false;
        }
        #endregion

        #region Location
		public bool IsCharacterInAdjacentRegionOfThis(Character targetCharacter){
			for (int i = 0; i < _currentRegion.adjacentRegionsViaRoad.Count; i++) {
				if(targetCharacter.party.currentRegion.id == _currentRegion.adjacentRegionsViaRoad[i].id){
					return true;
				}
			}
			return false;
		}
        #endregion

        #region Quests
        //public void AddQuestData(CharacterQuestData questData) {
        //    if (!_questData.Contains(questData)) {
        //        _questData.Add(questData);
        //    }
        //}
        //public void RemoveQuestData(CharacterQuestData questData) {
        //    _questData.Remove(questData);
        //}
        //public bool HasQuest(Quest quest) {
        //    for (int i = 0; i < questData.Count; i++) {
        //        if (questData[i].parentQuest.id == quest.id) {
        //            return true;
        //        }
        //    }
        //    return false;
        //}
        //public CharacterQuestData GetDataForQuest(Quest quest) {
        //    for (int i = 0; i < questData.Count; i++) {
        //        CharacterQuestData data = questData[i];
        //        if (data.parentQuest.id == quest.id) {
        //            return data;
        //        }
        //    }
        //    return null;
        //}
        //public CharacterQuestData GetSquadDataForQuest(Quest quest) {
        //    //get quest data for quest that can come from a squad member
        //    if (IsSquadLeader() && quest.groupType == GROUP_TYPE.PARTY) {
        //        for (int i = 0; i < squad.squadMembers.Count; i++) {
        //            ICharacter currMember = squad.squadMembers[i];
        //            if (currMember is Character) {
        //                Character member = currMember as Character;
        //                CharacterQuestData questData = member.GetDataForQuest(quest);
        //                if (questData != null) {
        //                    return questData;
        //                }
        //            }
        //        }
        //    }
        //    return null;
        //}
        //public List<Quest> GetAcceptedQuestsByGroup(GROUP_TYPE groupType) {
        //    List<Quest> quests = new List<Quest>();
        //    for (int i = 0; i < questData.Count; i++) {
        //        CharacterQuestData data = questData[i];
        //        if (data.parentQuest.groupType == groupType) {
        //            quests.Add(data.parentQuest);
        //        }
        //    }
        //    return quests;
        //}
        //public void OnTakeQuest(Quest takenQuest) {
            //if (takenQuest.groupType == GROUP_TYPE.PARTY && this.squad == null) { //When a character gains a Party Type Quest and he isnt a part of a Squad yet,
            //    if (this.role == null) {
            //        return;
            //    }
            //    if (this.role.roleType == CHARACTER_ROLE.CIVILIAN) { //If he is a Civilian-type
            //        if (this.mentalPoints <= -6) {
            //            //if Mental Points is -6 or below, the character will request to chat with the Player and ask for his help
            //        } else if (this.HasTag(ATTRIBUTE.IMPULSIVE)) {
            //            //else, if character has impulsive trait, a change action to a randomized Hero class will be added at the end of his Action Queue.
            //            ChangeClassAction changeClassAction = ownParty.icharacterObject.currentState.GetAction(ACTION_TYPE.CHANGE_CLASS) as ChangeClassAction;
            //            string[] choices = new string[] { "Warrior" };
            //            changeClassAction.SetAdvertisedClass(choices[UnityEngine.Random.Range(0, choices.Length)]);
            //            AddActionToQueue(changeClassAction, ownParty.icharacterObject);
            //        } else {
            //            //else, character will advertise his Quest for other people to take
            //        }
            //    } else if (this.role.roleType == CHARACTER_ROLE.HERO) { //If he is a Hero-type
            //        if (this.mentalPoints <= -6) {
            //            //if Mental Points is -6 or below, the character will request to chat with the Player and ask for his help
            //        } else if (this.HasTag(ATTRIBUTE.IMPULSIVE)) {
            //            //else, if character has impulsive trait, he will attempt the quest without any other people's help
            //        } else {
            //            //else, character will create a new Squad and become its Squad Leader and will perform a Recruit Squadmates action
            //        }
            //    }
            //}
        //}
        public bool HasQuest() {
            return currentQuest != null;
        }
        public void SetQuest(Quest quest) {
            currentQuest = quest;
            if (currentQuest != null) {
                currentQuest.OnAcceptQuest(this);
                Debug.Log("Set " + this.name + "'s quest to " + currentQuest.name);
            }
        }
        #endregion

        #region HP
        public bool IsHealthFull() {
            return _currentHP >= maxHP;
        }
        #endregion

        #region Utilities
        public void ChangeGender(GENDER gender) {
            _gender = gender;
            Messenger.Broadcast(Signals.GENDER_CHANGED, this, gender);
        }
        public void ChangeRace(RACE race) {
            if (_raceSetting.race == race) {
                return; //current race is already the new race, no change
            }
            if (_raceSetting.tags != null) {
                RemoveAttributes(_raceSetting.tags);
            }
            RaceSetting raceSetting = RaceManager.Instance.racesDictionary[race.ToString()];
            _raceSetting = raceSetting.CreateNewCopy();
            GenerateRaceAttributes(); //regenerate attributes from new race
        }
        public void ChangeClass(string className) {
            //TODO: Change data as needed
            string previousClassName = _characterClass.className;
            CharacterClass charClass = CharacterManager.Instance.classesDictionary[className];
            _characterClass = charClass.CreateNewCopy();
            OnCharacterClassChange();

#if !WORLD_CREATION_TOOL
            _homeLandmark.tileLocation.areaOfTile.excessClasses.Remove(previousClassName);
            _homeLandmark.tileLocation.areaOfTile.missingClasses.Remove(_characterClass.className);

            Log log = new Log(GameManager.Instance.Today(), "CharacterActions", "ChangeClassAction", "change_class");
            log.AddToFillers(this, this.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddToFillers(null, previousClassName, LOG_IDENTIFIER.STRING_1);
            log.AddToFillers(null, _characterClass.className, LOG_IDENTIFIER.STRING_2);
            AddHistory(log);
            //check equipped items
#endif

        }
        public void SetName(string newName){
			_name = newName;
		}
        //If true, character can't do daily action (onDailyAction), i.e. actions, needs
        //public void SetIsIdle(bool state) {
        //    _isIdle = state;
        //}
        //public bool HasPathToParty(Party partyToJoin) {
        //    return PathGenerator.Instance.GetPath(currLocation, partyToJoin.currLocation, PATHFINDING_MODE.PASSABLE, _faction) != null;
        //}
        public void CenterOnCharacter() {
            if (!this.isDead) {
                CameraMove.Instance.CenterCameraOn(currentParty.specificLocation.tileLocation.gameObject);
            }
        }
        private void GetRandomCharacterColor() {
            _characterColor = CombatManager.Instance.UseRandomCharacterColor();
            _characterColorCode = ColorUtility.ToHtmlStringRGBA(_characterColor).Substring(0, 6);
        }
        public void SetCharacterColor(Color color) {
            _characterColor = color;
            _characterColorCode = ColorUtility.ToHtmlStringRGBA(_characterColor).Substring(0, 6);
        }
        //public void EverydayAction() {
        //    if (onDailyAction != null) {
        //        onDailyAction();
        //    }
        //    CheckForPPDeath();
        //}
        //public void AdvertiseSelf(ActionThread actionThread) {
        //    if(actionThread.character.id != this.id && _currentRegion.id == actionThread.character.party.currentRegion.id) {
        //        actionThread.AddToChoices(_characterObject);
        //    }
        //}
        public bool CanObtainResource(List<RESOURCE> resources) {
            if (this.role != null) {//characters without a role cannot get actions, and therefore cannot obtain resources
                for (int i = 0; i < _ownParty.currentRegion.landmarks.Count; i++) {
                    BaseLandmark landmark = _ownParty.currentRegion.landmarks[i];
                    StructureObj iobject = landmark.landmarkObj;
                    if (iobject.currentState.actions != null && iobject.currentState.actions.Count > 0) {
                        for (int k = 0; k < iobject.currentState.actions.Count; k++) {
                            CharacterAction action = iobject.currentState.actions[k];
                            if (action.actionData.resourceGiven != RESOURCE.NONE && resources.Contains(action.actionData.resourceGiven)) { //does the action grant a resource, and is that a resource that is needed
                                if (action.MeetsRequirements(_ownParty, landmark) && action.CanBeDone(iobject) && action.CanBeDoneBy(_ownParty, iobject)) { //Filter
                                    //if the character can do an action that yields a needed resource, return true
                                    return true;
                                }
                            }

                        }
                    }
                }
            }
            return false;
        }
        public bool IsSpecialCivilian() {
            if (this.characterClass != null) {
                if (this.characterClass.className.Equals("Farmer") || this.characterClass.className.Equals("Miner") || this.characterClass.className.Equals("Retired Hero") ||
                    this.characterClass.className.Equals("Shopkeeper") || this.characterClass.className.Equals("Woodcutter")) {
                    return true;
                }
            }
            return false;
        }
        private void OnOtherCharacterDied(Character character) {
            if (character.id != this.id) {
                if (IsCharacterLovedOne(character)) { //A character gains heartbroken tag for 15 days when a family member or a loved one dies.
                    AddAttribute(ATTRIBUTE.HEARTBROKEN);
                }
                //RemoveRelationshipWith(character);
            }
        }
        public bool IsCharacterLovedOne(Character otherCharacter) {
            Relationship rel = GetRelationshipWith(otherCharacter);
            if (rel != null) {
                CHARACTER_RELATIONSHIP[] lovedOneStatuses = new CHARACTER_RELATIONSHIP[] {
                    CHARACTER_RELATIONSHIP.FATHER,
                    CHARACTER_RELATIONSHIP.MOTHER,
                    CHARACTER_RELATIONSHIP.BROTHER,
                    CHARACTER_RELATIONSHIP.SISTER,
                    CHARACTER_RELATIONSHIP.SON,
                    CHARACTER_RELATIONSHIP.DAUGHTER,
                    CHARACTER_RELATIONSHIP.LOVER,
                    CHARACTER_RELATIONSHIP.HUSBAND,
                    CHARACTER_RELATIONSHIP.WIFE,
                };
                if (rel.HasAnyStatus(lovedOneStatuses)) {
                    return true;
                }
            }
            return false;
        }
        public void SetMode(MODE mode) {
            _currentMode = mode;
        }
        public void SetDoNotDisturb(bool state) {
            _doNotDisturb = state;
        }
        #endregion

        #region Relationships
        public void AddNewRelationship(Character relWith, Relationship relationship) {
            if (!_relationships.ContainsKey(relWith)) {
                _relationships.Add(relWith, relationship);
                Messenger.Broadcast<Relationship>(Signals.RELATIONSHIP_CREATED, relationship);
            } else {
                throw new Exception(this.name + " already has a relationship with " + relWith.name + ", but something is trying to create a new one!");
            }
        }
        public void RemoveRelationshipWith(Character relWith) {
            if (_relationships.ContainsKey(relWith)) {
                Relationship rel = _relationships[relWith];
                _relationships.Remove(relWith);
                Messenger.Broadcast<Relationship>(Signals.RELATIONSHIP_REMOVED, rel);
            }
        }
        public Relationship GetRelationshipWith(Character character) {
            if (_relationships.ContainsKey(character)) {
                return _relationships[character];
            }
            return null;
        }
        public bool AlreadyHasRelationshipStatus(CHARACTER_RELATIONSHIP relStat) {
            foreach (KeyValuePair<Character, Relationship> kvp in relationships) {
                if (kvp.Value.HasStatus(relStat)) {
                    return true;
                }
            }
            return false;
        }
        public Character GetCharacterWithRelationshipStatus(CHARACTER_RELATIONSHIP relStat) {
            foreach (KeyValuePair<Character, Relationship> kvp in relationships) {
                if (kvp.Value.HasStatus(relStat)) {
                    return kvp.Key;
                }
            }
            return null;
        }
        public List<Character> GetCharactersWithRelationshipStatus(CHARACTER_RELATIONSHIP relStat) {
            List<Character> characters = new List<Character>();
            foreach (KeyValuePair<Character, Relationship> kvp in relationships) {
                if (kvp.Value.HasStatus(relStat) && characters.Contains(kvp.Key)) {
                    characters.Add(kvp.Key);
                }
            }
            return characters;
        }
        public void LoadRelationships(List<RelationshipSaveData> data) {
            _relationships = new Dictionary<Character, Relationship>();
            for (int i = 0; i < data.Count; i++) {
                RelationshipSaveData currData = data[i];
                Character otherCharacter = CharacterManager.Instance.GetCharacterByID(currData.targetCharacterID);
                Relationship rel = new Relationship(this, otherCharacter);
                rel.AddRelationshipStatus(currData.relationshipStatuses);
                _relationships.Add(otherCharacter, rel);

            }
        }
        public Character GetPartner() {
            foreach (KeyValuePair<Character, Relationship> kvp in _relationships) {
                for (int i = 0; i < kvp.Value.relationshipStatuses.Count; i++) {
                    CHARACTER_RELATIONSHIP status = kvp.Value.relationshipStatuses[i];
                    if (status == CHARACTER_RELATIONSHIP.HUSBAND || status == CHARACTER_RELATIONSHIP.WIFE) {
                        return kvp.Key;
                    }
                }
            }
            return null;
        }
        public bool HasRelationshipWith(Character otherCharacter) {
            return _relationships.ContainsKey(otherCharacter);
        }
        public bool HasRelationshipStatusWith(Character otherCharacter, CHARACTER_RELATIONSHIP relStat) {
            return _relationships[otherCharacter].HasStatus(relStat);
        }
        #endregion

        #region History
        public void AddHistory(Log log) {
            log.SetInspected(_isBeingInspected);
            _history.Add(log);
            //if (UIManager.Instance.characterInfoUI.currentlyShowingCharacter != null && this.id == UIManager.Instance.characterInfoUI.currentlyShowingCharacter.id) {
            //    Debug.Log("Added log to history of " + this.name + ". " + log.isInspected);
            //}
            if (this._history.Count > 60) {
                this._history.RemoveAt(0);
            }
            Messenger.Broadcast(Signals.HISTORY_ADDED, this as object);
        }
        #endregion

        #region Character
		public bool IsHostileWith(Character character){
            if (this.faction == null) {
                return true; //this character has no faction
            }
            //if (this.currentAction != null && this.currentAction.HasHostilitiesBecauseOfTask(combatInitializer)) {
            //    return true;
            //}
            //Check here if the combatInitializer is hostile with this character, if yes, return true
            Faction factionOfEnemy = character.faction;
            
            //if (combatInitializer.icharacterType == ICHARACTER_TYPE.CHARACTER) {
            //    factionOfEnemy = (combatInitializer as Character).faction;
            //}else if(combatInitializer is Party) {
            //    factionOfEnemy = (combatInitializer as Party).faction;
            //}
            if(factionOfEnemy != null) {
                if(factionOfEnemy.id == this.faction.id) {
                    return false; //characters are of same faction
                }
                FactionRelationship rel = this.faction.GetRelationshipWith(factionOfEnemy);
                if(rel.relationshipStatus == FACTION_RELATIONSHIP_STATUS.HOSTILE) {
                    return true; //factions of combatants are hostile
                }
                return false;
            } else {
                return true; //enemy has no faction
            }
			
		}
        public STANCE GetCurrentStance() {
            return STANCE.NEUTRAL;
        }
        #endregion

        #region Combat Handlers
		public void SetIsInCombat (bool state){
			_isInCombat = state;
		}
        #endregion

        #region Landmarks
        public void AddExploredLandmark(BaseLandmark landmark){
			_exploredLandmarks.Add(landmark); //did not add checking if landmark is already in list, since I want to allow duplicates
            //schedule removal of landmark after 6 months
            GameDate expiration = GameManager.Instance.Today();
            expiration.AddMonths(6);
            SchedulingManager.Instance.AddEntry(expiration, () => RemoveLandmarkAsExplored(landmark));
		}
        private void RemoveLandmarkAsExplored(BaseLandmark landmark) {
            _exploredLandmarks.Remove(landmark);
        }
        private void OnDestroyLandmark(BaseLandmark landmark) {
            if(specificLocation.tileLocation.landmarkOnTile != null && specificLocation.tileLocation.landmarkOnTile.id == landmark.id) {
                if(!_isDead && _currentParty.icon.isTravelling) {
                    return;
                }
                Death();
            }
        }
        #endregion

        //#region Psytoxin
		//private void RegionPsytoxin(List<Region> regions){
		//	for (int i = 0; i < regions.Count; i++) {
		//		if(_party.currentRegion.id == regions[i].id){
		//			InfectedByPsytoxin ();
		//			break;
		//		}
		//	}
		//}
		//private void InfectedByPsytoxin(){
		//	if(HasTag(ATTRIBUTE.SEVERE_PSYTOXIN)){
		//		return;	
		//	}
		//	ModeratePsytoxin modPsytoxin = (ModeratePsytoxin)GetTag (ATTRIBUTE.MODERATE_PSYTOXIN);
		//	if(modPsytoxin != null){
		//		modPsytoxin.TriggerWorsenCase ();
		//	}else{
		//		MildPsytoxin mildPsytoxin = (MildPsytoxin)GetTag (ATTRIBUTE.MILD_PSYTOXIN);
		//		if(mildPsytoxin != null){
		//			mildPsytoxin.TriggerWorsenCase ();
		//		}else{
		//			AssignTag (ATTRIBUTE.MILD_PSYTOXIN);
		//		}
		//	}
		//}
        //#endregion

        #region Traces
		public void LeaveTraceOnLandmark(){
			ILocation location = _ownParty.specificLocation;
			if(location != null && location.locIdentifier == LOCATION_IDENTIFIER.LANDMARK){
				BaseLandmark landmark = location as BaseLandmark;
				int chance = UnityEngine.Random.Range (0, 100);
				int value = GetLeaveTraceChance ();
				if(chance < value){
                    landmark.AddTrace(this);
                }
			}
		}
		private int GetLeaveTraceChance(){
			STANCE stance = GetCurrentStance ();
			switch(stance){
			case STANCE.COMBAT:
				return 100;
			case STANCE.NEUTRAL:
				return 50;
			case STANCE.STEALTHY:
				return 25;
			}
			return 0;
		}
		public void AddTraceInfo(Character character, string identifier, bool isUnique){
			if(isUnique){
				Character previousTrace = GetCharacterFromTraceInfo (identifier);
				if(previousTrace != null){ //If there is an existing trace of the item, replace it with this new trace
					if(previousTrace.id != character.id){
						_traceInfo [previousTrace].Remove (identifier);
						if(_traceInfo[previousTrace].Count <= 0){
							_traceInfo.Remove (previousTrace);
						}
						Debug.Log (this.name +  " REMOVED TRACE INFO OF " + previousTrace.name + " FOR " + identifier);
					}else{
						return;
					}
				}
			}
			if(_traceInfo.ContainsKey(character)){
				if(!_traceInfo [character].Contains(identifier)){
					_traceInfo [character].Add (identifier);
					Debug.Log (this.name +  " ADDED TRACE INFO OF " + character.name + " FOR " + identifier);
				}
			}else{
				_traceInfo.Add (character, new List<string> (){ identifier });
				Debug.Log (this.name +  " ADDED TRACE INFO OF " + character.name + " FOR " + identifier);
			}
		}
		public Character GetCharacterFromTraceInfo(string info){
			foreach (Character character in _traceInfo.Keys) {
				if(_traceInfo[character].Contains(info)){
					return character;
				}
			}
			return null;
		}
        #endregion

        #region Action Queue
        public bool DoesSatisfiesPrerequisite(IPrerequisite prerequisite) {
            if(prerequisite.prerequisiteType == PREREQUISITE.RESOURCE) {
                ResourcePrerequisite resourcePrerequisite = prerequisite as ResourcePrerequisite;
                if(resourcePrerequisite.resourceType != RESOURCE.NONE && _ownParty.characterObject.resourceInventory[resourcePrerequisite.resourceType] >= resourcePrerequisite.amount) {
                    return true;
                }
            }
            return false;
        }
        #endregion

        //#region Needs
        //private void CiviliansDiedReduceSanity(StructureObj whereCiviliansDied, int amount) {
        //    if(_currentRegion.id == whereCiviliansDied.objectLocation.tileLocation.region.id) {
        //        ILocation location = _ownParty.specificLocation;
        //        if (location.tileLocation.id == whereCiviliansDied.objectLocation.tileLocation.id || whereCiviliansDied.objectLocation.tileLocation.neighbourDirections.ContainsValue(location.tileLocation)){
        //            int sanityToReduce = amount * 5;
        //            //this.role.AdjustSanity(-sanityToReduce);
        //            Debug.Log(this.name + " has reduced its sanity by " + sanityToReduce + " because " + amount + " civilians died in " + whereCiviliansDied.objectLocation.tileLocation.tileName + " (" + whereCiviliansDied.objectLocation.tileLocation.coordinates + ")");
        //        }
        //    }
        //}
        //#endregion

        #region Portrait Settings
        public void SetPortraitSettings(PortraitSettings settings) {
            _portraitSettings = settings;
        }
        #endregion

        #region RPG
        public void LevelUp() {
            if(_level < CharacterManager.Instance.maxLevel) {
                _level += 1;
                _experience = 0;
                RecomputeMaxExperience();
                //Add stats per level from class
                _attackPower += (int) ((_characterClass.attackPowerPerLevel / 100f) * (float) _raceSetting.baseAttackPower);
                _speed += (int) ((_characterClass.speedPerLevel / 100f) * (float) _raceSetting.baseSpeed);
                AdjustMaxHP((int) ((_characterClass.hpPerLevel / 100f) * (float) _raceSetting.baseHP));
                _maxSP += _characterClass.spPerLevel;
                //Add stats per level from race
                if(_level > 1) {
                    int hpIndex = _level % _raceSetting.hpPerLevel.Length;
                    hpIndex = hpIndex == 0 ? _raceSetting.hpPerLevel.Length : hpIndex;
                    int attackIndex = _level % _raceSetting.attackPerLevel.Length;
                    attackIndex = attackIndex == 0 ? _raceSetting.attackPerLevel.Length : attackIndex;

                    AdjustMaxHP(_raceSetting.hpPerLevel[hpIndex - 1]);
                    _attackPower += _raceSetting.attackPerLevel[attackIndex - 1];
                }

                //Reset to full health and sp
                ResetToFullHP();
                ResetToFullSP();
            }
        }
        public void OnCharacterClassChange() {
            if(_currentHP > _maxHP) {
                _currentHP = _maxHP;
            }
            if (_sp > _maxSP) {
                _sp = _maxSP;
            }
        }
        public void AdjustExperience(int amount) {
            _experience += amount;
            if(_experience >= _maxExperience) {
                LevelUp();
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
            for (int i = 0; i < currentParty.icharacters.Count; i++) {
                compPower += currentParty.icharacters[i].attackPower;
            }
            return compPower;
        }
        public void SetMaxHP(int amount) {
            int previousMaxHP = maxHP;
            _maxHP = amount;
            int currentMaxHP = maxHP;
            if(_currentHP > currentMaxHP || _currentHP == previousMaxHP) {
                _currentHP = currentMaxHP;
            }
        }
        public void AdjustMaxHP(int amount) {
            int previousMaxHP = maxHP;
            _maxHP += amount;
            int currentMaxHP = maxHP;
            if (_currentHP > currentMaxHP || _currentHP == previousMaxHP) {
                _currentHP = currentMaxHP;
            }
        }
        #endregion

        #region Player/Character Actions
        public void OnThisCharacterSnatched() {
            BaseLandmark snatcherLair = PlayerManager.Instance.player.GetAvailableSnatcherLair();
            if (snatcherLair == null) {
                throw new Exception("There is not available snatcher lair!");
            } else {
                Imprison();
                if (IsInOwnParty()) {
                    //character is in his/her own party
                    ILocation location = currentParty.specificLocation;
                    if (location != null && location.locIdentifier == LOCATION_IDENTIFIER.LANDMARK) { //if character is at a landmark
                        location.RemoveCharacterFromLocation(currentParty);
                    }
                } else {
                    //character is in another party
                    this.currentParty.RemoveCharacter(this);
                }
                snatcherLair.AddCharacterToLocation(ownParty);
            }
        }
        private void ConstructDefaultMiscActions() {
            _miscActions = new List<CharacterAction>();
            CharacterAction read = ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.READ);
            CharacterAction foolingAround = ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.FOOLING_AROUND);
            CharacterAction argue = ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.ARGUE);
            CharacterAction chat = ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.CHAT);

            read.SetActionCategory(ACTION_CATEGORY.MISC);
            foolingAround.SetActionCategory(ACTION_CATEGORY.MISC);
            chat.SetActionCategory(ACTION_CATEGORY.MISC);
            argue.SetActionCategory(ACTION_CATEGORY.MISC);

            AddMiscAction(read);
            AddMiscAction(foolingAround);
            AddMiscAction(chat);
            AddMiscAction(argue);
            //_miscActions.Add(read);
            //_miscActions.Add(foolingAround);
            //_idleActions.Add(chat);
        }
        public CharacterAction GetRandomMiscAction(ref IObject targetObject) {
            targetObject = _ownParty.characterObject;
            CharacterAction chosenAction = _miscActions[UnityEngine.Random.Range(0, _miscActions.Count)];
            if (chosenAction is ChatAction) {
                List<CharacterParty> partyPool = new List<CharacterParty>();
                CharacterParty chosenParty = GetPriority1TargetChatAction(partyPool);
                if(chosenParty == null) {
                    chosenParty = GetPriority2TargetChatAction(partyPool);
                    if (chosenParty == null) {
                        chosenParty = GetPriority3TargetChatAction(partyPool);
                        if (chosenParty == null) {
                            chosenParty = GetPriority4TargetChatAction(partyPool);
                        }
                    }
                }
                if(chosenParty == null) {
                    _miscActions.Remove(chosenAction);
                    CharacterAction newChosenAction = _miscActions[Utilities.rng.Next(0, _miscActions.Count)];
                    _miscActions.Add(chosenAction);
                    return newChosenAction;
                } else {
                    targetObject = chosenParty.icharacterObject;
                }
            }
            return chosenAction;
        }
        public CharacterAction GetWeightedMiscAction(ref IObject targetObject, ref string actionLog) {
            WeightedDictionary<ActionAndTarget> miscActionOptions = new WeightedDictionary<ActionAndTarget>();
            for (int i = 0; i < _miscActions.Count; i++) {
                CharacterAction currentAction = _miscActions[i];
                if(currentAction.disableCounter <= 0 && currentAction.enableCounter > 0) {
                    IObject currentTarget = currentAction.GetTargetObject(_ownParty);
                    if(currentTarget != null) {
                        ActionAndTarget actionAndTarget = new ActionAndTarget() {
                            action = currentAction,
                            target = currentTarget
                        };
                        miscActionOptions.AddElement(actionAndTarget, currentAction.weight);
                    }
                }
            }
            actionLog += "\n" + miscActionOptions.GetWeightsSummary("Misc Action Weights: ");
            ActionAndTarget chosen = miscActionOptions.PickRandomElementGivenWeights();
            targetObject = chosen.target;
            return chosen.action;
        }
        public CharacterAction GetMiscAction(ACTION_TYPE type) {
            for (int i = 0; i < _miscActions.Count; i++) {
                if(_miscActions[i].actionData.actionType == type) {
                    return _miscActions[i];
                }
            }
            return null;
        }
        public void AddMiscAction(CharacterAction characterAction) {
            CharacterAction sameAction = GetMiscAction(characterAction.actionType);
            if (sameAction == null) {
                _miscActions.Add(characterAction);
                characterAction.AdjustEnableCounter(1);
                characterAction.OnAddActionToCharacter(this);
            } else {
                sameAction.AdjustEnableCounter(1);
            }
        }

        public void RemoveMiscAction(ACTION_TYPE actionType) {
            CharacterAction sameAction = GetMiscAction(actionType);
            if (sameAction != null) {
                if (sameAction.enableCounter > 1) {
                    sameAction.AdjustEnableCounter(-1);
                } else {
                    if (_miscActions.Remove(sameAction)) {
                        sameAction.OnRemoveActionFromCharacter(this);
                    }
                }
            }
        }
        private CharacterParty GetPriority1TargetChatAction(List<CharacterParty> partyPool) {
            //random parties within same faction within settlements
            partyPool.Clear();
            for (int i = 0; i < faction.characters.Count; i++) {
                CharacterParty party = faction.characters[i].party;
                if (party.id != this._ownParty.id && party.characterObject.currentState.stateName == "Alive" && party.landmarkLocation != null && faction.ownedAreas.Contains(party.specificLocation.tileLocation.areaOfTile) && !partyPool.Contains(party)) {
                    partyPool.Add(party);
                }
            }
            if(partyPool.Count > 0) {
                return partyPool[Utilities.rng.Next(0, partyPool.Count)];
            }
            return null;
        }
        private CharacterParty GetPriority2TargetChatAction(List<CharacterParty> partyPool) {
            //random parties within non-hostile factions within settlements
            partyPool.Clear();
            List<Faction> nonHostileFactions = FactionManager.Instance.GetFactionsWithByStatus(faction, FACTION_RELATIONSHIP_STATUS.NON_HOSTILE); 
            for (int i = 0; i < nonHostileFactions.Count; i++) {
                Faction nonHostileFaction = nonHostileFactions[i];
                for (int k = 0; k < nonHostileFaction.characters.Count; k++) {
                    CharacterParty party = nonHostileFaction.characters[k].party;
                    if (party.id != this._ownParty.id && party.characterObject.currentState.stateName == "Alive" && party.landmarkLocation != null && faction.ownedAreas.Contains(party.landmarkLocation.tileLocation.areaOfTile) && !partyPool.Contains(party)) {
                        partyPool.Add(party);
                    }
                }
            }
            if (partyPool.Count > 0) {
                return partyPool[Utilities.rng.Next(0, partyPool.Count)];
            }
            return null;
        }
        private CharacterParty GetPriority3TargetChatAction(List<CharacterParty> partyPool) {
            //random parties within same faction outside settlements currently performing an Idle Action
            partyPool.Clear();
            for (int i = 0; i < faction.characters.Count; i++) {
                CharacterParty party = faction.characters[i].party;
                if (party.id != this._ownParty.id && party.characterObject.currentState.stateName == "Alive" && party.landmarkLocation != null 
                    && !faction.ownedAreas.Contains(party.landmarkLocation.tileLocation.areaOfTile) && !partyPool.Contains(party) 
                    && party.actionData.currentAction != null && party.actionData.currentAction.actionData.actionCategory == ACTION_CATEGORY.MISC) {
                    partyPool.Add(party);
                }
            }
            if (partyPool.Count > 0) {
                return partyPool[Utilities.rng.Next(0, partyPool.Count)];
            }
            return null;
        }
        private CharacterParty GetPriority4TargetChatAction(List<CharacterParty> partyPool) {
            //random parties within non-hostile faction outside settlements currently performing an Idle Action
            partyPool.Clear();
            List<Faction> nonHostileFactions = FactionManager.Instance.GetFactionsWithByStatus(faction, FACTION_RELATIONSHIP_STATUS.NON_HOSTILE);
            for (int i = 0; i < nonHostileFactions.Count; i++) {
                Faction nonHostileFaction = nonHostileFactions[i];
                for (int k = 0; k < nonHostileFaction.characters.Count; k++) {
                    CharacterParty party = nonHostileFaction.characters[k].party;
                    if (party.id != this._ownParty.id && party.characterObject.currentState.stateName == "Alive" && party.landmarkLocation != null 
                        && !faction.ownedAreas.Contains(party.landmarkLocation.tileLocation.areaOfTile) && !partyPool.Contains(party) 
                        && party.actionData.currentAction != null && party.actionData.currentAction.actionData.actionCategory == ACTION_CATEGORY.MISC) {
                        partyPool.Add(party);
                    }
                }
            }
            if (partyPool.Count > 0) {
                return partyPool[Utilities.rng.Next(0, partyPool.Count)];
            }
            return null;
        }
        #endregion

        #region Snatch
        /*
         When the player successfully snatches a character, other characters with relation to 
         the snatched one would all be sent signals to check whether they should react or not. 
         Other character reaction would depend on their relationship, happiness and traits.
             */
        //private void OnCharacterSnatched(ECS.Character otherCharacter) {
        //    if (otherCharacter.id != this.id && this.party.characterObject.currentState.stateName != "Imprisoned") {
        //        if (relationships.ContainsKey(otherCharacter)) { //if this character has a relationship with the one that was snatched
        //            Debug.Log(this.name + " will react to " + otherCharacter.name + " being snatched!");
        //            //For now make all characters that have relationship with the snatched character, react.
        //            if (UnityEngine.Random.Range(0, 1) == 0) {
        //                //obtain release character questline
        //                Debug.Log(this.name + " decided to release " + otherCharacter.name + " by himself");
        //                //QuestManager.Instance.TakeQuest(QUEST_TYPE.RELEASE_CHARACTER, this, otherCharacter);
        //            } else {
        //                //bargain with player
        //                Debug.Log(this.name + " will bargain for " + otherCharacter.name + "'s freedom!");
        //                TriggerBargain(otherCharacter);
        //            }
        //        }
        //    }
        //}
        //private void TriggerBargain(ECS.Character bargainingFor) {
        //    List<CharacterDialogChoice> dialogChoices = new List<CharacterDialogChoice>();
        //    CharacterDialogChoice killYourselfChoice = new CharacterDialogChoice("Kill yourself!", () => this.Death());
        //    List<Character> otherCharacters = new List<Character>(CharacterManager.Instance.allCharacters.Where(x => x.party.characterObject.currentState.stateName != "Imprisoned"));
        //    otherCharacters.Remove(this);
        //    dialogChoices.Add(killYourselfChoice);
        //    if (otherCharacters.Count > 0) {
        //        ECS.Character characterToAttack = otherCharacters[UnityEngine.Random.Range(0, otherCharacters.Count)];
        //        CharacterDialogChoice attackCharacterChoice = new CharacterDialogChoice("Attack " + characterToAttack.name, 
        //            () => party.actionData.ForceDoAction(characterToAttack.party.characterObject.currentState.GetAction(ACTION_TYPE.ATTACK)
        //            , characterToAttack.party.characterObject));
        //        dialogChoices.Add(attackCharacterChoice);
        //    }

        //    UnityEngine.Events.UnityAction onClickAction = () => Messenger.Broadcast(Signals.SHOW_CHARACTER_DIALOG, this, "Please release " + bargainingFor.name + "!", dialogChoices);

        //    Messenger.Broadcast<string, int, UnityEngine.Events.UnityAction>
        //        (Signals.SHOW_NOTIFICATION, this.name + " wants to bargain for " + bargainingFor.name + "'s freedom!",
        //        144,
        //        onClickAction);
        //}
        #endregion

        #region Home
        public void LookForNewHome() {
            //Try to get a new home structure from this character's area
            BaseLandmark landmark = GetNewHomeFromArea(_homeLandmark.tileLocation.areaOfTile);
            if (landmark != null) {
                _homeLandmark.RemoveCharacterHomeOnLandmark(this);
                landmark.AddCharacterHomeOnLandmark(this);
            } else {
                //If there is no available structure, look for it in other areas of the faction and migrate there
                landmark = GetNewHomeFromFaction();
                if (landmark != null) {
                    //SetHome(landmark.tileLocation.areaOfTile);
                    _homeLandmark.RemoveCharacterHomeOnLandmark(this);
                    landmark.AddCharacterHomeOnLandmark(this);
                } else {
                    //TODO: For future update, migrate to another friendly faction's structure
                }
            }
        }
        private BaseLandmark GetNewHomeFromArea(Area area) {
            BaseLandmark chosenLandmark = null;
            for (int i = 0; i < area.landmarks.Count; i++) {
                BaseLandmark landmark = area.landmarks[i];
                if (landmark != _homeLandmark && landmark.landmarkObj.specificObjectType == _homeLandmark.landmarkObj.specificObjectType) {
                    if (chosenLandmark == null) {
                        chosenLandmark = landmark;
                    } else {
                        if (landmark.charactersWithHomeOnLandmark.Count < chosenLandmark.charactersWithHomeOnLandmark.Count) {
                            chosenLandmark = landmark;
                        }
                    }
                }
            }
            return chosenLandmark;
        }
        private BaseLandmark GetNewHomeFromFaction() {
            BaseLandmark chosenLandmark = null;
            if (_faction != null) {
                for (int i = 0; i < _faction.ownedAreas.Count; i++) {
                    Area area = _faction.ownedAreas[i];
                    if (area.id != _homeLandmark.tileLocation.areaOfTile.id) {
                        chosenLandmark = GetNewHomeFromArea(area);
                        if (chosenLandmark != null) {
                            break;
                        }
                    }
                }
            }
            return chosenLandmark;
        }
        //public void SetHome(Area newHome) {
        //    _home = newHome;
        //}
        public void SetHomeLandmark(BaseLandmark newHomeLandmark) {
            BaseLandmark previousHome = _homeLandmark;
            this._homeLandmark = newHomeLandmark;
            if (!(this is CharacterArmyUnit)) {
                if (previousHome != null) {
                    previousHome.tileLocation.areaOfTile.residents.Remove(this);
                    if (_homeLandmark != null) {
                        _homeLandmark.tileLocation.areaOfTile.residents.Add(this);
                        if (_homeLandmark.tileLocation.areaOfTile.id != previousHome.tileLocation.areaOfTile.id) {
#if !WORLD_CREATION_TOOL
                            LookForNewWorkplace();
#endif
                        }
                    }

                } else {
                    if (_homeLandmark != null) {
                        if (_homeLandmark.tileLocation.areaOfTile != null) {
                            _homeLandmark.tileLocation.areaOfTile.residents.Add(this);
                        }
#if !WORLD_CREATION_TOOL
                        LookForNewWorkplace();
#endif
                    }
                }
            }
        }
        //public void SetHomeStructure(StructureObj newHomeStructure) {
        //    if (_homeStructure != null) {
        //        _homeStructure.AdjustNumOfResidents(-1);
        //    }
        //    _homeStructure = newHomeStructure;
        //    newHomeStructure.AdjustNumOfResidents(1);
        //}
        //private void OnAreaDeleted(Area deletedArea) {
        //    if (_home.id == deletedArea.id) {
        //        SetHome(null);
        //    }
        //}
        #endregion

        #region Work
        public bool LookForNewWorkplace() {
            if (_characterClass.workActionType == ACTION_TYPE.WORKING) {
                _workplace = _homeLandmark;
                return true;
            } else {
                List<BaseLandmark> workplaceChoices = new List<BaseLandmark>();
                for (int i = 0; i < _homeLandmark.tileLocation.areaOfTile.landmarks.Count; i++) {
                    StructureObj structure = _homeLandmark.tileLocation.areaOfTile.landmarks[i].landmarkObj;
                    for (int j = 0; j < structure.currentState.actions.Count; j++) {
                        if(structure.currentState.actions[j].actionType == _characterClass.workActionType) {
                            workplaceChoices.Add(_homeLandmark.tileLocation.areaOfTile.landmarks[i]);
                            break;
                        }
                    }
                }
                if (workplaceChoices.Count != 0) {
                    _workplace = workplaceChoices[UnityEngine.Random.Range(0, workplaceChoices.Count)];
                    return true;
                }
                //throw new Exception("Could not find workplace for " + this.name);
            }
            return false;
        }
        #endregion

        #region Squads
        public void SetSquad(Squad squad) {
            _squad = squad;
        }
        public bool IsSquadLeader() {
            if (_squad == null) {
                return false;
            } else {
                if (_squad.squadLeader != null && _squad.squadLeader.id == this.id) {
                    return true;
                }
                return false;
            }
        }
        public bool IsSquadMember() {
            if (_squad == null) {
                return false;
            } else {
                if (_squad.squadLeader != null && _squad.squadLeader.id != this.id) {
                    return true;
                }
                return false;
            }
        }
        //public List<Quest> GetElligibleQuests() {
        //    List<Quest> quests = new List<Quest>();
        //    if (this.IsSquadLeader()) {
        //        quests.AddRange(this.GetAcceptedQuestsByGroup(GROUP_TYPE.SOLO));
        //        quests.AddRange(squad.GetSquadQuests());
        //    } else if (this.IsSquadMember()) {
        //        quests.AddRange(this.GetAcceptedQuestsByGroup(GROUP_TYPE.SOLO));
        //    } else if (squad == null) {
        //        quests.AddRange(this.GetAcceptedQuestsByGroup(GROUP_TYPE.SOLO));
        //    }
        //    return quests;
        //}
        #endregion

        #region Action Queue
        public void AddActionToQueue(CharacterAction action, IObject targetObject, Quest associatedQuest = null, int position = -1) {
            if (position == -1) {
                //add action to end
                _actionQueue.Enqueue(new ActionQueueItem(action, targetObject, associatedQuest));
            } else {
                //Insert action to specified position
                _actionQueue.Enqueue(new ActionQueueItem(action, targetObject, associatedQuest), position);
            }
        }
        public void AddActionToQueue(EventAction eventAction, GameEvent associatedEvent, int position = -1) {
            if (position == -1) {
                //add action to end
                _actionQueue.Enqueue(new ActionQueueItem(eventAction.action, eventAction.targetObject, null, associatedEvent));
            } else {
                //Insert action to specified position
                _actionQueue.Enqueue(new ActionQueueItem(eventAction.action, eventAction.targetObject, null, associatedEvent), position);
            }
        }
        public void RemoveActionFromQueue(ActionQueueItem item) {
            _actionQueue.Remove(item);
        }
        public void RemoveActionFromQueue(CharacterAction action) {
            for (int i = 0; i < _actionQueue.Count; i++) {
                ActionQueueItem queueItem = _actionQueue.GetBasedOnIndex(i);
                if(queueItem.action == action) {
                    _actionQueue.RemoveAt(i);
                    break;
                }
            }
        }
        #endregion

        #region Schedule
        public void SetSchedule(CharacterSchedule schedule) {
            this.schedule = schedule;
            schedule.Initialize();
        }
        public void OnSchedulePhaseStarted(SCHEDULE_ACTION_CATEGORY startedPhase) {
            return; //disable scheduled movement
            Debug.Log(this.name + " started phase " + startedPhase.ToString());
            //once a new phase has started
            //check if it is a different phase from the one before it
            if (schedule.previousPhase != SCHEDULE_ACTION_CATEGORY.NONE && startedPhase != schedule.previousPhase) {//if it is
                if (_ownParty.actionData.currentAction != null && _ownParty.actionData.currentAction.actionType != ACTION_TYPE.DEFENDER) {
                    _ownParty.actionData.EndAction(); //end the current action
                    _ownParty.actionData.LookForAction(); //then look for a new action that is part of the phase category (Work/Rest)
                }
            }
            //if it is not, do nothing (continue current action)
        }
        public int GetWorkDeadlineTick() {
            return 0;
            //return this.dailySchedule.currentPhase.startTick + 7; //start of work phase + 1 hour(6 ticks) + 1 tick (because if other character arrives at exactly the work deadline, he/she will not be included in party, even though they are technically not late)
        }
        #endregion

        #region Event Schedule
        private GameEvent nextScheduledEvent;
        private DateRange nextScheduledEventDate;
        public void AddScheduledEvent(DateRange dateRange, GameEvent gameEvent) {
            Debug.Log("[" + GameManager.Instance.Today().GetDayAndTicksString() + "]" + this.name + " will schedule an event to " + dateRange.ToString());
            if (eventSchedule.HasConflictingSchedule(dateRange)) {
                //There is a conflict in the current schedule of the character, move the new event to a new schedule.
                GameDate nextFreeDate = eventSchedule.GetNextFreeDateForEvent(gameEvent);
                GameDate endDate = nextFreeDate;
                endDate.AddHours(gameEvent.GetEventDurationRoughEstimateInTicks());
                DateRange newSched = new DateRange(nextFreeDate, endDate);
                eventSchedule.AddElement(newSched, gameEvent);
                Debug.Log("[" + GameManager.Instance.Today().GetDayAndTicksString() + "]" + this.name + " has a conflicting schedule. Rescheduled event to " + newSched.ToString());
            } else {
                eventSchedule.AddElement(dateRange, gameEvent);
                Debug.Log("[" + GameManager.Instance.Today().GetDayAndTicksString() + "]" + this.name + " added scehduled event " + gameEvent.name + " on " + dateRange.ToString());

                GameDate checkDate = dateRange.startDate;
                checkDate.ReduceHours(GameManager.hoursPerDay);
                if (checkDate.IsBefore(GameManager.Instance.Today())) { //if the check date is before today
                    //start check on the next tick
                    checkDate = GameManager.Instance.Today();
                    checkDate.AddHours(1);
                }
                //Once event has been scheduled, schedule every tick checking 144 ticks before the start date of the new event
                SchedulingManager.Instance.AddEntry(checkDate, () => StartEveryTickCheckForEvent());
                Debug.Log(this.name + " scheduled every tick check for event " + gameEvent.name + " on " + checkDate.GetDayAndTicksString());
            }
        }
        public void AddScheduledEvent(GameDate startDate, GameEvent gameEvent) {
            GameDate endDate = startDate;
            endDate.AddHours(gameEvent.GetEventDurationRoughEstimateInTicks());

            DateRange dateRange = new DateRange(startDate, endDate);
            AddScheduledEvent(dateRange, gameEvent);
        }
        public void AddScheduledEvent(GameEvent gameEvent) {
            //schedule a game event without specifing a date
            GameDate nextFreeDate = eventSchedule.GetNextFreeDateForEvent(gameEvent);
            GameDate endDate = nextFreeDate;
            endDate.AddHours(gameEvent.GetEventDurationRoughEstimateInTicks());
            DateRange newSched = new DateRange(nextFreeDate, endDate);
            AddScheduledEvent(newSched, gameEvent);
        }
        public void ForceEvent(GameEvent gameEvent) {
            //if we need to force an event to happen (i.e. Defend Action for Monster Attacks Event)
            //
        }
        public bool HasEventScheduled(GameDate date) {
            return eventSchedule[date] != null;
        }
        public bool HasEventScheduled(GAME_EVENT eventType) {
            return eventSchedule.HasEventOfType(eventType);
        }
        public GameEvent GetScheduledEvent(GameDate date) {
            return eventSchedule[date];
        }
        private void StartEveryTickCheckForEvent() {
            nextScheduledEvent = eventSchedule.GetNextEvent(); //Set next game event variable to the next event, to prevent checking the schecule every tick
            nextScheduledEventDate = eventSchedule.GetDateRangeForEvent(nextScheduledEvent);
            Debug.Log(this.name + " started checking every tick for event " + nextScheduledEvent.name);
            Messenger.AddListener(Signals.HOUR_ENDED, EventEveryTick); //Add every tick listener for events
        }
        private void EventEveryTick() {
            HexTile currentLoc = this.currentParty.specificLocation.tileLocation; //check the character's current location
            EventAction nextEventAction = nextScheduledEvent.PeekNextEventAction(this);
            int travelTime = PathGenerator.Instance.GetTravelTimeInTicks(this.specificLocation, nextEventAction.targetLocation, PATHFINDING_MODE.PASSABLE);

            GameDate eventArrivalDate = GameManager.Instance.Today();
            eventArrivalDate.AddHours(travelTime); //given the start date and the travel time, check if the character has to leave now to reach the event in time
            if (!eventArrivalDate.IsBefore(nextScheduledEventDate.startDate) && !party.actionData.isCurrentActionFromEvent) { //if the estimated arrival date is NOT before the next events' scheduled start date
                if (party.actionData.isCurrentActionFromEvent) { //if this character's current action is from an event, do not perform the action from the next scheduled event.
                    Debug.LogWarning(this.name + " did not perform the next event action, since their current action is already from an event");
                } else { //else if this character's action is not from an event
                        //leave now and do the event action
                    AddActionToQueue(nextScheduledEvent.GetNextEventAction(this), nextScheduledEvent); //queue the event action
                    _ownParty.actionData.EndCurrentAction();  //then end their current action
                }
                Messenger.RemoveListener(Signals.HOUR_ENDED, EventEveryTick);
                Debug.Log(GameManager.Instance.TodayLogString() + this.name + " stopped checking every tick for event " + nextScheduledEvent.name);
                nextScheduledEvent = null;

            }
        }
        #endregion

        #region IInteractable
        public void SetIsBeingInspected(bool state) {
            _isBeingInspected = state;
            if(_currentParty.icon != null) {
                _currentParty.icon.UpdateVisualState();
            }
            if (_currentParty.specificLocation != null && _currentParty.specificLocation.tileLocation.landmarkOnTile != null) {
                _currentParty.specificLocation.tileLocation.landmarkOnTile.landmarkVisual.ToggleCharactersVisibility();
            }
        }
        public void SetHasBeenInspected(bool state) {
            _hasBeenInspected = state;
        }
        public void EndedInspection() {
            uiData.UpdateData(this);
        }
        public void AddInteraction(Interaction interaction) {
            _currentInteractions.Add(interaction);
            Messenger.Broadcast(Signals.ADDED_INTERACTION, this as IInteractable, interaction);
        }
        public void RemoveInteraction(Interaction interaction) {
            if (_currentInteractions.Remove(interaction)) {
                Messenger.Broadcast(Signals.REMOVED_INTERACTION, this as IInteractable, interaction);
            }
        }
        #endregion

        #region Hidden Desire
        public void SetHiddenDesire(HiddenDesire hiddenDesire) {
            _hiddenDesire = hiddenDesire;
        }
        public void AwakenHiddenDesire() {
            if (_hiddenDesire == null || _hiddenDesire.isAwakened) {
                //if the character does not have a hidden desire
                //or the hidden desire is already awakened
                return;
            }
            _hiddenDesire.Awaken();
        }
        #endregion

        //#region Intel
        //public void AddIntelReaction(int intelID, GAME_EVENT reaction) {
        //    if (!_intelReactions.ContainsKey(intelID)) {
        //        _intelReactions.Add(intelID, reaction);
        //    } else {
        //        _intelReactions[intelID] = reaction;
        //    }
        //}
        //public void AddIntelReaction(Intel intel, GAME_EVENT reaction) {
        //    AddIntelReaction(intel.id, reaction);
        //}
        //public void RemoveIntelReaction(Intel intel) {
        //    RemoveIntelReaction(intel.id);
        //}
        //public void RemoveIntelReaction(int intelID) {
        //    _intelReactions.Remove(intelID);
        //}
        //public void OnIntelGiven(Intel intel) {
        //    Debug.Log(GameManager.Instance.TodayLogString() + this.name + " was given intel that " + intel.description);
        //    GameEvent gameEvent = EventManager.Instance.AddNewEvent(this.intelReactions[intel.id]);
        //    if (gameEvent.MeetsRequirements(this)) {
        //        List<Character> characters = new List<Character>();
        //        switch (gameEvent.type) {
        //            case GAME_EVENT.SUICIDE:
        //                characters.Add(this);
        //                break;
        //            default:
        //                break;
        //        }
        //        gameEvent.Initialize(characters);
        //    }
        //    //Remove intel reaction from character, even if he/she did not meet the requirements for the reaction?
        //    RemoveIntelReaction(intel);
        //}
        //#endregion

        #region Secrets
        public void AddSecret(Secret secret) {
            if (!_secrets.Contains(secret)) {
                _secrets.Add(secret);
            }
        }
        public void AddSecret(int secretID) {
            Secret secret = SecretManager.Instance.CreateNewSecret(secretID);
            AddSecret(secret);
        }
        public bool HasSecret(int secretID) {
            for (int i = 0; i < _secrets.Count; i++) {
                Secret secret = _secrets[i];
                if (secret.id == secretID) {
                    return true;
                }
            }
            return false;
        }
        public Secret GetSecret(int secretID) {
            for (int i = 0; i < _secrets.Count; i++) {
                Secret secret = _secrets[i];
                if (secret.id == secretID) {
                    return secret;
                }
            }
            return null;
        }
        public void RemoveSecret(int secretID) {
            Secret secret = GetSecret(secretID);
            if (secret != null) {
                RemoveSecret(secret);
            }
        }
        public void RemoveSecret(Secret secret) {
            _secrets.Remove(secret);
        }
        #endregion

        #region Defender
        public void OnSetAsDefender(BaseLandmark defending) {
            defendingLandmark = defending;
            defendingLandmark.RemoveCharacterFromLocation(this.ownParty, false);
            ownParty.SetSpecificLocation(defending);
#if !WORLD_CREATION_TOOL
            if (ownParty is CharacterParty) {
                (ownParty as CharacterParty).actionData.ForceDoAction(ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.DEFENDER), defending.landmarkObj);
            }
#endif
        }
        public void OnRemoveAsDefender() {
            defendingLandmark.AddCharacterToLocation(this.ownParty);
            defendingLandmark = null;
#if !WORLD_CREATION_TOOL
            _ownParty.actionData.EndCurrentAction(); //end the defender action
#endif
        }
        public bool IsDefending(BaseLandmark landmark) {
            if (defendingLandmark != null && defendingLandmark.id == landmark.id) {
                return true;
            }
            return false;
        }
        #endregion

        #region Combat Attributes
        public void AddCombatAttribute(CombatAttribute combatAttribute) {
            if(string.IsNullOrEmpty(GetCombatAttribute(combatAttribute.name).name)) {
                _combatAttributes.Add(combatAttribute);
                ApplyCombatAttributeEffects(combatAttribute);
            }
        }
        public bool RemoveCombatAttribute(CombatAttribute combatAttribute) {
            for (int i = 0; i < _combatAttributes.Count; i++) {
                if (_combatAttributes[i].name == combatAttribute.name) {
                    _combatAttributes.RemoveAt(i);
                    UnapplyCombatAttributeEffects(combatAttribute);
                    return true;    
                }
            }
            return false;
        }
        public CombatAttribute GetCombatAttribute(string attributeName) {
            for (int i = 0; i < _combatAttributes.Count; i++) {
                if(_combatAttributes[i].name == attributeName) {
                    return _combatAttributes[i];
                }
            }
            return new CombatAttribute();
        }
        private void ApplyCombatAttributeEffects(CombatAttribute combatAttribute) {
            if(!combatAttribute.hasRequirement) {
                if(combatAttribute.stat == STAT.ATTACK) {
                    if (combatAttribute.isPercentage) {
                        float result = _attackPower * (combatAttribute.amount / 100f);
                        _attackPower += (int) result;
                    } else {
                        _attackPower += (int) combatAttribute.amount;
                    }
                }else if (combatAttribute.stat == STAT.HP) {
                    int previousMaxHP = _maxHP;
                    if (combatAttribute.isPercentage) {
                        float result = _maxHP * (combatAttribute.amount / 100f);
                        AdjustMaxHP((int) result);
                    } else {
                        AdjustMaxHP((int) combatAttribute.amount);
                    }
                } else if (combatAttribute.stat == STAT.SPEED) {
                    if (combatAttribute.isPercentage) {
                        float result = _speed * (combatAttribute.amount / 100f);
                        _speed += (int) result;
                    } else {
                        _speed += (int) combatAttribute.amount;
                    }
                }
            }
        }
        private void UnapplyCombatAttributeEffects(CombatAttribute combatAttribute) {
            if (!combatAttribute.hasRequirement) {
                if (combatAttribute.stat == STAT.ATTACK) {
                    if (combatAttribute.isPercentage) {
                        float result = _attackPower * (combatAttribute.amount / 100f);
                        _attackPower -= (int) result;
                    } else {
                        _attackPower -= (int) combatAttribute.amount;
                    }
                } else if (combatAttribute.stat == STAT.HP) {
                    int previousMaxHP = _maxHP;
                    if (combatAttribute.isPercentage) {
                        float result = _maxHP * (combatAttribute.amount / 100f);
                        AdjustMaxHP(-(int) result);
                    } else {
                        AdjustMaxHP(-(int) combatAttribute.amount);
                    }
                } else if (combatAttribute.stat == STAT.SPEED) {
                    if (combatAttribute.isPercentage) {
                        float result = _speed * (combatAttribute.amount / 100f);
                        _speed -= (int) result;
                    } else {
                        _speed -= (int) combatAttribute.amount;
                    }
                }
            }
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
    }
}
