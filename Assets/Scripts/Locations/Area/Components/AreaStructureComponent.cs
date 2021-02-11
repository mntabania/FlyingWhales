using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class AreaStructureComponent : AreaComponent {
    public List<LocationStructure> structures { get; private set; }

    public AreaStructureComponent() {
        structures = new List<LocationStructure>();
    }

    #region Utilities
    public bool AddStructureInArea(LocationStructure p_structure) {
        if (!structures.Contains(p_structure)) {
            structures.Add(p_structure);
            return true;
        }
        return false;
    }
    public bool RemoveStructureInArea(LocationStructure p_structure) {
        return structures.Remove(p_structure);
    }
    public bool HasStructureInArea() {
        return structures.Count > 0;
    }
    public bool HasStructureInArea(STRUCTURE_TYPE p_structureType) {
        for (int i = 0; i < structures.Count; i++) {
            LocationStructure structure = structures[i];
            if(structure.structureType == p_structureType) {
                return true;
            }
        }
        return false;
    }
    public LocationStructure GetMostImportantStructureOnTile() {
        LocationStructure mostImportant = null;
        for (int i = 0; i < structures.Count; i++) {
            LocationStructure structure = structures[i];
            if (structure.HasTileOnArea(owner)) {
                int value = structure.structureType.StructurePriority();
                if (value > mostImportant.structureType.StructurePriority()) {
                    mostImportant = structure;
                }
            }
        }
        if(mostImportant == null) {
            mostImportant = owner.region.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS);
        }
        return mostImportant;
        //foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> pair in region.structures) {
        //    for (int i = 0; i < pair.Value.Count; i++) {
        //        if (pair.Key == STRUCTURE_TYPE.WILDERNESS) {
        //            continue;
        //        }
        //        LocationStructure structure = pair.Value[i];
        //        if (structure.HasTileOnArea(this)) {
        //            int value = pair.Key.StructurePriority();
        //            if (value > mostImportant.structureType.StructurePriority()) {
        //                mostImportant = structure;
        //            }
        //        }

        //        // if (structure is Cave cave) {
        //        //     if (cave.occupiedHexTile != null && cave.caveHexTiles.Contains(innerMapHexTile)) {
        //        //         int value = pair.Key.StructurePriority(); 
        //        //         if (value > mostImportant.structureType.StructurePriority()) {
        //        //             mostImportant = structure;
        //        //         }    
        //        //     }
        //        // } else {
        //        //     if (structure.occupiedHexTile != null && structure.occupiedHexTile == innerMapHexTile) {
        //        //         int value = pair.Key.StructurePriority(); 
        //        //         if (value > mostImportant.structureType.StructurePriority()) {
        //        //             mostImportant = structure;
        //        //         }    
        //        //     }
        //        // }
        //    }
        //}

        //return mostImportant;
    }
    #endregion
}