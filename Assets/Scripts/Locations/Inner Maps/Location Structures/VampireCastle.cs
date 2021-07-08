using System.Collections.Generic;
namespace Inner_Maps.Location_Structures {
    public class VampireCastle : ManMadeStructure {
        public VampireCastle(Region location) : base(STRUCTURE_TYPE.VAMPIRE_CASTLE, location) { }
        public VampireCastle(Region location, SaveDataManMadeStructure data) : base(location, data) { }

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
                //for (int i = 0; i < objs.Count; i++) {
                //    TileObject obj = objs[i];
                //    if (obj.isPreplaced) {
                //        //only update owners of objects that were preplaced
                //        obj.UpdateOwners();    
                //    }
                //}    
            }
        }
        public override bool AddPOI(IPointOfInterest poi, LocationGridTile tileLocation = null) {
            if (base.AddPOI(poi, tileLocation)) {
                if (poi is TileObject tileObject && poi.gridTileLocation != null) {
                    for (int i = 0; i < residents.Count; i++) {
                        Character resident = residents[i];
                        resident.eventDispatcher.ExecuteObjectPlacedInCharactersHome(resident, this, tileObject);
                    }
                }
                return true;
            }
            return false;
        }
        public override bool RemovePOI(IPointOfInterest poi, Character removedBy = null, bool isPlayerSource = false) {
            if (base.RemovePOI(poi, removedBy, isPlayerSource)) {
                if (poi is TileObject tileObject) {
                    for (int i = 0; i < residents.Count; i++) {
                        Character resident = residents[i];
                        resident.eventDispatcher.ExecuteObjectRemovedFromCharactersHome(resident, this, tileObject);
                    }
                }
                return true;
            }
            return false;
        }
        public override bool RemovePOIWithoutDestroying(IPointOfInterest poi) {
            if (base.RemovePOIWithoutDestroying(poi)) {
                if (poi is TileObject tileObject) {
                    for (int i = 0; i < residents.Count; i++) {
                        Character resident = residents[i];
                        resident.eventDispatcher.ExecuteObjectRemovedFromCharactersHome(resident, this, tileObject);
                    }
                }
                return true;
            }
            return false;
        }
        public override bool RemovePOIDestroyVisualOnly(IPointOfInterest poi, Character remover = null) {
            if (base.RemovePOIDestroyVisualOnly(poi, remover)) {
                if (poi is TileObject tileObject) {
                    for (int i = 0; i < residents.Count; i++) {
                        Character resident = residents[i];
                        resident.eventDispatcher.ExecuteObjectRemovedFromCharactersHome(resident, this, tileObject);
                    }
                }
                return true;
            }
            return false;
        }
        #endregion
        
        #region Tile Objects
        public void OnTileObjectInDwellingSetAsUnbuilt(TileObject p_tileObject) {
            for (int i = 0; i < residents.Count; i++) {
                Character resident = residents[i];
                resident.eventDispatcher.ExecuteObjectRemovedFromCharactersHome(resident, this, p_tileObject);
            }
        }
        public void OnTileObjectInDwellingSetAsBuilt(TileObject p_tileObject) {
            for (int i = 0; i < residents.Count; i++) {
                Character resident = residents[i];
                resident.eventDispatcher.ExecuteObjectPlacedInCharactersHome(resident, this, p_tileObject);
            }
        }
        #endregion
    }
}