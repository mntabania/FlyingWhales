using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using BayatGames.SaveGameFree.Types;
using Debug = System.Diagnostics.Debug;

namespace Inner_Maps.Location_Structures {
    public class DemonicStructure : LocationStructure {

        public LocationStructureObject structureObj {get; private set;}
        public HashSet<Character> currentAttackers { get; }
        public Character preOccupiedBy { get; private set; }
        public string templateName { get; private set; } //Do not save this since this will be filled up automatically upon loading in SetStructureObject
        public Vector3 structureObjectWorldPos { get; private set; } //Do not save this since this will be filled up automatically upon loading in SetStructureObject

        #region Getters
        public override Vector2 selectableSize => structureObj.size;
        public override Type serializedData => typeof(SaveDataDemonicStructure);
        public virtual SUMMON_TYPE housedMonsterType => SUMMON_TYPE.None;
        #endregion

        protected DemonicStructure(STRUCTURE_TYPE structureType, Region location) : base(structureType, location) {
            SetMaxHPAndReset(3000);
            currentAttackers = new HashSet<Character>();
        }
        public DemonicStructure(Region location, SaveDataDemonicStructure data) : base(location, data) {
            currentAttackers = new HashSet<Character>();
        }

        #region Overrides
        protected override void DestroyStructure(Character p_responsibleCharacter = null, bool isPlayerSource = false) {
            if (hasBeenDestroyed) {
                return;
            }
            InnerMapManager.Instance.RemoveWorldKnownDemonicStructure(this);
            base.DestroyStructure(p_responsibleCharacter);
        }
        protected override void AfterStructureDestruction(Character p_responsibleCharacter = null) {
            structureObj.OnOwnerStructureDestroyed(region.innerMap); 
            //Area area = occupiedArea;
            base.AfterStructureDestruction(p_responsibleCharacter);
            //area.RemoveCorruption();
            //CharacterManager.Instance.SetNewCurrentDemonicStructureTargetOfAngels();
            for (int i = 0; i < currentAttackers.Count; i++) {
                Character attacker = currentAttackers.ElementAt(i);
                if (attacker.race == RACE.ANGEL) {
                    PlayerManager.Instance.player.retaliationComponent.AddDestroyedStructureByAngels();
                    break;
                }
            }
            currentAttackers.Clear();
            if (structureType != STRUCTURE_TYPE.THE_PORTAL) {
                DemonicStructurePlayerSkill skill = PlayerSkillManager.Instance.GetDemonicStructureSkillData(structureType);
                if(skill.isInUse) {
                    int numberOfActiveStructures = PlayerManager.Instance.player.playerSettlement.GetStructureCount(structureType);
                    if (numberOfActiveStructures < skill.maxCharges) {
                        skill.AdjustCharges(1);
                    }
                }
            }
            Messenger.RemoveListener<Character, CharacterBehaviourComponent>(CharacterSignals.CHARACTER_REMOVED_BEHAVIOUR, OnCharacterRemovedBehaviour);
            Messenger.Broadcast(PlayerSkillSignals.RELOAD_PLAYER_ACTIONS, this as IPlayerActionTarget);
        }
        public override void ConstructDefaultActions() {
            base.ConstructDefaultActions();
            AddPlayerAction(PLAYER_SKILL_TYPE.REPAIR);
            AddPlayerAction(PLAYER_SKILL_TYPE.DESTROY_STRUCTURE);
        }
        #endregion

        #region Loading
        public override void LoadReferences(SaveDataLocationStructure saveDataLocationStructure) {
            base.LoadReferences(saveDataLocationStructure);
            SaveDataDemonicStructure demonicStructure = saveDataLocationStructure as SaveDataDemonicStructure;
            if (!string.IsNullOrEmpty(demonicStructure.preOccupiedBy)) {
                preOccupiedBy = CharacterManager.Instance.GetCharacterByPersistentID(demonicStructure.preOccupiedBy);
            }
            // activeSnatchJobs = demonicStructure.activeSnatchJobs;
        }
        #endregion

        #region Listeners
        protected override void SubscribeListeners() {
            Messenger.AddListener<TileObject, int, bool>(TileObjectSignals.TILE_OBJECT_DAMAGED, OnObjectDamaged);
            Messenger.AddListener<TileObject, int, Character, bool>(TileObjectSignals.TILE_OBJECT_DAMAGED_BY, OnObjectDamagedBy);
            Messenger.AddListener<TileObject, int>(TileObjectSignals.TILE_OBJECT_REPAIRED, OnObjectRepaired);
        }
        protected override void UnsubscribeListeners() {
            Messenger.RemoveListener<TileObject, int, bool>(TileObjectSignals.TILE_OBJECT_DAMAGED, OnObjectDamaged);
            Messenger.RemoveListener<TileObject, int, Character, bool>(TileObjectSignals.TILE_OBJECT_DAMAGED_BY, OnObjectDamagedBy);
            Messenger.RemoveListener<TileObject, int>(TileObjectSignals.TILE_OBJECT_REPAIRED, OnObjectRepaired);
        }
        private bool DoesSnatchJobTargetThisStructure(JobQueueItem job) {
            if (job is GoapPlanJob goapPlanJob) {
                OtherData[] otherData = goapPlanJob.GetOtherDataSpecific(INTERACTION_TYPE.DROP);
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
        public override void ShowSelectorOnStructure() {
            Selector.Instance.Select(this);
        }
        public void RepairStructure() {
            ResetHP();
            structureObj.OnRepairStructure(region.innerMap, this, out int createdWalls, out int totalWallsInTemplate);
            if (createdWalls < totalWallsInTemplate) {
                int missingWalls = totalWallsInTemplate - createdWalls;
                TileObjectData tileObjectData = TileObjectDB.GetTileObjectData(TILE_OBJECT_TYPE.BLOCK_WALL);
                AdjustHP(-(missingWalls * tileObjectData.maxHP));
            }
            Messenger.Broadcast(StructureSignals.DEMONIC_STRUCTURE_REPAIRED, this);
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
        public void SetPreOccupiedBy(Character p_character) {
            preOccupiedBy = p_character;
        }
        #endregion

        #region HP
        public override void OnTileRepaired(LocationGridTile tile, int amount) {
            if (hasBeenDestroyed) { return; }
            if (tile.tileObjectComponent.genericTileObject.currentHP >= tile.tileObjectComponent.genericTileObject.maxHP) {
                // ReSharper disable once Unity.NoNullPropagation
                structureObj?.ApplyGroundTileAssetForTile(tile);    
                tile.CreateSeamlessEdgesForSelfAndNeighbours();
            }
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
        #endregion

        #region Attackers
        public void AddAttacker(Character p_attacker) {
            if (!currentAttackers.Contains(p_attacker)) {
                bool wasEmptyBeforeAdding = currentAttackers.Count == 0;
                currentAttackers.Add(p_attacker);
                Messenger.Broadcast(CharacterSignals.CHARACTER_HIT_DEMONIC_STRUCTURE, p_attacker, this);
                if (wasEmptyBeforeAdding) {
                    Messenger.AddListener<Character, CharacterBehaviourComponent>(CharacterSignals.CHARACTER_REMOVED_BEHAVIOUR, OnCharacterRemovedBehaviour);
                    Messenger.AddListener<Character>(CharacterSignals.CHARACTER_MARKER_DESTROYED, OnCharacterMarkerDestroyed);
                }
#if DEBUG_LOG
                UnityEngine.Debug.Log($"Added attacker {p_attacker.name} to {this.name}");
#endif
            }
        }
        private void RemoveAttacker(Character p_attacker) {
            if (currentAttackers.Remove(p_attacker)) {
#if DEBUG_LOG
                UnityEngine.Debug.Log($"Removed attacker {p_attacker.name} to {this.name}");
#endif
                if (currentAttackers.Count == 0) {
                    Messenger.RemoveListener<Character, CharacterBehaviourComponent>(CharacterSignals.CHARACTER_REMOVED_BEHAVIOUR, OnCharacterRemovedBehaviour);
                    Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_MARKER_DESTROYED, OnCharacterMarkerDestroyed);
                }
            }
        }
        private void OnCharacterRemovedBehaviour(Character p_character, CharacterBehaviourComponent p_removedBehaviour) {
            if (p_removedBehaviour is AttackDemonicStructureBehaviour || p_removedBehaviour is DemonRescueBehaviour) {
                RemoveAttacker(p_character);    
            }
        }
        private void OnCharacterMarkerDestroyed(Character p_character) {
            RemoveAttacker(p_character);
        }
#endregion
        
#region IPartyTargetDestination
        public override bool IsAtTargetDestination(Character character) {
            bool isAtTargetDestination = base.IsAtTargetDestination(character);
            if (!isAtTargetDestination) {
                return CanSeeObjectLocatedHere(character);
            }
            return true;
        }
#endregion

        public bool CanSeeObjectLocatedHere(Character p_character) {
            if (p_character.hasMarker) {
                //since characters usually cannot directly step on a demonic structure,
                //consider character as arrived if it has a tile object in its vision that is at the target structure
                for (int i = 0; i < p_character.marker.inVisionTileObjects.Count; i++) {
                    TileObject tileObject = p_character.marker.inVisionTileObjects[i];
                    if (tileObject.structureLocation != null && tileObject.structureLocation == this) {
                        return true;
                    }
                }    
            }
            return false;
        }
    }
}
public class SaveDataDemonicStructure : SaveDataLocationStructure
{

    public string structureTemplateName;
    public Vector3Save structureObjectWorldPosition;
    public string preOccupiedBy;
    // public int activeSnatchJobs;

    public override void Save(LocationStructure locationStructure) {
        base.Save(locationStructure);
        DemonicStructure demonicStructure = locationStructure as DemonicStructure;
        Assert.IsNotNull(demonicStructure);

        if (demonicStructure.preOccupiedBy != null) {
            preOccupiedBy = demonicStructure.preOccupiedBy.persistentID;
        }
        if (demonicStructure.hasBeenDestroyed) {
            structureTemplateName = string.Empty;
            structureObjectWorldPosition = Vector3.zero;
        } else {
            //structure object
            string templateName = demonicStructure.templateName;
            templateName = templateName.Replace("(Clone)", "");
            structureTemplateName = templateName;
            structureObjectWorldPosition = demonicStructure.structureObjectWorldPos;
        }

        // activeSnatchJobs = demonicStructure.activeSnatchJobs;
    }
}