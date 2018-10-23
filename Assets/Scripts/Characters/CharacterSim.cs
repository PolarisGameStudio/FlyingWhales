﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;
using System.IO;

public class CharacterSim : ICharacterSim {
    [SerializeField] private string _name;
    [SerializeField] private string _className;
    [SerializeField] private string _raceName;
    [SerializeField] private string _weaponName;
    [SerializeField] private string _armorName;
    [SerializeField] private string _accessoryName;
    [SerializeField] private string _consumableName;
    [SerializeField] private string _skillName;
    [SerializeField] private int _level;
    [SerializeField] private int _armyCount;
    [SerializeField] private bool _isArmy;
    [SerializeField] private GENDER _gender;

    //[SerializeField] private int _strBuild;
    //[SerializeField] private int _intBuild;
    //[SerializeField] private int _agiBuild;
    //[SerializeField] private int _vitBuild;
    //[SerializeField] private int _str;
    //[SerializeField] private int _int;
    //[SerializeField] private int _agi;
    //[SerializeField] private int _vit;
    //[SerializeField] private int _defHead;
    //[SerializeField] private int _defBody;
    //[SerializeField] private int _defLegs;
    //[SerializeField] private int _defHands;
    //[SerializeField] private int _defFeet;

    private int _id;
    private int _currentHP;
    private int _currentSP;
    private int _currentRow;
    private int _singleMaxHP;
    private int _maxSP;
    private int _singleAttackPower;
    private int _singleSpeed;
    private int _attackPower;
    private int _speed;
    private int _maxHP;
    private float _actRate;
    private bool _isDead;
    private SIDES _currentSide;
    private RaceSetting _raceSetting;
    private CharacterClass _characterClass;
    private CharacterBattleTracker _battleTracker;
    private CharacterBattleOnlyTracker _battleOnlyTracker;
    private Weapon _equippedWeapon;
    private Armor _equippedArmor;
    private Item _equippedAccessory;
    private Item _equippedConsumable;
    private List<Skill> _skills;
    private List<CombatAttribute> _combatAttributes;
    private List<Attribute> _attributes;
    //private List<BodyPart> _bodyParts;
    //private List<Item> _equippedItems;
    private Dictionary<ELEMENT, float> _elementalWeaknesses;
    private Dictionary<ELEMENT, float> _elementalResistances;

    #region getters/setters
    public string name {
        get { return _name; }
    }
    public string idName {
        get { return "[" + _id + "]" + this._name; }
    }
    public string className {
        get { return _className; }
    }
    public string raceName {
        get { return _raceName; }
    }
    public string weaponName {
        get { return _weaponName; }
    }
    public string armorName {
        get { return _armorName; }
    }
    public string accessoryName {
        get { return _accessoryName; }
    }
    public string consumableName {
        get { return _consumableName; }
    }
    public int id {
        get { return _id; }
    }
    public int level {
        get { return _level; }
    }
    public int maxHP {
        get { return _maxHP; }
    }
    public int maxSP {
        get { return _maxSP; }
    }
    //public int defHead {
    //    get { return _defHead; }
    //}
    //public int defBody {
    //    get { return _defBody; }
    //}
    //public int defLegs {
    //    get { return _defLegs; }
    //}
    //public int defHands {
    //    get { return _defHands; }
    //}
    //public int defFeet {
    //    get { return _defFeet; }
    //}
    public int currentRow {
        get { return _currentRow; }
    }
    public float actRate {
        get { return _actRate; }
        set { _actRate = value; }
    }
    public int speed {
        get { return _speed; }
    }
    public int attackPower {
        get { return _attackPower; }
    }
    public int singleAttackPower {
        get { return _singleAttackPower; }
    }
    public int singleSpeed {
        get { return _singleSpeed; }
    }
    public int singleMaxHP {
        get { return _singleMaxHP; }
    }
    public int currentHP {
        get { return _currentHP; }
    }
    public int currentSP {
        get { return _currentSP; }
    }
    public int armyCount {
        get { return _armyCount; }
    }
    public bool isArmy {
        get { return _isArmy; }
    }
    public SIDES currentSide {
        get { return _currentSide; }
    }
    public ICHARACTER_TYPE icharacterType {
        get { return ICHARACTER_TYPE.CHARACTER; }
    }
    public GENDER gender {
        get { return _gender; }
    }
    public RACE race {
        get { return _raceSetting.race; }
    }
    public CharacterClass characterClass {
        get { return _characterClass; }
    }
    public CharacterBattleOnlyTracker battleOnlyTracker {
        get { return _battleOnlyTracker; }
    }
    public CharacterBattleTracker battleTracker {
        get { return _battleTracker; }
    }
    public Weapon equippedWeapon {
        get { return _equippedWeapon; }
    }
    public string skillName {
        get { return _skillName; }
    }
    public List<Skill> skills {
        get { return _skills; }
    }
    public List<CombatAttribute> combatAttributes {
        get { return _combatAttributes; }
    }
    public Dictionary<ELEMENT, float> elementalWeaknesses {
        get { return _elementalWeaknesses; }
    }
    public Dictionary<ELEMENT, float> elementalResistances {
        get { return _elementalResistances; }
    }
    #endregion

    public void InitializeSim() {
        _id = Utilities.SetID(this);
        ConstructClass();
        ConstructSkills();
        _raceSetting = JsonUtility.FromJson<RaceSetting>(System.IO.File.ReadAllText(Utilities.dataPath + "RaceSettings/" + _raceName + ".json"));
        _battleOnlyTracker = new CharacterBattleOnlyTracker();
        _battleTracker = new CharacterBattleTracker();
        _elementalWeaknesses = new Dictionary<ELEMENT, float>(CombatSimManager.Instance.elementsChanceDictionary);
        _elementalResistances = new Dictionary<ELEMENT, float>(CombatSimManager.Instance.elementsChanceDictionary);
        _attributes = new List<Attribute>();
        _combatAttributes = new List<CombatAttribute>();
        AllocateStats();
        LevelUp();
        ArmyModifier();
        ResetToFullHP();
        ResetToFullSP();
        EquipWeaponArmors();
    }
    public void SetDataFromCharacterPanelUI() {
        _name = CharacterPanelUI.Instance.nameInput.text;
        _className = CharacterPanelUI.Instance.classOptions.options[CharacterPanelUI.Instance.classOptions.value].text;
        _raceName = CharacterPanelUI.Instance.raceOptions.options[CharacterPanelUI.Instance.raceOptions.value].text;
        _weaponName = CharacterPanelUI.Instance.weaponName;
        _armorName = CharacterPanelUI.Instance.armorName;
        _accessoryName = CharacterPanelUI.Instance.accessoryName;
        //_consumableName = CharacterPanelUI.Instance.consumableOptions.options[CharacterPanelUI.Instance.consumableOptions.value].text;

        _gender = (GENDER) System.Enum.Parse(typeof(GENDER), CharacterPanelUI.Instance.genderOptions.options[CharacterPanelUI.Instance.genderOptions.value].text);
        _level = int.Parse(CharacterPanelUI.Instance.levelInput.text);
        _skillName = CharacterPanelUI.Instance.skillName;

        _isArmy = CharacterPanelUI.Instance.toggleArmy.isOn;
        _armyCount = int.Parse(CharacterPanelUI.Instance.armyInput.text);

        //_defHead = int.Parse(CharacterPanelUI.Instance.dHeadInput.text);
        //_defBody = int.Parse(CharacterPanelUI.Instance.dBodyInput.text);
        //_defLegs = int.Parse(CharacterPanelUI.Instance.dLegsInput.text);
        //_defHands = int.Parse(CharacterPanelUI.Instance.dHandsInput.text);
        //_defFeet = int.Parse(CharacterPanelUI.Instance.dFeetInput.text);
    }
    private void EquipWeaponArmors() {
        if(!string.IsNullOrEmpty(_weaponName)) {
            Weapon weapon = JsonUtility.FromJson<Weapon>(System.IO.File.ReadAllText(Utilities.dataPath + "Items/WEAPON/" + _weaponName + ".json"));
            EquipItem(weapon);
        }
        if (!string.IsNullOrEmpty(_armorName)) {
            Armor armor = JsonUtility.FromJson<Armor>(System.IO.File.ReadAllText(Utilities.dataPath + "Items/ARMOR/" + _armorName + ".json"));
            EquipItem(armor);
        }
        if (!string.IsNullOrEmpty(_accessoryName)) {
            Item item = JsonUtility.FromJson<Item>(System.IO.File.ReadAllText(Utilities.dataPath + "Items/ACCESSORY/" + _accessoryName + ".json"));
            EquipItem(item);
        }
        if (!string.IsNullOrEmpty(_consumableName)) {
            Item item = JsonUtility.FromJson<Item>(System.IO.File.ReadAllText(Utilities.dataPath + "Items/CONSUMABLE/" + _consumableName + ".json"));
            EquipItem(item);
        }
    }

    #region Interface
    public void SetSide(SIDES side) {
        _currentSide = side;
    }
    public void SetRowNumber(int row) {
        _currentRow = row;
    }
    public void ResetToFullHP() {
        AdjustHP(_singleMaxHP);
        _isDead = false;
    }
    public void ResetToFullSP() {
        AdjustSP(_maxSP);
    }
    public void AdjustHP(int amount, ICharacter killer = null) {
        int previous = this._currentHP;
        this._currentHP += amount;
        this._currentHP = Mathf.Clamp(this._currentHP, 0, _maxHP);
        if (previous != this._currentHP) {
            if (this._currentHP == 0) {
                DeathSim();
            }
        }
    }
    public void AdjustSP(int amount) {
        _currentSP += amount;
        _currentSP = Mathf.Clamp(_currentSP, 0, _maxSP);
    }
    public void DeathSim() {
        _isDead = true;
        CombatSimManager.Instance.currentCombat.CharacterDeath(this);
    }
    public void EnableDisableSkills(CombatSim combatSim) {
        //Body part skills / general skills
        for (int i = 0; i < this._skills.Count; i++) {
            Skill skill = this._skills[i];
            skill.isEnabled = true;
            if (skill is FleeSkill) {
                skill.isEnabled = false;
            }
        }
    }
    #endregion

    #region Utilities
    private void ConstructClass() {
        string path = Utilities.dataPath + "CharacterClasses/" + _className + ".json";
        _characterClass = JsonUtility.FromJson<CharacterClass>(System.IO.File.ReadAllText(path));
    }
    private void ConstructSkills() {
        _skills = new List<Skill>();
        string path = Utilities.dataPath + "Skills/" + _skillName + ".json";
        Skill skill = JsonUtility.FromJson<Skill>(System.IO.File.ReadAllText(path));
        _skills.Add(skill);
        //string path = string.Empty;
        //path = Utilities.dataPath + "Skills/GENERAL/";
        //string[] directories = Directory.GetDirectories(path);
        //for (int i = 0; i < directories.Length; i++) {
        //    string skillType = new DirectoryInfo(directories[i]).Name;
        //    SKILL_TYPE currSkillType = (SKILL_TYPE) System.Enum.Parse(typeof(SKILL_TYPE), skillType);
        //    string[] files = Directory.GetFiles(directories[i], "*.json");
        //    for (int j = 0; j < files.Length; j++) {
        //        string dataAsJson = File.ReadAllText(files[j]);
        //        switch (currSkillType) {
        //            case SKILL_TYPE.ATTACK:
        //            AttackSkill attackSkill = JsonUtility.FromJson<AttackSkill>(dataAsJson);
        //            _skills.Add(attackSkill);
        //            break;
        //            case SKILL_TYPE.HEAL:
        //            HealSkill healSkill = JsonUtility.FromJson<HealSkill>(dataAsJson);
        //            _skills.Add(healSkill);
        //            break;
        //            case SKILL_TYPE.OBTAIN_ITEM:
        //            ObtainSkill obtainSkill = JsonUtility.FromJson<ObtainSkill>(dataAsJson);
        //            _skills.Add(obtainSkill);
        //            break;
        //            case SKILL_TYPE.FLEE:
        //            break;
        //            case SKILL_TYPE.MOVE:
        //            MoveSkill moveSkill = JsonUtility.FromJson<MoveSkill>(dataAsJson);
        //            _skills.Add(moveSkill);
        //            break;
        //        }
        //    }
        //}
    }
    private void AllocateStats() {
        _singleAttackPower = _raceSetting.baseAttackPower;
        _singleSpeed = _raceSetting.baseSpeed;
        _singleMaxHP = _raceSetting.baseHP;
        //_sp = characterClass.baseSP;
    }
    private void LevelUp() {
        int multiplier = _level - 1;
        if (multiplier < 0) {
            multiplier = 0;
        }
        _singleAttackPower += (multiplier * (int) ((characterClass.attackPowerPerLevel / 100f) * (float) _raceSetting.baseAttackPower));
        _singleSpeed += (multiplier * (int) ((characterClass.speedPerLevel / 100f) * (float) _raceSetting.baseSpeed));
        _singleMaxHP += (multiplier * (int) ((characterClass.hpPerLevel / 100f) * (float) _raceSetting.baseHP));
        //_sp += ((int)multiplier * characterClass.spPerLevel);

        //Add stats per level from race
        if (level > 1) {
            int hpIndex = level % _raceSetting.hpPerLevel.Length;
            hpIndex = hpIndex == 0 ? _raceSetting.hpPerLevel.Length : hpIndex;
            int attackIndex = level % _raceSetting.attackPerLevel.Length;
            attackIndex = attackIndex == 0 ? _raceSetting.attackPerLevel.Length : attackIndex;

            _singleMaxHP += _raceSetting.hpPerLevel[hpIndex - 1];
            _singleAttackPower += _raceSetting.attackPerLevel[attackIndex - 1];
        }
    }
    private void ArmyModifier() {
        if (_isArmy) {
            _attackPower = _singleAttackPower * _armyCount;
            _speed = _singleSpeed * _armyCount;
            _maxHP = _singleMaxHP * _armyCount;
        } else {
            _attackPower = _singleAttackPower;
            _speed = _singleSpeed;
            _maxHP = _singleMaxHP;
        }
        _currentHP = _maxHP;
    }
    #endregion

    #region Equipment
    public bool EquipItem(Item item) {
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
                CombatAttribute newCombatAttribute = AttributeManager.Instance.allCombatAttributes[item.attributeNames[i]];
                RemoveCombatAttribute(newCombatAttribute);
            }
        }
    }
    public bool TryEquipWeapon(Weapon weapon) {
        _equippedWeapon = weapon;
        weapon.SetEquipped(true);
        return true;
    }
    public bool TryEquipArmor(Armor armor) {
        _equippedArmor = armor;
        armor.SetEquipped(true);
        return true;
    }
    //Try to equip an accessory
    internal bool TryEquipAccessory(Item accessory) {
        accessory.SetEquipped(true);
        _equippedAccessory = accessory;
        return true;
    }
    //Try to equip an consumable
    internal bool TryEquipConsumable(Item consumable) {
        consumable.SetEquipped(true);
        _equippedConsumable = consumable;
        return true;
    }
    private void UnequipWeapon(Weapon weapon) {
        _equippedWeapon = null;
        weapon.SetEquipped(false);
    }
    private void UnequipArmor(Armor armor) {
        _equippedArmor = null;
        armor.SetEquipped(false);
    }
    //Unequips accessory of a character
    private void UnequipAccessory(Item accessory) {
        accessory.SetEquipped(false);
        _equippedAccessory = null;
    }
    //Unequips consumable of a character
    private void UnequipConsumable(Item consumable) {
        consumable.SetEquipped(false);
        _equippedConsumable = null;
    }
    #endregion

    #region Attributes
    public Attribute GetAttribute(string attribute) {
        for (int i = 0; i < _attributes.Count; i++) {
            if (_attributes[i].name.ToLower() == attribute.ToLower()) {
                return _attributes[i];
            }
        }
        return null;
    }
    public void AddCombatAttribute(CombatAttribute combatAttribute) {
        if (string.IsNullOrEmpty(GetCombatAttribute(combatAttribute.name).name)) {
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
            if (_combatAttributes[i].name == attributeName) {
                return _combatAttributes[i];
            }
        }
        return new CombatAttribute();
    }
    private void ApplyCombatAttributeEffects(CombatAttribute combatAttribute) {
        if (!combatAttribute.hasRequirement) {
            if (combatAttribute.stat == STAT.ATTACK) {
                if (combatAttribute.isPercentage) {
                    float result = _singleAttackPower * (combatAttribute.amount / 100f);
                    _singleAttackPower += (int) result;
                } else {
                    _singleAttackPower += (int) combatAttribute.amount;
                }
            } else if (combatAttribute.stat == STAT.HP) {
                int previousMaxHP = _singleMaxHP;
                if (combatAttribute.isPercentage) {
                    float result = _singleMaxHP * (combatAttribute.amount / 100f);
                    _singleMaxHP += (int) result;
                } else {
                    _singleMaxHP += (int) combatAttribute.amount;
                }
                //if (_currentHP > _maxHP || _currentHP == previousMaxHP) {
                //    _currentHP = _singleMaxHP;
                //}
            } else if (combatAttribute.stat == STAT.SPEED) {
                if (combatAttribute.isPercentage) {
                    float result = _singleSpeed * (combatAttribute.amount / 100f);
                    _singleSpeed += (int) result;
                } else {
                    _singleSpeed += (int) combatAttribute.amount;
                }
            }
            ArmyModifier();
        }
    }
    private void UnapplyCombatAttributeEffects(CombatAttribute combatAttribute) {
        if (!combatAttribute.hasRequirement) {
            if (combatAttribute.stat == STAT.ATTACK) {
                if (combatAttribute.isPercentage) {
                    float result = _singleAttackPower * (combatAttribute.amount / 100f);
                    _singleAttackPower -= (int) result;
                } else {
                    _singleAttackPower -= (int) combatAttribute.amount;
                }
            } else if (combatAttribute.stat == STAT.HP) {
                int previousMaxHP = _singleMaxHP;
                if (combatAttribute.isPercentage) {
                    float result = _singleMaxHP * (combatAttribute.amount / 100f);
                    _singleMaxHP -= (int) result;
                } else {
                    _singleMaxHP -= (int) combatAttribute.amount;
                }
                if (_currentHP > _singleMaxHP || _currentHP == previousMaxHP) {
                    _currentHP = _singleMaxHP;
                }
            } else if (combatAttribute.stat == STAT.SPEED) {
                if (combatAttribute.isPercentage) {
                    float result = _singleSpeed * (combatAttribute.amount / 100f);
                    _singleSpeed -= (int) result;
                } else {
                    _singleSpeed -= (int) combatAttribute.amount;
                }
            }
            ArmyModifier();
        }
    }
    #endregion
}
