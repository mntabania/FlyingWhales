using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;

public class Necronomicon : Artifact {

    public Necronomicon() : base(ARTIFACT_TYPE.Necronomicon) {
    }
    public Necronomicon(SaveDataArtifact data) : base(data) {
    }

    #region Overrides
    public override void ActivateTileObject() {
        if (gridTileLocation != null) {
            base.ActivateTileObject();
            List<LocationGridTile> tilesInRange = gridTileLocation.GetTilesInRadius(1);
            LocationGridTile tile1 = null;
            LocationGridTile tile2 = null;
            LocationGridTile tile3 = null;

            int index1 = UnityEngine.Random.Range(0, tilesInRange.Count);
            tile1 = tilesInRange[index1];
            tilesInRange.RemoveAt(index1);

            tile2 = tile1;
            tile3 = tile1;

            if(tilesInRange.Count > 0) {
                int index2 = UnityEngine.Random.Range(0, tilesInRange.Count);
                tile2 = tilesInRange[index2];
                tilesInRange.RemoveAt(index2);
            }
            if (tilesInRange.Count > 0) {
                int index3 = UnityEngine.Random.Range(0, tilesInRange.Count);
                tile3 = tilesInRange[index3];
            }

            Character skeleton1 = CharacterManager.Instance.CreateNewSummon(SUMMON_TYPE.Skeleton, FactionManager.Instance.neutralFaction, homeRegion: gridTileLocation.parentMap.region, className: "Marauder");
            Character skeleton2 = CharacterManager.Instance.CreateNewSummon(SUMMON_TYPE.Skeleton, FactionManager.Instance.neutralFaction, homeRegion: gridTileLocation.parentMap.region, className: "Archer");
            Character skeleton3 = CharacterManager.Instance.CreateNewSummon(SUMMON_TYPE.Skeleton, FactionManager.Instance.neutralFaction, homeRegion: gridTileLocation.parentMap.region, className: "Mage");
            skeleton1.CreateMarker();
            skeleton2.CreateMarker();
            skeleton3.CreateMarker();
            skeleton1.InitialCharacterPlacement(tile1);
            skeleton2.InitialCharacterPlacement(tile2);
            skeleton3.InitialCharacterPlacement(tile3);

            gridTileLocation.structure.RemovePOI(this);
        }
    }
    #endregion
}
