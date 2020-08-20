﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveDataPlayerSkill : SaveData<SpellData> {
    public SPELL_TYPE type;
    public int maxCharges;
    public int charges;
    public int manaCost;
    public int cooldown;
    public int threat;
    public int threatPerHour;

    public int currentCooldownTick;

    #region Overrides
    public override void Save(SpellData data) {
        type = data.type;
        maxCharges = data.maxCharges;
        charges = data.charges;
        manaCost = data.manaCost;
        cooldown = data.cooldown;
        threat = data.threat;
        threatPerHour = data.threatPerHour;
        currentCooldownTick = data.currentCooldownTick;
    }
    public override SpellData Load() {
        SpellData data = PlayerSkillManager.Instance.GetPlayerSpellData(type);
        data.SetCharges(charges);
        data.SetCooldown(cooldown);
        data.SetManaCost(manaCost);
        data.SetThreat(threat);
        data.SetThreatPerHour(threatPerHour);

        data.SetCurrentCooldownTick(currentCooldownTick);
        data.SetMaxCharges(maxCharges);
        return data;
    }
    #endregion
}