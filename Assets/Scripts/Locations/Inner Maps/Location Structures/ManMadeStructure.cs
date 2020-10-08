using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
namespace Inner_Maps.Location_Structures {
    public abstract class ManMadeStructure : LocationStructure {

        private StructureTileObject _structureTileObject;
        public List<StructureWallObject> structureWalls { get; private set; }

        public RESOURCE wallsAreMadeOf { get; protected set; }
        public LocationStructureObject structureObj {get; private set;}

        #region Getters
        public override Vector2 selectableSize => structureObj.size;
        #endregion
        
        protected ManMadeStructure(STRUCTURE_TYPE structureType, Region location) : base(structureType, location) { }
        protected ManMadeStructure(Region location, SaveDataLocationStructure data) : base(location, data) { }

        #region Listeners
        protected override void SubscribeListeners() {
            if (hasBeenDestroyed) { return; }
            Messenger.AddListener<StructureWallObject, int>(Signals.WALL_DAMAGED, OnWallDamaged);
            Messenger.AddListener<StructureWallObject, int>(Signals.WALL_REPAIRED, OnWallRepaired);
            Messenger.AddListener<IPointOfInterest, int>(Signals.OBJECT_DAMAGED, OnObjectDamaged);
            Messenger.AddListener<IPointOfInterest, int>(Signals.OBJECT_REPAIRED, OnObjectRepaired);
        }
        protected override void UnsubscribeListeners() {
            if (hasBeenDestroyed) { return; }
            Messenger.RemoveListener<StructureWallObject, int>(Signals.WALL_DAMAGED, OnWallDamaged);
            Messenger.RemoveListener<StructureWallObject, int>(Signals.WALL_REPAIRED, OnWallRepaired);
            Messenger.RemoveListener<IPointOfInterest, int>(Signals.OBJECT_DAMAGED, OnObjectDamaged);
            Messenger.RemoveListener<IPointOfInterest, int>(Signals.OBJECT_REPAIRED, OnObjectRepaired);
        }
        #endregion

        #region POIs
        public override bool AddPOI(IPointOfInterest poi, LocationGridTile tileLocation = null) {
            if (base.AddPOI(poi, tileLocation)) {
                if (poi is StructureTileObject structureTileObject) {
                    SetStructureTileObject(structureTileObject);
                }
                return true;
            }
            return false;
        }
        #endregion
        
        #region Structure Tile Object
        private void SetStructureTileObject(StructureTileObject structureTileObject) {
            _structureTileObject = structureTileObject;
        }
        #endregion
        
        #region HP
        private void OnWallRepaired(StructureWallObject structureWall, int amount) {
            if (structureWalls != null && structureWalls.Contains(structureWall)) {
                structureObj.RescanPathfindingGridOfStructure(region.innerMap);
                CheckInteriorState();
            }
            if (objectsThatContributeToDamage.Contains(structureWall)) {
                AdjustHP(amount);
            }
        }
        private void OnWallDamaged(StructureWallObject structureWall, int amount) {
            Assert.IsNotNull(structureObj, $"Wall of {this.ToString()} was damaged, but it has no structure object");
            if (structureWalls != null && structureWalls.Contains(structureWall)) {
                //create repair job
                structureObj.RescanPathfindingGridOfStructure(region.innerMap);
                OnStructureDamaged();
            }
            if (objectsThatContributeToDamage.Contains(structureWall)) {
                AdjustHP(amount);
            }
        }
        public override void OnTileRepaired(LocationGridTile tile, int amount) {
            if (hasBeenDestroyed) { return; }
            if (tile.genericTileObject.currentHP >= tile.genericTileObject.maxHP) {
                // ReSharper disable once Unity.NoNullPropagation
                structureObj?.ApplyGroundTileAssetForTile(tile);    
                tile.CreateSeamlessEdgesForSelfAndNeighbours();
            }
        }
        #endregion

        #region Utilities
        private void OnStructureDamaged() {
            if (structureType.IsOpenSpace() || structureType.IsSettlementStructure() == false) {
                return; //do not check for damage if structure is open space or structure is not a settlement structure
            }
            CheckInteriorState(); //check if the damage made this structure an exterior one.
            if (_structureTileObject != null) {
                //allow the structure tile object of this structure to advertise repair
                _structureTileObject.AddAdvertisedAction(INTERACTION_TYPE.REPAIR_STRUCTURE);    
                if (settlementLocation is NPCSettlement npcSettlement) {
                    //create a repair job if this structure is part of a settlement, and does not already have
                    //a repair job targeting it.
                    if (npcSettlement.HasJob(JOB_TYPE.REPAIR, _structureTileObject) == false) {
                        CreateRepairJob();
                    }    
                }
            }
        }
        private void CheckInteriorState() {
            //if structure object only has 70% or less of walls intact, set it as exterior, else, set it as interior
            int neededWallsToBeConsideredExterior = Mathf.FloorToInt(structureWalls.Count * 0.7f);
            int intactWalls = structureWalls.Count(wall => wall.currentHP > 0);
            SetInteriorState(intactWalls > neededWallsToBeConsideredExterior);
        }
        public override void CenterOnStructure() {
            if (InnerMapManager.Instance.isAnInnerMapShowing && InnerMapManager.Instance.currentlyShowingMap != region.innerMap) {
                InnerMapManager.Instance.HideAreaMap();
            }
            if (region.innerMap.isShowing == false) {
                InnerMapManager.Instance.ShowInnerMap(region);
            }
            if (structureObj != null) {
                InnerMapCameraMove.Instance.CenterCameraOn(structureObj.gameObject);
            } 
        }
        #endregion

        #region Repair
        private void CreateRepairJob() {
            Assert.IsNotNull(_structureTileObject, $"Repair job is being created for {this} but it does not have a structure tile object");
            if (settlementLocation is NPCSettlement npcSettlement) {
                GoapPlanJob repairJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.REPAIR, INTERACTION_TYPE.REPAIR_STRUCTURE, _structureTileObject, npcSettlement);
                npcSettlement.AddToAvailableJobs(repairJob);    
            }
        }
        #endregion

        #region Destroy
        protected override void DestroyStructure() {
            if (hasBeenDestroyed) {
                return;
            }
            if (_structureTileObject != null && settlementLocation is NPCSettlement npcSettlement) {
                Messenger.Broadcast(Signals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI, _structureTileObject as IPointOfInterest, ""); 
                JobQueueItem existingRepairJob = npcSettlement.GetJob(JOB_TYPE.REPAIR, _structureTileObject);
                 if (existingRepairJob != null) {
                     npcSettlement.RemoveFromAvailableJobs(existingRepairJob);
                 }
            }
            base.DestroyStructure();
        }
        protected override void AfterStructureDestruction() {
            structureObj.OnOwnerStructureDestroyed(region.innerMap); 
            base.AfterStructureDestruction();
        }
        #endregion

        #region Walls
        public void SetWallObjects(List<StructureWallObject> wallObjects, RESOURCE resource) {
            structureWalls = wallObjects;
            wallsAreMadeOf = resource;
        }
        #endregion

        #region Structure Object
        public virtual void SetStructureObject(LocationStructureObject structureObj) {
            this.structureObj = structureObj;
            Vector3 position = structureObj.transform.position;
            position.x -= 0.5f;
            position.y -= 0.5f;
            worldPosition = position;
        }
        #endregion
    }
}