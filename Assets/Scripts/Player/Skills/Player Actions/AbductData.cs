﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbductData : PlayerAction {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.ABDUCT;
    public override string name => "Abduct";
    public override string description => "This Action can be used to summon a Demon or Minion to Abduct a Resident.";
    public AbductData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }
}
