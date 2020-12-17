using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;
using Logs;
using Locations.Settlements;

public class InduceMigrationData : SchemeData {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.INDUCE_MIGRATION;
    public override string name => "Induce Migration";
    public override string description => "Induce Migration";
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
            base.ActivateAbility(targetSettlement);
        }
    }
    public override bool IsValid(IPlayerActionTarget target) {
        return target is NPCSettlement npcSettlement && npcSettlement.migrationComponent.IsMigrationEventAllowed() && base.IsValid(target);
    }
    #endregion
}
