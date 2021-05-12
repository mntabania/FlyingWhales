using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using Locations.Area_Features;
using Traits;
using UnityEngine;
using UtilityScripts;

public class WolfBehaviour : BaseMonsterBehaviour {
    
    public WolfBehaviour() {
        priority = 9;
    }
    protected override bool WildBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
        log += $"\n-{character.name} is a Wolf";
        if ((character.homeStructure == null || character.homeStructure.hasBeenDestroyed) && !character.HasTerritory()) {
            log += $"\n-{character.name} has no home";
            //wolf has no home structure
            List<BaseSettlement> settlementChoices = null;
            //find Settlement where wolves are living at
            for (int i = 0; i < character.currentRegion.settlementsInRegion.Count; i++) {
                BaseSettlement settlement = character.currentRegion.settlementsInRegion[i];
                if (settlement is NPCSettlement && settlement.HasResidentWithRace(RACE.WOLF, character)) {
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
                LocationStructure randomStructure = randomSettlement.GetRandomStructureThatCharacterCanBeResidentAndIsNot(character, STRUCTURE_TYPE.WILDERNESS);
                if (randomStructure != null) {
                    log += $"\n-Found valid structure at {randomSettlement.name}. Structure is {randomStructure.name}. Setting home to that.";
                    character.MigrateHomeStructureTo(randomStructure);
                    return true; //will return here, because character will gain return home urgent after this
                }
            } else {
                log += $"\n-Could not find valid settlement checking unoccupied monster lairs";
                List<LocationStructure> monsterLairs = character.currentRegion.GetStructuresAtLocation(STRUCTURE_TYPE.MONSTER_LAIR);
                List<LocationStructure> choices = RuinarchListPool<LocationStructure>.Claim();
                //if there were no settlements found, then check if there are any unoccupied monster lairs
                for (int i = 0; i < monsterLairs.Count; i++) {
                    LocationStructure monsterLair = monsterLairs[i];
                    if (monsterLair.CanBeResidentHere(character)) {
                        choices.Add(monsterLair);
                    }
                }
                LocationStructure randomStructure = null;
                if (choices.Count > 0) {
                    randomStructure = CollectionUtilities.GetRandomElement(choices);
                }
                RuinarchListPool<LocationStructure>.Release(choices);
                if (randomStructure != null) {
                    log += $"\n-Found unoccupied monster lair {randomStructure.name}. Setting home to that.";
                    character.MigrateHomeStructureTo(randomStructure);
                    return true; //will return here, because character will gain return home urgent after this
                }
            }

            Area chosenArea = null;
            Area targetArea = character.areaLocation;
            if(targetArea != null && targetArea.elevationComponent.IsFully(ELEVATION.PLAIN) && !targetArea.structureComponent.HasStructureInArea() && !targetArea.IsNextToOrPartOfVillage()) {
                chosenArea = targetArea;
            }
            if (chosenArea == null) {
                chosenArea = GetNoStructurePlainAreaInRegion(character.currentRegion);
            }
            if (chosenArea == null) {
                chosenArea = GetNoStructurePlainAreaInAllRegions();
            }
            log += $"\n-{character.name} could not find valid home settlement and structure, will do build lair.";
            LocationGridTile centerTileOfHex = chosenArea.gridTileComponent.centerGridTile;
            //if none, wolf will create a monster lair away from village.
            //NOTE: Create monster lair action should check if a monster lair is already being built on a tile, to avoid conflicts    
            character.jobComponent.TriggerSpawnWolfLair(centerTileOfHex, out producedJob);
            return true;
        }

        if (UtilityScripts.Utilities.IsEven(GameManager.Instance.Today().day) &&
            GameManager.Instance.GetHoursBasedOnTicks(GameManager.Instance.Today().tick) == 6 && Random.Range(0, 2) == 1) {
            log += $"\n-Chance to hunt met. Will try to find target tile to hunt at.";
            Area nearestArea = character.currentRegion.GetAreaWithFeatureThatIsNearestTo(AreaFeatureDB.Game_Feature, character);
            if (nearestArea != null) {
                Hunting hunting = TraitManager.Instance.CreateNewInstancedTraitClass<Hunting>("Hunting");
                hunting.SetTargetArea(nearestArea);
                character.traitContainer.AddTrait(character, hunting);
                log += $"\n-Found valid hunting spot at {nearestArea}";
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
    private Area GetNoStructurePlainAreaInAllRegions() {
        Area chosenArea = null;
        for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
            Region region = GridMap.Instance.allRegions[i];
            chosenArea = GetNoStructurePlainAreaInRegion(region);
            if (chosenArea != null) {
                return chosenArea;
            }
        }
        return chosenArea;
    }
    private Area GetNoStructurePlainAreaInRegion(Region region) {
        return region.GetRandomAreaThatIsUncorruptedFullyPlainNoStructureAndNotNextToOrPartOfVillage();
    }
}
