using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UtilityScripts;

public class InvadeBehaviour : CharacterBehaviourComponent {
    public InvadeBehaviour() {
        priority = 10;
    }
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        log += $"\n{character.name} is an Invader";
        if (character.behaviourComponent.invadeVillageTarget == null) {
            log += $"\n-No invade target yet, setting one...";
            character.behaviourComponent.SetInvadeVillageTarget(character.behaviourComponent.GetVillageTargetsByPriority());
            if (character.behaviourComponent.invadeVillageTarget == null) {
                log += $"\n-Still no invade target, un-summoning minion.";
                //if still no valid location was set, then unsummon minion.
                character.minion.Death();
            }
            producedJob = null;
            return true;
        } else {
            log += $"\n-Already has village target";
            if (character.hexTileLocation != null && character.behaviourComponent.invadeVillageTarget.Contains(character.hexTileLocation)) {
                log += $"\n-Already att village target, will find character to attack";
                //character is already at target village
                List<Character> targets = GetTargetChoices(character.behaviourComponent.invadeVillageTarget);
                if (targets != null) {
                    //Fight a random target
                    Character chosenTarget = CollectionUtilities.GetRandomElement(targets);
                    log += $"\n-Chosen target is {chosenTarget.name}";
                    character.combatComponent.Fight(chosenTarget, CombatManager.Hostility);
                } else {
                    log += $"\n-No more valid targets, clearing target village data...";
                    //No more valid targets exist, clearing village target. 
                    character.behaviourComponent.SetInvadeVillageTarget(null);
                }
                producedJob = null;
                return true;
            } else {
                log += $"\n-character is not yet at village target, will go there now...";
                //character is not yet at target village
                HexTile targetHextile =
                    CollectionUtilities.GetRandomElement(character.behaviourComponent.invadeVillageTarget);
                LocationGridTile targetTile = CollectionUtilities.GetRandomElement(targetHextile.locationGridTiles);
                return character.jobComponent.CreateGoToJob(targetTile, out producedJob);
            }
        }
    }
    private List<Character> GetTargetChoices(List<HexTile> tiles) {
        List<Character> characters = null;
        for (int i = 0; i < tiles.Count; i++) {
            HexTile tile = tiles[i];
            List<Character> charactersAtHexTile =
                tile.GetAllCharactersInsideHexThatMeetCriteria(c => c.isNormalCharacter && c.isDead == false);
            if (charactersAtHexTile != null) {
                characters = new List<Character>(charactersAtHexTile);
            }
        }
        return characters;
    }
    // public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
    //     producedJob = null;
    //     log += $"\n-{character.name} will invade";
    //     if (character.gridTileLocation.collectionOwner.isPartOfParentRegionMap
    //         && character.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner 
    //         && character.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner.settlementOnTile == character.behaviourComponent.assignedTargetSettlement) {
    //         log += "\n-Already in the target npcSettlement, will try to combat residents";
    //         //It will only go here if the invader is not combat anymore, meaning there are no more hostiles in his vision, so we must make sure that he attacks a resident in the settlement even though he can't see it
    //         character.behaviourComponent.invadeCombatantTargetList.Clear();
    //         character.behaviourComponent.invadeNonCombatantTargetList.Clear();
    //         BaseSettlement settlement = character.behaviourComponent.assignedTargetSettlement;
    //         for (int i = 0; i < settlement.residents.Count; i++) {
    //             Character resident = settlement.residents[i];
    //             if (!resident.isDead && resident.gridTileLocation != null && resident.gridTileLocation.IsPartOfSettlement(settlement) 
    //                 && resident.isAlliedWithPlayer == false) {
    //                 if (resident.traitContainer.HasTrait("Combatant")) {
    //                     character.behaviourComponent.invadeCombatantTargetList.Add(resident);
    //                 } else {
    //                     character.behaviourComponent.invadeNonCombatantTargetList.Add(resident);
    //                 }
    //             }
    //         }
    //         if(character.behaviourComponent.invadeCombatantTargetList.Count > 0) {
    //             Character chosenTarget = character.behaviourComponent.invadeCombatantTargetList[UnityEngine.Random.Range(0, character.behaviourComponent.invadeCombatantTargetList.Count)];
    //             log += "\n-Will attack combatant resident: " + chosenTarget.name;
    //             character.combatComponent.Fight(chosenTarget, CombatManager.Hostility);
    //         } else if (character.behaviourComponent.invadeNonCombatantTargetList.Count > 0) {
    //             Character chosenTarget = character.behaviourComponent.invadeNonCombatantTargetList[UnityEngine.Random.Range(0, character.behaviourComponent.invadeNonCombatantTargetList.Count)];
    //             log += "\n-Will attack non-combatant resident: " + chosenTarget.name;
    //             character.combatComponent.Fight(chosenTarget, CombatManager.Hostility);
    //             //character.Death();
    //         } else {
    //             log += "\n-No resident found in settlement, dissipate";
    //             //character.jobComponent.TriggerRoamAroundTile();
    //             character.Death();
    //         }
    //     } else {
    //         log += "\n-Is not in the target npcSettlement";
    //         log += "\n-Roam there";
    //         HexTile targetHex = character.behaviourComponent.assignedTargetSettlement.tiles[UnityEngine.Random.Range(0, character.behaviourComponent.assignedTargetSettlement.tiles.Count)];
    //         LocationGridTile targetTile = targetHex.locationGridTiles[UnityEngine.Random.Range(0, targetHex.locationGridTiles.Count)];
    //         character.jobComponent.TriggerRoamAroundTile(out producedJob, targetTile);
    //     }
    //     return true;
    // }
}
