﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Relative : RelationshipTrait {
        //public override string nameInUI {
        //    get { return "Relative: " + targetCharacter.name; }
        //}

        public Relative(Character target) : base(target) {
            name = "Relative";
            description = "This character is a relative of " + targetCharacter.name;
            type = TRAIT_TYPE.RELATIONSHIP;
            effect = TRAIT_EFFECT.NEUTRAL;
            relType = RELATIONSHIP_TYPE.RELATIVE;
            
            ticksDuration = 0;
            //effects = new List<TraitEffect>();
        }

        #region overrides
        public override bool IsUnique() {
            return false;
        }
        public override string GetNameInUI(ITraitable traitable) {
            return "Relative: " + traitable.name;
        }
        #endregion
    }
}
