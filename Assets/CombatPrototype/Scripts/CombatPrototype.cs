﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ECS{
	public enum SIDES{
		A,
		B,
	}

	public class CombatPrototype : MonoBehaviour {
		public static CombatPrototype Instance;

        public float updateIntervals = 0.5f;

//		public Dictionary<SIDES, List<Character>> allCharactersAndSides;
		internal List<Character> charactersSideA;
		internal List<Character> charactersSideB;

		void Awake(){
			Instance = this;
		}

		void Start(){
//			this.allCharactersAndSides = new Dictionary<SIDES, List<Character>> ();
			this.charactersSideA = new List<Character> ();
			this.charactersSideB = new List<Character> ();
			Messenger.AddListener<Character> ("CharacterDeath", CharacterDeath);
		}

        #region Character Management
        //Add a character to a side
        internal void AddCharacter(SIDES side, Character character) {
            //			if(this.allCharactersAndSides.ContainsKey(side)){
            //				this.allCharactersAndSides [side].Add (character);
            //			}else{
            //				this.allCharactersAndSides.Add (side, new List<Character>(){character});
            //			}

            if (side == SIDES.A) {
                this.charactersSideA.Add(character);
            } else {
                this.charactersSideB.Add(character);
            }
            CombatPrototypeUI.Instance.UpdateCharactersList(side);
        }
        //Remove a character from a side
        internal void RemoveCharacter(SIDES side, Character character) {
            //			if(this.allCharactersAndSides.ContainsKey(side)){
            //				this.allCharactersAndSides [side].Remove (character);
            //			}

            if (side == SIDES.A) {
                this.charactersSideA.Remove(character);
            } else {
                this.charactersSideB.Remove(character);
            }
            CombatPrototypeUI.Instance.UpdateCharactersList(side);
        }
        //Remove character without specifying a side
        internal void RemoveCharacter(Character character) {
            if (this.charactersSideA.Remove(character)) {
                CombatPrototypeUI.Instance.UpdateCharactersList(SIDES.A);
            } else {
                this.charactersSideB.Remove(character);
                CombatPrototypeUI.Instance.UpdateCharactersList(SIDES.B);
            }
        }
        internal List<Character> GetCharactersOnSide(SIDES side) {
            if (side == SIDES.A) {
                return charactersSideA;
            } else {
                return charactersSideB;
            }
        }
        #endregion

        public void StartCombatSimulationCoroutine() {
            StartCoroutine(CombatSimulation());
        }

        //This simulates the whole combat system
        public IEnumerator CombatSimulation(){
            CombatPrototypeUI.Instance.ClearCombatLogs();
            //			List<Character> charactersSideA = this.allCharactersAndSides [SIDES.A];
            //			List<Character> charactersSideB = this.allCharactersAndSides [SIDES.B];
            ResetAllArmorHitPoints(); //Reset all hitpoints of each characters armor
            bool isInitial = true;
			bool isOneSideDefeated = false;
			SetRowNumber (this.charactersSideA, 1);
			SetRowNumber (this.charactersSideB, 5);

            int rounds = 1;
			while(this.charactersSideA.Count > 0 && this.charactersSideB.Count > 0){
                Debug.Log("========== Round " + rounds.ToString() + " ==========");
				Character characterThatWillAct = GetCharacterThatWillAct (this.charactersSideA, this.charactersSideB, isInitial);
				characterThatWillAct.EnableDisableSkills ();
                Debug.Log(characterThatWillAct.characterClass.className + " " + characterThatWillAct.name + " will act");
                Debug.Log("Available Skills: ");
                for (int i = 0; i < characterThatWillAct.characterClass.skills.Count; i++) {
                    Skill currSkill = characterThatWillAct.characterClass.skills[i];
                    if (currSkill.isEnabled) {
                        Debug.Log(currSkill.skillName);
                    }
                }

                Skill skillToUse = GetSkillToUse (characterThatWillAct);
				if(skillToUse != null){
                    Debug.Log(characterThatWillAct.name + " decides to use " + skillToUse.skillName);
					characterThatWillAct.CureStatusEffects ();
					Character targetCharacter = GetTargetCharacter (characterThatWillAct, skillToUse);
                    Debug.Log(characterThatWillAct.name + " decides to use it on " + targetCharacter.name);
                    DoSkill (skillToUse, characterThatWillAct, targetCharacter);
				}
                if (isInitial) {
                    isInitial = false;
                }
                CombatPrototypeUI.Instance.UpdateCharacterSummary();
                Debug.Log("========== End Round " + rounds.ToString() + " ==========");
                rounds++;
                yield return new WaitForSeconds(updateIntervals);
            }
		}

		//Set row number to a list of characters
		private void SetRowNumber(List<Character> characters, int rowNumber){
			for (int i = 0; i < characters.Count; i++) {
				characters [i].SetRowNumber(rowNumber);
			}
		}

		//Return a character that will act from a pool of characters based on their act rate
		private Character GetCharacterThatWillAct(List<Character> charactersSideA, List<Character> charactersSideB, bool isInitial){
			Dictionary<Character, int> characterActivationWeights = new Dictionary<Character, int> ();
			int modifier = 1;
			if(isInitial){
				modifier = 10;
			}

			for (int i = 0; i < charactersSideA.Count; i++) {
				int actRate = charactersSideA [i].actRate * modifier;
				characterActivationWeights.Add (charactersSideA [i], actRate);
			}
			for (int i = 0; i < charactersSideB.Count; i++) {
				int actRate = charactersSideB [i].actRate * modifier;
				characterActivationWeights.Add (charactersSideB [i], actRate);
			}

			Character chosenCharacter = Utilities.PickRandomElementWithWeights<Character>(characterActivationWeights);
			foreach (Character character in characterActivationWeights.Keys) {
				character.actRate += character.characterClass.actRate;
			}
			chosenCharacter.actRate = 0;
			return chosenCharacter;
		}

		//Get a random character from the opposite side to be the target
		private Character GetTargetCharacter(Character sourceCharacter, Skill skill){
//			if(this.allCharactersAndSides[SIDES.A].Contains(sourceCharacter)){
//				return this.allCharactersAndSides [SIDES.B] [UnityEngine.Random.Range (0, this.allCharactersAndSides [SIDES.B].Count)];
//			}
//			return this.allCharactersAndSides [SIDES.A] [UnityEngine.Random.Range (0, this.allCharactersAndSides [SIDES.A].Count)];
			List<Character> possibleTargets = new List<Character>();
			if (skill is AttackSkill) {
				if (this.charactersSideA.Contains (sourceCharacter)) {
					for (int i = 0; i < this.charactersSideB.Count; i++) {
						Character targetCharacter = this.charactersSideB [i];
						int rowDistance = GetRowDistanceBetweenTwoCharacters (sourceCharacter, targetCharacter);
						if (skill.range >= rowDistance) {
							possibleTargets.Add (targetCharacter);
						}
					}
				} else {
					for (int i = 0; i < this.charactersSideA.Count; i++) {
						Character targetCharacter = this.charactersSideA [i];
						int rowDistance = GetRowDistanceBetweenTwoCharacters (sourceCharacter, targetCharacter);
						if (skill.range >= rowDistance) {
							possibleTargets.Add (targetCharacter);
						}
					}
				}
			} else if (skill is HealSkill) {
				if (this.charactersSideA.Contains (sourceCharacter)) {
					for (int i = 0; i < this.charactersSideA.Count; i++) {
						Character targetCharacter = this.charactersSideA [i];
						int rowDistance = GetRowDistanceBetweenTwoCharacters (sourceCharacter, targetCharacter);
						if (skill.range >= rowDistance) {
							possibleTargets.Add (targetCharacter);
						}
					}
				} else {
					for (int i = 0; i < this.charactersSideB.Count; i++) {
						Character targetCharacter = this.charactersSideB [i];
						int rowDistance = GetRowDistanceBetweenTwoCharacters (sourceCharacter, targetCharacter);
						if (skill.range >= rowDistance) {
							possibleTargets.Add (targetCharacter);
						}
					}
				}
			}else{
				possibleTargets.Add (sourceCharacter);
			}

			return possibleTargets [UnityEngine.Random.Range (0, possibleTargets.Count)];
		}

		//Get Skill that the character will use based on activation weights, target character must be within skill range
		private Skill GetSkillToUse(Character sourceCharacter){
			Dictionary<Skill, int> skillActivationWeights = new Dictionary<Skill, int> ();
			for (int i = 0; i < sourceCharacter.characterClass.skills.Count; i++) {
				Skill skill = sourceCharacter.characterClass.skills [i];
				if(skill.isEnabled && HasTargetInRangeForSkill(skill, sourceCharacter)){
					int activationWeight = skill.activationWeight;
					if(skill.actWeightType == ACTIVATION_WEIGHT_TYPE.CURRENT_HEALTH){
						activationWeight = (int)(((float)sourceCharacter.currentHP / (float)sourceCharacter.maxHP) * 100f);
					}else if(skill.actWeightType == ACTIVATION_WEIGHT_TYPE.MISSING_HEALTH){
						int missingHealth = sourceCharacter.maxHP - sourceCharacter.currentHP;
						int weight = (int)(((float)missingHealth / (float)sourceCharacter.maxHP) * 100f);
						activationWeight =  weight > 0f ? weight : 1;
					}else if(skill.actWeightType == ACTIVATION_WEIGHT_TYPE.ALLY_MISSING_HEALTH){
						int highestMissingHealth = 0;
						Character chosenCharacter = null;
						if(charactersSideA.Contains(sourceCharacter)){
							for (int j = 0; j < charactersSideA.Count; j++) {
								Character character = charactersSideA [j];
								int missingHealth = character.maxHP - character.currentHP;
								if(chosenCharacter == null){
									highestMissingHealth = missingHealth;
									chosenCharacter = character;
								}else{
									if(missingHealth > highestMissingHealth){
										highestMissingHealth = missingHealth;
										chosenCharacter = character;
									}
								}
							}
						}else{
							for (int j = 0; j < charactersSideB.Count; j++) {
								Character character = charactersSideB [j];
								int missingHealth = character.maxHP - character.currentHP;
								if(chosenCharacter == null){
									highestMissingHealth = missingHealth;
									chosenCharacter = character;
								}else{
									if(missingHealth > highestMissingHealth){
										highestMissingHealth = missingHealth;
										chosenCharacter = character;
									}
								}
							}
						}
						if(chosenCharacter != null){
							int weight = (int)((((float)highestMissingHealth / (float)chosenCharacter.maxHP) * 100f) * 2f);
							activationWeight = weight > 0f ? weight : 1;
						}
					}
					if(skill is MoveSkill && HasTargetInRangeForSkill(SKILL_TYPE.ATTACK, sourceCharacter)){
						activationWeight /= 2;
					}
					skillActivationWeights.Add (skill, activationWeight);
				}
			}

			if(skillActivationWeights.Count > 0){
				Skill chosenSkill = Utilities.PickRandomElementWithWeights<Skill> (skillActivationWeights);
				return chosenSkill;
			}
			return null;
		}

		//Check if there are targets in range for the specific skill so that the character can know if the skill can be activated 
		internal bool HasTargetInRangeForSkill(Skill skill, Character sourceCharacter){
			if(skill is AttackSkill){
				if(this.charactersSideA.Contains(sourceCharacter)){
					for (int i = 0; i < this.charactersSideB.Count; i++) {
						Character targetCharacter = this.charactersSideB [i];
						int rowDistance = GetRowDistanceBetweenTwoCharacters (sourceCharacter, targetCharacter);
						if(skill.range >= rowDistance){
							return true;
						}
					}
				}else{
					for (int i = 0; i < this.charactersSideA.Count; i++) {
						Character targetCharacter = this.charactersSideA [i];
						int rowDistance = GetRowDistanceBetweenTwoCharacters (sourceCharacter, targetCharacter);
						if(skill.range >= rowDistance){
							return true;
						}
					}
				}
				return false;
			}else{
				return true;
			}

		}

		internal bool HasTargetInRangeForSkill(SKILL_TYPE skillType, Character sourceCharacter){
			if(skillType == SKILL_TYPE.ATTACK){
				for (int i = 0; i < sourceCharacter.characterClass.skills.Count; i++) {
					Skill skill = sourceCharacter.characterClass.skills [i];
					if(skill is AttackSkill){
						if(this.charactersSideA.Contains(sourceCharacter)){
							for (int j = 0; j < this.charactersSideB.Count; j++) {
								Character targetCharacter = this.charactersSideB [j];
								int rowDistance = GetRowDistanceBetweenTwoCharacters (sourceCharacter, targetCharacter);
								if(skill.range >= rowDistance){
									return true;
								}
							}
						}else{
							for (int j = 0; j < this.charactersSideA.Count; j++) {
								Character targetCharacter = this.charactersSideA [j];
								int rowDistance = GetRowDistanceBetweenTwoCharacters (sourceCharacter, targetCharacter);
								if(skill.range >= rowDistance){
									return true;
								}
							}
						}
						return false;
					}
				}
			}
			return true;
		}
		//Returns the row distance/difference of two characters
		private int GetRowDistanceBetweenTwoCharacters(Character sourceCharacter, Character targetCharacter){
			int distance = targetCharacter.currentRow - sourceCharacter.currentRow;
			if(distance < 0){
				distance *= -1;
			}
			return distance;
		}
		//Character will do the skill specified, but its success will be determined by the skill's accuracy
		private void DoSkill(Skill skill, Character sourceCharacter, Character targetCharacter){
			float chance = UnityEngine.Random.Range (0f, 99f);
			if(chance < skill.accuracy){
				//Successful
				SuccessfulSkill(skill, sourceCharacter, targetCharacter);
			}else{
				//Fail
				FailedSkill(skill, sourceCharacter, targetCharacter);
			}
		}

		//Go here if skill is accurate and is successful
		private void SuccessfulSkill(Skill skill, Character sourceCharacter, Character targetCharacter){
			if (skill is AttackSkill) {
				AttackSkill (skill, sourceCharacter, targetCharacter);
			} else if (skill is HealSkill) {
				HealSkill (skill, sourceCharacter, targetCharacter);
			} else if (skill is FleeSkill) {
				FleeSkill (sourceCharacter, targetCharacter);
			} else if (skill is ObtainSkill) {
				ObtainItemSkill (sourceCharacter, targetCharacter);
			} else if (skill is MoveSkill) {
				MoveSkill (skill, sourceCharacter, targetCharacter);
			}
		}

		//Skill is not accurate and therefore has failed to execute
		private void FailedSkill(Skill skill, Character sourceCharacter, Character targetCharacter){
			//TODO: What happens when a skill has failed?
			if(skill is FleeSkill){
				CombatPrototypeUI.Instance.AddCombatLog(sourceCharacter.name + " tried to flee but got tripped over and fell down!");
				CounterAttack (targetCharacter);
			}else if(skill is AttackSkill){
				CombatPrototypeUI.Instance.AddCombatLog (sourceCharacter.name + " tried to " + skill.skillName + " " + targetCharacter.name + " but missed!");
			}else if(skill is HealSkill){
				if(sourceCharacter == targetCharacter){
					CombatPrototypeUI.Instance.AddCombatLog (sourceCharacter.name + " tried to use " + skill.skillName + " to heal himself/herself but it is already expired!");
				}else{
					CombatPrototypeUI.Instance.AddCombatLog (sourceCharacter.name + " tried to use " + skill.skillName + " to heal " + targetCharacter.name + " but it is already expired!");
				}
			}
		}

		//Get DEFEND_TYPE for the attack skill, if DEFEND_TYPE is NONE, then target character has not defend successfully, therefore, the target character will be damaged
		private DEFEND_TYPE CanTargetCharacterDefend(Character targetCharacter){
			int dodgeChance = UnityEngine.Random.Range (0, 100);
			if(dodgeChance < targetCharacter.dodgeRate){
				return DEFEND_TYPE.DODGE;
			}else{
				int parryChance = UnityEngine.Random.Range (0, 100);
				if(parryChance < targetCharacter.parryRate){
					return DEFEND_TYPE.PARRY;
				}else{
					int blockChance = UnityEngine.Random.Range (0, 100);
					if(blockChance < targetCharacter.blockRate){
						return DEFEND_TYPE.BLOCK;
					}else{
						return DEFEND_TYPE.NONE;
					}
				}
			}
		}

		private void CounterAttack(Character character){
			//TODO: Counter attack
			CombatPrototypeUI.Instance.AddCombatLog (character.name + " counterattacked!");
		}

		private void InstantDeath(Character character){
			character.Death();
		}

		#region Attack Skill
		private void AttackSkill(Skill skill, Character sourceCharacter, Character targetCharacter){
			AttackSkill attackSkill = (AttackSkill)skill;
			DEFEND_TYPE defendType = CanTargetCharacterDefend (targetCharacter);
			if(defendType == DEFEND_TYPE.NONE){
				//Successfully hits the target character
				HitTargetCharacter(attackSkill, sourceCharacter, targetCharacter, GetWeaponForSkill(attackSkill, sourceCharacter));
			}else{
				//Target character has defend successfully and will roll for counter attack
				if(defendType == DEFEND_TYPE.DODGE){
					CombatPrototypeUI.Instance.AddCombatLog(targetCharacter.name + " dodged " + sourceCharacter.name + "'s " + attackSkill.skillName + ".");
				} else if(defendType == DEFEND_TYPE.BLOCK){
					CombatPrototypeUI.Instance.AddCombatLog(targetCharacter.name + " blocked " + sourceCharacter.name + "'s " + attackSkill.skillName + ".");
				} else if(defendType == DEFEND_TYPE.PARRY){
					CombatPrototypeUI.Instance.AddCombatLog(targetCharacter.name + " parried " + sourceCharacter.name + "'s " + attackSkill.skillName + ".");
				}
				CounterAttack(targetCharacter);
			}
		}
			
		//Hits the target with an attack skill
		private void HitTargetCharacter(AttackSkill attackSkill, Character sourceCharacter, Character targetCharacter, Weapon weapon = null){
            int damage = 0;
            if(weapon == null) {
                //Total Damage = [Spower * str] + [Ipower * int] + [Apower * agi]
                damage = (int)((attackSkill.strengthPower * (float)sourceCharacter.strength)
                    + (attackSkill.intellectPower * (float)sourceCharacter.intelligence)
                    + (attackSkill.agilityPower * (float)sourceCharacter.agility));
            } else {
                //Total Damage = [Spower * (str + Watk)] + [Ipower * (int + Watk)] + [Apower * (agi + Watk)]
                damage = (int)((attackSkill.strengthPower * (float)(sourceCharacter.strength + weapon.weaponPower))
                    + (attackSkill.intellectPower * (float)(sourceCharacter.intelligence + weapon.weaponPower))
                    + (attackSkill.agilityPower * (float)(sourceCharacter.agility + weapon.weaponPower)));

                //reduce weapon durability by 1
                weapon.AdjustDurability(-1);
            }
            
            BodyPart damagedBodyPart =	DealDamageToBodyPart (attackSkill, targetCharacter, sourceCharacter);

            //Get all armor pieces on body part that still has hit points
			List<Item> armorOnBodyPart = damagedBodyPart.GetAttachedItemsOfType(ITEM_TYPE.ARMOR).OrderByDescending(x => ((Armor)x).currHitPoints).ToList();
            if (armorOnBodyPart.Count > 0) {
				if(attackSkill.attackType != ATTACK_TYPE.PIERCE){
					for (int i = 0; i < armorOnBodyPart.Count; i++) {
						Armor currArmor = (Armor)armorOnBodyPart[i];
						if(currArmor.currHitPoints > 0) {
							//Deal damage to armor
							currArmor.AdjustHitPoints (-damage);
							CombatPrototypeUI.Instance.AddCombatLog(sourceCharacter.name + " used " + attackSkill.skillName + " and damages " + targetCharacter.name
								+ "'s " + currArmor.itemName + " for " + damage.ToString() + ".");

							damage -= currArmor.currHitPoints;
							if(damage < 0 ){
								damage = 0;
							}
						}
					}
				}

				int damageToDurability = attackSkill.durabilityDamage; //[SkillDurabilityDamage  +  WeaponDurabilityDamage]
				if (weapon != null) {
					damageToDurability += weapon.durabilityDamage;
				}
				for (int i = 0; i < armorOnBodyPart.Count; i++) {
					Armor currArmor = (Armor)armorOnBodyPart[i];
					if(currArmor.durability > 0) {
						//Reduce Armor Durability
						currArmor.AdjustDurability(-damageToDurability);
						if(currArmor.durability <= 0) {
							CombatPrototypeUI.Instance.AddCombatLog(sourceCharacter.name + " destroys " + targetCharacter.name + "'s " + currArmor.itemName);
						}
					}
				}

                if (damage > 0) {
                        //Deal damage to hp
                        CombatPrototypeUI.Instance.AddCombatLog(sourceCharacter.name + " used " + attackSkill.skillName + " and damages " + targetCharacter.name
                            + " for " + damage.ToString() + ".");
                        targetCharacter.AdjustHP(-damage);
                }
                    
            } else {
                //Deal damage to hp
                CombatPrototypeUI.Instance.AddCombatLog(sourceCharacter.name + " used " + attackSkill.skillName + " and damages " + targetCharacter.name
					+ " for " + damage.ToString() + ".");
                    targetCharacter.AdjustHP(-damage);
            }
        }

		//This will select, deal damage, and apply status effect to a body part if possible 
		private BodyPart DealDamageToBodyPart(AttackSkill attackSkill, Character targetCharacter, Character sourceCharacter){
			int chance = UnityEngine.Random.Range (0, 100);
            BodyPart chosenBodyPart = GetRandomBodyPart(targetCharacter);

			if(attackSkill.statusEffectRates != null && attackSkill.statusEffectRates.Count > 0){
				for (int i = 0; i < attackSkill.statusEffectRates.Count; i++) {
					int value = attackSkill.statusEffectRates[i].ratePercentage;
					string log = string.Empty;
					if(attackSkill.statusEffectRates[i].statusEffect == STATUS_EFFECT.INJURED){
						if(attackSkill.attackType == ATTACK_TYPE.CRUSH){
							value += 7;
							log = sourceCharacter.name + " used " + attackSkill.skillName.ToLower () + " and crushes " + targetCharacter.name
							+ "'s " + chosenBodyPart.bodyPart.ToString ().ToLower () + ", injuring it.";
						}else{
							log = sourceCharacter.name + " used " + attackSkill.skillName.ToLower () + " and injures " + targetCharacter.name
							+ "'s " + chosenBodyPart.bodyPart.ToString ().ToLower () + ".";
						}
					}else if(attackSkill.statusEffectRates[i].statusEffect == STATUS_EFFECT.BLEEDING){
						if(attackSkill.attackType == ATTACK_TYPE.PIERCE){
							value += 10;
							log = sourceCharacter.name + " used " + attackSkill.skillName + " and pierces " + targetCharacter.name + 
								"'s " + chosenBodyPart.bodyPart.ToString().ToLower() + ", causing it to bleed.";
						}else{
							log = sourceCharacter.name + " used " + attackSkill.skillName.ToLower () + " and causes " + targetCharacter.name
								+ "'s " + chosenBodyPart.bodyPart.ToString ().ToLower () + " to bleed.";
						}
					}else if(attackSkill.statusEffectRates[i].statusEffect == STATUS_EFFECT.DECAPITATED){
						if(attackSkill.attackType == ATTACK_TYPE.SLASH){
							value += 5;
							log = sourceCharacter.name + " used " + attackSkill.skillName + " and slashes " + targetCharacter.name + 
								"'s " + chosenBodyPart.bodyPart.ToString().ToLower() + ", decapitating it.";
						}else{
							log = sourceCharacter.name + " used " + attackSkill.skillName.ToLower () + " and decapitates " + targetCharacter.name
								+ "'s " + chosenBodyPart.bodyPart.ToString ().ToLower () + ".";
						}
					}else if(attackSkill.statusEffectRates[i].statusEffect == STATUS_EFFECT.BURNING){
						if(attackSkill.attackType == ATTACK_TYPE.BURN){
							value += 5;
						}
						log = sourceCharacter.name + " used " + attackSkill.skillName + " and burns " + targetCharacter.name + 
							"'s " + chosenBodyPart.bodyPart.ToString().ToLower() + ".";
					}

					if(chance < value){
						chosenBodyPart.statusEffects.Add(attackSkill.statusEffectRates[i].statusEffect);
						chosenBodyPart.ApplyStatusEffectOnSecondaryBodyParts (attackSkill.statusEffectRates[i].statusEffect);
						CombatPrototypeUI.Instance.AddCombatLog (log);
					}
				}
			}else{
				if (attackSkill.attackType == ATTACK_TYPE.CRUSH){
					if(chance < 7){
						chosenBodyPart.statusEffects.Add(STATUS_EFFECT.INJURED);
						chosenBodyPart.ApplyStatusEffectOnSecondaryBodyParts (STATUS_EFFECT.INJURED);
						CombatPrototypeUI.Instance.AddCombatLog(sourceCharacter.name + " used " + attackSkill.skillName + " and crushes " + targetCharacter.name 
							+ "'s " + chosenBodyPart.bodyPart.ToString().ToLower() + ", injuring it.");
					}
				}else if(attackSkill.attackType == ATTACK_TYPE.PIERCE){
					if(chance < 10){
						chosenBodyPart.statusEffects.Add(STATUS_EFFECT.BLEEDING);
						chosenBodyPart.ApplyStatusEffectOnSecondaryBodyParts (STATUS_EFFECT.BLEEDING);
						CombatPrototypeUI.Instance.AddCombatLog(sourceCharacter.name + " used " + attackSkill.skillName + " and pierces " + targetCharacter.name + 
							"'s " + chosenBodyPart.bodyPart.ToString().ToLower() + ", causing it to bleed.");
					}
				}else if(attackSkill.attackType == ATTACK_TYPE.SLASH){
					if(chance < 5){
						chosenBodyPart.statusEffects.Add(STATUS_EFFECT.DECAPITATED);
						chosenBodyPart.ApplyStatusEffectOnSecondaryBodyParts (STATUS_EFFECT.DECAPITATED);
						CombatPrototypeUI.Instance.AddCombatLog(sourceCharacter.name + " used " + attackSkill.skillName + " and slashes " + targetCharacter.name + 
							"'s " + chosenBodyPart.bodyPart.ToString().ToLower() + ", decapitating it.");
						//If body part is essential, instant death to the character
						if (chosenBodyPart.importance == IBodyPart.IMPORTANCE.ESSENTIAL){
							CheckBodyPart (chosenBodyPart.bodyPart, targetCharacter);
						}
					}
				}else if(attackSkill.attackType == ATTACK_TYPE.BURN){
					if(chance < 5){
						chosenBodyPart.statusEffects.Add(STATUS_EFFECT.BURNING);
						chosenBodyPart.ApplyStatusEffectOnSecondaryBodyParts (STATUS_EFFECT.BURNING);
						CombatPrototypeUI.Instance.AddCombatLog(sourceCharacter.name + " used " + attackSkill.skillName + " and burns " + targetCharacter.name + 
							"'s " + chosenBodyPart.bodyPart.ToString().ToLower());
					}
				}
			}
            return chosenBodyPart;
        }


		//Returns a random body part of a character
		private BodyPart GetRandomBodyPart(Character character){
//			List<IBodyPart> allBodyParts = new List<IBodyPart> (character.bodyParts);
//			for (int i = 0; i < character.bodyParts.Count; i++) {
//				allBodyParts.AddRange (character.bodyParts [i].secondaryBodyParts);
//			}

//			return allBodyParts [UnityEngine.Random.Range (0, allBodyParts.Count)];
			List<BodyPart> allBodyParts = character.bodyParts.Where(x => !x.statusEffects.Contains(STATUS_EFFECT.DECAPITATED)).ToList();
			return allBodyParts [UnityEngine.Random.Range (0, allBodyParts.Count)];
		}
        /*
         * Get Weapon that meets the requirements of a given skill,
         * if the skill does not require a weapon, this will just return null.
         * */
        private Weapon GetWeaponForSkill(Skill skill, Character sourceCharacter) {
            if (skill.RequiresItem()) {
                List<Weapon> weapons = sourceCharacter.GetAllAttachedWeapons();
                for (int i = 0; i < weapons.Count; i++) {
                    Weapon currWeapon = weapons[i];
                    bool meetsRequirements = true;
                    for (int j = 0; j < skill.skillRequirements.Length; j++) {
                        SkillRequirement currRequirement = skill.skillRequirements[j];
                        if(currRequirement.equipmentType == EQUIPMENT_TYPE.NONE) {
                            //skip requirements that don't require an item
                            continue;
                        }
                        if(currRequirement.equipmentType != (EQUIPMENT_TYPE)currWeapon.weaponType || !currWeapon.attributes.Contains(currRequirement.attributeRequired)) {
                            meetsRequirements = false;
                            break;
                        }
                    }
                    if (meetsRequirements) {
                        return currWeapon;
                    }
                }
            }
            return null;
        }
		#endregion

		#region Heal Skill
		private void HealSkill(Skill skill, Character sourceCharacter, Character targetCharacter){
			HealSkill healSkill = (HealSkill)skill;	
			targetCharacter.AdjustHP (healSkill.healPower);
			if(sourceCharacter == targetCharacter){
				CombatPrototypeUI.Instance.AddCombatLog(sourceCharacter.name + " used " + healSkill.skillName + " and healed himself/herself for " + healSkill.healPower.ToString() + ".");
			}else if(sourceCharacter == targetCharacter){
				CombatPrototypeUI.Instance.AddCombatLog(sourceCharacter.name + " used " + healSkill.skillName + " and healed " + targetCharacter.name + " for " + healSkill.healPower.ToString() + ".");
			}

		}
		#endregion

		#region Flee Skill
		private void FleeSkill(Character sourceCharacter, Character targetCharacter){
			//TODO: Character flees
			RemoveCharacter(targetCharacter);
			CombatPrototypeUI.Instance.AddCombatLog(targetCharacter.name + " chickened out and ran away!");
		}
		#endregion

		#region Obtain Item Skill
		private void ObtainItemSkill(Character sourceCharacter, Character targetCharacter){
			//TODO: Character obtains an item
			CombatPrototypeUI.Instance.AddCombatLog(targetCharacter.name + " obtained an item.");
		}
		#endregion

		#region Move Skill
		private void MoveSkill(Skill skill, Character sourceCharacter, Character targetCharacter){
			if(skill.skillName == "MoveLeft"){
				if (targetCharacter.currentRow != 1) {
					targetCharacter.SetRowNumber(targetCharacter.currentRow - 1);
				}
				CombatPrototypeUI.Instance.AddCombatLog(targetCharacter.name + " moved to the left. (" + targetCharacter.currentRow + ")");
			}else if(skill.skillName == "MoveRight"){
				if (targetCharacter.currentRow != 5) {
					targetCharacter.SetRowNumber(targetCharacter.currentRow + 1);
				}
				CombatPrototypeUI.Instance.AddCombatLog(targetCharacter.name + " moved to the right.(" + targetCharacter.currentRow + ")");
			}

//			if(sourceCharacter.currentRow == 1){
//				//Automatically moves to the right since 1 is the last row on the left
//				sourceCharacter.SetRowNumber (2);
//			}else if(sourceCharacter.currentRow == 5){
//				//Automatically moves to the left since 5 is the last row on the right
//				sourceCharacter.SetRowNumber (4);
//			}else{
//				//TODO: Is it totally random, or there will determining factors if movement is to the left or to the right
//				int chance = UnityEngine.Random.Range (0, 2);
//				if(chance == 0){
//					//Move left
//					sourceCharacter.SetRowNumber(sourceCharacter.currentRow - 1);
//				}else{
//					//Move right
//					sourceCharacter.SetRowNumber(sourceCharacter.currentRow + 1);
//				}
//			}
		}
		#endregion

		//This will receive the "CharacterDeath" signal when broadcasted, this is a listener
		private void CharacterDeath(Character character){
			RemoveCharacter (character);
			CombatPrototypeUI.Instance.AddCombatLog(character.name + " has died horribly!");
		}

		//Check essential body part quantity, if all are decapitated, instant death
		private void CheckBodyPart(BODY_PART bodyPart, Character character){
			for (int i = 0; i < character.bodyParts.Count; i++) {
				BodyPart characterBodyPart = character.bodyParts [i];
				if(characterBodyPart.bodyPart == bodyPart && !characterBodyPart.statusEffects.Contains(STATUS_EFFECT.DECAPITATED)){
					return;
				}

				for (int j = 0; j < characterBodyPart.secondaryBodyParts.Count; j++) {
					SecondaryBodyPart secondaryBodyPart = characterBodyPart.secondaryBodyParts [j];
					if(secondaryBodyPart.bodyPart == bodyPart && !secondaryBodyPart.statusEffects.Contains(STATUS_EFFECT.DECAPITATED)){
						return;
					}
				}
			}
			InstantDeath (character);
		}

        #region Items
        private void ResetAllArmorHitPoints() {
            List<Character> allCharacters = new List<Character>(charactersSideA);
            allCharacters.AddRange(charactersSideB);
            for (int i = 0; i < allCharacters.Count; i++) {
                Character currCharacter = allCharacters[i];
                List<Armor> armorOfCharacter = currCharacter.GetAllAttachedArmor();
                for (int j = 0; j < armorOfCharacter.Count; j++) {
                    Armor currArmor = armorOfCharacter[j];
                    currArmor.ResetHitPoints();
                }
            }
        }
        #endregion
    }
}

