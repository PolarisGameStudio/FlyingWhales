﻿using Inner_Maps;
using Traits;
using System.Collections.Generic;

public class MagicalAngel : Summon {
    public override string raceClassName => $"Magical Angel";
    
    public MagicalAngel() : base(SUMMON_TYPE.Magical_Angel, "Magical Angel", RACE.ANGEL,
        UtilityScripts.Utilities.GetRandomGender()) {
    }
    public MagicalAngel(string className) : base(SUMMON_TYPE.Magical_Angel, className, RACE.ANGEL,
        UtilityScripts.Utilities.GetRandomGender()) {
    }

    #region Overrides
    public override void ConstructDefaultActions() {
        if (actions == null) {
            actions = new List<SPELL_TYPE>();
        } else {
            actions.Clear();
        }
        AddPlayerAction(SPELL_TYPE.ZAP);
        AddPlayerAction(SPELL_TYPE.SEIZE_CHARACTER);
    }
    #endregion  
}