﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class TileObjectTraitProcessor : TraitProcessor {

        public override void OnTraitAdded(ITraitable traitable, Trait trait, Character characterResponsible = null, ActualGoapNode gainedFromDoing = null) {
            TileObject obj = traitable as TileObject;
            DefaultProcessOnAddTrait(traitable, trait, characterResponsible, gainedFromDoing);
            obj.OnTileObjectGainedTrait(trait);
        }

        public override void OnTraitRemoved(ITraitable traitable, Trait trait, Character removedBy) {
            DefaultProcessOnRemoveTrait(traitable, trait, removedBy);
        }
    }

}
