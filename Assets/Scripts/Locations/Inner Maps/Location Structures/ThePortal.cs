using System;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UtilityScripts;
namespace Inner_Maps.Location_Structures {
    public class ThePortal : DemonicStructure {

        public int level { get; private set; }
        public PortalUpgradeTier currentTier {
            get {
                PlayerSkillLoadout skillLoadout = PlayerSkillManager.Instance.GetSelectedLoadout();
                PortalUpgradeTier tier = skillLoadout.portalUpgradeTiers[level - 1];
                return tier;
            }
        }
        public PortalUpgradeTier nextTier {
            get {
                PlayerSkillLoadout skillLoadout = PlayerSkillManager.Instance.GetSelectedLoadout();
                PortalUpgradeTier tier = skillLoadout.portalUpgradeTiers[level];
                return tier;
            }
        }
        public override Type serializedData => typeof(SaveDataThePortal);
        
        public ThePortal(Region location) : base(STRUCTURE_TYPE.THE_PORTAL, location){
            name = "Portal";
            SetMaxHPAndReset(5000);
            level = 1;
            Messenger.AddListener(UISignals.START_GAME_AFTER_LOADOUT_SELECT, StartGameAfterLoadoutSelected);
        }
        public ThePortal(Region location, SaveDataDemonicStructure data) : base(location, data) {
            if (data is SaveDataThePortal portal) {
                level = portal.level;
            } else {
                level = 1;    
            }
        }
        protected override void DestroyStructure() {
            base.DestroyStructure();
            PlayerUI.Instance.LoseGameOver();
        }
        private void StartGameAfterLoadoutSelected() {
            Messenger.RemoveListener(UISignals.START_GAME_AFTER_LOADOUT_SELECT, StartGameAfterLoadoutSelected);
            GainUpgradePowers(currentTier);
        }
        public override void ConstructDefaultActions() {
            base.ConstructDefaultActions();
            AddPlayerAction(PLAYER_SKILL_TYPE.RELEASE_ABILITIES);
            AddPlayerAction(PLAYER_SKILL_TYPE.UPGRADE_PORTAL);
        }
        
        #region Structure Object
        public override void SetStructureObject(LocationStructureObject structureObj) {
            base.SetStructureObject(structureObj);
            Vector3 position = structureObj.transform.position;
            position.x -= 0.5f;
            position.y += 0.1f;
            worldPosition = position;
        }
        #endregion

        #region Level Up
        public bool IsMaxLevel() {
            PlayerSkillLoadout skillLoadout = PlayerSkillManager.Instance.GetSelectedLoadout();
            return skillLoadout.portalUpgradeTiers.IsLastIndex(level - 1);
        }
        public void IncreaseLevel() {
            level++;
        }
        public void GainUpgradePowers(PortalUpgradeTier p_tier) {
            for (int i = 0; i < p_tier.skillTypesToUnlock.Length; i++) {
                PLAYER_SKILL_TYPE skill = p_tier.skillTypesToUnlock[i];
                PlayerManager.Instance.player.playerSkillComponent.AddMaxCharges(skill, 1);
            }
            for (int i = 0; i < p_tier.passiveSkillsToUnlock.Length; i++) {
                PASSIVE_SKILL passiveSkill = p_tier.passiveSkillsToUnlock[i];
                PlayerManager.Instance.player.playerSkillComponent.AddPassiveSkills(passiveSkill);
            }
        }
        public void PayForUpgrade(PortalUpgradeTier p_tier) {
            for (int i = 0; i < p_tier.upgradeCost.Length; i++) {
                PlayerManager.Instance.player.ReduceCurrency(p_tier.upgradeCost[i]);    
            }
        }
        #endregion
    }
}

public class SaveDataThePortal : SaveDataDemonicStructure {
    public int level;
    public override void Save(LocationStructure locationStructure) {
        base.Save(locationStructure);
        if (locationStructure is ThePortal portal) {
            level = portal.level;
        }
    }
}