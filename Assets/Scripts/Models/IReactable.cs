﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IReactable {
    string name { get; }
    string classificationName { get; }
    Character actor { get; }
    IPointOfInterest target { get; }
    Character disguisedActor { get; }
    Character disguisedTarget { get; }
    Log informationLog { get; }
    bool isStealth { get; }
    List<Character> awareCharacters { get; }
    string ReactionToActor(Character actor, IPointOfInterest target, Character witness, REACTION_STATUS status);
    string ReactionToTarget(Character actor, IPointOfInterest target, Character witness, REACTION_STATUS status);
    string ReactionOfTarget(Character actor, IPointOfInterest target, REACTION_STATUS status);
    void AddAwareCharacter(Character character);
    REACTABLE_EFFECT GetReactableEffect(Character witness);
}

public interface IRumorable {
    string name { get; }
    RUMOR_TYPE rumorType { get; }
    Character actor { get; }
    IPointOfInterest target { get; }
    Character disguisedActor { get; }
    Character disguisedTarget { get; }
    Log informationLog { get; }
    bool isStealth { get; }
    List<Character> awareCharacters { get; }
    void SetAsRumor(Rumor rumor);
    void AddAwareCharacter(Character character);
    string ReactionToActor(Character actor, IPointOfInterest target, Character witness, REACTION_STATUS status);
    string ReactionToTarget(Character actor, IPointOfInterest target, Character witness, REACTION_STATUS status);
    string ReactionOfTarget(Character actor, IPointOfInterest target, REACTION_STATUS status);
}
