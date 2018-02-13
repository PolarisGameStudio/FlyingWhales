﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ExploreTile : Quest {

    private BaseLandmark _landmarkToExplore;

    #region getters/setters
    public BaseLandmark landmarkToExplore {
        get { return _landmarkToExplore; }
    }
    #endregion
    public ExploreTile(TaskCreator createdBy, BaseLandmark landmarkToExplore) : base(createdBy, QUEST_TYPE.EXPLORE_TILE) {
        //_questFilters = new List<QuestFilter>() {
        //    new MustBeRole(CHARACTER_ROLE.CHIEFTAIN),
        //    new MustBeRole(CHARACTER_ROLE.CHIEFTAIN)
        //};
        _landmarkToExplore = landmarkToExplore;
    }

    #region overrides
    protected override void ConstructQuestLine() {
        base.ConstructQuestLine();

        GoToLocation goToLandmark = new GoToLocation(this); //Go to the picked region
        goToLandmark.InititalizeAction(_landmarkToExplore);
        goToLandmark.SetPathfindingMode(PATHFINDING_MODE.NORMAL_FACTION_RELATIONSHIP);
        goToLandmark.onTaskActionDone += ScheduleRandomResult;
        goToLandmark.onTaskDoAction += goToLandmark.Generic;
        goToLandmark.onTaskDoAction += LogGoToLocation;

        //Enqueue all actions
        _questLine.Enqueue(goToLandmark);
    }
	internal override void Result(bool isSuccess){
		if (isSuccess) {
			_landmarkToExplore.SetExploredState(true);
			//EndQuest(TASK_RESULT.SUCCESS);
			AddNewLog(_assignedParty.name + " successfully explores " + _landmarkToExplore.location.name);
            GoBackToQuestGiver(TASK_STATUS.SUCCESS);
		} else {
			//AddNewLog("All members of " + _assignedParty.name + " died in combat, they were unable to explore the landmark.");
			GoBackToQuestGiver(TASK_STATUS.CANCEL);
		}
	}
    /*
     This party failed to explore the tile, and died.
         */
    protected override void QuestFail() {
        _isAccepted = false;
        if (_currentAction != null) {
            _currentAction.ActionDone(TASK_ACTION_RESULT.FAIL);
        }
        //RetaskParty(_assignedParty.partyLeader.OnReachNonHostileSettlementAfterQuest);
        //_assignedParty.OnQuestEnd();
        ResetQuestValues();
    }
    #endregion

    private void TriggerRandomResult() {
        if (_taskStatus != TASK_STATUS.IN_PROGRESS) {
            return;
        }
		if(!_assignedParty.isDefeated){
			if(_assignedParty.currLocation.currentCombat == null){
				StartExploration();
			}else{
				ScheduleQuestAction(1, () => TriggerRandomResult());
			}
		}
	}
	private void StartExploration(){
		if(_landmarkToExplore.landmarkEncounterable != null){
			AddNewLog("The party encounters a " + _landmarkToExplore.landmarkEncounterable.encounterName);
			_landmarkToExplore.landmarkEncounterable.StartEncounter(_assignedParty);
		}else{
			Result (true);
		}
	}
    private void ScheduleRandomResult() {
        //Once it arrives, log which Landmark is hidden in the tile.
        Log newLog = new Log(GameManager.Instance.Today(), "Quests", "ExploreTile", "discover_landmark");
        newLog.AddToFillers(_assignedParty, _assignedParty.name, LOG_IDENTIFIER.ALLIANCE_NAME);
        newLog.AddToFillers(_landmarkToExplore, Utilities.NormalizeString(_landmarkToExplore.specificLandmarkType.ToString()), LOG_IDENTIFIER.OTHER);
        UIManager.Instance.ShowNotification(newLog);
        AddNewLog("The party discovers an " + Utilities.NormalizeString(_landmarkToExplore.specificLandmarkType.ToString()));
        _landmarkToExplore.SetHiddenState(false);
        //After 5 days in the tile, the Quest triggers a random result based on data from the Landmark being explored.
        ScheduleQuestAction(5, () => TriggerRandomResult());
    }

    #region Logs
    private void LogGoToLocation() {
        AddNewLog("The party travels to " + _landmarkToExplore.location.name);
    }
    #endregion
}
