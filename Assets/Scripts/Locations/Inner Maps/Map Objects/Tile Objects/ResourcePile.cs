﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;

public abstract class ResourcePile : TileObject {

	public RESOURCE providedResource { get; protected set; }
    public int resourceInPile { get { return storedResources[providedResource]; } }

    public ResourcePile(RESOURCE providedResource) {
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.TAKE_RESOURCE, INTERACTION_TYPE.CARRY, INTERACTION_TYPE.DEPOSIT_RESOURCE_PILE, INTERACTION_TYPE.DROP, INTERACTION_TYPE.DESTROY_RESOURCE_AMOUNT };
        this.providedResource = providedResource;
    }

    #region Virtuals
    public virtual void SetResourceInPile(int amount) {
        SetResource(providedResource, amount);
        if(resourceInPile <= 0 && gridTileLocation != null && isBeingCarriedBy == null) {
            gridTileLocation.structure.RemovePOI(this);
        }
    }
    public virtual void AdjustResourceInPile(int adjustment) {
        AdjustResource(providedResource, adjustment);
        Messenger.Broadcast(Signals.RESOURCE_IN_PILE_CHANGED, this);
        if (resourceInPile <= 0) {
            if(gridTileLocation != null && isBeingCarriedBy == null) {
                gridTileLocation.structure.RemovePOI(this);
            } else if (isBeingCarriedBy != null) {
                //If amount in pile was reduced to zero and is still being carried, remove from being carried and destroy it
                isBeingCarriedBy.UncarryPOI(this, addToLocation: false);
            }
        }
    }
    public virtual bool HasResource() {
        return resourceInPile > 0;
    }
    protected override void ConstructMaxResources() {
        maxResourceValues = new Dictionary<RESOURCE, int>();
        RESOURCE[] resourceTypes = CollectionUtilities.GetEnumValues<RESOURCE>();
        for (int i = 0; i < resourceTypes.Length; i++) {
            RESOURCE resourceType = resourceTypes[i];
            //only allow resource type of what this resource pile provides.
            maxResourceValues.Add(resourceType, resourceType == providedResource ? 1000 : 0);
        }
    }
    #endregion

    #region Overrides
    public override void OnPlacePOI() {
        base.OnPlacePOI();
        // Messenger.AddListener<Region>(Signals.REGION_CHANGE_STORAGE, OnRegionChangeStorage);
    }
    public override void OnDestroyPOI() {
        base.OnDestroyPOI();
        // Messenger.RemoveListener<Region>(Signals.REGION_CHANGE_STORAGE, OnRegionChangeStorage);
        // Messenger.Broadcast(Signals.CHECK_JOB_APPLICABILITY, JOB_TYPE.HAUL, this as IPointOfInterest);
        Messenger.Broadcast(Signals.CHECK_JOB_APPLICABILITY, JOB_TYPE.DESTROY, this as IPointOfInterest);
    }
    private INTERACTION_TYPE[] storedActions;
    protected override void OnMapObjectStateChanged() {
        if (mapObjectState == MAP_OBJECT_STATE.UNBUILT) {
            mapVisual.SetVisualAlpha(0f / 255f);
            SetSlotAlpha(0f / 255f);
            //store advertised actions
            storedActions = new INTERACTION_TYPE[advertisedActions.Count];
            for (int i = 0; i < advertisedActions.Count; i++) {
                storedActions[i] = advertisedActions[i];
            }
            advertisedActions.Clear();
            AddAdvertisedAction(INTERACTION_TYPE.DEPOSIT_RESOURCE_PILE);
            UnsubscribeListeners();
        } else if (mapObjectState == MAP_OBJECT_STATE.BUILDING) {
            mapVisual.SetVisualAlpha(128f / 255f);
            SetSlotAlpha(128f / 255f);
        } else {
            mapVisual.SetVisualAlpha(255f / 255f);
            SetSlotAlpha(255f / 255f);
            RemoveAdvertisedAction(INTERACTION_TYPE.DEPOSIT_RESOURCE_PILE);
            for (int i = 0; i < storedActions.Length; i++) {
                AddAdvertisedAction(storedActions[i]);
            }
            storedActions = null;
            SubscribeListeners();
        }
    }
    #endregion

    protected bool IsDepositResourcePileStillApplicable() {
        return gridTileLocation != null && gridTileLocation.structure != gridTileLocation.structure.location.mainStorage;
    }
    // protected void OnRegionChangeStorage(Region regionOfMainStorage) {
    //     if(gridTileLocation != null && structureLocation.location.IsSameCoreLocationAs(regionOfMainStorage) && structureLocation != structureLocation.location.mainStorage) {
    //         CreateHaulJob();
    //     }
    // }
    // protected void CreateHaulJob() {
    //     if (!structureLocation.settlementLocation.HasJob(JOB_TYPE.HAUL, this)) {
    //         ResourcePile chosenPileToBeDeposited = structureLocation.settlementLocation.mainStorage.GetResourcePileObjectWithLowestCount(tileObjectType);
    //         GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.HAUL, new GoapEffect(GOAP_EFFECT_CONDITION.DEPOSIT_RESOURCE, string.Empty, false, GOAP_EFFECT_TARGET.TARGET), this, structureLocation.settlementLocation);
    //         if (chosenPileToBeDeposited != null) {
    //             job.AddOtherData(INTERACTION_TYPE.DEPOSIT_RESOURCE_PILE, new object[] { chosenPileToBeDeposited });
    //         }
    //         job.SetStillApplicableChecker(IsDepositResourcePileStillApplicable);
    //         job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanDoObtainSupplyJob);
    //         structureLocation.settlementLocation.AddToAvailableJobs(job);
    //     }
    // }
    protected void ForceCancelNotAssignedProduceJob(JOB_TYPE jobType) {
        JobQueueItem job = structureLocation.settlementLocation.GetJob(jobType);
        if (job != null && job.assignedCharacter == null) {
            structureLocation.settlementLocation.ForceCancelJob(job);
        }
    }
}
