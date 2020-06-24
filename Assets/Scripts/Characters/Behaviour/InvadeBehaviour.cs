using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Locations.Settlements;

public class InvadeBehaviour : CharacterBehaviourComponent {
    public InvadeBehaviour() {
        priority = 10;
    }
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
        log += $"\n-{character.name} will invade";
        if (character.gridTileLocation.collectionOwner.isPartOfParentRegionMap
            && character.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner 
            && character.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner.settlementOnTile == character.behaviourComponent.assignedTargetSettlement) {
            log += "\n-Already in the target npcSettlement, will try to combat residents";
            //It will only go here if the invader is not combat anymore, meaning there are no more hostiles in his vision, so we must make sure that he attacks a resident in the settlement even though he can't see it
            character.behaviourComponent.invadeCombatantTargetList.Clear();
            character.behaviourComponent.invadeNonCombatantTargetList.Clear();
            BaseSettlement settlement = character.behaviourComponent.assignedTargetSettlement;
            for (int i = 0; i < settlement.residents.Count; i++) {
                Character resident = settlement.residents[i];
                if (!resident.isDead && resident.gridTileLocation != null && resident.gridTileLocation.IsPartOfSettlement(settlement) 
                    && resident.isAlliedWithPlayer == false) {
                    if (resident.traitContainer.HasTrait("Combatant")) {
                        character.behaviourComponent.invadeCombatantTargetList.Add(resident);
                    } else {
                        character.behaviourComponent.invadeNonCombatantTargetList.Add(resident);
                    }
                }
            }
            if(character.behaviourComponent.invadeCombatantTargetList.Count > 0) {
                Character chosenTarget = character.behaviourComponent.invadeCombatantTargetList[UnityEngine.Random.Range(0, character.behaviourComponent.invadeCombatantTargetList.Count)];
                log += "\n-Will attack combatant resident: " + chosenTarget.name;
                character.combatComponent.Fight(chosenTarget, CombatManager.Hostility);
            } else if (character.behaviourComponent.invadeNonCombatantTargetList.Count > 0) {
                Character chosenTarget = character.behaviourComponent.invadeNonCombatantTargetList[UnityEngine.Random.Range(0, character.behaviourComponent.invadeNonCombatantTargetList.Count)];
                log += "\n-Will attack non-combatant resident: " + chosenTarget.name;
                character.combatComponent.Fight(chosenTarget, CombatManager.Hostility);
                //character.Death();
            } else {
                log += "\n-No resident found in settlement, dissipate";
                //character.jobComponent.TriggerRoamAroundTile();
                character.Death();
            }
        } else {
            log += "\n-Is not in the target npcSettlement";
            log += "\n-Roam there";
            HexTile targetHex = character.behaviourComponent.assignedTargetSettlement.tiles[UnityEngine.Random.Range(0, character.behaviourComponent.assignedTargetSettlement.tiles.Count)];
            LocationGridTile targetTile = targetHex.locationGridTiles[UnityEngine.Random.Range(0, targetHex.locationGridTiles.Count)];
            character.jobComponent.TriggerRoamAroundTile(out producedJob, targetTile);
        }
        return true;
    }
}
