﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Debug = System.Diagnostics.Debug;
namespace Inner_Maps.Location_Structures {
    public class DemonicStructure : LocationStructure {
        
        public LocationStructureObject structureObj {get; private set;}
        public HashSet<Character> currentAttackers { get; }
        // public int activeSnatchJobs { get; private set; }
        
        #region Getters
        public override Vector2 selectableSize => structureObj.size;
        #endregion
        
        protected DemonicStructure(STRUCTURE_TYPE structureType, Region location) : base(structureType, location) {
            SetMaxHPAndReset(3000);
            currentAttackers = new HashSet<Character>();
        }
        public DemonicStructure(Region location, SaveDataLocationStructure data) : base(location, data) {
            currentAttackers = new HashSet<Character>();
        }

        #region Overrides
        protected override void DestroyStructure() {
            if (hasBeenDestroyed) {
                return;
            }
            InnerMapManager.Instance.RemoveWorldKnownDemonicStructure(this);
            base.DestroyStructure();
        }
        protected override void AfterStructureDestruction() {
            structureObj.OnOwnerStructureDestroyed(region.innerMap); 
            HexTile hexTile = occupiedHexTile.hexTileOwner;
            base.AfterStructureDestruction();
            hexTile.RemoveCorruption();
            CharacterManager.Instance.SetNewCurrentDemonicStructureTargetOfAngels();
            Messenger.Broadcast(Signals.RELOAD_PLAYER_ACTIONS, this as IPlayerActionTarget);
        }
        public override void ConstructDefaultActions() {
            base.ConstructDefaultActions();
            AddPlayerAction(SPELL_TYPE.REPAIR);
        }
        #endregion

        #region Loading
        public override void LoadReferences(SaveDataLocationStructure saveDataLocationStructure) {
            base.LoadReferences(saveDataLocationStructure);
            SaveDataDemonicStructure demonicStructure = saveDataLocationStructure as SaveDataDemonicStructure;
            // activeSnatchJobs = demonicStructure.activeSnatchJobs;
        }
        #endregion

        #region Listeners
        protected override void SubscribeListeners() {
            Messenger.AddListener<IPointOfInterest, int>(Signals.OBJECT_DAMAGED, OnObjectDamaged);
            Messenger.AddListener<IPointOfInterest, int>(Signals.OBJECT_REPAIRED, OnObjectRepaired);
            // Messenger.AddListener<JobQueueItem, Character>(Signals.JOB_ADDED_TO_QUEUE, OnSnatcherAddedJobToQueue);
            // Messenger.AddListener<JobQueueItem, Character>(Signals.JOB_REMOVED_FROM_QUEUE, OnSnatchJobRemoved);
        }
        protected override void UnsubscribeListeners() {
            Messenger.RemoveListener<IPointOfInterest, int>(Signals.OBJECT_DAMAGED, OnObjectDamaged);
            Messenger.RemoveListener<IPointOfInterest, int>(Signals.OBJECT_REPAIRED, OnObjectRepaired);
            // Messenger.RemoveListener<JobQueueItem, Character>(Signals.JOB_ADDED_TO_QUEUE, OnSnatcherAddedJobToQueue);
            // Messenger.RemoveListener<JobQueueItem, Character>(Signals.JOB_REMOVED_FROM_QUEUE, OnSnatchJobRemoved);
        }
        private void OnSnatcherAddedJobToQueue(JobQueueItem job, Character character) {
            if (job.jobType == JOB_TYPE.SNATCH && DoesSnatchJobTargetThisStructure(job)) {
                // activeSnatchJobs++;
            }
        }
        private void OnSnatchJobRemoved(JobQueueItem job, Character character) {
            if (job.jobType == JOB_TYPE.SNATCH && DoesSnatchJobTargetThisStructure(job)) {
                // activeSnatchJobs--;
            }
        }
        private bool DoesSnatchJobTargetThisStructure(JobQueueItem job) {
            if (job is GoapPlanJob goapPlanJob) {
                OtherData[] otherData = goapPlanJob.GetOtherData(INTERACTION_TYPE.DROP);
                if (otherData != null) {
                    for (int i = 0; i < otherData.Length; i++) {
                        OtherData data = otherData[i];
                        if (data is LocationStructureOtherData structureOtherData && structureOtherData.locationStructure == this) {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        #endregion

        #region Utilities
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
        public void RepairStructure() {
            ResetHP();
            structureObj.OnRepairStructure(region.innerMap, this);
        }
        /// <summary>
        /// Does this structure has an unoccupied room?
        /// NOTE: Will consider room unoccupied if there is no alive character in the room.
        /// </summary>
        public bool HasUnoccupiedRoom() {
            if (rooms == null) { return false; }
            for (int i = 0; i < rooms.Length; i++) {
                StructureRoom room = rooms[i];
                if (!room.HasAnyAliveCharacterInRoom()) {
                    return true;
                }
            }
            return false;
        }
        public int GetUnoccupiedRoomCount() {
            if (rooms == null) { return 0; }
            int count = 0;
            for (int i = 0; i < rooms.Length; i++) {
                StructureRoom room = rooms[i];
                if (!room.HasAnyAliveCharacterInRoom()) {
                    count++;
                }
            }
            return count;
        }
        #endregion

        #region HP
        public override void OnTileRepaired(LocationGridTile tile, int amount) {
            if (hasBeenDestroyed) { return; }
            if (tile.genericTileObject.currentHP >= tile.genericTileObject.maxHP) {
                // ReSharper disable once Unity.NoNullPropagation
                structureObj?.ApplyGroundTileAssetForTile(tile);    
                tile.CreateSeamlessEdgesForSelfAndNeighbours();
            }
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

        #region Attackers
        public void AddAttacker(Character attacker) {
            if (!currentAttackers.Contains(attacker)) {
                bool wasEmptyBeforeAdding = currentAttackers.Count == 0;
                currentAttackers.Add(attacker);
                Messenger.Broadcast(Signals.CHARACTER_ATTACKED_DEMONIC_STRUCTURE, attacker, this);
                if (wasEmptyBeforeAdding) {
                    Messenger.AddListener<Character, CharacterState>(Signals.CHARACTER_ENDED_STATE, OnCharacterEndedState);
                }
            }
        }
        public void RemoveAttacker(Character attacker) {
            currentAttackers.Remove(attacker);
            if (currentAttackers.Count == 0) {
                Messenger.RemoveListener<Character, CharacterState>(Signals.CHARACTER_ENDED_STATE, OnCharacterEndedState);
            }
        }
        private void OnCharacterEndedState(Character character, CharacterState characterState) {
            RemoveAttacker(character);
        }
        #endregion
    }
}
