﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecruitAction : Interaction {

    public Character targetCharacter;

    private const string Disrupted_Recruitment_Success = "Disrupted Recruitment Success";
    private const string Disrupted_Recruitment_Fail = "Disrupted Recruitment Fail";
    private const string Assisted_Recruitment_Success = "Assisted Recruitment Success";
    private const string Assisted_Recruitment_Fail = "Assisted Recruitment Fail";
    private const string Normal_Recruitment_Success = "Normal Recruitment Success";
    private const string Normal_Recruitment_Fail = "Normal Recruitment Fail";

    public RecruitAction(BaseLandmark interactable) 
        : base(interactable, INTERACTION_TYPE.RECRUIT_ACTION, 0) {
        _name = "Recruit Action";
        _jobFilter = new JOB[] { JOB.INSTIGATOR, JOB.DIPLOMAT };
    }

    #region Override
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState disruptedRecruitmentSuccess = new InteractionState(Disrupted_Recruitment_Success, this);
        InteractionState disruptedRecruitmentFail = new InteractionState(Disrupted_Recruitment_Fail, this);
        InteractionState assistedRecruitmentSuccess = new InteractionState(Assisted_Recruitment_Success, this);
        InteractionState assistedRecruitmentFail = new InteractionState(Assisted_Recruitment_Fail, this);
        InteractionState normalRecruitmenSuccess = new InteractionState(Normal_Recruitment_Success, this);
        InteractionState normalRecruitmentFail = new InteractionState(Normal_Recruitment_Fail, this);

        if (targetCharacter == null) {
            SetTargetCharacter(GetTargetCharacter(_characterInvolved));
        }

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description");
        startStateDescriptionLog.AddToFillers(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1);
        startStateDescriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);
        disruptedRecruitmentSuccess.SetEffect(() => DisruptedRecruitmentSuccessRewardEffect(disruptedRecruitmentSuccess));
        disruptedRecruitmentFail.SetEffect(() => DisruptedRecruitmentFailRewardEffect(disruptedRecruitmentFail));
        assistedRecruitmentSuccess.SetEffect(() => AssistedRecruitmentSuccessRewardEffect(assistedRecruitmentSuccess));
        assistedRecruitmentFail.SetEffect(() => AssistedRecruitmentFailRewardEffect(assistedRecruitmentFail));
        normalRecruitmenSuccess.SetEffect(() => NormalRecruitmentSuccessRewardEffect(normalRecruitmenSuccess));
        normalRecruitmentFail.SetEffect(() => NormalRecruitmentFailRewardEffect(normalRecruitmentFail));

        _states.Add(startState.name, startState);
        _states.Add(disruptedRecruitmentSuccess.name, disruptedRecruitmentSuccess);
        _states.Add(disruptedRecruitmentFail.name, disruptedRecruitmentFail);
        _states.Add(assistedRecruitmentSuccess.name, assistedRecruitmentSuccess);
        _states.Add(assistedRecruitmentFail.name, assistedRecruitmentFail);
        _states.Add(normalRecruitmenSuccess.name, normalRecruitmenSuccess);
        _states.Add(normalRecruitmentFail.name, normalRecruitmentFail);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption disrupt = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Disrupt the meeting.",
                effect = () => DisruptOptionEffect(state),
                jobNeeded = JOB.INSTIGATOR,
                doesNotMeetRequirementsStr = "Minion must be an instigator",
            };
            ActionOption assist = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Assist with the recruitment.",
                effect = () => AssistOptionEffect(state),
                jobNeeded = JOB.DIPLOMAT,
                doesNotMeetRequirementsStr = "Minion must be a diplomat",
            };
            ActionOption doNothing = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                effect = () => DoNothingOptionEffect(state),
            };
            state.AddActionOption(disrupt);
            state.AddActionOption(assist);
            state.AddActionOption(doNothing);
            state.SetDefaultOption(doNothing);
        }
    }
    #endregion

    #region Option Effect
    private void DisruptOptionEffect(InteractionState state) {
        WeightedDictionary<RESULT> resultWeights = _characterInvolved.job.GetJobRateWeights();
        resultWeights.RemoveElement(RESULT.CRITICAL_FAIL);
        resultWeights.AddWeightToElement(RESULT.FAIL, 30);

        string nextState = string.Empty;
        switch (resultWeights.PickRandomElementGivenWeights()) {
            case RESULT.SUCCESS:
                nextState = Disrupted_Recruitment_Success;
                break;
            case RESULT.FAIL:
                nextState = Disrupted_Recruitment_Fail;
                break;
        }
        SetCurrentState(_states[nextState]);
    }
    private void AssistOptionEffect(InteractionState state) {
        WeightedDictionary<RESULT> resultWeights = _characterInvolved.job.GetJobRateWeights();
        resultWeights.RemoveElement(RESULT.CRITICAL_FAIL);
        resultWeights.AddWeightToElement(RESULT.SUCCESS, 30);

        string nextState = string.Empty;
        switch (resultWeights.PickRandomElementGivenWeights()) {
            case RESULT.SUCCESS:
                nextState = Assisted_Recruitment_Success;
                break;
            case RESULT.FAIL:
                nextState = Assisted_Recruitment_Fail;
                break;
        }
        SetCurrentState(_states[nextState]);
    }
    private void DoNothingOptionEffect(InteractionState state) {
        WeightedDictionary<RESULT> resultWeights = _characterInvolved.job.GetJobRateWeights();
        resultWeights.RemoveElement(RESULT.CRITICAL_FAIL);

        string nextState = string.Empty;
        switch (resultWeights.PickRandomElementGivenWeights()) {
            case RESULT.SUCCESS:
                nextState = Normal_Recruitment_Success;
                break;
            case RESULT.FAIL:
                nextState = Normal_Recruitment_Fail;
                break;
        }
        SetCurrentState(_states[nextState]);
    }
    #endregion

    #region Reward Effect
    private void DisruptedRecruitmentSuccessRewardEffect(InteractionState state) {
        /* Mechanics**: Transfer Character 2 to Character 1's Faction. 
         * Change its home to be the same as Character 1's home Area. 
         * Override his next action as https://trello.com/c/PTkSE6DZ/439-character-move-to-return-home
         */
        TransferCharacter(targetCharacter, _characterInvolved.faction);
        //**Level Up**: Recruiting Character +1
        _characterInvolved.LevelUp();
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            state.descriptionLog.AddToFillers(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1);
        }
        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        state.AddLogFiller(new LogFiller(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1));
    }
    private void DisruptedRecruitmentFailRewardEffect(InteractionState state) {
        //**Level Up**: Instigator Minion +1
        investigatorMinion.LevelUp();
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            state.descriptionLog.AddToFillers(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1);
        }
        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        state.AddLogFiller(new LogFiller(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1));
    }
    private void AssistedRecruitmentSuccessRewardEffect(InteractionState state) {
        /* Mechanics**: Transfer Character 2 to Character 1's Faction. 
         * Change its home to be the same as Character 1's home Area. 
         * Override his next action as https://trello.com/c/PTkSE6DZ/439-character-move-to-return-home
         */
        TransferCharacter(targetCharacter, _characterInvolved.faction);
        //**Level Up**: Diplomat +1, Recruiting Character +1
        _characterInvolved.LevelUp();
        investigatorMinion.LevelUp();

        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            state.descriptionLog.AddToFillers(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1);
        }
        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        state.AddLogFiller(new LogFiller(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1));
    }
    private void AssistedRecruitmentFailRewardEffect(InteractionState state) {
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            state.descriptionLog.AddToFillers(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1);
        }
        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        state.AddLogFiller(new LogFiller(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1));
    }
    private void NormalRecruitmentSuccessRewardEffect(InteractionState state) {
        /* Mechanics**: Transfer Character 2 to Character 1's Faction. 
         * Change its home to be the same as Character 1's home Area. 
         * Override his next action as https://trello.com/c/PTkSE6DZ/439-character-move-to-return-home
         */
        TransferCharacter(targetCharacter, _characterInvolved.faction);
        //**Level Up**: Recruiting Character +1
        _characterInvolved.LevelUp();

        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            state.descriptionLog.AddToFillers(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1);
        }
        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        state.AddLogFiller(new LogFiller(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1));
    }
    private void NormalRecruitmentFailRewardEffect(InteractionState state) {
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        }
        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    #endregion

    private void TransferCharacter(Character character, Faction faction) {
        character.faction.RemoveCharacter(character);
        faction.AddNewCharacter(character);
        character.homeLandmark.RemoveCharacterHomeOnLandmark(character);
        _characterInvolved.homeLandmark.AddCharacterHomeOnLandmark(character);
        Interaction interaction = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.MOVE_TO_RETURN_HOME, character.specificLocation.tileLocation.landmarkOnTile);
        character.SetForcedInteraction(interaction);
    }

    public void SetTargetCharacter(Character targetCharacter) {
        this.targetCharacter = targetCharacter;
    }
    public Character GetTargetCharacter(Character characterInvolved) {
        WeightedDictionary<Character> characterWeights = new WeightedDictionary<Character>();
        for (int i = 0; i < interactable.tileLocation.areaOfTile.charactersAtLocation.Count; i++) {
            Character currCharacter = interactable.tileLocation.areaOfTile.charactersAtLocation[i];
            if (currCharacter.id != characterInvolved.id 
                && !currCharacter.isLeader 
                && !currCharacter.isDefender 
                && currCharacter.minion == null
                && currCharacter.faction.id != characterInvolved.faction.id) { //- character must not be in Defender Tile.
                int weight = 0;
                if (currCharacter.isFactionless) {
                    weight += 35; //- character is not part of any Faction: Weight +35
                } else if (currCharacter.faction.id != characterInvolved.faction.id) { //exclude characters with same faction
                    FactionRelationship rel = currCharacter.faction.GetRelationshipWith(characterInvolved.faction);
                    if (rel.relationshipStatus == FACTION_RELATIONSHIP_STATUS.NEUTRAL) {
                        weight += 15; //- character is part of a Faction with Neutral relationship with recruiter's Faction: Weight +15
                    } else if (rel.relationshipStatus == FACTION_RELATIONSHIP_STATUS.FRIEND) {
                        weight += 25; //- character is part of a Faction with Friend relationship with recruiter's Faction: Weight +25
                    }
                }

                if (currCharacter.level > characterInvolved.level) {
                    weight -= 30; //- character is higher level than Recruiter: Weight -30
                } else if (currCharacter.level < characterInvolved.level) { //- character is same level as Recruiter: Weight +0
                    weight += 10; //- character is lower level than Recruiter: Weight +10
                }

                weight = Mathf.Max(0, weight);
                characterWeights.AddElement(currCharacter, weight);
            }
        }
        if (characterWeights.GetTotalOfWeights() > 0) {
            return characterWeights.PickRandomElementGivenWeights();
        }
        return null;
        //throw new System.Exception("Could not find any character to recruit!");
    }
}
