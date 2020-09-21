using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;

namespace Inner_Maps.Location_Structures {
    public class DemonicStructure : LocationStructure {
        
        public LocationStructureObject structureObj {get; private set;}
        public HashSet<Character> currentAttackers { get; }
        
        #region Getters
        public override Vector2 selectableSize => structureObj.size;
        #endregion
        
        protected DemonicStructure(STRUCTURE_TYPE structureType, Region location) : base(structureType, location) {
            SetMaxHPAndReset(3000);
            currentAttackers = new HashSet<Character>();
        }
        public DemonicStructure(Region location, SaveDataLocationStructure data) : base(location, data) {
            SetMaxHPAndReset(3000);
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
            structureObj.OnOwnerStructureDestroyed(); 
            HexTile hexTile = occupiedHexTile.hexTileOwner;
            base.AfterStructureDestruction();
            hexTile.RemoveCorruption();
            CharacterManager.Instance.SetNewCurrentDemonicStructureTargetOfAngels();
            Messenger.Broadcast(Signals.RELOAD_PLAYER_ACTIONS, this as IPlayerActionTarget);
        }
        #endregion

        #region Listeners
        protected override void SubscribeListeners() {
            Messenger.AddListener<IPointOfInterest, int>(Signals.OBJECT_DAMAGED, OnObjectDamaged);
            Messenger.AddListener<IPointOfInterest, int>(Signals.OBJECT_REPAIRED, OnObjectRepaired);
        }
        protected override void UnsubscribeListeners() {
            Messenger.RemoveListener<IPointOfInterest, int>(Signals.OBJECT_DAMAGED, OnObjectDamaged);
            Messenger.RemoveListener<IPointOfInterest, int>(Signals.OBJECT_REPAIRED, OnObjectRepaired);
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
