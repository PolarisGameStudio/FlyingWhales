﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealData : PlayerAction {
    public override SPELL_TYPE type => SPELL_TYPE.HEAL;
    public override string name { get { return "Heal"; } }
    public override string description { get { return "Heal"; } }

    public HealData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }
}