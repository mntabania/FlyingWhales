using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using Ruinarch;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class StringIntDictionary : SerializableDictionary<string, int> { }
[System.Serializable]
public class BiomeLandmarkSpriteListDictionary : SerializableDictionary<BIOMES, List<LandmarkStructureSprite>, LandmarkSpriteListStorage> { }
[System.Serializable]
public class TileSpriteCorruptionListDictionary : SerializableDictionary<Sprite, List<GameObject>, CorruptionObjectsListStorage> { }
[System.Serializable]
public class RolePortraitFramesDictionary : SerializableDictionary<CHARACTER_ROLE, PortraitFrame> { }
[System.Serializable]
public class BiomeSpriteAnimationDictionary : SerializableDictionary<Sprite, RuntimeAnimatorController> { }
[System.Serializable]
public class LogReplacerDictionary : SerializableDictionary<string, LOG_IDENTIFIER> { }
[System.Serializable]
public class StringSpriteDictionary : SerializableDictionary<string, Sprite> { }
[System.Serializable]
public class FactionEmblemDictionary : SerializableDictionary<int, Sprite> { }
[System.Serializable]
public class CombatModeSpriteDictionary : SerializableDictionary<COMBAT_MODE, Sprite> { }
// [System.Serializable]
// public class ItemAsseteDictionary : SerializableDictionary<SPECIAL_TOKEN, Sprite> { }
[System.Serializable]
public class TileObjectAssetDictionary : SerializableDictionary<TILE_OBJECT_TYPE, TileObjectTileSetting> { }
[System.Serializable]
public class ArtifactDataDictionary : SerializableDictionary<ARTIFACT_TYPE, ArtifactData> { }
[System.Serializable]
public class ElementalDamageDataDictionary : SerializableDictionary<ELEMENTAL_TYPE, ElementalDamageData> { }
// [System.Serializable]
// public class ItemSpriteDictionary : SerializableDictionary<SPECIAL_TOKEN, Sprite> { }
[System.Serializable]
public class TileObjectBiomeAssetDictionary : SerializableDictionary<BIOMES, BiomeTileObjectTileSetting> { }
[System.Serializable]
public class TileObjectSlotDictionary : SerializableDictionary<Sprite, List<TileObjectSlotSetting>, TileObjectSlotListStorage> { }
[System.Serializable]
public class CursorTextureDictionary : SerializableDictionary<Ruinarch.InputManager.Cursor_Type, Texture2D> { }
[System.Serializable]
public class AreaTypeSpriteDictionary : SerializableDictionary<LOCATION_TYPE, Sprite> { }
[System.Serializable]
public class SummonSettingDictionary : SerializableDictionary<SUMMON_TYPE, SummonSettings> { }
[System.Serializable]
public class ArtifactSettingDictionary : SerializableDictionary<ARTIFACT_TYPE, ArtifactSettings> { }
[System.Serializable]
public class SeamlessEdgeAssetsDictionary : SerializableDictionary<LocationGridTile.Ground_Type, List<TileBase>, TileBaseListStorage> { }
[System.Serializable]
public class YieldTypeLandmarksDictionary : SerializableDictionary<LANDMARK_YIELD_TYPE, List<LANDMARK_TYPE>, LandmarkTypeListStorage> { }
[System.Serializable]
public class InterventionAbilityTierDictionary : SerializableDictionary<SPELL_TYPE, int> { }
[System.Serializable]
public class CharacterClassAssetDictionary : SerializableDictionary<string, CharacterClassAsset> { }
[System.Serializable]
public class LocationStructurePrefabDictionary : SerializableDictionary<STRUCTURE_TYPE, List<GameObject>, GameObjectListStorage> { }
[System.Serializable]
public class WallResourceAssetDictionary : SerializableDictionary<RESOURCE, WallResouceAssets> { }
[System.Serializable]
public class WallAssetDictionary : SerializableDictionary<string, WallAsset> { }
[System.Serializable]
public class ParticleEffectAssetDictionary : SerializableDictionary<PARTICLE_EFFECT, GameObject> { }

//List storage
[System.Serializable]
public class LandmarkSpriteListStorage : SerializableDictionary.Storage<List<LandmarkStructureSprite>> { }
[System.Serializable]
public class CorruptionObjectsListStorage : SerializableDictionary.Storage<List<GameObject>> { }
[System.Serializable]
public class TileObjectSlotListStorage : SerializableDictionary.Storage<List<TileObjectSlotSetting>> { }
[System.Serializable]
public class TileBaseListStorage : SerializableDictionary.Storage<List<TileBase>> { }
[System.Serializable]
public class LandmarkTypeListStorage : SerializableDictionary.Storage<List<LANDMARK_TYPE>> { }
[System.Serializable]
public class GameObjectListStorage : SerializableDictionary.Storage<List<GameObject>> { }