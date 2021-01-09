﻿using System.Collections.Generic;
using System.Collections;
using Locations;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using System;

namespace UtilityScripts {
    public static class LocationAwarenessUtility {
        public static List<ILocationAwareness> allLocationsToBeUpdated = new List<ILocationAwareness>();

        /*
         * this function will add the awareness to pending awareness list
         * */
        public static void AddToAwarenessList(IPointOfInterest poi, LocationGridTile gridTileLocation) {
            ILocationAwareness locationAwareness = null;
            if (gridTileLocation.structure.structureType != STRUCTURE_TYPE.WILDERNESS && gridTileLocation.structure.structureType != STRUCTURE_TYPE.OCEAN) {
                locationAwareness = gridTileLocation.structure.locationAwareness;
            } else if (gridTileLocation.collectionOwner != null && gridTileLocation.collectionOwner.isPartOfParentRegionMap) {
                locationAwareness = gridTileLocation.collectionOwner.partOfHextile.hexTileOwner.locationAwareness;
            }
            if (locationAwareness != null) {
                locationAwareness.RemoveAwarenessFromPendingRemoveList(poi);
                if (poi.currentLocationAwareness != locationAwareness) {
                    //Only add awareness to pending if the current location awareness of the poi is not this one, if it is already this one, do not add it anymore
                    locationAwareness.AddAwarenessToPendingAddList(poi);
                }
                AddOrRemoveFromLocationsToBeUpdated(locationAwareness);
            }
        }
        public static void AddToAwarenessList(INTERACTION_TYPE actionType, IPointOfInterest targetAwareness) {
            ILocationAwareness awareness = targetAwareness.currentLocationAwareness;
            if (awareness != null) {
                awareness.AddSpecificAwarenessToPendingAddList(actionType, targetAwareness);
                AddLocationAwarenessToBeUpdated(awareness);
            }
        }

        public static void RemoveFromAwarenessList(IPointOfInterest poi) {
            ILocationAwareness awareness = poi.currentLocationAwareness;
            if (awareness != null) {
                awareness.RemoveAwarenessFromPendingAddList(poi);
                awareness.AddAwarenessToPendingRemoveList(poi);
                AddOrRemoveFromLocationsToBeUpdated(awareness);
            } else {
                LocationGridTile gridTileLocation = poi.gridTileLocation;
                if (gridTileLocation != null) {
                    ILocationAwareness locationAwareness = null;
                    if (gridTileLocation.structure.structureType != STRUCTURE_TYPE.WILDERNESS && gridTileLocation.structure.structureType != STRUCTURE_TYPE.OCEAN) {
                        locationAwareness = gridTileLocation.structure.locationAwareness;
                    } else if (gridTileLocation.collectionOwner != null && gridTileLocation.collectionOwner.isPartOfParentRegionMap) {
                        locationAwareness = gridTileLocation.collectionOwner.partOfHextile.hexTileOwner.locationAwareness;
                    }
                    if (locationAwareness != null) {
                        locationAwareness.RemoveAwarenessFromPendingAddList(poi);
                        //If the poi has no current location awareness then it means that it does not have a grid tile location with structure or hex
                        //Or poi has not been added to the main list yet
                        //That is why we only remove from pendingAddAwareness list but we do not add to pendingRemoveAwareness list
                        AddOrRemoveFromLocationsToBeUpdated(locationAwareness);
                    }
                }
            }
        }
        public static void RemoveFromAwarenessList(INTERACTION_TYPE actionType, IPointOfInterest targetAwareness) {
            ILocationAwareness awareness = targetAwareness.currentLocationAwareness;
            if (awareness != null) {
                awareness.AddSpecificAwarenessToPendingRemoveList(actionType, targetAwareness);
                AddLocationAwarenessToBeUpdated(awareness);
            }
        }

        private static void AddLocationAwarenessToBeUpdated(ILocationAwareness locationAwareness) {
            if (!locationAwareness.flaggedForUpdate) {
                allLocationsToBeUpdated.Add(locationAwareness);
                locationAwareness.SetFlaggedForUpdate(true);
            }
        }
        private static void RemoveLocationAwarenessToBeUpdated(ILocationAwareness locationAwareness) {
            if (locationAwareness.flaggedForUpdate) {
                allLocationsToBeUpdated.Remove(locationAwareness);
                locationAwareness.SetFlaggedForUpdate(false);
            }
        }
        private static void AddOrRemoveFromLocationsToBeUpdated(ILocationAwareness locationAwareness) {
            if (locationAwareness.HasPendingAddOrRemoveAwareness()) {
                AddLocationAwarenessToBeUpdated(locationAwareness);
            } else {
                RemoveLocationAwarenessToBeUpdated(locationAwareness);
            }
        }
        public static void UpdateAllPendingAwareness() {
            for (int i = 0; i < allLocationsToBeUpdated.Count; i++) {
                allLocationsToBeUpdated[i].UpdateAwareness();
            }
            allLocationsToBeUpdated.Clear();
        }

        public static IEnumerator UpdateAllPendingAwarenessThread() {
            for (int i = 0; i < allLocationsToBeUpdated.Count; i++) {
                allLocationsToBeUpdated[i].UpdateAwareness();
                yield return null;
            }
            allLocationsToBeUpdated.Clear();
        }
    }
}