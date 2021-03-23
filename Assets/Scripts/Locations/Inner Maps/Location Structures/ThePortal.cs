using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UtilityScripts;
namespace Inner_Maps.Location_Structures {
    public class ThePortal : DemonicStructure {

        public int level { get; private set; }
        public ThePortal(Region location) : base(STRUCTURE_TYPE.THE_PORTAL, location){
            name = "Portal";
            SetMaxHPAndReset(5000);
            level = 1;
        }
        public ThePortal(Region location, SaveDataDemonicStructure data) : base(location, data) {
            level = 1;
        }
        protected override void DestroyStructure() {
            base.DestroyStructure();
            PlayerUI.Instance.LoseGameOver();
        }
        
        #region Structure Object
        public override void ConstructDefaultActions() {
            base.ConstructDefaultActions();
            AddPlayerAction(PLAYER_SKILL_TYPE.UNLOCK_ABILITIES);
        }
        public override void SetStructureObject(LocationStructureObject structureObj) {
            base.SetStructureObject(structureObj);
            Vector3 position = structureObj.transform.position;
            position.x -= 0.5f;
            position.y += 0.1f;
            worldPosition = position;
        }
        #endregion

        #region Level Up
        public void LevelUp() {
            level++;
            PlayerSkillLoadout skillLoadout = PlayerSkillManager.Instance.GetSelectedLoadout();
            PortalUpgradeTier tier = skillLoadout.portalUpgradeTiers[level - 1];
            GainUpgradePowers(tier);
            PayForUpgrade(tier);
        }
        private void GainUpgradePowers(PortalUpgradeTier p_tier) {
            for (int i = 0; i < p_tier.skillTypesToUnlock.Length; i++) {
                PLAYER_SKILL_TYPE skill = p_tier.skillTypesToUnlock[i];
                PlayerManager.Instance.player.playerSkillComponent.AddCharges(skill, 1);
            }
            for (int i = 0; i < p_tier.passiveSkillsToUnlock.Length; i++) {
                PASSIVE_SKILL passiveSkill = p_tier.passiveSkillsToUnlock[i];
                PlayerManager.Instance.player.playerSkillComponent.AddPassiveSkills(passiveSkill);
            }
        }
        private void PayForUpgrade(PortalUpgradeTier p_tier) {
            for (int i = 0; i < p_tier.upgradeCost.Length; i++) {
                PlayerManager.Instance.player.AdjustCurrency(p_tier.upgradeCost[i]);    
            }
        }
        #endregion
    }
}