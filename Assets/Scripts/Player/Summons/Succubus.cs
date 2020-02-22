﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Succubus : SeducerSummon {

    public const string ClassName = "Succubus";
    
    public Succubus() : base(SUMMON_TYPE.Succubus, GENDER.FEMALE){
        combatComponent.SetElementalDamage(ELEMENTAL_TYPE.Ice);
    }
    public Succubus(SaveDataCharacter data) : base(data) {
        combatComponent.SetElementalDamage(ELEMENTAL_TYPE.Ice);
    }
    
    public override string GetClassForRole(CharacterRole role) {
        return ClassName;
    }
}