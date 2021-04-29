using System;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UtilityScripts;
namespace Inner_Maps.Location_Structures {
    public class ThePortal : DemonicStructure {

        public override string scenarioDescription => "The Portal is the Ruinarch's primary connection to this world. Protect it at all costs! Spend Chaotic Energy to obtain Bonus Charges of Powers from the other Archetypes here.";
        public override string customDescription => "The Portal is the Ruinarch's primary connection to this world. Protect it at all costs! Spend Spirit Energy to upgrade the Portal and permanently unlock new Powers. You may also spend Chaotic Energy to obtain Bonus Charges of Powers from the other Archetypes here.";

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
            SetMaxHPAndReset(13666);
            if (WorldSettings.Instance.worldSettingsData.IsScenarioMap()) {
                level = PlayerSkillLoadout.MAX_SKILLS_PER_UPGRADE_TIER - 1; //start portal at max level - 1 for scenarios. This is so that release abilities can show most powers
            } else {
                level = 1;    
            }
            // Messenger.AddListener(UISignals.START_GAME_AFTER_LOADOUT_SELECT, StartGameAfterLoadoutSelected);
        }
        public ThePortal(Region location, SaveDataDemonicStructure data) : base(location, data) {
            if (data is SaveDataThePortal portal) {
                level = portal.level;
            } else {
                level = 1;    
            }
        }
        protected override void DestroyStructure(Character p_responsibleCharacter = null, bool isPlayerSource = false) {
            base.DestroyStructure(p_responsibleCharacter, isPlayerSource);
            PlayerUI.Instance.LoseGameOver();
        }
        // private void StartGameAfterLoadoutSelected() {
        //     Messenger.RemoveListener(UISignals.START_GAME_AFTER_LOADOUT_SELECT, StartGameAfterLoadoutSelected);
        //     GainUpgradePowers(currentTier);
        // }
        public override void ConstructDefaultActions() {
            base.ConstructDefaultActions();
            AddPlayerAction(PLAYER_SKILL_TYPE.RELEASE_ABILITIES);
            AddPlayerAction(PLAYER_SKILL_TYPE.UPGRADE_PORTAL);
        }
        public override string GetTestingInfo() {
            string info = base.GetTestingInfo();
            info = $"{info}\nLevel: {level.ToString()}";
            return info;
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
        public bool IsCurrentLevelUpTheLastOne() {
            PlayerSkillLoadout skillLoadout = PlayerSkillManager.Instance.GetSelectedLoadout();
            return skillLoadout.portalUpgradeTiers.IsLastIndex(level);
        }
        public void IncreaseLevel() {
            level++;
        }
        public void GainUpgradePowers(PortalUpgradeTier p_tier) {
            for (int i = 0; i < p_tier.skillTypesToUnlock.Length; i++) {
                PLAYER_SKILL_TYPE skill = p_tier.skillTypesToUnlock[i];
                SkillData skillData = PlayerSkillManager.Instance.GetSkillData(skill);
                PlayerSkillData playerSkillData = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(skill);
                if (skillData.isInUse) {
                    skillData.AdjustMaxCharges(playerSkillData.unlockChargeOnPortalUpgrade);
                    skillData.AdjustCharges(playerSkillData.unlockChargeOnPortalUpgrade);
                } else {
                    PlayerManager.Instance.player.playerSkillComponent.AddAndCategorizePlayerSkill(skill);
                }
            }
            // for (int i = 0; i < p_tier.passiveSkillsToUnlock.Length; i++) {
            //     PASSIVE_SKILL passiveSkill = p_tier.passiveSkillsToUnlock[i];
            //     PlayerManager.Instance.player.playerSkillComponent.AddPassiveSkills(passiveSkill);
            // }
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