using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Logs;
using UnityEngine;
using UtilityScripts;

public class FoundCultData : PlayerAction {
    public override SPELL_TYPE type => SPELL_TYPE.FOUND_CULT;
    public override string name => "Found Cult";
    public override string description => "This Action forces the character to create a new Demon Cult faction.";
    public override SPELL_CATEGORY category => SPELL_CATEGORY.PLAYER_ACTION;
    public override bool canBeCastOnBlessed => true;
    
    public FoundCultData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        if (targetPOI is Character character) {
            character.MigrateHomeStructureTo(null);
            character.interruptComponent.TriggerInterrupt(INTERRUPT.Create_Faction, character);
            Messenger.Broadcast(SpellSignals.RELOAD_PLAYER_ACTIONS, targetPOI as IPlayerActionTarget);
            if (!character.currentRegion.IsRegionVillageCapacityReached()) {
                HexTile targetTile = character.currentRegion.GetRandomHexThatMeetCriteria(currHex => currHex.elevationType != ELEVATION.WATER && currHex.elevationType != ELEVATION.MOUNTAIN && currHex.landmarkOnTile == null && !currHex.IsNextToOrPartOfVillage() && !currHex.isCorrupted);
                if (targetTile != null) {
                    StructureSetting structureSetting = new StructureSetting(STRUCTURE_TYPE.CITY_CENTER, character.faction.factionType.mainResource, character.faction.factionType.usesCorruptedStructures);
                    List<GameObject> choices = InnerMapManager.Instance.GetIndividualStructurePrefabsForStructure(structureSetting);
                    GameObject chosenStructurePrefab = CollectionUtilities.GetRandomElement(choices);
                    character.jobComponent.TriggerFindNewVillage(targetTile.GetCenterLocationGridTile(), chosenStructurePrefab.name);
                }    
            } else {
                PlayerUI.Instance.ShowGeneralConfirmation("Village Capacity Reached", 
                    $"{character.visuals.GetCharacterNameWithIconAndColor()} has founded a new faction: {character.faction.name}, but can no longer build a village on " +
                    $"{character.currentRegion.name} since the limit has been reached!");
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
        }
        return base.IsValid(target);
    }
    #endregion
}