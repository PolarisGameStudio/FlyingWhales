﻿using ECS;
using Pathfinding;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterIcon : MonoBehaviour {

    [SerializeField] private SpriteRenderer _icon;
    [SerializeField] private GameObject _avatarGO;
    [SerializeField] private CharacterAIPath _aiPath;
    [SerializeField] private SpriteRenderer _avatarSprite;
    [SerializeField] private Animator _avatarAnimator;
    [SerializeField] private AIDestinationSetter _destinationSetter;

    private Character _character;

    private ILocation _targetLocation;
    private bool _isIdle;
    private string upOrDown = "Down";
    private string previousDir;

    #region getters/setters
    public Character character {
        get { return _character; }
    }
    public CharacterAIPath aiPath {
        get { return _aiPath; }
    }
    public ILocation targetLocation {
        get { return _targetLocation; }
    }
    public AIDestinationSetter destinationSetter {
        get { return _destinationSetter; }
    }
    #endregion

    public void SetCharacter(Character character) {
        _character = character;
        //UpdateColor();
        this.name = _character.name + "'s Icon";
        _isIdle = true;
        //if (_character.role != null) {
        //    _avatarSprite.sprite = CharacterManager.Instance.GetSpriteByRole(_character.role.roleType);
        //}
        
        Messenger.AddListener<ECS.Character>(Signals.ROLE_CHANGED, OnRoleChanged);
        Messenger.AddListener<bool>(Signals.PAUSED, SetMovementState);
        Messenger.AddListener<PROGRESSION_SPEED>(Signals.PROGRESSION_SPEED_CHANGED, OnProgressionSpeedChanged);
    }

    public void SetTarget(ILocation target) {
        //remove character from his/her specific location
        if (character.specificLocation != null) {
            character.specificLocation.RemoveCharacterFromLocation(character);
        }
        _targetLocation = target;
        if (target != null) {
            _destinationSetter.target = target.tileLocation.transform;
        } else {
            _destinationSetter.target = null;
        }
        
        //_aiPath.destination = _targetLocation.tileLocation.transform.position;
        //_aiPath.SetRecalculatePathState(true);
    }

    #region Visuals
    private void UpdateColor() {
        if (_character.role == null) {
            return;
        }
        switch (_character.role.roleType) {
            case CHARACTER_ROLE.HERO:
                _icon.color = Color.blue;
                break;
            case CHARACTER_ROLE.VILLAIN:
                _icon.color = Color.red;
                break;
            default:
                break;
        }
    }
    public void SetAvatarState(bool state) {
        //_avatarGO.SetActive(state);
    }
    #endregion

    #region Speed
    private void SetMovementState(bool state) {
        if (state) {
            _aiPath.maxSpeed = 0f;
        }
    }
    private void OnProgressionSpeedChanged(PROGRESSION_SPEED speed) {
        switch (speed) {
            case PROGRESSION_SPEED.X1:
                _aiPath.maxSpeed = 1;
                break;
            case PROGRESSION_SPEED.X2:
                _aiPath.maxSpeed = 2;
                break;
            case PROGRESSION_SPEED.X4:
                _aiPath.maxSpeed = 4;
                break;
        }
    }
    #endregion

    private void OnRoleChanged(Character character) {
        if (_character.id == character.id) {
            //UpdateColor();
            //if (_character.role != null) {
            //    _avatarSprite.sprite = CharacterManager.Instance.GetSpriteByRole(_character.role.roleType);
            //}
        }
    }

    public void SetPosition(Vector3 position) {
        this.transform.position = position;
    }

    public void SetActionOnTargetReached(Action action) {
        _aiPath.SetActionOnTargetReached(action);
    }

    #region Monobehaviours
    private void LateUpdate() {
        //Debug.Log(_aiPath.velocity);
        Vector3 newPos = _aiPath.transform.localPosition;
        newPos.y += 0.38f;
        newPos.x += 0.02f;
        _avatarGO.transform.localPosition = newPos;

        
    }
    void FixedUpdate() {
        //Debug.Log(_aiPath.velocity);
        if (_aiPath.velocity != Vector3.zero) {
            if (GetLeftRight() == "Left") {
                if (_avatarAnimator.transform.localScale.x < 0f) {
                    _avatarAnimator.transform.localScale = new Vector3(_avatarAnimator.transform.localScale.x * -1f, _avatarAnimator.transform.localScale.y, _avatarAnimator.transform.localScale.z);
                }
            } else {
                if (_avatarAnimator.transform.localScale.x >= 0f) {
                    _avatarAnimator.transform.localScale = new Vector3(_avatarAnimator.transform.localScale.x * -1f, _avatarAnimator.transform.localScale.y, _avatarAnimator.transform.localScale.z);
                }
            }
            upOrDown = GetUpDown();
            Walk(upOrDown);
        } else {
            Idle(upOrDown);
        }
    }
    #endregion

    #region Animation
    private void Idle(string direction) {
        if (!_isIdle) {
            _isIdle = true;
            _avatarAnimator.Play("Idle_" + direction);
        }
    }
    private void Walk(string direction) {
        if (_isIdle || previousDir != direction) {
            _isIdle = false;
            _avatarAnimator.Play("Walk_" + direction);
            previousDir = direction;
        }
    }
    private string GetLeftRight() {
        //Debug.Log(_aiPath.velocity.x);
        if(_aiPath.velocity.x <= 0f) {
            return "Left";
        } else {
            return "Right";
        }
    }
    private string GetUpDown() {
        if (_aiPath.velocity.y <= 0f) {
            return "Down";
        } else {
            return "Up";
        }
    }
    #endregion
}
