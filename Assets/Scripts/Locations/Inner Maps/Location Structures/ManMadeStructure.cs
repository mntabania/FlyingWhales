using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
namespace Inner_Maps.Location_Structures {
    public abstract class ManMadeStructure : LocationStructure {

        private StructureTileObject _structureTileObject;
        public List<ThinWall> structureWalls { get; private set; }

        public RESOURCE wallsAreMadeOf { get; protected set; }
        public LocationStructureObject structureObj {get; private set;}
        public string assignedWorkerID { get; private set; }

        #region Getters
        public override Vector2 selectableSize => structureObj.size;
        public override System.Type serializedData => typeof(SaveDataManMadeStructure);
        public Character assignedWorker => string.IsNullOrEmpty(assignedWorkerID) ? null : CharacterManager.Instance.GetCharacterByPersistentID(assignedWorkerID);
        #endregion

        protected ManMadeStructure(STRUCTURE_TYPE structureType, Region location) : base(structureType, location) { }
        protected ManMadeStructure(Region location, SaveDataManMadeStructure data) : base(location, data) {
            assignedWorkerID = data.assignedWorkerID;
        }

        #region Behaviours
        public void ProcessWorkerBehaviour(out JobQueueItem producedJob) {
            //We wrapped the actual process of getting worker jobs by structure in here so that the assignedWorker is only called once 
            //and be passed down as a parameter since assignedWorker is a getter
            //It would be waste of processing to call it every time
            Character worker = assignedWorker;
            ProcessWorkStructureJobsByWorker(worker, out producedJob);
        }
        protected virtual void ProcessWorkStructureJobsByWorker(Character p_worker, out JobQueueItem producedJob) { producedJob = null; }
        #endregion

        #region Listeners
        protected override void SubscribeListeners() {
            if (hasBeenDestroyed) { return; }
            Messenger.AddListener<ThinWall, int, bool>(StructureSignals.WALL_DAMAGED, OnWallDamaged);
            Messenger.AddListener<ThinWall, int, Character, bool>(StructureSignals.WALL_DAMAGED_BY, OnWallDamagedBy);
            Messenger.AddListener<ThinWall, int>(StructureSignals.WALL_REPAIRED, OnWallRepaired);
            // Messenger.AddListener<TileObject, int, bool>(TileObjectSignals.TILE_OBJECT_DAMAGED, OnObjectDamaged);
            // Messenger.AddListener<TileObject, int, Character, bool>(TileObjectSignals.TILE_OBJECT_DAMAGED_BY, OnObjectDamagedBy);
            Messenger.AddListener<TileObject, int>(TileObjectSignals.TILE_OBJECT_REPAIRED, OnObjectRepaired);
        }
        protected override void UnsubscribeListeners() {
            if (hasBeenDestroyed) { return; }
            Messenger.RemoveListener<ThinWall, int, bool>(StructureSignals.WALL_DAMAGED, OnWallDamaged);
            Messenger.RemoveListener<ThinWall, int, Character, bool>(StructureSignals.WALL_DAMAGED_BY, OnWallDamagedBy);
            Messenger.RemoveListener<ThinWall, int>(StructureSignals.WALL_REPAIRED, OnWallRepaired);
            // Messenger.RemoveListener<TileObject, int, bool>(TileObjectSignals.TILE_OBJECT_DAMAGED, OnObjectDamaged);
            // Messenger.RemoveListener<TileObject, int, Character, bool>(TileObjectSignals.TILE_OBJECT_DAMAGED_BY, OnObjectDamagedBy);
            Messenger.RemoveListener<TileObject, int>(TileObjectSignals.TILE_OBJECT_REPAIRED, OnObjectRepaired);
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
        public virtual void OnUseStructureConnector(LocationGridTile p_usedConnector) { }
        #endregion
        
        #region HP
        private void OnWallRepaired(ThinWall structureWall, int amount) {
            if (structureWalls != null && structureWalls.Contains(structureWall)) {
                structureObj.RescanPathfindingGridOfStructure(region.innerMap);
                CheckInteriorState();
            }
            // if (objectsThatContributeToDamage.Contains(structureWall)) {
            //     AdjustHP(amount);
            // }
        }
        private void OnWallDamaged(ThinWall structureWall, int amount, bool isPlayerSource) {
            if (structureWalls != null && structureWalls.Contains(structureWall)) {
                structureObj.RescanPathfindingGridOfStructure(region.innerMap);
                // OnStructureDamaged();
            }
            // if (objectsThatContributeToDamage.Contains(structureWall)) {
            //     AdjustHP(amount, isPlayerSource: isPlayerSource);
            // }
        }
        private void OnWallDamagedBy(ThinWall structureWall, int amount, Character p_responsibleCharacter, bool isPlayerSource) {
            if (structureWalls != null && structureWalls.Contains(structureWall)) {
                //create repair job
                structureObj.RescanPathfindingGridOfStructure(region.innerMap);
                // OnStructureDamaged();
            }
            // if (objectsThatContributeToDamage.Contains(structureWall)) {
            //     AdjustHP(amount, p_responsibleCharacter, isPlayerSource: isPlayerSource);
            // }
        }
        #endregion

        #region Utilities
        protected void OnStructureDamaged() {
            if (structureType.IsOpenSpace() || structureType.IsVillageStructure() == false) {
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
        public override void ShowSelectorOnStructure() {
            Selector.Instance.Select(this);
        }

        #endregion

        #region Repair
        private void CreateRepairJob() {
            Assert.IsNotNull(_structureTileObject, $"Repair job is being created for {this} but it does not have a structure tile object");
            if (settlementLocation is NPCSettlement npcSettlement) {
                GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.REPAIR, INTERACTION_TYPE.REPAIR_STRUCTURE, _structureTileObject, npcSettlement);
                UtilityScripts.JobUtilities.PopulatePriorityLocationsForTakingNonEdibleResources(npcSettlement, job, INTERACTION_TYPE.TAKE_RESOURCE);
                job.AddOtherData(INTERACTION_TYPE.TAKE_RESOURCE, new object[]{ structureObj.repairCost });
                npcSettlement.AddToAvailableJobs(job);    
            }
        }
        #endregion

        #region Destroy
        protected override void DestroyStructure(Character p_responsibleCharacter = null, bool isPlayerSource = false) {
            if (hasBeenDestroyed) {
                return;
            }
            if (_structureTileObject != null && settlementLocation is NPCSettlement npcSettlement) {
                Messenger.Broadcast(CharacterSignals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI, _structureTileObject as IPointOfInterest, "");
                Messenger.Broadcast(CharacterSignals.FORCE_CANCEL_ALL_ACTIONS_TARGETING_POI, _structureTileObject as IPointOfInterest, "");
                JobQueueItem existingRepairJob = npcSettlement.GetJob(JOB_TYPE.REPAIR, _structureTileObject);
                 if (existingRepairJob != null) {
                     npcSettlement.RemoveFromAvailableJobs(existingRepairJob);
                 }
            }
            base.DestroyStructure(p_responsibleCharacter, isPlayerSource);
        }
        protected override void AfterStructureDestruction(Character p_responsibleCharacter = null) {
            structureObj.OnOwnerStructureDestroyed(region.innerMap);
            Area hexTile = occupiedArea;
            base.AfterStructureDestruction(p_responsibleCharacter);
            if (hexTile != null) {
                hexTile.CheckIfSettlementIsStillOnArea();
            }
            SetAssignedWorker(null);
        }
        #endregion

        #region Walls
        public void SetWallObjects(List<ThinWall> wallObjects, RESOURCE resource) {
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

        #region Worker
        public void SetAssignedWorker(Character p_assignedWorker) {
            Character prevWorker = assignedWorker;
            if (p_assignedWorker != prevWorker) {
                assignedWorkerID = p_assignedWorker != null ? p_assignedWorker.persistentID : string.Empty;
                Character newWorker = assignedWorker;
                if (prevWorker != null) {
                    prevWorker.structureComponent.SetWorkPlaceStructure(null);
                }
                if (newWorker != null) {
                    if (newWorker.structureComponent.workPlaceStructure != null) {
                        newWorker.structureComponent.workPlaceStructure.SetAssignedWorker(null);
                    }
                    newWorker.structureComponent.SetWorkPlaceStructure(this);
                }
            }
        }
        public bool HasAssignedWorker() {
            return assignedWorker != null;
        }
        public bool CanPurchaseFromHereBasedOnAssignedWorker(Character p_buyer, out bool needsToPay) {
            if (assignedWorker != null) {
                if (assignedWorker == p_buyer) {
                    //structure is owned by self
                    needsToPay = false;
                    return true;
                } else if (p_buyer.relationshipContainer.HasRelationshipWith(assignedWorker, RELATIONSHIP_TYPE.LOVER)) {
                    //structure is owned by a lover
                    needsToPay = false;
                    return true;
                } else if (p_buyer.relationshipContainer.IsFamilyMember(assignedWorker) && 
                           p_buyer.relationshipContainer.GetOpinionLabel(assignedWorker) == RelationshipManager.Close_Friend) {
                    //structure is owned by a close friend family member
                    needsToPay = false;
                    return true;
                } else if (!p_buyer.relationshipContainer.IsEnemiesWith(assignedWorker)) {
                    //structure is owned by a non-enemy villager
                    needsToPay = true;
                    return true;
                }
            }
            needsToPay = true;
            return false;
        }
        #endregion

        #region Damage
        public override void OnTileDamaged(LocationGridTile tile, int amount, bool isPlayerSource) {
            base.OnTileDamaged(tile, amount, isPlayerSource);
            if (tile.groundType == LocationGridTile.Ground_Type.Structure_Stone || 
                tile.groundType == LocationGridTile.Ground_Type.Wood || 
                tile.groundType == LocationGridTile.Ground_Type.Demon_Stone || 
                tile.groundType == LocationGridTile.Ground_Type.Cobble) {
                AdjustHP(amount, isPlayerSource: isPlayerSource);
                OnStructureDamaged();    
            }
        }
        public override void OnTileRepaired(LocationGridTile tile, int amount) {
            if (hasBeenDestroyed) { return; }
            AdjustHP(amount);
            if (tile.tileObjectComponent.genericTileObject.currentHP >= tile.tileObjectComponent.genericTileObject.maxHP) {
                // ReSharper disable once Unity.NoNullPropagation
                structureObj?.ApplyGroundTileAssetForTile(tile);    
                tile.CreateSeamlessEdgesForSelfAndNeighbours();
            }
        }
        #endregion

        public override string GetTestingInfo() {
            string info = base.GetTestingInfo();
            info = $"{info}\n Assigned Worker: {assignedWorker?.name}";
            return info;
        }

        #region Loading
        public override void LoadReferences(SaveDataLocationStructure saveDataLocationStructure) {
            base.LoadReferences(saveDataLocationStructure);
            Character worker = assignedWorker;
            if (worker != null) {
                worker.structureComponent.SetWorkPlaceStructure(this);
            }
        }
        #endregion
    }
}