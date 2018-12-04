﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Raider : Job {

    private string _action; //Raid or Scavenge

    #region getters/setters
    public string action {
        get { return _action; }
    }
    #endregion

    public Raider(Character character) : base(character, JOB.RAIDER) {
        _actionDuration = 50;
        _hasCaptureEvent = false;
        _characterInteractions = new INTERACTION_TYPE[] { INTERACTION_TYPE.MOVE_TO_SCAVENGE };
    }

    #region Overrides
    public override void DoJobAction() {
        base.DoJobAction();

        int baseSuccessRate = 50;
        int baseFailRate = 40;
        int criticalFailRate = 12;

        //Success Rate +1 per level starting at Level 6
        baseSuccessRate += (Mathf.Max(character.level - 5, 0));
        //Critical Fail Rate -1 per mult of 4 level starting at Level 6
        if (character.level > 6) {
            criticalFailRate -= Mathf.FloorToInt(character.level / 4);
        }

        WeightedDictionary<RESULT> rateWeights = new WeightedDictionary<RESULT>();
        rateWeights.AddElement(RESULT.SUCCESS, baseSuccessRate);
        //rateWeights.AddElement(RESULT.FAIL, baseFailRate);
        //rateWeights.AddElement(RESULT.CRITICAL_FAIL, criticalFailRate);

        if (rateWeights.GetTotalOfWeights() > 0) {
            RESULT chosenResult = rateWeights.PickRandomElementGivenWeights();
            switch (chosenResult) {
                case RESULT.SUCCESS:
                    RaidSuccess();
                    break;
                case RESULT.FAIL:
                    RaidFail();
                    break;
                case RESULT.CRITICAL_FAIL:
                    CriticalRaidFail();
                    break;
                default:
                    break;
            }
        } else {
            //go back home
            GoBackHome();
        }
    }
    public override int GetSuccessRate() {
        int baseRate = 60;
        int multiplier = _character.level - 5;
        if (multiplier < 0) {
            multiplier = 0;
        }
        return baseRate + multiplier;
    }
    #endregion

    private void RaidSuccess() {
        int obtainedSupply = GetSupplyObtained(character.specificLocation.tileLocation.areaOfTile);
        SetCreatedInteraction(InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.RAID_SUCCESS, character.specificLocation.tileLocation.landmarkOnTile));
        _createdInteraction.AddEndInteractionAction(() => GoBackHomeSuccess(obtainedSupply));
        _createdInteraction.ScheduleSecondTimeOut();
        _createdInteraction.SetOtherData(new object[] { obtainedSupply });
        character.AddInteraction(_createdInteraction);
        //When a raid succeeds, the target Faction's Favor Count towards the raider is reduced by -2. 
        //FavorEffects(-2);
    }
    private void RaidFail() {
        SetCreatedInteraction(InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.MINION_FAILED, character.specificLocation.tileLocation.landmarkOnTile));
        //raidSuccess.SetEndInteractionAction(() => GoBackHome());
        _createdInteraction.ScheduleSecondTimeOut();
        //When a raid fails, the target Faction's Favor Count towards the raider is reduced by -1. The raider will not get anything.
        FavorEffects(-1);
        //GoBackHome();
    }
    private void CriticalRaidFail() {
        //When a raid critically fails, the target Faction's Favor Count towards the raider is reduced by -1. The raider will also perish.
        SetCreatedInteraction(InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.MINION_CRITICAL_FAIL, character.specificLocation.tileLocation.landmarkOnTile));
        //raidSuccess.SetEndInteractionAction(() => GoBackHome());
        _createdInteraction.ScheduleSecondTimeOut();
        FavorEffects(-1);
        GoBackHome();
    }

    private void GoBackHome() {
        if (character.minion != null) {
            character.minion.GoBackFromAssignment();
        } else {
            character.currentParty.GoHome();
        }
        
    }
    private void GoBackHomeSuccess(int supplyObtained) {
        character.homeLandmark.tileLocation.areaOfTile.AdjustSuppliesInBank(supplyObtained);
        GoBackHome();
    }

    public int GetSupplyObtained(Area targetArea) {
        //When a raid succeeds, the amount of Supply obtained is based on character level.
        //5% to 15% of location's supply 
        //+1% every other level starting at level 6
        Area characterHomeArea = character.homeLandmark.tileLocation.areaOfTile;
        //Area targetArea = character.specificLocation.tileLocation.areaOfTile;
        int supplyObtainedPercent = Random.Range(5, 16);
        supplyObtainedPercent += (character.level - 5);

        return Mathf.FloorToInt(targetArea.suppliesInBank * (supplyObtainedPercent / 100f));
        //characterHomeArea.AdjustSuppliesInBank(obtainedSupply);
    }

    private void FavorEffects(int amount) {
        Area targetArea = character.specificLocation.tileLocation.areaOfTile;
        if (targetArea.owner != null) {
            targetArea.owner.AdjustFavorFor(character.faction, amount);
        }
    }
}
