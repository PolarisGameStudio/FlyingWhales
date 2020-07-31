﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemonPrideData : MinionPlayerSkill {
    public override SPELL_TYPE type => SPELL_TYPE.DEMON_PRIDE;
    public override string name => "Pride Demon";
    public override string description => "This Demon is a strong ranged magic-user that deals Electric damage. Can be summoned to Defend, Harass or Invade a target area or Assault a target character.";
    public DemonPrideData() {
        className = "Pride";
    }
}
