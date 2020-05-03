using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Inner_Maps.Location_Structures {
    public class Cave : NaturalStructure {

        //#region getters
        //public override bool isDwelling => true;
        //public List<Character> residents => null;
        //#endregion

        public Cave(Region location)
            : base(STRUCTURE_TYPE.CAVE, location) {
        }

        public Cave(Region location, SaveDataLocationStructure data)
            : base(location, data) {
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
        public override void SetStructureObject(LocationStructureObject structureObj) {
            base.SetStructureObject(structureObj);
            Vector3 position = structureObj.transform.position;
            worldPosition = position;
        }
    }
}