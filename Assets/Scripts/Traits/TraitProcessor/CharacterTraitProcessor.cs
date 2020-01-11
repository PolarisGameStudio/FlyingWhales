﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    /// <summary>
    /// Functions to be used to determine what happens when a trait is added/removed to a character
    /// </summary>
    public class CharacterTraitProcessor : TraitProcessor {
        public override void OnTraitStacked(ITraitable traitable, Trait trait, Character characterResponsible = null, ActualGoapNode gainedFromDoing = null) {
        }
        public override void OnTraitAdded(ITraitable traitable, Trait trait, Character characterResponsible = null, ActualGoapNode gainedFromDoing = null) {
            Character character = traitable as Character;
            ApplyTraitEffects(character, trait);
            //ApplyPOITraitInteractions(character, trait);
            character.currentAlterEgo.AddTrait(trait);

            if (GameManager.Instance.gameHasStarted) {
                if (trait.name == "Starving") {
                    character.needsComponent.PlanFullnessRecoveryActions(character);
                } else if (trait.name == "Forlorn" || trait.name == "Lonely") {
                    character.needsComponent.PlanHappinessRecoveryActions(character);
                } else if (trait.name == "Exhausted") {
                    character.needsComponent.PlanTirednessRecoveryActions(character);
                }
            }
            if (character.canMove == false || character.canWitness == false) {
                //when a character gains a negative disabler trait, drop all location jobs that this character is assigned to
                //TODO: //character.jobQueue.UnassignAllJobsTakenBy(character);
            }
            DefaultProcessOnAddTrait(traitable, trait, characterResponsible, gainedFromDoing);
            Messenger.Broadcast(Signals.TRAIT_ADDED, character, trait);
        }
        public override void OnTraitRemoved(ITraitable traitable, Trait trait, Character removedBy) {
            Character character = traitable as Character;
            UnapplyTraitEffects(character, trait);
            //UnapplyPOITraitInteractions(character, trait);
            character.currentAlterEgo.RemoveTrait(trait);

            DefaultProcessOnRemoveTrait(traitable, trait, removedBy);
            Messenger.Broadcast(Signals.TRAIT_REMOVED, character, trait);
        }

        private void ApplyTraitEffects(Character character, Trait trait) {
            if (trait.hindersWitness) {
                character.DecreaseCanWitness();
            }
            if (trait.hindersMovement) {
                character.DecreaseCanMove();
            }
            if (trait.hindersAttackTarget) {
                character.DecreaseCanBeAttacked();
            }
            if (trait.type == TRAIT_TYPE.DISABLER) {
                character.AdjustDoNotDisturb(1);
                if (trait.effect == TRAIT_EFFECT.NEGATIVE) {
                    character.AdjustIgnoreHostilities(1);
                    //character.CancelAllJobsAndPlansExceptNeedsRecovery();
                }
                character.ownParty.RemoveCarriedPOI();
                character.ForceCancelAllJobsTargettingThisCharacter(JOB_TYPE.KNOCKOUT);
            }
            if (trait.name == "Abducted" || trait.name == "Restrained") {
                character.needsComponent.AdjustDoNotGetTired(1);
            } else if (trait.name == "Packaged" || trait.name == "Hibernating" || trait.name == "Reanimated") {
                character.needsComponent.AdjustDoNotGetTired(1);
                character.needsComponent.AdjustDoNotGetHungry(1);
                character.needsComponent.AdjustDoNotGetLonely(1);
            } else if (trait.name == "Eating") {
                character.needsComponent.AdjustDoNotGetHungry(1);
            } else if (trait.name == "Resting") {
                character.needsComponent.AdjustDoNotGetTired(1);
                character.needsComponent.AdjustDoNotGetHungry(1);
                character.needsComponent.AdjustDoNotGetLonely(1);
            } else if (trait.name == "Charmed") {
                character.needsComponent.AdjustDoNotGetLonely(1);
            } else if (trait.name == "Daydreaming") {
                character.needsComponent.AdjustDoNotGetTired(1);
                character.needsComponent.AdjustDoNotGetLonely(1);
            } else if (trait.name == "Forlorn") {
                character.AdjustMoodValue(-35, trait, trait.gainedFromDoing);
            } else if (trait.name == "Lonely") {
                character.AdjustMoodValue(-20, trait, trait.gainedFromDoing);
            } else if (trait.name == "Exhausted") {
                character.marker.AdjustUseWalkSpeed(1);
                character.AdjustMoodValue(-35, trait, trait.gainedFromDoing);
            } else if (trait.name == "Tired") {
                character.AdjustSpeedModifier(-0.2f);
                character.AdjustMoodValue(-10, trait, trait.gainedFromDoing);
            } else if (trait.name == "Starving") {
                character.AdjustMoodValue(-25, trait, trait.gainedFromDoing);
            } else if (trait.name == "Hungry") {
                character.AdjustMoodValue(-10, trait, trait.gainedFromDoing);
            } else if (trait.name == "Injured") {
                character.AdjustMoodValue(-15, trait, trait.gainedFromDoing);
            } else if (trait.name == "Cursed") {
                character.AdjustMoodValue(-25, trait, trait.gainedFromDoing);
            } else if (trait.name == "Sick") {
                character.AdjustMoodValue(-15, trait, trait.gainedFromDoing);
            } else if (trait.name == "Satisfied") {
                character.AdjustMoodValue(15, trait, trait.gainedFromDoing);
            } else if (trait.name == "Annoyed") {
                character.AdjustMoodValue(-15, trait, trait.gainedFromDoing);
            } else if (trait.name == "Lethargic") {
                character.AdjustMoodValue(-20, trait, trait.gainedFromDoing);
            } else if (trait.name == "Heartbroken") {
                character.AdjustMoodValue(-25, trait, trait.gainedFromDoing);
            } else if (trait.name == "Griefstricken") {
                character.AdjustMoodValue(-20, trait, trait.gainedFromDoing);
            } else if (trait.name == "Encumbered") {
                character.AdjustSpeedModifier(-0.5f);
            } else if (trait.name == "Vampiric") {
                character.needsComponent.AdjustDoNotGetTired(1);
            } else if (trait.name == "Unconscious") {
                character.needsComponent.AdjustDoNotGetTired(1);
                character.needsComponent.AdjustDoNotGetHungry(1);
                character.needsComponent.AdjustDoNotGetLonely(1);
            } else if (trait.name == "Optimist") {
                character.needsComponent.AdjustHappinessDecreaseRate(-Mathf.CeilToInt(CharacterManager.DEFAULT_HAPPINESS_DECREASE_RATE * 0.5f)); //Reference: https://trello.com/c/Aw8kIbB1/2654-optimist
            } else if (trait.name == "Pessimist") {
                character.needsComponent.AdjustHappinessDecreaseRate(Mathf.CeilToInt(CharacterManager.DEFAULT_HAPPINESS_DECREASE_RATE * 0.5f)); //Reference: https://trello.com/c/lcen0P9l/2653-pessimist
            } else if (trait.name == "Fast") {
                character.AdjustSpeedModifier(0.25f); //Reference: https://trello.com/c/Gb3kfZEm/2658-fast
            } else if (trait.name == "Shellshocked") {
                character.AdjustMoodValue(-30, trait, trait.gainedFromDoing);
            } else if (trait.name == "Ashamed") {
                character.AdjustMoodValue(-5, trait, trait.gainedFromDoing);
            }
            if (trait.effects != null) {
                for (int i = 0; i < trait.effects.Count; i++) {
                    TraitEffect traitEffect = trait.effects[i];
                    if (!traitEffect.hasRequirement && traitEffect.target == TRAIT_REQUIREMENT_TARGET.SELF) {
                        if (traitEffect.isPercentage) {
                            if (traitEffect.stat == STAT.ATTACK) {
                                character.AdjustAttackPercentMod((int)traitEffect.amount);
                            } else if (traitEffect.stat == STAT.HP) {
                                character.AdjustMaxHPPercentMod((int)traitEffect.amount);
                            } else if (traitEffect.stat == STAT.SPEED) {
                                character.AdjustSpeedPercentMod((int)traitEffect.amount);
                            }
                        } else {
                            if (traitEffect.stat == STAT.ATTACK) {
                                character.AdjustAttackMod((int)traitEffect.amount);
                            } else if (traitEffect.stat == STAT.HP) {
                                character.AdjustMaxHPMod((int)traitEffect.amount);
                            } else if (traitEffect.stat == STAT.SPEED) {
                                character.AdjustSpeedMod((int)traitEffect.amount);
                            }
                        }
                    }
                }
            }
        }
        public void UnapplyTraitEffects(Character character, Trait trait) {
            if (trait.hindersWitness) {
                character.IncreaseCanWitness();
            }
            if (trait.hindersMovement) {
                character.IncreaseCanMove();
            }
            if (trait.hindersAttackTarget) {
                character.IncreaseCanBeAttacked();
            }
            if (trait.type == TRAIT_TYPE.DISABLER) {
                character.AdjustDoNotDisturb(-1);
                if (trait.effect == TRAIT_EFFECT.NEGATIVE) {
                    character.AdjustIgnoreHostilities(-1);
                }
            }
            if (trait.name == "Abducted" || trait.name == "Restrained") {
                character.needsComponent.AdjustDoNotGetTired(-1);
            } else if (trait.name == "Packaged" || trait.name == "Hibernating" || trait.name == "Reanimated") {
                character.needsComponent.AdjustDoNotGetTired(-1);
                character.needsComponent.AdjustDoNotGetHungry(-1);
                character.needsComponent.AdjustDoNotGetLonely(-1);
            } else if (trait.name == "Eating") {
                character.needsComponent.AdjustDoNotGetHungry(-1);
            } else if (trait.name == "Resting") {
                character.needsComponent.AdjustDoNotGetTired(-1);
                character.needsComponent.AdjustDoNotGetHungry(-1);
                character.needsComponent.AdjustDoNotGetLonely(-1);
            } else if (trait.name == "Charmed") {
                character.needsComponent.AdjustDoNotGetLonely(-1);
            } else if (trait.name == "Daydreaming") {
                character.needsComponent.AdjustDoNotGetTired(-1);
                character.needsComponent.AdjustDoNotGetLonely(-1);
            } else if (trait.name == "Forlorn") {
                character.AdjustMoodValue(35, trait, trait.gainedFromDoing);
            } else if (trait.name == "Lonely") {
                character.AdjustMoodValue(20, trait, trait.gainedFromDoing);
            } else if (trait.name == "Exhausted") {
                character.marker.AdjustUseWalkSpeed(-1);
                character.AdjustMoodValue(35, trait, trait.gainedFromDoing);
            } else if (trait.name == "Tired") {
                character.AdjustSpeedModifier(0.2f);
                character.AdjustMoodValue(10, trait, trait.gainedFromDoing);
            } else if (trait.name == "Starving") {
                character.AdjustMoodValue(25, trait, trait.gainedFromDoing);
            } else if (trait.name == "Hungry") {
                character.AdjustMoodValue(10, trait, trait.gainedFromDoing);
            } else if (trait.name == "Injured") {
                character.AdjustMoodValue(15, trait, trait.gainedFromDoing);
            } else if (trait.name == "Cursed") {
                character.AdjustMoodValue(25, trait, trait.gainedFromDoing);
            } else if (trait.name == "Sick") {
                character.AdjustMoodValue(15, trait, trait.gainedFromDoing);
            } else if (trait.name == "Satisfied") {
                character.AdjustMoodValue(-15, trait, trait.gainedFromDoing);
            } else if (trait.name == "Annoyed") {
                character.AdjustMoodValue(15, trait, trait.gainedFromDoing);
            } else if (trait.name == "Lethargic") {
                character.AdjustMoodValue(20, trait, trait.gainedFromDoing);
            } else if (trait.name == "Encumbered") {
                character.AdjustSpeedModifier(0.5f);
            } else if (trait.name == "Vampiric") {
                character.needsComponent.AdjustDoNotGetTired(-1);
            } else if (trait.name == "Unconscious") {
                character.needsComponent.AdjustDoNotGetTired(-1);
                character.needsComponent.AdjustDoNotGetHungry(-1);
                character.needsComponent.AdjustDoNotGetLonely(-1);
            } else if (trait.name == "Optimist") {
                character.needsComponent.AdjustHappinessDecreaseRate(Mathf.CeilToInt(CharacterManager.DEFAULT_HAPPINESS_DECREASE_RATE * 0.5f)); //Reference: https://trello.com/c/Aw8kIbB1/2654-optimist
            } else if (trait.name == "Pessimist") {
                character.needsComponent.AdjustHappinessDecreaseRate(-Mathf.CeilToInt(CharacterManager.DEFAULT_HAPPINESS_DECREASE_RATE * 0.5f)); //Reference: https://trello.com/c/lcen0P9l/2653-pessimist
            } else if (trait.name == "Fast") {
                character.AdjustSpeedModifier(-0.25f); //Reference: https://trello.com/c/Gb3kfZEm/2658-fast
            } else if (trait.name == "Shellshocked") {
                character.AdjustMoodValue(30, trait, trait.gainedFromDoing);
            } else if (trait.name == "Ashamed") {
                character.AdjustMoodValue(5, trait, trait.gainedFromDoing);
            }

            if (trait.effects != null) {
                for (int i = 0; i < trait.effects.Count; i++) {
                    TraitEffect traitEffect = trait.effects[i];
                    if (!traitEffect.hasRequirement && traitEffect.target == TRAIT_REQUIREMENT_TARGET.SELF) {
                        if (traitEffect.isPercentage) {
                            if (traitEffect.stat == STAT.ATTACK) {
                                character.AdjustAttackPercentMod(-(int)traitEffect.amount);
                            } else if (traitEffect.stat == STAT.HP) {
                                character.AdjustMaxHPPercentMod(-(int)traitEffect.amount);
                            } else if (traitEffect.stat == STAT.SPEED) {
                                character.AdjustSpeedPercentMod(-(int)traitEffect.amount);
                            }
                        } else {
                            if (traitEffect.stat == STAT.ATTACK) {
                                character.AdjustAttackMod(-(int)traitEffect.amount);
                            } else if (traitEffect.stat == STAT.HP) {
                                character.AdjustMaxHPMod(-(int)traitEffect.amount);
                            } else if (traitEffect.stat == STAT.SPEED) {
                                character.AdjustSpeedMod(-(int)traitEffect.amount);
                            }
                        }
                    }
                }
            }
        }
    }
}

