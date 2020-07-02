using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldSettingsData {
    public int numOfRegions { get; private set; }
    public bool omnipotentMode { get; private set; }
    public bool noThreatMode { get; private set; }
    public bool chaosVictoryMode { get; private set; }
    public List<RACE> races { get; private set; }
    public List<BIOMES> biomes { get; private set; }

    public WorldSettingsData() {
        races = new List<RACE>();
        biomes = new List<BIOMES>();
    }

    public void SetNumOfRegions(int amount) {
        numOfRegions = amount;
    }
    public void SetOmnipotentMode(bool state) {
        omnipotentMode = state;
    }
    public void SetNoThreatMode(bool state) {
        noThreatMode = state;
    }
    public void SetChaosVictoryMode(bool state) {
        chaosVictoryMode = state;
    }

    public void AddRace(RACE race) {
        if (!races.Contains(race)) {
            races.Add(race);
        }
    }
    public bool RemoveRace(RACE race) {
        return races.Remove(race);
    }
    public void ClearRaces() {
        races.Clear();
    }

    public void AddBiome(BIOMES biome) {
        if (!biomes.Contains(biome)) {
            biomes.Add(biome);
        }
    }
    public bool RemoveBiome(BIOMES biome) {
        return biomes.Remove(biome);
    }
    public void ClearBiomes() {
        biomes.Clear();
    }
}
