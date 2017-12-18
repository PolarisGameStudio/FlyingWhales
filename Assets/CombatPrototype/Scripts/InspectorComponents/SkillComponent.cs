﻿using UnityEngine;
using System.Collections;

namespace ECS{
	public class SkillComponent : MonoBehaviour {
		public SKILL_TYPE skillType;
		public string skillName;
		public int activationWeight;
		public float accuracy;
		public int range;
        public float strengthPower;
        public float intellectPower;
        public float agilityPower;
        public SkillRequirement[] skillRequirements;
		public CHARACTER_ATTRIBUTES attributeModifier;

		//Attack Skill Fields
		public int attackPower;
		public ATTACK_TYPE attackType;
		public STATUS_EFFECT statusEffect;
		public int statusEffectRate;
		public int injuryRate;
		public int decapitationRate;
        public int durabilityDamage;

		//Heal Skill Fields
		public int healPower;

        //Shared Fields
        public int durabilityCost;
    }
}

