﻿using System;
using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Pathfinding;
using UnityEngine;
using Traits;

public class CharacterMarkerVisionCollider : BaseVisionCollider {

    public CharacterMarker parentMarker;
    private bool isApplicationQuitting = false;
    
    private void OnDisable() {
        if (isApplicationQuitting) { return; }
        if (LevelLoaderManager.Instance.isLoadingNewScene) { return; }
        if (SchedulingManager.Instance == null) { return; }
        if (parentMarker.inVisionPOIs != null) {
            parentMarker.ClearPOIsInVisionRange();
        }
        if (parentMarker.character?.combatComponent.hostilesInRange != null) {
            parentMarker.character.combatComponent.ClearHostilesInRange();
        }
        if (parentMarker.character?.combatComponent.avoidInRange != null) {
            parentMarker.character.combatComponent.ClearAvoidInRange();
        }
    }
    private void OnApplicationQuit() {
        isApplicationQuitting = true;
    }
    public void Initialize() {
        VoteToFilterVision();
        Messenger.AddListener<Character, LocationStructure>(Signals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
    }
    public override void Reset() {
        base.Reset();
        Messenger.RemoveListener<Character, LocationStructure>(Signals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
        OnDisable();
    }
    protected override void OnDestroy() {
        base.OnDestroy();
        parentMarker = null;
    }

    #region Triggers
    protected override void OnTriggerEnter2D(Collider2D collision) {
        if(!parentMarker || parentMarker.character == null) {
            return;
        }
        if (!parentMarker.character.carryComponent.IsNotBeingCarried()) {
            return;
        }
        POIVisionTrigger collidedWith = collision.gameObject.GetComponent<POIVisionTrigger>();
        if (collidedWith != null && collidedWith.damageable != null
            && collidedWith.damageable != parentMarker.character) {
            if (collidedWith.damageable is Character target) {
                if (!target.carryComponent.IsNotBeingCarried()) {
                    return;
                }
            }

            if (collidedWith.damageable.gridTileLocation == null) {
                if(collidedWith.damageable is FeebleSpirit || collidedWith.damageable is RavenousSpirit || collidedWith.damageable is ForlornSpirit) {
                    //Spirits can be collided even without gridTileLocation
                    //TODO: Put inside a system instead of special case?
                } else {
                    return; //ignore, Usually happens if an item is picked up just as this character sees it.
                }
            }
            List<Trait> traitOverrideFunctions = collidedWith.poi.traitContainer.GetTraitOverrideFunctions(TraitManager.Collision_Trait);
            if(traitOverrideFunctions != null) {
                for (int i = 0; i < traitOverrideFunctions.Count; i++) {
                    Trait trait = traitOverrideFunctions[i];
                    trait.OnCollideWith(parentMarker.character, collidedWith.poi);
                }
            }

            TryAddPOIToVision(collidedWith.poi);
        }
    }
    protected override void OnTriggerExit2D(Collider2D collision) {
        POIVisionTrigger collidedWith = collision.gameObject.GetComponent<POIVisionTrigger>();
        if (collidedWith != null && collidedWith.poi != null
            && collidedWith.poi != parentMarker.character) {
            parentMarker.RemovePOIFromInVisionRange(collidedWith.poi);
            parentMarker.RemovePOIAsInRangeButDifferentStructure(collidedWith.poi);
        }
    }
    #endregion
    
    private void NormalEnterHandling(IPointOfInterest poi) {
        parentMarker.AddPOIAsInVisionRange(poi);
    }
    public void TransferAllDifferentStructureCharacters() {
        Debug.Log($"{GameManager.Instance.TodayLogString()} {parentMarker.character.name} is transferring all objects in different structures to its normal vision");
        for (int i = 0; i < parentMarker.inVisionPOIsButDiffStructure.Count; i++) {
            IPointOfInterest poi = parentMarker.inVisionPOIsButDiffStructure[i];
            if (poi.gridTileLocation == null) { continue; }
            if (TryAddPOIToVision(poi)) {
                if (parentMarker.RemovePOIAsInRangeButDifferentStructure(poi)) {
                    i--;
                }
            }
        }
    }
    public void ReCategorizeVision() {
        Debug.Log($"{GameManager.Instance.TodayLogString()} {parentMarker.character.name} Re categorizing objects in its normal vision");
        //List<IPointOfInterest> poisInVision = new List<IPointOfInterest>(parentMarker.inVisionPOIs);
        for (int i = 0; i < parentMarker.inVisionPOIs.Count; i++) {
            IPointOfInterest pointOfInterest = parentMarker.inVisionPOIs[i];
            if (pointOfInterest.gridTileLocation == null) { continue; }
            //if poi wasn't added to the characters normal vision, remove that poi from inVisionPOIs 
            if (TryAddPOIToVision(pointOfInterest) == false) {
                if (parentMarker.RemovePOIFromInVisionRange(pointOfInterest)) {
                    i--;
                }
            }
        }
    }
    /// <summary>
    /// Try to add a POI to this character's normal vision.
    /// </summary>
    /// <param name="poi">The POI to check.</param>
    /// <returns>Whether or not the poi was added to the character's normal vision.</returns>
    private bool TryAddPOIToVision(IPointOfInterest poi) {
        //(parentMarker.character.stateComponent.currentState != null && parentMarker.character.stateComponent.currentState is CombatState && 
        //parentMarker.character.movementComponent.HasPathTo(poi.gridTileLocation))|| 
        if (parentMarker && poi.gridTileLocation != null && parentMarker.character.gridTileLocation != null) {
            //if it is, just follow the normal procedure when a poi becomes in range
            if (poi.gridTileLocation.structure == parentMarker.character.gridTileLocation.structure ||
                (poi.mapObjectVisual.visionTrigger is POIVisionTrigger poiVisionTrigger && poiVisionTrigger.IgnoresStructureDifference())) {
                bool shouldAddToVisionBasedOnRoom = ShouldAddToVisionBasedOnRoom(poi);
                if (shouldAddToVisionBasedOnRoom) {
                    NormalEnterHandling(poi);
                    return true;    
                }
            }
        }
        if (poi.gridTileLocation?.structure != null && 
            parentMarker.character.gridTileLocation?.structure != null && 
            poi.gridTileLocation.structure.structureType.IsOpenSpace() && 
            parentMarker.character.gridTileLocation.structure.structureType.IsOpenSpace()) {
            //if both the character that owns this and the object is part of a structure that is open space
            //process as if normal
            NormalEnterHandling(poi);
            return true;
        } else {
            parentMarker.AddPOIAsInRangeButDifferentStructure(poi);
            return false;
        }
    }
    private bool ShouldAddToVisionBasedOnRoom(IPointOfInterest seenObject) {
        bool shouldAddToVision = true;
        if (seenObject.gridTileLocation.structure.IsTilePartOfARoom(seenObject.gridTileLocation, out var firstRoom)) {
            //if tile of seen object is part of a room, check that this character is also part of that room
            shouldAddToVision = parentMarker.character.gridTileLocation.structure.IsTilePartOfARoom(parentMarker.character.gridTileLocation, out var otherRoom) && otherRoom == firstRoom;
        }
        else if (parentMarker.character.gridTileLocation.structure.IsTilePartOfARoom(parentMarker.character.gridTileLocation, out firstRoom)) {
            //if seen object is not part of a room, then check if this character is part of a room, if it is then immediately set shouldAddToVision as false since this character is in a room
            //and the seen object is not.
            shouldAddToVision = false;
        }
        return shouldAddToVision;
    }

    #region Different Structure Handling
    private void OnCharacterArrivedAtStructure(Character character, LocationStructure structure) {
        //if (parentMarker.character.combatComponent.isInCombat) { return; } //if character is in combat, ignore this
        //if the character that arrived at the new structure is in this character different structure list
        //check if that character now has the same structure as this character,
        if (parentMarker.inVisionPOIsButDiffStructure.Contains(character) && 
            (structure == parentMarker.character.currentStructure || (structure.structureType.IsOpenSpace() && parentMarker.character.currentStructure.structureType.IsOpenSpace()))) {
            //if it does, add as normal
            bool shouldAddToVisionBasedOnRoom = ShouldAddToVisionBasedOnRoom(character);
            if (shouldAddToVisionBasedOnRoom) {
                NormalEnterHandling(character);
                parentMarker.RemovePOIAsInRangeButDifferentStructure(character);
            }
            
            // NormalEnterHandling(character);
            // parentMarker.RemovePOIAsInRangeButDifferentStructure(character);
        }
        //else if the character that arrived at the new structure is in this character's vision list and the character no longer has the same structure as this character, 
        else if (parentMarker.inVisionCharacters.Contains(character) && structure != parentMarker.character.currentStructure) {
            //if both characters are in open space, do not remove from vision
            if (structure.structureType.IsOpenSpace() && parentMarker.character.currentStructure.structureType.IsOpenSpace()) {
                return;
            }
            //remove from vision and hostile range
            //parentMarker.RemovePOIFromInVisionRange(character);
            parentMarker.AddPOIAsInRangeButDifferentStructure(character);
        }
        //if the character that changed structures is this character
        else if (character.id == parentMarker.character.id) {
            //check all pois that were in different structures and revalidate them
            for (int i = 0; i < parentMarker.inVisionPOIsButDiffStructure.Count; i++) {
                IPointOfInterest poi = parentMarker.inVisionPOIsButDiffStructure[i];
                if (poi.gridTileLocation == null || poi.gridTileLocation.structure == null) {
                    if (parentMarker.RemovePOIAsInRangeButDifferentStructure(poi)) {
                        i--;
                    }
                } else if (poi.gridTileLocation.structure == parentMarker.character.currentStructure || 
                           (poi.gridTileLocation.structure.structureType.IsOpenSpace() && parentMarker.character.currentStructure.structureType.IsOpenSpace())) {
                    bool shouldAddToVisionBasedOnRoom = ShouldAddToVisionBasedOnRoom(poi);
                    if (shouldAddToVisionBasedOnRoom) {
                        NormalEnterHandling(poi);
                        if (parentMarker.RemovePOIAsInRangeButDifferentStructure(poi)) {
                            i--;
                        }
                    }
                }
            }
            //also check all pois in vision
            for (int i = 0; i < parentMarker.inVisionPOIs.Count; i++) {
                IPointOfInterest poi = parentMarker.inVisionPOIs[i];
                if (poi.gridTileLocation == null || poi.gridTileLocation.structure == null) {
                    if (parentMarker.RemovePOIFromInVisionRange(poi)) {
                        i--;
                    }
                } else if (poi.gridTileLocation.structure != parentMarker.character.currentStructure 
                    && (!poi.gridTileLocation.structure.structureType.IsOpenSpace() || !parentMarker.character.currentStructure.structureType.IsOpenSpace())) {
                    //if the character in vision no longer has the same structure as the character, and at least one of them is not in an open space structure
                    if (parentMarker.RemovePOIFromInVisionRange(poi)) {
                        i--;
                    }
                    parentMarker.AddPOIAsInRangeButDifferentStructure(poi);
                }
            }

            if (character.currentActionNode != null && character.currentActionNode.action.actionLocationType == ACTION_LOCATION_TYPE.UPON_STRUCTURE_ARRIVAL && character.currentActionNode.targetStructure == structure && structure.structureType != STRUCTURE_TYPE.WILDERNESS) {
                parentMarker.pathfindingAI.ClearAllCurrentPathData();
                character.PerformGoapAction();
            }
        }
    }
    #endregion

    [ContextMenu("Log Diff Struct")]
    public void LogCharactersInDifferentStructures() {
        string summary = $"{parentMarker.character.name}'s diff structure pois";
        for (int i = 0; i < parentMarker.inVisionPOIsButDiffStructure.Count; i++) {
            summary += $"\n{parentMarker.inVisionPOIsButDiffStructure[i].name}";
        }
        Debug.Log(summary);
    }

    #region Utilities
    public void OnDeath() {
        parentMarker.inVisionPOIsButDiffStructure.Clear();
        OnDisable();
    }
    #endregion
}
