using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;

namespace Inner_Maps.Location_Structures {
    public class Crypt : DemonicStructure {

        public Crypt(Region location) : base(STRUCTURE_TYPE.CRYPT, location){ }
        public Crypt(Region location, SaveDataDemonicStructure data) : base(location, data) { }

        #region Overrides
        public override void OnBuiltNewStructure() {
            base.OnBuiltNewStructure();
            PlayerManager.Instance.player.underlingsComponent.AdjustMonsterUnderlingMaxCharge(SUMMON_TYPE.Skeleton, 5);
        }
        protected override void DestroyStructure(Character p_responsibleCharacter = null) {
            base.DestroyStructure(p_responsibleCharacter);
            PlayerManager.Instance.player.underlingsComponent.AdjustMonsterUnderlingMaxCharge(SUMMON_TYPE.Skeleton, -5);
        }
        #endregion

        #region Structure Object
        public override void SetStructureObject(LocationStructureObject structureObj) {
            base.SetStructureObject(structureObj);
            Vector3 position = structureObj.transform.position;
            position.x -= 0.5f;
            worldPosition = position;
        }
        #endregion
    }
}