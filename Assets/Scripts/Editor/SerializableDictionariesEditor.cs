using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[CustomPropertyDrawer(typeof(StringIntDictionary))]
public class StringIntDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(StringFloatDictionary))]
public class StringFloatDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
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
// [CustomPropertyDrawer(typeof(ItemAsseteDictionary))]
// public class ItemTileBaseDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(TileObjectAssetDictionary))]
public class TileObjectTileBaseDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
// [CustomPropertyDrawer(typeof(ItemSpriteDictionary))]
// public class ItemSpriteDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
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
public class MinionSettingDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(MinionSettingDictionary))]
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
[CustomPropertyDrawer(typeof(ParticleEffectAssetDictionary))]
public class ParticleEffectAssetDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(SoundEffectDictionary))]
public class SoundEffectDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(ProjectileDictionary))]
public class ProjectileDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(TimeOfDayLightDictionary))]
public class TimeOfDayLightDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(BiomeHighlightColorDictionary))]
public class BiomeHighlightColorDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(SpriteSpriteDictionary))]
public class SpriteSpriteDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(BiomeMonsterDictionary))]
public class BiomeMonsterDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(BiomeItemDictionary))]
public class BiomeItemDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(PlayerSkillTreeNodeDictionary))]
public class PlayerSkillTreeNodeDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(PlayerSkillTreeNodeItemDictionary))]
public class PlayerSkillTreeNodeItemDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(PlayerSkillDataDictionary))]
public class PlayerSkillDataDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(PlayerArchetypeLoadoutDictionary))]
public class PlayerArchetypeLoadoutDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(LogTagSpriteDictionary))]
public class LogTagSpriteDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(RaceSpriteListDictionary))]
public class RaceSpriteListDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(RaceDataDictionary))]
public class RaceDataDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(MonsterMigrationBiomeDictionary))]
public class MonsterMigrationBiomeDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(ScenarioSettingsDataDictionary))]
public class ScenarioSettingsDataDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(StructureDataDictionary))]
public class StructureDataDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(IntChancesDictionary))]
public class IntChancesDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(PowerAndLevelDictionary))]
public class PowerAndLevelDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(TileTypeAssetDictionary))]
public class TileTypeAssetDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(TemperatureTypeDictionary))]
public class TemperatureTypeDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(PrecipitationTypeDictionary))]
public class PrecipitationTypeDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }

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
[CustomPropertyDrawer(typeof(ArtifactDataDictionary))]
public class ArtifactDataDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(ElementalDamageDataDictionary))]
public class ElementalDamageDataDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(MonsterSettingListStorage))]
public class MonsterTypeListStoragePropertyDrawer : SerializableDictionaryStoragePropertyDrawer { }
[CustomPropertyDrawer(typeof(TileObjectSettingListStorage))]
public class TileObjectTypeListStoragePropertyDrawer : SerializableDictionaryStoragePropertyDrawer { }
[CustomPropertyDrawer(typeof(AudioClipListStorage))]
public class AudioClipListStoragePropertyDrawer : SerializableDictionaryStoragePropertyDrawer { }
