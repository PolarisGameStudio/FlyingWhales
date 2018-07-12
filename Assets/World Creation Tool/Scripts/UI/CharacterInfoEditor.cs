﻿using BayatGames.SaveGameFree;
using ECS;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using UnityEngine.UI.Extensions.ColorPicker;

namespace worldcreator {
    public class CharacterInfoEditor : MonoBehaviour {

        private Character _character;

        [Header("Portrait Settings")]
        [SerializeField] private CharacterPortrait portrait;
        [SerializeField] private Dropdown templatesDropdown;

        [Header("Basic Info")]
        [SerializeField] private InputField nameField;
        [SerializeField] private Dropdown raceField;
        [SerializeField] private Dropdown genderField;
        [SerializeField] private Dropdown roleField;
        [SerializeField] private Dropdown jobField;
        [SerializeField] private Dropdown classField;
        [SerializeField] private Dropdown factionField;
        [SerializeField] private Text otherInfoLbl;

        [Header("Relationship Info")]
        [SerializeField] private GameObject relationshipItemPrefab;
        [SerializeField] private ScrollRect relationshipScrollView;
        [SerializeField] private Dropdown charactersRelationshipDropdown;
        [SerializeField] private Button createRelationshipBtn;

        [Header("Equipment Info")]
        [SerializeField] private GameObject itemEditorPrefab;
        [SerializeField] private ScrollRect equipmentScrollView;
        [SerializeField] private Dropdown equipmentChoicesDropdown;
        [SerializeField] private Button addEquipmentBtn;

        [Header("Inventory Info")]
        [SerializeField] private ScrollRect inventoryScrollView;
        [SerializeField] private Dropdown inventoryChoicesDropdown;
        [SerializeField] private Button addInventoryBtn;

        public Dictionary<string, PortraitSettings> portraitTemplates;

        public void Initialize() {
            Messenger.AddListener<Relationship>(Signals.RELATIONSHIP_CREATED, OnRelationshipCreated);
            Messenger.AddListener<Relationship>(Signals.RELATIONSHIP_REMOVED, OnRelationshipRemoved);

            LoadEquipmentChoices();
            LoadInventoryChoices();
        }

        public void UpdateInfo() {
            if (_character != null) {
                ShowCharacterInfo(_character);
            }
        }

        public void ShowCharacterInfo(Character character) {
            _character = character;
            portrait.GeneratePortrait(_character, IMAGE_SIZE.X256);
            LoadDropdownOptions();
            //UpdatePortraitControls();
            UpdateBasicInfo();
            LoadRelationships();
            LoadCharacters();
            LoadEquipment();
            LoadInventory();
            LoadTemplateChoices();
            Messenger.AddListener<Item, Character>(Signals.ITEM_EQUIPPED, OnItemEquipped);
            Messenger.AddListener<Item, Character>(Signals.ITEM_UNEQUIPPED, OnItemUnequipped);
            Messenger.AddListener<Item, Character>(Signals.ITEM_OBTAINED, OnItemObtained);
            Messenger.AddListener<Item, Character>(Signals.ITEM_THROWN, OnItemThrown);
            this.gameObject.SetActive(true);
        }
        public void Close() {
            Messenger.RemoveListener<Item, Character>(Signals.ITEM_EQUIPPED, OnItemEquipped);
            Messenger.RemoveListener<Item, Character>(Signals.ITEM_UNEQUIPPED, OnItemUnequipped);
            Messenger.RemoveListener<Item, Character>(Signals.ITEM_OBTAINED, OnItemObtained);
            Messenger.RemoveListener<Item, Character>(Signals.ITEM_THROWN, OnItemThrown);
            this.gameObject.SetActive(false);
        }

        #region Portrait Editor
        public void LoadTemplateChoices() {
            if (_character == null) {
                return;
            }
            portraitTemplates = new Dictionary<string, PortraitSettings>();
            string path = Utilities.portraitsSavePath + _character.raceSetting.race + "/" + _character.gender.ToString() + "/";
            Directory.CreateDirectory(path);
            DirectoryInfo info = new DirectoryInfo(path);
            FileInfo[] files = info.GetFiles("*" + Utilities.portraitFileExt);
            for (int i = 0; i < files.Length; i++) {
                FileInfo currInfo = files[i];
                portraitTemplates.Add(currInfo.Name, SaveGame.Load<PortraitSettings>(currInfo.FullName));
            }
            templatesDropdown.ClearOptions();
            templatesDropdown.AddOptions(portraitTemplates.Keys.ToList());
        }
        public void OnValueChangedPortraitTemplate(int choice) {
            string chosenTemplateName = templatesDropdown.options[choice].text;
            PortraitSettings chosenSettings = portraitTemplates[chosenTemplateName];
            _character.SetPortraitSettings(chosenSettings);
            portrait.GeneratePortrait(_character, IMAGE_SIZE.X256);
        }
        //public void ApplyPortraitTemplate() {
        //    string chosenTemplateName = templatesDropdown.options[templatesDropdown.value].text;
        //    PortraitSettings chosenSettings = portraitTemplates[chosenTemplateName];
        //    _character.SetPortraitSettings(chosenSettings);
        //    portrait.GeneratePortrait(_character);
        //}
        #endregion

        #region Basic Info
        private void LoadDropdownOptions() {
            raceField.ClearOptions();
            genderField.ClearOptions();
            roleField.ClearOptions();
            jobField.ClearOptions();
            classField.ClearOptions();

            raceField.AddOptions(Utilities.GetEnumChoices<RACE>());
            genderField.AddOptions(Utilities.GetEnumChoices<GENDER>());
            roleField.AddOptions(Utilities.GetEnumChoices<CHARACTER_ROLE>());
            //jobField.AddOptions(Utilities.GetEnumChoices<CHARACTER_JOB>(true));
            classField.AddOptions(Utilities.GetFileChoices(Utilities.dataPath + "CharacterClasses/", "*.json"));
            LoadFactionDropdownOptions();
        }
        public void LoadFactionDropdownOptions() {
            factionField.ClearOptions();
            List<string> options = new List<string>();
            options.Add("Factionless");
            options.AddRange(FactionManager.Instance.allFactions.Select(x => x.name).ToList());
            factionField.AddOptions(options);
        }
        public void UpdateBasicInfo() {
            nameField.text = _character.name;
            raceField.value = Utilities.GetOptionIndex(raceField, _character.raceSetting.race.ToString());
            genderField.value = Utilities.GetOptionIndex(genderField, _character.gender.ToString());
            roleField.value = Utilities.GetOptionIndex(roleField, _character.role.roleType.ToString());
            //if (_character.role.job != null) {
            //    jobField.value = Utilities.GetOptionIndex(jobField, _character.role.job.jobType.ToString());
            //} else {
            //    jobField.value = Utilities.GetOptionIndex(jobField, CHARACTER_JOB.NONE.ToString());
            //}
            
            classField.value = Utilities.GetOptionIndex(classField, _character.characterClass.className);
            string factionName = "Factionless";
            if (_character.faction != null) {
                factionName = _character.faction.name;
            }
            factionField.value = Utilities.GetOptionIndex(factionField, factionName);
            otherInfoLbl.text = string.Empty;
            if (_character.home == null) {
                otherInfoLbl.text += "Home: NONE";
            } else {
                otherInfoLbl.text += "Home Area: " + _character.home.name.ToString();
                if (_character.homeLandmark != null) {
                    otherInfoLbl.text += "(" + _character.homeLandmark.landmarkName + ")";
                }
            }
            if (_character.party.specificLocation == null) {
                otherInfoLbl.text += "\nLocation: NONE";
            } else {
                otherInfoLbl.text += "\nLocation: " + _character.party.specificLocation.ToString();
            }
        }
        public void SetName(string newName) {
            _character.SetName(newName);
        }
        public void SetRace(int choice) {
            RACE newRace = (RACE)Enum.Parse(typeof(RACE), raceField.options[choice].text);
            _character.ChangeRace(newRace);
        }
        public void SetGender(int choice) {
            GENDER newGender = (GENDER)Enum.Parse(typeof(GENDER), genderField.options[choice].text);
            _character.ChangeGender(newGender);
        }
        public void SetRole(int choice) {
            CHARACTER_ROLE newRole = (CHARACTER_ROLE)Enum.Parse(typeof(CHARACTER_ROLE), roleField.options[choice].text);
            //CHARACTER_JOB previousJob = CHARACTER_JOB.NONE;
            //if (_character.role.job != null) {
            //    previousJob = _character.role.job.jobType;
            //}
            _character.AssignRole(newRole);
            //if (previousJob != CHARACTER_JOB.NONE) {
            //    SetJob(Utilities.GetOptionIndex(jobField, previousJob.ToString())); //to recreate the job for the new role
            //}
        }
        //public void SetJob(int choice) {
        //    CHARACTER_JOB newJob = (CHARACTER_JOB)Enum.Parse(typeof(CHARACTER_JOB), jobField.options[choice].text);
        //    _character.role.AssignJob(newJob);
        //}
        public void SetClass(int choice) {
            string newClass = classField.options[choice].text;
            _character.ChangeClass(newClass);
        }
        public void SetFaction(int choice) {
            string factionName = factionField.options[choice].text;
            Faction faction = FactionManager.Instance.GetFactionBasedOnName(factionName);
            if (_character.faction != null) {
                _character.faction.RemoveCharacter(_character);
            }
            _character.SetFaction(faction);
            if (faction != null) {
                faction.AddNewCharacter(_character);
            }
            WorldCreatorUI.Instance.editFactionsMenu.UpdateItems();
        }
        #endregion

        #region Relationship Info
        private void LoadRelationships() {
            Transform[] children = Utilities.GetComponentsInDirectChildren<Transform>(relationshipScrollView.content.gameObject);
            for (int i = 0; i < children.Length; i++) {
                GameObject.Destroy(children[i].gameObject);
            }
            foreach (KeyValuePair<Character, Relationship> kvp in _character.relationships) {
                GameObject relItemGO = GameObject.Instantiate(relationshipItemPrefab, relationshipScrollView.content);
                RelationshipEditorItem relItem = relItemGO.GetComponent<RelationshipEditorItem>();
                relItem.SetRelationship(kvp.Value);
            }
        }
        public void LoadCharacters() {
            List<string> options = new List<string>();
            charactersRelationshipDropdown.ClearOptions();
            for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
                Character currCharacter = CharacterManager.Instance.allCharacters[i];
                if (currCharacter.id != _character.id && _character.GetRelationshipWith(currCharacter) == null) {
                    options.Add(currCharacter.name);
                }
            }
            charactersRelationshipDropdown.AddOptions(options);
            if (charactersRelationshipDropdown.options.Count == 0) {
                createRelationshipBtn.interactable = false;
            } else {
                createRelationshipBtn.interactable = true;
            }
        }
        public void CreateRelationship() {
            string chosenCharacterName = charactersRelationshipDropdown.options[charactersRelationshipDropdown.value].text;
            Character chosenCharacter = CharacterManager.Instance.GetCharacterByName(chosenCharacterName);
            CharacterManager.Instance.CreateNewRelationshipTowards(_character, chosenCharacter);
        }
        private void OnRelationshipCreated(Relationship newRel) {
            if (_character == null) {
                return;
            }
            GameObject relItemGO = GameObject.Instantiate(relationshipItemPrefab, relationshipScrollView.content);
            RelationshipEditorItem relItem = relItemGO.GetComponent<RelationshipEditorItem>();
            relItem.SetRelationship(newRel);
            LoadCharacters();
        }
        public void OnRelationshipRemoved(Relationship removedRel) {
            if (_character == null || !this.gameObject.activeSelf) {
                return;
            }
            RelationshipEditorItem itemToRemove = GetRelationshipItem(removedRel);
            if (itemToRemove != null) {
                GameObject.Destroy(itemToRemove.gameObject);
                LoadCharacters();
            }
        }
        private RelationshipEditorItem GetRelationshipItem(Relationship rel) {
            RelationshipEditorItem[] children = Utilities.GetComponentsInDirectChildren<RelationshipEditorItem>(relationshipScrollView.content.gameObject);
            for (int i = 0; i < children.Length; i++) {
                RelationshipEditorItem currItem = children[i];
                if (currItem.relationship == rel) {
                    return currItem;
                }
            }
            return null;
        }
        #endregion

        #region Equipment Info
        private void LoadEquipmentChoices() {
            List<string> choices = new List<string>();
            choices.AddRange(ItemManager.Instance.allWeapons.Keys);
            choices.AddRange(ItemManager.Instance.allArmors.Keys);

            equipmentChoicesDropdown.AddOptions(choices);
        }
        private void LoadEquipment() {
            Utilities.DestroyChildren(equipmentScrollView.content);
            for (int i = 0; i < _character.equippedItems.Count; i++) {
                Item currItem = _character.equippedItems[i];
                OnItemEquipped(currItem, _character);
            }
        }
        public void AddEquipment() {
            string chosenItem = equipmentChoicesDropdown.options[equipmentChoicesDropdown.value].text;
            Item item = ItemManager.Instance.allItems[chosenItem].CreateNewCopy();
            if (!_character.EquipItem(item)) {
                WorldCreatorUI.Instance.messageBox.ShowMessageBox(MESSAGE_BOX.OK, "Equipment error", "Cannot equip " + item.itemName);
            }
        }
        private void OnItemEquipped(Item item, Character character) {
            GameObject itemGO = GameObject.Instantiate(itemEditorPrefab, equipmentScrollView.content);
            ItemEditorItem itemComp = itemGO.GetComponent<ItemEditorItem>();
            itemComp.SetItem(item, character);
            itemComp.SetDeleteItemAction(() => character.UnequipItem(item));
        }
        private void OnItemUnequipped(Item item, Character character) {
            GameObject.Destroy(GetEquipmentEditorItem(item).gameObject);
        }
        private ItemEditorItem GetEquipmentEditorItem(Item item) {
            ItemEditorItem[] children = Utilities.GetComponentsInDirectChildren<ItemEditorItem>(equipmentScrollView.content.gameObject);
            for (int i = 0; i < children.Length; i++) {
                ItemEditorItem currItem = children[i];
                if (currItem.item == item) {
                    return currItem;
                }
            }
            return null;
        }
        #endregion

        #region Inventory Info
        private void LoadInventoryChoices() {
            List<string> choices = new List<string>(ItemManager.Instance.allItems.Keys);
            inventoryChoicesDropdown.AddOptions(choices);
        }
        private void LoadInventory() {
            Utilities.DestroyChildren(inventoryScrollView.content);
            for (int i = 0; i < _character.inventory.Count; i++) {
                Item currItem = _character.inventory[i];
                OnItemObtained(currItem, _character);
            }
        }
        public void AddInventory() {
            string chosenItem = inventoryChoicesDropdown.options[inventoryChoicesDropdown.value].text;
            Item item = ItemManager.Instance.allItems[chosenItem].CreateNewCopy();
            _character.PickupItem(item);
        }
        private void OnItemObtained(Item item, Character character) {
            GameObject itemGO = GameObject.Instantiate(itemEditorPrefab, inventoryScrollView.content);
            ItemEditorItem itemComp = itemGO.GetComponent<ItemEditorItem>();
            itemComp.SetItem(item, character);
            itemComp.SetDeleteItemAction(() => _character.ThrowItem(item));
        }
        private void OnItemThrown(Item item, Character character) {
            GameObject.Destroy(GetInventoryEditorItem(item).gameObject);
        }
        private ItemEditorItem GetInventoryEditorItem(Item item) {
            ItemEditorItem[] children = Utilities.GetComponentsInDirectChildren<ItemEditorItem>(inventoryScrollView.content.gameObject);
            for (int i = 0; i < children.Length; i++) {
                ItemEditorItem currItem = children[i];
                if (currItem.item == item) {
                    return currItem;
                }
            }
            return null;
        }
        #endregion
    }
}

