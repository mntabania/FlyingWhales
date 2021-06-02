using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UtilityScripts;
namespace Inner_Maps.Location_Structures {
    public class Dwelling : ManMadeStructure {

        public int differentFoodPileKindsInDwelling { get; private set; }

        #region getters
        public override bool isDwelling => true;
        public override Type serializedData => typeof(SaveDataDwelling);
        #endregion

        public Dwelling(Region location) : base(STRUCTURE_TYPE.DWELLING, location) {
            maxResidentCapacity = 2;
            SetMaxHPAndReset(3500);
        }

        public Dwelling(Region location, SaveDataManMadeStructure data) : base(location, data) {
            maxResidentCapacity = 2;
            SetMaxHP(3500);
            SaveDataDwelling saveDataDwelling = data as SaveDataDwelling;
            differentFoodPileKindsInDwelling = saveDataDwelling.differentFoodPileKindsInDwelling;
        }

        #region Overrides
        protected override void OnAddResident(Character newResident) {
            base.OnAddResident(newResident);
            if (GameManager.Instance.gameHasStarted) {
                //only Update owners on add residency if resident is not from initial generation.
                ProcessAllTileObjects(t => {
                    if (t.isPreplaced) {
                        t.UpdateOwners();
                    }
                });
                //List<TileObject> objs = ProcessAllTileObjects();
                //for (int i = 0; i < objs.Count; i++) {
                //    TileObject obj = objs[i];
                //    if (obj.isPreplaced) {
                //        //only update owners of objects that were preplaced
                //        obj.UpdateOwners();    
                //    }
                //}    
            }
        }

        //Removed this because the unowning of items is in the virtual function now
        //protected override void OnRemoveResident(Character newResident) {
        //    base.OnRemoveResident(newResident);
        //    List<TileObject> objs = GetTileObjects();
        //    for (int i = 0; i < objs.Count; i++) {
        //        TileObject obj = objs[i];
        //        if (obj.isPreplaced) {
        //            //only update owners of objects that were preplaced
        //            obj.UpdateOwners();
        //        }
        //    }
        //}
        public override bool AddPOI(IPointOfInterest poi, LocationGridTile tileLocation = null) {
            if (base.AddPOI(poi, tileLocation)) {
                if (poi is TileObject tileObject && poi.gridTileLocation != null) {
                    if (tileObject is FoodPile foodPile && GetTileObjectsOfType(foodPile.tileObjectType).Count == 1) {
                        differentFoodPileKindsInDwelling++;
                    }
                    for (int i = 0; i < residents.Count; i++) {
                        Character resident = residents[i];
                        resident.eventDispatcher.ExecuteObjectPlacedInCharactersDwelling(resident, this, tileObject);
                    }
                }
                return true;
            }
            return false;
        }
        public override bool RemovePOI(IPointOfInterest poi, Character removedBy = null, bool isPlayerSource = false) {
            if (base.RemovePOI(poi, removedBy, isPlayerSource)) {
                if (poi is TileObject tileObject) {
                    if (tileObject is FoodPile foodPile && GetTileObjectsOfType(foodPile.tileObjectType).Count <= 0) {
                        differentFoodPileKindsInDwelling--;
                    }
                    for (int i = 0; i < residents.Count; i++) {
                        Character resident = residents[i];
                        resident.eventDispatcher.ExecuteObjectRemovedFromCharactersDwelling(resident, this, tileObject);
                    }
                }
                return true;
            }
            return false;
        }
        public override bool RemovePOIWithoutDestroying(IPointOfInterest poi) {
            if (base.RemovePOIWithoutDestroying(poi)) {
                if (poi is TileObject tileObject) {
                    if (tileObject is FoodPile foodPile && GetTileObjectsOfType(foodPile.tileObjectType).Count <= 0) {
                        differentFoodPileKindsInDwelling--;
                    }
                    for (int i = 0; i < residents.Count; i++) {
                        Character resident = residents[i];
                        resident.eventDispatcher.ExecuteObjectRemovedFromCharactersDwelling(resident, this, tileObject);
                    }
                }
                return true;
            }
            return false;
        }
        public override bool RemovePOIDestroyVisualOnly(IPointOfInterest poi, Character remover = null) {
            if (base.RemovePOIDestroyVisualOnly(poi, remover)) {
                if (poi is TileObject tileObject) {
                    if (tileObject is FoodPile foodPile && GetTileObjectsOfType(foodPile.tileObjectType).Count <= 0) {
                        differentFoodPileKindsInDwelling--;
                    }
                    for (int i = 0; i < residents.Count; i++) {
                        Character resident = residents[i];
                        resident.eventDispatcher.ExecuteObjectRemovedFromCharactersDwelling(resident, this, tileObject);
                    }
                }
                return true;
            }
            return false;
        }
        public override bool CanBeResidentHere(Character character) {
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
        #endregion

        #region Tile Objects
        public void OnTileObjectInDwellingSetAsUnbuilt(TileObject p_tileObject) {
            for (int i = 0; i < residents.Count; i++) {
                Character resident = residents[i];
                resident.eventDispatcher.ExecuteObjectRemovedFromCharactersDwelling(resident, this, p_tileObject);
            }
        }
        public void OnTileObjectInDwellingSetAsBuilt(TileObject p_tileObject) {
            for (int i = 0; i < residents.Count; i++) {
                Character resident = residents[i];
                resident.eventDispatcher.ExecuteObjectPlacedInCharactersDwelling(resident, this, p_tileObject);
            }
        }
        #endregion

        #region Misc
        public override string GetNameRelativeTo(Character character) {
            if (character.homeStructure == this) {
                //- Dwelling where Actor Resides: "at [his/her] home"
                return $"{UtilityScripts.Utilities.GetPronounString(character.gender, PRONOUN_TYPE.POSSESSIVE, false)} home";
            } else if (residents.Count > 0) {
                //- Dwelling where Someone else Resides: "at [Resident Name]'s home"
                string residentSummary = residents[0].name;
                for (int i = 1; i < residents.Count; i++) {
                    if (i + 1 == residents.Count) {
                        residentSummary = $"{residentSummary} and ";
                    } else {
                        residentSummary = $"{residentSummary}, ";
                    }
                    residentSummary = $"{residentSummary}{residents[i].name}";
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
        #endregion
    }
}

#region Save Data
public class SaveDataDwelling : SaveDataManMadeStructure {
    public int differentFoodPileKindsInDwelling;
    public override void Save(LocationStructure locationStructure) {
        base.Save(locationStructure);
        Dwelling dwelling = locationStructure as Dwelling;
        differentFoodPileKindsInDwelling = dwelling.differentFoodPileKindsInDwelling;
    }
}
#endregion