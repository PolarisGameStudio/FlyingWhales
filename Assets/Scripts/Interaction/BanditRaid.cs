﻿using ECS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BanditRaid : Interaction {

    private BaseLandmark chosenLandmarkToRaid;
    private BaseLandmark originLandmark;

    //private WeightedDictionary<LandmarkDefender> assaultSpawnWeights;
    //private WeightedDictionary<LandmarkDefender> firstElementAssaultSpawnWeights; //TODO: Make this more elegant!

    public BanditRaid(IInteractable interactable) : base(interactable, INTERACTION_TYPE.BANDIT_RAID, 70) {
        _name = "Bandit Raid";
    }

    #region Overrides
    public override void CreateStates() {
        if (_interactable is BaseLandmark) {
            originLandmark = interactable as BaseLandmark;
            //CreateExploreStates();
            //CreateWhatToDoNextState("What do you want %minion% to do next?");
            //ConstructAssaultSpawnWeights();
            chosenLandmarkToRaid = GetLandmarkToRaid(originLandmark);
            
            InteractionState startState = new InteractionState("Start", this);
            //string startStateDesc = "The bandits are preparing to raid " + chosenLandmarkToRaid.landmarkName;
            //startState.SetDescription(startStateDesc);
            
            //action option states
            InteractionState doNothingState = new InteractionState("Do nothing", this); //raid
            InteractionState successfullyCancelledRaidState = new InteractionState("Successfully Cancelled Raid", this); //successfully cancelled raid
            InteractionState failedToCancelRaidState = new InteractionState("Failed to Cancel Raid", this); //failed to cancel raid
            InteractionState criticallyFailedToCancelRaidState = new InteractionState("Critically Failed to Cancel Raid", this); //critically failed to cancel raid
            InteractionState empoweredRaidState = new InteractionState("Empowered Raid", this); //empowered raid
            InteractionState misusedFundsState = new InteractionState("Misused Funds", this); //misused funds
            InteractionState demonDiesState = new InteractionState("Demon Dies", this); //demon dies

            CreateActionOptions(startState);
            //CreateActionOptions(raidState);
            //CreateActionOptions(successfullyCancelledRaidState);
            //CreateActionOptions(failedToCancelRaidState);
            //CreateActionOptions(criticallyFailedToCancelRaidState);
            //CreateActionOptions(empoweredRaidState);
            //CreateActionOptions(misusedFundsState);

            doNothingState.SetEndEffect(() => DoNothingRewardEffect(doNothingState));
            successfullyCancelledRaidState.SetEndEffect(() => SuccessfullyCancelledRaidRewardEffect(successfullyCancelledRaidState));
            failedToCancelRaidState.SetEndEffect(() => FailedToCancelRaidRewardEffect(failedToCancelRaidState));
            criticallyFailedToCancelRaidState.SetEndEffect(() => CriticalFailToCancelRaidRewardEffect(criticallyFailedToCancelRaidState));
            empoweredRaidState.SetEndEffect(() => EmpoweredRaidRewardEffect(empoweredRaidState));
            misusedFundsState.SetEndEffect(() => MisusedFundsRewardEffect(misusedFundsState));
            demonDiesState.SetEndEffect(() => DemonDiesRewardEffect(demonDiesState));

            _states.Add(startState.name, startState);
            _states.Add(doNothingState.name, doNothingState);
            _states.Add(successfullyCancelledRaidState.name, successfullyCancelledRaidState);
            _states.Add(failedToCancelRaidState.name, failedToCancelRaidState);
            _states.Add(criticallyFailedToCancelRaidState.name, criticallyFailedToCancelRaidState);
            _states.Add(empoweredRaidState.name, empoweredRaidState);
            _states.Add(misusedFundsState.name, misusedFundsState);
            _states.Add(demonDiesState.name, demonDiesState);

            SetCurrentState(startState);
        }
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption stopThemFromAttacking = new ActionOption {
                interactionState = state,
                cost = new ActionOptionCost { amount = 20, currency = CURRENCY.SUPPLY },
                name = "Stop them from attacking.",
                duration = 0,
                needsMinion = false,
                effect = () => StopThemFromAttackingEffect(state),
            };
            ActionOption provideSomeAssistance = new ActionOption {
                interactionState = state,
                cost = new ActionOptionCost { amount = 100, currency = CURRENCY.SUPPLY },
                name = "Provide them some assistance.",
                duration = 0,
                needsMinion = false,
                neededObjects = new List<System.Type>() { typeof(IUnit) },
                effect = () => ProvideThemSomeAssistanceEffect(state),
            };
            ActionOption doNothing = new ActionOption {
                interactionState = state,
                cost = new ActionOptionCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                duration = 0,
                needsMinion = false,
                effect = () => DoNothingEffect(state),
            };

            state.AddActionOption(stopThemFromAttacking);
            state.AddActionOption(provideSomeAssistance);
            state.AddActionOption(doNothing);
            state.SetDefaultOption(doNothing);
        }
    }
    #endregion

    private BaseLandmark GetLandmarkToRaid(BaseLandmark originLandmark) {
        List<HexTile> surrounding = originLandmark.tileLocation.GetTilesInRange(8);
        List<BaseLandmark> choices = new List<BaseLandmark>();
        for (int i = 0; i < surrounding.Count; i++) {
            HexTile currTile = surrounding[i];
            if (currTile.landmarkOnTile != null) {
                if (currTile.landmarkOnTile.owner == null || currTile.landmarkOnTile.owner.id != originLandmark.owner.id) {
                    //select a location within 8 tile distance around the camp owned by a different faction
                    choices.Add(currTile.landmarkOnTile);
                }
            }
        }
        if (choices.Count > 0) {
            return choices[Random.Range(0, choices.Count)];
        }
        return null;
    }

    private void StopThemFromAttackingEffect(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement("Successfully Cancelled Raid", 25);
        effectWeights.AddElement("Failed to Cancel Raid", 10);
        effectWeights.AddElement("Critically Failed to Cancel Raid", 5);

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[chosenEffect]);
        //if (chosenEffect == "Successfully Cancelled Raid") {
        //    SuccessfullyCancelledRaid(state, chosenEffect);
        //} else if (chosenEffect == "Failed to Cancel Raid") {
        //    FailedToCancelRaid(state, chosenEffect);
        //} else if (chosenEffect == "Critically Failed to Cancel Raid") {
        //    CriticalFailToCancelRaid(state, chosenEffect);
        //}
    }
    private void ProvideThemSomeAssistanceEffect(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement("Empowered Raid", 25);
        effectWeights.AddElement("Misused Funds", 10);
        effectWeights.AddElement("Demon Dies", 5);

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[chosenEffect]);
        //if (chosenEffect == "Empowered Raid") {
        //    EmpoweredRaid(state, chosenEffect);
        //} else if (chosenEffect == "Misused Funds") {
        //    MisusedFunds(state, chosenEffect);
        //} else if (chosenEffect == "Demon Dies") {
        //    DemonDies(state, chosenEffect);
        //}
    }
    private void DoNothingEffect(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement("Do nothing", 25);

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[chosenEffect]);
        //if (chosenEffect == "Do nothing") {
        //    DoNothing(state, chosenEffect);
        //}
    }

    private void SuccessfullyCancelledRaidRewardEffect(InteractionState state) {
        explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1)); //**Reward**: Demon gains Exp 1
        if (state.minionLog != null) {
            state.minionLog.AddToFillers(originLandmark.owner, originLandmark.owner.name, LOG_IDENTIFIER.FACTION_1);
            state.minionLog.AddToFillers(chosenLandmarkToRaid, chosenLandmarkToRaid.landmarkName, LOG_IDENTIFIER.LANDMARK_1);
        }
        if (state.landmarkLog != null) {
            state.minionLog.AddToFillers(originLandmark.owner, originLandmark.owner.name, LOG_IDENTIFIER.FACTION_1);
            state.minionLog.AddToFillers(chosenLandmarkToRaid, chosenLandmarkToRaid.landmarkName, LOG_IDENTIFIER.LANDMARK_1);
        }
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(originLandmark.owner, originLandmark.owner.name, LOG_IDENTIFIER.FACTION_1);
            state.descriptionLog.AddToFillers(chosenLandmarkToRaid, chosenLandmarkToRaid.landmarkName, LOG_IDENTIFIER.LANDMARK_1);
        }
    }
    private void FailedToCancelRaidRewardEffect(InteractionState state) {
        explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1)); //**Reward**: Demon gains Exp 1
        //**Mechanics**: combine characters into a single party of up to 4 units and send it to raid target
        Party createdParty = CombineCharacters(4);
        CharacterAction characterAction = ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.RAID_LANDMARK);
        createdParty.iactionData.AssignAction(characterAction, chosenLandmarkToRaid.landmarkObj);
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(originLandmark, originLandmark.landmarkName, LOG_IDENTIFIER.LANDMARK_1);
            state.descriptionLog.AddToFillers(chosenLandmarkToRaid, chosenLandmarkToRaid.landmarkName, LOG_IDENTIFIER.LANDMARK_2);
        }
        if (state.minionLog != null) {
            state.minionLog.AddToFillers(originLandmark.owner, originLandmark.owner.name, LOG_IDENTIFIER.FACTION_1);
            state.minionLog.AddToFillers(chosenLandmarkToRaid, chosenLandmarkToRaid.landmarkName, LOG_IDENTIFIER.LANDMARK_1);
        }
        if (state.landmarkLog != null) {
            state.landmarkLog.AddToFillers(originLandmark.owner, originLandmark.owner.name, LOG_IDENTIFIER.FACTION_1);
            state.landmarkLog.AddToFillers(chosenLandmarkToRaid, chosenLandmarkToRaid.landmarkName, LOG_IDENTIFIER.LANDMARK_1);
        }
    }
    private void CriticalFailToCancelRaidRewardEffect(InteractionState state) {
        explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1)); //**Reward**: Demon gains Exp 1
        //**Mechanics**: combine characters into a single party of up to 4 units and send it to raid target
        Party createdParty = CombineCharacters(4);
        CharacterAction characterAction = ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.RAID_LANDMARK);
        createdParty.iactionData.AssignAction(characterAction, chosenLandmarkToRaid.landmarkObj);
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(originLandmark, originLandmark.landmarkName, LOG_IDENTIFIER.LANDMARK_1);
            state.descriptionLog.AddToFillers(chosenLandmarkToRaid, chosenLandmarkToRaid.landmarkName, LOG_IDENTIFIER.LANDMARK_2);
        }
        if (state.minionLog != null) {
            state.minionLog.AddToFillers(originLandmark.owner, originLandmark.owner.name, LOG_IDENTIFIER.FACTION_1);
        }
        if (state.landmarkLog != null) {
            state.landmarkLog.AddToFillers(originLandmark.owner, originLandmark.owner.name, LOG_IDENTIFIER.FACTION_1);
        }
    }

    private void EmpoweredRaidRewardEffect(InteractionState state) {
        explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1)); //**Reward**: Demon gains Exp 1
        //**Mechanics**: combine characters into a single party of up to 4 units and send it to raid target, all raiding units gain "Empowered" buff
        Party createdParty = CombineCharacters(4);
        CharacterAction characterAction = ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.RAID_LANDMARK);
        createdParty.iactionData.AssignAction(characterAction, chosenLandmarkToRaid.landmarkObj);
        CombatAttribute empoweredTrait = AttributeManager.Instance.allCombatAttributes["Empowered"];
        for (int i = 0; i < createdParty.icharacters.Count; i++) {
            createdParty.icharacters[i].AddCombatAttribute(empoweredTrait);
        }
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(originLandmark, originLandmark.landmarkName, LOG_IDENTIFIER.LANDMARK_1);
            state.descriptionLog.AddToFillers(chosenLandmarkToRaid, chosenLandmarkToRaid.landmarkName, LOG_IDENTIFIER.LANDMARK_2);
        }
        if (state.minionLog != null) {
            state.minionLog.AddToFillers(originLandmark.owner, originLandmark.owner.name, LOG_IDENTIFIER.FACTION_1);
            state.minionLog.AddToFillers(chosenLandmarkToRaid, chosenLandmarkToRaid.landmarkName, LOG_IDENTIFIER.LANDMARK_1);
        }
        if (state.landmarkLog != null) {
            state.landmarkLog.AddToFillers(originLandmark.owner, originLandmark.owner.name, LOG_IDENTIFIER.FACTION_1);
            state.landmarkLog.AddToFillers(chosenLandmarkToRaid, chosenLandmarkToRaid.landmarkName, LOG_IDENTIFIER.LANDMARK_1);
        }
    }
    private void MisusedFundsRewardEffect(InteractionState state) {
        explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1)); //**Reward**: Demon gains Exp 1
        //**Mechanics**: combine characters into a single party of up to 4 units and send it to raid target, add 100 Supply to Bandit Camp
        Party createdParty = CombineCharacters(4);
        CharacterAction characterAction = ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.RAID_LANDMARK);
        createdParty.iactionData.AssignAction(characterAction, chosenLandmarkToRaid.landmarkObj);
        originLandmark.tileLocation.areaOfTile.AdjustSuppliesInBank(100);
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(originLandmark, originLandmark.landmarkName, LOG_IDENTIFIER.LANDMARK_1);
            state.descriptionLog.AddToFillers(chosenLandmarkToRaid, chosenLandmarkToRaid.landmarkName, LOG_IDENTIFIER.LANDMARK_2);
        }
        if (state.minionLog != null) {
            state.minionLog.AddToFillers(originLandmark.owner, originLandmark.owner.name, LOG_IDENTIFIER.FACTION_1);
        }
        if (state.landmarkLog != null) {
            state.landmarkLog.AddToFillers(originLandmark.owner, originLandmark.owner.name, LOG_IDENTIFIER.FACTION_1);
            state.landmarkLog.AddToFillers(chosenLandmarkToRaid, chosenLandmarkToRaid.landmarkName, LOG_IDENTIFIER.LANDMARK_1);
        }
    }
    private void DemonDiesRewardEffect(InteractionState state) {
        //**Effect**: Demon is removed from Minion List
        PlayerManager.Instance.player.RemoveMinion(explorerMinion);
        //**Mechanics**: combine characters into a single party of up to 4 units and send it to raid target
        Party createdParty = CombineCharacters(4);
        CharacterAction characterAction = ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.RAID_LANDMARK);
        createdParty.iactionData.AssignAction(characterAction, chosenLandmarkToRaid.landmarkObj);
        if (state.landmarkLog != null) {
            state.landmarkLog.AddToFillers(originLandmark.owner, originLandmark.owner.name, LOG_IDENTIFIER.FACTION_1);
            state.landmarkLog.AddToFillers(chosenLandmarkToRaid, chosenLandmarkToRaid.landmarkName, LOG_IDENTIFIER.LANDMARK_1);
        }
    }
    private void DoNothingRewardEffect(InteractionState state) {
        //**Mechanics**: combine characters into a single party of up to 4 units and send it to raid target
        Party createdParty = CombineCharacters(4);
        CharacterAction characterAction = ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.RAID_LANDMARK);
        createdParty.iactionData.AssignAction(characterAction, chosenLandmarkToRaid.landmarkObj);
        if (state.minionLog != null) {
            state.minionLog.AddToFillers(originLandmark.owner, originLandmark.owner.name, LOG_IDENTIFIER.FACTION_1);
            state.minionLog.AddToFillers(chosenLandmarkToRaid, chosenLandmarkToRaid.landmarkName, LOG_IDENTIFIER.LANDMARK_1);
        }
        if (state.landmarkLog != null) {
            state.landmarkLog.AddToFillers(originLandmark.owner, originLandmark.owner.name, LOG_IDENTIFIER.FACTION_1);
            state.landmarkLog.AddToFillers(chosenLandmarkToRaid, chosenLandmarkToRaid.landmarkName, LOG_IDENTIFIER.LANDMARK_1);
        }
    }

    private Party CombineCharacters(int upTo) {
        Party partyToUse = null;
        for (int i = 0; i < originLandmark.charactersWithHomeOnLandmark.Count; i++) {
            ICharacter currCharacter = originLandmark.charactersWithHomeOnLandmark[i];
            if (currCharacter is Character && (currCharacter as Character).isDefender) {
                continue; //skip characters that are defending
            }
            Party currCharacterParty = currCharacter.ownParty;
            if (currCharacterParty == null ||
                (currCharacterParty != null && currCharacterParty.icharacters.Count > partyToUse.icharacters.Count)) {
                partyToUse = currCharacterParty;
            }
        }
        if (partyToUse != null) {
            if (partyToUse.icharacters.Count < upTo) {
                for (int i = 0; i < originLandmark.charactersWithHomeOnLandmark.Count; i++) {
                    ICharacter currCharacter = originLandmark.charactersWithHomeOnLandmark[i];
                    if (currCharacter is Character && (currCharacter as Character).isDefender) {
                        continue; //skip characters that are defending
                    }
                    if (partyToUse.owner.id != currCharacter.id && !currCharacter.IsInParty()) { //the current character is not the owner of the party
                        partyToUse.AddCharacter(currCharacter);
                        if (partyToUse.icharacters.Count >= upTo) {
                            break;
                        }
                    }
                }
            }
        }

        return partyToUse;
    }
}
