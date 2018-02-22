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

	protected ILocation _currLocation;
    protected ILocation targetLocation;
    protected bool _startCombatOnReachLocation;

	protected List<HexTile> path;

	protected bool _hasArrived = false;
    private bool _isInititalized = false;
    private bool _isMovementPaused = false;
    private bool _isTravelling = false;

    #region getters/setters
    public List<ECS.Character> characters {
        get { return _characters; }
    }
    public ILocation currLocation {
        get { return _currLocation; }
    }
    public bool isTravelling {
        get { return _isTravelling; }
    }
    #endregion

    internal virtual void Init(ECS.Character character) {
        this.smoothMovement.avatarGO = this.gameObject;
        _characters = new List<ECS.Character>();
        AddNewCharacter(character);
        _currLocation = character.specificLocation;
        this.smoothMovement.onMoveFinished += OnMoveFinished;
        _isInititalized = true;
    }
    internal virtual void Init(Party party) {
        this.smoothMovement.avatarGO = this.gameObject;
        _characters = new List<ECS.Character>();
        for (int i = 0; i < party.partyMembers.Count; i++) {
            AddNewCharacter(party.partyMembers[i]);
        }
		_currLocation = party.specificLocation;
        this.smoothMovement.onMoveFinished += OnMoveFinished;
        _isInititalized = true;
    }

    #region For Testing
    [ContextMenu("Log Characters")]
    public void LogPartyMembers() {
        Debug.Log("========== Characters ==========");
        if (characters[0].party != null) {
            Debug.Log("Party: " + characters[0].party.name);
        }
        for (int i = 0; i < characters.Count; i++) {
            ECS.Character currMember = characters[i];
            Debug.Log(currMember.name);
        }
    }
    #endregion

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
    internal void SetTarget(ILocation target, bool startCombatOnReachLocation = false) {
        targetLocation = target;
        _startCombatOnReachLocation = startCombatOnReachLocation;
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
            if (_characters[0].party == null) {
                faction = _characters[0].faction;
            } else {
                faction = _characters[0].party.partyLeader.faction;
            }
			PathGenerator.Instance.CreatePath(this, this.currLocation.tileLocation, targetLocation.tileLocation, pathFindingMode, faction);
            
            //this.path = PathGenerator.Instance.GetPath(this.currLocation, this.targetLocation, pathFindingMode, faction);
            //NewMove();
        }
    }
    internal virtual void ReceivePath(List<HexTile> path) {
        if (!_isInititalized) {
            return;
        }
        if (path != null && path.Count > 0) {
            if (this.currLocation.tileLocation == null) {
                throw new Exception("Curr location of avatar is null! Is Inititalized: " + _isInititalized.ToString());
            }
			if(this.currLocation.tileLocation.landmarkOnTile != null){
				if(_characters[0].party != null){
					this.currLocation.tileLocation.landmarkOnTile.AddHistory (_characters [0].party.name + " left.");
				}else{
					for (int i = 0; i < _characters.Count; i++) {
						this.currLocation.tileLocation.landmarkOnTile.AddHistory (_characters [i].name + " left.");
					}
				}
			}

            this.path = path;
            _isTravelling = true;
            NewMove();
        }
    }
    internal virtual void NewMove() {
		if(_characters[0].isInCombat){
			_characters[0].SetCurrentFunction (() => NewMove ());
			return;
		}
        if (this.targetLocation != null) {
            if (this.path != null) {
                if (this.path.Count > 0) {
                    RemoveCharactersFromLocation(this.currLocation);
                    this.MakeCitizenMove(this.currLocation.tileLocation, this.path[0]);
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
			//RemoveCharactersFromLocation(this.currLocation);
			AddCharactersToLocation(this.path[0]);

            _currLocation = this.path[0];
            this.path.RemoveAt(0);
        }
        //RevealRoads();
        //RevealLandmarks();

        HasArrivedAtTargetLocation();
    }
    internal virtual void HasArrivedAtTargetLocation() {
        if (this.currLocation.tileLocation == targetLocation.tileLocation) {
            if (!this._hasArrived) {
                _isTravelling = false;
                AddCharactersToLocation(targetLocation, _startCombatOnReachLocation);
                _currLocation = targetLocation; //set location as the target location, in case the target location is a landmark
                if (this.currLocation.tileLocation.landmarkOnTile != null){
					string historyText = "Visited landmark ";
					if (this.currLocation.tileLocation.landmarkOnTile is Settlement) {
						historyText = "Arrived at settlement ";
					}
						
					if(_characters[0].party != null){
						this.currLocation.tileLocation.landmarkOnTile.AddHistory (_characters [0].party.name + " visited.");
						for (int i = 0; i < _characters.Count; i++) {
							_characters [i].AddHistory (historyText + this.currLocation.tileLocation.landmarkOnTile.landmarkName + ".");
						}
					}else{
						for (int i = 0; i < _characters.Count; i++) {
							_characters [i].AddHistory (historyText + this.currLocation.tileLocation.landmarkOnTile.landmarkName + ".");
							this.currLocation.tileLocation.landmarkOnTile.AddHistory (_characters [i].name + " visited.");
						}
					}
				}
                SetHasArrivedState(true);
                if(onPathFinished != null) {
                    onPathFinished();
                }
            }
		}else{
            if (!_isMovementPaused) {
                NewMove();
            }
		}
    }
    internal void SetHasArrivedState(bool state) {
        _hasArrived = state;
    }
    internal void PauseMovement() {
        _isMovementPaused = true;
        smoothMovement.ForceStopMovement();
    }
    internal void ResumeMovement() {
        _isMovementPaused = false;
        NewMove();
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
        this.currLocation.tileLocation.SetRoadState(true);
    }
    private void RevealLandmarks() {
        if(this.currLocation.tileLocation.landmarkOnTile != null) {
            this.currLocation.tileLocation.landmarkOnTile.SetHiddenState(false);
        }
    }
    private void RemoveCharactersFromLocation(ILocation location) {
		if(_characters[0].party == null){
			for (int i = 0; i < _characters.Count; i++) {
				ECS.Character currCharacter = _characters[i];
				location.RemoveCharacterFromLocation(currCharacter);
			}
		}else{
			location.RemoveCharacterFromLocation(_characters[0].party);
		}
        
		UIManager.Instance.UpdateHexTileInfo();
        UIManager.Instance.UpdateSettlementInfo();
    }
	private void AddCharactersToLocation(ILocation location, bool startCombatOnReachLocation = true) {
		if(_characters[0].party == null){
			for (int i = 0; i < _characters.Count; i++) {
				ECS.Character currCharacter = _characters[i];
				location.AddCharacterToLocation(currCharacter, startCombatOnReachLocation);
			}
		}else{
			location.AddCharacterToLocation(_characters[0].party, startCombatOnReachLocation);
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
        _isInititalized = false;
    }
    #endregion
}
