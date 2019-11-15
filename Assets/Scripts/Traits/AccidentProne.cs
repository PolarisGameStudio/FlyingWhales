﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AccidentProne : Trait {

    public Character owner { get; private set; }
    public CharacterState storedState { get; private set; }

    public AccidentProne() {
        name = "Accident Prone";
        description = "Accident Prone characters often gets injured.";
        type = TRAIT_TYPE.FLAW;
        effect = TRAIT_EFFECT.NEUTRAL;
        trigger = TRAIT_TRIGGER.OUTSIDE_COMBAT;
        associatedInteraction = INTERACTION_TYPE.NONE;
        advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ACCIDENT, INTERACTION_TYPE.STUMBLE };
        crimeSeverity = CRIME_CATEGORY.NONE;
        daysDuration = 0;
        canBeTriggered = true;

    }

    #region Overrides
    public override void OnAddTrait(ITraitable sourceCharacter) {
        base.OnAddTrait(sourceCharacter);
        if (sourceCharacter is Character) {
            owner = sourceCharacter as Character;
        }
    }
    public override bool PerTickOwnerMovement() {
        int stumbleChance = UnityEngine.Random.Range(0, 100);
        bool hasCreatedJob = false;
        if(stumbleChance < 2) {
            if(owner.currentActionNode == null || (owner.currentActionNode.goapType != INTERACTION_TYPE.STUMBLE && owner.currentActionNode.goapType != INTERACTION_TYPE.ACCIDENT)) {
                DoStumble();
                hasCreatedJob = true;
            }
        }
        return hasCreatedJob;
    }
    public override bool OnStartPerformGoapAction(GoapAction action, ref bool willStillContinueAction) {
        int accidentChance = UnityEngine.Random.Range(0, 100);
        bool hasCreatedJob = false;
        if (accidentChance < 10) {
            if (action != null && !AttributeManager.Instance.excludedActionsFromAccidentProneTrait.Contains(action.goapType)) {
                DoAccident(action);
                hasCreatedJob = true;
                willStillContinueAction = false;
            }
        }
        return hasCreatedJob;
    }
    public override string TriggerFlaw(Character character) {
        if (character.marker.isMoving) {
            //If moving, the character will stumble and get injured.
            DoStumble();
        } else if (character.currentActionNode != null && !AttributeManager.Instance.excludedActionsFromAccidentProneTrait.Contains(character.currentActionNode.goapType)) {
            //If doing something, the character will fail and get injured.
            DoAccident(character.currentActionNode);
        }
        return base.TriggerFlaw(character);
    }
    #endregion

    private void DoStumble() {
        GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.STUMBLE, owner, owner);

        GoapNode goalNode = new GoapNode(null, goapAction.cost, goapAction);
        GoapPlan plan = new GoapPlan(goalNode, new GOAP_EFFECT_CONDITION[] { GOAP_EFFECT_CONDITION.NONE }, GOAP_CATEGORY.REACTION);
        GoapPlanJob job = new GoapPlanJob(JOB_TYPE.MISC, INTERACTION_TYPE.STUMBLE, owner);
        plan.ConstructAllNodes();
        plan.SetDoNotRecalculate(true);
        job.SetAssignedPlan(plan);
        job.SetAssignedCharacter(owner);
        job.SetCancelOnFail(true);

        owner.jobQueue.AddJobInQueue(job, false);

        owner.AdjustIsWaitingForInteraction(1);
        if (owner.currentParty.icon.isTravelling) {
            owner.marker.StopMovement();
        }
        if (owner.IsInOwnParty()) {
            owner.ownParty.RemoveAllOtherCharacters();
        }
        if (owner.currentActionNode != null) {
            //If current action is a roaming action like Hunting To Drink Blood, we must requeue the job after it is removed by StopCurrentAction
            JobQueueItem currentJob = null;
            JobQueue currentJobQueue = null;
            if (owner.currentActionNode.isRoamingAction && owner.currentActionNode.parentPlan != null && owner.currentActionNode.parentPlan.job != null) {
                currentJob = owner.currentActionNode.parentPlan.job;
                currentJobQueue = currentJob.currentOwner;
            }
            owner.StopCurrentAction(false);
            if (currentJob != null) {
                currentJobQueue.AddJobInQueue(currentJob, false);
            }
        }
        if (owner.stateComponent.currentState != null) {
            storedState = owner.stateComponent.currentState;
            owner.stateComponent.currentState.PauseState();
            goapAction.SetEndAction(ResumePausedState);
        } 
        //else if (owner.stateComponent.stateToDo != null) {
        //    storedState = owner.stateComponent.stateToDo;
        //    owner.stateComponent.SetStateToDo(null, false, false);
        //    goapAction.SetEndAction(ResumeStateToDoState);
        //}
        owner.AdjustIsWaitingForInteraction(-1);

        owner.AddPlan(plan, true, false);
        owner.PerformGoapPlans();
    }

    private void DoAccident(GoapAction action) {
        GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.ACCIDENT, owner, owner);
        goapAction.InitializeOtherData(new object[] { action });

        GoapNode goalNode = new GoapNode(null, goapAction.cost, goapAction);
        GoapPlan plan = new GoapPlan(goalNode, new GOAP_EFFECT_CONDITION[] { GOAP_EFFECT_CONDITION.NONE }, GOAP_CATEGORY.REACTION);
        GoapPlanJob job = new GoapPlanJob(JOB_TYPE.MISC, INTERACTION_TYPE.ACCIDENT, owner);
        plan.ConstructAllNodes();
        plan.SetDoNotRecalculate(true);
        job.SetAssignedPlan(plan);
        job.SetAssignedCharacter(owner);
        job.SetCancelOnFail(true);

        owner.jobQueue.AddJobInQueue(job, false);

        if (owner.currentActionNode != null && owner.currentActionNode.parentPlan != null && owner.currentActionNode.parentPlan.job != null
            && owner.currentActionNode.parentPlan.job.id == owner.sleepScheduleJobID) {
            owner.SetHasCancelledSleepSchedule(true);
        }

        owner.AdjustIsWaitingForInteraction(1);
        owner.currentParty.RemoveAllOtherCharacters();
        if (owner.currentParty.icon.isTravelling) {
            owner.marker.StopMovement();
        }
        action.StopAction(true);
        owner.AdjustIsWaitingForInteraction(-1);

        owner.AddPlan(plan, true, false);
        owner.PerformGoapPlans();
    }

    private void ResumePausedState(string result, GoapAction action) {
        owner.GoapActionResult(result, action);
        storedState.ResumeState();
    }
    //private void ResumeStateToDoState(string result, GoapAction action) {
    //    owner.GoapActionResult(result, action);
    //    //owner.stateComponent.SetStateToDo(storedState);
    //}
}
