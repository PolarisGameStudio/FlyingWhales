﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;

public class CounterattackParty : Party {

    public LocationStructure targetStructure { get; private set; }
    public HexTile waitingArea { get; private set; }

    #region getters
    public override IPartyTarget target => targetStructure;
    public override HexTile waitingHexArea => waitingArea;
    #endregion

    public CounterattackParty() : base(PARTY_TYPE.Counterattack) {
        minimumPartySize = 3;
        waitTimeInTicks = GameManager.Instance.GetTicksBasedOnHour(1) + GameManager.Instance.GetTicksBasedOnMinutes(30);
        relatedBehaviour = typeof(AttackDemonicStructureBehaviour);
        jobQueueOwnerType = JOB_OWNER.FACTION;
    }

    #region Overrides
    public override bool IsAllowedToJoin(Character character) {
        return (character.characterClass.IsCombatant() && character.characterClass.identifier == "Normal") || character.characterClass.className == "Noble";
    }
    protected override void OnWaitTimeOver() {
        base.OnWaitTimeOver();
        // Messenger.Broadcast(Signals.START_THREAT_EFFECT);
        //for (int i = 0; i < members.Count; i++) {
        //    Character member = members[i];
        //    member.traitContainer.AddTrait(member, "Travelling");
        //}
        Messenger.Broadcast(Signals.CHARACTERS_ATTACKING_DEMONIC_STRUCTURE, members, targetStructure);
    }
    // protected override void OnWaitTimeOverButPartyIsDisbanded() {
    //     base.OnWaitTimeOverButPartyIsDisbanded();
        // PlayerManager.Instance.player.threatComponent.DivineIntervention();
        // Messenger.Broadcast(Signals.START_THREAT_EFFECT);
    // }
    protected override void OnAddMember(Character member) {
        base.OnAddMember(member);
        member.movementComponent.SetEnableDigging(true);
        member.traitContainer.AddTrait(member, "Fervor");
        member.traitContainer.AddTrait(member, "Travelling");
    }
    protected override void OnRemoveMember(Character member) {
        base.OnRemoveMember(member);
        member.movementComponent.SetEnableDigging(false);
        member.traitContainer.RemoveTrait(member, "Fervor");
        member.traitContainer.RemoveTrait(member, "Travelling");
    }
    #endregion

    #region General
    public void SetTargetStructure(LocationStructure structure) {
        if(targetStructure != structure) {
            targetStructure = structure;
            if (targetStructure != null) {
                SetWaitingArea();
            }
        }
    }
    private void SetWaitingArea() {
        waitingArea = targetStructure.settlementLocation.GetAPlainAdjacentHextile();
    }
    #endregion
}
