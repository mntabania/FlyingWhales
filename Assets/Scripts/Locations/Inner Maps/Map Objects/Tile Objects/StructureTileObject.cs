﻿using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;

public class StructureTileObject : TileObject {

    public LocationStructure structureParent => gridTileLocation?.structure;
    
    public StructureTileObject() {
        Initialize(TILE_OBJECT_TYPE.STRUCTURE_TILE_OBJECT, false);
        AddAdvertisedAction(INTERACTION_TYPE.BUILD_BLUEPRINT);
        RemoveAdvertisedAction(INTERACTION_TYPE.STEAL_ANYTHING);
        traitContainer.RemoveTrait(this, "Flammable");
    }
    public StructureTileObject(SaveDataTileObject data) { }

    #region Overrides
    public override string ToString() {
        return $"Structure Tile Object {id.ToString()}";
    }
    public override bool CanBeDamaged() {
        return false;
    }
    public override bool CanBeSelected() {
        return false;
    }
    public override bool OccupiesTile() {
        return false;
    }
    public override void SetCharacterOwner(Character characterOwner) { } //do not set character owner of this
    #endregion

    // public void SetBuildingSpot(BuildingSpot spot) {
    //     this.spot = spot;
    // }
    // public void PlaceBlueprintOnBuildingSpot(STRUCTURE_TYPE structureType) {
    //     List<GameObject> choices = InnerMapManager.Instance.GetStructurePrefabsForStructure(structureType);
    //     GameObject chosenStructurePrefab = null;
    //     for (int i = 0; i < choices.Count; i++) {
    //         GameObject currPrefab = choices[i];
    //         LocationStructureObject so = currPrefab.GetComponent<LocationStructureObject>();
    //         if (spot.CanFitStructureOnSpot(so, gridTileLocation.parentMap.location.innerMap, "NPC")) {
    //             chosenStructurePrefab = currPrefab;
    //             break;
    //         }
    //
    //     }
    //     if (chosenStructurePrefab != null) {
    //         GameObject structurePrefab = ObjectPoolManager.Instance.InstantiateObjectFromPool(chosenStructurePrefab.name, Vector3.zero, Quaternion.identity, gridTileLocation.parentMap.structureParent);
    //         LocationStructureObject structureObject = structurePrefab.GetComponent<LocationStructureObject>();
    //         structurePrefab.transform.localPosition = spot.GetPositionToPlaceStructure(structureObject);
    //         
    //         structureObject.RefreshAllTilemaps();
    //         List<LocationGridTile> occupiedTiles = structureObject.GetTilesOccupiedByStructure(gridTileLocation.parentMap);
    //         for (int j = 0; j < occupiedTiles.Count; j++) {
    //             LocationGridTile tile = occupiedTiles[j];
    //             tile.SetHasBlueprint(true);
    //         }
    //         spot.SetIsOccupied(true);
    //         // spot.SetAllAdjacentSpotsAsOpen(gridTileLocation.parentMap);
    //         spot.UpdateAdjacentSpotsOccupancy(gridTileLocation.parentMap);
    //         structureObject.SetVisualMode(LocationStructureObject.Structure_Visual_Mode.Blueprint);
    //         spot.SetBlueprint(structureObject, structureType);
    //         structureObject.SetTilesInStructure(occupiedTiles.ToArray());
    //     } else {
    //         Debug.LogWarning($"Could not find a prefab for structure {structureType.ToString()} on build spot {spot.ToString()}");
    //     }
    // }
    // public LocationStructure BuildBlueprint(NPCSettlement npcSettlement) {
    //     spot.blueprint.SetVisualMode(LocationStructureObject.Structure_Visual_Mode.Built);
    //     LocationStructure structure = LandmarkManager.Instance.CreateNewStructureAt(gridTileLocation.structure.location, spot.blueprintType, npcSettlement);
    //
    //     spot.blueprint.ClearOutUnimportantObjectsBeforePlacement();
    //
    //     for (int j = 0; j < spot.blueprint.tiles.Length; j++) {
    //         LocationGridTile tile = spot.blueprint.tiles[j];
    //         tile.SetStructure(structure);
    //     }
    //     structure.SetStructureObject(spot.blueprint);
    //     spot.blueprint.PlacePreplacedObjectsAsBlueprints(structure, gridTileLocation.parentMap, npcSettlement);
    //     
    //     structure.SetOccupiedBuildSpot(this);
    //     spot.blueprint.OnStructureObjectPlaced(gridTileLocation.parentMap, structure);
    //     spot.ClearBlueprints();
    //
    //     npcSettlement.AddTileToSettlement(spot.hexTileOwner);
    //
    //     return structure;
    //     
    // }
    // public void RemoveOccupyingStructure(LocationStructure structure) {
    //     spot.SetIsOccupied(false);
    //     spot.UpdateAdjacentSpotsOccupancy(structure.location.innerMap);
    // }

}
