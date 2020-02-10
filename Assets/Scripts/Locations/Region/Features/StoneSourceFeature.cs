﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoneSourceFeature : TileFeature {

    public StoneSourceFeature() {
        name = "Stone Source";
        description = "Provides stone.";
        type = REGION_FEATURE_TYPE.PASSIVE;
        isRemovedOnInvade = true;
    }  
}