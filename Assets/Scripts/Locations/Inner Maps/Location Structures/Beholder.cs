using UnityEngine;

namespace Inner_Maps.Location_Structures {
    public class Beholder : DemonicStructure {
        public Beholder(Region location) : base(STRUCTURE_TYPE.BEHOLDER, location){ }
        public Beholder(Region location, SaveDataDemonicStructure data) : base(location, data) { }

        #region Overrides
        //Note: Removed this because of the update that the Beholder will now have max charges upon building instead of 1 charge only
        //https://trello.com/c/t4CezyZO/3805-eye-updates
        //public override void OnBuiltNewStructure() {
        //    base.OnBuiltNewStructure();
        //    //Spawn Eye Ward should start at 1 charge, not max charge
        //    PlayerAction spawnEyeWardAction = PlayerSkillManager.Instance.GetPlayerActionData(PLAYER_SKILL_TYPE.SPAWN_EYE_WARD);
        //    int chargeToDeduct = spawnEyeWardAction.charges - 1;
        //    if(chargeToDeduct > 0) {
        //        spawnEyeWardAction.AdjustCharges(-chargeToDeduct);
        //    }
        //}
        public override void ConstructDefaultActions() {
            base.ConstructDefaultActions();
            AddPlayerAction(PLAYER_SKILL_TYPE.SPAWN_EYE_WARD);
        }
        #endregion
    }
}