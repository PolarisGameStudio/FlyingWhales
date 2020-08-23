﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;
using Locations.Settlements;

public interface IPartyTarget {
    LocationStructure currentStructure { get; }
    BaseSettlement currentSettlement { get; }
}
