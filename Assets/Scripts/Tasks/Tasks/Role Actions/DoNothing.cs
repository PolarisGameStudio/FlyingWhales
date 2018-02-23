﻿using UnityEngine;
using System.Collections;
using ECS;
using System;

public class DoNothing : CharacterTask {

    private Action endAction;
    private GameDate endDate;
	private int daysLeft;

    public DoNothing(TaskCreator createdBy) 
        : base(createdBy, TASK_TYPE.DO_NOTHING) {
		daysLeft = 0;
    }

	public void SetDays (int days){
		this.daysLeft = days;
	}
    private void EndQuestAfterDays() {
        GameDate dueDate = GameManager.Instance.Today();
		if(daysLeft == 0){
			dueDate.AddDays(UnityEngine.Random.Range(4, 9));
		}else{
			dueDate.AddDays(daysLeft);
		}
        endDate = dueDate;
        endAction = () => EndTask(TASK_STATUS.SUCCESS);
        SchedulingManager.Instance.AddEntry(dueDate, () => endAction());
        //ScheduleTaskEnd(Random.Range(4, 9), TASK_RESULT.SUCCESS); //Do Nothing should only last for a random number of days between 4 days to 8 days
    }

    #region overrides
    public override void PerformTask() {
        base.PerformTask();
		_assignedCharacter.SetCurrentTask(this);
		if(_assignedCharacter.party != null) {
			_assignedCharacter.party.SetCurrentTask(this);
        }
        EndQuestAfterDays();
    }
    public override void TaskCancel() {
        //Unschedule task end!
        SchedulingManager.Instance.RemoveSpecificEntry(endDate, endAction);
		if(_assignedCharacter.faction != null){
			_assignedCharacter.DetermineAction ();
		}
    }
    //public override void AcceptQuest(ECS.Character partyLeader) {
    //    _isAccepted = true;
    //    partyLeader.SetCurrentTask(this);
    //    if(partyLeader.party != null) {
    //        partyLeader.party.SetCurrentTask(this);
    //    }
    //    this.SetWaitingStatus(false);
    //    if (onQuestAccepted != null) {
    //        onQuestAccepted();
    //    }
    //}
    //internal override void EndQuest(TASK_RESULT result) {
    //    if (!_isDone) {
    //        _questResult = result;
    //        _isDone = true;
    //        _createdBy.RemoveQuest(this);
    //        ((ECS.Character)_createdBy).DetermineAction();
    //    }
    //}
    //internal override void QuestCancel() {
    //    _isDone = true;
    //    _createdBy.RemoveQuest(this);
    //    _questResult = TASK_RESULT.SUCCESS;
    //}
    #endregion
}