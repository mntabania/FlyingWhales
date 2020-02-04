﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;

public interface IDwelling {
    int id { get; }
    string name { get; }
    STRUCTURE_TYPE structureType { get; }
    bool isDwelling { get; }
    List<Character> charactersHere { get; }
    List<Character> residents { get; }
    ILocation location { get; }
    Settlement settlementLocation { get; }
    List<SpecialToken> itemsInStructure { get; }
    List<IPointOfInterest> pointsOfInterest { get; }
    POI_STATE state { get; }
    LocationStructureObject structureObj { get; }
    BuildSpotTileObject occupiedBuildSpot { get; }

    //Inner Map
    List<LocationGridTile> tiles { get; }
    List<LocationGridTile> unoccupiedTiles { get; }
    LocationGridTile entranceTile { get; }

    void AddResident(Character character);
    void RemoveResident(Character character);
    string GetNameRelativeTo(Character character);
    bool IsResident(Character character);
    bool IsOccupied();
    bool CanBeResidentHere(Character character);
    bool HasPositiveRelationshipWithAnyResident(Character character);
    bool HasEnemyOrNoRelationshipWithAnyResident(Character character);
    bool AddPOI(IPointOfInterest poi, LocationGridTile tileLocation = null, bool placeObject = true);
    bool RemovePOI(IPointOfInterest poi, Character removedBy = null);
    bool HasUnoccupiedFurnitureSpot();
    bool HasFacilityDeficit();
    FACILITY_TYPE GetMostNeededValidFacility();
    LocationStructure GetLocationStructure();
    TileObject GetUnoccupiedTileObject(params TILE_OBJECT_TYPE[] type);
    List<LocationGridTile> GetUnoccupiedFurnitureSpotsThatCanProvide(FACILITY_TYPE type);
    List<TileObject> GetTileObjectsOfType(TILE_OBJECT_TYPE type);
    T GetTileObjectOfType<T>(TILE_OBJECT_TYPE type) where T : TileObject;
}
