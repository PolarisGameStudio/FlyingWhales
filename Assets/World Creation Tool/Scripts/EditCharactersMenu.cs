﻿using ECS;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace worldcreator {
    public class EditCharactersMenu : MonoBehaviour {

        [SerializeField] private GameObject characterItemPrefab;
        [SerializeField] private Dropdown raceDropdown;
        [SerializeField] private Dropdown genderDropdown;
        [SerializeField] private Dropdown roleDropdown;
        [SerializeField] private Dropdown classDropdown;
        [SerializeField] private ScrollRect charactersScrollView;

        [SerializeField] private CharacterInfoEditor _characterInfoEditor;

        #region getters/setter
        public CharacterInfoEditor characterInfoEditor {
            get { return _characterInfoEditor; }
        }
        #endregion

        public void Initialize() {
            Messenger.AddListener<ECS.Character>(Signals.CHARACTER_CREATED, OnCreateNewCharacter);
            Messenger.AddListener<ECS.Character>(Signals.CHARACTER_REMOVED, OnCharacterRemoved);
            PopulateDropdowns();
        }

        #region Character Creation
        public void CreateNewCharacter() {
            RACE race = (RACE)Enum.Parse(typeof(RACE), raceDropdown.options[raceDropdown.value].text);
            GENDER gender = (GENDER)Enum.Parse(typeof(GENDER), genderDropdown.options[genderDropdown.value].text);
            CHARACTER_ROLE role = (CHARACTER_ROLE)Enum.Parse(typeof(CHARACTER_ROLE), roleDropdown.options[roleDropdown.value].text);
            string className = classDropdown.options[classDropdown.value].text;

            ECS.CharacterSetup setup = ECS.CombatManager.Instance.GetBaseCharacterSetup(className, race);
            ECS.Character newCharacter = CharacterManager.Instance.CreateNewCharacter(role, gender, setup);
            //Debug.Log("Created new character " + newCharacter.name + "")
        }
        private void OnCreateNewCharacter(Character newCharacter) {
            GameObject characterItemGO = GameObject.Instantiate(characterItemPrefab, charactersScrollView.content.transform);
            CharacterEditorItem characterItem = characterItemGO.GetComponent<CharacterEditorItem>();
            characterItem.SetCharacter(newCharacter);
            characterItem.SetEditAction(() => ShowCharacterInfoEditor(newCharacter));
            if (characterInfoEditor.gameObject.activeSelf) {
                characterInfoEditor.LoadCharacters();
            }
        }
        private void OnCharacterRemoved(Character characterToBeRemoved) {
            CharacterEditorItem characterItem = GetCharacterItem(characterToBeRemoved);
            GameObject.Destroy(characterItem.gameObject);
        }
        private CharacterEditorItem GetCharacterItem(ECS.Character character) {
            Transform[] children = Utilities.GetComponentsInDirectChildren<Transform>(charactersScrollView.content.gameObject);
            for (int i = 0; i < children.Length; i++) {
                CharacterEditorItem item = children[i].gameObject.GetComponent<CharacterEditorItem>();
                if (item.character.id == character.id) {
                    return item;
                }
            }
            return null;
        }
        #endregion

        #region Dropdown Data
        private void PopulateDropdowns() {
            classDropdown.ClearOptions();
            raceDropdown.ClearOptions();
            genderDropdown.ClearOptions();
            roleDropdown.ClearOptions();
            classDropdown.AddOptions(Utilities.GetFileChoices(Utilities.dataPath + "CharacterClasses/", "*.json"));
            raceDropdown.AddOptions(Utilities.GetEnumChoices<RACE>());
            genderDropdown.AddOptions(Utilities.GetEnumChoices<GENDER>());
            roleDropdown.AddOptions(Utilities.GetEnumChoices<CHARACTER_ROLE>());
        }
        #endregion

        #region Character Info Editor
        private void ShowCharacterInfoEditor(Character character) {
            characterInfoEditor.ShowCharacterInfo(character);
        }
        public void OnPortraitTemplatesChanged() {
            characterInfoEditor.LoadTemplateChoices();
        }
        #endregion
    }
}

