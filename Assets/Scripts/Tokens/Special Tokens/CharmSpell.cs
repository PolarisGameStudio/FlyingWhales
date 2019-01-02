﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharmSpell : SpecialToken {

    public CharmSpell() : base(SPECIAL_TOKEN.CHARM_SPELL) {
        quantity = 4;
        weight = 50;
        npcAssociatedInteractionType = INTERACTION_TYPE.USE_ITEM_ON_CHARACTER;
    }

    #region Overrides
    public override void CreateJointInteractionStates(Interaction interaction, Character user, object target) {
        TokenInteractionState itemUsedState = new TokenInteractionState(Item_Used, interaction, this);
        TokenInteractionState stopFailState = new TokenInteractionState(Stop_Fail, interaction, this);
        itemUsedState.SetTokenUserAndTarget(user, target);
        stopFailState.SetTokenUserAndTarget(user, target);

        itemUsedState.SetEffect(() => ItemUsedEffect(itemUsedState));
        stopFailState.SetEffect(() => StopFailEffect(stopFailState));

        interaction.AddState(itemUsedState);
        interaction.AddState(stopFailState);
        //interaction.SetCurrentState(itemUsed);
    }
    public override Character GetTargetCharacterFor(Character sourceCharacter) {
        if (!sourceCharacter.isFactionless) {
            Area location = sourceCharacter.ownParty.specificLocation.tileLocation.areaOfTile;
            List<Character> choices = new List<Character>();
            for (int i = 0; i < location.charactersAtLocation.Count; i++) {
                Character currCharacter = location.charactersAtLocation[i];
                if (currCharacter.id != sourceCharacter.id
                    && (currCharacter.isFactionless || currCharacter.faction.id != sourceCharacter.faction.id)
                    && !currCharacter.isLeader) {
                    choices.Add(currCharacter);
                }
            }
            if (choices.Count > 0) {
                return choices[Random.Range(0, choices.Count)];
            }
        }
        return base.GetTargetCharacterFor(sourceCharacter);
    }
    public override bool CanBeUsedBy(Character sourceCharacter) {
        if (!sourceCharacter.isFactionless) {
            if (sourceCharacter.homeLandmark.tileLocation.areaOfTile.IsResidentsFull()) {
                return false; //resident capacity is already full, do not use charm spell
            }
            Area location = sourceCharacter.ownParty.specificLocation.tileLocation.areaOfTile;
            for (int i = 0; i < location.charactersAtLocation.Count; i++) {
                Character currCharacter = location.charactersAtLocation[i];
                if (currCharacter.id != sourceCharacter.id 
                    && (currCharacter.isFactionless || currCharacter.faction.id != sourceCharacter.faction.id) 
                    && !currCharacter.isLeader) {
                    return true;
                }
            }
        }
        return false;
    }
    #endregion

    private void ItemUsedEffect(TokenInteractionState state) {
        state.tokenUser.LevelUp();

        //**Mechanics**: Target character will transfer to character or player's faction
        if (state.target is Character) {
            Character target = state.target as Character;
            FactionManager.Instance.TransferCharacter(target, state.tokenUser.faction, state.tokenUser.homeLandmark);
        }

        state.descriptionLog.AddToFillers(state.tokenUser.faction, state.tokenUser.faction.name, LOG_IDENTIFIER.FACTION_1);
        state.AddLogFiller(new LogFiller(state.tokenUser.faction, state.tokenUser.faction.name, LOG_IDENTIFIER.FACTION_1));
    }
    private void StopFailEffect(TokenInteractionState state) {
        state.tokenUser.LevelUp();

        //**Mechanics**: Target character will transfer to character or player's faction
        if (state.target is Character) {
            Character target = state.target as Character;
            FactionManager.Instance.TransferCharacter(target, state.tokenUser.faction, state.tokenUser.homeLandmark);
        }

        state.descriptionLog.AddToFillers(state.tokenUser.faction, state.tokenUser.faction.name, LOG_IDENTIFIER.FACTION_1);
        state.descriptionLog.AddToFillers(state.interaction.investigatorMinion, state.interaction.investigatorMinion.name, LOG_IDENTIFIER.MINION_1);
        state.descriptionLog.AddToFillers(null, this.name, LOG_IDENTIFIER.ITEM_1);

        state.AddLogFiller(new LogFiller(state.tokenUser.faction, state.tokenUser.faction.name, LOG_IDENTIFIER.FACTION_1));
        state.AddLogFiller(new LogFiller(state.interaction.investigatorMinion, state.interaction.investigatorMinion.name, LOG_IDENTIFIER.MINION_1));
        state.AddLogFiller(new LogFiller(null, this.name, LOG_IDENTIFIER.ITEM_1));
    }
}
