using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using UtilityScripts;

public class SettlementRulerBehaviour : CharacterBehaviourComponent {
    public SettlementRulerBehaviour() {
        priority = 20;
        attributes = new BEHAVIOUR_COMPONENT_ATTRIBUTE[] { BEHAVIOUR_COMPONENT_ATTRIBUTE.WITHIN_HOME_SETTLEMENT_ONLY, BEHAVIOUR_COMPONENT_ATTRIBUTE.ONCE_PER_DAY };
    }

    public override bool TryDoBehaviour(Character character, ref string log) {
        return false;
        log += $"\n-{character.name} will try to place blueprint";
        if (character.isAtHomeRegion && character.homeSettlement != null && character.homeSettlement.GetNumberOfJobsWith(JOB_TYPE.BUILD_BLUEPRINT) < 2 && HasCharacterWithPlaceBlueprintJobInSettlement(character.homeSettlement) == false) {
            log += $"\n-{character.name} will roll for blueprint placement.";
            int chance = 35;
            int roll = Random.Range(0, 100);
            log += $"\n-Roll is {roll.ToString()}, chance is {chance.ToString()}";
            if (roll < chance) {
                log += $"\n-Roll successful";
                STRUCTURE_TYPE neededStructure = character.buildStructureComponent.GetCurrentStructureToBuild();
                log += $"\n-Structure Type to build is {neededStructure.ToString()}";

                List<GameObject> choices = InnerMapManager.Instance.GetStructurePrefabsForStructure(neededStructure);
                GameObject chosenStructurePrefab = CollectionUtilities.GetRandomElement(choices);
                log += $"\n-Structure Prefab chosen is {chosenStructurePrefab.name}";

                LocationStructureObject lso = chosenStructurePrefab.GetComponent<LocationStructureObject>();
                StructureTileObject chosenBuildingSpot;
                // if (character.homeRegion.innerMap.TryGetValidBuildSpotTileObjectForStructure(lso, character.homeSettlement, out chosenBuildingSpot) == false) {
                //     log += $"\n-Could not find spot that can house new structure. Abandoning...";
                //     return false;
                // }
                log += $"\n-Creating new Place Blueprint job targeting {chosenBuildingSpot.ToString()} at {chosenBuildingSpot.gridTileLocation.ToString()}";
                GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.PLACE_BLUEPRINT, INTERACTION_TYPE.PLACE_BLUEPRINT, chosenBuildingSpot, character);
                job.AddOtherData(INTERACTION_TYPE.PLACE_BLUEPRINT, new object[] { neededStructure });
                character.jobQueue.AddJobInQueue(job);

                return true;
            }
        }
        log += $"\n-{character.name} failed to place blueprint";
        return false;
    }

    private bool HasCharacterWithPlaceBlueprintJobInSettlement(NPCSettlement npcSettlement) {
        for (int i = 0; i < npcSettlement.residents.Count; i++) {
            Character resident = npcSettlement.residents[i];
            if (resident.jobQueue.HasJob(JOB_TYPE.PLACE_BLUEPRINT)) {
                return true;
            }
        }
        return false;
    }
}
