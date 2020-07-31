﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemonWrathData : MinionPlayerSkill {
    public override SPELL_TYPE type => SPELL_TYPE.DEMON_WRATH;
    public override string name => "Wrath Demon";
    public override string description => "This Demon is a powerful melee combatant that deals Normal damage. Can be summoned to Defend, Harass or Invade a target area or Assault a target character.";
    public DemonWrathData() {
        className = "Wrath";
    }
}
