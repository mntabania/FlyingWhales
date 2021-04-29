using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;

namespace Inner_Maps.Location_Structures {
    public class Crypt : DemonicStructure {

        public override string scenarioDescription => "Each Crypt can produce up to 3 Skeleton minions. You may use these Skeletons to spawn monster parties using your Maraud, Prism, Kennel or Prison.";

        public override SUMMON_TYPE housedMonsterType => SUMMON_TYPE.Skeleton;
        public Crypt(Region location) : base(STRUCTURE_TYPE.CRYPT, location){
            SetMaxHPAndReset(1500);
        }
        public Crypt(Region location, SaveDataDemonicStructure data) : base(location, data) { }

        #region Overrides
        public override void OnBuiltNewStructure() {
            base.OnBuiltNewStructure();
            PlayerManager.Instance.player.underlingsComponent.AdjustMonsterUnderlingMaxCharge(SUMMON_TYPE.Skeleton, 3);
        }
        protected override void DestroyStructure(Character p_responsibleCharacter = null, bool isPlayerSource = false) {
            base.DestroyStructure(p_responsibleCharacter, isPlayerSource);
            PlayerManager.Instance.player.underlingsComponent.AdjustMonsterUnderlingMaxCharge(SUMMON_TYPE.Skeleton, -3);
        }
        #endregion

        #region Structure Object
        public override void SetStructureObject(LocationStructureObject structureObj) {
            base.SetStructureObject(structureObj);
            Vector3 position = structureObj.transform.position;
            position.x -= 0.5f;
            position.y -= 0.5f;
            worldPosition = position;
        }
        #endregion
    }
}