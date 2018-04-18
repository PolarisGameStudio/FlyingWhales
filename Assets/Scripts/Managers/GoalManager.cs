﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GoalManager : MonoBehaviour {

    public static GoalManager Instance = null;

    [SerializeField] private List<WeightedActionRequirements> actionRequirements;
    [SerializeField] private List<TraitWeightedActionRequirements> traitActionRequirements;

    public Dictionary<WEIGHTED_ACTION, List<WEIGHTED_ACTION_REQS>> weightedActionRequirements;
    public Dictionary<TRAIT, Dictionary<WEIGHTED_ACTION, List<WEIGHTED_ACTION_REQS>>> weightedActionRequirementsForTraits;

    public static HashSet<WEIGHTED_ACTION> indirectActionTypes = new HashSet<WEIGHTED_ACTION>() {
        WEIGHTED_ACTION.ALLIANCE_OF_CONQUEST
    };

    public static HashSet<WEIGHTED_ACTION> specialActionTypes = new HashSet<WEIGHTED_ACTION>() {
        //WEIGHTED_ACTION.DECLARE_PEACE,
        WEIGHTED_ACTION.LEAVE_ALLIANCE, WEIGHTED_ACTION.LEAVE_TRADE_DEAL
    };

    private void Awake() {
        Instance = this;
        ConstructActionRequirementsDictionary();
    }

    private void ConstructActionRequirementsDictionary() {
        weightedActionRequirements = new Dictionary<WEIGHTED_ACTION, List<WEIGHTED_ACTION_REQS>>();
        for (int i = 0; i < actionRequirements.Count; i++) {
            WeightedActionRequirements currReq = actionRequirements[i];
            weightedActionRequirements.Add(currReq.weightedAction, currReq.requirements);
        }
        weightedActionRequirementsForTraits = new Dictionary<TRAIT, Dictionary<WEIGHTED_ACTION, List<WEIGHTED_ACTION_REQS>>>();
        for (int i = 0; i < traitActionRequirements.Count; i++) {
            TraitWeightedActionRequirements currTrait = traitActionRequirements[i];
            Dictionary <WEIGHTED_ACTION, List<WEIGHTED_ACTION_REQS>> reqs = new Dictionary<WEIGHTED_ACTION, List<WEIGHTED_ACTION_REQS>>();
            for (int j = 0; j < currTrait.actionRequirements.Count; j++) {
                WeightedActionRequirements currReq = currTrait.actionRequirements[j];
                reqs.Add(currReq.weightedAction, currReq.requirements);
            }
            weightedActionRequirementsForTraits.Add(currTrait.trait, reqs);
        }
    }

    internal WEIGHTED_ACTION DetermineWeightedActionToPerform(Kingdom sourceKingdom) {
        Debug.Log("========== " + GameManager.Instance.month + "/" + GameManager.Instance.days + "/" + GameManager.Instance.year + " - " + sourceKingdom.name + " is trying to decide what to do... ==========");

        WeightedDictionary<WEIGHTED_ACTION> weightedActions = new WeightedDictionary<WEIGHTED_ACTION>();
        weightedActions.AddElement(WEIGHTED_ACTION.DO_NOTHING, 150); //Add 150 Base Weight on Do Nothing Action

        if (ActionMeetsRequirements(sourceKingdom, WEIGHTED_ACTION.LEAVE_TRADE_DEAL)) {
            //If in a trade deal, add 5 weight to leave trade deal for each active trade deal
            weightedActions.AddElement(WEIGHTED_ACTION.LEAVE_TRADE_DEAL, 5 * sourceKingdom.kingdomsInTradeDealWith.Count);
        }
        for (int i = 0; i < sourceKingdom.king.allTraits.Count; i++) {
            Trait currTrait = sourceKingdom.king.allTraits[i];
            Dictionary<WEIGHTED_ACTION, int> weightsFromCurrTrait = currTrait.GetTotalActionWeights();
            weightedActions.AddElements(weightsFromCurrTrait);
        }

        weightedActions.LogDictionaryValues("Action Weights of " + sourceKingdom.name);
        WEIGHTED_ACTION chosenAction = weightedActions.PickRandomElementGivenWeights();
        Debug.Log("Chosen action of " + sourceKingdom.name + " is " + chosenAction.ToString());
        return chosenAction;
    }

    #region Weight Dictionaries
    internal WeightedDictionary<T> GetWeightsForSpecialActionType<T>(Kingdom source, List<T> choices, WEIGHTED_ACTION actionType, ref int weightToNotPerformAction) {
        WeightedDictionary<T> weights = new WeightedDictionary<T>();
        for (int i = 0; i < choices.Count; i++) {
            T currChoice = choices[i];
            int weightForCurrChoice = 0;
            //loop through all the traits of the current king
            for (int j = 0; j < source.king.allTraits.Count; j++) {
                Trait currTrait = source.king.allTraits[j];
                int modificationFromTrait = currTrait.GetWeightOfActionGivenTarget(actionType, currChoice, weightForCurrChoice);
                weightToNotPerformAction += currTrait.GetDontDoActionWeight(actionType, currChoice);
                weightForCurrChoice += modificationFromTrait;
            }
            ApplySpecialActionModificationForAll(actionType, source, currChoice, ref weightForCurrChoice, ref weightToNotPerformAction);
            weights.AddElement(currChoice, weightForCurrChoice);
        }
        return weights;
    }
    internal WeightedDictionary<Kingdom> GetKingdomWeightsForActionType(Kingdom sourceKingdom, WEIGHTED_ACTION weightedAction) {
        WeightedDictionary<Kingdom> kingdomWeights = new WeightedDictionary<Kingdom>();
        for (int i = 0; i < sourceKingdom.discoveredKingdoms.Count; i++) {
            Kingdom otherKingdom = sourceKingdom.discoveredKingdoms[i];
            //int weightForOtherKingdom = GetDefaultWeightForAction(weightedAction, sourceKingdom, otherKingdom);
            int weightForOtherKingdom = 0;
            //loop through all the traits of the current king
            for (int j = 0; j < sourceKingdom.king.allTraits.Count; j++) {
                Trait currTrait = sourceKingdom.king.allTraits[j];
                int modificationFromTrait = currTrait.GetWeightOfActionGivenTarget(weightedAction, otherKingdom, weightForOtherKingdom);
                weightForOtherKingdom += modificationFromTrait;
            }
            ApplyActionModificationForAll(weightedAction, sourceKingdom, otherKingdom, ref weightForOtherKingdom);
            kingdomWeights.AddElement(otherKingdom, weightForOtherKingdom);
        }
        return kingdomWeights;
    }
    internal Dictionary<Kingdom, Dictionary<Kingdom, int>> GetKingdomWeightsForIndirectActionType(Kingdom sourceKingdom, WEIGHTED_ACTION specialWeightedAction) {
        Dictionary<Kingdom, Dictionary<Kingdom, int>> kingdomWeights = new Dictionary<Kingdom, Dictionary<Kingdom, int>>();
        for (int i = 0; i < sourceKingdom.discoveredKingdoms.Count; i++) {
            Kingdom otherKingdom = sourceKingdom.discoveredKingdoms[i]; //the cause of the action
            Dictionary<Kingdom, int> possibleAllies = new Dictionary<Kingdom, int>();
            for (int j = 0; j < otherKingdom.discoveredKingdoms.Count; j++) {
                Kingdom discoveredKingdomOfOtherKingdom = otherKingdom.discoveredKingdoms[j]; //the target of the action
                if (discoveredKingdomOfOtherKingdom.id != sourceKingdom.id) {
                    int weightForOtherKingdom = 0;
                    //loop through all the traits of the current king
                    for (int k = 0; k < sourceKingdom.king.allTraits.Count; k++) {
                        Trait currTrait = sourceKingdom.king.allTraits[k];
                        int modificationFromTrait = currTrait.GetWeightOfActionGivenTargetAndCause(specialWeightedAction, discoveredKingdomOfOtherKingdom, otherKingdom, weightForOtherKingdom);
                        weightForOtherKingdom += modificationFromTrait;
                    }
                    ApplyActionModificationForAll(specialWeightedAction, sourceKingdom, otherKingdom, ref weightForOtherKingdom);
                    possibleAllies.Add(discoveredKingdomOfOtherKingdom, weightForOtherKingdom);
                }
            }
            kingdomWeights.Add(otherKingdom, possibleAllies);
        }
        return kingdomWeights;
    }
    #endregion

    #region Default Weights
    //private int GetDefaultWeightForAction(WEIGHTED_ACTION weightedAction, object source, object target) {
    //    switch (weightedAction) {
    //        case WEIGHTED_ACTION.WAR_OF_CONQUEST:
    //            return 0;
    //        case WEIGHTED_ACTION.ALLIANCE_OF_CONQUEST:
    //            return 0;
    //        case WEIGHTED_ACTION.ALLIANCE_OF_PROTECTION:
    //            return 0;
    //        case WEIGHTED_ACTION.TRADE_DEAL:
    //            return 0;
    //        case WEIGHTED_ACTION.INCITE_UNREST:
    //            return 0;
    //        case WEIGHTED_ACTION.START_INTERNATIONAL_INCIDENT:
    //            return 0;
    //        case WEIGHTED_ACTION.FLATTER:
    //            return 0;
    //        case WEIGHTED_ACTION.SEND_AID:
    //            return 0;
    //        case WEIGHTED_ACTION.LEAVE_TRADE_DEAL:
    //            return 0;
    //        default:
    //            return 0;
    //    }
    //}
    //private int GetTradeDealDefaultWeight(Kingdom sourceKingdom, Kingdom targetKingdom) {
    //    if (sourceKingdom.kingdomsInTradeDealWith.Contains(targetKingdom)) {
    //        return 0;
    //    }
    //    int defaultWeight = 0;
        
    //    KingdomRelationship relWithOtherKingdom = sourceKingdom.GetRelationshipWithKingdom(targetKingdom);
    //    KingdomRelationship relOfOtherWithSource = targetKingdom.GetRelationshipWithKingdom(sourceKingdom);

    //    if (relWithOtherKingdom.sharedRelationship.isAdjacent) {
    //        defaultWeight = 40;
    //        if (relWithOtherKingdom.totalLike > 0) {
    //            defaultWeight += 2 * relWithOtherKingdom.totalLike;//add 2 to Default Weight per Positive Opinion I have towards target
    //        } else if (relWithOtherKingdom.totalLike < 0) {
    //            defaultWeight += 2 * relWithOtherKingdom.totalLike;//subtract 2 to Default Weight per Negative Opinion I have towards target
    //        }

    //        //add 1 to Default Weight per Positive Opinion target has towards me
    //        //subtract 1 to Default Weight per Negative Opinion target has towards me
    //        defaultWeight += relOfOtherWithSource.totalLike;
    //        defaultWeight = Mathf.Max(0, defaultWeight); //minimum 0

    //    }
    //    return defaultWeight;
    //}
    //private int GetInciteUnrestDefaultWeight(Kingdom sourceKingdom, Kingdom targetKingdom) {
    //    int defaultWeight = 0;
    //    KingdomRelationship relWithOtherKingdom = sourceKingdom.GetRelationshipWithKingdom(targetKingdom);
    //    KingdomRelationship relOfOtherWithSource = targetKingdom.GetRelationshipWithKingdom(sourceKingdom);

    //    if (!relWithOtherKingdom.AreAllies()) {
    //        defaultWeight = 40;
    //        if (relWithOtherKingdom.totalLike < 0) {
    //            defaultWeight += relWithOtherKingdom.totalLike;//subtract 2 to Default Weight per Negative Opinion I have towards target
    //        }
    //    }
    //    return defaultWeight;
    //}
    //private int GetInternationalIncidentDefaultWeight(Kingdom sourceKingdom, Kingdom targetKingdom) {
    //    int defaultWeight = 0;
    //    KingdomRelationship relWithOtherKingdom = sourceKingdom.GetRelationshipWithKingdom(targetKingdom);
    //    if (relWithOtherKingdom.totalLike < 0) {
    //        defaultWeight += Mathf.Abs(5 * relWithOtherKingdom.totalLike);
    //    }
    //    return defaultWeight;
    //}
    //private int GetFlatterDefaultWeight(Kingdom sourceKingdom, Kingdom targetKingdom) {
    //    int defaultWeight = 40;
    //    KingdomRelationship relOtherWithSource = targetKingdom.GetRelationshipWithKingdom(sourceKingdom);
    //    if (relOtherWithSource.totalLike < 0) {
    //        defaultWeight += Mathf.Abs(relOtherWithSource.totalLike);
    //    }
    //    return defaultWeight;
    //}
    //private int GetLeaveTradeDealDefaultWeight(Kingdom sourceKingdom, Kingdom targetKingdom) {
    //    int defaultWeight = 0;
    //    if (sourceKingdom.kingdomsInTradeDealWith.Contains(targetKingdom)) {
    //        defaultWeight = 100; //Default Weight to Leave Trade Deal is 100
    //        KingdomRelationship relSourceWithOther = sourceKingdom.GetRelationshipWithKingdom(targetKingdom);
    //        if (relSourceWithOther.targetKingdomThreatLevel > 0) {
    //            defaultWeight += relSourceWithOther.targetKingdomThreatLevel; //add 1 to Default Weight for every Threat of the kingdom
    //        }
    //        if (relSourceWithOther.totalLike < 0) {
    //            defaultWeight += Mathf.Abs(2 * relSourceWithOther.totalLike); //add 2 to Default Weight for every negative Opinion I have towards the king
    //        } else if (relSourceWithOther.totalLike > 0) {
    //            defaultWeight -= 2 * relSourceWithOther.totalLike; //subtract 2 to Default Weight for every positive Opinion I have towards the king
    //        }

    //        //add Default Weight if Kingdom no longer benefits from any Surplus of the trade partner, otherwise, add its Default Weight to Not Leave Any Trade Deal
    //    }
    //    return defaultWeight;
    //}
    #endregion

    #region All Modifications
    private void ApplyActionModificationForAll(WEIGHTED_ACTION weightedAction, object source, object target, ref int defaultWeight) {
        switch (weightedAction) {
            //case WEIGHTED_ACTION.WAR_OF_CONQUEST:
            //    GetAllModificationForWarOfConquest((Kingdom)source, (Kingdom)target, ref defaultWeight);
            //    break;
            case WEIGHTED_ACTION.ALLIANCE_OF_PROTECTION:
                GetAllModificationForAllianceOfProtection((Kingdom)source, (Kingdom)target, ref defaultWeight);
                break;
            case WEIGHTED_ACTION.TRADE_DEAL:
                GetAllModificationForTradeDeal((Kingdom)source, (Kingdom)target, ref defaultWeight);
                break;
            case WEIGHTED_ACTION.FLATTER:
                GetAllModificationForFlatter((Kingdom)source, (Kingdom)target, ref defaultWeight);
                break;
            case WEIGHTED_ACTION.INCITE_UNREST:
                GetAllModificationForInciteUnrest((Kingdom)source, (Kingdom)target, ref defaultWeight);
                break;
            case WEIGHTED_ACTION.START_INTERNATIONAL_INCIDENT:
                GetAllModificationForInternationalIncident((Kingdom)source, (Kingdom)target, ref defaultWeight);
                break;
        }
    }
    //private void GetAllModificationForWarOfConquest(Kingdom sourceKingdom, Kingdom targetKingdom, ref int defaultWeight) {
    //    KingdomRelationship relWithTargetKingdom = sourceKingdom.GetRelationshipWithKingdom(targetKingdom);
    //    List<Kingdom> alliesAtWarWith = relWithTargetKingdom.GetAlliesTargetKingdomIsAtWarWith();
    //    //for each non-ally adjacent kingdoms that one of my allies declared war with recently
    //    if (relWithTargetKingdom.sharedRelationship.isAdjacent && !relWithTargetKingdom.AreAllies() && alliesAtWarWith.Count > 0) {
    //        //compare its theoretical power vs my theoretical power
    //        int sourceKingdomPower = relWithTargetKingdom._theoreticalPower;
    //        int otherKingdomPower = targetKingdom.GetRelationshipWithKingdom(sourceKingdom)._theoreticalPower;
    //        if (otherKingdomPower * 1.25f < sourceKingdomPower) {
    //            //If his theoretical power is not higher than 25% over mine
    //            defaultWeight = 20;
    //            for (int j = 0; j < alliesAtWarWith.Count; j++) {
    //                Kingdom currAlly = alliesAtWarWith[j];
    //                KingdomRelationship relationshipWithAlly = sourceKingdom.GetRelationshipWithKingdom(currAlly);
    //                if (relationshipWithAlly.totalLike > 0) {
    //                    defaultWeight += 2 * relationshipWithAlly.totalLike; //add 2 weight per positive opinion i have over my ally
    //                } else if (relationshipWithAlly.totalLike < 0) {
    //                    defaultWeight += relationshipWithAlly.totalLike; //subtract 1 weight per negative opinion i have over my ally (totalLike is negative)
    //                }
    //            }
    //            //add 1 weight per negative opinion i have over the target
    //            //subtract 1 weight per positive opinion i have over the target
    //            defaultWeight += (relWithTargetKingdom.totalLike * -1); //If totalLike is negative it becomes positive(+), otherwise it becomes negative(-)
    //            defaultWeight = Mathf.Max(0, defaultWeight);
    //        }
    //    }
    //}
    private void GetAllModificationForAllianceOfProtection(Kingdom sourceKingdom, Kingdom targetKingdom, ref int defaultWeight) {
        if (sourceKingdom.IsThreatened()) {
            //loop through known Kingdoms i am not at war with and whose Opinion of me is positive
            KingdomRelationship relWithOtherKingdom = sourceKingdom.GetRelationshipWithKingdom(targetKingdom);
            KingdomRelationship relOfOtherWithSource = targetKingdom.GetRelationshipWithKingdom(sourceKingdom);
            if (!relOfOtherWithSource.sharedRelationship.isAtWar && relOfOtherWithSource.totalLike > 0) {
                if (relOfOtherWithSource.totalLike > 0) {
                    defaultWeight += 3 * relOfOtherWithSource.totalLike;//add 3 Weight for every positive Opinion it has towards me
                }else if (relOfOtherWithSource.totalLike < 0) {
                    defaultWeight += relWithOtherKingdom.totalLike;//subtract 1 Weight for every negative Opinion I have towards it
                }
                if (sourceKingdom.recentlyRejectedOffers.ContainsKey(targetKingdom)) {
                    defaultWeight -= 50;
                } else if (sourceKingdom.recentlyBrokenAlliancesWith.Contains(targetKingdom)) {
                    defaultWeight -= 50;
                }
                defaultWeight = Mathf.Max(0, defaultWeight); //minimum 0
            }
        }
    }
    private void GetAllModificationForTradeDeal(Kingdom sourceKingdom, Kingdom targetKingdom, ref int defaultWeight) {
        if (sourceKingdom.kingdomsInTradeDealWith.Contains(targetKingdom)) {
            return;
        }

        int weightAdjustment = 0;
        KingdomRelationship relWithOtherKingdom = sourceKingdom.GetRelationshipWithKingdom(targetKingdom);
        KingdomRelationship relOfOtherWithSource = targetKingdom.GetRelationshipWithKingdom(sourceKingdom);

        if (relWithOtherKingdom.sharedRelationship.isAdjacent) {
            weightAdjustment = 40;
            if (relWithOtherKingdom.totalLike > 0) {
                weightAdjustment += 2 * relWithOtherKingdom.totalLike;//add 2 to Default Weight per Positive Opinion I have towards target
            } else if (relWithOtherKingdom.totalLike < 0) {
                weightAdjustment += 2 * relWithOtherKingdom.totalLike;//subtract 2 to Default Weight per Negative Opinion I have towards target
            }

            //add 1 to Default Weight per Positive Opinion target has towards me
            //subtract 1 to Default Weight per Negative Opinion target has towards me
            weightAdjustment += relOfOtherWithSource.totalLike;
            weightAdjustment = Mathf.Max(0, weightAdjustment); //minimum 0

        }

        Dictionary<RESOURCE_TYPE, int> deficitOfTargetKingdom = targetKingdom.GetDeficitResourcesFor(sourceKingdom);
        Dictionary<RESOURCE_TYPE, int> surplusOfThisKingdom = sourceKingdom.GetSurplusResourcesFor(targetKingdom);
        foreach (KeyValuePair<RESOURCE_TYPE, int> kvp in surplusOfThisKingdom) {
            RESOURCE_TYPE currSurplus = kvp.Key;
            int surplusAmount = kvp.Value;
            if (deficitOfTargetKingdom.ContainsKey(currSurplus)) {
                //otherKingdom has a deficit for currSurplus
                //add Default Weight for every point of Surplus they have on our Deficit Resources 
                int deficitAmount = deficitOfTargetKingdom[currSurplus];
                int modifier = 0;
                if(surplusAmount >= deficitAmount) {
                    modifier = deficitAmount;
                } else {
                    modifier = surplusAmount;
                }
                defaultWeight += (weightAdjustment * modifier);
            }
        }
    }
    private void GetAllModificationForFlatter(Kingdom sourceKingdom, Kingdom targetKingdom, ref int defaultWeight) {
        int weightModification = defaultWeight + 40; //Default Weight is 40
        defaultWeight = 0;
        KingdomRelationship relOtherWithSource = targetKingdom.GetRelationshipWithKingdom(sourceKingdom);
        if (relOtherWithSource.totalLike < 0) {
            defaultWeight += weightModification * Mathf.Abs(relOtherWithSource.totalLike);
        }
    }
    private void GetAllModificationForInciteUnrest(Kingdom sourceKingdom, Kingdom targetKingdom, ref int defaultWeight) {
        int weightModification = defaultWeight + 40;
        defaultWeight = 0;
        KingdomRelationship relWithOtherKingdom = sourceKingdom.GetRelationshipWithKingdom(targetKingdom);
        KingdomRelationship relOfOtherWithSource = targetKingdom.GetRelationshipWithKingdom(sourceKingdom);

        if (sourceKingdom.king.HasTrait(TRAIT.DECEITFUL)) {
            if (relWithOtherKingdom.AreAllies()) {
                if (relWithOtherKingdom.totalLike < 0) {
                    defaultWeight += weightModification * Mathf.Abs(relWithOtherKingdom.totalLike);//add Default Weight per Negative Opinion I have towards target
                }
            }
        }

        if (!relWithOtherKingdom.AreAllies()) {
            if (relWithOtherKingdom.totalLike < 0) {
                defaultWeight += Mathf.Abs(weightModification * relWithOtherKingdom.totalLike);//add Default Weight per Negative Opinion I have towards target
            }
        }
        
    }
    private void GetAllModificationForInternationalIncident(Kingdom sourceKingdom, Kingdom targetKingdom, ref int defaultWeight) {
        int weightModification = defaultWeight;
        defaultWeight = 0;
        KingdomRelationship relWithOtherKingdom = sourceKingdom.GetRelationshipWithKingdom(targetKingdom);
        if (relWithOtherKingdom.totalLike < 0) {
            defaultWeight += Mathf.Abs(5 * relWithOtherKingdom.totalLike);
        }
    }

    private void ApplySpecialActionModificationForAll(WEIGHTED_ACTION weightedAction, object source, object target, ref int defaultWeight, ref int weightNotToDoAction) {
        switch (weightedAction) {
            //case WEIGHTED_ACTION.DECLARE_PEACE:
            //    GetAllModificationForDeclarePeace((Kingdom)source, (Warfare)target, ref defaultWeight, ref weightNotToDoAction);
            //    break;
            case WEIGHTED_ACTION.LEAVE_ALLIANCE:
                GetAllModificationForLeaveAlliance((Kingdom)source, (AlliancePool)target, ref defaultWeight, ref weightNotToDoAction);
                break;
            case WEIGHTED_ACTION.LEAVE_TRADE_DEAL:
                GetAllModificationForLeaveTradeDeal((Kingdom)source, (Kingdom)target, ref defaultWeight, ref weightNotToDoAction);
                break;
        }
    }
    //private void GetAllModificationForDeclarePeace(Kingdom sourceKingdom, Warfare targetWar, ref int defaultWeight, ref int weightNotToDoAction) {
    //    WAR_SIDE sourceSide = targetWar.GetSideOfKingdom(sourceKingdom);
    //    WAR_SIDE otherSide = WAR_SIDE.A;
    //    if(sourceSide == WAR_SIDE.A) {
    //        otherSide = WAR_SIDE.B;
    //    }
    //    List<Kingdom> enemyKingdoms = targetWar.GetListFromSide(otherSide);
    //    for (int i = 0; i < enemyKingdoms.Count; i++) {
    //        Kingdom enemyKingdom = enemyKingdoms[i];
    //        KingdomRelationship sourceRelWithEnemy = sourceKingdom.GetRelationshipWithKingdom(enemyKingdom);
    //        KingdomRelationship enemyRelWithSource = enemyKingdom.GetRelationshipWithKingdom(sourceKingdom);
    //        //add 2 to Weight to Declare Peace for every Relative Strength the enemy kingdoms have over me
    //        if(enemyRelWithSource.relativeStrength > 0) {
    //            defaultWeight += 2 * enemyRelWithSource.relativeStrength;
    //        }
    //        //add 2 to Weight to Don't Declare Peace for every Relative Strength I have over each enemy kingdom
    //        if (sourceRelWithEnemy.relativeStrength > 0) {
    //            weightNotToDoAction += 2 * sourceRelWithEnemy.relativeStrength;
    //        }
    //        //add 3 Weight to Declare Peace for each War Weariness I have
    //        weightNotToDoAction += 3 * targetWar.kingdomSideWeariness[sourceKingdom.id].weariness;
    //    }
    //}
    private void GetAllModificationForLeaveAlliance(Kingdom sourceKingdom, AlliancePool alliance, ref int defaultWeight, ref int weightNotToDoAction) {
        //loop through the other kingdoms within the alliance
        for (int i = 0; i < alliance.kingdomsInvolved.Count; i++) {
            Kingdom ally = alliance.kingdomsInvolved[i];
            if (ally.id != sourceKingdom.id) {
                KingdomRelationship relWithAlly = sourceKingdom.GetRelationshipWithKingdom(ally);
                if(relWithAlly.targetKingdomThreatLevel > 0) {
                    defaultWeight += relWithAlly.targetKingdomThreatLevel; //add 1 weight to leave alliance for every threat of ally kingdom
                }
                if (relWithAlly.totalLike < 0) {
                    defaultWeight +=  Mathf.Abs(3 * relWithAlly.totalLike); //add 3 weight to leave alliance for every negative opinion I have towards the king
                } else if (relWithAlly.totalLike > 0) {
                    weightNotToDoAction += 2 * relWithAlly.totalLike; //add 2 weight to keep alliance for every positive opinion I have towards the king
                }
            }
        }

        for (int i = 0; i < sourceKingdom.adjacentKingdoms.Count; i++) {
            Kingdom otherKingdom = sourceKingdom.adjacentKingdoms[i];
            if (!alliance.kingdomsInvolved.Contains(otherKingdom)) {
                //loop through non-ally adjacent kingdoms
                KingdomRelationship relWithOther = sourceKingdom.GetRelationshipWithKingdom(otherKingdom);
                if(relWithOther.targetKingdomThreatLevel > 0) {
                    weightNotToDoAction += relWithOther.targetKingdomThreatLevel; //add 1 weight to keep alliance for every threat of the kingdom
                }
            }
        }
    }
    private void GetAllModificationForLeaveTradeDeal(Kingdom sourceKingdom, Kingdom targetKingdom, ref int defaultWeight, ref int weightNotToDoAction) {
        int weightAdjustment = 100;
        KingdomRelationship relSourceWithOther = sourceKingdom.GetRelationshipWithKingdom(targetKingdom);
        if (relSourceWithOther.targetKingdomThreatLevel > 0) {
            weightAdjustment += relSourceWithOther.targetKingdomThreatLevel; //add 1 to Default Weight for every Threat of the kingdom
        }
        if (relSourceWithOther.totalLike < 0) {
            weightAdjustment += Mathf.Abs(2 * relSourceWithOther.totalLike); //add 2 to Default Weight for every negative Opinion I have towards the king
        } else if (relSourceWithOther.totalLike > 0) {
            weightAdjustment -= 2 * relSourceWithOther.totalLike; //subtract 2 to Default Weight for every positive Opinion I have towards the king
        }

        //add Default Weight if Kingdom no longer benefits from any Surplus of the trade partner, otherwise, add its Default Weight to Not Leave Any Trade Deal
        if (sourceKingdom.IsTradeDealStillNeeded(targetKingdom)) {
            weightNotToDoAction += weightAdjustment;
        } else {
            defaultWeight += weightAdjustment;
        }
    }
    #endregion

    #region Action Requirements
    internal bool ActionMeetsRequirements(Kingdom sourceKingdom, WEIGHTED_ACTION actionType) {
        if (weightedActionRequirements.ContainsKey(actionType)) {
            List<WEIGHTED_ACTION_REQS> requirements = weightedActionRequirements[actionType];
            for (int i = 0; i < requirements.Count; i++) {
                WEIGHTED_ACTION_REQS currRequirement = requirements[i];
                switch (currRequirement) {
                    case WEIGHTED_ACTION_REQS.NO_ALLIANCE:
                        if(sourceKingdom.alliancePool != null) {
                            return false;
                        }
                        break;
                    case WEIGHTED_ACTION_REQS.HAS_ALLIANCE:
                        if (sourceKingdom.alliancePool == null) {
                            return false;
                        }
                        break;
                    case WEIGHTED_ACTION_REQS.HAS_WAR:
                        if(sourceKingdom.GetWarCount() <= 0) {
                            return false;
                        }
                        break;
                    case WEIGHTED_ACTION_REQS.HAS_ACTIVE_TRADE_DEAL:
                        if(sourceKingdom.kingdomsInTradeDealWith.Count <= 0) {
                            return false;
                        }
                        break;
                    default:
                        return true;
                }
            }
        }
        return true;
    }
    internal bool ActionMeetsRequirementsForTrait(Kingdom sourceKingdom, WEIGHTED_ACTION actionType, TRAIT traitType) {
        if (weightedActionRequirementsForTraits.ContainsKey(traitType)) {
            if (weightedActionRequirementsForTraits[traitType].ContainsKey(actionType)) {
                List<WEIGHTED_ACTION_REQS> requirements = weightedActionRequirementsForTraits[traitType][actionType];
                for (int i = 0; i < requirements.Count; i++) {
                    WEIGHTED_ACTION_REQS currRequirement = requirements[i];
                    switch (currRequirement) {
                        case WEIGHTED_ACTION_REQS.NO_ALLIANCE:
                            if (sourceKingdom.alliancePool != null) {
                                return false;
                            }
                            break;
                        case WEIGHTED_ACTION_REQS.HAS_ALLIANCE:
                            if (sourceKingdom.alliancePool == null) {
                                return false;
                            }
                            break;
                        case WEIGHTED_ACTION_REQS.HAS_WAR:
                            if (sourceKingdom.GetWarCount() <= 0) {
                                return false;
                            }
                            break;
                        case WEIGHTED_ACTION_REQS.HAS_ACTIVE_TRADE_DEAL:
                            if (sourceKingdom.kingdomsInTradeDealWith.Count <= 0) {
                                return false;
                            }
                            break;
                        default:
                            return true;
                    }
                }
            }
        }
        return true;
    }
    #endregion

}