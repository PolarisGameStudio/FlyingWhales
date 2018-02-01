﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using EZObjectPools;
using System;

public class CharacterAvatar : PooledObject{

    //public delegate void OnPathFinished();
    //public OnPathFinished onPathFinished;
    private Action onPathFinished;

	[SerializeField] protected SmoothMovement smoothMovement;
	[SerializeField] protected DIRECTION direction;

	protected List<ECS.Character> _characters;

	protected HexTile _currLocation;
    protected HexTile targetLocation;

	protected List<HexTile> path;

	protected bool _hasArrived = false;

    #region getters/setters
    public HexTile currLocation {
        get { return _currLocation; }
    }
    #endregion

    internal virtual void Init(ECS.Character character) {
        this.smoothMovement.avatarGO = this.gameObject;
        _characters = new List<ECS.Character>();
        AddNewCharacter(character);
        _currLocation = character.currLocation;
        this.smoothMovement.onMoveFinished += OnMoveFinished;
    }
    internal virtual void Init(Party party) {
        this.smoothMovement.avatarGO = this.gameObject;
        _characters = new List<ECS.Character>();
        for (int i = 0; i < party.partyMembers.Count; i++) {
            AddNewCharacter(party.partyMembers[i]);
        }
        _currLocation = party.currLocation;
        this.smoothMovement.onMoveFinished += OnMoveFinished;
    }

    #region ECS.Character Management
    public void AddNewCharacter(ECS.Character character) {
        if (!_characters.Contains(character)) {
            _characters.Add(character);
            character.SetAvatar(this);
        }
    }
    public void RemoveCharacter(ECS.Character character) {
        _characters.Remove(character);
        character.SetAvatar(null);
		if(_characters.Count <= 0){
			DestroyObject ();
		}
    }
    #endregion

    #region Pathfinding
    internal void SetTarget(HexTile target) {
        targetLocation = target;
    }
    internal void StartPath(PATHFINDING_MODE pathFindingMode, Action actionOnPathFinished = null) {
        if (smoothMovement.isMoving) {
            smoothMovement.ForceStopMovement();
        }
        if (this.targetLocation != null) {
            SetHasArrivedState(false);
            onPathFinished = actionOnPathFinished;
            //if(actionOnPathFinished != null) {
            //    onPathFinished += actionOnPathFinished;
            //}
            Faction faction = null;
            if(_characters[0].party == null) {
                faction = _characters[0].faction;
            } else {
                faction = _characters[0].party.partyLeader.faction;
            }
            PathGenerator.Instance.CreatePath(this, this.currLocation, this.targetLocation, pathFindingMode, faction);
        }
    }
    internal virtual void ReceivePath(List<HexTile> path) {
        if (path != null && path.Count > 0) {
			if(this.currLocation.landmarkOnTile != null){
				if(_characters[0].party != null){
					this.currLocation.landmarkOnTile.AddHistory (_characters [0].party.name + " left.");
				}else{
					for (int i = 0; i < _characters.Count; i++) {
						this.currLocation.landmarkOnTile.AddHistory (_characters [i].name + " left.");
					}
				}
			}

            this.path = path;
            NewMove();
        }
    }
    internal virtual void NewMove() {
        if (this.targetLocation != null) {
            if (this.path != null) {
                if (this.path.Count > 0) {
                    this.MakeCitizenMove(this.currLocation, this.path[0]);
                }
            }
        }
    }
    internal void MakeCitizenMove(HexTile startTile, HexTile targetTile) {
        this.smoothMovement.Move(targetTile.transform.position, this.direction);
    }
    /*
     This is called each time the avatar traverses a node in the
     saved path.
         */
    internal virtual void OnMoveFinished() {
        if (this.path.Count > 0) {
			RemoveCharactersFromTile(this.currLocation);
			AddCharactersToTile(this.path[0]);

            _currLocation = this.path[0];
            this.path.RemoveAt(0);
        }
        //RevealRoads();
        //RevealLandmarks();

        HasArrivedAtTargetLocation();
    }
    internal virtual void HasArrivedAtTargetLocation() {
        if (this.currLocation == this.targetLocation) {
            if (!this._hasArrived) {
				if(this.currLocation.landmarkOnTile != null){
					string historyText = "Visited landmark ";
					if (this.currLocation.landmarkOnTile is Settlement) {
						historyText = "Arrived at settlement ";
					}
						
					if(_characters[0].party != null){
						this.currLocation.landmarkOnTile.AddHistory (_characters [0].party.name + " visited.");
						for (int i = 0; i < _characters.Count; i++) {
							_characters [i].AddHistory (historyText + this.currLocation.landmarkOnTile.landmarkName + ".");
						}
					}else{
						for (int i = 0; i < _characters.Count; i++) {
							_characters [i].AddHistory (historyText + this.currLocation.landmarkOnTile.landmarkName + ".");
							this.currLocation.landmarkOnTile.AddHistory (_characters [i].name + " visited.");
						}
					}
				}
                SetHasArrivedState(true);
                if(onPathFinished != null) {
                    onPathFinished();
                }
            }
		}else{
			NewMove();
		}
    }
    internal void SetHasArrivedState(bool state) {
        _hasArrived = state;
    }
    #endregion

    #region Utilities
    /*
     This will set the avatar reference of all characters
     using this avatar to null, then return this object back to the pool.
         */
    public void DestroyObject() {
        for (int i = 0; i < _characters.Count; i++) {
            ECS.Character currCharacter = _characters[i];
            currCharacter.SetAvatar(null);
        }
        ObjectPoolManager.Instance.DestroyObject(this.gameObject);
    }
    private void RevealRoads() {
        this.currLocation.SetRoadState(true);
    }
    private void RevealLandmarks() {
        if(this.currLocation.landmarkOnTile != null) {
            this.currLocation.landmarkOnTile.SetHiddenState(false);
        }
    }
    private void RemoveCharactersFromTile(HexTile hextile) {
		if(_characters[0].party == null){
			for (int i = 0; i < _characters.Count; i++) {
				ECS.Character currCharacter = _characters[i];
				hextile.RemoveCharacterOnTile(currCharacter);
			}
		}else{
			hextile.RemoveCharacterOnTile(_characters[0].party);
		}
        
		UIManager.Instance.UpdateHexTileInfo();
        UIManager.Instance.UpdateSettlementInfo();
    }
	private void AddCharactersToTile(HexTile hextile) {
		if(_characters[0].party == null){
			for (int i = 0; i < _characters.Count; i++) {
				ECS.Character currCharacter = _characters[i];
				hextile.AddCharacterOnTile(currCharacter);
			}
		}else{
			hextile.AddCharacterOnTile(_characters[0].party);
		}

		UIManager.Instance.UpdateHexTileInfo();
        UIManager.Instance.UpdateSettlementInfo();
    }
    #endregion

    #region overrides
    public override void Reset() {
        base.Reset();
        smoothMovement.Reset();
        onPathFinished = null;
        direction = DIRECTION.LEFT;
        _currLocation = null;
        targetLocation = null;
        path = null;
        _hasArrived = false;
    }
    #endregion
}
