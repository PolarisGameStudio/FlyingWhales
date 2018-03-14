﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class QuestData {

    private Quest _activeQuest;
    private int _currentPhase;
    private ECS.Character _owner;

    private List<CharacterTask> _tasks;

    #region getters/setters
    public Quest activeQuest {
        get { return _activeQuest; }
    }
    public List<CharacterTask> tasks {
        get { return _tasks; }
    }
    #endregion

    public QuestData(ECS.Character owner) {
        _owner = owner;
    }

    public void SetActiveQuest(Quest quest) {
        _activeQuest = quest;
        if (quest == null) {
            //quest is being set to null, it means the quest is no longer available or the character chose to leave the quest
            if (_owner.currentTask != null && tasks.Contains(_owner.currentTask)) {
                _owner.currentTask.EndTask(TASK_STATUS.CANCEL); //cancel the characters current task if it comes from the list of tasks granted by his/her active quest
            }
        }
        SetQuestPhase(0);
    }
    public void SetQuestPhase(int phase) {
        _currentPhase = phase;
        if (_activeQuest != null) {
            _tasks = new List<CharacterTask>(GetQuestPhase().tasks);
        } else {
            _tasks.Clear();
        }
    }
    public void AdvanceToNextPhase() {
        SetQuestPhase(_currentPhase + 1);
    }
    public QuestPhase GetQuestPhase() {
        return _activeQuest.phases[_currentPhase];
    }

    private void EndQuest(TASK_STATUS taskStatus) {
        _activeQuest.EndQuest(taskStatus, _owner);
    }
    /*
     What to do when a task succeeds?
         */
    public void OnTaskSuccess(CharacterTask succeededTask) {
        if (!_tasks.Contains(succeededTask)) {
            throw new System.Exception(_owner.name + " does not have a task " + succeededTask.taskType.ToString() + " in his task list!");
        }
        //check if all the tasks in the tasks list are done
        for (int i = 0; i < _tasks.Count; i++) {
            CharacterTask currTask = _tasks[i];
            if (!currTask.isDone) {
                return; //there is still a pending task
            }
        }

        //All tasks are done, check if there is still a next phase
        if (_activeQuest.phases.Count > _currentPhase + 1) {
            //there is still a next phase, advance to the next phase
            AdvanceToNextPhase();
        } else {
            //there are no more phases, end the quest
            EndQuest(TASK_STATUS.SUCCESS);
        }
    }

    public void AddQuestTasksToWeightedDictionary(WeightedDictionary<CharacterTask> actionWeights) {
        for (int i = 0; i < _tasks.Count; i++) {
            CharacterTask currTask = _tasks[i];
            if (!currTask.isDone) {
                actionWeights.AddElement(currTask, currTask.GetSelectionWeight(_owner));
            }
        }
    }
}
