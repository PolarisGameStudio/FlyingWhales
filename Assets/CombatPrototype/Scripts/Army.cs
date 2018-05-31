﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Army {
    private BaseLandmark _originLandmark;
    private int _armyCount;
    private ArmyIcon _icon;
    private bool _isDestroyed;

    #region getters/setters
    public BaseLandmark originLandmark {
        get { return _originLandmark; }
    }
    public int armyCount {
        get { return _armyCount; }
    }
    #endregion

    public Army(BaseLandmark originLandmark, int armyCount) {
        _originLandmark = originLandmark;
        _isDestroyed = false;
        CreateIcon();
        AdjustArmyCount(armyCount);
    }

    #region Icon
    private void CreateIcon() {
        GameObject armyIconGO = GameObject.Instantiate(CharacterManager.Instance.armyIconPrefab,
               Vector3.zero, Quaternion.identity, CharacterManager.Instance.armyIconsParent);
        _icon = armyIconGO.GetComponent<ArmyIcon>();
        _icon.SetArmy(this);
        PathfindingManager.Instance.AddAgent(_icon.aiPath);
        _icon.SetPosition(_originLandmark.tileLocation.transform.position);
    }
    #endregion

    #region Utilities
    public void SetTarget(BaseLandmark landmark) {
        _icon.SetTarget(landmark);
    }
    public void ReachedTarget() {
        Damage(_icon.targetLandmark);
        if (!_isDestroyed) {
            DestroyArmy();
        }
    }
    public void CollidedWithLandmark(BaseLandmark landmark) {
        if(landmark.id == _icon.targetLandmark.id) {
            return;
        }
        if (_originLandmark.owner != null && landmark.owner != null) {
            if(_originLandmark.owner.id == landmark.owner.id) {
                return;
            }
        }
        Damage(landmark);
        //if(_originLandmark.owner != null && landmark.owner != null) {
        //    FactionRelationship relationship = _originLandmark.owner.GetRelationshipWith(landmark.owner);
        //    if (relationship.isAtWar) {
        //        Damage(landmark);
        //    }
        //} else {
        //    Damage(landmark);
        //}
    }
    private void Damage(BaseLandmark landmark) {
        int armyCount = _armyCount;
        int landmarkHP = landmark.landmarkObj.currentHP;

        AdjustArmyCount(-landmarkHP);
        landmark.landmarkObj.AdjustHP(-armyCount);
    }
    public void AdjustArmyCount(int amount) {
        _armyCount += amount;
        if(_armyCount <= 0) {
            _armyCount = 0;
            //Destroy Army
            DestroyArmy();
        }
        _icon.UpdateArmyCountLabel();
    }
    private void DestroyArmy() {
        _isDestroyed = true;
        _originLandmark.SetIsAttackingAnotherLandmarkState(false);
        if (_originLandmark.landmarkObj.specificObjectType == SPECIFIC_OBJECT_TYPE.HUMAN_SETTLEMENT) {
            (_originLandmark.landmarkObj as HumanSettlement).CommenceTraining();
        } else if (_originLandmark.landmarkObj.specificObjectType == SPECIFIC_OBJECT_TYPE.ELVEN_SETTLEMENT) {
            (_originLandmark.landmarkObj as ElvenSettlement).CommenceTraining();
        }
        GameObject.Destroy(_icon.gameObject);
    }
    #endregion
}