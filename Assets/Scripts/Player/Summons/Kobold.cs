﻿using Inner_Maps;
using Traits;

public class Kobold : Summon {

    public const string ClassName = "Kobold";
    
    public override string raceClassName => "Kobold";
    
    public Kobold() : base(SUMMON_TYPE.Kobold, CharacterRole.SOLDIER, ClassName, RACE.KOBOLD,
        UtilityScripts.Utilities.GetRandomGender()) {
		combatComponent.SetElementalDamage(ELEMENTAL_TYPE.Ice);
    }
    public Kobold(SaveDataCharacter data) : base(data) {
        combatComponent.SetElementalDamage(ELEMENTAL_TYPE.Ice);
    }
    public override void Initialize() {
        base.Initialize();
        traitContainer.AddTrait(this, "Cold Blooded");
    }
}
