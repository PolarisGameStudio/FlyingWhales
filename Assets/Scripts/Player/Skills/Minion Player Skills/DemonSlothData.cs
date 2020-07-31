﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemonSlothData : MinionPlayerSkill {
    public override SPELL_TYPE type => SPELL_TYPE.DEMON_SLOTH;
    public override string name => "Sloth Demon";
    public override string description => "This Demon is a tough melee magic-user that deals Ice damage. Can be summoned to defend an Area or Structure.";

    public DemonSlothData() {
        className = "Sloth";
    }
}
