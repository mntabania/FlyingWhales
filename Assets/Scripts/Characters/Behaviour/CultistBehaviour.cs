
using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UtilityScripts;

public class CultistBehaviour : CharacterBehaviourComponent {

    public CultistBehaviour() {
        priority = 18;
    }
    
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        if (character.homeSettlement == null && !WorldSettings.Instance.worldSettingsData.villageSettings.disableNewVillages && !character.currentRegion.IsRegionVillageCapacityReached() && character.faction != null && 
            character.faction.factionType.type == FACTION_TYPE.Demon_Cult && character.characterClass.className == "Cult Leader") {
            Area targetArea = character.currentRegion.GetRandomHexThatMeetCriteria(currArea => currArea.elevationType != ELEVATION.WATER && currArea.elevationType != ELEVATION.MOUNTAIN && !currArea.structureComponent.HasStructureInArea() && !currArea.IsNextToOrPartOfVillage() && !currArea.gridTileComponent.HasCorruption());
            if (targetArea != null) {
                StructureSetting structureSetting = new StructureSetting(STRUCTURE_TYPE.CITY_CENTER, character.faction.factionType.mainResource, true);
                List<GameObject> choices = InnerMapManager.Instance.GetIndividualStructurePrefabsForStructure(structureSetting);
                GameObject chosenStructurePrefab = CollectionUtilities.GetRandomElement(choices);
                return character.jobComponent.TriggerFindNewVillage(targetArea.gridTileComponent.centerGridTile, out producedJob, chosenStructurePrefab.name);
            }    
        }
        
        TIME_IN_WORDS timeInWords = GameManager.GetCurrentTimeInWordsOfTick();
        int chance = 0;
        if (timeInWords == TIME_IN_WORDS.EARLY_NIGHT) {
            chance = 6;
        } else if (timeInWords == TIME_IN_WORDS.LATE_NIGHT || timeInWords == TIME_IN_WORDS.AFTER_MIDNIGHT) {
            chance = 12;
        }

        // chance = 100;
        
        int roll = UnityEngine.Random.Range(0, 100);
        log += $"\nWill try to do cultist action. Chance is {chance.ToString()}. Roll is {roll.ToString()}";
        
        if (roll < chance) {
            return TryCreateCultistJob(character, ref log, out producedJob);
        }
        producedJob = null;
        return false;
    }
    public override void OnAddBehaviourToCharacter(Character character) {
        base.OnAddBehaviourToCharacter(character);
        character.behaviourComponent.OnBecomeCultist();
    }
    public override void OnRemoveBehaviourFromCharacter(Character character) {
        base.OnRemoveBehaviourFromCharacter(character);
        character.behaviourComponent.OnNoLongerCultist();
    }
    public override void OnLoadBehaviourToCharacter(Character character) {
        base.OnLoadBehaviourToCharacter(character);
        character.behaviourComponent.OnBecomeCultist();
    }
    public bool TryCreateCultistJob(Character character, ref string log, out JobQueueItem producedJob) {
        if (character.HasItem(TILE_OBJECT_TYPE.CULTIST_KIT) == false 
            && character.homeStructure?.GetFirstTileObjectOfType<TileObject>(TILE_OBJECT_TYPE.CULTIST_KIT) == null) {
            log += $"\n{character.name} has no cultist kit available. Will create obtain personal item job.";
            bool success = character.jobComponent.TryCreateObtainPersonalItemJob("Cultist Kit", out producedJob);
            if (success) {
                producedJob.AddOtherData(INTERACTION_TYPE.TAKE_RESOURCE, new object[] { TileObjectDB.GetTileObjectData(TILE_OBJECT_TYPE.CULTIST_KIT).mainRecipe });
            }
            return success;
        } else {
            Character targetCharacter;
            if (GameUtilities.RollChance(30) && character.jobComponent.TryGetValidSabotageNeighbourTarget(out targetCharacter)) {
                log += $"\n{character.name} has cultist kit available. Will create sabotage neighbour job.";
                return character.jobComponent.TryCreateSabotageNeighbourJob(targetCharacter, out producedJob);    
            } else if (GameUtilities.RollChance(30) && character.jobComponent.TryGetValidEvangelizeTarget(out targetCharacter)) {//30
                log += $"\n{character.name} has cultist kit available and could not sabotage neighbour. Will create evangelize job.";
                return character.jobComponent.TryCreateEvangelizeJob(targetCharacter, out producedJob);    
            } else {
                return character.jobComponent.TryCreateDarkRitualJob(out producedJob);
            }
        }
    }
}
