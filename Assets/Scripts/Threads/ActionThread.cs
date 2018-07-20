﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;
using System;

public class ActionThread : Multithread {
    private CharacterActionAdvertisement[] choices;
    private CharacterParty _party;
    private CharacterAction chosenAction;
    private IObject chosenObject;
    private List<CharacterActionAdvertisement> allChoices;
    private ChainAction chosenChainAction;
    private string actionLog;

    #region getters/setters
    public CharacterParty party {
        get { return _party; }
    }
    #endregion

    public ActionThread(CharacterParty party) {
        choices = new CharacterActionAdvertisement[3];
        allChoices = new List<CharacterActionAdvertisement>();
        _party = party;
    }

    #region Overrides
    public override void DoMultithread() {
        base.DoMultithread();
        try {
            LookForAction();
        } catch (Exception e) {
            Debug.LogError(e.Message + " ST: " + e.StackTrace);
        }
    }
    public override void FinishMultithread() {
        base.FinishMultithread();
        ReturnAction();
    }
    #endregion
    private void LookForAction() {
        if (_party.mainCharacter is Character) {
            actionLog = "[" + Utilities.GetDateString(GameManager.Instance.Today()) + "]" + _party.name + "'s Action Logic: ";
            Character character = _party.mainCharacter as Character;
            if (!character.actionQueue.IsEmpty) {//If Action Queue is not empty, pop the earliest one
                ActionQueueItem item = character.actionQueue.Dequeue();
                chosenAction = item.action;
                chosenObject = item.targetObject;
                actionLog += "\nGot action from queue " + chosenAction.actionData.actionName + "-" + chosenObject.objectName;
            } else {
                //If Action Queue is empty, check if Happiness is above 200 and Mental Points and Physical Points are both above -3.
                if (character.role.happiness > CharacterManager.Instance.HAPPINESS_THRESHOLD && character.mentalPoints > CharacterManager.Instance.MENTAL_THRESHOLD 
                    && character.physicalPoints > CharacterManager.Instance.PHYSICAL_THRESHOLD) {
                    List<Quest> availableQuests = character.GetElligibleQuests();
                    if (character.IsSquadLeader()) {
                        LookForActionForSquadLeader(character, availableQuests);
                    } else if (character.IsSquadMember()) {
                        LookForActionForSquadMember(character, availableQuests);
                    } else if (character.squad == null) {
                        LookForActionSquadless(character, availableQuests);
                    }                    
                } else {
                    IObject targetObject = null;
                    //If Action Queue is empty, and either Happiness is 200 or below or either of Mental Points or Physical Points is -3 or below, choose from Advertisements
                    CharacterAction actionFromAds = GetActionFromAdvertisements(ref targetObject);
                    actionLog += "\n" + character.name + " will choose action from advertisements";
                    if (character.IsSquadLeader()) {
                        //if character is a Squad Leader and has party members, perform Disband Party then add selected Action to the Queue
                        chosenAction = character.currentParty.icharacterObject.currentState.GetAction(ACTION_TYPE.DISBAND_PARTY);
                        chosenObject = character.currentParty.icharacterObject;
                        character.AddActionToQueue(actionFromAds, targetObject);
                        actionLog += "\n" + character.name + " disbanded party and added " + chosenAction.actionData.actionName + " " + chosenObject.objectName + " to action queue";
                    } else {
                        //otherwise, perform selected Action
                        chosenAction = actionFromAds;
                        chosenObject = targetObject;
                        actionLog += "\n" + character.name + " chose to " + chosenAction.actionData.actionName + " " + chosenObject.objectName;
                    }
                }
            }

            if (UIManager.Instance.characterInfoUI.currentlyShowingCharacter != null && UIManager.Instance.characterInfoUI.currentlyShowingCharacter.id == character.id) {
                Debug.Log(actionLog);
            }
        }
        //if (!LookForActionFromQuests()) {
        //    LookForActionFromAdvertisements();
        //}
    }
    private void LookForActionForSquadLeader(Character character, List<Quest> availableQuests) {
        actionLog += "\nGetting action for squad leader:"; 
        if (availableQuests.Count > 0) { //If yes, check if there is at least one Quest
            Quest selectedQuest = availableQuests[Utilities.rng.Next(0, availableQuests.Count)];
            QuestAction actionFromQuest = selectedQuest.GetQuestAction(character, character.GetDataForQuest(selectedQuest));
            actionLog += "\nSelected quest is " + selectedQuest.name;
            if (selectedQuest.groupType == GROUP_TYPE.SOLO) { //If selected Quest is a Solo Quest
                if (character.IsInParty()) { //If character is in a party
                    if (IsQuestActionAchieveable(actionFromQuest)) {//if action from Quest is achievable
                        chosenAction = character.currentParty.icharacterObject.currentState.GetAction(ACTION_TYPE.DISBAND_PARTY);
                        chosenObject = character.currentParty.icharacterObject;
                        //Disband Party then add action from the Quest to the Action Queue
                        character.AddActionToQueue(actionFromQuest.action, actionFromQuest.targetObject);
                        actionLog += "\n" + character.name + " disbanded party and added action " + actionFromQuest.action.actionData.actionName + " " + chosenObject.objectName + " to queue.";
                    } else {
                        //if action from Quest is not achievable, Disband Party and perform Grind Action
                        chosenAction = character.currentParty.icharacterObject.currentState.GetAction(ACTION_TYPE.DISBAND_PARTY);
                        chosenObject = character.currentParty.icharacterObject;
                        character.AddActionToQueue(character.currentParty.icharacterObject.currentState.GetAction(ACTION_TYPE.GRIND), character.currentParty.icharacterObject);
                        actionLog += "\n" + character.name + " disbanded party and grind action to queue";
                    }

                } else {
                    // If character is not in party
                    if (IsQuestActionAchieveable(actionFromQuest)) {//if action from Quest is achievable
                        //obtain and perform Action from the Quest
                        chosenAction = actionFromQuest.action;
                        chosenObject = actionFromQuest.targetObject;
                        actionLog += "\n" + character.name + " got action " + chosenAction.actionData.actionName + " " + chosenObject.objectName + " from quest";
                    } else {
                        //if action from Quest is not achievable, Grind
                        chosenAction = character.currentParty.icharacterObject.currentState.GetAction(ACTION_TYPE.GRIND);
                        chosenObject = character.currentParty.icharacterObject;
                        actionLog += "\n" + character.name + " decided to grind";
                    }
                }
            } else if (selectedQuest.groupType == GROUP_TYPE.PARTY) {
                //If selected Quest is a Party Quest, obtain Action from it
                //Perform Forming Party action which sends out signals to all other Squad Followers
                chosenAction = _party.icharacterObject.currentState.GetAction(ACTION_TYPE.FORM_PARTY);
                chosenObject = _party.icharacterObject;
                actionLog += "\n" + character.name + " chose to form a party.";
                if (IsQuestActionAchieveable(actionFromQuest)) {
                    //if action from Quest is achievable, add action from the Quest to the Action Queue
                    character.AddActionToQueue(actionFromQuest.action, actionFromQuest.targetObject);
                    actionLog += "\n" + character.name + " added action " + actionFromQuest.action.actionData.actionName + " " + actionFromQuest.targetObject.objectName + " from quest to queue.";
                } else {
                    //if action from Quest is not achievable, perform Grind Action
                    chosenAction = character.currentParty.icharacterObject.currentState.GetAction(ACTION_TYPE.GRIND);
                    chosenObject = character.currentParty.icharacterObject;
                    actionLog += "\n" + character.name + " decided to grind";
                }
            }
        } else { //If no, choose a random Idle Action
            IObject targetObject = null;
            chosenAction = character.GetRandomIdleAction(ref targetObject);
            chosenObject = targetObject;
            actionLog += "\n" + character.name + " chose to " + chosenAction.actionData.actionName + " " + chosenObject.objectName;
        }
    }
    private void LookForActionForSquadMember(Character character, List<Quest> availableQuests) {
        actionLog += "\nGetting action for squad follower:";
        //If Squad Follower, choose only from available Solo Quests, if None, perform a random Idle Action
        if (availableQuests.Count == 0) {
            IObject targetObject = null;
            chosenAction = character.GetRandomIdleAction(ref targetObject);
            chosenObject = targetObject;
            actionLog += "\n" + character.name + " chose to " + chosenAction.actionData.actionName + " " + chosenObject.objectName;
        } else {
            Quest chosenQuest = availableQuests[Utilities.rng.Next(0, availableQuests.Count)];
            QuestAction questAction = chosenQuest.GetQuestAction(character, character.GetDataForQuest(chosenQuest));
            chosenAction = questAction.action;
            chosenObject = questAction.targetObject;
            actionLog += "\n" + character.name + " decided to do action " + chosenAction.actionData.actionName + " " + chosenObject.objectName + " from quest " + chosenQuest.name;
        }
    }
    private void LookForActionSquadless(Character character, List<Quest> availableQuests) {
        actionLog += "\nGetting action for squadless:";
        //If Squadless, choose only from available Solo Quests:
        if (availableQuests.Count == 0) {
            //if no available Solo Quest, perform a random Idle Action
            IObject targetObject = null;
            chosenAction = character.GetRandomIdleAction(ref targetObject);
            chosenObject = targetObject;
            actionLog += "\n" + character.name + " chose to " + chosenAction.actionData.actionName + " " + chosenObject.objectName;
        } else {
            Quest chosenQuest = availableQuests[Utilities.rng.Next(0, availableQuests.Count)];
            QuestAction questAction = chosenQuest.GetQuestAction(character, character.GetDataForQuest(chosenQuest));
            if (IsQuestActionAchieveable(questAction)) {
                //if action from Quest is achievable, add action from the Quest to the Action Queue
                chosenAction = questAction.action;
                chosenObject = questAction.targetObject;
                actionLog += "\n" + character.name + " decided to do action " + chosenAction.actionData.actionName + " " + chosenObject.objectName + " from quest " + chosenQuest.name;
            } else {
                //if action from Quest is not achievable, perform Grind Action
                chosenAction = character.currentParty.icharacterObject.currentState.GetAction(ACTION_TYPE.GRIND);
                chosenObject = character.currentParty.icharacterObject;
                actionLog += "\n" + character.name + " decided to grind";
            }
        }
    }

    private bool LookForActionFromQuests() {
        if (_party.mainCharacter.role.happiness >= 100) {
            if (_party.questData.Count > 0) {
                //when a character's happiness is 100 or above, he chooses randomly between his active quests and calls a function from it which should return an instruction of which next action to execute
                int randomIndex = Utilities.rng.Next(0, _party.questData.Count);
                CharacterQuestData chosenQuestData = _party.questData[randomIndex];
   
                IObject targetObject = null;
                chosenAction = chosenQuestData.GetNextQuestAction(ref targetObject);
                chosenObject = targetObject;
                if (chosenAction != null) {
                    _party.actionData.questDataAssociatedWithCurrentAction = chosenQuestData;
                    return true;
                } else {
                    throw new Exception("Cannot find action from " + chosenQuestData.parentQuest.questType.ToString());
                }
            } else { //no quests
                chosenAction = _party.mainCharacter.GetRandomIdleAction(ref chosenObject); //Characters with no Quests with Happiness above 100 should perform a random Idle Action
                return true;
            }
        }
        
        return false;
    }
    private void LookForActionFromAdvertisements() {
        allChoices.Clear();

        actionLog = _party.name + "'s Action Advertisements: ";
        for (int i = 0; i < _party.currentRegion.landmarks.Count; i++) {
            BaseLandmark landmark = _party.currentRegion.landmarks[i];
            StructureObj iobject = landmark.landmarkObj;
            if (iobject.currentState.actions != null && iobject.currentState.actions.Count > 0) {
                for (int k = 0; k < iobject.currentState.actions.Count; k++) {
                    CharacterAction action = iobject.currentState.actions[k];
                    if (action.MeetsRequirements(_party, landmark) && action.CanBeDone(iobject) && action.CanBeDoneBy(_party, iobject)) { //Filter
                        float happinessIncrease = _party.TotalHappinessIncrease(action, iobject);
                        actionLog += "\n" + action.actionData.actionName + " = " + happinessIncrease + " (" + iobject.objectName + " at " + iobject.specificLocation.locationName + ")";
                        PutToChoices(action, iobject, happinessIncrease);
                    }
                }
            }
        }
        if (Messenger.eventTable.ContainsKey(Signals.LOOK_FOR_ACTION)) {
            Messenger.Broadcast<ActionThread>(Signals.LOOK_FOR_ACTION, this);
        }
        if (UIManager.Instance.characterInfoUI.currentlyShowingCharacter != null && UIManager.Instance.characterInfoUI.currentlyShowingCharacter.id == _party.id) {
            Debug.Log(actionLog);
        }
//#if UNITY_EDITOR
//        _party.actionData.actionHistory.Add(Utilities.GetDateString(GameManager.Instance.Today()) + " " + actionLog);
//#endif
        CharacterActionAdvertisement chosenActionAd = PickAction();
        chosenAction = chosenActionAd.action;
        chosenObject = chosenActionAd.targetObject;
        _party.actionData.questDataAssociatedWithCurrentAction = null;
        //#if UNITY_EDITOR
        //_party.actionData.actionHistory.Add(Utilities.GetDateString(GameManager.Instance.Today()) + 
        //    " Chosen action: " + chosenAction.actionType.ToString() + " at " + chosenAction.state.obj.objectLocation.landmarkName + "(" + chosenAction.state.obj.objectLocation.tileLocation.tileName + ")");
        //#endif

        //Check Prerequisites, currently for resource prerequisites only
        //CheckPrerequisites(chosenAction);

    }
    private CharacterAction GetActionFromAdvertisements(ref IObject targetObject) {
        allChoices.Clear();

        actionLog += _party.name + "'s Action Advertisements: ";
        for (int i = 0; i < _party.currentRegion.landmarks.Count; i++) {
            BaseLandmark landmark = _party.currentRegion.landmarks[i];
            StructureObj iobject = landmark.landmarkObj;
            if (iobject.currentState.actions != null && iobject.currentState.actions.Count > 0) {
                for (int k = 0; k < iobject.currentState.actions.Count; k++) {
                    CharacterAction action = iobject.currentState.actions[k];
                    if (action.MeetsRequirements(_party, landmark) && action.CanBeDone(iobject) && action.CanBeDoneBy(_party, iobject)) { //Filter
                        float happinessIncrease = _party.TotalHappinessIncrease(action, iobject);
                        actionLog += "\n" + action.actionData.actionName + " = " + happinessIncrease + " (" + iobject.objectName + " at " + iobject.specificLocation.locationName + ")";
                        PutToChoices(action, iobject, happinessIncrease);
                    }
                }
            }
        }
        if (Messenger.eventTable.ContainsKey(Signals.LOOK_FOR_ACTION)) {
            Messenger.Broadcast<ActionThread>(Signals.LOOK_FOR_ACTION, this);
        }
        if (UIManager.Instance.characterInfoUI.currentlyShowingCharacter != null && UIManager.Instance.characterInfoUI.currentlyShowingCharacter.id == _party.id) {
            Debug.Log(actionLog);
        }

        CharacterActionAdvertisement chosenActionAd = PickAction();
        targetObject = chosenActionAd.targetObject;
        return chosenActionAd.action;
    }
    //private void CheckPrerequisites(CharacterAction characterAction) {
    //    if (HasPrerequisite(characterAction)) {
    //        if (CanDoPrerequisite(characterAction)) {
    //            DoPrerequisite(characterAction, null, null);
    //        } else {
    //            RemoveActionFromChoices(characterAction);
    //            chosenAction = PickAction();
    //            CheckPrerequisites(chosenAction);
    //        }
    //    }
    //}
    //private bool CanDoPrerequisite(CharacterAction characterAction) {
    //    if(HasPrerequisite(characterAction)) {
    //        for (int i = 0; i < characterAction.actionData.prerequisites.Count; i++) {
    //            IPrerequisite prerequisite = characterAction.actionData.prerequisites[i];
    //            if (_party.DoesSatisfiesPrerequisite(prerequisite)) {
    //                continue;
    //            }
    //            if (prerequisite.prerequisiteType == PREREQUISITE.RESOURCE) {
    //                ResourcePrerequisite resourcePrerequisite = prerequisite as ResourcePrerequisite;
    //                CharacterAction satisfyingAction = GetActionThatMatchesResourcePrerequisite(resourcePrerequisite);
    //                if (satisfyingAction == null || (satisfyingAction != null && !CanDoPrerequisite(satisfyingAction))) {
    //                    return false;
    //                }
    //            }
    //        }
    //    }
    //    return true;
    //}
    //private void DoPrerequisite(CharacterAction characterAction, ChainAction parentAction, IPrerequisite iprerequisite) {
    //    ChainAction chainAction = new ChainAction();
    //    chainAction.parentChainAction = parentAction;
    //    chainAction.action = characterAction;
    //    chainAction.prerequisite = iprerequisite;
    //    chainAction.satisfiedPrerequisites = new List<ChainAction>();
    //    chainAction.finishedPrerequisites = new List<IPrerequisite>();
    //    if (parentAction != null) {
    //        parentAction.satisfiedPrerequisites.Add(chainAction);
    //    }
    //    chosenChainAction = chainAction;
    //    if (HasPrerequisite(characterAction)) {
    //        for (int i = 0; i < characterAction.actionData.prerequisites.Count; i++) {
    //            IPrerequisite prerequisite = characterAction.actionData.prerequisites[i];
    //            if (_party.DoesSatisfiesPrerequisite(prerequisite)) {
    //                chainAction.finishedPrerequisites.Add(prerequisite);
    //                continue;
    //            }
    //            if (prerequisite.prerequisiteType == PREREQUISITE.RESOURCE) {
    //                ResourcePrerequisite resourcePrerequisite = prerequisite as ResourcePrerequisite;
    //                CharacterAction satisfyingAction = GetActionThatMatchesResourcePrerequisite(resourcePrerequisite);
    //                if (satisfyingAction != null) {
    //                    DoPrerequisite(satisfyingAction, chainAction, prerequisite);
    //                }
    //            }
    //        }
    //    }
    //}
    //private bool HasPrerequisite(CharacterAction characterAction) {
    //    return characterAction.actionData.prerequisites != null;
    //}
    //private CharacterAction GetActionThatMatchesResourcePrerequisite(ResourcePrerequisite resourcePrerequisite) {
    //    for (int i = 0; i < allChoices.Count; i++) {
    //        CharacterAction action = allChoices[i].action;
    //        if (action.actionData.advertisedResource != RESOURCE.NONE && resourcePrerequisite.resourceType != RESOURCE.NONE && action.actionData.advertisedResource == resourcePrerequisite.resourceType && action.state.obj.resourceInventory != null) {
    //            if (action.state.obj.resourceInventory[resourcePrerequisite.resourceType] >= resourcePrerequisite.amount) {
    //                return action;
    //            }
    //        }
    //    }
    //    return null;
    //}
    private void ReturnAction() {
        _party.actionData.ReturnActionFromThread(chosenAction, chosenObject, chosenChainAction);
    }
    private void PutToChoices(CharacterAction action, IObject targetObject, float advertisement) {
        CharacterActionAdvertisement actionAdvertisement = new CharacterActionAdvertisement();
        actionAdvertisement.Set(action, targetObject, advertisement);
        allChoices.Add(actionAdvertisement);
       
    }
    private CharacterActionAdvertisement PickAction() {
        choices[0].Reset();
        choices[1].Reset();
        choices[2].Reset();
        float advertisement = 0f;
        CharacterAction action = null;
        IObject targetObject = null;
        for (int i = 0; i < allChoices.Count; i++) {
            action = allChoices[i].action;
            targetObject = allChoices[i].targetObject;
            advertisement = allChoices[i].advertisement;
            if (choices[0].action == null) {
                choices[0].Set(action, targetObject, advertisement);
            } else {
                if (advertisement > choices[0].advertisement) {
                    choices[0].Set(action, targetObject, advertisement);
                }
            }
            //if (choices[0].action == null) {
            //    choices[0].Set(action, advertisement);
            //} else if (choices[1].action == null) {
            //    choices[1].Set(action, advertisement);
            //} else if (choices[2].action == null) {
            //    choices[2].Set(action, advertisement);
            //} else {
            //    if (choices[0].advertisement <= choices[1].advertisement && choices[0].advertisement <= choices[2].advertisement) {
            //        if (advertisement > choices[0].advertisement) {
            //            choices[0].Set(action, advertisement);
            //        }
            //    } else if (choices[1].advertisement <= choices[0].advertisement && choices[1].advertisement <= choices[2].advertisement) {
            //        if (advertisement > choices[1].advertisement) {
            //            choices[1].Set(action, advertisement);
            //        }
            //    } else if (choices[2].advertisement <= choices[0].advertisement && choices[2].advertisement <= choices[1].advertisement) {
            //        if (advertisement > choices[2].advertisement) {
            //            choices[2].Set(action, advertisement);
            //        }
            //    }
            //}
        }

        //int maxChoice = 3;
        //if (choices[1].action == null) {
        //    maxChoice = 1;
        //} else if (choices[2].action == null) {
        //    maxChoice = 2;
        //}
        int chosenIndex = 0; //Utilities.rng.Next(0, maxChoice);
        CharacterActionAdvertisement chosenActionAd = choices[chosenIndex];

        if (chosenActionAd.action == null) {
            string error = _party.name + " could not find an action to do! Choices were ";
            for (int i = 0; i < choices.Length; i++) {
                CharacterActionAdvertisement currAd = choices[i];
                if (currAd.action != null) {
                    error += "\n" + currAd.action.actionType.ToString() + " " + currAd.targetObject.objectName + " at " + currAd.targetObject.objectLocation.landmarkName;
                }
            }
            throw new Exception(error);
        }
        //if (UIManager.Instance.characterInfoUI.currentlyShowingCharacter != null && _party.icharacters.Contains(UIManager.Instance.characterInfoUI.currentlyShowingCharacter)) {
        //    Debug.Log("Chosen Action: " + chosenActionAd.action.actionData.actionName + " = " + chosenActionAd.advertisement + " (" + chosenActionAd.targetObject.objectName + " at " + chosenActionAd.targetObject.specificLocation.locationName + ")");
        //}
        //Debug.Log("Chosen Action: " + chosenAction.actionData.actionName + " = " + choices[chosenIndex].advertisement + " (" + chosenAction.state.obj.objectName + " at " + chosenAction.state.obj.objectLocation.landmarkName + ")");
        return chosenActionAd;
    }

    private void RemoveActionFromChoices(CharacterAction action) {
        for (int i = 0; i < allChoices.Count; i++) {
            if(allChoices[i].action == action) {
                allChoices.RemoveAt(i);
                break;
            }
        }
    }
    public void AddToChoices(IObject iobject) {
        if (iobject.currentState.actions != null && iobject.currentState.actions.Count > 0) {
            for (int k = 0; k < iobject.currentState.actions.Count; k++) {
                CharacterAction action = iobject.currentState.actions[k];
                if(action is ChangeClassAction) {
                    continue; //temp only, make a new variable in action data, ie. bool dontAdvertise, to force an action to advertise or not
                }
                if (action.MeetsRequirements(_party, null) && action.CanBeDone(iobject) && action.CanBeDoneBy(_party, iobject)) { //Filter
                    float happinessIncrease = _party.TotalHappinessIncrease(action, iobject);
                    actionLog += "\n" + action.actionData.actionName + " = " + happinessIncrease + " (" + iobject.objectName + ")";
                    PutToChoices(action, iobject, happinessIncrease);
                }
            }
        }
    }
    private bool IsQuestActionAchieveable(QuestAction action) {
        if (_party.computedPower >= action.requiredPower) {
            return true;
        }
        return false;
    }
}
