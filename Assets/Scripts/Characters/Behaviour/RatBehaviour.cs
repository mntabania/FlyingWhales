﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Locations.Settlements;
using UtilityScripts;
using Inner_Maps.Location_Structures;

public class RatBehaviour : CharacterBehaviourComponent {
    public RatBehaviour() {
        priority = 10;
    }
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        log += $"\n{character.name} is a Rat";
        TIME_IN_WORDS currentTimeInWords = GameManager.GetCurrentTimeInWordsOfTick();
        if (currentTimeInWords == TIME_IN_WORDS.LATE_NIGHT || currentTimeInWords == TIME_IN_WORDS.AFTER_MIDNIGHT) {
            if (character.behaviourComponent.pestVillageTarget == null) {
                character.behaviourComponent.SetPestVillageTarget(GetVillageTargetsByPriority(character));
            }
            if (character.behaviourComponent.pestVillageTarget != null && character.behaviourComponent.pestVillageTarget.Count > 0) {
                log += $"\n-Already has village target";
                BaseSettlement targetSettlement = character.behaviourComponent.pestVillageTarget[0].settlementOnTile;
                if (targetSettlement != null) {
                    LocationStructure targetStructure = targetSettlement.GetRandomStructure();
                    if (targetStructure != null) {
                        LocationGridTile targetTile = targetStructure.GetRandomPassableTile();
                        if (targetTile != null) {
                            return character.jobComponent.CreateGoToJob(targetTile, out producedJob);
                        }
                    }
                }
            }
            return character.jobComponent.TriggerRoamAroundTile(JOB_TYPE.ROAM_AROUND_TILE, out producedJob);
        } else {
            if (character.behaviourComponent.pestVillageTarget != null) {
                character.behaviourComponent.SetPestVillageTarget(null);
            }
            BaseSettlement settlement = null;
            if (character.gridTileLocation != null && character.gridTileLocation.IsPartOfSettlement(out settlement)) {
                int chanceToLeave = 30;
                if (settlement.locationType == LOCATION_TYPE.VILLAGE) {
                    chanceToLeave = 60;
                }
                if (GameUtilities.RollChance(chanceToLeave)) {
                    if (character.currentRegion != null) {
                        HexTile targetHex = character.currentRegion.GetRandomHexThatMeetCriteria(h => h.settlementOnTile == null && h.elevationType != ELEVATION.WATER && h.elevationType != ELEVATION.MOUNTAIN);
                        if (targetHex != null) {
                            LocationGridTile targetTile = CollectionUtilities.GetRandomElement(targetHex.locationGridTiles);
                            if (targetTile != null) {
                                return character.jobComponent.CreateGoToJob(targetTile, out producedJob);
                            }
                        }
                    }
                }
            }
            return character.jobComponent.TriggerRoamAroundTile(JOB_TYPE.ROAM_AROUND_TILE, out producedJob);
        }
    }
    private List<HexTile> GetVillageTargetsByPriority(Character owner) {
        //get settlements in region that have normal characters living there.
        List<BaseSettlement> settlementsInRegion = owner.currentRegion?.GetSettlementsInRegion(
            settlement => settlement.HasResidentThatMeetsCriteria(x => !x.isDead)
        );
        if (settlementsInRegion != null) {
            List<BaseSettlement> villageChoices = settlementsInRegion.Where(
                x => x.locationType == LOCATION_TYPE.VILLAGE
            ).ToList();
            if (villageChoices.Count > 0) {
                //a random village occupied by Villagers within current region
                BaseSettlement chosenVillage = CollectionUtilities.GetRandomElement(villageChoices);
                return new List<HexTile>(chosenVillage.tiles);
            } else {
                //a random special structure occupied by Villagers within current region
                List<BaseSettlement> specialStructureChoices = settlementsInRegion.Where(x => x.locationType == LOCATION_TYPE.DUNGEON).ToList();
                if (specialStructureChoices.Count > 0) {
                    BaseSettlement chosenSpecialStructure = CollectionUtilities.GetRandomElement(specialStructureChoices);
                    return new List<HexTile>(chosenSpecialStructure.tiles);
                }
            }
        }
        // //no settlements in region.
        // //a random area occupied by Villagers within current region
        // List<HexTile> occupiedAreas = owner.currentRegion?.GetAreasOccupiedByVillagers();
        // if (occupiedAreas != null) {
        //     HexTile randomArea = CollectionUtilities.GetRandomElement(occupiedAreas);
        //     return new List<HexTile>() { randomArea };
        // }
        return null;
    }
}