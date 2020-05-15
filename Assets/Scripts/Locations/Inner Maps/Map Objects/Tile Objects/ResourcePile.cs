﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;

public abstract class ResourcePile : TileObject {

	public RESOURCE providedResource { get; protected set; }
    public int resourceInPile { get { return storedResources[providedResource]; } }

    public ResourcePile(RESOURCE providedResource) {
        AddAdvertisedAction(INTERACTION_TYPE.TAKE_RESOURCE);
        AddAdvertisedAction(INTERACTION_TYPE.PICK_UP);
        AddAdvertisedAction(INTERACTION_TYPE.DEPOSIT_RESOURCE_PILE);
        AddAdvertisedAction(INTERACTION_TYPE.DROP);
        AddAdvertisedAction(INTERACTION_TYPE.DESTROY_RESOURCE_AMOUNT);
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
        AddAdvertisedAction(INTERACTION_TYPE.BOOBY_TRAP);
        //advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.TAKE_RESOURCE, INTERACTION_TYPE.PICK_UP, INTERACTION_TYPE.DEPOSIT_RESOURCE_PILE, INTERACTION_TYPE.DROP, INTERACTION_TYPE.DESTROY_RESOURCE_AMOUNT/*, INTERACTION_TYPE.DROP_RESOURCE*/ };
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
    // public override void OnPlacePOI() {
    //     base.OnPlacePOI();
    //     // Messenger.AddListener<Region>(Signals.REGION_CHANGE_STORAGE, OnRegionChangeStorage);
    // }
    public override void OnDestroyPOI() {
        base.OnDestroyPOI();
        // Messenger.RemoveListener<Region>(Signals.REGION_CHANGE_STORAGE, OnRegionChangeStorage);
        // Messenger.Broadcast(Signals.CHECK_JOB_APPLICABILITY, JOB_TYPE.HAUL, this as IPointOfInterest);
        Messenger.Broadcast(Signals.CHECK_JOB_APPLICABILITY, JOB_TYPE.DESTROY, this as IPointOfInterest);
    }
    private INTERACTION_TYPE[] storedActions;
    protected override void OnMapObjectStateChanged() {
        if (mapVisual == null) { return; }
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
            if (_unbuiltObjectValidityChecker != null) {
                Messenger.AddListener(Signals.CHECK_UNBUILT_OBJECT_VALIDITY, CheckUnbuiltObjectValidity);
            }
        } else if (mapObjectState == MAP_OBJECT_STATE.BUILDING) {
            mapVisual.SetVisualAlpha(128f / 255f);
            SetSlotAlpha(128f / 255f);
            _unbuiltObjectValidityChecker = null;
            Messenger.RemoveListener(Signals.CHECK_UNBUILT_OBJECT_VALIDITY, CheckUnbuiltObjectValidity);
        } else {
            _unbuiltObjectValidityChecker = null;
            Messenger.RemoveListener(Signals.CHECK_UNBUILT_OBJECT_VALIDITY, CheckUnbuiltObjectValidity);
            mapVisual.SetVisualAlpha(255f / 255f);
            SetSlotAlpha(255f / 255f);
            // RemoveAdvertisedAction(INTERACTION_TYPE.DEPOSIT_RESOURCE_PILE);
            if (storedActions != null) {
                for (int i = 0; i < storedActions.Length; i++) {
                    AddAdvertisedAction(storedActions[i]);
                }    
            }
            storedActions = null;
            SubscribeListeners();
        }
    }
    public override string GetAdditionalTestingData() {
        string data = base.GetAdditionalTestingData();
        data = $"{data}\n\tResource in Pile: {resourceInPile.ToString()}";
        return data;
    }
    #endregion
}
