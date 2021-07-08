using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Locations.Settlements;
using UtilityScripts;
using Inner_Maps.Location_Structures;

public class PestBehaviour : CharacterBehaviourComponent {
    public PestBehaviour() {
        priority = 10;
    }
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
#if DEBUG_LOG
        log += $"\n{character.name} is a Pest";
#endif
        if (!character.limiterComponent.canDoFullnessRecovery) {
            if (character.behaviourComponent.pestSettlementTarget != null) {
                BaseSettlement targetSettlement = character.behaviourComponent.pestSettlementTarget;
                if (targetSettlement != null) {
                    if (character.currentRegion != null) {
                        Area targetArea = character.currentRegion.GetRandomAreaThatIsNotMountainAndWaterAndNoSettlement();
                        if (targetArea != null) {
                            LocationGridTile targetTile = targetArea.gridTileComponent.GetRandomPassableTile();
                            if (targetTile != null) {
                                character.behaviourComponent.SetPestSettlementTarget(null);
                                return character.jobComponent.CreateGoToJob(targetTile, out producedJob);
                            }
                        }
                    }
                }
            }
        } else {
            if (character.behaviourComponent.pestSettlementTarget == null) {
                BaseSettlement currentSettlement = character.currentSettlement;
                if (currentSettlement != null && (currentSettlement.locationType == LOCATION_TYPE.VILLAGE || currentSettlement.locationType == LOCATION_TYPE.DUNGEON)) {
                    character.behaviourComponent.SetPestSettlementTarget(currentSettlement);
                } else {
                    character.behaviourComponent.SetPestSettlementTarget(GetVillageTargetsByPriority(character));
                }
            }
            if (character.behaviourComponent.pestSettlementTarget != null) {
#if DEBUG_LOG
                log += $"\n-Already has village target";
#endif
                BaseSettlement targetSettlement = character.behaviourComponent.pestSettlementTarget;
                if (targetSettlement != null) {
                    if (character.gridTileLocation != null && character.gridTileLocation.IsPartOfSettlement(targetSettlement)) {
                        return character.jobComponent.CreateRatFullnessRecovery(targetSettlement, out producedJob);
                    } else {
                        LocationStructure targetStructure = targetSettlement.GetRandomStructure();
                        if (targetStructure != null) {
                            LocationGridTile targetTile = targetStructure.GetRandomPassableTile();
                            if (targetTile != null) {
                                return character.jobComponent.CreateGoToJob(targetTile, out producedJob);
                            }
                        }
                    }
                }
            }
        }
        return character.jobComponent.TriggerRoamAroundTile(out producedJob);
    }
    private BaseSettlement GetVillageTargetsByPriority(Character owner) {
        //get settlements in region that have normal characters living there.
        BaseSettlement chosenSettlement = null;
        List<BaseSettlement> settlementsInRegion = RuinarchListPool<BaseSettlement>.Claim(); 
        owner.currentRegion?.PopulateSettlementsInRegionForPestBehaviour(settlementsInRegion, owner);
        if (settlementsInRegion.Count > 0) {
            List<BaseSettlement> villageChoices = RuinarchListPool<BaseSettlement>.Claim();
            for (int i = 0; i < settlementsInRegion.Count; i++) {
                BaseSettlement s = settlementsInRegion[i];
                if (s.locationType == LOCATION_TYPE.VILLAGE) {
                    villageChoices.Add(s);
                }
            }
            if (villageChoices.Count > 0) {
                //a random village occupied by Villagers within current region
                chosenSettlement = CollectionUtilities.GetRandomElement(villageChoices);
            } else {
                //a random special structure occupied by Villagers within current region
                List<BaseSettlement> specialStructureChoices = RuinarchListPool<BaseSettlement>.Claim();
                for (int i = 0; i < settlementsInRegion.Count; i++) {
                    BaseSettlement s = settlementsInRegion[i];
                    if (s.locationType == LOCATION_TYPE.DUNGEON) {
                        specialStructureChoices.Add(s);
                    }
                }
                if (specialStructureChoices.Count > 0) {
                    chosenSettlement = CollectionUtilities.GetRandomElement(specialStructureChoices);
                }
                RuinarchListPool<BaseSettlement>.Release(specialStructureChoices);
            }
            RuinarchListPool<BaseSettlement>.Release(villageChoices);
        }
        RuinarchListPool<BaseSettlement>.Release(settlementsInRegion);
        // //no settlements in region.
        // //a random area occupied by Villagers within current region
        // List<HexTile> occupiedAreas = owner.currentRegion?.GetAreasOccupiedByVillagers();
        // if (occupiedAreas != null) {
        //     HexTile randomArea = CollectionUtilities.GetRandomElement(occupiedAreas);
        //     return new List<HexTile>() { randomArea };
        // }
        return chosenSettlement;
    }
}
