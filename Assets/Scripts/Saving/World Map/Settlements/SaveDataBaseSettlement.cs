using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BayatGames.SaveGameFree.Types;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using UnityEngine.Serialization;

[System.Serializable]
public abstract class SaveDataBaseSettlement {
    public int id;
    public LOCATION_TYPE locationType;
    public string name;
    public List<int> tileIDs;

    public List<SaveDataLocationStructure> structures;
    
    public virtual void Save(BaseSettlement baseSettlement) {
        id = baseSettlement.id;
        locationType = baseSettlement.locationType;
        name = baseSettlement.name;

        tileIDs = new List<int>();
        for (int i = 0; i < baseSettlement.tiles.Count; i++) {
            HexTile tile = baseSettlement.tiles[i];
            tileIDs.Add(tile.id);
        }

        // structures = new List<SaveDataLocationStructure>();
        // foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> kvp in baseSettlement.structures) {
        //     for (int i = 0; i < kvp.Value.Count; i++) {
        //         SaveDataLocationStructure data = new SaveDataLocationStructure();
        //         data.Save(kvp.Value[i]);
        //         structures.Add(data);
        //     }
        // }
    }
}
