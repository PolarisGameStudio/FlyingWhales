﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using EZObjectPools;
using System;
using ECS;

public class CharacterAvatar : MonoBehaviour{

    private Action onPathFinished;

    private PathFindingThread _currPathfindingRequest; //the current pathfinding request this avatar is waiting for

	[SerializeField] protected SmoothMovement smoothMovement;
	[SerializeField] protected DIRECTION direction;
    [SerializeField] protected GameObject _avatarHighlight;
    [SerializeField] protected GameObject _avatarVisual;
    [SerializeField] protected SpriteRenderer _avatarSpriteRenderer;
    [SerializeField] protected SpriteRenderer _frameSpriteRenderer;
    [SerializeField] protected SpriteRenderer _centerSpriteRenderer;

    protected NewParty _party;

    protected ILocation targetLocation;

    [SerializeField] protected List<HexTile> path;

	[SerializeField] private bool _hasArrived = false;
    [SerializeField] private bool _isInitialized = false;
    [SerializeField] private bool _isMovementPaused = false;
    [SerializeField] private bool _isTravelling = false;
    [SerializeField] private bool _isMovingToHex = false;
	private Action queuedAction = null;

    public CharacterPortrait characterPortrait { get; private set; }

    #region getters/setters
    public NewParty party {
        get { return _party; }
    }
    public bool isTravelling {
        get { return _isTravelling; }
    }
    public bool isMovementPaused {
        get { return _isMovementPaused; }
    }
    public bool isMovingToHex {
		get { return _isMovingToHex; }
	}
    public bool hasArrived {
        get { return _hasArrived; }
    }
    public GameObject avatarVisual {
        get { return _avatarVisual; }
    }
    #endregion

    public virtual void Init(NewParty party) {
        _party = party;
        //SetPosition(_party.specificLocation.tileLocation.transform.position);
        this.smoothMovement.avatarGO = this.gameObject;
        this.smoothMovement.onMoveFinished += OnMoveFinished;
        _isInitialized = true;
        _hasArrived = true;
		if(_party.mainCharacter.role != null){
			SetSprite (_party.mainCharacter.role.roleType);
		}
#if !WORLD_CREATION_TOOL
        GameObject portraitGO = UIManager.Instance.InstantiateUIObject(CharacterManager.Instance.characterPortraitPrefab.name, this.transform);
        characterPortrait = portraitGO.GetComponent<CharacterPortrait>();
        characterPortrait.GeneratePortrait(_party.mainCharacter, IMAGE_SIZE.X64, false);
        portraitGO.SetActive(false);

        CharacterManager.Instance.AddCharacterAvatar(this);
#endif
    }

    #region Monobehaviour
    private void OnDestroy() {
#if !WORLD_CREATION_TOOL
        CharacterManager.Instance.RemoveCharacterAvatar(this);
#endif
    }
    #endregion

    #region Pathfinding
    public void SetTarget(ILocation target) {
        targetLocation = target;
    }
    public void StartPath(PATHFINDING_MODE pathFindingMode, Action actionOnPathFinished = null) {
        //if (smoothMovement.isMoving) {
        //    smoothMovement.ForceStopMovement();
        //}
        Reset();
        if (this.targetLocation != null) {
            SetHasArrivedState(false);
            onPathFinished = actionOnPathFinished;
			Faction faction = _party.faction;
			_currPathfindingRequest = PathGenerator.Instance.CreatePath(this, _party.specificLocation.tileLocation, targetLocation.tileLocation, pathFindingMode, faction);
        }
    }
    public virtual void ReceivePath(List<HexTile> path, PathFindingThread fromThread) {
        if (!_isInitialized) {
            return;
        }
        if (_currPathfindingRequest == null) {
            return; //this avatar currently has no pathfinding request
        } else {
            if (_currPathfindingRequest != fromThread) {
                return; //the current pathfinding request and the thread that returned the path are not the same
            }
        }
        if (path == null) {
            Debug.LogError(_party.name + ". There is no path from " + _party.specificLocation.tileLocation.name + " to " + targetLocation.tileLocation.name, this);
            return;
        }
        if (path != null && path.Count > 0) {
            this.path = path;
            _currPathfindingRequest = null;
            _isTravelling = true;
            if(_party.specificLocation.locIdentifier == LOCATION_IDENTIFIER.LANDMARK) {
                _party.specificLocation.tileLocation.landmarkOnTile.landmarkVisual.OnCharacterExitedLandmark(_party);
            }
            NewMove();
        }
    }
    public virtual void NewMove() {
        if (this.targetLocation != null && this.path != null) {
            if (this.path.Count > 0) {
				this.MakeCitizenMove(_party.specificLocation.tileLocation, this.path[0]);
                //RemoveCharactersFromLocation(_party.specificLocation);
                //AddCharactersToLocation(this.path[0]);
                //this.path.RemoveAt(0);
            }
        }
    }
    public void MakeCitizenMove(HexTile startTile, HexTile targetTile) {
		//CharacterHasLeftTile ();
		_isMovingToHex = true;
        this.smoothMovement.Move(targetTile.transform.position, this.direction);
    }
    /*
     This is called each time the avatar traverses a node in the
     saved path.
         */
    public virtual void OnMoveFinished() {
		_isMovingToHex = false;
		if(this.path == null){
			Debug.LogError (GameManager.Instance.Today ().ToStringDate());
			Debug.LogError ("Location: " + _party.specificLocation.locationName);
		}
        if (this.path.Count > 0) {
            RemoveCharactersFromLocation(_party.specificLocation);
            AddCharactersToLocation(this.path[0]);
            this.path.RemoveAt(0);
        }
        HasArrivedAtTargetLocation();
    }
    public virtual void HasArrivedAtTargetLocation() {
		if (_party.specificLocation.tileLocation.id == targetLocation.tileLocation.id) {
            if (!this._hasArrived) {
                _isTravelling = false;
                SetHasArrivedState(true);
                targetLocation.AddCharacterToLocation(_party);
                Debug.Log(_party.name + " has arrived at " + targetLocation.locationName + " on " + GameManager.Instance.Today().GetDayAndTicksString());
                //Every time the party arrives at home, check if it still not ruined
                if(_party.mainCharacter is Character && _party.mainCharacter.homeLandmark.specificLandmarkType == LANDMARK_TYPE.CAMP && _party.mainCharacter.homeLandmark.landmarkObj.currentState.stateName == "Ruined") {
                    //Check if the location the character arrived at is the character's home landmark
                    if (targetLocation.tileLocation.id == _party.mainCharacter.homeLandmark.tileLocation.id) {
                        //Check if the current landmark in the location is a camp and it is not yet ruined
                        if (targetLocation.tileLocation.landmarkOnTile.specificLandmarkType == LANDMARK_TYPE.CAMP) {
                            Character character = _party.mainCharacter as Character;
                            if (targetLocation.tileLocation.landmarkOnTile.landmarkObj.currentState.stateName != "Ruined") {
                                //Make it the character's new home landmark
                                _party.mainCharacter.homeLandmark.RemoveCharacterHomeOnLandmark(character);
                                targetLocation.tileLocation.landmarkOnTile.AddCharacterHomeOnLandmark(character);
                            } else {
                                //Create new camp
                                BaseLandmark newCamp = targetLocation.tileLocation.areaOfTile.CreateCampOnTile(targetLocation.tileLocation);
                                _party.mainCharacter.homeLandmark.RemoveCharacterHomeOnLandmark(character);
                                newCamp.AddCharacterHomeOnLandmark(character);
                            }
                        }
                    }
                }
                if(onPathFinished != null) {
                    onPathFinished();
                }
            }
			if(queuedAction != null){
				queuedAction ();
				queuedAction = null;
				return;
			}
		}else{
			if(queuedAction != null){
				queuedAction ();
				queuedAction = null;
				return;
			}
            if (!_isMovementPaused) {
                NewMove();
            }
		}
    }
    public void SetHasArrivedState(bool state) {
        _hasArrived = state;
    }
    public void PauseMovement() {
        Debug.Log(_party.name + " has paused movement!");
        _isMovementPaused = true;
        smoothMovement.ForceStopMovement();
    }
    public void ResumeMovement() {
        Debug.Log(_party.name + " has resumed movement!");
        _isMovementPaused = false;
        NewMove();
    }
    public void AddActionOnPathFinished(Action action) {
        onPathFinished += action;
    }
    #endregion

    #region Utilities
    /*
     This will set the avatar reference of all characters
     using this avatar to null, then return this object back to the pool.
         */
    public void DestroyObject() {
        ObjectPoolManager.Instance.DestroyObject(this.gameObject);
    }
    protected void RemoveCharactersFromLocation(ILocation location) {
        location.RemoveCharacterFromLocation(_party);
        UIManager.Instance.UpdateHexTileInfo();
    }
	protected void AddCharactersToLocation(ILocation location) {
        if(location.tileLocation.id == targetLocation.id) {
            targetLocation.AddCharacterToLocation(_party);
        } else {
            location.AddCharacterToLocation(_party);
        }
		UIManager.Instance.UpdateHexTileInfo();
    }
    public void ReclaimPortrait() {
        characterPortrait.transform.SetParent(this.transform);
        //(characterPortrait.transform as RectTransform).pivot = new Vector2(1f, 1f);
        characterPortrait.gameObject.SetActive(false);
    }
    public void SetVisualState(bool state) {
        _avatarVisual.SetActive(state);
    }
    public void SetQueuedAction(Action action){
		queuedAction = action;
	}
    public void SetHighlightState(bool state) {
        _avatarHighlight.SetActive(state);
    }
    public void SetPosition(Vector3 position) {
        this.transform.position = position;
    }
    //private void CharacterHasLeftTile(){
    //	LeaveCharacterTrace();
    //       CheckForItemDrop();
    //}
    public void SetSprite(CHARACTER_ROLE role){
		Sprite sprite = CharacterManager.Instance.GetSpriteByRole (role);
		if(sprite != null){
			_avatarSpriteRenderer.sprite = sprite;
		}
	}
    public void SetMovementState(bool state) {
        smoothMovement.isHalted = state;
    }
    public void SetFrameOrderLayer(int layer) {
        _frameSpriteRenderer.sortingOrder = layer;
    }
    public void SetCenterOrderLayer(int layer) {
        _centerSpriteRenderer.sortingOrder = layer;
    }
    #endregion

    //#region Traces
    //private void LeaveCharacterTrace() {
    //    if (_characters[0].party == null) {
    //        _characters[0].LeaveTraceOnLandmark();
    //    } else {
    //        if (_characters[0].party.mainCharacter is Character) {
    //            Character character = _characters[0].party.mainCharacter as Character;
    //            character.LeaveTraceOnLandmark();
    //        }
    //    }
    //}
    //#endregion

    //#region Items
    //private void CheckForItemDrop() {
    //    if (_characters[0].party == null) {
    //        _characters[0].CheckForItemDrop();
    //    } else {
    //        if (_characters[0].party.mainCharacter is Character) {
    //            Character character = _characters[0].party.mainCharacter as Character;
    //            character.LeaveTraceOnLandmark();
    //        }
    //    }
    //}
    //#endregion

    #region overrides
    public void Reset() {
        //base.Reset();
        smoothMovement.Reset();
        onPathFinished = null;
        direction = DIRECTION.LEFT;
        //targetLocation = null;
        path = null;
        _isMovementPaused = false;
        _hasArrived = false;
        //_isInitialized = false;
        _currPathfindingRequest = null;
        SetHighlightState(false);
    }
    #endregion


}
