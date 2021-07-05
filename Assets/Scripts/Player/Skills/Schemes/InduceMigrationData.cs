using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;
using Logs;
using Locations.Settlements;

public class InduceMigrationData : PlayerAction {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.INDUCE_MIGRATION;
    public override string name => "Induce Migration";
    public override string description => "This Ability forces a migration event of new residents to the target Village.";

    public InduceMigrationData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.SETTLEMENT, SPELL_TARGET.STRUCTURE };
    }

    #region Overrides
    public override void ActivateAbility(BaseSettlement targetSettlement) {
        if(targetSettlement is NPCSettlement npcSettlement) {
            npcSettlement.migrationComponent.SetVillageMigrationMeter(0);
            npcSettlement.migrationComponent.InduceMigrationEvent();
            Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "WorldEvents", "VillagerMigration", "induced_migration", providedTags: LOG_TAG.Major);
            log.AddToFillers(npcSettlement, npcSettlement.name, LOG_IDENTIFIER.LANDMARK_1);
            if(npcSettlement.owner != null) {
                log.AddToFillers(npcSettlement.owner, npcSettlement.owner.name, LOG_IDENTIFIER.FACTION_1);
            }
            log.AddLogToDatabase();
            PlayerManager.Instance.player.ShowNotificationFromPlayer(log, true);

            //LogSchemeVillage(npcSettlement);

            //PlayerSkillManager.Instance.GetPlayerActionData(PLAYER_SKILL_TYPE.SCHEME).OnExecutePlayerSkill();
            base.ActivateAbility(targetSettlement);
        }
    }
    public override void ActivateAbility(LocationStructure targetStructure) {
        if(targetStructure.settlementLocation != null) {
            ActivateAbility(targetStructure.settlementLocation);
        }
    }
    public override bool CanPerformAbilityTowards(BaseSettlement targetSettlement) {
        if (targetSettlement is NPCSettlement npcSettlement && npcSettlement.locationType != LOCATION_TYPE.VILLAGE /*!npcSettlement.migrationComponent.IsMigrationEventAllowed()*/) {
            return false;
        }
        if (targetSettlement.owner != null) {
            if (targetSettlement.owner.factionType.type != FACTION_TYPE.Human_Empire && targetSettlement.owner.factionType.type != FACTION_TYPE.Elven_Kingdom) {
                //Cannot induce migration to settlements owned by a faction but that faction is not a human empire or elven kingdom
                return false;
            }
        }
        //if (PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.SCHEME).charges <= 0) {
        //    return false;
        //}
        return base.CanPerformAbilityTowards(targetSettlement);
    }
    public override bool CanPerformAbilityTowards(LocationStructure targetStructure) {
        if (targetStructure.settlementLocation != null) {
            return CanPerformAbilityTowards(targetStructure.settlementLocation);
        }
        return base.CanPerformAbilityTowards(targetStructure);
    }
    public override bool IsValid(IPlayerActionTarget target) {
        if (target is Character) {
            return false;
        }
        return base.IsValid(target);
    }
    public override string GetReasonsWhyCannotPerformAbilityTowards(BaseSettlement p_targetSettlement) {
        string reasons = base.GetReasonsWhyCannotPerformAbilityTowards(p_targetSettlement);
        if (p_targetSettlement is NPCSettlement npcSettlement /*&& !npcSettlement.migrationComponent.IsMigrationEventAllowed()*/) {
            if (npcSettlement.locationType != LOCATION_TYPE.VILLAGE) {
                reasons += $"{p_targetSettlement.name} is not a village,";
            }
            if (npcSettlement.owner != null) {
                if (npcSettlement.owner.factionType.type != FACTION_TYPE.Human_Empire && npcSettlement.owner.factionType.type != FACTION_TYPE.Elven_Kingdom) {
                    //Cannot induce migration to settlements owned by a faction but that faction is not a human empire or elven kingdom
                    reasons += $"{p_targetSettlement.name} is not owned by a Human Empire or Elven Kingdom faction,";
                }
            }
            //if (npcSettlement.residents.Count <= 0) {
            //    reasons += $"{p_targetSettlement.name} does not have any residents,";
            //}
            //if (npcSettlement.owner != null && !npcSettlement.owner.isMajorNonPlayer) {
            //    reasons += $"{p_targetSettlement.name} is not owned by a major faction,";
            //}
            //if (npcSettlement.owner != null && npcSettlement.owner.factionType.type != FACTION_TYPE.Human_Empire && npcSettlement.owner.factionType.type != FACTION_TYPE.Elven_Kingdom) {
            //    reasons += $"{p_targetSettlement.name} is not owned by a Human Empire or Elven Kingdom faction,";
            //}
        }
        return reasons;
    }
    public override string GetReasonsWhyCannotPerformAbilityTowards(LocationStructure p_targetStructure) {
        if (p_targetStructure.settlementLocation != null) {
            return GetReasonsWhyCannotPerformAbilityTowards(p_targetStructure.settlementLocation);
        }
        return base.GetReasonsWhyCannotPerformAbilityTowards(p_targetStructure);
    }
    #endregion
}
