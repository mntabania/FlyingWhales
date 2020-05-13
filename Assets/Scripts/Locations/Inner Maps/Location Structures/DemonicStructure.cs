using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inner_Maps.Location_Structures {
    public class DemonicStructure : LocationStructure {
        protected DemonicStructure(STRUCTURE_TYPE structureType, Region location) : base(structureType, location) {
            maxHP = 8000;
            currentHP = maxHP;
        }
        public DemonicStructure(Region location, SaveDataLocationStructure data) : base(location, data) {
            maxHP = 8000;
            currentHP = maxHP;
        }

        #region Overrides
        public override void ConstructDefaultActions() {
            base.ConstructDefaultActions();
            AddPlayerAction(SPELL_TYPE.DEFEND);
        }
        protected override void DestroyStructure() {
            base.DestroyStructure();
            CharacterManager.Instance.SetNewCurrentDemonicStructureTargetOfAngels();
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
