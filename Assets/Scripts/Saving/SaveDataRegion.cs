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
    public RegionTemplate regionTemplate;
    public int[] residentIDs;
    public SaveDataInnerMap innerMapSave;

    public void Save(Region region) {
        persistentID = region.persistentID;
        id = region.id;
        name = region.name;
        coreTileID = region.coreTile.id;
        regionColor = region.regionColor;
        regionTemplate = region.regionTemplate;
        
        //residents
        residentIDs = new int[region.residents.Count];
        for (int i = 0; i < region.residents.Count; i++) {
            Character character = region.residents[i];
            residentIDs[i] = character.id;
        }
        innerMapSave = new SaveDataInnerMap();
        innerMapSave.Save(region.innerMap);
    }
}
