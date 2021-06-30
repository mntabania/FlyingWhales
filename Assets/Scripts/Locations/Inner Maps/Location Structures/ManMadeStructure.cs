using System.Collections.Generic;
using System.Linq;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;
namespace Inner_Maps.Location_Structures {
    public abstract class ManMadeStructure : LocationStructure, TileObjectEventDispatcher.ITraitListener {

        private StructureTileObject _structureTileObject;
        public List<ThinWall> structureWalls { get; private set; }
        public RESOURCE wallsAreMadeOf { get; protected set; }
        public LocationStructureObject structureObj {get; private set;}
        public List<string> assignedWorkerIDs { get; private set; }
        public string templateName { get; private set; } //Do not save this since this will be filled up automatically upon loading in SetStructureObject
        public Vector3 structureObjectWorldPos { get; private set; } //Do not save this since this will be filled up automatically upon loading in SetStructureObject

        private GameDate m_scheduledDirtProduction;
        private readonly List<TileObject> m_dirtyObjects;
        private const int UncomfortableNeededDirtyObjects = 2;

        #region Getters
        public override Vector2 selectableSize => structureObj.size;
        public override System.Type serializedData => typeof(SaveDataManMadeStructure);
        // public Character assignedWorker => string.IsNullOrEmpty(assignedWorkerID) ? null : CharacterManager.Instance.GetCharacterByPersistentID(assignedWorkerID);
        public List<TileObject> dirtyObjects => m_dirtyObjects;
        public GameDate scheduledDirtProduction => m_scheduledDirtProduction;
        #endregion

        protected ManMadeStructure(STRUCTURE_TYPE structureType, Region location) : base(structureType, location) {
            m_dirtyObjects = new List<TileObject>();
            assignedWorkerIDs = new List<string>();
        }
        protected ManMadeStructure(Region location, SaveDataManMadeStructure data) : base(location, data) {
            assignedWorkerIDs = new List<string>(data.assignedWorkerIDs);
            m_dirtyObjects = new List<TileObject>();
        }

        #region Behaviours
        public void ProcessWorkerBehaviour(Character p_worker, out JobQueueItem producedJob) {
            Assert.IsTrue(assignedWorkerIDs.Contains(p_worker.persistentID));
            ProcessWorkStructureJobsByWorker(p_worker, out producedJob);
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
                if (poi is TileObject tileObject) {
                    if (tileObject.traitContainer.HasTrait("Dirty")) {
                        AddDirtyObject(tileObject);    
                    }
                    tileObject.eventDispatcher.SubscribeToTileObjectGainedTrait(this);
                    tileObject.eventDispatcher.SubscribeToTileObjectLostTrait(this);
                }
                return true;
            }
            return false;
        }
        public override bool RemovePOI(IPointOfInterest poi, Character removedBy = null, bool isPlayerSource = false) {
            if (base.RemovePOI(poi, removedBy, isPlayerSource)) {
                if (poi is TileObject tileObject) {
                    if (tileObject.traitContainer.HasTrait("Dirty")) {
                        RemoveDirtyObject(tileObject);    
                    }
                    tileObject.eventDispatcher.UnsubscribeToTileObjectGainedTrait(this);
                    tileObject.eventDispatcher.UnsubscribeToTileObjectLostTrait(this);
                }
                return true;
            }
            return false;
        }
        public override bool RemovePOIWithoutDestroying(IPointOfInterest poi) {
            if (base.RemovePOIWithoutDestroying(poi)) {
                if (poi is TileObject tileObject) {
                    if (tileObject.traitContainer.HasTrait("Dirty")) {
                        RemoveDirtyObject(tileObject);    
                    }
                    tileObject.eventDispatcher.UnsubscribeToTileObjectGainedTrait(this);
                    tileObject.eventDispatcher.UnsubscribeToTileObjectLostTrait(this);
                }
                return true;
            }
            return false;
        }
        public override bool RemovePOIDestroyVisualOnly(IPointOfInterest poi, Character remover = null) {
            if (base.RemovePOIDestroyVisualOnly(poi, remover)) {
                if (poi is TileObject tileObject) {
                    if (tileObject.traitContainer.HasTrait("Dirty")) {
                        RemoveDirtyObject(tileObject);    
                    }
                    tileObject.eventDispatcher.UnsubscribeToTileObjectGainedTrait(this);
                    tileObject.eventDispatcher.UnsubscribeToTileObjectLostTrait(this);
                }
                return true;
            }
            return false;
        }
        public override bool LoadPOI(TileObject poi, LocationGridTile tileLocation) {
            if (base.LoadPOI(poi, tileLocation)) {
                if (poi is TileObject tileObject) {
                    tileObject.eventDispatcher.SubscribeToTileObjectGainedTrait(this);
                    tileObject.eventDispatcher.SubscribeToTileObjectLostTrait(this);
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
            RemoveAllAssignedWorkers();
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
            templateName = structureObj.name;
            structureObjectWorldPos = structureObj.transform.position;
            Vector3 position = structureObj.transform.position;
            position.x -= 0.5f;
            position.y -= 0.5f;
            worldPosition = position;
        }
        public override void OnBuiltNewStructure() {
            base.OnBuiltNewStructure();
            if (ProducesDirt()) { ScheduleDirtProduction(); }
        }
        #endregion

        #region Worker
        // public void SetAssignedWorker(Character p_assignedWorker) {
        //     Character prevWorker = assignedWorker;
        //     if (p_assignedWorker != prevWorker) {
        //         assignedWorkerIDs = p_assignedWorker != null ? p_assignedWorker.persistentID : string.Empty;
        //         Character newWorker = assignedWorker;
        //         if (prevWorker != null) {
        //             prevWorker.structureComponent.SetWorkPlaceStructure(null);
        //         }
        //         if (newWorker != null) {
        //             if (newWorker.structureComponent.workPlaceStructure != null) {
        //                 newWorker.structureComponent.workPlaceStructure.SetAssignedWorker(null);
        //             }
        //             newWorker.structureComponent.SetWorkPlaceStructure(this);
        //         }
        //     }
        // }
        public bool DoesCharacterWorkHere(Character p_character) {
            return assignedWorkerIDs.Contains(p_character.persistentID);
        }
        public void AddAssignedWorker(Character p_worker) {
            if (!assignedWorkerIDs.Contains(p_worker.persistentID)) {
                assignedWorkerIDs.Add(p_worker.persistentID);
                p_worker.structureComponent.SetWorkPlaceStructure(this);
                Messenger.Broadcast(StructureSignals.ON_WORKER_HIRED, p_worker, this);
            }
        }
        public bool RemoveAssignedWorker(Character p_worker) {
            if (assignedWorkerIDs.Remove(p_worker.persistentID)) {
                p_worker.structureComponent.SetWorkPlaceStructure(null);
                return true;
            }
            return false;
        }
        private void RemoveAllAssignedWorkers() {
            List<string> ids = RuinarchListPool<string>.Claim();
            ids.AddRange(assignedWorkerIDs);
            for (int i = 0; i < ids.Count; i++) {
                string assignedWorkerID = ids[i];
                Character assignedWorker = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(assignedWorkerID);
                RemoveAssignedWorker(assignedWorker);
            }
            RuinarchListPool<string>.Release(ids);
        }
        public bool HasAssignedWorker() {
            return assignedWorkerIDs.Count > 0;
        }
        public virtual bool CanHireAWorker() {
            return false;
        }
        public bool HasWorkerThatIsNotAnEnemyOfCharacter(Character p_character) {
            for (int i = 0; i < assignedWorkerIDs.Count; i++) {
                string assignedWorkerID = assignedWorkerIDs[i];
                Character assignedWorker = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(assignedWorkerID);
                if (!p_character.relationshipContainer.IsEnemiesWith(assignedWorker)) {
                    return true;
                }
            }
            return false;
        }
        public virtual bool CanPurchaseFromHere(Character p_buyer, out bool needsToPay, out int buyerOpinionOfWorker) {
            needsToPay = true;
            buyerOpinionOfWorker = -100;
            return false;
            // if (assignedWorker != null) {
            //     if (assignedWorker == p_buyer) {
            //         //structure is owned by self
            //         needsToPay = false;
            //         return true;
            //     } else if (p_buyer.relationshipContainer.HasRelationshipWith(assignedWorker, RELATIONSHIP_TYPE.LOVER)) {
            //         //structure is owned by a lover
            //         needsToPay = false;
            //         return true;
            //     } else if (p_buyer.relationshipContainer.IsFamilyMember(assignedWorker) && 
            //                p_buyer.relationshipContainer.GetOpinionLabel(assignedWorker) == RelationshipManager.Close_Friend) {
            //         //structure is owned by a close friend family member
            //         //needs to pay if worker does not consider buyer a close friend
            //         needsToPay = assignedWorker.relationshipContainer.GetOpinionLabel(p_buyer) != RelationshipManager.Close_Friend;
            //         return true;
            //     } else if (!p_buyer.relationshipContainer.IsEnemiesWith(assignedWorker)) {
            //         //structure is owned by a non-enemy villager
            //         needsToPay = true;
            //         return true;
            //     }
            // }
            // needsToPay = true;
            // return false;
        }
        protected bool DefaultCanPurchaseFromHereForSingleWorkerStructures(Character p_buyer, out bool needsToPay, out int buyerOpinionOfWorker) {
            if (HasAssignedWorker()) {
                string assignedWorkerID = assignedWorkerIDs[0];
                Character assignedWorker = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(assignedWorkerID);
                if (assignedWorker == p_buyer) {
                    //structure is owned by self
                    needsToPay = false;
                    buyerOpinionOfWorker = 100;
                    return true;
                } else if (p_buyer.relationshipContainer.HasRelationshipWith(assignedWorker, RELATIONSHIP_TYPE.LOVER)) {
                    //structure is owned by a lover
                    needsToPay = false;
                    buyerOpinionOfWorker = p_buyer.relationshipContainer.GetTotalOpinion(assignedWorker);
                    return true;
                } else if (p_buyer.relationshipContainer.IsFamilyMember(assignedWorker) && 
                           p_buyer.relationshipContainer.GetOpinionLabel(assignedWorker) == RelationshipManager.Close_Friend) {
                    //structure is owned by a close friend family member
                    //needs to pay if worker does not consider buyer a close friend
                    needsToPay = assignedWorker.relationshipContainer.GetOpinionLabel(p_buyer) != RelationshipManager.Close_Friend;
                    buyerOpinionOfWorker = p_buyer.relationshipContainer.GetTotalOpinion(assignedWorker);
                    return true;
                } else if (!p_buyer.relationshipContainer.IsEnemiesWith(assignedWorker)) {
                    //structure is owned by a non-enemy villager
                    needsToPay = assignedWorker.relationshipContainer.GetOpinionLabel(p_buyer) != RelationshipManager.Close_Friend;
                    buyerOpinionOfWorker = p_buyer.relationshipContainer.GetTotalOpinion(assignedWorker);
                    return true;
                }
                needsToPay = true;
                buyerOpinionOfWorker = -100;
                return false;    
            } else {
                //if no worker allow character to buy but buyer now always has to pay.
                needsToPay = true;
                buyerOpinionOfWorker = -100;
                return true;
            }
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

        #region Characters
        protected override void AfterCharacterAddedToLocation(Character p_character) {
            base.AfterCharacterAddedToLocation(p_character);
            if (p_character.isNormalCharacter) {
                if (dirtyObjects.Count >= UncomfortableNeededDirtyObjects) {
                    p_character.traitContainer.AddTrait(p_character, "Uncomfortable");
                }    
            }
        }
        #endregion

        #region Dirt
        private void AddDirtyObject(TileObject p_object) {
            if (!m_dirtyObjects.Contains(p_object)) {
                m_dirtyObjects.Add(p_object);
                
                //add uncomfortable to all characters currently inside structure if dirty objects reach exactly the needed amount
                //added checking to be exactly the needed amount since we do not want the effect to stack per object that becomes dirty
                if (m_dirtyObjects.Count == UncomfortableNeededDirtyObjects) {
                    for (int i = 0; i < charactersHere.Count; i++) {
                        Character character = charactersHere[i];
                        if (character.isNormalCharacter) {
                            character.traitContainer.AddTrait(character, "Uncomfortable");
                        }
                    }
                }
            }
        }
        private void RemoveDirtyObject(TileObject p_object) {
            m_dirtyObjects.Remove(p_object);
        }
        private bool ProducesDirt() {
            return structureType.IsVillageStructure() && structureType != STRUCTURE_TYPE.CITY_CENTER && structureType != STRUCTURE_TYPE.CEMETERY && 
                   structureType != STRUCTURE_TYPE.PRISON && structureType != STRUCTURE_TYPE.MINE && structureType != STRUCTURE_TYPE.LUMBERYARD;
        }
        private void ScheduleDirtProduction() {
            GameDate dueDate = GameManager.Instance.Today();
            dueDate.AddTicks(GameManager.Instance.GetTicksBasedOnHour(12));
            m_scheduledDirtProduction = dueDate;
            SchedulingManager.Instance.AddEntry(dueDate, ProduceDirtOnSchedule, null);
        }
        private void LoadDirtProduction(GameDate p_date) {
            m_scheduledDirtProduction = p_date;
            SchedulingManager.Instance.AddEntry(p_date, ProduceDirtOnSchedule, null);
        }
        private void ProduceDirtOnSchedule() {
            if (hasBeenDestroyed) { return; }
            List<TileObject> builtTileObjects = RuinarchListPool<TileObject>.Claim();
            PopulateBuiltTileObjects(builtTileObjects);
            if (builtTileObjects.Count > 0) {
                TileObject targetObject = CollectionUtilities.GetRandomElement(builtTileObjects);
                targetObject.traitContainer.AddTrait(targetObject, "Dirty");
            }
            ScheduleDirtProduction();
        }
        #endregion

        #region Clean Up Job
        protected bool TryCreateCleanJob(Character p_worker, out JobQueueItem producedJob) {
            List<TileObject> wetObjects = RuinarchListPool<TileObject>.Claim();
            PopulateBuiltTileObjectsThatHaveTrait(wetObjects, "Wet");
            if (dirtyObjects.Count > 0 || wetObjects.Count > 0) {
                int chance = 100;
                if (p_worker.traitContainer.HasTrait("Lazy")) { chance = 6; }
                if (GameUtilities.RollChance(chance)) {
                    List<TileObject> cleanChoices = RuinarchListPool<TileObject>.Claim();
                    cleanChoices.AddRange(dirtyObjects);
                    cleanChoices.AddRange(wetObjects);
                    TileObject cleanTarget = CollectionUtilities.GetRandomElement(cleanChoices);
                    RuinarchListPool<TileObject>.Release(cleanChoices);
                    if (p_worker.jobComponent.TryCreateCleanItemJob(cleanTarget, out producedJob)) {
                        RuinarchListPool<TileObject>.Release(wetObjects);
                        return true;
                    }
                }
            }
            RuinarchListPool<TileObject>.Release(wetObjects);
            producedJob = null;
            return false;
        }
        #endregion

        public override string GetTestingInfo() {
            string info = base.GetTestingInfo();
            List<Character> assignedWorkers = RuinarchListPool<Character>.Claim();
            for (int i = 0; i < assignedWorkerIDs.Count; i++) {
                string assignedWorkerID = assignedWorkerIDs[i];
                Character character = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(assignedWorkerID);
                assignedWorkers.Add(character);
            }
            info = $"{info}\n Assigned Workers: {assignedWorkers?.ComafyList()}";
            RuinarchListPool<Character>.Release(assignedWorkers);
            info = $"{info}\n Dirty Objects: {m_dirtyObjects?.ComafyList()}";
            return info;
        }

        #region Loading
        public override void LoadReferences(SaveDataLocationStructure saveDataLocationStructure) {
            base.LoadReferences(saveDataLocationStructure);
            for (int i = 0; i < assignedWorkerIDs.Count; i++) {
                string workerID = assignedWorkerIDs[i];
                Character worker = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(workerID);
                if (worker != null) {
                    worker.structureComponent.SetWorkPlaceStructure(this);
                }
            }
            SaveDataManMadeStructure saveDataManMadeStructure = saveDataLocationStructure as SaveDataManMadeStructure;
            if (saveDataManMadeStructure.dirtyObjects != null) {
                for (int i = 0; i < saveDataManMadeStructure.dirtyObjects.Length; i++) {
                    string dirtyObjectID = saveDataManMadeStructure.dirtyObjects[i];
                    TileObject tileObject = DatabaseManager.Instance.tileObjectDatabase.GetTileObjectByPersistentID(dirtyObjectID);
                    dirtyObjects.Add(tileObject);
                }
            }
            if (saveDataManMadeStructure.scheduledDirtProduction.hasValue) {
                LoadDirtProduction(saveDataManMadeStructure.scheduledDirtProduction);
            }
        }
        #endregion

        #region TileObjectEventDispatcher.ITraitListener Implementation
        public void OnTileObjectGainedTrait(TileObject p_tileObject, Trait p_trait) {
            if (p_trait is Dirty) {
                AddDirtyObject(p_tileObject);
            }
        }
        public void OnTileObjectLostTrait(TileObject p_tileObject, Trait p_trait) {
            if (p_trait is Dirty && !p_tileObject.traitContainer.HasTrait("Dirty")) {
                RemoveDirtyObject(p_tileObject);
            }
        }
        #endregion
        
    }
}