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
    public List<StructureConnector> structureConnectors { get; private set; }
    
    private AutoDestroyParticle _buildParticles;

    public AreaStructureComponent() {
        structures = new List<LocationStructure>();
        structureConnectors = new List<StructureConnector>();
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
    public bool HasStructureInArea(List<STRUCTURE_TYPE> p_structureTypes) {
        for (int i = 0; i < structures.Count; i++) {
            LocationStructure structure = structures[i];
            if(p_structureTypes.Contains(structure.structureType)) {
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
                if (mostImportant == null || value > mostImportant.structureType.StructurePriority()) {
                    mostImportant = structure;
                }
            }
        }
        if(mostImportant == null) {
            mostImportant = owner.region.wilderness;
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

    #region Building
    public void StartBuild(PLAYER_SKILL_TYPE structureType) {
        _buildParticles = GameManager.Instance.CreateParticleEffectAt(owner.gridTileComponent.centerGridTile, PARTICLE_EFFECT.Build_Demonic_Structure).GetComponent<AutoDestroyParticle>();
        DemonicStructurePlayerSkill demonicStructureSkill = PlayerSkillManager.Instance.GetDemonicStructureSkillData(structureType);
        demonicStructureSkill.OnExecutePlayerSkill();
        LandmarkManager.Instance.StartCoroutine(BuildCoroutine(structureType, owner));
        PlayerManager.Instance.player.SetIsCurrentlyBuildingDemonicStructure(true);
    }
    private IEnumerator BuildCoroutine(PLAYER_SKILL_TYPE structureType, Area p_area) {
        yield return new WaitForSeconds(3f);
        _buildParticles.StopEmission();
        DemonicStructurePlayerSkill demonicStructureSkill = PlayerSkillManager.Instance.GetDemonicStructureSkillData(structureType);
        // demonicStructureSkill.BuildDemonicStructureAt(p_area);
        _buildParticles = null;
        PlayerManager.Instance.player.SetIsCurrentlyBuildingDemonicStructure(false);
    }
    public bool CanBuildDemonicStructureHere(STRUCTURE_TYPE structureType, out string o_cannotBuildReason) {
        if (InnerMapManager.Instance.currentlyShowingLocation == null && structureType != STRUCTURE_TYPE.THE_PORTAL) {
            //allow portal to be built while no inner map is shown, because portal is build on the overworld
            o_cannotBuildReason = string.Empty;
            return false;
        }
        if (structureType == STRUCTURE_TYPE.THE_PORTAL) {
            if (CanBuildDemonicStructureHere(out o_cannotBuildReason)) {
                if (owner.settlementOnArea != null && owner.settlementOnArea.locationType == LOCATION_TYPE.VILLAGE) {
                    o_cannotBuildReason = LocalizationManager.Instance.GetLocalizedValue("Locations", "Areas", "invalid_build_portal_village");
                    return false;        
                } else {
                    return true;
                }
            }
            return false;
        }
        //NOTE: No longer 1 eye per region
        //if (structureType == STRUCTURE_TYPE.BEHOLDER) {
        //    if (CanBuildDemonicStructureHere(out o_cannotBuildReason)) {
        //        if (InnerMapManager.Instance.currentlyShowingLocation != null && InnerMapManager.Instance.currentlyShowingLocation.HasStructure(STRUCTURE_TYPE.BEHOLDER)) {
        //            o_cannotBuildReason = LocalizationManager.Instance.GetLocalizedValue("Locations", "Areas", "invalid_build_one_eye");
        //            return false;        
        //        } else {
        //            return true; //only 1 eye per region.    
        //        }
        //    }
        //    return false;
        //}
        if (structureType == STRUCTURE_TYPE.SPIRE) {
            if (CanBuildDemonicStructureHere(out o_cannotBuildReason)) {
                if (InnerMapManager.Instance.currentlyShowingLocation != null && InnerMapManager.Instance.currentlyShowingLocation.HasStructure(STRUCTURE_TYPE.SPIRE)) {
                    o_cannotBuildReason = LocalizationManager.Instance.GetLocalizedValue("Locations", "Areas", "invalid_build_one_SPIRE");
                    return false;
                } else {
                    return true; //only 1 eye per region.    
                }
            }
            return false;
        }
        if (structureType == STRUCTURE_TYPE.MEDDLER) {
            if (CanBuildDemonicStructureHere(out o_cannotBuildReason)) {
                if (PlayerManager.Instance.player.playerSettlement.HasStructure(STRUCTURE_TYPE.MEDDLER)) {
                    o_cannotBuildReason = LocalizationManager.Instance.GetLocalizedValue("Locations", "Areas", "invalid_build_one_meddler");
                    return false;   
                } else {
                    return true; //Only 1 meddler should exist in the world    
                }
            }
            return false;
        }
        if (structureType == STRUCTURE_TYPE.BIOLAB) {
            if (CanBuildDemonicStructureHere(out o_cannotBuildReason)) {
                if (PlayerManager.Instance.player.playerSettlement.HasStructure(STRUCTURE_TYPE.BIOLAB)) {
                    o_cannotBuildReason = o_cannotBuildReason = LocalizationManager.Instance.GetLocalizedValue("Locations", "Areas", "invalid_build_one_biolab");
                    return false;
                } else {
                    return true; //Only 1 biolab should exist in the world    
                }
            }
            return false;
        }
        return CanBuildDemonicStructureHere(out o_cannotBuildReason);
    }
    private bool CanBuildDemonicStructureHere(out string o_cannotBuildReason) {
        // if (owner.HasBlueprintOnTile()) {
        //     o_cannotBuildReason = LocalizationManager.Instance.GetLocalizedValue("Locations", "Areas", "invalid_build_has_blueprint");
        //     return false;
        // }
        if (PlayerManager.Instance.player != null && PlayerManager.Instance.player.isCurrentlyBuildingDemonicStructure) {
            o_cannotBuildReason = LocalizationManager.Instance.GetLocalizedValue("Locations", "Areas", "invalid_build_currently_building");
            return false;
        }
        // if (owner.settlementOnArea != null) { //|| HasStructureInArea()
        //     return false;
        // }
        if (_buildParticles != null) {
            o_cannotBuildReason = LocalizationManager.Instance.GetLocalizedValue("Locations", "Areas", "invalid_build_currently_building");
            return false;
        }
        // if(owner.elevationType == ELEVATION.WATER || owner.elevationType == ELEVATION.MOUNTAIN) {
        //     return false;
        // }
        o_cannotBuildReason = string.Empty;
        return true;
        // //Cannot build on settlements and hex tiles with blueprints right now
        // if(settlementOnTile == null && landmarkOnTile == null && elevationType != ELEVATION.WATER && elevationType != ELEVATION.MOUNTAIN && _buildParticles == null) {
        //     return true;
        // }
        // return false;
    }
    #endregion

    #region Structure Connectors
    public void AddStructureConnector(StructureConnector p_connector) {
        if (!structureConnectors.Contains(p_connector)) {
            structureConnectors.Add(p_connector);
        }
    }
    public void RemoveStructureConnector(StructureConnector p_connector) {
        structureConnectors.Remove(p_connector);
    }
    #endregion
}