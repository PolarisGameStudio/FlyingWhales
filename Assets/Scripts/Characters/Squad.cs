﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Squad {

    public int id { get; private set; }
    public string name { get; private set; }
    public ICharacter squadLeader { get; private set; }
    public List<ICharacter> squadMembers { get; private set; } //all members + leader

    #region getters/setters
    public List<ICharacter> squadFollowers {
        get {
            List<ICharacter> followers = new List<ICharacter>(squadMembers);
            followers.Remove(squadLeader);
            return followers;
        }
    }
    public float potentialPower {
        get { return squadMembers.Sum(x => x.computedPower); }
    }
    #endregion

    public Squad() {
        id = Utilities.SetID(this);
        SetName("Squad " + id.ToString());
        squadMembers = new List<ICharacter>();
    }

    public Squad(SquadSaveData data) {
        id = Utilities.SetID(this, data.squadID);
        SetName(data.squadName);
        squadMembers = new List<ICharacter>();
    }

    #region Misc
    public void SetName(string name) {
        this.name = name;
    }
    #endregion

    #region Leader
    public void SetLeader(ICharacter leader) {
        squadLeader = leader;
        Messenger.Broadcast(Signals.SQUAD_LEADER_SET, leader, this);
        AddMember(leader);
    }
    #endregion

    #region Members
    public void AddMember(ICharacter member) {
        if (!squadMembers.Contains(member)) {
            squadMembers.Add(member);
            member.SetSquad(this);
            Messenger.Broadcast(Signals.SQUAD_MEMBER_ADDED, member, this);
        }
    }
    public void RemoveMember(ICharacter member) {
        if (squadMembers.Remove(member)) {
            Messenger.Broadcast(Signals.SQUAD_MEMBER_REMOVED, member, this);
            member.SetSquad(null);
        }
    }
    #endregion

    #region Squad Management
    public void Disband() {
        while (squadMembers.Count != 0) {
            RemoveMember(squadMembers[0]);
        }
    }
    #endregion

    #region Quests
    public List<Quest> GetSquadQuests() {
        List<Quest> quests = new List<Quest>();
        for (int i = 0; i < squadMembers.Count; i++) {
            ICharacter currMember = squadMembers[i];
            if (currMember is ECS.Character) {
                ECS.Character character = currMember as ECS.Character;
                for (int j = 0; j < character.questData.Count; j++) {
                    CharacterQuestData currData = character.questData[j];
                    if (currData.parentQuest.groupType == GROUP_TYPE.PARTY && !quests.Contains(currData.parentQuest)) {
                        quests.Add(currData.parentQuest);
                    }
                }
            }
        }
        return quests;
    }
    public List<CharacterQuestData> GetSquadQuestData() {
        List<CharacterQuestData> questData = new List<CharacterQuestData>();
        for (int i = 0; i < squadMembers.Count; i++) {
            ICharacter currMember = squadMembers[i];
            if (currMember is ECS.Character) {
                ECS.Character character = currMember as ECS.Character;
                for (int j = 0; j < character.questData.Count; j++) {
                    CharacterQuestData currData = character.questData[j];
                    if (currData.parentQuest.groupType == GROUP_TYPE.PARTY && !questData.Contains(currData)) {
                        questData.Add(currData);
                    }
                }
            }
        }
        return questData;
    }
    #endregion

}
