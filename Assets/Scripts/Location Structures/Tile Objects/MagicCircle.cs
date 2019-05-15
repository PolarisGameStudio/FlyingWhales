﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MagicCircle : TileObject {
    public MagicCircle(LocationStructure location) {
        this.structureLocation = location;
        poiGoapActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.MAGIC_CIRCLE_PERFORM_RITUAL, INTERACTION_TYPE.TILE_OBJECT_DESTROY, };
        Initialize(TILE_OBJECT_TYPE.MAGIC_CIRCLE);
    }

    public override string ToString() {
        return "Magic Circle " + id.ToString();
    }
}
