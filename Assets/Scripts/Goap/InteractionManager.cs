﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;
using Inner_Maps;
using Interrupts;

public partial class InteractionManager : MonoBehaviour {
    public static InteractionManager Instance = null;

    public const string Goap_State_Success = "Success";
    public const string Goap_State_Fail = "Fail";

    public static readonly int Character_Action_Delay = 5;

    private string dailyInteractionSummary;
    public Dictionary<INTERACTION_TYPE, GoapAction> goapActionData { get; private set; }
    public Dictionary<POINT_OF_INTEREST_TYPE, List<GoapAction>> allGoapActionAdvertisements { get; private set; }
    public Dictionary<INTERRUPT, Interrupt> interruptData { get; private set; }


    private void Awake() {
        Instance = this;
    }
    public void Initialize() {
        ConstructGoapActionData();
        ConstructAllGoapActionAdvertisements();
        ConstructInterruptData();
    }

    private void ConstructAllGoapActionAdvertisements() {
        POINT_OF_INTEREST_TYPE[] poiTypes = Utilities.GetEnumValues<POINT_OF_INTEREST_TYPE>();
        allGoapActionAdvertisements = new Dictionary<POINT_OF_INTEREST_TYPE, List<GoapAction>>();
        for (int i = 0; i < poiTypes.Length; i++) {
            POINT_OF_INTEREST_TYPE currType = poiTypes[i];
            allGoapActionAdvertisements.Add(currType, new List<GoapAction>());
        }
        for (int i = 0; i < goapActionData.Values.Count; i++) {
            GoapAction currAction = goapActionData.Values.ElementAt(i);
            for (int j = 0; j < currAction.advertisedBy.Length; j++) {
                POINT_OF_INTEREST_TYPE currType = currAction.advertisedBy[j];
                allGoapActionAdvertisements[currType].Add(currAction);
            }
        }
    }
    private void ConstructGoapActionData() {
        goapActionData = new Dictionary<INTERACTION_TYPE, GoapAction>();
        INTERACTION_TYPE[] allGoapActions = Utilities.GetEnumValues<INTERACTION_TYPE>();
        for (int i = 0; i < allGoapActions.Length; i++) {
            INTERACTION_TYPE currType = allGoapActions[i];
            var typeName = Utilities.NormalizeStringUpperCaseFirstLettersNoSpace(currType.ToString());
            System.Type type = System.Type.GetType(typeName);
            if (type != null) {
                GoapAction data = System.Activator.CreateInstance(type) as GoapAction;
                goapActionData.Add(currType, data);
            } else {
                Debug.LogWarning(currType.ToString() + " has no data!");
            }
        }
    }
    private void ConstructInterruptData() {
        interruptData = new Dictionary<INTERRUPT, Interrupt>();
        INTERRUPT[] allInterrupts = Utilities.GetEnumValues<INTERRUPT>();
        for (int i = 0; i < allInterrupts.Length; i++) {
            INTERRUPT interrupt = allInterrupts[i];
            var typeName = $"Interrupts.{ Utilities.NotNormalizedConversionEnumToStringNoSpaces(interrupt.ToString()) }"; ;
            System.Type type = System.Type.GetType(typeName);
            if (type != null) {
                Interrupt data = System.Activator.CreateInstance(type) as Interrupt;
                interruptData.Add(interrupt, data);
            } else {
                Debug.LogWarning(typeName + " has no data!");
            }
        }
    }
    public Interrupt GetInterruptData(INTERRUPT interrupt) {
        if (interruptData.ContainsKey(interrupt)) {
            return interruptData[interrupt];
        }
        return null;
    }

    public bool CanSatisfyGoapActionRequirements(INTERACTION_TYPE goapType, Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (goapActionData.ContainsKey(goapType)) {
            return goapActionData[goapType].CanSatisfyRequirements(actor, poiTarget, otherData);
        }
        throw new Exception("No Goap Action Data for " + goapType.ToString());
    }

    #region Intel
    //public Intel CreateNewIntel(params object[] obj) {
    //    if (obj[0] is GoapAction) {
    //        GoapAction action = obj[0] as GoapAction;
    //        switch (action.goapType) {
    //            case INTERACTION_TYPE.POISON:
    //                return new PoisonTableIntel(obj[1] as Character, obj[0] as GoapAction);
    //            default:
    //                return new EventIntel(obj[1] as Character, obj[0] as GoapAction);
    //        }

            
    //    }
    //    return null;
    //}
    
    //TODO: Object pool this
    public Intel CreateNewIntel(ActualGoapNode node) {
        return new Intel(node);
    }
    #endregion

    #region Goap Action Utilities
    private bool CanRegionAdvertiseActionTo(Region region, Character actor, INTERACTION_TYPE interactionType) {
        GoapAction action = goapActionData[interactionType];
        return action.CanSatisfyRequirements(actor, region.regionTileObject, null);
    }
    public Region GetRandomRegionTarget(Character actor, INTERACTION_TYPE interactionType) {
        List<Region> choices = new List<Region>();
        for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
            Region currRegion = GridMap.Instance.allRegions[i];
            if (CanRegionAdvertiseActionTo(currRegion, actor, interactionType)) {
                choices.Add(currRegion);
            }
        }
        if (choices.Count > 0) {
            return Utilities.GetRandomElement(choices);    
        }
        Debug.LogWarning($"{actor.name} cannot find a region to target with action {interactionType.ToString()}");
        return null;
    }
    #endregion

    #region Precondition Resolvers
    public bool TargetHasNegativeTraitEffect(Character actor, IPointOfInterest target) {
        return target.traitContainer.HasTraitOf(TRAIT_EFFECT.NEGATIVE);
    }
    #endregion
}