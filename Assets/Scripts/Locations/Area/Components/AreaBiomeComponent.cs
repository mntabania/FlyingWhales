using System;
using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

public class AreaBiomeComponent : AreaComponent {

    public Dictionary<BIOMES, int> biomeDictionary { get; }
    public BIOMES biomeType { get; private set; }
    
    public AreaBiomeComponent() {
        biomeDictionary = new Dictionary<BIOMES, int>();
        biomeType = BIOMES.NONE;
    }

    public void OnTileAddedToArea(LocationGridTile p_tile) {
        AddBiomeVoteToDictionary(p_tile.mainBiomeType);
    }
    public void OnTileInAreaChangedBiome(LocationGridTile p_tile, BIOMES p_biome) {
        RemoveBiomeVoteFromDictionary(p_biome);
        AddBiomeVoteToDictionary(p_tile.mainBiomeType);
    }

    private void AddBiomeVoteToDictionary(BIOMES p_biome) {
        if (!biomeDictionary.ContainsKey(p_biome)) {
            biomeDictionary.Add(p_biome, 0);
        }
        biomeDictionary[p_biome]++;
        UpdateBiomeBasedOnVotes();
    }
    private void RemoveBiomeVoteFromDictionary(BIOMES p_biome) {
        if (!biomeDictionary.ContainsKey(p_biome)) {
            biomeDictionary.Add(p_biome, 0);
        }
        biomeDictionary[p_biome]--;
        UpdateBiomeBasedOnVotes();
    }
    private void UpdateBiomeBasedOnVotes() {
        BIOMES previousBiomeType = biomeType;
        int highestVotes = Int32.MinValue;
        BIOMES majorityElevation = BIOMES.NONE;
        foreach (var kvp in biomeDictionary) {
            int currentVotes = kvp.Value;
            if (currentVotes > highestVotes) {
                majorityElevation = kvp.Key;
                highestVotes = currentVotes;
            }
        }
        biomeType = majorityElevation;
        if (previousBiomeType != biomeType) {
            BiomeDivision previousBiomeDivision = GridMap.Instance.mainRegion.biomeDivisionComponent.GetBiomeDivision(previousBiomeType);
            previousBiomeDivision?.RemoveArea(owner);
            BiomeDivision newBiomeDivision = GridMap.Instance.mainRegion.biomeDivisionComponent.GetBiomeDivision(biomeType);
            newBiomeDivision?.AddArea(owner);
        }
    }
}
