﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JobQueueItem {
    public JobQueue jobQueueParent { get; protected set; }
    public Character assignedCharacter { get; protected set; }
    public string name { get; private set; }
    public JOB_TYPE jobType { get; protected set; }
    public bool cannotCancelJob { get; private set; }
    public bool cancelJobOnFail { get; private set; }
    public bool cannotOverrideJob { get; private set; }
    public bool isStealth { get; private set; }
    public List<Character> blacklistedCharacters { get; private set; }
    public int priority { get { return GetPriority(); } }

    protected System.Func<Character, JobQueueItem, bool> _canTakeThisJob;
    protected System.Func<Character, Character, JobQueueItem, bool> _canTakeThisJobWithTarget;
    protected System.Action<Character, JobQueueItem> _onTakeJobAction;
    protected int _priority; //The lower the amount the higher the priority

    public JobQueueItem(JOB_TYPE jobType) {
        this.jobType = jobType;
        this.name = Utilities.NormalizeStringUpperCaseFirstLetters(this.jobType.ToString());
        this.blacklistedCharacters = new List<Character>();
        SetInitialPriority();
    }

    #region Virtuals
    public virtual void UnassignJob(bool shouldDoAfterEffect = true) { }
    protected virtual bool CanTakeJob(Character character) {
        if (jobQueueParent.isAreaJobQueue) {
            //Criminals and Characters with Negative Disabler Traits should no longer create and take Location Jobs
            return !character.HasTraitOf(TRAIT_TYPE.CRIMINAL) && !character.HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER);
        }
        return true;
    }
    public virtual void OnAddJobToQueue() { }
    public virtual bool OnRemoveJobFromQueue() { return true; }
    public virtual bool CanCharacterTakeThisJob(Character character) {
        //All jobs that are personal will bypass _canTakeThisJob/_canTakeThisJobWithTarget function checkers if the character parameter is the owner of the job queue
        if (character == jobQueueParent.character) {
            return CanTakeJob(character);
        }
        if (_canTakeThisJob != null) {
            if (_canTakeThisJob(character, this)) {
                return CanTakeJob(character);
            }
            return false;
        }
        return CanTakeJob(character);
    }
    public virtual void OnCharacterAssignedToJob(Character character) {
        _onTakeJobAction?.Invoke(character, this);
    }
    #endregion

    public void SetJobQueueParent(JobQueue parent) {
        jobQueueParent = parent;
    }
    public void SetAssignedCharacter(Character character) {
        if (assignedCharacter != null) {
            assignedCharacter.SetCurrentJob(null);
            Debug.Log(assignedCharacter.name + " quit job " + name);
        }
        if (character != null) {
            character.SetCurrentJob(this);
            Debug.Log(character.name + " took job " + name);
        }
        
        assignedCharacter = character;
        if (assignedCharacter != null) {
            OnCharacterAssignedToJob(assignedCharacter);
        }
    }
    public void SetCanTakeThisJobChecker(System.Func<Character, JobQueueItem, bool> function) {
        _canTakeThisJob = function;
    }
    public void SetCanTakeThisJobChecker(System.Func<Character, Character, JobQueueItem, bool> function) {
        _canTakeThisJobWithTarget = function;
    }
    public void SetOnTakeJobAction(System.Action<Character, JobQueueItem> action) {
        _onTakeJobAction = action;
    }
    public void SetCannotCancelJob(bool state) {
        cannotCancelJob = state;
    }
    public void SetCancelOnFail(bool state) {
        cancelJobOnFail = state;
    }
    public void SetCannotOverrideJob(bool state) {
        cannotOverrideJob = state;
    }
    public void AddBlacklistedCharacter(Character character) {
        if (!blacklistedCharacters.Contains(character)) {
            blacklistedCharacters.Add(character);
        }
    }
    public void RemoveBlacklistedCharacter(Character character) {
        blacklistedCharacters.Remove(character);
    }
    public void SetIsStealth(bool state) {
        isStealth = state;
    }

    #region Priority
    public int GetPriority() {
        return _priority;
    }
    public void SetPriority(int amount) {
        _priority = amount;
    }
    private void SetInitialPriority() {
        int priority = InteractionManager.Instance.GetInitialPriority(jobType);
        if(priority > 0) {
            SetPriority(priority);
        } else {
            Debug.LogError("Cannot set initial priority for " + name + " job because priority is " + priority);
        }
    }
    #endregion
}
