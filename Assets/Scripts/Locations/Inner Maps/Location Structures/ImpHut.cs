using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;

namespace Inner_Maps.Location_Structures {
    public class ImpHut : DemonicStructure {

        public ImpHut(Region location) : base(STRUCTURE_TYPE.IMP_HUT, location){ }
        public ImpHut(Region location, SaveDataDemonicStructure data) : base(location, data) { }

        #region Overrides
        public override void OnBuiltNewStructure() {
            base.OnBuiltNewStructure();
            PlayerManager.Instance.player.underlingsComponent.AdjustMonsterUnderlingMaxCharge(SUMMON_TYPE.Imp, 5);
        }
        protected override void DestroyStructure() {
            base.DestroyStructure();
            PlayerManager.Instance.player.underlingsComponent.AdjustMonsterUnderlingMaxCharge(SUMMON_TYPE.Imp, -5);
        }
        #endregion
    }
}