using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;

namespace Inner_Maps.Location_Structures {
    public class DemonicStructure : LocationStructure {
        protected DemonicStructure(STRUCTURE_TYPE structureType, Region location) : base(structureType, location) {
            SetMaxHPAndReset(3000);
        }
        public DemonicStructure(Region location, SaveDataLocationStructure data) : base(location, data) {
            SetMaxHPAndReset(3000);
        }

        #region Overrides
        public override void ConstructDefaultActions() {
            base.ConstructDefaultActions();
            AddPlayerAction(SPELL_TYPE.DEFEND);
        }
        protected override void DestroyStructure() {
            if (hasBeenDestroyed) {
                return;
            }
            InnerMapManager.Instance.RemoveWorldKnownDemonicStructure(this);
            HexTile hexTile = occupiedHexTile.hexTileOwner;
            base.DestroyStructure();
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
    }
}
