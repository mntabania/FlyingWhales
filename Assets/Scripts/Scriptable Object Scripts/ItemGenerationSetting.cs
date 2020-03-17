using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "New Item Generation Setting", menuName = "Scriptable Objects/Item Generation")]
public class ItemGenerationSetting : ScriptableObject {
    [SerializeField] private IntRange _iterations;
    [SerializeField] private BiomeItemDictionary _itemChoices;
     
    #region getters
    public IntRange iterations => _iterations;
    #endregion

    public List<ItemSetting> GetItemChoicesForBiome(BIOMES biome) {
        List<ItemSetting> settings = new List<ItemSetting>();
        if (_itemChoices.ContainsKey(biome)) {
            settings.AddRange(_itemChoices[biome]);
        }
        if (_itemChoices.ContainsKey(BIOMES.NONE)) {
            settings.AddRange(_itemChoices[BIOMES.NONE]);
        }
        return settings;
    }
}

[System.Serializable]
public struct ItemSetting {
    public TILE_OBJECT_TYPE itemType;
    public IntRange minMaxRange;
}