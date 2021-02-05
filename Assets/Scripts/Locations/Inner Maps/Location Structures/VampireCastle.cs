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
        #endregion
    }
}