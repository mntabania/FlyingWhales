using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Assertions;
using Inner_Maps.Location_Structures;

namespace Inner_Maps.Location_Structures {
    public class Biolab : DemonicStructure {
        public override Vector2 selectableSize { get; }
        public GameDate replenishDate { get; private set; }

        #region getters
        public override System.Type serializedData => typeof(SaveDataBiolab);
        #endregion

        public Biolab(Region location) : base(STRUCTURE_TYPE.BIOLAB, location) {
            selectableSize = new Vector2(10f, 10f);
        }
        public Biolab(Region location, SaveDataBiolab data) : base(location, data) {
            selectableSize = new Vector2(10f, 10f);
            replenishDate = data.replenishDate;
            SchedulingManager.Instance.AddEntry(replenishDate, ProcessReplenishingOfPlaguedRatCharge, null);
        }

        #region Overrides
        public override void OnBuiltNewStructure() {
            base.OnBuiltNewStructure();
            ReplenishPlaguedRatChargeWith3MaxCharges();
            ScheduleReplenishOfPlaguedRatCharge();
        }
        public override void ConstructDefaultActions() {
            base.ConstructDefaultActions();
            AddPlayerAction(SPELL_TYPE.UPGRADE);
        }
        #endregion

        #region Plagued Rat
        private void ScheduleReplenishOfPlaguedRatCharge() {
            replenishDate = GameManager.Instance.Today().AddDays(3);
            SchedulingManager.Instance.AddEntry(replenishDate, ProcessReplenishingOfPlaguedRatCharge, null);
        }
        private void ProcessReplenishingOfPlaguedRatCharge() {
            if(!hasBeenDestroyed && tiles.Count > 0) {
                ReplenishPlaguedRatChargeWith3MaxCharges();
                ScheduleReplenishOfPlaguedRatCharge();
            }
        }
        private void ReplenishPlaguedRatChargeWith3MaxCharges() {
            SummonPlayerSkill summonPlayerSkill = PlayerSkillManager.Instance.GetSummonPlayerSkillData(SPELL_TYPE.PLAGUED_RAT);
            if(summonPlayerSkill.charges < 3) {
                PlayerManager.Instance.player.playerSkillComponent.AddCharges(SPELL_TYPE.PLAGUED_RAT, 1);
            }
        }
        #endregion
    }
}

#region Save Data
public class SaveDataBiolab : SaveDataDemonicStructure {
    public GameDate replenishDate;

    public override void Save(LocationStructure structure) {
        base.Save(structure);
        Biolab biolab = structure as Biolab;
        replenishDate = biolab.replenishDate;
    }
}
#endregion