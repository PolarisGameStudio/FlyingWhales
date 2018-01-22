﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ECS{
	public class ClassComponent : MonoBehaviour {
		public string className;
		public float strPercentage;
		public float intPercentage;
		public float agiPercentage;
		public float hpPercentage;
		public int dodgeRate;
		public int parryRate;
		public int blockRate;

		public List<WEAPON_TYPE> allowedWeaponTypes;
//		public void AddSkillOfType(SKILL_TYPE skillType, Skill skillToAdd) {
//			switch (skillType) {
//			case SKILL_TYPE.ATTACK:
//				attackSkills.Add (skillToAdd.skillName);
//				break;
//			case SKILL_TYPE.HEAL:
//				healSkills.Add(skillToAdd.skillName);
//				break;
//			case SKILL_TYPE.OBTAIN_ITEM:
//				obtainSkills.Add(skillToAdd.skillName);
//				break;
//			case SKILL_TYPE.FLEE:
//				fleeSkills.Add(skillToAdd.skillName);
//				break;
//			case SKILL_TYPE.MOVE:
//				moveSkills.Add(skillToAdd.skillName);
//				break;
//			}
//			if(this._skills == null){
//				this._skills = new List<Skill> ();
//			}
//			this._skills.Add (skillToAdd);
//		}
	}

}
