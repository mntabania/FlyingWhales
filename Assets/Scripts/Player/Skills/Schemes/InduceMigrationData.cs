using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;
using Logs;
using Locations.Settlements;

public class InduceMigrationData : SchemeData {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.INDUCE_MIGRATION;
    public override string name => "Induce Migration";
    public override string description => "Force a Migration Event on the current Village.";
    public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SCHEME;

    public InduceMigrationData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.SETTLEMENT };
    }

    #region Overrides
    public override void ActivateAbility(BaseSettlement targetSettlement) {
        if(targetSettlement is NPCSettlement npcSettlement) {
            npcSettlement.migrationComponent.SetVillageMigrationMeter(1000);
            Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "WorldEvents", "VillagerMigration", "induced_migration", providedTags: LOG_TAG.Major);
            log.AddToFillers(npcSettlement, npcSettlement.name, LOG_IDENTIFIER.LANDMARK_1);
            if(npcSettlement.owner != null) {
                log.AddToFillers(npcSettlement.owner, npcSettlement.owner.name, LOG_IDENTIFIER.FACTION_1);
            }
            log.AddLogToDatabase();
            PlayerManager.Instance.player.ShowNotificationFromPlayer(log);

            LogSchemeVillage(npcSettlement);
            base.ActivateAbility(targetSettlement);
        }
    }
    public override bool CanPerformAbilityTowards(BaseSettlement targetSettlement) {
        if (targetSettlement is NPCSettlement npcSettlement && !npcSettlement.migrationComponent.IsMigrationEventAllowed()) {
            return false;
        }
        return base.CanPerformAbilityTowards(targetSettlement);
    }
    public override string GetReasonsWhyCannotPerformAbilityTowards(BaseSettlement p_targetSettlement) {
        string reasons = base.GetReasonsWhyCannotPerformAbilityTowards(p_targetSettlement);
        if (p_targetSettlement is NPCSettlement npcSettlement && !npcSettlement.migrationComponent.IsMigrationEventAllowed()) {
            if(npcSettlement.owner == null) {
                reasons += $"{p_targetSettlement.name} does not have a faction owner,";
            }
            if (npcSettlement.residents.Count <= 0) {
                reasons += $"{p_targetSettlement.name} does not have any residents,";
            }
            if (npcSettlement.owner != null && !npcSettlement.owner.isMajorNonPlayer) {
                reasons += $"{p_targetSettlement.name} is not owned by a major faction,";
            }
            if (npcSettlement.owner != null && npcSettlement.owner.factionType.type != FACTION_TYPE.Human_Empire && npcSettlement.owner.factionType.type != FACTION_TYPE.Elven_Kingdom) {
                reasons += $"{p_targetSettlement.name} is not owned by a Human Empire or Elven Kingdom faction,";
            }
        }
        return reasons;
    }
    public override bool IsValid(IPlayerActionTarget target) {
        if (!(target is NPCSettlement)) {
            return false;
        }
        return base.IsValid(target);
    }
    #endregion
}
