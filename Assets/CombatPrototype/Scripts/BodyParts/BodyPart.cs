﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ECS{
	[System.Serializable]
	public class BodyPartsData {
		public RACE race;
		public List<BodyPart> bodyParts;
	}


	[System.Serializable]
	public class BodyPart: IBodyPart {
		[SerializeField] internal List<SecondaryBodyPart> secondaryBodyParts;

		//This applies status effect to all secondary body part of this main body part
		//Whatever status effect added to the main body part will be added to secondary body part since they are linked
		internal void ApplyStatusEffectOnSecondaryBodyParts(STATUS_EFFECT statusEffect){
			for (int i = 0; i < this.secondaryBodyParts.Count; i++) {
				this.secondaryBodyParts [i].statusEffects.Add (statusEffect);
			}
		}

		//This removes status effect to all secondary body part of this main body part
		internal void RemoveStatusEffectOnSecondaryBodyParts(STATUS_EFFECT statusEffect){
			for (int i = 0; i < this.secondaryBodyParts.Count; i++) {
				this.secondaryBodyParts [i].statusEffects.Remove (statusEffect);
			}
		}
        //		internal void SetData(BODY_PART bodyPart, IMPORTANCE importance, List<ATTRIBUTE> attributes, List<SecondaryBodyPart> secondaryBodyParts, STATUS status){
        //			this.bodyPart = bodyPart;
        //			this.importance = importance;
        //			this.attributes = new List<ATTRIBUTE> (attributes);
        //			this.secondaryBodyParts = new List<SecondaryBodyPart> (secondaryBodyParts);
        //			this.status = status;
        //		}

        #region Utilities
        internal BodyPart CreateNewCopy() {
            BodyPart newBodyPart = new BodyPart();
            newBodyPart.bodyPart = this.bodyPart;
            newBodyPart.importance = this.importance;
            newBodyPart.attributes = new List<BodyAttribute>();
            for (int i = 0; i < this.attributes.Count; i++) {
                BodyAttribute originalAttribute = this.attributes[i];
                BodyAttribute newAttribute = new BodyAttribute();
                newAttribute.attribute = originalAttribute.attribute;
                newAttribute.SetAttributeAsUsed(originalAttribute.isUsed);
                newBodyPart.attributes.Add(newAttribute);
            }
            newBodyPart.statusEffects = new List<STATUS_EFFECT>(this.statusEffects);
            newBodyPart.itemsAttached = new List<Item>();
            newBodyPart.secondaryBodyParts = new List<SecondaryBodyPart>();
            for (int i = 0; i < this.secondaryBodyParts.Count; i++) {
                SecondaryBodyPart originalSecondary = this.secondaryBodyParts[i];
                newBodyPart.secondaryBodyParts.Add(originalSecondary.CreateNewCopy());
            }
            return newBodyPart;
        }
        #endregion
    }
}

