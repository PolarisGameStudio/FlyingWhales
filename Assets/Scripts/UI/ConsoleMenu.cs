﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

using TMPro;
using UnityEngine.UI;

public class ConsoleMenu : UIMenu {

    private Dictionary<string, Action<string[]>> _consoleActions;

    private List<string> commandHistory;
    private int currentHistoryIndex;

    [SerializeField] private Text consoleLbl;
    [SerializeField] private InputField consoleInputField;

    [SerializeField] private GameObject commandHistoryGO;
    [SerializeField] private TextMeshProUGUI commandHistoryLbl;

    internal override void Initialize() {
        commandHistory = new List<string>();
        _consoleActions = new Dictionary<string, Action<string[]>>() {
            {"/help", ShowHelp},
            {"/change_faction_rel_stat", ChangeFactionRelationshipStatus},
            //{"/force_accept_quest", AcceptQuest},
            {"/kill",  KillCharacter},
            //{"/quest_cancel", CancelQuest},
            //{"/adjust_gold", AdjustGold},
            {"/lfli", LogFactionLandmarkInfo},
            {"/log_actions", LogCharacterActions },
            //{"/adjust_resources", AdjustResources}
            {"/center_character", CenterOnCharacter},
            {"/center_landmark", CenterOnLandmark },
            //{"/l_combat_rooms", LogCombatRooms },
            {"/l_character_location_history", LogCharacterLocationHistory },
            {"/get_path", GetPath },
            {"/get_all_paths", GetAllPaths },
            {"/r_road_highlights", ResetRoadHighlights },
            {"/spawn_obj", SpawnNewObject },
            {"/toggle_road", ToggleRoads },
            {"/set_icon_target", SetIconTarget },
            {"/set_need", SetCharacterNeedsValue},
            {"/add_tag", AddCharacterAttribute},
            {"/log_event_schedule", LogEventSchedule },
            {"/share_intel", ShareIntel },
            {"/show_logs", ShowLogs },
            {"/change_landmark_state", ChangeLandmarkState },
            {"/adjust_faction_favor", AdjustFactionFavor},
        };
    }

    public void ShowConsole() {
        isShowing = true;
        this.gameObject.SetActive(true);
        ClearCommandField();
        consoleInputField.Select();
    }

    public void HideConsole() {
        isShowing = false;
        this.gameObject.SetActive(false);
        //consoleInputField.foc = false;
        HideCommandHistory();
        ClearCommandHistory();
    }
    private void ClearCommandField() {
        consoleLbl.text = string.Empty;
    }
    private void ClearCommandHistory() {
        commandHistoryLbl.text = string.Empty;
        commandHistory.Clear();
    }
    private void ShowCommandHistory() {
        commandHistoryGO.SetActive(true);
    }
    private void HideCommandHistory() {
        commandHistoryGO.SetActive(false);
    }
    public void SubmitCommand() {
        string command = consoleLbl.text;
        string[] words = command.Split(' ');
        string mainCommand = words[0];
        if (_consoleActions.ContainsKey(mainCommand)) {
            _consoleActions[mainCommand](words);
        } else {
            AddCommandHistory(command);
            AddErrorMessage("Error: there is no such command as " + mainCommand + "![-]");
        }
    }
    private void AddCommandHistory(string history) {
        commandHistoryLbl.text += history + "\n";
        commandHistory.Add(history);
        currentHistoryIndex = commandHistory.Count - 1;
        ShowCommandHistory();
    }
    private void AddErrorMessage(string errorMessage) {
        errorMessage += ". Use /help for a list of commands";
        commandHistoryLbl.text += "<color=#FF0000>" + errorMessage + "</color>\n";
        ShowCommandHistory();
    }
    private void AddSuccessMessage(string successMessage) {
        commandHistoryLbl.text += "<color=#00FF00>" + successMessage + "</color>\n";
        ShowCommandHistory();
    }

    private void Update() {
        if (isShowing && consoleInputField.text != "" && Input.GetKeyDown(KeyCode.Return)) {
            SubmitCommand();
        }
        if (Input.GetKeyDown(KeyCode.UpArrow)) {
            int newIndex = currentHistoryIndex - 1;
            string command = commandHistory.ElementAtOrDefault(newIndex);
            if (!string.IsNullOrEmpty(command)) {
                consoleLbl.text = command;
                currentHistoryIndex = newIndex;
            }
        }
        if (Input.GetKeyDown(KeyCode.DownArrow)) {
            int newIndex = currentHistoryIndex + 1;
            string command = commandHistory.ElementAtOrDefault(newIndex);
            if (!string.IsNullOrEmpty(command)) {
                consoleLbl.text = command;
                currentHistoryIndex = newIndex;
            }
        }
    }

    #region Misc
    private void ShowHelp(string[] parameters) {
        for (int i = 0; i < _consoleActions.Count; i++) {
            AddCommandHistory(_consoleActions.Keys.ElementAt(i));
        }
    }
    public void AddText(string text) {
        consoleInputField.text += " " + text;
    }
    #endregion

    #region Faction Relationship
    private void ChangeFactionRelationshipStatus(string[] parameters) {
        if (parameters.Length != 4) {
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of /change_faction_rel_stat");
            return;
        }
        string faction1ParameterString = parameters[1];
        string faction2ParameterString = parameters[2];
        string newRelStatusString = parameters[3];

        Faction faction1;
        Faction faction2;

        int faction1ID = -1;
        int faction2ID = -1;

        bool isFaction1Numeric = int.TryParse(faction1ParameterString, out faction1ID);
        bool isFaction2Numeric = int.TryParse(faction2ParameterString, out faction2ID);

        string faction1Name = parameters[1];
        string faction2Name = parameters[2];

        FACTION_RELATIONSHIP_STATUS newRelStatus;

        if (isFaction1Numeric) {
            faction1 = FactionManager.Instance.GetFactionBasedOnID(faction1ID);
        } else {
            faction1 = FactionManager.Instance.GetFactionBasedOnName(faction1Name);
        }

        if (isFaction2Numeric) {
            faction2 = FactionManager.Instance.GetFactionBasedOnID(faction2ID);
        } else {
            faction2 = FactionManager.Instance.GetFactionBasedOnName(faction2Name);
        }

        try {
            newRelStatus = (FACTION_RELATIONSHIP_STATUS)Enum.Parse(typeof(FACTION_RELATIONSHIP_STATUS), newRelStatusString, true);
        } catch {
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of /change_faction_rel_stat");
            return;
        }

        if (faction1 == null || faction2 == null) {
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of /change_faction_rel_stat");
            return;
        }

        FactionRelationship rel = FactionManager.Instance.GetRelationshipBetween(faction1, faction2);
        rel.ChangeRelationshipStatus(newRelStatus);

        AddSuccessMessage("Changed relationship status of " + faction1.name + " and " + faction2.name + " to " + rel.relationshipStatus.ToString());
    }
    private void AdjustFactionFavor(string[] parameters) {
        if (parameters.Length != 4) {
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of /change_faction_rel_stat");
            return;
        }
        string faction1ParameterString = parameters[1];
        string faction2ParameterString = parameters[2];
        string adjustmentStr = parameters[3];

        Faction faction1;
        Faction faction2;

        int faction1ID = -1;
        int faction2ID = -1;

        bool isFaction1Numeric = int.TryParse(faction1ParameterString, out faction1ID);
        bool isFaction2Numeric = int.TryParse(faction2ParameterString, out faction2ID);

        string faction1Name = parameters[1];
        string faction2Name = parameters[2];

        if (isFaction1Numeric) {
            faction1 = FactionManager.Instance.GetFactionBasedOnID(faction1ID);
        } else {
            faction1 = FactionManager.Instance.GetFactionBasedOnName(faction1Name);
        }

        if (isFaction2Numeric) {
            faction2 = FactionManager.Instance.GetFactionBasedOnID(faction2ID);
        } else {
            faction2 = FactionManager.Instance.GetFactionBasedOnName(faction2Name);
        }

        int adjustment;

        if (!Int32.TryParse(adjustmentStr, out adjustment) || faction1 == null || faction2 == null) {
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of " + parameters[0]);
            return;
        }

        faction1.AdjustFavorFor(faction2, adjustment);

        AddSuccessMessage("Changed favor of " + faction1.name + " towards " + faction2.name + ".New favor is " + faction1.favor[faction2].ToString());
    }
    #endregion

    #region Landmarks
    private void CenterOnLandmark(string[] parameters) {
        if (parameters.Length < 2) {
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of " + parameters[0]);
            return;
        }
        string landmarkParameterString = string.Empty;
        for (int i = 1; i < parameters.Length; i++) {
            landmarkParameterString += parameters[i];
            if (i + 1 < parameters.Length) {
                landmarkParameterString += " ";
            }
        }
        int landmarkID;

        bool isLandmarkParameterNumeric = int.TryParse(landmarkParameterString, out landmarkID);
        BaseLandmark landmark = null;
        if (isLandmarkParameterNumeric) {
            landmark = LandmarkManager.Instance.GetLandmarkByID(landmarkID);
        } else {
            landmark = LandmarkManager.Instance.GetLandmarkByName(landmarkParameterString);
        }

        if (landmark == null) {
            AddErrorMessage("There was an error in the command format of " + parameters[0]);
            return;
        }
        UIManager.Instance.ShowLandmarkInfo(landmark);
        //character.CenterOnCharacter();
    }
    private void ChangeLandmarkState(string[] parameters) {
        if (parameters.Length < 3) {
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of " + parameters[0]);
            return;
        }
        string stateParameterString = parameters[1];
        string landmarkParameterString = string.Empty;
        for (int i = 2; i < parameters.Length; i++) {
            landmarkParameterString += parameters[i];
            if (i + 1 < parameters.Length) {
                landmarkParameterString += " ";
            }
        }
        int landmarkID;

        bool isLandmarkParameterNumeric = int.TryParse(landmarkParameterString, out landmarkID);
        BaseLandmark landmark = null;
        if (isLandmarkParameterNumeric) {
            landmark = LandmarkManager.Instance.GetLandmarkByID(landmarkID);
        } else {
            landmark = LandmarkManager.Instance.GetLandmarkByName(landmarkParameterString);
        }

        if (landmark == null) {
            AddErrorMessage("There was an error in the command format of " + parameters[0]);
            return;
        }
        ObjectState state = landmark.landmarkObj.GetState(stateParameterString);
        if (state == null) {
            AddErrorMessage(landmark.landmarkName + " has no " + stateParameterString + " state!");
            return;
        }
        landmark.landmarkObj.ChangeState(state);
    }
    #endregion

    // #region Quests
    // private void AcceptQuest(string[] parameters) {
    //     if (parameters.Length != 3) {
    //         AddCommandHistory(consoleLbl.text);
    //         AddErrorMessage("There was an error in the command format of /force_accept_quest");
    //         return;
    //     }
    //     string questParameterString = parameters[1];
    //     string characterParameterString = parameters[2];

    //     int questID;
    //     int characterID;

    //     bool isQuestParameterNumeric = int.TryParse(questParameterString, out questID);
    //     bool isCharacterParameterNumeric = int.TryParse(characterParameterString, out characterID);

    //     if (isQuestParameterNumeric && isCharacterParameterNumeric) {
    //         OldQuest.Quest quest = FactionManager.Instance.GetQuestByID(questID);
    //         Character character = CharacterManager.Instance.GetCharacterByID(characterID);

    //         if(character.currentTask != null) {
    //             character.SetTaskToDoNext(quest);
    //             //cancel character's current quest
    //             character.currentTask.EndTask(TASK_STATUS.CANCEL);
    //         } else {
    //	quest.OnChooseTask (character);
    //             quest.PerformTask();
    //         }


    //         AddSuccessMessage(character.name + " has accepted quest " + quest.questName);
    //     } else {
    //         AddCommandHistory(consoleLbl.text);
    //         AddErrorMessage("There was an error in the command format of /force_accept_quest");
    //     }
    // }
    // private void CancelQuest(string[] parameters) {
    //     if (parameters.Length != 2) {
    //         AddCommandHistory(consoleLbl.text);
    //         AddErrorMessage("There was an error in the command format of /quest_cancel");
    //         return;
    //     }
    //     string questParameterString = parameters[1];

    //     int questID;

    //     bool isQuestParameterNumeric = int.TryParse(questParameterString, out questID);
    //     if (isQuestParameterNumeric) {
    //         OldQuest.Quest quest = FactionManager.Instance.GetQuestByID(questID);
    //         quest.GoBackToQuestGiver(TASK_STATUS.CANCEL);

    //AddSuccessMessage(quest.questName + " quest posted at " + quest.postedAt.tileLocation.name + " was cancelled.");
    //     } else {
    //         AddCommandHistory(consoleLbl.text);
    //         AddErrorMessage("There was an error in the command format of /cancel_quest");
    //     }
    // }
    // #endregion

    #region Characters
    private void KillCharacter(string[] parameters) {
        if (parameters.Length < 2) {
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of /kill");
            return;
        }
        string characterParameterString = parameters[1];
        int characterID;

        string characterName = string.Empty;
        for (int i = 1; i < parameters.Length; i++) {
            characterName += parameters[i] + " ";
        }
        characterName = characterName.Trim();

        bool isCharacterParameterNumeric = int.TryParse(characterParameterString, out characterID);

        Character character = null;

        if (isCharacterParameterNumeric) {
            character = CharacterManager.Instance.GetCharacterByID(characterID);
        } else {
            character = CharacterManager.Instance.GetCharacterByName(characterName);
        }

        if (character == null) {
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of /kill");
            return;
        }

        character.Death();
    }
    //private void AdjustGold(string[] parameters) {
    //    if (parameters.Length != 3) {
    //        AddCommandHistory(consoleLbl.text);
    //        AddErrorMessage("There was an error in the command format of /adjust_gold");
    //        return;
    //    }
    //    string characterParameterString = parameters[1];
    //    string goldAdjustmentParamterString = parameters[2];

    //    int characterID;
    //    int goldAdjustment;

    //    bool isCharacterParameterNumeric = int.TryParse(characterParameterString, out characterID);
    //    bool isGoldParameterNumeric = int.TryParse(goldAdjustmentParamterString, out goldAdjustment);
    //    if (isCharacterParameterNumeric && isGoldParameterNumeric) {
    //        Character character = CharacterManager.Instance.GetCharacterByID(characterID);
    //        character.AdjustGold(goldAdjustment);
    //        AddSuccessMessage(character.name + "'s gold was adjusted by " + goldAdjustment.ToString() + ". New gold is " + character.gold.ToString());
    //    } else {
    //        AddCommandHistory(consoleLbl.text);
    //        AddErrorMessage("There was an error in the command format of /adjust_gold");
    //    }
    //}
    private void LogCharacterActions(string[] parameters) {
        if (parameters.Length < 2) {
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of " + parameters[0]);
            return;
        }
        string characterParameterString = string.Empty;
        for (int i = 1; i < parameters.Length; i++) {
            characterParameterString += parameters[i] + " ";
        }
        characterParameterString = characterParameterString.Trim();
        int characterID;

        bool isCharacterParameterNumeric = int.TryParse(characterParameterString, out characterID);
        Character character = null;
        if (isCharacterParameterNumeric) {
            character = CharacterManager.Instance.GetCharacterByID(characterID);
        } else {
            character = CharacterManager.Instance.GetCharacterByName(characterParameterString);
        }

        if (character == null) {
            AddErrorMessage("There was an error in the command format of " + parameters[0]);
            return;
        }

        string text = character.name + "'s Action Log: \n";
        for (int i = 0; i < character.party.actionData.actionHistory.Count; i++) {
            text += character.party.actionData.actionHistory[i] + "\n";
        }
        Debug.Log(text);
        AddSuccessMessage(text);
    }
    private void LogEventSchedule(string[] parameters) {
        if (parameters.Length < 2) {
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of " + parameters[0]);
            return;
        }
        string characterParameterString = string.Empty;
        for (int i = 1; i < parameters.Length; i++) {
            characterParameterString += parameters[i] + " ";
        }
        characterParameterString = characterParameterString.Trim();
        int characterID;

        bool isCharacterParameterNumeric = int.TryParse(characterParameterString, out characterID);
        Character character = null;
        if (isCharacterParameterNumeric) {
            character = CharacterManager.Instance.GetCharacterByID(characterID);
        } else {
            character = CharacterManager.Instance.GetCharacterByName(characterParameterString);
        }

        if (character == null) {
            AddErrorMessage("There was an error in the command format of " + parameters[0]);
            return;
        }

        string text = character.name + "'s Event Schedule: \n";
        text += character.eventSchedule.GetEventScheduleSummary();
        Debug.Log(text);
        AddSuccessMessage(text);
    }
    private void CenterOnCharacter(string[] parameters) {
        if (parameters.Length < 2) {
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of " + parameters[0]);
            return;
        }
        string characterParameterString = string.Empty;
        for (int i = 1; i < parameters.Length; i++) {
            characterParameterString += parameters[i];
            if (i + 1 < parameters.Length) {
                characterParameterString += " ";
            }
        }
        int characterID;

        bool isCharacterParameterNumeric = int.TryParse(characterParameterString, out characterID);
        Character character = null;
        if (isCharacterParameterNumeric) {
            character = CharacterManager.Instance.GetCharacterByID(characterID);
        } else {
            character = CharacterManager.Instance.GetCharacterByName(characterParameterString);
        }

        if (character == null) {
            AddErrorMessage("There was an error in the command format of " + parameters[0]);
            return;
        }
        UIManager.Instance.ShowCharacterInfo(character);
        //character.CenterOnCharacter();
    }
    private void LogCharacterLocationHistory(string[] parameters) {
        if (parameters.Length != 2) {
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of " + parameters[0]);
            return;
        }
        string characterParameterString = parameters[1];
        int characterID;

        bool isCharacterParameterNumeric = int.TryParse(characterParameterString, out characterID);
        Character character = null;
        if (isCharacterParameterNumeric) {
            character = CharacterManager.Instance.GetCharacterByID(characterID);
        } else {
            character = CharacterManager.Instance.GetCharacterByName(characterParameterString);
        }

        if (character == null) {
            AddErrorMessage("There was an error in the command format of " + parameters[0]);
            return;
        }

        string text = character.name + "'s Location History: ";
        //for (int i = 0; i < character.specificLocationHistory.Count; i++) {
        //    text += "\n" + character.specificLocationHistory[i];
        //}
        Debug.Log(text);
        string fileLocation = Utilities.dataPath + "Logs/" + character.name + "'s_Location_History.txt";
        System.IO.File.WriteAllText(fileLocation, text);
        AddSuccessMessage("Logged " + character.name + "'s location history in console. And created text file of log at " + fileLocation);
    }
    private void SetCharacterNeedsValue(string[] parameters) {
        if (parameters.Length < 4) {//command, need type, need value, character name/id
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of /kill");
            return;
        }
        string characterParameterString = string.Empty;
        int characterID;
        for (int i = 3; i < parameters.Length; i++) {
            characterParameterString += parameters[i] + " ";
        }
        characterParameterString = characterParameterString.Trim();

        bool isCharacterParameterNumeric = int.TryParse(characterParameterString, out characterID);
        Character character;
        if (isCharacterParameterNumeric) {
            character = CharacterManager.Instance.GetCharacterByID(characterID);
        } else {
            character = CharacterManager.Instance.GetCharacterByName(characterParameterString);
        }

        NEEDS need = (NEEDS)Enum.Parse(typeof(NEEDS), parameters[1]);
        float needValue = float.Parse(parameters[2]);

        if (character == null) {
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of " + parameters[0]);
        }

        character.role.SetNeedValue(need, needValue);
        AddSuccessMessage("Set " + character.name + "'s " + need.ToString() + " to " + character.role.GetNeedValue(need).ToString());
    }
    private void AddCharacterAttribute(string[] parameters) {
        if (parameters.Length < 3) {//command, need type, need value, character name/id
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of " + parameters[0]);
            return;
        }
        string characterParameterString = string.Empty;
        int characterID;
        for (int i = 2; i < parameters.Length; i++) {
            characterParameterString += parameters[i] + " ";
        }
        characterParameterString = characterParameterString.Trim();

        bool isCharacterParameterNumeric = int.TryParse(characterParameterString, out characterID);
        Character character;
        if (isCharacterParameterNumeric) {
            character = CharacterManager.Instance.GetCharacterByID(characterID);
        } else {
            character = CharacterManager.Instance.GetCharacterByName(characterParameterString);
        }
        
        ATTRIBUTE tag = (ATTRIBUTE)Enum.Parse(typeof(ATTRIBUTE), parameters[1]);

        if (character == null) {
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of " + parameters[0]);
        }

        character.AddAttribute(tag);
        AddSuccessMessage("Added " + tag.ToString() + " tag to " + character.name);
    }
    private void ShareIntel(string[] parameters) {
        if (parameters.Length < 3) {
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of " + parameters[0]);
            return;
        }
        string intelIDParameterString = parameters[1];
        string characterParameterString = string.Empty;
        for (int i = 2; i < parameters.Length; i++) {
            characterParameterString += parameters[i] + " ";
        }
        characterParameterString = characterParameterString.Trim();
        int characterID;
        int intelID;

        bool isCharacterParameterNumeric = int.TryParse(characterParameterString, out characterID);
        Character character = null;
        if (isCharacterParameterNumeric) {
            character = CharacterManager.Instance.GetCharacterByID(characterID);
        } else {
            character = CharacterManager.Instance.GetCharacterByName(characterParameterString);
        }

        Intel intel = null;
        bool isIntelParameterNumeric = int.TryParse(intelIDParameterString, out intelID);
        if (isIntelParameterNumeric) {
            //intel = IntelManager.Instance.GetIntel(intelID);
        }

        if (character == null || intel == null) {
            AddErrorMessage("There was an error in the command format of " + parameters[0]);
            return;
        }

        //PlayerManager.Instance.player.GiveIntelToCharacter(intel, character);
        //AddSuccessMessage("Gave intel that " + intel.description + " to " + character.name);
    }
    private void ShowLogs(string[] parameters) {
        if (parameters.Length < 2) {
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of " + parameters[0]);
            return;
        }
        string characterParameterString = string.Empty;
        for (int i = 1; i < parameters.Length; i++) {
            characterParameterString += parameters[i] + " ";
        }
        characterParameterString = characterParameterString.Trim();
        int characterID;

        bool isCharacterParameterNumeric = int.TryParse(characterParameterString, out characterID);
        Character character = null;
        if (isCharacterParameterNumeric) {
            character = CharacterManager.Instance.GetCharacterByID(characterID);
        } else {
            character = CharacterManager.Instance.GetCharacterByName(characterParameterString);
        }

        if (character == null) {
            AddErrorMessage("There was an error in the command format of " + parameters[0]);
            return;
        }

        string logSummary = character.name + "'s logs: ";
        List<string> logs = CharacterManager.Instance.GetCharacterLogs(character);
        for (int i = 0; i < logs.Count; i++) {
            logSummary += "\n" + logs[i];
        }
        AddSuccessMessage(logSummary);
    }
    #endregion

    #region Resources
    //private void AdjustResources(string[] parameters) {
    //    if (parameters.Length != 4) { //command, landmark, material, adjustment 
    //        AddCommandHistory(consoleLbl.text);
    //        AddErrorMessage("There was an error in the command format of /adjust_resources");
    //        return;
    //    }
    //    string landmarkParameterString = parameters[1];
    //    string materialParameterString = parameters[2];
    //    string adjustmentParamterString = parameters[3];

    //    int landmarkID;
    //    MATERIAL material;
    //    int adjustment;

    //    BaseLandmark landmark = null;

    //    bool isLandmarkParameterNumeric = int.TryParse(landmarkParameterString, out landmarkID);
    //    if (isLandmarkParameterNumeric) {
    //        landmark = LandmarkManager.Instance.GetLandmarkByID(landmarkID);
    //    } else {
    //        landmark = LandmarkManager.Instance.GetLandmarkByName(landmarkParameterString);
    //    }

    //    try {
    //        material = (MATERIAL)Enum.Parse(typeof(MATERIAL), materialParameterString, true);
    //    } catch {
    //        AddCommandHistory(consoleLbl.text);
    //        AddErrorMessage("There was an error in the command format of /adjust_resources");
    //        return;
    //    }

    //    bool isAdjustmentParamterNumeric = int.TryParse(adjustmentParamterString, out adjustment);
    //    if (!isAdjustmentParamterNumeric) {
    //        AddCommandHistory(consoleLbl.text);
    //        AddErrorMessage("There was an error in the command format of /adjust_resources");
    //        return;
    //    }

    //    if(landmark != null) {
    //        landmark.AdjustMaterial(material, adjustment);
    //        AddSuccessMessage("Added " + adjustment.ToString() + " " + material.ToString() + " to " + landmark.landmarkName + ". Total is " + landmark.materialsInventory[material].totalCount);
    //    } else {
    //        AddCommandHistory(consoleLbl.text);
    //        AddErrorMessage("There was an error in the command format of /adjust_resources");
    //    }
    //}
    #endregion

    #region Faction
    private void LogFactionLandmarkInfo(string[] parameters) {
        if (parameters.Length != 2) {
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of /lfli");
            return;
        }
        string factionParameterString = parameters[1];
        int factionID;

        bool isFactionParameterNumeric = int.TryParse(factionParameterString, out factionID);
        Faction faction = null;
        if (isFactionParameterNumeric) {
            faction = FactionManager.Instance.GetFactionBasedOnID(factionID);
            if (faction == null) {
                AddErrorMessage("There was no faction with id " + factionID);
                return;
            }
        } else {
           faction = FactionManager.Instance.GetFactionBasedOnName(factionParameterString);
            if (faction == null) {
                AddErrorMessage("There was no faction with name " + factionParameterString);
                return;
            }
        }

        string text = faction.name + "'s Landmark Info: ";
        for (int i = 0; i < faction.landmarkInfo.Count; i++) {
            BaseLandmark currLandmark = faction.landmarkInfo[i];
			text += "\n" + currLandmark.landmarkName + " (" + currLandmark.tileLocation.name + ") ";
        }
        
        AddSuccessMessage(text);
    }
    #endregion

    #region Pathfinding
    private void GetPath(string[] parameters) {
        if (parameters.Length < 3) {
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of " + parameters[0]);
            return;
        }
        string startTileName = parameters[1];
        string targetTileName = parameters[2];
        //string pathfindingModeString = parameters[3];

        HexTile startTile = null;
        HexTile targetTile = null;
        PATHFINDING_MODE pathfindingMode = PATHFINDING_MODE.PASSABLE;
        if (parameters.Length > 3) {
            pathfindingMode = (PATHFINDING_MODE)Enum.Parse(typeof(PATHFINDING_MODE), parameters[3]);
        }

        string[] startTileCoords = startTileName.Split(',');
        int startTileX = Int32.Parse(startTileCoords[0]);
        int startTileY = Int32.Parse(startTileCoords[1]);

        string[] targetTileCoords = targetTileName.Split(',');
        int targetTileX = Int32.Parse(targetTileCoords[0]);
        int targetTileY = Int32.Parse(targetTileCoords[1]);

        startTile = GridMap.Instance.map[startTileX, startTileY];
        targetTile = GridMap.Instance.map[targetTileX, targetTileY];

        List<HexTile> path = PathGenerator.Instance.GetPath(startTile, targetTile, pathfindingMode);
        if (path != null) {
            for (int i = 0; i < path.Count; i++) {
                path[i].HighlightRoad(Color.red);
            }
            CameraMove.Instance.CenterCameraOn(startTile.gameObject);
        } else {
            AddErrorMessage("There is no path from " + startTile.name + " to " + targetTile.name + " using mode " + pathfindingMode.ToString());
        }
    }
    private void GetAllPaths(string[] parameters) {
        if (parameters.Length != 3) {
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of " + parameters[0]);
            return;
        }
        string startTileName = parameters[1];
        string targetTileName = parameters[2];

        HexTile startTile = null;
        HexTile targetTile = null;

        string[] startTileCoords = startTileName.Split(',');
        int startTileX = Int32.Parse(startTileCoords[0]);
        int startTileY = Int32.Parse(startTileCoords[1]);

        string[] targetTileCoords = targetTileName.Split(',');
        int targetTileX = Int32.Parse(targetTileCoords[0]);
        int targetTileY = Int32.Parse(targetTileCoords[1]);

        startTile = GridMap.Instance.map[startTileX, startTileY];
        targetTile = GridMap.Instance.map[targetTileX, targetTileY];

        List<List<HexTile>> paths = PathGenerator.Instance.GetAllPaths(startTile, targetTile);
        if (paths != null) {
            for (int i = 0; i < paths.Count; i++) {
                List<HexTile> currPath = paths[i];
                for (int j = 0; j < currPath.Count; j++) {
                    currPath[j].HighlightRoad(Color.red);
                }
            }
            CameraMove.Instance.CenterCameraOn(startTile.gameObject);
        } else {
            AddErrorMessage("There are no paths from " + startTile.name + " to " + targetTile.name);
        }
    }
    private void ResetRoadHighlights(string[] parameters) {
        for (int i = 0; i < GridMap.Instance.hexTiles.Count; i++) {
            HexTile currTile = GridMap.Instance.hexTiles[i];
            if (currTile.isRoad || currTile.hasLandmark) {
                currTile.ResetRoadsColors();
            }
        }
    }
    private void SetIconTarget(string[] parameters) {
        if (parameters.Length < 3) {
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of " + parameters[0]);
            return;
        }

        string destinationParameterString = parameters[1];
        string characterParameterString = string.Empty;
        for (int i = 2; i < parameters.Length; i++) {
            characterParameterString += parameters[i];
            if (i + 1 < parameters.Length) {
                characterParameterString += " ";
            }
        }
        int characterID;

        bool isCharacterParameterNumeric = int.TryParse(characterParameterString, out characterID);
        Character character = null;
        if (isCharacterParameterNumeric) {
            character = CharacterManager.Instance.GetCharacterByID(characterID);
        } else {
            character = CharacterManager.Instance.GetCharacterByName(characterParameterString);
        }

        if (character == null) {
            AddErrorMessage("There was an error in the command format of " + parameters[0]);
            return;
        }

        string[] targetTileCoords = destinationParameterString.Split(',');
        int targetTileX = Int32.Parse(targetTileCoords[0]);
        int targetTileY = Int32.Parse(targetTileCoords[1]);

        HexTile targetTile = GridMap.Instance.map[targetTileX, targetTileY];

        character.party.icon.SetTarget(targetTile);
    }
    #endregion

    #region Objects
    private void SpawnNewObject(string[] parameters) {
        if (parameters.Length != 3) { //command, object name, location
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of " + parameters[0]);
            return;
        }
        //string objectName = parameters[1];
        string landmarkParameterString = parameters[2];
        int landmarkID;

        bool isLandmarkParameterNumeric = int.TryParse(landmarkParameterString, out landmarkID);
        BaseLandmark landmark = null;
        if (isLandmarkParameterNumeric) {
            landmark = LandmarkManager.Instance.GetLandmarkByID(landmarkID);
        } else {
            landmark = LandmarkManager.Instance.GetLandmarkByName(landmarkParameterString);
        }

        if (landmark != null) {
            //ObjectManager.Instance.CreateNewObject(objectName, landmark);
            //AddSuccessMessage("Spawned a new " + objectName + " at " + landmark.landmarkName);
            //CameraMove.Instance.CenterCameraOn(landmark.tileLocation.gameObject);
        } else {
            AddErrorMessage("There was an error in the command format of " + parameters[0]);
        }
    }
    #endregion

    #region Roads
    private void ToggleRoads(string[] parameters) {
        if (parameters.Length != 1) { //command, object name, location
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of " + parameters[0]);
            return;
        }
        //for (int i = 0; i < GridMap.Instance.hexTiles.Count; i++) {
        //    HexTile currTile = GridMap.Instance.hexTiles[i];
        //    if (currTile.isRoad) {
        //        currTile.SetRoadState(!currTile.roadState);
        //    }
        //}
    }
    #endregion
}
