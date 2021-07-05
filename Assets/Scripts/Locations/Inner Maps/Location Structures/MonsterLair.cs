﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Inner_Maps.Location_Structures {
    public class MonsterLair : NaturalStructure {

        //#region getters
        //public override bool isDwelling => true;
        //public List<Character> residents => null;
        //#endregion

        public MonsterLair(Region location) : base(STRUCTURE_TYPE.MONSTER_LAIR, location) {
            AddStructureTag(STRUCTURE_TAG.Dangerous);
            AddStructureTag(STRUCTURE_TAG.Treasure);
            AddStructureTag(STRUCTURE_TAG.Monster_Spawner);
        }

        public MonsterLair(Region location, SaveDataNaturalStructure data) : base(location, data) {
            AddStructureTag(STRUCTURE_TAG.Dangerous);
            AddStructureTag(STRUCTURE_TAG.Treasure);
            AddStructureTag(STRUCTURE_TAG.Monster_Spawner);
        }

        //public void AddResident(Character character) {
        //    //Not Applicable
        //    character.SetHomeStructure(this);
        //}
        //public void RemoveResident(Character character) {
        //    //Not Applicable
        //    character.SetHomeStructure(null);
        //}
        //public bool CanBeResidentHere(Character character) {
        //    return false;
        //}

        public FACILITY_TYPE GetMostNeededValidFacility() {
            return FACILITY_TYPE.NONE;
        }

        public List<LocationGridTile> GetUnoccupiedFurnitureSpotsThatCanProvide(FACILITY_TYPE type) {
            return null;
        }

        //public bool HasEnemyOrNoRelationshipWithAnyResident(Character character) {
        //    return false;
        //}

        public bool HasFacilityDeficit() {
            return false;
        }

        //public bool HasPositiveRelationshipWithAnyResident(Character character) {
        //    return false;
        //}

        //public bool HasUnoccupiedFurnitureSpot() {
        //    return false;
        //}

        //public bool IsResident(Character character) {
        //    return character.homeStructure == this;
        //}

        public LocationStructure GetLocationStructure() {
            return this;
        }
        public override void CenterOnStructure() {
            if (InnerMapManager.Instance.isAnInnerMapShowing && InnerMapManager.Instance.currentlyShowingMap != region.innerMap) {
                InnerMapManager.Instance.HideAreaMap();
            }
            if (region.innerMap.isShowing == false) {
                InnerMapManager.Instance.ShowInnerMap(region);
            }
            if (occupiedArea != null) {
                InnerMapCameraMove.Instance.CenterCameraOn(occupiedArea.gridTileComponent.centerGridTile.centeredWorldLocation);
            }
        }
        public override void ShowSelectorOnStructure() { }
        // public override void ShowSelectorOnStructure() {
        //     if (occupiedArea != null) {
        //         Selector.Instance.Select(occupiedArea);
        //     }
        // }
    }
}