using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Assertions;
using Inner_Maps.Location_Structures;

namespace Inner_Maps.Location_Structures {
    public class Biolab : DemonicStructure {
        public GameDate replenishDate { get; private set; }

        public override string scenarioDescription => "The Biolab allows the player to afflict Villagers with a customizable Plague. You can customize your Plague's transmission, symptoms and other effects by spending Chaotic Energy.\n\nIf the Biolab is built, the player also has access to the Plagued Rats spell. This spell spawns 2 Plagued Rats that may be able to spread the Plague to the world.";

        #region getters
        public override System.Type serializedData => typeof(SaveDataBiolab);
        #endregion

        public Biolab(Region location) : base(STRUCTURE_TYPE.BIOLAB, location) {
            SetMaxHPAndReset(5000);
        }
        public Biolab(Region location, SaveDataBiolab data) : base(location, data) {
            //replenishDate = data.replenishDate;
            //SchedulingManager.Instance.AddEntry(replenishDate, ProcessReplenishingOfPlaguedRatCharge, null);
        }

        #region Overrides
        //public override void OnBuiltNewStructure() {
        //    base.OnBuiltNewStructure();
        //    ReplenishPlaguedRatChargeWith3MaxCharges();
        //    ScheduleReplenishOfPlaguedRatCharge();
        //}
        public override void ConstructDefaultActions() {
            base.ConstructDefaultActions();
            AddPlayerAction(PLAYER_SKILL_TYPE.UPGRADE);
        }
        #endregion

        //#region Plagued Rat
        //private void ScheduleReplenishOfPlaguedRatCharge() {
        //    replenishDate = GameManager.Instance.Today().AddDays(2);
        //    SchedulingManager.Instance.AddEntry(replenishDate, ProcessReplenishingOfPlaguedRatCharge, null);
        //}
        //private void ProcessReplenishingOfPlaguedRatCharge() {
        //    if(!hasBeenDestroyed && tiles.Count > 0) {
        //        ReplenishPlaguedRatChargeWith3MaxCharges();
        //        ScheduleReplenishOfPlaguedRatCharge();
        //    }
        //}
        //private void ReplenishPlaguedRatChargeWith3MaxCharges() {
        //    if(!HasMaxPlaguedRat()) {
        //        PlayerManager.Instance.player.playerSkillComponent.AddCharges(PLAYER_SKILL_TYPE.PLAGUED_RAT, 1);
        //    }
        //}
        //public bool HasMaxPlaguedRat() {
        //    SummonPlayerSkill summonPlayerSkill = PlayerSkillManager.Instance.GetSummonPlayerSkillData(PLAYER_SKILL_TYPE.PLAGUED_RAT);
        //    return summonPlayerSkill.charges >= 3;
        //}
        //#endregion
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