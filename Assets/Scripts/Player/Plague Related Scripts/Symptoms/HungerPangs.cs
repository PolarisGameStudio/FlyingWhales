﻿using Traits;
using UnityEngine;
using UtilityScripts;

namespace Plague.Symptom {
    public class HungerPangs : PlagueSymptom {
        
        public override PLAGUE_SYMPTOM symptomType => PLAGUE_SYMPTOM.Hunger_Pangs;

        protected override void ActivateSymptom(Character p_character) {
            p_character.needsComponent.AdjustFullness(-10);
            Debug.Log("Activated Hunger Pangs Symptom");
        }
        public override void PerTickMovement(Character p_character) {
            if (GameUtilities.RollChance(2)) {
                ActivateSymptom(p_character);
            }
        }
    }
}