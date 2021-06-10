namespace Inner_Maps.Location_Structures {
    public class Cemetery : ManMadeStructure {
        public Cemetery(Region location) : base(STRUCTURE_TYPE.CEMETERY, location) {
            wallsAreMadeOf = RESOURCE.WOOD;
        }
        public Cemetery(Region location, SaveDataManMadeStructure data) : base(location, data) { 
            wallsAreMadeOf = RESOURCE.WOOD;
        }
        
        #region Damage
        public override void OnTileDamaged(LocationGridTile tile, int amount, bool isPlayerSource) {
            //cemeteries can be damaged  by any tile
            AdjustHP(amount, isPlayerSource: isPlayerSource);
            OnStructureDamaged();
        }
        #endregion

        #region Destruction
        protected override void AfterStructureDestruction(Character p_responsibleCharacter = null) {
            base.AfterStructureDestruction(p_responsibleCharacter);
            //when a cemetery is destroyed check if bury jobs are still valid,
            //since settlement bury jobs will only bury residents if there are no more cemeteries 
            Messenger.Broadcast(JobSignals.CHECK_JOB_APPLICABILITY_OF_ALL_JOBS_OF_TYPE, JOB_TYPE.BURY);
        }
        #endregion

        #region Building
        public override void OnBuiltNewStructure() {
            //when a cemetery is built check if dead characters inside the settlement should create bury jobs.
            Messenger.Broadcast(CharacterSignals.TRY_CREATE_BURY_JOBS, settlementLocation);
        }
        #endregion
        
    }
}