﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAwareness {
    IPointOfInterest poi { get; }
    LocationGridTile knownGridLocation { get; }

    void SetKnownGridLocation(LocationGridTile tile);

    void OnAddAwareness(Character character);
    void OnRemoveAwareness(Character character);
}
