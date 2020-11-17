﻿using System;
using Traits;
namespace Plague.Fatality {
    public abstract class Fatality : Plagued.IPlaguedListener {

        public abstract PLAGUE_FATALITY fatalityType { get; }
        
        protected abstract void ActivateFatality(Character p_character);
        
        #region Plagued.IPlaguedListener Implementation
        public virtual void PerTickMovement(Character p_character) { }
        public virtual void CharacterGainedTrait(Character p_character, Trait p_gainedTrait) { }
        public virtual bool CharacterStartedPerformingAction(Character p_character, ActualGoapNode p_action) { return true; }
        public virtual void CharacterDonePerformingAction(Character p_character, ActualGoapNode p_actionPerformed) { }
        public virtual void HourStarted(Character p_character, int p_numOfHoursPassed) { }
        #endregion
    }

    public static class FatalityExtensions{
        public static int GetFatalityCost(this PLAGUE_FATALITY fatality) {
            switch (fatality) {
                case PLAGUE_FATALITY.Septic_Shock:
                    return 30;
                case PLAGUE_FATALITY.Heart_Attack:
                    return 30;
                case PLAGUE_FATALITY.Stroke:
                    return 30;
                case PLAGUE_FATALITY.Total_Organ_Failure:
                    return 30;
                case PLAGUE_FATALITY.Pneumonia:
                    return 30;
                default:
                    throw new ArgumentOutOfRangeException(nameof(fatality), fatality, null);
            }
        }
        
        public static string GetFatalityTooltip(this PLAGUE_FATALITY fatality) {
            switch (fatality) {
                case PLAGUE_FATALITY.Septic_Shock:
                    return "Plagued Villagers have a low risk of succumbing to Septic Shock each time they become Starving.";
                case PLAGUE_FATALITY.Heart_Attack:
                    return "Plagued Villagers have a low risk of having a Heart Attack while their Stamina is Low.";
                case PLAGUE_FATALITY.Stroke:
                    return "Plagued Villagers have a low risk of having a Stroke each time they become Exhausted.";
                case PLAGUE_FATALITY.Total_Organ_Failure:
                    return "Plagued Villagers have a very low risk of Total Organ Failure each time they perform an action.";
                case PLAGUE_FATALITY.Pneumonia:
                    return "Plagued Villagers have a very low risk of succumbing to Pneumonia while moving.";
                default:
                    throw new ArgumentOutOfRangeException(nameof(fatality), fatality, null);
            }
        }
    }
}