﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ScheduleTemplateDictionary : SerializableDictionary<string, CharacterScheduleTemplate> { }
[System.Serializable]
public class PhaseItemDictionary : SerializableDictionary<SCHEDULE_ACTION_CATEGORY, int> { }
[System.Serializable]
public class StringIntDictionary : SerializableDictionary<string, int> { }
[System.Serializable]
public class LandmarkDefenderWeightDictionary : SerializableDictionary<LandmarkDefender, int> { }
[System.Serializable]
public class InteractionWeightDictionary : SerializableDictionary<INTERACTION_TYPE, int> { }
[System.Serializable]
public class ActionCharacterTagListDictionary : SerializableDictionary<ACTION_TYPE, List<CharacterActionTagRequirement>, CharacterTagListStorage> { }
[System.Serializable]
public class BiomeLandmarkSpriteListDictionary : SerializableDictionary<BIOMES, List<LandmarkStructureSprite>, LandmarkSpriteListStorage> { }

[System.Serializable]
public class CharacterTagListStorage : SerializableDictionary.Storage<List<CharacterActionTagRequirement>> { }
[System.Serializable]
public class LandmarkSpriteListStorage : SerializableDictionary.Storage<List<LandmarkStructureSprite>> { }