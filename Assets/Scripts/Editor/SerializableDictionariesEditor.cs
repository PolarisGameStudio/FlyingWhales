﻿using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[CustomPropertyDrawer(typeof(StringIntDictionary))]
public class StringIntDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(BiomeLandmarkSpriteListDictionary))]
public class BiomeLandmarkSpriteListDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(TileSpriteCorruptionListDictionary))]
public class TileSpriteCorruptionListDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(RolePortraitFramesDictionary))]
public class JobPortraitFramesDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(BiomeSpriteAnimationDictionary))]
public class BiomeSpriteAnimationDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(LogReplacerDictionary))]
public class LogReplacerDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(StringSpriteDictionary))]
public class LocationPortraitDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(FactionEmblemDictionary))]
public class FactionEmblemDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(ItemAsseteDictionary))]
public class ItemTileBaseDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(TileObjectAssetDictionary))]
public class TileObjectTileBaseDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(ItemSpriteDictionary))]
public class ItemSpriteDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(TileObjectBiomeAssetDictionary))]
public class TileObjectBiomeAssetDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(TileObjectSlotDictionary))]
public class TileObjectSlotDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(CursorTextureDictionary))]
public class CursorTextureDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(AreaTypeSpriteDictionary))]
public class AreaTypeSpriteDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(SummonSettingDictionary))]
public class SummonSettingDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(ArtifactSettingDictionary))]
public class ArtifactSettingDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(SeamlessEdgeAssetsDictionary))]
public class SeamlessEdgeAssetsDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(YieldTypeLandmarksDictionary))]
public class YieldTypeLandmarksDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(InterventionAbilityTierDictionary))]
public class InterventionAbilityTierDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(CharacterClassAssetDictionary))]
public class CharacterClassAssetDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(LocationStructurePrefabDictionary))]
public class LocationStructurePrefabDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(WallResourceAssetDictionary))]
public class WallResourceAssetDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(WallAssetDictionary))]
public class WallAssetDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }

//list storage
[CustomPropertyDrawer(typeof(LandmarkSpriteListStorage))]
public class LandmarkSpriteListStorageStoragePropertyDrawer : SerializableDictionaryStoragePropertyDrawer { }
[CustomPropertyDrawer(typeof(CorruptionObjectsListStorage))]
public class CorruptionObjectsListStoragePropertyDrawer : SerializableDictionaryStoragePropertyDrawer { }
[CustomPropertyDrawer(typeof(TileObjectSlotListStorage))]
public class TileObjectSlotListStoragePropertyDrawer : SerializableDictionaryStoragePropertyDrawer { }
[CustomPropertyDrawer(typeof(TileBaseListStorage))]
public class TileBaseListStoragePropertyDrawer : SerializableDictionaryStoragePropertyDrawer { }
[CustomPropertyDrawer(typeof(LandmarkTypeListStorage))]
public class LandmarkTypeListStoragePropertyDrawer : SerializableDictionaryStoragePropertyDrawer { }
[CustomPropertyDrawer(typeof(GameObjectListStorage))]
public class GameObjectListStoragePropertyDrawer : SerializableDictionaryStoragePropertyDrawer { }

