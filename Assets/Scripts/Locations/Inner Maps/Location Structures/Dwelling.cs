using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UtilityScripts;
namespace Inner_Maps.Location_Structures {
    public class Dwelling : LocationStructure, IDwelling {

        public List<Character> residents { get; private set; }

        #region getters
        public override bool isDwelling => true;
        #endregion

        public Character owner {
            get { return residents.ElementAtOrDefault(0); }
        }

        //facilities
        public Dictionary<FACILITY_TYPE, int> facilities { get; protected set; }

        public Dwelling(Region location) 
            : base(STRUCTURE_TYPE.DWELLING, location) {
            residents = new List<Character>();
            InitializeFacilities();
        }

        public Dwelling(Region location, SaveDataLocationStructure data)
            : base(location, data) {
            residents = new List<Character>();
            InitializeFacilities();
        }


        #region Residents
        public void AddResident(Character character) {
            if (!residents.Contains(character)) {
                residents.Add(character);
                character.SetHomeStructure(this);
                List<TileObject> objs = GetTileObjects();
                for (int i = 0; i < objs.Count; i++) {
                    TileObject obj = objs[i];
                    obj.UpdateOwners();
                }
            }
        }
        public void RemoveResident(Character character) {
            if (residents.Remove(character)) {
                character.SetHomeStructure(null);
                List<TileObject> objs = GetTileObjects();
                for (int i = 0; i < objs.Count; i++) {
                    TileObject obj = objs[i];
                    obj.UpdateOwners();
                }
            }
        }
        public bool IsResident(Character character) {
            return residents.Contains(character);
            //for (int i = 0; i < residents.Count; i++) {
            //    if(residents[i].id == character.id) {
            //        return true;
            //    }
            //}
            //return false;
        }
        public override bool IsOccupied() {
            return residents.Count > 0;
        }
        public bool CanBeResidentHere(Character character) {
            //if (this.IsFull()) {
            //    return false;
            //}
            if (residents.Count == 0) {
                return true;
            } else {
                for (int i = 0; i < residents.Count; i++) {
                    Character currResident = residents[i];
                    List<RELATIONSHIP_TYPE> rels = currResident.relationshipContainer.GetRelationshipDataWith(character)?.relationships ?? null;
                    if (rels != null && rels.Contains(RELATIONSHIP_TYPE.LOVER)) {
                        return true;
                    }
                }
            }
            return false;
        }
        public bool HasPositiveRelationshipWithAnyResident(Character character) {
            if (residents.Contains(character)) {
                return true; //if the provided character is a resident of this dwelling, then yes, consider relationship as positive
            }
            for (int i = 0; i < residents.Count; i++) {
                Character currResident = residents[i];
                RELATIONSHIP_EFFECT effect = character.relationshipContainer.GetRelationshipEffectWith(currResident);
                if (effect == RELATIONSHIP_EFFECT.POSITIVE) {
                    return true;
                }
            }
            return false;
        }
        public bool HasEnemyOrNoRelationshipWithAnyResident(Character character) {
            //if (residents.Contains(character)) {
            //    return true; //if the provided character is a resident of this dwelling, then yes, consider relationship as positive
            //}
            for (int i = 0; i < residents.Count; i++) {
                Character currResident = residents[i];
                RELATIONSHIP_EFFECT effect = character.relationshipContainer.GetRelationshipEffectWith(currResident);
                if (effect == RELATIONSHIP_EFFECT.NEGATIVE || effect == RELATIONSHIP_EFFECT.NONE) {
                    return true;
                }
            }
            return false;
        }
        public override bool AddPOI(IPointOfInterest poi, LocationGridTile tileLocation = null, bool placeObject = true) {
            if (base.AddPOI(poi, tileLocation, placeObject)) {
                if (poi is TileObject) {
                    UpdateFacilityValues();
                }
                return true;
            }
            return false;
        }
        public override bool RemovePOI(IPointOfInterest poi, Character removedBy = null) {
            if (base.RemovePOI(poi, removedBy)) {
                if (poi is TileObject) {
                    UpdateFacilityValues();
                }
                return true;
            }
            return false;
        }
        #endregion

        #region Misc
        public override string GetNameRelativeTo(Character character) {
            if (character.homeStructure == this) {
                //- Dwelling where Actor Resides: "at [his/her] home"
                return
                    $"{UtilityScripts.Utilities.GetPronounString(character.gender, PRONOUN_TYPE.POSSESSIVE, false)} home";
            } else if (residents.Count > 0) {
                //- Dwelling where Someone else Resides: "at [Resident Name]'s home"
                string residentSummary = residents[0].name;
                for (int i = 1; i < residents.Count; i++) {
                    if (i + 1 == residents.Count) {
                        residentSummary += " and ";
                    } else {
                        residentSummary += ", ";
                    }
                    residentSummary += residents[i].name;
                }
                if (residentSummary.Last() == 's') {
                    return $"{residentSummary}' home";
                }
                return $"{residentSummary}'s home";
            } else {
                //- Dwelling where no one resides: "at an empty house"
                return "an empty house";
            }
        }
        public LocationStructure GetLocationStructure() {
            return this;
        }
        #endregion

        #region Facilities
        private void InitializeFacilities() {
            facilities = new Dictionary<FACILITY_TYPE, int>();
            FACILITY_TYPE[] facilityTypes = CollectionUtilities.GetEnumValues<FACILITY_TYPE>();
            for (int i = 0; i < facilityTypes.Length; i++) {
                if (facilityTypes[i] != FACILITY_TYPE.NONE) {
                    facilities.Add(facilityTypes[i], 0);
                }
            }
        }
        private void UpdateFacilityValues() {
            if (facilities == null) {
                return;
            }
            FACILITY_TYPE[] facilityTypes = CollectionUtilities.GetEnumValues<FACILITY_TYPE>();
            for (int i = 0; i < facilityTypes.Length; i++) {
                if (facilityTypes[i] != FACILITY_TYPE.NONE) {
                    facilities[facilityTypes[i]] = 0;
                }
            }
            List<TileObject> objects = GetTileObjects();
            for (int i = 0; i < objects.Count; i++) {
                TileObject currObj = objects[i];
                TileObjectData data;
                if (TileObjectDB.TryGetTileObjectData(currObj.tileObjectType, out data)) {
                    if (data.providedFacilities != null) {
                        for (int j = 0; j < data.providedFacilities.Length; j++) {
                            ProvidedFacility facility = data.providedFacilities[j];
                            facilities[facility.type] += facility.value;
                        }
                    }
                }
            }

        }
        public bool HasUnoccupiedFurnitureSpot() {
            for (int i = 0; i < tiles.Count; i++) {
                LocationGridTile currTile = tiles[i];
                if (currTile.objHere == null && currTile.hasFurnitureSpot) {
                    return true;
                }
            }
            return false;
        }
        public FACILITY_TYPE GetMostNeededValidFacility() {
            //get the facility with the lowest value, that can be provided given the unoccupied furnitureSpots
            int lowestValue = 99999;
            FACILITY_TYPE lowestFacility = FACILITY_TYPE.NONE;
            foreach (KeyValuePair<FACILITY_TYPE, int> keyValuePair in facilities) {
                if (keyValuePair.Value < lowestValue && HasUnoccupiedFurnitureSpotsThatCanProvide(keyValuePair.Key)) {
                    lowestValue = keyValuePair.Value;
                    lowestFacility = keyValuePair.Key;
                }
            }
            return lowestFacility;
        }
        public List<LocationGridTile> GetUnoccupiedFurnitureSpotsThatCanProvide(FACILITY_TYPE type) {
            List<LocationGridTile> validTiles = new List<LocationGridTile>();
            for (int i = 0; i < tiles.Count; i++) {
                LocationGridTile currTile = tiles[i];
                if (currTile.objHere == null && currTile.hasFurnitureSpot && currTile.furnitureSpot.allowedFurnitureTypes != null) {
                    for (int j = 0; j < currTile.furnitureSpot.allowedFurnitureTypes.Length; j++) {
                        FURNITURE_TYPE furnitureType = currTile.furnitureSpot.allowedFurnitureTypes[j];
                        TILE_OBJECT_TYPE tileObject = furnitureType.ConvertFurnitureToTileObject();
                        if (tileObject.CanProvideFacility(type)) {
                            validTiles.Add(currTile);
                            break;
                        }
                    }
                }
            }
            return validTiles;
        }
        private bool HasUnoccupiedFurnitureSpotsThatCanProvide(FACILITY_TYPE type) {
            for (int i = 0; i < tiles.Count; i++) {
                LocationGridTile currTile = tiles[i];
                if (currTile.objHere == null && currTile.hasFurnitureSpot && currTile.furnitureSpot.allowedFurnitureTypes != null) {
                    for (int j = 0; j < currTile.furnitureSpot.allowedFurnitureTypes.Length; j++) {
                        FURNITURE_TYPE furnitureType = currTile.furnitureSpot.allowedFurnitureTypes[j];
                        TILE_OBJECT_TYPE tileObject = furnitureType.ConvertFurnitureToTileObject();
                        if (tileObject.CanProvideFacility(type)) {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// Does this dwelling have any facilities that are at 0?
        /// </summary>
        /// <returns></returns>
        public bool HasFacilityDeficit() {
            foreach (KeyValuePair<FACILITY_TYPE, int> kvp in facilities) {
                if (kvp.Value <= 0) {
                    return true;
                }
            }
            return false;
        }
        #endregion
    }
}