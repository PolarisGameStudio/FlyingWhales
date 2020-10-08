﻿using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Reflection;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Logs;
using Traits;

public class GoapAction {

    public INTERACTION_TYPE goapType { get; private set; }
    public virtual ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.DIRECT; } }
    public string goapName { get; protected set; }
    public List<Precondition> basePreconditions { get; private set; }
    public List<GoapEffect> baseExpectedEffects { get; private set; }
    public List<GoapEffectConditionTypeAndTargetType> possibleExpectedEffectsTypeAndTargetMatching { get; private set; }
    public RACE[] racesThatCanDoAction { get; protected set; }
    public Dictionary<string, GoapActionState> states { get; protected set; }
    public ACTION_LOCATION_TYPE actionLocationType { get; protected set; } //This is set in every action's constructor
    public bool showNotification { get; protected set; } //should this action show a notification when it is done by its actor or when it receives a plan with this action as it's end node?
    public bool shouldAddLogs { get; protected set; } //should this action add logs to it's actor?
    // public bool shouldIntelNotificationOnlyIfActorIsActive { get; protected set; }
    public bool isNotificationAnIntel { get; protected set; }
    public string actionIconString { get; protected set; }
    public string animationName { get; protected set; } //what animation should the character be playing while doing this action
    public bool doesNotStopTargetCharacter { get; protected set; }
    public bool canBeAdvertisedEvenIfTargetIsUnavailable { get; protected set; }
    public bool canBePerformedEvenIfPathImpossible { get; protected set; } //can this action still be advertised even if there is no path towards the target
    protected TIME_IN_WORDS[] validTimeOfDays;
    public POINT_OF_INTEREST_TYPE[] advertisedBy { get; protected set; } //list of poi types that can advertise this action
    public LOG_TAG[] logTags { get; protected set; }

    #region getters
    public string name => goapName;
    public virtual Type uniqueActionDataType => null; 
    #endregion

    public GoapAction(INTERACTION_TYPE goapType) { //, INTERACTION_ALIGNMENT alignment, Character actor, IPointOfInterest poiTarget
        this.goapType = goapType;
        this.goapName = UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(goapType.ToString());
        showNotification = false;
        shouldAddLogs = true;
        basePreconditions = new List<Precondition>();
        baseExpectedEffects = new List<GoapEffect>();
        possibleExpectedEffectsTypeAndTargetMatching = new List<GoapEffectConditionTypeAndTargetType>();
        actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
        actionIconString = GoapActionStateDB.No_Icon;
        canBeAdvertisedEvenIfTargetIsUnavailable = false;
        canBePerformedEvenIfPathImpossible = false;
        animationName = "Interacting";
        ConstructBasePreconditionsAndEffects();
        CreateStates();
    }

    #region States
    public void SetState(string stateName, ActualGoapNode actionNode) {
        actionNode.OnActionStateSet(stateName);
        Messenger.Broadcast(Signals.AFTER_ACTION_STATE_SET, stateName, actionNode);
    }
    #endregion

    #region Virtuals
    private void CreateStates() {
        string summary = $"Creating states for goap action (Dynamic) {goapType}";
        states = new Dictionary<string, GoapActionState>();
        if (GoapActionStateDB.goapActionStates.ContainsKey(this.goapType)) {
            StateNameAndDuration[] statesSetup = GoapActionStateDB.goapActionStates[this.goapType];
            for (int i = 0; i < statesSetup.Length; i++) {
                StateNameAndDuration state = statesSetup[i];
                summary += $"\nCreating {state.name}";
                string trimmedState = UtilityScripts.Utilities.RemoveAllWhiteSpace(state.name);
                Type thisType = this.GetType();
                string estimatedPreMethodName = $"Pre{trimmedState}";
                string estimatedPerTickMethodName = $"PerTick{trimmedState}";
                string estimatedAfterMethodName = $"After{trimmedState}";

                MethodInfo preMethod = thisType.GetMethod(estimatedPreMethodName, new Type[] { typeof(ActualGoapNode) }); //
                MethodInfo perMethod = thisType.GetMethod(estimatedPerTickMethodName, new Type[] { typeof(ActualGoapNode) });
                MethodInfo afterMethod = thisType.GetMethod(estimatedAfterMethodName, new Type[] { typeof(ActualGoapNode) });
                Action<ActualGoapNode> preAction = null;
                Action<ActualGoapNode> perAction = null;
                Action<ActualGoapNode> afterAction = null;
                if (preMethod != null) {
                    preAction = (Action<ActualGoapNode>) Delegate.CreateDelegate(typeof(Action<ActualGoapNode>), this, preMethod, false);
                    summary += $"\n\tPre Method is {preMethod}";
                } else {
                    summary += "\n\tPre Method is null";
                }
                if (perMethod != null) {
                    perAction = (Action<ActualGoapNode>) Delegate.CreateDelegate(typeof(Action<ActualGoapNode>), this, perMethod, false);
                    summary += $"\n\tPer Tick Method is {perAction}";
                } else {
                    summary += "\n\tPer Tick Method is null";
                }
                if (afterMethod != null) {
                    afterAction = (Action<ActualGoapNode>) Delegate.CreateDelegate(typeof(Action<ActualGoapNode>), this, afterMethod, false);
                    summary += $"\n\tAfter Method is {afterAction}";
                } else {
                    summary += "\n\tAfter Method is null";
                }
                GoapActionState newState = new GoapActionState(state.name, this, preAction, perAction, afterAction, state.duration, state.status, state.animationName);
                states.Add(state.name, newState);
                //summary += "\n Creating state " + state.name;
            }
        }
        //Debug.Log(summary);
    }
    protected virtual void ConstructBasePreconditionsAndEffects() { }
    public virtual void Perform(ActualGoapNode actionNode) { }
    protected virtual bool AreRequirementsSatisfied(Character actor, IPointOfInterest target, OtherData[] otherData) { return true; }
    protected virtual int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
        return 0;
    }
    public virtual void AddFillersToLog(ref Log log, ActualGoapNode node) {
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        LocationStructure targetStructure = node.targetStructure;
        log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        if (log.DoesLogUseIdentifier(LOG_IDENTIFIER.TARGET_CHARACTER)) {
            //only automatically add Target Character log filler to log if the log actually needs it. This is to optimize saving so that unnecessary objects won't be included in log saves.
            log.AddToFillers(poiTarget, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER); //Target character is only the identifier but it doesn't mean that this is a character, it can be item, etc.    
        }
        if (targetStructure != null) {
            log.AddToFillers(targetStructure, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
        }
        // else {
        //     log.AddToFillers(actor.currentRegion, actor.currentRegion.name, LOG_IDENTIFIER.LANDMARK_1);
        // }
    }
    public virtual bool IsInvalidOnVision(ActualGoapNode node) {
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        if(poiTarget is Character targetCharacter) {
            if (targetCharacter.combatComponent.isInActualCombat) {
                return true;
            }
        }
        return false;
    }
    public virtual GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        string stateName = "Target Missing";
        bool defaultTargetMissing = IsTargetMissing(node);
        GoapActionInvalidity goapActionInvalidity = new GoapActionInvalidity(defaultTargetMissing, stateName);
        //if (defaultTargetMissing == false) {
        //    //check the target's traits, if any of them can make this action invalid
        //    for (int i = 0; i < poiTarget.traitContainer.allTraits.Count; i++) {
        //        Trait trait = poiTarget.traitContainer.allTraits[i];
        //        if (trait.TryStopAction(goapType, actor, poiTarget, ref goapActionInvalidity)) {
        //            break; //a trait made this action invalid, stop loop
        //        }
        //    }
        //}
        return goapActionInvalidity;
    }
    public virtual void OnInvalidAction(ActualGoapNode node) { }
    public virtual LocationStructure GetTargetStructure(ActualGoapNode node) {
        //if (poiTarget is Character) {
        //    return (poiTarget as Character).currentStructure;
        //}
        IPointOfInterest poiTarget = node.poiTarget;
        if (poiTarget.gridTileLocation == null) {
            if (poiTarget is TileObject tileObject && tileObject.isBeingCarriedBy?.currentStructure != null) {
                return tileObject.isBeingCarriedBy.currentStructure;
            }
            return null;
        }
        return poiTarget.gridTileLocation.structure;
    }
    /// <summary>
    /// Function to use when actionLocationType is NEAR_TARGET. <see cref="GoapNode.MoveToDoAction"/>
    /// Will, by default, return the poiTarget, but can be overridden to make actor go somewhere else.
    /// </summary>
    /// <returns></returns>
    public virtual IPointOfInterest GetTargetToGoTo(ActualGoapNode goapNode) {
        return goapNode.poiTarget;
    }
    public virtual LocationGridTile GetTargetTileToGoTo(ActualGoapNode goapNode) {
        return goapNode.poiTarget.gridTileLocation;
    }
    //If this action is being performed and is stopped abruptly, call this
    public virtual void OnStopWhilePerforming(ActualGoapNode node) { }
    /// <summary>
    /// What should happen when an action is stopped while the actor is still travelling towards it's target or when the action has already started?
    /// </summary>
    public virtual void OnStopWhileStarted(ActualGoapNode node) { }
    public virtual LocationGridTile GetOverrideTargetTile(ActualGoapNode goapNode) {
        return null;
    }
    /// <summary>
    /// If the actionLocationType is set to NEARBY, will check this first, if it is null, then will use default way.
    /// </summary>
    /// <param name="goapNode"></param>
    /// <returns>List of tile choices</returns>
    public virtual List<LocationGridTile> NearbyLocationGetter(ActualGoapNode goapNode) { return null; }
    public virtual string ReactionToActor(Character actor, IPointOfInterest target, Character witness,
        ActualGoapNode node, REACTION_STATUS status) { return string.Empty; }
    public virtual string ReactionToTarget(Character actor, IPointOfInterest target, Character witness,
        ActualGoapNode node, REACTION_STATUS status) { return string.Empty; }
    public virtual string ReactionOfTarget(Character actor, IPointOfInterest target, ActualGoapNode node,
        REACTION_STATUS status) { return string.Empty; }
    public virtual void OnActionStarted(ActualGoapNode node) { }
    public virtual void OnStoppedInterrupt(ActualGoapNode node) { }
    public virtual REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness) {
        return REACTABLE_EFFECT.Neutral;
    }
    public virtual void OnMoveToDoAction(ActualGoapNode node) { }
    #endregion

    #region Utilities
    public int GetCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
        int baseCost = GetBaseCost(actor, target, job, otherData);
        //modify costs based on actor's and target's traits
        //for (int i = 0; i < actor.traitContainer.allTraits.Count; i++) {
        //    Trait trait = actor.traitContainer.allTraits[i];
        //    trait.ExecuteCostModification(goapType, actor, target, otherData, ref baseCost);
        //}
        //for (int i = 0; i < target.traitContainer.allTraits.Count; i++) {
        //    Trait trait = target.traitContainer.allTraits[i];
        //    trait.ExecuteCostModification(goapType, actor, target, otherData, ref baseCost);
        //}
        return (baseCost * TimeOfDaysCostMultiplier(actor) * PreconditionCostMultiplier()) + GetDistanceCost(actor, target, job);
    }
    private bool IsTargetMissing(ActualGoapNode node) {
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        if (poiTarget.IsAvailable() == false || poiTarget.gridTileLocation == null) {
            return true;
        }
        if (actionLocationType != ACTION_LOCATION_TYPE.IN_PLACE && actor.currentRegion != poiTarget.gridTileLocation.structure.region) {
            return true;
        }
        if (actionLocationType == ACTION_LOCATION_TYPE.NEAR_TARGET) {
            //if the action type is NEAR_TARGET, then check if the actor is near the target, if not, this action is invalid.
            if (actor.gridTileLocation != poiTarget.gridTileLocation && actor.gridTileLocation.IsNeighbour(poiTarget.gridTileLocation) == false) {
                return true;
            }
        } else if (actionLocationType == ACTION_LOCATION_TYPE.NEAR_OTHER_TARGET) {
            //if the action type is NEAR_TARGET, then check if the actor is near the target, if not, this action is invalid.
            if (actor.gridTileLocation != node.targetTile && actor.gridTileLocation.IsNeighbour(node.targetTile) == false) {
                return true;
            }
        }
        return false;
    }
    public bool CanSatisfyRequirements(Character actor, IPointOfInterest poiTarget, OtherData[] otherData) {
        // bool requirementActionSatisfied = !(poiTarget.poiType != POINT_OF_INTEREST_TYPE.CHARACTER 
        //                                     && poiTarget.traitContainer.HasTrait("Frozen") 
        //                                     && (actionCategory == ACTION_CATEGORY.DIRECT || actionCategory == ACTION_CATEGORY.CONSUME));
        bool requirementActionSatisfied = true;
        if (poiTarget is TileObject tileObject) {
            if (tileObject.traitContainer.HasTrait("Frozen") && (actionCategory == ACTION_CATEGORY.DIRECT || actionCategory == ACTION_CATEGORY.CONSUME)) {
                //if tile object is frozen and action is direct or consume, do not advertise this action.
                requirementActionSatisfied = false;
            }
            if (actionCategory == ACTION_CATEGORY.CONSUME) {
                //if action is consume type and actor knows that the object is poisoned, do not advertise this action.
                Poisoned poisoned = tileObject.traitContainer.GetTraitOrStatus<Poisoned>("Poisoned");
                if (poisoned != null && poisoned.awareCharacters.Contains(actor)) {
                    requirementActionSatisfied = false;    
                }    
                //if action is consume type and actor knows that the object is booby trapped, do not advertise this action.
                BoobyTrapped boobyTrapped = tileObject.traitContainer.GetTraitOrStatus<BoobyTrapped>("Booby Trapped");
                if (boobyTrapped != null && boobyTrapped.awareCharacters.Contains(actor)) {
                    requirementActionSatisfied = false;    
                }    
            } else if (actionCategory == ACTION_CATEGORY.DIRECT) {
                //if action is direct type and actor knows that the object is booby trapped, do not advertise this action.
                BoobyTrapped boobyTrapped = tileObject.traitContainer.GetTraitOrStatus<BoobyTrapped>("Booby Trapped");
                if (boobyTrapped != null && boobyTrapped.awareCharacters.Contains(actor)) {
                    requirementActionSatisfied = false;    
                }    
            }
        }
        
        //https://trello.com/c/Pj6zRg3O/2404-paralyzed-vampire
        if (!actor.canPerform && actionLocationType != ACTION_LOCATION_TYPE.NEARBY && actionLocationType != ACTION_LOCATION_TYPE.IN_PLACE) {
            //Cannot perform characters can only perform NEARBY and IN PLACE actions
            requirementActionSatisfied = true;
        }
        
        if (requirementActionSatisfied) {
            requirementActionSatisfied = AreRequirementsSatisfied(actor, poiTarget, otherData);
        }
        return requirementActionSatisfied; //&& (validTimeOfDays == null || validTimeOfDays.Contains(GameManager.GetCurrentTimeInWordsOfTick()));
    }
    public bool DoesCharacterMatchRace(Character character) {
        //If no race is specified, assume all races are allowed
        if (racesThatCanDoAction == null) {
            return true;
        } else {
            return racesThatCanDoAction.Contains(character.race);
        }
    }
    private int GetDistanceCost(Character actor, IPointOfInterest poiTarget, JobQueueItem job) {
        // if (actor.currentNpcSettlement == null) {
        //     return 1;
        // }
        if (job.jobType == JOB_TYPE.SNATCH || job.jobType == JOB_TYPE.DROP_ITEM_PARTY) {
            return 1; //ignore distance cost if job is snatch, this is so that snatchers won't reach the maximum cost when trying to snatch someone from a different region. 
        }
        LocationGridTile tile = poiTarget.gridTileLocation;
        if (actor.gridTileLocation != null && tile != null) {
            int distance = Mathf.RoundToInt(actor.gridTileLocation.GetDistanceTo(tile));
            distance = (int) (distance * 2f);
            if (actor.currentRegion != tile.structure.region) {
                return distance + 100;
            }
            return distance;
        }
        return 1;
    }
    private int TimeOfDaysCostMultiplier(Character actor) {
        if (validTimeOfDays == null || validTimeOfDays.Contains(GameManager.GetCurrentTimeInWordsOfTick(actor))) {
            return 1;
        }
        return 3;
    }
    private int PreconditionCostMultiplier() {
        return 1; //Math.Max(basePreconditions.Count * 2, 1);
    }
    public void LogActionInvalid(GoapActionInvalidity goapActionInvalidity, ActualGoapNode node) {
        string invalidKey = goapActionInvalidity.stateName.ToLower() + "_description";
        if (goapActionInvalidity.stateName != "Target Missing" && LocalizationManager.Instance.HasLocalizedValue("GoapAction", name, invalidKey)) {
            Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "GoapAction", name, invalidKey, providedTags: LOG_TAG.Misc);
            AddFillersToLog(ref log, node);
            log.AddLogToDatabase();
        } else {
            Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "GoapAction", "Generic", "Invalid", providedTags: LOG_TAG.Misc);
            log.AddToFillers(node.actor, node.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddToFillers(node.poiTarget, node.poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            log.AddToFillers(null, UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetterOnly(goapType.ToString()), LOG_IDENTIFIER.STRING_1);
            log.AddLogToDatabase();
        }
    }
    #endregion

    #region Preconditions
    protected void AddPrecondition(GoapEffect effect, Func<Character, IPointOfInterest, OtherData[], JOB_TYPE, bool> condition) {
        basePreconditions.Add(new Precondition(effect, condition));
    }
    public bool CanSatisfyAllPreconditions(Character actor, IPointOfInterest target, OtherData[] otherData, JOB_TYPE jobType) {
        List<Precondition> preconditions = GetPreconditions(actor, target, otherData);
        for (int i = 0; i < preconditions.Count; i++) {
            if (!preconditions[i].CanSatisfyCondition(actor, target, otherData, jobType)) {
                return false;
            }
        }
        return true;
    }
    public virtual List<Precondition> GetPreconditions(Character actor, IPointOfInterest target, OtherData[] otherData) {
        return basePreconditions;
    }
    #endregion

    #region Effects
    protected void AddExpectedEffect(GoapEffect effect) {
        baseExpectedEffects.Add(effect);
        AddPossibleExpectedEffectForTypeAndTargetMatching(new GoapEffectConditionTypeAndTargetType(effect.conditionType, effect.target));
    }
    protected void AddPossibleExpectedEffectForTypeAndTargetMatching(GoapEffectConditionTypeAndTargetType effect) {
        possibleExpectedEffectsTypeAndTargetMatching.Add(effect);
    }
    public bool WillEffectsSatisfyPrecondition(GoapEffect precondition, Character actor, IPointOfInterest target, OtherData[] otherData) {
        List<GoapEffect> effects = GetExpectedEffects(actor, target, otherData);
        for (int i = 0; i < effects.Count; i++) {
            if(EffectPreconditionMatching(effects[i], precondition)) {
                return true;
            }
        }
        return false;
    }
    public bool WillEffectsMatchPreconditionTypeAndTarget(GoapEffect precondition) {
        List<GoapEffectConditionTypeAndTargetType> effects = possibleExpectedEffectsTypeAndTargetMatching;
        for (int i = 0; i < effects.Count; i++) {
            if (effects[i].conditionType == precondition.conditionType && effects[i].target == precondition.target) {
                return true;
            }
        }
        return false;
    }
    private bool EffectPreconditionMatching(GoapEffect effect, GoapEffect precondition) {
        if(effect.conditionType == precondition.conditionType && effect.target == precondition.target) { //&& CharacterManager.Instance.POIValueTypeMatching(effect.targetPOI, precondition.targetPOI)
            if (effect.conditionKey != "" && precondition.conditionKey != "") {
                if(effect.isKeyANumber && precondition.isKeyANumber) {
                    int effectInt = int.Parse(effect.conditionKey);
                    int preconditionInt = int.Parse(precondition.conditionKey);
                    return effectInt >= preconditionInt;
                } else {
                    if (precondition.conditionKey == "Food Pile") {
                        //if precondition is looking for a food pile, allow actions that have the following effects:
                        //TODO: There might be a better way to do this?
                        return effect.conditionKey == "Animal Meat" || effect.conditionKey == "Human Meat" ||
                               effect.conditionKey == "Elf Meat" || effect.conditionKey == "Vegetables" ||
                               effect.conditionKey == "Fish Pile" || effect.conditionKey == "Food Pile";
                    }
                    return effect.conditionKey == precondition.conditionKey;
                }
                //switch (effect.conditionType) {
                //    case GOAP_EFFECT_CONDITION.HAS_SUPPLY:
                //    case GOAP_EFFECT_CONDITION.HAS_FOOD:
                //        int effectInt = (int) effect.conditionKey;
                //        int preconditionInt = (int) precondition.conditionKey;
                //        return effectInt >= preconditionInt;
                //    default:
                //        return effect.conditionKey == precondition.conditionKey;
                //}
            } else {
                return true;
            }
        }
        return false;
    }
    protected virtual List<GoapEffect> GetExpectedEffects(Character actor, IPointOfInterest target, OtherData[] otherData) {
        List<GoapEffect> effects = new List<GoapEffect>(baseExpectedEffects);
        //modify expected effects depending on actor's traits
        List<Trait> traitOverrideFunctions = actor.traitContainer.GetTraitOverrideFunctions(TraitManager.Execute_Expected_Effect_Trait);
        if (traitOverrideFunctions != null) {
            for (int i = 0; i < traitOverrideFunctions.Count; i++) {
                Trait trait = traitOverrideFunctions[i];
                trait.ExecuteExpectedEffectModification(goapType, actor, target, otherData, ref effects);
            }
        }
        return effects;
    }
    #endregion

    #region Crime
    public virtual CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, ActualGoapNode crime) {
        return CRIME_TYPE.None;
    }
    #endregion
}

public struct GoapActionInvalidity {
    public bool isInvalid;
    public string stateName;

    public GoapActionInvalidity(bool isInvalid, string stateName) {
        this.isInvalid = isInvalid;
        this.stateName = stateName;
    }
}
public struct GoapEffectConditionTypeAndTargetType {
    public GOAP_EFFECT_CONDITION conditionType;
    public GOAP_EFFECT_TARGET target;
    
    public GoapEffectConditionTypeAndTargetType(GOAP_EFFECT_CONDITION conditionType, GOAP_EFFECT_TARGET target) {
        this.conditionType = conditionType;
        this.target = target;
    }
}

[System.Serializable]
public struct GoapEffect {
    public GOAP_EFFECT_CONDITION conditionType;
    //public object conditionKey;
    public string conditionKey;
    public bool isKeyANumber;
    public GOAP_EFFECT_TARGET target;
    //public IPointOfInterest targetPOI; //this is the target that will be affected by the condition type and key

    public GoapEffect(GOAP_EFFECT_CONDITION conditionType, string conditionKey, bool isKeyANumber, GOAP_EFFECT_TARGET target) {
        this.conditionType = conditionType;
        this.conditionKey = conditionKey;
        this.isKeyANumber = isKeyANumber;
        this.target = target;
    }
    public void Reset() {
        conditionType = GOAP_EFFECT_CONDITION.NONE;
        conditionKey = string.Empty;
        isKeyANumber = false;
        target = GOAP_EFFECT_TARGET.ACTOR;
    }

    public override string ToString() {
        return $"{conditionType.ToString()} - {conditionKey} - {target.ToString()}";
    }
    //public string conditionString() {
    //    if(conditionKey is string) {
    //        return conditionKey.ToString();
    //    } else if (conditionKey is int) {
    //        return conditionKey.ToString();
    //    } else if (conditionKey is Character) {
    //        return (conditionKey as Character).name;
    //    } else if (conditionKey is NPCSettlement) {
    //        return (conditionKey as NPCSettlement).name;
    //    } else if (conditionKey is Region) {
    //        return (conditionKey as Region).name;
    //    } else if (conditionKey is SpecialToken) {
    //        return (conditionKey as SpecialToken).name;
    //    } else if (conditionKey is IPointOfInterest) {
    //        return (conditionKey as IPointOfInterest).name;
    //    }
    //    return string.Empty;
    //}
    //public string conditionKeyToString() {
    //    if (conditionKey is string) {
    //        return (string)conditionKey;
    //    } else if (conditionKey is int) {
    //        return ((int)conditionKey).ToString();
    //    } else if (conditionKey is Character) {
    //        return (conditionKey as Character).id.ToString();
    //    } else if (conditionKey is NPCSettlement) {
    //        return (conditionKey as NPCSettlement).id.ToString();
    //    } else if (conditionKey is Region) {
    //        return (conditionKey as Region).id.ToString();
    //    } else if (conditionKey is SpecialToken) {
    //        return (conditionKey as SpecialToken).id.ToString();
    //    } else if (conditionKey is IPointOfInterest) {
    //        return (conditionKey as IPointOfInterest).id.ToString();
    //    }
    //    return string.Empty;
    //}
    //public string conditionKeyTypeString() {
    //    if (conditionKey is string) {
    //        return "string";
    //    } else if (conditionKey is int) {
    //        return "int";
    //    } else if (conditionKey is Character) {
    //        return "character";
    //    } else if (conditionKey is NPCSettlement) {
    //        return "npcSettlement";
    //    } else if (conditionKey is Region) {
    //        return "region";
    //    } else if (conditionKey is SpecialToken) {
    //        return "item";
    //    } else if (conditionKey is IPointOfInterest) {
    //        return "poi";
    //    }
    //    return string.Empty;
    //}

    //public override bool Equals(object obj) {
    //    if (obj is GoapEffect) {
    //        GoapEffect otherEffect = (GoapEffect)obj;
    //        if (otherEffect.conditionType == conditionType) {
    //            if (string.IsNullOrEmpty(conditionString())) {
    //                return true;
    //            } else {
    //                return otherEffect.conditionString() == conditionString();
    //            }
    //        }
    //    }
    //    return base.Equals(obj);
    //}
}

[System.Serializable]
public class SaveDataGoapEffect {
    public GOAP_EFFECT_CONDITION conditionType;

    public string conditionKey;
    public string conditionKeyIdentifier;
    public POINT_OF_INTEREST_TYPE conditionKeyPOIType;
    public TILE_OBJECT_TYPE conditionKeyTileObjectType;


    public int targetPOIID;
    public POINT_OF_INTEREST_TYPE targetPOIType;
    public TILE_OBJECT_TYPE targetPOITileObjectType;

    public void Save(GoapEffect goapEffect) {
        conditionType = goapEffect.conditionType;

        //if(goapEffect.conditionKey != null) {
        //    conditionKeyIdentifier = goapEffect.conditionKeyTypeString();
        //    conditionKey = goapEffect.conditionKeyToString();
        //    if(goapEffect.conditionKey is IPointOfInterest) {
        //        conditionKeyPOIType = (goapEffect.conditionKey as IPointOfInterest).poiType;
        //    }
        //    if (goapEffect.conditionKey is TileObject) {
        //        conditionKeyTileObjectType = (goapEffect.conditionKey as TileObject).tileObjectType;
        //    }
        //} else {
        //    conditionKeyIdentifier = string.Empty;
        //}

        //if(goapEffect.targetPOI != null) {
        //    targetPOIID = goapEffect.targetPOI.id;
        //    targetPOIType = goapEffect.targetPOI.poiType;
        //    if(goapEffect.targetPOI is TileObject) {
        //        targetPOITileObjectType = (goapEffect.targetPOI as TileObject).tileObjectType;
        //    }
        //} else {
        //    targetPOIID = -1;
        //}
    }

    public GoapEffect Load() {
        GoapEffect effect = new GoapEffect() {
            conditionType = conditionType,
        };
        //if(targetPOIID != -1) {
        //    GoapEffect tempEffect = effect;
        //    if (targetPOIType == POINT_OF_INTEREST_TYPE.CHARACTER) {
        //        tempEffect.targetPOI = CharacterManager.Instance.GetCharacterByID(targetPOIID);
        //    } else if (targetPOIType == POINT_OF_INTEREST_TYPE.ITEM) {
        //        tempEffect.targetPOI = TokenManager.Instance.GetSpecialTokenByID(targetPOIID);
        //    } else if (targetPOIType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
        //        tempEffect.targetPOI = InteriorMapManager.Instance.GetTileObject(targetPOITileObjectType, targetPOIID);
        //    }
        //    effect = tempEffect;
        //}
        //if(conditionKeyIdentifier != string.Empty) {
        //    GoapEffect tempEffect = effect;
        //    if (conditionKeyIdentifier == "string") {
        //        tempEffect.conditionKey = conditionKey;
        //    } else if (conditionKey == "int") {
        //        tempEffect.conditionKey = int.Parse(conditionKey);
        //    } else if (conditionKey == "character") {
        //        tempEffect.conditionKey = CharacterManager.Instance.GetCharacterByID(int.Parse(conditionKey));
        //    } else if (conditionKey == "npcSettlement") {
        //        tempEffect.conditionKey = LandmarkManager.Instance.GetAreaByID(int.Parse(conditionKey));
        //    } else if (conditionKey == "region") {
        //        tempEffect.conditionKey = GridMap.Instance.GetRegionByID(int.Parse(conditionKey));
        //    } else if (conditionKey == "item") {
        //        tempEffect.conditionKey = TokenManager.Instance.GetSpecialTokenByID(int.Parse(conditionKey));
        //    } else if (conditionKey == "poi") {
        //        if (conditionKeyPOIType == POINT_OF_INTEREST_TYPE.CHARACTER) {
        //            tempEffect.conditionKey = CharacterManager.Instance.GetCharacterByID(int.Parse(conditionKey));
        //        } else if (conditionKeyPOIType == POINT_OF_INTEREST_TYPE.ITEM) {
        //            tempEffect.conditionKey = TokenManager.Instance.GetSpecialTokenByID(int.Parse(conditionKey));
        //        } else if (conditionKeyPOIType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
        //            tempEffect.conditionKey = InteriorMapManager.Instance.GetTileObject(conditionKeyTileObjectType, int.Parse(conditionKey));
        //        }
        //    }
        //    effect = tempEffect;
        //}
        return effect;
    }
}