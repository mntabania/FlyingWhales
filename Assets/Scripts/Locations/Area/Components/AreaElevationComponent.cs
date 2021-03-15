using System;
using System.Collections.Generic;
using Inner_Maps;

public class AreaElevationComponent : AreaComponent {
    public Dictionary<ELEVATION, int> elevationDictionary { get; }
    public ELEVATION elevationType { get; private set; }

    public AreaElevationComponent() {
        elevationDictionary = new Dictionary<ELEVATION, int>();
    }

    public void OnTileAddedToArea(LocationGridTile p_tile) {
        AddElevationVoteToDictionary(p_tile.elevationType);
    }
    public void OnTileInAreaChangedElevation(LocationGridTile p_tile, ELEVATION p_oldElevation) {
        RemoveElevationVoteFromDictionary(p_oldElevation);
        AddElevationVoteToDictionary(p_tile.elevationType);
    }

    private void AddElevationVoteToDictionary(ELEVATION p_elevation) {
        if (!elevationDictionary.ContainsKey(p_elevation)) {
            elevationDictionary.Add(p_elevation, 0);
        }
        elevationDictionary[p_elevation]++;
        UpdateElevationBasedOnVotes();
    }
    private void RemoveElevationVoteFromDictionary(ELEVATION p_elevation) {
        if (!elevationDictionary.ContainsKey(p_elevation)) {
            elevationDictionary.Add(p_elevation, 0);
        }
        elevationDictionary[p_elevation]--;
        UpdateElevationBasedOnVotes();
    }

    private void UpdateElevationBasedOnVotes() {
        // if (elevationDictionary.ContainsKey(ELEVATION.MOUNTAIN) && elevationDictionary[ELEVATION.MOUNTAIN] >= 50) {
        //     elevationType = ELEVATION.MOUNTAIN; 
        // } else if (elevationDictionary.ContainsKey(ELEVATION.WATER) && elevationDictionary[ELEVATION.WATER] >= 50) {
        //     elevationType = ELEVATION.WATER; 
        // } else {
        //     elevationType = ELEVATION.PLAIN;
        // }
        int highestVotes = Int32.MinValue;
        ELEVATION elevationWithHighestVotes = ELEVATION.PLAIN;
        foreach (var kvp in elevationDictionary) {
            if (kvp.Value > highestVotes) {
                highestVotes = kvp.Value;
                elevationWithHighestVotes = kvp.Key;
            }
        }
        elevationType = elevationWithHighestVotes;
    }
}
