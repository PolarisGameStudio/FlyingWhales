﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoapThread : Multithread {
    public Character actor { get; private set; }
    public GoapPlan createdPlan { get; private set; }
    public GoapEffect goalEffect { get; private set; }
    public GoapAction goalAction { get; private set; }
    public INTERACTION_TYPE goalType { get; private set; }
    public IPointOfInterest target { get; private set; }
    public bool isPriority { get; private set; }
    public bool isPersonalPlan { get; private set; }
    public GOAP_CATEGORY category { get; private set; }
    public List<IPointOfInterest> characterTargetsAwareness { get; private set; }
    public GoapPlanJob job { get; private set; }
    public bool allowDeadTargets { get; private set; }
    //public List<INTERACTION_TYPE> actorAllowedActions { get; private set; }
    //public List<GoapAction> usableActions { get; private set; }
    public string log { get; private set; }
    private Dictionary<INTERACTION_TYPE, object[]> otherData;

    //For recalculation
    public GoapPlan recalculationPlan;

    public GoapThread(Character actor, IPointOfInterest target, GoapEffect goalEffect, GOAP_CATEGORY category, bool isPriority, List<IPointOfInterest> characterTargetsAwareness
        , bool isPersonalPlan, GoapPlanJob job, bool allowDeadTargets) {//, List<INTERACTION_TYPE> actorAllowedActions, List<GoapAction> usableActions
        this.createdPlan = null;
        this.recalculationPlan = null;
        this.actor = actor;
        this.target = target;
        this.goalEffect = goalEffect;
        this.isPriority = isPriority;
        this.characterTargetsAwareness = characterTargetsAwareness;
        //this.actorAllowedActions = actorAllowedActions;
        //this.usableActions = usableActions;
        this.isPersonalPlan = isPersonalPlan;
        this.category = category;
        this.job = job;
        this.allowDeadTargets = allowDeadTargets;
    }
    public GoapThread(Character actor, IPointOfInterest target, GoapEffect goalEffect, GOAP_CATEGORY category, bool isPriority, List<IPointOfInterest> characterTargetsAwareness
        , bool isPersonalPlan, GoapPlanJob job, bool allowDeadTargets, Dictionary<INTERACTION_TYPE, object[]> otherData = null) {//, List<INTERACTION_TYPE> actorAllowedActions, List<GoapAction> usableActions
        this.createdPlan = null;
        this.recalculationPlan = null;
        this.actor = actor;
        this.target = target;
        this.goalEffect = goalEffect;
        this.isPriority = isPriority;
        this.characterTargetsAwareness = characterTargetsAwareness;
        //this.actorAllowedActions = actorAllowedActions;
        //this.usableActions = usableActions;
        this.isPersonalPlan = isPersonalPlan;
        this.category = category;
        this.job = job;
        this.allowDeadTargets = allowDeadTargets;
        this.otherData = otherData;
    }
    public GoapThread(Character actor, IPointOfInterest target, GoapAction goalAction, GOAP_CATEGORY category, bool isPriority, List<IPointOfInterest> characterTargetsAwareness
        , bool isPersonalPlan, GoapPlanJob job, bool allowDeadTargets) {//, List<INTERACTION_TYPE> actorAllowedActions, List<GoapAction> usableActions
        this.createdPlan = null;
        this.recalculationPlan = null;
        this.actor = actor;
        this.target = target;
        this.goalAction = goalAction;
        this.isPriority = isPriority;
        this.characterTargetsAwareness = characterTargetsAwareness;
        //this.actorAllowedActions = actorAllowedActions;
        //this.usableActions = usableActions;
        this.isPersonalPlan = isPersonalPlan;
        this.category = category;
        this.job = job;
        this.allowDeadTargets = allowDeadTargets;
    }
    public GoapThread(Character actor, INTERACTION_TYPE goalType, GOAP_CATEGORY category, bool isPriority, List<IPointOfInterest> characterTargetsAwareness
        , bool isPersonalPlan, GoapPlanJob job, bool allowDeadTargets, Dictionary<INTERACTION_TYPE, object[]> otherData = null) {//, List<INTERACTION_TYPE> actorAllowedActions, List<GoapAction> usableActions
        this.createdPlan = null;
        this.recalculationPlan = null;
        this.actor = actor;
        this.goalType = goalType;
        this.isPriority = isPriority;
        this.characterTargetsAwareness = characterTargetsAwareness;
        //this.actorAllowedActions = actorAllowedActions;
        //this.usableActions = usableActions;
        this.isPersonalPlan = isPersonalPlan;
        this.category = category;
        this.job = job;
        this.allowDeadTargets = allowDeadTargets;
        this.otherData = otherData;
    }
    public GoapThread(Character actor, INTERACTION_TYPE goalType, IPointOfInterest target, GOAP_CATEGORY category, bool isPriority, List<IPointOfInterest> characterTargetsAwareness
        , bool isPersonalPlan, GoapPlanJob job, bool allowDeadTargets, Dictionary<INTERACTION_TYPE, object[]> otherData = null) {//, List<INTERACTION_TYPE> actorAllowedActions, List<GoapAction> usableActions
        this.createdPlan = null;
        this.recalculationPlan = null;
        this.actor = actor;
        this.target = target;
        this.goalType = goalType;
        this.isPriority = isPriority;
        this.characterTargetsAwareness = characterTargetsAwareness;
        this.isPersonalPlan = isPersonalPlan;
        this.category = category;
        this.job = job;
        this.allowDeadTargets = allowDeadTargets;
        this.otherData = otherData;
    }
    public GoapThread(Character actor, GoapPlan currentPlan) {//, List<GoapAction> usableActions
        this.createdPlan = null;
        this.actor = actor;
        this.recalculationPlan = currentPlan;
        //this.actorAllowedActions = actorAllowedActions;
        //this.usableActions = usableActions;
    }
    
    #region Overrides
    public override void DoMultithread() {
        base.DoMultithread();
        try {
            CreatePlan();
        }catch(System.Exception e) {
            Debug.LogError("Problem with " + actor.name + "'s GoapThread!\n" + e.Message + "\n" + e.StackTrace);
        }
    }
    public override void FinishMultithread() {
        base.FinishMultithread();
        ReturnPlanFromGoapThread();
    }
    #endregion

    public void CreatePlan() {
        if(recalculationPlan != null) {
            RecalculatePlan();
        } else {
            CreateNewPlan();
        }
    }
    private void CreateNewPlan() {
        log = "-----------------RECEIVING NEW PLAN FROM OTHER THREAD OF " + actor.name + " WITH TARGET " + target?.name ?? "None" + " (" + actor.specificLocation.name + ")-----------------------";
        if (goalType != INTERACTION_TYPE.NONE) {
            log += "\nGOAL: "  + goalType.ToString();
        } else {
            log += "\nGOAL: " + goalEffect.conditionType.ToString() + " - " + goalEffect.conditionString() + ", target: " + goalEffect.targetPOI.ToString();
        }
       

        List<INTERACTION_TYPE> actorAllowedActions = RaceManager.Instance.GetNPCInteractionsOfCharacter(actor);
        List<GoapAction> usableActions = new List<GoapAction>();
        //Dictionary<POINT_OF_INTEREST_TYPE, List<IAwareness>> orderedAwareness = actor.OrderAwarenessByStructure();
        Dictionary<POINT_OF_INTEREST_TYPE, List<IPointOfInterest>> awareness = new Dictionary<POINT_OF_INTEREST_TYPE, List<IPointOfInterest>>(actor.awareness);
        foreach (KeyValuePair<POINT_OF_INTEREST_TYPE, List<IPointOfInterest>> kvp in awareness) {
            if (kvp.Key == POINT_OF_INTEREST_TYPE.CHARACTER) {
                for (int i = 0; i < kvp.Value.Count; i++) {
                    Character character = kvp.Value[i] as Character;
                    if (character.isDead) {
                        //kvp.Value.RemoveAt(i);
                        //i--;
                        if (allowDeadTargets) {
                            //if dead targets are allowed, advertise actions from dead targets
                            List<GoapAction> awarenessActions = character.AdvertiseActionsToActorFromDeadCharacter(actor, actorAllowedActions);
                            if (awarenessActions != null && awarenessActions.Count > 0) {
                                usableActions.AddRange(awarenessActions);
                            }
                        }
                    } else {
                        if (character.specificLocation == actor.specificLocation || character == actor || actor.IsPOIInCharacterAwarenessList(character, characterTargetsAwareness)) {
                            List<GoapAction> awarenessActions = character.AdvertiseActionsToActor(actor, actorAllowedActions);
                            if (awarenessActions != null && awarenessActions.Count > 0) {
                                usableActions.AddRange(awarenessActions);
                            }
                        }
                    }
                }
            } else if (kvp.Key == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
                //NOTE: Not happy about this, very unoptimized. Need to think of a way to make this more performant.
                Dictionary<LocationStructure, Dictionary<INTERACTION_TYPE, List<GoapAction>>> sorted = new Dictionary<LocationStructure, Dictionary<INTERACTION_TYPE, List<GoapAction>>>();
                for (int i = 0; i < kvp.Value.Count; i++) {
                    IPointOfInterest currAwareness = kvp.Value[i];
                    if (currAwareness.gridTileLocation != null && currAwareness.gridTileLocation.structure != null) {
                        LocationStructure objStructure = currAwareness.gridTileLocation.structure;
                        if (!sorted.ContainsKey(objStructure)) {
                            sorted.Add(objStructure, new Dictionary<INTERACTION_TYPE, List<GoapAction>>());
                        }
                        List<GoapAction> awarenessActions = currAwareness.AdvertiseActionsToActor(actor, actorAllowedActions);
                        if (awarenessActions != null) {
                            for (int j = 0; j < awarenessActions.Count; j++) {
                                GoapAction currAction = awarenessActions[j];
                                if (!sorted[objStructure].ContainsKey(currAction.goapType)) {
                                    sorted[objStructure].Add(currAction.goapType, new List<GoapAction>());
                                }
                                sorted[objStructure][currAction.goapType].Add(currAction);
                            }
                        }
                    }
                }
                foreach (KeyValuePair<LocationStructure, Dictionary<INTERACTION_TYPE, List<GoapAction>>> structureActions in sorted) {
                    foreach (KeyValuePair<INTERACTION_TYPE, List<GoapAction>> actions in structureActions.Value) {
                        if (actions.Key == INTERACTION_TYPE.CHOP_WOOD) {
                            //allow all chop wood actions to be advertised
                            usableActions.AddRange(actions.Value);
                        } else if (actions.Key == INTERACTION_TYPE.DOUSE_FIRE) {
                            //allow all douse fire actions to be advertised
                            usableActions.AddRange(actions.Value);
                        } else if (actions.Value.Count > 1) { //if there are multiple advertised actions of a type in the given structure, pick a random one
                            GoapAction chosenAction = actions.Value[Utilities.rng.Next(0, actions.Value.Count)];
                            usableActions.Add(chosenAction);
                        } else if (actions.Value.Count == 1) { //else if there is only one, just use that
                            usableActions.Add(actions.Value[0]);
                        }
                    }
                }
            } else {
                for (int i = 0; i < kvp.Value.Count; i++) {
                    List<GoapAction> awarenessActions = kvp.Value[i].AdvertiseActionsToActor(actor, actorAllowedActions);
                    if (awarenessActions != null && awarenessActions.Count > 0) {
                        usableActions.AddRange(awarenessActions);
                    }
                }
            }
        }

        //other data handling
        if (otherData != null) {
            log += "\nOTHER DATA: ";
            for (int i = 0; i < usableActions.Count; i++) {
                GoapAction currAction = usableActions[i];
                if (otherData.ContainsKey(currAction.goapType)) {
                    log += currAction.goapName + " (" + currAction.poiTarget.name + ")";
                    if (currAction.InitializeOtherData(otherData[currAction.goapType])) {
                        log += ": Initialized!";
                        //if other data was initialized, check if the action still meets the needed requirements
                        if (!currAction.CanSatisfyRequirements() || !currAction.CanSatisfyRequirementOnBuildGoapTree()) {
                            //if it no longer does, add as invalid
                            log += " Removing!, ";
                            usableActions.RemoveAt(i);
                            i--;
                            continue;
                        }
                    }
                    log += ", ";
                }
                if (otherData.ContainsKey(INTERACTION_TYPE.NONE)) {
                    log += "Universal " + currAction.goapName + " (" + currAction.poiTarget.name + ")";
                    if (currAction.InitializeOtherData(otherData[INTERACTION_TYPE.NONE])) {
                        log += ": Initialized!";
                        //if other data was initialized, check if the action still meets the needed requirements
                        if (!currAction.CanSatisfyRequirements() || !currAction.CanSatisfyRequirementOnBuildGoapTree()) {
                            //if it no longer does, add as invalid
                            log += " Removing!";
                            usableActions.RemoveAt(i);
                            i--;
                        }
                    }
                    log += ", ";
                }
            }
        } else {
            log += "\nNO OTHER DATA";
        }
       

        log += "\nUSABLE ACTIONS: ";
        List<GoapPlan> allPlans = new List<GoapPlan>();
        if (job != null && job.planConstructor != null) {
            GoapPlan plan = job.planConstructor.Invoke();
            if (plan != null) {
                allPlans.Add(plan);
                plan.SetListOfCharacterAwareness(characterTargetsAwareness);
            }
        } else if (goalType != INTERACTION_TYPE.NONE) {
            //provided goal type
            if (target == null) {
                for (int i = 0; i < usableActions.Count; i++) {
                    if (i > 0) {
                        log += ", ";
                    }
                    log += usableActions[i].goapName + " (" + usableActions[i].poiTarget.name + ")";
                    string planLog = string.Empty;
                    if (usableActions[i].goapType == goalType) {
                        GoapPlan plan = actor.planner.PlanActions(usableActions[i].poiTarget, usableActions[i], usableActions, category, isPersonalPlan, ref planLog, job);
                        if (plan != null) {
                            allPlans.Add(plan);
                            plan.SetListOfCharacterAwareness(characterTargetsAwareness);
                        }
                    }
                    //log += planLog;
                }
            } else {
                for (int i = 0; i < usableActions.Count; i++) {
                    if (i > 0) {
                        log += ", ";
                    }
                    log += usableActions[i].goapName + " (" + usableActions[i].poiTarget.name + ")";
                    if (usableActions[i].goapType == goalType && usableActions[i].poiTarget == target) {
                        string planLog = string.Empty;
                        GoapPlan plan = actor.planner.PlanActions(usableActions[i].poiTarget, usableActions[i], usableActions, category, isPersonalPlan, ref planLog, job);
                        if (plan != null) {
                            allPlans.Add(plan);
                            plan.SetListOfCharacterAwareness(characterTargetsAwareness);
                        }
                        //log += planLog;
                    }
                }
            }
        } else if (goalAction != null) {
            //provided goal action
            for (int i = 0; i < usableActions.Count; i++) {
                if (i > 0) {
                    log += ", ";
                }
                log += usableActions[i].goapName + " (" + usableActions[i].poiTarget.name + ")";
            }
            string planLog = string.Empty;
            GoapPlan plan = actor.planner.PlanActions(target, goalAction, usableActions, category, isPersonalPlan, ref planLog, job);
            if (plan != null) {
                allPlans.Add(plan);
                plan.SetListOfCharacterAwareness(characterTargetsAwareness);
            }
            //log += planLog;
        } else {
            //default
            for (int i = 0; i < usableActions.Count; i++) {
                if (i > 0) {
                    log += ", ";
                }
                log += usableActions[i].goapName + " (" + usableActions[i].poiTarget.name + ")";
                if (usableActions[i].WillEffectsSatisfyPrecondition(goalEffect)) {
                    log += "(GOAL EFFECT SATISFIED!)";
                    if (job != null && job.forcedActions.Count > 0) {
                        bool satisfiedForcedActions = true;
                        ForcedActionsComparer comparer = new ForcedActionsComparer();
                        foreach (KeyValuePair<GoapEffect, INTERACTION_TYPE> kvp in job.forcedActions) {
                            if (comparer.Equals(kvp.Key, goalEffect)) {
                                if (kvp.Value != usableActions[i].goapType) {
                                    satisfiedForcedActions = false;
                                    break;
                                }
                            }
                        }
                        if (satisfiedForcedActions) {
                            string planLog = string.Empty;
                            GoapPlan plan = actor.planner.PlanActions(target, usableActions[i], usableActions, category, isPersonalPlan, ref planLog, job);
                            if (plan != null) {
                                allPlans.Add(plan);
                                plan.SetListOfCharacterAwareness(characterTargetsAwareness);
                            }
                            //log += planLog;
                        }
                    } else {
                        string planLog = string.Empty;
                        GoapPlan plan = actor.planner.PlanActions(target, usableActions[i], usableActions, category, isPersonalPlan, ref planLog, job);
                        if (plan != null) {
                            allPlans.Add(plan);
                            plan.SetListOfCharacterAwareness(characterTargetsAwareness);
                        }
                        //log += planLog;
                        //Debug.Log(planLog);
                    }
                }
            }
        }

        log += "\nALL GENERATED PLANS: ";
        if (allPlans.Count > 0) {
            GoapPlan shortestPathToGoal = null;
            for (int i = 0; i < allPlans.Count; i++) {
                log += "\n" + (i + 1) + ". (" + allPlans[i].startingNode.runningCost + ") " + allPlans[i].startingNode.action.goapName + " - " + allPlans[i].startingNode.action.poiTarget.name + "[number of nodes = " + allPlans[i].GetNumOfNodes() + "]";
                //for (int j = 0; j < allPlans[i].allNodes.Count; j++) {
                //    if (j > 0) {
                //        log += ", ";
                //    }
                //    log += allPlans[i].allNodes[j].action.goapName + " - " + allPlans[i].allNodes[j].action.poiTarget.name;
                //}
                if (shortestPathToGoal == null) {
                    shortestPathToGoal = allPlans[i];
                } else {
                    if (allPlans[i].startingNode.runningCost < shortestPathToGoal.startingNode.runningCost) {
                        shortestPathToGoal = allPlans[i];
                    }
                }
            }
            //shortestPathToGoal.SetListOfCharacterAwareness(characterTargetsAwareness);
            shortestPathToGoal.ConstructAllNodes();
            log += shortestPathToGoal.LogPlan();
            createdPlan = shortestPathToGoal;
        } else {
            log += "\nNO PLAN WAS GENERATED! End goap...";
        }
    }
    private void RecalculatePlan() {
        if (recalculationPlan.isEnd) {
            log = "-----------------RECALCULATING PLAN OF " + actor.name;
            log += "\nPlan has already ended! Cannot recalculate!";
            return;
        }
        log = "-----------------RECALCULATING PLAN OF " + actor.name + " WITH TARGET " + recalculationPlan.target.name + " (" + actor.specificLocation.name + ")-----------------------";
        log += "\nGOAL ACTION: " + recalculationPlan.endNode.action.goapName + " - " + recalculationPlan.endNode.action.poiTarget.name;
        List<GoapAction> usableActions = new List<GoapAction>();
        List<INTERACTION_TYPE> actorAllowedActions = RaceManager.Instance.GetNPCInteractionsOfCharacter(actor);
        foreach (KeyValuePair<POINT_OF_INTEREST_TYPE, List<IPointOfInterest>> kvp in actor.awareness) {
            if (kvp.Key == POINT_OF_INTEREST_TYPE.CHARACTER) {
                for (int i = 0; i < kvp.Value.Count; i++) {
                    Character character = kvp.Value[i] as Character;
                    if (character.isDead) {
                        //kvp.Value.RemoveAt(i);
                        //i--;
                    } else {
                        if (character.specificLocation == actor.specificLocation || character == actor || actor.IsPOIInCharacterAwarenessList(character, recalculationPlan.goalCharacterTargets)) {
                            List<GoapAction> awarenessActions = kvp.Value[i].AdvertiseActionsToActor(actor, actorAllowedActions);
                            if (awarenessActions != null && awarenessActions.Count > 0) {
                                usableActions.AddRange(awarenessActions);
                            }
                        }
                    }
                }
            } else {
                for (int i = 0; i < kvp.Value.Count; i++) {
                    List<GoapAction> awarenessActions = kvp.Value[i].AdvertiseActionsToActor(actor, actorAllowedActions);
                    if (awarenessActions != null && awarenessActions.Count > 0) {
                        usableActions.AddRange(awarenessActions);
                    }
                }
            }
        }

        log += "\nUSABLE ACTIONS: ";
        if (usableActions.Count > 0) {
            for (int i = 0; i < usableActions.Count; i++) {
                if (i > 0) {
                    log += ", ";
                }
                log += usableActions[i].goapName + " (" + usableActions[i].poiTarget.name + ")";
            }
            bool success = actor.planner.RecalculatePathForPlan(recalculationPlan, usableActions);
            if (success) {
                recalculationPlan.ConstructAllNodes();
                log += "\nSUCCESSFULLY RECALCULATED PLAN!";
                log += recalculationPlan.LogPlan();
                createdPlan = recalculationPlan;
            } else {
                log += "\nFAILED TO RECALCULATE PLAN!";
            }
        } else {
            log += "\nNO USABLE ACTIONS! FAILED TO RECALCULATE PLAN!";
        }
    }
    public void ReturnPlanFromGoapThread() {
        actor.ReceivePlanFromGoapThread(this);
    }
}
