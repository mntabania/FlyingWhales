using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BayatGames.SaveGameFree.Types;
using Inner_Maps.Location_Structures;
using UnityEngine.Assertions;

[System.Serializable]
public class SaveDataRegion : SaveData<Region> {
    public string persistentID;
    public int id;
    public string name;
    public int coreTileID;
    public ColorSave regionColor;
    public string[] residentIDs;
    public string[] charactersAtLocationIDs;
    public SaveDataInnerMap innerMapSave;
    public SaveDataVillageSpot[] villageSpots;
    //public Dictionary<GridNeighbourDirection, string> neighboursWithDirection;
    //public List<string> neighbours;
    public string[] factionsHereIDs;

    //Components
    public SaveDataRegionDivisionComponent regionDivisionComponent;
    public SaveDataGridTileFeatureComponent gridTileFeatureComponent;

    public override void Save(Region region) {
        persistentID = region.persistentID;
        id = region.id;
        name = region.name;
        coreTileID = region.coreTile.id;
        regionColor = region.regionColor;
        
        //residents
        residentIDs = new string[region.residents.Count];
        for (int i = 0; i < region.residents.Count; i++) {
            Character character = region.residents[i];
            residentIDs[i] = character.persistentID;
        }
        
        //characters at Location
        charactersAtLocationIDs = new string[region.charactersAtLocation.Count];
        for (int i = 0; i < region.charactersAtLocation.Count; i++) {
            Character character = region.charactersAtLocation[i];
            charactersAtLocationIDs[i] = character.persistentID;
        }

        //neighboursWithDirection = new Dictionary<GridNeighbourDirection, string>();
        //foreach (KeyValuePair<GridNeighbourDirection, Region> item in region.neighboursWithDirection) {
        //    neighboursWithDirection.Add(item.Key, item.Value.persistentID);
        //}

        //neighbours = new List<string>();
        //for (int i = 0; i < region.neighbours.Count; i++) {
        //    neighbours.Add(region.neighbours[i].persistentID);
        //}

        innerMapSave = new SaveDataInnerMap();
        innerMapSave.Save(region.innerMap);
        
        factionsHereIDs = new string[region.factionsHere.Count];
        for (int i = 0; i < region.factionsHere.Count; i++) {
            Faction factionHere = region.factionsHere[i];
            factionsHereIDs[i] = factionHere.persistentID;
        }

        villageSpots = new SaveDataVillageSpot[region.villageSpots.Count];
        for (int i = 0; i < region.villageSpots.Count; i++) {
            VillageSpot villageSpot = region.villageSpots[i];
            SaveDataVillageSpot saveDataVillageSpot = new SaveDataVillageSpot();
            saveDataVillageSpot.Save(villageSpot);
            villageSpots[i] = saveDataVillageSpot;
        }

        //Components
        regionDivisionComponent = new SaveDataRegionDivisionComponent(); regionDivisionComponent.Save(region.biomeDivisionComponent);
        gridTileFeatureComponent = new SaveDataGridTileFeatureComponent(); gridTileFeatureComponent.Save(region.gridTileFeatureComponent);
    }
}
