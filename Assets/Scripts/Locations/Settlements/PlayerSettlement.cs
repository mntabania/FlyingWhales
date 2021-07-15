using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using UtilityScripts;

namespace Locations.Settlements {
    public class PlayerSettlement : BaseSettlement {

        #region getters
        public override Type serializedData => typeof(SaveDataPlayerSettlement);
        #endregion
        
        public PlayerSettlement() : base(LOCATION_TYPE.DEMONIC_INTRUSION) { }
        public PlayerSettlement(SaveDataPlayerSettlement saveDataBaseSettlement) : base(saveDataBaseSettlement) { }


        #region Residents
        public override void AssignCharacterToDwellingInArea(Character character, LocationStructure dwellingOverride = null) {
            if (structures == null) {
                Debug.LogWarning(
                    $"{name} doesn't have any dwellings for {character.name} because structures have not been generated yet");
                return;
            }
            if (!character.isFactionless && !structures.ContainsKey(STRUCTURE_TYPE.DWELLING)) {
                Debug.LogWarning($"{name} doesn't have any dwellings for {character.name}");
                return;
            }
            if (character.isFactionless) {
                character.SetHomeStructure(null);
                return;
            }
            LocationStructure chosenDwelling = dwellingOverride;
            if (chosenDwelling == null) {
                if (PlayerManager.Instance != null && PlayerManager.Instance.player != null && id == PlayerManager.Instance.player.playerSettlement.id) {
                    chosenDwelling = structures[STRUCTURE_TYPE.DWELLING][0]; //to avoid errors, residents in player npcSettlement will all share the same dwelling
                }
            }
            if (chosenDwelling == null) {
                //if the code reaches here, it means that the npcSettlement could not find a dwelling for the character
                Debug.LogWarning(
                    $"{GameManager.Instance.TodayLogString()}Could not find a dwelling for {character.name} at {name}, setting home to Town Center");
                chosenDwelling = GetRandomStructureOfType(STRUCTURE_TYPE.CITY_CENTER);
            }
            character.ChangeHomeStructure(chosenDwelling);
        }
        protected override bool IsResidentsFull() {
            return false; //resident capacity is never full for player npcSettlement
        }
        #endregion

        //#region Loading
        //public override void LoadReferences(SaveDataBaseSettlement data) {
        //    base.LoadReferences(data);
        //    //Update tile nameplates
        //    //Fix for: https://trello.com/c/gAqpeACf/3194-loading-the-game-erases-the-faction-symbol-on-the-world-map
        //    for (int i = 0; i < areas.Count; i++) {
        //        Area area = areas[i];
        //        area.landmarkOnTile?.nameplate.UpdateVisuals();
        //    }    
        //}
        //#endregion

        public LocationStructure GetRandomStructureInRegion(Region region) {
            List<LocationStructure> structuresInRegion = null;
            for (int i = 0; i < allStructures.Count; i++) {
                LocationStructure structure = allStructures[i];
                if(structure.region == region) {
                    if(structuresInRegion == null) { structuresInRegion = new List<LocationStructure>(); }
                    structuresInRegion.Add(structure);
                }
            }
            if(structuresInRegion != null) {
                return UtilityScripts.CollectionUtilities.GetRandomElement(structuresInRegion);
            }
            return null;
        }

        public bool HasAvailableKennelForSnatch() {
            if (HasStructure(STRUCTURE_TYPE.KENNEL)) {
                List<LocationStructure> kennels = GetStructuresOfType(STRUCTURE_TYPE.KENNEL);
                for (int i = 0; i < kennels.Count; i++) {
                    LocationStructure structure = kennels[i];
                    if (structure is Kennel kennel) {
                        if (!kennel.HasReachedKennelCapacity()) { //&& kennel.activeSnatchJobs < kennel.GetAvailableCapacity()
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        public bool HasAvailablePrisonForSnatch() {
            if (HasStructure(STRUCTURE_TYPE.TORTURE_CHAMBERS)) {
                List<LocationStructure> structuresOfType = GetStructuresOfType(STRUCTURE_TYPE.TORTURE_CHAMBERS);
                for (int i = 0; i < structuresOfType.Count; i++) {
                    LocationStructure structure = structuresOfType[i];
                    if (structure is DemonicStructure demonicStructure) {
                        if (demonicStructure.HasUnoccupiedRoom()) { //demonicStructure.activeSnatchJobs < demonicStructure.GetUnoccupiedRoomCount()
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        public bool HasAvailableDefilerForSnatch() {
            if (HasStructure(STRUCTURE_TYPE.DEFILER)) {
                List<LocationStructure> structuresOfType = GetStructuresOfType(STRUCTURE_TYPE.DEFILER);
                for (int i = 0; i < structuresOfType.Count; i++) {
                    LocationStructure kennel = structuresOfType[i];
                    if (kennel is DemonicStructure demonicStructure) {
                        if (demonicStructure.HasUnoccupiedRoom()) {//demonicStructure.activeSnatchJobs < demonicStructure.GetUnoccupiedRoomCount()
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        #region Loading
        public override void LoadReferences(SaveDataBaseSettlement data) {
            base.LoadReferences(data);
            if (data is SaveDataPlayerSettlement dataSettlement) {
                List<Area> areas = RuinarchListPool<Area>.Claim();
                GameUtilities.PopulateAreasGivenCoordinates(areas, dataSettlement.tileCoordinates, GridMap.Instance.map);
                for (int i = 0; i < areas.Count; i++) {
                    Area a = areas[i];
                    AddAreaToSettlement(a);
                }
                RuinarchListPool<Area>.Release(areas);
            }
        }
        #endregion
    }

}