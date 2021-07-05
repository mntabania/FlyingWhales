using System.Collections.Generic;
using System.Linq;
using BayatGames.SaveGameFree.Types;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UnityEngine.Assertions;

[System.Serializable]
public class SaveDataLocationStructure : SaveData<LocationStructure> {
    public string persistentID;
    public int id;
    public string name;
    public string nameWithoutID;
    public STRUCTURE_TYPE structureType;
    public string regionLocationID;
    public STRUCTURE_TAG[] structureTags;
    public Point[] tileCoordinates;
    public int maxHP;
    public int currentHP;
    public List<string> residentIDs;
    public List<string> charactersHereIDs;
    public string occupiedAreaID;
    public string settlementLocationID;
    public bool isInterior;
    public SaveDataStructureRoom[] structureRoomSaveData;
    public bool hasBeenDestroyed;
    public bool isStoredAsTarget;
    public List<string> tileObjectDamageContributors;

    public override void Save(LocationStructure structure) {
        persistentID = structure.persistentID;
        id = structure.id;
        name = structure.name;
        nameWithoutID = structure.nameWithoutID;
        structureType = structure.structureType;
        regionLocationID = structure.region.persistentID;
        settlementLocationID = structure.settlementLocation?.persistentID ?? string.Empty;
        isStoredAsTarget = structure.isStoredAsTarget;

        //structure tags
        structureTags = new STRUCTURE_TAG[structure.structureTags.Count];
        for (int i = 0; i < structure.structureTags.Count; i++) {
            STRUCTURE_TAG structureTag = structure.structureTags[i];
            structureTags[i] = structureTag;
        }

        //tiles
        tileCoordinates = new Point[structure.tiles.Count];
        List<LocationGridTile> tiles = new List<LocationGridTile>(structure.tiles);
        for (int i = 0; i < tiles.Count; i++) {
            LocationGridTile tile = tiles[i];
            Point point = new Point(tile.localPlace.x, tile.localPlace.y);
            tileCoordinates[i] = point;
        }

        //hp
        maxHP = structure.maxHP;
        currentHP = structure.currentHP;
        
        //residents
        residentIDs = SaveUtilities.ConvertSavableListToIDs(structure.residents);

        //characters here
        charactersHereIDs = SaveUtilities.ConvertSavableListToIDs(structure.charactersHere);
        
        //occupied hex tile
        if (structure.occupiedArea != null) {
            occupiedAreaID = structure.occupiedArea.persistentID;    
        } else {
            occupiedAreaID = string.Empty;
            Debug.Log($"{structure.name} has no occupied area!");
        }
        
        isInterior = structure.isInterior;

        if (structure.rooms != null) {
            structureRoomSaveData = new SaveDataStructureRoom[structure.rooms.Length];
            for (int i = 0; i < structure.rooms.Length; i++) {
                StructureRoom structureRoom = structure.rooms[i];
                SaveDataStructureRoom saveDataStructureRoom = SaveUtilities.CreateSaveDataForRoom(structureRoom);
                saveDataStructureRoom.Save(structureRoom);
                structureRoomSaveData[i] = saveDataStructureRoom;
            }
        }

        hasBeenDestroyed = structure.hasBeenDestroyed;
        if (!structure.hasBeenDestroyed) {
            tileObjectDamageContributors = new List<string>();
            for (int i = 0; i < structure.objectsThatContributeToDamage.Count; i++) {
                IDamageable damageable = structure.objectsThatContributeToDamage.ElementAt(i);
                if (damageable is TileObject tileObject) {
                    tileObjectDamageContributors.Add(tileObject.persistentID);
                }
            }
        }
    }
    public LocationStructure InitialLoad(Region region) {
        return LandmarkManager.Instance.LoadNewStructureAt(region, structureType, this);
    }
}

public class SaveDataNaturalStructure : SaveDataLocationStructure {
    //No Unique data for now
}
public class SaveDataAnimalDen : SaveDataNaturalStructure {
    public string structureTemplateName;
    public Vector3Save structureObjectWorldPosition;
    public override void Save(LocationStructure structure) {
        base.Save(structure);
        AnimalDen animalDen = structure as AnimalDen;
        Assert.IsNotNull(animalDen);
        string templateName = animalDen.templateName;
        templateName = templateName.Replace("(Clone)", "");
        structureTemplateName = templateName;
        structureObjectWorldPosition = animalDen.structureObjectWorldPos;
    }
}

public class SaveDataManMadeStructure : SaveDataLocationStructure {

    public string structureTemplateName;
    public SaveDataTileObject[] structureWallObjects;
    public RESOURCE wallsMadeOf;
    public Vector3Save structureObjectWorldPosition;
    public SaveDataStructureConnector[] structureConnectors;
    public string[] assignedWorkerIDs;

    public string[] dirtyObjects;
    public GameDate scheduledDirtProduction;

    public override void Save(LocationStructure locationStructure) {
        base.Save(locationStructure);
        ManMadeStructure manMadeStructure = locationStructure as ManMadeStructure;
        Assert.IsNotNull(manMadeStructure);

        assignedWorkerIDs = manMadeStructure.assignedWorkerIDs.ToArray();
        if (manMadeStructure.hasBeenDestroyed) {
            structureTemplateName = string.Empty;
            structureObjectWorldPosition = Vector3.zero;
        } else {
            //structure object
            string templateName = manMadeStructure.templateName;
            templateName = templateName.Replace("(Clone)", "");
            structureTemplateName = templateName;
            structureObjectWorldPosition = manMadeStructure.structureObjectWorldPos;    
            structureConnectors = new SaveDataStructureConnector[manMadeStructure.structureObj.connectors.Length];
            for (int i = 0; i < structureConnectors.Length; i++) {
                StructureConnector connector = manMadeStructure.structureObj.connectors[i];
                SaveDataStructureConnector savedConnector = new SaveDataStructureConnector();
                savedConnector.Save(connector);
                structureConnectors[i] = savedConnector;
            }
        }
        
        //walls
        if (manMadeStructure.structureWalls != null) {
            structureWallObjects = new SaveDataTileObject[manMadeStructure.structureWalls.Count];
            for (int i = 0; i < manMadeStructure.structureWalls.Count; i++) {
                ThinWall structureWallObject = manMadeStructure.structureWalls[i];
                SaveDataTileObject saveDataStructureWallObject = new SaveDataTileObject();
                saveDataStructureWallObject.Save(structureWallObject);
                structureWallObjects[i] = saveDataStructureWallObject;
            }
            wallsMadeOf = manMadeStructure.wallsAreMadeOf;
        }

        if (manMadeStructure.dirtyObjects.Count > 0) {
            dirtyObjects = new string[manMadeStructure.dirtyObjects.Count];
            for (int i = 0; i < manMadeStructure.dirtyObjects.Count; i++) {
                TileObject dirtyObject = manMadeStructure.dirtyObjects[i];
                dirtyObjects[i] = dirtyObject.persistentID;
            }
        }
        if (manMadeStructure.scheduledDirtProduction.hasValue) {
            scheduledDirtProduction = manMadeStructure.scheduledDirtProduction;
        }
    }
}