using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Logs;
using UnityEngine;
using UtilityScripts;

public class FoundCultData : PlayerAction {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.FOUND_CULT;
    public override string name => "Found Cult";
    public override string description => "This Ability instructs the Cult Leader to start a new Demon Cult faction. Available only on Cult Leaders.";
    public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.PLAYER_ACTION;
    public override bool canBeCastOnBlessed => true;
    
    public FoundCultData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        if (targetPOI is Character character) {
            character.MigrateHomeStructureTo(null);
            character.interruptComponent.TriggerInterrupt(INTERRUPT.Create_Faction, character);
            Messenger.Broadcast(PlayerSkillSignals.RELOAD_PLAYER_ACTIONS, targetPOI as IPlayerActionTarget);
            if (!WorldSettings.Instance.worldSettingsData.villageSettings.disableNewVillages) {
                if (!character.currentRegion.IsRegionVillageCapacityReached()) {
                    // Area targetArea = character.currentRegion.GetRandomHexThatMeetCriteria(a => a.elevationType != ELEVATION.WATER && a.elevationType != ELEVATION.MOUNTAIN && !a.structureComponent.HasStructureInArea() && !a.IsNextToOrPartOfVillage() && !a.gridTileComponent.HasCorruption());
                    VillageSpot villageSpot = character.currentRegion.GetRandomUnoccupiedVillageSpot();
                    if (villageSpot != null) {
                        Area targetArea = villageSpot.mainSpot;
                        StructureSetting structureSetting = new StructureSetting(STRUCTURE_TYPE.CITY_CENTER, character.faction.factionType.mainResource, character.faction.factionType.usesCorruptedStructures);
                        List<GameObject> choices = InnerMapManager.Instance.GetStructurePrefabsForStructure(structureSetting);
                        GameObject chosenStructurePrefab = CollectionUtilities.GetRandomElement(choices);
                        if (LandmarkManager.Instance.HasEnoughSpaceForStructure(chosenStructurePrefab.name, targetArea.gridTileComponent.centerGridTile)) {
                            character.jobComponent.TriggerFindNewVillage(targetArea.gridTileComponent.centerGridTile, chosenStructurePrefab.name);
                        }
                    }    
                } else {
                    PlayerUI.Instance.ShowGeneralConfirmation("Village Capacity Reached", 
                        $"{character.visuals.GetCharacterNameWithIconAndColor()} has founded a new faction: {character.faction.nameWithColor}, but can no longer build a village on " +
                        $"{character.currentRegion.name} since the limit has been reached!");
                }
            }
        }
    }
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        bool canPerform = base.CanPerformAbilityTowards(targetCharacter);
        if (canPerform) {
            if (targetCharacter.isFactionLeader) {
                return false;
            }
            if (targetCharacter.faction != null && targetCharacter.faction.factionType.type == FACTION_TYPE.Demon_Cult) {
                return false;
            }
            if (targetCharacter.traitContainer.HasTrait("Enslaved")) {
                return false;
            }
            int villagerFactionCount = FactionManager.Instance.GetActiveVillagerFactionCount();
            if (villagerFactionCount >= FactionManager.MaxActiveVillagerFactions) {
                return false;
            }
            return true;
        }
        return false;
    }
    public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter) {
        string reasons = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter); 
        if (targetCharacter.isFactionLeader) {
            reasons += "Character is already a Faction Leader,";
        }
        if (targetCharacter.faction != null && targetCharacter.faction.factionType.type == FACTION_TYPE.Demon_Cult) {
            reasons += "Character is already part of a Demon Cult,";
        }
        if (targetCharacter.traitContainer.HasTrait("Enslaved")) {
            reasons += "Slaves cannot perform this action,";
        }
        int villagerFactionCount = FactionManager.Instance.GetActiveVillagerFactionCount();
        if (villagerFactionCount >= FactionManager.MaxActiveVillagerFactions) {
            reasons += "Maximum number of active factions have been reached,";
        }
        return reasons;
    }
    public override bool IsValid(IPlayerActionTarget target) {
        if (target is Character character) {
            if (character.isDead) {
                return false;
            }
            if (character.characterClass.className != "Cult Leader") {
                return false;
            }
            if (WorldSettings.Instance.worldSettingsData.factionSettings.disableNewFactions) {
                return false;
            }
        }
        return base.IsValid(target);
    }
    #endregion
}