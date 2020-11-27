using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using Locations.Tile_Features;
using Traits;
using UnityEngine;
using UtilityScripts;

public class WolfBehaviour : CharacterBehaviourComponent {
    
    public WolfBehaviour() {
        priority = 9;
    }
    
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
        log += $"\n-{character.name} is a Wolf";
        if ((character.homeStructure == null || character.homeStructure.hasBeenDestroyed) && !character.HasTerritory()) {
            log += $"\n-{character.name} has no home";
            //wolf has no home structure
            List<BaseSettlement> settlementChoices = null;
            //find Settlement where wolves are living at
            for (int i = 0; i < character.currentRegion.settlementsInRegion.Count; i++) {
                BaseSettlement settlement = character.currentRegion.settlementsInRegion[i];
                if (settlement is NPCSettlement && settlement.HasResidentThatMeetsCriteria(resident => character != resident && resident.race == RACE.WOLF)) {
                    if (settlementChoices == null) {
                        settlementChoices = new List<BaseSettlement>();
                    }
                    settlementChoices.Add(settlement);
                }
            }

            if (settlementChoices != null) {
                //if there is a settlement found, set the wolf's home to that
                BaseSettlement randomSettlement = CollectionUtilities.GetRandomElement(settlementChoices);
                log += $"\n-Found valid settlement {randomSettlement.name}";
                LocationStructure randomStructure = randomSettlement.GetRandomStructure(structure =>
                    structure.structureType != STRUCTURE_TYPE.WILDERNESS && structure.CanBeResidentHere(character));
                if (randomStructure != null) {
                    log += $"\n-Found valid structure at {randomSettlement.name}. Structure is {randomStructure.name}. Setting home to that.";
                    character.MigrateHomeStructureTo(randomStructure);
                    return true; //will return here, because character will gain return home urgent after this
                }
            } else {
                log += $"\n-Could not find valid settlement checking unoccupied monster lairs";
                List<LocationStructure> monsterLairs = character.currentRegion.GetStructuresAtLocation<LocationStructure>(STRUCTURE_TYPE.MONSTER_LAIR);
                List<LocationStructure> choices = null;
                //if there were no settlements found, then check if there are any unoccupied monster lairs
                for (int i = 0; i < monsterLairs.Count; i++) {
                    LocationStructure monsterLair = monsterLairs[i];
                    if (monsterLair.CanBeResidentHere(character)) {
                        if (choices == null) {
                            choices = new List<LocationStructure>();
                        }
                        choices.Add(monsterLair);
                    }
                }
                if (choices != null) {
                    LocationStructure randomStructure = CollectionUtilities.GetRandomElement(choices);
                    log += $"\n-Found unoccupied monster lair {randomStructure.name}. Setting home to that.";
                    character.MigrateHomeStructureTo(randomStructure);
                    return true; //will return here, because character will gain return home urgent after this
                }
            }

            HexTile chosenHex = null;
            if(character.gridTileLocation.collectionOwner.isPartOfParentRegionMap) {
                HexTile targetHex = character.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner;
                if(targetHex != null && targetHex.elevationType != ELEVATION.WATER && targetHex.elevationType != ELEVATION.MOUNTAIN && targetHex.landmarkOnTile == null && !targetHex.IsNextToOrPartOfVillage()) {
                    chosenHex = targetHex;
                }
            }
            if (chosenHex == null) {
                chosenHex = GetNoStructurePlainHexInRegion(character.currentRegion);
            }
            if (chosenHex == null) {
                chosenHex = GetNoStructurePlainHexInAllRegions();
            }
            log += $"\n-{character.name} could not find valid home settlement and structure, will do build lair.";
            LocationGridTile centerTileOfHex = chosenHex.GetCenterLocationGridTile();
            //if none, wolf will create a monster lair away from village.
            //NOTE: Create monster lair action should check if a monster lair is already being built on a tile, to avoid conflicts    
            character.jobComponent.TriggerSpawnWolfLair(centerTileOfHex, out producedJob);
            return true;
        }

        if (UtilityScripts.Utilities.IsEven(GameManager.Instance.Today().day) &&
            GameManager.Instance.GetHoursBasedOnTicks(GameManager.Instance.Today().tick) == 6 && Random.Range(0, 2) == 1) {
            log += $"\n-Chance to hunt met. Will try to find target tile to hunt at.";
            List<HexTile> choices = character.currentRegion.GetTilesWithFeature(TileFeatureDB.Game_Feature).OrderBy(x =>
                    Vector2.Distance(x.GetCenterLocationGridTile().centeredWorldLocation, character.worldPosition))
                .ToList();
            if (choices.Count > 0) {
                HexTile tileWithGameFeature = choices[0];
                Hunting hunting = TraitManager.Instance.CreateNewInstancedTraitClass<Hunting>("Hunting");
                hunting.SetTargetTile(tileWithGameFeature);
                character.traitContainer.AddTrait(character, hunting);
                log += $"\n-Found valid hunting spot at {tileWithGameFeature}";
                return true;
            } else {
                log += $"\n-Could not find valid hunting spot";
                return false;
            }
        }
        log += $"\n-Chance to hunt not met";
        return false;
    }
    public override void OnAddBehaviourToCharacter(Character character) {
        base.OnAddBehaviourToCharacter(character);
        character.AddAdvertisedAction(INTERACTION_TYPE.BUILD_WOLF_LAIR);
    }
    public override void OnRemoveBehaviourFromCharacter(Character character) {
        base.OnRemoveBehaviourFromCharacter(character);
        character.RemoveAdvertisedAction(INTERACTION_TYPE.BUILD_WOLF_LAIR);
    }
    private HexTile GetNoStructurePlainHexInAllRegions() {
        HexTile chosenHex = null;
        for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
            Region region = GridMap.Instance.allRegions[i];
            chosenHex = GetNoStructurePlainHexInRegion(region);
            if (chosenHex != null) {
                return chosenHex;
            }
        }
        return chosenHex;
    }
    private HexTile GetNoStructurePlainHexInRegion(Region region) {
        return region.GetRandomHexThatMeetCriteria(currHex => currHex.elevationType != ELEVATION.WATER && currHex.elevationType != ELEVATION.MOUNTAIN && currHex.landmarkOnTile == null && !currHex.IsNextToOrPartOfVillage() && !currHex.isCorrupted);
    }
}
