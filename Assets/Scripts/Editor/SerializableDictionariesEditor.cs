﻿#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ScheduleTemplateDictionary))]
public class ScheduleTemplateDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(PhaseItemDictionary))]
public class PhaseItemDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(StringIntDictionary))]
public class StringIntDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(LandmarkDefenderWeightDictionary))]
public class LandmarkDefenderWeightDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(InteractionWeightDictionary))]
public class InteractionWeightDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(ActionCharacterTagListDictionary))]
public class ActionCharacterTagDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(BiomeLandmarkSpriteListDictionary))]
public class BiomeLandmarkSpriteListDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(RoleInteractionsListDictionary))]
public class RoleInteractionsListDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(TileSpriteCorruptionListDictionary))]
public class TileSpriteCorruptionListDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(RaceDefenderListDictionary))]
public class RaceDefenderListDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }


[CustomPropertyDrawer(typeof(CharacterTagListStorage))]
public class CharacterTagListStoragePropertyDrawer : SerializableDictionaryStoragePropertyDrawer { }
[CustomPropertyDrawer(typeof(LandmarkSpriteListStorage))]
public class LandmarkSpriteListStorageStoragePropertyDrawer : SerializableDictionaryStoragePropertyDrawer { }
[CustomPropertyDrawer(typeof(CharacterInteractionWeightListStorage))]
public class CharacterInteractionWeightListStoragePropertyDrawer : SerializableDictionaryStoragePropertyDrawer { }
[CustomPropertyDrawer(typeof(CorruptionObjectsListStorage))]
public class CorruptionObjectsListStoragePropertyDrawer : SerializableDictionaryStoragePropertyDrawer { }
[CustomPropertyDrawer(typeof(RaceDefenderListStorage))]
public class DefenderListStoragePropertyDrawer : SerializableDictionaryStoragePropertyDrawer { }
#endif
