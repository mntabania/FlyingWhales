using System;
using System.Collections.Generic;
using Inner_Maps;
using UtilityScripts;

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
        List<ELEVATION> presentElevations = RuinarchListPool<ELEVATION>.Claim();
        foreach (var kvp in elevationDictionary) {
            if (kvp.Value > 0) {
                presentElevations.Add(kvp.Key);
            }
        }
        elevationType = presentElevations.Count == 1 ? presentElevations[0] : ELEVATION.MIXED;
        RuinarchListPool<ELEVATION>.Release(presentElevations);
    }
}
